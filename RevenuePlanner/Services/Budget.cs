using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public const string FixHeader = "ActivityId,Type,machinename,,,Activity,,Total Budget" + manageviewicon + ",Planned Cost" + manageviewicon + ",Total Actual" + manageviewicon;
        public const string EndColumnsHeader = ",Unallocated Budget";
        public const string FixColumnIds = "ActivityId,Type,MachineName,colourcode,LineItemTypeId,TaskName,Buttons,BudgetCost,PlannedCost,ActualCost";
        public const string EndColumnIds = ",Budget";
        public const string FixColType = "ro,ro,ro,ro,ro,tree,ro,ed,ed,ed";
        public const string EndColType = ",ro";
        public const string FixcolWidth = "100,100,100,10,100,302,75,100,110,100";
        public const string EndcolWidth = ",150";
        public const string FixColsorting = "na,na,na,na,na,na,na,na,na,na";
        public const string EndColsorting = ",na";
        public const string QuarterPrefix = "Q";
        public const string DhtmlxColSpan = "#cspan";
        public const string ColBudget = "Budget";
        public const string ColActual = "Actual";
        public const string ColPlanned = "Planned";
        public const string NotEditableCellStyle = "color:#a2a2a2 !important;";
        public const string ThreeDash = "---";
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
                isMultiYear = IsMultiyearTimeframe(year);
            }

            DataTable dt = objSp.GetBudget(PlanIds, UserID, viewBy, OwnerIds, TacticTypeids, StatusIds, year); //Get budget data for budget,planned cost and actual using store proc. GetplanBudget

            List<PlanBudgetModel> model = CreateBudgetDataModel(dt, PlanExchangeRate); //Convert datatable with budget data to PlanBudgetModel model
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
                    //AssetType = Convert.ToString(row["ROITacticType"]), //Uncomment this when data is passed from sp.
                    //AnchorTacticID = Convert.ToInt32(Convert.ToString(row["IsAnchorTacticId"])),
                    MonthValues = new BudgetMonth()
                    {
                        //Budget Month Allocation
                        Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y2"])), PlanExchangeRate),
                        Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y1"])), PlanExchangeRate),
                        Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y3"])), PlanExchangeRate),
                        Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y4"])), PlanExchangeRate),
                        May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y5"])), PlanExchangeRate),
                        Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y6"])), PlanExchangeRate),
                        Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y7"])), PlanExchangeRate),
                        Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y8"])), PlanExchangeRate),
                        Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y9"])), PlanExchangeRate),
                        Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y10"])), PlanExchangeRate),
                        Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y11"])), PlanExchangeRate),
                        Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y12"])), PlanExchangeRate),

                        //Cost Month Allocation
                        CFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY2"])), PlanExchangeRate),
                        CJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY1"])), PlanExchangeRate),
                        CMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY3"])), PlanExchangeRate),
                        CApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY4"])), PlanExchangeRate),
                        CMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY5"])), PlanExchangeRate),
                        CJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY6"])), PlanExchangeRate),
                        CJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY7"])), PlanExchangeRate),
                        CAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY8"])), PlanExchangeRate),
                        CSep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY9"])), PlanExchangeRate),
                        COct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY10"])), PlanExchangeRate),
                        CNov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY11"])), PlanExchangeRate),
                        CDec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY12"])), PlanExchangeRate),

                        //Actuals Month Allocation
                        AFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY2"])), PlanExchangeRate),
                        AJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY1"])), PlanExchangeRate),
                        AMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY3"])), PlanExchangeRate),
                        AApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY4"])), PlanExchangeRate),
                        AMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY5"])), PlanExchangeRate),
                        AJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY6"])), PlanExchangeRate),
                        AJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY7"])), PlanExchangeRate),
                        AAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY8"])), PlanExchangeRate),
                        ASep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY9"])), PlanExchangeRate),
                        AOct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY10"])), PlanExchangeRate),
                        ANov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY11"])), PlanExchangeRate),
                        ADec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY12"])), PlanExchangeRate)
                    },
                    NextYearMonthValues = new BudgetMonth()
                    {
                        //Budget Month Allocation
                        Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y14"])), PlanExchangeRate),
                        Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y13"])), PlanExchangeRate),
                        Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y15"])), PlanExchangeRate),
                        Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y16"])), PlanExchangeRate),
                        May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y17"])), PlanExchangeRate),
                        Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y18"])), PlanExchangeRate),
                        Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y19"])), PlanExchangeRate),
                        Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y20"])), PlanExchangeRate),
                        Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y21"])), PlanExchangeRate),
                        Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y22"])), PlanExchangeRate),
                        Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y23"])), PlanExchangeRate),
                        Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["Y24"])), PlanExchangeRate),

                        //Cost Month Allocation
                        CFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY14"])), PlanExchangeRate),
                        CJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY13"])), PlanExchangeRate),
                        CMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY15"])), PlanExchangeRate),
                        CApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY16"])), PlanExchangeRate),
                        CMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY17"])), PlanExchangeRate),
                        CJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY18"])), PlanExchangeRate),
                        CJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY19"])), PlanExchangeRate),
                        CAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY20"])), PlanExchangeRate),
                        CSep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY21"])), PlanExchangeRate),
                        COct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY22"])), PlanExchangeRate),
                        CNov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY23"])), PlanExchangeRate),
                        CDec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CostY24"])), PlanExchangeRate),

                        //Actuals Month Allocation
                        AFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY14"])), PlanExchangeRate),
                        AJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY13"])), PlanExchangeRate),
                        AMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY15"])), PlanExchangeRate),
                        AApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY16"])), PlanExchangeRate),
                        AMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY17"])), PlanExchangeRate),
                        AJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY18"])), PlanExchangeRate),
                        AJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY19"])), PlanExchangeRate),
                        AAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY20"])), PlanExchangeRate),
                        ASep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY21"])), PlanExchangeRate),
                        AOct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY22"])), PlanExchangeRate),
                        ANov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY23"])), PlanExchangeRate),
                        ADec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY24"])), PlanExchangeRate)
                    }
                }).ToList();
            }
            return model;
        }

        public List<Budgetdataobj> SetBudgetDhtmlxFormattedValues(List<PlanBudgetModel> model, PlanBudgetModel Entity, string OwnerName, string EntityType, string AllocatedBy, bool IsNextYear, bool IsMultiyearPlan, string DhtmlxGridRowId, bool IsAddEntityRights, bool isViewBy = false, string pcptid = "", string TacticType = "")  // pcptid = Plan-Campaign-Program-Tactic-Id
        {
            List<Budgetdataobj> BudgetDataObjList = new List<Budgetdataobj>();
            Budgetdataobj BudgetDataObj = new Budgetdataobj();
            string Roistring = string.Empty;
            var PackageTacticIds = string.Empty;
            BudgetDataObj.value = Entity.Id;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Entity.ActivityType;
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            BudgetDataObj.value = Entity.MachineName;
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

            if (Entity.AnchorTacticID != null && Entity.AnchorTacticID > 0 && !string.IsNullOrEmpty(Entity.Id))
            {
                if (Convert.ToString(Entity.AnchorTacticID) == Entity.Id) // If Anchor tacticid and Entity id both same then set ROI package icon
                {
                    // Get list of package tactic ids
                    Roistring = "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id=pkgIcon onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids=" + PackageTacticIds + "><i class='fa fa-object-group'></i></div>";
                    BudgetDataObj.value = Roistring + HttpUtility.HtmlEncode(Entity.ActivityName).Replace("'", "&#39;");
                }
            }
            else
            {
                BudgetDataObj.value = HttpUtility.HtmlEncode(Entity.ActivityName).Replace("'", "&#39;");
            }
            if (Entity.ActivityType == ActivityType.ActivityLineItem && Entity.LineItemTypeId == null)
            {
                BudgetDataObj.locked = CellLocked;
                BudgetDataObj.style = NotEditableCellStyle;
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
                iconsData.value = ThreeDash;
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
            BudgetDataObj.value = Convert.ToString(Entity.UnallocatedBudget);
            BudgetDataObjList.Add(BudgetDataObj);

            return BudgetDataObjList;
        }

        //html part of this function will be move into html helper as part of PL ticket 2676
        public string SetIcons(PlanBudgetModel Entity, string OwnerName, string EntityType, string DhtmlxGridRowId, bool IsAddEntityRights, string pcptid, string TacticType)
        {
            string doubledesh = "--";
            string IconsData = string.Empty;
            //Set icon of magnifying glass and honey comb for plan entity with respective ids
            if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityPlan.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
                IconsData = "<div class=grid_Search id=Plan title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Plan alt=" + Entity.ActivityId + " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " >";
                    IconsData += "<i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Plan dhtmlxrowid=" + Convert.ToString(DhtmlxGridRowId) + " TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName=" + Convert.ToString(Entity.ActivityName) + " altId=" + Entity.ActivityId;
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ColorCode=" + Convert.ToString(Entity.ColorCode) + " taskId=" + Convert.ToString(Entity.ActivityId);
                IconsData += " csvId=Plan_" + Convert.ToString(Entity.ActivityId) + " ></div>";
            }
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityCampaign.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
                IconsData = "<div class=grid_Search id=CP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Campaign alt=" + Entity.ParentActivityId + "_" + Entity.ActivityId;
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Campaign onclick=javascript:AddRemoveEntity(this) title=Select  dhtmlxrowid=" + Convert.ToString(DhtmlxGridRowId) + " TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName=" + (Convert.ToString(Entity.ActivityName));
                IconsData += " altId=" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId) + " per=" + Convert.ToString(IsAddEntityRights).ToLower();
                IconsData += " ColorCode=" + Convert.ToString(Entity.ColorCode) + " taskId= " + Convert.ToString(Entity.ActivityId) + " csvId=Campaign_" + Entity.ActivityId + "></div>";
            }
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityProgram.ToLower())
            {
                // Magnifying Glass to open Inspect Popup
                IconsData = "<div class=grid_Search id=PP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Program alt=_" + Entity.ParentActivityId + "_" + Entity.ActivityId;
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt id=Program onclick=javascript:AddRemoveEntity(this) title=Select  dhtmlxrowid=" + Convert.ToString(DhtmlxGridRowId) + " TacticType= " + doubledesh;
                IconsData += " OwnerName= '" + Convert.ToString(OwnerName) + "' TaskName=" + (Convert.ToString(Entity.ActivityName));
                IconsData += " altId=_" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId) + " ColorCode=" + Convert.ToString(Entity.ColorCode);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " taskId=" + Convert.ToString(Entity.ActivityId) + " csvId=Program_" + Convert.ToString(Entity.ActivityId) + " ></div>";
            }
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityTactic.ToLower())
            {
                //LinkTactic Permission based on Entity Year
                bool LinkTacticPermission = ((Entity.EndDate.Year - Entity.StartDate.Year) > 0) ? true : false;
                string LinkedTacticId = Entity.LinkTacticId == 0 ? "null" : Entity.LinkTacticId.ToString();

                // Magnifying Glass to open Inspect Popup
                IconsData = "<div class=grid_Search id=TP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) title=Add  id=Tactic alt=__" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId);
                    IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " LinkTacticper =" + Convert.ToString(LinkTacticPermission) + " LinkedTacticId = " + Convert.ToString(LinkedTacticId);
                    IconsData += " tacticaddId=" + Convert.ToString(Entity.ActivityId) + "><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
                }

                // HoneyComb Button
                IconsData += " <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) title=Select  id=Tactic pcptid = " + Convert.ToString(pcptid) + " dhtmlxrowid=" + Convert.ToString(DhtmlxGridRowId);
                IconsData += " TacticType= '" + Convert.ToString(TacticType) + "' OwnerName= '" + Convert.ToString(OwnerName) + "' roitactictype='" + Entity.AssetType + "' anchortacticid='" + Entity.AnchorTacticID + "'  ";
                IconsData += " TaskName=" + (Convert.ToString(Entity.ActivityName));
                IconsData += " altId=__" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId) + " ColorCode=" + Convert.ToString(Entity.ColorCode);
                IconsData += " per=" + Convert.ToString(IsAddEntityRights).ToLower() + " taskId=" + Convert.ToString(Entity.ActivityId) + " csvId=Tactic_" + Convert.ToString(Entity.ActivityId) + " ></div>";
            }
            else if (Convert.ToString(EntityType).ToLower() == ActivityType.ActivityLineItem.ToLower() && Entity.LineItemTypeId != null)
            {
                // Magnifying Glass to open Inspect Popup
                IconsData = "<div class=grid_Search id=LP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>";

                // Add Button
                if (IsAddEntityRights)
                {
                    IconsData += "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  title=Add  id=Line alt=___" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId);
                    IconsData += " lt=" + ((Entity.LineItemTypeId == null) ? 0 : Entity.LineItemTypeId) + " per=" + Convert.ToString(IsAddEntityRights).ToLower();
                    IconsData += " dt=" + Convert.ToString(Entity.ActivityName) + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>";
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
                    gridjsonlistViewby.Add(GenerateHierarchy(model, UserID, ClientId, Year, AllocatedBy, isViewby, bmViewby.ActivityId));
                    gridViewbyData.rows = gridjsonlistViewby;
                    gridjsonlist.Add(gridViewbyData);
                }
            }
            else
            {
                gridjsonlist.Add(GenerateHierarchy(model, UserID, ClientId, Year, AllocatedBy, false));
            }

            //Set plan entity in the dhtmlx formated model at top level of the hierarchy using loop

            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            objBudgetDHTMLXGrid.Grid.rows = gridjsonlist;
            return objBudgetDHTMLXGrid;
        }
        private BudgetDHTMLXGridDataModel GenerateHierarchy(List<PlanBudgetModel> model, int UserID, int ClientId, string Year, string AllocatedBy, bool isViewBy, string ParentId = "")
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

            Dictionary<int, string> lstUserDetails = new Dictionary<int, string>();
            lstUserDetails = objBDSServiceClient.GetUserListByClientIdEx(ClientId).ToDictionary(x => x.ID, x => x.FirstName + " " + x.LastName);

            Dictionary<int, string> lstTacticTypeTitle = new Dictionary<int, string>();
            List<int> TacticTypeIds = model.Where(t => t.ActivityType == ActivityType.ActivityTactic).Select(t => t.TacticTypeId).ToList();
            lstTacticTypeTitle = objDbMrpEntities.TacticTypes.Where(tt => TacticTypeIds.Contains(tt.TacticTypeId) && tt.IsDeleted == false).ToDictionary(tt => tt.TacticTypeId, tt => tt.Title);
            foreach (PlanBudgetModel bm in model.Where(p => p.ActivityType == ActivityType.ActivityPlan && (!isViewBy || p.ParentActivityId == ParentId)).OrderBy(p => p.ActivityName))
            {

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
                string firstYear = GetInitialYearFromTimeFrame(Year);
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
                if (lstUserDetails != null && lstUserDetails.Count > 0)
                {
                    if (lstUserDetails.ContainsKey(bm.CreatedBy))
                    {
                        OwnerName = Convert.ToString(lstUserDetails[bm.CreatedBy]);
                    }
                }

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

                    if (lstUserDetails != null && lstUserDetails.Count > 0)
                    {
                        if (lstUserDetails.ContainsKey(bm.CreatedBy))
                        {
                            OwnerName = Convert.ToString(lstUserDetails[bm.CreatedBy]);
                        }
                    }
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

                        if (lstUserDetails != null && lstUserDetails.Count > 0)
                        {
                            if (lstUserDetails.ContainsKey(bm.CreatedBy))
                            {
                                OwnerName = Convert.ToString(lstUserDetails[bm.CreatedBy]);
                            }
                        }
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

                            if (lstUserDetails != null && lstUserDetails.Count > 0)
                            {
                                if (lstUserDetails.ContainsKey(bm.CreatedBy))
                                {
                                    OwnerName = Convert.ToString(lstUserDetails[bm.CreatedBy]);
                                }
                            }

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

                                if (lstUserDetails != null && lstUserDetails.Count > 0)
                                {
                                    if (lstUserDetails.ContainsKey(bm.CreatedBy))
                                    {
                                        OwnerName = Convert.ToString(lstUserDetails[bm.CreatedBy]);
                                    }
                                }
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
            }
            return gridjsonlistPlanObj;
        }
        private BudgetDHTMLXGridModel GenerateHeaderString(string AllocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> model, string Year)
        {
            string firstYear = GetInitialYearFromTimeFrame(Year);
            string lastYear = string.Empty;
            //check if multiyear flag is on then last year will be firstyear+1
            if (isMultiYear)
            {
                lastYear = Convert.ToString(Convert.ToInt32(firstYear) + 1);
            }

            string setHeader = string.Empty, colType = string.Empty, width = string.Empty, colSorting = string.Empty, columnIds = string.Empty;
            string manageviewicon = "<a href=javascript:void(0) onclick=OpenCreateNew(false) class=manageviewicon  title='Open Column Management'><i class='fa fa-edit'></i></a>";
            List<string> attachHeader = new List<string>();

            setHeader = FixHeader;
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
                    setHeader = setHeader + ",Q" + quarterCounter.ToString() + "-" + headerYear + " Budget " + manageviewicon
                                        + ",Q" + quarterCounter.ToString() + "-" + headerYear + " Planned " + manageviewicon
                                     + ",Q" + quarterCounter.ToString() + "-" + headerYear + " Actual " + manageviewicon;


                    columnIds = columnIds + "," + "Budget,Planned,Actual";
                    colType = colType + ",ed,ed,ed";
                    width = width + ",130,130,130";
                    colSorting = colSorting + ",str,str,str";

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
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);

                    setHeader = setHeader + "," + dt.ToString("MMM").ToUpper() + "-" + headerYear + " Budget " + manageviewicon
                                        + "," + dt.ToString("MMM").ToUpper() + "-" + headerYear + " Planned " + manageviewicon
                                        + "," + dt.ToString("MMM").ToUpper() + "-" + headerYear + " Actual " + manageviewicon;
                    columnIds = columnIds + "," + "Budget,Planned,Actual";

                    colType = colType + ",ed,ed,ed";
                    width = width + ",130,130,130";
                    colSorting = colSorting + ",str,str,str";
                }
            }

            setHeader = setHeader + ",Unallocated Planned Cost";
            columnIds = columnIds + "," + "UnAllocatedCost";
            colType = colType + ",ro";
            width = width + ",150";
            colSorting = colSorting + ",str";



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
                    }

                    objTotalActual.value = Convert.ToString(Entity.TotalActuals);//Set values for Total actual
                    objTotalActual.locked = CellLocked;
                    objTotalActual.style = NotEditableCellStyle;

                    bool isOtherLineItem = activityType == ActivityType.ActivityLineItem && Entity.LineItemTypeId == null;
                    objTotalCost.value = Convert.ToString(Entity.TotalAllocatedCost);
                    objTotalCost.locked = Entity.isCostEditable && !isOtherLineItem ? CellNotLocked : CellLocked;
                    objTotalCost.style = Entity.isCostEditable && !isOtherLineItem ? string.Empty : NotEditableCellStyle;
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
            if (string.Compare(allocatedBy, "quarters", true) != 0)
            {
                if (!isNextYearPlan)
                {
                    BudgetDataObjList = CampignMonthlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, isViewBy);
                }
            }
            else
            {
                if (!isNextYearPlan)
                {
                    BudgetDataObjList = CampignQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, IsMulityearPlan, isViewBy);
                }
                else
                {
                    BudgetDataObjList = CampignNextYearQuarterlyAllocation(Entity, isTactic, isLineItem, BudgetDataObjList, isViewBy);
                }
            }
            return BudgetDataObjList;
        }

        private List<Budgetdataobj> CampignMonthlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool isViewby = false)
        {
            for (int i = 1; i <= 12; i++)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                if (!isViewby)
                {
                    objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;
                    if (i == 1)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jan) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJan);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJan);
                    }
                    else if (i == 2)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Feb) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CFeb);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AFeb);
                    }
                    else if (i == 3)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Mar) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CMar);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AMar);
                    }
                    else if (i == 4)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Apr) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CApr);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AApr);
                    }
                    else if (i == 5)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.May) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CMay);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AMay);
                    }
                    else if (i == 6)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jun) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJun);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJun);
                    }
                    else if (i == 7)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jul) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJul);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJul);
                    }
                    else if (i == 8)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Aug) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CAug);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AAug);
                    }
                    else if (i == 9)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Sep) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CSep);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ASep);
                    }
                    else if (i == 10)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Oct) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.COct);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AOct);
                    }
                    else if (i == 11)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Nov) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CNov);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ANov);
                    }
                    else if (i == 12)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Dec) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CDec);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.ADec);
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

        private List<Budgetdataobj> CampignQuarterlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool IsMultiYearPlan, bool isViewby = false)
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
                objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                objActualMonth.style = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;
                if (!isViewby)
                {
                    if (i == 1)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jan + Entity.MonthValues.Feb + Entity.MonthValues.Mar) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJan + Entity.MonthValues.CFeb + Entity.MonthValues.CMar);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJan + Entity.MonthValues.AFeb + Entity.MonthValues.AMar);

                    }
                    else if (i == 4)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Apr + Entity.MonthValues.May + Entity.MonthValues.Jun) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CApr + Entity.MonthValues.CMay + Entity.MonthValues.CJun);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AApr + Entity.MonthValues.AMay + Entity.MonthValues.AJun);
                    }
                    else if (i == 7)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jul + Entity.MonthValues.Aug + Entity.MonthValues.Sep) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJul + Entity.MonthValues.CAug + Entity.MonthValues.CSep);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJul + Entity.MonthValues.AAug + Entity.MonthValues.ASep);
                    }
                    else if (i == 10)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Oct + Entity.MonthValues.Nov + Entity.MonthValues.Dec) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.COct + Entity.MonthValues.CNov + Entity.MonthValues.CDec);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AOct + Entity.MonthValues.ANov + Entity.MonthValues.ADec);
                    }
                    else if (i == 13)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.Jan + Entity.NextYearMonthValues.Feb + Entity.NextYearMonthValues.Mar) : ThreeDash;
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CJan + Entity.NextYearMonthValues.CFeb + Entity.NextYearMonthValues.CMar) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.AJan + Entity.NextYearMonthValues.AFeb + Entity.NextYearMonthValues.AMar) : ThreeDash;
                    }
                    else if (i == 16)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.Apr + Entity.NextYearMonthValues.May + Entity.NextYearMonthValues.Jun) : ThreeDash;
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CApr + Entity.NextYearMonthValues.CMay + Entity.NextYearMonthValues.CJun) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.AApr + Entity.NextYearMonthValues.AMay + Entity.NextYearMonthValues.AJun) : ThreeDash;
                    }
                    else if (i == 19)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.Jul + Entity.NextYearMonthValues.Aug + Entity.NextYearMonthValues.Sep) : ThreeDash;
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.CJul + Entity.NextYearMonthValues.CAug + Entity.NextYearMonthValues.CSep) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.AJul + Entity.NextYearMonthValues.AAug + Entity.NextYearMonthValues.ASep) : ThreeDash;
                    }
                    else if (i == 22)
                    {
                        objBudgetMonth.value = IsMultiYearPlan && !isLineItem ? Convert.ToString(Entity.NextYearMonthValues.Oct + Entity.NextYearMonthValues.Nov + Entity.NextYearMonthValues.Dec) : ThreeDash;
                        objCostMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.COct + Entity.NextYearMonthValues.CNov + Entity.NextYearMonthValues.CDec) : ThreeDash;
                        objActualMonth.value = IsMultiYearPlan ? Convert.ToString(Entity.NextYearMonthValues.AOct + Entity.NextYearMonthValues.ANov + Entity.NextYearMonthValues.ADec) : ThreeDash;
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

        private List<Budgetdataobj> CampignNextYearQuarterlyAllocation(PlanBudgetModel Entity, bool isTactic, bool isLineItem, List<Budgetdataobj> BudgetDataObjList, bool isViewBy = false)
        {
            for (int i = 1; i <= 23; i += 3)
            {
                Budgetdataobj objBudgetMonth = new Budgetdataobj();
                Budgetdataobj objCostMonth = new Budgetdataobj();
                Budgetdataobj objActualMonth = new Budgetdataobj();
                if (!isViewBy)
                {
                    objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;
                    objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;
                    objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    objBudgetMonth.style = Entity.isBudgetEditable ? string.Empty : NotEditableCellStyle;
                    objCostMonth.style = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? string.Empty : NotEditableCellStyle;
                    objActualMonth.style = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? string.Empty : NotEditableCellStyle;

                    if (i < 13)
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
                    else if (i == 13)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jan + Entity.MonthValues.Feb + Entity.MonthValues.Mar) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJan + Entity.MonthValues.CFeb + Entity.MonthValues.CMar);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJan + Entity.MonthValues.AFeb + Entity.MonthValues.AMar);

                    }
                    else if (i == 16)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Apr + Entity.MonthValues.May + Entity.MonthValues.Jun) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CApr + Entity.MonthValues.CMay + Entity.MonthValues.CJun);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AApr + Entity.MonthValues.AMay + Entity.MonthValues.AJun);
                    }
                    else if (i == 19)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Jul + Entity.MonthValues.Aug + Entity.MonthValues.Sep) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.CJul + Entity.MonthValues.CAug + Entity.MonthValues.CSep);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AJul + Entity.MonthValues.AAug + Entity.MonthValues.ASep);
                    }
                    else if (i == 22)
                    {
                        objBudgetMonth.value = !isLineItem ? Convert.ToString(Entity.MonthValues.Oct + Entity.MonthValues.Nov + Entity.MonthValues.Dec) : ThreeDash;
                        objCostMonth.value = Convert.ToString(Entity.MonthValues.COct + Entity.MonthValues.CNov + Entity.MonthValues.CDec);
                        objActualMonth.value = Convert.ToString(Entity.MonthValues.AOct + Entity.MonthValues.ANov + Entity.MonthValues.ADec);
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
            foreach (PlanBudgetModel item in BudgetModel)
            {
                //Set permission for editing cell for respective entities
                if (item.ActivityType == ActivityType.ActivityPlan)
                {
                    //chek user's plan edit permsion or user is owner
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    if (item.CreatedBy == UserId || IsPlanEditAllAuthorized)
                    {
                        item.isBudgetEditable = true;
                    }
                    else if (lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        item.isBudgetEditable = true;
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityCampaign)
                {
                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        //to find tactic level permission ,first get program list and then get respective tactic list of program which will be used to get editable tactic list
                        List<string> modelprogramid = BudgetModel.Where(minner => minner.ActivityType == ActivityType.ActivityProgram && minner.ParentActivityId == item.ActivityId).Select(minner => minner.ActivityId).ToList();
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && modelprogramid.Contains(m.ParentActivityId)).Select(m => Convert.ToInt32(m.Id)).ToList();
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isBudgetEditable = true;
                        }
                        else
                        {
                            item.isBudgetEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityProgram)
                {
                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        //to find tactic level permission , get respective tactic list of program which will be used to get editable tactic list
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && m.ParentActivityId == item.ActivityId).Select(m => Convert.ToInt32(m.Id)).ToList();
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isBudgetEditable = true;
                        }
                        else
                        {
                            item.isBudgetEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityTactic)
                {
                    //chek user is subordinate or user is owner
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.Id));
                        //Check tactic is editable or not
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isBudgetEditable = true;
                            item.isCostEditable = true;
                        }
                        else
                        {
                            item.isBudgetEditable = false;
                            item.isCostEditable = false;
                        }
                    }
                }
                else if (item.ActivityType == ActivityType.ActivityLineItem)
                {
                    int tacticOwner = 0;
                    if (BudgetModel.Where(m => m.ActivityId == item.ParentActivityId).Any())
                    {
                        tacticOwner = BudgetModel.Where(m => m.ActivityId == item.ParentActivityId).FirstOrDefault().CreatedBy;
                    }

                    //chek user is subordinate or user is owner of line item or user is owner of tactic
                    if (item.CreatedBy == UserId || tacticOwner == UserId || lstSubordinatesIds.Contains(tacticOwner))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();

                        planTacticIds.Add(Convert.ToInt32(item.ParentId));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isActualEditable = true;
                            item.isCostEditable = true;
                        }
                        else
                        {
                            item.isActualEditable = false;
                            item.isCostEditable = false;
                        }

                    }
                }
            }
            return BudgetModel;
        }

        private List<PlanBudgetModel> ManageLineItems(List<PlanBudgetModel> model)
        {
            foreach (PlanBudgetModel l in model.Where(l => l.ActivityType == ActivityType.ActivityTactic))
            {
                //// Calculate Line items Difference.
                BudgetMonth lineDiff = new BudgetMonth();

                List<PlanBudgetModel> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                PlanBudgetModel otherLine = lines.Where(ol => ol.LineItemTypeId == null).FirstOrDefault();
                lines = lines.Where(ol => ol.LineItemTypeId != null).ToList();
                //calculate total line item difference with respective tactics
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiff.CJan = l.MonthValues.CJan - lines.Sum(lmon => (double?)lmon.MonthValues.CJan) ?? 0;
                        lineDiff.CFeb = l.MonthValues.CFeb - lines.Sum(lmon => (double?)lmon.MonthValues.CFeb) ?? 0;
                        lineDiff.CMar = l.MonthValues.CMar - lines.Sum(lmon => (double?)lmon.MonthValues.CMar) ?? 0;
                        lineDiff.CApr = l.MonthValues.CApr - lines.Sum(lmon => (double?)lmon.MonthValues.CApr) ?? 0;
                        lineDiff.CMay = l.MonthValues.CMay - lines.Sum(lmon => (double?)lmon.MonthValues.CMay) ?? 0;
                        lineDiff.CJun = l.MonthValues.CJun - lines.Sum(lmon => (double?)lmon.MonthValues.CJun) ?? 0;
                        lineDiff.CJul = l.MonthValues.CJul - lines.Sum(lmon => (double?)lmon.MonthValues.CJul) ?? 0;
                        lineDiff.CAug = l.MonthValues.CAug - lines.Sum(lmon => (double?)lmon.MonthValues.CAug) ?? 0;
                        lineDiff.CSep = l.MonthValues.CSep - lines.Sum(lmon => (double?)lmon.MonthValues.CSep) ?? 0;
                        lineDiff.COct = l.MonthValues.COct - lines.Sum(lmon => (double?)lmon.MonthValues.COct) ?? 0;
                        lineDiff.CNov = l.MonthValues.CNov - lines.Sum(lmon => (double?)lmon.MonthValues.CNov) ?? 0;
                        lineDiff.CDec = l.MonthValues.CDec - lines.Sum(lmon => (double?)lmon.MonthValues.CDec) ?? 0;


                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthValues = lineDiff;

                        double allocated = l.TotalAllocatedCost - lines.Sum(l1 => l1.TotalAllocatedCost);
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().TotalAllocatedCost = allocated;

                        // Calculate Balance UnAllocated Cost
                        double BalanceUnallocatedCost = l.UnallocatedCost - lines.Sum(lmon => lmon.UnallocatedCost);
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().UnallocatedCost = BalanceUnallocatedCost;

                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthValues = l.MonthValues;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().TotalAllocatedCost = l.TotalAllocatedCost < 0 ? 0 : l.TotalAllocatedCost;
                    }
                }
            }
            return model;
        }

        //This function sum up the total of planned and actuals cell of budget to child to parent
        private List<PlanBudgetModel> CalculateBottomUp(List<PlanBudgetModel> model, string ParentActivityType, string ChildActivityType, string ViewBy)
        {
            double totalMonthCostSum = 0;
            // int _ViewById = ViewBy;            

            foreach (PlanBudgetModel l in model.Where(_mdl => _mdl.ActivityType == ParentActivityType))
            {
                //// check if ViewBy is Campaign selected then set weightage value to 100;
                int weightage = 100;
                if (ViewBy != PlanGanttTypes.Tactic.ToString())
                {
                    weightage = l.Weightage;
                }
                weightage = weightage / 100;

                BudgetMonth parent = new BudgetMonth();
                parent.AJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJan * weightage)) ?? 0;
                parent.AFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AFeb * weightage)) ?? 0;
                parent.AMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AMar * weightage)) ?? 0;
                parent.AApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AApr * weightage)) ?? 0;
                parent.AMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AMay * weightage)) ?? 0;
                parent.AJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJun * weightage)) ?? 0;
                parent.AJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJul * weightage)) ?? 0;
                parent.AAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AAug * weightage)) ?? 0;
                parent.ASep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ASep * weightage)) ?? 0;
                parent.AOct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AOct * weightage)) ?? 0;
                parent.ANov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ANov * weightage)) ?? 0;
                parent.ADec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ADec * weightage)) ?? 0;

                if (ParentActivityType != ActivityType.ActivityTactic)
                {
                    parent.CJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJan * weightage)) ?? 0;
                    parent.CFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CFeb * weightage)) ?? 0;
                    parent.CMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CMar * weightage)) ?? 0;
                    parent.CApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CApr * weightage)) ?? 0;
                    parent.CMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CMay * weightage)) ?? 0;
                    parent.CJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJun * weightage)) ?? 0;
                    parent.CJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJul * weightage)) ?? 0;
                    parent.CAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CAug * weightage)) ?? 0;
                    parent.CSep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CSep * weightage)) ?? 0;
                    parent.COct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.COct * weightage)) ?? 0;
                    parent.CNov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CNov * weightage)) ?? 0;
                    parent.CDec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CDec * weightage)) ?? 0;
                    totalMonthCostSum = parent.CJan + parent.CFeb + parent.CMar + parent.CApr + parent.CMay + parent.CJun + parent.CJul + parent.CAug + parent.CSep + parent.COct + parent.CNov + parent.CDec;
                }
                else
                {
                    parent.CJan = l.MonthValues.CJan;
                    parent.CFeb = l.MonthValues.CFeb;
                    parent.CMar = l.MonthValues.CMar;
                    parent.CApr = l.MonthValues.CApr;
                    parent.CMay = l.MonthValues.CMay;
                    parent.CJun = l.MonthValues.CJun;
                    parent.CJul = l.MonthValues.CJul;
                    parent.CAug = l.MonthValues.CAug;
                    parent.CSep = l.MonthValues.CSep;
                    parent.COct = l.MonthValues.COct;
                    parent.CNov = l.MonthValues.CNov;
                    parent.CDec = l.MonthValues.CDec;
                }

                parent.Jan = l.MonthValues.Jan;
                parent.Feb = l.MonthValues.Feb;
                parent.Mar = l.MonthValues.Mar;
                parent.Apr = l.MonthValues.Apr;
                parent.May = l.MonthValues.May;
                parent.Jun = l.MonthValues.Jun;
                parent.Jul = l.MonthValues.Jul;
                parent.Aug = l.MonthValues.Aug;
                parent.Sep = l.MonthValues.Sep;
                parent.Oct = l.MonthValues.Oct;
                parent.Nov = l.MonthValues.Nov;
                parent.Dec = l.MonthValues.Dec;

                PlanBudgetModel Entity = model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault();
                Entity.MonthValues = parent;
                if (l.ActivityType != ActivityType.ActivityTactic)
                {
                    Entity.TotalAllocatedCost = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    Entity.UnallocatedCost = Entity.TotalAllocatedCost - totalMonthCostSum;
                }
                Entity.TotalActuals = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalActuals) ?? 0;
                //}
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
                BudgetMonth parent = new BudgetMonth();
                List<PlanBudgetModel> lstLineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();

                //// check if ViewBy is Campaign selected then set weightage value to 100;
                if (ViewBy != PlanGanttTypes.Tactic.ToString())
                {
                    weightage = l.Weightage;
                }

                foreach (PlanBudgetModel line in lstLineItems)
                {
                    BudgetMonth lineBudget = new BudgetMonth();
                    lineBudget.AJan = (double)(line.MonthValues.AJan * weightage) / 100;
                    lineBudget.AFeb = (double)(line.MonthValues.AFeb * weightage) / 100;
                    lineBudget.AMar = (double)(line.MonthValues.AMar * weightage) / 100;
                    lineBudget.AApr = (double)(line.MonthValues.AApr * weightage) / 100;
                    lineBudget.AMay = (double)(line.MonthValues.AMay * weightage) / 100;
                    lineBudget.AJun = (double)(line.MonthValues.AJun * weightage) / 100;
                    lineBudget.AJul = (double)(line.MonthValues.AJul * weightage) / 100;
                    lineBudget.AAug = (double)(line.MonthValues.AAug * weightage) / 100;
                    lineBudget.ASep = (double)(line.MonthValues.ASep * weightage) / 100;
                    lineBudget.AOct = (double)(line.MonthValues.AOct * weightage) / 100;
                    lineBudget.ANov = (double)(line.MonthValues.ANov * weightage) / 100;
                    lineBudget.ADec = (double)(line.MonthValues.ADec * weightage) / 100;

                    lineBudget.CJan = (double)(line.MonthValues.CJan * weightage) / 100;
                    lineBudget.CFeb = (double)(line.MonthValues.CFeb * weightage) / 100;
                    lineBudget.CMar = (double)(line.MonthValues.CMar * weightage) / 100;
                    lineBudget.CApr = (double)(line.MonthValues.CApr * weightage) / 100;
                    lineBudget.CMay = (double)(line.MonthValues.CMay * weightage) / 100;
                    lineBudget.CJun = (double)(line.MonthValues.CJun * weightage) / 100;
                    lineBudget.CJul = (double)(line.MonthValues.CJul * weightage) / 100;
                    lineBudget.CAug = (double)(line.MonthValues.CAug * weightage) / 100;
                    lineBudget.CSep = (double)(line.MonthValues.CSep * weightage) / 100;
                    lineBudget.COct = (double)(line.MonthValues.COct * weightage) / 100;
                    lineBudget.CNov = (double)(line.MonthValues.CNov * weightage) / 100;
                    lineBudget.CDec = (double)(line.MonthValues.CDec * weightage) / 100;

                    lineBudget.Jan = (double)(line.MonthValues.Jan * weightage) / 100;
                    lineBudget.Feb = (double)(line.MonthValues.Feb * weightage) / 100;
                    lineBudget.Mar = (double)(line.MonthValues.Mar * weightage) / 100;
                    lineBudget.Apr = (double)(line.MonthValues.Apr * weightage) / 100;
                    lineBudget.May = (double)(line.MonthValues.May * weightage) / 100;
                    lineBudget.Jun = (double)(line.MonthValues.Jun * weightage) / 100;
                    lineBudget.Jul = (double)(line.MonthValues.Jul * weightage) / 100;
                    lineBudget.Aug = (double)(line.MonthValues.Aug * weightage) / 100;
                    lineBudget.Sep = (double)(line.MonthValues.Sep * weightage) / 100;
                    lineBudget.Oct = (double)(line.MonthValues.Oct * weightage) / 100;
                    lineBudget.Nov = (double)(line.MonthValues.Nov * weightage) / 100;
                    lineBudget.Dec = (double)(line.MonthValues.Dec * weightage) / 100;
                    line.MonthValues = lineBudget;
                }
            }
            return model;
        }

        private string GetInitialYearFromTimeFrame(string Year)
        {
            if (!string.IsNullOrEmpty(Year))
            {
                string[] arrYear = Year.Split('-');
                if (arrYear.Length > 0)
                {
                    return arrYear[0];
                }
            }
            return string.Empty;

        }

        private bool IsMultiyearTimeframe(string Year)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(Year))
            {
                string[] arrYear = Year.Split('-');
                if (arrYear.Length == 2)//If array of year have 2 items then we can find diff between 2 years
                {
                    int FirstYear = Convert.ToInt32(arrYear[0]);
                    int LastYear = Convert.ToInt32(arrYear[1]);
                    int diff = LastYear - FirstYear;
                    if (diff > 0)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
