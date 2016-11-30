/**
 * This is the main routine that runs when the library is loaded
*/
import $ from 'jquery';
import transactions from 'transactions';
import resolveAppUri from 'util/resolveAppUri';

// Tell webpack where to find our assets
__webpack_public_path__ = resolveAppUri("web-assets/");

function main() {
    // TODO: use a router.  For now, we just assume the Transactions page.
    const $rootElement = $("#full-width-content-wrapper");
    transactions($rootElement);
}

// Run main once the document is ready
$(main);
