//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RevenuePlanner.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Transaction
    {
        public Transaction()
        {
            this.TransactionLineItemMappings = new HashSet<TransactionLineItemMapping>();
        }
    
        public int TransactionId { get; set; }
        public int ClientID { get; set; }
        public string ClientTransactionID { get; set; }
        public string TransactionDescription { get; set; }
        public decimal Amount { get; set; }
        public string Account { get; set; }
        public string AccountDescription { get; set; }
        public string SubAccount { get; set; }
        public string Department { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public System.DateTime AccountingDate { get; set; }
        public string Vendor { get; set; }
        public string PurchaseOrder { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string CustomField4 { get; set; }
        public string CustomField5 { get; set; }
        public string CustomField6 { get; set; }
        public Nullable<int> LineItemId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<double> AmountAttributed { get; set; }
        public Nullable<System.DateTime> LastProcessed { get; set; }
    
        public virtual ICollection<TransactionLineItemMapping> TransactionLineItemMappings { get; set; }
    }
}
