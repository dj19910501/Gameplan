using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.MarketingBudget;
using System.Data;

namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class MarketingBudgetTest
    {
        private IMarketingBudget _marketingBudget;

        #region Test Data 
        private const int ClientId = 24; //demo client     
        private const string BudgetTitle = "ZebraAdmin";
        private const int BudgetId = 80;
        private const int BudgetDetailId = 752;
        private const double ExchangeRate = 1.0; // Currency exchange rate

        #endregion Test Data

        public MarketingBudgetTest()
        {
            _marketingBudget = ObjectFactory.GetInstance<IMarketingBudget>();
        }

        [TestMethod]
        public void Test_MarketingBudget_GetBudgetlist()
        {
            var res = _marketingBudget.GetBudgetlist(ClientId);
            Assert.IsTrue(res.Count > 0);
        }

        public void Test_MarketingBudget_DeleteBudget()
        {
            var res = _marketingBudget.DeleteBudget(BudgetDetailId, ClientId);
            Assert.IsTrue(res >= 0);
        }

        /// <summary>
        /// Test case of the method of getting finance header values
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_GetFinanceHeaderValues()
        {
            // Get header values 
            DataTable headerValues = _marketingBudget.GetFinanceHeaderValues(BudgetId, ExchangeRate);

            Assert.IsTrue(headerValues.Rows.Count > 0);
            Assert.IsTrue(headerValues.Columns.IndexOf("Budget") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Forecast") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Planned") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Actual") > -1);
        }

    }

    /// <summary>
    /// This extension makes code more readable!
    /// </summary>

}
