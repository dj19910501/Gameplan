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

            HttpContext.Current = DataHelper.SetUserAndPermission();
            PlanController controller = new PlanController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            Sessions.PlanId = DataHelper.GetPlanId();
            var result = controller.PublishPlan(Sessions.User.UserId.ToString()) as JsonResult;
              if(result != null)
              { 
               // data object should not be null in json result
               Assert.IsNotNull(result.Data);
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

             HttpContext.Current = DataHelper.SetUserAndPermission();
             PlanController controller = new PlanController();
             controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
             Sessions.PlanId = DataHelper.GetPlanId();
             var result = controller.Budgeting(Sessions.PlanId) as JsonResult;
             if (result != null)
             {
                 // data object should not be null in json result
                 Assert.IsNotNull(result.Data);
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
             }

         }


        #endregion

        #region Grid View

         [TestMethod]
         public void LoadGrid()
         {
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

             var result = controller.LoadHomeGrid(PlanId.ToString(), Ownerids, tactictypeids, Status, CommaSeparatedCustomFields) as JsonResult;
             if (result != null)
             {
                 // data object should not be null in json result
                 Assert.IsNotNull(result.Data);
             }

         }

         /// To Get Improvement Tactic for the grid
         /// <author>Komal Rawal</author>
         /// <createdDate>11thAugust2015</createdDate>
         /// </summary>
         [TestMethod]
         public void GetImprovementTactic()
         {
             HttpContext.Current = DataHelper.SetUserAndPermission();
             PlanController controller = new PlanController();
             controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();
             int PlanId = DataHelper.GetPlanId();
             var result = controller.GetImprovementTactic(PlanId) as JsonResult;
             if (result != null)
             {
                 // data object should not be null in json result
                 Assert.IsNotNull(result.Data);
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