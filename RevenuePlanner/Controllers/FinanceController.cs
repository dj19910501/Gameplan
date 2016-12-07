using System.Web.Mvc;
using RevenuePlanner.Helpers;

namespace RevenuePlanner.Controllers
{
    public class FinanceController : CommonController
    {
        #region "Transactions"
        [AuthorizeUser(Enums.ApplicationActivity.TransactionAttribution)]
        public ActionResult Transactions(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Finance)
        {
            return View();
        }
        #endregion
    }
}
