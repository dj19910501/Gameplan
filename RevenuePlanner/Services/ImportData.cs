﻿using Excel;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RevenuePlanner.Services
{
    public class ImportData : IImportData
    {
        private MRPEntities objDbMrpEntities;
        public ImportData()
        {
            //objDbMrpEntities =  new Common.db;
            objDbMrpEntities =  new MRPEntities();
        }
        /// <summary>
        /// Following function will return filtered datatable as per planBudgetType.
        /// </summary>
        /// <param name="importData"></param>
        /// <param name="planBudgetType"></param>
        /// <returns></returns>
        public DataTable GetPlanBudgetDataByType(DataTable importData, string planBudgetType, bool isMonthly)
        {

            DataTable dtPlanBudget = importData;
            try
            {


                //planBudgetType veriable is used to identify type of plan budget(ie>plan,budget,actual) 

                int columnRemovedCount = 0;//veriable to identify how many columns are removed
                int columnCount = dtPlanBudget.Columns.Count;//veriable to identify total column in datatable           
                                                             //Loop to remove columns which is not related to planBudgetType where first three column will be fixed Activityid,type,task name.
                if (dtPlanBudget.Rows[dtPlanBudget.Rows.Count - 1][0].ToString().Trim() ==
                                         "This document was made with dhtmlx library. http://dhtmlx.com")
                {
                    dtPlanBudget.Rows.RemoveAt(dtPlanBudget.Rows.Count - 1);
                }

                for (int i = 3; i < columnCount; i++)
                {
                    //if (!Convert.ToString(dtPlanBudget.Rows[0][i - columnRemovedCount]).ToLower().Trim().Contains(planBudgetType))
                    //{
                    //    dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                    //    columnRemovedCount++;
                    //}
                    if (!Convert.ToString(dtPlanBudget.Columns[i - columnRemovedCount]).ToLower().Trim().Contains(planBudgetType))
                    {
                        dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                        columnRemovedCount++;
                    }
                }
                //remove extra row.

                //Set column name 
                dtPlanBudget = SetColumnName(dtPlanBudget, isMonthly);
                //convert current currency value in to dollar.
                return ConvertValueAsperCurrency(dtPlanBudget);
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public DataTable ReturnPlanYearData(DataTable dtPlanBudget)
        {
            int MonthQuarterStartIndex = 7;//this is month or quarter columnstart index
            List<string> listPlanActivityId = dtPlanBudget.AsEnumerable().Where(data => data.Field<string>("Type") == "plan")
                        .Select(r => r.Field<string>("ActivityId"))
                        .ToList();
            List<int> activityIdlist = listPlanActivityId.Select(int.Parse).ToList();

            List<Plan> planList = (from row in objDbMrpEntities.Plans where activityIdlist.Contains(row.PlanId) select row).ToList();

            for (int cntplanid = 0; cntplanid < planList.Count; cntplanid++)
            {
                string year = planList.ElementAt(cntplanid).Year;
                string activityId = Convert.ToString(planList.ElementAt(cntplanid).PlanId);
                //   List<DataRow> lst = new List<DataRow>();


                if (!Convert.ToString(dtPlanBudget.Columns[MonthQuarterStartIndex]).ToLower().Trim().Contains(year))
                {

                    DataRow planRow = dtPlanBudget.AsEnumerable().Where(row => row.Field<String>("activityid") == activityId && row.Field<String>("type") == "plan").FirstOrDefault();
                    if(planRow!=null)
                    dtPlanBudget.Rows.Remove(planRow);
                    int intActivityId = Convert.ToInt32(activityId);
                    //Remove cmapign of that plan
                    List<Plan_Campaign> lstCampaign = (from row in objDbMrpEntities.Plan_Campaign
                                                       where row.PlanId.Equals(intActivityId) && row.IsDeleted == false
                                                       select row).ToList();

                    for (int campaignCnt = 0; campaignCnt < lstCampaign.Count; campaignCnt++)
                    {
                        activityId = Convert.ToString(lstCampaign.ElementAt(campaignCnt).PlanCampaignId);
                        DataRow campaignRow = dtPlanBudget.AsEnumerable().Where(row => row.Field<String>("activityid") == activityId && row.Field<String>("type") == "campaign").FirstOrDefault();
                        if (campaignRow != null)
                            dtPlanBudget.Rows.Remove(campaignRow);
                    }


                    //Remove program of that plan
                    List<Plan_Campaign_Program> lstProgram = (from row in objDbMrpEntities.Plan_Campaign_Program
                                                              join objCampaign in objDbMrpEntities.Plan_Campaign on row.PlanCampaignId equals objCampaign.PlanCampaignId
                                                              where objCampaign.PlanId.Equals(intActivityId) && row.IsDeleted == false && objCampaign.IsDeleted == false
                                                              select row).ToList();

                    for (int programCnt = 0; programCnt < lstProgram.Count; programCnt++)
                    {
                        activityId = Convert.ToString(lstProgram.ElementAt(programCnt).PlanProgramId);
                        DataRow programRow = dtPlanBudget.AsEnumerable().Where(row => row.Field<String>("activityid") == activityId && row.Field<String>("type") == "program").FirstOrDefault();
                        if (programRow != null)
                            dtPlanBudget.Rows.Remove(programRow);
                    }
                    //remove tactic  of that plan
                    List<Plan_Campaign_Program_Tactic> lstTactic = (from row in objDbMrpEntities.Plan_Campaign_Program_Tactic
                                                                    join objProgram in objDbMrpEntities.Plan_Campaign_Program on row.PlanProgramId equals objProgram.PlanProgramId
                                                                    join objCampaign in objDbMrpEntities.Plan_Campaign on objProgram.PlanCampaignId equals objCampaign.PlanCampaignId
                                                                    where objCampaign.PlanId.Equals(intActivityId) && row.IsDeleted == false && objCampaign.IsDeleted == false &&
                                                                    objProgram.IsDeleted == false
                                                                    select row).ToList();

                    for (int tacticCnt = 0; tacticCnt < lstTactic.Count; tacticCnt++)
                    {
                        activityId = Convert.ToString(lstTactic.ElementAt(tacticCnt).PlanTacticId);
                        DataRow tacticRow = dtPlanBudget.AsEnumerable().Where(row => row.Field<String>("activityid") == activityId && row.Field<String>("type") == "tactic").FirstOrDefault();
                        if (tacticRow != null)
                            dtPlanBudget.Rows.Remove(tacticRow);
                    }
                    //remove line item of that plan

                    List<Plan_Campaign_Program_Tactic_LineItem> lstLineItem = (from row in objDbMrpEntities.Plan_Campaign_Program_Tactic_LineItem
                                                                               join objtactic in objDbMrpEntities.Plan_Campaign_Program_Tactic on row.PlanTacticId equals objtactic.PlanTacticId
                                                                               join objProgram in objDbMrpEntities.Plan_Campaign_Program on objtactic.PlanProgramId equals objProgram.PlanProgramId
                                                                               join objCampaign in objDbMrpEntities.Plan_Campaign on objProgram.PlanCampaignId equals objCampaign.PlanCampaignId
                                                                               where objCampaign.PlanId.Equals(intActivityId) && row.IsDeleted == false && objCampaign.IsDeleted == false &&
                                                                               objProgram.IsDeleted == false
                                                                               select row).ToList();

                    for (int lineItemCnt = 0; lineItemCnt < lstLineItem.Count; lineItemCnt++)
                    {
                        activityId = Convert.ToString(lstLineItem.ElementAt(lineItemCnt).PlanLineItemId);
                        DataRow lineItemRow = dtPlanBudget.AsEnumerable().Where(row => row.Field<String>("activityid") == activityId && row.Field<String>("type") == "lineitem").FirstOrDefault();
                        if (lineItemRow != null)
                            dtPlanBudget.Rows.Remove(lineItemRow);
                    }

                }
               

            }
            return dtPlanBudget;
        }
        /// <summary>
        /// Following method is used to set column Names(as per month and quarter)
        /// </summary>
        /// <param name="dtPlanBudget"></param>
        /// <param name="isMonthly"></param>
        /// <returns></returns>
        private DataTable SetColumnName(DataTable dtPlanBudget, bool isMonthly)
        {
            //Assign column name "Task Name" to first column of datatable  
            dtPlanBudget.Columns[1].ColumnName = "Type";

            //Assign column name "Task Name" to first column of datatable  
            dtPlanBudget.Columns[2].ColumnName = "Task Name";
            //Assign column name "budget" to second column of datatable
            dtPlanBudget.Columns[3].ColumnName = "Budget";

            // dtPlanBudget.Rows.RemoveAt(0);
            int columnCount = dtPlanBudget.Columns.Count;///veriable to manage total column counts
            int columnRemovedCount = 0;///veriable to identify how many columns are removed
            int monthQuartercount = 1;

            if (isMonthly)
            {
                //Following loop start from "4" as first three column name are activityid,type,taskname
                for (int i = 4; i < columnCount; i++)
                {
                    if (monthQuartercount <= 12)
                        dtPlanBudget.Columns[i].ColumnName = ReturnMonthName(monthQuartercount);
                    else
                    {
                        //Remove extra columns
                        dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                        columnRemovedCount++;
                    }

                    monthQuartercount++;
                }
            }
            else
            {
                for (int i = 4; i < columnCount; i++)
                {
                    if (monthQuartercount <= 4)
                        dtPlanBudget.Columns[i].ColumnName = ReturnQuarter(monthQuartercount);
                    else
                    {
                        //Remove extra columns
                        dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                        columnRemovedCount++;
                    }

                    monthQuartercount++;
                }
            }
            return dtPlanBudget;
        }

        /// <summary>
        /// Following method is used to return month name as per month id
        /// </summary>
        /// <param name="monthid"></param>
        /// <returns></returns>
        private string ReturnMonthName(int monthid)
        {
            Dictionary<int, string> dctMonths = new Dictionary<int, string>()
        {
            {1,"JAN"},
            {2,"FEB"},
            {3,"MAR"},
            {4,"APR"},
            {5,"MAY"},
            {6,"JUN"},
            {7,"JUL"},
            {8,"AUG"},
            {9,"SEP"},
            {10,"OCT"},
            {11,"NOV"},
            {12,"DEC"}
        };
            return dctMonths.Where(w => w.Key.Equals(monthid)).FirstOrDefault().Value;
        }
        /// <summary>
        /// Following method is used to return Quarter name as per Quarter id
        /// </summary>
        /// <param name="quarterId"></param>
        /// <returns></returns>
        private string ReturnQuarter(int quarterId)
        {
            Dictionary<int, string> dctMonths = new Dictionary<int, string>()
        {
            {1,"Q1"},
            {2,"Q2"},
            {3,"Q3"},
            {4,"Q4"},

        };
            return dctMonths.Where(w => w.Key.Equals(quarterId)).FirstOrDefault().Value;
        }
        /// <summary>
        /// Convert value in to dollar from current current currency. 
        /// </summary>
        /// <param name="dtimportData"></param>
        /// <returns></returns>
        private DataTable ConvertValueAsperCurrency(DataTable dtimportData)
        {
            //following index is value column startindex
            int valueColumnIndex = 3;
            for (int i = valueColumnIndex; i < dtimportData.Columns.Count; i++)
            {
                RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
                for (int j = 0; j < dtimportData.Rows.Count; j++)
                {
                    if (i > 2 && dtimportData.Rows[j][i].ToString().Trim() != "")
                    {
                        dtimportData.Rows[j][i] = dtimportData.Rows[j][i].ToString().Replace(",", "").Replace("---", "");
                        if (!string.IsNullOrEmpty(Convert.ToString(dtimportData.Rows[j][i])))
                        {
                            double value = 0;
                            double.TryParse(Convert.ToString(dtimportData.Rows[j][i]), out value);
                            dtimportData.Rows[j][i] = Convert.ToString(objCurrency.SetValueByExchangeRate(value, Sessions.PlanExchangeRate));
                        }
                    }
                    else
                        dtimportData.Rows[j][i] = "0";
                }
            }

            return dtimportData;
        }
    }
}