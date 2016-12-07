using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics.Contracts;

namespace RevenuePlanner.Services.Transactions
{
    public class FinancialTransaction : ITransaction
    {
        private MRPEntities _database;
        public FinancialTransaction(MRPEntities database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database", "MRPEntities database cannot be null.");
            }

            _database = database;
        }

        /// <summary>
        /// Return a dictionary of any line item mappings that exist in the db
        /// </summary>
        /// <param name="transactionLineItemMappings">list of transaction to line item mappings</param>
        /// <returns></returns>
        private Dictionary<int, Models.TransactionLineItemMapping> GetExistingLineItemMappings(List<TransactionLineItemMapping> transactionLineItemMappings)
        {
            // Get list of distinct ids, will include id '0' which if there are truely new items (non-null int => 0), 0 will not match an entry in the db.
            List<int> mappingIds = transactionLineItemMappings.Select(item => item.TransactionLineItemMappingId).Distinct().ToList();

            IQueryable<Models.TransactionLineItemMapping> sqlQuery = from tlim in _database.TransactionLineItemMappings
                                                                     where mappingIds.Contains(tlim.TransactionLineItemMappingId)
                                                                     select tlim;

            return sqlQuery.ToDictionary(tlim => tlim.TransactionLineItemMappingId, tlim => tlim);
        }

        /// <summary>
        /// Insert or update the Line Item Mapping checking the conditionals to verify the transactionId and LineItemId belong 
        /// to the ClientId.
        /// </summary>
        /// <param name="clientId">ClientId to check against</param>
        /// <param name="mapping">The Entity object to insert/update</param>
        /// <param name="modifyingUserId">The user making the change</param>
        /// <param name="existingMapping">Whether Entity exists in the database or not</param>
        private void SaveTransactionLineItemMapping(int clientId, Models.TransactionLineItemMapping mapping, int modifyingUserId, bool existingMapping)
        {
            List<SqlParameter> parameters = new List<SqlParameter>(7);
            parameters.Add(new SqlParameter("@TransactionId", mapping.TransactionId));
            parameters.Add(new SqlParameter("@LineItemId", mapping.LineItemId));
            parameters.Add(new SqlParameter("@Amount", mapping.Amount));
            parameters.Add(new SqlParameter("@DateModified", mapping.DateModified));
            parameters.Add(new SqlParameter("@ModifiedBy", modifyingUserId));
            parameters.Add(new SqlParameter("@ClientId", clientId));
            
            string sqlQuery;

            if (existingMapping)
            {
                parameters.Add(new SqlParameter("@TransactionLineItemMappingId", mapping.TransactionLineItemMappingId));

                sqlQuery = string.Format(@"update TransactionLineItemMapping 
                                            set TransactionId = @TransactionId, LineItemId = @LineITemId, Amount = @Amount, DateModified = @DateModified, ModifiedBy = @ModifiedBy
                                            from Transactions T
                                            join LineItemDetail L on L.ClientId = T.ClientId
                                            where T.TransactionId = @TransactionId and 
                                                L.PlanLineItemId = @LineitemId and 
                                                TransactionLineItemMappingId = @TransactionLineItemMappingId and
                                                T.clientId = @ClientId");
            }
            else
            {
                sqlQuery = string.Format(@"insert into TransactionLineItemMapping (TransactionId, LineItemId, Amount, DateModified, ModifiedBy)
	                                        select T.transactionId, L.PlanLineItemId, @Amount, @DateModified, @ModifiedBy 
	                                        from Transactions T
	                                        join LineItemDetail L on L.ClientId = T.clientid  
	                                        where T.TransactionId = @TransactionId and 
                                                L.PlanLineItemId = @LineItemId and 
                                                T.ClientID = @ClientId");
            }

            _database.Database.ExecuteSqlCommand(sqlQuery, parameters.ToArray());

        }
        public void SaveTransactionToLineItemMapping(int clientId, List<TransactionLineItemMapping> transactionLineItemMappings, int modifyingUserId)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientID less than or equal to zero is invalid, and likely indicates the clientID was not set properly");
            }
            if (transactionLineItemMappings == null)
            {
                throw new ArgumentNullException("transactionLineItemMappings", "transactionLineItemsMappings cannot be null");
            }
            if (modifyingUserId <= 0)
            {
                throw new ArgumentOutOfRangeException("modifyingUserId", "A modifyingUserId less than or equal to zero is invalid, and likely indicates the modifyingUserId was not set properly");
            }

            Dictionary<int, Models.TransactionLineItemMapping> existingMappings = GetExistingLineItemMappings(transactionLineItemMappings);

            foreach (TransactionLineItemMapping tlim in transactionLineItemMappings)
            {
                Models.TransactionLineItemMapping modelTlim = null;

                if (existingMappings.ContainsKey(tlim.TransactionLineItemMappingId))
                {
                    modelTlim = existingMappings[tlim.TransactionLineItemMappingId];
                }
                else
                {
                    modelTlim = new Models.TransactionLineItemMapping();
                    modelTlim.TransactionId = tlim.TransactionId;
                    modelTlim.LineItemId = tlim.LineItemId;
                }

                modelTlim.Amount = tlim.Amount;
                modelTlim.DateModified = System.DateTime.Now;
                modelTlim.ModifiedBy = modifyingUserId;

                SaveTransactionLineItemMapping(clientId, modelTlim, modifyingUserId, existingMappings.ContainsKey(modelTlim.TransactionLineItemMappingId));
            }

        }

        public void DeleteTransactionLineItemMapping(int clientId, int mappingId)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }
            if (mappingId <= 0)
            {
                throw new ArgumentOutOfRangeException("mappingId", "A mappingId less than or equal to zero is invalid, and likely indicates the mappingId was not set properly");
            }

            IQueryable<Models.TransactionLineItemMapping> sqlQuery = from tlim in _database.TransactionLineItemMappings
                                                                     join transaction in _database.Transactions on tlim.TransactionId equals transaction.TransactionId
                                                                     where tlim.TransactionLineItemMappingId == mappingId && transaction.ClientID == clientId
                                                                     select tlim;

            Models.TransactionLineItemMapping modelTLIM = sqlQuery.SingleOrDefault();

            if (modelTLIM != null)
            {
                _database.Entry(modelTLIM).State = EntityState.Deleted;
                _database.SaveChanges();
            }
        }

        /// <summary>
        ///NOTE: 
        ///this is simply a lookup table for customer preferred captions on transaction list.
        ///In addition, it also determines columns needed to on UI. For this reason, controller action 
        ///should consider using this API to trim down columns before shipping transactions out to UI
        ///since not all columns are needed by UI per client. 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public List<TransactionHeaderMapping> GetHeaderMappings(int clientId)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }

            //Use default for now until real customers are using transaction feature.
            return _defaultHeaderMapping;
        }

        public List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int clientId, int transactionId)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }
            if (transactionId <= 0)
            {
                throw new ArgumentOutOfRangeException("transactionId", "A transactionId less than or equal to zero is invalid, and likely indicates the transactionId was not set properly");
            }

            DataSet dataset = new DataSet();
            SqlCommand command = new SqlCommand("GetLinkedLineItemsForTransaction", _database.Database.Connection as SqlConnection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ClientID", clientId);
            command.Parameters.AddWithValue("@TransactionId", transactionId);           
            SqlDataAdapter adp = new SqlDataAdapter(command);
            adp.Fill(dataset);
            
            DataTable tacticsTable = dataset.Tables[0];
            DataTable lineItemsTable = dataset.Tables[1];
            Dictionary<int, LineItemsGroupedByTactic> lineItemsByTacticId = new Dictionary<int, LineItemsGroupedByTactic>();

            if (tacticsTable != null)
            {
                foreach (DataRow row in tacticsTable.AsEnumerable())
                {
                    LineItemsGroupedByTactic ligbt = new LineItemsGroupedByTactic()
                    {
                        TacticId = row.Field<int>("TacticId"),
                        Title = row.Field<string>("Title"),
                        PlannedCost = row.Field<double>("PlannedCost"),
                        TotalLinkedCost = row.Field<double>("TotalLinkedCost"),
                        ActualCost = row.Field<double>("TotalActual"),
                        LineItems = new List<LinkedLineItem>()
                    };

                    lineItemsByTacticId.Add(ligbt.TacticId, ligbt);
                }
            }

            if (lineItemsTable != null)
            {
                foreach (DataRow row in lineItemsTable.AsEnumerable())
                {
                    TransactionLineItemMapping tlim = new TransactionLineItemMapping()
                    {
                        TransactionLineItemMappingId = row.Field<int>("TransactionLineItemMappingId"),
                        Amount = row.Field<double>("TotalLinkedCost"),
                        TransactionId = row.Field<int>("TransactionId"),
                        LineItemId = row.Field<int>("PlanLineItemId")
                    };

                    LinkedLineItem item = new LinkedLineItem()
                    {
                        LineItemId = row.Field<int>("PlanLineItemId"),                        
                        Title = row.Field<string>("Title"),
                        Cost = row.Field<double>("Cost"),
                        Actual = row.Field<double>("Actual"),
                        TacticTitle = row.Field<string>("TacticTitle"),
                        ProgramTitle = row.Field<string>("ProgramTitle"),
                        CampaignTitle = row.Field<string>("CampaignTitle"),
                        PlanTitle = row.Field<string>("PlanTitle"),
                        LineItemMapping = tlim
                    };

                    int tacticId = row.Field<int>("TacticId");
                    if (lineItemsByTacticId.ContainsKey(tacticId))
                    {
                        lineItemsByTacticId[tacticId].LineItems.Add(item);
                    }
                    else
                    {
                        throw new InvalidOperationException("Inconsistent data state. It appears SP GetLinkedLineItemsForTransaction returned line items but did not include associated Tactics.");
                    }
                }
            }

            return new List<LineItemsGroupedByTactic>(lineItemsByTacticId.Values);
        }

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }

            int count = _database.Transactions.Count(transaction => (transaction.ClientID == clientId) &&
                                                    ((unprocessdedOnly && transaction.LastProcessed == null) || !unprocessdedOnly) &&
                                                    transaction.DateCreated >= start &&
                                                    transaction.DateCreated <= end);
            return count;

        }

        public List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unprocessdedOnly = true, int skip = 0, int take = 10000)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException("skip", "skip must be a postive integer");
            }
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException("take", "take must be a positive integer");
            }

            IQueryable<Transaction> sqlQuery =
                from transaction in _database.Transactions
                where transaction.ClientID == clientId && ((unprocessdedOnly && transaction.LastProcessed == null)  || !unprocessdedOnly && transaction.DateCreated >= start && transaction.DateCreated <= end)
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
            return sqlQuery.Skip(skip).Take(take).ToList();
        }

        public List<Transaction> GetTransactionsForLineItem(int clientId, int lineItemId)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }
            if (lineItemId <= 0)
            {
                throw new ArgumentOutOfRangeException("lineItemId", "A lineItemId less than or equal to zero is invalid, and likely indicates the lineItemId was not set properly");
            }

            IQueryable<Transaction> sqlQuery =
                from tlim in _database.TransactionLineItemMappings
                join transaction in _database.Transactions on tlim.TransactionId equals transaction.TransactionId
                where tlim.LineItemId == lineItemId && transaction.ClientID == clientId
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

        #region Internal Implementation 
        private static List<TransactionHeaderMapping> _defaultHeaderMapping = 
            new List<TransactionHeaderMapping>() {
                new TransactionHeaderMapping() { ClientHeader = "Transaction ID", Hive9Header = "ClientTransactionId", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Purchase Order", Hive9Header = "PurchaseOrder", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Vendor", Hive9Header = "Vendor", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 20 },
                new TransactionHeaderMapping() { ClientHeader = "Amount", Hive9Header = "Amount", HeaderFormat = HeaderMappingFormat.Currency, precision = 2, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Description", Hive9Header = "TransactionDescription", HeaderFormat = HeaderMappingFormat.Text, precision = 0, ExpectedCharacterLength = 30 },
                new TransactionHeaderMapping() { ClientHeader = "Account", Hive9Header = "Account", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 7 },
                new TransactionHeaderMapping() { ClientHeader = "Date", Hive9Header = "TransactionDate", HeaderFormat = HeaderMappingFormat.Date, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Department", Hive9Header = "Department", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
            };
        #endregion Internal Implementation 
    }
}