import moment from 'moment';
import $ from 'jquery';
import {GET_HEADER_MAPPINGS_URI} from './apiUri';

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

export default class TransactionModel {
    constructor() {
        // set the initial state.

        const now = moment();
        this.state = {
            filter: {
                // Initial date range is the past year.  Should we use some other default?
                endDate: now,
                startDate: now.clone().subtract(1, 'year'),
                includeProcessedTransactions: true,
            },
            pageSize: 100, // TODO make user selectable and/or change default based on Browser speed
            headerMappings: undefined,
        };

        // request the header mappings since we need those for everything else
        // use default mappings
        $.getJSON(GET_HEADER_MAPPINGS_URI)
            .then(h => this._updateState("headerMappings", h), () => this._updateState("headerMappings", DEFAULT_HEADER_MAPPINGS));
    }

    _updateState(key, newValue) {
        const oldValue = this.state[key];
        if (oldValue !== newValue) {
            this.state[key] = newValue;

            // notify listeners about the change
            $(this).trigger($.Event(`change-${key}`, {oldValue, newValue}));
        }
    }
}
