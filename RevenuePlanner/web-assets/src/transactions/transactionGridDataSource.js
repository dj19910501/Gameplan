import moment from 'moment';
import $ from 'jquery';
import gridDataSource from 'util/gridDataSource';
import {GET_HEADER_MAPPINGS_URI, GET_TRANSACTIONS_URI, GET_TRANSACTION_COUNT_URI} from './apiUri';
import COLUMN_DEFAULTS from './transactionGridColumnDefaults';
import SubRowCellType from 'gridCellTypes/sub_row_func';

const LINKED_ITEM_RENDERER_PROPERTY = "linkedItemRenderer";

function getGridColumns() {
    return $.getJSON(GET_HEADER_MAPPINGS_URI)
        .then(headerMappings => {
            const columns = headerMappings.map(mapping => ({
                id: mapping.Hive9Header,
                value: mapping.ClientHeader,
                type: "ro",
                width: COLUMN_DEFAULTS[mapping.Hive9Header].width || "150",
                align: COLUMN_DEFAULTS[mapping.Hive9Header].align || "left",
            }));

            // Add a column to the front to toggle the subgrid of linked line items
            columns.unshift({
                id: LINKED_ITEM_RENDERER_PROPERTY,
                value: "&nbsp;",
                type: SubRowCellType,
                width: 18,
                align: "left",
            });

            return columns;
        });
}

function getGridData(filter, paging) {
    // construct the URL
    const params = [
        `start=${filter.startDate.format("YYYY-MM-DD")}`,
        `end=${filter.endDate.format("YYYY-MM-DD")}`,
        `unprocessedOnly=${!filter.includeProcessedTransactions}`,
        `skip=${paging.skip}`,
        `take=${paging.take}`,
    ];

    const requestUrl = `${GET_TRANSACTIONS_URI}?${params.join("&")}`;

    // request the data
    return $.getJSON(requestUrl).then(records => {
        // doctor the records a bit
        for (const record of records) {
            // DHTMLX requires every record have an "id" property
            record.id = record.TransactionId;

            // Add the function to load and render the linked items
            record[LINKED_ITEM_RENDERER_PROPERTY] = (element, transactionId) => {
                element.innerHTML = `Loading ${transactionId}...`;

                const deferred = $.Deferred();

                setTimeout(() => {
                    element.innerHTML = `<h1>${transactionId} is now loaded</h1><div style="height: 150px; background-color: cyan;">Awesome!</div>`;
                    deferred.resolve();
                }, 1500);

                return deferred;
            };
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
export default function transactionGridDataSource() {
    const now = moment();
    const filter = {
        // Initial date range is the past year.  Should we use some other default?
        endDate: now,
        startDate: now.clone().subtract(1, 'year'),
        includeProcessedTransactions: true,
    };
    const paging = {
        skip: 0,
        take: 2, // TODO make user selectable and/or change default based on Browser speed
    };

    const dataSource = gridDataSource(filter, paging);

    // ask the server for our column definitions and give them to the dataSource once we know them
    getGridColumns().then(columns => dataSource.updateColumns(columns));

    // listen for filter/paging changes and request new data
    bindDataSourceToServer(dataSource);

    // return the dataSource
    return dataSource;
}
