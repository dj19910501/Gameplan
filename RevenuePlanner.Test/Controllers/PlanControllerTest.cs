using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
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
        [TestMethod]
        public void Save_Plan_With_Zero_Goal_Value()
        {
            //Set the PlanModel with data
            PlanModel plan = new PlanModel();
            string BudgetInputValues = string.Empty;
            string RedirectType = string.Empty;
            string UserId = string.Empty;
            plan.Title = "test plan title1";
            plan.GoalType = "mql";
            plan.GoalValue = "0";
            plan.AllocatedBy = "months";
            plan.Budget = 120000;
            plan.ModelId = DataHelper.GetModelId();
            plan.Year = DateTime.Now.Year.ToString();
            PlanController controller = new PlanController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            var result = controller.SavePlan(plan) as JsonResult;
            int planId = result.GetValue<int>("id");
            Assert.IsTrue(Convert.ToBoolean(planId));

        }

               
    }
}

//User user = new User();
//user.UserId = Guid.NewGuid();

////This will create the dummy session for Unit test project
//HttpContext.Current = MockHelpers.FakeHttpContext();
//HttpContext.Current.Session["User"] = user;