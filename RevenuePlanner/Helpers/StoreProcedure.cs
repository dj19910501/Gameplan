using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml;

namespace RevenuePlanner.Helpers
{
    // Add By Nishant Sheth
    // Desc :: common methods for stroed procedures
    #region stored procedures methods

    public class StoredProcedure
    {
        SqlConnection Connection;
        SqlCommand command = null;
        MRPEntities db = new MRPEntities();

        private SqlConnection Conn_Open()
        {
            Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            return Connection;
        }
        private void Conn_Close()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
                Connection.Close();
        }

        /// <summary>
        /// Add By Nishant Sheth 
        /// Desc:: Import Marketing finance Data from excel 
        /// </summary>
        /// <param name="XMLData"></param>
        /// <param name="ImportBudgetCol"></param>
        /// <returns></returns>
        public int ImportMarketingFinance(XmlDocument XMLData, DataTable ImportBudgetCol, int BudgetDetailId = 0, bool IsMonthly = false)
        {
            Connection = Conn_Open();
            int ExecuteCommand = 0;
            string spname = string.Empty;

            if (!IsMonthly)
            {
                spname = "ImportMarketingBudgetQuarter";
            }
            else
            {
                spname = "ImportMarketingBudgetMonthly";
            }
            using (command = new SqlCommand(spname, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                command.Parameters.AddWithValue("@XMLData", XMLData.InnerXml);
                command.Parameters.AddWithValue("@ImportBudgetCol", ImportBudgetCol);
                command.Parameters.AddWithValue("@BudgetDetailId", BudgetDetailId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                ExecuteCommand = command.ExecuteNonQuery();
            }
            Conn_Close();
            return ExecuteCommand;

        }

        // Get List of Line Items
        public List<Plan_Campaign_Program_Tactic_LineItem> GetLineItemList(string planid)
        {
            DataTable datatable = new DataTable();
            SqlParameter[] para = new SqlParameter[1];
            para[0] = new SqlParameter
            {
                ParameterName = "PlanId",
                Value = planid
            };
            var data = db.Database.SqlQuery<RevenuePlanner.Models.Plan_Campaign_Program_Tactic_LineItem>("GetLineItemList @PlanId", para).ToList();
            return data;
        }

        // Get List of Plan, Campaign, Program, Tactic
        public DataSet GetListPlanCampaignProgramTactic(string planid)
        {
            DataSet dataset = new DataSet();
            Connection = Conn_Open();
            using (command = new SqlCommand("GetListPlanCampaignProgramTactic", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", planid);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        // Get Custom field entity
        public DataSet GetCustomFieldEntityList(string EntityType, int? CustomTypeId = null)
        {
            DataSet dataset = new DataSet();
            Connection = Conn_Open();
            using (command = new SqlCommand("GetCustomFieldEntityList", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CustomTypeId", CustomTypeId);
                command.Parameters.AddWithValue("@EntityType", EntityType);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        /// <summary>
        /// Added by Mitesh Vaishnav reg. PL ticket 1646
        /// This function returns datatable which contains details reg. plan, campaign, program, tactic and line item's planned cost and actual 
        /// </summary>
        /// <param name="PlanId">int unique planid of plan which data will be return</param>
        /// <param name="budgetTab">string which contains value like Planned or Actual</param>
        /// <returns></returns>
        public DataTable GetPlannedActualDetail(int PlanId, string budgetTab)
        {
            DataTable dtPlanHirarchy = new DataTable();
            Connection = Conn_Open();
            using (command = new SqlCommand("Plan_Budget_Cost_Actual_Detail", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID.ToString());
                command.Parameters.AddWithValue("@SelectedTab", budgetTab);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanHirarchy);
                Conn_Close();
            }
            return dtPlanHirarchy;
        }

        /// <summary>
        /// Added by Mitesh Vaishnav reg. PL ticket 2616
        /// This function returns datatable which contains details reg. plan, campaign, program, tactic and line item's budget data including all tabs budget,planned and actuals
        /// </summary>
        /// <param name="PlanIds">Comma seperated plan ids for which budget hierarchy will be return</param>
        /// <param name="OwnerIds">ownerids filter which will apply on budget hierarchy only for tactic</param>
        /// <param name="TacticTypeids">tactic type id filter which will apply on budget hierarchy only for tactic</param>
        /// <param name="StatusIds">Status Id filter which will apply on budget hierarchy only for tactic</param>
        /// <param name="Year">selected year from timeframe to filter plan data</param>
        /// <returns></returns>
        public DataTable GetBudget(string PlanIds, int UserId, string viewBy, string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string Year = "")
        {
            DataTable dtPlanBudgetHirarchy = new DataTable();
            Connection = Conn_Open();
            using (command = new SqlCommand("GetPlanBudget", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanIds);
                command.Parameters.AddWithValue("@ownerIds", OwnerIds);
                command.Parameters.AddWithValue("@tactictypeIds", TacticTypeids);
                command.Parameters.AddWithValue("@statusIds", StatusIds);
                command.Parameters.AddWithValue("@UserID", UserId);
                command.Parameters.AddWithValue("@TimeFrame", Year);
                command.Parameters.AddWithValue("@ViewBy", viewBy);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanBudgetHirarchy);
                Conn_Close();
            }
            return dtPlanBudgetHirarchy;
        }

        // Get Tactic line ite,
        public List<Plan_Campaign_Program_Tactic_LineItem> GetTacticLineItemList(string tacticId)
        {
           
            SqlParameter[] para = new SqlParameter[1];
            para[0] = new SqlParameter
            {
                ParameterName = "tacticId",
                Value = tacticId
            };
            var data = db.Database.SqlQuery<RevenuePlanner.Models.Plan_Campaign_Program_Tactic_LineItem>("GetTacticLineItemList @tacticId", para).ToList();
            return data;
        }

        // Get List of tactic type
        public List<TacticTypeModel> GetTacticTypeList(string lstAllowedEntityIds)
        {
          
            SqlParameter[] para = new SqlParameter[1];
            para[0] = new SqlParameter
            {
                ParameterName = "TacticIds",
                Value = lstAllowedEntityIds
            };
            var data = db.Database.SqlQuery<RevenuePlanner.Models.TacticTypeModel>("GetTacticTypeList @TacticIds", para).ToList();
            return data;
        }

        // Get list of view by 
        public List<ViewByModel> spViewByDropDownList(string planId)
        {
          
            List<ViewByModel> viewByListResult = new List<ViewByModel>();
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Tactic.ToString(), Value = PlanGanttTypes.Tactic.ToString() });
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Stage.ToString(), Value = PlanGanttTypes.Stage.ToString() });
            viewByListResult.Add(new ViewByModel { Text = PlanGanttTypes.Status.ToString(), Value = PlanGanttTypes.Status.ToString() });
            // Added by Arpita Soni for Ticket #2357 on 07/12/2016
            viewByListResult.Add(new ViewByModel { Text = Enums.DictPlanGanttTypes[PlanGanttTypes.ROIPackage.ToString()].ToString(), Value = Enums.DictPlanGanttTypes[PlanGanttTypes.ROIPackage.ToString()].ToString() });

            SqlParameter[] para = new SqlParameter[2];
            para[0] = new SqlParameter()
            {
                ParameterName = "PlanId",
                Value = string.Join(",", planId)
            };
            para[1] = new SqlParameter()
            {
                ParameterName = "ClientId",
                Value = Sessions.User.CID
            };
            var customViewBy = db.Database.SqlQuery<ViewByModel>("spViewByDropDownList @PlanId,@ClientId", para).ToList();
            return viewByListResult = viewByListResult.Concat(customViewBy).ToList();
        }

        public DataSet GetExportCSV(int PlanId, string HoneyCombids = null)
        {
            DataSet dataset = new DataSet();
            Connection = Conn_Open();
            using (command = new SqlCommand("ExportToCSV", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                command.Parameters.AddWithValue("@HoneyCombids", HoneyCombids);
                command.Parameters.AddWithValue("@CurrencyExchangeRate", Sessions.PlanExchangeRate);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }
        /// <summary>
        /// Add By Nishant Sheth 
        /// Desc:: Get list of budget list and line item budget list
        /// </summary>
        /// <param name="BudgetId"></param>
        /// <returns></returns>
        public DataSet GetBudgetListAndLineItemBudgetList(int BudgetId = 0)
        {
            DataSet dataset = new DataSet();
          
            Connection = Conn_Open();
            using (command = new SqlCommand("GetBudgetListAndLineItemBudgetList", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                command.Parameters.AddWithValue("@BudgetId", BudgetId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        //Function to get the measure dashboard content - under reports section of the application
        public DataSet GetDashboardContent(int HomepageId = 0, int DashboardId = 0, int DashboardPageId = 0)
        {
            DataSet ds = new DataSet();
           
            Connection = Conn_Open();
            using (command = new SqlCommand("GetDashboardContent", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@HomepageId", 0);
                command.Parameters.AddWithValue("@DashboardId", DashboardId);
                command.Parameters.AddWithValue("@DashboardPageId", 0);
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(ds);
                Conn_Close();
            }
            return ds;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Desc :: Get list of dashbaod menu for measure reports
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="DashboardID"></param>
        /// <returns></returns>
        public DataSet GetDashboarContentData(int UserId, int DashboardID = 0)
        {
            DataSet dataset = new DataSet();
            Connection = Conn_Open();
            using (command = new SqlCommand("GetDashboardContentData", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", UserId);
                command.Parameters.AddWithValue("@DashboardID", DashboardID);
                command.CommandTimeout = 0;
                SqlDataAdapter adp = new SqlDataAdapter(command);
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        /// <summary>
        /// Added by Rushil Bhuptani on 15/06/2016 for #2227 
        /// Method to updated imported excel data in database.
        /// </summary>
        /// <param name="dtNew">Datatable containing excel data.</param>
        /// <param name="isMonthly">Flag to indicate whether data is monthly or quarterly.</param>
        /// <param name="userId">Id of user.</param>
        /// <returns>Dataset with conflicted ActivityIds.</returns>
        public DataSet ImportPlanBudgetList(DataTable dtNew, bool isMonthly, int userId)
        {
            DataSet dataset = new DataSet();
            string spname = string.Empty;
            Connection = Conn_Open();
            if (!isMonthly)
            {
                spname = "ImportPlanBudgetDataQuarterly";
            }
            else
            {
                spname = "ImportPlanBudgetDataMonthly";
            }
            using (command = new SqlCommand(spname, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ImportData", dtNew);
                command.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }
        /// <summary>
        /// Following method is used to import actuals data 29/09/2016 #2637 Kausha.
        /// </summary>
        /// <param name="dtNew"></param>
        /// <param name="isMonthly"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataSet ImportPlanActualList(DataTable dtNew, bool isMonthly, int userId)
        {
            DataSet dataset = new DataSet();
            string spname = string.Empty;
            Connection = Conn_Open();
            if (!isMonthly)
            {
                spname = "ImportPlanActualDataQuarterly";
            }
            else
            {
                spname = "ImportPlanActualDataMonthly";
            }
            using (command = new SqlCommand(spname, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ImportData", dtNew);
                command.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        /// <summary>
        /// Following method is used to import actuals data 29/09/2016 #2637 Kausha.
        /// </summary>
        /// <param name="dtNew"></param>
        /// <param name="isMonthly"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataSet ImportPlanCostList(DataTable dtNew, bool isMonthly, int userId)
        {
            DataSet dataset = new DataSet();
            string spname = string.Empty;
            Connection = Conn_Open();
            if (!isMonthly)
            {
                spname = "ImportPlanCostDataQuarterly";
            }
            else
            {
                spname = "ImportPlanCostDataMonthly";
            }
            using (command = new SqlCommand(spname, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ImportData", dtNew);
                command.Parameters.AddWithValue("@UserId", userId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                Conn_Close();
            }
            return dataset;
        }

        /// <summary>
        /// Following method is used to import actuals data 29/09/2016 #2637 Kausha.
        /// </summary>
        /// <param name="dtNew"></param>
        /// <param name="isMonthly"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        //public DataSet ImportPlanActuals(DataTable dtNew, bool isMonthly, int userId)
        //{
        //    DataSet dataset = new DataSet();
        //    string spname = string.Empty;
        //    Connection = Conn_Open();
        //    if (!isMonthly)
        //    {
        //        spname = "Sp_GetPlanActualDataQuarterly";
        //    }
        //    else
        //    {
        //        spname = "Sp_GetPlanActualDataMonthly";
        //    }
        //    using (command = new SqlCommand(spname, Connection))
        //    {
        //        command.CommandType = CommandType.StoredProcedure;
        //        //  command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
        //        command.Parameters.AddWithValue("@ImportData", dtNew);
        //        command.Parameters.AddWithValue("@UserId", userId);
        //        SqlDataAdapter adp = new SqlDataAdapter(command);
        //        command.CommandTimeout = 0;
        //        adp.Fill(dataset);
        //        Conn_Close();
        //    }
        //    return dataset;
        //}

        public string GetColumnValue(string Query)
        {
            Connection = Conn_Open();
            using (command = new SqlCommand(Query, Connection))
            {
                command.CommandTimeout = 120;
                command.CommandType = CommandType.Text;
                command.CommandText = Query; SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                object ColumnValue = command.ExecuteScalar();
                Conn_Close();
                if (ColumnValue == null && ColumnValue == System.DBNull.Value)
                {
                    return "";
                }
                else
                {
                    return Convert.ToString(ColumnValue);
                }                
            }
        }

        public List<CustomDashboardModel> GetCustomDashboardsClientwise(int UserId, int ClientId)
        {
            SqlParameter[] para = new SqlParameter[2];
            para[0] = new SqlParameter
            {
                ParameterName = "UserId",
                Value = UserId
            };
            para[1] = new SqlParameter
            {
                ParameterName = "ClientId",
                Value = ClientId
            };
            var data = db.Database.SqlQuery<RevenuePlanner.Models.CustomDashboardModel>("GetCustomDashboardsClientwise @UserId,@ClientId", para).ToList();
            return data;
        }

        //Added by komal rawal on 16-08-2016 regarding #2484 save notifications 
        public int SaveLogNoticationdata(string action, string actionSuffix, int? componentId, string componentTitle, string description, int? objectid, int? parentObjectId, string TableName, int ClientId, int User, string UserName, int EntityOwnerID, string ReportRecipientUserIds)
        {
            int returnvalue = 0;
           
             List<int> lst_RecipientId = new List<int>();
            if (description == Convert.ToString(Enums.ChangeLog_ComponentType.tactic).ToLower() && componentId != null)
            {
                if (action == Convert.ToString(Enums.ChangeLog_Actions.submitted))
                {
                    BDSService.BDSServiceClient objBDSUserRepository = new BDSService.BDSServiceClient();
                    var lstUserHierarchy = objBDSUserRepository.GetUserHierarchyEx(Sessions.User.CID, Sessions.ApplicationId);
                    var objOwnerUser = lstUserHierarchy.FirstOrDefault(u => u.UID == EntityOwnerID);
                    lst_RecipientId.Add(objOwnerUser.MID);
                    lst_RecipientId.Add(EntityOwnerID);
                }
                else
                {
                    lst_RecipientId = Common.GetCollaboratorForTactic(Convert.ToInt32(componentId));
                }
            }
            else if (description == Convert.ToString(Enums.ChangeLog_ComponentType.program).ToLower() && componentId != null)
            {
                lst_RecipientId = Common.GetCollaboratorForProgram(Convert.ToInt32(componentId));
            }
            else if (description == Convert.ToString(Enums.ChangeLog_ComponentType.campaign).ToLower() && componentId != null)
            {
                lst_RecipientId = Common.GetCollaboratorForCampaign(Convert.ToInt32(componentId));
            }
            else if ((description == Convert.ToString(Enums.ChangeLog_ComponentType.plan).ToLower() || description == Convert.ToString(Enums.PlanEntityValues[Enums.PlanEntity.LineItem.ToString()]).ToLower()) && componentId != null && EntityOwnerID != 0)
            {
                lst_RecipientId.Add(EntityOwnerID);
            }

            string RecipientIds = null;

            if (lst_RecipientId.Count > 0)
            {
                RecipientIds = String.Join(",", lst_RecipientId);

            }
            else if (TableName == Convert.ToString(Enums.ChangeLog_TableName.Report) && action == Convert.ToString(Enums.ChangeLog_Actions.shared))
            {
                RecipientIds = ReportRecipientUserIds;
            }
            Connection = Conn_Open();
            using (command = new SqlCommand("SaveLogNoticationdata", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@action", action);
                command.Parameters.AddWithValue("@actionSuffix", actionSuffix);
                command.Parameters.AddWithValue("@componentId", componentId);
                command.Parameters.AddWithValue("@componentTitle", componentTitle);
                command.Parameters.AddWithValue("@description", description);
                command.Parameters.AddWithValue("@objectId", objectid);
                command.Parameters.AddWithValue("@parentObjectId", parentObjectId);
                command.Parameters.AddWithValue("@TableName", TableName);
                command.Parameters.AddWithValue("@Userid", User);
                command.Parameters.AddWithValue("@ClientId", ClientId);
                command.Parameters.AddWithValue("@UserName", UserName);
                command.Parameters.AddWithValue("@RecipientIDs", RecipientIds);
                command.Parameters.AddWithValue("@EntityOwnerID", EntityOwnerID);
                string returnvalue1 = command.ExecuteScalar().ToString();
                //  adp.Fill(dataset);
                returnvalue = Convert.ToInt32(returnvalue1);
                Conn_Close();
            }
            return returnvalue;
        }

        //Added by Komal Rawal on 16-09-2016 to get goal values in header for plans
        public List<GoalValueModel> spgetgoalvalues(string Planids)
        {
            List<GoalValueModel> GoalValues = new List<GoalValueModel>();
            SqlParameter[] para = new SqlParameter[2];
            para[0] = new SqlParameter()
            {
                ParameterName = "PlanId",
                Value = Planids
            };
            para[1] = new SqlParameter()
            {
                ParameterName = "ClientId",
                Value = Sessions.User.CID
            };
            var CustomGoalValues = db.Database.SqlQuery<GoalValueModel>("spGetGoalValuesForPlan @PlanId,@ClientId", para).ToList();
            return GoalValues = GoalValues.Concat(CustomGoalValues).ToList();
        }
        //End

        // Add by Nishant Sheth
        // Get list of tactic ids based on clients custom field ids
        public List<CustomField_Entity> GetTacticIdsOnCustomField(int ClientId, int UserId)
        {
            List<CustomField_Entity> EntityList = new List<CustomField_Entity>();
            bool IsDefaultCustomRestrictionsViewable = Common.IsDefaultCustomRestrictionsViewable();
            SqlParameter[] para = new SqlParameter[3];

            para[0] = new SqlParameter { ParameterName = "userId", Value = UserId };

            para[1] = new SqlParameter { ParameterName = "ClientId", Value = ClientId };

            para[2] = new SqlParameter { ParameterName = "IsDefaultCustomRestrictionsViewable", Value = IsDefaultCustomRestrictionsViewable };

            EntityList = db.Database.SqlQuery<CustomField_Entity>("GetTacticIdsOnCustomField @userId,@ClientId,@IsDefaultCustomRestrictionsViewable", para)
                .ToList();
            return EntityList;
        }
    }
    #endregion
}