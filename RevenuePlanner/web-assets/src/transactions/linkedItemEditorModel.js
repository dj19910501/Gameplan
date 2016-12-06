import $ from 'jquery';
import {GET_LINKED_LINE_ITEMS, SAVE_LINKED_LINE_ITEMS} from './apiUri';
import flatMap from 'lodash/flatMap';
import gridDataSource from 'util/gridDataSource';
import keyBy from 'lodash/keyBy';
import map from 'lodash/map';

const columns = [
    { id: "title", value: "Name", width: 300, type: "rotxt", align: "left", sort: "na" },
    { id: "tacticName", value: "Plan", width: "*", type: "rotxt", align: "left", sort: "na" },
    { id: "lineItemCost", value: "Planned Cost", width: 100, type: "ron", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00` },
    { id: "lineItemActual", value: "Total Actual Costs", width: 100, type: "ron", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00` },
    { id: "mappedAmount", value: "Linked to Account", width: 100, type: "edn", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00`, validator: "ValidNumeric" },
];

function mapLinkedItems(result) {
    return flatMap(result, tactic => tactic.LineItems.map(lineItem => ({
        id: lineItem.LineItemMapping.TransactionLineItemMappingId,
        lineItemId: lineItem.LineItemId,
        tacticName: `${lineItem.PlanTitle} > ${lineItem.CampaignTitle} > ${lineItem.ProgramTitle} > ${tactic.Title}`,
        mappedAmount: lineItem.LineItemMapping.Amount,
        lineItemCost: lineItem.Cost,
        lineItemActual: lineItem.Actual,
        title: lineItem.Title,
    })));
}

function updateDataSource(state, dataSource, items) {
    // store a lookup table
    state.itemLookup = keyBy(items, "id");
    state.items = items;

    // give the records to the grid
    dataSource.updateRecords(items)
}

function queryLinkedItems(transactionId) {
    return $.getJSON(GET_LINKED_LINE_ITEMS, { transactionID: transactionId});
}

export default function createModel(transaction) {
    const dataSource = gridDataSource(undefined, undefined, columns);

    const state = {
        invalidIds: Object.create(null),
        invalidCount: 0,

        modifiedItems: Object.create(null),
        modifiedCount: 0,
        itemLookup: undefined,
    };

    const model = {
        linkedItemGridDataSource: dataSource,

        on(event, cb) { $(this).on(event, cb); },
        off(event, cb) { $(this).off(event, cb); },

        setIsValid(id, isValid) {
            if (isValid) {
                if (state.invalidIds[id]) {
                    state.invalidIds[id] = false;
                    if (--state.invalidCount === 0) {
                        $(this).trigger("invalid");
                    }
                }
            }
            else if (!state.invalidIds[id]) {
                state.invalidIds[id] = true;
                if (++state.invalidCount === 1) {
                    $(this).trigger("invalid");
                }
            }
        },

        /**
         * Returns true if the item is modified from its original, false if the item is not modified
         * @param id
         * @param value
         */
        modify(id, value) {
            const record = state.itemLookup[id];
            if (record && record.mappedAmount === value) {
                // the value matches the original value.
                // this record is "not modified"
                if (state.modifiedItems[id]) {
                    delete state.modifiedItems[id];
                    if (--state.modifiedCount === 0) {
                        $(this).trigger("modified");
                    }
                }
                return false;
            }

            // get the modification record (or create it if this is the first modification

            let modifiedRecord = state.modifiedItems[id];
            if (!modifiedRecord) {
                modifiedRecord = state.modifiedItems[id] = {...record};
                if (++state.modifiedCount === 1) {
                    $(this).trigger("modified");
                }
            }

            modifiedRecord.mappedAmount = value;

            $(this).trigger("availableFunds");

            return true;
        },

        get isModified() {
            return state.modifiedCount > 0;
        },

        get isValid() {
            return state.invalidCount === 0;
        },

        get availableFunds() {
            if (state.items) {
                const sum = state.items.reduce((total, item) => {
                    // see if we have a modified item for this
                    const modifiedItem = state.modifiedItems[item.id];
                    const delta = modifiedItem ? (modifiedItem.isDeleted ? 0 : modifiedItem.mappedAmount) : item.mappedAmount;
                    return total + delta;
                }, 0);

                const newItemSum = 0; // TODO

                return transaction.Amount - sum - newItemSum;
            }

            return undefined;
        },

        save() {
            // Collect all of the modified records and create an array to send to the server
            const updateRecords = map(state.modifiedItems, item => {
                const updateRecord = {
                    TransactionId: transaction.id,
                    LineItemId: item.lineItemId,
                    Amount: item.mappedAmount,
                };

                if (!item.isNew) {
                    updateRecord.TransactionLineItemMappingId = item.id;
                }

                return updateRecord;
            });

            const payload = updateRecords;

            return $.ajax({
                type: "POST",
                url: SAVE_LINKED_LINE_ITEMS,
                data: JSON.stringify(payload),
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
            }).then(() => {
                // reload the data
                return queryLinkedItems(transaction.id).then(result => {
                    state.invalidCount = 0;
                    state.invalidIds = Object.create(null);
                    state.modifiedItems = Object.create(null);
                    state.modifiedCount = 0;
                    updateDataSource(state, dataSource, mapLinkedItems(result));
                    this.updatedData = result;
                    $(this).trigger("modified");
                    $(this).trigger("invalid");
                });
            });
        }
    };

    queryLinkedItems(transaction.id).then(result => {
        updateDataSource(state, dataSource, mapLinkedItems(result));
        $(model).trigger("availableFunds");
    });

    return model;
}
