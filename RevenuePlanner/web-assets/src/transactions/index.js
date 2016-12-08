import mainView from './views/main.ejs';
import css from './transactions.scss';
import Grid from 'dhtmlXGridObject';
import uniqueId from 'lodash/uniqueId';
import resolveAppUri from 'util/resolveAppUri';
import transactionGridDataSource from './transactionGridDataSource';
import linkedItemEditor from './linkedItemEditor';
import find from 'lodash/find';
import "third-party/jquery.simplePagination";
import "third-party/jquery.simplePagination.scss";

function createGrid($gridContainer, dataSource) {
    const $grid = $gridContainer.find(`.${css.grid}`);
    const grid = new Grid($grid.get(0));
    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    grid.enableAutoHeight(true);
    grid.enableAutoWidth(true);
    grid.setDateFormat("%m/%d/%Y");
    dataSource.bindToGrid(grid);

    // add click handler to grid editLineItems whenever the grid re-renders
    grid.attachEvent("onXLE", () => {
        $grid
            .find(`.${css.editLineItems}`)
            .off("click.transactionGrid")
            .on("click.transactionGrid", function (ev) {
                const row = $(this).closest("tr");
                const transactionId = row[0].idd;
                ev.stopPropagation();

                // Find the transaction object and launch the linked item editor
                const transaction = find(dataSource.state.records, {id: transactionId});
                if (!transaction) {
                    console.error(`could not find transaction for ${transactionId}`);
                }
                else {
                    const popup = linkedItemEditor(transaction);

                    // if the linked items subgrid is open, then register an event so we can update it
                    // with any changes the user makes
                    const subGridDataSource = row.data("linkedItemsDataSource");
                    if (subGridDataSource) {
                        popup.on("linkedItemsChanged", ev => {
                            subGridDataSource.assignRawData(ev.records);
                        });
                    }
                }
            });
    });
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

function bindViewBy($viewBy, dataSource) {
    // use multiselect plugin for this dropdown
    $viewBy.multiselect({
        multiple: false,
        selectedList: 1,
        minWidth: 220,
        classes: css.viewByMultiSelect,
        position: {
            my: "right top",
            at: "right bottom+10",
        },
    });

    $viewBy.on("change", function () {
        const index = this.selectedIndex;
        const option = this.options[index];
        const value = option.value;
        dataSource.updateFilter({
            ...dataSource.state.filter,
            includeProcessedTransactions: value !== "unlinked",
        });
    });
}

function bindNoRecordsMessage(dataSource, $pager, $gridContainer) {
    function updateDisplay(ev) {
        if (!ev || ev.which.totalRecords) {
            const totalRecords = dataSource.state.totalRecords;

            // if totalRecords is undefined, that means a request is pending to get the count
            // do nothing in this situation.
            if (totalRecords !== undefined) {
                const noRecords = (totalRecords === 0);
                if (noRecords) {
                    $pager.addClass(css.noRecords);
                    $gridContainer.addClass(css.noRecords);
                }
                else {
                    $pager.removeClass(css.noRecords);
                    $gridContainer.removeClass(css.noRecords);
                }
            }
        }
    }

    dataSource.on("change", updateDisplay);

    // set the initial display
    updateDisplay();
}

export default function main($rootElement) {
    const viewOptions = {
        css,
        viewById: uniqueId("viewBy"),
        viewByValue: "all",
    };

    $rootElement
        .addClass("header-content-footer-layout")
        .html(mainView(viewOptions));

    const $pager = $rootElement.find(`.${css.pager}`);
    const $gridContainer = $rootElement.find(`.${css.gridContainer}`);
    const $viewBy = $rootElement.find(`#${viewOptions.viewById}`);

    const dataSource = transactionGridDataSource();
    bindViewBy($viewBy, dataSource);
    createPager($pager, dataSource);
    createGrid($gridContainer, dataSource);
    bindNoRecordsMessage(dataSource, $pager, $gridContainer);
}
