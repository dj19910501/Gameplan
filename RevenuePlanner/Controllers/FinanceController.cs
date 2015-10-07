using System;
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

namespace RevenuePlanner.Controllers
{
    public class FinanceController : Controller
    {
        //
        // GET: /Finance/
        private MRPEntities db = new MRPEntities();
        private const string PeriodPrefix = "Y";
        private const string formatThousand = "#,#0.##";
        private bool _IsBudgetCreate_Edit = true;
        private bool _IsForecastCreate_Edit = true;
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
            return PartialView("_newBudget");
        }
        public ActionResult CreateNewBudget(string budgetName)
        {
            int budgetId = 0;
            try
            {
                Budget objBudget = new Budget();
                objBudget.ClientId = Sessions.User.ClientId;
                objBudget.Name = budgetName;
                objBudget.Desc = "Test Budget Description";
                objBudget.CreatedBy = Sessions.User.UserId;
                objBudget.CreatedDate = DateTime.Now;
                objBudget.IsDeleted = false;
                db.Entry(objBudget).State = EntityState.Added;
                db.SaveChanges();
                budgetId = objBudget.Id;

                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail.BudgetId = budgetId;
                objBudgetDetail.Name = budgetName;
                //objBudgetDetail.ParentId = ;
                objBudgetDetail.CreatedBy = Sessions.User.UserId;
                objBudgetDetail.CreatedDate = DateTime.Now;
                db.Entry(objBudgetDetail).State = EntityState.Added;
                db.SaveChanges();
                return RefreshMainGridData(budgetId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public JsonResult RefreshBudgetList()
        {
            var budgetList = Common.GetBudgetlist();
            return Json(budgetList, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region  Main Grid Realted Methods
        public ActionResult RefreshMainGridData(int budgetId = 0, string mainTimeFrame = "Yearly")
        {
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();

            gridRowModel = GetFinanceMainGridData(budgetId, mainTimeFrame);
            var DetailId = db.Budget_Detail.Where(a => a.BudgetId == budgetId && a.ParentId == null).Select(a => a.Id).FirstOrDefault();
            if (DetailId != null)
            {
                var temp = gridRowModel.rows.Where(a => a.Detailid == Convert.ToString(DetailId)).Select(a => a.data).FirstOrDefault();
                if (temp != null)
                {
                    objFinanceHeader.Budget = Math.Round(Convert.ToDouble(temp[2]), 2);
                    objFinanceHeader.Forecast = Math.Round(Convert.ToDouble(temp[3]), 2);
                    objFinanceHeader.Planned = Math.Round(Convert.ToDouble(temp[4]), 2);
                    objFinanceHeader.Actual = Math.Round(Convert.ToDouble(temp[5]), 2);
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
            return PartialView("_MainGrid", gridRowModel);
        }
        public ActionResult SaveNewBudgetDetail(string BudgetId, string BudgetDetailName, string ParentId, string mainTimeFrame = "Yearly")
        {
            int _budgetId = 0;
            try
            {
                _budgetId = !string.IsNullOrEmpty(BudgetId) ? Int32.Parse(BudgetId) : 0;
                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail.BudgetId = _budgetId;
                objBudgetDetail.Name = BudgetDetailName;
                objBudgetDetail.ParentId = !string.IsNullOrEmpty(ParentId) ? Int32.Parse(ParentId) : 0;
                objBudgetDetail.CreatedBy = Sessions.User.UserId;
                objBudgetDetail.CreatedDate = DateTime.Now;
                db.Entry(objBudgetDetail).State = EntityState.Added;
                db.SaveChanges();
                return RefreshMainGridData(_budgetId, mainTimeFrame);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private DhtmlXGridRowModel GetFinanceMainGridData(int budgetId, string mainTimeFrame = "Yearly")
        {
            DhtmlXGridRowModel mainGridData = new DhtmlXGridRowModel();
            List<DhtmlxGridRowDataModel> gridRowData = new List<DhtmlxGridRowDataModel>();
            try
            {
                _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
                //IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
                _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
                //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);

                #region "GetFinancial Parent-Child Data"

                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(Int32));
                dataTable.Columns.Add("ParentId", typeof(Int32));
                dataTable.Columns.Add("RowId", typeof(String));
                dataTable.Columns.Add("Name", typeof(String));
                dataTable.Columns.Add("AddRow", typeof(String));
                dataTable.Columns.Add("Budget", typeof(String));
                dataTable.Columns.Add("Forecast", typeof(String));
                dataTable.Columns.Add("Planned", typeof(String));
                dataTable.Columns.Add("Actual", typeof(String));
                dataTable.Columns.Add("Action", typeof(String));
                dataTable.Columns.Add("LineItemCount", typeof(Int32));
                //budgetId = 8;
                var lstBudgetDetails = db.Budget_Detail.Where(bdgt => bdgt.Budget.ClientId.Equals(Sessions.User.ClientId) && bdgt.Budget.IsDeleted == false && bdgt.BudgetId.Equals(budgetId)).Select(a => new { a.Id, a.ParentId, a.Name }).ToList();

                List<int> lstBudgetDetailsIds = lstBudgetDetails.Select(bdgtdtls => bdgtdtls.Id).ToList();
                List<Budget_DetailAmount> BudgetDetailAmount = new List<Budget_DetailAmount>();
                BudgetDetailAmount = db.Budget_DetailAmount.Where(dtlAmnt => lstBudgetDetailsIds.Contains(dtlAmnt.BudgetDetailId)).ToList();

                List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => lstBudgetDetailsIds.Contains(a.BudgetDetailId)).Select(a => a).ToList();
                List<int> LineItemids = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

                List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
                List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();

                string rowId = string.Empty;
                List<int> PlanLineItemsId;
                BudgetAmount objBudgetAmount;
                bool isQuarterly = false;
                if (!string.IsNullOrEmpty(mainTimeFrame))
                {
                    if (!mainTimeFrame.Equals(Enums.QuarterFinance.Yearly.ToString()))
                        isQuarterly = true;
                }
                lstBudgetDetails.ForEach(
                    i =>
                    {
                        rowId = Regex.Replace(i.Name.Trim().Replace("_", ""), @"[^0-9a-zA-Z]+", "") + "_" + i.Id.ToString() + "_" + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        objBudgetAmount = new BudgetAmount();
                        PlanLineItemsId = new List<int>();
                        PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == i.Id).Select(a => a.PlanLineItemId).ToList();
                        objBudgetAmount = GetMainGridAmountValue(isQuarterly, mainTimeFrame, BudgetDetailAmount.Where(a => a.BudgetDetailId == i.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList());
                        //rowId = Regex.Replace(i.Name.Trim(), @"\s+", "") + i.Id.ToString() + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        //dataTable.Rows.Add(new Object[] { i.Id, i.ParentId == null ? 0 : i.ParentId, rowId, i.Name, "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row' />", objBudgetAmount.Budget.Sum().Value.ToString(formatThousand), objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand), objBudgetAmount.Plan.Sum().Value.ToString(formatThousand), objBudgetAmount.Actual.Sum().Value.ToString(formatThousand), "", PlanLineItemsId.Count });
                        dataTable.Rows.Add(new Object[] { i.Id, i.ParentId == null ? 0 : i.ParentId, rowId, i.Name, string.Empty, objBudgetAmount.Budget.Sum().Value.ToString(formatThousand), objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand), objBudgetAmount.Plan.Sum().Value.ToString(formatThousand), objBudgetAmount.Actual.Sum().Value.ToString(formatThousand), "", PlanLineItemsId.Count });
                    });

                var MinParentid = 0;
                List<DhtmlxGridRowDataModel> lstData = new List<DhtmlxGridRowDataModel>();
                lstData = GetTopLevelRows(dataTable, MinParentid)
                            .Select(row => CreateMainGridItem(dataTable, row)).ToList();

                #endregion

                mainGridData.rows = lstData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return mainGridData;
        }
        private DhtmlxGridRowDataModel CreateMainGridItem(DataTable dataTable, DataRow row)
        {
            var id = row.Field<Int32>("Id");
            string rowId = row.Field<String>("RowId");
            var name = row.Field<String>("Name");
            var addRow = row.Field<String>("AddRow");
            var budget = row.Field<String>("Budget");
            var forecast = row.Field<String>("Forecast");
            var planned = row.Field<String>("Planned");
            var actual = row.Field<String>("Actual");
            var lineItemCount = row.Field<Int32>("LineItemCount");
            //var action = row.Field<String>("Action");
            //var budget = row.Field<List<Double?>>("Budget");
            //var forcast = row.Field<List<Double?>>("ForeCast");
            //var budgetTotal = row.Field<Double>("BudgetTotal");
            //var forcastTotal = row.Field<Double>("ForeCastTotal");
            var lstChildren = GetChildren(dataTable, id);
            var children = lstChildren
              .Select(r => CreateMainGridItem(dataTable, r))
              .ToList();
            List<string> ParentData = new List<string>();


            int rwcount = dataTable != null ? dataTable.Rows.Count : 0;
            // Change By Nishant Sheth
            //if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
            //{
            //    forecast = Convert.ToString(GetSumofValueMainGrid(dataTable, id, "Forecast"));

            //    planned = Convert.ToString(GetSumofValueMainGrid(dataTable, id, "Planned"));

            //    actual = Convert.ToString(GetSumofValueMainGrid(dataTable, id, "Actual"));

            //    ParentData.Add(forecast);
            //    ParentData.Add(planned);
            //    ParentData.Add(actual);
            //}
            //else
            //{
            //    ParentData.Add(forecast);
            //    ParentData.Add(planned);
            //    ParentData.Add(actual);
            //}
            // End by Nishant Sheth

            #region "Add Action column link"
            string strAction = string.Empty;

            if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
            {
                double? forcastVal = 0, pannedVal = 0, actualVal = 0;
                forcastVal = GetSumofValueMainGrid(dataTable, id, "Forecast");
                pannedVal = GetSumofValueMainGrid(dataTable, id, "Planned");
                actualVal = GetSumofValueMainGrid(dataTable, id, "Actual");

                forecast = forcastVal.HasValue ? forcastVal.Value.ToString(formatThousand):"0";
                planned = pannedVal.HasValue ? pannedVal.Value.ToString(formatThousand) : "0";
                actual = actualVal.HasValue ? actualVal.Value.ToString(formatThousand) : "0";

                if (_IsBudgetCreate_Edit)
                {
                    addRow = "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row' />";
                    strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>Edit Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")));
                }
                else
                {
                    strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>View Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")));
                }
                if ((lstChildren != null && lstChildren.Count() > 0))
                {
                   lineItemCount = dataTable
                                       .Rows
                                       .Cast<DataRow>()
                                       .Where(rw => rw.Field<Int32>("ParentId") == id).Sum(chld => chld.Field<Int32>("LineItemCount"));
                                       row.SetField<Int32>("LineItemCount", lineItemCount); // Update LineItemCount in DataTable.
                }
            }
            else
            {
                if (_IsForecastCreate_Edit)
                {
                    addRow = "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row' />";
                    strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")));
                }
                else
                {
                    strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>View Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")));
                }
            }
            //forecast = forecast.ToString(new c
            ParentData.Add(HttpUtility.HtmlDecode(name));
            ParentData.Add(addRow);
            ParentData.Add(budget);
            ParentData.Add(forecast);
            ParentData.Add(planned);
            ParentData.Add(actual);
            ParentData.Add(strAction);
            ParentData.Add(lineItemCount.ToString());
            #endregion

            //return new FinanceParentChildModel { Id = id, Name = name, Children = children, Budget = budget, ForeCast = forcast, BudgetTotal = budgetTotal, ForeCastTotal = forcastTotal };
            return new DhtmlxGridRowDataModel { id = rowId, data = ParentData, rows = children, Detailid = Convert.ToString(id) };
        }
        #endregion

        #region Methods for Get Header Value
        public ActionResult GetFinanceHeaderValue(int budgetId = 0, string timeFrameOption = "", string isQuarterly = "Quarterly", bool IsMain = false)
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
                gridRowModel = GetFinanceMainGridData(budgetId);
                var DetailId = db.Budget_Detail.Where(a => a.BudgetId == budgetId && a.ParentId == null).Select(a => a.Id).FirstOrDefault();
                if (DetailId != null)
                {
                    var temp = gridRowModel.rows.Where(a => a.Detailid == Convert.ToString(DetailId)).Select(a => a.data).FirstOrDefault();
                    if (temp != null)
                    {
                        objFinanceHeader.Budget = Math.Round(Convert.ToDouble(temp[2]), 2);
                        objFinanceHeader.Forecast = Math.Round(Convert.ToDouble(temp[3]), 2);
                        objFinanceHeader.Planned = Math.Round(Convert.ToDouble(temp[4]), 2);
                        objFinanceHeader.Actual = Math.Round(Convert.ToDouble(temp[5]), 2);
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
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 1", Value = Enums.QuarterWithSpace.Quarter1.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 2", Value = Enums.QuarterWithSpace.Quarter2.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 3", Value = Enums.QuarterWithSpace.Quarter3.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarter 4", Value = Enums.QuarterWithSpace.Quarter4.ToString() });

            return Json(lstViewByAllocated, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Budget/Forecast Grid Data
        public JsonResult EditBudgetGridData(int BudgetId = 0, string IsQuaterly = "quarters")
        {
            DhtmlXGridRowModel budgetMain = new DhtmlXGridRowModel();
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
            dataTableMain.Columns.Add("LineItemCount", typeof(Int32));
            dataTableMain.Columns.Add("Budget", typeof(List<Double?>));
            dataTableMain.Columns.Add("ForeCast", typeof(List<Double?>));
            dataTableMain.Columns.Add("Plan", typeof(List<Double?>));
            dataTableMain.Columns.Add("Actual", typeof(List<Double?>));
            dataTableMain.Columns.Add("BudgetTotal", typeof(Double));
            dataTableMain.Columns.Add("ForeCastTotal", typeof(Double));
            dataTableMain.Columns.Add("PlanTotal", typeof(Double));
            dataTableMain.Columns.Add("ActualTotal", typeof(Double));

            //var Query = db.Budget_Detail.Where(a => a.BudgetId == (BudgetId > 0 ? BudgetId : a.BudgetId)).Select(a => new { a.Id, a.ParentId, a.Name }).ToList();
            var varBudgetIds = db.Budget_Detail.Where(a => a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId)).Select(a => new { a.BudgetId, a.ParentId }).FirstOrDefault();

            List<Budget_Detail> BudgetDetailList = db.Budget_Detail.Where(a => a.Id == (BudgetId > 0 ? BudgetId : a.BudgetId) || a.ParentId == (BudgetId > 0 ? BudgetId : a.ParentId)
                || a.BudgetId == (varBudgetIds.BudgetId != null ? varBudgetIds.BudgetId : 0)).Select(a => a).ToList();
            //MinParentid = BudgetId;
            List<int> BudgetDetailids = BudgetDetailList.Select(a => a.Id).ToList();
            List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<int> LineItemids = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

            var Query = BudgetDetailList.Select(a => new { a.Id, a.ParentId, a.Name }).ToList();

            List<Budget_DetailAmount> BudgetDetailAmount = db.Budget_DetailAmount.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = db.Plan_Campaign_Program_Tactic_LineItem_Actual.Where(a => LineItemids.Contains(a.PlanLineItemId)).Select(a => a).ToList();

            // foreach (var item in Query)

            Query.ForEach(
                item =>
                {
                    BudgetAmount objBudgetAmount = new BudgetAmount();
                    List<int> PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == item.Id).Select(a => a.PlanLineItemId).ToList();
                    if (varBudgetIds.ParentId != null)
                    {
                        if (item.Id != varBudgetIds.ParentId && item.ParentId != null)
                        {
                            objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList());
                            dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='grid_add' style='float:none !important' />", PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum() });
                        }
                    }
                    else
                    {
                        objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList());
                        dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='grid_add' style='float:none !important' />", PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum() });
                    }
                });


            var items = GetTopLevelRows(dataTableMain, MinParentid)
                        .Select(row => CreateItem(dataTableMain, row))
                        .ToList();

            var temp = items.Where(a => a.id == Convert.ToString(BudgetId)).Select(a => a.data).FirstOrDefault();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            if (temp != null)
            {
                objFinanceHeader.Budget = Math.Round(Convert.ToDouble(temp[temp.Count - 4]), 2);
                objFinanceHeader.Forecast = Math.Round(Convert.ToDouble(temp[temp.Count - 3]), 2);
                objFinanceHeader.Planned = Math.Round(Convert.ToDouble(temp[temp.Count - 2]), 2);
                objFinanceHeader.Actual = Math.Round(Convert.ToDouble(temp[temp.Count - 1]), 2);
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

            return Json(budgetMain, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditBudget(int BudgetId = 0, string level = "")
        {
            FinanceModel objFinanceModel = new FinanceModel();
            ViewBag.BudgetId = BudgetId;
            ViewBag.EditLevel = level;

            //bool isBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            //bool isBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            //bool isForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            //bool isForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);

            ViewBag.IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            //ViewBag.IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            ViewBag.IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            //IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);
            return PartialView("_EditBudget", objFinanceModel);
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
        DhtmlxGridRowDataModel CreateItem(DataTable dataTable, DataRow row)
        {
            var id = row.Field<Int32>("Id");
            var name = row.Field<String>("Name");
            var addRow = row.Field<String>("AddRow");
            var lineitemcount = row.Field<Int32>("LineItemCount");
            var budget = row.Field<List<Double?>>("Budget");
            var forcast = row.Field<List<Double?>>("ForeCast");
            var plan = row.Field<List<Double?>>("Plan");
            var actual = row.Field<List<Double?>>("Actual");
            var budgetTotal = row.Field<Double>("BudgetTotal");
            var forcastTotal = row.Field<Double>("ForeCastTotal");
            var plantotal = row.Field<Double?>("PlanTotal");
            var actualtotal = row.Field<Double?>("ActualTotal");
            var lstChildren = GetChildren(dataTable, id);
            var children = lstChildren
              .Select(r => CreateItem(dataTable, r))
              .ToList();
            List<string> ParentData = new List<string>();
            ParentData.Add(HttpUtility.HtmlDecode(name));
            string strAddRow = string.Empty;
            int rwcount = dataTable != null ? dataTable.Rows.Count : 0;
            if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
            {
                if (!_IsBudgetCreate_Edit)  // If user has not Create/Edit Budget permission then clear AddRow button Html.
                    addRow = string.Empty;

                lineitemcount = dataTable
                 .Rows
                 .Cast<DataRow>()
                 .Where(rw => rw.Field<Int32>("Id") == id).Sum(chld => chld.Field<Int32>("LineItemCount"));
                row.SetField<Int32>("LineItemCount", lineitemcount); // Update LineItemCount in DataTable.
                //ParentData.Add(Convert.ToString(lineitemcount));
            }
            else
            {
                if (!_IsForecastCreate_Edit)    // If user has not Create/Edit Forecast permission then clear AddRow button Html.
                    addRow = string.Empty;
            }
            ParentData.Add(addRow);
            ParentData.Add(Convert.ToString(lineitemcount));
            //ParentData.Add(string.Join(",", budget));
            //ParentData.Add(string.Join(",", forcast));

            int i = 0;

            for (i = 0; i < budget.Count; i++)
            {
                ParentData.Add(Convert.ToString(budget[i]));
                if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))  // if Grid has only single Budget record then set Edit Budget link.
                {
                    //var CheckIsparentZero=dataTable
                    //    .Rows
                    //    .Cast<DataRow>()
                    //    .Where(rw => rw.Field<Int32>("Id") == id).Select(a=>a.Field<List<Int32?>>)

                    var tempforcast = GetSumofPeriodValue(dataTable, id, i, "ForeCast");

                    var tempPlan = GetSumofPeriodValue(dataTable, id, i, "Plan");

                    var tempActual = GetSumofPeriodValue(dataTable, id, i, "Actual");

                    ParentData.Add(Convert.ToString(tempforcast));
                    ParentData.Add(Convert.ToString(tempPlan));
                    ParentData.Add(Convert.ToString(tempActual));
                    //ParentData.Add(Convert.ToString(forcast[i]));
                }
                else
                {
                    ParentData.Add(Convert.ToString(forcast[i]));
                    ParentData.Add(Convert.ToString(plan[i]));
                    ParentData.Add(Convert.ToString(actual[i]));
                }

            }

            ParentData.Add(Convert.ToString(budgetTotal));
            if ((lstChildren != null && lstChildren.Count() > 0) || (rwcount.Equals(1)))
            {
                var tempforcastTotal = GetSumofValue(dataTable, id, "ForeCastTotal");

                var tempPlanTotal = GetSumofValue(dataTable, id, "PlanTotal");

                var tempActualTotal = GetSumofValue(dataTable, id, "ActualTotal");

                ParentData.Add(Convert.ToString(tempforcastTotal));
                ParentData.Add(Convert.ToString(tempPlanTotal));
                ParentData.Add(Convert.ToString(tempActualTotal));
            }
            else
            {
                ParentData.Add(Convert.ToString(forcastTotal));
                ParentData.Add(Convert.ToString(plantotal));
                ParentData.Add(Convert.ToString(actualtotal));
            }

            List<userdata> objuserData = new List<userdata>();
            List<row_attrs> rows_attrData = new List<row_attrs>();
            objuserData.Add(new userdata { id = Convert.ToString(id) });
            objuserData.Add(new userdata { idwithName = "parent_" + Convert.ToString(id) });
            objuserData.Add(new userdata { row_attrs = "parent_" + Convert.ToString(id) });

            rows_attrData.Add(new row_attrs { id = Convert.ToString(id) });
            //return new FinanceParentChildModel { Id = id, Name = name, Children = children, Budget = budget, ForeCast = forcast, BudgetTotal = budgetTotal, ForeCastTotal = forcastTotal };
            return new DhtmlxGridRowDataModel { id = Convert.ToString(id), data = ParentData, rows = children, userdata = objuserData, row_attrs = rows_attrData };
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
        public BudgetAmount GetAmountValue(string isQuaterly, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount)
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
                    _Budget = Budget_DetailAmountList.Where(a => _curentBudget.Contains(a.Period)).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => _curentForeCast.Contains(a.Period)).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    _Plan = PlanDetailAmount.Where(a => _curentPlan.Contains(a.Period)).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    _Actual = ActualDetailAmount.Where(a => _curentActual.Contains(a.Period)).Sum(a => a.Value);
                    _actuallist.Add(_Actual);
                }
                #endregion
            }
            else if (isQuaterly == Enums.PlanAllocatedBy.months.ToString())
            {
                for (int i = 1; i <= 12; i++)
                {
                    _Budget = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    _Plan = PlanDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    _Actual = ActualDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
                    _actuallist.Add(_Actual);
                }
            }
            else
            {
                if (isQuaterly == Enums.QuarterWithSpace.Quarter1.ToString())
                {
                    //_curentBudget = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentForeCast = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentPlan = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentActual = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    _commonQuarters = Q1;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter2.ToString())
                {
                    //_curentBudget = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentForeCast = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentPlan = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentActual = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    _commonQuarters = Q2;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter3.ToString())
                {
                    //_curentBudget = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentForeCast = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentPlan = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentActual = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    _commonQuarters = Q3;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter4.ToString())
                {
                    //_curentBudget = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentForeCast = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentPlan = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    //_curentActual = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    _commonQuarters = Q4;
                }
                foreach (var item in _commonQuarters)
                {
                    _Budget = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Budget);
                    _budgetlist.Add(_Budget);

                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Forecast);
                    _forecastlist.Add(_ForeCast);

                    _Plan = PlanDetailAmount.Where(a => a.Period == item).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    _Actual = ActualDetailAmount.Where(a => a.Period == item).Sum(a => a.Value);
                    _actuallist.Add(_Actual);
                }
            }
            objbudget.Budget = _budgetlist;
            objbudget.ForeCast = _forecastlist;
            objbudget.Plan = _planlist;
            objbudget.Actual = _actuallist;
            return objbudget;
        }
        public BudgetAmount GetMainGridAmountValue(bool isQuaterly, string strTimeFrame, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount)
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

                    _Plan = PlanDetailAmount.Where(a => _curentPlan.Contains(a.Period)).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    _Actual = ActualDetailAmount.Where(a => _curentActual.Contains(a.Period)).Sum(a => a.Value);
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

                    _Plan = PlanDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
                    _planlist.Add(_Plan);

                    _Actual = ActualDetailAmount.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Value);
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
        public ActionResult UpdateBudgetGridData(int BudgetId = 0, bool IsQuaterly = true, string nValue = "0", string oValue = "0", string ColumnName = "", string Period = "", int ParentRowId = 0)
        {
            Budget_DetailAmount objBudAmount = new Budget_DetailAmount();
            nValue = HttpUtility.HtmlDecode(nValue);
            if (ColumnName == "Task Name")
            {
                Budget objBudget = new Budget();
                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail = db.Budget_Detail.Where(a => a.Id == BudgetId).FirstOrDefault();
                if (objBudgetDetail != null)
                {
                    objBudgetDetail.Name = Convert.ToString(nValue).Trim();
                    db.Entry(objBudgetDetail).State = EntityState.Modified;
                    db.SaveChanges();
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
                    var DetailParentId = db.Budget_Detail.Where(a => a.Id == ParentRowId).Select(a => new { a.ParentId, a.BudgetId }).FirstOrDefault();
                    objBudgetDetail = new Budget_Detail();
                    objBudgetDetail.Name = nValue;
                    objBudgetDetail.ParentId = Convert.ToInt32(ParentRowId);
                    objBudgetDetail.BudgetId = Convert.ToInt32(DetailParentId.BudgetId);
                    objBudgetDetail.CreatedDate = System.DateTime.Now;
                    objBudgetDetail.CreatedBy = Sessions.User.UserId;
                    db.Entry(objBudgetDetail).State = EntityState.Added;
                    db.SaveChanges();

                }
            }
            List<string> QuaterPeriod = new List<string>();
            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };
            string dataPeriod = "";
            var ParentDetails = db.Budget_Detail.Where(a => a.Id == BudgetId).Select(a => new { a.Id, a.ParentId }).FirstOrDefault();
            if (IsQuaterly)
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
                    if (ColumnName == "Budget")
                    {
                        objBudAmount.Budget = Convert.ToDouble(nValue);
                    }
                    else if (ColumnName == "Forecast")
                    {
                        objBudAmount.Forecast = Convert.ToDouble(nValue);
                    }

                    objBudAmount.Period = dataPeriod;
                    objBudAmount.BudgetDetailId = BudgetId;
                    db.Entry(objBudAmount).State = EntityState.Added;
                    db.SaveChanges();

                }
            }
            else
            {
                if (IsQuaterly)
                {
                    List<Budget_DetailAmount> BudgetAmountList = new List<Budget_DetailAmount>();
                    BudgetAmountList = db.Budget_DetailAmount.Where(a => a.BudgetDetailId == BudgetId && QuaterPeriod.Contains(a.Period)).OrderBy(a => a.Period).ToList();
                    double? QuaterSum = 0;
                    double? Maindiff = 0;
                    double? FirstOld = 0, SecondOld = 0, ThirdOld = 0;
                    double? FirstNew = 0, SecondNew = 0, ThirdNew = 0;
                    if (BudgetAmountList.Count > 0)
                    {
                        QuaterSum = (ColumnName == "Budget" ? Convert.ToDouble(BudgetAmountList.Select(a => a.Budget).Sum()) : Convert.ToDouble(BudgetAmountList.Select(a => a.Forecast).Sum()));
                        FirstNew = FirstOld = Convert.ToDouble(ColumnName == "Budget" ? BudgetAmountList[0].Budget : BudgetAmountList[0].Forecast);
                        SecondNew = SecondOld = Convert.ToDouble(BudgetAmountList.Count >= 2 ? (ColumnName == "Budget" ? BudgetAmountList[1].Budget : BudgetAmountList[1].Forecast) : 0);
                        ThirdNew = ThirdOld = Convert.ToDouble(BudgetAmountList.Count >= 3 ? (ColumnName == "Budget" ? BudgetAmountList[2].Budget : BudgetAmountList[2].Forecast) : 0);

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
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = FirstNew; }
                            else if (ColumnName == "Forecast")
                            { objBudAmountUpdate.Forecast = FirstNew; }
                        }
                        if (Count == 2)
                        {
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = SecondNew; }
                            else if (ColumnName == "Forecast")
                            { objBudAmountUpdate.Forecast = SecondNew; }
                        }
                        if (Count == 3)
                        {
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = ThirdNew; }
                            else if (ColumnName == "Forecast")
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
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = Convert.ToDouble(nValue); }
                            else if (ColumnName == "Forecast")
                            { objBudAmountUpdate.Forecast = Convert.ToDouble(nValue); }
                            db.Entry(objBudAmountUpdate).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            return Json(new { errormsg = "" });
        }
        #endregion
    }
}
