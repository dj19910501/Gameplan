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
            objDbMrpEntities = Common.db;
            objCurrency = new Currency();
        }

        private string formatThousand = "#,##0.##";  // format thousand number values with comma
        private string commaString = ",";
        private Dictionary<string, int> tacticYears = new Dictionary<string, int>(); // start and end date years of tactic
        private string Year = "Year";
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
        public BudgetDHTMLXGridModel GetCostAllocationLineItemInspectPopup(int curTacticId, string AllocatedBy, int UserId, int ClientId, double PlanExchangeRate, bool IsPlanEditable)
        {
            DataTable dtCosts = Common.GetTacticLineItemCostAllocation(curTacticId, UserId);    // Get cost allocation and set values to model
            // Logic to get the tactic for which line items allocation has been fetched and get start and end date
            DataRow objTacticRow = dtCosts.AsEnumerable().Where(x => Convert.ToInt32(x["id"]).Equals(curTacticId)).FirstOrDefault();
            if (objTacticRow != null)
            {
                int startYear = Convert.ToDateTime(objTacticRow["StartDate"]).Year;
                int endYear = Convert.ToDateTime(objTacticRow["EndDate"]).Year;
                if (startYear != endYear)
                {
                    // For multi year tactic
                    for (int year = startYear; year <= endYear; year++)
                    {
                        tacticYears.Add(string.Format(Year + "{0}", year), year);
                    }
                }
                else
                {
                    // For single year tactic
                    tacticYears.Add(string.Format(Year + "{0}", startYear), startYear);
                }
            }
            // Set values from datatable to model
            List<BudgetModel> model = SetAllocationValuesToModel(dtCosts, PlanExchangeRate, AllocatedBy, IsPlanEditable);

            BudgetDHTMLXGridModel objBudgetDHTMLXGrid = new BudgetDHTMLXGridModel(); //New is required due to access the property
            objBudgetDHTMLXGrid.Grid = new BudgetDHTMLXGrid();

            // Bind header of the line items allocation grid
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
            #region Bind  header part with common columns
            objBudgetDHTMLXGrid.SetHeader = Common.CommonHeaderForDefaultCols;         // header values for the grid
            objBudgetDHTMLXGrid.ColAlign = Common.CommonAlignForDefaultCols;           // alignment for the column
            objBudgetDHTMLXGrid.ColumnIds = Common.CommonColumnIdsForDefaultCols;      // ids of the columns
            objBudgetDHTMLXGrid.ColType = Common.CommonColTypeForDefaultCols;          // types of the columns
            objBudgetDHTMLXGrid.Width = Common.CommonWidthForDefaultCols;              // width of the columns
            objBudgetDHTMLXGrid.ColSorting = Common.CommonColSortingForDefaultCols;    // sorting options for the column
            #endregion

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
            objBudgetDHTMLXGrid.Width += commaString + "90";   // set cost column width to 90 so that 10 digits shows properly
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
                    objBudgetDHTMLXGrid.SetHeader += commaString + dt.ToString(Common.MonthlyCostHeaderFormat).ToUpper();
                    objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                    objBudgetDHTMLXGrid.ColumnIds += commaString + dt.ToString(Common.MonthlyCostHeaderFormat);
                    objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                    objBudgetDHTMLXGrid.Width += commaString + "90";    // set cost column width to 90 so that 10 digits shows properly
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
                for (int i = 1; i < 11; i += 3)
                {
                    objBudgetDHTMLXGrid.SetHeader += commaString + "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                    objBudgetDHTMLXGrid.ColAlign += commaString + objHomeGridProperty.aligncenter;
                    objBudgetDHTMLXGrid.ColumnIds += commaString + "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                    objBudgetDHTMLXGrid.ColType += commaString + objHomeGridProperty.typeEdn;
                    objBudgetDHTMLXGrid.Width += commaString + "90";  // set cost column width to 90 so that 10 digits shows properly
                    objBudgetDHTMLXGrid.ColSorting += commaString + "na";
                    quarterCounter++;
                }
            }
            return objBudgetDHTMLXGrid;
        }

        /// <summary>
        /// Get month wise allocated cost
        /// </summary>
        private List<Budgetdataobj> GetMonthlyQuarterlyAllocatedPlannedCost(string activityType, BudgetModel modelEntity, List<Budgetdataobj> lstBudgetData, string allocatedBy, string lockedstate, string stylecolor)
        {
            double monthlyValue = 0, totalAllocatedCost = 0;
            Budgetdataobj objBudgetData;
            // Add every months/quarters cost to model object to bind the grid
            foreach (string month in modelEntity.MonthlyCosts.Keys)
            {
                monthlyValue = modelEntity.MonthlyCosts[month];
                objBudgetData = new Budgetdataobj();
                // Set editable based on permission (balance always remain read only)
                if (modelEntity.LineItemTypeId == null)
                {
                    objBudgetData.locked = objHomeGridProperty.lockedstateone;
                    objBudgetData.style = objHomeGridProperty.stylecolorgray;
                }
                else
                {
                    objBudgetData.locked = lockedstate;
                    objBudgetData.style = stylecolor;
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
            string stylecolor, lockedstate;
            if (tacticModel.isEditable)
            {
                lockedstate = objHomeGridProperty.lockedstatezero;
                stylecolor = objHomeGridProperty.stylecolorblack;
            }
            else
            {
                lockedstate = objHomeGridProperty.lockedstateone;
                stylecolor = objHomeGridProperty.stylecolorgray;
            }

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
            objTacticData.locked = lockedstate;
            objTacticData.style = stylecolor;
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();    // Add Icons column value
            objTacticData.value = "";
            objTacticData.locked = lockedstate;
            objTacticData.style = stylecolor;
            lstTacticData.Add(objTacticData);

            objTacticData = new Budgetdataobj();    // Add Planned Cost column value
            objTacticData.value = tacticModel.PlannedCost.ToString(formatThousand);
            objTacticData.locked = lockedstate;
            objTacticData.style = stylecolor;
            lstTacticData.Add(objTacticData);

            // Add allocated cost data based on monthly/quarterly
            lstTacticData = GetMonthlyQuarterlyAllocatedPlannedCost(ActivityType.ActivityTactic, tacticModel, lstTacticData, AllocatedBy, lockedstate, stylecolor);

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
                lstLineItemRows.Add(BindGridJsonForLineItems(bml.Id, bml.ActivityName, bml.ActivityId, model, AllocatedBy, lockedstate, stylecolor));
            }
            objTacticRows.rows = lstLineItemRows;
            lstTacticRows.Add(objTacticRows);
            return lstTacticRows;
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
            strIconsData.Append("<div class=grid_add onclick=javascript:OpenLineItemGridPopup(this,event)  id=Line alt=" + Convert.ToString(Entity.ParentActivityId) + "_" + Convert.ToString(Entity.ActivityId));
            strIconsData.Append(" lt=" + ((Entity.LineItemTypeId == null) ? 0 : Entity.LineItemTypeId) + " per=true");
            strIconsData.Append(" dt=" + Convert.ToString(Entity.ActivityName) + " ><i class='fa fa-plus-circle' aria-hidden='true'></i></div>");
            return strIconsData;
        }

        /// <summary>
        /// Bind data for each line item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BudgetDHTMLXGridDataModel BindGridJsonForLineItems(string LineItemId, string ActivityName, string ActivityId, List<BudgetModel> model, string AllocatedBy, string lockedstate, string stylecolor)
        {
            BudgetDHTMLXGridDataModel objLineItemRows = new BudgetDHTMLXGridDataModel();

            #region Add common columns to the allocation grid
            objLineItemRows.id = ActivityType.ActivityLineItem + HttpUtility.HtmlEncode(ActivityId);
            objLineItemRows.open = null;
            objLineItemRows.bgColor = objHomeGridProperty.LineItemBackgroundColor;
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
            else
            {
                objLineItemData.locked = lockedstate;
                objLineItemData.style = stylecolor;
            }
            objLineItemData.value = HttpUtility.HtmlEncode(ActivityName).Replace("'", "&#39;"); // HttpUtility.HtmlEncode handles all character except ' so need to be replaced
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
            else
            {
                objLineItemData.locked = lockedstate;
                objLineItemData.style = stylecolor;
            }
            lstLineItemData.Add(objLineItemData);
            #endregion

            //bind monthly/quaterly allocation data for line items to the grid
            lstLineItemData = GetMonthlyQuarterlyAllocatedPlannedCost(ActivityType.ActivityLineItem, modelEntity, lstLineItemData, AllocatedBy, lockedstate, stylecolor);

            objLineItemRows.data = lstLineItemData;
            return objLineItemRows;
        }

        /// <summary>
        /// Bind allocated cost values to the model
        /// </summary>
        private List<BudgetModel> SetAllocationValuesToModel(DataTable dtCosts, double PlanExchangeRate, string allocatedBy, bool IsPlanEditable)
        {
            List<BudgetModel> model = dtCosts.AsEnumerable().Select(row => new BudgetModel
            {
                Id = Convert.ToString(row["Id"]),   // Id of the tactic or line item
                ActivityId = Convert.ToString(row["ActivityId"]),   // Activity id for the entity
                ParentActivityId = Convert.ToString(row["ParentActivityId"]),  // Activity Id of parent entity
                ActivityName = Convert.ToString(row["ActivityName"]),   // Title of the activity
                PlannedCost = row["Cost"] != DBNull.Value ? objCurrency.GetValueByExchangeRate(Convert.ToDouble(row["Cost"]), PlanExchangeRate) : 0,  // Planned cost for the entity
                StartDate = Convert.ToDateTime(row["StartDate"]),   // tactic start date
                EndDate = Convert.ToDateTime(row["EndDate"]),       // tactic end date
                MonthlyCosts = SetMonthlyCostsToModel(row, allocatedBy, PlanExchangeRate),   // list of months/quarters cost based on allocated by
                CreatedBy = row["CreatedBy"] == DBNull.Value ? 0 : Convert.ToInt32(row["CreatedBy"]),  // owner of the entity
                isEditable = IsPlanEditable,  // set default permission false
                ActivityType = Convert.ToString(row["ActivityType"]),
                LineItemTypeId = row["LineItemTypeId"] == DBNull.Value ? null : Common.ParseIntValue(Convert.ToString(row["LineItemTypeId"])),
                LinkTacticId = row["LinkTacticId"] == DBNull.Value ? 0 : Convert.ToInt32(row["LinkTacticId"]),
            }).ToList();

            model = ManageBalanceLineItemCost(model);   // Update model by setting balance line item costs

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
            int MonthCounter = 0;   // variable used to set month number as per multi year

            //if (String.Compare(allocatedBy, Convert.ToString(Enums.PlanAllocatedBy.quarters)) == 0)
            if (Convert.ToString(Enums.PlanAllocatedBy.quarters).Equals(allocatedBy))
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
                        Period = Common.PeriodPrefix + Convert.ToString(i + MonthCounter);      // When current year is 2016 then For Q1-2017 -> Y13
                        QuartersList = Common.GetMonthsOfQuarters(i + MonthCounter);            // Get months for the quarter
                        // Sum up month wise values by applying currency to get the sum of quarter
                        QuartersList.ForEach(x => SumOfMonths = SumOfMonths + objCurrency.GetValueByExchangeRate(Convert.ToDouble(row[x]),PlanExchangeRate));
                        QuarterLabel = "Q" + Convert.ToString(quarterCounter) + "-" + Convert.ToString(tacticYears[key]);
                        dictMonthlyCosts.Add(QuarterLabel, SumOfMonths);
                        quarterCounter++;
                    }
                    MonthCounter = MonthCounter + Convert.ToInt32(Enums.QuarterMonthDigit.Month);   // Add total no. of months(12) for second year
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
                        Period = Common.PeriodPrefix + Convert.ToString(i + MonthCounter);
                        // When current year is 2016 then For JAN-2017 -> Y13
                        dictMonthlyCosts.Add(dtDate.ToString(Common.MonthlyCostHeaderFormat), objCurrency.GetValueByExchangeRate(Convert.ToDouble(row[Period]), PlanExchangeRate));
                    }
                    MonthCounter = MonthCounter + Convert.ToInt32(Enums.QuarterMonthDigit.Month);   // Add total no. of months(12) for second year
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
            // When current year is 2016 and edited cell is MAR-2017, then convert it to Y15 
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
                // When current year is 2016 then For MAR-2017 -> Y + ((1*12) + 3) = Y15
                period = Common.PeriodPrefix + Convert.ToString((yearDiff * 12) + monthNumber);
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

                // When current year is 2016 then For Q1-2017 -> Y + ((1*12) + (1*3) - 2) = Y13
                monthNumber = (yearDiff * 12) + (quarterNumber * 3) - 2;
                // When current year is 2016 then For Q1-2017 -> Y13
                period = Common.PeriodPrefix + Convert.ToString(monthNumber);
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

            // When current year is 2016 then For Q1-2017 -> Y13 
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
            // When current year is 2016 and edited cell is MAR-2017, then convert it to Y15 
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
            // When current year is 2016 then For Q1-2017 -> Y13
            int monthNumber = GetPeriodBasedOnEditedQuarter(quarter, startDateYear,isSourceLineItem);
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
