import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';

export default function createGrid($container, $grid) {
    const grid = new Grid($grid.get(0));

    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.setHeader("Product Name,Internal Code,Price");
    grid.setInitWidths("*,150,150");
    grid.setColAlign("left,left,right");
    grid.init();
}
