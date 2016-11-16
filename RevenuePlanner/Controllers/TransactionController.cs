using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services.Transactions;

namespace RevenuePlanner.Controllers
{
    /// <summary>
    /// NOTE: User ID and Client ID comes from session object
    /// We don't expose to or collect these two IDs from UI!
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class TransactionController : ApiController
    {
        ITransaction _transaction;
        TransactionController(ITransaction transaction)
        {
            _transaction = transaction; //DI will take crea of populating this!
        }

        public void SaveTransactionToLineItemMapping(List<TransactionLineItemMapping> transactionLineItemMappings)
        {
            _transaction.SaveTransactionToLineItemMapping(transactionLineItemMappings, Sessions.User.ID);
        }

        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            _transaction.DeleteTransactionLineItemMapping(mappingId);
        }

        public IEnumerable<TransactionHeaderMapping> GetHeaderMappings()
        {
            return _transaction.GetHeaderMappings(Sessions.User.CID);
        }

        public IEnumerable<LineItem> GetLinkedLineItemsForTransaction(int transactionId)
        {
            return _transaction.GetLinkedLineItemsForTransaction(transactionId);
        }

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true)
        {
            return _transaction.GetTransactionCount(Sessions.User.CID, start, end, unprocessdedOnly);
        }

        public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end, bool unprocessdedOnly = true, List<ColumnFilter> columnFilters = null, int pageIndex = 1, int pageSize = 10000)
        {
            //Potential data transformation or triming per client per column heading mapping
            return _transaction.GetTransactions(Sessions.User.CID, start, end, unprocessdedOnly, columnFilters);
        }

        public IEnumerable<Transaction> SearchForTransactions(DateTime start, DateTime end, string searchText, bool unprocessdedOnly = true)
        {
            return _transaction.SearchForTransactions(Sessions.User.CID, start, end, searchText, unprocessdedOnly);
        }
    }
}