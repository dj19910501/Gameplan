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


namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class PlanControllerTest
    {
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

            int planId = result.GetValue<int>("id");

            Assert.IsTrue(Convert.ToBoolean(planId));
            if (Convert.ToBoolean(planId))
            {


                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planId);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + planId);
            }

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

            int planId = result.GetValue<int>("id");

            Assert.IsTrue(Convert.ToBoolean(planId));

            if (Convert.ToBoolean(planId))
            {


                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planId);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + planId);
            }


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

            // data object should not be null in json result
            Assert.IsNotNull(result.Data);

            int planId = result.GetValue<int>("id");

            Assert.IsTrue(Convert.ToBoolean(planId));
            if (Convert.ToBoolean(planId))
            {


                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planId);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + planId);
            }

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

            // data object should not be null in json result
            Assert.IsNotNull(result.Data);

            int planId = result.GetValue<int>("id");

            Assert.IsTrue(Convert.ToBoolean(planId));
            if (Convert.ToBoolean(planId))
            {


                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planId);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + planId);
            }

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
            var result = controller.PublishPlan(Sessions.User.UserId.ToString()) as JsonResult;
            if (result != null)
            {
                // data object should not be null in json result
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }


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
            if (result != null)
            {
                // data object should not be null in json result
                Assert.IsNotNull(result);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Model);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }


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
            if (result != null)
            {
                // data object should not be null in json result
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }

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
            if (result != null)
            {
                // data object should not be null in json result
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }

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

            Plan_Campaign campaign = db.Plan_Campaign.Where(a => a.Plan.Model.ClientId == Sessions.User.ClientId && a.IsDeleted == false && a.Plan.AllocatedBy.ToLower() != "quarter").OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            int CampaignId = 0;
            if (campaign != null)
            {
                CampaignId = campaign.PlanCampaignId;
            }
            var result = objPlanController.GetBudgetAllocationCampaignData(CampaignId);

            if (result != null)
            {
                var serializedData = new RouteValueDictionary(result.Data);
                var budgetData = serializedData["budgetData"];
                var planRemainingBudget = serializedData["planRemainingBudget"];
                // data object should not be null in json result
                Assert.IsNotNull(budgetData);
                Assert.IsNotNull(planRemainingBudget);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + budgetData);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planRemainingBudget);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
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

            Plan_Campaign campaign = DataHelper.GetPlanCampaign(Sessions.User.ClientId);
            int CampaignId = 0;
            if (campaign != null)
            {
                CampaignId = campaign.PlanCampaignId;
            }
            var result = objPlanController.GetBudgetAllocationCampaignData(CampaignId);
            
            if (result != null)
            {
                var serializedData = new RouteValueDictionary(result.Data);
                var budgetData = serializedData["budgetData"];
                var planRemainingBudget = serializedData["planRemainingBudget"];
                // data object should not be null in json result
                Assert.IsNotNull(budgetData);
                Assert.IsNotNull(planRemainingBudget);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + budgetData);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + planRemainingBudget);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #region Grid View

        [TestMethod]
        public void LoadGrid()
        {
            Console.WriteLine("To Load Grid View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            var result = controller.LoadHomeGrid(PlanId.ToString(), Ownerids, tactictypeids, Status, CommaSeparatedCustomFields) as Task<ActionResult>;
            if (result != null)
            {
                //     data object should not be null in json result
                Assert.IsNotNull(result, "Pass");

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Status);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Status);
            }

        }

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
            if (result != null)
            {
                // data object should not be null in json result
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }


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
            if (result != null)
            {
                Assert.AreEqual("~/Views/Plan/_CopyEntity.cshtml", result.ViewName);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.ViewName);
            }
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
            if (result != null)
            {
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
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
            Sessions.User.ClientId = DataHelper.GetClientId(planId);
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            var tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(t =>
                new
                {
                    TacticId = t.PlanTacticId,
                    PlanProgramId = t.Plan_Campaign_Program.PlanProgramId,
                    PlanId = t.Plan_Campaign_Program.Plan_Campaign.PlanId,
                    Title = t.Title
                }).ToList();

            var result = controller.ClonetoOtherPlan(Enums.Section.Tactic.ToString(), tactic[0].TacticId.ToString(), tactic[1].TacticId.ToString(), tactic[0].PlanId.ToString(), tactic[1].PlanId.ToString(), tactic[0].Title);
            if (result != null)
            {
                Assert.IsNotNull(result.Data);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }

        #endregion

        #region "Link Entities from One Plan to Another"

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
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            List<int> honeyCombIds = new List<int>();
            honeyCombIds.Add(DataHelper.GetPlanTactic(Sessions.User.ClientId).PlanTacticId);

            string honeyCombId = string.Join(",", honeyCombIds);

            var result = objPlanController.ExportToCsv(Ownerids, tactictypeids, Status, CommaSeparatedCustomFields, honeyCombId, PlanId) as JsonResult;



            if (result != null)
            {
                var serializedData = new RouteValueDictionary(result.Data);
                var fileName = serializedData["data"];
                Assert.IsNotNull(fileName);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + fileName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
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
            Sessions.PlanId = PlanId;
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            List<int> honeyCombIds = new List<int>();
            honeyCombIds.Add(DataHelper.GetPlanTactic(Sessions.User.ClientId).PlanTacticId);

            string honeyCombId = string.Join(",", honeyCombIds);

            var result = objPlanController.ExportToCsv(null, null, null, null, honeyCombId, PlanId) as JsonResult;

            if (result != null)
            {
                var serializedData = new RouteValueDictionary(result.Data);
                var fileName = serializedData["data"];
                Assert.IsNotNull(fileName);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + fileName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

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
            Sessions.User.ClientId = DataHelper.GetClientId(PlanId);
            string CommaSeparatedPlanId = DataHelper.GetPlanIdList();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<Guid> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);
            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(Sessions.User.UserId);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            var result = objPlanController.ExportToCsv(Ownerids, tactictypeids, Status, CommaSeparatedCustomFields, null, PlanId) as JsonResult;
            if (result != null)
            {
                var serializedData = new RouteValueDictionary(result.Data);
                var fileName = serializedData["data"];
                Assert.IsNotNull(fileName);

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + fileName);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

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
