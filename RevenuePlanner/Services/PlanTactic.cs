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
        #endregion

        #region Dictionary for month and quarter number which is used to get cost and other data 
        private Dictionary<string, string> MonthQuarterList = new Dictionary<string, string>() {
                {Convert.ToString(Enums.Months.January), Common.Jan },
                {Convert.ToString(Enums.Months.February), Common.Feb },
                {Convert.ToString(Enums.Months.March), Common.Mar },
                {Convert.ToString(Enums.Months.April), Common.Apr },
                {Convert.ToString(Enums.Months.May), Common.May },
                {Convert.ToString(Enums.Months.June), Common.Jun},
                {Convert.ToString(Enums.Months.July), Common.Jul },
                {Convert.ToString(Enums.Months.August), Common.Aug },
                {Convert.ToString(Enums.Months.September), Common.Sep },
                {Convert.ToString(Enums.Months.October), Common.Oct },
                {Convert.ToString(Enums.Months.November), Common.Nov },
                {Convert.ToString(Enums.Months.December), Common.Dec },
                {Enums.Quarters[Convert.ToString(Enums.QuarterWithSpace.Quarter1)], Common.Jan },
                {Enums.Quarters[Convert.ToString(Enums.QuarterWithSpace.Quarter2)], Common.Apr },
                {Enums.Quarters[Convert.ToString(Enums.QuarterWithSpace.Quarter3)], Common.Jul },
                {Enums.Quarters[Convert.ToString(Enums.QuarterWithSpace.Quarter4)], Common.Oct }
                };

        // dictionary to get in each quarter which values will be summed up
        private Dictionary<string, List<string>> QuartersList = new Dictionary<string, List<string>>()
        {
                {Common.Jan, new List<string>{Common.Jan, Common.Feb, Common.Mar} },
                {Common.Apr, new List<string>{Common.Apr, Common.May, Common.Jun } },
                {Common.Jul, new List<string>{Common.Jul, Common.Aug, Common.Sep } },
                {Common.Oct, new List<string>{Common.Oct, Common.Nov, Common.Dec } }
        };
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
            List<BudgetModel> model = SetAllocationValuesToModel(dtCosts, PlanExchangeRate);    // Set cost values to model

            model = SetPermissionForEditable(curTacticId, UserId, ClientId, model);             // Update model with view edit permissions
            model = ManageBalanceLineItemCost(model);                                           // Update model by setting balance line item costs

            //Set cost allocation for quarters by summed up respective months
            if (String.Compare(AllocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters),true) == 0)
            {
                model = SumOfMonthsForQuaterlyAllocated(model);
            }
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
            objBudgetDHTMLXGrid.SetHeader = "ActivityId,Task Name,Planned Cost";         // header values for the grid
            objBudgetDHTMLXGrid.ColAlign = objHomeGridProperty.alignleft + commaString +
                                            objHomeGridProperty.alignleft + commaString +
                                            objHomeGridProperty.aligncenter;             // alignment for the column

            objBudgetDHTMLXGrid.ColumnIds = "activityid,taskname,plannedcost";           // ids of the columns
            objBudgetDHTMLXGrid.ColType = objHomeGridProperty.typero + commaString +
                                            objHomeGridProperty.typetree + commaString +
                                            objHomeGridProperty.typeEdn;                  // types of the columns

            objBudgetDHTMLXGrid.Width = "0,100,50";                                      // width of the columns
            objBudgetDHTMLXGrid.ColSorting = "na,na,na";                                 // sorting options for the column
            
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
            objBudgetDHTMLXGrid.Width += commaString + "50";
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
            for (int i = 1; i <= 12; i++)
            {
                dt = new DateTime(2012, i, 1);
                objBudgetDHTMLXGrid.SetHeader += commaString + dt.ToString("MMM").ToUpper();
                objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                objBudgetDHTMLXGrid.ColumnIds += commaString + dt.ToString("MMM");
                objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                objBudgetDHTMLXGrid.Width += commaString + "40";
                objBudgetDHTMLXGrid.ColSorting += commaString + "na";
            }
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Bind header for quarterly allocated
        /// </summary>
        private BudgetDHTMLXGridModel GenerateHeaderStringForQuarterlyAllocated(BudgetDHTMLXGridModel objBudgetDHTMLXGrid)
        {
            int quarterCounter = 1;
            // add each quarter in header object
            for (int i = 1; i <= 11; i += 3)
            {
                objBudgetDHTMLXGrid.SetHeader += commaString + "Q" + Convert.ToString(quarterCounter);
                objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                objBudgetDHTMLXGrid.ColumnIds += commaString + "Q" + Convert.ToString(quarterCounter);
                objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                objBudgetDHTMLXGrid.Width += commaString + "50";
                objBudgetDHTMLXGrid.ColSorting += commaString + "na";
                quarterCounter++;
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

            if (String.Compare(allocatedBy, Enums.PlanAllocatedByList[Convert.ToString(Enums.PlanAllocatedBy.quarters)], true) == 0)
            {
                lstBudgetData = GetQuarterlyAllocatedPlannedCost(isLineItem, isBalance, modelLineItem, lstBudgetData);
            }
            else
            {
                lstBudgetData = GetMonthlyAllocatedPlannedCost(isLineItem, isBalance, modelLineItem, lstBudgetData, allocatedBy);
            }
            return lstBudgetData;
        }

        /// <summary>
        /// Get month wise allocated cost
        /// </summary>
        private List<Budgetdataobj> GetMonthlyAllocatedPlannedCost( bool isLineItem, bool isOtherLineItem, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData, string allocatedBy)
        {
            double monthlyValue = 0, totalAllocatedCost = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (i == 1)
                {
                    monthlyValue = modelEntity.Month.Jan;
                }
                else if (i == 2)
                {
                    monthlyValue = modelEntity.Month.Feb;
                }
                else if (i == 3)
                {
                    monthlyValue = modelEntity.Month.Mar;
                }
                else if (i == 4)
                {
                    monthlyValue = modelEntity.Month.Apr;
                }
                else if (i == 5)
                {
                    monthlyValue = modelEntity.Month.May;
                }
                else if (i == 6)
                {
                    monthlyValue = modelEntity.Month.Jun;
                }
                else if (i == 7)
                {
                    monthlyValue = modelEntity.Month.Jul;
                }
                else if (i == 8)
                {
                    monthlyValue = modelEntity.Month.Aug;
                }
                else if (i == 9)
                {
                    monthlyValue = modelEntity.Month.Sep;
                }
                else if (i == 10)
                {
                    monthlyValue = modelEntity.Month.Oct;
                }
                else if (i == 11)
                {
                    monthlyValue = modelEntity.Month.Nov;
                }
                else if (i == 12)
                {
                    monthlyValue = modelEntity.Month.Dec;
                }
                Budgetdataobj  objBudgetData = new Budgetdataobj();
                // editable based on permission and balance line item never editable
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
        /// Get quarter wise allocated cost
        /// </summary>
        private List<Budgetdataobj> GetQuarterlyAllocatedPlannedCost(bool isLineItem, bool isOtherLineItem, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData)
        {
            double monthlyValue = 0, totalAllocatedCost = 0;
            Budgetdataobj objBudgetData;
            for (int i = 1; i <= 11; i += 3)
            {
                if (i == 1)
                {
                    monthlyValue = modelEntity.Month.Jan;
                }
                else if (i == 4)
                {
                    monthlyValue = modelEntity.Month.Apr;
                }
                else if (i == 7)
                {
                    monthlyValue = modelEntity.Month.Jul;
                }
                else if (i == 10)
                {
                    monthlyValue = modelEntity.Month.Oct;
                }
                objBudgetData = new Budgetdataobj();
                // editable based on permission and balance line item never editable
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
        /// Get summed up cost for required months in case of quarterly allocated
        /// </summary>
        private List<BudgetModel> SumOfMonthsForQuaterlyAllocated(List<BudgetModel> model)
        {
            // for quarterly allocation summed up respective months
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
            return model;
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
        private List<BudgetModel> SetAllocationValuesToModel(DataTable dtCosts, double PlanExchangeRate)
        {
            List<BudgetModel> model = new List<BudgetModel>();
            model = dtCosts.AsEnumerable().Select(row => new BudgetModel
            {
                Id = Convert.ToString(row["Id"]),   // Id of the tactic or line item
                ActivityId = Convert.ToString(row["ActivityId"]),   // Activity id for the entity
                ParentActivityId = Convert.ToString(row["ParentActivityId"]),
                ActivityName = Convert.ToString(row["ActivityName"]),
                PlannedCost = row["Cost"] != null ? Convert.ToDouble(row["Cost"]) : 0,
                // Set values by applying exchange rate
                Month = new BudgetMonth()
                {
                    Jan = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY1"])), PlanExchangeRate),
                    Feb = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY2"])), PlanExchangeRate),
                    Mar = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY3"])), PlanExchangeRate),
                    Apr = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY4"])), PlanExchangeRate),
                    May = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY5"])), PlanExchangeRate),
                    Jun = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY6"])), PlanExchangeRate),
                    Jul = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY7"])), PlanExchangeRate),
                    Aug = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY8"])), PlanExchangeRate),
                    Sep = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY9"])), PlanExchangeRate),
                    Oct = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY10"])), PlanExchangeRate),
                    Nov = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY11"])), PlanExchangeRate),
                    Dec = objCurrency.GetValueByExchangeRate(Common.ParseDoubleValue(Convert.ToString(row["CY12"])), PlanExchangeRate)
                },
                CreatedBy = Convert.ToInt32(row["CreatedBy"]),
                isEditable = Convert.ToBoolean(row["IsEditable"]),
                ActivityType = Convert.ToString(row["ActivityType"]),
                LineItemTypeId = Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"]))
            }).ToList();
            return model;
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
                objLineitem.Cost = newCost;
                objDbMrpEntities.Entry(objLineitem).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
                UpdateBalanceLineItemCost(objLineitem.PlanTacticId);
            }
        }

        /// <summary>
        /// Save monthly cost allocation for line items and update balance line item
        /// </summary>
        public void SaveLineItemMonthlyCostAllocation(int EntityId, double newCost, string month, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                pcpt.PlanLineItemId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objLineitem != null)
            {
                string period = Convert.ToString(MonthQuarterList[month]);
                Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == period).FirstOrDefault();
                if (objLineItemCost != null)
                {
                    objLineItemCost.Value = newCost;
                    objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
                }
                else
                {
                    AddNewRowLineItemCost(EntityId, period, newCost, UserId);   // Add new record for line item cost for the month
                }
                objDbMrpEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Save quarterly cost allocation for line items
        /// </summary>
        public void SaveLineItemQuarterlyCostAllocation(int EntityId, double newCost, string quarter, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem objLineitem = objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem.Where(pcpt =>
                                                                                pcpt.PlanLineItemId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objLineitem != null)
            {
                string period = Convert.ToString(MonthQuarterList[quarter]);
                List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstLineItemCost = objLineitem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).ToList();
                if (QuartersList[period] != null && lstLineItemCost.Any())
                {
                    double oldCost = lstLineItemCost.Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (oldCost < newCost)
                    {
                        IncreaseQuarterlyLineItemCost(objLineitem, EntityId, period, newCost, oldCost, UserId);
                    }
                    // If new value is lesser then subtract from last month
                    else if (oldCost > newCost)
                    {
                        DecreaseQuarterlyLineItemCost(objLineitem, period, newCost, oldCost);
                    }
                }
                else
                {
                    AddNewRowLineItemCost(EntityId, period, newCost, UserId);   // Add new record for line item cost for the quarter
                }
                objDbMrpEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Save quarterly allocated line item cost while cost is increased
        /// </summary>
        private void IncreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineItem, int EntityId, string period, double newCost, double oldCost, int UserId)
        {
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost = objLineItem.Plan_Campaign_Program_Tactic_LineItem_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).FirstOrDefault();
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
        private void DecreaseQuarterlyLineItemCost(Plan_Campaign_Program_Tactic_LineItem objLineitem, string period, double newCost, double oldCost)
        {
            QuartersList[period].Reverse(); // Reversed list to subtract from last months
            double curPeriodVal = 0, needToSubtract = 0;
            needToSubtract = oldCost - newCost;

            // Subtract cost from each months of the quarter e.g. For Q1 -> subtract from Y3, Y2 and Y1 as per requirement
            Plan_Campaign_Program_Tactic_LineItem_Cost objLineItemCost;
            foreach (string quarter in QuartersList[period])
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
                }
                objDbMrpEntities.Entry(objLineItemCost).State = EntityState.Modified;
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
                objTactic.Cost = newCost;
                objDbMrpEntities.Entry(objTactic).State = EntityState.Modified;
                objDbMrpEntities.SaveChanges();
                UpdateBalanceLineItemCost(objTactic.PlanTacticId);
            }
        }

        /// <summary>
        /// Save monthly allocated cost for tactic and update other line item
        /// </summary>
        public void SaveTacticMonthlyCostAllocation(int EntityId, double newCost, string month, int UserId)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objTactic != null)
            {
                string period = Convert.ToString(MonthQuarterList[month]);
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
            }
        }

        /// <summary>
        /// Save quarterly allocated cost for tactic and update other line item/// 
        /// </summary>
        public void SaveTacticQuarterlyCostAllocation(int EntityId, double newCost, string month, int UserId)
        {
            Plan_Campaign_Program_Tactic objTactic = objDbMrpEntities.Plan_Campaign_Program_Tactic.Where(pcpt =>
                                                                                pcpt.PlanTacticId == EntityId &&
                                                                                pcpt.IsDeleted == false).FirstOrDefault();
            if (objTactic != null)
            {
                string period = Convert.ToString(MonthQuarterList[month]);
                List<Plan_Campaign_Program_Tactic_Cost> lstTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => QuartersList[period].Contains(pcptc.Period)).ToList();
                if (QuartersList[period] != null && lstTacCost.Any())
                {
                    double oldCost = lstTacCost.Sum(x => x.Value);

                    // If new value is greater then add into first month
                    if (oldCost < newCost)
                    {
                        IncreaseQuarterlyTacticCost(objTactic, EntityId, period, newCost, oldCost, UserId);
                    }
                    // If new value is lesser then subtract from last month
                    else if (oldCost > newCost)
                    {
                        DecreaseQuarterlyTacticCost(objTactic, period, newCost, oldCost);
                    }
                }
                else
                {
                    AddNewRowTacticCost(EntityId, period, newCost, UserId); // Add new record for tactic cost for the quarter
                }
                objDbMrpEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Save quarterly allocated tactic cost while cost is increased
        /// </summary>
        private void IncreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, int EntityId, string period, double newCost, double oldCost, int UserId)
        {
            Plan_Campaign_Program_Tactic_Cost objTacCost = objTactic.Plan_Campaign_Program_Tactic_Cost.Where(pcptc => pcptc.Period == QuartersList[period].First()).FirstOrDefault();
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
        private void DecreaseQuarterlyTacticCost(Plan_Campaign_Program_Tactic objTactic, string period, double newCost, double oldCost)
        {
            QuartersList[period].Reverse(); // Reversed list for subtract from last months
            double curPeriodVal = 0, costDiff = 0;
            costDiff = oldCost - newCost;
            Plan_Campaign_Program_Tactic_Cost objTacCost = new Plan_Campaign_Program_Tactic_Cost();
            // Subtract cost from each months of the quarter
            foreach (string quarter in QuartersList[period])
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
                }
                objDbMrpEntities.Entry(objTacCost).State = EntityState.Modified;
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
            // tactic cost model
            BudgetModel tacticModel = model.Where(l => l.ActivityType == ActivityType.ActivityTactic).FirstOrDefault();
            if (tacticModel != null)
            {
                BudgetMonth lineDiff = new BudgetMonth();

                // Get all line items for the tactic
                List<BudgetModel> lines = model.Where(line =>
                                                line.ActivityType == ActivityType.ActivityLineItem &&
                                                line.LineItemTypeId != null).ToList();

                // Get balance line item for the tactic
                BudgetModel otherLine = model.Where(line =>
                                                line.ActivityType == ActivityType.ActivityLineItem &&
                                                line.LineItemTypeId == null).FirstOrDefault();

                if (otherLine != null)
                {
                    if (lines.Count > 0)
                    {
                        lineDiff.Jan = tacticModel.Month.Jan - lines.Sum(lmon => (double?)lmon.Month.Jan) ?? 0;
                        lineDiff.Feb = tacticModel.Month.Feb - lines.Sum(lmon => (double?)lmon.Month.Feb) ?? 0;
                        lineDiff.Mar = tacticModel.Month.Mar - lines.Sum(lmon => (double?)lmon.Month.Mar) ?? 0;
                        lineDiff.Apr = tacticModel.Month.Apr - lines.Sum(lmon => (double?)lmon.Month.Apr) ?? 0;
                        lineDiff.May = tacticModel.Month.May - lines.Sum(lmon => (double?)lmon.Month.May) ?? 0;
                        lineDiff.Jun = tacticModel.Month.Jun - lines.Sum(lmon => (double?)lmon.Month.Jun) ?? 0;
                        lineDiff.Jul = tacticModel.Month.Jul - lines.Sum(lmon => (double?)lmon.Month.Jul) ?? 0;
                        lineDiff.Aug = tacticModel.Month.Aug - lines.Sum(lmon => (double?)lmon.Month.Aug) ?? 0;
                        lineDiff.Sep = tacticModel.Month.Sep - lines.Sum(lmon => (double?)lmon.Month.Sep) ?? 0;
                        lineDiff.Oct = tacticModel.Month.Oct - lines.Sum(lmon => (double?)lmon.Month.Oct) ?? 0;
                        lineDiff.Nov = tacticModel.Month.Nov - lines.Sum(lmon => (double?)lmon.Month.Nov) ?? 0;
                        lineDiff.Dec = tacticModel.Month.Dec - lines.Sum(lmon => (double?)lmon.Month.Dec) ?? 0;
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.LineItemTypeId == null).FirstOrDefault().Month = lineDiff;
                    }
                    else
                    {
                        model.Where(line => line.ActivityType == ActivityType.ActivityLineItem && line.LineItemTypeId == null).FirstOrDefault().Month = tacticModel.Month;
                    }
                }
            }
            return model;
        }
    }
}
