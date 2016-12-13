import $ from 'jquery';
import createDataSource from './linkedItemTreeGridDataSource';
import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';
import css from './transactions.scss';
import loadingMessage from './views/loading.ejs';
import noLinkedItemsMessage from './views/no-linked-items.ejs';

// ================= START - code that belongs in a general purpose utility module =====================
function doRecalc(grid, parentGrid, element, td, initialize) {
    parentGrid._detectHeight(element, td, grid.objBox.scrollHeight + grid.hdr.offsetHeight + (grid.ftr ? grid.ftr.offsetHeight : 0));
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
}

function createSubGrid(parentGrid, element, td, dataSource) {
    const grid = new Grid(element);
    subGridSetupThatShouldGoSomewhereElse(grid, parentGrid, element, td);

    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.setDateFormat("%m/%d/%Y");
    grid.enableTreeCellEdit(false);
    grid.enableAutoWidth(true);

    dataSource.bindToGrid(grid);

    doRecalc(grid, parentGrid, element, td, true, false);
}
// ================= END - code that belongs in a general purpose utility module =====================

export default function createLinkedItemSubGrid(element, transactionId, parentGrid, td) {
    element.innerHTML = loadingMessage({css});

    const dataSource = createDataSource(transactionId);

    $(td.parentNode).data("linkedItemsDataSource", dataSource);

    // do not create the grid until the data source has loaded
    const promise = $.Deferred();

    function onLoad(ev) {
        if (dataSource.state.records) {
            dataSource.off("change", onLoad);

            createSubGrid(parentGrid, element, td, dataSource);
            if (dataSource.state.records.length == 0 ) {
                //TODO: we need to show message as will as listening to the transaction changes
                //element.innerHTML = noLinkedItemsMessage({css});
            }

            // Tell our caller that we have finished loading
            promise.resolve();
        }
    }

    dataSource.on("change", onLoad);

    return promise;
}
