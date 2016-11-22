using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.Transactions;

namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class TransactionTest 
    {
        private ITransaction _transaction;
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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionCount()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_GetTransactions()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_GetTransactionsForLineItem()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_SaveTransactionToLineItemMapping()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Test_Transaction_SearchForTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
