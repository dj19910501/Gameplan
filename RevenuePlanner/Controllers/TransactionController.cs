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
        public TransactionController(ITransaction transaction)
        {
            _transaction = transaction; //DI will take care of populating this!
        }

        [System.Web.Http.HttpPost]
        public void SaveTransactionToLineItemMapping(List<TransactionLineItemMapping> transactionLineItemMappings)
        {
            _transaction.SaveTransactionToLineItemMapping(transactionLineItemMappings, Sessions.User.ID);
        }

        [System.Web.Http.HttpPost]
        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            _transaction.DeleteTransactionLineItemMapping(mappingId);
        }

        public IEnumerable<TransactionHeaderMapping> GetHeaderMappings()
        {
            return _transaction.GetHeaderMappings(Sessions.User.CID);
        }

        public IEnumerable<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {
            return _transaction.GetLinkedLineItemsForTransaction(transactionId);
        }

        public int GetTransactionCount(DateTime start, DateTime end, bool unprocessedOnly = true)
        {
            return _transaction.GetTransactionCount(Sessions.User.CID, start, end, unprocessedOnly);
        }

        public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end, bool unprocessedOnly = true, int pageIndex = 1, int pageSize = 100)
        {
            //Potential data transformation or triming per client per column heading mapping
            return _transaction.GetTransactions(Sessions.User.CID, start, end, unprocessedOnly, null, pageIndex, pageSize);
        }

        public IEnumerable<Transaction> GetTransactionsForLineItem(int lineItemId)
        {
            return _transaction.GetTransactionsForLineItem(lineItemId);
        }

    }
}