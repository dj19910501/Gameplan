using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RevenuePlanner.Services.MarketingBudget;
using StructureMap;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using System.Linq;

namespace RevenuePlanner.Controllers
{
    public class MarketingBudgetController : CommonController
    {
        IMarketingBudget _MarketingBudget;
        public MarketingBudgetController(IMarketingBudget MarketingBudget)
        {
            _MarketingBudget = MarketingBudget;
        }
        #region Declartion
        private bool _IsBudgetCreate_Edit = true;
        private bool _IsForecastCreate_Edit = true;
        #endregion
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Finance)
        {
            MarketingActivities MarketingActivities = new MarketingActivities();

            #region Check Permissions
            bool IsBudgetCreateEdit, IsBudgetView, IsForecastCreateEdit, IsForecastView;
            IsBudgetCreateEdit = _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            IsForecastCreateEdit = _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);
            if (IsBudgetCreateEdit == false && IsBudgetView == false && IsForecastCreateEdit == false && IsForecastView == false)
            {

                return RedirectToAction("Index", "NoAccess");
            }
            #endregion

            #region Bind Budget dropdown on grid
            List<BindDropdownData> lstMainBudget = _MarketingBudget.GetBudgetlist(Sessions.User.CID);// Budget dropdown
            MarketingActivities.ListofBudgets = lstMainBudget;
            #endregion

            #region "Bind TimeFrame Dropdown"
            List<BindDropdownData> lstMainAllocated = new List<BindDropdownData>();
            lstMainAllocated = Enums.QuartersFinance.Select(timeframe => new BindDropdownData { Text = timeframe.Key, Value = timeframe.Value }).ToList();
            MarketingActivities.TimeFrame = lstMainAllocated;
            #endregion

            #region Bind Column set dropdown
            var ColumnSet = _MarketingBudget.GetColumnSet(Sessions.User.CID);// Column set  dropdown
            MarketingActivities.Columnset = ColumnSet;
            #endregion

            return View(MarketingActivities);
        }

        public JsonResult RefreshBudgetList()
        {

            #region Bind Budget dropdown on grid
            List<BindDropdownData> budgetList = _MarketingBudget.GetBudgetlist(Sessions.User.CID);// Budget dropdown
            return Json(budgetList, JsonRequestBehavior.AllowGet);
            #endregion

        }

        public List<LineItemAllocatingAccount> GetAccountsForLineItem(int lineItemId)
        {
            //Do whatever needed here
            return ObjectFactory.GetInstance<IMarketingBudget>().GetAccountsForLineItem(lineItemId);
        }

        public List<PlanAllocatingAccount> GetAccountsForPlan(int planId)
        {
            throw new NotImplementedException();
        }

        public List<AllocatedLineItemForAccount> GetAllocatedLineItemsForAccount(int accountId)
        {
            throw new NotImplementedException();
        }

        //public List<BudgetItem> GetBudgetData(int budgetId, ViewByType viewByType, BudgetColumnFlag columnsRequested)
        //{
        //    throw new NotImplementedException();
        //}

        public JsonResult GetBudgetData(int budgetId, string viewByType, BudgetColumnFlag columnsRequested = 0) // need to pass columns requested
        {
            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            //Get all budget grid data.
            objBudgetGridModel = _MarketingBudget.GetBudgetGridData(budgetId, viewByType, columnsRequested, Sessions.User.CID, Sessions.User.ID, Sessions.PlanExchangeRate, Sessions.PlanCurrencySymbol);
            var jsonResult = Json(new { GridData = objBudgetGridModel.objGridDataModel, AttacheHeader = objBudgetGridModel.attachedHeader }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }


        public JsonResult GetColumns(int ColumnSetId = 0)
        {
            List<BindDropdownData> lstColumns = _MarketingBudget.GetColumns(ColumnSetId);// Columns  dropdown
            return Json(lstColumns, JsonRequestBehavior.AllowGet);
        }

        public BudgetSummary GetBudgetSummary(int budgetId)
        {
            throw new NotImplementedException();
        }

        public List<UserBudgetPermission> GetUserPermissionsForAccount(int accountId)
        {
            throw new NotImplementedException();
        }

        public void LinkLineItemsToAccounts(List<LineItemAccountAssociation> lineItemAccountAssociations)
        {
            throw new NotImplementedException();
        }

        public void LinkPlansToAccounts(List<PlanAccountAssociation> planAccountAssociations)
        {
            throw new NotImplementedException();
        }

        public Dictionary<BudgetCloumn, double> UpdateBudgetCell(int budgetId, BudgetCloumn columnIndex, double oldValue, double newValue)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Function to deleting budget data and its child heirarchy.
        /// Added By: Rahul Shah on 11/30/2016.
        /// </summary>
        /// <param name="SelectedBudgetId">Budget Detail Id.</param>
        /// <param name="CurrentBudgetId">Budget Id.</param>        
        /// <returns>Return Budget Id.</returns>
        public JsonResult DeleteBudgetData(string SelectedBudgetId, string BudgetId)
        {
            #region Delete Budget   

            if (SelectedBudgetId != null && SelectedBudgetId != "")
            {
                int _budgetId = 0, _currentBudgetId = 0;
                int ClientId = Sessions.User.CID; //Assign ClientId from Session
                int NextBudgetId = 0;

                int Selectedid = !string.IsNullOrEmpty(SelectedBudgetId) ? Int32.Parse(SelectedBudgetId) : 0;

                NextBudgetId = _MarketingBudget.DeleteBudget(Selectedid, ClientId); // call DeleteBudget function to delete selected data.

                _currentBudgetId = !string.IsNullOrEmpty(BudgetId) ? Int32.Parse(BudgetId) : 0;

                // assign next budget if current root budget is deleted to display budget other than the one deleted
                if (NextBudgetId > 0)
                {
                    _budgetId = NextBudgetId;
                }
                else
                {
                    _budgetId = _currentBudgetId;
                }
                return Json(new { IsSuccess = true, budgetId = _budgetId }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            return Json(new { IsSuccess = false, budgetId = BudgetId }, JsonRequestBehavior.AllowGet);

            //End
        }

    }
}