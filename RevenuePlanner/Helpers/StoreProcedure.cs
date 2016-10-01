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
        /// <summary>
        /// Add By Nishant Sheth 
        /// Desc:: Import Marketing finance Data from excel 
        /// </summary>
        /// <param name="XMLData"></param>
        /// <param name="ImportBudgetCol"></param>
        /// <returns></returns>
        public int ImportMarketingFinance(XmlDocument XMLData, DataTable ImportBudgetCol, int BudgetDetailId = 0, bool IsMonthly = false)
        {
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;
            int ExecuteCommand = 0;
            try
            {
                if (!IsMonthly)
                {
                    command = new SqlCommand("ImportMarketingBudgetQuarter", Connection);
                }
                else
                {
                    command = new SqlCommand("ImportMarketingBudgetMonthly", Connection);
                }
                using (command)
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
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            return ExecuteCommand;
        }

        // Get List of Line Items
        public List<Plan_Campaign_Program_Tactic_LineItem> GetLineItemList(string planid)
        {
            DataTable datatable = new DataTable();
            MRPEntities db = new MRPEntities();

            //List<SqlParameter> para = new List<SqlParameter>();
            //para.Add(new SqlParameter { ParameterName = "@PlanId", Value = planid });
            SqlParameter[] para = new SqlParameter[1];

            para[0] = new SqlParameter
            {
                ParameterName = "PlanId",
                Value = planid
            };

            var data = db.Database.SqlQuery<RevenuePlanner.Models.Plan_Campaign_Program_Tactic_LineItem>("GetLineItemList @PlanId", para).ToList();


            ///If connection is closed then it will be open
            //    var Connection = db.Database.Connection as SqlConnection;
            //    if (Connection.State == System.Data.ConnectionState.Closed)
            //        Connection.Open();
            //    SqlCommand command = null;

            //    command = new SqlCommand("GetLineItemList", Connection);

            //    using (command)
            //    {

            //        command.CommandType = CommandType.StoredProcedure;
            //        command.Parameters.AddWithValue("@PlanId", planid);
            //        SqlDataAdapter adp = new SqlDataAdapter(command);
            //        command.CommandTimeout = 0;
            //        adp.Fill(datatable);
            //        if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();

            //    }
            return data;
        }

        // Get List of Plan, Campaign, Program, Tactic
        public DataSet GetListPlanCampaignProgramTactic(string planid)
        {
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetListPlanCampaignProgramTactic", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", planid);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dataset;
        }

        // Get Custom field entity
        public DataSet GetCustomFieldEntityList(string EntityType, int? CustomTypeId = null)
        {

            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetCustomFieldEntityList", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CustomTypeId", CustomTypeId);
                command.Parameters.AddWithValue("@EntityType", EntityType);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
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

            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("Plan_Budget_Cost_Actual_Detail", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID.ToString());
                command.Parameters.AddWithValue("@SelectedTab", budgetTab);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanHirarchy);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
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
        public DataTable GetBudget(string PlanIds, int UserId, string OwnerIds = "", string TacticTypeids = "", string StatusIds = "", string Year = "")
        {
            DataTable dtPlanBudgetHirarchy = new DataTable();

            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetPlanBudget", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanIds);
                command.Parameters.AddWithValue("@ownerIds", OwnerIds);
                command.Parameters.AddWithValue("@tactictypeIds", TacticTypeids);
                command.Parameters.AddWithValue("@statusIds", StatusIds);
                command.Parameters.AddWithValue("@UserID", UserId);
                command.Parameters.AddWithValue("@TimeFrame", Year);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanBudgetHirarchy);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dtPlanBudgetHirarchy;
        }

        /// <summary>
        /// 
        /// This function returns datatable which contains details reg. plan, campaign, program, tactic and line item's planned cost and actual 
        /// </summary>
        /// <param name="PlanId">int unique planid of plan which data will be return</param>
        /// <param name="budgetTab">string which contains value like Planned or Actual</param>
        /// <returns></returns>
        public DataTable GetLineItemCostAllocation(int LineItemId)
        {
            DataTable dtPlanHirarchy = new DataTable();

            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("LineItem_Cost_Allocation", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanTacticId", LineItemId);
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dtPlanHirarchy);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }

            return dtPlanHirarchy;
        }

        // Get Tactic line ite,
        public List<Plan_Campaign_Program_Tactic_LineItem> GetTacticLineItemList(string tacticId)
        {

            MRPEntities db = new MRPEntities();

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
            MRPEntities db = new MRPEntities();
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
            MRPEntities db = new MRPEntities();
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
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("ExportToCSV", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PlanId", PlanId);
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                command.Parameters.AddWithValue("@HoneyCombids", HoneyCombids);
                command.Parameters.AddWithValue("@CurrencyExchangeRate", Sessions.PlanExchangeRate);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
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
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetBudgetListAndLineItemBudgetList", Connection);

            using (command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ClientId", Sessions.User.CID);
                command.Parameters.AddWithValue("@BudgetId", BudgetId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
            return dataset;
        }

        public DataSet GetDashboardContent(int HomepageId = 0, int DashboardId = 0, int DashboardPageId = 0)
        {
            DataSet ds = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetDashboardContent", Connection);

            using (command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@HomepageId", 0);
                command.Parameters.AddWithValue("@DashboardId", DashboardId);
                command.Parameters.AddWithValue("@DashboardPageId", 0);
                command.Parameters.AddWithValue("@UserId", Sessions.User.ID);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                adp.Fill(ds);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
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
            DataTable datatable = new DataTable();
            DataSet dataset = new DataSet();
            MRPEntities db = new MRPEntities();
            ///If connection is closed then it will be open
            var Connection = db.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
            SqlCommand command = null;

            command = new SqlCommand("GetDashboardContentData", Connection);

            using (command)
            {

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", UserId);
                command.Parameters.AddWithValue("@DashboardID", DashboardID);
                command.CommandTimeout = 0;
                SqlDataAdapter adp = new SqlDataAdapter(command);
                adp.Fill(dataset);
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
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
            try
            {
                // DataTable datatable = new DataTable();

                MRPEntities db = new MRPEntities();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;
                if (!isMonthly)
                {
                    command = new SqlCommand("Sp_ImportPlanBudgetDataQuarterly", Connection);
                }
                else
                {
                    command = new SqlCommand("Sp_ImportPlanBudgetDataMonthly", Connection);
                }

                using (command)
                {

                    command.CommandType = CommandType.StoredProcedure;
                    // command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
                    command.Parameters.AddWithValue("@ImportData", dtNew);
                    command.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;

                    // Modified by Rushil Bhuptani on 21/06/2016 for ticket #2267 for showing message for conflicting data.
                    adp.Fill(dataset);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
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
            try
            {
                //DataTable datatable = new DataTable();

                MRPEntities db = new MRPEntities();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;
                if (!isMonthly)
                {
                    command = new SqlCommand("Sp_ImportPlanActualDataQuarterly", Connection);
                }
                else
                {
                    command = new SqlCommand("Sp_ImportPlanActualDataMonthly", Connection);
                }

                using (command)
                {

                    command.CommandType = CommandType.StoredProcedure;
                    //  command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
                    command.Parameters.AddWithValue("@ImportData", dtNew);
                    command.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;


                    adp.Fill(dataset);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
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
            try
            {
                //DataTable datatable = new DataTable();

                MRPEntities db = new MRPEntities();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;
                if (!isMonthly)
                {
                    command = new SqlCommand("Sp_ImportPlanCostDataQuarterly", Connection);
                }
                else
                {
                    command = new SqlCommand("Sp_ImportPlanCostDataMonthly", Connection);
                }

                using (command)
                {

                    command.CommandType = CommandType.StoredProcedure;
                    //  command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
                    command.Parameters.AddWithValue("@ImportData", dtNew);
                    command.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;
                    adp.Fill(dataset);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
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
        public DataSet ImportPlanActuals(DataTable dtNew, bool isMonthly, int userId)
        {
            DataSet dataset = new DataSet();
            try
            {
                //DataTable datatable = new DataTable();
                
                MRPEntities db = new MRPEntities();
                ///If connection is closed then it will be open
                var Connection = db.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;
                if (!isMonthly)
                {
                    command = new SqlCommand("Sp_GetPlanActualDataQuarterly", Connection);
                }
                else
                {
                    command = new SqlCommand("Sp_GetPlanActualDataMonthly", Connection);
                }

                using (command)
                {

                    command.CommandType = CommandType.StoredProcedure;
                    //  command.Parameters.AddWithValue("@PlanId", Convert.ToInt32(dtNew.Rows[0][0]));
                    command.Parameters.AddWithValue("@ImportData", dtNew);
                    command.Parameters.AddWithValue("@UserId", userId);
                    SqlDataAdapter adp = new SqlDataAdapter(command);
                    command.CommandTimeout = 0;

              
                    adp.Fill(dataset);
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return dataset;
        }

        public string GetColumnValue(string Query)
        {
            SqlConnection DbConn = new SqlConnection();
            SqlDataAdapter DbAdapter = new SqlDataAdapter();
            SqlCommand DbCommand = new SqlCommand();
            try
            {
                if (DbConn.State == 0)
                {
                    try
                    {
                        MRPEntities mp = new MRPEntities();
                        DbConn.ConnectionString = mp.Database.Connection.ConnectionString;
                        DbConn.Open();
                    }
                    catch (Exception exp)
                    {
                        throw exp;
                    }
                }
                DbCommand.CommandTimeout = 120;
                DbCommand.Connection = DbConn;
                DbCommand.CommandType = CommandType.Text;
                DbCommand.CommandText = Query;


                object objResult = DbCommand.ExecuteScalar();
                if (objResult == null)
                {
                    return "";
                }
                if (objResult == System.DBNull.Value)
                {
                    return "";
                }
                else
                {
                    return Convert.ToString(objResult);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                DbAdapter.Dispose();
                DbConn.Close();
            }
        }

        public List<CustomDashboardModel> GetCustomDashboardsClientwise(int UserId, int ClientId)
        {
            DataTable datatable = new DataTable();
            MRPEntities db = new MRPEntities();

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
            MRPEntities db = new MRPEntities();
            var Connection = db.Database.Connection as SqlConnection;
            try
            {
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
                ///If connection is closed then it will be open

                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                SqlCommand command = null;

                command = new SqlCommand("SaveLogNoticationdata", Connection);

                using (command)
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
                    if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
                }

                return returnvalue;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (Connection.State == System.Data.ConnectionState.Open) Connection.Close();
            }
        }

        //ENd


        //Added by Komal Rawal on 16-09-2016 to get goal values in header for plans
        public List<GoalValueModel> spgetgoalvalues(string Planids)
        {
            MRPEntities db = new MRPEntities();
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
    }
    #endregion
}