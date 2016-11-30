import resolveAppUri from 'util/resolveAppUri';

function constructUri(endpoint) {
    return resolveAppUri(`api/Transaction/${endpoint}`);
}

export const GET_HEADER_MAPPINGS_URI = constructUri("GetHeaderMappings");
export const GET_TRANSACTION_COUNT_URI = constructUri("GetTransactionCount");
export const GET_TRANSACTIONS_URI = constructUri("GetTransactions");
