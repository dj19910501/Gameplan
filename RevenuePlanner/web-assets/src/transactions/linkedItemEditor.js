import $ from 'jquery';
import resolveAppUri from 'util/resolveAppUri';
import Grid from 'dhtmlXGridObject';
import view from './views/linkedItemEditor.ejs';
import css from './linkedItemEditor.scss';
import dhx4 from 'dhx4';
import {isValidNumeric} from 'dhtmlxValidation';
import createModel from './linkedItemEditorModel';
import allowGridClickEvents from 'util/allowGridClickEvents';

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
    //grid.enableAutoWidth(true);
    grid.setDateFormat("%m/%d/%Y");
    grid.enableEditEvents(true, false, false);
    dataSource.bindToGrid(grid);

    return grid;
}

function bindGrid(model, $container) {
    const dataSource = model.linkedItemGridDataSource;
    let grid;

    function onData(ev) {
        if ((!ev || ev.which.records) && dataSource.state.records) {
            dataSource.off("change", onData);

            const grid = model.linkedItemGrid = createGrid(dataSource, $container);

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

                // change the icon based on the delete status
                const icon = isDeleted ? "fa-undo" : "fa-trash-o";
                td.innerHTML = `<i class="fa ${icon} fa-fw"></i>`;

                // toggle the deleted css on the row
                $(tr).toggleClass(css.deleted, isDeleted);

                ev.stopPropagation();
            });
        }
    }

    dataSource.on("change", onData);
    onData();
}

function save(model, $window, $save, $cancel) {
    model.saving = true;
    $save.html('<i class="fa fa-spinner fa-pulse fa- fa-fw"></i>Saving...').prop('disabled', true);
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

function bindModelToEditor(transactionId, model, $window) {
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
}

function renderInitialView(transaction) {
    // use Bootstrap modal
    const $view = $(view({ css, transaction, currency }));
    // remove everything but the div
    $view.contents().filter(function () { return this.nodeType !== 1; }).remove();

    return $view.filter("div").appendTo(document.body);
}

export default function linkedItemEditor(transaction) {
    const model = createModel(transaction);
    const $window = renderInitialView(transaction);

    bindModelToEditor(transaction.id, model, $window);

    // destroy the modal when it is closed
    $window.on('hidden', () => $window.remove());

    // display the dialog
    $window.modal();

    return $window;
}
