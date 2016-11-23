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
        private const int ExpectedNumberOfUnprocessedTransaction = 22;
        private const int ExpectedNumberOfAllTransaction = 22;
        #endregion

        public TransactionTest()
        {
            _transaction = ObjectFactory.GetInstance<ITransaction>();
        }

        [TestMethod]
        public void Test_Transaction_DeleteTransactionLineItemMapping()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_GetHeaderMappings()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_GetLinkedLineItemsForTransaction()
        {
            _transaction.GetLinkedLineItemsForTransaction(120);
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
            List<Transaction> transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, true, null, 1, 100);
            Assert.AreEqual(22, transactionList.Count);

            // Get Page 1
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, true, null, 1, 10);
            Assert.AreEqual("39899", transactionList[0].ClientTransactionId);
            Assert.AreEqual(10, transactionList.Count);
            
            // Get Page 3
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, true, null, 3, 10);
            Assert.AreEqual("85316", transactionList[0].ClientTransactionId);
            Assert.AreEqual(2, transactionList.Count);

            // Get Page 10 (only 22 items, should be zero but not throw exception
            transactionList = _transaction.GetTransactions(ClientId, DateTime.MinValue, DateTime.MaxValue, true, null, 10, 10);
            Assert.AreEqual(0, transactionList.Count);

            // TODOWCR: Finish unit test
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionsForLineItem()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_SaveTransactionToLineItemMapping()
        {
            TransactionLineItemMapping tlim = new TransactionLineItemMapping
            {
                TransactionId = 120,
                LineItemId = 77,
                Amount = 300.10,
            };
            List<TransactionLineItemMapping> tlimList = new List<TransactionLineItemMapping>();
            tlimList.Add(tlim);

            _transaction.SaveTransactionToLineItemMapping(tlimList, 297);

            // TODOWCR: Finish unit test
        }

        [TestMethod]
        public void Test_Transaction_SearchForTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
