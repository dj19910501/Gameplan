import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';
import COLUMN_DEFAULTS from './transactionGridColumnDefaults';

function setupColumns(grid, headerMappings) {
    grid.setHeader(headerMappings.map(m => m.ClientHeader).join(","));
    grid.setInitWidths(headerMappings.map(m => COLUMN_DEFAULTS[m.Hive9Header].width || "150").join(","));
    grid.setColAlign(headerMappings.map(m => COLUMN_DEFAULTS[m.Hive9Header].align || "left").join(","));
}

export default function createGrid(model, $container, $grid) {
    const grid = new Grid($grid.get(0));

    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    setupColumns(grid, model.state.headerMappings);
    grid.init();
    model.getGridDataAsync().then(r => grid.parse(r, "json"));
}
