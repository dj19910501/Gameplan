import $ from 'jquery';
import createDataSource from './linkedItemTreeGridDataSource';
import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';

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

export default function createLinkedItemSubGrid(element, transactionId, parentGrid, td) {
    element.innerHTML = "Loading...";

    const dataSource = createDataSource(transactionId);

    // do not create the grid until the data source has loaded
    const promise = $.Deferred();


    function onLoad(ev) {
        if (dataSource.state.records) {
            dataSource.off("change", onLoad);

            const grid = new Grid(element);
            subGridSetupThatShouldGoSomewhereElse(grid, parentGrid, element, td);

            grid.setImagePath(resolveAppUri("codebase/imgs/"));
            grid.setDateFormat("%m/%d/%Y");
            grid.enableTreeCellEdit(false);
            grid.enableAutoWidth(true);

            dataSource.bindToGrid(grid);

            doRecalc(grid, parentGrid, element, td, true, false);

            // Tell our caller that we have finished loading
            promise.resolve();
        }
    }

    dataSource.on("change", onLoad);

    return promise;
}
