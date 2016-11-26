using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services.MarketingBudget
{
    public class MarketingBudget : IMarketingBudget
    {
        private MRPEntities _database;
        public MarketingBudget(MRPEntities database)
        {
            _database = database;
        }

        public List<BindDropdownData> GetBudgetlist(int ClientId)
        {
            //get budget name list for budget drop-down data binding.
            List<BindDropdownData> lstBudget = new List<BindDropdownData>();
            List<Models.Budget> customfieldlist = _database.Budgets.Where(bdgt => bdgt.ClientId == ClientId && (bdgt.IsDeleted == false || bdgt.IsDeleted == null) && !string.IsNullOrEmpty(bdgt.Name)).ToList();
            lstBudget = customfieldlist.Select(budget => new BindDropdownData { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return lstBudget;
        }


        public int GetOtherBudgetId(int ClientId)
        {
            // To get first OTHER budget Id for client.             
            int BudgetId = (from ParentBudget in _database.Budgets
                        join
                            ChildBudget in _database.Budget_Detail on ParentBudget.Id equals ChildBudget.BudgetId
                        where ParentBudget.ClientId == ClientId
                        && (ParentBudget.IsDeleted == false || ParentBudget.IsDeleted == null)
                        && ParentBudget.IsOther == true
                        select ChildBudget.Id
                        ).FirstOrDefault();
            return BudgetId;
        }
        public List<LineItemAllocatingAccount> GetAccountsForLineItem(int lineItemId)
        {
            throw new NotImplementedException();
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

        public void DeleteBudgetData(int SelectedRowIDs, int ClientId)
        {
            #region Delete Fields            
            if (SelectedRowIDs != 0)
            {               
                // To get Selected budget Data. 
                List<BudgetDetailforDeletion> SelectedBudgetDetail = (from details in _database.Budget_Detail
                                            where (details.ParentId == SelectedRowIDs || details.Id == SelectedRowIDs) && details.IsDeleted == false
                                            select new BudgetDetailforDeletion
                                            {
                                                Id = details.Id,
                                                BudgetId = details.BudgetId,
                                                ParentId = details.ParentId,
                                                IsDeleted = details.IsDeleted
                                            }).ToList();

                // To get Selected budget Data with its 'N' level heirarchy.
                List<BudgetDetailforDeletion> BudgetDetailJoin = (from details in _database.Budget_Detail
                                        join selectdetails in
                                            (from details in _database.Budget_Detail
                                             where (details.ParentId == SelectedRowIDs || details.Id == SelectedRowIDs) && details.IsDeleted == false
                                             select new BudgetDetailforDeletion
                                             {
                                                 Id = details.Id,
                                                 BudgetId = details.BudgetId,
                                                 ParentId = details.ParentId,                                                 
                                                 IsDeleted = details.IsDeleted
                                             }) on details.ParentId equals selectdetails.Id
                                        select new BudgetDetailforDeletion
                                        {
                                            Id = details.Id,
                                            BudgetId = details.BudgetId,
                                            ParentId = details.ParentId,                                            
                                            IsDeleted = details.IsDeleted
                                        }).ToList();

                List<BudgetDetailforDeletion> BudgetDetailData = SelectedBudgetDetail.Union(BudgetDetailJoin).ToList();
                List<BudgetDetailforDeletion> BudgetDetailData1 = new List<BudgetDetailforDeletion>();
                BudgetDetailData1 = BudgetDetailData.Distinct().ToList();               
                if (BudgetDetailData.Count > 0)
                {
                    BudgetDetailforDeletion ParentBudgetData = BudgetDetailData.Where(a => a.ParentId == null).Select(a => a).FirstOrDefault();
                    List<int> BudgetDetailIds = BudgetDetailData.Select(a => a.Id).ToList();
                    int OtherBudgetId = GetOtherBudgetId(ClientId);

                    if (ParentBudgetData != null)
                    {
                        // Delete Budget From Budget Table
                        RevenuePlanner.Models.Budget objBudget = _database.Budgets.Where(a => a.Id == ParentBudgetData.BudgetId && a.IsDeleted == false).FirstOrDefault();
                        if (objBudget != null)
                        {
                            objBudget.IsDeleted = true;
                            _database.Entry(objBudget).State = EntityState.Modified;
                        }
                    }

                    // Update Line Item with Other Budget Id
                    List<LineItem_Budget> LineItemBudgetList = _database.LineItem_Budget.Where(a => BudgetDetailIds.Contains(a.BudgetDetailId)).ToList();
                    foreach (var LineitemBudget in LineItemBudgetList)
                    {
                        LineitemBudget.BudgetDetailId = OtherBudgetId;
                        _database.Entry(LineitemBudget).State = EntityState.Modified;
                    }

                    // Delete Budget Id from Budget_Detail Table
                    List<RevenuePlanner.Models.Budget_Detail> BudgetDetailList = _database.Budget_Detail.Where(a => BudgetDetailIds.Contains(a.Id)).ToList();
                    foreach (var BudgetDetail in BudgetDetailList)
                    {
                        BudgetDetail.IsDeleted = true;
                        _database.Entry(BudgetDetail).State = EntityState.Modified;
                    }
                    _database.SaveChanges();
                }
            }
            #endregion  
        }
    }
}