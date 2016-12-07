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
        [AuthorizeUser(Enums.ApplicationActivity.BudgetCreateEdit | Enums.ApplicationActivity.ForecastCreateEdit | Enums.ApplicationActivity.ForecastView)]
        public ActionResult Index()
        {
            MarketingActivities MarketingActivities = new MarketingActivities();

            #region Bind Budget dropdown on grid
            MarketingActivities.ListofBudgets = _MarketingBudget.GetBudgetlist(Sessions.User.CID);// Budget dropdown
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

		/// <summary>
        /// Method to get marketing budget grid data for perticular budget
        /// </summary>
        /// <param name="budgetId">bedget id for which user want to get the data</param>
        /// <param name="TimeFrame">in which view user want to view the data quater/months</param>
        /// <param name="columnsRequested">Which columns user wants to see</param>
        /// <returns>Json data to bind the grid</returns>
        public JsonResult GetBudgetData(int budgetId, string TimeFrame, BudgetColumnFlag columnsRequested = 0) // need to pass columns requested
        {
            // set budgetId  and timeframe in session for import
            Sessions.ImportTimeFrame = TimeFrame;
            Sessions.BudgetDetailId = budgetId;
            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            //Get all budget grid data.
            objBudgetGridModel = _MarketingBudget.GetBudgetGridData(budgetId, TimeFrame, columnsRequested, Sessions.User.CID, Sessions.User.ID, Sessions.PlanExchangeRate, Sessions.PlanCurrencySymbol);
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
        public JsonResult GetFinanceHeaderValues(int BudgetId)
        {
            // Call function to get header values 
            MarketingBudgetHeadsUp objHeader = _MarketingBudget.GetFinanceHeaderValues(BudgetId, Sessions.PlanExchangeRate);

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
        public void SaveNewBudgetDetail(int BudgetId, string BudgetDetailName, int ParentId, string mainTimeFrame = "Yearly")
        {
            try
            {
                _MarketingBudget.SaveNewBudgetDetail(BudgetId, BudgetDetailName, ParentId, Sessions.User.CID, Sessions.User.ID, mainTimeFrame);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }


        #endregion

    }
}