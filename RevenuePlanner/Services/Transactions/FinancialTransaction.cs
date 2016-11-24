using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;
using System.Data;

namespace RevenuePlanner.Services.Transactions
{
    public class FinancialTransaction : ITransaction
    {
        private MRPEntities _database;
        public FinancialTransaction(MRPEntities database)
        {
            _database = database;
        }

        public void SaveTransactionToLineItemMapping(List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId)
        {

            foreach (TransactionLineItemMapping tlim in transactionLineItemMappings)
            {

                Models.TransactionLineItemMapping modelTLIM = _database.TransactionLineItemMappings.Where(dbtlim => dbtlim.TransactionId == tlim.TransactionId && dbtlim.LineItemId == tlim.LineItemId).SingleOrDefault();

                if (modelTLIM == null)
                {
                    modelTLIM = new Models.TransactionLineItemMapping();
                    modelTLIM.TransactionId = tlim.TransactionId;
                    modelTLIM.LineItemId = tlim.LineItemId;
                    _database.Entry(modelTLIM).State = EntityState.Added;
                }
                else
                {
                    _database.Entry(modelTLIM).State = EntityState.Modified;
                }

                modelTLIM.Amount = tlim.Amount;
                modelTLIM.DateModified = DateTime.Now;
                modelTLIM.ModifiedBy = modifyingUserId;
            }

            _database.SaveChanges();
        }

        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            // TODOWCR: I don't like having to look this up to delete it
            Models.TransactionLineItemMapping modelTLIM = _database.TransactionLineItemMappings.Where(dbtlim => dbtlim.TransactionLineItemMappingId == mappingId).SingleOrDefault();

            if (modelTLIM != null)
            {
                _database.Entry(modelTLIM).State = EntityState.Deleted;
                _database.SaveChanges();
            }
        }

        public List<TransactionHeaderMapping> GetHeaderMappings(int clientId)
        {
            throw new NotImplementedException();
        }

        public List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {   
            var datalist = _database.GetLinkedLineItemsForTransaction(transactionId).ToList();

            throw new NotImplementedException();
        }

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true)
        {
            int count = _database.Transactions.Count(transaction => (transaction.ClientID == clientId) &&
                                                    ((unprocessdedOnly && transaction.LastProcessed == null) || !unprocessdedOnly));
            return count;

        }

        public List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true, List<ColumnFilter> columnFilters = null, int pageIndex = 1, int pageSize = 10000)
        {

            // TODOWCR: For paging, what do we order by? Creation date?
            IQueryable<Transaction> sqlQuery =
                from transaction in _database.Transactions
                where transaction.ClientID == clientId && ((unprocessdedOnly && transaction.LastProcessed == null)  || !unprocessdedOnly)
                orderby transaction.DateCreated
                select new Transaction {
                    TransactionId = transaction.TransactionId,
                    ClientTransactionId = transaction.ClientTransactionID,
                    TransactionDescription = transaction.TransactionDescription,
                    Amount = (double)transaction.Amount,
                    Account = transaction.Account,
                    AccountDescription = transaction.AccountDescription,
                    SubAccount = transaction.SubAccount,
                    Department = transaction.Department,
                    TransactionDate = transaction.TransactionDate != null ? (DateTime)transaction.TransactionDate : DateTime.MinValue,
                    AccountingDate = transaction.AccountingDate,
                    Vendor = transaction.Vendor,
                    PurchaseOrder = transaction.PurchaseOrder,
                    CustomField1 = transaction.CustomField1,
                    CustomField2 = transaction.CustomField2,
                    CustomField3 = transaction.CustomField3,
                    CustomField4 = transaction.CustomField4,
                    CustomField5 = transaction.CustomField5,
                    CustomField6 = transaction.CustomField6,
                    LineItemId = transaction.LineItemId != null ? (int)transaction.LineItemId : 0,
                    DateCreated = transaction.DateCreated,
                    AmountAttributed = transaction.AmountAttributed != null ? (double)transaction.AmountAttributed : 0.0,
                    LastProcessed = transaction.LastProcessed
                };

            // TODOWCR: It appears that linq's query for pagination is not terribly efficient (appears to be 3 embedded selects vs 2)
            return sqlQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<Transaction> SearchForTransactions(int clientId, DateTime start, DateTime end, string searchText, bool unprocessdedOnly = true)
        {
            throw new NotImplementedException();
        }

        public List<Transaction> GetTransactionsForLineItem(int lineItemId)
        {
            throw new NotImplementedException();
        }
    }
}