using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Services.PlanPicker;
using StructureMap;

namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class PlanPickerTest
    {
        private IPlanPicker _planPicker;

        #region Test Data 
        private const int ClientId = 30; //demo client
        private const string Year = "2016";
        private const int PlanId = 1398;
        private const string PlanTitle = "Marketing Operations";
        private const int CampaignId = 2527;
        private const string CampaignTitle = "Headcount and Capex";
        private const int ProgramId = 4524;
        private const string ProgramTitle = "Headcount";
        private const int TacticId = 16618;
        private const string TacticTitle = "Demand Center";
        private const string LineItemTitle = "Demand Center Payroll";
        private const int NumberOfYears = 4;
        #endregion Test Data

        public PlanPickerTest()
        {
            _planPicker = ObjectFactory.GetInstance<IPlanPicker>();
        }

        [TestMethod]
        public void Test_PlanPicker_GetCampaigns()
        {
            var res = _planPicker.GetCampaigns(PlanId);
            Assert.IsTrue(res.Count > 0 && res.ContainsTitle(CampaignTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetLineItems()
        {
            var res = _planPicker.GetLineItems(TacticId);
            Assert.IsTrue(res.Count > 0 && res.ContainsTitle(LineItemTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetPlans()
        {
            var res = _planPicker.GetPlans(ClientId, Year);
            Assert.IsTrue(res.Count > 0 && res.ContainsTitle(PlanTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetPrograms()
        {
            var res = _planPicker.GetPrograms(CampaignId);
            Assert.IsTrue(res.Count > 0 && res.ContainsTitle(ProgramTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetTactics()
        {
            var res = _planPicker.GetTactics(ProgramId);
            Assert.IsTrue(res.Count > 0 && res.ContainsTitle(TacticTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetYears()
        {
            List<string> years = _planPicker.GetYears(ClientId);
            Assert.AreEqual(years.Count, NumberOfYears);
        }
    }

    /// <summary>
    /// This extension makes code more readable!
    /// </summary>
    public static class TestExtension
    {
        public static bool ContainsTitle<T>(this List<T> list, string title) where T : PlanItem
        {
            foreach (var item in list)
            {
                if (item.Title == title)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
