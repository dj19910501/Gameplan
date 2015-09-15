using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;

namespace RevenuePlanner.Controllers
{
    public class FinanceController : Controller
    {
        //
        // GET: /Finance/

        public ActionResult Index()
        {
            var lstbudget = Common.GetBudgetlist();
            ViewBag.budgetlist = lstbudget;

            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;
            return View();
        }

    }
}
