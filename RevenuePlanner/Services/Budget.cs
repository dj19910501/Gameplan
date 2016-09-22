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
        private StoredProcedure objSp;
        public Budget()
        {
            objDbMrpEntities = new MRPEntities();
            objSp = new StoredProcedure();
        }
        
        private const string Open = "1";
        private const string CellLocked = "1";
        private const string CellNotLocked = "0";
        public const string FixHeader = "ActivityId,,,,,";
        public const string FixColumnIds = "ActivityId,TaskName,Buttons,Total Budget,PlannedCost,Total Actual";
        public const string FixColType = "ro,tree,ro,ed,ed,ed";
        public const string FixcolWidth = "100,250,50,100,100,100";
        public const string FixColsorting = "na,na,na,na,na,na";
        public const string QuarterPrefix = "Q";
        public const string DhtmlxColSpan = "#cspan";
        public const string ColBudget = "Budget";
        public const string ColActual = "Actual";
        public const string ColPlanned = "Planned";


        public BudgetDHTMLXGridModel GetBudget(string PlanIds, double PlanExchangeRate, Enums.ViewBy viewBy = Enums.ViewBy.Campaign, string year = "", string CustomFieldId = "", string OwnerIds = "", string TacticTypeids = "", string StatusIds = "")
        {
            DataTable dt = objSp.GetBudget(PlanIds, OwnerIds, TacticTypeids, StatusIds); //Get budget data for budget,planned cost and actual using store proc. GetplanBudget

            List<PlanBudgetModel> model = CreateBudgetDataModel(dt, PlanExchangeRate); //Convert datatable with budget data to PlanBudgetModel model

            model = SetCustomFieldRestriction(model);//Set customfield permission for budget cells. budget cell will editable or not.
            int ViewByID = (int)viewBy;
             //Set actual for quarters
            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
           
            model = ManageLineItems(model);//Manage lineitems unallocated cost values in other line item

            #region "Calculate Monthly Budget from Bottom to Top for Hierarchy level like: LineItem > Tactic > Program > Campaign > CustomField(if filtered) > Plan"

            //// Set ViewBy data to model.
            model = CalculateBottomUp(model, ActivityType.ActivityTactic, ActivityType.ActivityLineItem, ViewByID);//// Calculate monthly Tactic budget from it's child budget i.e LineItem

            model = CalculateBottomUp(model, ActivityType.ActivityProgram, ActivityType.ActivityTactic, ViewByID);//// Calculate monthly Program budget from it's child budget i.e Tactic

            model = CalculateBottomUp(model, ActivityType.ActivityCampaign, ActivityType.ActivityProgram, ViewByID);//// Calculate monthly Campaign budget from it's child budget i.e Program

            model = CalculateBottomUp(model, ActivityType.ActivityPlan, ActivityType.ActivityCampaign, ViewByID);//// Calculate monthly Plan budget from it's child budget i.e Campaign
            
            #endregion

             model = SetLineItemCostByWeightage(model, ViewByID);//// Set LineItem monthly budget cost by it's parent tactic weightage.

            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel();
            objBudgetDHTMLXGrid = GenerateHeaderString(AllocatedBy, objBudgetDHTMLXGrid, model);

            objBudgetDHTMLXGrid = CreateDhtmlxFormattedBudgetData(objBudgetDHTMLXGrid, model, AllocatedBy);//create model to bind data in grid as per DHTMLx grid format.
            return objBudgetDHTMLXGrid;
        }

        public List<PlanBudgetModel> CreateBudgetDataModel(DataTable DtBudget, double PlanExchangeRate)
        {
            List<PlanBudgetModel> model = new List<PlanBudgetModel>();
            RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
            model = DtBudget.AsEnumerable().Select(row => new PlanBudgetModel
            {
                Id = row["Id"].ToString(),
                ActivityId = Convert.ToString( row["ActivityId"]),
                ActivityName = Convert.ToString( row["Title"]),
                ActivityType = Convert.ToString( row["ActivityType"]),
                ParentActivityId = Convert.ToString( row["ParentActivityId"]),
                YearlyBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Budget"])), PlanExchangeRate),
                TotalAllocatedBudget = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["TotalAllocationBudget"])), PlanExchangeRate),
                TotalActuals = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["TotalAllocationActual"])), PlanExchangeRate),
                TotalAllocatedCost = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["TotalAllocationCost"])), PlanExchangeRate),
                IsOwner = Convert.ToBoolean(row["IsOwner"]),
                CreatedBy = int.Parse(row["CreatedBy"].ToString()),
                LineItemTypeId = Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"])),
                isAfterApproved = Convert.ToBoolean(row["IsAfterApproved"]),
                MonthValues = new BudgetMonth()
                {
                    //Budget Month Allocation
                    Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y2"])), PlanExchangeRate),
                    Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y1"])), PlanExchangeRate),
                    Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y3"])), PlanExchangeRate),
                    Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y4"])), PlanExchangeRate),
                    May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y5"])), PlanExchangeRate),
                    Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y6"])), PlanExchangeRate),
                    Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y7"])), PlanExchangeRate),
                    Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y8"])), PlanExchangeRate),
                    Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y9"])), PlanExchangeRate),
                    Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y10"])), PlanExchangeRate),
                    Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y11"])), PlanExchangeRate),
                    Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["Y12"])), PlanExchangeRate),

                    //Cost Month Allocation
                    CFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY2"])), PlanExchangeRate),
                    CJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY1"])), PlanExchangeRate),
                    CMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY3"])), PlanExchangeRate),
                    CApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY4"])), PlanExchangeRate),
                    CMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY5"])), PlanExchangeRate),
                    CJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY6"])), PlanExchangeRate),
                    CJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY7"])), PlanExchangeRate),
                    CAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY8"])), PlanExchangeRate),
                    CSep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY9"])), PlanExchangeRate),
                    COct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY10"])), PlanExchangeRate),
                    CNov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY11"])), PlanExchangeRate),
                    CDec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["CostY12"])), PlanExchangeRate),

                    //Actuals Month Allocation
                    AFeb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["ActualY2"])), PlanExchangeRate),
                    AJan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["ActualY1"])), PlanExchangeRate),
                    AMar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY3"])), PlanExchangeRate),
                    AApr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY4"])), PlanExchangeRate),
                    AMay = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY5"])), PlanExchangeRate),
                    AJun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY6"])), PlanExchangeRate),
                    AJul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY7"])), PlanExchangeRate),
                    AAug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY8"])), PlanExchangeRate),
                    ASep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY9"])), PlanExchangeRate),
                    AOct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["ActualY10"])), PlanExchangeRate),
                    ANov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["ActualY11"])), PlanExchangeRate),
                    ADec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString( row["ActualY12"])), PlanExchangeRate)
                }
            }).ToList();
            return model;
        }
        public List<Budgetdataobj> SetBudgetDhtmlxFormattedValues(List<Budgetdataobj> BudgetDataObjList,List<PlanBudgetModel> model,PlanBudgetModel Entity,string EntityType,string AllocatedBy)
        {
            Budgetdataobj BudgetDataObj = new Budgetdataobj();

            BudgetDataObj.value = Entity.Id + ";" + Convert.ToString(EntityType);
            BudgetDataObjList.Add(BudgetDataObj);

            BudgetDataObj = new Budgetdataobj();
            //Add title of plan entity into dhtmlx model
            BudgetDataObj.value = HttpUtility.HtmlEncode("<a id=aPlanDetails onClick=OpenPlanInspectPopup() ondblclick=PreventDoubleClick()>" + (HttpUtility.HtmlEncode(Entity.ActivityName).Replace("'", "&#39;")) + "</a>");
            BudgetDataObjList.Add(BudgetDataObj);

            Budgetdataobj iconsData = new Budgetdataobj();
            //Set icon of magnifying glass and honey comb for plan entity with respective ids
            iconsData.value = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Entity.ActivityId + " per=true></div> <div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this) id=PlanAdd taskId=" + Entity.ActivityId + " csvId=" + Entity.ActivityId.ToString() + "></div>";
            BudgetDataObjList.Add(iconsData);

            //Set Total Actual,Total Budget and Total planned cost for plan entity
            BudgetDataObjList = CampaignBudgetSummary(model,EntityType, Entity.ParentActivityId,
                  BudgetDataObjList, AllocatedBy, Entity.ActivityId);
            //Set monthly/quarterly allocation of budget,actuals and planned for plan
            BudgetDataObjList = CampaignMonth(model,EntityType, Entity.ParentActivityId,
                    BudgetDataObjList, AllocatedBy, Entity.ActivityId);
            return BudgetDataObjList;
        }
        public BudgetDHTMLXGridModel CreateDhtmlxFormattedBudgetData(BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> model, string AllocatedBy)
        {

            List<BudgetDHTMLXGridDataModel> gridjsonlist = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel gridjsonlistPlanObj = new BudgetDHTMLXGridDataModel();

            List<Budgetdataobj> BudgetDataObjList ;
            Budgetdataobj BudgetDataObj;
            //Set plan entity in the dhtmlx formated model at top level of the hierarchy using loop
            foreach (PlanBudgetModel bm in model.Where(p => p.ActivityType == ActivityType.ActivityPlan).OrderBy(p => p.ActivityName))
            {
                gridjsonlistPlanObj = new BudgetDHTMLXGridDataModel();
                gridjsonlistPlanObj.id = ActivityType.ActivityPlan + HttpUtility.HtmlEncode(bm.ActivityId);
                gridjsonlistPlanObj.open = Open;

                BudgetDataObjList = new List<Budgetdataobj>();
                BudgetDataObjList=   SetBudgetDhtmlxFormattedValues(BudgetDataObjList,model,bm,ActivityType.ActivityPlan,AllocatedBy);
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
                    CampaignRowsObj.id = ActivityType.ActivityCampaign + HttpUtility.HtmlEncode(bmc.ActivityId);
                    CampaignRowsObj.open = null;

                    List<Budgetdataobj> CampaignDataObjList = new List<Budgetdataobj>();
                    CampaignDataObjList = SetBudgetDhtmlxFormattedValues(CampaignDataObjList, model, bmc, ActivityType.ActivityCampaign, AllocatedBy);

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
                        ProgramRowsObj.id = ActivityType.ActivityProgram + HttpUtility.HtmlEncode(bmp.ActivityId);
                        ProgramRowsObj.open = null;

                        List<Budgetdataobj> ProgramDataObjList = new List<Budgetdataobj>();
                        ProgramDataObjList = SetBudgetDhtmlxFormattedValues(ProgramDataObjList, model, bmp, ActivityType.ActivityProgram, AllocatedBy);
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
                            TacticRowsObj.id = ActivityType.ActivityTactic + HttpUtility.HtmlEncode(bmt.ActivityId);
                            TacticRowsObj.open = null;

                            List<Budgetdataobj> TacticDataObjList = new List<Budgetdataobj>();
                            TacticDataObjList = SetBudgetDhtmlxFormattedValues(TacticDataObjList, model, bmt, ActivityType.ActivityTactic, AllocatedBy);

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
                                LineRowsObj.id = ActivityType.ActivityLineItem + HttpUtility.HtmlEncode(bml.ActivityId);
                                LineRowsObj.open = null;
                                List<Budgetdataobj> LineDataObjList = new List<Budgetdataobj>();
                                LineDataObjList = SetBudgetDhtmlxFormattedValues(LineDataObjList, model, bml, ActivityType.ActivityLineItem, AllocatedBy);
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
                gridjsonlist.Add(gridjsonlistPlanObj);
            }
            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            objBudgetDHTMLXGrid.Grid.rows = gridjsonlist;
            return objBudgetDHTMLXGrid;
        }

        private BudgetDHTMLXGridModel GenerateHeaderString(string AllocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid, List<PlanBudgetModel> model)
        {
            string setHeader = string.Empty, colType = string.Empty, width = string.Empty, colSorting = string.Empty, columnIds = string.Empty;
            List<string> attachHeader = new List<string>();
             
            setHeader = FixHeader;
            columnIds = FixColumnIds;
            colType = FixColType;
            width = FixcolWidth;
            colSorting = FixColsorting;
            attachHeader.Add("ActivityId");
            attachHeader.Add("Task Name");
            attachHeader.Add("");
            attachHeader.Add("Total Budget");
            attachHeader.Add("PlannedCost");
            attachHeader.Add("Total Actual");
            
            if (AllocatedBy.ToLower() == Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()].ToLower())
            {
                int quarterCounter = 1;
                for (int i = 1; i <= 11; i += 3)
                {
                    DateTime dt = new DateTime(2012, i, 1);

                    setHeader = setHeader + ",Q" + quarterCounter.ToString() + ",#cspan,#cspan";//CQ" + quarterCounter.ToString() + ",AQ" + quarterCounter.ToString();
                    attachHeader.Add(ColBudget);
                    attachHeader.Add(ColPlanned);
                    attachHeader.Add(ColActual);


                    columnIds = columnIds + ",Q" + quarterCounter.ToString() + ",#cspan,#cspan";//CQ" + quarterCounter.ToString() + ",AQ" + quarterCounter.ToString();
                    colType = colType + ",ed,ed,ed";
                    width = width + ",100,100,100";
                    colSorting = colSorting + ",str,str,str";
                    
                    quarterCounter++;
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    DateTime dt = new DateTime(2012, i, 1);

                    setHeader = setHeader + "," + dt.ToString("MMM").ToUpper() + ",#cspan,#cspan";//C" + dt.ToString("MMM").ToUpper() + ",A" + dt.ToString("MMM").ToUpper();
                    columnIds = columnIds + "," + dt.ToString("MMM").ToUpper() + ",#cspan,#cspan";//C" + dt.ToString("MMM").ToUpper() + ",A" + dt.ToString("MMM").ToUpper();
                    attachHeader.Add(ColBudget);
                    attachHeader.Add(ColPlanned);
                    attachHeader.Add(ColActual);
                    colType = colType + ",ed,ed,ed";
                    width = width + ",100,100,100";
                    colSorting = colSorting + ",str,str,str";
                }
            }
            objBudgetDHTMLXGrid.SetHeader = setHeader;
            objBudgetDHTMLXGrid.ColType = colType;
            objBudgetDHTMLXGrid.Width = width;
            objBudgetDHTMLXGrid.ColSorting = colSorting;
            objBudgetDHTMLXGrid.ColumnIds = columnIds;
            objBudgetDHTMLXGrid.AttachHeader = attachHeader;
            return objBudgetDHTMLXGrid;
        }

        private List<Budgetdataobj> CampaignBudgetSummary(List<PlanBudgetModel> model, string activityType, string parentActivityId, List<Budgetdataobj> BudgetDataObjList, string allocatedBy, string activityId)
        {
            PlanBudgetModel Entity = model.Where(pl => pl.ActivityType == activityType && pl.ParentActivityId == parentActivityId && pl.ActivityId == activityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();

            if (Entity != null)
            {
                Budgetdataobj objTotalBudget = new Budgetdataobj();
                Budgetdataobj objTotalCost = new Budgetdataobj();
                Budgetdataobj objTotalActual = new Budgetdataobj();
                double childTotalAllocated = 0.0; //Child Total of respective entities
                if (activityType == ActivityType.ActivityPlan)
                {
                    childTotalAllocated = model.Where(p => p.ActivityType == ActivityType.ActivityCampaign && p.ParentActivityId == Entity.ActivityId).Select(p => p.TotalAllocatedBudget).Sum();
                }
                else if (activityType == ActivityType.ActivityCampaign)
                {
                    childTotalAllocated = model.Where(p => p.ActivityType == ActivityType.ActivityPlan && p.ParentActivityId == Entity.ActivityId).Select(p => p.TotalAllocatedBudget).Sum();
                }
                else if (activityType == ActivityType.ActivityProgram)
                {
                    childTotalAllocated = model.Where(c => c.ActivityType == ActivityType.ActivityTactic && c.ParentActivityId == Entity.ActivityId).Select(c => c.TotalAllocatedBudget).Sum();
                }
                else if (activityType == ActivityType.ActivityTactic)
                {
                    childTotalAllocated = model.Where(c => c.ActivityType == ActivityType.ActivityLineItem && c.ParentActivityId == Entity.ActivityId).Select(c => c.TotalAllocatedBudget).Sum();
                }
                objTotalBudget.value = Convert.ToString(childTotalAllocated);//Set values for Total budget
                objTotalBudget.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                objTotalActual.value = Convert.ToString( Entity.TotalActuals);//Set values for Total actual
                objTotalActual.locked = CellLocked;

                bool isOtherLineItem = activityType == ActivityType.ActivityLineItem && Entity.LineItemTypeId == null;
                objTotalCost.value = Convert.ToString(Entity.TotalAllocatedCost);
                objTotalCost.locked = Entity.isCostEditable && !isOtherLineItem ? CellNotLocked : CellLocked;

                BudgetDataObjList.Add(objTotalBudget);
                BudgetDataObjList.Add(objTotalCost);
                BudgetDataObjList.Add(objTotalActual);

            }
            return BudgetDataObjList;
        }

        private List<Budgetdataobj> CampaignMonth(List<PlanBudgetModel> model, string activityType, string parentActivityId, List<Budgetdataobj> BudgetDataObjList, string allocatedBy, string activityId)
        {
            PlanBudgetModel Entity = model.Where(pl => pl.ActivityType == activityType && pl.ParentActivityId == parentActivityId && pl.ActivityId == activityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();
            bool isTactic = activityType == Helpers.ActivityType.ActivityTactic ? true : false;
            bool isLineItem = activityType == Helpers.ActivityType.ActivityLineItem ? true : false;
            if (allocatedBy != "quarters")  // Modified by Sohel Pathan on 08/09/2014 for PL ticket #642.
            {

                for (int i = 1; i <= 12; i++)
                {
                    Budgetdataobj objBudgetMonth = new Budgetdataobj();
                    Budgetdataobj objCostMonth = new Budgetdataobj();
                    Budgetdataobj objActualMonth = new Budgetdataobj();
                    if (i == 1)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Jan.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CJan.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AJan.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 2)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Feb.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CFeb.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AFeb.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 3)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Mar.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CMar.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AMar.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 4)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Apr.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CApr.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AApr.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 5)
                    {
                        objBudgetMonth.value = Entity.MonthValues.May.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CMay.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AMay.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 6)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Jun.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CJun.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AJun.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 7)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Jul.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CJul.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AJul.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 8)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Aug.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CAug.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AAug.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 9)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Sep.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CSep.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.ASep.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 10)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Oct.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.COct.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AOct.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 11)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Nov.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CNov.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.ANov.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 12)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Dec.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CDec.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.ADec.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    BudgetDataObjList.Add(objBudgetMonth);
                    BudgetDataObjList.Add(objCostMonth);
                    BudgetDataObjList.Add(objActualMonth);
                }
            }
            else
            {
                for (int i = 1; i <= 11; i += 3)
                {
                    Budgetdataobj objBudgetMonth = new Budgetdataobj();
                    Budgetdataobj objCostMonth = new Budgetdataobj();
                    Budgetdataobj objActualMonth = new Budgetdataobj();
                    if (i == 1)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Jan.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CJan.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AJan.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 4)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Apr.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CApr.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AApr.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 7)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Jul.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.CJul.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AJul.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    else if (i == 10)
                    {
                        objBudgetMonth.value = Entity.MonthValues.Oct.ToString();
                        objBudgetMonth.locked = Entity.isBudgetEditable ? CellNotLocked : CellLocked;

                        objCostMonth.value = Entity.MonthValues.COct.ToString();
                        objCostMonth.locked = Entity.isCostEditable && (isTactic || (isLineItem && Entity.LineItemTypeId != null)) ? CellNotLocked : CellLocked;

                        objActualMonth.value = Entity.MonthValues.AOct.ToString();
                        objActualMonth.locked = Entity.isActualEditable && isLineItem && Entity.isAfterApproved ? CellNotLocked : CellLocked;
                    }
                    BudgetDataObjList.Add(objBudgetMonth);
                    BudgetDataObjList.Add(objCostMonth);
                    BudgetDataObjList.Add(objActualMonth);
                }


            }
            return BudgetDataObjList;
        }
                  
        public List<PlanBudgetModel> SetCustomFieldRestriction(List<PlanBudgetModel> BudgetModel)
        {
            List<int> lstSubordinatesIds = new List<int>();

            //get list of subordiantes which will be use to chekc if user is subordinate
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);
            }
            //Custom field type dropdown list
            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();
            //Custom field type text box
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            //flag will be use to set if custom field is display for filter or not
            bool isDisplayForFilter = false;

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), Sessions.User.CID);
            
            //Get list tactic's custom field
            List<CustomField> customfieldlist = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.CID && customfield.EntityType.Equals(EntityTypeTactic) && customfield.IsDeleted.Equals(false)).ToList();
            //Check custom field whic are not set to display for filter and is required are exist
            bool CustomFieldexists = customfieldlist.Where(customfield => customfield.IsRequired && !isDisplayForFilter).Any();
            //get dropdown type of custom fields ids
            List<int> customfieldids = customfieldlist.Where(customfield => customfield.CustomFieldType.Name == DropDownList && (isDisplayForFilter ? customfield.IsDisplayForFilter : true)).Select(customfield => customfield.CustomFieldId).ToList();
            //Get tactics only for budget model
            List<string> tacIds = BudgetModel.Where(t =>  t.ActivityType.ToUpper() == EntityTypeTactic.ToUpper()).Select(t => t.ActivityId).ToList();
            
            //get tactic ids from tactic list
            List<int> intList = tacIds.ConvertAll(s => Int32.Parse(s));
            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => intList.Contains(entityid.EntityId)).ToList();

            //Get tactic custom fields list
            List<CustomField_Entity> lstAllTacticCustomFieldEntities = Entities.Where(customFieldEntity => customfieldids.Contains(customFieldEntity.CustomFieldId))
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList(); 
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.ID, true);
            foreach (PlanBudgetModel item in BudgetModel)
            {
                //Set permission for editing cell for respective entities
                if (item.ActivityType == ActivityType.ActivityPlan)
                {
                    //chek user's plan edit permsion or user is owner
                    bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                    if (item.CreatedBy == Sessions.User.ID || IsPlanEditAllAuthorized)
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
                    if (item.CreatedBy == Sessions.User.ID || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                       //to find tactic level permission ,first get program list and then get respective tactic list of program which will be used to get editable tactic list
                        List<string> modelprogramid = BudgetModel.Where(minner => minner.ActivityType == ActivityType.ActivityProgram && minner.ParentActivityId == item.ActivityId).Select(minner => minner.ActivityId).ToList();
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && modelprogramid.Contains(m.ParentActivityId)).Select(m => Convert.ToInt32(m.ActivityId)).ToList();
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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
                    if (item.CreatedBy == Sessions.User.ID || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        //to find tactic level permission , get respective tactic list of program which will be used to get editable tactic list
                        planTacticIds = BudgetModel.Where(m => m.ActivityType == ActivityType.ActivityTactic && m.ParentActivityId == item.ActivityId).Select(m => Convert.ToInt32(m.ActivityId)).ToList();
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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
                    if (item.CreatedBy == Sessions.User.ID || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ActivityId));
                        //Check tactic is editable or not
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isBudgetEditable = true;
                            item. isCostEditable = true;
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
                    if (item.CreatedBy == Sessions.User.ID || tacticOwner == Sessions.User.ID || lstSubordinatesIds.Contains(tacticOwner))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ParentActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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

                        lineDiff.CJan = lineDiff.CJan < 0 ? 0 : lineDiff.CJan;
                        lineDiff.CFeb = lineDiff.CFeb < 0 ? 0 : lineDiff.CFeb;
                        lineDiff.CMar = lineDiff.CMar < 0 ? 0 : lineDiff.CMar;
                        lineDiff.CApr = lineDiff.CApr < 0 ? 0 : lineDiff.CApr;
                        lineDiff.CMay = lineDiff.CMay < 0 ? 0 : lineDiff.CMay;
                        lineDiff.CJun = lineDiff.CJun < 0 ? 0 : lineDiff.CJun;
                        lineDiff.CJul = lineDiff.CJul < 0 ? 0 : lineDiff.CJul;
                        lineDiff.CAug = lineDiff.CAug < 0 ? 0 : lineDiff.CAug;
                        lineDiff.CSep = lineDiff.CSep < 0 ? 0 : lineDiff.CSep;
                        lineDiff.COct = lineDiff.COct < 0 ? 0 : lineDiff.COct;
                        lineDiff.CNov = lineDiff.CNov < 0 ? 0 : lineDiff.CNov;
                        lineDiff.CDec = lineDiff.CDec < 0 ? 0 : lineDiff.CDec;

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthValues = lineDiff;

                        double allocated = l.TotalAllocatedCost - lines.Sum(l1 => l1.TotalAllocatedCost);
                        allocated = allocated < 0 ? 0 : allocated;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().TotalAllocatedCost = allocated;
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
        private List<PlanBudgetModel> CalculateBottomUp(List<PlanBudgetModel> model, string ParentActivityType, string ChildActivityType, int ViewBy)
        {
            int _ViewById = ViewBy ;
            int weightage = 100;

            if (ParentActivityType == ActivityType.ActivityTactic)
            {
                List<PlanBudgetModel> LineCheck;
                foreach (PlanBudgetModel l in model.Where(_mdl => _mdl.ActivityType == ParentActivityType))
                {
                    LineCheck = new List<PlanBudgetModel>();
                    LineCheck = model.Where(lines => lines.ParentActivityId == l.ActivityId && lines.ActivityType == ActivityType.ActivityLineItem).ToList();
                    if (LineCheck.Count() > 0)
                    {
                        //// check if ViewBy is Campaign selected then set weightage value to 100;
                        if (_ViewById > 0)
                            weightage = l.Weightage;

                        BudgetMonth parent = new BudgetMonth();
                        parent.AJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJan * weightage) / 100) ?? 0;
                        parent.AFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AFeb * weightage) / 100) ?? 0;
                        parent.AMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AMar * weightage) / 100) ?? 0;
                        parent.AApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AApr * weightage) / 100) ?? 0;
                        parent.AMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AMay * weightage) / 100) ?? 0;
                        parent.AJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJun * weightage) / 100) ?? 0;
                        parent.AJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AJul * weightage) / 100) ?? 0;
                        parent.AAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AAug * weightage) / 100) ?? 0;
                        parent.ASep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ASep * weightage) / 100) ?? 0;
                        parent.AOct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.AOct * weightage) / 100) ?? 0;
                        parent.ANov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ANov * weightage) / 100) ?? 0;
                        parent.ADec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.ADec * weightage) / 100) ?? 0;

                        parent.CJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJan * weightage) / 100) ?? 0;
                        parent.CFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CFeb * weightage) / 100) ?? 0;
                        parent.CMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CMar * weightage) / 100) ?? 0;
                        parent.CApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CApr * weightage) / 100) ?? 0;
                        parent.CMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CMay * weightage) / 100) ?? 0;
                        parent.CJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJun * weightage) / 100) ?? 0;
                        parent.CJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CJul * weightage) / 100) ?? 0;
                        parent.CAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CAug * weightage) / 100) ?? 0;
                        parent.CSep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CSep * weightage) / 100) ?? 0;
                        parent.COct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.COct * weightage) / 100) ?? 0;
                        parent.CNov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CNov * weightage) / 100) ?? 0;
                        parent.CDec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)(line.MonthValues.CDec * weightage) / 100) ?? 0;

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

                        model.Where(m => m.ActivityId == l.ActivityId).FirstOrDefault().MonthValues = parent;
                    }
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().TotalAllocatedCost = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().TotalActuals = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalActuals) ?? 0;
                }
            }
            else
            {
                BudgetMonth parent;
                foreach (PlanBudgetModel l in model.Where(l => l.ActivityType == ParentActivityType))
                {
                    parent = new BudgetMonth();
                    
                    parent.AJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AJan) ?? 0;
                    parent.AFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AFeb) ?? 0;
                    parent.AMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AMar) ?? 0;
                    parent.AApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AApr) ?? 0;
                    parent.AMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AMay) ?? 0;
                    parent.AJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AJun) ?? 0;
                    parent.AJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AJul) ?? 0;
                    parent.AAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AAug) ?? 0;
                    parent.ASep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.ASep) ?? 0;
                    parent.AOct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.AOct) ?? 0;
                    parent.ANov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.ANov) ?? 0;
                    parent.ADec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.ADec) ?? 0;

                    parent.CJan = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CJan) ?? 0;
                    parent.CFeb = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CFeb) ?? 0;
                    parent.CMar = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CMar) ?? 0;
                    parent.CApr = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CApr) ?? 0;
                    parent.CMay = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CMay) ?? 0;
                    parent.CJun = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CJun) ?? 0;
                    parent.CJul = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CJul) ?? 0;
                    parent.CAug = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CAug) ?? 0;
                    parent.CSep = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CSep) ?? 0;
                    parent.COct = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.COct) ?? 0;
                    parent.CNov = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CNov) ?? 0;
                    parent.CDec = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.MonthValues.CDec) ?? 0;

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

                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().TotalAllocatedCost = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalAllocatedCost) ?? 0;
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().TotalActuals = model.Where(line => line.ActivityType == ChildActivityType && line.ParentActivityId == l.ActivityId).Sum(line => (double?)line.TotalActuals) ?? 0;
                    
                    model.Where(_mdl => _mdl.ActivityId == l.ActivityId).FirstOrDefault().MonthValues = parent;
                }
            }
            return model;
        }
        //This function apply weightage to budget cell values
        private List<PlanBudgetModel> SetLineItemCostByWeightage(List<PlanBudgetModel> model, int ViewBy)
        {
            int _ViewById = ViewBy != null ? ViewBy : 0;
            int weightage = 100;
            foreach (PlanBudgetModel l in model.Where(_mdl => _mdl.ActivityType == ActivityType.ActivityTactic))
            {
                BudgetMonth parent = new BudgetMonth();
                List<PlanBudgetModel> lstLineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();

                //// check if ViewBy is Campaign selected then set weightage value to 100;
                if (_ViewById > 0)
                    weightage = l.Weightage;

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

    }
}
