using Excel;
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
            objDbMrpEntities = new MRPEntities();
        }
        /// <summary>
        /// Following function will return filtered datatable as per planBudgetType.
        /// </summary>
        /// <param name="importData"></param>
        /// <param name="planBudgetType"></param>
        /// <returns></returns>
        public DataTable GetPlanBudgetDataByType(DataTable importData, string planBudgetType,bool isMonthly)
        {
            DataTable dtPlanBudget = importData;
            try
            {
                //planBudgetType veriable is used to identify type of plan budget(ie>plan,budget,actual) 

                int columnRemovedCount = 0;//veriable to identify how many columns are removed
                int columnCount = dtPlanBudget.Columns.Count;//veriable to identify total column in datatable           
                for (int i = 3; i < columnCount; i++) //Loop to remove columns which is not related to planBudgetType.
                {
                    if (!Convert.ToString(dtPlanBudget.Rows[0][i - columnRemovedCount]).ToLower().Trim().Contains(planBudgetType))
                    {
                        dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                        columnRemovedCount++;
                    }
                }
                if (dtPlanBudget.Rows[dtPlanBudget.Rows.Count - 1][0].ToString().Trim() ==
                              "This document was made with dhtmlx library. http://dhtmlx.com")
                {
                    dtPlanBudget.Rows.RemoveAt(dtPlanBudget.Rows.Count - 1);
                }
                // dtPlanBudget.AcceptChanges();
                dtPlanBudget = SetColumnName(dtPlanBudget, isMonthly);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return ConvertValueAsperCurrency(dtPlanBudget);
        }
        private DataTable SetColumnName(DataTable dtPlanBudget,bool isMonthly)
        {
            try
            {
                //Assign column name "Task Name" to first column of datatable  
                dtPlanBudget.Columns[1].ColumnName = "Type";

                //Assign column name "Task Name" to first column of datatable  
                dtPlanBudget.Columns[2].ColumnName = "Task Name";
                //Assign column name "budget" to second column of datatable
                dtPlanBudget.Columns[3].ColumnName = "Budget";

                dtPlanBudget.Rows.RemoveAt(0);
                int columnCount = dtPlanBudget.Columns.Count;///veriable to manage total column counts
                int columnRemovedCount = 0;///veriable to identify how many columns are removed
                int monthQuartercount = 1;
              
                if (isMonthly)
                {
                    for (int i = 4; i < columnCount; i++)
                    {
                        if (monthQuartercount <= 12)
                            dtPlanBudget.Columns[i].ColumnName = ReturnMonthName(monthQuartercount);
                        else
                        {
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
                            dtPlanBudget.Columns.RemoveAt(i - columnRemovedCount);
                            columnRemovedCount++;
                        }

                        monthQuartercount++;
                    }
                }
                //   dtPlanBudget.AcceptChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return dtPlanBudget;
        }

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
        /// 
        /// </summary>
        /// <param name="dtimportData"></param>
        /// <returns></returns>
        private DataTable ConvertValueAsperCurrency(DataTable dtimportData)
        {
            try
            {
                for (int i = 0; i < dtimportData.Columns.Count; i++)
                {
                    //insertation start 20/08/2016 #2504 Multi-Currency: used SetValueByExchangeRate method to update cell value
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
                    }
                    //insertation End 20/08/2016 #2504 Multi-Currency: used SetValueByExchangeRate method to update cell value
                }
                //dtimportData.AcceptChanges();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return dtimportData;
        }
    }
}