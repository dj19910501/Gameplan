using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class CurrencyController : Controller
    {
        //
        // GET: /Currency/
        
        /// <summary>
        /// Add By Nishant Sheth
        /// To Get Plan Exchange rate and it's currency rate
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPlanCurrencyDetail()
        {
            try
            {
                var ExchangeRate = Sessions.PlanExchangeRate;
                var CurrencySymbol = Sessions.PlanCurrencySymbol;
                var jsonresult = Json(new
                {
                    ExchangeRate,
                    CurrencySymbol
                }, JsonRequestBehavior.AllowGet);
                return jsonresult;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

    }
}
