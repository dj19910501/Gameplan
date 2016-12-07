using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.Transactions;
using System.Collections.Generic;
using System.Data;

namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class TransactionTest 
    {
        private ITransaction _transaction;
        private Models.MRPEntities _database;

        #region Test Data 
        private const int testClientId = 30; //demo client
        private const int testOtherClientId = 31; // not the client id associated with any test data
        private const int testUserId = 297;
        private List<Models.Transaction> unprocessedTransactions = null;
        private const int numberOfUnprocessedTransactionsCreated = 5;
        #endregion

        public TransactionTest()
        {
            _transaction = ObjectFactory.GetInstance<ITransaction>();
            _database = ObjectFactory.GetInstance<Models.MRPEntities>();
        }

        /// <summary>
        /// Create some unprocessed transactions for testing purposes
        /// </summary>
        [TestInitialize]
        public void InitializeData()
        {
            _database.Database.ExecuteSqlCommand("DELETE FROM transactions WHERE ClientTransactionID LIKE '%TransactionTest%'");            

            unprocessedTransactions = new List<Models.Transaction>();
            for (int ndx = 0; ndx < numberOfUnprocessedTransactionsCreated; ndx++)
            {
                unprocessedTransactions.Add(_database.Transactions.Add(new Models.Transaction { ClientID = testClientId, ClientTransactionID = "TransactionTest" + ndx.ToString(), Amount = new decimal(100.1), AccountingDate = DateTime.Now, DateCreated = DateTime.Now }));
            }
            _database.SaveChanges();
                           
        }

        /// <summary>
        /// Delete those unprocessed transactions created in InitializeData
        /// </summary>
        [TestCleanup]
        public void CleanupData()
        {
            foreach (Models.Transaction transaction in unprocessedTransactions)
            {
                _database.Entry(transaction).State = EntityState.Deleted;
            }
            _database.SaveChanges();
        }

        [TestMethod]
        public void Test_Transaction_DeleteTransactionLineItemMapping_InvalidArguments()
        {

            // Test lineItemId == 0
            try
            {
                _transaction.DeleteTransactionLineItemMapping(testClientId, 0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("mappingId"));
            }

            // Test negative lineItemId
            try
            {
                _transaction.DeleteTransactionLineItemMapping(testClientId, -100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("mappingId"));
            }

            // Test clientId == 0
            try
            {
                _transaction.DeleteTransactionLineItemMapping(0, 200);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId 
            try
            {
                _transaction.DeleteTransactionLineItemMapping(-100, 200);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }
        }

        [TestMethod]
        public void Test_Transaction_DeleteTransactionLineItemMapping()
        {
            // From the test Data
            const int testTacticId = 2079;
            const int testLineItemId = 301;
            const int testTransactionId = 75;
            const double testAmount = 300.10;

            // Create a Line Item Mapping
            List<TransactionLineItemMapping> tlimList = new List<TransactionLineItemMapping>();
            tlimList.Add(new TransactionLineItemMapping { TransactionId = testTransactionId, LineItemId = testLineItemId, Amount = testAmount });
            _transaction.SaveTransactionToLineItemMapping(testClientId, tlimList, testUserId);

            // Verify its created
            List<LineItemsGroupedByTactic> groupedLineItems = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);

            LineItemsGroupedByTactic ligbt = groupedLineItems.Find(item => item.TacticId == testTacticId);
            Assert.IsNotNull(ligbt);
            LinkedLineItem lineItem = ligbt.LineItems.Find(item => item.LineItemId == testLineItemId);
            Assert.IsNotNull(lineItem);

            // Test deleting line item mapping with invalid clientId. MUST Be run before actually deleting the line item.
            _transaction.DeleteTransactionLineItemMapping(testOtherClientId, lineItem.LineItemMapping.TransactionLineItemMappingId);

            // Verify not deleted
            groupedLineItems = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);
            ligbt = groupedLineItems.Find(item => item.TacticId == testTacticId);
            Assert.IsNotNull(ligbt);
            lineItem = ligbt.LineItems.Find(item => item.LineItemId == testLineItemId);
            Assert.IsNotNull(lineItem);

            // Test deleting line item mapping
            _transaction.DeleteTransactionLineItemMapping(testClientId, lineItem.LineItemMapping.TransactionLineItemMappingId);

            // Verify its deleted.
            groupedLineItems = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);

            ligbt = groupedLineItems.Find(item => item.TacticId == testTacticId);
            // If there is no LineItemsGroupedByTactic entry, no mappings were found so the delete worked. However, if there is
            // a LineItemGroupedByTactic, then we need to verify the specific line item is not mapped (others could be mapped).
            if (ligbt != null)
            {
                lineItem = ligbt.LineItems.Find(item => item.LineItemId == testLineItemId);
                Assert.IsNull(lineItem);
            }

        }

        [TestMethod]
        public void Test_Transaction_GetHeaderMappings_InvalidArguments()
        {
            // Test clientId == 0
            try
            {
                _transaction.GetHeaderMappings(0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId.
            try
            {
                _transaction.GetHeaderMappings(-100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }
        }
        [TestMethod]
        public void Test_Transaction_GetHeaderMappings()
        {
            const HeaderMappingFormat testAmountFormat = HeaderMappingFormat.Currency;
            const int testAmountPrecision = 2;

            // Test that we get mappings for valid clientId
            List<TransactionHeaderMapping> mappings = _transaction.GetHeaderMappings(testClientId) ;
            Assert.IsTrue(mappings.Count > 0);

            // Test that the Amount mapping has its headerformat and precision set to the expected values.
            TransactionHeaderMapping thm = mappings.Find(mapping => mapping.Hive9Header == "Amount");
            Assert.AreEqual(testAmountFormat, thm.HeaderFormat);
            Assert.AreEqual(testAmountPrecision, thm.precision);
        }

        [TestMethod]
        public void Test_Transaction_GetLinkedLineItemsForTransaction_InvalidArguments()
        {
            // Test clientId == 0
            try
            {
                _transaction.GetLinkedLineItemsForTransaction(0, 100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId.
            try
            {
                _transaction.GetLinkedLineItemsForTransaction(-100, 100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test transactionId == 0
            try
            {
                _transaction.GetLinkedLineItemsForTransaction(testClientId, 0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("transactionId"));
            }

            // Test negative transactionId.
            try
            {
                _transaction.GetLinkedLineItemsForTransaction(testClientId, -100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("transactionId"));
            }
        }

        [TestMethod]
        public void Test_Transaction_GetLinkedLineItemsForTransaction()
        {
            const int testTransactionId = 30;
            const int expectedLineItemsGroupedByTactic = 5;
            const int testTacticId = 4671;
            const int expectedLineItemsForTactic = 4;
               
            // Test that we get line items by tactic
            List<LineItemsGroupedByTactic> ligbtList = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);           
            Assert.AreEqual(expectedLineItemsGroupedByTactic, ligbtList.Count);

            // Test that for the given tactic, we have the expected number of line items
            LineItemsGroupedByTactic ligbt = ligbtList.Find(item => item.TacticId == testTacticId);
            Assert.AreEqual(expectedLineItemsForTactic, ligbt.LineItems.Count);

            // Test with invalid clientId
            ligbtList = _transaction.GetLinkedLineItemsForTransaction(testOtherClientId, testTransactionId);
            Assert.AreEqual(0, ligbtList.Count);
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionCount_InvalidArguments()
        {
            // Test clientId == 0
            try
            {
                _transaction.GetTransactionCount(0, DateTime.MinValue, DateTime.MaxValue);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId.
            try
            {
                _transaction.GetTransactionCount(-100, DateTime.MinValue, DateTime.MaxValue);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionCount()
        {
            const int expectedAllDatesUnprocessedCount = numberOfUnprocessedTransactionsCreated; 
            const int expectedAllDatesAllItemsCount = 22 + expectedAllDatesUnprocessedCount;
            DateTime testStartDate = new DateTime(2016, 01, 01);
            DateTime testEndDate = new DateTime(2016, 07, 01);
            const int expectedDateRangeCount = 14;

            // Get transaction count with default unprocessedOnly set
            int transactionCount = _transaction.GetTransactionCount(testClientId, DateTime.MinValue, DateTime.MaxValue);
            Assert.AreEqual(expectedAllDatesUnprocessedCount, transactionCount);

            // Get Transaction count with all dates, unprocessedOnly == false
            transactionCount = _transaction.GetTransactionCount(testClientId, DateTime.MinValue, DateTime.MaxValue, false);
            Assert.AreEqual(expectedAllDatesAllItemsCount, transactionCount);

            // Get Transaction count with all dates, unprocessedOnly == true
            transactionCount = _transaction.GetTransactionCount(testClientId, DateTime.MinValue, DateTime.MaxValue, true);
            Assert.AreEqual(expectedAllDatesUnprocessedCount, transactionCount);

            // Get transaction count with limited date range
            transactionCount = _transaction.GetTransactionCount(testClientId, testStartDate, testEndDate, false);
            Assert.AreEqual(expectedDateRangeCount, transactionCount);

        }

        [TestMethod]
        public void Test_Transaction_GetTransactions_InvalidArguments()
        {
            // Test clientId == 0
            try
            {
                _transaction.GetTransactions(0, DateTime.MinValue, DateTime.MaxValue);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId 
            try
            {
                _transaction.GetTransactions(-100, DateTime.MinValue, DateTime.MaxValue);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test with negative skip
            try
            {
                _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false, -100, 0);
                Assert.Fail();
            } catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("skip"));
            }

            // Test with negative take
            try
            {
                _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false, 0, -100);
                Assert.Fail();
            } catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("take"));
            }

        }
        [TestMethod]
        public void Test_Transaction_GetTransactions()
        {
            const int expectedAllDatesUnprocessedCount = numberOfUnprocessedTransactionsCreated; 
            const int expectedAllDatesAllItemsCount = 22 + expectedAllDatesUnprocessedCount;
            DateTime testStartDate = new DateTime(2016, 01, 01);
            DateTime testEndDate = new DateTime(2016, 07, 01);
            const int expectedDateRangeCount = 14;
            const int testTakeCount = 10;
            const int testFirstPageSkip = 0;
            const int testThirdPageSkip = 20;
            const int testTenthPageSkip = 90;
            const int paginationExpectedFirstPageCount = 10;
            const int paginationExpectedThirdPageCount = 2 + expectedAllDatesUnprocessedCount;
            const int paginationExpctedTenthPageCount = 0;
            const string expectedFirstPageFirstItemClientTransactionId = "39899";
            const string expectedThirdPageFirstItemClientTransactionId = "85316";

            // Get transactions with default unprocessed set, default pagination
            List<Transaction> transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue);
            Assert.AreEqual(expectedAllDatesUnprocessedCount, transactionList.Count);

            // Get transactions with all dates, unprocessedOnly == true, default pagination
            transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, true);
            Assert.AreEqual(expectedAllDatesUnprocessedCount, transactionList.Count);
            foreach (Transaction transaction in transactionList)
            {
                Assert.IsNull(transaction.LastProcessed);
            }

            // Get transactions with all dates, unprocessedOnly == false, default pagination
            transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false);
            Assert.AreEqual(expectedAllDatesAllItemsCount, transactionList.Count);

            // Get transactions with limited date range, unprocessedOnly == false, default pagination
            transactionList = _transaction.GetTransactions(testClientId, testStartDate, testEndDate, false);
            Assert.AreEqual(expectedDateRangeCount, transactionList.Count);
            foreach (Transaction transaction in transactionList)
            {
                Assert.IsTrue(transaction.DateCreated >= testStartDate);
                Assert.IsTrue(transaction.DateCreated <= testEndDate);
            }

            // Exercise pagination
            // Get Page 1
            transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false, testFirstPageSkip, testTakeCount);
            Assert.AreEqual(expectedFirstPageFirstItemClientTransactionId, transactionList[0].ClientTransactionId);
            Assert.AreEqual(paginationExpectedFirstPageCount, transactionList.Count);

            // Get Page 3
            transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false, testThirdPageSkip, testTakeCount);
            Assert.AreEqual(expectedThirdPageFirstItemClientTransactionId, transactionList[0].ClientTransactionId);
            Assert.AreEqual(paginationExpectedThirdPageCount, transactionList.Count);

            // Get Page 10 (less than 90 items, should be zero but not throw exception or return null
            transactionList = _transaction.GetTransactions(testClientId, DateTime.MinValue, DateTime.MaxValue, false, testTenthPageSkip, testTakeCount);
            Assert.AreEqual(paginationExpctedTenthPageCount, transactionList.Count);

        }

        [TestMethod]
        public void Test_Transaction_GetTransactionsForLineItems_InvalidArguments()
        {
            // Test clientId == 0
            try
            {
                _transaction.GetTransactionsForLineItem(0, 100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId 
            try
            {
                _transaction.GetTransactionsForLineItem(-100, 100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test lineItemId == 0
            try
            {
                _transaction.GetTransactionsForLineItem(testClientId, 0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("lineItemId"));
            }

            // Test negative lineItemId
            try
            {
                _transaction.GetTransactionsForLineItem(testClientId, -100);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("lineItemId"));
            }
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionsForLineItem()
        {
            const int testTransactionId = 570;
            const int testLineItemId = 8837;
            const int expectedMappedTransactions = 1;

            // Get transactions for a line item
            List<Transaction> transactions = _transaction.GetTransactionsForLineItem(testClientId, testLineItemId);
            Assert.AreEqual(expectedMappedTransactions, transactions.Count);
            Assert.AreEqual(testTransactionId, transactions[0].TransactionId);

            // test with invalid clientId
            transactions = _transaction.GetTransactionsForLineItem(testOtherClientId, testLineItemId);
            Assert.AreEqual(0, transactions.Count);
        }

        [TestMethod]
        public void Test_Transaction_SaveTransactionToLineItemMapping_InvalidArgument()
        {
            // Test clientId == 0
            try
            {
                _transaction.SaveTransactionToLineItemMapping(0, new List<TransactionLineItemMapping>(), testUserId);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test negative clientId 
            try
            {
                _transaction.SaveTransactionToLineItemMapping(-100, new List<TransactionLineItemMapping>(), testUserId);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("clientId"));
            }

            // Test null transactionLineItemMappings
            try
            {
                _transaction.SaveTransactionToLineItemMapping(testClientId, null, testUserId);
                Assert.Fail();
            } catch (ArgumentNullException e)
            {
                Assert.IsTrue(e.Message.Contains("transactionLineItemMappings"));
            }

            // Test userId == 0
            try
            {
                _transaction.SaveTransactionToLineItemMapping(testClientId, new List<TransactionLineItemMapping>(), 0);
            } catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("modifyingUserId"));
            }

            // Test negative userId
            try
            {
                _transaction.SaveTransactionToLineItemMapping(testClientId, new List<TransactionLineItemMapping>(), -100);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Assert.IsTrue(e.Message.Contains("modifyingUserId"));
            }
        }
        [TestMethod]
        public void Test_Transaction_SaveTransactionToLineItemMapping()
        {
            const int testTransactionId = 120;
            const int testTacticId = 2077;
            int[] testLineItemIds = new int[] { 297, 298 };
            double[] testMappingAmounts = new double[] { 300.10, 100.10 };
            const int testModifiedLineItemId = 297;
            const double testModifiedMappingAmount = 400.4;
            const int testNewLineItemId = 296;
            const double testNewMappingAmount = 500.5;
            const int testOtherClientTransactionId = 214;
            const int testOtherClientLineItemId = 1810; 

            // Test Create line item
            List<TransactionLineItemMapping> tlimList = new List<TransactionLineItemMapping>();
            for (int ndx=0; ndx < testLineItemIds.Length; ndx++)
            {
                tlimList.Add(new TransactionLineItemMapping { TransactionId = testTransactionId, LineItemId = testLineItemIds[ndx], Amount = testMappingAmounts[ndx] });
            }

            _transaction.SaveTransactionToLineItemMapping(testClientId, tlimList, testUserId);

            // Verify they are added
            List<LineItemsGroupedByTactic> ligbtList = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);

            LineItemsGroupedByTactic ligbt = ligbtList.Find(item => item.TacticId == testTacticId);
            Assert.IsNotNull(ligbt);

            for (int ndx=0; ndx < testLineItemIds.Length; ndx++)
            {
                LinkedLineItem li = ligbt.LineItems.Find(item => item.LineItemId == testLineItemIds[ndx]);
                Assert.IsNotNull(li);
                Assert.AreEqual(testMappingAmounts[ndx], li.LineItemMapping.Amount);
            }


            // Test Update line item AND Test Create and Update in same list
            tlimList = new List<TransactionLineItemMapping>();
            // Update
            LinkedLineItem lineItem = ligbt.LineItems.Find(item => item.LineItemId == testModifiedLineItemId);
            lineItem.LineItemMapping.Amount = testModifiedMappingAmount;
            tlimList.Add(lineItem.LineItemMapping);
            // New item
            tlimList.Add(new TransactionLineItemMapping { TransactionId = testTransactionId, LineItemId = testNewLineItemId, Amount = testNewMappingAmount });
            _transaction.SaveTransactionToLineItemMapping(testClientId, tlimList, testUserId);

            ligbtList = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);
            ligbt = ligbtList.Find(item => item.TacticId == testTacticId);
            lineItem = ligbt.LineItems.Find(item => item.LineItemId == testModifiedLineItemId);
            Assert.AreEqual(testModifiedMappingAmount, lineItem.LineItemMapping.Amount);
            lineItem = ligbt.LineItems.Find(item => item.LineItemId == testNewLineItemId);
            Assert.AreEqual(testNewMappingAmount, lineItem.LineItemMapping.Amount);

            // Clean up line item mappings
            ligbtList = _transaction.GetLinkedLineItemsForTransaction(testClientId, testTransactionId);
            ligbt = ligbtList.Find(item => item.TacticId == testTacticId);
            List<int> lineItemIdsToDelete = new List<int>(testLineItemIds);
            lineItemIdsToDelete.Add(testNewLineItemId);
            foreach (int lineItemId in lineItemIdsToDelete)
            {
                LinkedLineItem li = ligbt.LineItems.Find(item => item.LineItemMapping.LineItemId == lineItemId);
                _transaction.DeleteTransactionLineItemMapping(testClientId, li.LineItemMapping.TransactionLineItemMappingId);                   
            }

            // Test with clientId that doesn't match transaction
            tlimList = new List<TransactionLineItemMapping>();
            tlimList.Add(new TransactionLineItemMapping { TransactionId = testOtherClientTransactionId, LineItemId = testLineItemIds[0], Amount = testMappingAmounts[0] });
            _transaction.SaveTransactionToLineItemMapping(testClientId, tlimList, testUserId);

            List<Models.TransactionLineItemMapping> lineItemMappings = GetTransactionLineItem(testOtherClientTransactionId, testLineItemIds[0]);
            Assert.AreEqual(0, lineItemMappings.Count);

            // Test with clientId that doesn't match line item
            tlimList = new List<TransactionLineItemMapping>();
            tlimList.Add(new TransactionLineItemMapping { TransactionId = testTransactionId, LineItemId = testOtherClientLineItemId, Amount = testMappingAmounts[0] });
            _transaction.SaveTransactionToLineItemMapping(testClientId, tlimList, testUserId);
            lineItemMappings = GetTransactionLineItem(testTransactionId, testOtherClientLineItemId);
            Assert.AreEqual(0, lineItemMappings.Count);
        }

        /// <summary>
        /// We need to explicitly get the line item mapping from the database since our Service level routine properly checks 
        /// for clientId too and will not return any results even if a mapping was created by the save.
        /// </summary>
        /// <param name="transactionId">Transaction id of the mapping we're looking up</param>
        /// <param name="lineItemId">Line Item Id of the mapping we're looking up</param>
        /// <returns>List of line item mappings</returns>
        private List<Models.TransactionLineItemMapping> GetTransactionLineItem(int transactionId, int lineItemId)
        {
            String sqlQuery = String.Format("Select * from TransactionLineItemMapping where TransactionId={0} and LineItemId={1}", transactionId, lineItemId);

            return new List<Models.TransactionLineItemMapping>(_database.Database.SqlQuery<Models.TransactionLineItemMapping>(sqlQuery));
        }

    }
}
