import $ from 'jquery';
import {GET_LINKED_LINE_ITEMS, SAVE_LINKED_LINE_ITEMS, DELETE_LINKED_LINE_ITEMS} from './apiUri';
import flatMap from 'lodash/flatMap';
import gridDataSource from 'util/gridDataSource';
import keyBy from 'lodash/keyBy';
import groupBy from 'lodash/groupBy';
import find from 'lodash/find';
import findIndex from 'lodash/findIndex';
import createNewItemModel from './linkedItemEditorNewLinkModel';
import css from './linkedItemEditor.scss';
import uniqueId from 'lodash/uniqueId';

const columns = [
    { id: "title", value: "Name", width: 300, type: "rotxt", align: "left", sort: "na" },
    { id: "tacticName", value: "Plan", width: "*", type: "rotxt", align: "left", sort: "na" },
    { id: "lineItemCost", value: "Planned Cost", width: 100, type: "ron", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00` },
    { id: "lineItemActual", value: "Total Actual Costs", width: 100, type: "ron", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00` },
    { id: "mappedAmount", value: "Linked to Account", width: 100, type: "edn", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00`, validator: "ValidNumeric" },
    { id: "trash", value: "&nbsp;", width: 32, type: "ro", align: "center", sort: "na" },
];

function mapLinkedItems(result) {
    return flatMap(result, tactic => tactic.LineItems
        .filter(lineItem => lineItem.LineItemId !== -1) // filter out Sys_Gen_Balance line items from the editor.
        .map(lineItem => ({
            id: lineItem.LineItemMapping.TransactionLineItemMappingId,
            lineItemId: lineItem.LineItemId,
            tacticName: `${lineItem.PlanTitle} > ${lineItem.CampaignTitle} > ${lineItem.ProgramTitle} > ${tactic.Title}`,
            mappedAmount: lineItem.LineItemMapping.Amount,
            lineItemCost: lineItem.Cost,
            lineItemActual: lineItem.Actual,
            title: lineItem.Title,
            trash: `<i class="fa fa-trash-o fa-fw"></i>`,
        })));
}

function updateDataSource(state, dataSource, items) {
    // store a lookup table
    state.itemLookup = keyBy(items, "id");
    state.items = [...items];

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
        items: undefined,
    };

    const model = {
        linkedItemGridDataSource: dataSource,
        newItemModel: createNewItemModel(),

        containsLineItem(lineItemId) {
            return !!(state.items && find(state.items, {lineItemId}));
        },

        refreshLineItems() {
            this.newItemModel.refreshLineItems();
        },

        /**
         * GridDataSource bound to one of the arrays in newItemModel
         * @param which
         * @param title
         */
        createNewItemGridDataSource(which, title) {
            const columns = [
                { id: "Title", value: title, width: "*", type: "rotxt", align: "left", sort: "na" },
            ];

            let mapItem = item => ({id: item.Id, Title: item.Title });

            if (which === "lineItems") {
                // add some extra columns for this grid
                columns.push(
                    { id: "Cost", value: "Cost", width: 100, type: "ron", align: "right", sort: "na", numberFormat: `${window.CurrencySybmol} 0,000.00` },
                    { id: "action", value: "&nbsp;", width: 100, type: "ro", align: "left", sort: "na" },
                );

                mapItem = item => {
                    const isInList = this.containsLineItem(item.Id);
                    const action = isInList ? "" : '<i class="fa fa-plus fa-fw"></i> Add To List';
                    const rowClass = isInList ? undefined : css.notMapped;
                    return {
                        id: item.Id,
                        Title: item.Title,
                        Cost: item.Cost,
                        action: action,
                        class: rowClass,
                    };
                };
            }

            const ds = gridDataSource(undefined, undefined, columns);

            this.newItemModel.subscribe(which, ev => {
                const records = ev.value && ev.value.map(mapItem);
                ds.updateRecords(records);
            });

            return ds;
        },

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

                    $(this).trigger("availableFunds");
                }
                return false;
            }

            // get the modification record (or create it if this is the first modification

            let modifiedRecord = state.modifiedItems[id];
            if (!modifiedRecord) {
                modifiedRecord = state.modifiedItems[id] = {...record, mappedAmount: value};
                if (++state.modifiedCount === 1) {
                    $(this).trigger("modified");
                }
            }
            else {
                modifiedRecord.mappedAmount = value;
            }

            $(this).trigger("availableFunds");

            return true;
        },

        addNewMapping(lineItemId) {
            const lineItem = find(this.newItemModel.lineItems, {Id: lineItemId});
            const campaign = find(this.newItemModel.campaigns, {Id: this.newItemModel.selectedCampaign});
            const program = find(this.newItemModel.programs, {Id: this.newItemModel.selectedProgram});
            const tactic = find(this.newItemModel.tactics, {Id: this.newItemModel.selectedTactic});
            const plan = find(this.newItemModel.plans, {Id: this.newItemModel.selectedPlan});
            const newItem = {
                isNew: true,
                // this is just a temporary id until we save the record and get a real id from the sever
                id: uniqueId("mappedItem"),
                lineItemId: lineItem.Id,
                tacticName: `${plan.Title} > ${campaign.Title} > ${program.Title} > ${tactic.Title}`,
                mappedAmount: lineItem.Cost || 0,
                lineItemCost: lineItem.Cost,
                lineItemActual: lineItem.Actual,
                title: lineItem.Title,
                trash: `<i class="fa fa-trash-o fa-fw"></i>`,
            };

            state.items.push(newItem);
            state.itemLookup[newItem.id] = newItem;
            state.modifiedItems[newItem.id] = newItem;
            if (++state.modifiedCount === 1) {
                $(this).trigger("modified");
            }

            $(this).trigger("availableFunds");

            return newItem;
        },

        toggleDelete(id) {
            let modified = state.modifiedItems[id];
            if (!modified) {
                // this record has not been modified
                const record = state.itemLookup[id];
                modified = state.modifiedItems[id] = {...record, isDeleted: true};
                if (++state.modifiedCount === 1) {
                    $(this).trigger("modified");
                }

                $(this).trigger("availableFunds");

                return true;
            }
            else if (modified.isNew) {
                // they are deleting a "new" unsaved item.  We want to completely remove it from the list
                delete state.modifiedItems[id];
                delete state.itemLookup[id];
                const itemIndex = findIndex(state.items, {id});
                state.items.splice(itemIndex, 1);

                if (--state.modifiedCount === 0) {
                    $(this).trigger("modified");
                }

                $(this).trigger("availableFunds");

                this.refreshLineItems();

                return undefined; // signals the caller to remove the item from the list
            }
            else {
                modified.isDeleted = !modified.isDeleted;
                if (modified.isDeleted) {
                    $(this).trigger("availableFunds");
                    return true;
                }

                // call modify() which will detect if there are any other changes
                // and remove the record if there aren't
                this.modify(id, modified.mappedAmount);
                return false;
            }
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
            const changes = groupBy(state.modifiedItems, item => item.isDeleted ? "del" : "upd");
            const updateRecords = changes.upd && changes.upd.map(item => {
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
            const deleteRecords = changes.del && changes.del.map(item => item.id);

            // run the update and the delete in parallel
            const promises = [];
            if (updateRecords) {
                const payload = JSON.stringify(updateRecords);
                promises.push($.ajax({
                    type: "POST",
                    url: SAVE_LINKED_LINE_ITEMS,
                    data: payload,
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
                }));
            }
            if (deleteRecords) {
                const payload = JSON.stringify(deleteRecords);
                promises.push($.ajax({
                    type: "POST",
                    url: DELETE_LINKED_LINE_ITEMS,
                    data: payload,
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
                }));
            }

            // wait for both calls to finish
            return $.when(...promises).then(() => {
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
