import $ from 'jquery';

const DEFAULT_PAGING = {
    skip: 0,
    take: 100,
};

/**
 * transforms the records into DHTMLX format
 * @param records
 * @param columns
 * @param getRecordId
 * @param getColumnValue
 * @returns {{rows}}
 */
function transformRecords(records, columns, getRecordId, getColumnValue) {
    if (records && columns) {
        return {
            rows: records.map(record => ({
                id: getRecordId(record),
                data: columns.map(column => getColumnValue(record, column)),
            })),
        }
    }
}

function initializeGridColumns(grid, columns) {
    grid.setHeader(columns.map(c => c.label).join(","));
    grid.setInitWidths(columns.map(c => c.width).join(","));
    grid.setColAlign(columns.map(c => c.align).join(","));
}

class GridDataSource {
    constructor(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords, getRecordId, getColumnValue) {
        this._getRecordId = getRecordId;
        this._getColumnValue = getColumnValue;
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
        initializeGridColumns(grid, this.state.columns);
        grid.init();

        // Now listen for records
        const renderGridData = ev => {
            if (!ev || ev.which.records) {
                const records = this.state.records;
                this.gridData = transformRecords(records, this.state.columns, this._getRecordId, this._getColumnValue);

                grid.clearAll(false);
                if (this.gridData) {
                    grid.parse(this.gridData, "json");
                }
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

export default function gridDataSource(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords, getRecordId, getColumnValue) {
    return new GridDataSource(initialFilter, initialPaging, initialColumns, initialRecords, initialTotalRecords, getRecordId, getColumnValue);
}

