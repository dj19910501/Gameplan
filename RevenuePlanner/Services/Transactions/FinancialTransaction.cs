using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;

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
            throw new NotImplementedException();
        }

        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            throw new NotImplementedException();
        }

        public List<TransactionHeaderMapping> GetHeaderMappings(int clientId)
        {
            throw new NotImplementedException();
        }

        public List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {
            throw new NotImplementedException();
        }

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true)
        {
            throw new NotImplementedException();
        }

        public List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true, List<ColumnFilter> columnFilters = null, int pageIndex = 1, int pageSize = 10000)
        {
            throw new NotImplementedException();
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