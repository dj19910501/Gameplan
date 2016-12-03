using System;
using System.Collections.Generic;

namespace RevenuePlanner.Services.Transactions
{
    /// <summary>
    /// Transaction captures all fields from a client transaction integration
    /// plus a couple plan tracking fields 
    /// </summary>
    public class Transaction
    {

        /// <summary>
        /// Hive9 Plan internal auto ID
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// Required and unique per client 
        /// </summary>
        public string ClientTransactionId { get; set; }
        public string TransactionDescription { get; set; }

        /// <summary>
        /// Required mapping
        /// Ammount must be extracted from transaction data and put in this field during import process (step 1) 
        /// </summary>
        public double Amount { get; set; }
        public string Account { get; set; }
        public string AccountDescription { get; set; }
        public string SubAccount { get; set; }
        public string Department { get; set; }
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Required mapping. 
        /// Date that determines Period in Plan app (step 1)
        /// </summary>
        public DateTime AccountingDate { get; set; }
        public string Vendor { get; set; }
        public string PurchaseOrder { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }
        public string CustomField6 { get; set; }

        /// <summary>
        /// If a client transaction has already linked to a line item, the integration will have to put the 
        /// line item ID in this field during data import process (step 1). This will trigger an auto attribution without mapping 
        /// we as know the specific line item this transaction should be attributed to. 
        /// </summary>
        public int LineItemId { get; set; }

        /// <summary>
        /// Date integration puts the data in (step 1 timestamp). 
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Total amount attributed from this transaction calculated from all attributing line items
        /// The balance will be Amount - AmountAttributed (step 3)
        /// </summary>
        public double AmountAttributed { get; set; }

        /// <summary>
        /// Date and time this transaction was processed by batch attribution process (step 3) 
        /// </summary>
        public DateTime? LastProcessed { get; set; }
    }
    
    /// <summary>
    /// List of line items grouped by tactic along with info regarding the tactic and some tactic level cost values
    /// </summary>
    public class LineItemsGroupedByTactic
    {
        public int TacticId { get; set; }
        public string Title { get; set; } //note: sys_gen_balance will be represented as if its a tactic
        public double TotalLinkedCost { get; set; }
        public double PlannedCost { get; set; }
        public double ActualCost { get; set; }
        public List<LinkedLineItem> LineItems { get; set; }
    }

    /// <summary>
    /// List of linked line items, includes some information about the heritage of the line item along with a 
    /// transaction line item mapping object that can be used to modify such a mapping.
    /// </summary>
    public class LinkedLineItem
    {
        public int LineItemId { get; set; }
        public string Title { get; set; }
        public double Cost { get; set; }
        public double Actual { get; set; }
        public TransactionLineItemMapping LineItemMapping { get; set; }

        public string TacticTitle { get; set; }
        public string ProgramTitle { get; set; }
        public string CampaignTitle { get; set; }
        public string PlanTitle { get; set; }
    }

    /// <summary>
    /// TransactionLineItemMapping is to capture how a transaction is attributing towards a line item
    /// </summary>
    public class TransactionLineItemMapping
    {
        public int TransactionLineItemMappingId { get; set; } 
        public int TransactionId { get; set; }
        public int LineItemId { get; set; }
        public double Amount { get; set; }
        public DateTime DateModified { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime DateProcessed { get; set; }
    }

    /// <summary>
    /// The format for the custom mapped fields, instead of specific css we are just categorizing them at the moment.
    /// I explicitly gave values to the enum because this information will likely be pulled from a DB some day.
    /// </summary>
    public enum HeaderMappingFormat
    {
        Label = 0,
        Text = 1,
        Date = 2,
        Currency = 3,
        Number = 4,
        Percent = 5 
    }

    /// <summary>
    /// TransactionHeaderMapping intends to customize transaction display on UI per client taste
    /// </summary>
    public class TransactionHeaderMapping
    {
        public string ClientHeader { get; set; }
        public string Hive9Header { get; set; }
        public HeaderMappingFormat HeaderFormat { get; set; }
        public int ExpectedCharacterLength { get; set; }
        public int precision { get; set; }
    }

    /// <summary>
    /// Operational interface for transactions
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Get the number of transactions for the given time period. Can Optionally pass in whether to get only unprocessed
        /// transactions or to return all transactions for the period.
        /// </summary>
        /// <param name="clientId">The clientId whose transactions we are counting</param>
        /// <param name="start">Non-null start date, transactions will be counted that occur on or after this date</param>
        /// <param name="end">Non-null end date, transaction will be counted that occur before or on this date</param>
        /// <param name="unprocessedOnly">Optional. Whether to return only unprocessed transactions. True == return unprocessed only, false == return all</param>
        /// <returns>Integer indicating the number of transactions</returns>
        int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessedOnly = true);

        /// <summary>
        /// Get transactions for the given time period. Can optionally pass in whether to get only unprocessed transactions
        /// or to return all transactions for this period. Can also optionally pass in pagination information to skip a
        /// certain number of transactions and take a certain number of transactions
        /// </summary>
        /// <param name="clientId">The clientId whose transactions we are retrieving</param>
        /// <param name="start">Non-null start date, transactions will be returned that occur on or after this date</param>
        /// <param name="end">Non-null end date , transactions will be returned that occur before or on this date</param>
        /// <param name="unprocessedOnly">Optional. Whether to return only unprocessed transactions or all of them. True == return unprocess only, false == return all</param>
        /// <param name="skip">Optional. This number of transactsions will be skipped in the results returned</param>
        /// <param name="take">Optiona. This is the top number of transactions that will be returned by this call, fewer may be returned if there are fewer transactions available</param>
        /// <returns>returns at most <param name="take" /> transactions after skipping the first <param name="skip" /></returns>
        List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessedOnly = true, int skip = 0, int take = 10000);

        /// <summary>
        /// Gets the line items that are mapped to the specified transaction. 
        /// The results are grouped by tactic, so if a transaction has line items mapped to multiple tactics, the returned list
        /// will be a list that contains tactic information and itself contains a list of all the line items associated with that
        /// tactic. Included in the results are the actual TransactionLineMapping information which can be utilized to modify
        /// a mapping via SaveTransactionToLineItemMapping
        /// </summary>
        /// <param name="clientId">The clientId whose line items we are retrieving</param>
        /// <param name="transactionId">The transaction id whose mapped line items are being retrieved</param>
        /// <returns>A Lit of line items associated with the transaction id</returns>
        List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int clientId, int transactionId);

        /// <summary>
        /// Gets the transactions that are mapped to the specified line item.
        /// </summary>
        /// <param name="clientId">The clientId whose transactions we are retrieving</param>
        /// <param name="lineItemId">The ID of the line item whose mapped transactions we are retreiving</param>
        /// <returns>The list of transactions mapped to the specified line item</returns>
        List<Transaction> GetTransactionsForLineItem(int clientId, int lineItemId);

        /// <summary>
        /// Save a list of Transaction to Line Item mappings with the associated amounts. This includes new mappings as well as 
        /// udpates to existing mappings.
        /// </summary>
        /// <param name="clientId">The clientId whose transactions mappins we are saving</param>
        /// <param name="transactionLineItemMappings">The list of mappings to create or update.</param>
        /// <param name="modifyingUserId">The user id who is initiating the creation/modification of the mappins</param>
        void SaveTransactionToLineItemMapping(int clientId, List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId);

        /// <summary>
        /// Return a list of "Display name" to "hive9 data name" mappings. This is used to map columns in the transactoin data
        /// to column header names that will be displayed in the UI. This structure will also include a basic "field type"
        /// value that indicates if each column is text/percent/currency/etc. There should not be hard coded formatting 
        /// settings in this file.
        /// </summary>
        /// <param name="clientId">The clientId whose header mappings we are retrieving</param>
        /// <returns>List of transaction header mapping objects, each object represents a column of transaction data</returns>
        List<TransactionHeaderMapping> GetHeaderMappings(int clientId);

        /// <summary>
        /// Delete an individual line item mapping. This just deletes the link between a transaction and a line item, neither
        /// the transaction nor the line item are modified or deleted during this call.
        /// This is a POST call, however there is no request body.
        /// </summary>
        /// <param name="clientId">The clientId whose mappings we are deleting</param>
        /// <param name="mappingId">The id of the transaction line item mapping reference to be deleted.</param>
        void DeleteTransactionLineItemMapping(int clientId, int mappingId);
    }
}
