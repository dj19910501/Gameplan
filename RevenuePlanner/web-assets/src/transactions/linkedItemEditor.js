import $ from 'jquery';
import view from './views/linkedItemEditor.ejs';
import css from './linkedItemEditor.scss';
import dhx4 from 'dhx4';

// define a currency formatter
const currencyFormat = dhx4.template._parseFmt(`${window.CurrencySybmol} 0,000.00`);
function currency(value) {
    return dhx4.template._getFmtValue(value, currencyFormat);
}

export default function linkedItemEditor(transaction) {
    // use Bootstrap modal
    const $view = $(view({ css, transaction, currency }));
    // remove everything but the div
    $view.contents().filter(function () { return this.nodeType !== 1; }).remove();

    const $window = $view.filter("div").appendTo(document.body);
    $window.modal();
}
