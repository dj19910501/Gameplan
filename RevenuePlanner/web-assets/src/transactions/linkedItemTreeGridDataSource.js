import $ from 'jquery';
import gridDataSource from 'util/gridDataSource';
import {GET_LINKED_LINE_ITEMS} from './apiUri';
import escape from 'lodash/escape';

const columns = [
    {
        id: "Tree",
        value: "Title",
        width: 400,
        type: "tree",
        align: "left",
        sort: "na",
    },
    {
        id: "MappedAmount",
        value: "Amount Linked",
        width: 100,
        type: "ron[=sum]",
        align: "right",
        numberFormat: `${window.CurrencySybmol} 0,000.00`,
        sort: "na",
    },
    {
        id: "ActualCost",
        value: "Total Actual Cost",
        width: 100,
        type: "ron[=sum]",
        align: "right",
        numberFormat: `${window.CurrencySybmol} 0,000.00`,
        sort: "na",
    },
    {
        id: "PlannedCost",
        value: "Planned Cost",
        width: 100,
        type: "ron[=sum]",
        align: "right",
        numberFormat: `${window.CurrencySybmol} 0,000.00`,
        sort: "na",
    },
    {
        id: "Plan",
        value: "Plan",
        width: "*",
        type: "rotxt",
        align: "left",
        sort: "na",
    },
];

function renameProperty(object, from, to) {
    object[to] = object[from];
    object[from] = undefined;
}

function transformData(result) {
    // if there are no results, then we are done
    if (!result.length) {
        return result;
    }

    // convert the data into a TreeGrid format
    for (const tactic of result) {
        tactic.id = tactic.TacticId;

        // needs to be called "rows", not LineItems
        renameProperty(tactic, "LineItems", "rows");

        // remove PlannedCost, ActualCost because we want to sum the line items instead
        tactic.PlannedCost = undefined;
        tactic.ActualCost = undefined;

        // the "tree" column needs to be HTML-encoded "by hand"
        tactic.Tree = escape(tactic.Title);

        for (const item of tactic.rows) {
            item.id = item.LineItemId;

            renameProperty(item, "Cost", "PlannedCost");
            renameProperty(item, "Actual", "ActualCost");
            item.MappedAmount = item.LineItemMapping.Amount;

            item.Tree = escape(item.Title);
            item.Plan = `${item.PlanTitle} > ${item.CampaignTitle} > ${item.ProgramTitle}`;

            // convert to actual dates
            item.DateModified = {value: new Date(item.LineItemMapping.DateModified)};
            item.DateProcessed = {value: new Date(item.LineItemMapping.DateProcessed)};
        }
    }

    // If there is only 1 tactic, go ahead and start with it expanded
    if (result.length === 1) {
        result[0].open = true;
    }

    return result;
}

function getData(filter) {
    const params = {
        transactionID: filter.transactionId,
    };

    return $.getJSON(GET_LINKED_LINE_ITEMS, params);
}

function bindDataSourceToServer(dataSource) {
    let currentGridDataRequest;
    function onChange(ev) {
        if (!ev || ev.which.filter || ev.which.paging) {
            // cancel the current in-flight request
            if (currentGridDataRequest) {
                currentGridDataRequest.abort();
                currentGridDataRequest = undefined;
            }

            currentGridDataRequest = getData(dataSource.state.filter);
            currentGridDataRequest.then(result => {
                currentGridDataRequest = undefined;
                dataSource.assignRawData(result);
            });
        }
    }

    dataSource.on("change", onChange);

    // start the initial query
    onChange();
}

/**
 * Creates a GridDataSource that populates with Transactions
 */
export default function linkedItemTreeGridDataSource(transactionId) {
    const filter = { transactionId };

    const dataSource = gridDataSource(filter, undefined, columns);
    dataSource.assignRawData = function (data) { this.updateRecords(transformData(data)); };

    // listen for filter/paging changes and request new data
    bindDataSourceToServer(dataSource);


    // return the dataSource
    return dataSource;
}
