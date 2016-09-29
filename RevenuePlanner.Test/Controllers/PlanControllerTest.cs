using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class PlanControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        // Test Methods according to Old UI
        #region PL #975 Plans need to be able to have a goal of 0

        #region Save plan with zero goal value
        /// <summary>
        /// To check to save plan with 0 goal value
        /// <author>Sohel Pathan</author>
        /// <createdDate>10Dec2014</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Plan_With_Zero_Goal_Value()
        {
            Console.WriteLine("To check to save plan with 0 goal value\n");
            //Set the PlanModel with data
            PlanModel plan = new PlanModel();
            plan.Title = "test plan #975";
            plan.GoalType = Enums.PlanGoalType.MQL.ToString();
            plan.GoalValue = "0";
            plan.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
            plan.Budget = 120000;
            plan.ModelId = DataHelper.GetModelId();
            plan.Year = DateTime.Now.Year.ToString();

            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var result = controller.SavePlan(plan) as JsonResult;

            // data object should not be null in json result
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            int planId = result.GetValue<int>("id");
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planid:  " + planId);
            Assert.IsTrue(Convert.ToBoolean(planId));



        }
        #endregion

        #region Save plan with some goal value other than zero
        /// <summary>
        /// To check to save plan with some goal value other than zero
        /// <author>Sohel Pathan</author>
        /// <createdDate>10Dec2014</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Plan_Without_Zero_Goal_Value()
        {
            Console.WriteLine("To check to save plan with some goal value other than zero.\n");
            //Set the PlanModel with data
            PlanModel plan = new PlanModel();
            plan.Title = "test plan #975";
            plan.GoalType = Enums.PlanGoalType.MQL.ToString();
            plan.GoalValue = Convert.ToString(1500);
            plan.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
            plan.Budget = 1200000000000;
            plan.ModelId = DataHelper.GetModelId();
            plan.Year = DateTime.Now.Year.ToString();

            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var result = controller.SavePlan(plan) as JsonResult;

            // data object should not be null in json result
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            int planId = result.GetValue<int>("id");
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planid:  " + planId);
            Assert.IsTrue(Convert.ToBoolean(planId));


        }
        #endregion

        #endregion
        //End

        #region SavePlanDefination with Zero Goal Value
        /// <summary>
        /// To check to save plan with 0 goal value
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void SavePlanDefination_With_Zero_Goal_Value()
        {
            Console.WriteLine("To check to save plan with 0 goal value.\n");
            //Set the PlanModel with data
            PlanModel plan = new PlanModel();
            plan.Title = "test plan #975";
            plan.GoalType = Enums.PlanGoalType.MQL.ToString();
            plan.GoalValue = "0";
            plan.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
            plan.Budget = 120000;
            plan.ModelId = DataHelper.GetModelId();
            plan.Year = DateTime.Now.Year.ToString();

            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var result = controller.SavePlanDefination(plan) as JsonResult;

            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            int planId = result.GetValue<int>("id");
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planid:  " + planId);
            Assert.IsTrue(Convert.ToBoolean(planId));

        }

        #endregion

        #region Save plan Defination with some goal value other than zero
        /// <summary>
        /// To check to save plan without 0 goal value
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void SavePlanDefination_Without_Zero_Goal_Value()
        {
            Console.WriteLine("To check to save plan without 0 goal value.\n");
            //Set the PlanModel with data
            PlanModel plan = new PlanModel();
            plan.Title = "test plan #975";
            plan.GoalType = Enums.PlanGoalType.MQL.ToString();
            plan.GoalValue = Convert.ToString(1500);
            plan.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
            plan.Budget = 1200000000000;
            plan.ModelId = DataHelper.GetModelId();
            plan.Year = DateTime.Now.Year.ToString();

            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var result = controller.SavePlanDefination(plan) as JsonResult;

            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            int planId = result.GetValue<int>("id");
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planid:  " + planId);
            Assert.IsTrue(Convert.ToBoolean(planId));

        }
        #endregion

        #region Publish Plan
        /// <summary>
        /// To Publish Plan
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void PublishPlan()
        {
            Console.WriteLine("To Publish Plan.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            Sessions.PlanId = DataHelper.GetPlanId();
            var result = controller.PublishPlan(Sessions.User.ID) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            // data object should not be null in json result
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Bugeting

        /// <summary>
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void Bugeting()
        {
            Console.WriteLine("To Budgeting.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            Sessions.PlanId = DataHelper.GetPlanId();
            var result = controller.Budgeting(Sessions.PlanId) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Model);
            // data object should not be null in json result
            Assert.IsNotNull(result.Model);
        }

        /// <summary>
        /// To Save Budget Cell for plan for the budget year
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void SaveBudgetCell()
        {
            Console.WriteLine("To Save Budget Cell for plan for the budget year.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int id = DataHelper.GetPlanId();
            List<string> lstinputs = new List<string>();
            {
                lstinputs.Add("[{\"Key\":\"BudgetYear\"");
                lstinputs.Add("\"Value\":\"10000\"}]");
            }
            string Inputs = string.Join(",", lstinputs);
            var result = controller.SaveBudgetCell(id.ToString(), "Plan", "", Inputs, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            // data object should not be null in json result
            Assert.IsNotNull(result.Data);

        }

        /// <summary>
        /// To Save Budget Cell for plan for the budget year and a month
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void SaveBudgetCell_Month()
        {
            Console.WriteLine("To Save Budget Cell for plan for the budget year and a month.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int id = DataHelper.GetPlanId();
            List<string> lstinputs = new List<string>();
            {
                lstinputs.Add("[{\"Key\":\"BudgetMonth\"");
                lstinputs.Add("\"Value\":\"6000\"");
                lstinputs.Add("\"Key\":\"BudgetYear\"");
                lstinputs.Add("\"Value\":\"10000\"}]");
            }
            string Inputs = string.Join(",", lstinputs);
            var result = controller.SaveBudgetCell(id.ToString(), "Plan", "March", Inputs, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            // data object should not be null in json result
            Assert.IsNotNull(result.Data);

        }

        /// <summary>
        /// To check to allocate budget to campaign having quarterly allocated plan
        /// <author>Arpita Soni</author>
        /// <createddate>25May2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetBudgetAllocationCampaignData_With_Plan_Quarter_Allocated()
        {
            Console.WriteLine("To check to allocate budget to campaign having quarterly allocated plan\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            Plan_Campaign campaign = db.Plan_Campaign.Where(a => a.Plan.Model.ClientId == Sessions.User.CID && a.IsDeleted == false && a.Plan.AllocatedBy.ToLower() != "quarter").OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            int CampaignId = 0;
            if (campaign != null)
            {
                CampaignId = campaign.PlanCampaignId;
            }
            var result = objPlanController.GetBudgetAllocationCampaignData(CampaignId);
            var serializedData = new RouteValueDictionary(result.Data);
            var budgetData = serializedData["budgetData"];
            var planRemainingBudget = serializedData["planRemainingBudget"];
            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value budgetData:  " + budgetData);
            Assert.IsNotNull(budgetData);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planRemainingBudget:  " + planRemainingBudget);
            Assert.IsNotNull(planRemainingBudget);
        }
        /// <summary>
        /// To check to allocate budget to campaign without having quarterly allocated plan
        /// <author>Arpita Soni</author>
        /// <createddate>25May2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetBudgetAllocationCampaignData_Without_Plan_Quarter_Allocated()
        {
            Console.WriteLine("To check to allocate budget to campaign without having quarterly allocated plan\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            Plan_Campaign campaign = DataHelper.GetPlanCampaign(Sessions.User.CID);
            int CampaignId = 0;
            if (campaign != null)
            {
                CampaignId = campaign.PlanCampaignId;
            }
            var result = objPlanController.GetBudgetAllocationCampaignData(CampaignId);
            var serializedData = new RouteValueDictionary(result.Data);
            var budgetData = serializedData["budgetData"];
            var planRemainingBudget = serializedData["planRemainingBudget"];
            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value budgetData:  " + budgetData);
            Assert.IsNotNull(budgetData);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value planRemainingBudget:  " + planRemainingBudget);
            Assert.IsNotNull(planRemainingBudget);
        }
        #endregion

        #region Grid View
        /// To Get Improvement Tactic for the grid
        /// <author>Komal Rawal</author>
        /// <createdDate>11thAugust2015</createdDate>
        /// </summary>
        [TestMethod]
        public void GetImprovementTactic()
        {
            Console.WriteLine("To Get Improvement Tactic for the grid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            var result = controller.GetImprovementTactic(PlanId) as JsonResult;

            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// To get home grid data from cache with out set cache memory object or null cache memory
        /// </summary>
        [TestMethod]
        public void GetPlanGridDataFromCache_WithOut_CacheMemoryData()
        {
            Console.WriteLine("To Get Home grid data from cache object without set cache data for grid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = controller.GetHomeGridDataFromCache() as ActionResult;
            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// To get home grid data from cache with set cache memory object 
        /// </summary>
        [TestMethod]
        public void GetPlanGridDataFromCache_With_CacheMemoryData()
        {
            Console.WriteLine("To Get Home grid data from cache object.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            string PlanId = Convert.ToString(DataHelper.GetPlanId());
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(int.Parse(PlanId)));
            int ModelId = DataHelper.GetPlanModelId(int.Parse(PlanId));
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            // Call the GetHomeGridData method for set Cache memory data
            controller.GetHomeGridData(PlanId, OwnerIds, TacticTypeIds, StatusIds, string.Empty);
            
            var result = controller.GetHomeGridDataFromCache() as ActionResult; // Call cache memory data method for grid data
            // Data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }


        /// <summary>
        /// Add By Nishant Sheth
        /// To get home grid data with pass all parameters
        /// </summary>
        [TestMethod]
        public void GetPlanGrid_With_ValidParameter()
        {
            Console.WriteLine("To Get Home grid data for the grid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            string PlanId = Convert.ToString(DataHelper.GetPlanId());
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(int.Parse(PlanId)));
            int ModelId = DataHelper.GetPlanModelId(int.Parse(PlanId));
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            var result = controller.GetHomeGridData(PlanId, OwnerIds, TacticTypeIds, StatusIds, string.Empty) as ActionResult;
            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }


        /// <summary>
        /// Add By Nishant Sheth
        /// To get home grid data with out pass paramters
        /// </summary>
        [TestMethod]
        public void GetPlanGrid_With_EmptyParameter()
        {
            Console.WriteLine("To Get Home grid data.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            string EmptyString = string.Empty;
            var result = controller.GetHomeGridData(EmptyString, EmptyString, EmptyString, EmptyString, EmptyString) as ActionResult;
            // data object should not be null in json result
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        #endregion

        #region "Feature: Copy or Link Tactic/Program/Campaign between Plan"

        #region "Bind Planlist & Tree list"
        /// To Load Copy Entity Popup
        /// <author>Maitri Gandhi</author>
        /// <createdDate>09Jan2016</createdDate>
        /// </summary>
        [TestMethod]
        public void LoadCopyEntityPopup()
        {
            Console.WriteLine("To Load Copy Entity Popup.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();
            string Section = Enums.Section.Tactic.ToString();
            string Popuptype = Enums.ModelTypeText.Copying.ToString();
            string RedirectType = Enums.InspectPopupRequestedModules.Index.ToString();
            var result = controller.LoadCopyEntityPopup(entityId, Section, Popuptype, RedirectType) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("~/Views/Plan/_CopyEntity.cshtml", result.ViewName);



        }

        /// To Refresh Parent Entity Selection List
        /// <author>Maitri Gandhi</author>
        /// <createdDate>09Jan2016</createdDate>
        /// </summary>
        [TestMethod]
        public void RefreshParentEntitySelectionList()
        {
            Console.WriteLine("To Refresh Parent Entity Selection List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            int Planid = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList()[0];

            var result = controller.RefreshParentEntitySelectionList(Planid.ToString()) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);


        }

        #endregion

        #region "Copy Entities from One Plan to Another"
        /// To Clone to Other Plan
        /// <author>Maitri Gandhi</author>
        /// <createdDate>11Jan2016</createdDate>
        /// </summary>
        [TestMethod]
        public void ClonetoOtherPlan()
        {
            Console.WriteLine("To Clone to Other Plan.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int planId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(planId);
            //string CommaSeparatedPlanId = DataHelper.GetPlanIdList().ToString();
            //List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            var tactic = db.Plan_Campaign_Program_Tactic.Where(id => planId == id.Plan_Campaign_Program.Plan_Campaign.PlanId).Select(t =>
                new
                {
                    TacticId = t.PlanTacticId,
                    PlanProgramId = t.Plan_Campaign_Program.PlanProgramId,
                    PlanId = t.Plan_Campaign_Program.Plan_Campaign.PlanId,
                    Title = t.Title
                }).ToList();
            if (tactic != null && tactic.Count > 1)
            {
                var result = controller.ClonetoOtherPlan(Enums.Section.Tactic.ToString(), tactic[0].TacticId.ToString(), tactic[1].PlanProgramId.ToString(), tactic[0].PlanId.ToString(), tactic[1].PlanId.ToString(), tactic[0].Title);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }

        #endregion

        #endregion

        #region ExportToCSV
        /// <summary>
        /// To check to export data in csv with passing all parameters
        /// <author>Arpita Soni</author>
        /// <createddate>25May2016</createddate>
        /// </summary>
        [TestMethod]
        public void Export_To_CSV_With_All_Params()
        {
            Console.WriteLine("To check to export data in csv with passing all parameters\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = db.Plan_Campaign_Program_Tactic.Where(id => PlanId == id.Plan_Campaign_Program.Plan_Campaign.PlanId).ToList();
            if (TaskData != null && TaskData.Count > 0)
            {
                List<int> Owner = TaskData.Select(plan => plan.CreatedBy).ToList();
                string Ownerids = string.Join(",", Owner);
                List<int> tactic = TaskData.Select(tactictype => tactictype.TacticTypeId).ToList();
                string tactictypeids = string.Join(",", tactic);
                string CommaSeparatedCustomFields = "";

                List<string> lststatus = new List<string>();
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
                lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

                string Status = string.Join(",", lststatus);


                string honeyCombId = null;

                var result = objPlanController.ExportToCsv(Ownerids, tactictypeids, Status, CommaSeparatedCustomFields, honeyCombId, PlanId) as JsonResult;
                var serializedData = new RouteValueDictionary(result.Data);
                var fileName = serializedData["data"];
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value fileName:  " + fileName);
                Assert.IsNotNull(fileName);

            }
        }

        /// <summary>
        /// To check to export data in csv with passing only honeycombids and planid
        /// <author>Arpita Soni</author>
        /// <createddate>25May2016</createddate>
        /// </summary>
        [TestMethod]
        public void Export_To_CSV_With_Only_HoneyCombId_And_PlanId()
        {
            Console.WriteLine("To check to export data in csv with passing only honeycombids and planid\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            int ExportPlanId = 0;
            Sessions.PlanId = PlanId;
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            List<int> honeyCombIds = new List<int>();
            var TaskData = DataHelper.GetPlanTactic(Sessions.User.CID);
            string honeyCombId = string.Empty;
            if (TaskData != null)
            {
                honeyCombIds.Add(TaskData.PlanTacticId);
                honeyCombId = string.Join(",", honeyCombIds);
                ExportPlanId = TaskData.Plan_Campaign_Program.Plan_Campaign.PlanId;
            }
            else
            {
                honeyCombId = null;
            }

            var result = objPlanController.ExportToCsv(null, null, null, null, honeyCombId, ExportPlanId) as JsonResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);



        }

        /// <summary>
        /// To check to export data in csv without honeycombids
        /// <author>Arpita Soni</author>
        /// <createddate>25May2016</createddate>
        /// </summary>
        [TestMethod]
        public void Export_To_CSV_Without_HoneyCombIds()
        {
            Console.WriteLine("To check to export data in csv with passing only honeycombids and planid\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.ID);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            var result = objPlanController.ExportToCsv(Ownerids, tactictypeids, Status, CommaSeparatedCustomFields, null, PlanId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);



        }
        #endregion

        #region Create Plan
        /// <summary>
        /// To check to create plan with passing all parameters
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void Create_Plan_With_All_Params()
        {
            Console.WriteLine("To check to create plan with passing all parameters\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            int PlanId = 0;
            bool isPlanSelector = false;
            bool isGridview = false;
            bool isPlanChange = false;
            var result = objPlanController.CreatePlan(PlanId, isPlanSelector, isGridview, isPlanChange) as ActionResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);


        }

        /// <summary>
        /// To check to create plan with passing only plan id
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void Create_Plan_With_Only_PlanId()
        {
            Console.WriteLine("To check to create plan with passing only plan id\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);


            int PlanId = 0;
            var result = objPlanController.CreatePlan(PlanId) as ActionResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);




        }

        #endregion

        #region NoModel
        /// <summary>
        /// To check to No Model Method.
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void No_Model()
        {
            Console.WriteLine("To check to create plan with passing all parameters\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objPlanController.NoModel() as ActionResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        #endregion

        #region Calculate Budget

        /// <summary>
        /// To check to Calculate Budget for plan with passing all parameters
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void Calculate_Budget_With_All_Params()
        {
            Console.WriteLine("To check to Calculate Budget for plan with passing all parameters\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var PlanData = db.Plans.Where(plan => plan.PlanId == PlanId).FirstOrDefault();
            string goalType = PlanData.GoalType;
            string goalValue = PlanData.GoalValue.ToString();
            int ModelId = PlanData.ModelId;
            var result = objPlanController.CalculateBudget(ModelId, goalType, goalValue) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Get Plan List

        /// <summary>
        /// To check to get Plan List.
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void get_Plan_List()
        {
            Console.WriteLine("To check to get Plan List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);

            var result = objPlanController.PlanList() as PartialViewResult;

            if (!(result.ViewName.Equals("_ApplytoCalendarPlanList")))
            {
                Assert.Fail();
            }
            else if (result.ViewName.Equals("_ApplytoCalendarPlanList"))
            {
                Assert.IsNotNull(result.Model);
            }
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Get Gantt Data
        /// <summary>
        /// To check to get Gantt Data
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void get_Gantt_Data_of_Plan()
        {
            Console.WriteLine("To check to get Gantt Data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string Year = db.Plans.Where(plan => plan.PlanId == PlanId).Select(pl => pl.Year).FirstOrDefault();
            var result = objPlanController.GetGanttData(PlanId, Year) as JsonResult;

            Assert.IsNotNull(result.Data);
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["planYear"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());



        }
        #endregion

        #region Get Get_HeaderData_With_MultiplePlanIds using new performance method
        /// <summary>
        /// To check to Get Get_HeaderData_With_MultiplePlanIds using new performance method.
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_HeaderData_With_MultiplePlanIds()
        {
            Console.WriteLine("To check to Get Get_HeaderData_With_MultiplePlanIds using new performance method.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            PlanController objPlanController = new PlanController();
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            string Year = DataHelper.GetYear();
            int planId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(planId);
            Common.PlanUserSavedViews = db.Plan_UserSavedViews.Where(t => t.Userid == Sessions.User.ID).ToList();
            var result = objPlanController.GetPlanByMultiplePlanIDsPer(Convert.ToString(planId), Enums.ActiveMenu.Home.ToString(), Year) as Task<JsonResult>;
            //// Json result data should not be null
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Status);
            Assert.IsNotNull(result);


        }
        #endregion



        #region "Check Permission By Owner for Entity owner upation"
        /// <summary>
        /// To Check Permission By Owner for Entity owner upation
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Check_Permission_By_Owner()
        {
            Console.WriteLine("To Check Permission By Owner for Entity owner upation.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TacticData = DataHelper.GetPlanTactic(Sessions.User.CID);
            int TacticId = TacticData.PlanTacticId;
            int OwnerId = TacticData.CreatedBy;

            var result = controller.CheckPermissionByOwner(OwnerId, Enums.Section.Tactic.ToString(), TacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion

        #region "Get Minimum And Maximum Date"
        /// <summary>
        /// To Check Get Minimum And Maximum Date for LineItem
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Min_Max_date_LineItem()
        {
            Console.WriteLine("To Check Get Minimum And Maximum Date for LineItem.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var LineItemData = DataHelper.GetPlanLineItem(Sessions.User.CID);
            int TacticId = LineItemData.PlanTacticId;
            int ProgramId = LineItemData.Plan_Campaign_Program_Tactic.PlanProgramId;


            var result = controller.GetMinMaxDate(ProgramId, Enums.Section.LineItem.ToString(), TacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);



        }
        /// <summary>
        /// To Check Get Minimum And Maximum Date for Tactic
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Min_Max_date_Tactic()
        {
            Console.WriteLine("To Check Get Minimum And Maximum Date for Tactic.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TacticData = DataHelper.GetPlanTactic(Sessions.User.CID);
            int ProgramId = TacticData.PlanProgramId;
            int CampaignId = TacticData.Plan_Campaign_Program.PlanCampaignId;

            var result = controller.GetMinMaxDate(CampaignId, Enums.Section.Tactic.ToString(), ProgramId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        /// <summary>
        /// To Check Get Minimum And Maximum Date for Program
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Min_Max_date_Program()
        {
            Console.WriteLine("To Check Get Minimum And Maximum Date for Program.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var ProgramData = DataHelper.GetPlanProgram(Sessions.User.CID);
            int CampaignId = ProgramData.PlanCampaignId;
            int PlanId = ProgramData.Plan_Campaign.PlanId;
            var result = controller.GetMinMaxDate(PlanId, Enums.Section.Program.ToString(), CampaignId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion

        #region "Save Grid Detail"
        /// <summary>
        /// To Check to Save the Grid Data for LineItem
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Grid_Detail_LineItem()
        {
            Console.WriteLine("To Check to Save the Grid Data for LineItem.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = DataHelper.GetPlanLineItem(Sessions.User.CID);
            if (TaskData != null)
            {
                string UpdateValue = "Copy_Test_cases" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");
                int EntityId = TaskData.PlanLineItemId;
                string updateColumnName = "Task Name";

                var result = controller.SaveGridDetail(Enums.Section.LineItem.ToString(), updateColumnName, UpdateValue, EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        /// <summary>
        /// To Check to Save the Grid Data for Tactic
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Grid_Detail_Tactic()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check to Save the Grid Data for LineItem.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.Url = new UrlHelper(
            new RequestContext(
            controller.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = DataHelper.GetPlanTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                //string UpdateValue = TaskData.Title.ToString() + "Copy_Test_cases";
                string UpdateValue = "Copy_Test_cases" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");
                int EntityId = TaskData.PlanTacticId;
                string updateColumnName = "Task Name";

                var result = controller.SaveGridDetail(Enums.Section.Tactic.ToString(), updateColumnName, UpdateValue, EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        /// <summary>
        /// To Check to Save the Grid Data for Program
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Grid_Detail_Program()
        {
            Console.WriteLine("To Check to Save the Grid Data for Program.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = DataHelper.GetPlanProgram(Sessions.User.CID);
            if (TaskData != null)
            {
                string UpdateValue = "Copy_Test_cases" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");
                int EntityId = TaskData.PlanProgramId;
                string updateColumnName = "Task Name";

                var result = controller.SaveGridDetail(Enums.Section.Program.ToString(), updateColumnName, UpdateValue, EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }


        }
        /// <summary>
        /// To Check to Save the Grid Data for Campaign
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Grid_Detail_Campaign()
        {
            Console.WriteLine("To Check to Save the Grid Data for Campaign.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = DataHelper.GetPlanCampaign(Sessions.User.CID);
            if (TaskData != null)
            {
                string UpdateValue = "Copy_Test_cases" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");
                int EntityId = TaskData.PlanCampaignId;
                string updateColumnName = "Task Name";

                var result = controller.SaveGridDetail(Enums.Section.Campaign.ToString(), updateColumnName, UpdateValue, EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        /// <summary>
        /// To Check to Save the Grid Data for Plan
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Grid_Detail_Plan()
        {
            Console.WriteLine("To Check to Save the Grid Data for Plan.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            var TaskData = DataHelper.GetPlan(Sessions.User.CID);
            if (TaskData != null)
            {
                string UpdateValue = "Copy_Test_cases" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");
                int EntityId = TaskData.PlanId;
                string updateColumnName = "Task Name";

                var result = controller.SaveGridDetail(Enums.Section.Plan.ToString(), updateColumnName, UpdateValue, EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }

        #endregion

        #region "Load Improvement Tactic Grid Data"
        /// <summary>
        /// To Check to Load Improvement Tactic Grid Data
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Load_Improvement_Grid()
        {
            Console.WriteLine("To Check to Load Improvement Tactic Grid Data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            int EntityId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(EntityId);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                var result = controller.LoadImprovementGrid(PlanId) as PartialViewResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.ViewName);
                Assert.IsNotNull(result.ViewName);
            }

        }
        #endregion

        #region "Load Add Actual Data"
        /// <summary>
        /// To Check to Load Add Actual Data
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Load_Add_Actual_Grid()
        {
            Console.WriteLine("To Check to Load Add Actual Data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            string userName = Convert.ToString(ConfigurationManager.AppSettings["Username"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            string singlehash = DataHelper.ComputeSingleHash(password);
            RevenuePlanner.BDSService.BDSServiceClient objBDSServiceClient = new RevenuePlanner.BDSService.BDSServiceClient();
            Sessions.User = objBDSServiceClient.Validate_UserOverAll(userName, singlehash);

            int EntityId = DataHelper.GetPlanId();
            bool isGridview = true;
            var result = controller.AddActual(EntityId, isGridview) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }
        #endregion

        #region "Get Actual Line Item data for Plan Budget Tab"
        /// <summary>
        /// To Check to Get Actual Line Item data for Plan Budget Tab
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Actual_LineItem_Data()
        {
            Console.WriteLine("Get Actual Line Item data for Plan Budget Tab.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int EntityId = DataHelper.GetPlanLineItem(Sessions.User.CID).PlanLineItemId;

            var result = controller.GetActualsLineitemData(EntityId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Get Budget Plan List"
        ///<summary>
        /// To Check to Get Budget Plan List.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Budget_Plan_List()
        {
            Console.WriteLine("Get Budget Plan List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            var result = controller.BudgetPlanList() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion

        #region Save Planned Cell for Bugeting

        /// <summary>
        /// To Save Planned Cell for plan for the budget year
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Planned_Cell()
        {
            Console.WriteLine("To Save Planned Cell for plan for the budget year.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int id = DataHelper.GetPlanId();
            List<string> lstinputs = new List<string>();
            {
                lstinputs.Add("[{\"Key\":\"BudgetYear\"");
                lstinputs.Add("\"Value\":\"10000\"}]");
            }
            string Inputs = string.Join(",", lstinputs);
            string tab = "Actual";
            var result = controller.SavePlannedCell(id.ToString(), "Plan", "", Inputs, tab, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        /// <summary>
        /// To Save Planned Cell for plan for the budget year and a month
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Save_Planned_Cell_Month()
        {
            Console.WriteLine("To Save Planned Cell for plan for the budget year and a month.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int id = DataHelper.GetPlanId();
            List<string> lstinputs = new List<string>();
            {
                lstinputs.Add("[{\"Key\":\"BudgetMonth\"");
                lstinputs.Add("\"Value\":\"6000\"");
                lstinputs.Add("\"Key\":\"BudgetYear\"");
                lstinputs.Add("\"Value\":\"10000\"}]");
            }
            string Inputs = string.Join(",", lstinputs);
            string tab = "Actual";
            var result = controller.SavePlannedCell(id.ToString(), "Plan", "March", Inputs, tab, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Get Budget Allocation Plan Data For Plan Budget Tab"
        ///<summary>
        /// To Check to Get Budget Allocation Plan Data For Plan Budget Tab
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Budget_Allocation_Plan_Data()
        {
            Console.WriteLine("Get Budget Allocation Plan Data For Plan Budget Tab.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int EntityId = DataHelper.GetPlanId();

            var result = controller.GetBudgetAllocationPlanData(EntityId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Get Allocated Budget Data for Plan"
        ///<summary>
        /// To Check to Get Allocated Budget Data for Plan.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Allocated_Budget_Data_For_Plan()
        {
            Console.WriteLine("Get Allocated Budget Data for Plan.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlan(Sessions.User.CID);
            int EntityId = TaskData.PlanId;
            string allocatedBy = TaskData.CreatedBy.ToString();
            var result = controller.GetAllocatedBudgetForPlan(EntityId, allocatedBy) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Delete Suggested Box Improvement Tactic"
        ///<summary>
        /// To Check Delete Suggested Box Improvement Tactic.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Delete_Suggested_Box_Improvement_Tactic()
        {
            Console.WriteLine("Delete Suggested Box Improvement Tactic.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID);
            string EntityId = string.Join(",", TaskData.Select(imp => imp.ImprovementPlanTacticId.ToString()));

            int allocatedBy = Sessions.User.ID;
            var result = controller.DeleteSuggestedBoxImprovementTactic(EntityId, allocatedBy) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Get Conversion Improvement Tactic Type"
        ///<summary>
        /// To Check Get Conversion Improvement Tactic Type.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Conversion_Improvement_TacticType()
        {
            Console.WriteLine("Get Conversion Improvement Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID);
            if (TaskData != null && TaskData.Count > 0)
            {
                string EntityId = string.Join(",", TaskData.Select(imp => imp.ImprovementPlanTacticId.ToString()));
                int PlanId = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID).Select(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId).FirstOrDefault();
                Sessions.PlanId = PlanId;
                var result = controller.GetConversionImprovementTacticType(EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region "Get Header Value For Suggested Improvement"
        ///<summary>
        /// To Check Get Header Value For Suggested Improvement.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Header_Value_For_Suggested_Improvement()
        {
            Console.WriteLine("Get_HeaderValue_For_Suggested_Improvement.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID);
            if (TaskData != null && TaskData.Count > 0)
            {
                string EntityId = string.Join(",", TaskData.Select(imp => imp.ImprovementPlanTacticId.ToString()));
                int PlanId = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID).Select(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId).FirstOrDefault();
                Sessions.PlanId = PlanId;
                var result = controller.GetHeaderValueForSuggestedImprovement(EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }


        }
        #endregion

        #region "Get_ADS_Improvement_TacticType"
        ///<summary>
        /// To Check Get ADS Improvement TacticType.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_ADS_Improvement_TacticType()
        {
            Console.WriteLine("Get ADS Improvement TacticType.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID);
            if (TaskData != null && TaskData.Count > 0)
            {
                string EntityId = string.Join(",", TaskData.Select(imp => imp.ImprovementPlanTacticId.ToString()));
                int PlanId = DataHelper.GetPlanImprovementTacticList(Sessions.User.CID).Select(imp => imp.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId).FirstOrDefault();
                Sessions.PlanId = PlanId;
                var result = controller.GetADSImprovementTacticType(EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region "Add Suggested Improvement Tactic"
        ///<summary>
        /// To Check Add Suggested Improvement Tactic
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Add_Suggested_Improvement_Tactic()
        {
            Console.WriteLine("Get ADS Improvement TacticType.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int EntityId = TaskData.ImprovementPlanProgramId;
                int TacticTypeId = TaskData.ImprovementTacticTypeId;
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.AddSuggestedImprovementTactic(EntityId, TacticTypeId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }


        }
        #endregion

        #region "Get Recommended Improvement TacticType"
        ///<summary>
        /// To Check Get Recommended Improvement TacticType
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Recommended_Improvement_TacticType()
        {
            Console.WriteLine("Get Recommended Improvement TacticType.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.GetRecommendedImprovementTacticType() as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region "Get Improvement Container Value"
        ///<summary>
        /// To Check Get Improvement Container Value
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Improvement_Container_Value()
        {
            Console.WriteLine("Get Recommended Improvement TacticType.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.GetImprovementContainerValue() as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region "Show Delete Improvement Tactic"
        ///<summary>
        /// To Check to Show Delete Improvement Tactic
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Show_Delete_Improvement_Tactic()
        {
            Console.WriteLine("Show Delete Improvement Tactic.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int EntityId = TaskData.ImprovementPlanTacticId;
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.ShowDeleteImprovementTactic(EntityId) as PartialViewResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
                Assert.IsNotNull(result.ViewName);
            }

        }
        #endregion

        #region "Delete Improvement Tactic From Grid"
        ///<summary>
        /// To Check to Delete Improvement Tactic From Grid.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Delete_Improvement_Tactic_From_Grid()
        {
            Console.WriteLine("Delete Improvement Tactic From Grid.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int EntityId = TaskData.ImprovementPlanTacticId;
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.DeleteImprovementTacticFromGrid(EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);

            }

        }
        #endregion

        #region "Delete Improvement Tactic"
        ///<summary>
        /// To Check to Delete Improvement Tactic.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Delete_Improvement_Tactic()
        {
            Console.WriteLine("Delete Improvement Tactic.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int EntityId = TaskData.ImprovementPlanTacticId;
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                var result = controller.DeleteImprovementTactic(EntityId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region "Update Effective Date Improvement"
        ///<summary>
        /// To Check to Update Effective Date Improvement.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Update_Effective_Date_Improvement()
        {
            Console.WriteLine("Update Effective Date Improvement.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int plan_Id = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(plan_Id);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var TaskData = DataHelper.GetPlanImprovementTactic(Sessions.User.CID);
            if (TaskData != null)
            {
                int EntityId = TaskData.ImprovementPlanTacticId;
                int PlanId = TaskData.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.PlanId;
                Sessions.PlanId = PlanId;
                string updatedDate = DateTime.Now.ToString();
                var result = controller.UpdateEffectiveDateImprovement(EntityId, updatedDate) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }


        }
        #endregion

        #region "Get Years Tab for Plan"
        ///<summary>
        /// To Check to Get Years Tab for Plan
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Years_Tab()
        {
            Console.WriteLine("Get Years Tab for Plan.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();

            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var result = controller.GetYearsTab() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Get Campaign"
        ///<summary>
        /// To Check to Get Campaign
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Campaign()
        {
            Console.WriteLine("Get Campaign.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var result = controller.GetCampaign() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Load TacticType Value"
        ///<summary>
        /// To Check to Load TacticType Value.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Load_TacticType_Value()
        {
            Console.WriteLine("to Load TacticType Value.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(PlanId);
            int TacticTypeId = DataHelper.GetPlanTactic(Sessions.User.CID).TacticTypeId;
            Sessions.PlanId = PlanId;
            var result = controller.LoadTacticTypeValue(TacticTypeId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Plan Selector"
        ///<summary>
        /// To Check to get Selected Plan
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Plan_Selector()
        {
            Console.WriteLine("to get Selected Plan.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var result = controller.PlanSelector() as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }
        #endregion

        #region "Get Plan Selector Data"
        ///<summary>
        /// To Check to Get Plan Selector Data for Passing Year
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Get_Plan_Selector_Data()
        {
            Console.WriteLine("to get Selected Plan.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string Year = DateTime.Now.Year.ToString();
            var result = controller.GetPlanSelectorData(Year) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }
        #endregion

        #region "Update StartDate And EndDate"
        ///<summary>
        /// To Check to Update StartDate And EndDate of Campaign
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Update_StartDate_And_EndDate_Campaign()
        {
            Console.WriteLine("Update StartDate And EndDate of Campaign.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var TaskData = DataHelper.GetPlanCampaign(Sessions.User.CID);
            int EntityId = TaskData.PlanCampaignId;
            string startDate = TaskData.StartDate.ToString();
            double Duration = 50.0;
            bool isCamp = true;
            bool isProg = false;
            bool isTact = false;

            var result = controller.UpdateStartEndDate(EntityId, startDate, Duration, isCamp, isProg, isTact) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        ///<summary>
        /// To Check to Update StartDate And EndDate of Program
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Update_StartDate_And_EndDate_Program()
        {
            Console.WriteLine("Update StartDate And EndDate of Program.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var TaskData = DataHelper.GetPlanProgram(Sessions.User.CID);
            int EntityId = TaskData.PlanProgramId;
            string startDate = TaskData.StartDate.ToString();
            double Duration = 50.0;
            bool isCamp = false;
            bool isProg = true;
            bool isTact = false;

            var result = controller.UpdateStartEndDate(EntityId, startDate, Duration, isCamp, isProg, isTact) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }


        ///<summary>
        /// To Check to Update StartDate And EndDate of Tactic
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Update_StartDate_And_EndDate_Tactic()
        {
            Console.WriteLine("Update StartDate And EndDate of Tactic.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var TaskData = DataHelper.GetPlanTactic(Sessions.User.CID);
            int EntityId = TaskData.PlanTacticId;
            string startDate = TaskData.StartDate.ToString();
            double Duration = 50.0;
            bool isCamp = false;
            bool isProg = false;
            bool isTact = true;

            var result = controller.UpdateStartEndDate(EntityId, startDate, Duration, isCamp, isProg, isTact) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region "Apply To Calendar"
        ///<summary>
        /// To Check to Apply To Calendar without passing Parameter.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Apply_To_Calendar_Without_Passing_Parameter()
        {
            Console.WriteLine("Apply To Calendar without passing Parameter.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            var result = controller.ApplyToCalendar() as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        ///<summary>
        /// To Check to Apply To Calendar with passing Parameter.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Apply_To_Calendar_With_Passing_Parameter()
        {
            Console.WriteLine("Apply To Calendar without passing Parameter.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string msg = "Apply To Calendar";
            bool isError = false;

            var result = controller.ApplyToCalendar(msg, isError) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        #endregion

        #region "Assortment"
        ///<summary>
        /// To Check to get Assortment with passing campaign id as Parameter.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Campaign_Program_Tacic_Assortment()
        {
            Console.WriteLine("Campaign Assortment.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            var TaskData = DataHelper.GetPlanTactic(Sessions.User.CID);
            int TacticId = TaskData.PlanTacticId;
            int ProgramId = TaskData.PlanProgramId;
            int CampaignId = TaskData.Plan_Campaign_Program.PlanCampaignId;
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            controller.ControllerContext.HttpContext.Session.Add("CampaignID", "123");
            Sessions.PlanId = PlanId;
            string updatedDate = DateTime.Now.ToString();
            var result = controller.Assortment(CampaignId, ProgramId, TacticId) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }

        #endregion

        #region "Create Clone"
        ///<summary>
        /// To Check to Create a Clone of LineItem.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void LineItem_Clone()
        {
            Console.WriteLine("Create a Clone of LineItem.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlanLineItem(Sessions.User.CID);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            string Title = TaskData.Title.ToString();
            int EntityId = TaskData.PlanLineItemId;
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var result = controller.Clone(Enums.EntityType.Lineitem.ToString(), EntityId, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        ///<summary>
        /// To Check to Create a Clone of Tactic.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Tactic_Clone()
        {
            Console.WriteLine("Create a Clone of Tactic.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlanTactic(Sessions.User.CID);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            string Title = TaskData.Title.ToString();
            int EntityId = TaskData.PlanTacticId;
            int PlanId = TaskData.Plan_Campaign_Program.Plan_Campaign.PlanId;
            Sessions.PlanId = PlanId;
            string CampaignId = TaskData.Plan_Campaign_Program.PlanCampaignId.ToString();
            var result = controller.Clone(Enums.EntityType.Tactic.ToString(), EntityId, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        ///<summary>
        /// To Check to Create a Clone of Program.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Program_Clone()
        {
            Console.WriteLine("Create a Clone of Program.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlanProgram(Sessions.User.CID);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            string Title = TaskData.Title.ToString();
            int EntityId = TaskData.PlanProgramId;
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;

            var result = controller.Clone(Enums.EntityType.Program.ToString(), EntityId, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        ///<summary>
        /// To Check to Create a Clone of Campaign.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Campaign_Clone()
        {
            Console.WriteLine("Create a Clone of Campaign.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlanCampaign(Sessions.User.CID);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            string Title = TaskData.Title.ToString();
            int EntityId = TaskData.PlanCampaignId;
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;

            var result = controller.Clone(Enums.EntityType.Campaign.ToString(), EntityId, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        ///<summary>
        /// To Check to Create a Clone of Plan.
        /// <author>Rahul Shah</author>
        /// <createdDate>30Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Plan_Clone()
        {
            Console.WriteLine("Create a Clone of Plan.\n");

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var TaskData = DataHelper.GetPlan(Sessions.User.CID);
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            controller.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), controller);
            string Title = TaskData.Title.ToString();
            int EntityId = TaskData.PlanId;

            Sessions.PlanId = EntityId;

            var result = controller.Clone(Enums.EntityType.Plan.ToString(), EntityId, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save Budget Allocation.
        /// <summary>
        /// To check to Save Budget Allocation.
        /// <author>Rahul Shah</author>
        /// <createddate>29June2016</createddate>
        /// </summary>
        [TestMethod]
        public void Save_Budget_Allocation()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Budget Allocation.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
    new RequestContext(
        objPlanController.HttpContext, new RouteData()
    ),
    routes
);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            List<string> lstinputs = new List<string>();
            {
                for (int i = 0; i < 12; i++)
                {
                    lstinputs.Add("1000");
                }
            }
            string Inputs = string.Join(",", lstinputs);
            var result = objPlanController.SaveBudgetAllocation(PlanId, Inputs) as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);

        }
        #endregion

        #region Get Budget Allocation.
        /// <summary>
        /// To get budget allocated data
        /// <author>Mitesh Vaishnav</author>
        /// <createddate>22Sept2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Budget_Allocation()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Budget Allocation.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
            new RequestContext(
            objPlanController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetPlanId();
            var result = objPlanController.GetBudgetData(PlanId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty) as PartialViewResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);
            Assert.AreEqual("~/Views/Budget/Budget.cshtml", result.ViewName);

        }
        #endregion

        #region "Import data for plan budget"
        /// <summary>
        /// To Check to import data for plan budget
        /// <author>Kausha Somaiya</author>
        /// <createdDate>26Sep2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Excel_FileUpload()
        {
            Console.WriteLine("Import data for plan budget.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            var result = controller.ExcelFileUpload() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

    }
}

//User user = new User();
//user.UserId = Guid.NewGuid();

////This will create the dummy session for Unit test project
//HttpContext.Current = MockHelpers.FakeHttpContext();
//HttpContext.Current.Session["User"] = user;



////// Maitri Notes

//public ActionResult LoadCopyEntityPopup(string entityId(PlanID_tactic ids), string section(enums.section.tactic), string PopupType(Enums.ModelTypeText.Copying))

//private DhtmlxGridRowDataModel CreateSubItem(ParentChildEntityMapping row, int parentId(parent of tactic=Program id(tactic.planprogram)), Enums.Section section, int destPlanId(planid), bool isAjaxCall)

//public JsonResult ClonetoOtherPlan(string CloneType, string srcEntityId (tacticId), string destEntityID(ProgramId), string srcPlanID, string destPlanID, string sourceEntityTitle(tacticTitle))
