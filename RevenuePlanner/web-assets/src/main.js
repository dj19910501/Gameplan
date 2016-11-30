/**
 * This is the main routine that runs when the library is loaded
*/
import $ from 'jquery';
import transactions from 'transactions';

function main() {
    // TODO: use a router.  For now, we just assume the Transactions page.
    const $rootElement = $("#full-width-content-wrapper");
    transactions($rootElement);
}

// Run main once the document is ready
$(main);
