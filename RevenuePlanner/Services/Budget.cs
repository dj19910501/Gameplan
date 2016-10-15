using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Budget : IBudget
    {
        private MRPEntities objDbMrpEntities;
        private BDSService.BDSServiceClient objBDSServiceClient;
        private StoredProcedure objSp;
        private IColumnView objColumnView;
        RevenuePlanner.Services.ICurrency objCurrency;
        public Budget()
        {
            objDbMrpEntities = new MRPEntities();
            objBDSServiceClient = new BDSService.BDSServiceClient();
            objSp = new StoredProcedure();
            objColumnView = new ColumnView();
            objCurrency = new RevenuePlanner.Services.Currency();
        }
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
        public const string FixcolWidth = "100,100,100,100,10,100,302,75,100,110,100";
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
        //public const string ThreeDash = "---";
        public const string ThreeDash = "0";
        public const string BudgetFlagval = "2";
        public const string CostFlagVal = "1";
        public bool isMultiYear = false;


        public BudgetDHTMLXGridModel GetBudget(int ClientId, int UserID, string PlanIds, double PlanExchangeRate, string viewBy, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "")
        {
            string strThisQuarter = Enums.UpcomingActivities.ThisYearQuaterly.ToString();
            string strThisMonth = Enums.UpcomingActivities.ThisYearMonthly.ToString();
            //Set actual for quarters
            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToString().ToLower();
            //Check timeframe selected for this year quarterly or this year monthly data and for this year option isMultiyear flag will always be false
            if (year == strThisQuarter)
            {
                isMultiYear = false;
                year = DateTime.Now.Year.ToString();
            }
            else if (year == strThisMonth)
            {
                isMultiYear = false;
                AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.months.ToString()].ToString().ToLower();
                year = DateTime.Now.Year.ToString();
            }
            else
            {
                isMultiYear =Common.IsMultiyearTimeframe(year);
            }

            DataTable dt = objSp.GetBudget(PlanIds, UserID, viewBy, OwnerIds, TacticTypeids, StatusIds, year); //Get budget data for budget,planned cost and actual using store proc. GetplanBudget

            List<PlanBudgetModel> model = CreateBudgetDataModel(dt, PlanExchangeRate); //Convert datatable with budget data to PlanBudgetModel model

            model = FilterPlanByTimeFrame(model, year);//Except plan al entity be filter at Db level so we remove plan object by applying timeframe filter.  

            List<int> CustomFieldFilteredTacticIds = FilterCustomField(model, CustomFieldId);

            //filter budget model by custom field filter list
            if (CustomFieldFilteredTacticIds != null && CustomFieldFilteredTacticIds.Count > 0)
            {
                model.RemoveAll(a => string.Compare(a.ActivityType, ActivityType.ActivityTactic, true) == 0 && !CustomFieldFilteredTacticIds.Contains(Convert.ToInt32(a.Id)));
            }
            model = SetCustomFieldRestriction(model, UserID, ClientId);//Set customfield permission for budget cells. budget cell will editable or not.
            //int ViewByID = (int)viewBy;


            //get number of tab views for user in column management
            bool isPlangrid = false;
            bool isSelectAll = false;

            model = ManageLineItems(model);//Manage lineitems unallocated cost values in other line item

            #region "Calculate Monthly Budget from Bottom to Top for Hierarchy level like: LineItem > Tactic > Program > Campaign > CustomField(if filtered) > Plan"

            //// Set ViewBy data to model.
            model = CalculateBottomUp(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, viewBy);//// Calculate monthly Tactic budget from it's child budget i.e LineItem

            model = CalculateBottomUp(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic, viewBy);//// Calculate monthly Program budget from it's child budget i.e Tactic

            model = CalculateBottomUp(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, viewBy);//// Calculate monthly Campaign budget from it's child budget i.e Program

            model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, viewBy);//// Calculate monthly Plan budget from it's child budget i.e Campaign

            #endregion

            model = SetLineItemCostByWeightage(model, viewBy);//// Set LineItem monthly budget cost by it's parent tactic weightage.

            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel();
            objBudgetDHTMLXGrid = GenerateHeaderString(AllocatedBy, objBudgetDHTMLXGrid, model, year);

            objBudgetDHTMLXGrid = CreateDhtmlxFormattedBudgetData(objBudgetDHTMLXGrid, model, AllocatedBy, UserID, ClientId, year, viewBy);//create model to bind data in grid as per DHTMLx grid format.

            List<ColumnViewEntity> userManagedColumns = objColumnView.GetCustomfieldModel(ClientId, isPlangrid, out isSelectAll, UserID);
            string hiddenTab = string.Empty;
            if (!userManagedColumns.Where(u => u.EntityIsChecked).Any())
            {
                var PlannedColumn = userManagedColumns.Where(u => u.EntityType == Enums.Budgetcolumn.Planned.ToString()).FirstOrDefault();
                if (PlannedColumn != null)
                {
                    PlannedColumn.EntityIsChecked = true;
                }
            }
            foreach (ColumnViewEntity item in userManagedColumns.Where(u => !u.EntityIsChecked).ToList())
            {
                hiddenTab = hiddenTab + item.EntityType + ',';
            }
            objBudgetDHTMLXGrid.HiddenTab = hiddenTab;


            return objBudgetDHTMLXGrid;
        }

        public List<int> FilterCustomField(List<PlanBudgetModel> BudgetModel, string CustomFieldFilter)
        {
            List<int> lstTacticIds = new List<int>();
            if (BudgetModel != null && BudgetModel.Count > 0)
            {
                #region "Declare & Initialize local Variables"
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                string tacticType = Enums.EntityType.Tactic.ToString().ToUpper();
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

        public List<PlanBudgetModel> CreateBudgetDataModel(DataTable DtBudget, double PlanExchangeRate)
        {
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
                    CreatedBy = int.Parse(row["CreatedBy"].ToString()),
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
                        //Budget Month Allocation
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

                        //Cost Month Allocation
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

                        //Actuals Month Allocation
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
                        //Budget Month Allocation
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

                        //Cost Month Allocation
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

                        //Actuals Month Allocation
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

        public List<Budgetdataobj> SetBudgetDhtmlxFormattedValues(List<PlanBudgetModel> model, PlanBudgetModel Entity, string OwnerName, string EntityType, string AllocatedBy, bool IsNextYear, bool IsMultiyearPlan, string DhtmlxGridRowId, bool IsAddEntityRights, bool isViewBy = false, string pcptid = "", string TacticType = "")  // pcptid = Plan-Campaign-Program-Tactic-Id
        {
            List<Budgetdataobj> BudgetDataObjList = new List<Budgetdataobj>();
            Budgetdataobj BudgetDataObj = new Budgetdataobj();
            string Roistring = string.Empty;
            var PackageTacticIds = Entity.CalendarHoneycombpackageIDs;
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
            //Add LineItemTypeId into dhtmlx model
            BudgetDataObj.value = Convert.ToString(Entity.LineItemTypeId);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            //Add title of plan entity into dhtmlx model

            bool IsExtendedTactic = (Entity.EndDate.Year - Entity.StartDate.Year) > 0 ? true : false;
            int? LinkedTacticId = Entity.LinkTacticId;
            if (LinkedTacticId == 0)
            {
                LinkedTacticId = null;
            }
            string Linkedstring = string.Empty;
            if (string.Compare(Entity.ActivityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
            {
                Linkedstring = HttpUtility.HtmlEncode(((IsExtendedTactic == true && LinkedTacticId == null) ?
                                    "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>" :
                                        ((IsExtendedTactic == true && LinkedTacticId != null) || (LinkedTacticId != null)) ?
                                        "<div class='unlink-icon unlink-icon-grid'  LinkedPlanName='" + (string.IsNullOrEmpty(Entity.LinkedPlanName) ?
                                        null :
                                    Entity.LinkedPlanName.Replace("'", "&#39;")) + "' id = 'LinkIcon' ><i class='fa fa-link'></i></div>" : ""));
            }            

            if (Entity.AnchorTacticID != null && Entity.AnchorTacticID > 0 && !string.IsNullOrEmpty(Entity.Id) && Convert.ToString(Entity.AnchorTacticID) == Entity.Id)
            {
                // Get list of package tactic ids
                Roistring = "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id='pkgIcon' onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids='" + PackageTacticIds + "'><i class='fa fa-object-group'></i></div>";
                BudgetDataObj.value = HttpUtility.HtmlEncode(Roistring).Replace("'", "&#39;").Replace("\"", "&#34;") + Linkedstring + HttpUtility.HtmlEncode(Entity.ActivityName).Replace("'", "&#39;").Replace("\"", "&#34;");
            }
            else
            {
                BudgetDataObj.value = Linkedstring + HttpUtility.HtmlEncode(Entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            }

            if (Entity.ActivityType == ActivityType.ActivityLineItem && Entity.LineItemTypeId == null)
            {
                BudgetDataObj.locked = CellLocked;
                BudgetDataObj.style = NotEditableCellStyle;
            }
            else
            {
                BudgetDataObj.locked = Entity.isEntityEditable ? CellNotLocked : CellLocked;
                BudgetDataObj.style = Entity.isEntityEditable ? string.Empty : NotEditableCellStyle;
            }
            BudgetDataObjList.Add(BudgetDataObj);

            //Set icon of magnifying glass and honey comb for plan entity with respective ids
            Budgetdataobj iconsData = new Budgetdataobj();
            if (!isViewBy)
            {
                iconsData.value = HttpUtility.HtmlEncode(SetIcons(Entity, OwnerName, EntityType, DhtmlxGridRowId, IsAddEntityRights, pcptid, TacticType));
            }
            else
            {
                iconsData.value = string.Empty;
            }
            BudgetDataObjList.Add(iconsData);

            //Set Total Actual,Total Budget and Total planned cost for plan entity
            BudgetDataObjList = CampaignBudgetSummary(model, EntityType, Entity.ParentActivityId,
                  BudgetDataObjList, AllocatedBy, Entity.ActivityId, isViewBy);
            //Set monthly/quarterly allocation of budget,actuals and planned for plan
            BudgetDataObjList = CampaignMonth(model, EntityType, Entity.ParentActivityId,
                    BudgetDataObjList, AllocatedBy, Entity.ActivityId, IsNextYear, IsMultiyearPlan, isViewBy);
            BudgetDataObj = new Budgetdataobj();
            //Add UnAllocated Cost into dhtmlx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(Entity.UnallocatedCost);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            //Add unAllocated budget into dhtmlx model
            BudgetDataObj.style = NotEditableCellStyle;
            BudgetDataObj.value = Convert.ToString(Entity.TotalUnallocatedBudget);
            BudgetDataObjList.Add(BudgetDataObj);

            return BudgetDataObjList;
        }

        //html part of this function will be move into html helper as part of PL ticket 2676
        public string SetIcons(PlanBudgetModel Entity, string OwnerName, string EntityType, string DhtmlxGridRowId, bool IsAddEntityRights, string pcptid, string TacticType)
        {
            string doubledesh = "--";
            string IconsData = string.Empty;
            //Set icon of magnifying glass and honey comb for plan entity with respective ids
            string Title = HttpUtility.HtmlEncode(Entity.ActivityName.Replace("'", "&#39;").Replace("\"", "&#34;"));
            if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityPlan.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
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
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityCampaign.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
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
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityProgram.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
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
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityTactic.ToLower())
            {
                //LinkTactic Permission based on Entity Year
                bool LinkTacticPermission = ((Entity.EndDate.Year - Entity.StartDate.Year) > 0) ? true : false;
                string LinkedTacticId = Entity.LinkTacticId == 0 ? "null" : Entity.LinkTacticId.ToString();

                // Magnifying Glass to open Inspect Popup
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
                IconsData += " TacticType= '" + Convert.ToString(TacticType) + "' OwnerName= '" + Convert.ToString(OwnerName) + "' roitactictype='" + Entity.AssetType + "' anchortacticid='" + Entity.AnchorTacticID + "'  ";
                IconsData += " TaskName='" + Title;
                IconsData += "' altId=" + Convert.ToString(DhtmlxGridRowId) + " ColorCode=" + Convert.ToString(Entity.ColorCode);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " taskId=" + Convert.ToString(Entity.Id) + " csvId=Tactic_" + Convert.ToString(Entity.Id) + " ></div>";
            }
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityLineItem.ToLower() && Entity.LineItemTypeId != null)
            {
                // Magnifying Glass to open Inspect Popup
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

        public BudgetDHTMLXGridModel CreateDhtmlxFormattedBudgetData(BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> model, string AllocatedBy, int UserID, int ClientId, string Year, string viewBy)
        {



            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();

            if (viewBy != PlanGanttTypes.Tactic.ToString())
            {
                foreach (PlanBudgetModel bmViewby in model.Where(p => p.ActivityType == viewBy).OrderBy(p => p.ActivityName))
                {
                    BudgetDHTMLXGridDataModel gridViewbyData = new BudgetDHTMLXGridDataModel();
                    List<BudgetDHTMLXGridDataModel> gridjsonlistViewby = new List<BudgetDHTMLXGridDataModel>();
                    gridViewbyData.id = bmViewby.ActivityId;
                    gridViewbyData.open = Open;
                    List<Budgetdataobj> BudgetviewbyDataList;
                    string EntityType = viewBy;
                    bool isViewby = true;
                    BudgetviewbyDataList = SetBudgetDhtmlxFormattedValues(model, bmViewby, string.Empty, EntityType, AllocatedBy, false, false, bmViewby.ActivityId, false, isViewby);
                    gridViewbyData.data = BudgetviewbyDataList;
                    List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(model, UserID, ClientId, Year, AllocatedBy, isViewby, bmViewby.ActivityId);
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
                List<BudgetDHTMLXGridDataModel> gridJsondata = GenerateHierarchy(model, UserID, ClientId, Year, AllocatedBy, false);
                foreach (BudgetDHTMLXGridDataModel item in gridJsondata)
                {
                    gridjsonlist.Add(item);
                }
            }

            //Set plan entity in the dhtmlx formated model at top level of the hierarchy using loop

            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            objBudgetDHTMLXGrid.Grid.rows = gridjsonlist;
            return objBudgetDHTMLXGrid;
        }

        private List<BudgetDHTMLXGridDataModel> GenerateHierarchy(List<PlanBudgetModel> model, int UserID, int ClientId, string Year, string AllocatedBy, bool isViewBy, string ParentId = "")
        {
            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel gridjsonlistPlanObj = new BudgetDHTMLXGridDataModel();
            List<Budgetdataobj> BudgetDataObjList;

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


            Dictionary<int, string> lstTacticTypeTitle = new Dictionary<int, string>();
            List<int> TacticTypeIds = model.Where(t => t.ActivityType == ActivityType.ActivityTactic).Select(t => t.TacticTypeId).ToList();
            lstTacticTypeTitle = objDbMrpEntities.TacticTypes.Where(tt => TacticTypeIds.Contains(tt.TacticTypeId) && tt.IsDeleted == false).ToDictionary(tt => tt.TacticTypeId, tt => tt.Title);
            foreach (PlanBudgetModel bm in model.Where(p => p.ActivityType == ActivityType.ActivityPlan && (!isViewBy || p.ParentActivityId == ParentId)).OrderBy(p => p.ActivityName))
            {
                gridjsonlistPlanObj = new BudgetDHTMLXGridDataModel();
                if (IsPlanCreateAll == false)
                {
                    if (bm.CreatedBy == UserID || lstSubordinatesIds.Contains(bm.CreatedBy))
                        IsPlanCreateAll = true;
                    else
                        IsPlanCreateAll = false;
                }
                bool isCampignExist = model.Where(p => p.ParentActivityId == bm.ActivityId).Any();
                DateTime MaxDate = default(DateTime); ;
                if (isCampignExist)
                {
                    MaxDate = model.Where(p => p.ParentActivityId == bm.ActivityId).Max(a => a.EndDate);
                }

                //Set flag to identify plan year. e.g.if timeframe is 2015-2016 and plan have plan year 2016 then we will not set month data for Jan-2015 to Dec-2015 of respective plan
                bool isNextYearPlan = false;
                bool isMultiYearPlan = false;
                string firstYear =Common.GetInitialYearFromTimeFrame(Year);
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
                gridjsonlistPlanObj.id = bm.TaskId;//ActivityType.ActivityPlan + "_" + HttpUtility.HtmlEncode(bm.ActivityId);
                gridjsonlistPlanObj.open = Open;

                string OwnerName = string.Empty;

                OwnerName = Convert.ToString(bm.CreatedBy);
                BudgetDataObjList = SetBudgetDhtmlxFormattedValues(model, bm, OwnerName, ActivityType.ActivityPlan, AllocatedBy, isNextYearPlan, isMultiYearPlan, gridjsonlistPlanObj.id, IsPlanCreateAll);
                gridjsonlistPlanObj.data = BudgetDataObjList;

                List<BudgetDHTMLXGridDataModel> CampaignRowsObjList = new List<BudgetDHTMLXGridDataModel>();
                BudgetDHTMLXGridDataModel CampaignRowsObj = new BudgetDHTMLXGridDataModel();
                foreach (
                    PlanBudgetModel bmc in
                        model.Where(
                            p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == bm.ActivityId).OrderBy(p => p.ActivityName)
                    )
                {
                    CampaignRowsObj = new BudgetDHTMLXGridDataModel();
                    CampaignRowsObj.id = bmc.TaskId;//ActivityType.ActivityCampaign + "_" + HttpUtility.HtmlEncode(bmc.ActivityId);
                    CampaignRowsObj.open = null;

                    bool IsCampCreateAll = IsPlanCreateAll = IsPlanCreateAll == false ? (bmc.CreatedBy == UserID || lstSubordinatesIds.Contains(bmc.CreatedBy)) ? true : false : true;

                    OwnerName = Convert.ToString(bm.CreatedBy);
                    List<Budgetdataobj> CampaignDataObjList = SetBudgetDhtmlxFormattedValues(model, bmc, OwnerName, ActivityType.ActivityCampaign, AllocatedBy, isNextYearPlan, isMultiYearPlan, CampaignRowsObj.id, IsCampCreateAll);

                    CampaignRowsObj.data = CampaignDataObjList;
                    List<BudgetDHTMLXGridDataModel> ProgramRowsObjList = new List<BudgetDHTMLXGridDataModel>();
                    BudgetDHTMLXGridDataModel ProgramRowsObj = new BudgetDHTMLXGridDataModel();
                    foreach (
                        PlanBudgetModel bmp in
                            model.Where(
                                p =>
                                    p.ActivityType == ActivityType.ActivityProgram &&
                                    p.ParentActivityId == bmc.ActivityId).OrderBy(p => p.ActivityName))
                    {
                        ProgramRowsObj = new BudgetDHTMLXGridDataModel();
                        ProgramRowsObj.id = bmp.TaskId;//ActivityType.ActivityProgram + "_" + HttpUtility.HtmlEncode(bmp.ActivityId);
                        ProgramRowsObj.open = null;

                        bool IsProgCreateAll = IsPlanCreateAll = IsPlanCreateAll == false ? (bmp.CreatedBy == UserID || lstSubordinatesIds.Contains(bmp.CreatedBy)) ? true : false : true;

                        OwnerName = Convert.ToString(bm.CreatedBy);
                        List<Budgetdataobj> ProgramDataObjList = SetBudgetDhtmlxFormattedValues(model, bmp, OwnerName, ActivityType.ActivityProgram, AllocatedBy, isNextYearPlan, isMultiYearPlan, ProgramRowsObj.id, IsProgCreateAll);
                        ProgramRowsObj.data = ProgramDataObjList;

                        List<BudgetDHTMLXGridDataModel> TacticRowsObjList = new List<BudgetDHTMLXGridDataModel>();
                        BudgetDHTMLXGridDataModel TacticRowsObj = new BudgetDHTMLXGridDataModel();
                        foreach (
                            PlanBudgetModel bmt in
                                model.Where(
                                    p =>
                                        p.ActivityType == ActivityType.ActivityTactic &&
                                        p.ParentActivityId == bmp.ActivityId).OrderBy(p => p.ActivityName).OrderBy(p => p.ActivityName))
                        {
                            TacticRowsObj = new BudgetDHTMLXGridDataModel();
                            TacticRowsObj.id = bmt.TaskId;//ActivityType.ActivityTactic + "_" + HttpUtility.HtmlEncode(bmt.ActivityId);
                            TacticRowsObj.open = null;

                            bool IsTacCreateAll = IsPlanCreateAll == false ? (bmt.CreatedBy == UserID || lstSubordinatesIds.Contains(bmt.CreatedBy)) ? true : false : true;


                            OwnerName = Convert.ToString(bm.CreatedBy);
                            string TacticType = string.Empty;
                            if (lstTacticTypeTitle != null && lstTacticTypeTitle.Count > 0)
                            {
                                if (lstTacticTypeTitle.ContainsKey(bmt.TacticTypeId))
                                {
                                    TacticType = Convert.ToString(lstTacticTypeTitle[bmt.TacticTypeId]);
                                }
                            }
                            List<Budgetdataobj> TacticDataObjList = SetBudgetDhtmlxFormattedValues(model, bmt, OwnerName, ActivityType.ActivityTactic, AllocatedBy, isNextYearPlan, isMultiYearPlan, TacticRowsObj.id, IsTacCreateAll, false, "L" + bm.ActivityId + "_C" + bmc.ActivityId + "_P" + bmp.ActivityId + "_T" + bmt.ActivityId, TacticType);

                            TacticRowsObj.data = TacticDataObjList;
                            List<BudgetDHTMLXGridDataModel> LineRowsObjList = new List<BudgetDHTMLXGridDataModel>();
                            BudgetDHTMLXGridDataModel LineRowsObj = new BudgetDHTMLXGridDataModel();
                            foreach (
                                PlanBudgetModel bml in
                                    model.Where(
                                        p =>
                                            p.ActivityType == ActivityType.ActivityLineItem &&
                                            p.ParentActivityId == bmt.ActivityId).OrderBy(p => p.ActivityName))
                            {
                                LineRowsObj = new BudgetDHTMLXGridDataModel();
                                LineRowsObj.id = bml.TaskId;//ActivityType.ActivityLineItem + "_" + HttpUtility.HtmlEncode(bml.ActivityId);
                                LineRowsObj.open = null;

                                bool IsLinItmCreateAll = IsPlanCreateAll == false ? (bml.CreatedBy == UserID || lstSubordinatesIds.Contains(bml.CreatedBy)) ? true : false : true;

                                OwnerName = Convert.ToString(bm.CreatedBy);
                                List<Budgetdataobj> LineDataObjList = SetBudgetDhtmlxFormattedValues(model, bml, OwnerName, ActivityType.ActivityLineItem, AllocatedBy, isNextYearPlan, isMultiYearPlan, LineRowsObj.id, IsLinItmCreateAll);

                                LineRowsObj.data = LineDataObjList;
                                LineRowsObjList.Add(LineRowsObj);
                            }
                            //set lineitem row data as child to respective tactic
                            TacticRowsObj.rows = LineRowsObjList;
                            TacticRowsObjList.Add(TacticRowsObj);
                        }
                        //set tactic row data as child to respective program
                        ProgramRowsObj.rows = TacticRowsObjList;
                        ProgramRowsObjList.Add(ProgramRowsObj);
                    }
                    //set program row data as child to respective campaign
                    CampaignRowsObj.rows = ProgramRowsObjList;
                    CampaignRowsObjList.Add(CampaignRowsObj);
                }
                //set campaign row data as child to respective plan
                gridjsonlistPlanObj.rows = CampaignRowsObjList;
                //gridjsonlist.Add(gridjsonlistPlanObj);
                gridjsonlist.Add(gridjsonlistPlanObj);
            }
            return gridjsonlist;
        }

        private BudgetDHTMLXGridModel GenerateHeaderString(string AllocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> model, string Year)
        {
            string firstYear = Common.GetInitialYearFromTimeFrame(Year);
            string lastYear = string.Empty;
            //check if multiyear flag is on then last year will be firstyear+1
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
            string headerYear = firstYear;//column header year text which will bind with respective header
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
            {
                int quarterCounter = 1;

                int multiYearCounter = 23;//If budget data need to show with multi year then set header for multi quarter
                if (!isMultiYear)
                {
                    multiYearCounter = 11;
                }
                for (int i = 1; i <= multiYearCounter; i += 3)
                {
                    //datetime object will be used to find respective month text by month numbers
                    DateTime dt;
                    if (i < 12)
                    {
                        dt = new DateTime(2012, i, 1);
                    }
                    else
                    {
                        dt = new DateTime(2012, i - 12, 1);
                    }
                    setHeader.Append(",Q").Append(quarterCounter.ToString()).Append("-").Append(headerYear)
                  .Append(" Budget ").Append(manageviewicon).Append(",Q").Append(quarterCounter.ToString())
                  .Append("-").Append(headerYear).Append(" Planned ").Append(manageviewicon)
                  .Append(",Q").Append(quarterCounter.ToString()).Append("-").Append(headerYear)
                  .Append(" Actual ").Append(manageviewicon);


                    columnIds = columnIds + "," + "Budget,Planned,Actual";
                    colType = colType + ",edn,edn,edn";
                    width = width + ",130,130,130";
                    colSorting = colSorting + ",int,int,int";

                    if (quarterCounter == 4)//Check if queter counter reach to last quarter then reset it
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
                    width = width + ",140,140,140";
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

        private List<Budgetdataobj> CampaignBudgetSummary(List<PlanBudgetModel> model, string activityType, string parentActivityId, List<Budgetdataobj> BudgetDataObjList, string allocatedBy, string activityId, bool isViewby = false)
        {
            PlanBudgetModel Entity = model.Where(pl => pl.ActivityType == activityType && pl.ParentActivityId == parentActivityId && pl.ActivityId == activityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();
            double ChildTotalBudget = model.Where(cl => cl.ParentActivityId == activityId).Sum(cl => cl.YearlyBudget);
            if (Entity != null)
            {
                Budgetdataobj objTotalBudget = new Budgetdataobj();
                Budgetdataobj objTotalCost = new Budgetdataobj();
                Budgetdataobj objTotalActual = new Budgetdataobj();
                //entity type line item has no budget so we set '---' value for line item
                if (!isViewby)
                {
                    if (Entity.ActivityType == ActivityType.ActivityLineItem)
                    {
                        objTotalBudget.value = ThreeDash;//Set values for Total budget
                        objTotalBudget.locked = CellLocked;
                        objTotalBudget.style = NotEditableCellStyle;
                    }
                    else
                    {
                        objTotalBudget.value = Convert.ToString(Entity.YearlyBudget);//Set values for Total budget
                        objTotalBudget.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                        objTotalBudget.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    }

                    objTotalActual.value = Convert.ToString(Entity.TotalActuals);//Set values for Total actual
                    objTotalActual.locked = CellLocked;
                    objTotalActual.style = NotEditableCellStyle;

                    bool isOtherLineItem = activityType == ActivityType.ActivityLineItem && Entity.LineItemTypeId == null;
                    objTotalCost.value = Convert.ToString(Entity.TotalAllocatedCost);
                    objTotalCost.locked = Entity.isCostEditable && !isOtherLineItem ? CellNotLocked : CellLocked;
                    objTotalCost.style = Entity.isCostEditable && !isOtherLineItem ? string.Empty : NotEditableCellStyle;
                    if (Common.ParseDoubleValue(objTotalCost.value) > Common.ParseDoubleValue(objTotalBudget.value) && Entity.ActivityType != ActivityType.ActivityLineItem)
                    {
                        objTotalCost.style = objTotalCost.style + RedCornerStyle;
                        objTotalCost.actval = CostFlagVal;
                    }
                    if (ChildTotalBudget > Common.ParseDoubleValue(objTotalBudget.value))
                    {
                        objTotalBudget.style = objTotalBudget.style + OrangeCornerStyle;
                        objTotalBudget.actval = BudgetFlagval;
                    }
                }
                else
                {
                    objTotalBudget.value = ThreeDash;//Set values for Total budget
                    objTotalBudget.locked = CellLocked;
                    objTotalBudget.style = NotEditableCellStyle;
                    objTotalActual.value = ThreeDash;
                    objTotalActual.locked = CellLocked;
                    objTotalActual.style = NotEditableCellStyle;
                    objTotalCost.value = ThreeDash;
                    objTotalCost.locked = CellLocked;
                    objTotalCost.style = NotEditableCellStyle;
                }


                BudgetDataObjList.Add(objTotalBudget);
                BudgetDataObjList.Add(objTotalCost);
                BudgetDataObjList.Add(objTotalActual);

            }
            return BudgetDataObjList;
        }

        private List<Budgetdataobj> CampaignMonth(List<PlanBudgetModel> model, string activityType, string parentActivityId, List<Budgetdataobj> BudgetDataObjList, string allocatedBy, string activityId, bool isNextYearPlan, bool IsMulityearPlan, bool isViewBy = false)
        {
            PlanBudgetModel Entity = model.Where(pl => pl.ActivityType == activityType && pl.ParentActivityId == parentActivityId && pl.ActivityId == activityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();
            bool isTactic = activityType == Helpers.ActivityType.ActivityTactic ? true : false;
            bool isLineItem = activityType == Helpers.ActivityType.ActivityLineItem ? true : false;
            bool isOtherLineitem = activityType == Helpers.ActivityType.ActivityLineItem && Entity.LineItemTypeId == null ? true : false;
            if (string.Compare(allocatedBy, "quarters", true) != 0)
            {
                if (!isNextYearPlan)
                {
                    BudgetDataObjList = CampignMonthlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, isViewBy,isOtherLineitem);
                }
            }
            else
            {
                if (!isNextYearPlan)
                {
                    BudgetDataObjList = CampignQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsMulityearPlan, isViewBy,isOtherLineitem);
                }
                else if (!isMultiYear)
                {
                    BudgetDataObjList = CampignNextYearQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsMulityearPlan, isViewBy, isOtherLineitem);
                }
                else
                {
                    BudgetDataObjList = CampignMulitYearQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, isViewBy, isOtherLineitem);
                }
            }
            return BudgetDataObjList;
        }

        private List<Budgetdataobj> CampignMonthlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool isViewby = false, bool IsOtherLineItem = false)
        {
            for (int monthNo = 1; monthNo <= 12; monthNo++)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                if (!isViewby)
                {
                    objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.locked = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;
                    if (monthNo == 1)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY1) : ThreeDash;
                        
                        if (!isLineItem && Entity.MonthValues.BudgetY1 < Entity.ChildMonthValues.BudgetY1 )
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY1);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY1);
                    }
                    else if (monthNo == 2)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY2) : ThreeDash;

                        if (!isLineItem && Entity.MonthValues.BudgetY2 < Entity.ChildMonthValues.BudgetY2)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY2);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY2);
                    }
                    else if (monthNo == 3)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY3) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY3 < Entity.ChildMonthValues.BudgetY3)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY3);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY3);
                    }
                    else if (monthNo == 4)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY4) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY4 < Entity.ChildMonthValues.BudgetY4)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY4);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY4);
                    }
                    else if (monthNo == 5)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY5) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY5 < Entity.ChildMonthValues.BudgetY5)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY5);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY5);
                    }
                    else if (monthNo == 6)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY6) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY6 < Entity.ChildMonthValues.BudgetY6)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY6);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY6);
                    }
                    else if (monthNo == 7)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY7) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY7 < Entity.ChildMonthValues.BudgetY7)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY7);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY7);
                    }
                    else if (monthNo == 8)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY8) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY8 < Entity.ChildMonthValues.BudgetY8)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY8);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY8);
                    }
                    else if (monthNo == 9)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY9) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY9 < Entity.ChildMonthValues.BudgetY9)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY9);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY9);
                    }
                    else if (monthNo == 10)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY10) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY10 < Entity.ChildMonthValues.BudgetY10)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY10);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY10);
                    }
                    else if (monthNo == 11)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY11) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY11 < Entity.ChildMonthValues.BudgetY11)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY11);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY11);
                    }
                    else if (monthNo == 12)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY12) : ThreeDash;
                        if (!isLineItem && Entity.MonthValues.BudgetY12 < Entity.ChildMonthValues.BudgetY12)
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY12);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY12);
                    }
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.actval = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.locked = CellLocked;
                    objCostMonth.locked = CellLocked;
                    objActualMonth.locked = CellLocked;
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

        private List<Budgetdataobj> CampignQuarterlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsMultiYearPlan, bool isViewby = false, bool IsOtherLineItem = false)
        {
            int multiYearCounter = 23;
            if (!isMultiYear)
            {
                multiYearCounter = 11;
            }
            for (int i = 1; i <= multiYearCounter; i += 3)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.locked = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;
                if (!isViewby)
                {
                    if (i == 1)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY1 + Entity.MonthValues.BudgetY2 + Entity.MonthValues.BudgetY3) : ThreeDash;
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY1 + Entity.ChildMonthValues.BudgetY2 + Entity.ChildMonthValues.BudgetY3))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY1 + Entity.MonthValues.CostY2 + Entity.MonthValues.CostY3);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY1 + Entity.MonthValues.ActualY2 + Entity.MonthValues.ActualY3);

                    }
                    else if (i == 4)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY4 + Entity.MonthValues.BudgetY5 + Entity.MonthValues.BudgetY6) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY4 + Entity.ChildMonthValues.BudgetY5 + Entity.ChildMonthValues.BudgetY6) )
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY4 + Entity.MonthValues.CostY5 + Entity.MonthValues.CostY6);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY4 + Entity.MonthValues.ActualY5 + Entity.MonthValues.ActualY6);
                    }
                    else if (i == 7)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY7 + Entity.MonthValues.BudgetY8 + Entity.MonthValues.BudgetY9) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY7 + Entity.ChildMonthValues.BudgetY8 + Entity.ChildMonthValues.BudgetY9))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY7 + Entity.MonthValues.CostY8 + Entity.MonthValues.CostY9);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY7 + Entity.MonthValues.ActualY8 + Entity.MonthValues.ActualY9);
                    }
                    else if (i == 10)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY10 + Entity.MonthValues.BudgetY11 + Entity.MonthValues.BudgetY12) : ThreeDash;
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY10 + Entity.ChildMonthValues.BudgetY11 + Entity.ChildMonthValues.BudgetY12))
                        {
                            objBudgetMonth.style =   objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY10 + Entity.MonthValues.CostY11 + Entity.MonthValues.CostY12);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY10 + Entity.MonthValues.ActualY11 + Entity.MonthValues.ActualY12);
                    }
                    else if (i == 13)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY1 + Entity.NextYearMonthValues.BudgetY2 + Entity.NextYearMonthValues.BudgetY3) : ThreeDash;
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY1 + Entity.ChildNextYearMonthValues.BudgetY2 + Entity.ChildNextYearMonthValues.BudgetY3))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CostY1 + Entity.NextYearMonthValues.CostY2 + Entity.NextYearMonthValues.CostY3) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.ActualY1 + Entity.NextYearMonthValues.ActualY2 + Entity.NextYearMonthValues.ActualY3) : ThreeDash;
                    }
                    else if (i == 16)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY4 + Entity.NextYearMonthValues.BudgetY5 + Entity.NextYearMonthValues.BudgetY6) : ThreeDash;
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY4 + Entity.ChildNextYearMonthValues.BudgetY5 + Entity.ChildNextYearMonthValues.BudgetY6))
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CostY4 + Entity.NextYearMonthValues.CostY5 + Entity.NextYearMonthValues.CostY6) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.ActualY4 + Entity.NextYearMonthValues.ActualY5 + Entity.NextYearMonthValues.ActualY6) : ThreeDash;
                    }
                    else if (i == 19)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY7 + Entity.NextYearMonthValues.BudgetY8 + Entity.NextYearMonthValues.BudgetY9) : ThreeDash;
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY7 + Entity.ChildNextYearMonthValues.BudgetY8 + Entity.ChildNextYearMonthValues.BudgetY9))
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CostY7 + Entity.NextYearMonthValues.CostY8 + Entity.NextYearMonthValues.CostY9) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.ActualY7 + Entity.NextYearMonthValues.ActualY8 + Entity.NextYearMonthValues.ActualY9) : ThreeDash;
                    }
                    else if (i == 22)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY10 + Entity.NextYearMonthValues.BudgetY11 + Entity.NextYearMonthValues.BudgetY12) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY10 + Entity.ChildNextYearMonthValues.BudgetY11 + Entity.ChildNextYearMonthValues.BudgetY12))
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CostY10 + Entity.NextYearMonthValues.CostY11 + Entity.NextYearMonthValues.CostY12) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.ActualY10 + Entity.NextYearMonthValues.ActualY11 + Entity.NextYearMonthValues.ActualY12) : ThreeDash;
                    }
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.actval = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.locked = CellLocked;
                    objCostMonth.locked = CellLocked;
                    objActualMonth.locked = CellLocked;
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

        private List<Budgetdataobj> CampignMulitYearQuarterlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool isViewBy = false, bool IsOtherLineItem = false)
        {
            for (int monthNo = 1; monthNo <= 23; monthNo += 3)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                if (!isViewBy)
                {
                    objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.locked = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                    if (monthNo < 13)
                    {
                        objBudgetMonth.value = ThreeDash;
                        objBudgetMonth.locked = CellLocked;
                        objBudgetMonth.style = NotEditableCellStyle;

                        objCostMonth.value = ThreeDash;
                        objCostMonth.locked = CellLocked;
                        objCostMonth.style = NotEditableCellStyle;

                        objActualMonth.value = ThreeDash;
                        objActualMonth.locked = CellLocked;
                        objActualMonth.style = NotEditableCellStyle;
                    }
                    else if (monthNo == 13)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY1 + Entity.MonthValues.BudgetY2 + Entity.MonthValues.BudgetY3) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY1 + Entity.ChildMonthValues.BudgetY2 + Entity.ChildMonthValues.BudgetY3))
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY1 + Entity.MonthValues.CostY2 + Entity.MonthValues.CostY3);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY1 + Entity.MonthValues.ActualY2 + Entity.MonthValues.ActualY3);

                    }
                    else if (monthNo == 16)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY4 + Entity.MonthValues.BudgetY5 + Entity.MonthValues.BudgetY6) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY4 + Entity.ChildMonthValues.BudgetY5 + Entity.ChildMonthValues.BudgetY6) )
                        {
                            objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY4 + Entity.MonthValues.CostY5 + Entity.MonthValues.CostY6);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY4 + Entity.MonthValues.ActualY5 + Entity.MonthValues.ActualY6);
                    }
                    else if (monthNo == 19)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY7 + Entity.MonthValues.BudgetY8 + Entity.MonthValues.BudgetY9) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY7 + Entity.ChildMonthValues.BudgetY8 + Entity.ChildMonthValues.BudgetY9))
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY7 + Entity.MonthValues.CostY8 + Entity.MonthValues.CostY9);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY7 + Entity.MonthValues.ActualY8 + Entity.MonthValues.ActualY9);
                    }
                    else if (monthNo == 22)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.BudgetY10 + Entity.MonthValues.BudgetY11 + Entity.MonthValues.BudgetY12) : ThreeDash;
                        
                        if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildMonthValues.BudgetY10 + Entity.ChildMonthValues.BudgetY11 + Entity.ChildMonthValues.BudgetY12))
                        {
                            objBudgetMonth.style =  objBudgetMonth.style + OrangeCornerStyle ;
                            objBudgetMonth.actval = BudgetFlagval;
                        }
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CostY10 + Entity.MonthValues.CostY11 + Entity.MonthValues.CostY12);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ActualY10 + Entity.MonthValues.ActualY11 + Entity.MonthValues.ActualY12);
                    }
                    if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
                    {
                        objCostMonth.style = objCostMonth.style + RedCornerStyle;
                        objCostMonth.actval = CostFlagVal;
                    }
                }
                else
                {
                    objBudgetMonth.locked = CellLocked;
                    objCostMonth.locked = CellLocked;
                    objActualMonth.locked = CellLocked;
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

        private List<Budgetdataobj> CampignNextYearQuarterlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsMultiYearPlan, bool isViewby = false, bool IsOtherLineItem = false)
        {
            for (int monthNo = 13; monthNo <= 23; monthNo += 3)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                objActualMonth.locked = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                objActualMonth.style = !IsOtherLineItem && Entity.isActualEditable && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;
                if (monthNo == 13)
                {
                    objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY1 + Entity.NextYearMonthValues.BudgetY2 + Entity.NextYearMonthValues.BudgetY3) : ThreeDash;
                    
                    if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY1 + Entity.ChildNextYearMonthValues.BudgetY2 + Entity.ChildNextYearMonthValues.BudgetY3))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle ;
                        objBudgetMonth.actval = BudgetFlagval;
                    }
                    objCostMonth.value = Convert.ToString(Entity.NextYearMonthValues.CostY1 + Entity.NextYearMonthValues.CostY2 + Entity.NextYearMonthValues.CostY3);
                    objActualMonth.value = Convert.ToString(Entity.NextYearMonthValues.ActualY1 + Entity.NextYearMonthValues.ActualY2 + Entity.NextYearMonthValues.ActualY3);

                }
                else if (monthNo == 16)
                {
                    objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY4 + Entity.NextYearMonthValues.BudgetY5 + Entity.NextYearMonthValues.BudgetY6) : ThreeDash;
                    
                    if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY4 + Entity.ChildNextYearMonthValues.BudgetY5 + Entity.ChildNextYearMonthValues.BudgetY6))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                        objBudgetMonth.actval = BudgetFlagval;
                    }
                    objCostMonth.value = Convert.ToString(Entity.NextYearMonthValues.CostY4 + Entity.NextYearMonthValues.CostY5 + Entity.NextYearMonthValues.CostY6);
                    objActualMonth.value = Convert.ToString(Entity.NextYearMonthValues.ActualY4 + Entity.NextYearMonthValues.ActualY5 + Entity.NextYearMonthValues.ActualY6);
                }
                else if (monthNo == 19)
                {
                    objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY7 + Entity.NextYearMonthValues.BudgetY8 + Entity.NextYearMonthValues.BudgetY9) : ThreeDash;
                    
                    if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY7 + Entity.ChildNextYearMonthValues.BudgetY8 + Entity.ChildNextYearMonthValues.BudgetY9))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                        objBudgetMonth.actval = BudgetFlagval;
                    }
                    objCostMonth.value = Convert.ToString(Entity.NextYearMonthValues.CostY7 + Entity.NextYearMonthValues.CostY8 + Entity.NextYearMonthValues.CostY9);
                    objActualMonth.value = Convert.ToString(Entity.NextYearMonthValues.ActualY7 + Entity.NextYearMonthValues.ActualY8 + Entity.NextYearMonthValues.ActualY9);
                }
                else if (monthNo == 22)
                {
                    objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.BudgetY10 + Entity.NextYearMonthValues.BudgetY11 + Entity.NextYearMonthValues.BudgetY12) : ThreeDash;
                    
                    if (!isLineItem && Common.ParseDoubleValue(objBudgetMonth.value) < (Entity.ChildNextYearMonthValues.BudgetY10 + Entity.ChildNextYearMonthValues.BudgetY11 + Entity.ChildNextYearMonthValues.BudgetY12))
                    {
                        objBudgetMonth.style = objBudgetMonth.style + OrangeCornerStyle;
                        objBudgetMonth.actval = BudgetFlagval;
                    }
                    objCostMonth.value = Convert.ToString(Entity.NextYearMonthValues.CostY10 + Entity.NextYearMonthValues.CostY11 + Entity.NextYearMonthValues.CostY12);
                    objActualMonth.value = Convert.ToString(Entity.NextYearMonthValues.ActualY10 + Entity.NextYearMonthValues.ActualY11 + Entity.NextYearMonthValues.ActualY12);
                }
                if (Common.ParseDoubleValue(objCostMonth.value) > Common.ParseDoubleValue(objBudgetMonth.value) && !isLineItem)
                {
                    objCostMonth.style = objCostMonth.style + RedCornerStyle;
                    objCostMonth.actval = CostFlagVal;
                }
                BudgetDataObjList.Add(objBudgetMonth);
                BudgetDataObjList.Add(objCostMonth);
                BudgetDataObjList.Add(objActualMonth);
            }
            return BudgetDataObjList;
        }

        public List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> BudgetModel, int UserId, int ClientId)
        {
            List<int> lstSubordinatesIds = new List<int>();

            //get list of subordiantes which will be use to chekc if user is subordinate
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(UserId);
            }
            //Custom field type dropdown list
            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);
            //Custom field type text box
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            //flag will be use to set if custom field is display for filter or not
            bool isDisplayForFilter = false;

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), ClientId);

            //Get list tactic's custom field
            List<CustomField> customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == ClientId && customfield.EntityType.Equals(EntityTypeTactic) && customfield.IsDeleted.Equals(false)).ToList();
            //Check custom field whic are not set to display for filter and is required are exist
            bool CustomFieldexists = customfieldlist.Where(customfield => customfield.IsRequired && !isDisplayForFilter).Any();
            //get dropdown type of custom fields ids
            List<int> customfieldids = customfieldlist.Where(customfield => customfield.CustomFieldType.Name == DropDownList && (isDisplayForFilter ? customfield.IsDisplayForFilter : true)).Select(customfield => customfield.CustomFieldId).ToList();
            //Get tactics only for budget model
            List<string> tacIds = BudgetModel.Where(t => t.ActivityType.ToUpper() == EntityTypeTactic.ToUpper()).Select(t => t.Id).ToList();

            //get tactic ids from tactic list
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).ToList();

            //Get tactic custom fields list
            List<CustomField_Entity> lstAllTacticCustomFieldEntities = Entities.Where(customFieldEntity => customfieldids.Contains(customFieldEntity.CustomFieldId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList();
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);


            #region "Set Permissions"
            #region "Set Plan Permission"
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);

            if (IsPlanEditAllAuthorized)
            {
                BudgetModel.Where(item => item.ActivityType == ActivityType.ActivityPlan)
                            .ToList().ForEach(item =>
                            {
                                item.isBudgetEditable = item.isEntityEditable = true;
                            });
            }
            else
            {
                BudgetModel.Where(item => (item.ActivityType == ActivityType.ActivityPlan) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                           .ToList().ForEach(item =>
                           {
                               item.isBudgetEditable = item.isEntityEditable = true;
                           });
            }
            #endregion

            int allwTaccount = 0;
            List<string> lstTacs = BudgetModel.Where(item => item.ActivityType == ActivityType.ActivityTactic).Select(t => t.Id).ToList();
            List<int> tIds = lstTacs.ConvertAll(s => Int32.Parse(s));
            List<int> lstAllAllowedTacIds = Common.GetEditableTacticListPO(UserId, ClientId, tIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);

            #region "Set Campaign Permission"
            BudgetModel.Where(item => (item.ActivityType == ActivityType.ActivityCampaign) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                       .ToList().ForEach(item =>
                {
                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        //to find tactic level permission ,first get program list and then get respective tactic list of program which will be used to get editable tactic list
                        List<string> modelprogramid = BudgetModel.Where(minner => minner.ActivityType == ActivityType.ActivityProgram && minner.ParentActivityId == item.ActivityId).Select(minner => minner.ActivityId).ToList();
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && modelprogramid.Contains(m.ParentActivityId)).Select(m => Convert.ToInt32(m.Id)).ToList();
                               //lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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

            BudgetModel.Where(item => (item.ActivityType == ActivityType.ActivityProgram) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                {

                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        //to find tactic level permission , get respective tactic list of program which will be used to get editable tactic list
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && m.ParentActivityId == item.ActivityId).Select(m => Convert.ToInt32(m.Id)).ToList();
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


            BudgetModel.Where(item => (item.ActivityType == ActivityType.ActivityTactic) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                {
                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                           bool isLineItem = BudgetModel.Where(ent => ent.ParentActivityId == item.ActivityId && item.LineItemTypeId != null).Any();
                           ////Check tactic is editable or not
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
            BudgetModel.Where(item => (item.ActivityType == ActivityType.ActivityLineItem) && ((item.CreatedBy == UserId) || (lstSubordinatesIds.Contains(item.CreatedBy))))
                   .ToList().ForEach(item =>
                {
                    int tacticOwner = 0;
                    if (BudgetModel.Where(m => m.ActivityId == item.ParentActivityId).Any())
                    {
                        tacticOwner = BudgetModel.Where(m => m.ActivityId == item.ParentActivityId).FirstOrDefault().CreatedBy;
                    }

                    //chek user is subordinate or user is owner of line item or user is owner of tactic
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

        private List<PlanBudgetModel> ManageLineItems(List<PlanBudgetModel> model)
        {
            BudgetMonth lineDiff;
            BudgetMonth NextYearlineDiff;
            foreach (PlanBudgetModel l in model.Where(l => l.ActivityType == ActivityType.ActivityTactic))
            {
                //// Calculate Line items Difference.
                lineDiff = new BudgetMonth();
                NextYearlineDiff = new BudgetMonth();
                List<PlanBudgetModel> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                PlanBudgetModel otherLine = lines.Where(ol => ol.LineItemTypeId == null).FirstOrDefault();
                lines = lines.Where(ol => ol.LineItemTypeId != null).ToList();
                //calculate total line item difference with respective tactics
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiff.CostY1 = l.MonthValues.CostY1 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY1) ?? 0;
                        lineDiff.CostY2 = l.MonthValues.CostY2 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY2) ?? 0;
                        lineDiff.CostY3 = l.MonthValues.CostY3 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY3) ?? 0;
                        lineDiff.CostY4 = l.MonthValues.CostY4 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY4) ?? 0;
                        lineDiff.CostY5 = l.MonthValues.CostY5 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY5) ?? 0;
                        lineDiff.CostY6 = l.MonthValues.CostY6 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY6) ?? 0;
                        lineDiff.CostY7 = l.MonthValues.CostY7 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY7) ?? 0;
                        lineDiff.CostY8 = l.MonthValues.CostY8 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY8) ?? 0;
                        lineDiff.CostY9 = l.MonthValues.CostY9 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY9) ?? 0;
                        lineDiff.CostY10 = l.MonthValues.CostY10 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY10) ?? 0;
                        lineDiff.CostY11 = l.MonthValues.CostY11 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY11) ?? 0;
                        lineDiff.CostY12 = l.MonthValues.CostY12 - lines.Sum(lmon => (double?)lmon.MonthValues.CostY12) ?? 0;

                        NextYearlineDiff.CostY1 = l.MonthValues.CostY1 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY1) ?? 0;
                        NextYearlineDiff.CostY2 = l.MonthValues.CostY2 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY2) ?? 0;
                        NextYearlineDiff.CostY3 = l.MonthValues.CostY3 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY3) ?? 0;
                        NextYearlineDiff.CostY4 = l.MonthValues.CostY4 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY4) ?? 0;
                        NextYearlineDiff.CostY5 = l.MonthValues.CostY5 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY5) ?? 0;
                        NextYearlineDiff.CostY6 = l.MonthValues.CostY6 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY6) ?? 0;
                        NextYearlineDiff.CostY7 = l.MonthValues.CostY7 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY7) ?? 0;
                        NextYearlineDiff.CostY8 = l.MonthValues.CostY8 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY8) ?? 0;
                        NextYearlineDiff.CostY9 = l.MonthValues.CostY9 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY9) ?? 0;
                        NextYearlineDiff.CostY10 = l.MonthValues.CostY10 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY10) ?? 0;
                        NextYearlineDiff.CostY11 = l.MonthValues.CostY11 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY11) ?? 0;
                        NextYearlineDiff.CostY12 = l.MonthValues.CostY12 - lines.Sum(lmon => (double?)lmon.NextYearMonthValues.CostY12) ?? 0;

                        

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthValues = lineDiff;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().NextYearMonthValues = NextYearlineDiff;
                        double allocated = l.TotalAllocatedCost - lines.Sum(l1 => l1.TotalAllocatedCost);
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().TotalAllocatedCost = allocated;                                                
                    }
                    else
                    {
                        PlanBudgetModel Balance = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault();//.TotalActuals = l.TotalActuals;
                        Balance.TotalActuals = l.TotalActuals;
                        Balance.MonthValues = l.MonthValues;
                        Balance.NextYearMonthValues = l.NextYearMonthValues;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().TotalAllocatedCost = l.TotalAllocatedCost < 0 ? 0 : l.TotalAllocatedCost;                        
                    }
                    // Calculate Balance UnAllocated Cost
                    double BalanceUnallocatedCost = l.UnallocatedCost - lines.Sum(lmon => lmon.UnallocatedCost);
                    model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().UnallocatedCost = BalanceUnallocatedCost;
                    
                }
            }
            return model;
        }

        //This function sum up the total of planned and actuals cell of budget to child to parent
        private List<PlanBudgetModel> CalculateBottomUp(List<PlanBudgetModel> model, string ParentActivityType, string ChildActivityType, string ViewBy)
        {
            double totalMonthCostSum = 0;

            foreach (PlanBudgetModel l in model.Where(_mdl => _mdl.ActivityType == ParentActivityType))
            {
                //// check if ViewBy is Campaign selected then set weightage value to 100;
                int weightage = 100;
                if (ViewBy != PlanGanttTypes.Tactic.ToString())
                {
                    weightage = l.Weightage;
                }
                weightage = weightage / 100;

                List<PlanBudgetModel> childs = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).ToList();
                if (l.ActivityType != ActivityType.ActivityTactic || model.Where(m => m.ParentActivityId == l.ActivityId && m.LineItemTypeId != null && m.ActivityType == ActivityType.ActivityLineItem).Any() && childs != null)
                {
                    if (childs != null)
                    {
                        l.MonthValues.ActualY1 = childs.Sum(line => (double?)(line.MonthValues.ActualY1 * weightage)) ?? 0;
                        l.MonthValues.ActualY2 = childs.Sum(line => (double?)(line.MonthValues.ActualY2 * weightage)) ?? 0;
                        l.MonthValues.ActualY3 = childs.Sum(line => (double?)(line.MonthValues.ActualY3 * weightage)) ?? 0;
                        l.MonthValues.ActualY4 = childs.Sum(line => (double?)(line.MonthValues.ActualY4 * weightage)) ?? 0;
                        l.MonthValues.ActualY5 = childs.Sum(line => (double?)(line.MonthValues.ActualY5 * weightage)) ?? 0;
                        l.MonthValues.ActualY6 = childs.Sum(line => (double?)(line.MonthValues.ActualY6 * weightage)) ?? 0;
                        l.MonthValues.ActualY7 = childs.Sum(line => (double?)(line.MonthValues.ActualY7 * weightage)) ?? 0;
                        l.MonthValues.ActualY8 = childs.Sum(line => (double?)(line.MonthValues.ActualY8 * weightage)) ?? 0;
                        l.MonthValues.ActualY9 = childs.Sum(line => (double?)(line.MonthValues.ActualY9 * weightage)) ?? 0;
                        l.MonthValues.ActualY10 = childs.Sum(line => (double?)(line.MonthValues.ActualY10 * weightage)) ?? 0;
                        l.MonthValues.ActualY11 = childs.Sum(line => (double?)(line.MonthValues.ActualY11 * weightage)) ?? 0;
                        l.MonthValues.ActualY12 = childs.Sum(line => (double?)(line.MonthValues.ActualY12 * weightage)) ?? 0;



                        l.NextYearMonthValues.ActualY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY1 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY2 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY3 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY4 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY5 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY6 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY7 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY8 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY9 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY10 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY11 * weightage)) ?? 0;
                        l.NextYearMonthValues.ActualY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.ActualY12 * weightage)) ?? 0;
                }
                else
                {
                        l.MonthValues.ActualY1 = 0;
                        l.MonthValues.ActualY2 = 0;
                        l.MonthValues.ActualY3 = 0;
                        l.MonthValues.ActualY4 = 0;
                        l.MonthValues.ActualY5 = 0;
                        l.MonthValues.ActualY6 = 0;
                        l.MonthValues.ActualY7 = 0;
                        l.MonthValues.ActualY8 = 0;
                        l.MonthValues.ActualY9 = 0;
                        l.MonthValues.ActualY10 = 0;
                        l.MonthValues.ActualY11 = 0;
                        l.MonthValues.ActualY12 = 0;

                        l.NextYearMonthValues.ActualY1 = 0;
                        l.NextYearMonthValues.ActualY2 = 0;
                        l.NextYearMonthValues.ActualY3 = 0;
                        l.NextYearMonthValues.ActualY4 = 0;
                        l.NextYearMonthValues.ActualY5 = 0;
                        l.NextYearMonthValues.ActualY6 = 0;
                        l.NextYearMonthValues.ActualY7 = 0;
                        l.NextYearMonthValues.ActualY8 = 0;
                        l.NextYearMonthValues.ActualY9 = 0;
                        l.NextYearMonthValues.ActualY10 = 0;
                        l.NextYearMonthValues.ActualY11 = 0;
                        l.NextYearMonthValues.ActualY12 = 0;
                    }
                }

                if (string.Compare(ParentActivityType, ActivityType.ActivityTactic, true) < 0 && childs != null)
                {
                    if (childs != null)
                    {
                        l.MonthValues.CostY1 = childs.Sum(line => (double?)(line.MonthValues.CostY1 * weightage)) ?? 0;
                        l.MonthValues.CostY2 = childs.Sum(line => (double?)(line.MonthValues.CostY2 * weightage)) ?? 0;
                        l.MonthValues.CostY3 = childs.Sum(line => (double?)(line.MonthValues.CostY3 * weightage)) ?? 0;
                        l.MonthValues.CostY4 = childs.Sum(line => (double?)(line.MonthValues.CostY4 * weightage)) ?? 0;
                        l.MonthValues.CostY5 = childs.Sum(line => (double?)(line.MonthValues.CostY5 * weightage)) ?? 0;
                        l.MonthValues.CostY6 = childs.Sum(line => (double?)(line.MonthValues.CostY6 * weightage)) ?? 0;
                        l.MonthValues.CostY7 = childs.Sum(line => (double?)(line.MonthValues.CostY7 * weightage)) ?? 0;
                        l.MonthValues.CostY8 = childs.Sum(line => (double?)(line.MonthValues.CostY8 * weightage)) ?? 0;
                        l.MonthValues.CostY9 = childs.Sum(line => (double?)(line.MonthValues.CostY9 * weightage)) ?? 0;
                        l.MonthValues.CostY10 = childs.Sum(line => (double?)(line.MonthValues.CostY10 * weightage)) ?? 0;
                        l.MonthValues.CostY11 = childs.Sum(line => (double?)(line.MonthValues.CostY11 * weightage)) ?? 0;
                        l.MonthValues.CostY12 = childs.Sum(line => (double?)(line.MonthValues.CostY12 * weightage)) ?? 0;


                        l.NextYearMonthValues.CostY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY1 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY2 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY3 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY4 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY5 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY6 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY7 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY8 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY9 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY10 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY11 * weightage)) ?? 0;
                        l.NextYearMonthValues.CostY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.CostY12 * weightage)) ?? 0;

                        totalMonthCostSum = l.MonthValues.CostY1 + l.MonthValues.CostY2 + l.MonthValues.CostY3 + l.MonthValues.CostY4 + l.MonthValues.CostY5 + l.MonthValues.CostY6 + l.MonthValues.CostY7 + l.MonthValues.CostY8 + l.MonthValues.CostY9 + l.MonthValues.CostY10 + l.MonthValues.CostY11 + l.MonthValues.CostY12
                            + l.NextYearMonthValues.CostY1 + l.NextYearMonthValues.CostY2 + l.NextYearMonthValues.CostY3 + l.NextYearMonthValues.CostY4 + l.NextYearMonthValues.CostY5 + l.NextYearMonthValues.CostY6 + l.NextYearMonthValues.CostY7 + l.NextYearMonthValues.CostY8 + l.NextYearMonthValues.CostY9 + l.NextYearMonthValues.CostY10 + l.NextYearMonthValues.CostY11 + l.NextYearMonthValues.CostY12;
                }
                else
                {
                        l.MonthValues.CostY1 = 0;
                        l.MonthValues.CostY2 = 0;
                        l.MonthValues.CostY3 = 0;
                        l.MonthValues.CostY4 = 0;
                        l.MonthValues.CostY5 = 0;
                        l.MonthValues.CostY6 = 0;
                        l.MonthValues.CostY7 = 0;
                        l.MonthValues.CostY8 = 0;
                        l.MonthValues.CostY9 = 0;
                        l.MonthValues.CostY10 = 0;
                        l.MonthValues.CostY11 = 0;
                        l.MonthValues.CostY12 = 0;

                        l.NextYearMonthValues.CostY1 = 0;
                        l.NextYearMonthValues.CostY2 = 0;
                        l.NextYearMonthValues.CostY3 = 0;
                        l.NextYearMonthValues.CostY4 = 0;
                        l.NextYearMonthValues.CostY5 = 0;
                        l.NextYearMonthValues.CostY6 = 0;
                        l.NextYearMonthValues.CostY7 = 0;
                        l.NextYearMonthValues.CostY8 = 0;
                        l.NextYearMonthValues.CostY9 = 0;
                        l.NextYearMonthValues.CostY10 = 0;
                        l.NextYearMonthValues.CostY11 = 0;
                        l.NextYearMonthValues.CostY12 = 0;
                    }
                }

                if (childs != null)
                {
                    l.ChildMonthValues.BudgetY1 = childs.Sum(line => (double?)(line.MonthValues.BudgetY1)) ?? 0;
                    l.ChildMonthValues.BudgetY2 = childs.Sum(line => (double?)(line.MonthValues.BudgetY2)) ?? 0;
                    l.ChildMonthValues.BudgetY3 = childs.Sum(line => (double?)(line.MonthValues.BudgetY3)) ?? 0;
                    l.ChildMonthValues.BudgetY4 = childs.Sum(line => (double?)(line.MonthValues.BudgetY4)) ?? 0;
                    l.ChildMonthValues.BudgetY5 = childs.Sum(line => (double?)(line.MonthValues.BudgetY5)) ?? 0;
                    l.ChildMonthValues.BudgetY6 = childs.Sum(line => (double?)(line.MonthValues.BudgetY6)) ?? 0;
                    l.ChildMonthValues.BudgetY7 = childs.Sum(line => (double?)(line.MonthValues.BudgetY7)) ?? 0;
                    l.ChildMonthValues.BudgetY8 = childs.Sum(line => (double?)(line.MonthValues.BudgetY8)) ?? 0;
                    l.ChildMonthValues.BudgetY9 = childs.Sum(line => (double?)(line.MonthValues.BudgetY9)) ?? 0;
                    l.ChildMonthValues.BudgetY10 = childs.Sum(line => (double?)(line.MonthValues.BudgetY10)) ?? 0;
                    l.ChildMonthValues.BudgetY11 = childs.Sum(line => (double?)(line.MonthValues.BudgetY11)) ?? 0;
                    l.ChildMonthValues.BudgetY12 = childs.Sum(line => (double?)(line.MonthValues.BudgetY12)) ?? 0;

                    l.ChildNextYearMonthValues.BudgetY1 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY1)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY2 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY2)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY3 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY3)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY4 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY4)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY5 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY5)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY6 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY6)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY7 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY7)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY8 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY8)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY9 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY9)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY10 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY10)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY11 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY11)) ?? 0;
                    l.ChildNextYearMonthValues.BudgetY12 = childs.Sum(line => (double?)(line.NextYearMonthValues.BudgetY12)) ?? 0;
                }
                else
                {
                    l.ChildMonthValues.BudgetY1 = 0;
                    l.ChildMonthValues.BudgetY2 = 0;
                    l.ChildMonthValues.BudgetY3 = 0;
                    l.ChildMonthValues.BudgetY4 = 0;
                    l.ChildMonthValues.BudgetY5 = 0;
                    l.ChildMonthValues.BudgetY6 = 0;
                    l.ChildMonthValues.BudgetY7 = 0;
                    l.ChildMonthValues.BudgetY8 = 0;
                    l.ChildMonthValues.BudgetY9 = 0;
                    l.ChildMonthValues.BudgetY10 = 0;
                    l.ChildMonthValues.BudgetY11 = 0;
                    l.ChildMonthValues.BudgetY12 = 0;

                    l.ChildNextYearMonthValues.BudgetY1 = 0;
                    l.ChildNextYearMonthValues.BudgetY2 = 0;
                    l.ChildNextYearMonthValues.BudgetY3 = 0;
                    l.ChildNextYearMonthValues.BudgetY4 = 0;
                    l.ChildNextYearMonthValues.BudgetY5 = 0;
                    l.ChildNextYearMonthValues.BudgetY6 = 0;
                    l.ChildNextYearMonthValues.BudgetY7 = 0;
                    l.ChildNextYearMonthValues.BudgetY8 = 0;
                    l.ChildNextYearMonthValues.BudgetY9 = 0;
                    l.ChildNextYearMonthValues.BudgetY10 = 0;
                    l.ChildNextYearMonthValues.BudgetY11 = 0;
                    l.ChildNextYearMonthValues.BudgetY12 = 0;
                }

                if (l.ActivityType != ActivityType.ActivityTactic)
                {
                    l.TotalAllocatedCost = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    l.UnallocatedCost = l.TotalAllocatedCost - totalMonthCostSum;
                }
                if (l.ActivityType != ActivityType.ActivityTactic || model.Where(m => m.ParentActivityId == l.ActivityId && m.LineItemTypeId != null && m.ActivityType == ActivityType.ActivityLineItem).Any())
                {
                    l.TotalActuals = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalActuals) ?? 0;
                }
            }

            return model;
        }

        //This function apply weightage to budget cell values
        private List<PlanBudgetModel> SetLineItemCostByWeightage(List<PlanBudgetModel> model, string ViewBy)
        {
            //int _ViewById = ViewBy != null ? ViewBy : 0;
            int weightage = 100;
            foreach (PlanBudgetModel l in model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityTactic))
            {
                List<PlanBudgetModel> lstLineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();

                //// check if ViewBy is Campaign selected then set weightage value to 100;
                if (ViewBy != PlanGanttTypes.Tactic.ToString())
                {
                    weightage = l.Weightage;
                }
                BudgetMonth lineBudget;
                BudgetMonth lineNextYearBudget;
                foreach (PlanBudgetModel line in lstLineItems)
                {
                    lineBudget = new BudgetMonth();
                    lineNextYearBudget = new BudgetMonth();
                    lineBudget.ActualY1 = (double)(line.MonthValues.ActualY1 * weightage) / 100;
                    lineBudget.ActualY2 = (double)(line.MonthValues.ActualY2 * weightage) / 100;
                    lineBudget.ActualY3 = (double)(line.MonthValues.ActualY3 * weightage) / 100;
                    lineBudget.ActualY4 = (double)(line.MonthValues.ActualY4 * weightage) / 100;
                    lineBudget.ActualY5 = (double)(line.MonthValues.ActualY5 * weightage) / 100;
                    lineBudget.ActualY6 = (double)(line.MonthValues.ActualY6 * weightage) / 100;
                    lineBudget.ActualY7 = (double)(line.MonthValues.ActualY7 * weightage) / 100;
                    lineBudget.ActualY8 = (double)(line.MonthValues.ActualY8 * weightage) / 100;
                    lineBudget.ActualY9 = (double)(line.MonthValues.ActualY9 * weightage) / 100;
                    lineBudget.ActualY10 = (double)(line.MonthValues.ActualY10 * weightage) / 100;
                    lineBudget.ActualY11 = (double)(line.MonthValues.ActualY11 * weightage) / 100;
                    lineBudget.ActualY12 = (double)(line.MonthValues.ActualY12 * weightage) / 100;

                    lineNextYearBudget.ActualY1 = (double)(line.NextYearMonthValues.ActualY1 * weightage) / 100;
                    lineNextYearBudget.ActualY2 = (double)(line.NextYearMonthValues.ActualY2 * weightage) / 100;
                    lineNextYearBudget.ActualY3 = (double)(line.NextYearMonthValues.ActualY3 * weightage) / 100;
                    lineNextYearBudget.ActualY4 = (double)(line.NextYearMonthValues.ActualY4 * weightage) / 100;
                    lineNextYearBudget.ActualY5 = (double)(line.NextYearMonthValues.ActualY5 * weightage) / 100;
                    lineNextYearBudget.ActualY6 = (double)(line.NextYearMonthValues.ActualY6 * weightage) / 100;
                    lineNextYearBudget.ActualY7 = (double)(line.NextYearMonthValues.ActualY7 * weightage) / 100;
                    lineNextYearBudget.ActualY8 = (double)(line.NextYearMonthValues.ActualY8 * weightage) / 100;
                    lineNextYearBudget.ActualY9 = (double)(line.NextYearMonthValues.ActualY9 * weightage) / 100;
                    lineNextYearBudget.ActualY10 = (double)(line.NextYearMonthValues.ActualY10 * weightage) / 100;
                    lineNextYearBudget.ActualY11 = (double)(line.NextYearMonthValues.ActualY11 * weightage) / 100;
                    lineNextYearBudget.ActualY12 = (double)(line.NextYearMonthValues.ActualY12 * weightage) / 100;

                    lineBudget.CostY1 = (double)(line.MonthValues.CostY1 * weightage) / 100;
                    lineBudget.CostY2 = (double)(line.MonthValues.CostY2 * weightage) / 100;
                    lineBudget.CostY3 = (double)(line.MonthValues.CostY3 * weightage) / 100;
                    lineBudget.CostY4 = (double)(line.MonthValues.CostY4 * weightage) / 100;
                    lineBudget.CostY5 = (double)(line.MonthValues.CostY5 * weightage) / 100;
                    lineBudget.CostY6 = (double)(line.MonthValues.CostY6 * weightage) / 100;
                    lineBudget.CostY7 = (double)(line.MonthValues.CostY7 * weightage) / 100;
                    lineBudget.CostY8 = (double)(line.MonthValues.CostY8 * weightage) / 100;
                    lineBudget.CostY9 = (double)(line.MonthValues.CostY9 * weightage) / 100;
                    lineBudget.CostY10 = (double)(line.MonthValues.CostY10 * weightage) / 100;
                    lineBudget.CostY11 = (double)(line.MonthValues.CostY11 * weightage) / 100;
                    lineBudget.CostY12 = (double)(line.MonthValues.CostY12 * weightage) / 100;

                    lineNextYearBudget.CostY1 = (double)(line.NextYearMonthValues.CostY1 * weightage) / 100;
                    lineNextYearBudget.CostY2 = (double)(line.NextYearMonthValues.CostY2 * weightage) / 100;
                    lineNextYearBudget.CostY3 = (double)(line.NextYearMonthValues.CostY3 * weightage) / 100;
                    lineNextYearBudget.CostY4 = (double)(line.NextYearMonthValues.CostY4 * weightage) / 100;
                    lineNextYearBudget.CostY5 = (double)(line.NextYearMonthValues.CostY5 * weightage) / 100;
                    lineNextYearBudget.CostY6 = (double)(line.NextYearMonthValues.CostY6 * weightage) / 100;
                    lineNextYearBudget.CostY7 = (double)(line.NextYearMonthValues.CostY7 * weightage) / 100;
                    lineNextYearBudget.CostY8 = (double)(line.NextYearMonthValues.CostY8 * weightage) / 100;
                    lineNextYearBudget.CostY9 = (double)(line.NextYearMonthValues.CostY9 * weightage) / 100;
                    lineNextYearBudget.CostY10 = (double)(line.NextYearMonthValues.CostY10 * weightage) / 100;
                    lineNextYearBudget.CostY11 = (double)(line.NextYearMonthValues.CostY11 * weightage) / 100;
                    lineNextYearBudget.CostY12 = (double)(line.NextYearMonthValues.CostY12 * weightage) / 100;

                    lineBudget.BudgetY1 = (double)(line.MonthValues.BudgetY1 * weightage) / 100;
                    lineBudget.BudgetY2 = (double)(line.MonthValues.BudgetY2 * weightage) / 100;
                    lineBudget.BudgetY3 = (double)(line.MonthValues.BudgetY3 * weightage) / 100;
                    lineBudget.BudgetY4 = (double)(line.MonthValues.BudgetY4 * weightage) / 100;
                    lineBudget.BudgetY5 = (double)(line.MonthValues.BudgetY5 * weightage) / 100;
                    lineBudget.BudgetY6 = (double)(line.MonthValues.BudgetY6 * weightage) / 100;
                    lineBudget.BudgetY7 = (double)(line.MonthValues.BudgetY7 * weightage) / 100;
                    lineBudget.BudgetY8 = (double)(line.MonthValues.BudgetY8 * weightage) / 100;
                    lineBudget.BudgetY9 = (double)(line.MonthValues.BudgetY9 * weightage) / 100;
                    lineBudget.BudgetY10 = (double)(line.MonthValues.BudgetY10 * weightage) / 100;
                    lineBudget.BudgetY11 = (double)(line.MonthValues.BudgetY11 * weightage) / 100;
                    lineBudget.BudgetY12 = (double)(line.MonthValues.BudgetY12 * weightage) / 100;

                    lineNextYearBudget.BudgetY1 = (double)(line.NextYearMonthValues.BudgetY1 * weightage) / 100;
                    lineNextYearBudget.BudgetY2 = (double)(line.NextYearMonthValues.BudgetY2 * weightage) / 100;
                    lineNextYearBudget.BudgetY3 = (double)(line.NextYearMonthValues.BudgetY3 * weightage) / 100;
                    lineNextYearBudget.BudgetY4 = (double)(line.NextYearMonthValues.BudgetY4 * weightage) / 100;
                    lineNextYearBudget.BudgetY5 = (double)(line.NextYearMonthValues.BudgetY5 * weightage) / 100;
                    lineNextYearBudget.BudgetY6 = (double)(line.NextYearMonthValues.BudgetY6 * weightage) / 100;
                    lineNextYearBudget.BudgetY7 = (double)(line.NextYearMonthValues.BudgetY7 * weightage) / 100;
                    lineNextYearBudget.BudgetY8 = (double)(line.NextYearMonthValues.BudgetY8 * weightage) / 100;
                    lineNextYearBudget.BudgetY9 = (double)(line.NextYearMonthValues.BudgetY9 * weightage) / 100;
                    lineNextYearBudget.BudgetY10 = (double)(line.NextYearMonthValues.BudgetY10 * weightage) / 100;
                    lineNextYearBudget.BudgetY11 = (double)(line.NextYearMonthValues.BudgetY11 * weightage) / 100;
                    lineNextYearBudget.BudgetY12 = (double)(line.NextYearMonthValues.BudgetY12 * weightage) / 100;

                    line.MonthValues = lineBudget;
                    line.NextYearMonthValues = lineNextYearBudget;
                }
            }
            return model;
        }        
       
        /// <summary>
        /// Added by Mitesh Vaishnav for PL ticket 2585
        /// </summary>
        /// <param name="BudgetModel"></param>
        /// <param name="Year"></param>
        /// <returns></returns>
        private List<PlanBudgetModel> FilterPlanByTimeFrame(List<PlanBudgetModel> BudgetModel,string Year)
        {
            foreach(PlanBudgetModel objPlan in BudgetModel.Where(p => p.ActivityType == ActivityType.ActivityPlan).ToList())
            {
                if (!BudgetModel.Where(ent=>ent.ParentActivityId==objPlan.ActivityId).Any())
                {
                    int planId=Convert.ToInt32(objPlan.Id);//
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
                        if (objPlan.PlanYear!=firstYear && objPlan.PlanYear!=lastYear)
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
