import $ from 'jquery';
import createDataSource from './linkedItemTreeGridDataSource';
import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';
import css from './transactions.scss';
import loadingMessage from './views/loading.ejs';
import linkedItemSubGridTemplate from './views/linkedItemSubGrid.ejs';

// ================= START - code that belongs in a general purpose utility module =====================
function doRecalc(grid, parentGrid, element, td, initialize) {
    const gridHeight = grid.objBox.scrollHeight + grid.hdr.offsetHeight + (grid.ftr ? grid.ftr.offsetHeight : 0);
    parentGrid._detectHeight(element, td, gridHeight);
    if (!initialize) {
        grid.objBox.style.overflow = "hidden";
    }
    parentGrid._correctMonolite();
    if (initialize || parentGrid._ahgr) {
        parentGrid.setSizes();
    }
    if (initialize && parentGrid.parentGrid) {
        parentGrid.callEvent("onGridReconstructed", [])
    }
}

function subGridSetupThatShouldGoSomewhereElse(grid, parentGrid, element, td) {
    if (parentGrid.skin_name) {
        grid.setSkin(parentGrid.skin_name);
    }
    grid.parentGrid = parentGrid;
    grid.imgURL = parentGrid.imgURL;
    grid.iconURL = parentGrid.iconURL;
    grid.enableAutoHeight(true);
    grid.attachEvent("onGridReconstructed", function () {
        doRecalc(grid, parentGrid, element, td, false, true);
    });

    // treegrid does not trigger onGridReconstructed at proper time when collapsing a tree branch, so we will trigger it again when the collapse is done
    grid.attachEvent("onOpenEnd", function (id, state) {
        if (state !== 1) { // not required on Expand
            grid.callEvent("onGridReconstructed", [])
        }
    });
}

function createSubGrid(parentGrid, element, gridContainer, td, dataSource) {
    const grid = new Grid(gridContainer);
    subGridSetupThatShouldGoSomewhereElse(grid, parentGrid, element, td);

    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.setDateFormat("%m/%d/%Y");
    grid.enableTreeCellEdit(false);
    grid.enableAutoWidth(true);

    dataSource.bindToGrid(grid);

    doRecalc(grid, parentGrid, element, td, true, false);
}
// ================= END - code that belongs in a general purpose utility module =====================

function createSubGridView(element) {
    const html = linkedItemSubGridTemplate({css});
    const $element = $(element);
    $element.html(html);

    return {
        $subGrid: $element.find(`.${css.subgrid}`),
        $noLineItems: $element.find(`.${css.noLineItems}`),
    };
}

export default function createLinkedItemSubGrid(element, transactionId, parentGrid, td) {
    element.innerHTML = loadingMessage({css});

    const dataSource = createDataSource(transactionId);

    $(td.parentNode).data("linkedItemsDataSource", dataSource);

    // do not create the grid until the data source has loaded
    const promise = $.Deferred();
    let view;

    function toggleNoLineItems() {
        // add/remove the noLikedItemsMessage depending on if there are any records.
        view.$noLineItems.toggleClass(css.hidden, !!dataSource.state.records.length);
    }

    function onLoad(ev) {
        if (dataSource.state.records) {
            dataSource.off("change", onLoad);

            view = createSubGridView(element);
            createSubGrid(parentGrid, element, view.$subGrid[0], td, dataSource);

            toggleNoLineItems();
            dataSource.on("change", toggleNoLineItems);

            // Tell our caller that we have finished loading
            promise.resolve();
        }
    }

    dataSource.on("change", onLoad);

    return promise;
}
