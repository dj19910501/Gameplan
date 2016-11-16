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
        public int TransactionID { get; set; }

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
        public DateTime DateProcessed { get; set; }
    }

    /// <summary>
    /// TransactionLineItemMapping is to capture how a transaction is attributing towards a line item
    /// </summary>
    public class TransactionLineItemMapping
    {
        public int TransactionId { get; set; }
        public int LineItemId { get; set; }
        public double Amount { get; set; }
        public DateTime DateModified { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime DateProcessed { get; set; }
    }

    /// <summary>
    /// TransactionHeaderMapping intends to customize transaction display on UI per client taste
    /// </summary>
    public class TransactionHeaderMapping
    {
        public string ClientHeader { get; set; }
        public string Hive9Header { get; set; }
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
    interface ITransaction
    {
        /// <summary>
        /// Returns total number of transactions  
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="unprocessdedOnly"></param>
        /// <returns></returns>
        int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true);
        /// <summary>
        /// returns a page of transactions according page size and page index
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="unprocessdedOnly"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true, List<ColumnFilter> columnFilters = null,  int pageIndex = 1, int pageSize = 10000);

        /// <summary>
        /// Search for transactions that matches searchText in any textual field
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="searchText"></param>
        /// <param name="unprocessdedOnly"></param>
        /// <returns></returns>
        List<Transaction> SearchForTransactions(int clientId, DateTime start, DateTime end, string searchText, bool unprocessdedOnly = true);

        /// <summary>
        /// This method handles both new mapping as well as updating existing mappings 
        /// </summary>
        /// <param name="transactionLineItemMappings"></param>
        void AttrbuteTransactionsToLineItems(List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId);
        /// <summary>
        /// Client may customize transaction column headers (to display on UI) to their taste by providing headers mappings
        /// from our standard header to theirs. Internally, we will always use standard headings (see Transaction class)
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        List<TransactionHeaderMapping> GetHeaderMappings(int clientId);
<<<<<<< HEAD

        void DeleteTransactionLineItemMapping(int mappingId);
=======
>>>>>>> afe55ed2e7ce4c2c108b0dbff11840aae0c516ec
    }
}
