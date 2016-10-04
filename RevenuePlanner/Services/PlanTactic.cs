using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace RevenuePlanner.Services
{
    public class PlanTactic : IPlanTactic
    {
        #region Declaration
        private MRPEntities objDbMrpEntities;
        private ICurrency objCurrency;
        private HomeGridProperties objHomeGridProperty = new HomeGridProperties();
        public PlanTactic()
        {
            objDbMrpEntities = new MRPEntities();
            objCurrency = new Currency();
        }

        private string formatThousand = "#,##0.##";  // format thousand number values with comma
        private string commaString = ",";
        private Dictionary<string, int> tacticYears = new Dictionary<string, int>(); // start and end date years of tactic
        #endregion

        /// <summary>
        /// Add Balance line item whether cost is 0 or greater
        /// </summary>
        public void AddBalanceLineItem(int tacticId, double Cost, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
            objNewLineitem.PlanTacticId = tacticId;             // tactic for which balance line item created
            objNewLineitem.Title = Common.LineItemTitleDefault; // default name is "Balance"
            objNewLineitem.Cost = Cost;                         // balanced cost
            objNewLineitem.Description = string.Empty;
            objNewLineitem.CreatedBy = UserId;
            objNewLineitem.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objNewLineitem).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Get cost allocation for line items in tactic inspect popup
        /// </summary>
        public BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId, string AllocatedBy, int UserId, int ClientId, double PlanExchangeRate)
        {
            DataTable dtCosts = Common.GetTacticLineItemCostAllocation(curTacticId, UserId);    // Get cost allocation and set values to model
            DataRow objTacticRow = dtCosts.AsEnumerable().Where(x => Convert.ToInt32(x["id"]).Equals(curTacticId)).FirstOrDefault();
            if (objTacticRow != null)
            {
                int startYear = Convert.ToDateTime(objTacticRow["StartDate"]).Year;
                int endYear = Convert.ToDateTime(objTacticRow["EndDate"]).Year;
                tacticYears.Add(Convert.ToString(Enums.TacticDates.StartDate), Convert.ToDateTime(objTacticRow["StartDate"]).Year);
                if (endYear - startYear > 0)
                {
                    tacticYears.Add(Convert.ToString(Enums.TacticDates.EndDate), Convert.ToDateTime(objTacticRow["EndDate"]).Year);
                }
            }
            // Set values from datatable to model
            List<BudgetModel> model = SetAllocationValuesToModel(dtCosts, PlanExchangeRate, AllocatedBy);

            model = SetPermissionForEditable(curTacticId, UserId, ClientId, model);             // Update model with view edit permissions
            model = ManageBalanceLineItemCost(model);                                           // Update model by setting balance line item costs

            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel(); //New is required due to access the property
            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();

            // Bind header of the grid
            objBudgetDHTMLXGrid = GenerateHeaderStringForInspectPopup(AllocatedBy, objBudgetDHTMLXGrid);

            // Bind final model grid data with tactic and lineitems
            objBudgetDHTMLXGrid.Grid.rows = BindFinalGridData(model, AllocatedBy);
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Added By: Arpita Soni 
        /// Date : 09/08/2016
        /// Action to Generate Header String for line item listing
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderStringForInspectPopup(string AllocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid)
        {
            objBudgetDHTMLXGrid.SetHeader = "ActivityId,Task Name,,Planned Cost";         // header values for the grid
            objBudgetDHTMLXGrid.ColAlign = objHomeGridProperty.alignleft + commaString +
                                            objHomeGridProperty.alignleft + commaString +
                                            objHomeGridProperty.aligncenter;             // alignment for the column

            objBudgetDHTMLXGrid.ColumnIds = "activityid,taskname,icons,plannedcost";           // ids of the columns
            objBudgetDHTMLXGrid.ColType = objHomeGridProperty.typero + commaString +
                                          objHomeGridProperty.typetree + commaString +
                                          objHomeGridProperty.typero + commaString +
                                          objHomeGridProperty.typeEdn;                  // types of the columns

            objBudgetDHTMLXGrid.Width = "0,200,60,80";                                      // width of the columns
            objBudgetDHTMLXGrid.ColSorting = "na,na,na,na";                                 // sorting options for the column

            if (String.Compare(AllocatedBy, Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)], true) == 0)
            {
                objBudgetDHTMLXGrid = GenerateHeaderStringForQuarterlyAllocated(objBudgetDHTMLXGrid);
            }
            else
            {
                objBudgetDHTMLXGrid = GenerateHeaderStringForMonthlyAllocated(objBudgetDHTMLXGrid);
            }

            // Add unallocated cost column into the header
            objBudgetDHTMLXGrid.SetHeader += commaString + Common.UnallocatedBudgetLabelText;
            objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
            objBudgetDHTMLXGrid.ColumnIds += commaString + "unallocatedcost";
            objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typero;
            objBudgetDHTMLXGrid.Width += commaString + "80";
            objBudgetDHTMLXGrid.ColSorting += commaString + "na";

            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Bind header for monthly allocated
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderStringForMonthlyAllocated(BudgetDHTMLXGridModel objBudgetDHTMLXGrid)
        {
            // add each month to the header
            DateTime dt;
            foreach (string key in tacticYears.Keys) // For multi year it will bind 24 columns
            {
                for (int i = 1; i <= 12; i++)
                {
                    dt = new DateTime(tacticYears[key], i, 1);
                    objBudgetDHTMLXGrid.SetHeader += commaString + dt.ToString("MMM-yyyy").ToUpper();
                    objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                    objBudgetDHTMLXGrid.ColumnIds += commaString + dt.ToString("MMM-yyyy");
                    objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                    objBudgetDHTMLXGrid.Width += commaString + "80";
                    objBudgetDHTMLXGrid.ColSorting += commaString + "na";
                }
            }
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Bind header for quarterly allocated
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderStringForQuarterlyAllocated(BudgetDHTMLXGridModel objBudgetDHTMLXGrid)
        {
            foreach (string key in tacticYears.Keys) // For multi year it will bind 8 columns
            {
                int quarterCounter = 1;
                for (int i = 1; i <= 11; i += 3)
                {
                    objBudgetDHTMLXGrid.SetHeader += commaString + "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                    objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                    objBudgetDHTMLXGrid.ColumnIds += commaString + "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                    objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                    objBudgetDHTMLXGrid.Width += commaString + "80";
                    objBudgetDHTMLXGrid.ColSorting += commaString + "na";
                    quarterCounter++;
                }
            }
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Get monthly and quarterly allocated cost for line items and tactic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Budgetdataobj> CostAllocationForInspectPopup(BudgetModel modelLineItem, string activityType, List<Budgetdataobj> lstBudgetData, string allocatedBy, string activityId)
        {
            // Flag to indicate whether line item or tactic
            bool isLineItem = (activityType == ActivityType.ActivityLineItem) ? true : false;
            // Flag to indicate whether it is balance line item
            bool isBalance = (activityType == ActivityType.ActivityLineItem && modelLineItem.LineItemTypeId == null) ? true : false;

            lstBudgetData = GetMonthlyQuarterlyAllocatedPlannedCost(isLineItem, isBalance, modelLineItem, lstBudgetData, allocatedBy);
            return lstBudgetData;
        }

        /// <summary>
        /// Get month wise allocated cost
        /// </summary>
        private List<Budgetdataobj> GetMonthlyQuarterlyAllocatedPlannedCost(bool isLineItem, bool isOtherLineItem, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData, string allocatedBy)
        {
            double monthlyValue = 0, totalAllocatedCost = 0;
            Budgetdataobj objBudgetData;
            // Add every months/quarters cost to model object bind the grid
            foreach (string month in modelEntity.MonthlyCosts.Keys)
            {
                monthlyValue = modelEntity.MonthlyCosts[month];
                objBudgetData = new Budgetdataobj();
                // Set editable based on permission (balance always remain read only)
                if (!modelEntity.isEditable || modelEntity.LineItemTypeId == null)
                {
                    objBudgetData.locked = objHomeGridProperty.lockedstateone;
                    objBudgetData.style = objHomeGridProperty.stylecolorgray;
                }
                else
                {
                    objBudgetData.locked = objHomeGridProperty.lockedstatezero;
                    objBudgetData.style = objHomeGridProperty.stylecolorblack;
                }

                objBudgetData.value = monthlyValue.ToString(formatThousand);
                lstBudgetData.Add(objBudgetData);

                totalAllocatedCost += monthlyValue;
            }
            // Add Unallocated Cost column to the grid
            lstBudgetData.Add(AddUnallocatedCostColumnToGrid(modelEntity.PlannedCost, totalAllocatedCost, formatThousand));

            return lstBudgetData;
        }

        /// <summary>
        /// Add unallocated cost column to the grid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Budgetdataobj AddUnallocatedCostColumnToGrid(double PlannedCost, double totalAllocatedCost, string formatThousand)
        {
            // Add Unallocated Cost column at the end
            Budgetdataobj objBudgetData = new Budgetdataobj();
            objBudgetData.locked = objHomeGridProperty.lockedstateone;
            objBudgetData.style = objHomeGridProperty.stylecolorgray;
            objBudgetData.value = (PlannedCost - totalAllocatedCost).ToString(formatThousand);
            return objBudgetData;
        }

        /// <summary>
        /// Bind model for final data with tactic and line items
        /// </summary>
        private List<BudgetDHTMLXGridDataModel> BindFinalGridData(List<BudgetModel> model, string AllocatedBy)
        {
            List<BudgetDHTMLXGridDataModel> lstTacticRows = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel objTacticRows = new BudgetDHTMLXGridDataModel();
            // Get tactic model 
            BudgetModel tacticModel = model.Where(p => p.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();

            objTacticRows = new BudgetDHTMLXGridDataModel();    // Add row for tactic into the model
            objTacticRows.id = ActivityType.ActivityTactic + HttpUtility.HtmlEncode(tacticModel.ActivityId);
            objTacticRows.open = "1";   // open = 1 means for this node child nodes will be expanded
            objTacticRows.bgColor = objHomeGridProperty.TacticBackgroundColor;

            List<Budgetdataobj> lstTacticData = new List<Budgetdataobj>();

            Budgetdataobj objTacticData = new Budgetdataobj();  // Add Activity Id column value
            objTacticData.value = tacticModel.ActivityId.Replace("cpt_", "");
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();    // Add Activity Name column value
            objTacticData.value = HttpUtility.HtmlEncode(tacticModel.ActivityName).Replace("'", "&#39;");   // HttpUtility.HtmlEncode handles all character except ' so need to be replaced
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();    // Add Icons column value
            objTacticData.value = Convert.ToString(BindIconsForTactic(tacticModel));
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();    // Add Planned Cost column value
            objTacticData.value = tacticModel.PlannedCost.ToString(formatThousand);
            objTacticData.locked = tacticModel.isEditable ? objHomeGridProperty.lockedstatezero : objHomeGridProperty.lockedstateone;
            objTacticData.style = tacticModel.isEditable ? objHomeGridProperty.stylecolorblack : objHomeGridProperty.stylecolorgray;
            lstTacticData.Add(objTacticData);

            // Add allocated cost data based on monthly/quarterly
            lstTacticData = CostAllocationForInspectPopup(tacticModel, ActivityType.ActivityTactic, lstTacticData, AllocatedBy, tacticModel.ActivityId);

            objTacticRows.data = lstTacticData; // assigning all columns data to row object
            List<BudgetDHTMLXGridDataModel> lstLineItemRows = new List<BudgetDHTMLXGridDataModel>();

            // Need to re-arrange order as balance line item always display at the end
            BudgetModel balanceLineItem = model.Where(p => p.ActivityType == ActivityType.ActivityLineItem && p.LineItemTypeId == null).FirstOrDefault();
            model = model.Where(p => p.ActivityType == ActivityType.ActivityLineItem && p.LineItemTypeId != null).OrderBy(p => p.ActivityName).ToList();
            if (balanceLineItem != null)
            {
                model.Add(balanceLineItem);
            }

            foreach (BudgetModel bml in model.Where(p => p.ActivityType == ActivityType.ActivityLineItem))
            {
                lstLineItemRows.Add(BindGridJsonForLineItems(bml.Id, bml.ActivityName, bml.ActivityId, model, AllocatedBy));
            }
            objTacticRows.rows = lstLineItemRows;
            lstTacticRows.Add(objTacticRows);
            return lstTacticRows;
        }

        /// <summary>
        /// Bind icons column for tactic
        /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        private StringBuilder BindIconsForTactic(BudgetModel Entity)
        {
            StringBuilder strIconsData = new StringBuilder();
            //LinkTactic Permission based on Entity Year
            bool LinkTacticPermission = ((Entity.EndDate.Year - Entity.StartDate.Year) > 0) ? true : false;
            string LinkedTacticId = Entity.LinkTacticId == 0 ? "null" : Convert.ToString(Entity.LinkTacticId);

            // Magnifying Glass to open Inspect Popup
            strIconsData.Append("<div class=grid_Search id=TP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>");

            // Add Button
            strIconsData.Append("<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Tactic alt=__" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId));
            strIconsData.Append(" per=true LinkTacticper =" + Convert.ToString(LinkTacticPermission) + " LinkedTacticId = " + Convert.ToString(LinkedTacticId));
            strIconsData.Append(" tacticaddId=" + Convert.ToString(Entity.ActivityId) + "><i class='fa fa-plus-circle' aria-hidden='true'></i></div>");

            return strIconsData;
        }

        /// <summary>
        /// Bind icons column for line item
        /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder BindIconsForLineItem(BudgetModel Entity)
        {
            StringBuilder strIconsData = new StringBuilder();

            // Magnifying Glass to open Inspect Popup
            strIconsData.Append("<div class=grid_Search id=LP title=View ><i class='fa fa-external-link-square' aria-hidden='true'></i></div>");

            // Add Button
            strIconsData.Append("<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Line alt=___" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId));
            strIconsData.Append(" lt=" + ((Entity.LineItemTypeId == null) ? 0 : Entity.LineItemTypeId) + " per=true");
            strIconsData.Append(" dt=" + Convert.ToString(Entity.ActivityName) + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>");
            return strIconsData;
        }

        /// <summary>
        /// Bind data for each line item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BudgetDHTMLXGridDataModel BindGridJsonForLineItems(string LineItemId, string ActivityName, string ActivityId, List<BudgetModel> model, string AllocatedBy)
        {
            BudgetDHTMLXGridDataModel objLineItemRows = new BudgetDHTMLXGridDataModel();

            objLineItemRows.id = ActivityType.ActivityLineItem + HttpUtility.HtmlEncode(ActivityId);
            objLineItemRows.open = null;
            objLineItemRows.bgColor = "#fff";
            List<Budgetdataobj> lstLineItemData = new List<Budgetdataobj>();
            Budgetdataobj objLineItemData = new Budgetdataobj();
            BudgetModel modelEntity = model.Where(pl => pl.ActivityType == ActivityType.ActivityLineItem && pl.ActivityId == ActivityId).ToList().FirstOrDefault();

            objLineItemData.value = LineItemId; // Add LineItem Id to the column
            lstLineItemData.Add(objLineItemData);

            objLineItemData = new Budgetdataobj();  // Add Activity Name to the column
            if (modelEntity.LineItemTypeId == null)
            {
                objLineItemData.locked = objHomeGridProperty.lockedstateone; // Balance row name should not be editable
                objLineItemData.style = objHomeGridProperty.stylecolorgray;
            }
            objLineItemData.value = HttpUtility.HtmlEncode(ActivityName).Replace("'", "&#39;");
            lstLineItemData.Add(objLineItemData);

            objLineItemData = new Budgetdataobj();  // Add Icons to the column
            objLineItemData.value = modelEntity.LineItemTypeId != null ? Convert.ToString(BindIconsForLineItem(modelEntity)) : "";
            lstLineItemData.Add(objLineItemData);

            objLineItemData = new Budgetdataobj();  // Add Planned Cost to the column
            objLineItemData.value = modelEntity.PlannedCost.ToString(formatThousand);
            if (modelEntity.LineItemTypeId == null)
            {
                objLineItemData.locked = objHomeGridProperty.lockedstateone;    // Balance row name should not be editable
                objLineItemData.style = objHomeGridProperty.stylecolorgray;
            }
            lstLineItemData.Add(objLineItemData);

            lstLineItemData = CostAllocationForInspectPopup(modelEntity, ActivityType.ActivityLineItem, lstLineItemData, AllocatedBy, ActivityId);

            objLineItemRows.data = lstLineItemData;
            return objLineItemRows;
        }

        /// <summary>
        /// Set permission for tactic and line items 
        /// </summary>
        private List<BudgetModel> SetPermissionForEditable(int curTacticId, int UserId, int ClientId, List<BudgetModel> model)
        {
            List<int> lstSubordinatesIds = new List<int>();
            string EntityTypeTactic = Convert.ToString(Enums.EntityType.Tactic);
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(UserId);
            }
            // Get list of custom fields for tactic
            List<CustomField_Entity> CstFields = objDbMrpEntities.CustomField_Entity.Where(entity => entity.EntityId == curTacticId && entity.CustomField.EntityType.Equals(EntityTypeTactic)).ToList();
            //Assign respective customfields to tactic
            foreach (BudgetModel Tactic in model.Where(m => m.ActivityType == ActivityType.ActivityTactic).ToList())
            {
                int tempTacticId = Convert.ToInt32(Tactic.Id);
                Tactic.CustomFieldEntities = CstFields.Where(entity => entity.EntityId == tempTacticId).ToList();
            }

            string DropDownList = Convert.ToString(Enums.CustomFieldType.DropDownList);

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Convert.ToString(Enums.EntityType.Tactic), ClientId);
            bool CustomFieldexists = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == ClientId &&
                                                                        customfield.EntityType.Equals(EntityTypeTactic) &&
                                                                        customfield.IsRequired &&
                                                                        customfield.IsDeleted.Equals(false)).Any();

            // Get list of entities
            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => entityid.EntityId == curTacticId).Select(entityid => entityid).ToList();

            // Get list of all custom fields of type drop down list
            List<CustomField_Entity> lstAllTacticCustomFieldEntities = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == ClientId &&
                                                                                                        customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                                                                                                        customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                                                                                                        customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        customFieldEntity.EntityId == curTacticId)
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList();
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(UserId, true);
            foreach (BudgetModel item in model)
            {
                #region Set Permission For Tactic
                if (item.ActivityType == ActivityType.ActivityTactic)
                {
                    // check permission for tactic
                    if (item.CreatedBy == UserId || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
                #endregion

                #region Set Permission for LineItem
                else if (item.ActivityType == ActivityType.ActivityLineItem)
                {
                    // check permission for line items
                    int tacticOwner = 0;
                    List<BudgetModel> tempModel = model.Where(m => m.ActivityId == item.ParentActivityId).ToList();
                    if (tempModel.Any())
                    {
                        tacticOwner = tempModel.FirstOrDefault().CreatedBy;
                    }

                    if (item.CreatedBy == UserId || tacticOwner == UserId || lstSubordinatesIds.Contains(tacticOwner))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ParentActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(UserId, ClientId, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
                        if (lstAllowedEntityIds.Count == planTacticIds.Count)
                        {
                            item.isEditable = true;
                        }
                        else
                        {
                            item.isEditable = false;
                        }
                    }
                }
                #endregion
            }
            return model;
        }

        /// <summary>
        /// Bind allocated cost values to the model
        /// </summary>
        private List<BudgetModel> SetAllocationValuesToModel(DataTable dtCosts, double PlanExchangeRate, string allocatedBy)
        {
            List<BudgetModel> model = dtCosts.AsEnumerable().Select(row => new BudgetModel
            {
                Id = Convert.ToString(row["Id"]),   // Id of the tactic or line item
                ActivityId = Convert.ToString(row["ActivityId"]),   // Activity id for the entity
                ParentActivityId = Convert.ToString(row["ParentActivityId"]),  // Activity Id of parent entity
                ActivityName = Convert.ToString(row["ActivityName"]),   // Title of the activity
                PlannedCost = row["Cost"] != DBNull.Value ? objCurrency.GetValueByExchangeRate(Convert.ToDouble(row["Cost"]),PlanExchangeRate) : 0,  // Planned cost for the entity
                StartDate = Convert.ToDateTime(row["StartDate"]),   // tactic start date
                EndDate = Convert.ToDateTime(row["EndDate"]),       // tactic end date
                MonthlyCosts = SetMonthlyCostsToModel(row, allocatedBy,PlanExchangeRate),   // list of months/quarters cost based on allocated by
                CreatedBy = row["CreatedBy"] == DBNull.Value ? 0: Convert.ToInt32(row["CreatedBy"]),  // owner of the entity
                isEditable = false,  // set default permission false
                ActivityType = Convert.ToString(row["ActivityType"]),
                LineItemTypeId = row["LineItemTypeId"] == DBNull.Value ? 0: Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"])),
                LinkTacticId = row["LinkTacticId"] == DBNull.Value ? 0: Convert.ToInt32(row["LinkTacticId"]),
            }).ToList();
            return model;
        }

        /// <summary>
        /// Generate dictionary with cost and respective months including multi year
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<string, double> SetMonthlyCostsToModel(DataRow row, string allocatedBy, double PlanExchangeRate)
        {
            Dictionary<string, double> dictMonthlyCosts = new Dictionary<string, double>();
            string Period = string.Empty;   // variable for month name text to get cost value e.g. Y13
            int baseyear = 0;

            if (String.Compare(allocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters)) == 0)
            {
                int quarterCounter = 0;
                double SumOfMonths = 0;
                string QuarterLabel = string.Empty;
                List<string> QuartersList;
                foreach (string key in tacticYears.Keys) // For multi year it will bind 8 columns for quarters
                {
                    quarterCounter = 1;
                    for (int i = 1; i <= Convert.ToInt32(Enums.QuarterMonthDigit.Month); i = i + 3)
                    {
                        SumOfMonths = 0;
                        Period = Common.PeriodPrefix + Convert.ToString(i + baseyear);      // For Q1-2017 -> Y13
                        QuartersList = Common.GetMonthsOfQuarters(i + baseyear);            // Get months for the quarter
                        QuartersList.ForEach(x => SumOfMonths = SumOfMonths + Convert.ToDouble(row[x]));    // Summed up months values
                        QuarterLabel = "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                        dictMonthlyCosts.Add(QuarterLabel, objCurrency.GetValueByExchangeRate(SumOfMonths, PlanExchangeRate));
                        quarterCounter++;
                    }
                    baseyear = baseyear + Convert.ToInt32(Enums.QuarterMonthDigit.Month);   // Add total no. of months(12) for second year
                }
            }
            else
            {
                DateTime dtDate;
                foreach (string key in tacticYears.Keys) // For multi year it will bind 24 columns for months
                {
                    for (int i = 1; i <= Convert.ToInt32(Enums.QuarterMonthDigit.Month); i++)
                    {
                        dtDate = new DateTime(tacticYears[key], i, 1);
                        Period = Common.PeriodPrefix + Convert.ToString(i + baseyear);
                        dictMonthlyCosts.Add(dtDate.ToString("MMM-yyyy"), objCurrency.GetValueByExchangeRate(Convert.ToDouble(row[Period]),PlanExchangeRate));   // For JAN-2017 -> Y13
                    }
                    baseyear = baseyear + Convert.ToInt32(Enums.QuarterMonthDigit.Month);   // Add total no. of months(12) for second year
                }
            }
            return dictMonthlyCosts;    // Returns dictionary of months/quarters and its values
        }

        /// <summary>
        /// Update balance line item cost for the tactic
        /// </summary>
        public double UpdateBalanceLineItemCost(int PlanTacticId)
        {
            List<Plan_Campaign_Program_Tactic_LineItem> lstLineItems = new List<Plan_Campaign_Program_Tactic_LineItem>();
            double tacticTotalLineItemsCost = 0;
            lstLineItems = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem =>
                                                                lineItem.PlanTacticId == PlanTacticId &&
                                                                lineItem.IsDeleted == false &&
                                                                lineItem.LineItemTypeId != null).ToList();
            if (lstLineItems.Any())
            {
                // Get total line item cost
                tacticTotalLineItemsCost = lstLineItems.Sum(x => x.Cost);
            }
            // Get cost of balance line item
            Plan_Campaign_Program_Tactic_LineItem objOtherLineItem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem =>
                                                                            lineItem.PlanTacticId == PlanTacticId &&
                                                                            lineItem.LineItemTypeId == null &&
                                                                            lineItem.IsDeleted == false).FirstOrDefault();
            if (objOtherLineItem != null)
            {
                objOtherLineItem.Cost = objOtherLineItem.Plan_Campaign_Program_Tactic.Cost - tacticTotalLineItemsCost;
                objDbMrpEntities.Entry(objOtherLineItem).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
                return objOtherLineItem.Cost;
            }
            return 0;
        }

        #region Functions related to save line item cost allocation

        /// <summary>
        /// Save total planned cost for line item
        /// </summary>
        public void SaveTotalLineItemCost(int EntityId, double newCost)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                    pcpt.PlanLineItemId == EntityId &&
                                                                                    pcpt.IsDeleted == false).FirstOrDefault();
            if (objLineitem != null)
            {
                // Update total cost for line item
                SaveLineItemTotalPlannedCost(objLineitem, newCost);
                // Update total cost for linked line item
                if (objLineitem.LinkedLineItemId != null)
                {
                    int LinkedLineItemId = Convert.ToInt32(objLineitem.LinkedLineItemId);
                    objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                    pcpt.PlanLineItemId == LinkedLineItemId &&
                                                                                    pcpt.IsDeleted == false).FirstOrDefault();
                    if (objLineitem != null)
                    {
                        SaveLineItemTotalPlannedCost(objLineitem, newCost);
                    }
                }
            }
        }

        /// <summary>
        /// Save total planned cost to database
        /// </summary>
        private void SaveLineItemTotalPlannedCost(Plan_Campaign_Program_Tactic_LineItem objLineItem, double newCost)
        {
            objLineItem.Cost = newCost;
            objDbMrpEntities.Entry(objLineItem).State = EntityState.Modified;
            objDbMrpEntities.SaveChanges();
            UpdateBalanceLineItemCost(objLineItem.PlanTacticId);
        }

        /// <summary>
        /// Save monthly line item cost 
        /// </summary>
        public bool SaveLineItemCostMonth(Plan_Campaign_Program_Tactic_LineItem objLineItem, int EntityId, double newCost, string month, int UserId)
        {
            int startDateYear = objLineItem.Plan_Campaign_Program_Tactic.StartDate.Year;
            int endDateYear = objLineItem.Plan_Campaign_Program_Tactic.EndDate.Year;
            bool isSourceLineItem = false;
            if (startDateYear < endDateYear)
            {
                isSourceLineItem = true;
            }
            if (!isSourceLineItem)
            {
                string[] monthYear = month.Split('-');
                if (monthYear != null && Convert.ToInt32(monthYear[1]) != endDateYear)
                {
                    return false;
                }
            }
            // Convert MAR-2017 to Y15 
            string period = GetPeriodBasedOnEditedMonth(month, startDateYear, isSourceLineItem);
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault();
            if (objLineItemCost != null)
            {
                objLineItemCost.Value = newCost;
                objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                AddNewRowLineItemCost(EntityId, period, newCost, UserId);   // Add new record for line item cost for the month
            }
            return true;
        }

        /// <summary>
        /// Get period based on edited month
        /// </summary>
        private string GetPeriodBasedOnEditedMonth(string month, int baseYear, bool isSourceEntity = false)
        {
            string[] monthYear = month.Split('-');  // split column header e.g. "JAN-2016"
            int monthNumber, yearDiff = 0;
            string period = string.Empty;
            if (monthYear != null)
            {
                if (!isSourceEntity)
                {
                    yearDiff = 0;   // For linked line item Y13 value will be in Y1 
                }
                else
                {
                    // get difference of edited month year and start year of the tactic
                    yearDiff = Convert.ToInt32(monthYear[1]) - baseYear;
                }
                monthNumber = DateTime.ParseExact(monthYear[0], "MMM", new CultureInfo("en-US")).Month;
                period = Common.PeriodPrefix + Convert.ToString((yearDiff * 12) + monthNumber);  // For MAR-2017 -> Y + ((1*12) + 3) = Y15
            }
            return period;
        }

        /// <summary>
        /// Get period based on edited month
        /// </summary>
        private int GetPeriodBasedOnEditedQuarter(string quarter, int baseYear, bool isSourceEntity = false)
        {
            string[] quarterYear = quarter.Split('-');  // split column header e.g. "JAN-2016"
            int quarterNumber, yearDiff, monthNumber = 0;
            string period = string.Empty;
            List<string> QuartersList = new List<string>();
            if (quarterYear != null)
            {
                if (!isSourceEntity)
                {
                    yearDiff = 0;   // For linked line item Y13 value will be in Y1 
                }
                else
                {
                    // get difference of edited month year and start year of the tactic
                    yearDiff = Convert.ToInt32(quarterYear[1]) - baseYear;
                }
                quarterNumber = Convert.ToInt32(quarterYear[0].Replace("Q", ""));   // For Q1 -> 1 i.e. get number of quarter

                // For Q1-2017 -> Y + ((1*12) + (1*3) - 2) = Y13
                monthNumber = (yearDiff * 12) + (quarterNumber * 3) - 2;
                period = Common.PeriodPrefix + Convert.ToString(monthNumber);       // For Q1-2017 -> Y13
            }
            return monthNumber;
        }

        /// <summary>
        /// Save cost allocation for line items and update balance line item
        /// </summary>
        public void SaveLineItemCostAllocation(int EntityId, double newCost, string strPeriod, int UserId, string AllocatedBy, bool isLinkedLineItem = false)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                pcpt.PlanLineItemId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objLineitem != null)
            {
                if (string.Compare(AllocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters), true) == 0)
                {
                    SaveLineItemCostQuarter(objLineitem, EntityId, newCost, strPeriod, UserId);   // Update quarterly cost for line item
                }
                else
                {
                    SaveLineItemCostMonth(objLineitem, EntityId, newCost, strPeriod, UserId);   // Update monthly cost for line item
                }

                // Update quarterly cost for linked line item
                if (objLineitem.LinkedLineItemId != null)
                {
                    int linkLineItemId = Convert.ToInt32(objLineitem.LinkedLineItemId);
                    objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                pcpt.PlanLineItemId == linkLineItemId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
                    if (objLineitem != null)
                    {
                        if (string.Compare(AllocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters), true) == 0)
                        {
                            SaveLineItemCostQuarter(objLineitem, linkLineItemId, newCost, strPeriod, UserId); // Update quarterly cost for linked line item
                        }
                        else
                        {
                            SaveLineItemCostMonth(objLineitem, linkLineItemId, newCost, strPeriod, UserId);   // Update monthly cost for linked line item
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save quarterly line item cost 
        /// </summary>
        private bool SaveLineItemCostQuarter(Plan_Campaign_Program_Tactic_LineItem objLineItem, int EntityId, double newCost, string quarter, int UserId)
        {
            bool isSourceLineItem = false;
            int startDateYear = objLineItem.Plan_Campaign_Program_Tactic.StartDate.Year;
            int endDateYear = objLineItem.Plan_Campaign_Program_Tactic.EndDate.Year;
            if (startDateYear < endDateYear)
            {
                isSourceLineItem = true;
            }
            if (!isSourceLineItem)
            {
                string[] quarterYear = quarter.Split('-');
                if (quarterYear != null && Convert.ToInt32(quarterYear[1]) != endDateYear)
                {
                    return false;
                }
            }

            // Convert Q1-2017 to Y15 
            int monthNumber = GetPeriodBasedOnEditedQuarter(quarter, startDateYear, isSourceLineItem);
            string period = Common.PeriodPrefix + Convert.ToString(monthNumber);
            List<string> QuartersList = Common.GetMonthsOfQuarters(monthNumber);

            if (QuartersList != null)
            {
                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemCost = objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => QuartersList.Contains(pcptc.Period)).ToList();
                if (lstLineItemCost.Any())
                {
                    double oldCost = lstLineItemCost.Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (oldCost < newCost)
                    {
                        IncreaseQuarterlyLineItemCost(objLineItem, EntityId, period, newCost, oldCost, UserId, QuartersList);
                    }
                    // If new value is lesser then subtract from last month
                    else if (oldCost > newCost)
                    {
                        DecreaseQuarterlyLineItemCost(objLineItem, period, newCost, oldCost, QuartersList);
                    }
                }
                else
                {
                    AddNewRowLineItemCost(EntityId, period, newCost, UserId);   // Add new record for line item cost for the quarter
                }
            }
            objDbMrpEntities.SaveChanges();
            return true;
        }

        /// <summary>
        /// Save quarterly allocated line item cost while cost is increased
        /// </summary>
        private void IncreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineItem, int EntityId, string period, double newCost, double oldCost, int UserId, List<string> QuartersList)
        {
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == QuartersList.First()).FirstOrDefault();
            // If cost is increased for quarter then added into first month e.g. Q1 -> Y1
            if (objLineItemCost != null)
            {
                objLineItemCost.Value += newCost - oldCost;
                objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                AddNewRowLineItemCost(EntityId, period, newCost, UserId);   // Add new record for line item cost for the quarter
            }
        }

        /// <summary>
        /// Save quarterly allocated line item cost while cost is decreased
        /// </summary>
        private void DecreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineitem, string period, double newCost, double oldCost, List<string> QuartersList)
        {
            QuartersList.Reverse(); // Reversed list to subtract from last months
            double curPeriodVal = 0, needToSubtract = 0;
            needToSubtract = oldCost - newCost;

            // Subtract cost from each months of the quarter e.g. For Q1 -> subtract from Y3, Y2 and Y1 as per requirement
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost;
            foreach (string quarter in QuartersList)
            {
                objLineItemCost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault();
                if (objLineItemCost != null)
                {
                    curPeriodVal = objLineItemCost.Value;
                    curPeriodVal = curPeriodVal - needToSubtract;
                    needToSubtract = -curPeriodVal;
                    objLineItemCost.Value = curPeriodVal < 0 ? 0 : curPeriodVal;
                    if (curPeriodVal >= 0)
                    {
                        objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
                        break;  // break when quarterly allocated value subtracted from months
                    }
                    objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
                }
            }
            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Add new row for allocated line item cost
        /// </summary>
        private void AddNewRowLineItemCost(int EntityId, string period, double newCost, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
            objLineItemCost.PlanLineItemId = EntityId;
            objLineItemCost.Period = period;
            objLineItemCost.Value = newCost;
            objLineItemCost.CreatedBy = UserId;
            objLineItemCost.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        #endregion

        #region Functions related to save tactic cost allocation

        /// <summary>
        /// Save total planned cost for tactic
        /// </summary>
        public void SaveTotalTacticCost(int EntityId, double newCost)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                    pcpt.PlanTacticId == EntityId &&
                                                                                    pcpt.IsDeleted == false).FirstOrDefault();
            if (objTactic != null)
            {
                // Update total cost for tactic
                SaveTacticTotalPlannedCost(objTactic, newCost);
                // Update total cost for linked tactic
                if (objTactic.LinkedTacticId != null)
                {
                    int LinkedTacitcId = Convert.ToInt32(objTactic.LinkedTacticId);
                    objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                    pcpt.PlanTacticId == EntityId &&
                                                                                    pcpt.IsDeleted == false).FirstOrDefault();
                    SaveTacticTotalPlannedCost(objTactic, newCost);
                }
            }
        }

        /// <summary>
        /// Save total planned cost to database
        /// </summary>
        private void SaveTacticTotalPlannedCost(Plan_Campaign_Program_Tactic objTactic, double newCost)
        {
            objTactic.Cost = newCost;
            objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
            objDbMrpEntities.SaveChanges();
            UpdateBalanceLineItemCost(objTactic.PlanTacticId);
        }

        /// <summary>
        /// Save allocated cost for tactic and update balance line item
        /// </summary>
        public void SaveTacticCostAllocation(int EntityId, double newCost, string strPeriod, int UserId, string AllocatedBy, bool isLinkedTactic = false)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objTactic != null)
            {
                if (string.Compare(AllocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters), true) == 0)
                {
                    SaveTacticCostQuarter(objTactic, EntityId, newCost, strPeriod, UserId);   // Update quarterly cost for line item
                }
                else
                {
                    SaveTacticCostMonth(objTactic, EntityId, newCost, strPeriod, UserId);   // Update monthly cost for line item
                }

                // Update quarterly cost for linked line item
                if (objTactic.LinkedTacticId != null)
                {
                    int linkTacticId = Convert.ToInt32(objTactic.LinkedTacticId);
                    objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == linkTacticId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
                    if (objTactic != null)
                    {
                        if (string.Compare(AllocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters), true) == 0)
                        {
                            SaveTacticCostQuarter(objTactic, linkTacticId, newCost, strPeriod, UserId); // Update quarterly cost for linked line item
                        }
                        else
                        {
                            SaveTacticCostMonth(objTactic, linkTacticId, newCost, strPeriod, UserId);   // Update monthly cost for linked line item
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save monthly allocated cost for tactic
        /// </summary>
        private bool SaveTacticCostMonth(Plan_Campaign_Program_Tactic objTactic, int EntityId, double newCost, string month, int UserId)
        {
            int startDateYear = objTactic.StartDate.Year;
            int endDateYear = objTactic.EndDate.Year;
            bool isSourceLineItem = false;
            if (startDateYear < endDateYear)
            {
                isSourceLineItem = true;
            }
            if (!isSourceLineItem)
            {
                string[] monthYear = month.Split('-');
                if (monthYear != null && Convert.ToInt32(monthYear[1]) != endDateYear)
                {
                    return false;
                }
            }
            // Convert MAR-2017 to Y15 
            string period = GetPeriodBasedOnEditedMonth(month, startDateYear, isSourceLineItem);
            Plan_Campaign_Program_Tactic_Cost objTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault();
            if (objTacCost != null)
            {
                objTacCost.Value = newCost;
                objDbMrpEntities.Entry(objTacCost).State = EntityState.Modified;
            }
            else
            {
                AddNewRowTacticCost(EntityId, period, newCost, UserId); // Add new record for tactic cost for the month
            }
            objDbMrpEntities.SaveChanges();
            return true;
        }

        /// <summary>
        /// Save quarterly allocated cost for tactic 
        /// </summary>
        public bool SaveTacticCostQuarter(Plan_Campaign_Program_Tactic objTactic, int EntityId, double newCost, string quarter, int UserId, bool isLinkedTactic = false)
        {
            int startDateYear = objTactic.StartDate.Year;
            int endDateYear = objTactic.EndDate.Year;
            bool isSourceLineItem = false;
            if (startDateYear < endDateYear)
            {
                isSourceLineItem = true;
            }
            if (!isSourceLineItem)
            {
                string[] quarterYear = quarter.Split('-');
                if (quarterYear != null && Convert.ToInt32(quarterYear[1]) != endDateYear)
                {
                    return false;
                }
            }
            // Convert Q1-2017 to Y15 
            int monthNumber = GetPeriodBasedOnEditedQuarter(quarter, startDateYear);
            string period = Common.PeriodPrefix + Convert.ToString(monthNumber);
            List<string> QuartersList = Common.GetMonthsOfQuarters(monthNumber);

            if (QuartersList != null)
            {
                List<Plan_Campaign_Program_Tactic_Cost> lstTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => QuartersList.Contains(pcptc.Period)).ToList();
                if (lstTacCost.Any())
                {
                    double oldCost = lstTacCost.Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (oldCost < newCost)
                    {
                        IncreaseQuarterlyTacticCost(objTactic, EntityId, period, newCost, oldCost, UserId, QuartersList);
                    }
                    // If new value is lesser then subtract from last month
                    else if (oldCost > newCost)
                    {
                        DecreaseQuarterlyTacticCost(objTactic, period, newCost, oldCost, QuartersList);
                    }
                }
                else
                {
                    AddNewRowTacticCost(EntityId, period, newCost, UserId); // Add new record for tactic cost for the quarter
                }
            }
            objDbMrpEntities.SaveChanges();
            return true;
        }

        /// <summary>
        /// Save quarterly allocated tactic cost while cost is increased
        /// </summary>
        private void IncreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, int EntityId, string period, double newCost, double oldCost, int UserId, List<string> QuartersList)
        {
            Plan_Campaign_Program_Tactic_Cost objTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == QuartersList.First()).FirstOrDefault();
            if (objTacCost != null)
            {
                objTacCost.Value += newCost - oldCost;
                objDbMrpEntities.Entry(objTacCost).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                AddNewRowTacticCost(EntityId, period, newCost, UserId); // Add new record for tactic cost for the quarter
            }
        }

        /// <summary>
        /// Save quarterly allocated tactic cost while cost is decreased
        /// </summary>
        private void DecreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, string period, double newCost, double oldCost, List<string> QuartersList)
        {
            QuartersList.Reverse(); // Reversed list for subtract from last months
            double curPeriodVal = 0, costDiff = 0;
            costDiff = oldCost - newCost;
            Plan_Campaign_Program_Tactic_Cost objTacCost = new Plan_Campaign_Program_Tactic_Cost();
            // Subtract cost from each months of the quarter
            foreach (string quarter in QuartersList)
            {
                objTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault();
                if (objTacCost != null)
                {
                    curPeriodVal = objTacCost.Value;
                    curPeriodVal = curPeriodVal - costDiff;
                    costDiff = -curPeriodVal;
                    objTacCost.Value = curPeriodVal < 0 ? 0 : curPeriodVal;
                    if (curPeriodVal >= 0)
                    {
                        objDbMrpEntities.Entry(objTacCost).State = EntityState.Modified;
                        break;
                    }
                    objDbMrpEntities.Entry(objTacCost).State = EntityState.Modified;
                }
            }

            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Add new record for allocated tactic cost 
        /// </summary>
        private void AddNewRowTacticCost(int EntityId, string period, double newCost, int UserId)
        {
            Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
            objTacticCost.PlanTacticId = EntityId;
            objTacticCost.Period = period;
            objTacticCost.Value = newCost;
            objTacticCost.CreatedBy = UserId;
            objTacticCost.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objTacticCost).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        #endregion

        /// <summary>
        /// Manage lines items if cost is allocated to other
        /// </summary>
        private List<BudgetModel> ManageBalanceLineItemCost(List<BudgetModel> model)
        {
            // Get tactic from the model
            BudgetModel tacticModel = model.Where(l => l.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();
            if (tacticModel != null)
            {
                // Get all line items from the model
                List<BudgetModel> lineItems = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.LineItemTypeId != null).ToList();
                // Get balance line item from the model
                BudgetModel balanceLineItem = model.Where(bal => bal.ActivityType == ActivityType.ActivityLineItem && bal.LineItemTypeId == null).FirstOrDefault();
                if (balanceLineItem != null)
                {
                    // Deduct line item monthly cost from tactic monthly cost for balance
                    if (lineItems != null && lineItems.Count > 0)
                    {
                        List<string> lstMonthKeys = balanceLineItem.MonthlyCosts.Keys.ToList();
                        foreach (string month in lstMonthKeys)
                        {
                            balanceLineItem.MonthlyCosts[month] = tacticModel.MonthlyCosts[month] - lineItems.Sum(lmon => (double?)lmon.MonthlyCosts[month]) ?? 0;
                        }
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == tacticModel.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthlyCosts = balanceLineItem.MonthlyCosts;
                    }
                    // If no line items then assign tactic costs to balance
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == tacticModel.ActivityId && line.LineItemTypeId == null).FirstOrDefault().MonthlyCosts = tacticModel.MonthlyCosts;
                    }
                }
            }
            return model;
        }
    }
}
