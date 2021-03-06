﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RevenuePlanner.Models;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Data.SqlTypes;

namespace RevenuePlanner.Services.Transactions
{
    public class FinancialTransaction : ITransaction
    {
        private MRPEntities _database;
        public FinancialTransaction(MRPEntities database)
        {
            Contract.Requires<ArgumentNullException>(database != null, "MRPEntities database cannot be null.");

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
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentNullException>(transactionLineItemMappings != null, "transactionLineItemsMappings cannot be null");
            Contract.Requires<ArgumentOutOfRangeException>(modifyingUserId > 0, "A modifyingUserId less than or equal to zero is invalid, and likely indicates the modifyingUserId was not set properly");


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
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(mappingId > 0, "A mappingId less than or equal to zero is invalid, and likely indicates the mappingId was not set properly");


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
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            

            //Use default for now until real customers are using transaction feature.
            return _defaultHeaderMapping;
        }

        public List<LineItemsGroupedByTactic> GetLinkedLineItemsForTransaction(int clientId, int transactionId)
        {
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(transactionId > 0, "A transactionId less than or equal to zero is invalid, and likely indicates the transactionId was not set properly");


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

        public int GetTransactionCount(int clientId, DateTime start, DateTime end, bool unlinkedOnly = true)
        {
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(start >= (DateTime)SqlDateTime.MinValue, "start date must be greater than '1/1/1753 12:00:00 AM'");
            Contract.Requires<ArgumentOutOfRangeException>(end <= (DateTime)SqlDateTime.MaxValue, "end date must be less than '12/31/9999 11:59:59 PM'");

            String sqlQuery;

            if (unlinkedOnly)
            {
                sqlQuery = @"SELECT COUNT(1) from Transactions T
                                LEFT JOIN TransactionLineItemMapping M ON T.TransactionId = M.TransactionId
                                WHERE T.ClientId = @ClientId AND T.TransactionDate >= @StartDate AND T.TransactionDate <= @EndDate AND T.LineItemId IS NULL AND M.TransactionId IS NULL";
            } else
            {
                sqlQuery = @"SELECT COUNT(1) from Transactions T
                                WHERE T.ClientId = @ClientId AND T.TransactionDate >= @StartDate AND T.TransactionDate <= @EndDate AND T.LineItemId IS NULL ";
            }

            if (_database.Database.Connection.State == ConnectionState.Closed)
            {
                _database.Database.Connection.Open();
            }

            SqlCommand sqlCmd = new SqlCommand(sqlQuery, _database.Database.Connection as SqlConnection);           
            sqlCmd.Parameters.AddWithValue("@ClientId", clientId);
            sqlCmd.Parameters.AddWithValue("@StartDate", start);
            sqlCmd.Parameters.AddWithValue("@EndDate", end);

            int count = (int)sqlCmd.ExecuteScalar();
            _database.Database.Connection.Close();

            return count;
        }

        public List<Transaction> GetTransactions(int clientId, DateTime start, DateTime end, bool unlinkedOnly = true, int skip = 0, int take = 10000)
        {
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(start >= (DateTime)SqlDateTime.MinValue, "start date must be greater than '1/1/1753 12:00:00 AM'"); 
            Contract.Requires<ArgumentOutOfRangeException>(end <= (DateTime)SqlDateTime.MaxValue, "end date must be less than '12/31/9999 11:59:59 PM'");
            Contract.Requires<ArgumentOutOfRangeException>(skip >= 0, "skip must be a positive integer");
            Contract.Requires<ArgumentOutOfRangeException>(take >= 0, "take must be a positive integer");

            String sqlQuery = null;

            SqlParameter[] parameters = new SqlParameter[5];
            parameters[0] = new SqlParameter { ParameterName = "@ClientId", Value = clientId };
            parameters[1] = new SqlParameter { ParameterName = "@StartDate", Value = start};
            parameters[2] = new SqlParameter { ParameterName = "@EndDate", Value = end};
            parameters[3] = new SqlParameter { ParameterName = "@SkipRows", Value = skip };
            parameters[4] = new SqlParameter { ParameterName = "@TakeRows", Value = take };

            if (unlinkedOnly)
            {
                sqlQuery = @"SELECT T.* FROM Transactions T
                                LEFT JOIN TransactionLineItemMapping M ON T.TransactionId = M.TransactionId
                                WHERE T.ClientId = @ClientId AND T.TransactionDate >= @StartDate AND T.TransactionDate <= @EndDate AND T.LineItemId IS NULL AND M.TransactionId IS NULL
                                ORDER BY T.TransactionDate DESC
                                OFFSET @SkipRows ROWS FETCH NEXT @TakeRows ROWS ONLY";
            } else
            {
                sqlQuery = @"SELECT T.* FROM Transactions T 
                                WHERE T.ClientId = @ClientId AND T.TransactionDate >= @StartDate AND T.TransactionDate <= @EndDate AND T.LineItemId IS NULL 
                                ORDER BY T.TransactionDate DESC
                                OFFSET @SkipRows ROWS FETCH NEXT @TakeRows ROWS ONLY";
            }

            IEnumerable<Models.Transaction> modelTransactions = _database.Database.SqlQuery<Models.Transaction>(sqlQuery, parameters);

            List<Transaction> transactions = new List<Transaction>();
            foreach (Models.Transaction transaction in modelTransactions)
            {
                transactions.Add(BuildTransaction(transaction));
            }

            return transactions;
        }

        /// <summary>
        /// Translate a db transaction model to a DTO transaction model
        /// </summary>
        /// <param name="modelTransaction"></param>
        /// <returns></returns>
        private static Transaction BuildTransaction(Models.Transaction modelTransaction)
        {
            Transaction transaction = null;

            if (modelTransaction != null)
            {
                transaction = new Transaction
                {
                    TransactionId = modelTransaction.TransactionId,
                    ClientTransactionId = modelTransaction.ClientTransactionID,
                    TransactionDescription = modelTransaction.TransactionDescription,
                    Amount = modelTransaction.Amount,
                    Account = modelTransaction.Account,
                    AccountDescription = modelTransaction.AccountDescription,
                    SubAccount = modelTransaction.SubAccount,
                    Department = modelTransaction.Department,
                    TransactionDate = modelTransaction.TransactionDate != null ? (DateTime)modelTransaction.TransactionDate : DateTime.MinValue,
                    AccountingDate = modelTransaction.AccountingDate,
                    Vendor = modelTransaction.Vendor,
                    PurchaseOrder = modelTransaction.PurchaseOrder,
                    CustomField1 = modelTransaction.CustomField1,
                    CustomField2 = modelTransaction.CustomField2,
                    CustomField3 = modelTransaction.CustomField3,
                    CustomField4 = modelTransaction.CustomField4,
                    CustomField5 = modelTransaction.CustomField5,
                    CustomField6 = modelTransaction.CustomField6,
                    LineItemId = modelTransaction.LineItemId != null ? (int)modelTransaction.LineItemId : 0,
                    DateCreated = modelTransaction.DateCreated,
                    AmountAttributed = modelTransaction.AmountAttributed != null ? (double)modelTransaction.AmountAttributed : 0.0,
                    AmountRemaining = (double)modelTransaction.Amount - (modelTransaction.AmountAttributed != null ? (double)modelTransaction.AmountAttributed : 0.0),
                    LastProcessed = modelTransaction.LastProcessed
                };
            }

            return transaction;
        }

        public List<LinkedTransaction> GetTransactionsForLineItem(int clientId, int lineItemId)
        {
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(lineItemId > 0, "A lineItemId less than or equal to zero is invalid, and likely indicates the lineItemId was not set properly");

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@ClientId", clientId);
            parameters[1] = new SqlParameter("@LineItemId", lineItemId);

            // We want to get all the transactions that the user has mapped AND the transaction with direct lineItemIds on them
            String sqlQuery = @"SELECT T.TransactionId, T.ClientTransactionID, T.Amount, T.PurchaseOrder, M.LineItemId, M.Amount AS LinkedAmount FROM Transactions T
                                    JOIN TransactionLineItemMapping M ON T.TransactionId = M.TransactionId
                                    WHERE T.ClientId = @ClientId AND M.LineItemId = @LineItemId
                                    UNION (SELECT T.TransactionId, T.ClientTransactionID, T.Amount, T.PurchaseOrder, T.LineItemId, T.Amount AS LinkedAmount FROM Transactions T
                                                WHERE T.ClientId = @ClientId AND T.LineItemId = @LineItemId)";

            return _database.Database.SqlQuery<LinkedTransaction>(sqlQuery, parameters).ToList();
        }

        public Transaction GetTransaction(int clientId, int transactionId)
        {
            Contract.Requires<ArgumentOutOfRangeException>(clientId > 0, "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            Contract.Requires<ArgumentOutOfRangeException>(transactionId > 0, "A transactionId less than or equal to zero is invalid, and likely indicates the transactionId was not set properly");

            String sqlQuery = null;

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter { ParameterName = "@ClientId", Value = clientId };
            parameters[1] = new SqlParameter { ParameterName = "@TransactionId", Value = transactionId };
            sqlQuery = @"SELECT * FROM Transactions WHERE ClientId = @ClientId AND TransactionId = @TransactionId";

            Models.Transaction transaction = _database.Database.SqlQuery<Models.Transaction>(sqlQuery, parameters).SingleOrDefault();
            return BuildTransaction(transaction);
        }

        #region Internal Implementation 
        private static List<TransactionHeaderMapping> _defaultHeaderMapping = 
            new List<TransactionHeaderMapping>() {
                new TransactionHeaderMapping() { ClientHeader = "Transaction ID", Hive9Header = "ClientTransactionId", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Purchase Order", Hive9Header = "PurchaseOrder", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Vendor", Hive9Header = "Vendor", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 20 },
                new TransactionHeaderMapping() { ClientHeader = "Amount", Hive9Header = "Amount", HeaderFormat = HeaderMappingFormat.Currency, precision = 2, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Attributed", Hive9Header = "AmountAttributed", HeaderFormat = HeaderMappingFormat.Currency, precision = 2, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Remaining", Hive9Header = "AmountRemaining", HeaderFormat = HeaderMappingFormat.Currency, precision = 2, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Description", Hive9Header = "TransactionDescription", HeaderFormat = HeaderMappingFormat.Text, precision = 0, ExpectedCharacterLength = 30 },
                new TransactionHeaderMapping() { ClientHeader = "Account", Hive9Header = "Account", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 7 },
                new TransactionHeaderMapping() { ClientHeader = "Date", Hive9Header = "TransactionDate", HeaderFormat = HeaderMappingFormat.Date, precision = 0, ExpectedCharacterLength = 10 },
                new TransactionHeaderMapping() { ClientHeader = "Department", Hive9Header = "Department", HeaderFormat = HeaderMappingFormat.Label, precision = 0, ExpectedCharacterLength = 10 },
            };
        #endregion Internal Implementation 
    }
}