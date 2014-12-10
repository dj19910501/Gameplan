using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class PlanControllerTest
    {
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

    }
}

//User user = new User();
//user.UserId = Guid.NewGuid();

////This will create the dummy session for Unit test project
//HttpContext.Current = MockHelpers.FakeHttpContext();
//HttpContext.Current.Session["User"] = user;