import resolveAppUri from 'util/resolveAppUri';

function constructUri(endpoint) {
    return resolveAppUri(`api/Transaction/${endpoint}`);
}

export const GET_HEADER_MAPPINGS_URI = constructUri("GetHeaderMappings");
export const GET_TRANSACTION_COUNT_URI = constructUri("GetTransactionCount");
export const GET_TRANSACTIONS_URI = constructUri("GetTransactions");
export const GET_SINGLE_TRANSACTION_URI = constructUri("GetTransaction");
export const GET_LINKED_LINE_ITEMS = constructUri("GetLinkedLineItemsForTransaction");
export const SAVE_LINKED_LINE_ITEMS = constructUri("SaveTransactionToLineItemMapping");
export const DELETE_LINKED_LINE_ITEMS = constructUri("DeleteTransactionLineItemMapping");
