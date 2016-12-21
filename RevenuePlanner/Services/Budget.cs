using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Budget : IBudget
    {
        RevenuePlanner.Services.ICurrency objCurrency;

        #region Variable Declaration
        private MRPEntities objDbMrpEntities;
        private BDSService.BDSServiceClient objBDSServiceClient;
        private StoredProcedure objSp;
        private IColumnView objColumnView;
        private CacheObject objCache;
        private const string manageviewicon = "<a href=javascript:void(0) onclick=OpenCreateNew(false) class=manageviewicon  title='Open Column Management'><i class='fa fa-edit'></i></a>";
        private const string Open = "1";
        private const string CellLocked = "1";
        private const string CellNotLocked = "0";
        public const string FixHeader = "ActivityId,Type,machinename,,,,Activity,,Total Budget" + manageviewicon + ",Planned Cost" + manageviewicon + ",Total Actual" + manageviewicon;
        public const string EndColumnsHeader = ",Unallocated Planned Cost,Unallocated Budget";
        public const string FixColumnIds = "ActivityId,Type,MachineName,LinkTacticId,colourcode,LineItemTypeId,TaskName,Buttons,BudgetCost,PlannedCost,ActualCost";
        public const string EndColumnIds = ",UnAllocatedCost,UnAllocatedBudget";
        public const string FixColType = "ro,ro,ro,ro,ro,ro,tree,ro,edn,edn,edn";
        public const string EndColType = ",ron,ron";
        public const string FixcolWidth = "100,100,100,100,10,100,302,75,110,110,110";
        public const string EndcolWidth = ",150,150";
        public const string FixColsorting = "na,na,na,na,na,na,na,na,int,int,int";
        public const string EndColsorting = ",int,int";
        public const string QuarterPrefix = "Q";
        public const string DhtmlxColSpan = "#cspan";
        public const string ColBudget = "Budget";
        public const string ColActual = "Actual";
        public const string ColPlanned = "Planned";
        public const string NotEditableCellStyle = "color:#a2a2a2 !important;";
        public const string RedCornerStyle = "background-image:url(./content/images/red-corner-budget.png); background-repeat: no-repeat; background-position: right top; ";
        public const string OrangeCornerStyle = "background-image: url(./content/images/orange-corner-budget.png); background-repeat: no-repeat; background-position: right top;";
        public const string ThreeDash = "0";
        public const string BudgetFlagval = "2";
        public const string CostFlagVal = "1";
        public bool isMultiYear = false;
        #endregion

        public Budget()
        {
            objDbMrpEntities = new MRPEntities();
            objBDSServiceClient = new BDSService.BDSServiceClient();
            objSp = new StoredProcedure();
            objColumnView = new ColumnView();
            objCurrency = new RevenuePlanner.Services.Currency();
            objCache = new CacheObject();
        }       

        /// <summary>
        /// Method to Get budget,planned and actuals grid data
        /// </summary>        
        public BudgetDHTMLXGridModel GetBudget(int ClientId, int UserID, string PlanIds, double PlanExchangeRate, string ViewBy, string Year = "", string CustomFieldIds = "", string OwnerIds = "", string TacticTypeIds = "", string StatusIds = "", string SearchText = "", string SearchBy = "", bool IsFromCache = false)
        {
            Contract.Requires<ArgumentNullException>(ClientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(UserID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(PlanIds != null, "At-least one plan should be selected");
            Contract.Requires<ArgumentNullException>(PlanExchangeRate > 0, "Plan Exchange Rate cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ViewBy != null, "ViewBy cannot be null.");

            string strThisQuarter = Convert.ToString(Enums.UpcomingActivities.ThisYearQuaterly);
            string strThisMonth = Convert.ToString(Enums.UpcomingActivities.ThisYearMonthly);
            // Set actual for quarters
            string AllocatedBy = Convert.ToString(Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)]).ToLower();
            // Check time frame selected for this year quarterly or this year monthly data and for this year option isMultiyear flag will always be false
            if (string.Compare(Year, strThisQuarter, true) == 0)
            {
                isMultiYear = false;
                Year = Convert.ToString(DateTime.Now.Year);
            }
            else if (string.Compare(Year, strThisMonth, true) == 0)
            {
                isMultiYear = false;
                AllocatedBy = Convert.ToString(Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.months)]).ToLower();
                Year = Convert.ToString(DateTime.Now.Year);
            }
            else
            {
                isMultiYear = Common.IsMultiyearTimeframe(Year);
            }
            List<PlanBudgetModel> GridDataList = new List<PlanBudgetModel>();
            List<PlanBudgetModel> PBModel = new List<PlanBudgetModel>(); // PBModel = Plan Budget Model
            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel();
            if (IsFromCache) // Create Model from Cache
                GridDataList = (List<PlanBudgetModel>)objCache.Returncache(Convert.ToString(Enums.CacheObject.AllBudgetGridData));
            if (GridDataList == null || GridDataList.Count() == 0)
            {
                DataTable dtBudget = objSp.GetBudget(PlanIds, UserID, ViewBy, OwnerIds, TacticTypeIds, StatusIds, Year); // Get budget data for budget,planned cost and actual using store procedure GetplanBudget

                PBModel = CreateBudgetDataModel(dtBudget, PlanExchangeRate); // Convert data table with budget data to PlanBudgetModel model

                PBModel = FilterPlanByTimeFrame(PBModel, Year); // Except plan level entity be filter at Db level so we remove plan object by applying time frame filter.  

                List<int> CustomFieldFilteredTacticIds = FilterCustomField(PBModel, CustomFieldIds);

                // filter budget model by custom field filter list
                if (CustomFieldFilteredTacticIds != null && CustomFieldFilteredTacticIds.Count > 0)
                {
                    PBModel.RemoveAll(a => string.Compare(a.ActivityType, ActivityType.ActivityTactic, true) == 0 && !CustomFieldFilteredTacticIds.Contains(Convert.ToInt32(a.Id)));
                }
                PBModel = SetCustomFieldRestriction(PBModel, UserID, ClientId); // Set custom field permission for budget cells. budget cell will editable or not.

                PBModel = ManageLineItems(PBModel); // Manage line items unallocated cost values in other line item

                #region "Calculate Monthly Budget from Bottom to Top for Hierarchy level like: LineItem > Tactic > Program > Campaign > CustomField(if filtered) > Plan"

                // Set ViewBy data to model.
                PBModel = RollUp(PBModel, ViewBy);

                #endregion
                if (ViewBy.Contains(Convert.ToString(PlanGanttTypes.Custom)))
                {
                    PBModel = SetLineItemCostByWeightage(PBModel, ViewBy); // Set LineItem monthly budget cost by it's parent tactic weight-age.
                }
                objCache.AddCache(Convert.ToString(Enums.CacheObject.AllBudgetGridData), PBModel);
            }
            else
            {
                PBModel = GridDataList;
            }
            objBudgetDHTMLXGrid = GenerateHeaderString(AllocatedBy, objBudgetDHTMLXGrid, Year); // Create header of model
            if (!string.IsNullOrEmpty(SearchText)) // Searching Text
            {
                List<PlanBudgetModel> SearchlistData;
                if (string.IsNullOrEmpty(SearchBy) || string.Compare(SearchBy, Convert.ToString(Enums.GlobalSearch.ActivityName), true) == 0)
                {
                    SearchlistData = PBModel.Where(a => a.ActivityName.ToLower().Contains(HttpUtility.HtmlEncode(SearchText.Trim().ToLower()))).ToList();
                }
                else
                {
                    SearchlistData = PBModel.Where(a => a.MachineName.ToLower().Contains(HttpUtility.HtmlEncode(SearchText.Trim().ToLower()))).ToList();
                }
                List<SearchParentDetail> parenttaskids = SearchlistData.Select(a => new SearchParentDetail
                {
                    ParentTaskId = a.ParentActivityId,
                    TaskId = a.TaskId
                }).ToList();
                // Call common method to search budget data as per search text
                List<string> selectedSerachId = Common.SearchGridCalendarData(parenttaskids, PBModel, Convert.ToString(Enums.ActivePlanTab.Budget));
                // Add all parents
                PBModel = (from Data in PBModel
                         join ParentData in selectedSerachId
                         on Data.ActivityId equals ParentData
                         select Data).ToList();
            }
            objBudgetDHTMLXGrid = CreateDhtmlxFormattedBudgetData(objBudgetDHTMLXGrid, PBModel, AllocatedBy, UserID, ClientId, Year, ViewBy); // Create model to bind data in grid as per DHTMLx grid format.
            // Get number of tab views for user in column management
            bool isPlangrid = false;
            bool isSelectAll = false;
            List<ColumnViewEntity> userManagedColumns = objColumnView.GetCustomfieldModel(ClientId, isPlangrid, out isSelectAll, UserID);
            string hiddenTab = string.Empty;
            if (!userManagedColumns.Where(u => u.EntityIsChecked).Any())
            {
                ColumnViewEntity PlannedColumn = userManagedColumns.Where(u => string.Compare(u.EntityType, Convert.ToString(Enums.Budgetcolumn.Planned), true) == 0).FirstOrDefault();
                if (PlannedColumn != null)
                {
                    PlannedColumn.EntityIsChecked = true;
                }
            }
            foreach (ColumnViewEntity item in userManagedColumns.Where(u => !u.EntityIsChecked).ToList())
            {
                hiddenTab = hiddenTab + item.EntityType + ',';
            }

            hiddenTab = string.Join(",", userManagedColumns.Where(u => !u.EntityIsChecked).Select(u => u.EntityType).ToList());

            objBudgetDHTMLXGrid.HiddenTab = hiddenTab;


            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method for roll-up bottom up values as per view by
        /// </summary>        
        public List<PlanBudgetModel> RollUp(List<PlanBudgetModel> PBModel, string ViewBy)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model item count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ViewBy != null, "ViewBy cannot be null.");

            List<PlanBudgetModel> Finalmodel = new List<PlanBudgetModel>();
            Finalmodel = CalculateBottomUp(PBModel, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, ViewBy); // Calculate monthly Tactic budget from it's child budget i.e LineItem

            Finalmodel = CalculateBottomUp(PBModel, ActivityType.ActivityProgram, ActivityType.ActivityTactic, ViewBy); // Calculate monthly Program budget from it's child budget i.e Tactic

            Finalmodel = CalculateBottomUp(PBModel, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, ViewBy); // Calculate monthly Campaign budget from it's child budget i.e Program

            Finalmodel = CalculateBottomUp(PBModel, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, ViewBy); // Calculate monthly Plan budget from it's child budget i.e Campaign

            return Finalmodel;
        }

        /// <summary>
        /// Method to Filter Data by Custom Field Selection
        /// </summary>        
        public List<int> FilterCustomField(List<PlanBudgetModel> BudgetModel, string CustomFieldFilter)
        {
            Contract.Requires<ArgumentNullException>(BudgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetModel.Count > 0, "Budget Model item count cannot be less than zero.");

            List<int> lstTacticIds = new List<int>();
            if (BudgetModel != null && BudgetModel.Count > 0)
            {
                #region "Declare & Initialize local Variables"
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                string tacticType = Convert.ToString(Enums.EntityType.Tactic).ToUpper();
                string[] filteredCustomFields = string.IsNullOrWhiteSpace(CustomFieldFilter) ? null : CustomFieldFilter.Split(',');
                List<PlanBudgetModel> tacData = BudgetModel.Where(tac => string.Compare(tac.ActivityType, tacticType, true) == 0).ToList();
                lstTacticIds = tacData.Select(tactic => Convert.ToInt32(tactic.Id)).ToList();
                #endregion

                if (filteredCustomFields != null)
                {
                    foreach (string customField in filteredCustomFields)
                    {
                        string[] splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    };

                    lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds);
                }
            }
            return lstTacticIds;
        }

        /// <summary>
        /// Method to set Budget Grid Model
        /// </summary>        
        public List<PlanBudgetModel> CreateBudgetDataModel(DataTable DtBudget, double PlanExchangeRate)
        {
            Contract.Requires<ArgumentNullException>(DtBudget != null, "Budget Data Table cannot be null.");
            Contract.Requires<ArgumentNullException>(DtBudget.Rows.Count > 0, "Budget Data Table rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(PlanExchangeRate > 0, "Plan Exchange Rate Filter cannot be null.");

            List<PlanBudgetModel> model = new List<PlanBudgetModel>();
            if (DtBudget != null)
            {
                model = DtBudget.AsEnumerable().Select(row => new PlanBudgetModel
                {
                    Id = Convert.ToString(row["Id"]),
                    TaskId = Convert.ToString(row["TaskId"]),
                    ParentId = Convert.ToString(row["ParentActivityId"]),
                    ActivityId = Convert.ToString(row["TaskId"]),
                    ActivityName = Convert.ToString(row["Title"]),
                    ActivityType = Convert.ToString(row["ActivityType"]),
                    ParentActivityId = Convert.ToString(row["ParentTaskId"]),
                    YearlyBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Budget"])), PlanExchangeRate),
                    TotalUnallocatedBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalUnallocatedBudget"])), PlanExchangeRate),
                    TotalActuals = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalAllocationActual"])), PlanExchangeRate),
                    TotalAllocatedCost = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Cost"])), PlanExchangeRate),
                    UnallocatedCost = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalUnAllocationCost"])), PlanExchangeRate),
                    IsOwner = Convert.ToBoolean(row["IsOwner"]),
                    CreatedBy = int.Parse(Convert.ToString(row["CreatedBy"])),
                    LineItemTypeId = Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"])),
                    isAfterApproved = Convert.ToBoolean(row["IsAfterApproved"]),
                    MachineName = Convert.ToString(row["MachineName"]),
                    ColorCode = Convert.ToString(row["ColorCode"]),
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = Convert.ToDateTime(row["EndDate"]),
                    LinkTacticId = Convert.ToInt32(row["LinkTacticId"]),
                    TacticTypeId = Convert.ToInt32(Convert.ToString(row["TacticTypeId"])),
                    PlanYear = Convert.ToString(row["PlanYear"]),
                    AssetType = Convert.ToString(row["ROITacticType"]),
                    AnchorTacticID = Convert.ToInt32(Convert.ToString(row["IsAnchorTacticId"])),
                    CalendarHoneycombpackageIDs = Convert.ToString(row["CalendarHoneycombpackageIDs"]),
                    LinkedPlanName = Convert.ToString(row["LinkedPlanName"]),

                    MonthValues = new BudgetMonth()
                    {
                        // Budget Month Allocation
                        BudgetY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y2"])), PlanExchangeRate),
                        BudgetY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y1"])), PlanExchangeRate),
                        BudgetY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y3"])), PlanExchangeRate),
                        BudgetY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y4"])), PlanExchangeRate),
                        BudgetY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y5"])), PlanExchangeRate),
                        BudgetY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y6"])), PlanExchangeRate),
                        BudgetY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y7"])), PlanExchangeRate),
                        BudgetY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y8"])), PlanExchangeRate),
                        BudgetY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y9"])), PlanExchangeRate),
                        BudgetY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y10"])), PlanExchangeRate),
                        BudgetY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y11"])), PlanExchangeRate),
                        BudgetY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y12"])), PlanExchangeRate),

                        // Cost Month Allocation
                        CostY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY2"])), PlanExchangeRate),
                        CostY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY1"])), PlanExchangeRate),
                        CostY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY3"])), PlanExchangeRate),
                        CostY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY4"])), PlanExchangeRate),
                        CostY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY5"])), PlanExchangeRate),
                        CostY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY6"])), PlanExchangeRate),
                        CostY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY7"])), PlanExchangeRate),
                        CostY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY8"])), PlanExchangeRate),
                        CostY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY9"])), PlanExchangeRate),
                        CostY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY10"])), PlanExchangeRate),
                        CostY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY11"])), PlanExchangeRate),
                        CostY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY12"])), PlanExchangeRate),

                        // Actuals Month Allocation
                        ActualY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY2"])), PlanExchangeRate),
                        ActualY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY1"])), PlanExchangeRate),
                        ActualY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY3"])), PlanExchangeRate),
                        ActualY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY4"])), PlanExchangeRate),
                        ActualY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY5"])), PlanExchangeRate),
                        ActualY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY6"])), PlanExchangeRate),
                        ActualY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY7"])), PlanExchangeRate),
                        ActualY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY8"])), PlanExchangeRate),
                        ActualY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY9"])), PlanExchangeRate),
                        ActualY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY10"])), PlanExchangeRate),
                        ActualY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY11"])), PlanExchangeRate),
                        ActualY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY12"])), PlanExchangeRate)
                    },
                    NextYearMonthValues = new BudgetMonth()
                    {
                        // Budget Month Allocation
                        BudgetY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y14"])), PlanExchangeRate),
                        BudgetY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y13"])), PlanExchangeRate),
                        BudgetY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y15"])), PlanExchangeRate),
                        BudgetY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y16"])), PlanExchangeRate),
                        BudgetY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y17"])), PlanExchangeRate),
                        BudgetY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y18"])), PlanExchangeRate),
                        BudgetY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y19"])), PlanExchangeRate),
                        BudgetY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y20"])), PlanExchangeRate),
                        BudgetY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y21"])), PlanExchangeRate),
                        BudgetY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y22"])), PlanExchangeRate),
                        BudgetY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y23"])), PlanExchangeRate),
                        BudgetY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y24"])), PlanExchangeRate),

                        // Cost Month Allocation
                        CostY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY14"])), PlanExchangeRate),
                        CostY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY13"])), PlanExchangeRate),
                        CostY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY15"])), PlanExchangeRate),
                        CostY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY16"])), PlanExchangeRate),
                        CostY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY17"])), PlanExchangeRate),
                        CostY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY18"])), PlanExchangeRate),
                        CostY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY19"])), PlanExchangeRate),
                        CostY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY20"])), PlanExchangeRate),
                        CostY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY21"])), PlanExchangeRate),
                        CostY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY22"])), PlanExchangeRate),
                        CostY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY23"])), PlanExchangeRate),
                        CostY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY24"])), PlanExchangeRate),

                        // Actuals Month Allocation
                        ActualY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY14"])), PlanExchangeRate),
                        ActualY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY13"])), PlanExchangeRate),
                        ActualY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY15"])), PlanExchangeRate),
                        ActualY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY16"])), PlanExchangeRate),
                        ActualY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY17"])), PlanExchangeRate),
                        ActualY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY18"])), PlanExchangeRate),
                        ActualY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY19"])), PlanExchangeRate),
                        ActualY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY20"])), PlanExchangeRate),
                        ActualY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY21"])), PlanExchangeRate),
                        ActualY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY22"])), PlanExchangeRate),
                        ActualY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY23"])), PlanExchangeRate),
                        ActualY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY24"])), PlanExchangeRate)
                    },
                    ChildMonthValues = new BudgetMonth(),
                    ChildNextYearMonthValues = new BudgetMonth()
                }).ToList();
            }
            return model;
        }

        /// <summary>
        /// Method to set Budget Object in DHTMLx Format
        /// </summary>        
        public List<Budgetdataobj> SetBudgetDhtmlxFormattedValues(List<PlanBudgetModel> PBModel, PlanBudgetModel Entity, string OwnerName, string EntityType, string AllocatedBy, bool IsNextYear, bool IsMultiyearPlan, string DhtmlxGridRowId, bool IsAddEntityRights, bool IsViewBy = false, string pcptid = "", string TacticType = "")  // pcptid = Plan-Campaign-Program-Tactic-Id
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(EntityType != null, "Entity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");

            List<Budgetdataobj> BudgetDataObjList = new List<Budgetdataobj>();
            Budgetdataobj BudgetDataObj = new Budgetdataobj();
            string Roistring = string.Empty;
            string PackageTacticIds = Entity.CalendarHoneycombpackageIDs;
            BudgetDataObj.value = Entity.Id;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Entity.ActivityType;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Entity.MachineName;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Convert.ToString(Entity.LinkTacticId);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.style = "background-color:#" + Entity.ColorCode;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add LineItemTypeId into DHTMLx model
            BudgetDataObj.value = Convert.ToString(Entity.LineItemTypeId);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add title of plan entity into DHTMLx model

            bool IsExtendedTactic = (Entity.EndDate.Year - Entity.StartDate.Year) > 0 ? true : false;
            int? LinkedTacticId = Entity.LinkTacticId;
            if (LinkedTacticId == 0)
            {
                LinkedTacticId = null;
            }
            string Linkedstring = string.Empty;
            if (string.Compare(Entity.ActivityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
            {
                Linkedstring = (((IsExtendedTactic && LinkedTacticId == null) ?
                                    "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>" :
                                        ((IsExtendedTactic && LinkedTacticId != null) || (LinkedTacticId != null)) ?
                                        "<div class='unlink-icon unlink-icon-grid'  LinkedPlanName='" + (string.IsNullOrEmpty(Entity.LinkedPlanName) ?
                                        null :
                                    Entity.LinkedPlanName.Replace("'", "&#39;")) + "' id = 'LinkIcon' ><i class='fa fa-link'></i></div>" : ""));
            }

            if (Entity.AnchorTacticID != null && Entity.AnchorTacticID > 0 && !string.IsNullOrEmpty(Entity.Id) && string.Compare(Convert.ToString(Entity.AnchorTacticID), Entity.Id, true) == 0)
            {
                // Get list of package tactic ids
                Roistring = "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id='pkgIcon' onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids='"
                            + PackageTacticIds + "'><i class='fa fa-object-group'></i></div>";
                BudgetDataObj.value = (Roistring).Replace("'", "&#39;").Replace("\"", "&#34;") + Linkedstring + (Entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            }
            else
            {
                BudgetDataObj.value = Linkedstring + (Entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            }

            if (string.Compare(Entity.ActivityType, ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId == null)
            {
                BudgetDataObj.lo = CellLocked;
                BudgetDataObj.style = NotEditableCellStyle;
            }
            else
            {
                BudgetDataObj.lo = Entity.isEntityEditable ? CellNotLocked : CellLocked;
                BudgetDataObj.style = Entity.isEntityEditable ? string.Empty : NotEditableCellStyle;
            }
            BudgetDataObjList.Add(BudgetDataObj);

            // Set icon of magnifying glass and honey comb for plan entity with respective ids
            Budgetdataobj iconsData = new Budgetdataobj();
            if (!IsViewBy)
            {
                iconsData.value = (SetIcons(Entity, OwnerName, EntityType, DhtmlxGridRowId, IsAddEntityRights, pcptid, TacticType));
            }
            else
            {
                iconsData.value = string.Empty;
            }
            BudgetDataObjList.Add(iconsData);

            // Set Total Actual,Total Budget and Total planned cost for plan entity
            BudgetDataObjList = CampaignBudgetSummary(PBModel, EntityType, Entity.ParentActivityId,
                  BudgetDataObjList, AllocatedBy, Entity.ActivityId, IsViewBy);
            // Set monthly/quarterly allocation of budget,actuals and planned for plan
            BudgetDataObjList = CampaignMonth(PBModel, EntityType, Entity.ParentActivityId,
                    BudgetDataObjList, AllocatedBy, Entity.ActivityId, IsNextYear, IsMultiyearPlan, IsViewBy);
            BudgetDataObj = new Budgetdataobj();
            // Add UnAllocated Cost into DHTMLx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(Entity.UnallocatedCost);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add unAllocated budget into DHTMLx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(Entity.TotalUnallocatedBudget);
            BudgetDataObjList.Add(BudgetDataObj);

            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set Magnifying Glass, Add Button & HoneyComb Button Icons
        /// </summary>
        public string SetIcons(PlanBudgetModel Entity, string OwnerName, string EntityType, string DhtmlxGridRowId, bool IsAddEntityRights, string pcptid, string TacticType)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(EntityType != null, "Entity Type cannot be null.");

            string doubledesh = "--";
            string IconsData = string.Empty;
            // Set icon of magnifying glass and honey comb for plan entity with respective ids
            string Title = (Entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            if (string.Compare(EntityType, ActivityType.ActivityPlan, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=Plan onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Plan alt=" + Entity.Id + " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " >";
                    IconsData += "<i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Plan  TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName='" + Title + "' altId=" + Convert.ToString(DhtmlxGridRowId);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ColorCode=" + Convert.ToString(Entity.ColorCode) + " taskId=" + Convert.ToString(Entity.Id);
                IconsData += " csvId=Plan_" + Convert.ToString(Entity.Id) + " ></div>";
            }
            else if (string.Compare(EntityType, ActivityType.ActivityCampaign, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=CP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Campaign alt=" + Entity.ParentId + "_" + Entity.Id;
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Campaign onclick=javascript:AddRemoveEntity(this) title=Select   TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(DhtmlxGridRowId) + " per=" + Convert.ToString(IsAddEntityRights).ToLower();
                IconsData += " ColorCode=" + Convert.ToString(Entity.ColorCode) + " taskId= " + Convert.ToString(Entity.Id) + " csvId=Campaign_" + Entity.Id + "></div>";
            }
            else if (string.Compare(EntityType, ActivityType.ActivityProgram, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=PP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Program alt=_" + Entity.ParentId + "_" + Entity.Id;
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Program onclick=javascript:AddRemoveEntity(this) title=Select  TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(DhtmlxGridRowId) + " ColorCode=" + Convert.ToString(Entity.ColorCode);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " taskId=" + Convert.ToString(Entity.Id) + " csvId=Program_" + Convert.ToString(Entity.Id) + " ></div>";
            }
            else if (string.Compare(EntityType, ActivityType.ActivityTactic, true) == 0)
            {
                // LinkTactic Permission based on Entity Year
                bool LinkTacticPermission = ((Entity.EndDate.Year - Entity.StartDate.Year) > 0) ? true : false;
                string LinkedTacticId = Entity.LinkTacticId == 0 ? "null" : Convert.ToString(Entity.LinkTacticId);

                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=TP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Tactic alt=__" + Convert.ToString(Entity.ParentId) + "_" + Convert.ToString(Entity.Id);
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " LinkTacticper =" + Convert.ToString(LinkTacticPermission) + " LinkedTacticId = " + Convert.ToString(LinkedTacticId);
                    IconsData += " tacticaddId=" + Convert.ToString(Entity.Id) + "><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Tactic ";
                IconsData += " TacticType= '" + Convert.ToString(TacticType) + "' OwnerName= '" + Convert.ToString(OwnerName) + "' roitactictype='"
                                + Entity.AssetType + "' anchortacticid='" + Entity.AnchorTacticID + "'  ";
                IconsData += " TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(DhtmlxGridRowId) + " ColorCode=" + Convert.ToString(Entity.ColorCode);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " taskId=" + Convert.ToString(Entity.Id) + " csvId=Tactic_" + Convert.ToString(Entity.Id) + " ></div>";
            }
            else if (string.Compare(EntityType, ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId != null)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=LP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  title=Add  id=Line alt=___" + Convert.ToString(Entity.ParentId) + "_" + Convert.ToString(Entity.Id);
                    IconsData += " lt=" + ((Entity.LineItemTypeId == null) ? 0 : Entity.LineItemTypeId) + " per=" + Convert.ToString(IsAddEntityRights).ToLower();
                    IconsData += " dt=" + Title + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }
            }
            return IconsData;
        }

        /// <summary>
        /// Method to create data in DHTMLx Format
        /// </summary>
        public BudgetDHTMLXGridModel CreateDhtmlxFormattedBudgetData(BudgetDHTMLXGridModel ObjBudgetDHTMLXGrid, List<PlanBudgetModel> PBModel, string AllocatedBy, int UserID, int ClientId, string Year, string ViewBy)
        {
            Contract.Requires<ArgumentNullException>(ObjBudgetDHTMLXGrid != null, "Budget DHTMLX Grid Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model rows cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ClientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(UserID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(ViewBy != null, "View By cannot be null.");

            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();

            if (ViewBy != Convert.ToString(PlanGanttTypes.Tactic))
            {
                foreach (PlanBudgetModel bmViewby in PBModel.Where(p => string.Compare(p.ActivityType, ViewBy, true) == 0).OrderBy(p => p.ActivityName))
                {
                    BudgetDHTMLXGridDataModel gridViewbyData = new BudgetDHTMLXGridDataModel();
                    List<BudgetDHTMLXGridDataModel> gridjsonlistViewby = new List<BudgetDHTMLXGridDataModel>();
                    gridViewbyData.id = bmViewby.ActivityId;
                    gridViewbyData.open = Open;
                    List<Budgetdataobj> BudgetviewbyDataList;
                    string EntityType = ViewBy;
                    bool isViewby = true;
                    BudgetviewbyDataList = SetBudgetDhtmlxFormattedValues(PBModel, bmViewby, string.Empty, EntityType, AllocatedBy, false, false, bmViewby.ActivityId, false, isViewby);
                    gridViewbyData.data = BudgetviewbyDataList;
                    List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(PBModel, UserID, ClientId, Year, AllocatedBy, isViewby, bmViewby.ActivityId);
                    foreach (BudgetDHTMLXGridDataModel item in gridJsondata)
                    {
                        gridjsonlistViewby.Add(item);
                    }
                    gridViewbyData.rows = gridjsonlistViewby;
                    gridjsonlist.Add(gridViewbyData);
                }
            }
            else
            {
                List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(PBModel, UserID, ClientId, Year, AllocatedBy, false);
                foreach (BudgetDHTMLXGridDataModel item in gridJsondata)
                {
                    gridjsonlist.Add(item);
                }
            }

            // Set plan entity in the DHTMLx formated model at top level of the hierarchy using loop

            ObjBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            ObjBudgetDHTMLXGrid.Grid.rows = gridjsonlist;
            return ObjBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method to set data in hierarchy
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> GenerateHierarchy(List<PlanBudgetModel> PBModel, int UserID, int ClientId, string Year, string AllocatedBy, bool IsViewBy, string ParentId = "")
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model rows cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ClientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(UserID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");

            bool IsPlanCreateAll = false;
            bool IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
            List<int> lstSubordinatesIds = new List<int>();

            if (IsPlanCreateAllAuthorized)
            {
                IsPlanCreateAll = true;
            }

            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(UserID);
            }

            IEnumerable<int> TacticTypeIds = PBModel.Where(t => string.Compare(t.ActivityType, ActivityType.ActivityTactic, true) == 0).Select(t => t.TacticTypeId).Distinct();
            Dictionary<int, string> lstTacticTypeTitle = objDbMrpEntities.TacticTypes.Where(tt => TacticTypeIds.Contains(tt.TacticTypeId) && tt.IsDeleted == false)
                                                            .ToDictionary(tt => tt.TacticTypeId, tt => tt.Title);

            List<BudgetDHTMLXGridDataModel> gridjsonlist = SetPlanHierarchy(PBModel, IsViewBy, ParentId, IsPlanCreateAll, UserID, lstSubordinatesIds, Year, lstTacticTypeTitle, AllocatedBy);

            return gridjsonlist;
        }

        /// <summary>
        /// Method to set data in hierarchy for Plan
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetPlanHierarchy(List<PlanBudgetModel> PBModel, bool IsViewBy, string ParentId, bool IsPlanCreateAll, int UserID, List<int> lstSubordinatesIds, string Year, Dictionary<int, string> lstTacticTypeTitle, string AllocatedBy)
        {
            List<Budgetdataobj> BudgetDataObjList;
            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel gridjsonlistPlanObj;
            foreach (PlanBudgetModel bm in PBModel.Where(p => string.Compare(p.ActivityType, ActivityType.ActivityPlan, true) == 0
                        && (!IsViewBy || string.Compare(p.ParentActivityId, ParentId, true) == 0)).OrderBy(p => p.ActivityName))
            {
                if (!IsPlanCreateAll)
                {
                    if (bm.CreatedBy == UserID || lstSubordinatesIds.Contains(bm.CreatedBy))
                        IsPlanCreateAll = true;
                    else
                        IsPlanCreateAll = false;
                }
                bool isCampignExist = PBModel.Where(p => string.Compare(p.ParentActivityId, bm.ActivityId, true) == 0).Any();
                DateTime MaxDate = default(DateTime); ;
                if (isCampignExist)
                {
                    MaxDate = PBModel.Where(p => string.Compare(p.ParentActivityId, bm.ActivityId, true) == 0).Max(a => a.EndDate);
                }

                // Set flag to identify plan year. e.g.if time frame is 2015-2016 and plan have plan year 2016 then we will not set month data for Jan-2015 to Dec-2015 of respective plan
                bool isNextYearPlan = false;
                bool isMultiYearPlan = false;
                string firstYear = Common.GetInitialYearFromTimeFrame(Year);
                if (bm.PlanYear != firstYear)
                {
                    isNextYearPlan = true;
                }
                if (MaxDate != default(DateTime))
                {
                    int MaxYear = MaxDate.Year;
                    if (MaxYear - Convert.ToInt32(bm.PlanYear) > 0)
                    {
                        isMultiYearPlan = true;
                    }
                }

                gridjsonlistPlanObj = new BudgetDHTMLXGridDataModel();
                gridjsonlistPlanObj.id = bm.TaskId;
                gridjsonlistPlanObj.open = Open;

                string OwnerName = string.Empty;
                OwnerName = Convert.ToString(bm.CreatedBy);

                BudgetDataObjList = SetBudgetDhtmlxFormattedValues(PBModel, bm, OwnerName, ActivityType.ActivityPlan, AllocatedBy, isNextYearPlan, isMultiYearPlan, gridjsonlistPlanObj.id, IsPlanCreateAll);
                gridjsonlistPlanObj.data = BudgetDataObjList;

                List<BudgetDHTMLXGridDataModel> CampaignRowsObjList = SetCampaignHierarchy(PBModel, bm, IsPlanCreateAll, UserID, lstSubordinatesIds, OwnerName, lstTacticTypeTitle, AllocatedBy, isNextYearPlan, isMultiYearPlan);

                // set campaign row data as child to respective plan
                gridjsonlistPlanObj.rows = CampaignRowsObjList;
                gridjsonlist.Add(gridjsonlistPlanObj);
            }
            return gridjsonlist;
        }

        /// <summary>
        /// Method to set data in hierarchy for Campaign
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetCampaignHierarchy(List<PlanBudgetModel> PBModel, PlanBudgetModel PlanModel, bool IsPlanCreateAll, int UserID, List<int> lstSubordinatesIds, string OwnerName, Dictionary<int, string> lstTacticTypeTitle, string AllocatedBy, bool IsNextYearPlan, bool IsMultiYearPlan)
        {
            List<BudgetDHTMLXGridDataModel> CampaignRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel CampaignRowsObj;
            foreach (
                PlanBudgetModel bmc in
                    PBModel.Where(
                        p => string.Compare(p.ActivityType, ActivityType.ActivityCampaign, true) == 0 && string.Compare(p.ParentActivityId, PlanModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName)
                )
            {
                CampaignRowsObj = new BudgetDHTMLXGridDataModel();
                CampaignRowsObj.id = bmc.TaskId;
                CampaignRowsObj.open = Open;

                bool IsCampCreateAll = IsPlanCreateAll = IsPlanCreateAll == false ? (bmc.CreatedBy == UserID || lstSubordinatesIds.Contains(bmc.CreatedBy)) ? true : false : true;

                OwnerName = Convert.ToString(PlanModel.CreatedBy);
                List<Budgetdataobj> CampaignDataObjList = SetBudgetDhtmlxFormattedValues(PBModel, bmc, OwnerName, ActivityType.ActivityCampaign, AllocatedBy, IsNextYearPlan,
                                                            IsMultiYearPlan, CampaignRowsObj.id, IsCampCreateAll);

                CampaignRowsObj.data = CampaignDataObjList;
                List<BudgetDHTMLXGridDataModel> ProgramRowsObjList = SetProgramHierarchy(PBModel, bmc, IsPlanCreateAll, UserID, lstSubordinatesIds, OwnerName, PlanModel, lstTacticTypeTitle, AllocatedBy, IsNextYearPlan, IsMultiYearPlan);

                // set program row data as child to respective campaign
                CampaignRowsObj.rows = ProgramRowsObjList;
                CampaignRowsObjList.Add(CampaignRowsObj);
            }
            return CampaignRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for Program
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetProgramHierarchy(List<PlanBudgetModel> PBModel, PlanBudgetModel CampaignModel, bool IsPlanCreateAll, int UserID, List<int> lstSubordinatesIds, string OwnerName, PlanBudgetModel PlanModel, Dictionary<int, string> lstTacticTypeTitle, string AllocatedBy, bool IsNextYearPlan, bool IsMultiYearPlan)
        {
            List<BudgetDHTMLXGridDataModel> ProgramRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel ProgramRowsObj;
            foreach (
                PlanBudgetModel bmp in
                    PBModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityProgram, true) == 0 &&
                            string.Compare(p.ParentActivityId, CampaignModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName))
            {
                ProgramRowsObj = new BudgetDHTMLXGridDataModel();
                ProgramRowsObj.id = bmp.TaskId;
                ProgramRowsObj.open = null;

                bool IsProgCreateAll = IsPlanCreateAll = IsPlanCreateAll == false ? (bmp.CreatedBy == UserID || lstSubordinatesIds.Contains(bmp.CreatedBy)) ? true : false : true;

                OwnerName = Convert.ToString(PlanModel.CreatedBy);
                List<Budgetdataobj> ProgramDataObjList = SetBudgetDhtmlxFormattedValues(PBModel, bmp, OwnerName, ActivityType.ActivityProgram, AllocatedBy, IsNextYearPlan,
                                                            IsMultiYearPlan, ProgramRowsObj.id, IsProgCreateAll);
                ProgramRowsObj.data = ProgramDataObjList;

                List<BudgetDHTMLXGridDataModel> TacticRowsObjList = SetTacticHierarchy(PBModel, bmp, IsPlanCreateAll, UserID, lstSubordinatesIds, OwnerName, PlanModel, lstTacticTypeTitle, AllocatedBy, IsNextYearPlan, IsMultiYearPlan, CampaignModel);

                // set tactic row data as child to respective program
                ProgramRowsObj.rows = TacticRowsObjList;
                ProgramRowsObjList.Add(ProgramRowsObj);
            }
            return ProgramRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for Tactic
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetTacticHierarchy(List<PlanBudgetModel> PBModel, PlanBudgetModel ProgramModel, bool IsPlanCreateAll, int UserID, List<int> lstSubordinatesIds, string OwnerName, PlanBudgetModel PlanModel, Dictionary<int, string> lstTacticTypeTitle, string AllocatedBy, bool IsNextYearPlan, bool IsMultiYearPlan, PlanBudgetModel CampaignModel)
        {
            List<BudgetDHTMLXGridDataModel> TacticRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel TacticRowsObj;
            foreach (
                PlanBudgetModel bmt in
                    PBModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
                            string.Compare(p.ParentActivityId, ProgramModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName).OrderBy(p => p.ActivityName))
            {
                TacticRowsObj = new BudgetDHTMLXGridDataModel();
                TacticRowsObj.id = bmt.TaskId;
                TacticRowsObj.open = null;

                bool IsTacCreateAll = IsPlanCreateAll == false ? (bmt.CreatedBy == UserID || lstSubordinatesIds.Contains(bmt.CreatedBy)) ? true : false : true;


                OwnerName = Convert.ToString(PlanModel.CreatedBy);
                string TacticType = string.Empty;
                if (lstTacticTypeTitle != null && lstTacticTypeTitle.Count > 0)
                {
                    if (lstTacticTypeTitle.ContainsKey(bmt.TacticTypeId))
                    {
                        TacticType = Convert.ToString(lstTacticTypeTitle[bmt.TacticTypeId]);
                    }
                }
                List<Budgetdataobj> TacticDataObjList = SetBudgetDhtmlxFormattedValues(PBModel, bmt, OwnerName, ActivityType.ActivityTactic, AllocatedBy, IsNextYearPlan,
                        IsMultiYearPlan, TacticRowsObj.id, IsTacCreateAll, false, "L" + PlanModel.ActivityId + "_C" + CampaignModel.ActivityId + "_P" + ProgramModel.ActivityId + "_T" + bmt.ActivityId, TacticType);

                TacticRowsObj.data = TacticDataObjList;
                List<BudgetDHTMLXGridDataModel> LineRowsObjList = SetLineItemHierarchy(PBModel, bmt, IsPlanCreateAll, UserID, lstSubordinatesIds, OwnerName, AllocatedBy, IsNextYearPlan, IsMultiYearPlan, PlanModel);

                // set line item row data as child to respective tactic
                TacticRowsObj.rows = LineRowsObjList;
                TacticRowsObjList.Add(TacticRowsObj);
            }
            return TacticRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for LineItem
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetLineItemHierarchy(List<PlanBudgetModel> PBModel, PlanBudgetModel TacticModel, bool IsPlanCreateAll, int UserID, List<int> lstSubordinatesIds, string OwnerName, string AllocatedBy, bool IsNextYearPlan, bool IsMultiYearPlan, PlanBudgetModel PlanModel)
        {
            List<BudgetDHTMLXGridDataModel> LineRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel LineRowsObj;
            foreach (
                PlanBudgetModel bml in
                    PBModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
                            string.Compare(p.ParentActivityId, TacticModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName))
            {
                LineRowsObj = new BudgetDHTMLXGridDataModel();
                LineRowsObj.id = bml.TaskId;
                LineRowsObj.open = null;

                bool IsLinItmCreateAll = IsPlanCreateAll == false ? (bml.CreatedBy == UserID || lstSubordinatesIds.Contains(bml.CreatedBy)) ? true : false : true;

                OwnerName = Convert.ToString(PlanModel.CreatedBy);
                List<Budgetdataobj> LineDataObjList = SetBudgetDhtmlxFormattedValues(PBModel, bml, OwnerName, ActivityType.ActivityLineItem, AllocatedBy, IsNextYearPlan,
                                                        IsMultiYearPlan, LineRowsObj.id, IsLinItmCreateAll);

                LineRowsObj.data = LineDataObjList;
                LineRowsObjList.Add(LineRowsObj);
            }
            return LineRowsObjList;
        }

        /// <summary>
        /// Method to generate header string in DHTMLx format
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderString(string AllocatedBy, BudgetDHTMLXGridModel ObjBudgetDHTMLXGrid, string Year)
        {
            Contract.Requires<ArgumentNullException>(ObjBudgetDHTMLXGrid != null, "Budget DHTMLX Grid Model cannot be null.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");

            string firstYear = Common.GetInitialYearFromTimeFrame(Year);
            string lastYear = string.Empty;
            // Check if multi year flag is on then last year will be firstyear + 1
            if (isMultiYear)
            {
                lastYear = Convert.ToString(Convert.ToInt32(firstYear) + 1);
            }

            StringBuilder setHeader = new StringBuilder();
            string colType = string.Empty, width = string.Empty, colSorting = string.Empty, columnIds = string.Empty;
            string manageviewicon = "<a href=javascript:void(0) onclick=OpenCreateNew(false) class=manageviewicon  title='Open Column Management'><i class='fa fa-edit'></i></a>";

            setHeader.Append(FixHeader);
            columnIds = FixColumnIds;
            colType = FixColType;
            width = FixcolWidth;
            colSorting = FixColsorting;
            string headerYear = firstYear; // Column header year text which will bind with respective header
            if (string.Compare(AllocatedBy, Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)], true) == 0)
            {
                int quarterCounter = 1;

                int multiYearCounter = 23; // If budget data need to show with multi year then set header for multi quarter
                if (!isMultiYear)
                {
                    multiYearCounter = 11;
                }
                for (int MonthNo = 1; MonthNo <= multiYearCounter; MonthNo += 3)
                {
                    // Date time object will be used to find respective month text by month numbers
                    DateTime dt;
                    if (MonthNo < 12)
                    {
                        dt = new DateTime(2012, MonthNo, 1);
                    }
                    else
                    {
                        dt = new DateTime(2012, MonthNo - 12, 1);
                    }
                    setHeader.Append(",Q").Append(Convert.ToString(quarterCounter)).Append("-").Append(headerYear)
                  .Append(" Budget ").Append(manageviewicon).Append(",Q").Append(Convert.ToString(quarterCounter))
                  .Append("-").Append(headerYear).Append(" Planned ").Append(manageviewicon)
                  .Append(",Q").Append(Convert.ToString(quarterCounter)).Append("-").Append(headerYear)
                  .Append(" Actual ").Append(manageviewicon);


                    columnIds = columnIds + "," + "Budget,Planned,Actual";
                    colType = colType + ",edn,edn,edn";
                    width = width + ",140,140,140";
                    colSorting = colSorting + ",int,int,int";

                    if (quarterCounter == 4) // Check if quarter counter reach to last quarter then reset it
                    {
                        quarterCounter = 1;
                        headerYear = lastYear;
                    }
                    else
                    {
                        quarterCounter++;
                    }
                }
            }
            else
            {
                for (int monthNo = 1; monthNo <= 12; monthNo++)
                {
                    DateTime dt = new DateTime(2012, monthNo, 1);

                    setHeader.Append(",")
                    .Append(dt.ToString("MMM").ToUpper())
                    .Append("-")
                    .Append(headerYear)
                    .Append(" Budget ")

                    .Append(manageviewicon)
                    .Append(",")
                    .Append(dt.ToString("MMM").ToUpper())
                    .Append("-")
                    .Append(headerYear)

                    .Append(" Planned ")
                    .Append(manageviewicon)
                    .Append(",")
                    .Append(dt.ToString("MMM").ToUpper())
                    .Append("-")

                    .Append(headerYear)
                    .Append(" Actual ")
                    .Append(manageviewicon);

                    columnIds = columnIds + "," + "Budget,Planned,Actual";
                    colType = colType + ",edn,edn,edn";
                    width = width + ",155,155,155";
                    colSorting = colSorting + ",int,int,int";
                }
            }

            ObjBudgetDHTMLXGrid.SetHeader = setHeader + EndColumnsHeader;
            ObjBudgetDHTMLXGrid.ColType = colType + EndColType;
            ObjBudgetDHTMLXGrid.Width = width + EndcolWidth;
            ObjBudgetDHTMLXGrid.ColSorting = colSorting + EndColsorting;
            ObjBudgetDHTMLXGrid.ColumnIds = columnIds + EndColumnIds;

            return ObjBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method to set Total Budget, Planned & Actual Data
        /// </summary>
        private List<Budgetdataobj> CampaignBudgetSummary(List<PlanBudgetModel> PBModel, string ActType, string ParentActivityId, List<Budgetdataobj> BudgetDataObjList, string AllocatedBy, string ActivityId, bool IsViewBy = false)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ActType != null, "Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(ParentActivityId != null, "Parent Activity Id cannot be null.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(ActivityId != null, "Activity Id cannot be null.");

            PlanBudgetModel Entity = PBModel.Where(pl => string.Compare(pl.ActivityType, ActType, true) == 0 &&
                                                    string.Compare(pl.ParentActivityId, ParentActivityId, true) == 0 &&
                                                    string.Compare(pl.ActivityId, ActivityId, true) == 0)
                                                .OrderBy(p => p.ActivityName).ToList().FirstOrDefault();
            double ChildTotalBudget = PBModel.Where(cl => string.Compare(cl.ParentActivityId, ActivityId, true) == 0).Sum(cl => cl.YearlyBudget);
            if (Entity != null)
            {
                Budgetdataobj objTotalBudget = new Budgetdataobj();
                Budgetdataobj objTotalCost = new Budgetdataobj();
                Budgetdataobj objTotalActual = new Budgetdataobj();
                // Entity type line item has no budget so we set '---' value for line item
                if (!IsViewBy)
                {
                    if (string.Compare(Entity.ActivityType, ActivityType.ActivityLineItem, true) == 0)
                    {
                        objTotalBudget.value = ThreeDash; // Set values for Total budget
                        objTotalBudget.lo = CellLocked;
                        objTotalBudget.style = NotEditableCellStyle;
                    }
                    else
                    {
                        objTotalBudget.value = Convert.ToString(Entity.YearlyBudget); // Set values for Total budget
                        objTotalBudget.lo = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                        objTotalBudget.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    }

                    objTotalActual.value = Convert.ToString(Entity.TotalActuals); // Set values for Total actual
                    objTotalActual.lo = CellLocked;
                    objTotalActual.style = NotEditableCellStyle;

                    bool isOtherLineItem = string.Compare(ActType, ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId == null;
                    objTotalCost.value = Convert.ToString(Entity.TotalAllocatedCost);
                    objTotalCost.lo = Entity.isCostEditable && !isOtherLineItem ? CellNotLocked : CellLocked;
                    objTotalCost.style = Entity.isCostEditable && !isOtherLineItem ? string.Empty : NotEditableCellStyle;
                    if (Common.ParseDoubleValue(objTotalCost.value) > Common.ParseDoubleValue(objTotalBudget.value) && Entity.ActivityType != ActivityType.ActivityLineItem)
                    {
                        objTotalCost.style = objTotalCost.style + RedCornerStyle;
                        objTotalCost.av = CostFlagVal;
                    }
                    if (ChildTotalBudget > Common.ParseDoubleValue(objTotalBudget.value))
                    {
                        objTotalBudget.style = objTotalBudget.style + OrangeCornerStyle;
                        objTotalBudget.av = BudgetFlagval;
                    }
                }
                else
                {
                    objTotalBudget.value = ThreeDash; // Set values for Total budget
                    objTotalBudget.lo = CellLocked;
                    objTotalBudget.style = NotEditableCellStyle;
                    objTotalActual.value = ThreeDash;
                    objTotalActual.lo = CellLocked;
                    objTotalActual.style = NotEditableCellStyle;
                    objTotalCost.value = ThreeDash;
                    objTotalCost.lo = CellLocked;
                    objTotalCost.style = NotEditableCellStyle;
                }


                BudgetDataObjList.Add(objTotalBudget);
                BudgetDataObjList.Add(objTotalCost);
                BudgetDataObjList.Add(objTotalActual);

            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to check view by i.e. quarterly or monthly & check plan is multi-year plan or not.
        /// </summary>
        private List<Budgetdataobj> CampaignMonth(List<PlanBudgetModel> PBModel, string ActType, string ParentActivityId, List<Budgetdataobj> BudgetDataObjList, string AllocatedBy, string ActivityId, bool IsNextYearPlan, bool IsMulityearPlan, bool IsViewBy = false)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ActType != null, "Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(ParentActivityId != null, "Parent Activity Id cannot be null.");
            Contract.Requires<ArgumentNullException>(AllocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(ActivityId != null, "Activity Id cannot be null.");

            PlanBudgetModel Entity = PBModel.Where(pl => string.Compare(pl.ActivityType, ActType, true) == 0 && string.Compare(pl.ParentActivityId, ParentActivityId, true) == 0 &&
                                                    string.Compare(pl.ActivityId, ActivityId, true) == 0).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();

            bool isTactic = string.Compare(ActType, Helpers.ActivityType.ActivityTactic, true) == 0 ? true : false;
            bool isLineItem = string.Compare(ActType, Helpers.ActivityType.ActivityLineItem, true) == 0 ? true : false;
            bool isOtherLineitem = string.Compare(ActType, Helpers.ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId == null ? true : false;

            if (string.Compare(AllocatedBy, "quarters", true) != 0)
            {
                if (!IsNextYearPlan)
                {
                    BudgetDataObjList = CampignMonthlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsViewBy, isOtherLineitem);
                }
            }
            else
            {
                if (!IsNextYearPlan)
                {
                    BudgetDataObjList = CampignQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsMulityearPlan, IsViewBy, isOtherLineitem);
                }
                else if (!isMultiYear)
                {
                    BudgetDataObjList = CampignNextYearQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsMulityearPlan, IsViewBy, isOtherLineitem);
                }
                else
                {
                    BudgetDataObjList = CampignMulitYearQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsViewBy, isOtherLineitem);
                }
            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set model in monthly view
        /// </summary>
        private List<Budgetdataobj> CampignMonthlyAllocation(PlanBudgetModel Entity, bool IsTactic, bool IsLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsViewby = false, bool IsOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            int BudgetIndex = 0;
            foreach (PropertyInfo BProp in BudgetProp)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();
                if (!IsViewby)
                {
                    // Set cell locked property
                    objBudgetMonth.lo = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.lo = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.lo = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;

                    // Set cell style property
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                    // Set cell value property
                    objBudgetMonth.value = !IsLineItem ? Convert.ToString(BProp.GetValue(Entity.MonthValues)) : ThreeDash;
                    objCostMonth.value = Convert.ToString(CostProp[BudgetIndex].GetValue(Entity.MonthValues));
                    objActualMonth.value = Convert.ToString(ActualProp[BudgetIndex].GetValue(Entity.MonthValues));

                    // Set Orange Flag
                    if (!IsLineItem && Convert.ToDouble(BProp.GetValue(Entity.MonthValues)) < Convert.ToDouble(BProp.GetValue(Entity.ChildMonthValues)))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                        objBudgetMonth.av = BudgetFlagval;
                    }

                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !IsLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.av = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.lo = CellLocked;
                    objCostMonth.lo = CellLocked;
                    objActualMonth.lo = CellLocked;
                    objBudgetMonth.value = ThreeDash;
                    objCostMonth.value = ThreeDash;
                    objActualMonth.value = ThreeDash;
                }
                BudgetDataObjList.Add(objBudgetMonth);
                BudgetDataObjList.Add(objCostMonth);
                BudgetDataObjList.Add(objActualMonth);

                BudgetIndex++;
            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set model in quarterly view
        /// </summary>
        private List<Budgetdataobj> CampignQuarterlyAllocation(PlanBudgetModel Entity, bool IsTactic, bool IsLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsMultiYearPlan, bool IsViewBy = false, bool IsOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");

            int multiYearCounter = 23;
            if (!isMultiYear)
            {
                multiYearCounter = 11;
            }

            PropertyInfo[] BudgetProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int MYCnt = 0; MYCnt <= multiYearCounter; MYCnt += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                // Set cell locked property
                objBudgetMonth.lo = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objCostMonth.lo = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objActualMonth.lo = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;

                // Set cell style property
                objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.style = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                if (!IsViewBy)
                {
                    if (MYCnt < 12)
                    {
                        // Set First Year Cost
                        SetCampaignQuarterlyAllocation(Entity, IsLineItem, BudgetProp, CostProp, ActualProp, objBudgetMonth, objCostMonth, objActualMonth, MYCnt);
                    }
                    else
                    {
                        // Set Multi Year Cost
                        SetCampaignMultiyearQuarterlyAllocation(Entity, IsLineItem, IsMultiYearPlan, BudgetProp, objBudgetMonth, objCostMonth, objActualMonth, MYCnt - 12);
                    }

                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !IsLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.av = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.lo = CellLocked;
                    objCostMonth.lo = CellLocked;
                    objActualMonth.lo = CellLocked;
                    objBudgetMonth.value = ThreeDash;
                    objCostMonth.value = ThreeDash;
                    objActualMonth.value = ThreeDash;
                }
                BudgetDataObjList.Add(objBudgetMonth);
                BudgetDataObjList.Add(objCostMonth);
                BudgetDataObjList.Add(objActualMonth);
            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set model for multi-year plan
        /// </summary>
        private static void SetCampaignMultiyearQuarterlyAllocation(PlanBudgetModel Entity, bool IsLineItem, bool IsMultiYearPlan, PropertyInfo[] BudgetProp, Budgetdataobj ObjBudgetMonth, Budgetdataobj ObjCostMonth, Budgetdataobj ObjActualMonth, int MonthNo)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetProp != null, "Budget Property cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetProp.Length > 0, "Budget Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(MonthNo >= 0, "Month Number cannot be less than zero.");

            double FMNBudget = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Budget Month of Quarter
            double SMNBudget = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Budget Month of Quarter
            double TMNBudget = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Budget Month of Quarter

            double FMNCBudget = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year First Child Budget Month of Quarter
            double SMNCBudget = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year Second Child Budget Month of Quarter
            double TMNCBudget = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year Third Child Budget Month of Quarter

            double FMCCost = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Cost Month of Quarter
            double SMCCost = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Cost Month of Quarter
            double TMCCost = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Cost Month of Quarter

            double FMCActual = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Actual Month of Quarter
            double SMCActual = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Actual Month of Quarter
            double TMCActual = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Actual Month of Quarter

            // Set cell value property
            ObjBudgetMonth.value = IsMultiYearPlan && !IsLineItem ? Convert.ToString(FMNBudget + SMNBudget + TMNBudget) : ThreeDash;
            ObjCostMonth.value = IsMultiYearPlan ? Convert.ToString(FMCCost + SMCCost + TMCCost) : ThreeDash;
            ObjActualMonth.value = IsMultiYearPlan ? Convert.ToString(FMCActual + SMCActual + TMCActual) : ThreeDash;

            // Set Orange Flag
            if (!IsLineItem && Common.ParseDoubleValue(ObjBudgetMonth.value) < (FMNCBudget + SMNCBudget + TMNCBudget))
            {
                ObjBudgetMonth.style = ObjBudgetMonth.style + OrangeCornerStyle;
                ObjBudgetMonth.av = BudgetFlagval;
            }
        }

        /// <summary>
        /// Method to set model for single-year plan
        /// </summary>
        private static void SetCampaignQuarterlyAllocation(PlanBudgetModel Entity, bool IsLineItem, PropertyInfo[] BudgetProp, PropertyInfo[] CostProp, PropertyInfo[] ActualProp, Budgetdataobj ObjBudgetMonth, Budgetdataobj ObjCostMonth, Budgetdataobj objActualMonth, int MonthNo)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetProp != null, "Budget Property cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetProp.Length > 0, "Budget Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(CostProp != null, "Cost Property cannot be null.");
            Contract.Requires<ArgumentNullException>(CostProp.Length > 0, "Cost Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ActualProp != null, "Actual Property cannot be null.");
            Contract.Requires<ArgumentNullException>(ActualProp.Length > 0, "Actual Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(MonthNo >= 0, "Month Number cannot be less than zero.");

            double FMBudget = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.MonthValues)); // First Budget Month of Quarter
            double SMBudget = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.MonthValues)); // Second Budget Month of Quarter
            double TMBudget = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.MonthValues)); // Third Budget Month of Quarter

            double FMCBudget = Convert.ToDouble(BudgetProp[MonthNo].GetValue(Entity.ChildMonthValues)); // First Child Budget Month of Quarter
            double SMCBudget = Convert.ToDouble(BudgetProp[MonthNo + 1].GetValue(Entity.ChildMonthValues)); // Second Child Budget Month of Quarter
            double TMCBudget = Convert.ToDouble(BudgetProp[MonthNo + 2].GetValue(Entity.ChildMonthValues)); // Third Child Budget Month of Quarter

            double FMCost = Convert.ToDouble(CostProp[MonthNo].GetValue(Entity.MonthValues)); // First Cost Month of Quarter
            double SMCost = Convert.ToDouble(CostProp[MonthNo + 1].GetValue(Entity.MonthValues)); // Second Cost Month of Quarter
            double TMCost = Convert.ToDouble(CostProp[MonthNo + 2].GetValue(Entity.MonthValues)); // Third Cost Month of Quarter

            double FMActual = Convert.ToDouble(ActualProp[MonthNo].GetValue(Entity.MonthValues)); // First Actual Month of Quarter
            double SMActual = Convert.ToDouble(ActualProp[MonthNo + 1].GetValue(Entity.MonthValues)); // Second Actual Month of Quarter
            double TMActual = Convert.ToDouble(ActualProp[MonthNo + 2].GetValue(Entity.MonthValues)); // Third Actual Month of Quarter

            // Set cell value property
            ObjBudgetMonth.value = !IsLineItem ? Convert.ToString(FMBudget + SMBudget + TMBudget) : ThreeDash;
            ObjCostMonth.value = Convert.ToString(FMCost + SMCost + TMCost);
            objActualMonth.value = Convert.ToString(FMActual + SMActual + TMActual);

            // Set Orange Flag
            if (!IsLineItem && Common.ParseDoubleValue(ObjBudgetMonth.value) < (FMCBudget + SMCBudget + TMCBudget))
            {
                ObjBudgetMonth.style = ObjBudgetMonth.style + OrangeCornerStyle;
                ObjBudgetMonth.av = BudgetFlagval;
            }
        }

        /// <summary>
        /// Method to set quarterly model for multi-year plan
        /// </summary>
        private List<Budgetdataobj> CampignMulitYearQuarterlyAllocation(PlanBudgetModel Entity, bool IsTactic, bool IsLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsViewBy = false, bool IsOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = Entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int monthNo = 0; monthNo < 23; monthNo += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                if (!IsViewBy)
                {
                    // Set cell locked property
                    objBudgetMonth.lo = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.lo = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.lo = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;

                    // Set cell style property
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                    if (monthNo < 12)
                    {
                        // Set cell value property
                        objBudgetMonth.value = ThreeDash;
                        objCostMonth.value = ThreeDash;
                        objActualMonth.value = ThreeDash;

                        // Set cell locked property
                        objBudgetMonth.lo = CellLocked;
                        objCostMonth.lo = CellLocked;
                        objActualMonth.lo = CellLocked;

                        // Set cell style property
                        objBudgetMonth.style = NotEditableCellStyle;
                        objCostMonth.style = NotEditableCellStyle;
                        objActualMonth.style = NotEditableCellStyle;
                    }
                    else
                    {
                        int SingleYrMonth = monthNo - 12;
                        double FMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth].GetValue(Entity.MonthValues)); // First Budget Month of Quarter
                        double SMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 1].GetValue(Entity.MonthValues)); // Second Budget Month of Quarter
                        double TMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 2].GetValue(Entity.MonthValues)); // Third Budget Month of Quarter

                        objBudgetMonth.value = !IsLineItem ? Convert.ToString(FMBudget + SMBudget + TMBudget) : ThreeDash; // Set cell value property

                        double FMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth].GetValue(Entity.ChildMonthValues)); // First Child Budget Month of Quarter
                        double SMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 1].GetValue(Entity.ChildMonthValues)); // Second Child Budget Month of Quarter
                        double TMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 2].GetValue(Entity.ChildMonthValues)); // Third Child Budget Month of Quarter

                        // Set Orange Flag
                        if (!IsLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMCBudget + SMCBudget + TMCBudget))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.av = BudgetFlagval;
                        }

                        double FMCost = Convert.ToDouble(CostProp[SingleYrMonth].GetValue(Entity.MonthValues)); // First Cost Month of Quarter
                        double SMCost = Convert.ToDouble(CostProp[SingleYrMonth + 1].GetValue(Entity.MonthValues)); // Second Cost Month of Quarter
                        double TMCost = Convert.ToDouble(CostProp[SingleYrMonth + 2].GetValue(Entity.MonthValues)); // Third Cost Month of Quarter

                        objCostMonth.value = Convert.ToString(FMCost + SMCost + TMCost); // Set cell value property

                        double FMActual = Convert.ToDouble(ActualProp[SingleYrMonth].GetValue(Entity.MonthValues)); // First Actual Month of Quarter
                        double SMActual = Convert.ToDouble(ActualProp[SingleYrMonth + 1].GetValue(Entity.MonthValues)); // Second Actual Month of Quarter
                        double TMActual = Convert.ToDouble(ActualProp[SingleYrMonth + 2].GetValue(Entity.MonthValues)); // Third Actual Month of Quarter                        

                        objActualMonth.value = Convert.ToString(FMActual + SMActual + TMActual); // Set cell value property
                    }
                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !IsLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.av = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.lo = CellLocked;
                    objCostMonth.lo = CellLocked;
                    objActualMonth.lo = CellLocked;
                    objBudgetMonth.value = ThreeDash;
                    objCostMonth.value = ThreeDash;
                    objActualMonth.value = ThreeDash;
                }
                BudgetDataObjList.Add(objBudgetMonth);
                BudgetDataObjList.Add(objCostMonth);
                BudgetDataObjList.Add(objActualMonth);
            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set quarterly model for next-year plan
        /// </summary>
        private List<Budgetdataobj> CampignNextYearQuarterlyAllocation(PlanBudgetModel Entity, bool IsTactic, bool IsLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsMultiYearPlan, bool isViewBy = false, bool IsOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(Entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = Entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = Entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = Entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int monthNo = 0; monthNo <= 10; monthNo += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                // Set cell locked property
                objBudgetMonth.lo = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objCostMonth.lo = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objActualMonth.lo = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;

                // Set cell style property
                objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.style = Entity.isCostEditable && (IsTactic || (IsLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                double FMNBudget = Convert.ToDouble(BudgetProp[monthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Budget Month of Quarter
                double SMNBudget = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Budget Month of Quarter
                double TMNBudget = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Budget Month of Quarter

                objBudgetMonth.value = !IsLineItem ? Convert.ToString(FMNBudget + SMNBudget + TMNBudget) : ThreeDash; // Set cell value property

                double FMNCBudget = Convert.ToDouble(BudgetProp[monthNo].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year First Child Budget Month of Quarter
                double SMNCBudget = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year Second Child Budget Month of Quarter
                double TMNCBudget = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(Entity.ChildNextYearMonthValues)); // Multi Year Third Child Budget Month of Quarter

                // Set Orange Flag
                if (!IsLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMNCBudget + SMNCBudget + TMNCBudget))
                {
                    objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                    objBudgetMonth.av = BudgetFlagval;
                }

                double FMCCost = Convert.ToDouble(BudgetProp[monthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Cost Month of Quarter
                double SMCCost = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Cost Month of Quarter
                double TMCCost = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Cost Month of Quarter

                objCostMonth.value = Convert.ToString(FMCCost + SMCCost + TMCCost); // Set cell value property

                double FMCActual = Convert.ToDouble(BudgetProp[monthNo].GetValue(Entity.NextYearMonthValues)); // Multi Year First Actual Month of Quarter
                double SMCActual = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(Entity.NextYearMonthValues)); // Multi Year Second Actual Month of Quarter
                double TMCActual = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(Entity.NextYearMonthValues)); // Multi Year Third Actual Month of Quarter

                objActualMonth.value = Convert.ToString(FMCActual + SMCActual + TMCActual); // Set cell value property

                // Set Red Flag
                if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !IsLineItem)
                {
                    objCostMonth.style = objCostMonth.style + RedCornerStyle;
                    objCostMonth.av = CostFlagVal;
                }
                BudgetDataObjList.Add(objBudgetMonth);
                BudgetDataObjList.Add(objCostMonth);
                BudgetDataObjList.Add(objActualMonth);
            }
            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set custom field restriction based on permission
        /// </summary>
        public List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> BudgetModel, int UserId, int ClientId)
        {
            Contract.Requires<ArgumentNullException>(BudgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(UserId > 0, "UserId cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ClientId > 0, "ClientId cannot be less than zero.");

            List<int> lstSubordinatesIds = new List<int>();

            // Get list of subordinates which will be use to check if user is subordinate
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(UserId);
            }
            // Custom field type drop-down list
            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);
            // Custom field type text box
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            // Flag will be use to set if custom field is display for filter or not
            bool isDisplayForFilter = false;

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Convert.ToString(Enums.EntityType.Tactic), ClientId);

            // Get list tactic's custom field
            List<CustomField> customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == ClientId && customfield.EntityType.Equals(EntityTypeTactic)
                                                    && customfield.IsDeleted.Equals(false)).ToList();
            // Check custom field which are not set to display for filter and is required are exist
            bool CustomFieldexists = customfieldlist.Where(customfield => customfield.IsRequired && !isDisplayForFilter).Any();
            // Get drop-down type of custom fields ids
            List<int> customfieldids = customfieldlist.Where(customfield => string.Compare(customfield.CustomFieldType.Name, DropDownList, true) == 0
                                        && (isDisplayForFilter ? customfield.IsDisplayForFilter : true)).Select(customfield => customfield.CustomFieldId).ToList();
            // Get tactics only for budget model
            List<string> tacIds = BudgetModel.Where(t => string.Compare(t.ActivityType, EntityTypeTactic, true) == 0).Select(t => t.Id).ToList();

            // Get tactic ids from tactic list
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).ToList();

            // Get tactic custom fields list
            List<CustomField_Entity> lstAllTacticCustomFieldEntities = Entities.Where(customFieldEntity => customfieldids.Contains(customFieldEntity.CustomFieldId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList();
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);


            #region "Set Permissions"
            #region "Set Plan Permission"
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

            if (IsPlanEditAllAuthorized)
            {
                BudgetModel.Where(item => string.Compare(item.ActivityType, ActivityType.ActivityPlan, true) == 0)
                            .ToList().ForEach(item =>
                            {
                                item.isBudgetEditable = item.isEntityEditable = true;
                            });
            }
            else
            {
                BudgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityPlan, true) == 0) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                           .ToList().ForEach(item =>
                           {
                               item.isBudgetEditable = item.isEntityEditable = true;
                           });
            }
            #endregion

            int allwTaccount = 0;
            List<string> lstTacs = BudgetModel.Where(item => string.Compare(item.ActivityType, ActivityType.ActivityTactic, true) == 0).Select(t => t.Id).ToList();
            List<int> tIds = lstTacs.ConvertAll(s => Int32.Parse(s));
            List<int> lstAllAllowedTacIds = Common.GetEditableTacticListPO(UserId, ClientId, tIds, IsCustomFeildExist, CustomFieldexists, Entities,
                                                lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);

            #region "Set Campaign Permission"
            BudgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityCampaign, true) == 0) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                       .ToList().ForEach(item =>
                       {
                           // Check user is subordinate or user is owner
                           if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                           {
                               List<int> planTacticIds = new List<int>();
                               // To find tactic level permission ,first get program list and then get respective tactic list of program which will be used to get editable tactic list
                               List<string> modelprogramid = BudgetModel.Where(minner => string.Compare(minner.ActivityType, ActivityType.ActivityProgram, true) == 0 &&
                                                                               string.Compare(minner.ParentActivityId, item.ActivityId, true) == 0).Select(minner => minner.ActivityId).ToList();
                               planTacticIds = BudgetModel.Where(m => string.Compare(m.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
                                                                       modelprogramid.Contains(m.ParentActivityId)).Select(m => Convert.ToInt32(m.Id)).ToList();
                               allwTaccount = lstAllAllowedTacIds.Where(t => planTacticIds.Contains(t)).Count();
                               if (allwTaccount == planTacticIds.Count)
                               {
                                   item.isBudgetEditable = item.isEntityEditable = true;
                               }
                               else
                               {
                                   item.isBudgetEditable = item.isEntityEditable = false;
                               }
                           }

                       });
            #endregion

            #region "Set Program Permission"

            BudgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityProgram, true) == 0) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {

                       // Check user is subordinate or user is owner
                       if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                       {
                           List<int> planTacticIds = new List<int>();
                           // To find tactic level permission , get respective tactic list of program which will be used to get editable tactic list
                           planTacticIds = BudgetModel.Where(m => string.Compare(m.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
                                                               string.Compare(m.ParentActivityId, item.ActivityId, true) == 0).Select(m => Convert.ToInt32(m.Id)).ToList();
                           allwTaccount = lstAllAllowedTacIds.Where(t => planTacticIds.Contains(t)).Count();
                           if (allwTaccount == planTacticIds.Count)
                           {
                               item.isBudgetEditable = item.isEntityEditable = true;
                           }
                           else
                           {
                               item.isBudgetEditable = item.isEntityEditable = false;
                           }
                       }
                   });
            #endregion

            #region "Set Tactic Permission"


            BudgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityTactic, true) == 0) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {
                       // Check user is subordinate or user is owner
                       if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                       {
                           bool isLineItem = BudgetModel.Where(ent => string.Compare(ent.ParentActivityId, item.ActivityId, true) == 0 && ent.LineItemTypeId != null).Any();
                           // Check tactic is editable or not
                           if (lstAllAllowedTacIds.Any(t => t == Convert.ToInt32(item.Id)))
                           {
                               item.isBudgetEditable = item.isCostEditable = item.isEntityEditable = true;
                               if (!isLineItem)
                               {
                                   item.isActualEditable = true;
                               }
                           }
                           else
                           {
                               item.isBudgetEditable = item.isCostEditable = item.isEntityEditable = false;
                           }
                       }
                   });
            #endregion

            #region "Set LineItem Permission"
            BudgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityLineItem, true) == 0) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {
                       int tacticOwner = 0;
                       if (BudgetModel.Where(m => string.Compare(m.ActivityId, item.ParentActivityId, true) == 0).Any())
                       {
                           tacticOwner = BudgetModel.Where(m => string.Compare(m.ActivityId, item.ParentActivityId, true) == 0).FirstOrDefault().CreatedBy;
                       }

                       // Check user is subordinate or user is owner of line item or user is owner of tactic
                       if (item.CreatedBy == UserId || tacticOwner == UserId || lstSubordinatesIds.Contains(tacticOwner))
                       {
                           if (lstAllAllowedTacIds.Any(t => t == Convert.ToInt32(item.ParentId)))
                           {
                               item.isActualEditable = item.isCostEditable = item.isEntityEditable = true;
                           }
                           else
                           {
                               item.isActualEditable = item.isCostEditable = item.isEntityEditable = false;
                           }
                       }
                   });
            #endregion

            #endregion

            return BudgetModel;
        }

        /// <summary>
        /// Method to set model for LineItem
        /// </summary>
        private List<PlanBudgetModel> ManageLineItems(List<PlanBudgetModel> PBModel)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model count cannot be less than zero.");

            foreach (PlanBudgetModel l in PBModel.Where(l => string.Compare(l.ActivityType, ActivityType.ActivityTactic, true) == 0))
            {
                // Calculate Line items Difference.
                List<PlanBudgetModel> lines = PBModel.Where(line => string.Compare(line.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
                                                            string.Compare(line.ParentActivityId, l.ActivityId, true) == 0).ToList();
                PlanBudgetModel otherLine = lines.Where(ol => ol.LineItemTypeId == null).FirstOrDefault();
                lines = lines.Where(ol => ol.LineItemTypeId != null).ToList();
                // Calculate total line item difference with respective tactics
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        otherLine.MonthValues.CostY1 = l.MonthValues.CostY1 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY1) ?? 0;
                        otherLine.MonthValues.CostY2 = l.MonthValues.CostY2 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY2) ?? 0;
                        otherLine.MonthValues.CostY3 = l.MonthValues.CostY3 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY3) ?? 0;
                        otherLine.MonthValues.CostY4 = l.MonthValues.CostY4 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY4) ?? 0;
                        otherLine.MonthValues.CostY5 = l.MonthValues.CostY5 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY5) ?? 0;
                        otherLine.MonthValues.CostY6 = l.MonthValues.CostY6 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY6) ?? 0;
                        otherLine.MonthValues.CostY7 = l.MonthValues.CostY7 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY7) ?? 0;
                        otherLine.MonthValues.CostY8 = l.MonthValues.CostY8 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY8) ?? 0;
                        otherLine.MonthValues.CostY9 = l.MonthValues.CostY9 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY9) ?? 0;
                        otherLine.MonthValues.CostY10 = l.MonthValues.CostY10 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY10) ?? 0;
                        otherLine.MonthValues.CostY11 = l.MonthValues.CostY11 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY11) ?? 0;
                        otherLine.MonthValues.CostY12 = l.MonthValues.CostY12 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY12) ?? 0;

                        otherLine.NextYearMonthValues.CostY1 = l.NextYearMonthValues.CostY1 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY1) ?? 0;
                        otherLine.NextYearMonthValues.CostY2 = l.NextYearMonthValues.CostY2 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY2) ?? 0;
                        otherLine.NextYearMonthValues.CostY3 = l.NextYearMonthValues.CostY3 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY3) ?? 0;
                        otherLine.NextYearMonthValues.CostY4 = l.NextYearMonthValues.CostY4 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY4) ?? 0;
                        otherLine.NextYearMonthValues.CostY5 = l.NextYearMonthValues.CostY5 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY5) ?? 0;
                        otherLine.NextYearMonthValues.CostY6 = l.NextYearMonthValues.CostY6 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY6) ?? 0;
                        otherLine.NextYearMonthValues.CostY7 = l.NextYearMonthValues.CostY7 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY7) ?? 0;
                        otherLine.NextYearMonthValues.CostY8 = l.NextYearMonthValues.CostY8 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY8) ?? 0;
                        otherLine.NextYearMonthValues.CostY9 = l.NextYearMonthValues.CostY9 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY9) ?? 0;
                        otherLine.NextYearMonthValues.CostY10 = l.NextYearMonthValues.CostY10 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY10) ?? 0;
                        otherLine.NextYearMonthValues.CostY11 = l.NextYearMonthValues.CostY11 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY11) ?? 0;
                        otherLine.NextYearMonthValues.CostY12 = l.NextYearMonthValues.CostY12 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY12) ?? 0;


                        double allocated = l.TotalAllocatedCost - lines.Sum(l1 => l1.TotalAllocatedCost);
                        otherLine.TotalAllocatedCost = allocated;
                    }
                    else
                    {
                        otherLine.TotalActuals = l.TotalActuals;
                        otherLine.MonthValues = l.MonthValues;
                        otherLine.NextYearMonthValues = l.NextYearMonthValues;
                        otherLine.TotalAllocatedCost = l.TotalAllocatedCost < 0 ? 0 : l.TotalAllocatedCost;
                    }
                    // Calculate Balance UnAllocated Cost
                    double BalanceUnallocatedCost = l.UnallocatedCost - lines.Sum(lmon => lmon.UnallocatedCost);
                    otherLine.UnallocatedCost = BalanceUnallocatedCost;

                }
            }
            return PBModel;
        }

        /// <summary>
        /// sum up the total of planned and actuals cell of budget to child to parent
        /// </summary>        
        private List<PlanBudgetModel> CalculateBottomUp(List<PlanBudgetModel> PBModel, string ParentActivityType, string ChildActivityType, string ViewBy)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ParentActivityType != null, "Parent Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(ChildActivityType != null, "Child Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(ViewBy != null, "View By cannot be null.");

            double totalMonthCostSum = 0;

            foreach (PlanBudgetModel l in PBModel.Where(_mdl => string.Compare(_mdl.ActivityType, ParentActivityType, true) == 0))
            {
                // Check if ViewBy is Campaign selected then set weight-age value to 100;
                int weightage = 100;
                if (ViewBy != Convert.ToString(PlanGanttTypes.Tactic))
                {
                    weightage = l.Weightage;
                }
                weightage = weightage / 100;

                List<PlanBudgetModel> childs = PBModel.Where(line => string.Compare(line.ActivityType, ChildActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0).ToList();
                if (l.ActivityType != ActivityType.ActivityTactic
                        || PBModel.Where(m => string.Compare(m.ParentActivityId, l.ActivityId, true) == 0 && m.LineItemTypeId != null &&
                            string.Compare(m.ActivityType, ActivityType.ActivityLineItem, true) == 0).Any() && childs != null)
                {
                    BottonUpActualMonthlyValues(l, weightage, childs);
                }

                if (string.Compare(ParentActivityType, ActivityType.ActivityTactic, true) < 0 && childs != null)
                {
                    BottonUpCostMonthlyValues(l, weightage, childs);
                    if (childs != null)
                    {
                        totalMonthCostSum = l.MonthValues.CostY1 + l.MonthValues.CostY2 + l.MonthValues.CostY3 + l.MonthValues.CostY4 + l.MonthValues.CostY5 + l.MonthValues.CostY6 +
                                            l.MonthValues.CostY7 + l.MonthValues.CostY8 + l.MonthValues.CostY9 + l.MonthValues.CostY10 + l.MonthValues.CostY11 + l.MonthValues.CostY12 +
                                            l.NextYearMonthValues.CostY1 + l.NextYearMonthValues.CostY2 + l.NextYearMonthValues.CostY3 + l.NextYearMonthValues.CostY4 + l.NextYearMonthValues.CostY5 +
                                            l.NextYearMonthValues.CostY6 + l.NextYearMonthValues.CostY7 + l.NextYearMonthValues.CostY8 + l.NextYearMonthValues.CostY9 + l.NextYearMonthValues.CostY10 +
                                            l.NextYearMonthValues.CostY11 + l.NextYearMonthValues.CostY12;
                    }
                }

                BottonUpBudgetMonthlyValues(l, childs);

                if (l.ActivityType != ActivityType.ActivityTactic)
                {
                    l.TotalAllocatedCost = PBModel.Where(line => string.Compare(line.ActivityType, ChildActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0)
                                            .Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    l.UnallocatedCost = l.TotalAllocatedCost - totalMonthCostSum;
                }
                if (l.ActivityType != ActivityType.ActivityTactic || PBModel.Where(m => string.Compare(m.ParentActivityId, l.ActivityId, true) == 0 && m.LineItemTypeId != null &&
                        string.Compare(m.ActivityType, ActivityType.ActivityLineItem, true) == 0).Any())
                {
                    l.TotalActuals = PBModel.Where(line => string.Compare(line.ActivityType, ChildActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0)
                                    .Sum(line => (double?)line.TotalActuals) ?? 0;
                }
            }

            return PBModel;
        }

        /// <summary>
        /// sum up the total of budget to child to parent
        /// </summary>
        private static void BottonUpBudgetMonthlyValues(PlanBudgetModel PBModel, List<PlanBudgetModel> Childs)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");

            if (Childs != null)
            {
                PBModel.ChildMonthValues.BudgetY1 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY1)) ?? 0;
                PBModel.ChildMonthValues.BudgetY2 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY2)) ?? 0;
                PBModel.ChildMonthValues.BudgetY3 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY3)) ?? 0;
                PBModel.ChildMonthValues.BudgetY4 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY4)) ?? 0;
                PBModel.ChildMonthValues.BudgetY5 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY5)) ?? 0;
                PBModel.ChildMonthValues.BudgetY6 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY6)) ?? 0;
                PBModel.ChildMonthValues.BudgetY7 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY7)) ?? 0;
                PBModel.ChildMonthValues.BudgetY8 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY8)) ?? 0;
                PBModel.ChildMonthValues.BudgetY9 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY9)) ?? 0;
                PBModel.ChildMonthValues.BudgetY10 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY10)) ?? 0;
                PBModel.ChildMonthValues.BudgetY11 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY11)) ?? 0;
                PBModel.ChildMonthValues.BudgetY12 = Childs.Sum(line => (double?)(line.MonthValues.BudgetY12)) ?? 0;

                PBModel.ChildNextYearMonthValues.BudgetY1 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY1)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY2 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY2)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY3 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY3)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY4 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY4)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY5 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY5)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY6 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY6)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY7 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY7)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY8 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY8)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY9 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY9)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY10 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY10)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY11 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY11)) ?? 0;
                PBModel.ChildNextYearMonthValues.BudgetY12 = Childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY12)) ?? 0;
            }
            else
            {
                PBModel.ChildMonthValues.BudgetY1 = 0;
                PBModel.ChildMonthValues.BudgetY2 = 0;
                PBModel.ChildMonthValues.BudgetY3 = 0;
                PBModel.ChildMonthValues.BudgetY4 = 0;
                PBModel.ChildMonthValues.BudgetY5 = 0;
                PBModel.ChildMonthValues.BudgetY6 = 0;
                PBModel.ChildMonthValues.BudgetY7 = 0;
                PBModel.ChildMonthValues.BudgetY8 = 0;
                PBModel.ChildMonthValues.BudgetY9 = 0;
                PBModel.ChildMonthValues.BudgetY10 = 0;
                PBModel.ChildMonthValues.BudgetY11 = 0;
                PBModel.ChildMonthValues.BudgetY12 = 0;

                PBModel.ChildNextYearMonthValues.BudgetY1 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY2 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY3 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY4 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY5 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY6 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY7 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY8 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY9 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY10 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY11 = 0;
                PBModel.ChildNextYearMonthValues.BudgetY12 = 0;
            }
        }

        /// <summary>
        /// sum up the total of planned cost to child to parent
        /// </summary>
        private static void BottonUpCostMonthlyValues(PlanBudgetModel PBModel, int Weightage, List<PlanBudgetModel> Childs)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(Weightage > 0, "Weight-age cannot be less than zero.");

            if (Childs != null)
            {
                PBModel.MonthValues.CostY1 = Childs.Sum(line => (double?)(line.MonthValues.CostY1 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY2 = Childs.Sum(line => (double?)(line.MonthValues.CostY2 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY3 = Childs.Sum(line => (double?)(line.MonthValues.CostY3 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY4 = Childs.Sum(line => (double?)(line.MonthValues.CostY4 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY5 = Childs.Sum(line => (double?)(line.MonthValues.CostY5 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY6 = Childs.Sum(line => (double?)(line.MonthValues.CostY6 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY7 = Childs.Sum(line => (double?)(line.MonthValues.CostY7 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY8 = Childs.Sum(line => (double?)(line.MonthValues.CostY8 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY9 = Childs.Sum(line => (double?)(line.MonthValues.CostY9 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY10 = Childs.Sum(line => (double?)(line.MonthValues.CostY10 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY11 = Childs.Sum(line => (double?)(line.MonthValues.CostY11 * Weightage)) ?? 0;
                PBModel.MonthValues.CostY12 = Childs.Sum(line => (double?)(line.MonthValues.CostY12 * Weightage)) ?? 0;


                PBModel.NextYearMonthValues.CostY1 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY1 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY2 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY2 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY3 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY3 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY4 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY4 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY5 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY5 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY6 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY6 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY7 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY7 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY8 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY8 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY9 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY9 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY10 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY10 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY11 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY11 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.CostY12 = Childs.Sum(line => (double?)(line.NextYearMonthValues.CostY12 * Weightage)) ?? 0;
            }
            else
            {
                PBModel.MonthValues.CostY1 = 0;
                PBModel.MonthValues.CostY2 = 0;
                PBModel.MonthValues.CostY3 = 0;
                PBModel.MonthValues.CostY4 = 0;
                PBModel.MonthValues.CostY5 = 0;
                PBModel.MonthValues.CostY6 = 0;
                PBModel.MonthValues.CostY7 = 0;
                PBModel.MonthValues.CostY8 = 0;
                PBModel.MonthValues.CostY9 = 0;
                PBModel.MonthValues.CostY10 = 0;
                PBModel.MonthValues.CostY11 = 0;
                PBModel.MonthValues.CostY12 = 0;

                PBModel.NextYearMonthValues.CostY1 = 0;
                PBModel.NextYearMonthValues.CostY2 = 0;
                PBModel.NextYearMonthValues.CostY3 = 0;
                PBModel.NextYearMonthValues.CostY4 = 0;
                PBModel.NextYearMonthValues.CostY5 = 0;
                PBModel.NextYearMonthValues.CostY6 = 0;
                PBModel.NextYearMonthValues.CostY7 = 0;
                PBModel.NextYearMonthValues.CostY8 = 0;
                PBModel.NextYearMonthValues.CostY9 = 0;
                PBModel.NextYearMonthValues.CostY10 = 0;
                PBModel.NextYearMonthValues.CostY11 = 0;
                PBModel.NextYearMonthValues.CostY12 = 0;
            }
        }

        /// <summary>
        /// sum up the total of actual cost to child to parent
        /// </summary>
        private static void BottonUpActualMonthlyValues(PlanBudgetModel PBModel, int Weightage, List<PlanBudgetModel> Childs)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(Weightage > 0, "Weight-age cannot be less than zero.");

            if (Childs != null)
            {
                PBModel.MonthValues.ActualY1 = Childs.Sum(line => (double?)(line.MonthValues.ActualY1 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY2 = Childs.Sum(line => (double?)(line.MonthValues.ActualY2 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY3 = Childs.Sum(line => (double?)(line.MonthValues.ActualY3 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY4 = Childs.Sum(line => (double?)(line.MonthValues.ActualY4 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY5 = Childs.Sum(line => (double?)(line.MonthValues.ActualY5 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY6 = Childs.Sum(line => (double?)(line.MonthValues.ActualY6 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY7 = Childs.Sum(line => (double?)(line.MonthValues.ActualY7 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY8 = Childs.Sum(line => (double?)(line.MonthValues.ActualY8 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY9 = Childs.Sum(line => (double?)(line.MonthValues.ActualY9 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY10 = Childs.Sum(line => (double?)(line.MonthValues.ActualY10 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY11 = Childs.Sum(line => (double?)(line.MonthValues.ActualY11 * Weightage)) ?? 0;
                PBModel.MonthValues.ActualY12 = Childs.Sum(line => (double?)(line.MonthValues.ActualY12 * Weightage)) ?? 0;

                PBModel.NextYearMonthValues.ActualY1 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY1 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY2 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY2 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY3 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY3 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY4 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY4 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY5 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY5 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY6 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY6 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY7 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY7 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY8 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY8 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY9 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY9 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY10 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY10 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY11 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY11 * Weightage)) ?? 0;
                PBModel.NextYearMonthValues.ActualY12 = Childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY12 * Weightage)) ?? 0;
            }
            else
            {
                PBModel.MonthValues.ActualY1 = 0;
                PBModel.MonthValues.ActualY2 = 0;
                PBModel.MonthValues.ActualY3 = 0;
                PBModel.MonthValues.ActualY4 = 0;
                PBModel.MonthValues.ActualY5 = 0;
                PBModel.MonthValues.ActualY6 = 0;
                PBModel.MonthValues.ActualY7 = 0;
                PBModel.MonthValues.ActualY8 = 0;
                PBModel.MonthValues.ActualY9 = 0;
                PBModel.MonthValues.ActualY10 = 0;
                PBModel.MonthValues.ActualY11 = 0;
                PBModel.MonthValues.ActualY12 = 0;

                PBModel.NextYearMonthValues.ActualY1 = 0;
                PBModel.NextYearMonthValues.ActualY2 = 0;
                PBModel.NextYearMonthValues.ActualY3 = 0;
                PBModel.NextYearMonthValues.ActualY4 = 0;
                PBModel.NextYearMonthValues.ActualY5 = 0;
                PBModel.NextYearMonthValues.ActualY6 = 0;
                PBModel.NextYearMonthValues.ActualY7 = 0;
                PBModel.NextYearMonthValues.ActualY8 = 0;
                PBModel.NextYearMonthValues.ActualY9 = 0;
                PBModel.NextYearMonthValues.ActualY10 = 0;
                PBModel.NextYearMonthValues.ActualY11 = 0;
                PBModel.NextYearMonthValues.ActualY12 = 0;
            }
        }

        /// <summary>
        /// apply weight-age to budget cell values
        /// </summary>
        private List<PlanBudgetModel> SetLineItemCostByWeightage(List<PlanBudgetModel> PBModel, string ViewBy)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(PBModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(ViewBy != null, "View By cannot be null.");

            int weightage = 100;
            foreach (PlanBudgetModel l in PBModel.Where(_mdl => string.Compare(_mdl.ActivityType, ActivityType.ActivityTactic, true) == 0))
            {
                List<PlanBudgetModel> lstLineItems = PBModel.Where(line => string.Compare(line.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
                                                        string.Compare(line.ParentActivityId, l.ActivityId, true) == 0).ToList();

                // Check if ViewBy is Campaign selected then set weight-age value to 100;
                if (ViewBy != Convert.ToString(PlanGanttTypes.Tactic))
                {
                    weightage = l.Weightage;
                }
                BudgetMonth lineBudget;
                BudgetMonth lineNextYearBudget;
                foreach (PlanBudgetModel line in lstLineItems)
                {
                    lineBudget = new BudgetMonth();
                    lineNextYearBudget = new BudgetMonth();

                    SetActualLineItembyWeightage(weightage, lineBudget, lineNextYearBudget, line);

                    SetCostLineItembyWeightage(weightage, lineBudget, lineNextYearBudget, line);

                    SetBudgetLineItembyWeightage(weightage, lineBudget, lineNextYearBudget, line);

                    line.MonthValues = lineBudget;
                    line.NextYearMonthValues = lineNextYearBudget;
                }
            }
            return PBModel;
        }

        /// <summary>
        /// apply weight-age to budget cell values
        /// </summary>
        private static void SetBudgetLineItembyWeightage(int Weightage, BudgetMonth LineBudget, BudgetMonth LineNextYearBudget, PlanBudgetModel PBModel)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(Weightage >= 0, "Weight-age cannot be less than zero.");
            
            LineBudget.BudgetY1 = (double)(PBModel.MonthValues.BudgetY1 * Weightage) / 100;
            LineBudget.BudgetY2 = (double)(PBModel.MonthValues.BudgetY2 * Weightage) / 100;
            LineBudget.BudgetY3 = (double)(PBModel.MonthValues.BudgetY3 * Weightage) / 100;
            LineBudget.BudgetY4 = (double)(PBModel.MonthValues.BudgetY4 * Weightage) / 100;
            LineBudget.BudgetY5 = (double)(PBModel.MonthValues.BudgetY5 * Weightage) / 100;
            LineBudget.BudgetY6 = (double)(PBModel.MonthValues.BudgetY6 * Weightage) / 100;
            LineBudget.BudgetY7 = (double)(PBModel.MonthValues.BudgetY7 * Weightage) / 100;
            LineBudget.BudgetY8 = (double)(PBModel.MonthValues.BudgetY8 * Weightage) / 100;
            LineBudget.BudgetY9 = (double)(PBModel.MonthValues.BudgetY9 * Weightage) / 100;
            LineBudget.BudgetY10 = (double)(PBModel.MonthValues.BudgetY10 * Weightage) / 100;
            LineBudget.BudgetY11 = (double)(PBModel.MonthValues.BudgetY11 * Weightage) / 100;
            LineBudget.BudgetY12 = (double)(PBModel.MonthValues.BudgetY12 * Weightage) / 100;

            LineNextYearBudget.BudgetY1 = (double)(PBModel.NextYearMonthValues.BudgetY1 * Weightage) / 100;
            LineNextYearBudget.BudgetY2 = (double)(PBModel.NextYearMonthValues.BudgetY2 * Weightage) / 100;
            LineNextYearBudget.BudgetY3 = (double)(PBModel.NextYearMonthValues.BudgetY3 * Weightage) / 100;
            LineNextYearBudget.BudgetY4 = (double)(PBModel.NextYearMonthValues.BudgetY4 * Weightage) / 100;
            LineNextYearBudget.BudgetY5 = (double)(PBModel.NextYearMonthValues.BudgetY5 * Weightage) / 100;
            LineNextYearBudget.BudgetY6 = (double)(PBModel.NextYearMonthValues.BudgetY6 * Weightage) / 100;
            LineNextYearBudget.BudgetY7 = (double)(PBModel.NextYearMonthValues.BudgetY7 * Weightage) / 100;
            LineNextYearBudget.BudgetY8 = (double)(PBModel.NextYearMonthValues.BudgetY8 * Weightage) / 100;
            LineNextYearBudget.BudgetY9 = (double)(PBModel.NextYearMonthValues.BudgetY9 * Weightage) / 100;
            LineNextYearBudget.BudgetY10 = (double)(PBModel.NextYearMonthValues.BudgetY10 * Weightage) / 100;
            LineNextYearBudget.BudgetY11 = (double)(PBModel.NextYearMonthValues.BudgetY11 * Weightage) / 100;
            LineNextYearBudget.BudgetY12 = (double)(PBModel.NextYearMonthValues.BudgetY12 * Weightage) / 100;
        }

        /// <summary>
        /// apply weight-age to planned cell values
        /// </summary>
        private static void SetCostLineItembyWeightage(int Weightage, BudgetMonth LineBudget, BudgetMonth LineNextYearBudget, PlanBudgetModel PBModel)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(Weightage >= 0, "Weight-age cannot be less than zero.");

            LineBudget.CostY1 = (double)(PBModel.MonthValues.CostY1 * Weightage) / 100;
            LineBudget.CostY2 = (double)(PBModel.MonthValues.CostY2 * Weightage) / 100;
            LineBudget.CostY3 = (double)(PBModel.MonthValues.CostY3 * Weightage) / 100;
            LineBudget.CostY4 = (double)(PBModel.MonthValues.CostY4 * Weightage) / 100;
            LineBudget.CostY5 = (double)(PBModel.MonthValues.CostY5 * Weightage) / 100;
            LineBudget.CostY6 = (double)(PBModel.MonthValues.CostY6 * Weightage) / 100;
            LineBudget.CostY7 = (double)(PBModel.MonthValues.CostY7 * Weightage) / 100;
            LineBudget.CostY8 = (double)(PBModel.MonthValues.CostY8 * Weightage) / 100;
            LineBudget.CostY9 = (double)(PBModel.MonthValues.CostY9 * Weightage) / 100;
            LineBudget.CostY10 = (double)(PBModel.MonthValues.CostY10 * Weightage) / 100;
            LineBudget.CostY11 = (double)(PBModel.MonthValues.CostY11 * Weightage) / 100;
            LineBudget.CostY12 = (double)(PBModel.MonthValues.CostY12 * Weightage) / 100;

            LineNextYearBudget.CostY1 = (double)(PBModel.NextYearMonthValues.CostY1 * Weightage) / 100;
            LineNextYearBudget.CostY2 = (double)(PBModel.NextYearMonthValues.CostY2 * Weightage) / 100;
            LineNextYearBudget.CostY3 = (double)(PBModel.NextYearMonthValues.CostY3 * Weightage) / 100;
            LineNextYearBudget.CostY4 = (double)(PBModel.NextYearMonthValues.CostY4 * Weightage) / 100;
            LineNextYearBudget.CostY5 = (double)(PBModel.NextYearMonthValues.CostY5 * Weightage) / 100;
            LineNextYearBudget.CostY6 = (double)(PBModel.NextYearMonthValues.CostY6 * Weightage) / 100;
            LineNextYearBudget.CostY7 = (double)(PBModel.NextYearMonthValues.CostY7 * Weightage) / 100;
            LineNextYearBudget.CostY8 = (double)(PBModel.NextYearMonthValues.CostY8 * Weightage) / 100;
            LineNextYearBudget.CostY9 = (double)(PBModel.NextYearMonthValues.CostY9 * Weightage) / 100;
            LineNextYearBudget.CostY10 = (double)(PBModel.NextYearMonthValues.CostY10 * Weightage) / 100;
            LineNextYearBudget.CostY11 = (double)(PBModel.NextYearMonthValues.CostY11 * Weightage) / 100;
            LineNextYearBudget.CostY12 = (double)(PBModel.NextYearMonthValues.CostY12 * Weightage) / 100;
        }

        /// <summary>
        /// apply weight-age to actual cell values
        /// </summary>
        private static void SetActualLineItembyWeightage(int Weightage, BudgetMonth LineBudget, BudgetMonth LineNextYearBudget, PlanBudgetModel PBModel)
        {
            Contract.Requires<ArgumentNullException>(PBModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(LineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(Weightage >= 0, "Weight-age cannot be less than zero.");

            LineBudget.ActualY1 = (double)(PBModel.MonthValues.ActualY1 * Weightage) / 100;
            LineBudget.ActualY2 = (double)(PBModel.MonthValues.ActualY2 * Weightage) / 100;
            LineBudget.ActualY3 = (double)(PBModel.MonthValues.ActualY3 * Weightage) / 100;
            LineBudget.ActualY4 = (double)(PBModel.MonthValues.ActualY4 * Weightage) / 100;
            LineBudget.ActualY5 = (double)(PBModel.MonthValues.ActualY5 * Weightage) / 100;
            LineBudget.ActualY6 = (double)(PBModel.MonthValues.ActualY6 * Weightage) / 100;
            LineBudget.ActualY7 = (double)(PBModel.MonthValues.ActualY7 * Weightage) / 100;
            LineBudget.ActualY8 = (double)(PBModel.MonthValues.ActualY8 * Weightage) / 100;
            LineBudget.ActualY9 = (double)(PBModel.MonthValues.ActualY9 * Weightage) / 100;
            LineBudget.ActualY10 = (double)(PBModel.MonthValues.ActualY10 * Weightage) / 100;
            LineBudget.ActualY11 = (double)(PBModel.MonthValues.ActualY11 * Weightage) / 100;
            LineBudget.ActualY12 = (double)(PBModel.MonthValues.ActualY12 * Weightage) / 100;

            LineNextYearBudget.ActualY1 = (double)(PBModel.NextYearMonthValues.ActualY1 * Weightage) / 100;
            LineNextYearBudget.ActualY2 = (double)(PBModel.NextYearMonthValues.ActualY2 * Weightage) / 100;
            LineNextYearBudget.ActualY3 = (double)(PBModel.NextYearMonthValues.ActualY3 * Weightage) / 100;
            LineNextYearBudget.ActualY4 = (double)(PBModel.NextYearMonthValues.ActualY4 * Weightage) / 100;
            LineNextYearBudget.ActualY5 = (double)(PBModel.NextYearMonthValues.ActualY5 * Weightage) / 100;
            LineNextYearBudget.ActualY6 = (double)(PBModel.NextYearMonthValues.ActualY6 * Weightage) / 100;
            LineNextYearBudget.ActualY7 = (double)(PBModel.NextYearMonthValues.ActualY7 * Weightage) / 100;
            LineNextYearBudget.ActualY8 = (double)(PBModel.NextYearMonthValues.ActualY8 * Weightage) / 100;
            LineNextYearBudget.ActualY9 = (double)(PBModel.NextYearMonthValues.ActualY9 * Weightage) / 100;
            LineNextYearBudget.ActualY10 = (double)(PBModel.NextYearMonthValues.ActualY10 * Weightage) / 100;
            LineNextYearBudget.ActualY11 = (double)(PBModel.NextYearMonthValues.ActualY11 * Weightage) / 100;
            LineNextYearBudget.ActualY12 = (double)(PBModel.NextYearMonthValues.ActualY12 * Weightage) / 100;
        }

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket 2585
        /// </summary>
        /// <param name="BudgetModel"></param>
        /// <param name="Year"></param>
        /// <returns></returns>
        private List<PlanBudgetModel> FilterPlanByTimeFrame(List<PlanBudgetModel> BudgetModel, string Year)
        {
            Contract.Requires<ArgumentNullException>(BudgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(BudgetModel.Count > 0, "Budget Model Count cannot be less than zero.");

            foreach (PlanBudgetModel objPlan in BudgetModel.Where(p => string.Compare(p.ActivityType, ActivityType.ActivityPlan, true) == 0).ToList())
            {
                if (!BudgetModel.Where(ent => string.Compare(ent.ParentActivityId, objPlan.ActivityId, true) == 0).Any())
                {
                    int planId = Convert.ToInt32(objPlan.Id);
                    bool isChildExist = objDbMrpEntities.Plan_Campaign.Where(p => p.PlanId == planId && p.IsDeleted == false).Any();
                    if (isChildExist)
                    {
                        BudgetModel.Remove(objPlan);
                    }
                    else
                    {
                        string firstYear = Common.GetInitialYearFromTimeFrame(Year);
                        string lastYear = firstYear;
                        if (isMultiYear)
                        {
                            lastYear = Convert.ToString(Convert.ToInt32(firstYear) + 1);
                        }
                        if (objPlan.PlanYear != firstYear && objPlan.PlanYear != lastYear)
                        {
                            BudgetModel.Remove(objPlan);
                        }
                    }
                }
            }
            return BudgetModel;
        }


    }

}
