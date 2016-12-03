import $ from 'jquery';

const DEFAULT_PAGING = {
    skip: 0,
    take: 100,
};

class GridDataSource {
    constructor(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords) {
        this.state = {
            filter: initialFilter,
            paging: {...DEFAULT_PAGING, ...initialPaging},
            totalRecords: initialTotalRecords,
            columns: initialColumns,
            records: initialRecords,
        };
    }

    _notify(which) {
        $(this).trigger($.Event("change", { which }));
    }

    on(eventType, callback) {
        $(this).on(eventType, callback);
    }

    off(eventType, callback) {
        $(this).off(eventType, callback);
    }

    _initializeGridColumns(grid) {
        let firstTime = true;

        // Now listen for records
        const renderGridData = ev => {
            if (!ev || ev.which.records) {
                const records = this.state.records;

                if (firstTime && !records) {
                    // do not initialize the grid until we have records
                    return;
                }

                const json = { data: records };

                if (firstTime) {
                    json.head = this.state.columns;

                    // set column resizing
                    grid.enableResizing(this.state.columns.map(c => !c.noresize).join(","));

                    // set number format
                    this.state.columns.forEach((column, icolumn) => {
                        if (column.numberFormat) {
                            grid.setNumberFormat(column.numberFormat, icolumn);
                        }
                    });
                }
                else {
                    grid.clearAll(false);
                }

                grid.parse(json, "js");

                firstTime = false;
            }
        };

        renderGridData();
        this.on("change", renderGridData);
    }

    /**
     * Updates the grid whenever the columns or data changes
     * @param grid
     */
    bindToGrid(grid) {
        if (this.state.columns) {
            this._initializeGridColumns(grid);
        }
        else {
            // wait until we have columns, then initialize the grid
            const handler = () => {
                if (this.state.columns) {
                    $(this).off("change", handler);
                    this._initializeGridColumns(grid);
                }
            };
            this.on("change", handler);
        }
    }

    updateRecords(newRecords) {
        this.state.records = newRecords;
        this._notify({records: true});
    }

    updateTotalRecords(totalRecords) {
        if (totalRecords !== this.state.totalRecords) {
            this.state.totalRecords = totalRecords;
            this._notify({totalRecords: true});
        }
    }

    updateFilter(newFilter) {
        this.state.filter = newFilter;
        this.state.totalRecords = undefined;
        this._notify({filter: true, totalRecords: true});
    }

    /**
     * @param newColumns
     * newColumns should be an array of columns to use. e.g.: [ column1, column2, ...]
     * Each column should be an object with these properties:
     *
     * id {string} - The name of the property on your data records to use for the value of this column
     * value {string} - the column label to display in the Header row
     * width {number|string} (optional) - the width (pixels) of this column.  Use "*" to make this column expand to fill space https://docs.dhtmlx.com/api__dhtmlxgrid_setinitwidths.html
     * type {string} (optional) - the DHTMLX grid cell type https://docs.dhtmlx.com/api__dhtmlxgrid_setcoltypes.html
     * align {string} (optional) - the column alignment: left, center, right, justify https://docs.dhtmlx.com/api__dhtmlxgrid_setcolalign.html
     * sort {string} (optional) - the column sorting type https://docs.dhtmlx.com/api__dhtmlxgrid_setcolsorting.html
     * hidden {bool} (optional) - mark the column as hidden
     * noresize {bool} (optional) - if true, then this column cannot be resized by the user
     */
    updateColumns(newColumns) {
        this.state.columns = newColumns;
        this._notify({columns: true});
    }

    updatePaging(newPaging) {
        this.state.paging = newPaging;
        this._notify({paging: true});
    }

    get pageNumber() {
        const {paging} = this.state;
        if (paging.take !== undefined && paging.skip !== undefined) {
            return Math.ceil((paging.skip + 1) / paging.take);
        }
    }

    get numPages() {
        const {paging, totalRecords} = this.state;

        if (totalRecords !== undefined && paging.take !== undefined) {
            return Math.ceil(totalRecords / paging.take);
        }
    }

    _setSkip(pSkip) {
        const {paging, totalRecords} = this.state;

        let newSkip = pSkip;

        // If we have a total, then ensure we do not go past it
        if (totalRecords !== undefined && newSkip >= totalRecords) {
            // ensure skip does not go negative!!!
            newSkip = totalRecords - 1;
        }

        // ensure skip does not go negative
        if (newSkip < 0) {
            newSkip = 0;
        }

        if (newSkip !== paging.skip) {
            this.updatePaging({...paging, skip: newSkip});
        }

    }

    /**
     * page forward, use negative numbers to page backwards
     * @param numPages
     */
    pageForward(numPages = 1) {
        const {paging} = this.state;

        if (paging.skip !== undefined && paging.take !== undefined) {
            this._setSkip(paging.skip + numPages * paging.take);
        }
    }

    gotoPage(pageNumber) {
        const {paging} = this.state;
        if (paging.take !== undefined) {
            this._setSkip((pageNumber - 1) * paging.take);
        }
    }
}

export default function gridDataSource(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords) {
    return new GridDataSource(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords);
}

