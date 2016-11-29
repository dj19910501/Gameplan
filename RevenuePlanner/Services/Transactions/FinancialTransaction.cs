using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;
using System.Data;
using System.Data.SqlClient;

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

                // TODOWCR: Figure out how to not run this on every iteration
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
                modelTLIM.DateModified = System.DateTime.Now;
                modelTLIM.ModifiedBy = modifyingUserId;
            }

            _database.SaveChanges();
        }

        public void DeleteTransactionLineItemMapping(int mappingId)
        {
            // TODOWCR: I don't like having to look this up to delete it, how to otherwise guard against deleting non-existent item?
            Models.TransactionLineItemMapping modelTLIM = _database.TransactionLineItemMappings.Where(dbtlim => dbtlim.TransactionLineItemMappingId == mappingId).SingleOrDefault();

            if (modelTLIM != null)
            {
                _database.Entry(modelTLIM).State = EntityState.Deleted;
                _database.SaveChanges();
            }
        }

        /// <summary>
        /// 
        ///NOTE: 
        ///this is simply a lookup table for customer preferred captions on transaction list.
        ///In addition, it also determines columns needed to on UI. For this reason, controller action 
        ///should consider using this API to trim down columns before shipping transactions out to UI
        ///since not all columns are needed by UI per client. 
        ///
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public List<TransactionHeaderMapping> GetHeaderMappings(int clientId)
        {
            //Use default for now until real customers are using transaction feature.
            return _defaultHeaderMapping;
        }

        public List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int transactionId)
        {
            // TODOWCR: Is there a better way to get multiple results from a stored procedure?
            DataSet dataset = new DataSet();
            SqlCommand command = new SqlCommand("GetLinkedLineItemsForTransaction", _database.Database.Connection as SqlConnection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TransactionId", transactionId);
            SqlDataAdapter adp = new SqlDataAdapter(command);
            adp.Fill(dataset);
            
            DataTable tacticsTable = dataset.Tables[0];
            DataTable lineItemsTable = dataset.Tables[1];
            Dictionary<int, LineItemsGroupedByTactic> lineItemsByTactic = new Dictionary<int, LineItemsGroupedByTactic>();

            if (tacticsTable != null)
            {
                foreach (DataRow row in tacticsTable.AsEnumerable())
                {
                    LineItemsGroupedByTactic ligbt = new LineItemsGroupedByTactic()
                    {
                        TacticId = Convert.ToInt32(row["TacticId"]),
                        Title = Convert.ToString(row["Title"]),
                        PlannedCost = Convert.ToDouble(row["PlannedCost"]),
                        TotalLinkedCost = Convert.ToDouble(row["TotalLinkedCost"]),
                        ActualCost = Convert.ToDouble(row["TotalActual"]),
                        LineItems = new List<LinkedLineItem>()
                    };

                    lineItemsByTactic.Add(ligbt.TacticId, ligbt);
                }
            }

            if (lineItemsTable != null)
            {
                foreach (DataRow row in lineItemsTable.AsEnumerable())
                {
                    TransactionLineItemMapping tlim = new TransactionLineItemMapping()
                    {
                        TransactionLineItemMappingId = Convert.ToInt32(row["TransactionLineItemMappingId"]),
                        Amount = Convert.ToDouble(row["TotalLinkedCost"]),
                        TransactionId = Convert.ToInt32(row["TransactionId"]),
                        LineItemId = Convert.ToInt32(row["PlanLineItemId"])
                    };

                    LinkedLineItem item = new LinkedLineItem()
                    {
                        LineItemId = Convert.ToInt32(row["PlanLineItemId"]),
                        Title = Convert.ToString(row["Title"]),
                        Cost = Convert.ToDouble(row["Cost"]),
                        Actual = Convert.ToDouble(row["Actual"]),
                        TacticTitle = Convert.ToString(row["TacticTitle"]),
                        ProgramTitle = Convert.ToString(row["ProgramTitle"]),
                        CampaignTitle = Convert.ToString(row["CampaignTitle"]),
                        PlanTitle = Convert.ToString(row["PlanTitle"]),
                        LineItemMapping = tlim
                    };

                    int tacticId = Convert.ToInt32(row["TacticId"]);
                    if (lineItemsByTactic.ContainsKey(tacticId))
                    {
                        lineItemsByTactic[tacticId].LineItems.Add(item);
                    }
                    else
                    {
                        // TODOWCR: Something is wrong with the query
                    }
                }
            }

            return new List<LineItemsGroupedByTactic>(lineItemsByTactic.Values);
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

        public List<Transaction> GetTransactionsForLineItem(int lineItemId)
        {
            IQueryable<Transaction> sqlQuery =
                from tlim in _database.TransactionLineItemMappings
                join transaction in _database.Transactions on tlim.TransactionId equals transaction.TransactionId
                where tlim.LineItemId == lineItemId
                select new Transaction
                {
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

            return sqlQuery.ToList();
        }

        #region Internal Impkementation 
        private static List<TransactionHeaderMapping> _defaultHeaderMapping = 
            new List<TransactionHeaderMapping>() {
                new TransactionHeaderMapping() { ClientHeader = "Transaction ID", Hive9Header = "ClientTransactionId"},
                new TransactionHeaderMapping() { ClientHeader = "Purchase Order", Hive9Header = "PurchaseOrder"},
                new TransactionHeaderMapping() { ClientHeader = "Vendor", Hive9Header = "Vendor"},
                new TransactionHeaderMapping() { ClientHeader = "Amount", Hive9Header = "Amount"},
                new TransactionHeaderMapping() { ClientHeader = "Description", Hive9Header = "TransactionDescription"},
                new TransactionHeaderMapping() { ClientHeader = "Account", Hive9Header = "Account"},
                new TransactionHeaderMapping() { ClientHeader = "Date", Hive9Header = "TransactionDate"},
                new TransactionHeaderMapping() { ClientHeader = "Department", Hive9Header = "Department"},
            };
        #endregion Internal Implementation 
    }
}