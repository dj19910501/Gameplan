/**
 * A read only date cell
 * @param cell
 */
function eXcell_roDate(cell) {
    window.eXcell_dhxCalendarA.call(this, cell);
}

eXcell_roDate.prototype = new window.eXcell_dhxCalendarA;

eXcell_roDate.prototype.edit = () => {};
eXcell_roDate.prototype.isDisabled = () => true;

window.eXcell_roDate = eXcell_roDate;

export default "roDate";
