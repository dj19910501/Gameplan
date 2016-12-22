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
        public BudgetDHTMLXGridModel GetBudget(int clientId, int userID, string planIds, double planExchangeRate, string viewBy, string year = "", string customFieldIds = "", string ownerIds = "", string tacticTypeIds = "", string statusIds = "", string searchText = "", string searchBy = "", bool isFromCache = false)
        {
            Contract.Requires<ArgumentNullException>(clientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(userID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(planIds != null, "At-least one plan should be selected");
            Contract.Requires<ArgumentNullException>(planExchangeRate > 0, "Plan Exchange Rate cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(viewBy != null, "ViewBy cannot be null.");

            string strThisQuarter = Convert.ToString(Enums.UpcomingActivities.ThisYearQuaterly);
            string strThisMonth = Convert.ToString(Enums.UpcomingActivities.ThisYearMonthly);
            // Set actual for quarters
            string AllocatedBy = Convert.ToString(Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)]).ToLower();
            // Check time frame selected for this year quarterly or this year monthly data and for this year option isMultiyear flag will always be false
            if (string.Compare(year, strThisQuarter, true) == 0)
            {
                isMultiYear = false;
                year = Convert.ToString(DateTime.Now.Year);
            }
            else if (string.Compare(year, strThisMonth, true) == 0)
            {
                isMultiYear = false;
                AllocatedBy = Convert.ToString(Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.months)]).ToLower();
                year = Convert.ToString(DateTime.Now.Year);
            }
            else
            {
                isMultiYear = Common.IsMultiyearTimeframe(year);
            }
            List<PlanBudgetModel> GridDataList = new List<PlanBudgetModel>();
            List<PlanBudgetModel> PBModel = new List<PlanBudgetModel>(); // PBModel = Plan Budget Model
            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel();
            if (isFromCache) // Create Model from Cache
                GridDataList = (List<PlanBudgetModel>)objCache.Returncache(Convert.ToString(Enums.CacheObject.AllBudgetGridData));
            if (GridDataList == null || GridDataList.Count() == 0)
            {
                DataTable dtBudget = objSp.GetBudget(planIds, userID, viewBy, ownerIds, tacticTypeIds, statusIds, year); // Get budget data for budget,planned cost and actual using store procedure GetplanBudget

                PBModel = CreateBudgetDataModel(dtBudget, planExchangeRate); // Convert data table with budget data to PlanBudgetModel model

                PBModel = FilterPlanByTimeFrame(PBModel, year); // Except plan level entity be filter at Db level so we remove plan object by applying time frame filter.  

                List<int> CustomFieldFilteredTacticIds = FilterCustomField(PBModel, customFieldIds);

                // filter budget model by custom field filter list
                if (CustomFieldFilteredTacticIds != null && CustomFieldFilteredTacticIds.Count > 0)
                {
                    PBModel.RemoveAll(a => string.Compare(a.ActivityType, ActivityType.ActivityTactic, true) == 0 && !CustomFieldFilteredTacticIds.Contains(Convert.ToInt32(a.Id)));
                }
                PBModel = SetCustomFieldRestriction(PBModel, userID, clientId); // Set custom field permission for budget cells. budget cell will editable or not.

                PBModel = ManageLineItems(PBModel); // Manage line items unallocated cost values in other line item

                #region "Calculate Monthly Budget from Bottom to Top for Hierarchy level like: LineItem > Tactic > Program > Campaign > CustomField(if filtered) > Plan"

                // Set ViewBy data to model.
                PBModel = RollUp(PBModel, viewBy);

                #endregion
                if (viewBy.Contains(Convert.ToString(PlanGanttTypes.Custom)))
                {
                    PBModel = SetLineItemCostByWeightage(PBModel, viewBy); // Set LineItem monthly budget cost by it's parent tactic weight-age.
                }
                objCache.AddCache(Convert.ToString(Enums.CacheObject.AllBudgetGridData), PBModel);
            }
            else
            {
                PBModel = GridDataList;
            }
            objBudgetDHTMLXGrid = GenerateHeaderString(AllocatedBy, objBudgetDHTMLXGrid, year); // Create header of model
            if (!string.IsNullOrEmpty(searchText)) // Searching Text
            {
                List<PlanBudgetModel> SearchlistData;
                if (string.IsNullOrEmpty(searchBy) || string.Compare(searchBy, Convert.ToString(Enums.GlobalSearch.ActivityName), true) == 0)
                {
                    SearchlistData = PBModel.Where(a => a.ActivityName.ToLower().Contains(HttpUtility.HtmlEncode(searchText.Trim().ToLower()))).ToList();
                }
                else
                {
                    SearchlistData = PBModel.Where(a => a.MachineName.ToLower().Contains(HttpUtility.HtmlEncode(searchText.Trim().ToLower()))).ToList();
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
            objBudgetDHTMLXGrid = CreateDhtmlxFormattedBudgetData(objBudgetDHTMLXGrid, PBModel, AllocatedBy, userID, clientId, year, viewBy); // Create model to bind data in grid as per DHTMLx grid format.
            // Get number of tab views for user in column management
            bool isPlangrid = false;
            bool isSelectAll = false;
            List<ColumnViewEntity> userManagedColumns = objColumnView.GetCustomfieldModel(clientId, isPlangrid, out isSelectAll, userID);
            string hiddenTab = string.Empty;
            if (!userManagedColumns.Where(u => u.EntityIsChecked).Any())
            {
                ColumnViewEntity PlannedColumn = userManagedColumns.Where(u => string.Compare(u.EntityType, Convert.ToString(Enums.Budgetcolumn.Planned), true) == 0).FirstOrDefault();
                if (PlannedColumn != null)
                {
                    PlannedColumn.EntityIsChecked = true;
                }
            }
            hiddenTab = string.Join(",", userManagedColumns.Where(u => !u.EntityIsChecked).Select(u => u.EntityType).ToList());

            objBudgetDHTMLXGrid.HiddenTab = hiddenTab;


            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method for roll-up bottom up values as per view by
        /// </summary>        
        public List<PlanBudgetModel> RollUp(List<PlanBudgetModel> budgetModel, string viewBy)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model item count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(viewBy != null, "ViewBy cannot be null.");

            List<PlanBudgetModel> Finalmodel = new List<PlanBudgetModel>();
            Finalmodel = CalculateBottomUp(budgetModel, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, viewBy); // Calculate monthly Tactic budget from it's child budget i.e LineItem

            Finalmodel = CalculateBottomUp(budgetModel, ActivityType.ActivityProgram, ActivityType.ActivityTactic, viewBy); // Calculate monthly Program budget from it's child budget i.e Tactic

            Finalmodel = CalculateBottomUp(budgetModel, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, viewBy); // Calculate monthly Campaign budget from it's child budget i.e Program

            Finalmodel = CalculateBottomUp(budgetModel, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, viewBy); // Calculate monthly Plan budget from it's child budget i.e Campaign

            return Finalmodel;
        }

        /// <summary>
        /// Method to Filter Data by Custom Field Selection
        /// </summary>        
        public List<int> FilterCustomField(List<PlanBudgetModel> budgetModel, string customFieldFilter)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model item count cannot be less than zero.");

            List<int> lstTacticIds = new List<int>();
            if (budgetModel != null && budgetModel.Count > 0)
            {
                #region "Declare & Initialize local Variables"
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                string tacticType = Convert.ToString(Enums.EntityType.Tactic).ToUpper();
                string[] filteredCustomFields = string.IsNullOrWhiteSpace(customFieldFilter) ? null : customFieldFilter.Split(',');
                List<PlanBudgetModel> tacData = budgetModel.Where(tac => string.Compare(tac.ActivityType, tacticType, true) == 0).ToList();
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
        public List<PlanBudgetModel> CreateBudgetDataModel(DataTable dtBudget, double planExchangeRate)
        {
            Contract.Requires<ArgumentNullException>(dtBudget != null, "Budget Data Table cannot be null.");
            Contract.Requires<ArgumentNullException>(dtBudget.Rows.Count > 0, "Budget Data Table rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(planExchangeRate > 0, "Plan Exchange Rate Filter cannot be null.");

            List<PlanBudgetModel> model = new List<PlanBudgetModel>();
            if (dtBudget != null)
            {
                model = dtBudget.AsEnumerable().Select(row => new PlanBudgetModel
                {
                    Id = Convert.ToString(row["Id"]),
                    TaskId = Convert.ToString(row["TaskId"]),
                    ParentId = Convert.ToString(row["ParentActivityId"]),
                    ActivityId = Convert.ToString(row["TaskId"]),
                    ActivityName = Convert.ToString(row["Title"]),
                    ActivityType = Convert.ToString(row["ActivityType"]),
                    ParentActivityId = Convert.ToString(row["ParentTaskId"]),
                    YearlyBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Budget"])), planExchangeRate),
                    TotalUnallocatedBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalUnallocatedBudget"])), planExchangeRate),
                    TotalActuals = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalAllocationActual"])), planExchangeRate),
                    TotalAllocatedCost = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Cost"])), planExchangeRate),
                    UnallocatedCost = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalUnAllocationCost"])), planExchangeRate),
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
                        BudgetY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y2"])), planExchangeRate),
                        BudgetY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y1"])), planExchangeRate),
                        BudgetY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y3"])), planExchangeRate),
                        BudgetY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y4"])), planExchangeRate),
                        BudgetY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y5"])), planExchangeRate),
                        BudgetY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y6"])), planExchangeRate),
                        BudgetY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y7"])), planExchangeRate),
                        BudgetY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y8"])), planExchangeRate),
                        BudgetY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y9"])), planExchangeRate),
                        BudgetY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y10"])), planExchangeRate),
                        BudgetY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y11"])), planExchangeRate),
                        BudgetY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y12"])), planExchangeRate),

                        // Cost Month Allocation
                        CostY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY2"])), planExchangeRate),
                        CostY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY1"])), planExchangeRate),
                        CostY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY3"])), planExchangeRate),
                        CostY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY4"])), planExchangeRate),
                        CostY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY5"])), planExchangeRate),
                        CostY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY6"])), planExchangeRate),
                        CostY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY7"])), planExchangeRate),
                        CostY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY8"])), planExchangeRate),
                        CostY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY9"])), planExchangeRate),
                        CostY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY10"])), planExchangeRate),
                        CostY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY11"])), planExchangeRate),
                        CostY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY12"])), planExchangeRate),

                        // Actuals Month Allocation
                        ActualY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY2"])), planExchangeRate),
                        ActualY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY1"])), planExchangeRate),
                        ActualY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY3"])), planExchangeRate),
                        ActualY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY4"])), planExchangeRate),
                        ActualY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY5"])), planExchangeRate),
                        ActualY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY6"])), planExchangeRate),
                        ActualY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY7"])), planExchangeRate),
                        ActualY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY8"])), planExchangeRate),
                        ActualY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY9"])), planExchangeRate),
                        ActualY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY10"])), planExchangeRate),
                        ActualY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY11"])), planExchangeRate),
                        ActualY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY12"])), planExchangeRate)
                    },
                    NextYearMonthValues = new BudgetMonth()
                    {
                        // Budget Month Allocation
                        BudgetY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y14"])), planExchangeRate),
                        BudgetY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y13"])), planExchangeRate),
                        BudgetY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y15"])), planExchangeRate),
                        BudgetY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y16"])), planExchangeRate),
                        BudgetY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y17"])), planExchangeRate),
                        BudgetY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y18"])), planExchangeRate),
                        BudgetY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y19"])), planExchangeRate),
                        BudgetY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y20"])), planExchangeRate),
                        BudgetY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y21"])), planExchangeRate),
                        BudgetY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y22"])), planExchangeRate),
                        BudgetY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y23"])), planExchangeRate),
                        BudgetY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y24"])), planExchangeRate),

                        // Cost Month Allocation
                        CostY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY14"])), planExchangeRate),
                        CostY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY13"])), planExchangeRate),
                        CostY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY15"])), planExchangeRate),
                        CostY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY16"])), planExchangeRate),
                        CostY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY17"])), planExchangeRate),
                        CostY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY18"])), planExchangeRate),
                        CostY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY19"])), planExchangeRate),
                        CostY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY20"])), planExchangeRate),
                        CostY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY21"])), planExchangeRate),
                        CostY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY22"])), planExchangeRate),
                        CostY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY23"])), planExchangeRate),
                        CostY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY24"])), planExchangeRate),

                        // Actuals Month Allocation
                        ActualY2 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY14"])), planExchangeRate),
                        ActualY1 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY13"])), planExchangeRate),
                        ActualY3 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY15"])), planExchangeRate),
                        ActualY4 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY16"])), planExchangeRate),
                        ActualY5 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY17"])), planExchangeRate),
                        ActualY6 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY18"])), planExchangeRate),
                        ActualY7 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY19"])), planExchangeRate),
                        ActualY8 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY20"])), planExchangeRate),
                        ActualY9 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY21"])), planExchangeRate),
                        ActualY10 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY22"])), planExchangeRate),
                        ActualY11 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY23"])), planExchangeRate),
                        ActualY12 = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY24"])), planExchangeRate)
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
        public List<Budgetdataobj> SetBudgetDhtmlxFormattedValues(List<PlanBudgetModel> budgetModel, PlanBudgetModel entity, string ownerName, string entityType, string allocatedBy, bool isNextYear, bool isMultiyearPlan, string dhtmlxGridRowId, bool isAddEntityRights, bool isViewBy = false, string pcptid = "", string tacticType = "")  // pcptid = Plan-Campaign-Program-Tactic-Id
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(entityType != null, "Entity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");

            List<Budgetdataobj> BudgetDataObjList = new List<Budgetdataobj>();
            Budgetdataobj BudgetDataObj = new Budgetdataobj();
            string Roistring = string.Empty;
            string PackageTacticIds = entity.CalendarHoneycombpackageIDs;
            BudgetDataObj.value = entity.Id;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = entity.ActivityType;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = entity.MachineName;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Convert.ToString(entity.LinkTacticId);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.style = "background-color:#" + entity.ColorCode;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add LineItemTypeId into DHTMLx model
            BudgetDataObj.value = Convert.ToString(entity.LineItemTypeId);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add title of plan entity into DHTMLx model

            bool IsExtendedTactic = (entity.EndDate.Year - entity.StartDate.Year) > 0 ? true : false;
            int? LinkedTacticId = entity.LinkTacticId;
            if (LinkedTacticId == 0)
            {
                LinkedTacticId = null;
            }
            string Linkedstring = string.Empty;
            if (string.Compare(entity.ActivityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
            {
                Linkedstring = (((IsExtendedTactic && LinkedTacticId == null) ?
                                    "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>" :
                                        ((IsExtendedTactic && LinkedTacticId != null) || (LinkedTacticId != null)) ?
                                        "<div class='unlink-icon unlink-icon-grid'  LinkedPlanName='" + (string.IsNullOrEmpty(entity.LinkedPlanName) ?
                                        null :
                                    entity.LinkedPlanName.Replace("'", "&#39;")) + "' id = 'LinkIcon' ><i class='fa fa-link'></i></div>" : ""));
            }

            if (entity.AnchorTacticID != null && entity.AnchorTacticID > 0 && !string.IsNullOrEmpty(entity.Id) && string.Compare(Convert.ToString(entity.AnchorTacticID), entity.Id, true) == 0)
            {
                // Get list of package tactic ids
                Roistring = string.Concat("<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id='pkgIcon' onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids='",
                            PackageTacticIds,
                            "'><i class='fa fa-object-group'></i></div>");
                BudgetDataObj.value = string.Concat((Roistring).Replace("'", "&#39;").Replace("\"", "&#34;"), Linkedstring, (entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;")));
            }
            else
            {
                BudgetDataObj.value = Linkedstring + (entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            }

            if (string.Compare(entity.ActivityType, ActivityType.ActivityLineItem, true) == 0 && entity.LineItemTypeId == null)
            {
                BudgetDataObj.lo = CellLocked;
                BudgetDataObj.style = NotEditableCellStyle;
            }
            else
            {
                BudgetDataObj.lo = entity.isEntityEditable ? CellNotLocked : CellLocked;
                BudgetDataObj.style = entity.isEntityEditable ? string.Empty : NotEditableCellStyle;
            }
            BudgetDataObjList.Add(BudgetDataObj);

            // Set icon of magnifying glass and honey comb for plan entity with respective ids
            Budgetdataobj iconsData = new Budgetdataobj();
            if (!isViewBy)
            {
                iconsData.value = (SetIcons(entity, ownerName, entityType, dhtmlxGridRowId, isAddEntityRights, pcptid, tacticType));
            }
            else
            {
                iconsData.value = string.Empty;
            }
            BudgetDataObjList.Add(iconsData);

            // Set Total Actual,Total Budget and Total planned cost for plan entity
            BudgetDataObjList = CampaignBudgetSummary(budgetModel, entityType, entity.ParentActivityId,
                  BudgetDataObjList, allocatedBy, entity.ActivityId, isViewBy);
            // Set monthly/quarterly allocation of budget,actuals and planned for plan
            BudgetDataObjList = CampaignMonth(budgetModel, entityType, entity.ParentActivityId,
                    BudgetDataObjList, allocatedBy, entity.ActivityId, isNextYear, isMultiyearPlan, isViewBy);
            BudgetDataObj = new Budgetdataobj();
            // Add UnAllocated Cost into DHTMLx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(entity.UnallocatedCost);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            // Add unAllocated budget into DHTMLx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(entity.TotalUnallocatedBudget);
            BudgetDataObjList.Add(BudgetDataObj);

            return BudgetDataObjList;
        }

        /// <summary>
        /// Method to set Magnifying Glass, Add Button & HoneyComb Button Icons
        /// </summary>
        public string SetIcons(PlanBudgetModel entity, string ownerName, string entityType, string dhtmlxGridRowId, bool isAddEntityRights, string pcptid, string tacticType)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(entityType != null, "Entity Type cannot be null.");

            string doubledesh = "--";
            string IconsData = string.Empty;
            string addIcon = "<i class='fa fa-plus-circle' aria-hidden='true'></i>";
            // Set icon of magnifying glass and honey comb for plan entity with respective ids
            string Title = (entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            if (string.Compare(entityType, ActivityType.ActivityPlan, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=Plan onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (isAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Plan alt=" + entity.Id + " per=" + Convert.ToString(isAddEntityRights).ToLower() + " >";
                    IconsData += addIcon;
                    IconsData += "</div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Plan  TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(ownerName) + "' TaskName='" + Title + "' altId=" + Convert.ToString(dhtmlxGridRowId);
                IconsData += " per=" + Convert.ToString(isAddEntityRights).ToLower() + " ColorCode=" + Convert.ToString(entity.ColorCode) + " taskId=" + Convert.ToString(entity.Id);
                IconsData += string.Concat(" csvId=Plan_", Convert.ToString(entity.Id), " ></div>");
            }
            else if (string.Compare(entityType, ActivityType.ActivityCampaign, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=CP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (isAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Campaign alt=" + entity.ParentId + "_" + entity.Id;
                    IconsData += string.Concat(" per=", Convert.ToString(isAddEntityRights).ToLower(), " >", addIcon, "</div>");
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Campaign onclick=javascript:AddRemoveEntity(this) title=Select   TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(ownerName) + "' TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(dhtmlxGridRowId) + " per=" + Convert.ToString(isAddEntityRights).ToLower();
                IconsData += " ColorCode=" + Convert.ToString(entity.ColorCode) + " taskId= " + Convert.ToString(entity.Id) + " csvId=Campaign_" + entity.Id + "></div>";
            }
            else if (string.Compare(entityType, ActivityType.ActivityProgram, true) == 0)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=PP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (isAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Program alt=_" + entity.ParentId + "_" + entity.Id;
                    IconsData += string.Concat(" per=", Convert.ToString(isAddEntityRights).ToLower(), " >", addIcon, "</div>");
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Program onclick=javascript:AddRemoveEntity(this) title=Select  TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(ownerName) + "' TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(dhtmlxGridRowId) + " ColorCode=" + Convert.ToString(entity.ColorCode);
                IconsData += string.Concat(" per=", Convert.ToString(isAddEntityRights).ToLower(), " taskId=", Convert.ToString(entity.Id), " csvId=Program_", Convert.ToString(entity.Id), " ></div>");
            }
            else if (string.Compare(entityType, ActivityType.ActivityTactic, true) == 0)
            {
                // LinkTactic Permission based on Entity Year
                bool LinkTacticPermission = ((entity.EndDate.Year - entity.StartDate.Year) > 0) ? true : false;
                string LinkedTacticId = entity.LinkTacticId == 0 ? "null" : Convert.ToString(entity.LinkTacticId);

                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=TP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (isAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Tactic alt=__" + Convert.ToString(entity.ParentId) + "_" + Convert.ToString(entity.Id);
                    IconsData += " per=" + Convert.ToString(isAddEntityRights).ToLower() + " LinkTacticper =" + Convert.ToString(LinkTacticPermission) + " LinkedTacticId = " + Convert.ToString(LinkedTacticId);
                    IconsData += string.Concat(" tacticaddId=", Convert.ToString(entity.Id), ">", addIcon, "</div>");
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Tactic ";
                IconsData += string.Concat(" TacticType= '", Convert.ToString(tacticType), "' OwnerName= '", Convert.ToString(ownerName), "' roitactictype='",
                                entity.AssetType, "' anchortacticid='", entity.AnchorTacticID, "'  ");
                IconsData += " TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(dhtmlxGridRowId) + " ColorCode=" + Convert.ToString(entity.ColorCode);
                IconsData += string.Concat(" per=", Convert.ToString(isAddEntityRights).ToLower(), " taskId=", Convert.ToString(entity.Id), " csvId=Tactic_", Convert.ToString(entity.Id), " ></div>");
            }
            else if (string.Compare(entityType, ActivityType.ActivityLineItem, true) == 0 && entity.LineItemTypeId != null)
            {
                // Magnifying Glass to open Inspect Pop up
                IconsData = "<div class=grid_Search id=LP onclick=javascript:DisplayPopupforBudget(this) title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (isAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  title=Add  id=Line alt=___" + Convert.ToString(entity.ParentId) + "_" + Convert.ToString(entity.Id);
                    IconsData += " lt=" + ((entity.LineItemTypeId == null) ? 0 : entity.LineItemTypeId) + " per=" + Convert.ToString(isAddEntityRights).ToLower();
                    IconsData += string.Concat(" dt=", Title, " >", addIcon, "</div>");
                }
            }
            return IconsData;
        }

        /// <summary>
        /// Method to create data in DHTMLx Format
        /// </summary>
        public BudgetDHTMLXGridModel CreateDhtmlxFormattedBudgetData(BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> budgetModel, string allocatedBy, int userID, int clientId, string year, string viewBy)
        {
            Contract.Requires<ArgumentNullException>(objBudgetDHTMLXGrid != null, "Budget DHTMLX Grid Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model rows cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(clientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(userID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(viewBy != null, "View By cannot be null.");

            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();

            if (viewBy != Convert.ToString(PlanGanttTypes.Tactic))
            {
                foreach (PlanBudgetModel bmViewby in budgetModel.Where(p => string.Compare(p.ActivityType, viewBy, true) == 0).OrderBy(p => p.ActivityName))
                {
                    BudgetDHTMLXGridDataModel gridViewbyData = new BudgetDHTMLXGridDataModel();
                    List<BudgetDHTMLXGridDataModel> gridjsonlistViewby = new List<BudgetDHTMLXGridDataModel>();
                    gridViewbyData.id = bmViewby.ActivityId;
                    gridViewbyData.open = Open;
                    List<Budgetdataobj> BudgetviewbyDataList;
                    string EntityType = viewBy;
                    bool isViewby = true;
                    BudgetviewbyDataList = SetBudgetDhtmlxFormattedValues(budgetModel, bmViewby, string.Empty, EntityType, allocatedBy, false, false, bmViewby.ActivityId, false, isViewby);
                    gridViewbyData.data = BudgetviewbyDataList;
                    List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(budgetModel, userID, clientId, year, allocatedBy, isViewby, bmViewby.ActivityId);
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
                List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(budgetModel, userID, clientId, year, allocatedBy, false);
                foreach (BudgetDHTMLXGridDataModel item in gridJsondata)
                {
                    gridjsonlist.Add(item);
                }
            }

            // Set plan entity in the DHTMLx formated model at top level of the hierarchy using loop

            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            objBudgetDHTMLXGrid.Grid.rows = gridjsonlist;
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method to set data in hierarchy
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> GenerateHierarchy(List<PlanBudgetModel> budgetModel, int userID, int clientId, string year, string allocatedBy, bool isViewBy, string parentId = "")
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model rows cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(clientId > 0, "Client Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(userID > 0, "User Id cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");

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
                lstSubordinatesIds = Common.GetAllSubordinates(userID);
            }

            IEnumerable<int> TacticTypeIds = budgetModel.Where(t => string.Compare(t.ActivityType, ActivityType.ActivityTactic, true) == 0).Select(t => t.TacticTypeId).Distinct();
            Dictionary<int, string> lstTacticTypeTitle = objDbMrpEntities.TacticTypes.Where(tt => TacticTypeIds.Contains(tt.TacticTypeId) && tt.IsDeleted == false)
                                                            .ToDictionary(tt => tt.TacticTypeId, tt => tt.Title);

            List<BudgetDHTMLXGridDataModel> gridjsonlist = SetPlanHierarchy(budgetModel, isViewBy, parentId, IsPlanCreateAll, userID, lstSubordinatesIds, year, lstTacticTypeTitle, allocatedBy);

            return gridjsonlist;
        }

        /// <summary>
        /// Method to set data in hierarchy for Plan
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetPlanHierarchy(List<PlanBudgetModel> budgetModel, bool isViewBy, string parentId, bool isPlanCreateAll, int userID, List<int> lstSubordinatesIds, string year, Dictionary<int, string> lstTacticTypeTitle, string allocatedBy)
        {
            List<Budgetdataobj> BudgetDataObjList;
            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel gridjsonlistPlanObj;
            foreach (PlanBudgetModel bm in budgetModel.Where(p => string.Compare(p.ActivityType, ActivityType.ActivityPlan, true) == 0
                        && (!isViewBy || string.Compare(p.ParentActivityId, parentId, true) == 0)).OrderBy(p => p.ActivityName))
            {
                if (!isPlanCreateAll)
                {
                    if (bm.CreatedBy == userID || lstSubordinatesIds.Contains(bm.CreatedBy))
                        isPlanCreateAll = true;
                    else
                        isPlanCreateAll = false;
                }
                bool isCampignExist = budgetModel.Where(p => string.Compare(p.ParentActivityId, bm.ActivityId, true) == 0).Any();
                DateTime MaxDate = default(DateTime); ;
                if (isCampignExist)
                {
                    MaxDate = budgetModel.Where(p => string.Compare(p.ParentActivityId, bm.ActivityId, true) == 0).Max(a => a.EndDate);
                }

                // Set flag to identify plan year. e.g.if time frame is 2015-2016 and plan have plan year 2016 then we will not set month data for Jan-2015 to Dec-2015 of respective plan
                bool isNextYearPlan = false;
                bool isMultiYearPlan = false;
                string firstYear = Common.GetInitialYearFromTimeFrame(year);
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

                BudgetDataObjList = SetBudgetDhtmlxFormattedValues(budgetModel, bm, OwnerName, ActivityType.ActivityPlan, allocatedBy, isNextYearPlan, isMultiYearPlan, gridjsonlistPlanObj.id, isPlanCreateAll);
                gridjsonlistPlanObj.data = BudgetDataObjList;

                List<BudgetDHTMLXGridDataModel> CampaignRowsObjList = SetCampaignHierarchy(budgetModel, bm, isPlanCreateAll, userID, lstSubordinatesIds, OwnerName, lstTacticTypeTitle, allocatedBy, isNextYearPlan, isMultiYearPlan);

                // set campaign row data as child to respective plan
                gridjsonlistPlanObj.rows = CampaignRowsObjList;
                gridjsonlist.Add(gridjsonlistPlanObj);
            }
            return gridjsonlist;
        }

        /// <summary>
        /// Method to set data in hierarchy for Campaign
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetCampaignHierarchy(List<PlanBudgetModel> budgetModel, PlanBudgetModel planModel, bool isPlanCreateAll, int userID, List<int> lstSubordinatesIds, string ownerName, Dictionary<int, string> lstTacticTypeTitle, string allocatedBy, bool isNextYearPlan, bool isMultiYearPlan)
        {
            List<BudgetDHTMLXGridDataModel> CampaignRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel CampaignRowsObj;
            foreach (
                PlanBudgetModel bmc in
                    budgetModel.Where(
                        p => string.Compare(p.ActivityType, ActivityType.ActivityCampaign, true) == 0 && string.Compare(p.ParentActivityId, planModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName)
                )
            {
                CampaignRowsObj = new BudgetDHTMLXGridDataModel();
                CampaignRowsObj.id = bmc.TaskId;
                CampaignRowsObj.open = Open;

                bool IsCampCreateAll = isPlanCreateAll = isPlanCreateAll == false ? (bmc.CreatedBy == userID || lstSubordinatesIds.Contains(bmc.CreatedBy)) ? true : false : true;

                ownerName = Convert.ToString(planModel.CreatedBy);
                List<Budgetdataobj> CampaignDataObjList = SetBudgetDhtmlxFormattedValues(budgetModel, bmc, ownerName, ActivityType.ActivityCampaign, allocatedBy, isNextYearPlan,
                                                            isMultiYearPlan, CampaignRowsObj.id, IsCampCreateAll);

                CampaignRowsObj.data = CampaignDataObjList;
                List<BudgetDHTMLXGridDataModel> ProgramRowsObjList = SetProgramHierarchy(budgetModel, bmc, isPlanCreateAll, userID, lstSubordinatesIds, ownerName, planModel, lstTacticTypeTitle, allocatedBy, isNextYearPlan, isMultiYearPlan);

                // set program row data as child to respective campaign
                CampaignRowsObj.rows = ProgramRowsObjList;
                CampaignRowsObjList.Add(CampaignRowsObj);
            }
            return CampaignRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for Program
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetProgramHierarchy(List<PlanBudgetModel> budgetModel, PlanBudgetModel campaignModel, bool isPlanCreateAll, int userID, List<int> lstSubordinatesIds, string ownerName, PlanBudgetModel planModel, Dictionary<int, string> lstTacticTypeTitle, string allocatedBy, bool isNextYearPlan, bool isMultiYearPlan)
        {
            List<BudgetDHTMLXGridDataModel> ProgramRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel ProgramRowsObj;
            foreach (
                PlanBudgetModel bmp in
                    budgetModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityProgram, true) == 0 &&
                            string.Compare(p.ParentActivityId, campaignModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName))
            {
                ProgramRowsObj = new BudgetDHTMLXGridDataModel();
                ProgramRowsObj.id = bmp.TaskId;
                ProgramRowsObj.open = null;

                bool IsProgCreateAll = isPlanCreateAll = isPlanCreateAll == false ? (bmp.CreatedBy == userID || lstSubordinatesIds.Contains(bmp.CreatedBy)) ? true : false : true;

                ownerName = Convert.ToString(planModel.CreatedBy);
                List<Budgetdataobj> ProgramDataObjList = SetBudgetDhtmlxFormattedValues(budgetModel, bmp, ownerName, ActivityType.ActivityProgram, allocatedBy, isNextYearPlan,
                                                            isMultiYearPlan, ProgramRowsObj.id, IsProgCreateAll);
                ProgramRowsObj.data = ProgramDataObjList;

                List<BudgetDHTMLXGridDataModel> TacticRowsObjList = SetTacticHierarchy(budgetModel, bmp, isPlanCreateAll, userID, lstSubordinatesIds, ownerName, planModel, lstTacticTypeTitle, allocatedBy, isNextYearPlan, isMultiYearPlan, campaignModel);

                // set tactic row data as child to respective program
                ProgramRowsObj.rows = TacticRowsObjList;
                ProgramRowsObjList.Add(ProgramRowsObj);
            }
            return ProgramRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for Tactic
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetTacticHierarchy(List<PlanBudgetModel> budgetModel, PlanBudgetModel programModel, bool isPlanCreateAll, int userID, List<int> lstSubordinatesIds, string ownerName, PlanBudgetModel planModel, Dictionary<int, string> lstTacticTypeTitle, string allocatedBy, bool isNextYearPlan, bool isMultiYearPlan, PlanBudgetModel campaignModel)
        {
            List<BudgetDHTMLXGridDataModel> TacticRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel TacticRowsObj;
            foreach (
                PlanBudgetModel bmt in
                    budgetModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
                            string.Compare(p.ParentActivityId, programModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName).OrderBy(p => p.ActivityName))
            {
                TacticRowsObj = new BudgetDHTMLXGridDataModel();
                TacticRowsObj.id = bmt.TaskId;
                TacticRowsObj.open = null;

                bool IsTacCreateAll = isPlanCreateAll == false ? (bmt.CreatedBy == userID || lstSubordinatesIds.Contains(bmt.CreatedBy)) ? true : false : true;


                ownerName = Convert.ToString(planModel.CreatedBy);
                string TacticType = string.Empty;
                if (lstTacticTypeTitle != null && lstTacticTypeTitle.Count > 0)
                {
                    if (lstTacticTypeTitle.ContainsKey(bmt.TacticTypeId))
                    {
                        TacticType = Convert.ToString(lstTacticTypeTitle[bmt.TacticTypeId]);
                    }
                }
                List<Budgetdataobj> TacticDataObjList = SetBudgetDhtmlxFormattedValues(budgetModel, bmt, ownerName, ActivityType.ActivityTactic, allocatedBy, isNextYearPlan,
                        isMultiYearPlan, TacticRowsObj.id, IsTacCreateAll, false, "L" + planModel.ActivityId + "_C" + campaignModel.ActivityId + "_P" + programModel.ActivityId + "_T" + bmt.ActivityId, TacticType);

                TacticRowsObj.data = TacticDataObjList;
                List<BudgetDHTMLXGridDataModel> LineRowsObjList = SetLineItemHierarchy(budgetModel, bmt, isPlanCreateAll, userID, lstSubordinatesIds, ownerName, allocatedBy, isNextYearPlan, isMultiYearPlan, planModel);

                // set line item row data as child to respective tactic
                TacticRowsObj.rows = LineRowsObjList;
                TacticRowsObjList.Add(TacticRowsObj);
            }
            return TacticRowsObjList;
        }

        /// <summary>
        /// Method to set data in hierarchy for LineItem
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> SetLineItemHierarchy(List<PlanBudgetModel> budgetModel, PlanBudgetModel tacticModel, bool isPlanCreateAll, int userID, List<int> lstSubordinatesIds, string ownerName, string allocatedBy, bool isNextYearPlan, bool isMultiYearPlan, PlanBudgetModel planModel)
        {
            List<BudgetDHTMLXGridDataModel> LineRowsObjList = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel LineRowsObj;
            foreach (
                PlanBudgetModel bml in
                    budgetModel.Where(
                        p =>
                            string.Compare(p.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
                            string.Compare(p.ParentActivityId, tacticModel.ActivityId, true) == 0).OrderBy(p => p.ActivityName))
            {
                LineRowsObj = new BudgetDHTMLXGridDataModel();
                LineRowsObj.id = bml.TaskId;
                LineRowsObj.open = null;

                bool IsLinItmCreateAll = isPlanCreateAll == false ? (bml.CreatedBy == userID || lstSubordinatesIds.Contains(bml.CreatedBy)) ? true : false : true;

                ownerName = Convert.ToString(planModel.CreatedBy);
                List<Budgetdataobj> LineDataObjList = SetBudgetDhtmlxFormattedValues(budgetModel, bml, ownerName, ActivityType.ActivityLineItem, allocatedBy, isNextYearPlan,
                                                        isMultiYearPlan, LineRowsObj.id, IsLinItmCreateAll);

                LineRowsObj.data = LineDataObjList;
                LineRowsObjList.Add(LineRowsObj);
            }
            return LineRowsObjList;
        }

        /// <summary>
        /// Method to generate header string in DHTMLx format
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderString(string allocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid, string year)
        {
            Contract.Requires<ArgumentNullException>(objBudgetDHTMLXGrid != null, "Budget DHTMLX Grid Model cannot be null.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");

            string firstYear = Common.GetInitialYearFromTimeFrame(year);
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
            if (string.Compare(allocatedBy, Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)], true) == 0)
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

            objBudgetDHTMLXGrid.SetHeader = setHeader + EndColumnsHeader;
            objBudgetDHTMLXGrid.ColType = colType + EndColType;
            objBudgetDHTMLXGrid.Width = width + EndcolWidth;
            objBudgetDHTMLXGrid.ColSorting = colSorting + EndColsorting;
            objBudgetDHTMLXGrid.ColumnIds = columnIds + EndColumnIds;

            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Method to set Total Budget, Planned & Actual Data
        /// </summary>
        private List<Budgetdataobj> CampaignBudgetSummary(List<PlanBudgetModel> budgetModel, string actType, string parentActivityId, List<Budgetdataobj> budgetDataObjList, string allocatedBy, string activityId, bool isViewBy = false)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(actType != null, "Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(parentActivityId != null, "Parent Activity Id cannot be null.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(activityId != null, "Activity Id cannot be null.");

            PlanBudgetModel Entity = budgetModel.Where(pl => string.Compare(pl.ActivityType, actType, true) == 0 &&
                                                    string.Compare(pl.ParentActivityId, parentActivityId, true) == 0 &&
                                                    string.Compare(pl.ActivityId, activityId, true) == 0)
                                                .OrderBy(p => p.ActivityName).ToList().FirstOrDefault();
            double ChildTotalBudget = budgetModel.Where(cl => string.Compare(cl.ParentActivityId, activityId, true) == 0).Sum(cl => cl.YearlyBudget);
            if (Entity != null)
            {
                Budgetdataobj objTotalBudget = new Budgetdataobj();
                Budgetdataobj objTotalCost = new Budgetdataobj();
                Budgetdataobj objTotalActual = new Budgetdataobj();
                // Entity type line item has no budget so we set '---' value for line item
                if (!isViewBy)
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

                    bool isOtherLineItem = string.Compare(actType, ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId == null;
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


                budgetDataObjList.Add(objTotalBudget);
                budgetDataObjList.Add(objTotalCost);
                budgetDataObjList.Add(objTotalActual);

            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to check view by i.e. quarterly or monthly & check plan is multi-year plan or not.
        /// </summary>
        private List<Budgetdataobj> CampaignMonth(List<PlanBudgetModel> budgetModel, string actType, string parentActivityId, List<Budgetdataobj> budgetDataObjList, string allocatedBy, string activityId, bool isNextYearPlan, bool isMulityearPlan, bool isViewBy = false)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget model rows count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(actType != null, "Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(parentActivityId != null, "Parent Activity Id cannot be null.");
            Contract.Requires<ArgumentNullException>(allocatedBy != null, "Allocated By cannot be null.");
            Contract.Requires<ArgumentNullException>(activityId != null, "Activity Id cannot be null.");

            PlanBudgetModel Entity = budgetModel.Where(pl => string.Compare(pl.ActivityType, actType, true) == 0 && string.Compare(pl.ParentActivityId, parentActivityId, true) == 0 &&
                                                    string.Compare(pl.ActivityId, activityId, true) == 0).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();

            bool isTactic = string.Compare(actType, Helpers.ActivityType.ActivityTactic, true) == 0 ? true : false;
            bool isLineItem = string.Compare(actType, Helpers.ActivityType.ActivityLineItem, true) == 0 ? true : false;
            bool isOtherLineitem = string.Compare(actType, Helpers.ActivityType.ActivityLineItem, true) == 0 && Entity.LineItemTypeId == null ? true : false;

            if (string.Compare(allocatedBy, "quarters", true) != 0)
            {
                if (!isNextYearPlan)
                {
                    budgetDataObjList = CampignMonthlyAllocation(Entity, isTactic, isLineItem, budgetDataObjList, isViewBy, isOtherLineitem);
                }
            }
            else
            {
                if (!isNextYearPlan)
                {
                    budgetDataObjList = CampignQuarterlyAllocation(Entity, isTactic, isLineItem, budgetDataObjList, isMulityearPlan, isViewBy, isOtherLineitem);
                }
                else if (!isMultiYear)
                {
                    budgetDataObjList = CampignNextYearQuarterlyAllocation(Entity, isTactic, isLineItem, budgetDataObjList, isMulityearPlan, isViewBy, isOtherLineitem);
                }
                else
                {
                    budgetDataObjList = CampignMulitYearQuarterlyAllocation(Entity, isTactic, isLineItem, budgetDataObjList, isViewBy, isOtherLineitem);
                }
            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to set model in monthly view
        /// </summary>
        private List<Budgetdataobj> CampignMonthlyAllocation(PlanBudgetModel entity, bool isTactic, bool isLineItem, List<Budgetdataobj> budgetDataObjList, bool isViewby = false, bool isOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            int BudgetIndex = 0;
            foreach (PropertyInfo BProp in BudgetProp)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();
                if (!isViewby)
                {
                    // Set cell locked property
                    objBudgetMonth.lo = entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.lo = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.lo = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? CellNotLocked : CellLocked;

                    // Set cell style property
                    objBudgetMonth.style = entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                    // Set cell value property
                    objBudgetMonth.value = !isLineItem ? Convert.ToString(BProp.GetValue(entity.MonthValues)) : ThreeDash;
                    objCostMonth.value = Convert.ToString(CostProp[BudgetIndex].GetValue(entity.MonthValues));
                    objActualMonth.value = Convert.ToString(ActualProp[BudgetIndex].GetValue(entity.MonthValues));

                    // Set Orange Flag
                    if (!isLineItem && Convert.ToDouble(BProp.GetValue(entity.MonthValues)) < Convert.ToDouble(BProp.GetValue(entity.ChildMonthValues)))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                        objBudgetMonth.av = BudgetFlagval;
                    }

                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
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
                budgetDataObjList.Add(objBudgetMonth);
                budgetDataObjList.Add(objCostMonth);
                budgetDataObjList.Add(objActualMonth);

                BudgetIndex++;
            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to set model in quarterly view
        /// </summary>
        private List<Budgetdataobj> CampignQuarterlyAllocation(PlanBudgetModel entity, bool isTactic, bool isLineItem, List<Budgetdataobj> budgetDataObjList, bool isMultiYearPlan, bool isViewBy = false, bool isOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");

            int multiYearCounter = 23;
            if (!isMultiYear)
            {
                multiYearCounter = 11;
            }

            PropertyInfo[] BudgetProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int MYCnt = 0; MYCnt <= multiYearCounter; MYCnt += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                // Set cell locked property
                objBudgetMonth.lo = entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objCostMonth.lo = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objActualMonth.lo = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? CellNotLocked : CellLocked;

                // Set cell style property
                objBudgetMonth.style = entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.style = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.style = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                if (!isViewBy)
                {
                    if (MYCnt < 12)
                    {
                        // Set First Year Cost
                        SetCampaignQuarterlyAllocation(entity, isLineItem, BudgetProp, CostProp, ActualProp, objBudgetMonth, objCostMonth, objActualMonth, MYCnt);
                    }
                    else
                    {
                        // Set Multi Year Cost
                        SetCampaignMultiyearQuarterlyAllocation(entity, isLineItem, isMultiYearPlan, BudgetProp, objBudgetMonth, objCostMonth, objActualMonth, MYCnt - 12);
                    }

                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
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
                budgetDataObjList.Add(objBudgetMonth);
                budgetDataObjList.Add(objCostMonth);
                budgetDataObjList.Add(objActualMonth);
            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to set model for multi-year plan
        /// </summary>
        private static void SetCampaignMultiyearQuarterlyAllocation(PlanBudgetModel entity, bool isLineItem, bool isMultiYearPlan, PropertyInfo[] budgetProp, Budgetdataobj objBudgetMonth, Budgetdataobj objCostMonth, Budgetdataobj objActualMonth, int monthNo)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetProp != null, "Budget Property cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetProp.Length > 0, "Budget Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(monthNo >= 0, "Month Number cannot be less than zero.");

            double FMNBudget = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Budget Month of Quarter
            double SMNBudget = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Budget Month of Quarter
            double TMNBudget = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Budget Month of Quarter

            double FMNCBudget = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.ChildNextYearMonthValues)); // Multi Year First Child Budget Month of Quarter
            double SMNCBudget = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.ChildNextYearMonthValues)); // Multi Year Second Child Budget Month of Quarter
            double TMNCBudget = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.ChildNextYearMonthValues)); // Multi Year Third Child Budget Month of Quarter

            double FMCCost = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Cost Month of Quarter
            double SMCCost = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Cost Month of Quarter
            double TMCCost = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Cost Month of Quarter

            double FMCActual = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Actual Month of Quarter
            double SMCActual = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Actual Month of Quarter
            double TMCActual = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Actual Month of Quarter

            // Set cell value property
            objBudgetMonth.value = isMultiYearPlan && !isLineItem ? Convert.ToString(FMNBudget + SMNBudget + TMNBudget) : ThreeDash;
            objCostMonth.value = isMultiYearPlan ? Convert.ToString(FMCCost + SMCCost + TMCCost) : ThreeDash;
            objActualMonth.value = isMultiYearPlan ? Convert.ToString(FMCActual + SMCActual + TMCActual) : ThreeDash;

            // Set Orange Flag
            if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMNCBudget + SMNCBudget + TMNCBudget))
            {
                objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                objBudgetMonth.av = BudgetFlagval;
            }
        }

        /// <summary>
        /// Method to set model for single-year plan
        /// </summary>
        private static void SetCampaignQuarterlyAllocation(PlanBudgetModel entity, bool isLineItem, PropertyInfo[] budgetProp, PropertyInfo[] costProp, PropertyInfo[] actualProp, Budgetdataobj objBudgetMonth, Budgetdataobj objCostMonth, Budgetdataobj objActualMonth, int monthNo)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetProp != null, "Budget Property cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetProp.Length > 0, "Budget Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(costProp != null, "Cost Property cannot be null.");
            Contract.Requires<ArgumentNullException>(costProp.Length > 0, "Cost Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(actualProp != null, "Actual Property cannot be null.");
            Contract.Requires<ArgumentNullException>(actualProp.Length > 0, "Actual Property count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(monthNo >= 0, "Month Number cannot be less than zero.");

            double FMBudget = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.MonthValues)); // First Budget Month of Quarter
            double SMBudget = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.MonthValues)); // Second Budget Month of Quarter
            double TMBudget = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.MonthValues)); // Third Budget Month of Quarter

            double FMCBudget = Convert.ToDouble(budgetProp[monthNo].GetValue(entity.ChildMonthValues)); // First Child Budget Month of Quarter
            double SMCBudget = Convert.ToDouble(budgetProp[monthNo + 1].GetValue(entity.ChildMonthValues)); // Second Child Budget Month of Quarter
            double TMCBudget = Convert.ToDouble(budgetProp[monthNo + 2].GetValue(entity.ChildMonthValues)); // Third Child Budget Month of Quarter

            double FMCost = Convert.ToDouble(costProp[monthNo].GetValue(entity.MonthValues)); // First Cost Month of Quarter
            double SMCost = Convert.ToDouble(costProp[monthNo + 1].GetValue(entity.MonthValues)); // Second Cost Month of Quarter
            double TMCost = Convert.ToDouble(costProp[monthNo + 2].GetValue(entity.MonthValues)); // Third Cost Month of Quarter

            double FMActual = Convert.ToDouble(actualProp[monthNo].GetValue(entity.MonthValues)); // First Actual Month of Quarter
            double SMActual = Convert.ToDouble(actualProp[monthNo + 1].GetValue(entity.MonthValues)); // Second Actual Month of Quarter
            double TMActual = Convert.ToDouble(actualProp[monthNo + 2].GetValue(entity.MonthValues)); // Third Actual Month of Quarter

            // Set cell value property
            objBudgetMonth.value = !isLineItem ? Convert.ToString(FMBudget + SMBudget + TMBudget) : ThreeDash;
            objCostMonth.value = Convert.ToString(FMCost + SMCost + TMCost);
            objActualMonth.value = Convert.ToString(FMActual + SMActual + TMActual);

            // Set Orange Flag
            if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMCBudget + SMCBudget + TMCBudget))
            {
                objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                objBudgetMonth.av = BudgetFlagval;
            }
        }

        /// <summary>
        /// Method to set quarterly model for multi-year plan
        /// </summary>
        private List<Budgetdataobj> CampignMulitYearQuarterlyAllocation(PlanBudgetModel entity, bool isTactic, bool isLineItem, List<Budgetdataobj> budgetDataObjList, bool isViewBy = false, bool isOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = entity.MonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int monthNo = 0; monthNo < 23; monthNo += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                if (!isViewBy)
                {
                    // Set cell locked property
                    objBudgetMonth.lo = entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.lo = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.lo = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? CellNotLocked : CellLocked;

                    // Set cell style property
                    objBudgetMonth.style = entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

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
                        double FMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth].GetValue(entity.MonthValues)); // First Budget Month of Quarter
                        double SMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 1].GetValue(entity.MonthValues)); // Second Budget Month of Quarter
                        double TMBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 2].GetValue(entity.MonthValues)); // Third Budget Month of Quarter

                        objBudgetMonth.value = !isLineItem ? Convert.ToString(FMBudget + SMBudget + TMBudget) : ThreeDash; // Set cell value property

                        double FMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth].GetValue(entity.ChildMonthValues)); // First Child Budget Month of Quarter
                        double SMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 1].GetValue(entity.ChildMonthValues)); // Second Child Budget Month of Quarter
                        double TMCBudget = Convert.ToDouble(BudgetProp[SingleYrMonth + 2].GetValue(entity.ChildMonthValues)); // Third Child Budget Month of Quarter

                        // Set Orange Flag
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMCBudget + SMCBudget + TMCBudget))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.av = BudgetFlagval;
                        }

                        double FMCost = Convert.ToDouble(CostProp[SingleYrMonth].GetValue(entity.MonthValues)); // First Cost Month of Quarter
                        double SMCost = Convert.ToDouble(CostProp[SingleYrMonth + 1].GetValue(entity.MonthValues)); // Second Cost Month of Quarter
                        double TMCost = Convert.ToDouble(CostProp[SingleYrMonth + 2].GetValue(entity.MonthValues)); // Third Cost Month of Quarter

                        objCostMonth.value = Convert.ToString(FMCost + SMCost + TMCost); // Set cell value property

                        double FMActual = Convert.ToDouble(ActualProp[SingleYrMonth].GetValue(entity.MonthValues)); // First Actual Month of Quarter
                        double SMActual = Convert.ToDouble(ActualProp[SingleYrMonth + 1].GetValue(entity.MonthValues)); // Second Actual Month of Quarter
                        double TMActual = Convert.ToDouble(ActualProp[SingleYrMonth + 2].GetValue(entity.MonthValues)); // Third Actual Month of Quarter                        

                        objActualMonth.value = Convert.ToString(FMActual + SMActual + TMActual); // Set cell value property
                    }
                    // Set Red Flag
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
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
                budgetDataObjList.Add(objBudgetMonth);
                budgetDataObjList.Add(objCostMonth);
                budgetDataObjList.Add(objActualMonth);
            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to set quarterly model for next-year plan
        /// </summary>
        private List<Budgetdataobj> CampignNextYearQuarterlyAllocation(PlanBudgetModel entity, bool isTactic, bool isLineItem, List<Budgetdataobj> budgetDataObjList, bool isMultiYearPlan, bool isViewBy = false, bool isOtherLineItem = false)
        {
            Contract.Requires<ArgumentNullException>(entity != null, "Entity cannot be null.");

            PropertyInfo[] BudgetProp = entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("budget")).ToArray(); // Get Array of Budget Month Properties
            PropertyInfo[] CostProp = entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("cost")).ToArray(); // Get Array of Cost Month Properties
            PropertyInfo[] ActualProp = entity.NextYearMonthValues.GetType().GetProperties().Where(x => x.Name.ToLower().Contains("actual")).ToArray(); // Get Array of Actual Month Properties

            Budgetdataobj objBudgetMonth;
            Budgetdataobj objCostMonth;
            Budgetdataobj objActualMonth;

            for (int monthNo = 0; monthNo <= 10; monthNo += 3)
            {
                objBudgetMonth = new Budgetdataobj();
                objCostMonth = new Budgetdataobj();
                objActualMonth = new Budgetdataobj();

                // Set cell locked property
                objBudgetMonth.lo = entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objCostMonth.lo = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objActualMonth.lo = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? CellNotLocked : CellLocked;

                // Set cell style property
                objBudgetMonth.style = entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.style = entity.isCostEditable && (isTactic || (isLineItem && entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.style = !isOtherLineItem && entity.isActualEditable && entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                double FMNBudget = Convert.ToDouble(BudgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Budget Month of Quarter
                double SMNBudget = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Budget Month of Quarter
                double TMNBudget = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Budget Month of Quarter

                objBudgetMonth.value = !isLineItem ? Convert.ToString(FMNBudget + SMNBudget + TMNBudget) : ThreeDash; // Set cell value property

                double FMNCBudget = Convert.ToDouble(BudgetProp[monthNo].GetValue(entity.ChildNextYearMonthValues)); // Multi Year First Child Budget Month of Quarter
                double SMNCBudget = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(entity.ChildNextYearMonthValues)); // Multi Year Second Child Budget Month of Quarter
                double TMNCBudget = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(entity.ChildNextYearMonthValues)); // Multi Year Third Child Budget Month of Quarter

                // Set Orange Flag
                if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (FMNCBudget + SMNCBudget + TMNCBudget))
                {
                    objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                    objBudgetMonth.av = BudgetFlagval;
                }

                double FMCCost = Convert.ToDouble(BudgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Cost Month of Quarter
                double SMCCost = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Cost Month of Quarter
                double TMCCost = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Cost Month of Quarter

                objCostMonth.value = Convert.ToString(FMCCost + SMCCost + TMCCost); // Set cell value property

                double FMCActual = Convert.ToDouble(BudgetProp[monthNo].GetValue(entity.NextYearMonthValues)); // Multi Year First Actual Month of Quarter
                double SMCActual = Convert.ToDouble(BudgetProp[monthNo + 1].GetValue(entity.NextYearMonthValues)); // Multi Year Second Actual Month of Quarter
                double TMCActual = Convert.ToDouble(BudgetProp[monthNo + 2].GetValue(entity.NextYearMonthValues)); // Multi Year Third Actual Month of Quarter

                objActualMonth.value = Convert.ToString(FMCActual + SMCActual + TMCActual); // Set cell value property

                // Set Red Flag
                if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
                {
                    objCostMonth.style = objCostMonth.style + RedCornerStyle;
                    objCostMonth.av = CostFlagVal;
                }
                budgetDataObjList.Add(objBudgetMonth);
                budgetDataObjList.Add(objCostMonth);
                budgetDataObjList.Add(objActualMonth);
            }
            return budgetDataObjList;
        }

        /// <summary>
        /// Method to set custom field restriction based on permission
        /// </summary>
        public List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> budgetModel, int userId, int clientId)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(userId > 0, "UserId cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(clientId > 0, "ClientId cannot be less than zero.");

            List<int> lstSubordinatesIds = new List<int>();

            // Get list of subordinates which will be use to check if user is subordinate
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(userId);
            }
            // Custom field type drop-down list
            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);
            // Custom field type text box
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            // Flag will be use to set if custom field is display for filter or not
            bool isDisplayForFilter = false;

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Convert.ToString(Enums.EntityType.Tactic), clientId);

            // Get list tactic's custom field
            List<CustomField> customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == clientId && customfield.EntityType.Equals(EntityTypeTactic)
                                                    && customfield.IsDeleted.Equals(false)).ToList();
            // Check custom field which are not set to display for filter and is required are exist
            bool CustomFieldexists = customfieldlist.Where(customfield => customfield.IsRequired && !isDisplayForFilter).Any();
            // Get drop-down type of custom fields ids
            List<int> customfieldids = customfieldlist.Where(customfield => string.Compare(customfield.CustomFieldType.Name, DropDownList, true) == 0
                                        && (isDisplayForFilter ? customfield.IsDisplayForFilter : true)).Select(customfield => customfield.CustomFieldId).ToList();
            // Get tactics only for budget model
            List<string> tacIds = budgetModel.Where(t => string.Compare(t.ActivityType, EntityTypeTactic, true) == 0).Select(t => t.Id).ToList();

            // Get tactic ids from tactic list
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).ToList();

            // Get tactic custom fields list
            List<CustomField_Entity> lstAllTacticCustomFieldEntities = Entities.Where(customFieldEntity => customfieldids.Contains(customFieldEntity.CustomFieldId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList();
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(userId, true);


            #region "Set Permissions"
            #region "Set Plan Permission"
            bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

            if (IsPlanEditAllAuthorized)
            {
                budgetModel.Where(item => string.Compare(item.ActivityType, ActivityType.ActivityPlan, true) == 0)
                            .ToList().ForEach(item =>
                            {
                                item.isBudgetEditable = item.isEntityEditable = true;
                            });
            }
            else
            {
                budgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityPlan, true) == 0) && ((item.CreatedBy == userId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                           .ToList().ForEach(item =>
                           {
                               item.isBudgetEditable = item.isEntityEditable = true;
                           });
            }
            #endregion

            int allwTaccount = 0;
            List<string> lstTacs = budgetModel.Where(item => string.Compare(item.ActivityType, ActivityType.ActivityTactic, true) == 0).Select(t => t.Id).ToList();
            List<int> tIds = lstTacs.ConvertAll(s => Int32.Parse(s));
            List<int> lstAllAllowedTacIds = Common.GetEditableTacticListPO(userId, clientId, tIds, IsCustomFeildExist, CustomFieldexists, Entities,
                                                lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);

            #region "Set Campaign Permission"
            budgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityCampaign, true) == 0) && ((item.CreatedBy == userId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                       .ToList().ForEach(item =>
                       {
                           // Check user is subordinate or user is owner
                           if (item.CreatedBy == userId || lstSubordinatesIds.Contains(item.CreatedBy))
                           {
                               List<int> planTacticIds = new List<int>();
                               // To find tactic level permission ,first get program list and then get respective tactic list of program which will be used to get editable tactic list
                               List<string> modelprogramid = budgetModel.Where(minner => string.Compare(minner.ActivityType, ActivityType.ActivityProgram, true) == 0 &&
                                                                               string.Compare(minner.ParentActivityId, item.ActivityId, true) == 0).Select(minner => minner.ActivityId).ToList();
                               planTacticIds = budgetModel.Where(m => string.Compare(m.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
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

            budgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityProgram, true) == 0) && ((item.CreatedBy == userId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {

                       // Check user is subordinate or user is owner
                       if (item.CreatedBy == userId || lstSubordinatesIds.Contains(item.CreatedBy))
                       {
                           List<int> planTacticIds = new List<int>();
                           // To find tactic level permission , get respective tactic list of program which will be used to get editable tactic list
                           planTacticIds = budgetModel.Where(m => string.Compare(m.ActivityType, ActivityType.ActivityTactic, true) == 0 &&
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


            budgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityTactic, true) == 0) && ((item.CreatedBy == userId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {
                       // Check user is subordinate or user is owner
                       if (item.CreatedBy == userId || lstSubordinatesIds.Contains(item.CreatedBy))
                       {
                           bool isLineItem = budgetModel.Where(ent => string.Compare(ent.ParentActivityId, item.ActivityId, true) == 0 && ent.LineItemTypeId != null).Any();
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
            budgetModel.Where(item => (string.Compare(item.ActivityType, ActivityType.ActivityLineItem, true) == 0) && ((item.CreatedBy == userId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                   {
                       int tacticOwner = 0;
                       if (budgetModel.Where(m => string.Compare(m.ActivityId, item.ParentActivityId, true) == 0).Any())
                       {
                           tacticOwner = budgetModel.Where(m => string.Compare(m.ActivityId, item.ParentActivityId, true) == 0).FirstOrDefault().CreatedBy;
                       }

                       // Check user is subordinate or user is owner of line item or user is owner of tactic
                       if (item.CreatedBy == userId || tacticOwner == userId || lstSubordinatesIds.Contains(tacticOwner))
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

            return budgetModel;
        }

        /// <summary>
        /// Method to set model for LineItem
        /// </summary>
        private List<PlanBudgetModel> ManageLineItems(List<PlanBudgetModel> budgetModel)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model count cannot be less than zero.");

            foreach (PlanBudgetModel l in budgetModel.Where(l => string.Compare(l.ActivityType, ActivityType.ActivityTactic, true) == 0))
            {
                // Calculate Line items Difference.
                List<PlanBudgetModel> lines = budgetModel.Where(line => string.Compare(line.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
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
            return budgetModel;
        }

        /// <summary>
        /// sum up the total of planned and actuals cell of budget to child to parent
        /// </summary>        
        private List<PlanBudgetModel> CalculateBottomUp(List<PlanBudgetModel> budgetModel, string parentActivityType, string childActivityType, string viewBy)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(parentActivityType != null, "Parent Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(childActivityType != null, "Child Activity Type cannot be null.");
            Contract.Requires<ArgumentNullException>(viewBy != null, "View By cannot be null.");

            double totalMonthCostSum = 0;

            foreach (PlanBudgetModel l in budgetModel.Where(_mdl => string.Compare(_mdl.ActivityType, parentActivityType, true) == 0))
            {
                // Check if ViewBy is Campaign selected then set weight-age value to 100;
                int weightage = 100;
                if (viewBy != Convert.ToString(PlanGanttTypes.Tactic))
                {
                    weightage = l.Weightage;
                }
                weightage = weightage / 100;

                List<PlanBudgetModel> childs = budgetModel.Where(line => string.Compare(line.ActivityType, childActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0).ToList();
                if (l.ActivityType != ActivityType.ActivityTactic
                        || budgetModel.Where(m => string.Compare(m.ParentActivityId, l.ActivityId, true) == 0 && m.LineItemTypeId != null &&
                            string.Compare(m.ActivityType, ActivityType.ActivityLineItem, true) == 0).Any() && childs != null)
                {
                    BottonUpActualMonthlyValues(l, weightage, childs);
                }

                if (string.Compare(parentActivityType, ActivityType.ActivityTactic, true) < 0 && childs != null)
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
                    l.TotalAllocatedCost = budgetModel.Where(line => string.Compare(line.ActivityType, childActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0)
                                            .Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    l.UnallocatedCost = l.TotalAllocatedCost - totalMonthCostSum;
                }
                if (l.ActivityType != ActivityType.ActivityTactic || budgetModel.Where(m => string.Compare(m.ParentActivityId, l.ActivityId, true) == 0 && m.LineItemTypeId != null &&
                        string.Compare(m.ActivityType, ActivityType.ActivityLineItem, true) == 0).Any())
                {
                    l.TotalActuals = budgetModel.Where(line => string.Compare(line.ActivityType, childActivityType, true) == 0 && string.Compare(line.ParentActivityId, l.ActivityId, true) == 0)
                                    .Sum(line => (double?)line.TotalActuals) ?? 0;
                }
            }

            return budgetModel;
        }

        /// <summary>
        /// sum up the total of budget to child to parent
        /// </summary>
        private static void BottonUpBudgetMonthlyValues(PlanBudgetModel budgetModel, List<PlanBudgetModel> childs)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");

            if (childs != null)
            {
                budgetModel.ChildMonthValues.BudgetY1 = childs.Sum(line => (double?)(line.MonthValues.BudgetY1)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY2 = childs.Sum(line => (double?)(line.MonthValues.BudgetY2)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY3 = childs.Sum(line => (double?)(line.MonthValues.BudgetY3)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY4 = childs.Sum(line => (double?)(line.MonthValues.BudgetY4)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY5 = childs.Sum(line => (double?)(line.MonthValues.BudgetY5)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY6 = childs.Sum(line => (double?)(line.MonthValues.BudgetY6)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY7 = childs.Sum(line => (double?)(line.MonthValues.BudgetY7)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY8 = childs.Sum(line => (double?)(line.MonthValues.BudgetY8)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY9 = childs.Sum(line => (double?)(line.MonthValues.BudgetY9)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY10 = childs.Sum(line => (double?)(line.MonthValues.BudgetY10)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY11 = childs.Sum(line => (double?)(line.MonthValues.BudgetY11)) ?? 0;
                budgetModel.ChildMonthValues.BudgetY12 = childs.Sum(line => (double?)(line.MonthValues.BudgetY12)) ?? 0;

                budgetModel.ChildNextYearMonthValues.BudgetY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY1)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY2)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY3)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY4)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY5)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY6)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY7)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY8)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY9)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY10)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY11)) ?? 0;
                budgetModel.ChildNextYearMonthValues.BudgetY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY12)) ?? 0;
            }
            else
            {
                budgetModel.ChildMonthValues.BudgetY1 = 0;
                budgetModel.ChildMonthValues.BudgetY2 = 0;
                budgetModel.ChildMonthValues.BudgetY3 = 0;
                budgetModel.ChildMonthValues.BudgetY4 = 0;
                budgetModel.ChildMonthValues.BudgetY5 = 0;
                budgetModel.ChildMonthValues.BudgetY6 = 0;
                budgetModel.ChildMonthValues.BudgetY7 = 0;
                budgetModel.ChildMonthValues.BudgetY8 = 0;
                budgetModel.ChildMonthValues.BudgetY9 = 0;
                budgetModel.ChildMonthValues.BudgetY10 = 0;
                budgetModel.ChildMonthValues.BudgetY11 = 0;
                budgetModel.ChildMonthValues.BudgetY12 = 0;

                budgetModel.ChildNextYearMonthValues.BudgetY1 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY2 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY3 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY4 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY5 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY6 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY7 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY8 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY9 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY10 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY11 = 0;
                budgetModel.ChildNextYearMonthValues.BudgetY12 = 0;
            }
        }

        /// <summary>
        /// sum up the total of planned cost to child to parent
        /// </summary>
        private static void BottonUpCostMonthlyValues(PlanBudgetModel budgetModel, int weightage, List<PlanBudgetModel> childs)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(weightage >= 0, "Weight-age cannot be less than zero.");

            if (childs != null)
            {
                budgetModel.MonthValues.CostY1 = childs.Sum(line => (double?)(line.MonthValues.CostY1 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY2 = childs.Sum(line => (double?)(line.MonthValues.CostY2 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY3 = childs.Sum(line => (double?)(line.MonthValues.CostY3 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY4 = childs.Sum(line => (double?)(line.MonthValues.CostY4 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY5 = childs.Sum(line => (double?)(line.MonthValues.CostY5 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY6 = childs.Sum(line => (double?)(line.MonthValues.CostY6 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY7 = childs.Sum(line => (double?)(line.MonthValues.CostY7 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY8 = childs.Sum(line => (double?)(line.MonthValues.CostY8 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY9 = childs.Sum(line => (double?)(line.MonthValues.CostY9 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY10 = childs.Sum(line => (double?)(line.MonthValues.CostY10 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY11 = childs.Sum(line => (double?)(line.MonthValues.CostY11 * weightage)) ?? 0;
                budgetModel.MonthValues.CostY12 = childs.Sum(line => (double?)(line.MonthValues.CostY12 * weightage)) ?? 0;


                budgetModel.NextYearMonthValues.CostY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY1 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY2 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY3 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY4 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY5 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY6 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY7 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY8 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY9 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY10 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY11 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.CostY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY12 * weightage)) ?? 0;
            }
            else
            {
                budgetModel.MonthValues.CostY1 = 0;
                budgetModel.MonthValues.CostY2 = 0;
                budgetModel.MonthValues.CostY3 = 0;
                budgetModel.MonthValues.CostY4 = 0;
                budgetModel.MonthValues.CostY5 = 0;
                budgetModel.MonthValues.CostY6 = 0;
                budgetModel.MonthValues.CostY7 = 0;
                budgetModel.MonthValues.CostY8 = 0;
                budgetModel.MonthValues.CostY9 = 0;
                budgetModel.MonthValues.CostY10 = 0;
                budgetModel.MonthValues.CostY11 = 0;
                budgetModel.MonthValues.CostY12 = 0;

                budgetModel.NextYearMonthValues.CostY1 = 0;
                budgetModel.NextYearMonthValues.CostY2 = 0;
                budgetModel.NextYearMonthValues.CostY3 = 0;
                budgetModel.NextYearMonthValues.CostY4 = 0;
                budgetModel.NextYearMonthValues.CostY5 = 0;
                budgetModel.NextYearMonthValues.CostY6 = 0;
                budgetModel.NextYearMonthValues.CostY7 = 0;
                budgetModel.NextYearMonthValues.CostY8 = 0;
                budgetModel.NextYearMonthValues.CostY9 = 0;
                budgetModel.NextYearMonthValues.CostY10 = 0;
                budgetModel.NextYearMonthValues.CostY11 = 0;
                budgetModel.NextYearMonthValues.CostY12 = 0;
            }
        }

        /// <summary>
        /// sum up the total of actual cost to child to parent
        /// </summary>
        private static void BottonUpActualMonthlyValues(PlanBudgetModel budgetModel, int weightage, List<PlanBudgetModel> childs)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(weightage >= 0, "Weight-age cannot be less than zero.");

            if (childs != null)
            {
                budgetModel.MonthValues.ActualY1 = childs.Sum(line => (double?)(line.MonthValues.ActualY1 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY2 = childs.Sum(line => (double?)(line.MonthValues.ActualY2 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY3 = childs.Sum(line => (double?)(line.MonthValues.ActualY3 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY4 = childs.Sum(line => (double?)(line.MonthValues.ActualY4 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY5 = childs.Sum(line => (double?)(line.MonthValues.ActualY5 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY6 = childs.Sum(line => (double?)(line.MonthValues.ActualY6 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY7 = childs.Sum(line => (double?)(line.MonthValues.ActualY7 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY8 = childs.Sum(line => (double?)(line.MonthValues.ActualY8 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY9 = childs.Sum(line => (double?)(line.MonthValues.ActualY9 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY10 = childs.Sum(line => (double?)(line.MonthValues.ActualY10 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY11 = childs.Sum(line => (double?)(line.MonthValues.ActualY11 * weightage)) ?? 0;
                budgetModel.MonthValues.ActualY12 = childs.Sum(line => (double?)(line.MonthValues.ActualY12 * weightage)) ?? 0;

                budgetModel.NextYearMonthValues.ActualY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY1 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY2 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY3 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY4 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY5 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY6 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY7 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY8 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY9 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY10 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY11 * weightage)) ?? 0;
                budgetModel.NextYearMonthValues.ActualY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY12 * weightage)) ?? 0;
            }
            else
            {
                budgetModel.MonthValues.ActualY1 = 0;
                budgetModel.MonthValues.ActualY2 = 0;
                budgetModel.MonthValues.ActualY3 = 0;
                budgetModel.MonthValues.ActualY4 = 0;
                budgetModel.MonthValues.ActualY5 = 0;
                budgetModel.MonthValues.ActualY6 = 0;
                budgetModel.MonthValues.ActualY7 = 0;
                budgetModel.MonthValues.ActualY8 = 0;
                budgetModel.MonthValues.ActualY9 = 0;
                budgetModel.MonthValues.ActualY10 = 0;
                budgetModel.MonthValues.ActualY11 = 0;
                budgetModel.MonthValues.ActualY12 = 0;

                budgetModel.NextYearMonthValues.ActualY1 = 0;
                budgetModel.NextYearMonthValues.ActualY2 = 0;
                budgetModel.NextYearMonthValues.ActualY3 = 0;
                budgetModel.NextYearMonthValues.ActualY4 = 0;
                budgetModel.NextYearMonthValues.ActualY5 = 0;
                budgetModel.NextYearMonthValues.ActualY6 = 0;
                budgetModel.NextYearMonthValues.ActualY7 = 0;
                budgetModel.NextYearMonthValues.ActualY8 = 0;
                budgetModel.NextYearMonthValues.ActualY9 = 0;
                budgetModel.NextYearMonthValues.ActualY10 = 0;
                budgetModel.NextYearMonthValues.ActualY11 = 0;
                budgetModel.NextYearMonthValues.ActualY12 = 0;
            }
        }

        /// <summary>
        /// apply weight-age to budget cell values
        /// </summary>
        private List<PlanBudgetModel> SetLineItemCostByWeightage(List<PlanBudgetModel> budgetModel, string viewBy)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model count cannot be less than zero.");
            Contract.Requires<ArgumentNullException>(viewBy != null, "View By cannot be null.");

            int weightage = 100;
            foreach (PlanBudgetModel l in budgetModel.Where(_mdl => string.Compare(_mdl.ActivityType, ActivityType.ActivityTactic, true) == 0))
            {
                List<PlanBudgetModel> lstLineItems = budgetModel.Where(line => string.Compare(line.ActivityType, ActivityType.ActivityLineItem, true) == 0 &&
                                                        string.Compare(line.ParentActivityId, l.ActivityId, true) == 0).ToList();

                // Check if ViewBy is Campaign selected then set weight-age value to 100;
                if (viewBy != Convert.ToString(PlanGanttTypes.Tactic))
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
            return budgetModel;
        }

        /// <summary>
        /// apply weight-age to budget cell values
        /// </summary>
        private static void SetBudgetLineItembyWeightage(int weightage, BudgetMonth lineBudget, BudgetMonth lineNextYearBudget, PlanBudgetModel budgetModel)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(weightage >= 0, "Weight-age cannot be less than zero.");

            lineBudget.BudgetY1 = (double)(budgetModel.MonthValues.BudgetY1 * weightage) / 100;
            lineBudget.BudgetY2 = (double)(budgetModel.MonthValues.BudgetY2 * weightage) / 100;
            lineBudget.BudgetY3 = (double)(budgetModel.MonthValues.BudgetY3 * weightage) / 100;
            lineBudget.BudgetY4 = (double)(budgetModel.MonthValues.BudgetY4 * weightage) / 100;
            lineBudget.BudgetY5 = (double)(budgetModel.MonthValues.BudgetY5 * weightage) / 100;
            lineBudget.BudgetY6 = (double)(budgetModel.MonthValues.BudgetY6 * weightage) / 100;
            lineBudget.BudgetY7 = (double)(budgetModel.MonthValues.BudgetY7 * weightage) / 100;
            lineBudget.BudgetY8 = (double)(budgetModel.MonthValues.BudgetY8 * weightage) / 100;
            lineBudget.BudgetY9 = (double)(budgetModel.MonthValues.BudgetY9 * weightage) / 100;
            lineBudget.BudgetY10 = (double)(budgetModel.MonthValues.BudgetY10 * weightage) / 100;
            lineBudget.BudgetY11 = (double)(budgetModel.MonthValues.BudgetY11 * weightage) / 100;
            lineBudget.BudgetY12 = (double)(budgetModel.MonthValues.BudgetY12 * weightage) / 100;

            lineNextYearBudget.BudgetY1 = (double)(budgetModel.NextYearMonthValues.BudgetY1 * weightage) / 100;
            lineNextYearBudget.BudgetY2 = (double)(budgetModel.NextYearMonthValues.BudgetY2 * weightage) / 100;
            lineNextYearBudget.BudgetY3 = (double)(budgetModel.NextYearMonthValues.BudgetY3 * weightage) / 100;
            lineNextYearBudget.BudgetY4 = (double)(budgetModel.NextYearMonthValues.BudgetY4 * weightage) / 100;
            lineNextYearBudget.BudgetY5 = (double)(budgetModel.NextYearMonthValues.BudgetY5 * weightage) / 100;
            lineNextYearBudget.BudgetY6 = (double)(budgetModel.NextYearMonthValues.BudgetY6 * weightage) / 100;
            lineNextYearBudget.BudgetY7 = (double)(budgetModel.NextYearMonthValues.BudgetY7 * weightage) / 100;
            lineNextYearBudget.BudgetY8 = (double)(budgetModel.NextYearMonthValues.BudgetY8 * weightage) / 100;
            lineNextYearBudget.BudgetY9 = (double)(budgetModel.NextYearMonthValues.BudgetY9 * weightage) / 100;
            lineNextYearBudget.BudgetY10 = (double)(budgetModel.NextYearMonthValues.BudgetY10 * weightage) / 100;
            lineNextYearBudget.BudgetY11 = (double)(budgetModel.NextYearMonthValues.BudgetY11 * weightage) / 100;
            lineNextYearBudget.BudgetY12 = (double)(budgetModel.NextYearMonthValues.BudgetY12 * weightage) / 100;
        }

        /// <summary>
        /// apply weight-age to planned cell values
        /// </summary>
        private static void SetCostLineItembyWeightage(int weightage, BudgetMonth lineBudget, BudgetMonth lineNextYearBudget, PlanBudgetModel budgetModel)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(weightage >= 0, "Weight-age cannot be less than zero.");

            lineBudget.CostY1 = (double)(budgetModel.MonthValues.CostY1 * weightage) / 100;
            lineBudget.CostY2 = (double)(budgetModel.MonthValues.CostY2 * weightage) / 100;
            lineBudget.CostY3 = (double)(budgetModel.MonthValues.CostY3 * weightage) / 100;
            lineBudget.CostY4 = (double)(budgetModel.MonthValues.CostY4 * weightage) / 100;
            lineBudget.CostY5 = (double)(budgetModel.MonthValues.CostY5 * weightage) / 100;
            lineBudget.CostY6 = (double)(budgetModel.MonthValues.CostY6 * weightage) / 100;
            lineBudget.CostY7 = (double)(budgetModel.MonthValues.CostY7 * weightage) / 100;
            lineBudget.CostY8 = (double)(budgetModel.MonthValues.CostY8 * weightage) / 100;
            lineBudget.CostY9 = (double)(budgetModel.MonthValues.CostY9 * weightage) / 100;
            lineBudget.CostY10 = (double)(budgetModel.MonthValues.CostY10 * weightage) / 100;
            lineBudget.CostY11 = (double)(budgetModel.MonthValues.CostY11 * weightage) / 100;
            lineBudget.CostY12 = (double)(budgetModel.MonthValues.CostY12 * weightage) / 100;

            lineNextYearBudget.CostY1 = (double)(budgetModel.NextYearMonthValues.CostY1 * weightage) / 100;
            lineNextYearBudget.CostY2 = (double)(budgetModel.NextYearMonthValues.CostY2 * weightage) / 100;
            lineNextYearBudget.CostY3 = (double)(budgetModel.NextYearMonthValues.CostY3 * weightage) / 100;
            lineNextYearBudget.CostY4 = (double)(budgetModel.NextYearMonthValues.CostY4 * weightage) / 100;
            lineNextYearBudget.CostY5 = (double)(budgetModel.NextYearMonthValues.CostY5 * weightage) / 100;
            lineNextYearBudget.CostY6 = (double)(budgetModel.NextYearMonthValues.CostY6 * weightage) / 100;
            lineNextYearBudget.CostY7 = (double)(budgetModel.NextYearMonthValues.CostY7 * weightage) / 100;
            lineNextYearBudget.CostY8 = (double)(budgetModel.NextYearMonthValues.CostY8 * weightage) / 100;
            lineNextYearBudget.CostY9 = (double)(budgetModel.NextYearMonthValues.CostY9 * weightage) / 100;
            lineNextYearBudget.CostY10 = (double)(budgetModel.NextYearMonthValues.CostY10 * weightage) / 100;
            lineNextYearBudget.CostY11 = (double)(budgetModel.NextYearMonthValues.CostY11 * weightage) / 100;
            lineNextYearBudget.CostY12 = (double)(budgetModel.NextYearMonthValues.CostY12 * weightage) / 100;
        }

        /// <summary>
        /// apply weight-age to actual cell values
        /// </summary>
        private static void SetActualLineItembyWeightage(int weightage, BudgetMonth lineBudget, BudgetMonth lineNextYearBudget, PlanBudgetModel budgetModel)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineBudget != null, "Line Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(lineNextYearBudget != null, "Line Next Year Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(weightage >= 0, "Weight-age cannot be less than zero.");

            lineBudget.ActualY1 = (double)(budgetModel.MonthValues.ActualY1 * weightage) / 100;
            lineBudget.ActualY2 = (double)(budgetModel.MonthValues.ActualY2 * weightage) / 100;
            lineBudget.ActualY3 = (double)(budgetModel.MonthValues.ActualY3 * weightage) / 100;
            lineBudget.ActualY4 = (double)(budgetModel.MonthValues.ActualY4 * weightage) / 100;
            lineBudget.ActualY5 = (double)(budgetModel.MonthValues.ActualY5 * weightage) / 100;
            lineBudget.ActualY6 = (double)(budgetModel.MonthValues.ActualY6 * weightage) / 100;
            lineBudget.ActualY7 = (double)(budgetModel.MonthValues.ActualY7 * weightage) / 100;
            lineBudget.ActualY8 = (double)(budgetModel.MonthValues.ActualY8 * weightage) / 100;
            lineBudget.ActualY9 = (double)(budgetModel.MonthValues.ActualY9 * weightage) / 100;
            lineBudget.ActualY10 = (double)(budgetModel.MonthValues.ActualY10 * weightage) / 100;
            lineBudget.ActualY11 = (double)(budgetModel.MonthValues.ActualY11 * weightage) / 100;
            lineBudget.ActualY12 = (double)(budgetModel.MonthValues.ActualY12 * weightage) / 100;

            lineNextYearBudget.ActualY1 = (double)(budgetModel.NextYearMonthValues.ActualY1 * weightage) / 100;
            lineNextYearBudget.ActualY2 = (double)(budgetModel.NextYearMonthValues.ActualY2 * weightage) / 100;
            lineNextYearBudget.ActualY3 = (double)(budgetModel.NextYearMonthValues.ActualY3 * weightage) / 100;
            lineNextYearBudget.ActualY4 = (double)(budgetModel.NextYearMonthValues.ActualY4 * weightage) / 100;
            lineNextYearBudget.ActualY5 = (double)(budgetModel.NextYearMonthValues.ActualY5 * weightage) / 100;
            lineNextYearBudget.ActualY6 = (double)(budgetModel.NextYearMonthValues.ActualY6 * weightage) / 100;
            lineNextYearBudget.ActualY7 = (double)(budgetModel.NextYearMonthValues.ActualY7 * weightage) / 100;
            lineNextYearBudget.ActualY8 = (double)(budgetModel.NextYearMonthValues.ActualY8 * weightage) / 100;
            lineNextYearBudget.ActualY9 = (double)(budgetModel.NextYearMonthValues.ActualY9 * weightage) / 100;
            lineNextYearBudget.ActualY10 = (double)(budgetModel.NextYearMonthValues.ActualY10 * weightage) / 100;
            lineNextYearBudget.ActualY11 = (double)(budgetModel.NextYearMonthValues.ActualY11 * weightage) / 100;
            lineNextYearBudget.ActualY12 = (double)(budgetModel.NextYearMonthValues.ActualY12 * weightage) / 100;
        }

        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket 2585
        /// </summary>
        /// <param name="budgetModel"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private List<PlanBudgetModel> FilterPlanByTimeFrame(List<PlanBudgetModel> budgetModel, string year)
        {
            Contract.Requires<ArgumentNullException>(budgetModel != null, "Budget Model cannot be null.");
            Contract.Requires<ArgumentNullException>(budgetModel.Count > 0, "Budget Model Count cannot be less than zero.");

            foreach (PlanBudgetModel objPlan in budgetModel.Where(p => string.Compare(p.ActivityType, ActivityType.ActivityPlan, true) == 0).ToList())
            {
                if (!budgetModel.Where(ent => string.Compare(ent.ParentActivityId, objPlan.ActivityId, true) == 0).Any())
                {
                    int planId = Convert.ToInt32(objPlan.Id);
                    bool isChildExist = objDbMrpEntities.Plan_Campaign.Where(p => p.PlanId == planId && p.IsDeleted == false).Any();
                    if (isChildExist)
                    {
                        budgetModel.Remove(objPlan);
                    }
                    else
                    {
                        string firstYear = Common.GetInitialYearFromTimeFrame(year);
                        string lastYear = firstYear;
                        if (isMultiYear)
                        {
                            lastYear = Convert.ToString(Convert.ToInt32(firstYear) + 1);
                        }
                        if (objPlan.PlanYear != firstYear && objPlan.PlanYear != lastYear)
                        {
                            budgetModel.Remove(objPlan);
                        }
                    }
                }
            }
            return budgetModel;
        }


    }

}
