using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.MarketingBudget;
namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class MarketingBudgetTest
    {
        private IMarketingBudget _marketingBudget;

        #region Test Data 
        private const int ClientId = 24; //demo client     
        private const string BudgetTitle = "ZebraAdmin";
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

        public void Test_MarketingBudget_GetOtherBudgetId()
        {
            var res = _marketingBudget.GetOtherBudgetId(ClientId);
            Assert.IsTrue(res > 0);
        }
        
    }

    /// <summary>
    /// This extension makes code more readable!
    /// </summary>
   
}
