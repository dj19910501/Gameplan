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
            // set budgetId  and timeframe in session for import
            Sessions.ImportTimeFrame = viewByType;
            Sessions.BudgetDetailId = budgetId;
            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            //Get all budget grid data.
            objBudgetGridModel = _MarketingBudget.GetBudgetGridData(budgetId, viewByType, columnsRequested, Sessions.User.CID, Sessions.User.ID, Sessions.PlanExchangeRate, Sessions.PlanCurrencySymbol);
            var jsonResult = Json(new { GridData = objBudgetGridModel.objGridDataModel, AttacheHeader = objBudgetGridModel.attachedHeader }, JsonRequestBehavior.AllowGet);
            return jsonResult;
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
        #region Import Marketing Budget

        /// <summary>
        /// Created By Devanshi gandhi
        /// Import finance budget #2804
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
                            string excelConnectionString = string.Empty;
                            Request.Files[0].SaveAs(fileLocation);

                            if (fileExtension == ".xls")
                            {
                                excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                                                                   fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                                IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(Request.Files[0].InputStream);
                                ds = excelReader.AsDataSet();
                                if (ds != null && ds.Tables.Count > 0)
                                {
                                    objImprtData = _MarketingBudget.GetXLSData(viewByType, ds,ClientId, BudgetDetailId, PlanExchangeRate, CurrencySymbol); // Read Data from excel 2003/(.xls) format file to xml
                                }
                                if (ds == null)
                                {
                                    return Json(new { msg = "error", error = "Invalid data." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                objImprtData = _MarketingBudget.GetXLSXData(viewByType, fileLocation,ClientId, BudgetDetailId, PlanExchangeRate, CurrencySymbol); // Read Data from excel 2007/(.xlsx) and above version format file to xml
                            }
                            dtColumns = objImprtData.MarketingBudgetColumns;
                            xmlData = objImprtData.XmlData;
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
                            _MarketingBudget.ImportMarketingFinance(xmlData, dtColumns,UserID,ClientId, BudgetDetailId); // Import data to the database using store procedure
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
        #endregion
        #region Header
        /// <summary>
        /// Get finance header values(Budget, Forecast, Planned and Actual)
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <returns>Returns values in json format</returns>
        public JsonResult GetFinanceHeaderValues(int BudgetId)
        {
            // Call function to get header values 
            DataTable dtHeader = _MarketingBudget.GetFinanceHeaderValues(BudgetId, Sessions.PlanExchangeRate);

            string Budget = string.Empty;
            string Forecast = string.Empty;
            string Planned = string.Empty;
            string Actual = string.Empty;
            if (dtHeader != null && dtHeader.Rows.Count > 0)
            {
                // Get values from datatable
                Budget = Convert.ToString(dtHeader.Rows[0][Convert.ToString(Enums.FinanceHeader_Label.Budget)]);
                Forecast = Convert.ToString(dtHeader.Rows[0][Convert.ToString(Enums.FinanceHeader_Label.Forecast)]);
                Planned = Convert.ToString(dtHeader.Rows[0][Convert.ToString(Enums.FinanceHeader_Label.Planned)]);
                Actual = Convert.ToString(dtHeader.Rows[0][Convert.ToString(Enums.FinanceHeader_Label.Actual)]);
            }
            return Json(new { Budget = Budget, Forecast = Forecast, Planned = Planned, Actual = Actual }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}