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

    public class LineItemsGroupedByTactic
    {
        public int TacticId { get; set; }
        public string Title { get; set; } //note: sys_gen_balance will be represented as if its a tactic
        public double TotalLinkedCost{ get; set; }
        public double PlannedCost { get; set; }
        public double ActualCost { get; set; }   
        public List<LinkedLineItem> LineItems {get; set;}
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
        Percent = 5,
        Identifier = 6,
    }

    /// <summary>
    /// TransactionHeaderMapping intends to customize transaction display on UI per client taste
    /// </summary>
    public class TransactionHeaderMapping
    {
        public string ClientHeader { get; set; }
        public string Hive9Header { get; set; }
        public HeaderMappingFormat HeaderFormat { get; set; }
    }

    /// <summary>
    /// Column filters will filter transactions by a list of keywords per column
    /// Multiple column filters are dipicted by a list of column filters
    /// </summary>
    public class ColumnFilter
    {
        public string ColumnName { get; set; }
        public List<string> filters {get; set;}
    }   

    /// <summary>
    /// Operational interface for transactions
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Returns total number of transactions  
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="unprocessedOnly"></param>
        /// <returns></returns>
        int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessedOnly = true);
        /// <summary>
        /// returns at most <param name="take" /> transactions after skipping the first <param name="skip" />
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="unprocessedOnly"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessedOnly = true, List<ColumnFilter> columnFilters = null,  int skip = 0, int take = 10000);

        List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int clientId, int transactionId);

        /// <summary>
        /// Reverse listing of transactions per line item 
        /// </summary>
        /// <param name="lineItemId"></param>
        /// <returns></returns>
        List<Transaction> GetTransactionsForLineItem(int clientId, int lineItemId);

        /// <summary>
        /// Search for transactions that matches searchText in any textual field
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="searchText"></param>
        /// <param name="unprocessedOnly"></param>
        /// <returns></returns>
        //List<Transaction> SearchForTransactions(int clientId, DateTime start, DateTime end, string searchText, bool unprocessedOnly = true);

        /// <summary>
        /// This method handles both new mapping as well as updating existing mappings 
        /// </summary>
        /// <param name="transactionLineItemMappings"></param>
        void SaveTransactionToLineItemMapping(int clientId, List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId);
        /// <summary>
        /// Client may customize transaction column headers (to display on UI) to their taste by providing headers mappings
        /// from our standard header to theirs. Internally, we will always use standard headings (see Transaction class)
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        List<TransactionHeaderMapping> GetHeaderMappings(int clientId);

        void DeleteTransactionLineItemMapping(int clientId, int mappingId);
    }
}
