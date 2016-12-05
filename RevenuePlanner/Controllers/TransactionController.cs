﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services.Transactions;
using RevenuePlanner.Controllers.Filters;

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
            if (transaction == null)
            {
                throw new ArgumentNullException("Transaction", "Argument Transaction cannot be null");
            }

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
            if (transactionLineItemMappings == null)
            {
                throw new ArgumentNullException("transactionLineItemMappings", "transactionLineItemMappings cannot be null.");
            }

            _transaction.SaveTransactionToLineItemMapping(Sessions.User.CID, transactionLineItemMappings, Sessions.User.ID);
        }

        /// <summary>
        /// Delete an individual line item mapping. This just deletes the link between a transaction and a line item, neither
        /// the transaction nor the line item are modified or deleted during this call.
        /// This is a POST call, however there is no request body.
        /// </summary>
        /// <param name="mappingId">The id of the transaction line item mapping reference to be deleted.</param>
        [System.Web.Http.HttpPost]
        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            if (mappingId <= 0)
            {
                throw new ArgumentOutOfRangeException("mappingId", "A mappingId less than or equal to zero is invalid, and likely indicates the mappingId was not set properly");
            }

            _transaction.DeleteTransactionLineItemMapping(Sessions.User.CID, mappingId);
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
            if (transactionId <= 0)
            {
                throw new ArgumentOutOfRangeException("transactionId", "A transactionId less than or equal to zero is invalid, and likely indicates the transactionId was not set properly");
            }

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
        public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end, bool unprocessedOnly = true, int skip = 0, int take = 10000)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException("skip", "skip must be a postive integer");
            }
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException("take", "take must be a positive integer");
            }

            return _transaction.GetTransactions(Sessions.User.CID, start, end, unprocessedOnly, skip, take);
        }

        /// <summary>
        /// Gets the transactions that are mapped to the specified line item.
        /// </summary>
        /// <param name="lineItemId">The ID of the line item whose mapped transactions we are retreiving</param>
        /// <returns>The list of transactions mapped to the specified line item</returns>
        public IEnumerable<Transaction> GetTransactionsForLineItem(int lineItemId)
        {
            if (lineItemId <= 0)
            {
                throw new ArgumentOutOfRangeException("lineItemId", "A lineItemId less than or equal to zero is invalid, and likely indicates the lineItemId was not set properly");
            }

            return _transaction.GetTransactionsForLineItem(Sessions.User.CID, lineItemId);
        }

    }
}