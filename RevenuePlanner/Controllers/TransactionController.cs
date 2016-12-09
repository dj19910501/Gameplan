using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services.Transactions;
using RevenuePlanner.Controllers.Filters;
using System.Linq;
using System.Diagnostics.Contracts;

namespace RevenuePlanner.Controllers
{
    /// <summary>
    /// The TransactionController serves up all data related to Transactions and their relationship with Line Items. This 
    /// includes retreiving information as well as saving and deleting mappings between transactions and line items.
    /// 
    /// Currency conversion into and out of the User's preferred currency are also done in this controller. Note that all
    /// currencies are stored in USD in the database, so this is conversaion to to get number in the UI in the preferred
    /// currency and take user inputs in perferred currency.
    /// </summary>
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    [ApiAuthorizeUser(Enums.ApplicationActivity.TransactionAttribution)]
    public class TransactionController : ApiController
    {
        private ITransaction _transaction;

        public TransactionController(ITransaction transaction)
        {
            Contract.Requires<ArgumentNullException>(transaction != null, "Argument Transaction cannot be null");

            _transaction = transaction; //DI will take care of populating this!
        }

        /// <summary>
        /// Save a list of Transaction to Line Item mappings with the associated amounts. This includes new mappings as well as 
        /// udpates to existing mappings.
        /// This is a POST call and the request body should contain the json for the items to be saved.
        /// </summary>
        /// <param name="transactionLineItemMappings">The list of mappings to create or update.</param>
        [System.Web.Http.HttpPost]
        public void SaveTransactionToLineItemMapping(List<TransactionLineItemMapping> transactionLineItemMappings)
        {
            Contract.Requires<ArgumentNullException>(transactionLineItemMappings != null, "transactionLineItemsMappings cannot be null");

            _transaction.SaveTransactionToLineItemMapping(Sessions.User.CID, transactionLineItemMappings, Sessions.User.ID);
        }

        /// <summary>
        /// Deletes a group of individual line item mappings. This just deletes the link between a transaction and a line item, neither
        /// the transaction nor the line item are modified or deleted during this call.
        /// </summary>
        /// <param name="mappingIds">The id of the transaction line item mapping reference to be deleted.</param>
        [System.Web.Http.HttpPost]
        public void DeleteTransactionLineItemMapping(List<int> mappingIds)
        {
            Contract.Requires<ArgumentNullException>(mappingIds != null, "mappingIds cannot be null");
            Contract.Requires<ArgumentOutOfRangeException>(Contract.ForAll(mappingIds, id => id > 0), "A mappingId less than or equal to zero is invalid, and likely indicates the mappingId was not set properly");

            // delete them one by one.  Perhaps the interface can be changed to delete them in bulk?
            mappingIds.ForEach(id => _transaction.DeleteTransactionLineItemMapping(Sessions.User.CID, id));
        }

        /// <summary>
        /// Return a list of "Display name" to "hive9 data name" mappings. This is used to map columns in the transactoin data
        /// to column header names that will be displayed in the UI. This structure will also include a basic "field type"
        /// value that indicates if each column is text/percent/currency/etc. There should not be hard coded formatting 
        /// settings in this file.
        /// </summary>
        /// <returns>List of transaction header mapping objects, each object represents a column of transaction data</returns>
        public IEnumerable<TransactionHeaderMapping> GetHeaderMappings()
        {
            return _transaction.GetHeaderMappings(Sessions.User.CID);
        }

        /// <summary>
        /// Gets the line items that are mapped to the specified transaction. 
        /// The results are grouped by tactic, so if a transaction has line items mapped to multiple tactics, the returned list
        /// will be a list that contains tactic information and itself contains a list of all the line items associated with that
        /// tactic. Included in the results are the actual TransactionLineMapping information which can be utilized to modify
        /// a mapping via SaveTransactionToLineItemMapping
        /// </summary>
        /// <param name="transactionId">The transaction id whose mapped line items are being retrieved</param>
        /// <returns>A Lit of line items associated with the transaction id</returns>
        public IEnumerable<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {
            Contract.Requires<ArgumentOutOfRangeException>(transactionId > 0, "A transactionId less than or equal to zero is invalid, and likely indicates the transactionId was not set properly");

            return _transaction.GetLinkedLineItemsForTransaction(Sessions.User.CID, transactionId);
        }

        /// <summary>
        /// Get the number of transactions for the given time period. Can Optionally pass in whether to get only unprocessed
        /// transactions or to return all transactions for the period.
        /// </summary>
        /// <param name="start">Non-null start date, transactions will be counted that occur on or after this date</param>
        /// <param name="end">Non-null end date, transaction will be counted that occur before or on this date</param>
        /// <param name="unprocessedOnly">Optional. Whether to return only unprocessed transactions. True == return unprocessed only, false == return all</param>
        /// <returns>Integer indicating the number of transactions</returns>
        public int GetTransactionCount(DateTime start, DateTime end, bool unprocessedOnly = true)
        {
            return _transaction.GetTransactionCount(Sessions.User.CID, start, end, unprocessedOnly);
        }

        /// <summary>
        /// Get transactions for the given time period. Can optionally pass in whether to get only unprocessed transactions
        /// or to return all transactions for this period. Can also optionally pass in pagination information to skip a
        /// certain number of transactions and take a certain number of transactions
        /// </summary>
        /// <param name="start">Non-null start date, transactions will be returned that occur on or after this date</param>
        /// <param name="end">Non-null end date , transactions will be returned that occur before or on this date</param>
        /// <param name="unprocessedOnly">Optional. Whether to return only unprocessed transactions or all of them. True == return unprocess only, false == return all</param>
        /// <param name="skip">Optional. This number of transactsions will be skipped in the results returned</param>
        /// <param name="take">Optiona. This is the top number of transactions that will be returned by this call, fewer may be returned if there are fewer transactions available</param>
        /// <returns>returns at most <param name="take" /> transactions after skipping the first <param name="skip" /></returns>
        public IEnumerable<LeanTransaction> GetTransactions(DateTime start, DateTime end, bool unprocessedOnly = true, int skip = 0, int take = 10000)
        {
            Contract.Requires<ArgumentOutOfRangeException>(skip >= 0, "skip must be a postive integer");
            Contract.Requires<ArgumentOutOfRangeException>(take >= 0, "take must be a positive integer");

            return Trim(_transaction.GetTransactions(Sessions.User.CID, start, end, unprocessedOnly, skip, take));
        }

        /// <summary>
        /// Gets the transactions that are mapped to the specified line item.
        /// </summary>
        /// <param name="lineItemId">The ID of the line item whose mapped transactions we are retreiving</param>
        /// <returns>The list of transactions mapped to the specified line item</returns>
        public IEnumerable<LinkedTransaction> GetTransactionsForLineItem(int lineItemId)
        {
            Contract.Requires<ArgumentOutOfRangeException>(lineItemId > 0, "A lineItemId less than or equal to zero is invalid, and likely indicates the lineItemId was not set properly");

            return _transaction.GetTransactionsForLineItem(Sessions.User.CID, lineItemId);
        }

        #region trim transactions for a lean data structure
        /// <summary>
        /// A trimmed version of Transaction with only required fields 
        /// </summary>
        public class LeanTransaction : Dictionary<string, object>
        {
            //nothing should be here 
        }

        /// <summary>
        /// These are minimum required fields for transaction UI to work 
        /// </summary>
        private static List<string> _requiredTransactionColoumns = new List<string>()
        {   "TransactionId", //internal transacion ID 
            "ClientTransactionId", //Client transaction ID
            "Amount", //amount on the transaction 
            "AmountAttributed", //Amount attributed already 
            "AmountRemaining", //Amount unattributed (remaining)
            "TransactionDate", // Transaction date 
        };  

        /// <summary>
        /// This function build a lean version of transaction that only contain the columns a client is needed.
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        private List<LeanTransaction> Trim(IEnumerable<Transaction> transactions)
        {
            //Get all headers the current client requests 
            var headers = new HashSet<string>(_transaction.GetHeaderMappings(Sessions.User.CID).Select(x => x.Hive9Header).ToList());

            //make sure required headers are always included for UI to function correctly 
            foreach(var rh in _requiredTransactionColoumns)
            {
                if (!headers.Contains(rh))
                {
                    headers.Add(rh);
                }
            }

            //to hold the fimal list of trimmed transactions 
            var leanTransactions = new List<LeanTransaction>();

            foreach (var trans in transactions)
            {
                var lean = new LeanTransaction();
                foreach (var header in headers)
                {
                    lean.Add(header, trans.GetType().GetProperty(header).GetValue(trans));
                }

                leanTransactions.Add(lean);
            }

            return leanTransactions;
        }

        #endregion 
    }
}