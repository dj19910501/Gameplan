import Grid from 'dhtmlXGridObject';
import $ from 'jquery';

/**
 * Dummy code
 */
export default function renderGrid() {
    if (false) {
        $("#loading").hide();

        const grid = new Grid("gridbox");
        grid.init();
    }

    console.log(`hello from transactions/renderGrid at ${new Date().toISOString()}`);
}
