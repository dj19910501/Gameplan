import mainView from './views/main.ejs';
import css from './transactions.scss';
import Grid from 'dhtmlXGridObject';
import resolveAppUri from 'util/resolveAppUri';
import transactionGridDataSource from './transactionGridDataSource';
import "third-party/jquery.simplePagination";
import "third-party/jquery.simplePagination.scss";

function createGrid($gridContainer, dataSource) {
    const $grid = $gridContainer.find(`.${css.grid}`);
    const grid = new Grid($grid.get(0));
    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.enableAutoHeight(true);
    grid.enableAutoWidth(true);
    grid.enableAutoWidth(true);
    dataSource.bindToGrid(grid);
}

function createPager($pager, dataSource) {
    $pager.pagination({
        cssStyle: "light-theme",
        pages: dataSource.numPages || 1,
        currentPage: dataSource.pageNumber || 1,
        onPageClick: (pageNumber, ev) => {
            dataSource.gotoPage(pageNumber);

            // ev is undefined if user used ellipses to type in a page number
            if (ev) {
                // do not let the click event update the URL hash
                ev.preventDefault();
            }
        }
    });

    // listen to the datasource and whenever the paging changes, update the pager
    dataSource.on("change", ev => {
        if (ev.which.paging || ev.which.totalRecords) {
            $pager.pagination("setPagesCount", dataSource.numPages || 1);
            $pager.pagination("drawPage", dataSource.pageNumber || 1);
        }
    });
}

export default function main($rootElement) {
    $rootElement
        .addClass("header-content-footer-layout")
        .html(mainView({css}));

    const $pager = $rootElement.find(`.${css.pager}`);
    const $gridContainer = $rootElement.find(`.${css.gridContainer}`);

    const dataSource = transactionGridDataSource();
    createPager($pager, dataSource);
    createGrid($gridContainer, dataSource);
}
