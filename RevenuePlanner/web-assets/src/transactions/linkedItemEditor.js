import $ from 'jquery';
import resolveAppUri from 'util/resolveAppUri';
import Grid from 'dhtmlXGridObject';
import viewTemplate from './views/linkedItemEditor.ejs';
import optionsTemplate from './views/selectOptions.ejs';
import uniqueId from 'lodash/uniqueId';
import css from './linkedItemEditor.scss';
import dhx4 from 'dhx4';
import {isValidNumeric} from 'dhtmlxValidation';
import createModel from './linkedItemEditorModel';
import allowGridClickEvents from 'util/allowGridClickEvents';

const SPINNER = '<i class="fa fa-spinner fa-pulse fa fa-fw"></i>';

// define a currency formatter
const currencyFormat = dhx4.template._parseFmt(`${window.CurrencySybmol} 0,000.00`);
function currency(value) {
    return dhx4.template._getFmtValue(value, currencyFormat);
}

function createGrid(dataSource, $container) {
    $container.empty();
    const $div = $("<div>").appendTo($container);
    const grid = new Grid($div[0]);
    allowGridClickEvents(grid);

    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.enableAutoHeight(true);
    grid.enableAutoWidth(true);
    grid.setDateFormat("%m/%d/%Y");
    grid.enableEditEvents(true, false, false);
    dataSource.bindToGrid(grid);

    return grid;
}

function createBrowserGrid(view, model, which, selectedWhich, title) {
    const $container = view[`$${which}`];
    const $div = $("<div>").appendTo($container);
    const grid = new Grid($div[0]);
    grid.setImagePath(resolveAppUri("codebase/imgs/"));

    if (which !== "lineItems") {
        grid.attachEvent("onRowSelect", id => model.newItemModel[selectedWhich] = id);
    }
    else {
        allowGridClickEvents(grid);
        $container.on("click", `tr.${css.notMapped} td:last-child`, function () {
            // remove the action from the grid
            const $td = $(this);
            const $tr = $td.parent();
            $td.html("");
            $tr.removeClass(css.notMapped);

            // update the model
            const id = $tr[0].idd;
            const newMapping = model.addNewMapping(id);

            // Add the row to the grid.
            model.linkedItemGridDataSource.appendRecord(newMapping);

            // mark the entire row as "modified"
            const newRow = model.linkedItemGrid.getRowById(newMapping.id);
            $(newRow).children().addClass(css.modified);
            model.linkedItemGrid.selectRowById(newMapping.id);
        });
    }

    const dataSource = model.createNewItemGridDataSource(which, title);
    dataSource.bindToGrid(grid);
}

function bindGrid(model, $container) {
    const dataSource = model.linkedItemGridDataSource;

    function onData(ev) {
        if ((!ev || ev.which.records) && dataSource.state.records) {
            dataSource.off("change", onData);

            const grid = model.linkedItemGrid = createGrid(dataSource, $container);

            if (dhx4.isIE11) {
                // IE11 grid seems to always calculate a 0 size :/
                let inSetSize = false;
                grid.attachEvent("onSetSizes", () => {
                    if (!inSetSize) {
                        grid.entBox.style.width = "auto";
                        inSetSize = true;

                        try {
                            grid.setSizes();
                        }
                        finally {
                            inSetSize = false;
                        }

                        console.log(grid.entBox.style.width);
                    }
                });
            }

            // listen for edit events and validation events
            grid.attachEvent("onEditCell", (stage, id, index, newValue, oldValue) => {
                if (stage === 2) {
                    const cell = grid.cellById(id, index);
                    const isValid = isValidNumeric(newValue);

                    if (isValid) {
                        const value = parseFloat(newValue);
                        model.setIsValid(id, true);

                        const isModified = model.modify(id, value);
                        // update the cell style
                        $(cell.cell).toggleClass(css.modified, isModified);
                    }
                    else {
                        model.setIsValid(id, false);
                        cell.setCValue(newValue);
                    }
                }

                return true;
            });

            // Listen for clicks on the trash icon
            $container.on("click", ".objbox td:last-child", function (ev) {
                const td = this;
                const tr = td.parentNode;
                const itemId = tr.idd;

                grid.clearSelection();

                // toggle the delete status of the item
                const isDeleted = model.toggleDelete(itemId);

                if (isDeleted === undefined) {
                    // the user deleted a "new" record.  We want to completely remove this row from the grid.
                    grid.deleteRow(itemId);
                }
                else {

                    // change the icon based on the delete status
                    const icon = isDeleted ? "fa-undo" : "fa-trash-o";
                    td.innerHTML = `<i class="fa ${icon} fa-fw"></i>`;

                    // toggle the deleted css on the row
                    $(tr).toggleClass(css.deleted, isDeleted);
                }

                ev.stopPropagation();
            });
        }
    }

    dataSource.on("change", onData);
    onData();
}

function save(model, $window, $save, $cancel) {
    model.saving = true;
    $save.html(`${SPINNER} Saving...`).prop('disabled', true);
    model.save().then(
        () => {
            model.saving = false;
            $save.text("Save");
            // TODO: update grids
        },
        err => {
            model.saving = false;
            $save.text("Save").prop('disabled', false);
        });
}

function bindModelToEditor(transactionId, model, view) {
    const {$window, $selectPlan, $selectYear} = view;
    const $content = $window.find(`.${css.links}`);
    $content.removeClass(css.loading);
    bindGrid(model, $content);

    const $buttons = $window.find(`.modal-footer button`);
    const $cancel = $buttons.eq(0);
    const $save = $buttons.eq(1);

    // update the button states.
    model.on("invalid modified", () => {
        if (model.isModified) {
            $cancel.text("Cancel");
            $save.prop('disabled', !model.isValid);
        }
        else {
            $cancel.text("Close");
            $save.prop('disabled', true);
        }
    });

    // Perform save
    $save.on("click", () => save(model, $window, $save, $cancel));

    // confirm if the user tries to close without saving
    $window.on("hide", ev => {
        if (model.saving) {
            return ev.preventDefault();
        }

        if (model.isModified) {
            if (!confirm("You have unsaved changes.  Are you sure you wish to close this window?")) {
                ev.preventDefault();
            }
        }

        if (model.updatedData) {
            $window.trigger($.Event("linkedItemsChanged", { transactionId, records: model.updatedData }));
        }
    });

    const $availableFunds = $window.find(`.${css.availableFunds}`);
    model.on("availableFunds", () => {
        const amount = model.availableFunds;
        $availableFunds.html((amount == null) ? "&nbsp;" : currency(amount));
    });

    // Update the combo boxes when there is data
    model.newItemModel.subscribe("years", ev => {
        const years = ev.value;
        if (years) {
            const optionsHtml = optionsTemplate({ prompt: "Select Year", items: years.map(y => ({value: y, text: y})) });
            $selectYear.html(optionsHtml);
            $selectYear.multiselect("refresh");
            $selectYear.multiselect("enable");
        }
        else {
            $selectYear.multiselect("disable");
            $selectYear.multiselect("getButton").children().eq(0).html(`${SPINNER} Loading...`);
        }
    });

    $selectYear.on("change", function () {
        const option = this.options[this.selectedIndex];
        const value = option && option.value;
        model.newItemModel.selectedYear = value || undefined;
    });

    model.newItemModel.subscribe("plans", ev => {
        const plans = ev.value;
        if (plans) {
            const optionsHtml = optionsTemplate({ prompt: "Select Plan", items: plans.map(p => ({value: p.Id, text: p.Title }))});
            $selectPlan.html(optionsHtml);
            $selectPlan.multiselect("refresh");
            $selectPlan.multiselect("enable");
        }
        else {
            $selectPlan.multiselect("disable");
            const label = model.newItemModel.selectedYear ? `${SPINNER} Loading...` : "&nbsp;";
            $selectPlan.multiselect("getButton").children().eq(0).html(label);
        }
    });

    $selectPlan.on("change", function () {
        const option = this.options[this.selectedIndex];
        const value = option && parseInt(option.value, 10);
        model.newItemModel.selectedPlan = value || undefined;
    });

    createBrowserGrid(view, model, "campaigns", "selectedCampaign", "Campaigns");
    createBrowserGrid(view, model, "programs", "selectedProgram", "Programs");
    createBrowserGrid(view, model, "tactics", "selectedTactic", "Tactics");
    createBrowserGrid(view, model, "lineItems", null, "Line Items");
}

function renderInitialView(transaction) {
    const viewOptions = {
        css,
        transaction,
        currency,
        selectYearId: uniqueId("select"),
        selectPlanId: uniqueId("select"),
    };

    // use Bootstrap modal
    const $view = $(viewTemplate(viewOptions));
    // remove everything but the div
    $view.contents().filter(function () { return this.nodeType !== 1; }).remove();

    const $window = $view.filter("div").appendTo(document.body);
    const $selectYear = $window.find(`#${viewOptions.selectYearId}`);
    const $selectPlan = $window.find(`#${viewOptions.selectPlanId}`);
    const $campaigns = $window.find(`.${css.campaigns}`);
    const $programs = $window.find(`.${css.programs}`);
    const $tactics = $window.find(`.${css.tactics}`);
    const $lineItems = $window.find(`.${css.lineItems}`);

    $selectYear.multiselect({
        multiple: false,
        selectedList: 1,
        minWidth: 220,
        classes: css.selectYear,
        position: {
            my: "left top",
            at: "left bottom+10",
        },
    }).multiselectfilter();

    $selectPlan.multiselect({
        multiple: false,
        selectedList: 1,
        minWidth: 220,
        classes: css.selectPlan,
        position: {
            my: "left top",
            at: "left bottom+10",
        },
    }).multiselectfilter();

    return { $window, $selectYear, $selectPlan, $campaigns, $programs, $tactics, $lineItems };
}

export default function linkedItemEditor(transaction) {
    const model = createModel(transaction);
    const view = renderInitialView(transaction);

    bindModelToEditor(transaction.id, model, view);

    // destroy the modal when it is closed
    view.$window.on('hidden', () => view.$window.remove());

    // display the dialog
    view.$window.modal();

    return view.$window;
}
