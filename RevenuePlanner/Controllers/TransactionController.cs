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
        private ITransaction _transaction;

        public TransactionController(ITransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("Transaction", "Argument Transaction cannot be null");
            }

            _transaction = transaction; //DI will take care of populating this!
        }

        /// <summary>
        /// Save a list of Transaction to Line Item mappings with the associated amounts. This includes new mappings as well as 
        /// udpates to existing mappings.
        /// </summary>
        /// <param name="transactionLineItemMappings">The list of mappings to create or update.</param>
        [System.Web.Http.HttpPost]
        public void SaveTransactionToLineItemMapping(List<TransactionLineItemMapping> transactionLineItemMappings)
        {
            _transaction.SaveTransactionToLineItemMapping(Sessions.User.CID, transactionLineItemMappings, Sessions.User.ID);
        }

        [System.Web.Http.HttpPost]
        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            _transaction.DeleteTransactionLineItemMapping(Sessions.User.CID, mappingId);
        }

        public IEnumerable<TransactionHeaderMapping> GetHeaderMappings()
        {
            return _transaction.GetHeaderMappings(Sessions.User.CID);
        }

        public IEnumerable<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {
            return _transaction.GetLinkedLineItemsForTransaction(Sessions.User.CID, transactionId);
        }

        public int GetTransactionCount(DateTime start, DateTime end, bool unprocessedOnly = true)
        {
            return _transaction.GetTransactionCount(Sessions.User.CID, start, end, unprocessedOnly);
        }

        public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end, bool unprocessedOnly = true, int skip = 0, int take = 10000)
        {
            //Potential data transformation or triming per client per column heading mapping
            return _transaction.GetTransactions(Sessions.User.CID, start, end, unprocessedOnly, null, skip, take);
        }

        public IEnumerable<Transaction> GetTransactionsForLineItem(int lineItemId)
        {
            return _transaction.GetTransactionsForLineItem(Sessions.User.CID, lineItemId);
        }

    }
}