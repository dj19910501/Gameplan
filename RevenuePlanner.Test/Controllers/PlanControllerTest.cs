using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using System.Configuration;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class PlanControllerTest
    { 
        PlanController objPlanController;
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
            objPlanController = new PlanController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            objPlanController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objPlanController);
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        }
             
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
            var result = controller.PublishPlan(Sessions.User.UserId) as JsonResult;
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
        #endregion

        #region Grid View
        
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
            string viewBy = PlanGanttTypes.Tactic.ToString();
            var result = controller.GetHomeGridData() as ActionResult;
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
            var result = controller.GetHomeGridData() as ActionResult;
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

        #region get plan grid data
         /// <summary>
        /// To get budget grid data with search
        /// <author>Devanshi gandhi</author>
        /// <createddate>22 nov 2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_PlanGridData_WithSearch()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get grid Data with Search .\n");
            MRPEntities db = new MRPEntities();
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
            new RequestContext(
            objPlanController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetMultiYearPlanId();
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(PlanId));
            int ModelId = DataHelper.GetPlanModelId(PlanId);
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            string viewby = PlanGanttTypes.Tactic.ToString();
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            string PlanYear = DataHelper.GetPlanYear(PlanId);
            string NextYear = Convert.ToString(Convert.ToInt32(PlanYear) + 1);
            string SearchText = "tactic";
            string SearchBy=Enums.GlobalSearch.ActivityName.ToString();
            var result = objPlanController.GetHomeGridDataJSON(PlanId.ToString(), OwnerIds, TacticTypeIds, StatusIds, string.Empty,viewby,false, SearchText,SearchBy, false) as JsonResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);

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
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
            new RequestContext(
            objPlanController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetPlanIdforBudget();
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(PlanId));
            int ModelId = DataHelper.GetPlanModelId(PlanId);
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            string PlanYear=DataHelper.GetPlanYear(PlanId);
            string viewby = PlanGanttTypes.Tactic.ToString();
            var result = objPlanController.GetBudgetData(PlanId.ToString(),viewby,OwnerIds,TacticTypeIds,StatusIds,string.Empty,PlanYear) as PartialViewResult;

            Assert.AreEqual("~/Views/Budget/Budget.cshtml", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);

        }

        /// <summary>
        /// To get budget allocated data
        /// <author>Mitesh Vaishnav</author>
        /// <createddate>22Sept2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Budget_Allocation_Monthly()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Monthly Budget Allocation .\n");
            MRPEntities db = new MRPEntities();
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
            new RequestContext(
            objPlanController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetPlanId();
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(PlanId));
            int ModelId = DataHelper.GetPlanModelId(PlanId);
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            string viewby = PlanGanttTypes.Tactic.ToString();
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            //string monthText = Enums.UpcomingActivitiesValues[strThisMonth].ToString();
            var result = objPlanController.GetBudgetData(PlanId.ToString(), viewby, OwnerIds, TacticTypeIds, StatusIds, string.Empty, strThisMonth) as PartialViewResult;

            Assert.AreEqual("~/Views/Budget/Budget.cshtml", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);

        }

        /// <summary>
        /// To get budget allocated data
        /// <author>Mitesh Vaishnav</author>
        /// <createddate>22Sept2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Budget_Allocation_Multiyear()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get multiyear Budget Allocation .\n");
            MRPEntities db = new MRPEntities();
            objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objPlanController.Url = new UrlHelper(
            new RequestContext(
            objPlanController.HttpContext, new RouteData()
            ),
            routes
            );
            int PlanId = DataHelper.GetMultiYearPlanId();
            string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(PlanId));
            int ModelId = DataHelper.GetPlanModelId(PlanId);
            List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
            string TacticTypeIds = string.Join(",", lstTacticTypeIds);
            List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
            string StatusIds = string.Join(",", lstStatus);
            string viewby = PlanGanttTypes.Tactic.ToString();
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            string PlanYear = DataHelper.GetPlanYear(PlanId);
            string NextYear =Convert.ToString(Convert.ToInt32(PlanYear) + 1);
            var result = objPlanController.GetBudgetData(PlanId.ToString(), viewby, OwnerIds, TacticTypeIds, StatusIds, string.Empty, PlanYear+"-"+NextYear) as PartialViewResult;

            Assert.AreEqual("~/Views/Budget/Budget.cshtml", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);

        }
        ///// <summary>
        ///// To get budget grid data with search
        ///// <author>Devanshi gandhi</author>
        ///// <createddate>22 nov 2016</createddate>
        ///// </summary>
        //[TestMethod]
        //public void Test_Get_Budget_Data_WithSearch()
        //{
        //    var routes = new RouteCollection();
        //    Console.WriteLine("Get Budget Data with Search .\n");
        //    MRPEntities db = new MRPEntities();
        //    objPlanController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        //    objPlanController.Url = new UrlHelper(
        //    new RequestContext(
        //    objPlanController.HttpContext, new RouteData()
        //    ),
        //    routes
        //    );
        //    int PlanId = DataHelper.GetPlanIdforBudget();
        //    string OwnerIds = Convert.ToString(DataHelper.GetPlanOwnerId(PlanId));
        //    int ModelId = DataHelper.GetPlanModelId(PlanId);
        //    List<string> lstTacticTypeIds = DataHelper.GetTacticTypeList(ModelId).Select(a => Convert.ToString(a.TacticTypeId)).ToList();
        //    string TacticTypeIds = string.Join(",", lstTacticTypeIds);
        //    List<string> lstStatus = Enums.TacticStatusValues.Select(a => a.Value).ToList();
        //    string StatusIds = string.Join(",", lstStatus);
        //    string viewby = PlanGanttTypes.Tactic.ToString();
        //    string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
        //    string PlanYear = DataHelper.GetPlanYear(PlanId);
        //    string NextYear = Convert.ToString(Convert.ToInt32(PlanYear) + 1);
        //    string SearchText = "test";
        //    string SearchBy = Enums.GlobalSearch.ActivityName.ToString();
        //    var result = objPlanController.GetBudgetData(PlanId.ToString(), viewby, OwnerIds, TacticTypeIds, StatusIds, string.Empty, PlanYear + "-" + NextYear, SearchText, SearchBy,true) as PartialViewResult;

        //    Assert.AreEqual("~/Views/Budget/Budget.cshtml", result.ViewName);
        //    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.ViewName);

        //}
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
