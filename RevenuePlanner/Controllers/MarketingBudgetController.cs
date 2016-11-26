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
        public MarketingBudgetController(IMarketingBudget MarketingBudget) {
            _MarketingBudget = MarketingBudget;
        }

        public ActionResult Index()
        {
            return View();
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

        public List<BudgetItem> GetBudgetData(int budgetId, ViewByType viewByType, BudgetColumnFlag columnsRequested)
        {
            throw new NotImplementedException();
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

        public ActionResult DeleteBudgetData(string SelectedRowIDs, string mainTimeFrame, string currentBudgetId, string ListofCheckedColums = "")
        {   
                     
            int ClientId = Sessions.User.CID;  //Assign ClientId from Session
            #region Delete Budget Fields  
             if (SelectedRowIDs != null && SelectedRowIDs != "")
            {
                List<DeleteRowID> Values = JsonConvert.DeserializeObject<List<DeleteRowID>>(SelectedRowIDs); // Deserialize Data 
                int Selectedids = Values.Select(ids => int.Parse(ids.Id.ToString())).FirstOrDefault();
                _MarketingBudget.DeleteBudgetData(Selectedids, ClientId); // call DeleteBudgetData function to delete selected data.
            }            
            List<BindDropdownData> lstchildbudget = _MarketingBudget.GetBudgetlist(ClientId); // get Child budget list.
            int _budgetId = 0, _currentBudgetId = 0;
            if (lstchildbudget != null)
            {
                _currentBudgetId = !string.IsNullOrEmpty(currentBudgetId) ? Int32.Parse(currentBudgetId) : 0;
                //check if any child budet is exist or not if not then assign first budget's of client to display budget grid.
                if (lstchildbudget.Any(budgt => budgt.Value == _currentBudgetId.ToString()))
                    _budgetId = _currentBudgetId;
                else
                {
                    string strbudgetId = lstchildbudget.Select(bdgt => bdgt.Value).FirstOrDefault();
                    _budgetId = !string.IsNullOrEmpty(strbudgetId) ? Int32.Parse(strbudgetId) : 0;
                }
            }
            #endregion
            return PartialView("_MainGrid"); //TODO : here we need to call bind finance grid function to load updated finance grid.
            
            //End
        }
      
    }
}