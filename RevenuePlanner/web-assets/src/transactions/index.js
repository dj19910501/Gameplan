import mainView from './views/main.ejs';
import css from './transactions.scss';
import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';
import transactionGridDataSource from './transactionGridDataSource';

export default function main($rootElement) {

    $rootElement
        .addClass("header-content-footer-layout")
        .html(mainView({css}));

    const $gridContainer = $rootElement.find(`.${css.gridContainer}`);
    const $grid = $gridContainer.find(`.${css.grid}`);
    const grid = new Grid($grid.get(0));
    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    const dataSource = transactionGridDataSource();
    dataSource.bindToGrid(grid);
}
