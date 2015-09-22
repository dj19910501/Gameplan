using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System.Text;


namespace RevenuePlanner.Controllers
{
    public class FinanceController : Controller
    {
        //
        // GET: /Finance/

        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Finance)
        {
            finacemodel financeObj = new finacemodel();
            StringBuilder GridString = new StringBuilder();
            var lstbudget = Common.GetBudgetlist();
            ViewBag.budgetlist = lstbudget;
            ViewBag.ActiveMenu = activeMenu;
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;
            financeObj.FinanemodelheaderObj = Common.GetFinanceHeaderValue();
            //GridString = GenerateFinaceXMHeader(GridString);

            financeObj.xmlstring = GridString.ToString();
            return View(financeObj);
        }


        public JsonResult GetFinanceHeaderValue(int budgetId = 2, string timeFrameOption = "", string isQuarterly = "Quarterly")
        {
            FinanceModelHeaders objfinanceheader = Common.GetFinanceHeaderValue(budgetId, timeFrameOption, isQuarterly);
            return Json(objfinanceheader, JsonRequestBehavior.AllowGet);
        }

    }
}
