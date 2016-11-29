/**
 * This is the main routine that runs when the library is loaded
*/
import $ from 'jquery';
import displayTransactions from './transactions/displayTransactions';

function main() {
    // TODO: use a router.  For now, we just assume the Transactions page

    const $rootElement = $("#full-width-content-wrapper");
    $rootElement.html("content goes here");
}


// Run main once the document is ready
$(main);
