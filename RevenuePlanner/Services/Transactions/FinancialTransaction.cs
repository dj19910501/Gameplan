using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services.Transactions
{
    public class FinancialTransaction : ITransaction
    {
        public void AttrbuteTransactionsToLineItems(List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId)
        {
            throw new NotImplementedException();
        }

        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            throw new NotImplementedException();
        }

        public List<TransactionHeaderMapping> GetHeaderMappings(int clientId)
        {
            throw new NotImplementedException();
        }

        public List<LineItem> GetLinkedLineItemsForTransaction(int transactionId)
        {
            throw new NotImplementedException();
        }

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true)
        {
            throw new NotImplementedException();
        }

        public List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true, List<ColumnFilter> columnFilters = null, int pageIndex = 1, int pageSize = 10000)
        {
            throw new NotImplementedException();
        }

        public List<Transaction> SearchForTransactions(int clientId, DateTime start, DateTime end, string searchText, bool unprocessdedOnly = true)
        {
            throw new NotImplementedException();
        }
    }
}