/**
 * This is the main routine that runs when the library is loaded
*/
import $ from 'jquery';
import test from './test.ejs';

function main() {
    // TODO: use a router.  For now, we just assume the Transactions page

    const $rootElement = $("#full-width-content-wrapper");
    $rootElement.html(test({value: "<this is a template>"}));
}


// Run main once the document is ready
$(main);
