using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
//using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Moq;
using System.Web;
using System.Web.Routing;


namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class InspectControllerTest //: CommonController
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        #region Save Plan
        ///// <summary>
        ///// To Save the Plan
        ///// </summary>
        ///// <auther>Komal Rawal</auther>
        ///// <createddate>29June2015</createddate>
        //[TestMethod]
        //public void Save_Plan()
        //{
        //    Console.WriteLine("To Save the Plan.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    //base.Initialize(System.Web.HttpContext.Current.r);
        //    InspectController objInspectController = new InspectController();

        //    InspectModel objPlanModel = new InspectModel();
        //    objPlanModel.GoalType = Enums.PlanGoalType.MQL.ToString();
        //    objPlanModel.GoalValue = "0";
        //    objPlanModel.Title = "test plan #975";
        //    objPlanModel.ModelId = DataHelper.GetModelId();
        //    objPlanModel.StartDate = DateTime.Now;
        //    objPlanModel.EndDate = DateTime.MaxValue;
        //    objPlanModel.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
        //    objPlanModel.PlanId = db.Plans.Where(plan => plan.Title == objPlanModel.Title && plan.ModelId == objPlanModel.ModelId).Select(plan => plan.PlanId).FirstOrDefault();
        //    List<int> PlanIds = new List<int>();
        //    PlanIds.Add(objPlanModel.PlanId);
        //    Sessions.PlanPlanIds = PlanIds;
        //    string UserID = (Sessions.User.UserId).ToString();
        //    string planBudget = "50000";

        //    string Budgetvalues = GetBudgetValues(objPlanModel.AllocatedBy);

        //    var result = objInspectController.SavePlanDetails(objPlanModel, Budgetvalues, planBudget, "", UserID) as JsonResult;

        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result.Data);
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
        //    }
        //    else
        //    {

        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }
        //}


        #endregion

        #region Campaign Related Methods

        #region Save Campaign
        /// <summary>
        /// To Save the Campaign
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Save_Campaign()
        {

            Console.WriteLine("To Save the Campaign.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;
            string Title = "Test Campaign" + "_ " + DateTime.Now;
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = 0;
            Form.Owner = "sys Admin";
            Form.Title = "Test Campaign" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;
            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save Campaign Check duplicate
        /// <summary>
        /// To check the duplicate Campaign
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Save_Campaign_Check_Duplicate()
        {
            Console.WriteLine("To check the duplicate Campaign.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            string Title = db.Plan_Campaign.Where(id => id.PlanId == PlanID).Select(campaign => campaign.Title).FirstOrDefault();
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = 0;
            Form.Owner = "sys Admin";
            Form.Title = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;

            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Update Campaign
        /// <summary>
        /// To Update Campaign
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Update_Campaign()
        {
            Console.WriteLine("To Update Campaign.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            Sessions.PlanId = PlanID;
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;
            string Title = "Update Campaign";
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            Form.Owner = "sys Admin";
            Form.Title = "Update Campaign";
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;

            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
       
        }
        #endregion

        #region Save Campaign Budget Allocation
        ///// <summary>
        ///// To Save the Plan
        ///// </summary>
        ///// <auther>Komal Rawal</auther>
        ///// <createddate>29June2015</createddate>
        //[TestMethod]
        //public void Save_Campaign_Budget_Allocation()
        //{
        //    Console.WriteLine("To Save the Plan.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    InspectController objInspectController = new InspectController();
        //    string UserID = (Sessions.User.UserId).ToString();
        //    int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
        //    Sessions.PlanId = PlanID;
        //    string Title = db.Plan_Campaign.Where(id => id.PlanId == PlanID).Select(campaign => campaign.Title).FirstOrDefault();

        //    Plan_CampaignModel Form = new Plan_CampaignModel();
        //    Form.PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
        //    Form.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
        //    Form.CampaignBudget = 10000;
        //    Form.PlanId = PlanID;

        //    string Budgetvalues = GetBudgetValues(Form.AllocatedBy);

        //    var result = objInspectController.SaveCampaignBudgetAllocation(Form, Budgetvalues, UserID, Title) as JsonResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result.Data);
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
        //    }
        //    else
        //    {

        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }
        //}

        #endregion

        #region Save comment in Review Tab
        /// <summary>
        /// To Save  comment in Review Tab
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Comment_Campaign()
        {
            Console.WriteLine("To Save  comment in Review Tab.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //System.Web.HttpContext.Current.Request.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            //// Call index method
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            var result = Save_Comment(PlanCampaignId, Enums.Section.Campaign.ToString().ToLower());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  \n The Assert Value result :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }



        #endregion

        #endregion

        #region Program Related Methods

        #region Save Program
        /// <summary>
        /// To Save the Program
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Program()
        {

            Console.WriteLine("To Save the Program.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            string Title = "Test Program" + "_ " + DateTime.Now;
            Plan_Campaign_ProgramModel Form = new Plan_Campaign_ProgramModel();
            Form.PlanProgramId = 0;
            Form.OwnerId = Sessions.User.UserId;
            Form.Title = "Test Program" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanCampaignId = PlanCampaignId;
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;
            var result = objInspectController.SetupSaveProgram(Form, "[]", UserID, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  \n The Assert Value result :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save Program Check duplicate
        /// <summary>
        /// To check the duplicate Program
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Program_Check_Duplicate()
        {
            Console.WriteLine("To check the duplicate Program.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            string Title = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.Title).FirstOrDefault();
            Plan_Campaign_ProgramModel Form = new Plan_Campaign_ProgramModel();
            Form.PlanProgramId = 0;
            Form.OwnerId = Sessions.User.UserId;
            Form.Title = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanCampaignId = PlanCampaignId;
            var result = objInspectController.SetupSaveProgram(Form, "[]", UserID, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        
        }
        #endregion

        #region Update Program
        /// <summary>
        /// To Update Program
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Update_Program()
        {
            Console.WriteLine("To Update Program.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;
            string Title = "Update Program";
            Plan_Campaign_ProgramModel Form = new Plan_Campaign_ProgramModel();
            Form.PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            Form.OwnerId = Sessions.User.UserId;
            Form.Title = "Update Program";
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanCampaignId = PlanCampaignId;

            var result = objInspectController.SetupSaveProgram(Form, "[]", UserID, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
         
        }
        #endregion

        #region Save Program Budget Allocation
        ///// <summary>
        ///// To Save the Program
        ///// </summary>
        ///// <auther>Komal Rawal</auther>
        ///// <createddate>29June2015</createddate>
        //[TestMethod]
        //public void Save_Program_Budget_Allocation()
        //{
        //    Console.WriteLine("To Save the Program.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    InspectController objInspectController = new InspectController();
        //    string UserID = (Sessions.User.UserId).ToString();
        //    int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
        //    int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();

        //    string Title = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.Title).FirstOrDefault();
        //    Plan_Campaign_ProgramModel Form = new Plan_Campaign_ProgramModel();
        //    Form.PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
        //    Form.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
        //    Form.ProgramBudget = 10000;

        //    string Budgetvalues = GetBudgetValues(Form.AllocatedBy);

        //    var result = objInspectController.SaveProgramBudgetAllocation(Form, Budgetvalues, UserID, Title) as JsonResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result.Data);
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
        //    }
        //    else
        //    {

        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }
        //}

        #endregion

        #region Save comment in Review Tab
        /// <summary>
        /// To Save  comment in Review Tab
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Comment_Program()
        {
            Console.WriteLine("To Save  comment in Review Tab.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int PlanID = DataHelper.GetPlanId();
            Sessions.User.ClientId = DataHelper.GetClientId(PlanID);
            Sessions.User.UserId = DataHelper.GetUserId(PlanID);
            string UserID = (Sessions.User.UserId).ToString();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            var result = Save_Comment(PlanProgramId, Enums.Section.Program.ToString().ToLower());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }



        #endregion

        #endregion

        #region Tactic Related Methods

        #region Save Tactic
        /// <summary>
        /// To Save the Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Tactic()
        {
            Console.WriteLine("To Save the Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            string UserID = (Sessions.User.UserId).ToString();
            
            int PlanID = DataHelper.GetPlanId();
            int ModelId = db.Plans.Where(pl => pl.PlanId == PlanID).Select(pl => pl.ModelId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int TacticTypeId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.TacticTypeId).FirstOrDefault();
            int? StageId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.StageId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;

            string Title = "Test Tactic" + "_ " + DateTime.Now;
            Inspect_Popup_Plan_Campaign_Program_TacticModel Form = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            Form.PlanTacticId = 0;
            Form.PlanProgramId = PlanProgramId;
            Form.PlanCampaignId = PlanCampaignId;
            Form.OwnerId = Sessions.User.UserId;
            Form.TacticTypeId = TacticTypeId;
            Form.StageId = Convert.ToInt32(StageId);
            Form.TacticTitle = "Test Tactic" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.Cost = 20;

            var result = objInspectController.SetupSaveTactic(Form, "", "", "[]", UserID, "", false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        [TestMethod]
        public void Save_ExistingTactic()
        {
            Console.WriteLine("To Save the Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            string UserID = (Sessions.User.UserId).ToString();
            var planTactic = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false && a.LinkedTacticId != null).FirstOrDefault();

            int PlanID = planTactic.Plan_Campaign_Program.Plan_Campaign.PlanId;
            int ModelId = planTactic.Plan_Campaign_Program.Plan_Campaign.Plan.ModelId;
            int PlanCampaignId = planTactic.Plan_Campaign_Program.PlanCampaignId;
            int PlanProgramId = planTactic.PlanProgramId;
            
            int TacticTypeId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.TacticTypeId).FirstOrDefault();
            int? StageId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.StageId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;

            string Title = "Test Tactic" + "_ " + DateTime.Now;
            Inspect_Popup_Plan_Campaign_Program_TacticModel Form = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            Form.PlanTacticId = planTactic.PlanTacticId;
            Form.PlanProgramId = PlanProgramId;
            Form.PlanCampaignId = PlanCampaignId;
            Form.OwnerId = Sessions.User.UserId;
            Form.TacticTypeId = TacticTypeId;
            Form.StageId = Convert.ToInt32(StageId);
            Form.TacticTitle = "Test Tactic" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.Cost = 20;

            var result = objInspectController.SetupSaveTactic(Form, "", "", "[]", UserID, "", false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region LoadSetup
        [TestMethod]
        public void LoadImprovementSetupEditMode()
        {
            Console.WriteLine("To open inspect popup for improvement tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var improvementTactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(i => i.IsDeleted == false).FirstOrDefault();
            int improvementTacticId = improvementTactic.ImprovementPlanTacticId;
            var Editresult = objInspectController.LoadImprovementSetup(improvementTacticId, "Edit") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + Editresult.ViewName);
            int improvementProgramId = improvementTactic.ImprovementPlanProgramId;
            var Addresult = objInspectController.LoadImprovementSetup(improvementProgramId, "Add") as PartialViewResult;
            Assert.AreEqual("_SetupImprovementTactic", Editresult.ViewName);
            Assert.AreEqual("_SetupImprovementTactic", Addresult.ViewName);
        }
        [TestMethod]
        public void LoadEditSetupCampaign()
        {
            Console.WriteLine("To open inspect popup for campaign setup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int planCampaignId = db.Plan_Campaign.Where(a => a.IsDeleted == false).Select(a => a.PlanCampaignId).FirstOrDefault();
            var result= objInspectController.LoadEditSetupCampaign(planCampaignId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_EditSetupCampaign", result.ViewName);
        }

        [TestMethod]
        public void LoadSetupProgramEdit()
        {
            Console.WriteLine("To open inspect popup for program setup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int planProgramId = db.Plan_Campaign_Program.Where(a => a.IsDeleted == false).Select(a => a.PlanProgramId).FirstOrDefault();
            var result = objInspectController.LoadSetupProgramEdit(planProgramId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_EditSetupProgram", result.ViewName);
        }
        //[TestMethod]
        //public void LoadActuals()
        //{
        //    Console.WriteLine("To load actuals of tactic.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    InspectController objInspectController = new InspectController();
        //    objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
        //    objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
        //    int tacticId = db.Plan_Campaign_Program_Tactic_Actual.Where(a => a.Plan_Campaign_Program_Tactic.IsDeleted == false).Select(a => a.PlanTacticId).FirstOrDefault();
        //    var result = objInspectController.LoadActuals(tacticId) as PartialViewResult;
        //    Assert.AreEqual("Actual", result.ViewName);
        //}
        [TestMethod]
        public void LoadReviewCampaign()
        {
            Console.WriteLine("To load review tab of campaign.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int campignId = db.Plan_Campaign.Where(a => a.IsDeleted == false).Select(a => a.PlanCampaignId).FirstOrDefault();
            var result = objInspectController.LoadReviewCampaign(campignId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReviewCampaign", result.ViewName);
        }

        [TestMethod]
        public void LoadReviewProgram()
        {
            Console.WriteLine("To load review tab of program.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int programId = db.Plan_Campaign_Program.Where(a => a.IsDeleted == false).Select(a => a.PlanProgramId).FirstOrDefault();
            var result = objInspectController.LoadReviewProgram(programId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_ReviewProgram", result.ViewName);
        }
        #endregion

        #region Load Inspect Popup
        [TestMethod]
        public void LoadInspectPopupTactic()
        {
            Console.WriteLine("To open tactic popup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var planTactic = db.Plan_Campaign_Program_Tactic.Where(a => a.IsDeleted == false && a.LinkedTacticId != null).FirstOrDefault();
            var result = objInspectController.LoadInspectPopup(planTactic.PlanTacticId, "Tactic", "Setup", "Edit", planTactic.PlanProgramId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("InspectPopup", result.ViewName); //InspectPopup
        }
        [TestMethod]
        public void LoadInspectPopupProgram()
        {
            Console.WriteLine("To open program popup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var planProgram = db.Plan_Campaign_Program.Where(a => a.IsDeleted == false).FirstOrDefault();
            var result = objInspectController.LoadInspectPopup(planProgram.PlanProgramId, "Program", "Setup", "Edit", planProgram.PlanCampaignId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_InspectPopupProgram", result.ViewName); //_InspectPopupProgram
        }
        [TestMethod]
        public void LoadInspectPopupCampaign()
        {
            Console.WriteLine("To open campaign popup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var planCampign = db.Plan_Campaign.Where(a => a.IsDeleted == false).FirstOrDefault();
            var result = objInspectController.LoadInspectPopup(planCampign.PlanCampaignId, "Campaign", "Setup", "Edit", planCampign.PlanId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_InspectPopupCampaign", result.ViewName);
        }

        [TestMethod]
        public void LoadInspectPopupPlan()
        {
            Console.WriteLine("To open Plan popup.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var Plan = db.Plans.Where(a => a.IsDeleted == false).FirstOrDefault();
            var result = objInspectController.LoadInspectPopup(Plan.PlanId, "Plan", "Setup", "Edit") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_InspectPopupPlan", result.ViewName);
        }

        #endregion

        #region Save Tactic Check duplicate
        /// <summary>
        /// To check the duplicate Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Tactic_Check_Duplicate()
        {
            Console.WriteLine("To check the duplicate Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            string Title = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.Title).FirstOrDefault();
            Inspect_Popup_Plan_Campaign_Program_TacticModel Form = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            Form.PlanTacticId = 0;
            Form.PlanProgramId = PlanProgramId;
            Form.PlanCampaignId = PlanCampaignId;
            Form.OwnerId = Sessions.User.UserId;
            Form.TacticTitle = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;


            var result = objInspectController.SetupSaveTactic(Form, "", "", "[]", UserID, "", false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Update Tactic
        /// <summary>
        /// To Update Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Update_Tactic()
        {
            Console.WriteLine("To Update Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);

            string UserID = (Sessions.User.UserId).ToString();
            int ModelId = DataHelper.GetModelId();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int TacticTypeId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.TacticTypeId).FirstOrDefault();
            int? StageId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.StageId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;
            string Title = "Update Tactic";
            Inspect_Popup_Plan_Campaign_Program_TacticModel Form = new Inspect_Popup_Plan_Campaign_Program_TacticModel();
            Form.PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            Form.PlanProgramId = PlanProgramId;
            Form.PlanCampaignId = PlanCampaignId;
            Form.OwnerId = Sessions.User.UserId;
            Form.TacticTypeId = TacticTypeId;
            Form.StageId = Convert.ToInt32(StageId);
            Form.TacticTitle = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.Cost = 500;

            var result = objInspectController.SetupSaveTactic(Form, "", "", "[]", UserID, "", false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save Tactic Budget Allocation
        // Commented by Arpita Soni for Ticket #2236 on 06/07/2016
        /// <summary>
        /// To Save the Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        //[TestMethod]
        //public void Save_Tactic_Budget_Allocation()
        //{

        //    Console.WriteLine("To Save the Tactic.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    InspectController objInspectController = new InspectController();
        //    string UserID = (Sessions.User.UserId).ToString();
        //    int ModelId = DataHelper.GetModelId();
        //    int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
        //    int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
        //    int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
        //    int TacticTypeId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.TacticTypeId).FirstOrDefault();
        //    int? StageId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.StageId).FirstOrDefault();


        //    string Title = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.Title).FirstOrDefault();
        //    Plan_Campaign_Program_TacticModel Form = new Plan_Campaign_Program_TacticModel();
        //    Form.PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault(); ;
        //    Form.PlanProgramId = PlanProgramId;
        //    Form.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
        //    Form.TacticBudget = 10000;

        //    string Budgetvalues = GetBudgetValues(Form.AllocatedBy);

        //    var result = objInspectController.SaveTacticBudgetAllocation(Form, Budgetvalues, UserID, Title) as JsonResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result.Data);
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
        //    }
        //    else
        //    {

        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }
        //}

        #endregion

        #region Save comment in Review Tab
        /// <summary>
        /// To Save  comment in Review Tab
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Comment_Tactic()
        {
            Console.WriteLine("To Save  comment in Review Tab.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            var result = Save_Comment(PlanTacticId, Enums.Section.Tactic.ToString().ToLower());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
       }


        #endregion

        #region Save Tactic Actual data
        /// <summary>
        /// To Save Tactic Actual data
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Tactic_Actuals()
        {
            Console.WriteLine("To Save Tactic Actual data.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int ModelId = DataHelper.GetModelId();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int TacticTypeId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.TacticTypeId).FirstOrDefault();
            int PlanLineItemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(id => id.PlanLineItemId).FirstOrDefault();
            int? StageId = db.TacticTypes.Where(id => id.ModelId == ModelId).Select(tactictype => tactictype.StageId).FirstOrDefault();

            string Title = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.Title).FirstOrDefault();

            List<InspectActual> tacticactual = new List<InspectActual>();
            tacticactual.Add(new InspectActual { ActualValue = 0, IsActual = false, Period = "0", PlanLineItemId = 0, PlanTacticId = PlanTacticId, ROI = -1.0, ROIActual = -1.0, StageId = Convert.ToInt32(StageId), StageTitle = "0", TotalCostActual = 0, TotalCWActual = 0, TotalMQLActual = 0, TotalProjectedStageValueActual = 0, TotalRevenueActual = 0 });

            List<Plan_Campaign_Program_Tactic_LineItem_Actual> lineItemActual = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
            lineItemActual.Add(new Plan_Campaign_Program_Tactic_LineItem_Actual { Period = "Y1", PlanLineItemId = PlanLineItemId, Value = 110 });
            lineItemActual.Add(new Plan_Campaign_Program_Tactic_LineItem_Actual { Period = "Y2", PlanLineItemId = PlanLineItemId, Value = 0 });
            lineItemActual.Add(new Plan_Campaign_Program_Tactic_LineItem_Actual { Period = "Y3", PlanLineItemId = PlanLineItemId, Value = 100 });
            lineItemActual.Add(new Plan_Campaign_Program_Tactic_LineItem_Actual { Period = "Y4", PlanLineItemId = PlanLineItemId, Value = 0 });
            var result = objInspectController.UploadResult(tacticactual, lineItemActual, UserID, Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #endregion

        #region LineItem Related Methods

        #region Save LineItem
        /// <summary>
        /// To Save the LineItem
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_LineItem()
        {

            Console.WriteLine("To Save the LineItem.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int ModelId = DataHelper.GetModelId();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.IsDeleted==false && id.LinkedTacticId!=null).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int LineitemTypeId = db.LineItemTypes.Where(id => id.ModelId == ModelId).Select(Lineitem => Lineitem.LineItemTypeId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            string Title = "Test Lineitem" + "_ " + DateTime.Now;
            Plan_Campaign_Program_Tactic_LineItemModel Form = new Plan_Campaign_Program_Tactic_LineItemModel();
            Form.PlanTacticId = PlanTacticId;
            Form.PlanLineItemId = 0;
            Form.LineItemTypeId = LineitemTypeId;
            Form.Title = "Test Lineitem" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.Cost = 20;
            var result = objInspectController.SaveLineitem(Form, Title, "[{Id:13,Weightage:500}]", "[{\"Key\":\"1\",\"Value\":\"bar\"}]", UserID, PlanTacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }


        [TestMethod]
        public void Save_CostLineItem()
        {

            Console.WriteLine("To Save the LineItem.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int ModelId = DataHelper.GetModelId();
            string UserID = (Sessions.User.UserId).ToString();
            //int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            //int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            //int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            //int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int CostLineItemId = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(lc => lc.Plan_Campaign_Program_Tactic_LineItem.IsDeleted == false && lc.Plan_Campaign_Program_Tactic_LineItem.LinkedLineItemId!=null && lc.Plan_Campaign_Program_Tactic_LineItem.Plan_Campaign_Program_Tactic.IsDeleted == false).Select(a => a.PlanLineItemId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.PlanLineItemId == CostLineItemId).Select(a => a.PlanTacticId).FirstOrDefault();
                //db.Plan_Campaign_Program_Tactic.Where(id => id.IsDeleted == false && id.LinkedTacticId != null).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int LineitemTypeId = db.LineItemTypes.Where(id => id.ModelId == ModelId).Select(Lineitem => Lineitem.LineItemTypeId).FirstOrDefault();

            List<int> PlanIds = new List<int>();
            //PlanIds.Add(PlanID);
            //Sessions.PlanPlanIds = PlanIds;
            string Title = "Test Lineitem" + "_ " + DateTime.Now;
            Plan_Campaign_Program_Tactic_LineItemModel Form = new Plan_Campaign_Program_Tactic_LineItemModel();
            Form.PlanTacticId = PlanTacticId;
            Form.PlanLineItemId = CostLineItemId;
            Form.LineItemTypeId = LineitemTypeId;
            Form.Title = "Test Lineitem" + "_ " + DateTime.Now;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.Cost = 20;
            var result = objInspectController.SaveLineitem(Form, Title, "[{Id:13,Weightage:500}]", "[{\"Key\":\"1\",\"Value\":\"bar\"}]", UserID, PlanTacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Save LineItem Check duplicate
        /// <summary>
        /// To check the duplicate LineItem
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_LineItem_Check_Duplicate()
        {
            Console.WriteLine("To check the duplicate LineItem.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int ModelId = DataHelper.GetModelId();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int LineitemTypeId = db.LineItemTypes.Where(id => id.ModelId == ModelId).Select(Lineitem => Lineitem.LineItemTypeId).FirstOrDefault();

            string Title = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(Lineitem => Lineitem.Title).FirstOrDefault();
            Plan_Campaign_Program_Tactic_LineItemModel Form = new Plan_Campaign_Program_Tactic_LineItemModel();
            Form.PlanTacticId = PlanTacticId;
            Form.PlanLineItemId = 0;
            Form.LineItemTypeId = LineitemTypeId;
            Form.Title = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;

            var result = objInspectController.SaveLineitem(Form, Title, "[{Id:13,Weightage:500}]", "[{\"Key\":\"1\",\"Value\":\"bar\"}]", UserID, PlanTacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Update LineItem
        /// <summary>
        /// To Update LineItem
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Update_LineItem()
        {
            Console.WriteLine("To Update LineItem.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            int ModelId = DataHelper.GetModelId();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int LineitemTypeId = db.LineItemTypes.Where(id => id.ModelId == ModelId).Select(Lineitem => Lineitem.LineItemTypeId).FirstOrDefault();
            List<int> PlanIds = new List<int>();
            PlanIds.Add(PlanID);
            Sessions.PlanPlanIds = PlanIds;

            string Title = "Updatee LineItem";
            Plan_Campaign_Program_Tactic_LineItemModel Form = new Plan_Campaign_Program_Tactic_LineItemModel();
            Form.PlanTacticId = PlanTacticId;
            Form.PlanLineItemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(tactic => tactic.PlanLineItemId).FirstOrDefault();
            Form.LineItemTypeId = LineitemTypeId;
            Form.Title = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;


            var result = objInspectController.SaveLineitem(Form, Title, "[{Id:13,Weightage:500}]", "[{\"Key\":\"1\",\"Value\":\"bar\"}]", UserID, PlanTacticId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save LineItem Budget Allocation
        ///// <summary>
        ///// To Save the LineItem Budget Allocation
        ///// </summary>
        ///// <auther>Komal Rawal</auther>
        ///// <createddate>29June2015</createddate>
        //[TestMethod]
        //public void Save_LineItem_Budget_Allocation()
        //{
        //    Console.WriteLine("To Save the LineItem Budget Allocation.\n");
        //    MRPEntities db = new MRPEntities();
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    //// Call index method
        //    InspectController objInspectController = new InspectController();
        //    string UserID = (Sessions.User.UserId).ToString();
        //    int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
        //    int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
        //    int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
        //    int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
        //    int LineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(tactic => tactic.PlanLineItemId).FirstOrDefault();

        //    string Title = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.Title).FirstOrDefault();
        //    Plan_Campaign_Program_Tactic_LineItemModel Form = new Plan_Campaign_Program_Tactic_LineItemModel();
        //    Form.PlanTacticId = PlanTacticId;
        //    Form.PlanLineItemId = LineitemId;
        //    Form.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
        //    Form.Cost = 10000;


        //    string Budgetvalues = GetBudgetValues(Form.AllocatedBy);

        //    var result = objInspectController.SaveLineItemBudgetAllocation(Form, Budgetvalues, UserID, Title) as JsonResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result.Data);
        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
        //    }
        //    else
        //    {

        //        Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
        //    }
        //}

        #endregion

        #region Save LineItem Actual data
        /// <summary>
        /// To Save LineItem Actual data
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_LineItem_Actuals()
        {
            Console.WriteLine("To Save LineItem Actual data.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController objPlanController = new PlanController();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId).Select(program => program.PlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            int LineitemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(tactic => tactic.PlanLineItemId).FirstOrDefault();
            string Title = db.Plan_Campaign_Program_Tactic_LineItem.Where(id => id.PlanTacticId == PlanTacticId).Select(tactic => tactic.Title).FirstOrDefault();

            string Y1, Y2, Y3, Y4, Y5, Y6, Y7, Y8, Y9, Y10, Y11, Y12;

            Y1 = "100";
            Y2 = "200";
            Y3 = "300";
            Y4 = "100";
            Y5 = "200";
            Y6 = "300";
            Y7 = "200";
            Y8 = "200";
            Y9 = "300";
            Y10 = "100";
            Y11 = "200";
            Y12 = "300";

            string Values = Y1 + ',' + Y2 + ',' + Y3 + ',' + Y4 + ',' + Y5 + ',' + Y6 + ',' + Y7 + ',' + Y8 + ',' + Y9 + ',' + Y10 + ',' + Y11 + ',' + Y12;


            var result = objPlanController.SaveActualsLineitemData(Values, LineitemId.ToString(), Title) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Load Line Item Grid in Tactic Inspect Popup
        /// To Load Line Item Grid in Tactic Inspect Popup
        /// <author>Arpita Soni</author>
        /// <createdDate>09Jun2016</createdDate>
        /// </summary>
        [TestMethod]
        public void LoadLineItemTabFromTacticPopup()
        {
            Console.WriteLine("To Load Line Item Grid in Tactic Inspect Popup.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController controller = new InspectController();
            Guid UserId = ((RevenuePlanner.BDSService.User)(System.Web.HttpContext.Current.Session["User"])).UserId;
            int tacticId = db.Plan_Campaign_Program_Tactic.Where(t => t.CreatedBy.Equals(UserId)).
                            Select(tac => tac.PlanTacticId).FirstOrDefault();

            var result = controller.LoadLineItemTabFromTacticPopup(tacticId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_TacticLineItemListing", result.ViewName);
          
        }
        #endregion

        #endregion

        #region Improvement Tactic Related Methods

        #region Save Improvement  Tactic
        /// <summary>
        /// To Save the Improvement Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_ImprovementTactic()
        {
            Console.WriteLine("To Save the Improvement Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int ImprovementPlanCampaignId = db.Plan_Improvement_Campaign.Where(id => id.ImprovePlanId == PlanID).Select(id => id.ImprovementPlanCampaignId).FirstOrDefault();
            int ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(id => id.ImprovementPlanCampaignId == ImprovementPlanCampaignId).Select(id => id.ImprovementPlanProgramId).FirstOrDefault();
            int ImprovementTactictypeid = db.ImprovementTacticTypes.Where(id => id.ClientId == Sessions.User.ClientId && id.IsDeleted == false).Select(id => id.ImprovementTacticTypeId).FirstOrDefault();


            InspectModel form = new InspectModel();
            form.ImprovementPlanTacticId = 0;
            form.ImprovementPlanProgramId = ImprovementPlanProgramId;
            form.ImprovementTacticTypeId = ImprovementTactictypeid;
            form.Title = "Test ImprovementTactic" + "_ " + DateTime.Now;
            form.EffectiveDate = DateTime.Now;
            // form.Owner = "sys Admin";

            var result = objInspectController.SaveImprovementTactic(form, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
           
        }
        #endregion

        #region Save Improvement Tactic Check duplicate
        /// <summary>
        /// To check the duplicate Improvement Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_ImprovementTactic_Check_Duplicate()
        {
            Console.WriteLine("To check the duplicate Improvement Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method


            InspectController objInspectController = new InspectController();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            int ImprovementPlanCampaignId = db.Plan_Improvement_Campaign.Where(id => id.ImprovePlanId == PlanID).Select(id => id.ImprovementPlanCampaignId).FirstOrDefault();
            int ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(id => id.ImprovementPlanCampaignId == ImprovementPlanCampaignId).Select(id => id.ImprovementPlanProgramId).FirstOrDefault();
            int ImprovementTactictypeid = db.ImprovementTacticTypes.Where(id => id.ClientId == Sessions.User.ClientId && id.IsDeleted == false).Select(id => id.ImprovementTacticTypeId).FirstOrDefault();

            string Title = db.Plan_Improvement_Campaign_Program_Tactic.Where(id => id.ImprovementPlanProgramId == ImprovementPlanProgramId).Select(tactic => tactic.Title).FirstOrDefault();
            InspectModel form = new InspectModel();
            form.ImprovementPlanTacticId = 0;
            form.ImprovementPlanProgramId = ImprovementPlanProgramId;
            form.ImprovementTacticTypeId = ImprovementTactictypeid;
            form.Title = Title;
            form.EffectiveDate = DateTime.Now;

            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objInspectController.SaveImprovementTactic(form, false) as JsonResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
           
        }
        #endregion

        #region Update Improvement Tactic
        /// <summary>
        /// To Update Improvement Tactic
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>25June2015</createddate>
        [TestMethod]
        public void Update_ImprovementTactic()
        {
            Console.WriteLine("To Update Improvement Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            InspectController objInspectController = new InspectController();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            int ImprovementPlanCampaignId = db.Plan_Improvement_Campaign.Where(id => id.ImprovePlanId == PlanID).Select(id => id.ImprovementPlanCampaignId).FirstOrDefault();
            int ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(id => id.ImprovementPlanCampaignId == ImprovementPlanCampaignId).Select(id => id.ImprovementPlanProgramId).FirstOrDefault();
            int ImprovementTactictypeid = db.ImprovementTacticTypes.Where(id => id.ClientId == Sessions.User.ClientId && id.IsDeleted == false).Select(id => id.ImprovementTacticTypeId).FirstOrDefault();

            string Title = "Update Improvement Tactic";
            InspectModel form = new InspectModel();
            form.ImprovementPlanTacticId = db.Plan_Improvement_Campaign_Program_Tactic.Where(id => id.ImprovementPlanProgramId == ImprovementPlanProgramId).Select(tactic => tactic.ImprovementPlanTacticId).FirstOrDefault();
            form.ImprovementPlanProgramId = ImprovementPlanProgramId;
            form.ImprovementTacticTypeId = ImprovementTactictypeid;
            form.Title = Title;
            form.EffectiveDate = DateTime.Now;


            var result = objInspectController.SaveImprovementTactic(form, false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion


        #region Save comment in Review Tab
        /// <summary>
        /// To Save  comment in Review Tab
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Comment_ImprovementTactic()
        {
            Console.WriteLine("To Save  comment in Review Tab.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int ImprovementPlanCampaignId = db.Plan_Improvement_Campaign.Where(id => id.ImprovePlanId == PlanID).Select(id => id.ImprovementPlanCampaignId).FirstOrDefault();
            int ImprovementPlanProgramId = db.Plan_Improvement_Campaign_Program.Where(id => id.ImprovementPlanCampaignId == ImprovementPlanCampaignId).Select(id => id.ImprovementPlanProgramId).FirstOrDefault();
            int PlanTacticId = db.Plan_Improvement_Campaign_Program_Tactic.Where(id => id.ImprovementPlanProgramId == ImprovementPlanProgramId).Select(tactic => tactic.ImprovementPlanTacticId).FirstOrDefault(); ;
            var result = Save_Comment(PlanTacticId, Enums.Section.ImprovementTactic.ToString().ToLower());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #endregion

        #region CommonFunctions
        public string GetBudgetValues(string AllocatedBY)
        {

            string Y1, Y2, Y3, Y4, Y5, Y6, Y7, Y8, Y9, Y10, Y11, Y12;
            string Values = "";

            if (AllocatedBY == "months")
            {
                Y1 = "100";
                Y2 = "200";
                Y3 = "300";
                Y4 = "100";
                Y5 = "200";
                Y6 = "300";
                Y7 = "200";
                Y8 = "200";
                Y9 = "300";
                Y10 = "100";
                Y11 = "200";
                Y12 = "300";

                Values = Y1 + ',' + Y2 + ',' + Y3 + ',' + Y4 + ',' + Y5 + ',' + Y6 + ',' + Y7 + ',' + Y8 + ',' + Y9 + ',' + Y10 + ',' + Y11 + ',' + Y12;
            }
            else
            {
                Y1 = "600";
                Y4 = "600";
                Y7 = "700";
                Y10 = "600";
                Values = Y1 + ',' + Y4 + ',' + Y7 + ',' + Y10;
            }

            return Values;

        }

        public JsonResult Save_Comment(int Id, string section)
        {
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var url = objInspectController.Request.Url;
            string Comment = "Test";
            var result = objInspectController.SaveComment(Comment, Id, section) as JsonResult;
            return result;

        }
        #endregion

        #region Media code related Methods
          #region Load Media Code
        /// <summary>
        /// To Load Media Code
        /// </summary>
        /// <auther>Devasnhi gandhi</auther>
        /// <createddate>12July2016</createddate>
        [TestMethod]
        public void LoadActiveMediaCode()
        {
            Console.WriteLine("To Load Media Code for Promotion Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            string UserID = (Sessions.User.UserId).ToString();
            Plan_Campaign_Program_Tactic objtactic = DataHelper.GetPlanTactic(Sessions.User.ClientId);
            int tacticId=objtactic.PlanTacticId;
            string mode = Enums.InspectPopupMode.Edit.ToString();
            var result = objInspectController.LoadMediaCodeFromTacticPopup(tacticId, mode) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("_TacticMediaCode", result.ViewName);
        }
          #endregion

        #region Generate Media Code
        /// <summary>
        /// To Load Media Code
        /// </summary>
        /// <auther>Devasnhi gandhi</auther>
        /// <createddate>12July2016</createddate>
        [TestMethod]
        public void GenerateMediaCode()
        {
            Console.WriteLine("To Generate Media Code for Promotion Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
           
            Plan_Campaign_Program_Tactic objtactic = DataHelper.GetPlanTactic(Sessions.User.ClientId);
            string tacticId = Convert.ToString(objtactic.PlanTacticId);
            var lstmediaCodeCustomfield = db.MediaCodes_CustomField_Configuration.Where(a => a.ClientId == Sessions.User.ClientId).ToList().Select(a => new TacticCustomfieldConfig
            {
                CustomFieldId = a.CustomFieldId,
                CustomFieldName = a.CustomField.Name,
                CustomFieldTypeName = a.CustomField.CustomFieldType.Name,
                IsRequired = a.CustomField.IsRequired,
                Sequence = a.Sequence,
                Option = a.CustomField.CustomFieldOptions.Select(opt => new CustomFieldOptionList
                {
                    CustomFieldOptionId = opt.CustomFieldOptionId,
                    CustomFieldOptionValue = opt.Value
                }).ToList()
            }).OrderBy(a => a.Sequence).FirstOrDefault();
            if (lstmediaCodeCustomfield !=null)
            {
                List<CustomFieldValue> lstcustomfieldvalue = new List<CustomFieldValue>();
                CustomFieldValue objcustomfield = new CustomFieldValue();
                objcustomfield.CustomFieldId = lstmediaCodeCustomfield.CustomFieldId;
                objcustomfield.CustomFieldName = lstmediaCodeCustomfield.CustomFieldName;
                objcustomfield.CustomFieldType = lstmediaCodeCustomfield.CustomFieldTypeName;
                if(lstmediaCodeCustomfield.CustomFieldTypeName==Convert.ToString(Enums.CustomFieldType.TextBox))
                    objcustomfield.CustomFieldOptionValue = "test1234";
                else
                {
                    var optvalue = lstmediaCodeCustomfield.Option.Select(a => a.CustomFieldOptionId).FirstOrDefault();
                    objcustomfield.CustomFieldOptionValue = optvalue.ToString();
                }
                lstcustomfieldvalue.Add(objcustomfield);
                var result = objInspectController.GenerateMediaCode(tacticId, lstcustomfieldvalue) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
                Assert.IsNotNull(result.Data);
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["Success"];
                Assert.AreEqual("true", resultvalue.ToString(), true);
            }
        }
        #endregion

        #region Archive/Unarchvie MediaCode
        /// <summary>
        /// method to archive/unarchive media code
        ///  <auther>Devanshi gandhi</auther>
        /// <createddate>22July2016</createddate>
        /// </summary>
        [TestMethod]
        public void ArchiveUnarchvieMediacode()
        {
            Console.WriteLine("To archive/unarchive Media code for Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var listmediacode = db.vClientWise_Tactic.Where(a => a.ClientId == Sessions.User.ClientId).ToList();
            if(listmediacode!=null)
            {
                int tacticId = listmediacode.Select(a => a.TacticId).FirstOrDefault();
                int mediacodeId = listmediacode.Where(a => a.TacticId == tacticId).Select(a => a.MediaCodeId).FirstOrDefault();
                string mode = Convert.ToString(Enums.InspectPopupMode.Edit);
                if (mediacodeId != 0)
                {
                    string mediacodestr = Convert.ToString(mediacodeId);
                    var result = objInspectController.ArchiveMediaCode(tacticId, mode, 0, mediacodestr) as PartialViewResult;
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
                    Assert.IsNotNull(result.Model);
                    Assert.AreEqual("_ArchiveMediaCode", result.ViewName);
                }
            }
           
        }
        #endregion

        #region Archive/Unarchvie MediaCode
        /// <summary>
        /// method to archive/unarchive media code
        ///  <auther>Devanshi gandhi</auther>
        /// <createddate>22July2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetArchiveMediacode()
        {
            Console.WriteLine("To Get list of archived Media code for Tactic.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            var listmediacode = db.vClientWise_Tactic.Where(a => a.ClientId == Sessions.User.ClientId).ToList();
            if (listmediacode != null)
            {
                int tacticId = listmediacode.Select(a => a.TacticId).FirstOrDefault();
               
                string mode = Convert.ToString(Enums.InspectPopupMode.Edit);
               
                    
                var result = objInspectController.ArchiveMediaCode(tacticId, mode, 0, string.Empty) as PartialViewResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
                Assert.IsNotNull(result.Model);
                Assert.AreEqual("_ArchiveMediaCode", result.ViewName);
                
                }
            
        }
        #endregion
        #endregion

        #region Load Review Tactic
        [TestMethod]
        public void LoadReview()
        {
            Console.WriteLine("To load review tab of tactic.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            int planId = DataHelper.GetPlanId();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == planId && c.IsDeleted==false).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId && id.IsDeleted==false).Select(program => program.PlanProgramId).FirstOrDefault();
            
            int PlanTacticId = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId && id.IsDeleted==false).Select(tactic => tactic.PlanTacticId).FirstOrDefault();
            var result = objInspectController.LoadReview(PlanTacticId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("Review", result.ViewName);
        }

        [TestMethod]
        public void SaveTacticTitle()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To save tactic title.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int planId = DataHelper.GetPlanId();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == planId && c.IsDeleted == false).Select(c => c.PlanCampaignId).FirstOrDefault();
            int PlanProgramId = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId && id.IsDeleted == false).Select(program => program.PlanProgramId).FirstOrDefault();

            var PlanTactic = db.Plan_Campaign_Program_Tactic.Where(id => id.PlanProgramId == PlanProgramId && id.IsDeleted == false).FirstOrDefault();
            string tId=PlanTactic.PlanTacticId.ToString();
            string newTitle=PlanTactic.Title+"New";
            var result = objInspectController.SaveTitle("Tactic", newTitle, tId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void SaveCampaignTitle()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To save campaign title.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int planId = DataHelper.GetPlanId();
            var PlanCampaign = db.Plan_Campaign.Where(c => c.PlanId == planId && c.IsDeleted == false).FirstOrDefault();
            string cId = PlanCampaign.PlanCampaignId.ToString();
            string newTitle = PlanCampaign.Title + "New";
            var result = objInspectController.SaveTitle("Campaign", newTitle, cId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void SaveProgramTitle()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To save program title.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int planId = DataHelper.GetPlanId();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == planId && c.IsDeleted == false).Select(c => c.PlanCampaignId).FirstOrDefault();
            var PlanProgram = db.Plan_Campaign_Program.Where(id => id.PlanCampaignId == PlanCampaignId && id.IsDeleted == false).FirstOrDefault();

            string pId = PlanProgram.PlanProgramId.ToString();
            string newTitle = PlanProgram.Title + "New";
            var result = objInspectController.SaveTitle("Program", newTitle, pId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void SaveLineItemTitle()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To save lineitem title.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var LineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(l=> l.IsDeleted == false && l.LineItemType!=null).FirstOrDefault();
            string lId = LineItem.PlanLineItemId.ToString();
            string newTitle = LineItem.Title + "New";
            var result = objInspectController.SaveTitle("LineItem", newTitle, lId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result);
        }
        #endregion

        #region CreateProgram view
        [TestMethod]
        public void CreateProgram()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To open program view.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int campaignId = db.Plan_Campaign.Where(a => a.IsDeleted == false).Select(a=>a.PlanCampaignId).FirstOrDefault();
            var result = objInspectController.CreateProgram(campaignId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_EditSetupProgram", result.ViewName);
        }
        #endregion

        #region LoadEditSetupLineitem
        [TestMethod]
        public void LoadEditSetupLineitem()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To open setup line item.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int lineItemId = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted == false).Select(a => a.PlanLineItemId).FirstOrDefault();
            var result = objInspectController.LoadEditSetupLineitem(lineItemId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.AreEqual("_EditSetupLineitem", result.ViewName);
        }

        [TestMethod]
        public void LoadSetupLineitem()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To open setup line item.\n");
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            InspectController objInspectController = new InspectController();
            objInspectController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objInspectController);
            objInspectController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int lineitemid = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted == false).Select(a => a.PlanLineItemId).FirstOrDefault();
            var result = objInspectController.LoadSetupLineitem(lineitemid) as PartialViewResult;
            Assert.AreEqual("_SetupLineitem", result.ViewName);
        }
        #endregion

    }
}
