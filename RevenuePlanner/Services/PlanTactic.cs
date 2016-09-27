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
    public class PlanTactic : IPlanTactic
    {
        private MRPEntities objDbMrpEntities;
        public PlanTactic()
        {
            objDbMrpEntities = new MRPEntities();
        }

        private Dictionary<string, string> MonthQuarterList = new Dictionary<string, string>() {
                {Enums.Months.January.ToString(), "Y1" },
                {Enums.Months.February.ToString(), "Y2" },
                {Enums.Months.March.ToString(), "Y3" },
                {Enums.Months.April.ToString(), "Y4" },
                {Enums.Months.May.ToString(), "Y5" },
                {Enums.Months.June.ToString(), "Y6" },
                {Enums.Months.July.ToString(), "Y7" },
                {Enums.Months.August.ToString(), "Y8" },
                {Enums.Months.September.ToString(), "Y9" },
                {Enums.Months.October.ToString(), "Y10" },
                {Enums.Months.November.ToString(), "Y11" },
                {Enums.Months.December.ToString(), "Y12" },
                {"Quarter 1", "Y1" },
                {"Quarter 2", "Y4" },
                {"Quarter 3", "Y7" },
                {"Quarter 4", "Y10" }
                };

        private Dictionary<string, List<string>> QuartersList = new Dictionary<string, List<string>>()
        {
                {"Y1", new List<string>{"Y1","Y2","Y3"} },
                {"Y4", new List<string>{"Y4","Y5","Y6"} },
                {"Y7", new List<string>{"Y7","Y8","Y9"} },
                {"Y10", new List<string>{"Y10","Y11","Y12"} }
        };

        /// <summary>
        /// Add Balance line item whether cost is 0 or greater
        /// </summary>
        /// <param name="tacticId"></param>
        /// <param name="Cost"></param>
        /// <param name="UserId"></param>
        public void AddBalanceLineItem(int tacticId, double Cost, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem objNewLineitem = new Plan_Campaign_Program_Tactic_LineItem();
            objNewLineitem.PlanTacticId = tacticId;
            objNewLineitem.Title = Common.LineItemTitleDefault;
            objNewLineitem.Cost = Cost;
            objNewLineitem.Description = string.Empty;
            objNewLineitem.CreatedBy = UserId;
            objNewLineitem.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objNewLineitem).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Get cost allocation for line items in tactic inspect popup
        /// </summary>
        /// <param name="LineItemId"></param>
        /// <returns></returns>
        public BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId)
        {
            #region Variables
            string AllocatedBy = Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.defaults.ToString()].ToString().ToLower();
            List<BudgetModel> model = new List<BudgetModel>();
            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel();
            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();
            #endregion

            // Get allocated by based on Plan
            int PlanId = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(x => x.PlanTacticId == curTacticId).FirstOrDefault().Plan_Campaign_Program.Plan_Campaign.PlanId;
            Plan objPlan = objDbMrpEntities.Plans.FirstOrDefault(_pln => _pln.PlanId.Equals(PlanId));
            AllocatedBy = objPlan.AllocatedBy;

            // Get cost allocation and set values to model
            DataTable dtCosts = Common.GetLineItemCostAllocation(curTacticId);
            model = SetAllocationValuesToModel(dtCosts);

            // Set view edit permissions
            SetPermissionForEditable(curTacticId, ref model);
            model = ManageLineItems(model);

            //Set actual for quarters
            if (AllocatedBy == "quarters")
            {
                SumOfMonthsForQuaterlyAllocated(ref model);
            }

            // GenerateHeader
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
        /// <param name="AllocatedBy">AllocatedBy</param>
        private BudgetDHTMLXGridModel GenerateHeaderStringForInspectPopup(string AllocatedBy, BudgetDHTMLXGridModel objBudgetDHTMLXGrid)
        {
            string setHeader = "", colType = "", width = "", colSorting = "", columnIds = "", colAlign = "";
            setHeader = "ActivityId,Task Name,Planned Cost";
            columnIds = "activityid,taskname,plannedcost";
            colAlign = "left,left,center";
            colType = "ro,tree,ed";
            width = "0,100,50";
            colSorting = "na,na,na";

            if (String.Compare(AllocatedBy, Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)], true) == 0)
            {
                GenerateHeaderStringForQuarterlyAllocated(ref setHeader, ref columnIds, ref colType, ref width, ref colSorting, ref colAlign);
            }
            else
            {
                GenerateHeaderStringForMonthlyAllocated(ref setHeader, ref columnIds, ref colType, ref width, ref colSorting, ref colAlign);
            }
            setHeader += ",Unallocated Cost";
            columnIds += ",unallocatedcost";
            colAlign += ",center";
            colType += ",ro";
            width += ",50";
            colSorting += ",na";

            objBudgetDHTMLXGrid.SetHeader = setHeader;
            objBudgetDHTMLXGrid.ColAlign = colAlign;
            objBudgetDHTMLXGrid.ColumnIds = columnIds;
            objBudgetDHTMLXGrid.ColType = colType;
            objBudgetDHTMLXGrid.Width = width;
            objBudgetDHTMLXGrid.ColSorting = colSorting;
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Bind header for monthly allocated
        /// </summary>
        /// <param name="setHeader"></param>
        /// <param name="columnIds"></param>
        /// <param name="colType"></param>
        /// <param name="width"></param>
        /// <param name="colSorting"></param>
        /// <param name="colAlign"></param>
        private void GenerateHeaderStringForMonthlyAllocated(ref string setHeader, ref string columnIds, ref string colType, ref string width, ref string colSorting, ref string colAlign)
        {
            for (int i = 1; i <= 12; i++)
            {
                DateTime dt = new DateTime(2012, i, 1);
                setHeader = setHeader + "," + dt.ToString("MMM").ToUpper();
                colAlign = colAlign + ",center";
                columnIds = columnIds + "," + dt.ToString("MMM");
                colType = colType + ",ed";
                width = width + ",40";
                colSorting = colSorting + ",str";
            }
        }

        /// <summary>
        /// Bind header for quarterly allocated
        /// </summary>
        /// <param name="setHeader"></param>
        /// <param name="columnIds"></param>
        /// <param name="colType"></param>
        /// <param name="width"></param>
        /// <param name="colSorting"></param>
        /// <param name="colAlign"></param>
        private void GenerateHeaderStringForQuarterlyAllocated(ref string setHeader, ref string columnIds, ref string colType, ref string width, ref string colSorting, ref string colAlign)
        {
            int quarterCounter = 1;
            for (int i = 1; i <= 11; i += 3)
            {
                DateTime dt = new DateTime(2012, i, 1);
                setHeader = setHeader + ",Q" + Convert.ToString(quarterCounter);
                colAlign = colAlign + ",center";
                columnIds = columnIds + ",Q" + Convert.ToString(quarterCounter);
                colType = colType + ",ed";
                width = width + ",50";
                colSorting = colSorting + ",str";
                quarterCounter++;
            }
        }

        /// <summary>
        /// Get total planned cost for line items and tactic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Budgetdataobj> TotalPlannedCostForInspectPopup(BudgetModel modelEntity, string activityType, List<Budgetdataobj> lstBudgetData, string activityId)
        {
            string formatThousand = "#,#0.##";
            string stylecolorblack = "color:#000";
            string stylecolorgray = "color:#999";
            
            if (modelEntity != null)
            {
                bool isLineItem = activityType == ActivityType.ActivityLineItem ? true : false;
                bool isOtherLineItem = activityType == ActivityType.ActivityLineItem && modelEntity.LineItemTypeId == null;
                Budgetdataobj objBudgetData = new Budgetdataobj();
                string divValue = "0";
                divValue = modelEntity.PlannedCost.ToString(formatThousand);
                if (modelEntity.isEditable && (!isLineItem || (!isOtherLineItem && isLineItem && modelEntity.LineItemTypeId != null)))
                {
                    objBudgetData.locked = "0";
                    objBudgetData.style = stylecolorblack;
                }
                else
                {
                    objBudgetData.locked = "1";
                    objBudgetData.style = stylecolorgray;
                }
                objBudgetData.value = divValue;

                lstBudgetData.Add(objBudgetData);
            }
            return lstBudgetData;
        }

        /// <summary>
        /// Get monthly and quarterly allocated cost for line items and tactic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Budgetdataobj> CostAllocationForInspectPopup(BudgetModel modelLineItem, string activityType, List<Budgetdataobj> lstBudgetData, string allocatedBy, string activityId)
        {
            Budgetdataobj objBudgetData = new Budgetdataobj();

            bool isLineItem = activityType == ActivityType.ActivityLineItem ? true : false;
            bool isOtherLineItem = activityType == ActivityType.ActivityLineItem && modelLineItem.LineItemTypeId == null;

            if (String.Compare(allocatedBy, Enums.PlanAllocatedByList[Enums.PlanAllocatedBy.quarters.ToString()], true) == 0)
            {
                string cellTextColor = "color:#000";
                lstBudgetData = GetQuarterlyAllocatedPlannedCost(objBudgetData, isLineItem, isOtherLineItem, modelLineItem, lstBudgetData, cellTextColor);
            }
            else
            {
                lstBudgetData = GetMonthlyAllocatedPlannedCost(objBudgetData, isLineItem, isOtherLineItem, modelLineItem, lstBudgetData, allocatedBy);
            }
            return lstBudgetData;
        }

        /// <summary>
        /// Get month wise allocated cost
        /// </summary>
        /// <param name="objBudgetData"></param>
        /// <param name="isLineItem"></param>
        /// <param name="isOtherLineItem"></param>
        /// <param name="modelLineItem"></param>
        /// <param name="lstBudgetData"></param>
        /// <param name="allocatedBy"></param>
        /// <returns></returns>
        private List<Budgetdataobj> GetMonthlyAllocatedPlannedCost(Budgetdataobj objBudgetData, bool isLineItem, bool isOtherLineItem, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData, string allocatedBy)
        {
            string formatThousand = "#,#0.##";
            double monthlyCampaignValue = 0, totalAllocatedCost = 0;
            string stylecolorblack = "color:#000";
            string stylecolorgray = "color:#999";
            for (int i = 1; i <= 12; i++)
            {
                if (i == 1)
                {
                    monthlyCampaignValue = modelEntity.Month.Jan;
                }
                else if (i == 2)
                {
                    monthlyCampaignValue = modelEntity.Month.Feb;
                }
                else if (i == 3)
                {
                    monthlyCampaignValue = modelEntity.Month.Mar;
                }
                else if (i == 4)
                {
                    monthlyCampaignValue = modelEntity.Month.Apr;
                }
                else if (i == 5)
                {
                    monthlyCampaignValue = modelEntity.Month.May;
                }
                else if (i == 6)
                {
                    monthlyCampaignValue = modelEntity.Month.Jun;
                }
                else if (i == 7)
                {
                    monthlyCampaignValue = modelEntity.Month.Jul;
                }
                else if (i == 8)
                {
                    monthlyCampaignValue = modelEntity.Month.Aug;
                }
                else if (i == 9)
                {
                    monthlyCampaignValue = modelEntity.Month.Sep;
                }
                else if (i == 10)
                {
                    monthlyCampaignValue = modelEntity.Month.Oct;
                }
                else if (i == 11)
                {
                    monthlyCampaignValue = modelEntity.Month.Nov;
                }
                else if (i == 12)
                {
                    monthlyCampaignValue = modelEntity.Month.Dec;
                }
                objBudgetData = new Budgetdataobj();
                if (modelEntity.isEditable && (!isLineItem || (!isOtherLineItem && isLineItem && modelEntity.LineItemTypeId != null)))
                {
                    objBudgetData.locked = "0";
                    objBudgetData.style = stylecolorblack;
                }
                else
                {
                    objBudgetData.locked = "1";
                    objBudgetData.style = stylecolorgray;
                }

                objBudgetData.value = monthlyCampaignValue.ToString(formatThousand);
                lstBudgetData.Add(objBudgetData);

                totalAllocatedCost += monthlyCampaignValue;
            }
            // Add Unallocated Cost column to the grid
            lstBudgetData.Add(AddUnallocatedCostColumnToGrid(stylecolorgray, modelEntity.PlannedCost, totalAllocatedCost, formatThousand));

            return lstBudgetData;
        }

        /// <summary>
        /// Get quarter wise allocated cost
        /// </summary>
        /// <param name="objBudgetData"></param>
        /// <param name="isLineItem"></param>
        /// <param name="isOtherLineItem"></param>
        /// <param name="modelLineItem"></param>
        /// <param name="lstBudgetData"></param>
        /// <param name="cellTextColor"></param>
        /// <returns></returns>
        private List<Budgetdataobj> GetQuarterlyAllocatedPlannedCost(Budgetdataobj objBudgetData, bool isLineItem, bool isOtherLineItem, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData, string cellTextColor)
        {
            string formatThousand = "#,#0.##";
            double monthlyCampaignValue = 0, totalAllocatedCost = 0;
            string stylecolorblack = "color:#000";
            string stylecolorgray = "color:#999";
            for (int i = 1; i <= 11; i += 3)
            {
                if (i == 1)
                {
                    monthlyCampaignValue = modelEntity.Month.Jan;
                }
                else if (i == 4)
                {
                    monthlyCampaignValue = modelEntity.Month.Apr;
                }
                else if (i == 7)
                {
                    monthlyCampaignValue = modelEntity.Month.Jul;
                }
                else if (i == 10)
                {
                    monthlyCampaignValue = modelEntity.Month.Oct;
                }
                objBudgetData = new Budgetdataobj();
                if (modelEntity.isEditable && (!isLineItem || (!isOtherLineItem && isLineItem && modelEntity.LineItemTypeId != null)))
                {
                    objBudgetData.locked = "0";
                    objBudgetData.style = stylecolorblack;
                }
                else
                {
                    objBudgetData.locked = "1";
                    objBudgetData.style = stylecolorgray;
                }
                objBudgetData.value = monthlyCampaignValue.ToString(formatThousand);
                lstBudgetData.Add(objBudgetData);
                totalAllocatedCost += monthlyCampaignValue;
            }
            // Add Unallocated Cost column to the grid
            lstBudgetData.Add(AddUnallocatedCostColumnToGrid(stylecolorgray,modelEntity.PlannedCost,totalAllocatedCost,formatThousand));

            return lstBudgetData;
        }

        /// <summary>
        /// Add unallocated cost column to the grid
        /// </summary>
        /// <param name="stylecolorgray"></param>
        /// <param name="PlannedCost"></param>
        /// <param name="totalAllocatedCost"></param>
        /// <param name="formatThousand"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Budgetdataobj AddUnallocatedCostColumnToGrid(string stylecolorgray, double PlannedCost, double totalAllocatedCost, string formatThousand)
        {
            // Add Unallocated Cost column at the end
            Budgetdataobj objBudgetData = new Budgetdataobj();
            objBudgetData.locked = "1";
            objBudgetData.style = stylecolorgray;
            objBudgetData.value = (PlannedCost - totalAllocatedCost).ToString(formatThousand);
            return objBudgetData;
        }

        /// <summary>
        /// Get summed up cost for required months in case of quarterly allocated
        /// </summary>
        /// <param name="model"></param>
        private void SumOfMonthsForQuaterlyAllocated(ref List<BudgetModel> model)
        {
            foreach (BudgetModel bm in model)
            {
                bm.Month.Jan = bm.Month.Jan + bm.Month.Feb + bm.Month.Mar;
                bm.Month.Apr = bm.Month.Apr + bm.Month.May + bm.Month.Jun;
                bm.Month.Jul = bm.Month.Jul + bm.Month.Aug + bm.Month.Sep;
                bm.Month.Oct = bm.Month.Oct + bm.Month.Nov + bm.Month.Dec;
                bm.Month.Feb = 0;
                bm.Month.Mar = 0;
                bm.Month.May = 0;
                bm.Month.Jun = 0;
                bm.Month.Aug = 0;
                bm.Month.Sep = 0;
                bm.Month.Nov = 0;
                bm.Month.Dec = 0;
            }
        }

        /// <summary>
        /// Bind model for final data with tactic and line items
        /// </summary>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        private List<BudgetDHTMLXGridDataModel> BindFinalGridData(List<BudgetModel> model, string AllocatedBy)
        {
            string TacticBackgroundColor = "#e4f1e1";
            List<BudgetDHTMLXGridDataModel> lstTacticRows = new List<BudgetDHTMLXGridDataModel>();
            BudgetDHTMLXGridDataModel objTacticRows = new BudgetDHTMLXGridDataModel();
            BudgetModel bmt = model.Where(p => p.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();

            objTacticRows = new BudgetDHTMLXGridDataModel();
            objTacticRows.id = ActivityType.ActivityTactic + HttpUtility.HtmlEncode(bmt.ActivityId);
            objTacticRows.open = "1";
            objTacticRows.bgColor = TacticBackgroundColor;
            List<Budgetdataobj> lstTacticData = new List<Budgetdataobj>();
            BudgetModel modelEntity = model.Where(pl => pl.ActivityType == ActivityType.ActivityTactic && pl.ActivityId == bmt.ActivityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();

            Budgetdataobj objTacticData = new Budgetdataobj();
            objTacticData.value = bmt.ActivityId.Replace("cpt_", "");
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();
            objTacticData.value = HttpUtility.HtmlEncode(bmt.ActivityName).Replace("'", "&#39;");
            lstTacticData.Add(objTacticData);

            lstTacticData = TotalPlannedCostForInspectPopup(modelEntity, ActivityType.ActivityTactic,
                                lstTacticData, bmt.ActivityId);

            lstTacticData = CostAllocationForInspectPopup(modelEntity, ActivityType.ActivityTactic,
                                lstTacticData, AllocatedBy, bmt.ActivityId);

            objTacticRows.data = lstTacticData;
            List<BudgetDHTMLXGridDataModel> lstLineItemRows = new List<BudgetDHTMLXGridDataModel>();

            BudgetModel balanceLineItem = model.Where(p => p.ActivityType == ActivityType.ActivityLineItem && p.LineItemTypeId == null).FirstOrDefault();
            model = model.Where(p => p.ActivityType == ActivityType.ActivityLineItem && p.LineItemTypeId != null).OrderBy(p => p.ActivityName).ToList();
            model.Add(balanceLineItem);

            foreach (BudgetModel bml in model.Where(p => p.ActivityType == ActivityType.ActivityLineItem))
            {
                lstLineItemRows.Add(BindGridJsonForLineItems(bml.Id, bml.ActivityName, bml.ActivityId, model, AllocatedBy));
            }
            objTacticRows.rows = lstLineItemRows;
            lstTacticRows.Add(objTacticRows);

            return lstTacticRows;
        }

        /// <summary>
        /// Bind data for each line item
        /// </summary>
        /// <param name="LineItemId"></param>
        /// <param name="ActivityName"></param>
        /// <param name="ActivityId"></param>
        /// <param name="model"></param>
        /// <param name="AllocatedBy"></param>
        /// <returns></returns>
        private BudgetDHTMLXGridDataModel BindGridJsonForLineItems(string LineItemId, string ActivityName, string ActivityId, List<BudgetModel> model, string AllocatedBy)
        {
            BudgetDHTMLXGridDataModel objLineItemRows = new BudgetDHTMLXGridDataModel();
            objLineItemRows.id = ActivityType.ActivityLineItem + HttpUtility.HtmlEncode(ActivityId);
            objLineItemRows.open = null;
            objLineItemRows.bgColor = "#fff";
            List<Budgetdataobj> lstLineItemData = new List<Budgetdataobj>();
            Budgetdataobj objLineItemData = new Budgetdataobj();
            BudgetModel modelEntity = model.Where(pl => pl.ActivityType == ActivityType.ActivityLineItem && pl.ActivityId == ActivityId).OrderBy(p => p.ActivityName).ToList().FirstOrDefault();

            objLineItemData.value = LineItemId;
            lstLineItemData.Add(objLineItemData);

            objLineItemData = new Budgetdataobj();
            if (modelEntity.LineItemTypeId == null)
            {
                objLineItemData.locked = "1"; // Balance row name should not be editable
                objLineItemData.style = "color:#999";
            }
            objLineItemData.value = HttpUtility.HtmlEncode(ActivityName).Replace("'", "&#39;");
            lstLineItemData.Add(objLineItemData);

            lstLineItemData = TotalPlannedCostForInspectPopup(modelEntity, ActivityType.ActivityLineItem,lstLineItemData, ActivityId);

            lstLineItemData = CostAllocationForInspectPopup(modelEntity, ActivityType.ActivityLineItem, lstLineItemData, AllocatedBy, ActivityId);

            objLineItemRows.data = lstLineItemData;
            return objLineItemRows;

        }

        /// <summary>
        /// Set permission for tactic and line items 
        /// </summary>
        /// <param name="curTacticId"></param>
        /// <param name="model"></param>
        private void SetPermissionForEditable(int curTacticId, ref List<BudgetModel> model)
        {
            List<int> lstSubordinatesIds = new List<int>();
            string EntityTypeTactic = Enums.EntityType.Tactic.ToString();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);
            }

            var CstFields = objDbMrpEntities.CustomField_Entity.Where(entity => entity.EntityId == curTacticId && entity.CustomField.EntityType.Equals(EntityTypeTactic)).ToList();
            //Assign respective customfields to tactic
            foreach (BudgetModel Tactic in model.Where(m => m.ActivityType == ActivityType.ActivityTactic).ToList())
            {
                int tempTacticId = Convert.ToInt32(Tactic.Id);
                Tactic.CustomFieldEntities = CstFields.Where(entity => entity.EntityId == tempTacticId).ToList();
            }

            string DropDownList = Enums.CustomFieldType.DropDownList.ToString();

            bool IsCustomFeildExist = Common.IsCustomFeildExist(Enums.EntityType.Tactic.ToString(), Sessions.User.CID);
            bool CustomFieldexists = objDbMrpEntities.CustomFields.Where(customfield => customfield.ClientId == Sessions.User.CID &&
                                                                        customfield.EntityType.Equals(EntityTypeTactic) &&
                                                                        customfield.IsRequired &&
                                                                        customfield.IsDeleted.Equals(false)).Any();


            List<CustomField_Entity> Entities = objDbMrpEntities.CustomField_Entity.Where(entityid => entityid.EntityId == curTacticId).Select(entityid => entityid).ToList();

            List<CustomField_Entity> lstAllTacticCustomFieldEntities = objDbMrpEntities.CustomField_Entity.Where(customFieldEntity => customFieldEntity.CustomField.ClientId == Sessions.User.CID &&
                                                                                                        customFieldEntity.CustomField.IsDeleted.Equals(false) &&
                                                                                                        customFieldEntity.CustomField.EntityType.Equals(EntityTypeTactic) &&
                                                                                                        customFieldEntity.CustomField.CustomFieldType.Name.Equals(DropDownList) &&
                                                                                                        customFieldEntity.EntityId == curTacticId)
                                                                                                .Select(customFieldEntity => customFieldEntity).Distinct().ToList(); //todo : able to move up
            List<RevenuePlanner.Models.CustomRestriction> userCustomRestrictionList = Common.GetUserCustomRestrictionsList(Sessions.User.ID, true);
            foreach (var item in model)
            {
                #region Set Permission For Tactic
                if (item.ActivityType == ActivityType.ActivityTactic)
                {
                    if (item.CreatedBy == Sessions.User.ID || lstSubordinatesIds.Contains(item.CreatedBy))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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
                    int tacticOwner = 0;
                    if (model.Where(m => m.ActivityId == item.ParentActivityId).Any())
                    {
                        tacticOwner = model.Where(m => m.ActivityId == item.ParentActivityId).FirstOrDefault().CreatedBy;
                    }

                    if (item.CreatedBy == Sessions.User.ID || tacticOwner == Sessions.User.ID || lstSubordinatesIds.Contains(tacticOwner))
                    {
                        List<int> planTacticIds = new List<int>();
                        List<int> lstAllowedEntityIds = new List<int>();
                        planTacticIds.Add(Convert.ToInt32(item.ParentActivityId.Replace("cpt_", "")));
                        lstAllowedEntityIds = Common.GetEditableTacticListPO(Sessions.User.ID, Sessions.User.CID, planTacticIds, IsCustomFeildExist, CustomFieldexists, Entities, lstAllTacticCustomFieldEntities, userCustomRestrictionList, false);
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
        }

        /// <summary>
        /// Bind allocated cost values to the model
        /// </summary>
        /// <param name="dtCosts"></param>
        /// <returns></returns>
        private List<BudgetModel> SetAllocationValuesToModel(DataTable dtCosts)
        {
            List<BudgetModel> model = new List<BudgetModel>();
            double PlanExchangeRate = Sessions.PlanExchangeRate;
            RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();

            model = dtCosts.AsEnumerable().Select(row => new BudgetModel
            {
                Id = Convert.ToString(row["Id"]),
                ActivityId = Convert.ToString(row["ActivityId"]),
                ParentActivityId = Convert.ToString(row["ParentActivityId"]),
                ActivityName = Convert.ToString(row["ActivityName"]),
                PlannedCost = row["Cost"] != null ? Convert.ToDouble(row["Cost"]) : 0,
                Month = new BudgetMonth()
                {
                    Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY1"].ToString()), PlanExchangeRate),
                    Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY2"].ToString()), PlanExchangeRate),
                    Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY3"].ToString()), PlanExchangeRate),
                    Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY4"].ToString()), PlanExchangeRate),
                    May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY5"].ToString()), PlanExchangeRate),
                    Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY6"].ToString()), PlanExchangeRate),
                    Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY7"].ToString()), PlanExchangeRate),
                    Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY8"].ToString()), PlanExchangeRate),
                    Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY9"].ToString()), PlanExchangeRate),
                    Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY10"].ToString()), PlanExchangeRate),
                    Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY11"].ToString()), PlanExchangeRate),
                    Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY12"].ToString()), PlanExchangeRate)
                },
                ParentMonth = row["ActivityType"].ToString() == ActivityType.ActivityLineItem.ToString() ? new BudgetMonth()
                {
                    Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY1"].ToString()), PlanExchangeRate),
                    Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY2"].ToString()), PlanExchangeRate),
                    Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY3"].ToString()), PlanExchangeRate),
                    Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY4"].ToString()), PlanExchangeRate),
                    May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY5"].ToString()), PlanExchangeRate),
                    Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY6"].ToString()), PlanExchangeRate),
                    Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY7"].ToString()), PlanExchangeRate),
                    Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY8"].ToString()), PlanExchangeRate),
                    Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY9"].ToString()), PlanExchangeRate),
                    Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY10"].ToString()), PlanExchangeRate),
                    Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY11"].ToString()), PlanExchangeRate),
                    Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(row["CY12"].ToString()), PlanExchangeRate)
                } : null,
                CreatedBy = Convert.ToInt32(row["CreatedBy"]),
                isEditable = Convert.ToBoolean(row["IsEditable"]),
                ActivityType = row["ActivityType"].ToString(),
                LineItemTypeId = Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"]))
            }).ToList();

            return model;
        }

        /// <summary>
        /// Update balance line item cost for the tactic
        /// </summary>
        /// <param name="PlanTacticId"></param>
        /// <returns></returns>
        public double UpdateBalanceLineItemCost(int PlanTacticId)
        {
            // Get total line item cost
            double tacticTotalLineItemsCost = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(lineItem => 
                                                                lineItem.PlanTacticId == PlanTacticId && 
                                                                lineItem.IsDeleted == false && 
                                                                lineItem.LineItemTypeId != null).Sum(x => x.Cost);

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

        #region Functions related to line item cost allocation

        /// <summary>
        /// Save monthly cost allocation for line items and update balance line item
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="monthlycost"></param>
        /// <param name="month"></param>
        public void SaveLineItemMonthlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt => 
                                                                                pcpt.PlanLineItemId == EntityId && 
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (isTotalCost) // Total planned cost is changed
            {
                objLineitem.Cost = monthlycost;
                objDbMrpEntities.Entry(objLineitem).State = EntityState.Modified;
            }
            else // When month wise cost allocation is changed
            {
                string period = Convert.ToString(MonthQuarterList[month]);
                if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).Any())
                {
                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlycost;
                    objDbMrpEntities.Entry(objLineitem).State = EntityState.Modified;
                }
                else
                {
                    AddNewRowLineItemCost(EntityId, period, monthlycost);
                }
            }
            objDbMrpEntities.SaveChanges();

            UpdateBalanceLineItemCost(objLineitem.PlanTacticId);
        }

        /// <summary>
        /// Save quarterly cost allocation for line items
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="monthlycost"></param>
        /// <param name="yearlycost"></param>
        /// <param name="month"></param>
        public void SaveLineItemQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                pcpt.PlanLineItemId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (isTotalCost) // Total planned cost is changed
            {
                objLineitem.Cost = monthlycost;
                objDbMrpEntities.Entry(objLineitem).State = EntityState.Modified;
            }
            else // When month wise cost allocation is changed
            {
                string period = Convert.ToString(MonthQuarterList[month]);

                if (QuartersList[period] != null && objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).Any())
                {
                    double curQuarterCost = 0;
                    curQuarterCost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (curQuarterCost < monthlycost)
                    {
                        IncreaseQuarterlyLineItemCost(objLineitem, EntityId, period, monthlycost, curQuarterCost);
                    }
                    // If new value is lesser then subtract from last month
                    else if (curQuarterCost > monthlycost)
                    {
                        DecreaseQuarterlyLineItemCost(objLineitem, period, monthlycost, curQuarterCost);
                    }
                }
                else
                {
                    AddNewRowLineItemCost(EntityId, period, monthlycost);
                }
            }
            objDbMrpEntities.SaveChanges();

            UpdateBalanceLineItemCost(objLineitem.PlanTacticId);
        }
        
        /// <summary>
        /// Save quarterly allocated line item cost while cost is increased
        /// </summary>
        /// <param name="objLineItem"></param>
        /// <param name="EntityId"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        /// <param name="curQuarterCost"></param>
        private void IncreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineItem, int EntityId, string period, double monthlycost, double curQuarterCost)
        {
            if (objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).Any())
            {
                objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).FirstOrDefault().Value += monthlycost - curQuarterCost;
                objDbMrpEntities.Entry(objLineItem).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                AddNewRowLineItemCost(EntityId, period, monthlycost);
            }
        }

        /// <summary>
        /// Save quarterly allocated line item cost while cost is decreased
        /// </summary>
        /// <param name="objLineitem"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        /// <param name="curQuarterCost"></param>
        private void DecreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineitem, string period, double monthlycost, double curQuarterCost)
        {
            QuartersList[period].Reverse(); // Reversed list for subtract from last months
            double curPeriodVal = 0, costDiff = 0;
            costDiff = curQuarterCost - monthlycost;
            // Subtract cost from each months of the quarter
            foreach (string quarter in QuartersList[period])
            {
                if (objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == quarter).Any())
                {
                    curPeriodVal = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault().Value;
                    curPeriodVal = curPeriodVal - costDiff;
                    costDiff = -curPeriodVal;
                    objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault().Value = curPeriodVal < 0 ? 0 : curPeriodVal;
                    if (curPeriodVal >= 0)
                    {
                        break;
                    }
                }
            }
            objDbMrpEntities.Entry(objLineitem).State = EntityState.Modified;
            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Add new row for allocated line item cost
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        private void AddNewRowLineItemCost(int EntityId, string period, double monthlycost)
        {
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = new Plan_Campaign_Program_Tactic_LineItem_Cost();
            objLineItemCost.PlanLineItemId = EntityId;
            objLineItemCost.Period = period;
            objLineItemCost.Value = monthlycost;
            objLineItemCost.CreatedBy = Sessions.User.ID;
            objLineItemCost.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        #endregion

        #region Functions related to tactic cost allocation

        /// <summary>
        /// Save monthly allocated cost for tactic and update other line item
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="monthlycost"></param>
        /// <param name="month"></param>
        public void SaveTacticMonthlyCostAllocation(int EntityId, double monthlycost, string month,bool isTotalCost)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (isTotalCost) // Total planned cost is changed
            {
                objTactic.Cost = monthlycost;
                objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
            }
            else // When month wise cost allocation is changed
            {
                string period = Convert.ToString(MonthQuarterList[month]);
                if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).Any())
                {
                    objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault().Value = monthlycost;
                    objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
                }
                else
                {
                    AddNewRowTacticCost(EntityId, period, monthlycost);
                }
            }
            objDbMrpEntities.SaveChanges();

            UpdateBalanceLineItemCost(objTactic.PlanTacticId);
        }

        /// <summary>
        /// Save quarterly allocated cost for tactic and update other line item/// 
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="monthlycost"></param>
        /// <param name="month"></param>
        /// <param name="isTotalCost"></param>
        public void SaveTacticQuarterlyCostAllocation(int EntityId, double monthlycost, string month, bool isTotalCost)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (isTotalCost) // Total planned cost is changed
            {
                objTactic.Cost = monthlycost;
                objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
            }
            else // When month wise cost allocation is changed
            {
                string period = Convert.ToString(MonthQuarterList[month]);

                if (QuartersList[period] != null && objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).Any())
                {
                    double curQuarterCost = 0;
                    curQuarterCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (curQuarterCost < monthlycost)
                    {
                        IncreaseQuarterlyTacticCost(objTactic, EntityId, period, monthlycost, curQuarterCost);
                    }
                    // If new value is lesser then subtract from last month
                    else if (curQuarterCost > monthlycost)
                    {
                        DecreaseQuarterlyTacticCost(objTactic, period, monthlycost, curQuarterCost);
                    }
                }
                else
                {
                    AddNewRowTacticCost(EntityId, period, monthlycost);
                }
            }
            objDbMrpEntities.SaveChanges();

            UpdateBalanceLineItemCost(objTactic.PlanTacticId);
        }

        /// <summary>
        /// Save quarterly allocated tactic cost while cost is increased
        /// </summary>
        /// <param name="objTactic"></param>
        /// <param name="EntityId"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        /// <param name="curQuarterCost"></param>
        private void IncreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, int EntityId, string period, double monthlycost, double curQuarterCost)
        {
            if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).Any())
            {
                objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).FirstOrDefault().Value += monthlycost - curQuarterCost;
                objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
            }
            else
            {
                AddNewRowTacticCost(EntityId, period, monthlycost);
            }
        }

        /// <summary>
        /// Save quarterly allocated tactic cost while cost is decreased
        /// </summary>
        /// <param name="objTactic"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        /// <param name="curQuarterCost"></param>
        private void DecreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, string period, double monthlycost, double curQuarterCost)
        {
            QuartersList[period].Reverse(); // Reversed list for subtract from last months
            double curPeriodVal = 0, costDiff = 0;
            costDiff = curQuarterCost - monthlycost;
            // Subtract cost from each months of the quarter
            foreach (string quarter in QuartersList[period])
            {
                if (objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == quarter).Any())
                {
                    curPeriodVal = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault().Value;
                    curPeriodVal = curPeriodVal - costDiff;
                    costDiff = -curPeriodVal;
                    objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == quarter).FirstOrDefault().Value = curPeriodVal < 0 ? 0 : curPeriodVal;
                    if (curPeriodVal >= 0)
                    {
                        break;
                    }
                }
            }
            objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
            objDbMrpEntities.SaveChanges();
        }

        /// <summary>
        /// Add new record for allocated tactic cost 
        /// </summary>
        /// <param name="EntityId"></param>
        /// <param name="period"></param>
        /// <param name="monthlycost"></param>
        private void AddNewRowTacticCost(int EntityId, string period, double monthlycost)
        {
            Plan_Campaign_Program_Tactic_Cost objTacticCost = new Plan_Campaign_Program_Tactic_Cost();
            objTacticCost.PlanTacticId = EntityId;
            objTacticCost.Period = period;
            objTacticCost.Value = monthlycost;
            objTacticCost.CreatedBy = Sessions.User.ID;
            objTacticCost.CreatedDate = DateTime.Now;
            objDbMrpEntities.Entry(objTacticCost).State = EntityState.Added;
            objDbMrpEntities.SaveChanges();
        }

        #endregion

        /// <summary>
        /// Manage lines items if cost is allocated to other
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<BudgetModel> ManageLineItems(List<BudgetModel> model)
        {
            foreach (BudgetModel l in model.Where(l => l.ActivityType == ActivityType.ActivityTactic))
            {
                //// Calculate Line Difference.
                BudgetMonth lineDiff = new BudgetMonth();
                List<BudgetModel> lines = model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId).ToList();
                BudgetModel otherLine = lines.Where(ol => ol.LineItemTypeId == null).FirstOrDefault();
                lines = lines.Where(ol => ol.LineItemTypeId != null).ToList();
                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiff.Jan = l.Month.Jan - lines.Sum(lmon => (double?)lmon.Month.Jan) ?? 0;
                        lineDiff.Feb = l.Month.Feb - lines.Sum(lmon => (double?)lmon.Month.Feb) ?? 0;
                        lineDiff.Mar = l.Month.Mar - lines.Sum(lmon => (double?)lmon.Month.Mar) ?? 0;
                        lineDiff.Apr = l.Month.Apr - lines.Sum(lmon => (double?)lmon.Month.Apr) ?? 0;
                        lineDiff.May = l.Month.May - lines.Sum(lmon => (double?)lmon.Month.May) ?? 0;
                        lineDiff.Jun = l.Month.Jun - lines.Sum(lmon => (double?)lmon.Month.Jun) ?? 0;
                        lineDiff.Jul = l.Month.Jul - lines.Sum(lmon => (double?)lmon.Month.Jul) ?? 0;
                        lineDiff.Aug = l.Month.Aug - lines.Sum(lmon => (double?)lmon.Month.Aug) ?? 0;
                        lineDiff.Sep = l.Month.Sep - lines.Sum(lmon => (double?)lmon.Month.Sep) ?? 0;
                        lineDiff.Oct = l.Month.Oct - lines.Sum(lmon => (double?)lmon.Month.Oct) ?? 0;
                        lineDiff.Nov = l.Month.Nov - lines.Sum(lmon => (double?)lmon.Month.Nov) ?? 0;
                        lineDiff.Dec = l.Month.Dec - lines.Sum(lmon => (double?)lmon.Month.Dec) ?? 0;

                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Month = lineDiff;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().ParentMonth = lineDiff;

                        //double allocated = l.Allocated - lines.Sum(l1 => l1.Allocated);
                        //allocated = allocated < 0 ? 0 : allocated;
                        //model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Allocated = allocated;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Month = l.Month;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().ParentMonth = l.Month;
                        //model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.ParentActivityId == l.ActivityId && line.LineItemTypeId == null).FirstOrDefault().Allocated = l.Allocated < 0 ? 0 : l.Allocated;
                    }
                }
            }
            return model;
        }
    }
}