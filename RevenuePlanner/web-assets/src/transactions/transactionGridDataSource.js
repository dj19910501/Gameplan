import moment from 'moment';
import $ from 'jquery';
import gridDataSource from 'util/gridDataSource';
import {GET_HEADER_MAPPINGS_URI, GET_TRANSACTIONS_URI, GET_TRANSACTION_COUNT_URI} from './apiUri';
import SubRowCellType from 'gridCellTypes/sub_row_func';
import mapHive9Column from 'util/mapHive9Column';
import css from './transactions.scss';
import createLinkedItemSubGrid from './createLinkedItemSubGrid';

export const LINKED_ITEM_RENDERER_PROPERTY = "linkedItemRenderer";
const EDIT_LINKED_ITEMS = "editLinkedItems";

function getGridColumns() {
    return $.getJSON(GET_HEADER_MAPPINGS_URI)
        .then(headerMappings => {
            const columns = headerMappings.map(mapping => {
                const column = mapHive9Column(mapping, false);
                column.sort = "na";
                return column;
            });

            // Add a column to the front to toggle the subgrid of linked line items
            columns.unshift({
                id: LINKED_ITEM_RENDERER_PROPERTY,
                value: "&nbsp;",
                type: SubRowCellType,
                width: 18,
                align: "left",
                noresize: true,
                sort: "na",
            });

            // Add a column to let the user launch the popup
            columns.splice(2, 0, {
                id: EDIT_LINKED_ITEMS,
                value: "&nbsp;",
                type: "ro",
                width: 32,
                align: "center",
                noresize: true,
                sort: "na",
            });

            return columns;
        });
}

function transformRecord(record) {
    // convert anything whose name ends in Date to a date
    for (const propertyName in record) {
        if (typeof record[propertyName] === "string" && /Date$/.test(propertyName)) {
            // note DHX needs to have the dates wrapped in an object like this otherwise it can't "see" them due to a bug
            record[propertyName] = { value: new Date(record[propertyName]) };
        }
    }

    // DHTMLX requires every record have an "id" property
    record.id = record.TransactionId;

    // Add the function to load and render the linked items
    record[LINKED_ITEM_RENDERER_PROPERTY] = createLinkedItemSubGrid;

    // Add the button to open the popup editor
    record[EDIT_LINKED_ITEMS] = `<i class='fa fa-plus-circle ${css.editLineItems}' title='Add/Remove Linked Items'></i>`;
}

function getGridData(filter, paging) {
    const params = {
        start: filter.startDate.format("YYYY-MM-DD"),
        end: filter.endDate.format("YYYY-MM-DD"),
        unprocessedOnly: !filter.includeProcessedTransactions,
        skip: paging.skip,
        take: paging.take,
    };

    // request the data
    return $.getJSON(GET_TRANSACTIONS_URI, params).then(records => {
        // doctor the records a bit
        for (const record of records) {
            transformRecord(record);
        }

        return records;
    });
}

function getRecordCount(filter) {
    // construct the URL
    const params = [
        `start=${filter.startDate.format("YYYY-MM-DD")}`,
        `end=${filter.endDate.format("YYYY-MM-DD")}`,
        `unprocessedOnly=${!filter.includeProcessedTransactions}`,
    ];

    const requestUrl = `${GET_TRANSACTION_COUNT_URI}?${params.join("&")}`;

    // request the data
    return $.getJSON(requestUrl);
}

function bindDataSourceToServer(dataSource) {
    let currentGridDataRequest;
    let currentCountRequest;
    function onChange(ev) {
        if (!ev || ev.which.filter || ev.which.paging) {
            // cancel the current in-flight request
            if (currentGridDataRequest) {
                currentGridDataRequest.abort();
                currentGridDataRequest = undefined;
            }

            currentGridDataRequest = getGridData(dataSource.state.filter, dataSource.state.paging);
            currentGridDataRequest.then(result => {
                currentGridDataRequest = undefined;
                dataSource.updateRecords(result);
            });
        }

        if (!ev || ev.which.filter) {
            // cancel the current in-flight request
            if (currentCountRequest) {
                currentCountRequest.abort();
                currentCountRequest = undefined;
            }

            currentCountRequest = getRecordCount(dataSource.state.filter);
            currentCountRequest.then(result => {
                currentCountRequest = undefined;
                dataSource.updateTotalRecords(result);
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
export default function transactionGridDataSource(rowsPerPage = 10) {
    const now = moment();
    const filter = {
        // Initial date range is past 6 months
        endDate: now,
        startDate: now.clone().subtract(6, 'months'),
        includeProcessedTransactions: true,
    };
    const paging = {
        skip: 0,
        take: rowsPerPage,
    };

    const dataSource = gridDataSource(filter, paging);

    // ask the server for our column definitions and give them to the dataSource once we know them
    getGridColumns().then(columns => dataSource.updateColumns(columns));

    // listen for filter/paging changes and request new data
    bindDataSourceToServer(dataSource);

    dataSource.updateTransaction = transaction => {
        transformRecord(transaction);
        dataSource.updateRecord(transaction);
    };

    // return the dataSource
    return dataSource;
}
