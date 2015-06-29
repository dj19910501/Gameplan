using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class InspectControllerTest
    {
        #region Save Plan
        /// <summary>
        /// To Save the Plan
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Plan()
        {
             MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
          
            InspectController objInspectController = new InspectController();
            
             InspectModel objPlanModel= new InspectModel();
             objPlanModel.GoalType = Enums.PlanGoalType.MQL.ToString();
             objPlanModel.GoalValue = "0";
             objPlanModel.Title = "test plan #975";
             objPlanModel.ModelId = DataHelper.GetModelId();
             objPlanModel.StartDate = DateTime.Now;
             objPlanModel.EndDate = DateTime.MaxValue;
             objPlanModel.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
             objPlanModel.PlanId = db.Plans.Where(plan => plan.Title == objPlanModel.Title && plan.ModelId == objPlanModel.ModelId).Select(plan => plan.PlanId).FirstOrDefault();

             string UserID = (Sessions.User.UserId).ToString();
             string planBudget = "50000";

             string Budgetvalues = GetBudgetValues(objPlanModel.AllocatedBy);

             var result = objInspectController.SavePlanDetails(objPlanModel, Budgetvalues , planBudget, "", UserID) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
               
            }
        }


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

            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            Random random = new Random();
            int Number = random.Next();
            string Title = "Test Campaign" + Number;
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = 0;
            Form.Owner = "sys Admin";
            Form.Title = "Test Campaign" + Number;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;

            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
            }
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
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault(); 
            Sessions.PlanId = PlanID;
            string Title = db.Plan_Campaign.Where(id=>id.PlanId == PlanID).Select(campaign => campaign.Title).FirstOrDefault();
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = 0;
            Form.Owner = "sys Admin";
            Form.Title = Title;
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;

            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
                //Assert.IsNotNull(result.GetValue("taskData"));
            }
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
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            InspectController objInspectController = new InspectController();
            string UserID = (Sessions.User.UserId).ToString();
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            Sessions.PlanId = PlanID;
            string Title = "Update Campaign";
            Plan_CampaignModel Form = new Plan_CampaignModel();
            Form.PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            Form.Owner = "sys Admin";
            Form.Title = "Update Campaign";
            Form.StartDate = DateTime.Now;
            Form.EndDate = DateTime.MaxValue;
            Form.PlanId = PlanID;

            var result = objInspectController.SaveCampaign(Form, Title, "[]", UserID, PlanID) as JsonResult;

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
            }
        }
        #endregion

        #region Save Campaign Budget Allocation
           /// <summary>
        /// To Save the Plan
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>29June2015</createddate>
        [TestMethod]
        public void Save_Campaign_Budget_Allocation()
        {

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
            Form.PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            Form.AllocatedBy = Enums.PlanAllocatedBy.months.ToString();
            Form.CampaignBudget = 10000;
            Form.PlanId = PlanID;

            string Budgetvalues = GetBudgetValues(Form.AllocatedBy);

            var result = objInspectController.SaveCampaignBudgetAllocation(Form, Budgetvalues, UserID, Title) as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);
            }
        }

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
            MRPEntities db = new MRPEntities();
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            //// Call index method
            int PlanID = db.Plans.Where(plan => plan.Title == "test plan #975").Select(plan => plan.PlanId).FirstOrDefault();
            int PlanCampaignId = db.Plan_Campaign.Where(c => c.PlanId == PlanID).Select(c => c.PlanCampaignId).FirstOrDefault();
            var result = Save_Comment(PlanCampaignId, Enums.Section.Campaign.ToString());

            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result.Data);

            }
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
            string Comment = "Test";
            var result = objInspectController.SaveComment(Comment,Id,section) as JsonResult;
            return result;

        }
        #endregion

    }
}
