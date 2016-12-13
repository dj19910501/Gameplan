using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RevenuePlanner.Services.MarketingBudget;
using StructureMap;
using Newtonsoft.Json;
using RevenuePlanner.Helpers;
using System.Linq;
using Excel;
using System.Xml;
using System.Data;
//using RevenuePlanner.Models;

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
        
        public ActionResult Index()
        {
            MarketingActivities MarketingActivities = new MarketingActivities();
            #region Check Permissions
            bool IsBudgetCreateEdit, IsBudgetView, IsForecastCreateEdit, IsForecastView;
            IsBudgetCreateEdit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            IsBudgetView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetView);
            IsForecastCreateEdit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            IsForecastView = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastView);
            if (IsBudgetCreateEdit == false && IsBudgetView == false && IsForecastCreateEdit == false && IsForecastView == false)
            {

                return RedirectToAction("Index", "NoAccess");
            }
            #endregion

            #region Set session for current client users
            // Set list of users for the current client into session
            Sessions.ClientUsers = _MarketingBudget.GetUserListByClientId(Sessions.User.CID);
            #endregion
            // Add owner list to the ViewBag to bind into drop down in the grid
            ViewBag.OwnerList = _MarketingBudget.GetOwnerListForDropdown(Sessions.User.CID, Sessions.ApplicationId, Sessions.ClientUsers);

            #region Bind Budget dropdown on grid
            MarketingActivities.ListofBudgets = _MarketingBudget.GetBudgetlist(Sessions.User.CID);// Budget dropdown
            //method to get  parent and child budget list
            ViewBag.parentbudgetlist = Common.GetParentBudgetlist();
            ViewBag.childbudgetlist = Common.GetChildBudgetlist(0);
            //end
            #endregion

            #region "Bind TimeFrame Dropdown"
            MarketingActivities.TimeFrame = Enums.QuartersFinance.Select(timeframe => new BindDropdownData { Text = timeframe.Key, Value = timeframe.Value }).ToList();
            #endregion

            #region Bind Column set dropdown
            List<BindDropdownData> ColumnSet = _MarketingBudget.GetColumnSet(Sessions.User.CID);// Column set  dropdown
            MarketingActivities.Columnset = ColumnSet;
            #endregion

            #region Bind Filter Columns dropdown
            // Filter Columns dropdown
            if (ColumnSet != null && ColumnSet.Count > 0)
            {
                string strColumnSetId = ColumnSet.FirstOrDefault().Value;
                List<RevenuePlanner.Models.Budget_Columns> BudgetColumns = _MarketingBudget.GetColumns(Convert.ToInt32(strColumnSetId));// Columns  dropdown
                MarketingActivities.FilterColumns = GetFilterColumnList(BudgetColumns); // Get Filter columns list

                MarketingActivities.StandardCols = GetStandardColumnList(BudgetColumns); // Get standard columns list
            }
            else
            {
                MarketingActivities.FilterColumns = new List<BindDropdownData>();
            }
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

		/// <summary>
        /// Added by Komal Rawal
        /// Returns Budget Data Hierarchy
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <param name="TimeFrame">Selected time frame</param>
        /// <returns>Json data to bind the grid</returns>
        [CompressAttribute]
        public JsonResult GetBudgetData(int budgetId, string TimeFrame) // need to pass columns requested
        {
            // set budgetId  and timeframe in session for import
            Sessions.ImportTimeFrame = TimeFrame;
            Sessions.BudgetDetailId = budgetId;
            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            try
            {
            //Get all budget grid data.
                objBudgetGridModel = _MarketingBudget.GetBudgetGridData(budgetId, TimeFrame, Sessions.User.CID, Sessions.User.ID, Sessions.PlanExchangeRate, Sessions.PlanCurrencySymbol, Sessions.ClientUsers);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { GridData = objBudgetGridModel.objGridDataModel, AttacheHeader = objBudgetGridModel.attachedHeader, SumColumns = objBudgetGridModel.colIndexes, nonPermissionIDs = objBudgetGridModel.nonePermissonIDs }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetColumns(int ColumnSetId = 0)
        {
            List<RevenuePlanner.Models.Budget_Columns> BudgetColumns = _MarketingBudget.GetColumns(ColumnSetId);// Columns  dropdown

            List<BindDropdownData> lstColumns = GetFilterColumnList(BudgetColumns); //Get Filter Columns list data.

            List<string> StandardTimeFrameColumns = GetStandardColumnList(BudgetColumns); //Get standard Columns list data.

            return Json(new { Columnlist = lstColumns, standardlist = StandardTimeFrameColumns }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Komal on 12/03/2016
        /// Return FilterColumns List
        /// </summary>
        /// <param name="BudgetColumns"> list of standard and custom budget columns </param>
        /// <returns> list of columns to bind Filter columns dropdown list</returns>
        private List<BindDropdownData> GetFilterColumnList(List<RevenuePlanner.Models.Budget_Columns> BudgetColumns)
        {
            List<BindDropdownData> lstColumns = new List<BindDropdownData>();

            if (BudgetColumns != null && BudgetColumns.Count > 0)
            {
                //All standard and custom columns
                lstColumns = BudgetColumns.Select(a => new BindDropdownData { Text = a.CustomField.Name, Value = Convert.ToString(a.CustomField.CustomFieldId) }).ToList();
            }
            return lstColumns;
        }

        /// <summary>
        /// Added by Komal on 12/03/2016
        /// Return FilterColumns List
        /// </summary>
        /// <param name="BudgetColumns"> list of standard and custom budget columns </param>
        /// <returns> list of columns to bind Filter columns dropdown list</returns>
        private List<string> GetStandardColumnList(List<RevenuePlanner.Models.Budget_Columns> BudgetColumns)
        {
            List<string> StandardTimeFrameColumns = new List<string>();

            if (BudgetColumns != null && BudgetColumns.Count > 0)
            {
                //All standard columns
                StandardTimeFrameColumns = BudgetColumns.Where(a => a.IsTimeFrame == true).Select(a => a.CustomField.Name).ToList();
            }
            return StandardTimeFrameColumns;
        }

        public Dictionary<BudgetCloumn, double> UpdateBudgetCell(int budgetId, BudgetCloumn columnIndex, double oldValue, double newValue)
        {
            throw new NotImplementedException();
        }
     
        
       /// <summary>
        /// Function to deleting budget data and its child heirarchy.
        /// Added By: Rahul Shah on 11/30/2016.
        /// </summary>
        public JsonResult DeleteBudgetData(int SelectedBudgetId, int BudgetId)
        {
            if (SelectedBudgetId <= 0 || BudgetId <= 0)
            {
                return Json(new { IsSuccess = false, ErrorMessage = Common.objCached.InvalidBudgetId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    int NextBudgetId = _MarketingBudget.DeleteBudget(SelectedBudgetId, Sessions.User.CID); // call DeleteBudget function to delete selected data.
                    if (NextBudgetId > 0)
                    {
                        BudgetId = NextBudgetId;
                    }
                    return Json(new { IsSuccess = true, budgetId = BudgetId }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { IsSuccess = false, ErrorMessage = Common.objCached.ClientPermissionDeleteBudgetRestrictionMessage }, JsonRequestBehavior.AllowGet);
                    
                }
                
            }
        }
        #region Import Marketing Budget

        /// <summary>
        /// Created By Devanshi gandhi
        /// Method used for upload/Import finance budget using xls/xlsx file - PL ticket #2804
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExcelFileUpload()
        {
            string viewByType = Enums.QuarterFinance.Yearly.ToString();
            int BudgetDetailId = Sessions.BudgetDetailId;
            double PlanExchangeRate = Sessions.PlanExchangeRate;
            string CurrencySymbol = Sessions.PlanCurrencySymbol;
            XmlDocument xmlData = new XmlDocument();
            DataTable dtColumns = new DataTable();
            BudgetImportData objImprtData = new BudgetImportData();
            DataSet ds = new DataSet();
            int ClientId = Sessions.User.CID;
            int UserID = Sessions.User.ID;
            if (Sessions.ImportTimeFrame != null)
            {
                viewByType = Convert.ToString(Sessions.ImportTimeFrame);
            }
            try
            {
                if (Request != null)
                {
                    if (Request.Files[0].ContentLength > 0)
                    {
                        string fileExtension = System.IO.Path.GetExtension(Request.Files[0].FileName);
                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            string DirectoryLocation = Server.MapPath("~/Content/");
                            string FileSessionId = Convert.ToString(Session.Contents.SessionID);
                            string FileName = FileSessionId + DateTime.Now.ToString("mm.dd.yyyy.hh.mm.ss.fff") + fileExtension;
                            string fileLocation = DirectoryLocation + FileName;
                            Request.Files[0].SaveAs(fileLocation);

                            if (fileExtension == ".xls")
                            {
                               ds= ReadXlSFile(fileLocation); //method to read xls file which uploaded
                                if (ds != null && ds.Tables.Count > 0)
                                {
                                    objImprtData = _MarketingBudget.GetXLSData(viewByType, ds, ClientId, BudgetDetailId, PlanExchangeRate, CurrencySymbol); // Read Data from excel 2003/(.xls) format file to xml
                                }
                                if (ds == null)
                                {
                                    return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                objImprtData = _MarketingBudget.GetXLSXData(viewByType, fileLocation, ClientId, BudgetDetailId, PlanExchangeRate, CurrencySymbol); // Read Data from excel 2007/(.xlsx) and above version format file to xml
                            }
                            if (objImprtData != null)
                            {
                            dtColumns = objImprtData.MarketingBudgetColumns;
                            xmlData = objImprtData.XmlData;
                            }
                            if (System.IO.File.Exists(fileLocation))
                            {
                                System.IO.File.Delete(fileLocation);
                            }
                            if (!string.IsNullOrEmpty(objImprtData.ErrorMsg))
                            {
                                return Json(new { msg = "error", error = objImprtData.ErrorMsg }, JsonRequestBehavior.AllowGet);
                            }
                            if (dtColumns == null)
                            {
                                return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                            }
                            if (dtColumns.Rows.Count == 0 || dtColumns.Rows[0][0] == DBNull.Value)
                            {
                                return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                            }
                            _MarketingBudget.ImportMarketingFinance(xmlData, dtColumns, UserID, ClientId, BudgetDetailId); // Import data to the database using store procedure
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                if (ex.Message.Contains("process"))
                {
                    return Json(new { msg = "error", error = "File is being used by another process." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                }
            }
            return new EmptyResult();
        }
        /// <summary>
        /// Method to read XLS file while import/upload
        /// </summary>
        /// <param name="fileLocation">location of file to read the data</param>
        /// <returns></returns>
        private DataSet ReadXlSFile(string fileLocation)
        {
            string excelConnectionString = string.Empty;

            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                                                                   fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(Request.Files[0].InputStream);
            DataSet ds = excelReader.AsDataSet();

            return ds;
        }
        #endregion
    
        #region Header
        /// <summary>
        /// Get finance header values(Budget, Forecast, Planned and Actual)
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <returns>Returns values in json format</returns>
        public JsonResult GetFinanceHeaderValues(int BudgetId, bool IsLineItem=false)
        {
            // Call function to get header values 
            MarketingBudgetHeadsUp objHeader = _MarketingBudget.GetFinanceHeaderValues(BudgetId, Sessions.PlanExchangeRate, Sessions.ClientUsers,IsLineItem);

            string Budget, Forecast, Planned, Actual;
            Budget = Forecast = Planned = Actual = string.Empty;

            if (objHeader != null)
            {
                // Get values from datatable
                Budget = objHeader.Budget.ToString();
                Forecast = objHeader.Forecast.ToString();
                Planned = objHeader.Planned.ToString();
                Actual = objHeader.Actual.ToString();
            }
            return Json(new { Budget = Budget, Forecast = Forecast, Planned = Planned, Actual = Actual }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region "Create new Budget related methods"
        /// <summary>
        /// Added by Komal on 12/05/2016
        /// returns add new budget partial view
        /// </summary>
        public ActionResult LoadnewBudget()
        {
            return PartialView("_newBudget");
        }


        /// <summary>
        /// Added by Komal on 12/05/2016
        /// Method to save new budget
        /// </summary>
        ///  /// <param name="budgetName">Name of the Budget</param>
        /// <returns>Returns budget id in json format</returns>
        public JsonResult SaveNewBudget(string budgetName)
        {
            int budgetId = 0;
            try
            {
                 budgetId = _MarketingBudget.SaveNewBudget(budgetName,Sessions.User.CID,Sessions.User.ID);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { budgetId = budgetId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Komal on 12/05/2016
        /// Method to save new item /child item
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <param name="BudgetDetailName">Name of the item</param>
        /// <param name="ParentId">ParentId of the Budget item</param>
        /// <param name="mainTimeFrame">Selected time frame value </param>
        public JsonResult SaveNewBudgetDetail(int BudgetId, string BudgetDetailName, int ParentId, string mainTimeFrame = "Yearly")
        {
            int _BudgetDetailId = 0;
            try
            {
                _BudgetDetailId = _MarketingBudget.SaveNewBudgetDetail(BudgetId, BudgetDetailName, ParentId, Sessions.User.CID, Sessions.User.ID, mainTimeFrame);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { BudgetDetailId = _BudgetDetailId }, JsonRequestBehavior.AllowGet);
        }


        #endregion


        /// <summary>
        /// Function to Updating budget data.
        /// Added By: Rahul Shah on 12/07/2016.
        /// </summary>
        /// <param name="BudgetId">Budget Id.</param>        
        /// <param name="BudgetDetailId">Budget Detail Id.</param>
        /// <param name="ParentId">Parent Budget Detail Id.</param>       
        /// <param name="nValue">new Value.</param>       
        /// <param name="ChildItemIds">Child Budget Detail Ids.</param>       
        /// <param name="ColumnName">Column Name.</param>       
        /// <param name="AllocationType">Allocation Type.</param>       
        /// <param name="Period">Perido (i.e Jan,Feb..etc).</param>       

        /// <summary>
        /// Function to Updating budget data.
        /// Added By: Rahul Shah on 12/07/2016.
        /// </summary>
        /// <param name="BudgetId">Budget Id.</param>        
        /// <param name="BudgetDetailId">Budget Detail Id.</param>
        /// <param name="ParentId">Parent Budget Detail Id.</param>       
        /// <param name="nValue">new Value.</param>       
        /// <param name="ChildItemIds">Child Budget Detail Ids.</param>       
        /// <param name="ColumnName">Column Name.</param>       
        /// <param name="AllocationType">Allocation Type.</param>       
        /// <param name="Period">Perido (i.e Jan,Feb..etc).</param>       

        public JsonResult UpdateMarketingBudget(int BudgetId, int BudgetDetailId, int ParentId, string nValue, string ChildItemIds, string ColumnName, string AllocationType, string Period)
        {
            //Check budget Id and budget detaild id is valid or not.
            if (BudgetId <= 0 || BudgetDetailId <= 0)
            {
                return Json(new { IsSuccess = false, ErrorMessage = Common.objCached.InvalidBudgetId }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    List<string> ListItems = new List<string>();
                    List<RevenuePlanner.Models.Budget_Columns> objColumns = _MarketingBudget.GetBudgetColumn(Sessions.User.CID); // get budget column of the client.
                    bool isForecast = false;

                    //Check Child budget items are exist or not 
                    if (!string.IsNullOrEmpty(ChildItemIds))
                    {
                        ListItems = ChildItemIds.Split(',').ToList();

                    }
                    ListItems.Add(Convert.ToString(BudgetDetailId));


                    if (string.Compare(ColumnName, Enums.DefaultGridColumn.Owner.ToString(), true) == 0)
                    {
                        int OwnerId = 0;
                        int.TryParse(nValue, out OwnerId);
                        if (OwnerId <= 0)
                        {
                            return Json(new { IsSuccess = false, ErrorMsg = "Owner is not valid" }, JsonRequestBehavior.AllowGet);
                        }
                        _MarketingBudget.UpdateOwnerName(BudgetDetailId, ListItems, OwnerId, Sessions.User.CID);
                    }
                    else if (string.Compare(ColumnName, Enums.DefaultGridColumn.Name.ToString(), true) == 0)
                    {
                        _MarketingBudget.UpdateTaskName(BudgetId, BudgetDetailId, ParentId, Sessions.User.CID, nValue);
                    }
                    else if (string.Compare(ColumnName, Enums.DefaultGridColumn.Budget.ToString(), true) == 0 ||
                             string.Compare(ColumnName, Enums.DefaultGridColumn.Forecast.ToString(), true) == 0 ||
                             string.Compare(ColumnName, Enums.DefaultGridColumn.Total_Budget.ToString(), true) == 0 ||
                             string.Compare(ColumnName, Enums.DefaultGridColumn.Total_Forecast.ToString(), true) == 0)
                    {

                        _MarketingBudget.UpdateTotalAmount(BudgetDetailId, nValue, ColumnName, Sessions.User.CID, Sessions.PlanExchangeRate);
                    }
                    else if (string.Compare(ColumnName.Split('_')[0], "cust", true) == 0)
                    {
                        if (objColumns != null && objColumns.Count > 0)
                        {
                            //here we get customfield column name Like 'cust_' + 'customfieldId' so we need to split and convert 'cusmfieldid' from string to int.
                            int CustomfieldId = 0;
                            if (ColumnName.Split('_').Length > 1) {
                                int.TryParse(ColumnName.Split('_')[1].ToString(),out CustomfieldId);
                            }
                          
                            RevenuePlanner.Models.Budget_Columns objCustomColumns = objColumns.Where(a => a.IsTimeFrame == false && a.CustomField.CustomFieldId == CustomfieldId).Select(a => a).FirstOrDefault();

                            if (objCustomColumns != null)
                            {
                                _MarketingBudget.SaveCustomColumnValues(CustomfieldId, objCustomColumns, BudgetDetailId, nValue, Sessions.User.ID, Sessions.User.CID, Sessions.PlanExchangeRate);//Call SaveBudgetorForecast method to save customfield cell value.
                            }
                        }
                    }
                    else
                    {
                        if (objColumns != null && objColumns.Count > 0)
                        {
                            //Get Name of the budget column             
                            string BudgetColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Budget && a.IsDeleted == false && a.IsTimeFrame == true
                                                    && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();
                            //Get Name of the Forecast column
                            string ForecastColName = objColumns.Where(a => a.ValueOnEditable == (int)Enums.ValueOnEditable.Forecast && a.IsDeleted == false && a.IsTimeFrame == true
                                                        && a.MapTableName == Enums.MapTableName.Budget_DetailAmount.ToString()).Select(a => a.CustomField.Name).FirstOrDefault();

                            //here monthly/quarterly column name with Y1/Q1 prefix so we need to split and check column is forecast or budget then perform the updation logic.
                            if (!string.IsNullOrEmpty(ColumnName))
                            {
                                if (ColumnName.Split('_').Length > 0)
                                {
                                    ColumnName = ColumnName.Split('_')[1];
                                }

                                if (ColumnName == BudgetColName)
                                {
                                    isForecast = false;
                                }
                                else
                                {
                                    isForecast = true;
                                }
                                //Call SaveBudgetorForecast method to save budget or forecast cell value.
                                _MarketingBudget.SaveBudgetorForecast(BudgetDetailId, nValue, Sessions.User.CID, isForecast, AllocationType, Period, Sessions.PlanExchangeRate);
                            }
                        }
                    }
                    return Json(new { IsSuccess = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { IsSuccess = false, ErrorMessage = Common.objCached.InvalidBudgetId }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #region method to get line items list
        /// <summary>
        /// Method to get budget parent list item
        /// </summary>
        /// <param name="BudgetDetailId">for which budget want get parent list item</param>
        public JsonResult GetParentLineItemList(int BudgetDetailId )
        {
            LineItemDropdownModel objParentDDLModel = new LineItemDropdownModel();

            if (BudgetDetailId > 0)
            {
            objParentDDLModel = _MarketingBudget.GetParentLineItemBudgetDetailslist(BudgetDetailId, Sessions.User.CID);
            }
            return Json(objParentDDLModel, JsonRequestBehavior.AllowGet);
        }
        //Method to get all child items for budget
        public JsonResult GetChildLineItemList(int BudgetDetailId )
        {
            if (BudgetDetailId > 0)
            {
                return Json(_MarketingBudget.GetChildLineItemBudgetDetailslist(BudgetDetailId, Sessions.User.CID), JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
           
        }
        /// <summary>
        /// Method to get all lineitem list foe budget as per time frame
        /// </summary>
        /// <param name="BudgetDetailId"></param>
        /// <param name="IsQuaterly"></param>
        /// <returns></returns>
        public ActionResult GetFinanceLineItemData(int BudgetDetailId, string TimeFrame = "quarters")
        {
            LineItemDetail AlllineItemdetail = new LineItemDetail();
            if (BudgetDetailId > 0)
            {
            #region "Set Create/Edit or View permission for Budget and Forecast to Global varialble."
            _IsBudgetCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BudgetCreateEdit);
            _IsForecastCreate_Edit = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ForecastCreateEdit);
            #endregion
                AlllineItemdetail = _MarketingBudget.GetLineItemGrid(BudgetDetailId,Sessions.User.CID, TimeFrame, Sessions.PlanExchangeRate);
            ViewBag.HasLineItems = AlllineItemdetail.childLineItemCount;
            }
            return PartialView("_LineItem", AlllineItemdetail.LineItemGridData);
        }

        #endregion

        #region Method to get user permission for budget
        /// <summary>
        /// method to get list of user permission for selected budget
        /// </summary>
        public ActionResult EditPermission(int BudgetId = 0, string level = "", string FlagCondition = "", string rowid = "")
        {
            try
            {
                ViewBag.EditLevel = level;
                ViewBag.GridRowID = rowid;

                ViewBag.childbudgetlist = _MarketingBudget.GetChildBudget(BudgetId);

                if (string.Compare(FlagCondition, "Edit", true) == 0)
                {
                    ViewBag.FlagCondition = "Edit";
                }
                else
                {
                    ViewBag.FlagCondition = "View";
                }

                List<RevenuePlanner.Models.Budget_Permission> UserList = _MarketingBudget.GetUserList(BudgetId);
                if (UserList.Count == 0)
                {
                    ViewBag.NoRecord = "NoRecord";
                }

                #region bindUser List for search
                List<BDSService.User> lstUserDetail = new List<BDSService.User>();
                lstUserDetail = _MarketingBudget.GetAllUserList(Sessions.User.CID, Sessions.User.ID, Sessions.ApplicationId);
                if (Sessions.User != null)
                {
                lstUserDetail.Add(new BDSService.User
                {
                    UserId = Sessions.User.UserId,
                    ID = Sessions.User.ID,
                    FirstName = Sessions.User.FirstName,
                    LastName = Sessions.User.LastName,
                    JobTitle = Sessions.User.JobTitle
                });
                }
                TempData["Userlist"] = lstUserDetail;
                #endregion
                FinanceModel objFinanceModel = _MarketingBudget.EditPermission(BudgetId, Sessions.ApplicationId, UserList, Sessions.User.ID,Sessions.User.CID);

                return PartialView("_UserPermission", objFinanceModel);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("_UserPermission", null);
        }

        /// <summary>
        /// Added by Nandish Shah
        /// Get specific record based on dropdown selection value of budgetdetail id
        /// </summary>
        public JsonResult GetUserFilterByBudget(int BudgetId = 0, string level = "", string FlagCondition = "")
        {
            // Sessions.BudgetDetailId = BudgetId;
            string strUserPermission = _MarketingBudget.CheckUserPermission(BudgetId, Sessions.User.CID, Sessions.User.ID);
            if (strUserPermission == "Edit")
            {
                ViewBag.FlagCondition = "Edit";
            }
            else
            {
                ViewBag.FlagCondition = "View";
            }
            List<UserPermission> _user = _MarketingBudget.FilterByBudget(BudgetId, Sessions.ApplicationId);
            return Json(new { _user = _user, Flag = ViewBag.FlagCondition }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Nandish Shah
        /// Get list of user records when typing letter on textbox 
        /// </summary>
        public JsonResult getData(string term, string UserIds)
        {
            List<RevenuePlanner.Models.UserModel> Getvalue = new List<RevenuePlanner.Models.UserModel>();
            try
            {
                List<BDSService.User> lstUserDetail = new List<BDSService.User>();
                if (TempData["Userlist"] != null)
                {
                    lstUserDetail = TempData["Userlist"] as List<BDSService.User>;
                    if (lstUserDetail.Count > 0)
                    {
                        lstUserDetail = lstUserDetail.OrderBy(user => user.FirstName).ThenBy(user => user.LastName).ToList();

                        var searchresult=lstUserDetail.Where(user => user.FirstName.ToLower().Contains(term.ToLower()) || user.LastName.ToLower().Contains(term.ToLower()) || (user.JobTitle!=null && user.JobTitle.ToLower().Contains(term.ToLower()))).ToList();
                        if(searchresult!=null && searchresult.Count>0)
                        {
                            Getvalue = searchresult.Select(user => new RevenuePlanner.Models.UserModel { UserId = user.ID, JobTitle = Convert.ToString(user.JobTitle), DisplayName = string.Format("{0} {1}", user.FirstName, user.LastName) }).ToList();
                        }
                        string[] keepList = UserIds.Split(',');
                        Getvalue = Getvalue.Where(i => !keepList.Contains(i.UserId.ToString())).ToList();
                        TempData["Userlist"] = lstUserDetail;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(Getvalue, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Nandish Shah
        /// Get specific of user record on selection of dropdown list
        /// </summary>
        public JsonResult GetuserRecord(int Id)
        {
            RevenuePlanner.Models.UserModel objUserModel = new RevenuePlanner.Models.UserModel();
            try
            {
                objUserModel = _MarketingBudget.GetuserRecord(Id, Sessions.User.ID, Sessions.ApplicationId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(objUserModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Delete record from Budget_Permission table
        /// </summary>
        /// <param name="id">contains user's Id</param>
        /// <returns>If success than return true</returns>
        [HttpPost]
        public JsonResult DeleteUser(int id, int budgetId, string ChildItems)
        {
            List<string> ListItems = new List<string>();
            List<int> BudgetDetailIds = new List<int>();
            if (ChildItems != "" && ChildItems != null)
            {
                ListItems = ChildItems.Split(',').ToList();

            }
            for (int i = 0; i < ListItems.Count; i++)
            {
                if (ListItems[i].Contains("_"))
                {
                    BudgetDetailIds.Add(Convert.ToInt32(ListItems[i].Split('_')[1]));
                }
            }
            BudgetDetailIds.Add(budgetId);
            _MarketingBudget.DeleteUserForBudget(BudgetDetailIds, id);
            return Json(new { Flag = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Added by Dashrath Prajapati for PL ticket #1679
        /// Save record in Budget_Permission table
        /// </summary>
        /// <param name="UserData">contains user's UserId,permission code,dropdown selection id</param>
        /// <param name="ID">contains user's id</param>
        /// <returns>if sucess then return true else false</returns>
        [HttpPost]
        public JsonResult SaveDetail(List<UserBudgetPermissionDetail> UserData, string ParentID, int[] CreatedBy, string ChildItems)
        {
            //Modified by Komal Rawal for #2242 change child item permission on change of parent item
            if (UserData != null)
            {
               _MarketingBudget.SaveUSerPermission(UserData,ChildItems,ParentID, Sessions.User.ID);

                //End
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}