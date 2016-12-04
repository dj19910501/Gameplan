/**
 * Defines a DHTMLX grid cell type "sub_row_func".
 * The value of the cell should be a function which will be invoked to render the "sub content" whenever the show/hide toggle is clicked
 *
 * The func will be invoked as:
 *
 * func(containerElement, rowID, gridInstance).
 * If the function needs to work asynchronously, it should return a thenable (e.g. deferred or Promise) to indicate when it is finished rendering.
 * otherwise it should return nothing
 *
 * Example:
 *
 * function myRender(containerElement, rowID, grid) {
 *     // tell the user their data is loading
 *     containerElement.innerHTML = "Loading...";
 *
 *     return $.getJSON(`/GetSomeData?id${rowID}`).then(result => {
 *         containerElement.innerHTML = myContentTemplate(result);
 *     }, error => {
 *         containerElement.innerHTML = "sad face";
 *     });
 * }
 *
 * grid.setColTypes("sub_row_func,txt,txt");
 *
 * grid.parse({
 *    rows: [
 *        { id: 42, data: [myRender, "column2", "column3"] },
 *        { id: 44, data: [myRender, "column2", "column3"] },
 *        { id: 45, data: [myRender, "column2", "column3"] },
 *    ]
 * }, "json");
 */
window.eXcell_sub_row_func = function (a) {
    this.base = window.eXcell_sub_row;
    this.base(a);
    this.setValue = function(b) {
        if (b) {
            this.grid.setUserData(this.cell.parentNode.idd, "__sub_row", b)
        }
        this.cell._sub_row_type = "func";
        this.cell._previous_content = null;
        this._setState(b ? "plus.gif" : "blank.gif")
    }
}

window.eXcell_sub_row_func.prototype = new eXcell_sub_row;

window.dhtmlXGridObject.prototype._sub_row_render.func = function(that, element, td, func) {
    // func should populate element with goodies
    const result = func(element, td.parentNode.idd, that);

    if (result) {
        result.then(() => {
            that._detectHeight(element, td);
            that._correctMonolite();
            /*
             that.setUserData(td.parentNode.idd, "__sub_row", xml.xmlDoc.responseText);
             td._sub_row_type = null;
             */
            if (that._ahgr) {
                that.setSizes()
            }
            // that.callEvent("onSubAjaxLoad", [td.parentNode.idd, xml.xmlDoc.responseText])
        });
    }
}

// Export the cell type to use for this
export default "sub_row_func";
