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


namespace RevenuePlanner.Controllers
{
    public class FinanceController : Controller
    {
        //
        // GET: /Finance/
        private MRPEntities db = new MRPEntities();
        private const string PeriodPrefix = "Y";
        private const string formatThousand = "#,#0.##";
        public ActionResult Index(Enums.ActiveMenu activeMenu = Enums.ActiveMenu.Finance)
        {
            FinanceModel financeObj = new FinanceModel();
            //FinanceModelHeaders financeObj = new FinanceModelHeaders();
            StringBuilder GridString = new StringBuilder();
            var lstparnetbudget = Common.GetParentBudgetlist();
            var lstchildbudget = Common.GetChildBudgetlist(0);
            ViewBag.budgetlist = Common.GetBudgetlist();// main budget drp
            ViewBag.parentbudgetlist = lstparnetbudget;
            ViewBag.childbudgetlist = lstchildbudget;
            ViewBag.ActiveMenu = activeMenu;
            List<ViewByModel> lstViewByAllocated = new List<ViewByModel>();
            lstViewByAllocated.Add(new ViewByModel { Text = "Quarterly", Value = Enums.PlanAllocatedBy.months.ToString() });
            lstViewByAllocated.Add(new ViewByModel { Text = "Monthly", Value = Enums.PlanAllocatedBy.quarters.ToString() });
            lstViewByAllocated = lstViewByAllocated.Where(modal => !string.IsNullOrEmpty(modal.Text)).ToList();
            ViewBag.ViewByAllocated = lstViewByAllocated;
            financeObj.FinanemodelheaderObj = Common.GetFinanceHeaderValue();
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            string strbudgetId = lstparnetbudget != null && lstparnetbudget.Count > 0 ? lstparnetbudget.Select(budgt => budgt.Value).FirstOrDefault() : "0";
            gridRowModel = GetFinanceMainGridData(int.Parse(strbudgetId));
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
        public ActionResult RefreshMainGridData(int budgetId)
        {
            DhtmlXGridRowModel gridRowModel = new DhtmlXGridRowModel();
            gridRowModel = GetFinanceMainGridData(budgetId);
            return PartialView("_MainGrid", gridRowModel);
        }
        public ActionResult SaveNewBudgetDetail(string BudgetId, string BudgetDetailName, string ParentId)
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
                return RefreshMainGridData(_budgetId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private DhtmlXGridRowModel GetFinanceMainGridData(int budgetId)
        {
            DhtmlXGridRowModel mainGridData = new DhtmlXGridRowModel();
            List<DhtmlxGridRowDataModel> gridRowData = new List<DhtmlxGridRowDataModel>();
            try
            {

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
                var lstBudgetDetails = db.Budget_Detail.Where(bdgt => bdgt.BudgetId.Equals(budgetId)).Select(a => new { a.Id, a.ParentId, a.Name }).ToList();

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
                lstBudgetDetails.ForEach(
                    i =>
                    {
                        rowId = Regex.Replace(i.Name.Trim().Replace("_", ""), @"\s+", "") + "_" + i.Id.ToString() + "_" + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        objBudgetAmount = new BudgetAmount();
                        PlanLineItemsId = new List<int>();
                        PlanLineItemsId = LineItemidBudgetList.Where(a => a.BudgetDetailId == i.Id).Select(a => a.PlanLineItemId).ToList();
                        objBudgetAmount = GetAmountValue(false, BudgetDetailAmount.Where(a => a.BudgetDetailId == i.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList());
                        //rowId = Regex.Replace(i.Name.Trim(), @"\s+", "") + i.Id.ToString() + (i.ParentId == null ? "0" : i.ParentId.ToString());
                        dataTable.Rows.Add(new Object[] { i.Id, i.ParentId == null ? 0 : i.ParentId, rowId, i.Name, "<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row' />", objBudgetAmount.Budget.Sum().Value.ToString(formatThousand), objBudgetAmount.ForeCast.Sum().Value.ToString(formatThousand), objBudgetAmount.Plan.Sum().Value.ToString(formatThousand), objBudgetAmount.Actual.Sum().Value.ToString(formatThousand), "", PlanLineItemsId.Count });
                    });

                var MinParentid = 0;
                List<DhtmlxGridRowDataModel> lstData = new List<DhtmlxGridRowDataModel>();
                lstData = GetTopLevelRows(dataTable, MinParentid)
                            .Select(row => CreateMainGridItem(dataTable, row)).ToList();

                #endregion

                #region "Create Model for Parent Child data mapping"
                //DhtmlxGridRowDataModel objRowData1 = new DhtmlxGridRowDataModel();
                //objRowData1.id = "1001";
                //List<string> objData1 = new List<string>();
                //objData1.Add("rowA");
                //objData1.Add("<div id='row1' row-id='1001' class='finance_grid_add' />");
                //objData1.Add("John Grisham");
                //objData1.Add("John Grisham1");
                //objData1.Add("12.99");
                //objData1.Add("0");
                ////objData1.Add("05/01/1998");
                //objRowData1.data = objData1;

                //List<DhtmlxGridRowDataModel> lstchildRowData = new List<DhtmlxGridRowDataModel>();

                //DhtmlxGridRowDataModel childRowData1 = new DhtmlxGridRowDataModel();
                //childRowData1.id = "sub_1001";
                //List<string> childData1 = new List<string>();
                //childData1.Add("subrowA");
                //childData1.Add("<div id='subrow1' row-id='sub_1001' class='finance_grid_add' />");
                //childData1.Add("Blood and Smoke");
                //childData1.Add("Stephen King");
                //childData1.Add("0");
                //childData1.Add("1");
                ////childData1.Add("01/01/2000");
                //childRowData1.data = childData1;
                //childRowData1.rows = new List<DhtmlxGridRowDataModel>();
                //lstchildRowData.Add(childRowData1);

                //DhtmlxGridRowDataModel childRowData2 = new DhtmlxGridRowDataModel();
                //childRowData2.id = "sub_1002";
                //List<string> childData2 = new List<string>();
                //childData2.Add("subrowA");
                //childData2.Add("<div id='subrow2' row-id='sub_1002' class='finance_grid_add' />");
                //childData2.Add("Blood and Smoke");
                //childData2.Add("Stephen King");
                //childData2.Add("0");
                //childData2.Add("1");
                ////childData2.Add("01/01/2000");
                //childRowData2.data = childData2;
                //childRowData2.rows = new List<DhtmlxGridRowDataModel>();
                //lstchildRowData.Add(childRowData2);

                //objRowData1.rows = new List<DhtmlxGridRowDataModel>();
                //objRowData1.rows = lstchildRowData;
                //gridRowData.Add(objRowData1);

                //DhtmlxGridRowDataModel objRowData2 = new DhtmlxGridRowDataModel();
                //objRowData2.id = "1002";
                //List<string> objData2 = new List<string>();
                //objData2.Add("row2");
                //objData2.Add("<div id='row2' row-id='1002' class='finance_grid_add' />");
                //objData2.Add("John Grisham2");
                //objData2.Add("John Grisham2");
                //objData2.Add("12.99");
                //objData2.Add("0");
                ////objData2.Add("05/01/1998");
                //objRowData2.data = objData2;
                //objRowData2.rows = new List<DhtmlxGridRowDataModel>();
                ////objRowData2.rows = lstchildRowData;

                //gridRowData.Add(objRowData2);
                ////mainGridData.rows = gridRowData; 
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
            ParentData.Add(name);
            ParentData.Add(addRow);
            ParentData.Add(budget);
            ParentData.Add(forecast);
            ParentData.Add(planned);
            ParentData.Add(actual);
            #region "Add Action column link"
            string strAction = string.Empty;
            if (lstChildren != null && lstChildren.Count() > 0)
            {
                strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>Edit Budget</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'Budget'")));
                lineItemCount = dataTable
              .Rows
              .Cast<DataRow>()
              .Where(rw => rw.Field<Int32>("ParentId") == id).Sum(chld => chld.Field<Int32>("LineItemCount"));
                row.SetField<Int32>("LineItemCount", lineItemCount); // Update LineItemCount in DataTable.
            }
            else
            {
                strAction = string.Format("<div onclick='EditBudget({0},false,{1})' class='finance_link'>Edit Forecast</div>", id.ToString(), HttpUtility.HtmlEncode(Convert.ToString("'ForeCast'")));
            }
            ParentData.Add(strAction);
            ParentData.Add(lineItemCount.ToString());
            #endregion

            //return new FinanceParentChildModel { Id = id, Name = name, Children = children, Budget = budget, ForeCast = forcast, BudgetTotal = budgetTotal, ForeCastTotal = forcastTotal };
            return new DhtmlxGridRowDataModel { id = rowId, data = ParentData, rows = children };
        }
        #endregion

        #region Methods for Get Header Value
        public ActionResult GetFinanceHeaderValue(int budgetId = 0, string timeFrameOption = "", string isQuarterly = "Quarterly")
        {
            FinanceModelHeaders objfinanceheader = Common.GetFinanceHeaderValue(budgetId, timeFrameOption, isQuarterly);
            return PartialView("_financeheader", objfinanceheader);
        }
        #endregion

        #region Get parent Child dropdown value
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
        #endregion

        #region Get Budget/Forecast Grid Data
        public JsonResult EditBudgetGridData(int BudgetId = 0, bool IsQuaterly = true)
        {
            DhtmlXGridRowModel budgetMain = new DhtmlXGridRowModel();
            var MinBudgetid = 0; var MinParentid = 0;

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
                            dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='grid_add' />", PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum() });
                        }
                    }
                    else
                    {
                        objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount.Where(a => a.BudgetDetailId == item.Id).ToList(), PlanDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList(), ActualDetailAmount.Where(a => PlanLineItemsId.Contains(a.PlanLineItemId)).ToList());
                        dataTableMain.Rows.Add(new Object[] { item.Id, item.ParentId == null ? 0 : (item.Id == BudgetId ? 0 : item.ParentId), item.Name, "<div id='dv" + item.Id + "' row-id='" + item.Id + "' onclick='AddRow(this)'  class='grid_add' />", PlanLineItemsId.Count(), objBudgetAmount.Budget, objBudgetAmount.ForeCast, objBudgetAmount.Plan, objBudgetAmount.Actual, objBudgetAmount.Budget.Sum(), objBudgetAmount.ForeCast.Sum(), objBudgetAmount.Plan.Sum(), objBudgetAmount.Actual.Sum() });
                    }
                });


            var items = GetTopLevelRows(dataTableMain, MinParentid)
                        .Select(row => CreateItem(dataTableMain, row))
                        .ToList();

            budgetMain.rows = items;

            return Json(budgetMain, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditBudget(int BudgetId = 0, string level = "")
        {
            FinanceModel objFinanceModel = new FinanceModel();
            ViewBag.BudgetId = BudgetId;
            ViewBag.EditLevel = level;
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
            var children = GetChildren(dataTable, id)
              .Select(r => CreateItem(dataTable, r))
              .ToList();
            List<string> ParentData = new List<string>();
            ParentData.Add(name);
            ParentData.Add(addRow);
            ParentData.Add(Convert.ToString(lineitemcount));
            //ParentData.Add(string.Join(",", budget));
            //ParentData.Add(string.Join(",", forcast));

            int i = 0;
            for (i = 0; i < budget.Count; i++)
            {
                ParentData.Add(Convert.ToString(budget[i]));
                ParentData.Add(Convert.ToString(forcast[i]));
                ParentData.Add(Convert.ToString(plan[i]));
                ParentData.Add(Convert.ToString(actual[i]));
            }

            ParentData.Add(Convert.ToString(budgetTotal));
            ParentData.Add(Convert.ToString(forcastTotal));
            ParentData.Add(Convert.ToString(plantotal));
            ParentData.Add(Convert.ToString(actualtotal));
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
        public BudgetAmount GetAmountValue(bool isQuaterly, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount)
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
                        _curentActual = Q2.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
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
                    else if (ColumnName == "ForeCast")
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
                                    double? SecondDiff = Maindiff - ThirdOld;
                                    if (SecondDiff >= SecondOld)
                                    {
                                        SecondNew = SecondDiff - SecondOld;
                                        if (SecondNew <= SecondOld)
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
                            else if (ColumnName == "ForeCast")
                            { objBudAmountUpdate.Forecast = FirstNew; }
                        }
                        if (Count == 2)
                        {
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = SecondNew; }
                            else if (ColumnName == "ForeCast")
                            { objBudAmountUpdate.Forecast = SecondNew; }
                        }
                        if (Count == 3)
                        {
                            if (ColumnName == "Budget")
                            { objBudAmountUpdate.Budget = ThirdNew; }
                            else if (ColumnName == "ForeCast")
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
                            else if (ColumnName == "ForeCast")
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