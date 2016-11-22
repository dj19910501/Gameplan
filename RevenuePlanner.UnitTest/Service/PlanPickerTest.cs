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
        private const string Year = "2015";
        private const int PlanId = 1270;
        private const string PlanTitle = "Enterprise Big Data 2015";
        private const int CampaignId = 1456;
        private const string CampaignTitle = "Mobile Analytics and Big Data";
        private const int ProgramId = 2001;
        private const string ProgramTitle = "5. Sales Conversion";
        private const int TacticId = 4596;
        private const string TacticTitle = "Configuration Guide";
        private const string LineItemTitle = "Configuration Guide Development";
        #endregion Test Data

        private bool ContainsTitle(List<PlanItem> list, string title)
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

        public PlanPickerTest()
        {
            _planPicker = ObjectFactory.GetInstance<IPlanPicker>(); 
        }

        [TestMethod]
        public void Test_PlanPicker_GetCampaigns()
        {
            var res = _planPicker.GetCampaigns(PlanId);
            Assert.IsTrue(res.Count > 0 && ContainsTitle(res, CampaignTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetLineItems()
        {
            var res = _planPicker.GetLineItems(TacticId);
            Assert.IsTrue(res.Count > 0 && ContainsTitle(res, LineItemTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetPlans()
        {
            var res = _planPicker.GetPlans(ClientId, Year);
            Assert.IsTrue(res.Count > 0 && ContainsTitle(res, PlanTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetPrograms()
        {
            var res = _planPicker.GetPrograms(CampaignId);
            Assert.IsTrue(res.Count > 0 && ContainsTitle(res, ProgramTitle));
        }

        [TestMethod]
        public void Test_PlanPicker_GetTatics()
        {
            var res = _planPicker.GetTatics(ProgramId);
            Assert.IsTrue(res.Count > 0 && ContainsTitle(res, TacticTitle));
        }
    }
}
