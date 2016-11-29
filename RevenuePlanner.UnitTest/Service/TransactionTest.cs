using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.Transactions;
using System.Collections.Generic;

namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class TransactionTest 
    {
        private ITransaction _transaction;
        #region Test Data 
        private const int ClientId = 30; //demo client
        private const int ExpectedNumberOfUnprocessedTransaction = 0;
        private const int ExpectedNumberOfAllTransaction = 22;
        #endregion

        public TransactionTest()
        {
            _transaction = ObjectFactory.GetInstance<ITransaction>();
        }

        [TestMethod]
        public void Test_Transaction_DeleteTransactionLineItemMapping()
        {
            List<TransactionLineItemMapping> tlimList = new List<TransactionLineItemMapping>();
            // tacticId = 2079, lineitemId = 301
            TransactionLineItemMapping tlim = new TransactionLineItemMapping
            { 
                TransactionId = 75,
                LineItemId = 301,
                Amount = 300.10,
            };
            tlimList.Add(tlim);
            _transaction.SaveTransactionToLineItemMapping(tlimList, 297);

            List<LineItemsGroupedByTactic> lineItems = _transaction.GetLinkedLineItemsForTransaction(75);

            LineItemsGroupedByTactic ligbt = lineItems.Find(item => item.TacticId == 2079);
            Assert.IsNotNull(ligbt);
            LinkedLineItem lineItem = ligbt.LineItems.Find(item => item.LineItemId == 301);
            Assert.IsNotNull(lineItem);
            _transaction.DeleteTransactionLineItemMapping(lineItem.LineItemMapping.TransactionLineItemMappingId);

        }

        [TestMethod]
        public void Test_Transaction_GetHeaderMappings()
        {
            var mapping = _transaction.GetHeaderMappings(ClientId) ;
            Assert.IsTrue(mapping.Count > 0);
        }

        [TestMethod]
        public void Test_Transaction_GetLinkedLineItemsForTransaction()
        {
            List<LineItemsGroupedByTactic> lineItems = _transaction.GetLinkedLineItemsForTransaction(30);

            Assert.AreEqual(lineItems.Count, 4);

            LineItemsGroupedByTactic ligbt = lineItems[0];
            Assert.AreEqual(ligbt.TacticId, 4591);

            // TODOWCR: Finish unit test
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionCount()
        {
            int transactionCount = _transaction.GetTransactionCount(ClientId, DateTime.MinValue, DateTime.MaxValue, false);
            Assert.AreEqual(ExpectedNumberOfAllTransaction, transactionCount);

            transactionCount = _transaction.GetTransactionCount(ClientId, DateTime.MinValue, DateTime.MaxValue, true);
            Assert.AreEqual(ExpectedNumberOfUnprocessedTransaction, transactionCount);

            // TODOWCR: Finish unit test
        }

        [TestMethod]
        public void Test_Transaction_GetTransactions()
        {
            // Get the whole list
            List<Transaction> transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, false, null, 1, 100);
            Assert.AreEqual(22, transactionList.Count);

            // Get Page 1
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, false, null, 1, 10);
            Assert.AreEqual("39899", transactionList[0].ClientTransactionId);
            Assert.AreEqual(10, transactionList.Count);
            
            // Get Page 3
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, false, null, 3, 10);
            Assert.AreEqual("85316", transactionList[0].ClientTransactionId);
            Assert.AreEqual(2, transactionList.Count);

            // Get Page 10 (only 22 items, should be zero but not throw exception
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, false, null, 10, 10);
            Assert.AreEqual(0, transactionList.Count);

            // TODOWCR: Finish unit test
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionsForLineItem()
        {
            List<Transaction> transactions = _transaction.GetTransactionsForLineItem(297);
        }

        [TestMethod]
        public void Test_Transaction_SaveTransactionToLineItemMapping()
        {
            List<TransactionLineItemMapping> tlimList = new List<TransactionLineItemMapping>();
            TransactionLineItemMapping tlim = new TransactionLineItemMapping
            {
                TransactionId = 120,
                LineItemId = 297,
                Amount = 300.10,
            };
            tlimList.Add(tlim);
            TransactionLineItemMapping tlim2 = new TransactionLineItemMapping
            {
                TransactionId = 120,
                LineItemId = 298,
                Amount = 100.10,
            };
            tlimList.Add(tlim2);

            _transaction.SaveTransactionToLineItemMapping(tlimList, 297);

            List<LineItemsGroupedByTactic> lineItems = _transaction.GetLinkedLineItemsForTransaction(120);

            LineItemsGroupedByTactic ligbt = lineItems.Find(item => item.TacticId == 2077);
            Assert.IsNotNull(ligbt);
            LinkedLineItem lineItem = ligbt.LineItems.Find(item => item.LineItemId == 297);
            Assert.IsNotNull(lineItem);
            Assert.AreEqual(300.1, lineItem.LineItemMapping.Amount);
            _transaction.DeleteTransactionLineItemMapping(lineItem.LineItemMapping.TransactionLineItemMappingId);


            lineItem = ligbt.LineItems.Find(item => item.LineItemId == 298);
            Assert.IsNotNull(lineItem);
            Assert.AreEqual(100.1, lineItem.LineItemMapping.Amount);
            _transaction.DeleteTransactionLineItemMapping(lineItem.LineItemMapping.TransactionLineItemMappingId);


            // TODOWCR: Finish unit test
        }

    }
}
