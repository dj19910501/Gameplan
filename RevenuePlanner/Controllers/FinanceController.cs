﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using RevenuePlanner.Helpers;
using Newtonsoft.Json;
using RevenuePlanner.BDSService;
using System.Globalization;
using Elmah;

namespace RevenuePlanner.Controllers
{
    public class FinanceController : CommonController
    {
        //
        // GET: /Finance/
        #region Declartion
        private MRPEntities db = new MRPEntities();
        private const string PeriodPrefix = "Y";
        private const string formatThousand = "#,#0.##";
        private bool _IsBudgetCreate_Edit = true;
        private bool _IsForecastCreate_Edit = true;
        List<User> lstUserDetails = new List<User>();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        #endregion

        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Finance)
        {
            //Added by Rahul Shah on 02/10/2015 for PL #1650
            bool IsBudgetCreateEdit, IsBudgetView, IsForecastCreateEdit, IsForecastView;
            IsBudgetCreateEdit = _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            IsForecastCreateEdit = _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);
            //// if BestInClass editing rights then redirect to "BestInClass" else "NoAccess" action of Index View.
            if (IsBudgetCreateEdit == false && IsBudgetView == false && IsForecastCreateEdit == false && IsForecastView == false)
            {

                return RedirectToAction("Index", "NoAccess");
            }
            FinanceModel financeObj = new FinanceModel();
            //FinanceModelHeaders financeObj = new FinanceModelHeaders();
            StringBuilder GridString = new StringBuilder();
            var lstparnetbudget = Common.GetParentBudgetlist();
            var lstchildbudget = Common.GetChildBudgetlist(0);
            var lstMainBudget = Common.GetBudgetlist();// main budget drp
            ViewBag.budgetlist = lstMainBudget;
            ViewBag.parentbudgetlist = lstparnetbudget;
            ViewBag.childbudgetlist = lstchildbudget;
            ViewBag.ActiveMenu = activeMenu;
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;

            #region "Bind MainGrid TimeFrame Dropdown"
            List<ViewByModel> lstMainAllocated = new List<ViewByModel>();
            lstMainAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstMainAllocated = Enums.QuartersFinance.Select(timeframe => new ViewByModel { Text = timeframe.Key, Value = timeframe.Value }).ToList();
            ViewBag.ViewByMainGridAllocated = lstMainAllocated;
            #endregion

            #region "Bind Column Set"
            ViewBag.ColumnSet = GetColumnSet();
            #endregion
            //financeObj.FinanemodelheaderObj = Common.GetFinanceHeaderValue();
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            string strbudgetId = lstMainBudget != null && lstMainBudget.Count > 0 ? lstMainBudget.Select(budgt => budgt.Value).FirstOrDefault() : "0";
            //gridRowModel = GetFinanceMainGridData(int.Parse(strbudgetId));
            financeObj.FinanemodelheaderObj = gridRowModel.FinanemodelheaderObj;
            financeObj.DhtmlXGridRowModelObj = gridRowModel;
            //GridString = GenerateFinaceXMHeader(GridString);

            return View(financeObj);
        }

        #region "Create new Budget related methods"
        public ActionResult LoadnewBudget()
        {
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            objFinanceHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            objFinanceHeader.Budget = 0;
            objFinanceHeader.Forecast = 0;
            objFinanceHeader.Planned = 0;
            objFinanceHeader.Actual = 0;
            TempData["FinanceHeader"] = objFinanceHeader;

            return PartialView("_newBudget");
        }
        public ActionResult CreateNewBudget(string budgetName, string ListofCheckedColums = "")
        {
            try
            {

                int budgetId = SaveNewBudget(budgetName);
                return RefreshMainGridData(budgetId, "Yearly", ListofCheckedColums);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int SaveNewBudget(string budgetName)
        {
            int budgetId = 0;
            try
            {
                Budget objBudget = new Budget();
                objBudget.ClientId = Sessions.User.ClientId;
                objBudget.Name = budgetName;
                objBudget.Desc = string.Empty;
                objBudget.CreatedBy = Sessions.User.UserId;
                objBudget.CreatedDate = DateTime.Now;
                objBudget.IsDeleted = false;
                db.Entry(objBudget).State = EntityState.Added;
                db.SaveChanges();
                budgetId = objBudget.Id;

                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail.BudgetId = budgetId;
                objBudgetDetail.Name = budgetName;
                objBudgetDetail.IsDeleted = false;
                objBudgetDetail.CreatedBy = Sessions.User.UserId;
                objBudgetDetail.CreatedDate = DateTime.Now;
                db.Entry(objBudgetDetail).State = EntityState.Added;
                db.SaveChanges();
                int _budgetid = objBudgetDetail.Id;
                SaveUserBudgetpermission(_budgetid);
            }
            catch (Exception)
            {
                throw;
            }
            return budgetId;
        }
        public JsonResult RefreshBudgetList()
        {
            var budgetList = Common.GetBudgetlist();
            return Json(budgetList, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region  Main Grid related Methods

        #region "Delete MainGrid"
        public ActionResult DeleteMainGrid(string SelectedRowIDs, string mainTimeFrame, string curntBudgetId, string ListofCheckedColums = "")
        {
            ViewBag.isDelete = true;
            //Added By Komal Rawal for #1639
            #region Delete Fields
            if (SelectedRowIDs != null)
            {

                var Values = JsonConvert.DeserializeObject<List<DeleteRowID>>(SelectedRowIDs);
                var Selectedids = Values.Select(ids => int.Parse(ids.Id.ToString())).FirstOrDefault();

                #region Old Code
                //List<Budget_Detail> BudgetDetail = db.Budget_Detail.Where(budgetdetail => Selectedids.Contains(budgetdetail.Id) && budgetdetail.IsDeleted == false).Select(a => a).ToList();

                //List<Budget_Detail> BudgetDetailList = db.Budget_Detail.Where(budgetdetail => budgetdetail.IsDeleted == false).Select(a => a).ToList();

                //foreach (var item in BudgetDetail)
                //{
                //    var ParentID = item.ParentId;
                //    if (ParentID == null)
                //    {
                //        var Budget = db.Budgets.Where(a => a.Id == item.BudgetId).Select(a => a).ToList();
                //        foreach (var value in Budget)
                //        {
                //            value.IsDeleted = true;
                //            db.Entry(value).State = EntityState.Modified;
                //            db.SaveChanges();
                //        }

                //    }

                //    var ChildItems = BudgetDetailList.Where(child => child.ParentId == item.Id).Select(child => child).ToList();
                //    foreach (var child in ChildItems)
                //    {
                //        child.IsDeleted = true;
                //        db.Entry(child).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }

                //    item.IsDeleted = true;
                //    db.Entry(item).State = EntityState.Modified;
                //}
                //db.SaveChanges();
                #endregion

                var SelectedBudgetDetail = (from details in db.Budget_Detail
                                            where (details.ParentId == Selectedids || details.Id == Selectedids) && details.IsDeleted == false || details.IsDeleted == null
                                            select new
                                            {
                                                details.Id,
                                                details.BudgetId,
                                                details.ParentId,
                                                details.Name,
                                                details.IsDeleted
                                            }).ToList();

                var BudgetDetailJoin = (from details in db.Budget_Detail
                                        join selectdetails in
                                            (from details in db.Budget_Detail
                                             where (details.ParentId == Selectedids || details.Id == Selectedids) && details.IsDeleted == false || details.IsDeleted == null
                                             select new
                                             {
                                                 details.Id,
                                                 details.BudgetId,
                                                 details.ParentId,
                                                 details.Name,
                                                 details.IsDeleted
                                             }) on details.ParentId equals selectdetails.Id
                                        select new
                                        {
                                            details.Id,
                                            details.BudgetId,
                                            details.ParentId,
                                            details.Name,
                                            details.IsDeleted
                                        }).ToList();

                var BudgetDetailData = SelectedBudgetDetail.Union(BudgetDetailJoin).ToList();
                if (BudgetDetailData.Count > 0)
                {
                    var ParentId = BudgetDetailData.Where(a => a.ParentId == null).Select(a => a).FirstOrDefault();
                    var BudgetDetailIds = BudgetDetailData.Select(a => a.Id).ToList();
                    int OtherBudgetId = Common.GetOtherBudgetId();

                    if (ParentId != null)
                    {
                        // Delete Budget From Budget Table

                        Budget objBudget = db.Budgets.Where(a => a.Id == ParentId.BudgetId && a.IsDeleted == false).FirstOrDefault();
                        if (objBudget != null)
                        {
                            objBudget.IsDeleted = true;
                            db.Entry(objBudget).State = EntityState.Modified;
                        }
                    }

                    // Update Line Item with Other Budget Id
                    List<LineItem_Budget> LineItemBudgetList = db.LineItem_Budget.Where(a => BudgetDetailIds.Contains(a.BudgetDetailId)).ToList();
                    foreach (var LineitemBudget in LineItemBudgetList)
                    {
                        LineitemBudget.BudgetDetailId = OtherBudgetId;
                        db.Entry(LineitemBudget).State = EntityState.Modified;
                    }

                    // Delete Budget Id
                    List<Budget_Detail> BudgetDetailList = db.Budget_Detail.Where(a => BudgetDetailIds.Contains(a.Id)).ToList();
                    foreach (var BudgetDetail in BudgetDetailList)
                    {
                        BudgetDetail.IsDeleted = true;
                        db.Entry(BudgetDetail).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                //db.DeleteBudget(Selectedids, Convert.ToString(Sessions.User.ClientId));
            }
            var lstchildbudget = Common.GetBudgetlist();
            int _budgetId = 0, _curntBudgetId = 0;
            if (lstchildbudget != null)
            {
                _curntBudgetId = !string.IsNullOrEmpty(curntBudgetId) ? Int32.Parse(curntBudgetId) : 0;
                if (lstchildbudget.Any(budgt => budgt.Value == _curntBudgetId.ToString()))
                    _budgetId = _curntBudgetId;
                else
                {
                    string strbudgetId = lstchildbudget.Select(bdgt => bdgt.Value).FirstOrDefault();
                    _budgetId = !string.IsNullOrEmpty(strbudgetId) ? Int32.Parse(strbudgetId) : 0;
                }
            }
            return RefreshMainGridData(_budgetId, mainTimeFrame, ListofCheckedColums);


            #endregion
            //End
        }
        #endregion
        public ActionResult RefreshMainGridData(int budgetId = 0, string mainTimeFrame = "Yearly", string ListofCheckedColums = "")
        {
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            bool IsBudgetCreateEdit, IsBudgetView, IsForecastCreateEdit, IsForecastView;
            IsBudgetCreateEdit = _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            IsForecastCreateEdit = _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);

            gridRowModel = GetFinanceMainGridData(budgetId, mainTimeFrame, ListofCheckedColums);
            var DetailId = db.Budget_Detail.Where(a => a.BudgetId == budgetId && a.ParentId == null && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
            if (DetailId != 0)
            {
                var temp = gridRowModel.rows.Where(a => a.Detailid == Convert.ToString(DetailId)).Select(a => a.FinanemodelheaderObj).FirstOrDefault();
                if (temp != null)
                {
                    objFinanceHeader.Budget = Convert.ToDouble(temp.Budget);
                    objFinanceHeader.Forecast = Convert.ToDouble(temp.Forecast);
                    objFinanceHeader.Planned = Convert.ToDouble(temp.Planned);
                    objFinanceHeader.Actual = Convert.ToDouble(temp.Actual);
                }
                else
                {
                    objFinanceHeader.Budget = 0;
                    objFinanceHeader.Forecast = 0;
                    objFinanceHeader.Planned = 0;
                    objFinanceHeader.Actual = 0;
                }
                objFinanceHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
                objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
                objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
                objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();

                TempData["FinanceHeader"] = objFinanceHeader;
                gridRowModel.FinanemodelheaderObj = Common.CommonGetFinanceHeaderValue(objFinanceHeader);
            }
            ViewBag.BudgetId = (Int32)budgetId;
            ViewBag.IsBudgetCreateEdit = IsBudgetCreateEdit;
            ViewBag.IsBudgetView = IsBudgetView;
            ViewBag.IsForecastView = IsForecastView;
            ViewBag.IsForecastCreateEditView = IsForecastCreateEdit;

            return PartialView("_MainGrid", gridRowModel);
        }
        public ActionResult SaveNewBudgetDetail(string BudgetId, string BudgetDetailName, string ParentId, string mainTimeFrame = "Yearly", bool isNewBudget = false, string ListofCheckedColums = "")
        {
            int _budgetId = 0;
            try
            {
                _budgetId = !string.IsNullOrEmpty(BudgetId) ? Int32.Parse(BudgetId) : 0;
                if (isNewBudget)
                {
                    SaveNewBudget(BudgetDetailName);
                }
                else
                {

                    Budget_Detail objBudgetDetail = new Budget_Detail();
                    objBudgetDetail.BudgetId = _budgetId;
                    objBudgetDetail.Name = BudgetDetailName;
                    objBudgetDetail.ParentId = !string.IsNullOrEmpty(ParentId) ? Int32.Parse(ParentId) : 0;
                    objBudgetDetail.CreatedBy = Sessions.User.UserId;
                    objBudgetDetail.CreatedDate = DateTime.Now;
                    objBudgetDetail.IsDeleted = false;
                    db.Entry(objBudgetDetail).State = EntityState.Added;
                    db.SaveChanges();
                    int _budgetid = objBudgetDetail.Id;
                    SaveUserBudgetpermission(_budgetid);

                    #region Udate LineItem with child item
                    //Add By Nishant Sheth
                    int? BudgetDetailParentid = objBudgetDetail.ParentId;
                    int BudgetDetailId = objBudgetDetail.Id;

                    using (MRPEntities context = new MRPEntities())
                    {
                        IQueryable<LineItem_Budget> objLineItem = context.LineItem_Budget
                            .Where(x => x.BudgetDetailId == BudgetDetailParentid);
                        foreach (LineItem_Budget LineItem in objLineItem)
                        {
                            LineItem.BudgetDetailId = BudgetDetailId;
                        }
                        context.SaveChanges();
                    }
                    #endregion
                }
                return RefreshMainGridData(_budgetId, mainTimeFrame, ListofCheckedColums);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DhtmlXGridRowModel GetFinanceMainGridData(int budgetId, string mainTimeFrame = "Yearly", string ListofCheckedColums = "")
        {
            DhtmlXGridRowModel mainGridData = new DhtmlXGridRowModel();
            List<DhtmlxGridRowDataModel> gridRowData = new List<DhtmlxGridRowDataModel>();
            try
            {
                // Add By Nishant Sheth
                // Desc #1678 
                StringBuilder setHeader = new StringBuilder();
                StringBuilder attachHeader = new StringBuilder();
                StringBuilder setInitWidths = new StringBuilder();
                StringBuilder setColAlign = new StringBuilder();
                StringBuilder setColValidators = new StringBuilder();
                StringBuilder setColumnIds = new StringBuilder();
                StringBuilder setColTypes = new StringBuilder();
                StringBuilder setColumnsVisibility = new StringBuilder();
                StringBuilder HeaderStyle = new StringBuilder();
                #region Set coulmn base on columnset
                //List<Budget_Columns> objColumns = db.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false).Select(a => a).ToList();
                List<Budget_Columns> objColumns = (from ColumnSet in db.Budget_ColumnSet
                                                   join Columns in db.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                                                   where ColumnSet.IsDeleted == false && Columns.IsDeleted == false
                                                   && ColumnSet.ClientId == Sessions.User.ClientId
                                                   select Columns).ToList();
                var objCustomColumns = objColumns.Where(a => a.IsTimeFrame == false).Select(a => a).ToList();
                var objTimeFrameColumns = objColumns.Where(a => a.IsTimeFrame == true).Select(a => a).ToList();

                #endregion
                // End By Nishant Sheth


                _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
                //IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
                _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
                //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);

                var Listofcheckedcol = ListofCheckedColums.Split(',');

                #region "GetFinancial Parent-Child Data"

                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(Int32));
                dataTable.Columns.Add("ParentId", typeof(Int32));
                dataTable.Columns.Add("RowId", typeof(String));
                dataTable.Columns.Add("Name", typeof(String));
                dataTable.Columns.Add("AddRow", typeof(String));

                //  dataTable.Columns.Add("Selection", typeof(String));
                //dataTable.Columns.Add("Budget", typeof(String));
                //dataTable.Columns.Add("Forecast", typeof(String));
                //dataTable.Columns.Add("Planned", typeof(String));
                //dataTable.Columns.Add("Actual", typeof(String));
                #region set dynamic columns in dataTable
                foreach (var Columns in objColumns)
                {
                    dataTable.Columns.Add(Convert.ToString(Columns.CustomField.Name), typeof(String));
                }
                #endregion
                dataTable.Columns.Add("Action", typeof(String));
                dataTable.Columns.Add("LineItemCount", typeof(Int32));
                dataTable.Columns.Add("IsForcast", typeof(Boolean));
                dataTable.Columns.Add("lstLineItemIds", typeof(List<int>));
                dataTable.Columns.Add("User", typeof(String)); // Add By Nishant
                dataTable.Columns.Add("Owner", typeof(String)); // Add By Nishant
                dataTable.Columns.Add("Permission", typeof(String)); // Add By Nishant
                #region Set Tree Grid Properties and methods

                setHeader.Append("Task Name,,,");// Default 1st 4 columns header
                setInitWidths.Append("200,100,50,");
                setColAlign.Append("left,center,center,");
                setColTypes.Append("tree,ro,ro,");
                setColValidators.Append("CustomNameValid,,,");
                setColumnIds.Append("title,action,addrow,");
                HeaderStyle.Append("text-align:center;border-right:0px solid #d4d4d4;,border-left:0px solid #d4d4d4;,,");
                if (!_IsBudgetCreate_Edit && !_IsForecastCreate_Edit)
                {
                    setColumnsVisibility.Append("false,false,true,");
                }
                else
                {
                    setColumnsVisibility.Append("false,false,false,");
                }
                foreach (var columns in objColumns)
                {
                    setHeader.Append(columns.CustomField.Name + ",");
                    setInitWidths.Append("100,");
                    setColAlign.Append("center,");
                    setColTypes.Append("ro,");
                    setColValidators.Append(",");
                    setColumnIds.Append(columns.CustomField.Name + ",");
                    if (Listofcheckedcol.Contains(columns.CustomFieldId.ToString()))
                    {
                        setColumnsVisibility.Append("false,");
                    }
                    else
                    {
                        setColumnsVisibility.Append("true,");
                    }
                    HeaderStyle.Append("text-align:center;,");
                }
                setHeader.Append("User,Line Items,Owner");
                setInitWidths.Append("100,100,100");
                setColAlign.Append("center,center,center");
                setColTypes.Append("ro,ro,ro");
                setColumnIds.Append("action,lineitems,owner");
                setColumnsVisibility.Append("false,false,false");
                HeaderStyle.Append("text-align:center;,text-align:center;,text-align:center;");

                string trimSetheader = setHeader.ToString().TrimEnd(',');
                string trimAttachheader = attachHeader.ToString().TrimEnd(',');
                string trimSetInitWidths = setInitWidths.ToString().TrimEnd(',');
                string trimSetColAlign = setColAlign.ToString().TrimEnd(',');
                string trimSetColValidators = setColValidators.ToString().TrimEnd(',');
                string trimSetColumnIds = setColumnIds.ToString().TrimEnd(',');
                string trimSetColTypes = setColTypes.ToString().TrimEnd(',');
                string trimSetColumnsVisibility = setColumnsVisibility.ToString().TrimEnd(',');
                string trimHeaderStyle = HeaderStyle.ToString().TrimEnd(',');
                #endregion
                //budgetId = 8;
                List<Budget_Detail> tblBudgetDetails = db.Budget_Detail.Where(bdgt => bdgt.Budget.ClientId.Equals(Sessions.User.ClientId) && (bdgt.Budget.IsDeleted == false || bdgt.Budget.IsDeleted == null) && bdgt.BudgetId.Equals(budgetId) && bdgt.IsDeleted == false).ToList();
                var lstBudgetDetails = tblBudgetDetails.Select(a => new { a.Id, a.ParentId, a.Name, a.IsForecast, a.CreatedBy }).ToList();
                List<string> tacticStatus = Common.GetStatusListAfterApproved();// Add By Nishant Sheth

                List<int> lstBudgetDetailsIds = lstBudgetDetails.Select(bdgtdtls => bdgtdtls.Id).ToList();
                List<Budget_DetailAmount> BudgetDetailAmount = new List<Budget_DetailAmount>();
                BudgetDetailAmount = db.Budget_DetailAmount.Where(dtlAmnt => lstBudgetDetailsIds.Contains(dtlAmnt.BudgetDetailId)).ToList();

                List<int> tblPlanLineItemIds = new List<int>();
                tblPlanLineItemIds = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.IsDeleted == false).Select(a => a.PlanLineItemId).ToList();

                //List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => lstBudgetDetailsIds.Contains(a.BudgetDetailId)).Select(a => a).ToList();
                List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => lstBudgetDetailsIds.Contains(a.BudgetDetailId)).Select(a => a).ToList();

                LineItemidBudgetList = LineItemidBudgetList.Where(lnBudget => tblPlanLineItemIds.Contains(lnBudget.PlanLineItemId)).ToList();

                // Change By Nishant Sheth
                List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();
                List<int> LineItemids = tblPlanLineItemIds.Where(a => PlanLineItemBudgetDetail.Contains(a)).ToList();

                //List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
                // #1590 Changes Observation:5 - Nishant Sheth
                List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in db.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                     //join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Cost.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                     select Cost).ToList();
                //List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
                // #1590 Changes Observation:9 - Nishant Sheth
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in db.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                         //join LineItemBudget in db.LineItem_Budget on Actual.PlanLineItemId equals LineItemBudget.PlanLineItemId
                                                                                         join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                         join Tactic in db.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                         where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                         && tacticStatus.Contains(Tactic.Status)
                                                                                         select Actual).ToList();

                List<BDSService.User> lstUser = null;
                lstUser = objBDSServiceClient.GetUserListByClientId(Sessions.User.ClientId).ToList();
                var lstUserId = lstUser.Select(a => a.UserId).ToList();
                var ListOfUserPermission = db.Budget_Permission.Where(a => lstUserId.Contains(a.UserId)).ToList();
                string rowId = string.Empty;
                List<int> PlanLineItemsId, lstLineItemIds;
                BudgetAmount objBudgetAmount;
                bool isQuarterly = false;
                int cntlineitem = 0;
                if (!string.IsNullOrEmpty(mainTimeFrame))
                {
                    if (!mainTimeFrame.Equals(Enums.QuarterFinance.Yearly.ToString()))
                        isQuarterly = true;
                }

                var DefaultColumnList = Enums.DefaultGridColumnValues.Select(a => a.Key).ToList();
                var CustomCoulmnsId = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && a.IsTimeFrame == false
                    && a.MapTableName == Enums.MapTableName.CustomField_Entity.ToString()).Select(a => a.CustomFieldId).ToList();

                var CustomColumnsValue = db.CustomField_Entity.Where(a => CustomCoulmnsId.Contains(a.CustomFieldId)).Select(a => new { a.Value, a.CustomFieldId, a.CustomFieldEntityId, a.EntityId }).ToList();

                lstBudgetDetails.ForEach(
                    i =>
                    {
                        rowId = Regex.Replace(i.Name.Trim().Replace("_", ""), @"[^0-9a-zA-Z]+", "") + "_" + i.Id.ToString() + "_" + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        objBudgetAmount = new BudgetAmount();
                        PlanLineItemsId = new List<int>();
                        lstLineItemIds = new List<int>();
                        PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == i.Id && LineItemids.Contains(a.PlanLineItemId)).Select(a => a.PlanLineItemId).Distinct().ToList();
                        if (i.ParentId != null)
                        {
                            cntlineitem = PlanLineItemsId.Count;
                            lstLineItemIds = PlanLineItemsId;
                        }
                        else
                            cntlineitem = LineItemids.Count;
                        objBudgetAmount = GetMainGridAmountValue(isQuarterly, mainTimeFrame, BudgetDetailAmount.Where(a => a.BudgetDetailId == i.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), LineItemidBudgetList.Where(l => l.BudgetDetailId == i.Id).ToList());
                        //rowId = Regex.Replace(i.Name.Trim(), @"\s+", "") + i.Id.ToString() + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        //dataTable.Rows.Add(new Object[] { i.Id, i.ParentId == null ? 0 : i.ParentId, rowId, i.Name, "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row' />", objBudgetAmount.Budget.Sum().Value.ToString(formatThousand), objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand), objBudgetAmount.Plan.Sum().Value.ToString(formatThousand), objBudgetAmount.Actual.Sum().Value.ToString(formatThousand), "", PlanLineItemsId.Count });
                        // Get Owner name
                        var OwnerName = lstUser.Where(a => a.UserId == i.CreatedBy).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                        //dataTable.Rows.Add(new Object[] { i.Id, i.ParentId == null ? 0 : i.ParentId, rowId, i.Name, string.Empty, objBudgetAmount.Budget.Sum().Value.ToString(formatThousand), objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand), objBudgetAmount.Plan.Sum().Value.ToString(formatThousand), objBudgetAmount.Actual.Sum().Value.ToString(formatThousand), "", cntlineitem, i.IsForecast, lstLineItemIds, Convert.ToString(OwnerName) });
                       

                        int count = 0;
                        var CountUser = ListOfUserPermission.Where(a => a.BudgetDetailId == (Int32)i.Id).Select(t => t.UserId).Distinct().ToList();
                        if (CountUser.Count > 0)
                        {
                            count = CountUser.Count;
                        }
                        var CheckUserPermission = ListOfUserPermission.Where(a => a.BudgetDetailId == (Int32)i.Id && a.UserId == Sessions.User.UserId).ToList();
                        string isEdit = "";
                        string strUserAction = string.Empty;
                        if (CheckUserPermission.Count > 0)
                        {
                            if (CheckUserPermission.First().PermisssionCode == 0)
                            {
                                isEdit = "Edit";
                            }
                            else
                            {
                                isEdit = "View";
                            }
                        }
                        else
                        {
                            isEdit = "Edit";
                        }

                        if (_IsBudgetCreate_Edit)
                        {
                            strUserAction = string.Format("<div onclick='Edit({0},false,{1},this)' class='finance_link'><a>" + count + "</a>&nbsp;&nbsp;&nbsp;<span style='border-left:1px solid #000;height:20px'></span><span>&nbsp;&nbsp;<span style='text-decoration: underline;'>" + isEdit + "</div>", i.Id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'User'")));
                        }
                        else
                        {
                            strUserAction = string.Format("<div onclick='Edit({0},false,{1},this)' class='finance_link'><a>" + count + "</a>&nbsp;&nbsp;&nbsp;<span style='border-left:1px solid #000;height:20px'></span><span>&nbsp;&nbsp;<span style='text-decoration: underline;'>" + isEdit + "</div>", i.Id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'User'")));
                        }

                        DataRow Datarow = dataTable.NewRow();
                        Datarow[Enums.DefaultGridColumn.Id.ToString()] = i.Id;
                        Datarow[Enums.DefaultGridColumn.ParentId.ToString()] = i.ParentId == null ? 0 : i.ParentId;
                        Datarow[Enums.DefaultGridColumn.RowId.ToString()] = rowId;
                        Datarow[Enums.DefaultGridColumn.Name.ToString()] = i.Name;
                        Datarow[Enums.DefaultGridColumn.AddRow.ToString()] = string.Empty;
                        Datarow[Enums.DefaultGridColumn.Owner.ToString()] = Convert.ToString(OwnerName);
                        Datarow[Enums.DefaultGridColumn.lstLineItemIds.ToString()] = lstLineItemIds;
                        Datarow[Enums.DefaultGridColumn.IsForcast.ToString()] = i.IsForecast;
                        Datarow[Enums.DefaultGridColumn.LineItemCount.ToString()] = cntlineitem;
                        Datarow[Enums.DefaultGridColumn.Action.ToString()] = "";
                        Datarow[Enums.DefaultGridColumn.Permission.ToString()] = isEdit;

                        Datarow[Enums.DefaultGridColumn.User.ToString()] = strUserAction;
                        foreach (var col in objColumns)
                        {
                            string colname = col.CustomField.Name;

                            if (!DefaultColumnList.Contains(col.CustomField.Name))
                            {

                                if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    Datarow[colname] = objBudgetAmount.Budget.Sum().Value.ToString(formatThousand);
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    Datarow[colname] = objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand);
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString())
                                {
                                    Datarow[colname] = objBudgetAmount.Plan.Sum().Value.ToString(formatThousand);
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString())
                                {
                                    Datarow[colname] = objBudgetAmount.Actual.Sum().Value.ToString(formatThousand);
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && col.IsTimeFrame == false && col.MapTableName == Enums.MapTableName.CustomField_Entity.ToString())
                                {
                                    var CustomValue = CustomColumnsValue.Where(a => a.EntityId == i.Id && a.CustomFieldId == col.CustomFieldId).Select(a => a.Value).FirstOrDefault();
                                    Datarow[colname] = CustomValue != null ? CustomValue : "0";
                                }
                            }
                        }

                        dataTable.Rows.Add(Datarow);
                    });
                var columnNames = dataTable.Columns.Cast<DataColumn>()
                  .Select(x => x)
                  .ToList();
                var MinParentid = 0;
                int OtherBudgetid = Common.GetOtherBudgetId();
                List<DhtmlxGridRowDataModel> lstData = new List<DhtmlxGridRowDataModel>();
                lstData = GetTopLevelRows(dataTable, MinParentid)
                            .Select(row => CreateMainGridItem(dataTable, row, tblBudgetDetails, LineItemidBudgetList, columnNames, objColumns, OtherBudgetid)).ToList();

                #endregion

                mainGridData.setHeader = trimSetheader;
                mainGridData.attachHeader = trimAttachheader;
                mainGridData.setInitWidths = trimSetInitWidths;
                mainGridData.setColAlign = trimSetColAlign;
                mainGridData.setColValidators = trimSetColValidators;
                mainGridData.setColumnIds = trimSetColumnIds;
                mainGridData.setColTypes = trimSetColTypes;
                mainGridData.setColumnsVisibility = trimSetColumnsVisibility;
                mainGridData.HeaderStyle = trimHeaderStyle;
                mainGridData.rows = lstData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mainGridData;
        }
        private DhtmlxGridRowDataModel CreateMainGridItem(DataTable dataTable, DataRow row, List<Budget_Detail> lstBudgetDetails, List<LineItem_Budget> lstLineItemBudget, List<DataColumn> DataTablecolumnNames, List<Budget_Columns> objColumns, int OtherBudgetId = 0)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>();

            var BudgetCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.Budget).Select(a => a.CustomField.Name).FirstOrDefault();
            var ForecastCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast).Select(a => a.CustomField.Name).FirstOrDefault();
            var PlannedCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.None).Select(a => a.CustomField.Name).FirstOrDefault();
            var ActualCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.None).Select(a => a.CustomField.Name).FirstOrDefault();

            foreach (var col in DataTablecolumnNames)
            {
                variables.Add(col.ColumnName.ToString(), row[col.ColumnName.ToString()]);
            }

            //var id = row.Field<Int32>("Id");
            var id = variables.Where(a => a.Key == Enums.DefaultGridColumn.Id.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();
            //string rowId = row.Field<String>("RowId");
            var rowId = variables.Where(a => a.Key == Enums.DefaultGridColumn.RowId.ToString()).Select(a => a.Value.ToString()).FirstOrDefault();
            //var name = row.Field<String>("Name");
            var name = variables.Where(a => a.Key == Enums.DefaultGridColumn.Name.ToString()).Select(a => a.Value.ToString()).FirstOrDefault();
            //var addRow = row.Field<String>("AddRow");
            var addRow = variables.Where(a => a.Key == Enums.DefaultGridColumn.AddRow.ToString()).Select(a => a.Value.ToString()).FirstOrDefault();
            //  var SelectCheckbox = row.Field<String>("Selection");
            var budget = variables.Where(a => a.Key == BudgetCol).Select(a => a.Value).FirstOrDefault();
            var forecast = variables.Where(a => a.Key == ForecastCol).Select(a => a.Value).FirstOrDefault();
            var planned = variables.Where(a => a.Key == PlannedCol).Select(a => a.Value).FirstOrDefault();
            var actual = variables.Where(a => a.Key == ActualCol).Select(a => a.Value).FirstOrDefault();

            //var lineItemCount = row.Field<Int32>("LineItemCount");
            var lineItemCount = variables.Where(a => a.Key == Enums.DefaultGridColumn.LineItemCount.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();
            //int parentId = row.Field<Int32>("ParentId");
            var parentId = variables.Where(a => a.Key == Enums.DefaultGridColumn.ParentId.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();
            //bool IsForcast = row.Field<Boolean>("IsForcast");
            var IsForcast = variables.Where(a => a.Key == Enums.DefaultGridColumn.IsForcast.ToString()).Select(a => a.Value.ToString()).FirstOrDefault();
            List<int> lstLineItemIds = row.Field<List<int>>("lstLineItemIds");
            //var Owner = row.Field<String>("Owner");  // Add By Nishant
            var Owner = Convert.ToString(variables.Where(a => a.Key == Enums.DefaultGridColumn.Owner.ToString()).Select(a => a.Value.ToString()).FirstOrDefault());
            //var User = row.Field<String>("User");  // Add By Nishant
            var User = Convert.ToString(variables.Where(a => a.Key == Enums.DefaultGridColumn.User.ToString()).Select(a => a.Value.ToString()).FirstOrDefault());
            var Permission = Convert.ToString(variables.Where(a => a.Key == Enums.DefaultGridColumn.Permission.ToString()).Select(a => a.Value.ToString()).FirstOrDefault());
            //var action = row.Field<String>("Action");
            //var budget = row.Field<List<Double?>>("Budget");
            //var forcast = row.Field<List<Double?>>("ForeCast");
            //var budgetTotal = row.Field<Double>("BudgetTotal");
            //var forcastTotal = row.Field<Double>("ForeCastTotal");
            List<DhtmlxGridRowDataModel> children = new List<DhtmlxGridRowDataModel>();
            IEnumerable<DataRow> lstChildren = null;
            lstChildren = GetChildren(dataTable, id);
            if (lstChildren != null && lstChildren.Count() > 0)
                lstChildren = lstChildren.OrderBy(child => child.Field<String>("Name"), new AlphaNumericComparer()).ToList();
            if (!Convert.ToBoolean(IsForcast))
            {
                children = lstChildren
                  .Select(r => CreateMainGridItem(dataTable, r, lstBudgetDetails, lstLineItemBudget, DataTablecolumnNames, objColumns, OtherBudgetId))
                  .ToList();
            }
            List<string> ParentData = new List<string>();
            int rwcount = dataTable != null ? dataTable.Rows.Count : 0;
            name = HttpUtility.HtmlEncode(Convert.ToString(name));
            #region "Add Action column link"
            string strAction = string.Empty;

            if ((lstChildren != null && lstChildren.Count() > 0 && Convert.ToBoolean(IsForcast) == false) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
            {
                double? forcastVal = 0, pannedVal = 0, actualVal = 0;
                forcastVal = GetSumofValueMainGrid(dataTable, id, "Forecast");
                pannedVal = GetSumofValueMainGrid(dataTable, id, "Planned");
                actualVal = GetSumofValueMainGrid(dataTable, id, "Actual");

                forecast = forcastVal.HasValue ? forcastVal.Value.ToString(formatThousand) : "0";
                planned = pannedVal.HasValue ? pannedVal.Value.ToString(formatThousand) : "0";
                actual = actualVal.HasValue ? actualVal.Value.ToString(formatThousand) : "0";
                rowId = rowId + "_" + _IsBudgetCreate_Edit.ToString(); // Append Create/Edit flag value for Budget permission to RowId.

                if (_IsBudgetCreate_Edit)
                {
                    if (id != OtherBudgetId)
                    {
                        addRow = "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row'></div><div id='cb" + rowId + "' row-id='" + rowId + "' name='" + name + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>";
                    }
                    else
                    {
                        addRow = "";
                    }
                    //  SelectCheckbox = "<input id='cb" + rowId + "' row-id='" + rowId + "' onclick='CheckboxClick(this)' type='checkbox' />";
                    if (Convert.ToBoolean(IsForcast))
                    {
                        if (Permission != null)
                        {
                            if (Permission == "Edit")
                            {
                                strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                            }
                            else
                            {
                                strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                            }
                        }
                        else
                        {
                            strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                        }
                    }
                    else
                    {
                        if (Permission != null)
                        {
                            if (Permission == "Edit")
                            {
                                strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                            }
                            else
                            {
                                strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                            }
                        }
                        else
                        {
                            strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                        }
                    }
                }
                else
                {
                    if (Convert.ToBoolean(IsForcast))
                    {
                        strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                    }
                    else
                    {
                        strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                    }
                }
                if ((lstChildren != null && lstChildren.Count() > 0) && parentId > 0) // LineItem count will be not set for Most Parent Item & last Child Item.
                {
                    //lineItemCount = dataTable
                    //                    .Rows
                    //                    .Cast<DataRow>()
                    //                    .Where(rw => rw.Field<Int32>("ParentId") == id).Sum(chld => chld.Field<Int32>("LineItemCount"));

                    var LineItemIds = dataTable
                                        .Rows
                                        .Cast<DataRow>()
                                        .Where(rw => rw.Field<Int32>("ParentId") == id).Select(chld => chld.Field<List<int>>("lstLineItemIds")).ToList();

                    if (lstLineItemIds == null)
                        lstLineItemIds = new List<int>();

                    LineItemIds.ForEach(lstIds => lstLineItemIds.AddRange(lstIds));

                    row.SetField<List<int>>("lstLineItemIds", lstLineItemIds);
                    lineItemCount = lstLineItemIds != null ? lstLineItemIds.Distinct().Count() : 0;
                    row.SetField<Int32>("LineItemCount", lineItemCount); // Update LineItemCount in DataTable.
                }
            }
            else
            {
                rowId = rowId + "_" + _IsForecastCreate_Edit.ToString(); // Append Create/Edit flag value for Forecast permission to RowId.
                if (Convert.ToBoolean(IsForcast))
                {
                    double? forcastVal = 0, pannedVal = 0, actualVal = 0;
                    forcastVal = GetSumofValueMainGrid(dataTable, id, "Forecast");
                    pannedVal = GetSumofValueMainGrid(dataTable, id, "Planned");
                    actualVal = GetSumofValueMainGrid(dataTable, id, "Actual");

                    forecast = forcastVal.HasValue ? forcastVal.Value.ToString(formatThousand) : "0";
                    planned = pannedVal.HasValue ? pannedVal.Value.ToString(formatThousand) : "0";
                    actual = actualVal.HasValue ? actualVal.Value.ToString(formatThousand) : "0";

                    var fltrChildIds = lstBudgetDetails.Where(budgt => budgt.ParentId == id).Select(budgt => budgt.Id).ToList();
                    List<int> lstlineItemIds = lstLineItemBudget.Where(line => fltrChildIds.Contains(line.BudgetDetailId)).Select(planLineItem => planLineItem.PlanLineItemId).Distinct().ToList();
                    row.SetField<List<int>>("lstLineItemIds", lstlineItemIds);
                    lineItemCount = lstlineItemIds.Count;
                    row.SetField<Int32>("LineItemCount", lstlineItemIds.Count);
                }
                if (_IsForecastCreate_Edit)
                {
                    if (Permission != null)
                    {
                        if (Permission == "Edit")
                        {
                            addRow = "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row'></div><div id='cb" + rowId + "' row-id='" + rowId + "' name='" + name + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>";
                            //  SelectCheckbox = "<input id='cb" + rowId + "' row-id='" + rowId + "'  onclick='CheckboxClick(this)' type='checkbox' />";
                            strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                        }
                        else
                        {
                            strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                        }
                    }
                    else
                    {
                        addRow = "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row'></div><div id='cb" + rowId + "' row-id='" + rowId + "' name='" + name + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>";
                        //  SelectCheckbox = "<input id='cb" + rowId + "' row-id='" + rowId + "'  onclick='CheckboxClick(this)' type='checkbox' />";
                        strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'Edit'")));
                    }
                }
                else
                {
                    strAction = string.Format("<div onclick='EditBudget({0},false,{1},{2})' class='finance_link'>View Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")), HttpUtility.HtmlEncode(Convert.ToString("'View'")));
                }
            }
            //forecast = forecast.ToString(new c
            ParentData.Add(name);
            ParentData.Add(strAction);
            ParentData.Add(addRow);
            //   ParentData.Add(SelectCheckbox);
            FinanceModelHeaders objHeader = new FinanceModelHeaders();
            objHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            objHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();

            if (BudgetCol != null)
            {
                ParentData.Add(Convert.ToString(budget));
                objHeader.Budget = Convert.ToDouble(budget);
            }
            if (ForecastCol != null)
            {
                ParentData.Add(Convert.ToString(forecast));
                objHeader.Forecast = Convert.ToDouble(forecast);
            }
            if (PlannedCol != null)
            {
                ParentData.Add(Convert.ToString(planned));
                objHeader.Planned = Convert.ToDouble(planned);
            }
            if (ActualCol != null)
            {
                ParentData.Add(Convert.ToString(actual));
                objHeader.Actual = Convert.ToDouble(actual);
            }
            var CustColList = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && a.IsTimeFrame == false && a.MapTableName == Enums.MapTableName.CustomField_Entity.ToString()).ToList();
            foreach (var custCol in CustColList)
            {
                var custval = variables.Where(a => a.Key == custCol.CustomField.Name).Select(a => a.Value).FirstOrDefault();
                ParentData.Add(Convert.ToString(custval));
            }
            ParentData.Add(User);
            ParentData.Add(lineItemCount.ToString());
            ParentData.Add(Owner);
            #endregion

            //return new FinanceParentChildModel { Id = id, Name = name, Children = children, Budget = budget, ForeCast = forcast, BudgetTotal = budgetTotal, ForeCastTotal = forcastTotal };
            return new DhtmlxGridRowDataModel { id = rowId, data = ParentData, rows = children, Detailid = Convert.ToString(id), FinanemodelheaderObj = objHeader };
        }
        public ActionResult UpdateBudgetDetail(string BudgetId, string BudgetDetailName, string BudgetDetailId, string ParentId, string mainTimeFrame = "Yearly", string ListofCheckedColums = "")
        {
            int budgetId = 0, budgetDetailId = 0, parentId = 0;
            try
            {
                budgetId = !string.IsNullOrEmpty(BudgetId) ? Int32.Parse(BudgetId) : 0;
                budgetDetailId = !string.IsNullOrEmpty(BudgetDetailId) ? Int32.Parse(BudgetDetailId) : 0;
                parentId = !string.IsNullOrEmpty(ParentId) ? Int32.Parse(ParentId) : 0;
                if (budgetDetailId > 0 && parentId > 0)
                {
                    #region "Update BudgetDetail Name"
                    Budget_Detail objBudgetDetail = new Budget_Detail();
                    objBudgetDetail = db.Budget_Detail.Where(budgtDtl => budgtDtl.Id == budgetDetailId && budgtDtl.IsDeleted == false).FirstOrDefault();
                    if (objBudgetDetail != null)
                    {
                        objBudgetDetail.Name = BudgetDetailName.Trim();
                        db.Entry(objBudgetDetail).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    #endregion
                }
                else if (budgetDetailId > 0 && parentId <= 0)
                {
                    #region "Update Budget Name"
                    Budget objBudget = new Budget();
                    Guid clientId = Sessions.User.ClientId;
                    objBudget = db.Budgets.Where(budgt => budgt.Id == budgetId && budgt.IsDeleted == false && budgt.ClientId == clientId).FirstOrDefault();
                    if (objBudget != null)
                    {
                        objBudget.Name = BudgetDetailName.Trim();
                        db.Entry(objBudget).State = EntityState.Modified;
                    }
                    #endregion

                    #region "Update Budget Detail Name"
                    Budget_Detail objMainBudgetDetail = new Budget_Detail();
                    objMainBudgetDetail = db.Budget_Detail.Where(budgtDtl => budgtDtl.Id == budgetDetailId && budgtDtl.IsDeleted == false).FirstOrDefault();
                    if (objMainBudgetDetail != null)
                    {
                        objMainBudgetDetail.Name = BudgetDetailName.Trim();
                        db.Entry(objMainBudgetDetail).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    #endregion
                }
                return RefreshMainGridData(budgetId, mainTimeFrame, ListofCheckedColums);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region "User Permission:Ability to add multiple users to Budget items"

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// 
        /// </summary>
        /// <param name="BudgetId">contains BudgetDetail's Id</param>
        /// <param name="FlagCondition">contains of edit or view</param>
        /// <returns>return model to partial view</returns>
        public ActionResult EditPermission(int BudgetId = 0, string level = "", string FlagCondition = "")
        {
            FinanceModel objFinanceModel = new FinanceModel();
            ViewBag.BudgetId = BudgetId;
            ViewBag.EditLevel = level;
            List<ViewByModel> lstchildbudget = new List<ViewByModel>();
            lstchildbudget = Common.GetParentBudgetlist(BudgetId);
            List<UserPermission> _user = new List<UserPermission>();
            if (lstchildbudget.Count > 0)
            {
                ViewBag.childbudgetlist = lstchildbudget;
            }
            else
            {
                lstchildbudget.Add(new ViewByModel { Text = "Please Select", Value = "0" });
                ViewBag.childbudgetlist = lstchildbudget;
            }
            if (FlagCondition == "Edit")
            {
                ViewBag.FlagCondition = "Edit";
            }
            else
            {
                ViewBag.FlagCondition = "View";

            }
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

            Guid userId = new Guid();
            BDSService.User objUser = new BDSService.User();

            var UserList = (from c in db.Budget_Permission
                            where c.BudgetDetailId == BudgetId
                            orderby c.UserId
                            select c).GroupBy(g => g.UserId).Select(x => x.FirstOrDefault()).ToList();

            for (int i = 0; i < UserList.Count; i++)
            {
                UserPermission user = new UserPermission();
                objUser = objBDSServiceClient.GetTeamMemberDetails(userId = Guid.Parse(UserList[i].UserId.ToString()), Sessions.ApplicationId);
                user.budgetID = BudgetId;
                user.id = objUser.UserId.ToString();
                user.FirstName = objUser.FirstName;
                user.LastName = objUser.LastName;
                user.Role = objUser.RoleTitle;
                user.Permission = UserList[i].PermisssionCode;
                user.createdby = UserList[i].CreatedBy.ToString();
                _user.Add(user);
            }
            if (UserList.Count == 0)
            {
                ViewBag.NoRecord = "NoRecord";
            }
            objFinanceModel.Userpermission = _user;
            return PartialView("_UserPermission", objFinanceModel);
        }


        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Delete record from Budget_Permission table
        /// </summary>
        /// <param name="id">contains user's Id</param>
        /// <returns>If success than return true</returns>
        [HttpPost]
        public JsonResult Delete(Guid id, int budgetId)
        {
            List<Budget_Permission> BudgetDetailList = db.Budget_Permission.Where(i => i.BudgetDetailId == budgetId && i.UserId == id).ToList();
            foreach (var BudgetDetail in BudgetDetailList)
            {
                db.Entry(BudgetDetail).State = EntityState.Deleted;
            }
            db.SaveChanges();
            return Json(new { Flag = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Get list of user records when typing letter on textbox 
        /// </summary>
        /// <param name="term">contains typed letter of textbox</param>
        /// <returns>User list</returns>
        public JsonResult getData(string term)
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            List<User> lstUserDetail = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true).ToList();

            lstUserDetail.Add(new User
            {
                UserId = Sessions.User.UserId,
                FirstName = Sessions.User.FirstName,
                LastName = Sessions.User.LastName,
                JobTitle = Sessions.User.JobTitle
            });

            List<User> Getvalue = new List<User>();
            if (lstUserDetail.Count > 0)
            {
                lstUserDetail = lstUserDetail.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();
                Getvalue = lstUserDetail.Where(user => user.FirstName.ToLower().Contains(term.ToLower())).Select(user => new User { UserId = user.UserId, JobTitle = user.JobTitle, DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
            }
            else
            {
                Getvalue = new List<User>();
            }
            return Json(Getvalue, JsonRequestBehavior.AllowGet);
        }

   
        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Get specific of user record on selection of dropdown list
        /// </summary>
        /// <param name="Id">contains user's id</param>
        /// <returns>User Record</returns>
        public JsonResult GetuserRecord(string Id)
        {
            Guid userId = new Guid();
            if (Id == null)
            {
                userId = Sessions.User.UserId;
            }
            else
            {
                userId = Guid.Parse(Id);
            }
            BDSService.User objUser = new BDSService.User();
            UserModel objUserModel = new UserModel();
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            try
            {
                objUser = objBDSServiceClient.GetTeamMemberDetails(userId, Sessions.ApplicationId);
                if (objUser != null)
                {
                    objUserModel.DisplayName = objUser.DisplayName;
                    objUserModel.Email = objUser.Email;
                    objUserModel.Phone = objUser.Phone;
                    objUserModel.FirstName = objUser.FirstName;
                    objUserModel.JobTitle = objUser.JobTitle;
                    objUserModel.LastName = objUser.LastName;
                    objUserModel.UserId = objUser.UserId;
                    objUserModel.RoleTitle = objUser.RoleTitle;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }


            return Json(objUserModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Save record in Budget_Permission table
        /// </summary>
        /// <param name="UserData">contains user's UserId,permission code,dropdown selection id</param>
        /// <param name="ID">contains user's id</param>
        /// <returns>if sucess then return true else false</returns>
        [HttpPost]
        public JsonResult SaveDetail(List<UserBudgetPermission> UserData, int ID)
        {
            if (UserData != null)
            {
                for (int i = 0; i < UserData.Count; i++)
                {
                    Budget_Permission objBudget_Permission = new Budget_Permission();
                    Guid id = Guid.Parse(UserData[i].UserId);

                    Budget_Permission BudgetDetailList = db.Budget_Permission.Where(t => t.BudgetDetailId.Equals(ID) && t.UserId.Equals(id)).FirstOrDefault();
                    if (BudgetDetailList == null)
                    {
                        objBudget_Permission.UserId = Guid.Parse(UserData[i].UserId);
                        objBudget_Permission.BudgetDetailId = Convert.ToInt32(ID);
                        objBudget_Permission.CreatedBy = Sessions.User.UserId;
                        objBudget_Permission.CreatedDate = DateTime.Now;
                        objBudget_Permission.PermisssionCode = UserData[i].PermisssionCode;
                        db.Entry(objBudget_Permission).State = EntityState.Added;
                        db.SaveChanges();
                    }
                    else
                    {

                        BudgetDetailList.UserId = Guid.Parse(UserData[i].UserId);
                        BudgetDetailList.BudgetDetailId = Convert.ToInt32(ID);
                        BudgetDetailList.CreatedBy = Sessions.User.UserId;
                        BudgetDetailList.CreatedDate = DateTime.Now;
                        BudgetDetailList.PermisssionCode = UserData[i].PermisssionCode;
                        db.Entry(BudgetDetailList).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Get specific record based on dropdown selection value of budgetdetail id
        /// </summary>
        /// <param name="BudgetId">contains BudgetDetailid</param>
        /// <param name="FlagCondition">contains Condion of edit or view</param>
        /// <returns>if sucess then return true else false</returns>
        public JsonResult DrpFilterByBudget(int BudgetId = 0, string level = "", string FlagCondition = "")
        {

            ViewBag.BudgetId = BudgetId;
            List<UserPermission> _user = new List<UserPermission>();
            if (FlagCondition == "Edit")
            {
                ViewBag.FlagCondition = "Edit";
            }
            else
            {
                ViewBag.FlagCondition = "View";

            }
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

            Guid userId = new Guid();
            BDSService.User objUser = new BDSService.User();
            var UserList = (from c in db.Budget_Permission
                            where c.BudgetDetailId == BudgetId
                            orderby c.UserId
                            select c).GroupBy(g => g.UserId).Select(x => x.FirstOrDefault()).ToList();

            for (int i = 0; i < UserList.Count; i++)
            {
                UserPermission user = new UserPermission();
                objUser = objBDSServiceClient.GetTeamMemberDetails(userId = Guid.Parse(UserList[i].UserId.ToString()), Sessions.ApplicationId);
                user.budgetID = BudgetId;
                user.id = objUser.UserId.ToString();
                user.FirstName = objUser.FirstName;
                user.LastName = objUser.LastName;
                user.Role = objUser.RoleTitle;
                user.Permission = UserList[i].PermisssionCode;
                _user.Add(user);
            }
            return Json(_user, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Save detail on aading new budgetitem in Budget_Permission table
        /// </summary>
        /// <param name="BudgetId">contains BudgetDetailid</param>
        public void SaveUserBudgetpermission(int budgetId)
        {
            Budget_Permission objBudget_Permission = new Budget_Permission();
            objBudget_Permission.UserId = Sessions.User.UserId;
            objBudget_Permission.BudgetDetailId = budgetId;
            objBudget_Permission.CreatedDate = System.DateTime.Now.Date;
            objBudget_Permission.CreatedBy = Sessions.User.UserId;
            objBudget_Permission.PermisssionCode = 0;
            db.Entry(objBudget_Permission).State = EntityState.Added;
            db.SaveChanges();
        }

        #endregion

        #region Methods for Get Header Value
        public ActionResult GetFinanceHeaderValue(int budgetId = 0, string timeFrameOption = "", string isQuarterly = "Quarterly", bool IsMain = false, string mainTimeFrame = "Yearly")
        {
            //FinanceModelHeaders objfinanceheader = new FinanceModelHeaders();
            //if (TempData["FinanceHeader"] != null)
            //{
            //    objfinanceheader = TempData["FinanceHeader"] as FinanceModelHeaders;
            //}
            //objfinanceheader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            //objfinanceheader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            //objfinanceheader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            //objfinanceheader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            if (IsMain)
            {
                objFinanceHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
                objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
                objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
                objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
                if (TempData["FinanceHeader"] != null)
                {
                    gridRowModel = GetFinanceMainGridData(budgetId, mainTimeFrame);
                    var DetailId = db.Budget_Detail.Where(a => a.BudgetId == budgetId && a.ParentId == null && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
                    if (DetailId != 0)
                    {
                        var temp = gridRowModel.rows.Where(a => a.Detailid == Convert.ToString(DetailId)).Select(a => a.FinanemodelheaderObj).FirstOrDefault();
                        if (temp != null)
                        {
                            objFinanceHeader.Budget = Convert.ToDouble(temp.Budget);
                            objFinanceHeader.Forecast = Convert.ToDouble(temp.Forecast);
                            objFinanceHeader.Planned = Convert.ToDouble(temp.Planned);
                            objFinanceHeader.Actual = Convert.ToDouble(temp.Actual);
                        }
                        else
                        {
                            objFinanceHeader.Budget = 0;
                            objFinanceHeader.Forecast = 0;
                            objFinanceHeader.Planned = 0;
                            objFinanceHeader.Actual = 0;
                        }


                        TempData["FinanceHeader"] = objFinanceHeader;
                        gridRowModel.FinanemodelheaderObj = Common.CommonGetFinanceHeaderValue(objFinanceHeader);
                    }
                }
                else
                {
                    objFinanceHeader = TempData["FinanceHeader"] as FinanceModelHeaders;
                }
            }
            else
            {
                objFinanceHeader = TempData["FinanceHeader"] as FinanceModelHeaders;

            }

            return PartialView("_financeheader", objFinanceHeader);
        }
        #endregion

        #region Get parent Child dropdown value and bind other dropdowns
        public JsonResult GetChildBudget(int BudgetId = 0)
        {
            var lstchildbudget = Common.GetChildBudgetlist(BudgetId);
            return Json(lstchildbudget, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParentBudget(int BudgetId = 0)
        {
            var lstparnetbudget = Common.GetParentBudgetlist(BudgetId);
            return Json(lstparnetbudget, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetChildTimeFrame()
        {
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "This Year (Quarterly)", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "This Year (Monthly)", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 1", Value = Enums.QuarterWithSpace.Quarter1.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 2", Value = Enums.QuarterWithSpace.Quarter2.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 3", Value = Enums.QuarterWithSpace.Quarter3.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 4", Value = Enums.QuarterWithSpace.Quarter4.ToString() });

            return Json(lstViewByAllocated, JsonRequestBehavior.AllowGet);
        }

        public List<ViewByModel> GetColumnSet()
        {

            //List<ViewByModel> lstColumnset = db.Budget_ColumnSet.Where(a => a.ClientId == Sessions.User.ClientId && a.IsDeleted == false)
            //    .Select(a => new { a.Name, a.Id }).ToList().Select(a => new ViewByModel { Text = a.Name, Value = Convert.ToString(a.Id) }).ToList();


            List<ViewByModel> lstColumnset = (from ColumnSet in db.Budget_ColumnSet
                                              join Columns in db.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                                              where ColumnSet.ClientId == Sessions.User.ClientId && ColumnSet.IsDeleted == false
                                              && Columns.IsDeleted == false
                                              select new
                                              {
                                                  ColumnSet.Name,
                                                  ColumnSet.Id,
                                                  ColId = Columns.Id
                                              }).ToList().GroupBy(g => new { Id = g.Id, Name = g.Name })
                                              .Select(a => new { a.Key.Id, a.Key.Name, Count = a.Count() })
                                              .Where(a => a.Count > 0)
                                              .Select(a => new ViewByModel { Text = a.Name, Value = Convert.ToString(a.Id) }).ToList();

            //return Json(lstColumnset, JsonRequestBehavior.AllowGet);
            return lstColumnset;
        }

        public JsonResult GetColumns(int ColumnSetId = 0)
        {
            List<ViewByModel> lstColumns = db.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false)
                .Select(a => new { a.CustomField.Name, a.CustomField.CustomFieldId }).ToList()
                .Select(a => new ViewByModel { Text = a.Name, Value = Convert.ToString(a.CustomFieldId) }).ToList();

            return Json(lstColumns, JsonRequestBehavior.AllowGet);
            //return lstColumnset;
        }
        #endregion

        #region Get Budget/Forecast Grid Data

        public JsonResult DeleteBudgetForecastData(string SelectedRowIDs = null)
        {
            #region Delete Fields
            if (SelectedRowIDs != null)
            {
                var Values = JsonConvert.DeserializeObject<List<DeleteRowID>>(SelectedRowIDs);
                var Selectedids = Values.Select(ids => int.Parse(ids.Id.ToString())).FirstOrDefault();

                #region Old Code
                //List<Budget_Detail> BudgetDetail = db.Budget_Detail.Where(budgetdetail => Selectedids.Contains(budgetdetail.Id) && budgetdetail.IsDeleted == false).Select(a => a).ToList();

                //List<Budget_Detail> DetailList = db.Budget_Detail.Where(budgetdetail => budgetdetail.IsDeleted == false).Select(a => a).ToList();

                //foreach (var item in BudgetDetail)
                //{
                //    var ParentID = item.ParentId;
                //    if (ParentID == null)
                //    {
                //        var Budget = db.Budgets.Where(a => a.Id == item.BudgetId).Select(a => a).ToList();
                //        foreach (var value in Budget)
                //        {
                //            value.IsDeleted = true;
                //            db.Entry(value).State = EntityState.Modified;
                //            db.SaveChanges();
                //        }

                //    }

                //    var ChildItems = DetailList.Where(child => child.ParentId == item.Id).Select(child => child).ToList();
                //    foreach (var child in ChildItems)
                //    {
                //        child.IsDeleted = true;
                //        db.Entry(child).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }

                //    item.IsDeleted = true;
                //    db.Entry(item).State = EntityState.Modified;
                //}
                //db.SaveChanges();
                #endregion


                //db.DeleteBudget(Selectedids, Convert.ToString(Sessions.User.ClientId));
                var SelectedBudgetDetail = (from details in db.Budget_Detail
                                            where (details.ParentId == Selectedids || details.Id == Selectedids) && details.IsDeleted == false || details.IsDeleted == null
                                            select new
                                            {
                                                details.Id,
                                                details.BudgetId,
                                                details.ParentId,
                                                details.Name,
                                                details.IsDeleted
                                            }).ToList();

                var BudgetDetailJoin = (from details in db.Budget_Detail
                                        join selectdetails in
                                            (from details in db.Budget_Detail
                                             where (details.ParentId == Selectedids || details.Id == Selectedids) && details.IsDeleted == false || details.IsDeleted == null
                                             select new
                                             {
                                                 details.Id,
                                                 details.BudgetId,
                                                 details.ParentId,
                                                 details.Name,
                                                 details.IsDeleted
                                             }) on details.ParentId equals selectdetails.Id
                                        select new
                                        {
                                            details.Id,
                                            details.BudgetId,
                                            details.ParentId,
                                            details.Name,
                                            details.IsDeleted
                                        }).ToList();

                var BudgetDetailData = SelectedBudgetDetail.Union(BudgetDetailJoin).ToList();
                if (BudgetDetailData.Count > 0)
                {
                    var ParentId = BudgetDetailData.Where(a => a.ParentId == null).Select(a => a).FirstOrDefault();
                    var BudgetDetailIds = BudgetDetailData.Select(a => a.Id).ToList();
                    int OtherBudgetId = Common.GetOtherBudgetId();

                    if (ParentId != null)
                    {
                        // Delete Budget From Budget Table

                        Budget objBudget = db.Budgets.Where(a => a.Id == ParentId.BudgetId && a.IsDeleted == false).FirstOrDefault();
                        if (objBudget != null)
                        {
                            objBudget.IsDeleted = true;
                            db.Entry(objBudget).State = EntityState.Modified;
                        }
                    }

                    // Update Line Item with Other Budget Id
                    List<LineItem_Budget> LineItemBudgetList = db.LineItem_Budget.Where(a => BudgetDetailIds.Contains(a.BudgetDetailId)).ToList();
                    foreach (var LineitemBudget in LineItemBudgetList)
                    {
                        LineitemBudget.BudgetDetailId = OtherBudgetId;
                        db.Entry(LineitemBudget).State = EntityState.Modified;
                    }

                    // Delete Budget Id
                    List<Budget_Detail> BudgetDetailList = db.Budget_Detail.Where(a => BudgetDetailIds.Contains(a.Id)).ToList();
                    foreach (var BudgetDetail in BudgetDetailList)
                    {
                        BudgetDetail.IsDeleted = true;
                        db.Entry(BudgetDetail).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                //db.DeleteBudget(Selectedids, Convert.ToString(Sessions.User.ClientId));
            }

            #endregion

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditBudgetGridData(int BudgetId = 0, string IsQuaterly = "quarters", string EditLevel = "", int ColumnSetId = 0, string BudgetCreateEdit = "", string ForecastCreateEdit = "", string ListofCheckedColums = "", string EditPermission = "")
        {
            DhtmlXGridRowModel budgetMain = new DhtmlXGridRowModel();
            StringBuilder setHeader = new StringBuilder();
            StringBuilder attachHeader = new StringBuilder();
            StringBuilder setInitWidths = new StringBuilder();
            StringBuilder setColAlign = new StringBuilder();
            StringBuilder setColValidators = new StringBuilder();
            StringBuilder setColumnIds = new StringBuilder();
            StringBuilder setColTypes = new StringBuilder();
            StringBuilder setColumnsVisibility = new StringBuilder();

            bool _isBudgetCreateEdit = false;
            bool _isForecastCreateEdit = false;

            var Listofcheckedcol = ListofCheckedColums.Split(',');
            #region Set coulmn base on columnset
            //List<Budget_Columns> objColumns = db.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false).Select(a => a).ToList();
            List<Budget_Columns> objColumns = (from ColumnSet in db.Budget_ColumnSet
                                               join Columns in db.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                                               where ColumnSet.IsDeleted == false && Columns.IsDeleted == false
                                               && ColumnSet.ClientId == Sessions.User.ClientId
                                               select Columns).ToList();
            var objCustomColumns = objColumns.Where(a => a.IsTimeFrame == false).Select(a => a).ToList();
            var objTimeFrameColumns = objColumns.Where(a => a.IsTimeFrame == true).Select(a => a).ToList();

            #endregion

            budgetMain.BudgetColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && a.IsDeleted == false && a.IsTimeFrame == true
                    && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            budgetMain.ForecastColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && a.IsDeleted == false && a.IsTimeFrame == true
                    && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            budgetMain.PlanColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.None && a.IsDeleted == false && a.IsTimeFrame == true
                    && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            budgetMain.ActualColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.None && a.IsDeleted == false && a.IsTimeFrame == true
                    && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            if (EditLevel == "Budget")
            {
                if (!string.IsNullOrEmpty(BudgetCreateEdit) && Convert.ToString(BudgetCreateEdit).ToLower() == "true")
                {
                    _isBudgetCreateEdit = true;
                    budgetMain.enableTreeCellEdit = true;
                }
                else
                {
                    budgetMain.enableTreeCellEdit = false;
                }

                if (!_isBudgetCreateEdit && !_isForecastCreateEdit)
                {
                    setColumnsVisibility.Append("false,true,true,");
                }
                else
                {
                    setColumnsVisibility.Append("false,false,true,");
                }
                budgetMain.ColumneditLevel = budgetMain.BudgetColName;
            }
            else
            {
                if (!string.IsNullOrEmpty(ForecastCreateEdit) && Convert.ToString(ForecastCreateEdit).ToLower() == "true")
                {
                    _isForecastCreateEdit = true;
                    budgetMain.enableTreeCellEdit = true;
                }
                else
                {
                    budgetMain.enableTreeCellEdit = false;
                }

                if (!_isBudgetCreateEdit && !_isForecastCreateEdit)
                {
                    setColumnsVisibility.Append("false,true,false,");
                }
                else
                {
                    setColumnsVisibility.Append("false,false,false,");
                }
                budgetMain.ColumneditLevel = budgetMain.ForecastColName;
            }


            var MinBudgetid = 0; var MinParentid = 0;

            #region "Set Create/Edit or View permission for Budget and Forecast to Global varialble."
            _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            //IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView); 
            #endregion

            var dataTableMain = new DataTable();
            dataTableMain.Columns.Add("Id", typeof(Int32));
            dataTableMain.Columns.Add("ParentId", typeof(Int32));
            dataTableMain.Columns.Add("Name", typeof(String));
            dataTableMain.Columns.Add("AddRow", typeof(String));
            //  dataTableMain.Columns.Add("Selection", typeof(String));
            dataTableMain.Columns.Add("LineItemCount", typeof(Int32));

            #region set dynamic columns in dataTable
            var IntegerColumnValidation = new string[] { Enums.ColumnValidation.ValidInteger.ToString() };
            var DoubleColumnValidation = new string[] { Enums.ColumnValidation.ValidCurrency.ToString(), Enums.ColumnValidation.ValidNumeric.ToString() };
            //var StringColumnValidation = new string[] { Enums.ColumnValidation.CustomNameValid.ToString(), Enums.ColumnValidation.Empty.ToString(), 
            //    Enums.ColumnValidation.None.ToString(), Enums.ColumnValidation.NotEmpty.ToString(),
            //    Enums.ColumnValidation.ValidAplhaNumeric.ToString(),Enums.ColumnValidation.ValidEmail.ToString(),Enums.ColumnValidation.ValidSIN.ToString(),
            //    Enums.ColumnValidation.ValidIPv4.ToString()
            //};
            var boolColumnValidation = new string[] { Enums.ColumnValidation.ValidBoolean.ToString() };
            var DateTimeColumnValidation = new string[] { Enums.ColumnValidation.ValidDate.ToString(), Enums.ColumnValidation.ValidDatetime.ToString(),
                Enums.ColumnValidation.ValidTime.ToString() };

            // Add TimeFrame columns
            foreach (var timeFrameCol in objTimeFrameColumns)
            {
                Type ColumnType = typeof(List<String>);
                if (IntegerColumnValidation.Contains(timeFrameCol.ValidationType))
                {
                    ColumnType = typeof(List<int?>);
                }
                else if (DoubleColumnValidation.Contains(timeFrameCol.ValidationType))
                {
                    ColumnType = typeof(List<Double?>);
                }
                else if (boolColumnValidation.Contains(timeFrameCol.ValidationType))
                {
                    ColumnType = typeof(List<bool?>);
                }
                else if (DateTimeColumnValidation.Contains(timeFrameCol.ValidationType))
                {
                    ColumnType = typeof(List<DateTime?>);
                }

                dataTableMain.Columns.Add(Convert.ToString(timeFrameCol.CustomField.Name), ColumnType);
            }
            // Add Total columns of timeframe columns
            foreach (var timeFrameCol in objTimeFrameColumns)
            {
                Type ColumnType = typeof(String);
                // add total columns if the value is integer or double
                if (IntegerColumnValidation.Contains(timeFrameCol.ValidationType) || DoubleColumnValidation.Contains(timeFrameCol.ValidationType))
                {
                    if (IntegerColumnValidation.Contains(timeFrameCol.ValidationType))
                    {
                        ColumnType = typeof(int);
                    }
                    else if (DoubleColumnValidation.Contains(timeFrameCol.ValidationType))
                    {
                        ColumnType = typeof(Double);
                    }
                    dataTableMain.Columns.Add(Convert.ToString(timeFrameCol.CustomField.Name) + "Total", ColumnType);
                }
            }

            // Add Custom columns
            foreach (var custcol in objCustomColumns)
            {
                Type ColumnType = typeof(String);
                if (IntegerColumnValidation.Contains(custcol.ValidationType))
                {
                    ColumnType = typeof(int);
                }
                else if (DoubleColumnValidation.Contains(custcol.ValidationType))
                {
                    ColumnType = typeof(Double);
                }
                else if (boolColumnValidation.Contains(custcol.ValidationType))
                {
                    ColumnType = typeof(bool);
                }
                else if (DateTimeColumnValidation.Contains(custcol.ValidationType))
                {
                    ColumnType = typeof(DateTime);
                }

                dataTableMain.Columns.Add(Convert.ToString(custcol.CustomField.Name), ColumnType);
            }
            #endregion
            //dataTableMain.Columns.Add("Budget", typeof(List<Double?>));
            //dataTableMain.Columns.Add("ForeCast", typeof(List<Double?>));
            //dataTableMain.Columns.Add("Plan", typeof(List<Double?>));
            //dataTableMain.Columns.Add("Actual", typeof(List<Double?>));
            //dataTableMain.Columns.Add("BudgetTotal", typeof(Double));
            //dataTableMain.Columns.Add("ForeCastTotal", typeof(Double));
            //dataTableMain.Columns.Add("PlanTotal", typeof(Double));
            //dataTableMain.Columns.Add("ActualTotal", typeof(Double));
            dataTableMain.Columns.Add("lstLineItemIds", typeof(List<int>));

            #region Set Tree Grid Properties and methods
            setHeader.Append(",,,");// Default 1st 3 columns header
            attachHeader.Append("Task Name,,Line Items,");
            setInitWidths.Append("200,65,65,");
            setColAlign.Append("left,center,center,");
            setColValidators.Append(Enums.ColumnValidation.CustomNameValid.ToString() + ",,,");
            setColumnIds.Append("Title,,LineItems,");
            setColTypes.Append("tree,ro,ro,");


            #region Time Frame Columns
            if (objTimeFrameColumns != null)
            {
                //foreach (var timeframecol in objTimeFrameColumns)
                string HeaderPerfix = "";
                int loopStartLength = 1;
                int loopEndLength = 0;
                List<string> listQuarter = Enums.Quarters.Select(a => a.Key).ToList();
                if (IsQuaterly == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    loopEndLength = 4;
                    HeaderPerfix = "Q";
                }
                else if (IsQuaterly == Enums.PlanAllocatedBy.months.ToString())
                {
                    loopEndLength = 12;
                }
                else if (listQuarter.Contains(IsQuaterly))
                {
                    // here loop start length as month start and same as end month
                    if (IsQuaterly == Enums.QuarterWithSpace.Quarter1.ToString())
                    {
                        loopStartLength = 1;
                    }
                    else if (IsQuaterly == Enums.QuarterWithSpace.Quarter2.ToString())
                    {
                        loopStartLength = 4;
                    }
                    else if (IsQuaterly == Enums.QuarterWithSpace.Quarter3.ToString())
                    {
                        loopStartLength = 7;
                    }
                    else if (IsQuaterly == Enums.QuarterWithSpace.Quarter4.ToString())
                    {
                        loopStartLength = 10;
                    }
                    loopEndLength = loopStartLength + 2;

                }

                for (int j = loopStartLength; j <= loopEndLength; j++)
                {
                    if (IsQuaterly == Enums.PlanAllocatedBy.months.ToString() || listQuarter.Contains(IsQuaterly))
                    {
                        HeaderPerfix = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(j);
                        HeaderPerfix = HeaderPerfix.Substring(0, 3);
                        for (int i = 0; i < objTimeFrameColumns.Count; i++)
                        {
                            setHeader.Append(HeaderPerfix + ","); // Set month header
                            attachHeader.Append(Convert.ToString(objTimeFrameColumns[i].CustomField.Name + ",")); // set attach header or column title
                            setInitWidths.Append("65,");// set width of columns
                            setColAlign.Append("center,"); //set column allignment
                            setColValidators.Append((!string.IsNullOrEmpty(objTimeFrameColumns[i].ValidationType) ? (objTimeFrameColumns[i].ValidationType != Enums.ColumnValidation.None.ToString() ? objTimeFrameColumns[i].ValidationType : "") : "") + ","); // set column validation
                            if (listQuarter.Contains(IsQuaterly))
                            {
                                setColumnIds.Append(objTimeFrameColumns[i].CustomField.Name + i + j + ",");
                            }
                            else
                            {
                                setColumnIds.Append(objTimeFrameColumns[i].CustomField.Name + "M" + j + ",");
                            }

                            if (EditLevel == "Budget")
                            {
                                if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Budget && _IsBudgetCreate_Edit)
                                {
                                    if (EditPermission == "Edit")
                                    {
                                        setColTypes.Append("ed,");
                                    }
                                    else
                                    {
                                        setColTypes.Append("ro,");
                                    }
                                    
                                }
                                else
                                {
                                    setColTypes.Append("ro,");
                                }
                            }
                            else
                            {
                                if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && _IsForecastCreate_Edit)
                                {
                                    if (EditPermission == "Edit")
                                    {
                                        setColTypes.Append("ed,");
                                    }
                                    else
                                    {
                                        setColTypes.Append("ro,");
                                    }
                                  
                                }
                                else
                                {
                                    setColTypes.Append("ro,");
                                }
                            }

                            if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Budget || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Forecast || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Custom)
                            {
                                if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                                {
                                    setColumnsVisibility.Append("false,");
                                }
                                else
                                {
                                    setColumnsVisibility.Append("true,");
                                }
                            }
                            else
                            {
                                if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                                {
                                    setColumnsVisibility.Append("false,");
                                }
                                else
                                {
                                    setColumnsVisibility.Append("true,");
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < objTimeFrameColumns.Count; i++)
                        {
                            setHeader.Append(HeaderPerfix + j + ","); // Set Quarter header
                            attachHeader.Append(Convert.ToString(objTimeFrameColumns[i].CustomField.Name + ","));// set attach header or column title
                            setInitWidths.Append("65,");// set width of columns
                            setColAlign.Append("center,"); //set column allignment
                            setColValidators.Append((!string.IsNullOrEmpty(objTimeFrameColumns[i].ValidationType) ? (objTimeFrameColumns[i].ValidationType != Enums.ColumnValidation.None.ToString() ? objTimeFrameColumns[i].ValidationType : "") : "") + ","); // set column validation
                            setColumnIds.Append(objTimeFrameColumns[i].CustomField.Name + "Q" + j + ",");
                            if (EditLevel == "Budget")
                            {
                                if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Budget && _IsBudgetCreate_Edit)
                                {
                                    if (EditPermission == "Edit")
                                    {
                                        setColTypes.Append("ed,");
                                    }
                                    else
                                    {
                                        setColTypes.Append("ro,");
                                    }
                                }
                                else
                                {
                                    setColTypes.Append("ro,");
                                }
                            }
                            else
                            {
                                if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && _IsForecastCreate_Edit)
                                {
                                    if (EditPermission == "Edit")
                                    {
                                        setColTypes.Append("ed,");
                                    }
                                    else
                                    {
                                        setColTypes.Append("ro,");
                                    }
                                }
                                else
                                {
                                    setColTypes.Append("ro,");
                                }
                            }

                            if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Budget || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Forecast
                                || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Custom || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.None)
                            {
                                if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                                {
                                    setColumnsVisibility.Append("false,");
                                }
                                else
                                {
                                    setColumnsVisibility.Append("true,");
                                }
                            }
                            else
                            {
                                if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                                {
                                    setColumnsVisibility.Append("false,");
                                }
                                else
                                {
                                    setColumnsVisibility.Append("true,");
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < objTimeFrameColumns.Count; i++)
                {
                    setHeader.Append("Total,"); // Set Total as header
                    attachHeader.Append(Convert.ToString(objTimeFrameColumns[i].CustomField.Name + ","));// set attach header or column title
                    setInitWidths.Append("65,");// set width of columns
                    setColAlign.Append("center,"); //set column allignment
                    setColValidators.Append((!string.IsNullOrEmpty(objTimeFrameColumns[i].ValidationType) ? (objTimeFrameColumns[i].ValidationType != Enums.ColumnValidation.None.ToString() ? objTimeFrameColumns[i].ValidationType : "") : "") + ","); // set column validation
                    setColumnIds.Append(objTimeFrameColumns[i].CustomField.Name + "Total,");
                    setColTypes.Append("ro,");

                    if (objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Budget || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Forecast
                        || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.Custom || objTimeFrameColumns[i].ValueOnEditable == (int)Enums.ValueOnEditable.None)
                    {
                        if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                        {
                            setColumnsVisibility.Append("false,");
                        }
                        else
                        {
                            setColumnsVisibility.Append("true,");
                        }
                    }
                    else
                    {
                        if (Listofcheckedcol.Contains(objTimeFrameColumns[i].CustomFieldId.ToString()))
                        {
                            setColumnsVisibility.Append("false,");
                        }
                        else
                        {
                            setColumnsVisibility.Append("true,");
                        }
                    }
                }
            }
            #endregion

            #region Custom Columns
            // set header for custom field 
            if (objCustomColumns != null)
            {
                foreach (var custcol in objCustomColumns)
                {
                    setHeader.Append(Convert.ToString(custcol.CustomField.Name) + ",");
                    attachHeader.Append(Convert.ToString(custcol.CustomField.Name) + ",");
                    setColAlign.Append("center,"); //set column allignment
                    setColValidators.Append((!string.IsNullOrEmpty(custcol.ValidationType) ? (custcol.ValidationType != Enums.ColumnValidation.None.ToString() ? custcol.ValidationType : "") : "") + ","); // set column validation
                    setColumnIds.Append(custcol.CustomField.Name + ",");
                    setInitWidths.Append("65,");
                    if (EditLevel == "Budget")
                    {
                        if (custcol.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && _IsBudgetCreate_Edit)
                        {
                            if (EditPermission == "Edit")
                            {
                                setColTypes.Append("ed,");
                            }
                            else
                            {
                                setColTypes.Append("ro,");
                            }
                        }
                        else
                        {
                            setColTypes.Append("ro,");
                        }
                    }
                    else
                    {
                        if (custcol.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && _IsForecastCreate_Edit)
                        {
                            if (EditPermission == "Edit")
                            {
                                setColTypes.Append("ed,");
                            }
                            else
                            {
                                setColTypes.Append("ro,");
                            }
                        }
                        else
                        {
                            setColTypes.Append("ro,");
                        }
                    }

                    if (custcol.ValueOnEditable == (int)Enums.ValueOnEditable.Budget || custcol.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast
                        || custcol.ValueOnEditable == (int)Enums.ValueOnEditable.Custom || custcol.ValueOnEditable == (int)Enums.ValueOnEditable.None)
                    {
                        if (Listofcheckedcol.Contains(custcol.CustomFieldId.ToString()))
                        {
                            setColumnsVisibility.Append("false,");
                        }
                        else
                        {
                            setColumnsVisibility.Append("true,");
                        }
                    }
                    else
                    {
                        if (Listofcheckedcol.Contains(custcol.CustomFieldId.ToString()))
                        {
                            setColumnsVisibility.Append("false,");
                        }
                        else
                        {
                            setColumnsVisibility.Append("true,");
                        }
                    }
                }
            }
            #endregion

            string trimSetheader = setHeader.ToString().TrimEnd(',');
            string trimAttachheader = attachHeader.ToString().TrimEnd(',');
            string trimSetInitWidths = setInitWidths.ToString().TrimEnd(',');
            string trimSetColAlign = setColAlign.ToString().TrimEnd(',');
            string trimSetColValidators = setColValidators.ToString().TrimEnd(',');
            string trimSetColumnIds = setColumnIds.ToString().TrimEnd(',');
            string trimSetColTypes = setColTypes.ToString().TrimEnd(',');
            string trimSetColumnsVisibility = setColumnsVisibility.ToString().TrimEnd(',');

            #endregion

            //var Query = db.Budget_Detail.Where(a => a.BudgetId == (BudgetId > 0 ? BudgetId : a.BudgetId)).Select(a => new { a.Id, a.ParentId, a.Name }).ToList();
            var varBudgetIds = db.Budget_Detail.Where(a => a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId) && a.IsDeleted == false).Select(a => new { a.BudgetId, a.ParentId }).FirstOrDefault();
            List<Budget_Detail> BudgetDetailList = new List<Budget_Detail>();
            if (varBudgetIds != null)
            {
                BudgetDetailList = db.Budget_Detail.Where(a => (a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId) || a.ParentId == (BudgetId > 0 ? BudgetId : a.ParentId)
                   || a.BudgetId == (varBudgetIds.BudgetId != null ? varBudgetIds.BudgetId : 0)) && a.IsDeleted == false).Select(a => a).ToList();
            }
            //MinParentid = BudgetId;
            List<int> BudgetDetailids = BudgetDetailList.Select(a => a.Id).ToList();
            List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();

            // Change By Nishant Sheth
            List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();
            List<int> LineItemids = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => PlanLineItemBudgetDetail.Contains(a.PlanLineItemId) && a.IsDeleted == false).Select(a => a.PlanLineItemId).ToList(); ;


            var Query = BudgetDetailList.Select(a => new { a.Id, a.ParentId, a.Name }).ToList();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            List<Budget_DetailAmount> BudgetDetailAmount = db.Budget_DetailAmount.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            //List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
            // #1590 Changes Observation:5 - Nishant Sheth
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in db.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                 //join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Cost.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                 where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                 select Cost).ToList();
            //List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
            // #1590 Changes Observation:9 - Nishant Sheth
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in db.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                     //join LineItemBudget in db.LineItem_Budget on Actual.PlanLineItemId equals LineItemBudget.PlanLineItemId
                                                                                     join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     join Tactic in db.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                     where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                     && tacticStatus.Contains(Tactic.Status)
                                                                                     select Actual).ToList();

            // foreach (var item in Query)
            var columnNames = dataTableMain.Columns.Cast<DataColumn>()
                    .Select(x => x)
                    .ToList();
            int OtherBudgetid = Common.GetOtherBudgetId();
            var DefaultColumnList = Enums.DefaultGridColumnValues.Select(a => a.Key).ToList();
            var CustomCoulmnsId = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && a.IsTimeFrame == false
                && a.MapTableName == Enums.MapTableName.CustomField_Entity.ToString()).Select(a => a.CustomFieldId).ToList();

            var CustomColumnsValue = db.CustomField_Entity.Where(a => CustomCoulmnsId.Contains(a.CustomFieldId)).Select(a => new { a.Value, a.CustomFieldId, a.CustomFieldEntityId, a.EntityId }).ToList();

            Query.ForEach(
                item =>
                {
                    BudgetAmount objBudgetAmount = new BudgetAmount();
                    List<int> PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == item.Id && LineItemids.Contains(a.PlanLineItemId)).Select(a => a.PlanLineItemId).ToList();

                    if (varBudgetIds.ParentId != null)
                    {
                        if (item.Id != varBudgetIds.ParentId && item.ParentId != null)
                        {
                            #region when child budget selectd
                            var BudgetDetailAmountValue = BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList();
                            var PlanDetailAmountValue = PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList();
                            var ActualDetailAmountValue = ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList();
                            var LineItemBudhgetListValue = LineItemidBudgetList.Where(l => l.BudgetDetailId == item.Id).ToList();
                            objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmountValue, PlanDetailAmountValue, ActualDetailAmountValue, LineItemBudhgetListValue);
                            //dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>" : ""), PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum(), PlanLineItemsId });
                            string Addrow = (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? (EditPermission == "Edit" ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>" : "") : "");
                            Dictionary<string, object> variables = new Dictionary<string, object>();

                            DataRow row = dataTableMain.NewRow();

                            row[Enums.DefaultGridColumn.Id.ToString()] = (Int32)item.Id;

                            row[Enums.DefaultGridColumn.ParentId.ToString()] = item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId);

                            row[Enums.DefaultGridColumn.Name.ToString()] = item.Name;

                            row[Enums.DefaultGridColumn.AddRow.ToString()] = Addrow;

                            row[Enums.DefaultGridColumn.LineItemCount.ToString()] = PlanLineItemsId.Count();

                            foreach (var col in objColumns)
                            {
                                string colname = col.CustomField.Name;

                                if (!DefaultColumnList.Contains(col.CustomField.Name))
                                {

                                    if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Budget;
                                    }
                                    else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                    {
                                        row[colname] = objBudgetAmount.ForeCast;
                                    }
                                    else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Plan;
                                    }
                                    else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Actual;
                                    }
                                    else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && col.IsTimeFrame == false && col.MapTableName == Enums.MapTableName.CustomField_Entity.ToString())
                                    {
                                        var CustomValue = CustomColumnsValue.Where(a => a.EntityId == item.Id && a.CustomFieldId == col.CustomFieldId).Select(a => a.Value).FirstOrDefault();
                                        row[colname] = CustomValue != null ? CustomValue : "0";
                                    }
                                }
                            }
                            foreach (var colTotal in objColumns)
                            {
                                string colname = colTotal.CustomField.Name + "Total";

                                if (!DefaultColumnList.Contains(colTotal.CustomField.Name))
                                {
                                    if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && colTotal.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Budget.Sum() > 0 ? objBudgetAmount.Budget.Sum() : 0;
                                    }
                                    else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && colTotal.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                    {
                                        row[colname] = objBudgetAmount.ForeCast.Sum() > 0 ? objBudgetAmount.ForeCast.Sum() : 0;
                                    }
                                    else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.None && colTotal.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Plan.Sum() > 0 ? objBudgetAmount.Plan.Sum() : 0;
                                    }
                                    else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.None && colTotal.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString())
                                    {
                                        row[colname] = objBudgetAmount.Actual.Sum() > 0 ? objBudgetAmount.Actual.Sum() : 0;
                                    }
                                }

                            }
                            row[Enums.DefaultGridColumn.lstLineItemIds.ToString()] = PlanLineItemsId;

                            dataTableMain.Rows.Add(row);
                            //dataTableMain.AcceptChanges();

                            //dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "'  onclick='CheckboxClick(this)' class='grid_Delete'></div>" : ""), PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum(), PlanLineItemsId });
                            #endregion
                        }
                    }
                    else
                    {
                        #region When Parent Budget seelcted
                        var BudgetDetailAmountValue = BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList();
                        var PlanDetailAmountValue = PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList();
                        var ActualDetailAmountValue = ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList();
                        var LineItemBudhgetListValue = LineItemidBudgetList.Where(l => l.BudgetDetailId == item.Id).ToList();
                        objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmountValue, PlanDetailAmountValue, ActualDetailAmountValue, LineItemBudhgetListValue);
                        //dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>" : ""), PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum(), PlanLineItemsId });
                        string Addrow = (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? (EditPermission == "Edit" ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "' onclick='CheckboxClick(this)' class='grid_Delete'></div>" : "") : "");
                        Dictionary<string, object> variables = new Dictionary<string, object>();

                        DataRow row = dataTableMain.NewRow();

                        row[Enums.DefaultGridColumn.Id.ToString()] = (Int32)item.Id;

                        row[Enums.DefaultGridColumn.ParentId.ToString()] = item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId);

                        row[Enums.DefaultGridColumn.Name.ToString()] = item.Name;

                        row[Enums.DefaultGridColumn.AddRow.ToString()] = Addrow;

                        row[Enums.DefaultGridColumn.LineItemCount.ToString()] = PlanLineItemsId.Count();

                        foreach (var col in objColumns)
                        {
                            string colname = col.CustomField.Name;

                            if (!DefaultColumnList.Contains(col.CustomField.Name))
                            {

                                if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    row[colname] = objBudgetAmount.Budget;
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    row[colname] = objBudgetAmount.ForeCast;
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString())
                                {
                                    row[colname] = objBudgetAmount.Plan;
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.None && col.IsTimeFrame == true && col.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString())
                                {
                                    row[colname] = objBudgetAmount.Actual;
                                }
                                else if (col.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && col.IsTimeFrame == false && col.MapTableName == Enums.MapTableName.CustomField_Entity.ToString())
                                {
                                    var CustomValue = CustomColumnsValue.Where(a => a.EntityId == item.Id && a.CustomFieldId == col.CustomFieldId).Select(a => a.Value).FirstOrDefault();
                                    row[colname] = CustomValue != null ? CustomValue : "0";
                                }
                            }
                        }
                        foreach (var colTotal in objColumns)
                        {
                            string colname = colTotal.CustomField.Name + "Total";

                            if (!DefaultColumnList.Contains(colTotal.CustomField.Name))
                            {
                                if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && colTotal.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    row[colname] = objBudgetAmount.Budget.Sum() > 0 ? objBudgetAmount.Budget.Sum() : 0;
                                }
                                else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && colTotal.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString())
                                {
                                    row[colname] = objBudgetAmount.ForeCast.Sum() > 0 ? objBudgetAmount.ForeCast.Sum() : 0;
                                }
                                else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.None && colTotal.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString())
                                {
                                    row[colname] = objBudgetAmount.Plan.Sum() > 0 ? objBudgetAmount.Plan.Sum() : 0;
                                }
                                else if (colTotal.ValueOnEditable == (int)Enums.ValueOnEditable.None && colTotal.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString())
                                {
                                    row[colname] = objBudgetAmount.Actual.Sum() > 0 ? objBudgetAmount.Actual.Sum() : 0;
                                }
                            }

                        }
                        row[Enums.DefaultGridColumn.lstLineItemIds.ToString()] = PlanLineItemsId;

                        dataTableMain.Rows.Add(row);
                        //objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), LineItemidBudgetList.Where(l => l.BudgetDetailId == item.Id).ToList());
                        //dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, (Convert.ToString(item.Id) != Convert.ToString(OtherBudgetid) ? "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='finance_grid_add' style='float:none !important' parentId='" + (item.ParentId.HasValue ? item.ParentId.ToString() : Convert.ToString(0)) + "'></div><div  id='cb" + item.Id + "' row-id='" + item.Id + "' Name='" + HttpUtility.HtmlEncode(item.Name) + "'  onclick='CheckboxClick(this)' class='grid_Delete'></div>" : ""), PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum(), PlanLineItemsId });
                        #endregion
                    }
                });


            var items = GetTopLevelRows(dataTableMain, MinParentid)
                        .Select(row => CreateItem(dataTableMain, row, EditLevel, columnNames, objColumns))
                        .ToList();

            var temp = items.Where(a => a.id == Convert.ToString(BudgetId)).Select(a => a.FinanemodelheaderObj).FirstOrDefault();

            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            if (temp != null)
            {
                objFinanceHeader.Budget = Convert.ToDouble(temp.Budget);
                objFinanceHeader.Forecast = Convert.ToDouble(temp.Forecast);
                objFinanceHeader.Planned = Convert.ToDouble(temp.Planned);
                objFinanceHeader.Actual = Convert.ToDouble(temp.Actual);
            }
            else
            {
                objFinanceHeader.Budget = 0;
                objFinanceHeader.Forecast = 0;
                objFinanceHeader.Planned = 0;
                objFinanceHeader.Actual = 0;
            }
            objFinanceHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            TempData["FinanceHeader"] = objFinanceHeader;

            budgetMain.rows = items;
            budgetMain.setHeader = trimSetheader;
            budgetMain.attachHeader = trimAttachheader;
            budgetMain.setInitWidths = trimSetInitWidths;
            budgetMain.setColAlign = trimSetColAlign;
            budgetMain.setColValidators = trimSetColValidators;
            budgetMain.setColumnIds = trimSetColumnIds;
            budgetMain.setColTypes = trimSetColTypes;
            budgetMain.setColumnsVisibility = trimSetColumnsVisibility;
            budgetMain.CustColumnsList = objCustomColumns.Select(a => a.CustomField.Name).ToList();
            return Json(budgetMain, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditBudget(int BudgetId = 0, string level = "", string EditPermission = "")
        {
            FinanceModel objFinanceModel = new FinanceModel();
            ViewBag.BudgetId = BudgetId;
            ViewBag.EditLevel = level;
            ViewBag.EditPermission = EditPermission;
            ViewBag.IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            //ViewBag.IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            ViewBag.IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);
            return PartialView("_EditBudget", objFinanceModel);
        }

        public JsonResult ListOfBudgetName()
        {
            var ListOfBudgetName = db.Budgets.Where(a => a.IsDeleted == false).Select(a => a.Name.ToLower()).ToList();
            return Json(ListOfBudgetName, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetListofForecastNames(int BudgetId)
        {
            var budgeparentids = db.Budgets.Where(m => m.ClientId == Sessions.User.ClientId && (m.IsDeleted == false || m.IsDeleted == null)).Select(m => m.Id).ToList();
            int? ParentId = 0;
            var checkParent = db.Budget_Detail.Where(a => a.Id == BudgetId && (a.IsDeleted == false || a.IsDeleted == false)).Select(a => a.ParentId).ToList();
            ParentId = checkParent.Count > 0 ? checkParent[0] : 0;

            var ListofForecastNames = db.Budget_Detail.Where(a => (ParentId > 0 ? a.ParentId == (ParentId != null ? ParentId : null) : a.ParentId == null) && budgeparentids.Contains(a.BudgetId) && (a.IsDeleted == false || a.IsDeleted == false) && !string.IsNullOrEmpty(a.Name)).Select(a => a.Name.ToLower()).ToList();
            return Json(ListofForecastNames, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Declarion of Budget/Forecast for Parent Child List

        IEnumerable<DataRow> GetChildren(DataTable dataTable, Int32 parentId)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Int32>("ParentId") == parentId);
        }

        double? GetSumofPeriodValue(DataTable datatable, Int32 Id, int index = 0, string ColumnName = "")
        {
            double? Sum = 0;
            var ChildList = datatable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("ParentId") == Id)
                 .Select(chld => new { Id = chld.Field<Int32>("Id"), Value = chld.Field<List<Double?>>(ColumnName)[index] }).ToList();
            if (ChildList.Count > 0)
            {
                foreach (var Child in ChildList)
                {
                    Sum += GetSumofPeriodValue(datatable, Child.Id, index, ColumnName);
                }
            }
            else
            {
                Sum += datatable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("Id") == Id)
                 .Sum(chld => chld.Field<List<Double?>>(ColumnName)[index]);
            }

            return Sum;
        }

        double? GetSumofValue(DataTable datatable, Int32 Id, string ColumnName = "")
        {
            double? Sum = 0;
            var ChildList = datatable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("ParentId") == Id)
                 .Select(chld => new { Id = chld.Field<Int32>("Id"), Value = chld.Field<Double>(ColumnName) }).ToList();

            if (ChildList.Count > 0)
            {
                foreach (var Child in ChildList)
                {
                    Sum += GetSumofValue(datatable, Child.Id, ColumnName);
                }
            }
            else
            {
                Sum += datatable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("Id") == Id)
                 .Sum(chld => chld.Field<Double>(ColumnName));
            }

            return Sum;
        }
        double? GetSumofValueMainGrid(DataTable datatable, Int32 Id, string ColumnName = "")
        {
            double? Sum = 0;
            var ChildList = datatable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("ParentId") == Id)
                 .Select(chld => new { Id = chld.Field<Int32>("Id"), Value = chld.Field<String>(ColumnName) }).ToList();

            if (ChildList.Count > 0)
            {
                foreach (var Child in ChildList)
                {
                    Sum += Convert.ToDouble(GetSumofValueMainGrid(datatable, Child.Id, ColumnName));
                }
            }
            else
            {
                var SumofParent = datatable
                            .Rows
                            .Cast<DataRow>()
                            .Where(rw => rw.Field<Int32>("Id") == Id)
                            .Sum(chld => Convert.ToDouble(chld.Field<String>(ColumnName))).ToString();

                Sum += Convert.ToDouble(SumofParent);
            }

            return Sum;
        }
        DhtmlxGridRowDataModel CreateItem(DataTable dataTable, DataRow row, string EditLevel, List<DataColumn> DataTablecolumnNames, List<Budget_Columns> objColumns)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>();
            //var columnNames = dataTable.Columns.Cast<DataColumn>()
            //         .Select(x => new { x.ColumnName, x.DataType })
            //         .ToList();

            var BudgetCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.Budget).Select(a => a.CustomField.Name).FirstOrDefault();
            var ForecastCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast).Select(a => a.CustomField.Name).FirstOrDefault();
            var PlannedCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Cost.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.None).Select(a => a.CustomField.Name).FirstOrDefault();
            var ActualCol = objColumns.Where(a => a.IsTimeFrame == true && a.MapTableName == Enums.MapTableName.Plan_Campaign_Program_Tactic_LineItem_Actual.ToString()
                && a.ValueOnEditable == (int)Enums.ValueOnEditable.None).Select(a => a.CustomField.Name).FirstOrDefault();

            var BudgetTotalCol = DataTablecolumnNames.Where(a => a.ColumnName == Convert.ToString(BudgetCol) + "Total").Select(a => a.ColumnName).FirstOrDefault();
            var ForecastTotalCol = DataTablecolumnNames.Where(a => a.ColumnName == Convert.ToString(ForecastCol) + "Total").Select(a => a.ColumnName).FirstOrDefault();
            var PlannedTotalCol = DataTablecolumnNames.Where(a => a.ColumnName == Convert.ToString(PlannedCol) + "Total").Select(a => a.ColumnName).FirstOrDefault();
            var ActualTotalCol = DataTablecolumnNames.Where(a => a.ColumnName == Convert.ToString(ActualCol) + "Total").Select(a => a.ColumnName).FirstOrDefault();

            foreach (var col in DataTablecolumnNames)
            {
                variables.Add(col.ColumnName.ToString(), row[col.ColumnName.ToString()]);
            }
            var id = variables.Where(a => a.Key == Enums.DefaultGridColumn.Id.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();
            var name = variables.Where(a => a.Key == Enums.DefaultGridColumn.Name.ToString()).Select(a => Convert.ToString(a.Value)).FirstOrDefault();
            var addRow = variables.Where(a => a.Key == Enums.DefaultGridColumn.AddRow.ToString()).Select(a => Convert.ToString(a.Value)).FirstOrDefault();
            //  var SelectBox = row.Field<String>("Selection");
            var lineitemcount = variables.Where(a => a.Key == Enums.DefaultGridColumn.LineItemCount.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();
            var parentid = variables.Where(a => a.Key == Enums.DefaultGridColumn.ParentId.ToString()).Select(a => int.Parse(a.Value.ToString())).FirstOrDefault();

            var budget = variables.Where(a => a.Key == BudgetCol).Select(a => (List<Double?>)a.Value).ToList();
            var forcast = variables.Where(a => a.Key == ForecastCol).Select(a => (List<Double?>)a.Value).ToList();
            var plan = variables.Where(a => a.Key == PlannedCol).Select(a => (List<Double?>)a.Value).ToList();
            var actual = variables.Where(a => a.Key == ActualCol).Select(a => (List<Double?>)a.Value).ToList();

            var budgetTotal = variables.Where(a => a.Key == BudgetTotalCol).Select(a => Double.Parse(a.Value.ToString())).FirstOrDefault();
            var forcastTotal = variables.Where(a => a.Key == ForecastTotalCol).Select(a => Double.Parse(a.Value.ToString())).FirstOrDefault();
            var plantotal = variables.Where(a => a.Key == PlannedTotalCol).Select(a => Double.Parse(a.Value.ToString())).FirstOrDefault();
            var actualtotal = variables.Where(a => a.Key == ActualTotalCol).Select(a => Double.Parse(a.Value.ToString())).FirstOrDefault();
            List<int> lstLineItemIds = row.Field<List<int>>("lstLineItemIds");
            //variables.Where(a => a.Key == Enums.DefaultGridColumn.lstLineItemIds.ToString()).Select(a => int.Parse(a.Value.ToString())).ToList();

            var lstChildren = GetChildren(dataTable, id);
            if (lstChildren != null && lstChildren.Count() > 0)
                lstChildren = lstChildren.OrderBy(child => child.Field<String>("Name")).ToList();
            var children = lstChildren
              .Select(r => CreateItem(dataTable, r, EditLevel, DataTablecolumnNames, objColumns))
              .ToList();
            userdata objuserData = new userdata();
            List<row_attrs> rows_attrData = new List<row_attrs>();
            //objuserData.Add(new userdata { idwithName = "parent_" + Convert.ToString(id) });
            //objuserData.Add(new userdata { row_attrs = "parent_" + Convert.ToString(id) });

            List<string> ParentData = new List<string>();
            ParentData.Add(HttpUtility.HtmlDecode(name));
            string strAddRow, strLineItemLink;
            strAddRow = strLineItemLink = string.Empty;
            int rwcount = dataTable != null ? dataTable.Rows.Count : 0;
            if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
            {
                string IsTitleEdit = "1";

                #region "Get LineItem Count"
                var LineItemIds = dataTable
                         .Rows
                         .Cast<DataRow>()
                                                .Where(rw => rw.Field<Int32>("ParentId") == id).Select(chld => chld.Field<List<int>>("lstLineItemIds")).ToList();
                if (lstLineItemIds == null)
                    lstLineItemIds = new List<int>();

                LineItemIds.ForEach(lstIds => lstLineItemIds.AddRange(lstIds));

                row.SetField<List<int>>("lstLineItemIds", lstLineItemIds);
                lineitemcount = lstLineItemIds != null ? lstLineItemIds.Distinct().Count() : 0;
                row.SetField<Int32>("LineItemCount", lineitemcount); // Update LineItemCount in DataTable. 
                #endregion


                if (EditLevel.ToUpper().Equals("BUDGET"))
                {
                    if (!_IsBudgetCreate_Edit)  // If user has not Create/Edit Budget permission then clear AddRow button Html.
                    {
                        addRow = string.Empty;
                        //   SelectBox = string.Empty;
                        IsTitleEdit = "0";
                    }
                    strLineItemLink = lineitemcount.ToString();
                }
                else
                {
                    if (!_IsForecastCreate_Edit)    // If user has not Create/Edit Forecast permission then clear AddRow button Html.
                    {
                        addRow = string.Empty;
                        //  SelectBox = string.Empty;
                        IsTitleEdit = "0";
                    }
                    if (rwcount.Equals(1))
                        strLineItemLink = string.Format("<div onclick='LoadLineItemGrid({0})' class='finance_lineItemlink'>{1}</div>", id.ToString(), lineitemcount.ToString());
                    else
                        strLineItemLink = lineitemcount.ToString();
                }




                //lineitemcount = dataTable
                // .Rows
                // .Cast<DataRow>()
                // .Where(rw => rw.Field<Int32>("Id") == id).Sum(chld => chld.Field<Int32>("LineItemCount"));
                //row.SetField<Int32>("LineItemCount", lineitemcount); // Update LineItemCount in DataTable.
                if (rwcount == 1 && lstChildren.Count() == 0)
                {
                    objuserData = (new userdata { id = Convert.ToString(id), idwithName = "parent_" + Convert.ToString(id), row_attrs = "parent_" + Convert.ToString(id), row_locked = "0", isTitleEdit = IsTitleEdit });
                }
                else
                {
                    objuserData = (new userdata { id = Convert.ToString(id), idwithName = "parent_" + Convert.ToString(id), row_attrs = "parent_" + Convert.ToString(id), row_locked = "1", isTitleEdit = IsTitleEdit });
                }

                //ParentData.Add(Convert.ToString(lineitemcount));
            }
            else
            {
                if (!_IsForecastCreate_Edit)    // If user has not Create/Edit Forecast permission then clear AddRow button Html.
                {
                    addRow = string.Empty;
                    objuserData = (new userdata { id = Convert.ToString(id), idwithName = "parent_" + Convert.ToString(id), row_attrs = "parent_" + Convert.ToString(id), row_locked = "0", isTitleEdit = "0" });
                }
                else
                    objuserData = (new userdata { id = Convert.ToString(id), idwithName = "parent_" + Convert.ToString(id), row_attrs = "parent_" + Convert.ToString(id), row_locked = "0", isTitleEdit = "1" });
                strLineItemLink = string.Format("<div onclick='LoadLineItemGrid({0})' class='finance_lineItemlink'>{1}</div>", id.ToString(), lineitemcount.ToString());
            }
            ParentData.Add(addRow);
            // ParentData.Add(SelectBox);
            ParentData.Add(strLineItemLink);
            //ParentData.Add(string.Join(",", budget));
            //ParentData.Add(string.Join(",", forcast));

            int i = 0;
            int TimeFrameLoopLength = budget != null && budget.Count > 0 ? budget[0].Count : (forcast != null && forcast.Count > 0 ? forcast[0].Count : (
                plan != null && plan.Count > 0 ? plan[0].Count : (actual != null && actual.Count > 0 ? actual[0].Count : 0)
                ));

            for (i = 0; i < TimeFrameLoopLength; i++)
            {
                //#,#0.##
                //0:0.##
                if (budget.Count > 0)
                {
                    if (budget[0].Count > 0)
                    {
                        ParentData.Add(String.Format("{0:#,##0.##}", Convert.ToDouble(Convert.ToString(budget[0][i]))));
                    }
                }
                if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
                {
                    //var CheckIsparentZero=dataTable
                    //    .Rows
                    //    .Cast<DataRow>()
                    //    .Where(rw => rw.Field<Int32>("Id") == id).Select(a=>a.Field<List<Int32?>>)
                    if (forcast.Count > 0)
                    {
                        if (forcast[0].Count > 0)
                        {
                            var tempforcast = GetSumofPeriodValue(dataTable, id, i, ForecastCol);
                            ParentData.Add(Convert.ToString(tempforcast.Value.ToString(formatThousand)));
                        }
                    }

                    if (plan.Count > 0)
                    {
                        if (plan[0].Count > 0)
                        {
                            var tempPlan = GetSumofPeriodValue(dataTable, id, i, PlannedCol);
                            ParentData.Add(Convert.ToString(tempPlan.Value.ToString(formatThousand)));
                        }
                    }

                    if (actual.Count > 0)
                    {
                        if (actual[0].Count > 0)
                        {
                            var tempActual = GetSumofPeriodValue(dataTable, id, i, ActualCol);
                            ParentData.Add(Convert.ToString(tempActual.Value.ToString(formatThousand)));
                        }
                    }

                    //ParentData.Add(Convert.ToString(forcast[i]));
                }
                else
                {
                    if (forcast.Count > 0)
                    {
                        if (forcast[0].Count > 0)
                        {
                            ParentData.Add(String.Format("{0:#,##0.##}", Convert.ToDouble(Convert.ToString(forcast[0][i]))));
                        }
                    }
                    if (plan.Count > 0)
                    {
                        if (plan[0].Count > 0)
                        {
                            ParentData.Add(String.Format("{0:#,##0.##}", Convert.ToDouble(Convert.ToString(plan[0][i]))));
                        }
                    }
                    if (actual.Count > 0)
                    {
                        if (actual[0].Count > 0)
                        {
                            ParentData.Add(String.Format("{0:#,##0.##}", Convert.ToDouble(Convert.ToString(actual[0][i]))));
                        }
                    }
                }

            }
            FinanceModelHeaders objHeader = new FinanceModelHeaders();
            objHeader.BudgetTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Budget.ToString()].ToString();
            objHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            if (budget.Count > 0)
            {
                if (budget[0].Count > 0)
                {
                    ParentData.Add(Convert.ToString(budgetTotal.ToString(formatThousand)));
                    objHeader.Budget = budgetTotal;
                }
            }
            if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))
            {
                if (forcast.Count > 0)
                {
                    if (forcast[0].Count > 0)
                    {
                        var tempforcastTotal = GetSumofValue(dataTable, id, ForecastTotalCol);
                        ParentData.Add(Convert.ToString(tempforcastTotal.Value.ToString(formatThousand)));
                        objHeader.Forecast = Convert.ToDouble(tempforcastTotal);
                    }
                }
                if (plan.Count > 0)
                {
                    if (plan[0].Count > 0)
                    {
                        var tempPlanTotal = GetSumofValue(dataTable, id, PlannedTotalCol);
                        ParentData.Add(Convert.ToString(tempPlanTotal.Value.ToString(formatThousand)));
                        objHeader.Planned = Convert.ToDouble(tempPlanTotal);
                    }
                }
                if (actual.Count > 0)
                {
                    if (actual[0].Count > 0)
                    {
                        var tempActualTotal = GetSumofValue(dataTable, id, ActualTotalCol);
                        ParentData.Add(Convert.ToString(tempActualTotal.Value.ToString(formatThousand)));
                        objHeader.Actual = Convert.ToDouble(tempActualTotal);
                    }
                }

            }
            else
            {
                if (forcast.Count > 0)
                {
                    if (forcast[0].Count > 0)
                    {
                        ParentData.Add(Convert.ToString(forcastTotal.ToString(formatThousand)));
                        objHeader.Forecast = Convert.ToDouble(forcastTotal);
                    }
                }
                if (plan.Count > 0)
                {
                    if (plan[0].Count > 0)
                    {
                        ParentData.Add(Convert.ToString(plantotal.ToString(formatThousand)));
                        objHeader.Planned = Convert.ToDouble(plantotal);
                    }
                }
                if (actual.Count > 0)
                {
                    if (actual[0].Count > 0)
                    {
                        ParentData.Add(Convert.ToString(actualtotal.ToString(formatThousand)));
                        objHeader.Actual = Convert.ToDouble(actualtotal);
                    }
                }
            }
            var CustColList = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Custom && a.IsTimeFrame == false && a.MapTableName == Enums.MapTableName.CustomField_Entity.ToString()).ToList();
            foreach (var custCol in CustColList)
            {
                var custval = variables.Where(a => a.Key == custCol.CustomField.Name).Select(a => a.Value).FirstOrDefault();
                ParentData.Add(Convert.ToString(custval));
            }

            //List<userdata> objuserData = new List<userdata>();
            //List<row_attrs> rows_attrData = new List<row_attrs>();
            //objuserData.Add(new userdata { id = Convert.ToString(id) });
            //objuserData.Add(new userdata { idwithName = "parent_" + Convert.ToString(id) });
            //objuserData.Add(new userdata { row_attrs = "parent_" + Convert.ToString(id) });

            rows_attrData.Add(new row_attrs { id = Convert.ToString(id) });
            //return new FinanceParentChildModel { Id = id, Name = name, Children = children, Budget = budget, ForeCast = forcast, BudgetTotal = budgetTotal, ForeCastTotal = forcastTotal };
            return new DhtmlxGridRowDataModel { id = Convert.ToString(id), data = ParentData, rows = children, userdata = objuserData, row_attrs = rows_attrData, FinanemodelheaderObj = objHeader };
        }

        IEnumerable<DataRow> GetTopLevelRows(DataTable dataTable, int minParentId = 0)
        {
            return dataTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Int32>("ParentId") == minParentId);
        }

        #endregion

        #region Get Budget/ForeCast/Plan/Actual Value
        public BudgetAmount GetAmountValue(string isQuaterly, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount, List<LineItem_Budget> LineItemidBudgetList)
        {
            #region Declartion
            BudgetAmount objbudget = new BudgetAmount();
            List<double?> _budgetlist = new List<double?>();
            List<double?> _forecastlist = new List<double?>();
            List<double?> _planlist = new List<double?>();
            List<double?> _actuallist = new List<double?>();
            int currentEndMonth = 12;
            double? _Budget = 0, _ForeCast = 0, _Plan = 0, _Actual = 0;
            #endregion

            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

            List<string> _curentBudget = new List<string>();
            List<string> _curentForeCast = new List<string>();
            List<string> _curentPlan = new List<string>();
            List<string> _curentActual = new List<string>();

            List<string> _commonQuarters = new List<string>();

            List<int> LineItemIds = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

            if (isQuaterly == Enums.PlanAllocatedBy.quarters.ToString())
            {
                #region "Get Quarter list based on loop value"
                for (int i = 1; i <= 4; i++)
                {
                    if (i == 1)
                    {
                        _curentBudget = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 2)
                    {
                        _curentBudget = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 3)
                    {
                        _curentBudget = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 4)
                    {
                        _curentBudget = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where _curentPlan.Contains(plandetail.Period)
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where _curentActual.Contains(actualdetail.Period)
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    _Budget = Budget_DetailAmountList.Where(a => _curentBudget.Contains(a.Period)).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => _curentForeCast.Contains(a.Period)).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    //_Plan = (PlanDetailAmount.Where(a => _curentPlan.Contains(a.Period)).Sum(a => a.Value) * weightage.Value) / 100;
                    _planlist.Add(_Plan);

                    //_Actual = (ActualDetailAmount.Where(a => _curentActual.Contains(a.Period)).Sum(a => a.Value) * weightage.Value) / 100;
                    _actuallist.Add(_Actual);
                }
                #endregion
            }
            else if (isQuaterly == Enums.PlanAllocatedBy.months.ToString())
            {
                for (int i = 1; i <= 12; i++)
                {
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where plandetail.Period == PeriodPrefix + i.ToString()
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where actualdetail.Period == PeriodPrefix + i.ToString()
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    _Budget = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    //_Plan = (PlanDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value) * weightage.Value) / 100;
                    _planlist.Add(_Plan);

                    //_Actual = (ActualDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value) * weightage.Value) / 100;
                    _actuallist.Add(_Actual);
                }
            }
            else
            {
                if (isQuaterly == Enums.QuarterWithSpace.Quarter1.ToString())
                {
                    _commonQuarters = Q1;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter2.ToString())
                {
                    _commonQuarters = Q2;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter3.ToString())
                {
                    _commonQuarters = Q3;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter4.ToString())
                {
                    _commonQuarters = Q4;
                }
                foreach (var item in _commonQuarters)
                {
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where plandetail.Period == item
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where actualdetail.Period == item
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    _Budget = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    //_Plan = (PlanDetailAmount.Where(a => a.Period == item).Sum(a => a.Value) * weightage.Value) / 100;
                    _planlist.Add(_Plan);

                    //_Actual = (ActualDetailAmount.Where(a => a.Period == item).Sum(a => a.Value) * weightage.Value) / 100;
                    _actuallist.Add(_Actual);
                }
            }
            objbudget.Budget = _budgetlist;
            objbudget.ForeCast = _forecastlist;
            objbudget.Plan = _planlist;
            objbudget.Actual = _actuallist;
            return objbudget;
        }
        public BudgetAmount GetMainGridAmountValue(bool isQuaterly, string strTimeFrame, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount, List<LineItem_Budget> LineItemidBudgetList)
        {
            #region Declartion
            BudgetAmount objbudget = new BudgetAmount();
            List<double?> _budgetlist = new List<double?>();
            List<double?> _forecastlist = new List<double?>();
            List<double?> _planlist = new List<double?>();
            List<double?> _actuallist = new List<double?>();
            int currentEndMonth = 12;
            double? _Budget = 0, _ForeCast = 0, _Plan = 0, _Actual = 0;
            #endregion

            if (isQuaterly)
            {
                List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
                List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
                List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
                List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

                List<string> _curentBudget = new List<string>();
                List<string> _curentForeCast = new List<string>();
                List<string> _curentPlan = new List<string>();
                List<string> _curentActual = new List<string>();

                int startIndex = 0, endIndex = 0;
                if (strTimeFrame.Equals(Enums.QuarterFinance.Quarter1.ToString()))
                    startIndex = endIndex = 1;
                else if (strTimeFrame.Equals(Enums.QuarterFinance.Quarter2.ToString()))
                    startIndex = endIndex = 2;
                else if (strTimeFrame.Equals(Enums.QuarterFinance.Quarter3.ToString()))
                    startIndex = endIndex = 3;
                else if (strTimeFrame.Equals(Enums.QuarterFinance.Quarter4.ToString()))
                    startIndex = endIndex = 4;
                else
                {
                    startIndex = 1; endIndex = 4;
                }

                #region "Get Quarter list based on loop value"
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (i == 1)
                    {
                        _curentBudget = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 2)
                    {
                        _curentBudget = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 3)
                    {
                        _curentBudget = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 4)
                    {
                        _curentBudget = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    _Budget = Budget_DetailAmountList.Where(a => _curentBudget.Contains(a.Period)).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => _curentForeCast.Contains(a.Period)).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where _curentPlan.Contains(plandetail.Period)
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where _curentActual.Contains(actualdetail.Period)
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    //_Plan = PlanDetailAmount.Where(a => _curentPlan.Contains(a.Period)).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    //_Actual = ActualDetailAmount.Where(a => _curentActual.Contains(a.Period)).Sum(a => a.Value);
                    _actuallist.Add(_Actual);
                }
                #endregion
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    _Budget = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where plandetail.Period == PeriodPrefix + i.ToString()
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where actualdetail.Period == PeriodPrefix + i.ToString()
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    //_Plan = PlanDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    //_Actual = ActualDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
                    _actuallist.Add(_Actual);
                }
            }
            objbudget.Budget = _budgetlist;
            objbudget.ForeCast = _forecastlist;
            objbudget.Plan = _planlist;
            objbudget.Actual = _actuallist;
            return objbudget;
        }
        #endregion

        #region Update Forecast/Budget Data
        [HttpPost]
        public ActionResult UpdateBudgetGridData(int BudgetId = 0, string IsQuaterly = "quarters", string nValue = "0", string oValue = "0", string ColumnName = "", string Period = "", int ParentRowId = 0, string GlobalEditLevel = "", bool isFromForecastChild = false)
        {
            Budget_DetailAmount objBudAmount = new Budget_DetailAmount();
            nValue = HttpUtility.HtmlDecode(nValue.Trim());

            List<Budget_Columns> objColumns = (from ColumnSet in db.Budget_ColumnSet
                                               join Columns in db.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                                               where ColumnSet.IsDeleted == false && Columns.IsDeleted == false
                                               && ColumnSet.ClientId == Sessions.User.ClientId
                                               select Columns).ToList();
            var objCustomColumns = objColumns.Where(a => a.IsTimeFrame == false).Select(a => a).ToList();
            var objTimeFrameColumns = objColumns.Where(a => a.IsTimeFrame == true).Select(a => a).ToList();

            var CustomCol = objCustomColumns.Where(a => a.CustomField.Name == ColumnName.Trim()).Select(a => a).FirstOrDefault();
            string BudgetColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && a.IsDeleted == false && a.IsTimeFrame == true
                   && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            string ForecastColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && a.IsDeleted == false && a.IsTimeFrame == true
                    && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
            if (ColumnName == "Task Name")
            {
                Budget objBudget = new Budget();
                Budget_Detail objBudgetDetail = new Budget_Detail();

                if (BudgetId == 0 && ParentRowId == 0 && !string.IsNullOrEmpty(nValue))
                {
                    SaveNewBudget(nValue);
                }
                else
                {
                    objBudgetDetail = db.Budget_Detail.Where(a => a.Id == BudgetId && a.IsDeleted == false).FirstOrDefault();
                    if (objBudgetDetail != null)
                    {
                        objBudgetDetail.Name = Convert.ToString(nValue).Trim();
                        db.Entry(objBudgetDetail).State = EntityState.Modified;
                        db.SaveChanges();
                        //if ((objBudgetDetail.BudgetId > 0) && ((objBudgetDetail.ParentId == null) || (objBudgetDetail.ParentId == 0)))
                        if (objBudgetDetail.ParentId == null)
                        {
                            objBudget = db.Budgets.Where(a => a.Id == objBudgetDetail.BudgetId).FirstOrDefault();
                            if (objBudget != null)
                            {
                                objBudget.Name = nValue;
                                db.Entry(objBudget).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        Budget_Detail DetailParentId = db.Budget_Detail.Where(a => a.Id == ParentRowId).FirstOrDefault();
                        if (GlobalEditLevel == "ForeCast" && isFromForecastChild)
                        {
                            DetailParentId.IsForecast = true;
                            db.Entry(DetailParentId).State = EntityState.Modified;
                        }
                        objBudgetDetail = new Budget_Detail();
                        objBudgetDetail.Name = nValue;
                        objBudgetDetail.ParentId = Convert.ToInt32(ParentRowId);
                        objBudgetDetail.BudgetId = Convert.ToInt32(DetailParentId.BudgetId);
                        objBudgetDetail.CreatedDate = System.DateTime.Now;
                        objBudgetDetail.CreatedBy = Sessions.User.UserId;
                        objBudgetDetail.IsDeleted = false;
                        db.Entry(objBudgetDetail).State = EntityState.Added;
                        db.SaveChanges();
                        int _budgetid = objBudgetDetail.Id;
                        SaveUserBudgetpermission(_budgetid);

                        #region Udate LineItem with child item
                        //Add By Nishant Sheth
                        int? BudgetDetailParentid = objBudgetDetail.ParentId;
                        int BudgetDetailId = objBudgetDetail.Id;

                        using (MRPEntities context = new MRPEntities())
                        {
                            IQueryable<LineItem_Budget> objLineItem = context.LineItem_Budget
                                .Where(x => x.BudgetDetailId == BudgetDetailParentid);
                            foreach (LineItem_Budget LineItem in objLineItem)
                            {
                                LineItem.BudgetDetailId = BudgetDetailId;
                            }
                            context.SaveChanges();
                        }
                        #endregion
                    }
                }
            }
            List<string> QuaterPeriod = new List<string>();
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            string dataPeriod = "";
            var ParentDetails = db.Budget_Detail.Where(a => a.Id == BudgetId && a.IsDeleted == false).Select(a => new { a.Id, a.ParentId }).FirstOrDefault();
            if (IsQuaterly == Enums.PlanAllocatedBy.quarters.ToString())
            {
                if (Period == "Q1")
                {
                    dataPeriod = "Y1";
                    QuaterPeriod = Q1;
                    objBudAmount = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && Q1.Contains(a.Period)).FirstOrDefault();
                }
                else if (Period == "Q2")
                {
                    dataPeriod = "Y4";
                    QuaterPeriod = Q2;
                    objBudAmount = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && Q2.Contains(a.Period)).FirstOrDefault();
                }
                else if (Period == "Q3")
                {
                    dataPeriod = "Y7";
                    QuaterPeriod = Q3;
                    objBudAmount = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && Q3.Contains(a.Period)).FirstOrDefault();
                }
                else if (Period == "Q4")
                {
                    dataPeriod = "Y10";
                    QuaterPeriod = Q4;
                    objBudAmount = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && Q4.Contains(a.Period)).FirstOrDefault();
                }
            }
            else
            {
                //ActualPeriod = Period;
                var s = Enums.ReportMonthDisplayValuesWithPeriod.Where(a => a.Key == Period).Select(a => a.Value).FirstOrDefault();
                Period = s;
                dataPeriod = Period;
                objBudAmount = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && a.Period == Period).FirstOrDefault();
            }



            //if (IsQuaterly)
            //{
            //    if (Period == "Q1")
            //    {
            //        dataPeriod = "Y1";
            //    }
            //    else if (Period == "Q2")
            //    {
            //        dataPeriod = "Y4";
            //    }
            //    else if (Period == "Q3")
            //    {
            //        dataPeriod = "Y7";
            //    }
            //    else if (Period == "Q4")
            //    {
            //        dataPeriod = "Y10";
            //    }
            //}
            //else
            //{
            //    var s = Enums.ReportMonthDisplayValuesWithPeriod.Where(a => a.Key == Period).Select(a => a.Value).FirstOrDefault();
            //    dataPeriod = s;
            //}

            if (objBudAmount == null)
            {
                objBudAmount = new Budget_DetailAmount();
                if (ColumnName != "Task Name")
                {
                    if (ColumnName == BudgetColName)
                    {
                        objBudAmount.Budget = Convert.ToDouble(nValue);
                    }
                    else if (ColumnName == ForecastColName)
                    {
                        objBudAmount.Forecast = Convert.ToDouble(nValue);
                    }
                    if (BudgetId > 0)
                    {
                        objBudAmount.Period = dataPeriod;
                        objBudAmount.BudgetDetailId = BudgetId;
                        db.Entry(objBudAmount).State = EntityState.Added;
                        db.SaveChanges();
                    }

                }
            }
            else
            {
                if (IsQuaterly == Enums.PlanAllocatedBy.quarters.ToString())
                {
                    List<Budget_DetailAmount> BudgetAmountList = new List<Budget_DetailAmount>();
                    BudgetAmountList = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && QuaterPeriod.Contains(a.Period)).OrderBy(a => a.Period).ToList();
                    double? QuaterSum = 0;
                    double? Maindiff = 0;
                    double? FirstOld = 0, SecondOld = 0, ThirdOld = 0;
                    double? FirstNew = 0, SecondNew = 0, ThirdNew = 0;
                    if (BudgetAmountList.Count > 0)
                    {
                        QuaterSum = (ColumnName == BudgetColName ? Convert.ToDouble(BudgetAmountList.Select(a => a.Budget).Sum()) : Convert.ToDouble(BudgetAmountList.Select(a => a.Forecast).Sum()));
                        FirstNew = FirstOld = Convert.ToDouble(ColumnName == BudgetColName ? BudgetAmountList[0].Budget : BudgetAmountList[0].Forecast);
                        SecondNew = SecondOld = Convert.ToDouble(BudgetAmountList.Count >= 2 ? (ColumnName == BudgetColName ? BudgetAmountList[1].Budget : BudgetAmountList[1].Forecast) : 0);
                        ThirdNew = ThirdOld = Convert.ToDouble(BudgetAmountList.Count >= 3 ? (ColumnName == BudgetColName ? BudgetAmountList[2].Budget : BudgetAmountList[2].Forecast) : 0);

                        if (Convert.ToDouble(nValue) > QuaterSum)
                        {
                            Maindiff = Convert.ToDouble(nValue) - QuaterSum;
                            //FirstOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[0].Budget : BudgetAmountList[0].Forecast);
                            FirstNew = FirstOld + Maindiff;
                        }
                        else if (Convert.ToDouble(nValue) < QuaterSum)
                        {
                            Maindiff = QuaterSum - Convert.ToDouble(nValue);
                            if (BudgetAmountList.Count >= 3)
                            {
                                //ThirdOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[2].Budget : BudgetAmountList[2].Forecast);
                                if (ThirdOld >= Maindiff)
                                {
                                    ThirdNew = ThirdOld - Maindiff;
                                }
                                else if (Maindiff >= ThirdOld)
                                {
                                    //SecondOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[1].Budget : BudgetAmountList[1].Forecast);
                                    ThirdNew = 0;
                                    double? SecondDiff = Maindiff - ThirdOld;
                                    if (SecondDiff >= SecondOld)
                                    {
                                        SecondNew = SecondDiff - SecondOld;
                                        if (SecondNew >= SecondOld)
                                        {
                                            SecondNew = 0;
                                        }
                                    }
                                    else if (SecondOld >= SecondDiff)
                                    {
                                        SecondNew = SecondOld - SecondDiff;
                                    }

                                    if (SecondNew <= 0)
                                    {
                                        //double? FirstDiff = 0;
                                        //if (FirstOld >= SecondDiff)
                                        //{
                                        //    FirstDiff = FirstOld - SecondDiff;
                                        //}
                                        //else
                                        //{
                                        //    FirstDiff = SecondDiff - FirstOld;
                                        //}

                                        FirstNew = Convert.ToDouble(nValue);
                                    }

                                }
                            }
                            else if (BudgetAmountList.Count >= 2)
                            {
                                //SecondOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[1].Budget : BudgetAmountList[1].Forecast);
                                if (SecondOld >= Maindiff)
                                {
                                    SecondNew = SecondOld - Maindiff;
                                }
                                else if (Maindiff >= SecondOld)
                                {
                                    //FirstOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[0].Budget : BudgetAmountList[0].Forecast);
                                    SecondNew = 0;
                                    double? FirstDiff = Maindiff - SecondOld;
                                    if (FirstDiff >= FirstOld)
                                    {
                                        FirstNew = FirstDiff - FirstOld;
                                        if (FirstNew <= FirstOld)
                                        {
                                            FirstNew = 0;
                                        }
                                    }
                                    else if (FirstOld >= FirstDiff)
                                    {
                                        FirstNew = FirstOld - FirstDiff;

                                    }

                                    if (FirstNew <= 0)
                                    {
                                        FirstNew = Convert.ToDouble(nValue);
                                    }
                                }
                            }
                            else
                            {
                                FirstNew = Convert.ToDouble(nValue);
                            }
                        }
                    }

                    int Count = 1;
                    foreach (var data in BudgetAmountList.OrderBy(a => a.Period.Replace(PeriodPrefix, "")))
                    {

                        Budget_DetailAmount objBudAmountUpdate = db.Budget_DetailAmount.Where(a => a.Id == data.Id).FirstOrDefault();
                        if (Count == 1)
                        {
                            if (ColumnName == BudgetColName)
                            { objBudAmountUpdate.Budget = FirstNew; }
                            else if (ColumnName == ForecastColName)
                            { objBudAmountUpdate.Forecast = FirstNew; }
                        }
                        if (Count == 2)
                        {
                            if (ColumnName == BudgetColName)
                            { objBudAmountUpdate.Budget = SecondNew; }
                            else if (ColumnName == ForecastColName)
                            { objBudAmountUpdate.Forecast = SecondNew; }
                        }
                        if (Count == 3)
                        {
                            if (ColumnName == BudgetColName)
                            { objBudAmountUpdate.Budget = ThirdNew; }
                            else if (ColumnName == ForecastColName)
                            { objBudAmountUpdate.Forecast = ThirdNew; }
                        }
                        db.Entry(objBudAmountUpdate).State = EntityState.Modified;
                        db.SaveChanges();

                        Count++;
                    }
                }
                else
                {
                    Budget_DetailAmount objBudAmountUpdate = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && a.Period == dataPeriod).FirstOrDefault();
                    if (objBudAmountUpdate != null)
                    {
                        if (ColumnName != "Task Name")
                        {
                            if (ColumnName == BudgetColName)
                            { objBudAmountUpdate.Budget = Convert.ToDouble(nValue); }
                            else if (ColumnName == ForecastColName)
                            { objBudAmountUpdate.Forecast = Convert.ToDouble(nValue); }
                            db.Entry(objBudAmountUpdate).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                if (CustomCol != null)
                {
                    if (ColumnName == CustomCol.CustomField.Name)
                    {
                        CustomField_Entity objCustomFieldEnity = new CustomField_Entity();
                        objCustomFieldEnity = db.CustomField_Entity.Where(a => a.EntityId == (BudgetId != null ? BudgetId : 0) && a.CustomFieldId == CustomCol.CustomFieldId).FirstOrDefault();
                        if (objCustomFieldEnity != null)
                        {
                            objCustomFieldEnity.Value = nValue;
                            db.Entry(objCustomFieldEnity).State = EntityState.Modified;
                        }
                        else
                        {
                            objCustomFieldEnity = new CustomField_Entity();
                            objCustomFieldEnity.CustomFieldId = CustomCol.CustomFieldId;
                            objCustomFieldEnity.EntityId = (BudgetId != null ? BudgetId : 0);
                            objCustomFieldEnity.Value = nValue;
                            objCustomFieldEnity.CreatedBy = Sessions.User.UserId;
                            objCustomFieldEnity.CreatedDate = System.DateTime.Now;
                            db.Entry(objCustomFieldEnity).State = EntityState.Added;
                        }
                        db.SaveChanges();
                    }
                }
            }
            return Json(new { errormsg = "" });
        }
        #endregion

        #region "Finance LineItem Grid related Methods"
        public ActionResult GetFinanceLineItemData(int BudgetDetailId = 0, string IsQuaterly = "quarters")
        {
            DhtmlXGridRowModel lineItemGridData = new DhtmlXGridRowModel();

            #region "Set Create/Edit or View permission for Budget and Forecast to Global varialble."
            _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            //IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView); 
            #endregion
            //var Query = db.Budget_Detail.Where(a => a.BudgetId == (BudgetId > 0 ? BudgetId : a.BudgetId)).Select(a => new { a.Id, a.ParentId, a.Name }).ToList();
            //var varBudgetIds = db.Budget_Detail.Where(a => a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId) && a.IsDeleted == false).Select(a => new { a.BudgetId, a.ParentId }).FirstOrDefault();
            //List<Budget_Detail> BudgetDetailList = new List<Budget_Detail>();
            //if (varBudgetIds != null)
            //{
            //    BudgetDetailList = db.Budget_Detail.Where(a => (a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId) || a.ParentId == (BudgetId > 0 ? BudgetId : a.ParentId)
            //       || a.BudgetId == (varBudgetIds.BudgetId != null ? varBudgetIds.BudgetId : 0)) && a.IsDeleted == false).Select(a => a).ToList();
            //}
            //MinParentid = BudgetId;
            Budget_Detail objBudgetDetail = db.Budget_Detail.Where(a => a.Id == (BudgetDetailId > 0 ? BudgetDetailId : a.BudgetId) && a.IsDeleted == false).FirstOrDefault();
            List<LineItem_Budget> LineItemidBudgetList = new List<LineItem_Budget>();
            if (objBudgetDetail == null)
                objBudgetDetail = new Budget_Detail();

            LineItemidBudgetList = db.LineItem_Budget.Where(a => objBudgetDetail.Id == a.BudgetDetailId).Select(a => a).ToList();

            //// Change By Nishant Sheth
            List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem> lstPlanLineItems = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => PlanLineItemBudgetDetail.Contains(a.PlanLineItemId) && a.IsDeleted == false).ToList();
            List<int> LineItemids = lstPlanLineItems != null ? lstPlanLineItems.Select(line => line.PlanLineItemId).ToList() : new List<int>();

            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            List<Budget_DetailAmount> BudgetDetailAmount = db.Budget_DetailAmount.Where(a => objBudgetDetail.Id == a.BudgetDetailId).Select(a => a).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in db.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                 //join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Cost.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                 where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                 select Cost).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in db.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                     //join LineItemBudget in db.LineItem_Budget on Actual.PlanLineItemId equals LineItemBudget.PlanLineItemId
                                                                                     join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     join Tactic in db.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                     where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                     && tacticStatus.Contains(Tactic.Status)
                                                                                     select Actual).ToList();

            #region "Create Child Data Model"

            #region "Declare local variables"
            List<DhtmlxGridRowDataModel> childModelList = new List<DhtmlxGridRowDataModel>();
            DhtmlxGridRowDataModel childModel = new DhtmlxGridRowDataModel();
            List<string> childData = new List<string>();
            BudgetAmount objBudgetAmount;
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstPlannedValue;
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstActualValue;
            List<LineItem_Budget> lstLineItemBudget;
            double planVal = 0, actualVal = 0, forecstVal = 0, totalPlanned = 0, totalActual = 0;
            string strlineItemPopupUrl;
            int planId;
            #endregion

            #region "Foreach LineItem list to set Value into Model"
            foreach (Plan_Campaign_Program_Tactic_LineItem lineitem in lstPlanLineItems)
            {
                totalPlanned = totalActual = 0;
                childModel = new DhtmlxGridRowDataModel();
                childData = new List<string>();
                planId = 0;
                strlineItemPopupUrl = string.Empty;
                // Name,View,Forecast,Planned,Actual
                childData.Add(HttpUtility.HtmlDecode(lineitem.Title));
                planId = lineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId;
                strlineItemPopupUrl = GetNotificationURLbyStatus(planId, lineitem.PlanLineItemId, Enums.Section.LineItem.ToString());
                childData.Add(string.Format("<div id='{0}' onclick=OpenLineItemPopup('{1}') title='View' class='grid_Search'></div>", lineitem.PlanLineItemId, HttpUtility.HtmlDecode(strlineItemPopupUrl)));

                #region "Filter Planned Value"
                lstPlannedValue = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                lstPlannedValue = PlanDetailAmount.Where(plan => plan.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Filter Actual Value"
                lstActualValue = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                lstActualValue = ActualDetailAmount.Where(actual => actual.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Filter LineItem_Budget List"
                lstLineItemBudget = new List<LineItem_Budget>();
                lstLineItemBudget = LineItemidBudgetList.Where(line => line.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Get Planned & Actual value based on Timeframe selected value"
                objBudgetAmount = new BudgetAmount();
                objBudgetAmount = GetAmountValue(IsQuaterly, new List<Budget_DetailAmount>(), lstPlannedValue, lstActualValue, lstLineItemBudget);
                #endregion

                if (objBudgetAmount != null)
                {
                    #region "Insert data as per timeframe wise"
                    int timeframeRowCount = objBudgetAmount.Plan != null ? objBudgetAmount.Plan.Count : 0;
                    for (int row = 0; row < timeframeRowCount; row++)
                    {
                        planVal = objBudgetAmount.Plan[row] != null ? Convert.ToDouble(objBudgetAmount.Plan[row]) : 0;
                        actualVal = objBudgetAmount.Actual[row] != null ? Convert.ToDouble(objBudgetAmount.Actual[row]) : 0;
                        totalPlanned = totalPlanned + planVal;
                        totalActual = totalActual + actualVal;
                        childData.Add("<div style='color: gray;'>0</div>"); // Insert Forecast Column Value
                        childData.Add(planVal.ToString(formatThousand));    // Insert Planned Column Value
                        childData.Add(actualVal.ToString(formatThousand));  // Insert Actual Column Value
                    }
                    #endregion
                }

                #region "Add Child Items Total values to Model"
                childData.Add("<div style='color: gray;'>0</div>");
                childData.Add(totalPlanned.ToString(formatThousand));    // Insert Planned Column Total Value
                childData.Add(totalActual.ToString(formatThousand));  // Insert Actual Column Total Value 
                #endregion

                #region "Set LinetItem Value in Model"
                childModel.id = lineitem.PlanLineItemId.ToString();
                childModel.data = childData;
                childModelList.Add(childModel);
                #endregion
            }
            #endregion

            #endregion

            #region "Create Parent Data Model"
            List<DhtmlxGridRowDataModel> parentModelList = new List<DhtmlxGridRowDataModel>();
            List<string> parentData = new List<string>();
            DhtmlxGridRowDataModel parentDataModel = new DhtmlxGridRowDataModel();
            double totalParentForecast = 0, totalParentPlanned = 0, totalParentActual = 0;
            parentData.Add(HttpUtility.HtmlDecode(objBudgetDetail.Name));   // Set TaskName column value
            parentData.Add(string.Empty);           // Set View column value
            objBudgetAmount = new BudgetAmount();
            objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount, LineItemidBudgetList);

            if (objBudgetAmount != null)
            {
                #region "Insert data as per timeframe wise"
                int timeframeRowCount = objBudgetAmount.Plan != null ? objBudgetAmount.Plan.Count : 0;

                for (int row = 0; row < timeframeRowCount; row++)
                {
                    forecstVal = objBudgetAmount.ForeCast[row] != null ? Convert.ToDouble(objBudgetAmount.ForeCast[row]) : 0;
                    planVal = objBudgetAmount.Plan[row] != null ? Convert.ToDouble(objBudgetAmount.Plan[row]) : 0;
                    actualVal = objBudgetAmount.Actual[row] != null ? Convert.ToDouble(objBudgetAmount.Actual[row]) : 0;

                    totalParentForecast = totalParentForecast + forecstVal;
                    totalParentPlanned = totalParentPlanned + planVal;
                    totalParentActual = totalParentActual + actualVal;

                    parentData.Add(forecstVal.ToString(formatThousand)); // Insert Forecast Column Value
                    parentData.Add(planVal.ToString(formatThousand));    // Insert Planned Column Value
                    parentData.Add(actualVal.ToString(formatThousand));  // Insert Actual Column Value
                }
                #endregion
            }

            #region "Add Child Items Total values to Model"
            parentData.Add(totalParentForecast.ToString(formatThousand));   // Insert Forecast Column Total Value
            parentData.Add(totalParentPlanned.ToString(formatThousand));    // Insert Planned Column Total Value
            parentData.Add(totalParentActual.ToString(formatThousand));  // Insert Actual Column Total Value 
            #endregion

            parentDataModel.id = BudgetDetailId.ToString();
            parentDataModel.data = parentData;
            parentDataModel.rows = childModelList;
            parentModelList.Add(parentDataModel);

            ViewBag.HasLineItems = childModelList != null && childModelList.Count > 0 ? true : false;

            #endregion

            //DhtmlxGridRowDataModel  objResultModel = new DhtmlxGridRowDataModel { id = Convert.ToString(id), data = ParentData, rows = children, userdata = objuserData, row_attrs = rows_attrData };

            var temp = parentModelList.Where(a => a.id == Convert.ToString(BudgetDetailId)).Select(a => a.data).FirstOrDefault();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            if (temp != null)
            {
                objFinanceHeader.Budget = 0;
                objFinanceHeader.Forecast = Convert.ToDouble(temp[temp.Count - 3]);
                objFinanceHeader.Planned = Convert.ToDouble(temp[temp.Count - 2]);
                objFinanceHeader.Actual = Convert.ToDouble(temp[temp.Count - 1]);
            }
            else
            {
                objFinanceHeader.Budget = objFinanceHeader.Forecast = objFinanceHeader.Planned = objFinanceHeader.Actual = 0;
            }
            objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();
            //TempData["FinanceHeader"] = objFinanceHeader;
            TempData["FinanceHeader"] = new FinanceModelHeaders();

            lineItemGridData.rows = parentModelList;
            lineItemGridData.FinanemodelheaderObj = objFinanceHeader;
            return PartialView("_LineItem", lineItemGridData);
        }

        public JsonResult GetParentLineItemList(int BudgetDetailId = 0)
        {
            LineItemDropdownModel objParentDDLModel = new LineItemDropdownModel();
            objParentDDLModel = Common.GetParentLineItemBudgetDetailslist(BudgetDetailId);
            return Json(objParentDDLModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetChildLineItemList(int BudgetDetailId = 0)
        {
            var lstchildbudget = Common.GetChildLineItemBudgetDetailslist(BudgetDetailId);
            return Json(lstchildbudget, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Function to get Notification URL.
        /// Added By: Viral Kadiya on 10/26/2015.
        /// </summary>
        /// <param name="planId">Plan Id.</param>
        /// <param name="planTacticId">Plan Tactic Id.</param>
        /// <param name="section">Section.</param>
        /// <returns>Return NotificationURL.</returns>
        public string GetNotificationURLbyStatus(int planId = 0, int entityId = 0, string section = "")
        {
            string strURL = string.Empty;
            try
            {
                if (section == Convert.ToString(Enums.Section.LineItem))
                    strURL = Url.Action("Index", "Home", new { currentPlanId = planId, planLineItemId = entityId, activeMenu = "Plan" }, Request.Url.Scheme);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
            return strURL;
        }

        #endregion
    }
}
