import moment from 'moment';
import $ from 'jquery';
import gridDataSource from 'util/gridDataSource';
import {GET_HEADER_MAPPINGS_URI, GET_TRANSACTIONS_URI, GET_TRANSACTION_COUNT_URI} from './apiUri';
import COLUMN_DEFAULTS from './transactionGridColumnDefaults';

/**
 * We will use this default mapping if we are unable to retrieve a mapping from the server
 */
const DEFAULT_HEADER_MAPPINGS = [{
    "ClientHeader": "Transaction ID",
    "Hive9Header": "ClientTransactionId"
}, {
    "ClientHeader": "Purchase Order",
    "Hive9Header": "PurchaseOrder"
}, {
    "ClientHeader": "Vendor",
    "Hive9Header": "Vendor"
}, {
    "ClientHeader": "Amount",
    "Hive9Header": "Amount"
}, {
    "ClientHeader": "Description",
    "Hive9Header": "TransactionDescription"
}, {
    "ClientHeader": "Account",
    "Hive9Header": "Account"
}, {
    "ClientHeader": "Date",
    "Hive9Header": "TransactionDate"
}, {
    "ClientHeader": "Department",
    "Hive9Header": "Department"
}];

function getRecordId(transactionRecord) {
    return transactionRecord.TransactionId;
}

function getColumnValue(record, column) {
    return record[column.property];
}

function createColumnFromHeaderMapping(mapping) {
    return {
        label: mapping.ClientHeader,
        property: mapping.Hive9Header,
        width: COLUMN_DEFAULTS[mapping.Hive9Header].width || "150",
        align: COLUMN_DEFAULTS[mapping.Hive9Header].align || "left",
    };
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
    return $.getJSON(requestUrl);
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
        take: 3, // TODO make user selectable and/or change default based on Browser speed
    };

    const dataSource = gridDataSource(filter, paging, undefined, undefined, undefined, getRecordId, getColumnValue);

    // ask the server for our column definitions and give them to the dataSource once we know them
    function setColumns(headerMappings) {
        const columns = headerMappings.map(createColumnFromHeaderMapping);
        dataSource.updateColumns(columns);
    }

    $.getJSON(GET_HEADER_MAPPINGS_URI).then(setColumns, () => setColumns(DEFAULT_HEADER_MAPPINGS));

    // listen for filter/paging changes and request new data
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
    onChange(); // start the initial query

    // return the dataSource
    return dataSource;
}
