import mainView from './views/main.ejs';
import css from './transactions.scss';
import Grid from 'dhtmlXGridObject';
import uniqueId from 'lodash/uniqueId';
import resolveAppUri from 'util/resolveAppUri';
import transactionGridDataSource, {LINKED_ITEM_RENDERER_PROPERTY} from './transactionGridDataSource';
import linkedItemEditor from './linkedItemEditor';
import find from 'lodash/find';
import "third-party/jquery.simplePagination";
import "third-party/jquery.simplePagination.scss";
import createFilteredContentView from 'components/filteredContent/filteredContent';
import createFilterView from './filter';

function createGrid($gridContainer, dataSource, filteredView) {
    const $grid = $gridContainer.find(`.${css.grid}`);
    const grid = new Grid($grid.get(0));
    grid.setImagePath(resolveAppUri("codebase/imgs/"));
    // grid.enableAutoHeight(true);
    grid.enableAutoWidth(true);
    grid.setDateFormat("%m/%d/%Y");
    dataSource.bindToGrid(grid);

    // refresh the grid size whenever the filter panel is toggled
    $(filteredView).on("filterToggled", () => {
        grid.entBox.style.width = "auto";
        grid.setSizes()
    });

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

                    // if the popup changes any values, update our transaction and also
                    // update the subgrid if it is open
                    popup.on("linkedItemsChanged", ev => {
                        const subGridDataSource = row.data("linkedItemsDataSource");
                        if (subGridDataSource) {
                            subGridDataSource.assignRawData(ev.records.links);
                        }

                        // when we update the transaction, it will close the subgrid.
                        // so we need to check if it is open and re-open it after the update
                        const subgridCell = grid.cellById(transactionId, grid.getColIndexById(LINKED_ITEM_RENDERER_PROPERTY));
                        const isOpen = subgridCell && subgridCell.isOpen();
                        dataSource.updateTransaction(ev.records.transaction);
                        if (isOpen && !subgridCell.isOpen()) {
                            subgridCell.open();
                        }
                    });
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

function calculateRowsPerPage($gridContainer) {
    const HEADER_HEIGHT = 32; // based on  measurement in Chrome
    const ROW_HEIGHT = 26; // based on measurement in Chrome
    const containerHeight = $gridContainer.height();

    const rowsPerPage = Math.floor((containerHeight - HEADER_HEIGHT) / ROW_HEIGHT);

    return Math.max(5, rowsPerPage);
}

export default function main($rootElement) {
    const filteredView = createFilteredContentView($rootElement);
    const viewOptions = {
        css,
        viewById: uniqueId("viewBy"),
        viewByValue: "all",
    };

    filteredView.$filterPanel

    filteredView.$content
        .addClass("header-content-footer-layout")
        .html(mainView(viewOptions));

    const $pager = filteredView.$content.find(`.${css.pager}`);
    const $gridContainer = filteredView.$content.find(`.${css.gridContainer}`);
    const $viewBy = filteredView.$content.find(`#${viewOptions.viewById}`);

    const dataSource = transactionGridDataSource(calculateRowsPerPage($gridContainer));
    const filterPanel = createFilterView(filteredView.$filterPanel, dataSource);
    bindViewBy($viewBy, dataSource);
    createPager($pager, dataSource);
    createGrid($gridContainer, dataSource, filteredView);
    bindNoRecordsMessage(dataSource, $pager, $gridContainer);
}
