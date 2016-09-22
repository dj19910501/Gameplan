using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Grid : IGrid
    {
        #region Declartion
        private MRPEntities objDbMrpEntities;
        List<Plandataobj> plandataobjlistCreateItem = new List<Plandataobj>();
        List<Plandataobj> EntitydataobjCreateItem = new List<Plandataobj>();
        List<CustomfieldPivotData> EntityCustomDataValues = new List<CustomfieldPivotData>();
        List<Plandataobj> EmptyCustomValues;
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        HomeGridProperties objHomeGridProp = new HomeGridProperties();
        #endregion

        // Constructor
        public Grid()
        {
            objDbMrpEntities = new MRPEntities();
        }

        #region Plan Grid Methods

        #region Method to get grid default data
        /// <summary>
        /// Add By Nishant Sheth
        /// call stored procedure to get list of entities for plan home grid
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <param name="ClientId"></param>
        /// <param name="ownerIds"></param>
        /// <param name="TacticTypeid"></param>
        /// <param name="StatusIds"></param>
        /// <param name="customFieldIds"></param>
        /// <returns></returns>
        public List<GridDefaultModel> GetGridDefaultData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {
            List<GridDefaultModel> EntityList = new List<GridDefaultModel>();
            DataTable datatable = new DataTable();
            var Connection = objDbMrpEntities.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();

            try
            {
                SqlParameter[] para = new SqlParameter[5];

                para[0] = new SqlParameter
                {
                    ParameterName = "PlanId",
                    Value = PlanIds
                };

                para[1] = new SqlParameter
                {
                    ParameterName = "ClientId",
                    Value = ClientId
                };

                para[2] = new SqlParameter
                {
                    ParameterName = "OwnerIds",
                    Value = ownerIds
                };

                para[3] = new SqlParameter
                {
                    ParameterName = "TacticTypeIds",
                    Value = TacticTypeid
                };

                para[4] = new SqlParameter
                {
                    ParameterName = "StatusIds",
                    Value = StatusIds
                };

                EntityList = objDbMrpEntities.Database.SqlQuery<RevenuePlanner.Models.GridDefaultModel>("GetGridData @PlanId,@ClientId,@OwnerIds,@TacticTypeIds,@StatusIds", para).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return EntityList;
        }
        #endregion

        #region Mtehod to get grid customfield and it's entity value

        public GridCustomColumnData GetGridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {

            GridCustomColumnData EntityList = new GridCustomColumnData();
            DataTable datatable = new DataTable();
            var Connection = objDbMrpEntities.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();

            // Create a SQL command to execute the sproc 
            var cmd = objDbMrpEntities.Database.Connection.CreateCommand();
            cmd.CommandText = "[dbo].[GridCustomFieldData]";
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {

                DbParameter paraPlanId = cmd.CreateParameter();
                paraPlanId.ParameterName = "PlanId";
                paraPlanId.DbType = DbType.String;
                paraPlanId.Direction = ParameterDirection.Input;
                paraPlanId.Value = PlanIds;

                DbParameter paraClientId = cmd.CreateParameter();
                paraClientId.ParameterName = "ClientId";
                paraClientId.DbType = DbType.Int32;
                paraClientId.Direction = ParameterDirection.Input;
                paraClientId.Value = ClientId;

                DbParameter paraOwnerIds = cmd.CreateParameter();
                paraOwnerIds.ParameterName = "OwnerIds";
                paraOwnerIds.DbType = DbType.String;
                paraOwnerIds.Direction = ParameterDirection.Input;
                paraOwnerIds.Value = string.Join(",", ownerIds);

                DbParameter paraTacticTypeIds = cmd.CreateParameter();
                paraTacticTypeIds.ParameterName = "TacticTypeIds";
                paraTacticTypeIds.DbType = DbType.String;
                paraTacticTypeIds.Direction = ParameterDirection.Input;
                paraTacticTypeIds.Value = TacticTypeid;

                DbParameter paraStatusIds = cmd.CreateParameter();
                paraStatusIds.ParameterName = "StatusIds";
                paraStatusIds.DbType = DbType.String;
                paraStatusIds.Direction = ParameterDirection.Input;
                paraStatusIds.Value = StatusIds;

                cmd.Parameters.Add(paraPlanId);
                cmd.Parameters.Add(paraClientId);
                cmd.Parameters.Add(paraOwnerIds);
                cmd.Parameters.Add(paraTacticTypeIds);
                cmd.Parameters.Add(paraStatusIds);

                // Run the sproc  
                var reader = cmd.ExecuteReader();

                // Read CustomField from the first result set 
                var CustomFields = ((IObjectContextAdapter)objDbMrpEntities)
                    .ObjectContext
                    .Translate<GridCustomFields>(reader).ToList();

                // Move to second result set and read Posts // Get Custom fields value
                reader.NextResult();
                var CustomFieldEntityValues = ((IObjectContextAdapter)objDbMrpEntities)
                    .ObjectContext
                    .Translate<GridCustomFieldEntityValues>(reader).ToList();


                EntityList.CustomFields = CustomFields;
                EntityList.CustomFieldValues = CustomFieldEntityValues;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return EntityList;
        }
        #endregion

        #region Get Model for Grid Data
        /// <summary>
        /// Add By Nishant Sheth
        /// Get plan grid data with default and custom fields columns 
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <param name="ClientId"></param>
        /// <param name="ownerIds"></param>
        /// <param name="TacticTypeid"></param>
        /// <param name="StatusIds"></param>
        /// <param name="customFieldIds"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <returns></returns>
        public PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            //Get list of entities for plan grid
            var GridHireachyData = GetGridDefaultData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);

            // Get MQL title label
            List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false)
                .Select(stage => stage).ToList();
            string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();

            // Genrate header methods for default columns
            var ListOfDefaultColumnHeader = GenerateJsonHeader(MQLTitle);

            // Get List of customfields and it's entity's values
            var ListOfCustomData = GridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);

            // Merge header of plan grid with custom fields
            GridCustomHead(ListOfCustomData.CustomFields, ref ListOfDefaultColumnHeader);

            // Round up the Revnue and mql value for Progam/Campaign/Plan
            RoundupRevnueMqlforHireachyData(ref GridHireachyData);

            // Genrate Hireachy of Plan grid
            var griditems = GetTopLevelRowsGrid(GridHireachyData, null)
                     .Select(row => CreateItemGrid(GridHireachyData, row, ListOfCustomData, PlanCurrencySymbol, PlanExchangeRate))
                     .ToList();

            objPlanMainDHTMLXGrid.head = ListOfDefaultColumnHeader;
            objPlanMainDHTMLXGrid.rows = griditems;
            return objPlanMainDHTMLXGrid;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set background color for differnt entities
        /// </summary>
        /// <param name="EntityType"></param>
        /// <returns></returns>
        public string GridBackgroundColor(string EntityType)
        {
            try
            {
                EntityType = EntityType.ToLower();
                if (EntityType == Convert.ToString(Enums.EntityType.Plan).ToLower())
                {
                    return objHomeGridProp.PlanBackgroundColor;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Campaign).ToLower())
                {
                    return objHomeGridProp.CampaignBackgroundColor;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Program).ToLower())
                {
                    return objHomeGridProp.ProgramBackgroundColor;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Tactic).ToLower())
                {
                    return objHomeGridProp.TacticBackgroundColor;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Lineitem).ToLower())
                {
                    return objHomeGridProp.LineItemBackgroundColor;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return string.Empty;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get list of custom fields values for each entities
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <param name="ClientId"></param>
        /// <param name="ownerIds"></param>
        /// <param name="TacticTypeid"></param>
        /// <param name="StatusIds"></param>
        /// <param name="customFieldIds"></param>
        /// <returns></returns>
        public GridCustomColumnData GridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {
            GridCustomColumnData data = new GridCustomColumnData();
            try
            {
                // Call the method of stored procedure that return list of custom field and it's entities values
                data = GetGridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);

                // Pivoting the customfields entities values
                EntityCustomDataValues = data.CustomFieldValues.ToPivotList(item => item.CustomFieldId,
                         item => item.EntityId,
                            items => items.Max(a => a.Value))
                            .ToList();

                List<Plandataobj> lstCustomPlanData = new List<Plandataobj>();
                // Create empty list for where there is no any custom fields value on entity
                foreach (var objCustomField in data.CustomFields)
                {
                    lstCustomPlanData.Add(new Plandataobj
                        {
                            value = string.Empty,
                            locked = objHomeGridProp.lockedstateone,
                            style = objHomeGridProp.stylecolorblack
                        });
                }
                EmptyCustomValues = lstCustomPlanData;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return data;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Create header of custom fields for plan grid
        /// </summary>
        /// <param name="lstCustomFields"></param>
        /// <param name="ListHead"></param>
        public void GridCustomHead(List<GridCustomFields> lstCustomFields, ref List<PlanHead> ListHead)
        {
            try
            {
                PlanHead headobj = new PlanHead();
                foreach (var CustomField in lstCustomFields)
                {
                    headobj = new PlanHead();
                    headobj.type = "ed";
                    headobj.id = "custom_" + CustomField.CustomFieldId;
                    headobj.sort = "str";
                    headobj.width = 150;
                    headobj.value = CustomField.CustomFieldName;
                    ListHead.Add(headobj);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

        }

        /// <summary>
        /// Add by Nishant Sheth
        /// set open/close entity state for respective entity
        /// </summary>
        /// <param name="EntityType"></param>
        /// <returns></returns>
        public string GridEntityOpenState(string EntityType)
        {
            try
            {
                EntityType = EntityType.ToLower();
                if (EntityType == Convert.ToString(Enums.EntityType.Plan).ToLower())
                {
                    return objHomeGridProp.openstateone;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Campaign).ToLower())
                {
                    return objHomeGridProp.openstateone;
                }
                else if (EntityType == Convert.ToString(Enums.EntityType.Program).ToLower())
                {
                    return objHomeGridProp.openstateone;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return string.Empty;
        }
        #endregion

        #region Method to generate grid header
        /// <summary>
        /// Add By Nishant Sheth
        /// Create Plan Grid header for default columns
        /// </summary>
        /// <param name="MQLTitle"></param>
        /// <param name="IsNotLineItemListing"></param>
        /// <returns></returns>
        public List<PlanHead> GenerateJsonHeader(string MQLTitle, bool IsNotLineItemListing = true)
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            PlanHead headobj = new PlanHead();
            List<PlanOptions> lstOwner = new List<PlanOptions>();
            List<PlanOptions> lstTacticType = new List<PlanOptions>();
            try
            {

                // First Column Activity Type
                headobj.type = "ro";
                headobj.id = "activitytype";
                headobj.sort = "na";
                headobj.width = 0;
                headobj.value = "Activity Type";
                headobjlist.Add(headobj);

                if (!IsNotLineItemListing)
                {
                    // First Column Activity Type
                    headobj.type = "sub_row_grid";
                    headobj.id = "subgridicon";
                    headobj.sort = "na";
                    headobj.width = 30;
                    headobj.value = "";
                    headobjlist.Add(headobj);
                }

                //Second Column : Task Name
                headobj = new PlanHead();
                headobj.type = "tree";
                headobj.align = "left";
                headobj.id = "taskname";
                headobj.sort = "str";
                headobj.width = 330;
                headobj.value = "Task Name";
                headobjlist.Add(headobj);

                if (IsNotLineItemListing)
                {
                    // Third Column : Empty
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.align = "center";
                    headobj.id = "add";
                    headobj.sort = "na";
                    headobj.width = 85;
                    headobj.value = "";
                    headobjlist.Add(headobj);
                }
                else
                {
                    // Third Column : Empty
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.align = "center";
                    headobj.id = "add";
                    headobj.sort = "na";
                    headobj.width = 45;
                    headobj.value = "";
                    headobjlist.Add(headobj);
                }

                // Fourth Column : Id
                headobj = new PlanHead();
                headobj.type = "ro";
                headobj.id = "id";
                headobj.sort = "na";
                headobj.width = 0;
                headobj.value = "id";
                headobjlist.Add(headobj);

                if (IsNotLineItemListing)
                {
                    // Fifth Column : Start Date
                    headobj = new PlanHead();
                    headobj.type = "dhxCalendar";
                    headobj.align = "center";
                    headobj.id = "startdate";
                    headobj.sort = "date";
                    headobj.width = 110;
                    headobj.value = "Start Date";
                    headobjlist.Add(headobj);

                    // Sixth Column : End Date
                    headobj = new PlanHead();
                    headobj.type = "dhxCalendar";
                    headobj.align = "center";
                    headobj.id = "enddate";
                    headobj.sort = "date";
                    headobj.width = 100;
                    headobj.value = "End Date";
                    headobjlist.Add(headobj);
                }

                // Seventh Column: Planned Cost
                headobj = new PlanHead();
                headobj.type = "ron";
                headobj.align = "center";
                headobj.id = "plannedcost";
                headobj.sort = "int";
                headobj.width = 160;
                headobj.value = "Planned Cost";
                headobjlist.Add(headobj);

                if (IsNotLineItemListing)
                {
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.align = "center";
                    headobj.id = "roitactictype";
                    headobj.sort = "str";
                    headobj.width = 150;
                    headobj.value = "Tactic Category";
                    headobjlist.Add(headobj);
                }

                // Eight Column : Type
                headobj = new PlanHead();
                headobj.type = "coro";
                headobj.align = "center";
                headobj.id = "tactictype";
                headobj.sort = "sort_TacticType";
                headobj.width = 150;
                headobj.value = "Type";
                headobj.options = lstTacticType;
                headobjlist.Add(headobj);

                //Nineth Column : Owner
                headobj = new PlanHead();
                headobj.type = "coro";
                headobj.align = "center";
                headobj.id = "owner";
                headobj.sort = "sort_Owner";
                headobj.width = 115;
                headobj.value = "Owner";
                headobj.options = lstOwner;
                headobjlist.Add(headobj);

                if (IsNotLineItemListing)
                {
                    // Tenth Column : Target Stage Goal
                    headobj = new PlanHead();
                    headobj.type = "ron";
                    headobj.align = "center";
                    headobj.id = "inq";
                    headobj.sort = "int";
                    headobj.width = 150;
                    headobj.value = "Target Stage Goal";
                    headobjlist.Add(headobj);

                    // Eleventh Column: MQl
                    headobj = new PlanHead();
                    headobj.type = "ron";
                    headobj.align = "center";
                    headobj.id = "mql";
                    headobj.sort = "int";
                    headobj.width = 150;
                    headobj.value = MQLTitle;
                    headobjlist.Add(headobj);

                    // Twelveth Column : Revenue
                    headobj = new PlanHead();
                    headobj.type = "ron";
                    headobj.align = "center";
                    headobj.id = "revenue";
                    headobj.sort = "int";
                    headobj.width = 150;
                    headobj.value = "Revenue";
                    headobjlist.Add(headobj);

                    //Add External Name Column as a last column of gridview
                    //Thirteenth Column : Empty
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.id = "machinename";
                    headobj.sort = "str";
                    headobj.width = 0;
                    headobj.value = "Machine Name";
                    headobjlist.Add(headobj);

                }
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return headobjlist;
        }
        #endregion

        #region Calculate Revenue Mql for hireachy
        /// <summary>
        /// Add By Nishant Sheth
        /// Calculate Revenue and MQL value for Program,Campaign, and Plan based on Tactic values
        /// </summary>
        /// <param name="DataList"></param>
        public void RoundupRevnueMqlforHireachyData(ref List<GridDefaultModel> DataList)
        {
            // Reverse the list from Plan -> Lineitem to Lineitem -> Plan
            var ListofParentIds = DataList
                .Select(a => new
                {
                    a.ParentUniqueId,
                    a.EntityType
                }).Distinct()
            .Reverse()
            .ToList();

            //Rounup the values from child to parent
            foreach (var ParentDetail in ListofParentIds)
            {
                if (ParentDetail.EntityType.ToLower() != Convert.ToString(Enums.EntityType.Lineitem).ToLower())
                {
                    var RevenueMql = DataList.Where(a => a.ParentUniqueId == ParentDetail.ParentUniqueId)
                        .Select(a => new
                        {
                            mqlSum = a.MQL,
                            revnueSum = a.Revenue
                        }).ToList();

                    DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).ToList()
                        .ForEach(a =>
                        {
                            a.MQL = RevenueMql.Select(ab => ab.mqlSum).Sum();
                            a.Revenue = RevenueMql.Select(ab => ab.revnueSum).Sum();
                        });
                }

            }

        }
        #endregion

        #region Hireachy Methods
        /// <summary>
        /// Add By Nishant Sheth
        /// Get first row of plan grid
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="minParentId"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<GridDefaultModel> GetTopLevelRowsGrid(List<GridDefaultModel> DataList, string minParentId)
        {
            try
            {
                return DataList
                  .Where(row => row.ParentUniqueId == minParentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get Childs records of parent entity
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<GridDefaultModel> GetChildrenGrid(List<GridDefaultModel> DataList, string parentId)
        {
            try
            {
                return DataList
                  .Where(row => row.ParentUniqueId == parentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Genrate items for hireachy 
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="Row"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PlanDHTMLXGridDataModel CreateItemGrid(List<GridDefaultModel> DataList, GridDefaultModel Row, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            List<PlanDHTMLXGridDataModel> children = new List<PlanDHTMLXGridDataModel>();
            try
            {
                // Get entity childs records
                var lstChildren = GetChildrenGrid(DataList, Row.UniqueId);

                // Call recursive if any other child entity
                children = lstChildren
                .Select(r => CreateItemGrid(DataList, r, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate)).ToList();

                EntitydataobjCreateItem = new List<Plandataobj>();

                // Get list of custom field values for particular entity based on pivoted entities list
                List<Plandataobj> lstCustomfieldData = EntityCustomDataValues.Where(a => a.EntityId == Row.EntityId)
                                           .Select(a => a.CustomFieldData).FirstOrDefault();

                if (lstCustomfieldData == null)
                {
                    lstCustomfieldData = EmptyCustomValues;
                }

                // Set the values of row
                EntitydataobjCreateItem = GridDataRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, lstCustomfieldData);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return new PlanDHTMLXGridDataModel { id = Row.UniqueId, data = EntitydataobjCreateItem.Select(a => a).ToList(), rows = children, bgColor = GridBackgroundColor(Row.EntityType), open = GridEntityOpenState(Row.EntityType), userdata = GridUserData(DataList, Row.EntityType, Row.UniqueId, Row) };
        }
        #endregion

        #region Create Data object for Grid
        /// <summary>
        /// Add By Nishant Sheth
        /// Set the grid rows for entities
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridDataRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Plan).ToLower())
                {
                    return GridPlanRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, objCustomfieldData);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Campaign).ToLower())
                {
                    return GridCampaignRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, objCustomfieldData);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Program).ToLower())
                {
                    return GridProgramRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, objCustomfieldData);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Tactic).ToLower())
                {
                    return GridTacticRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, objCustomfieldData);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Lineitem).ToLower())
                {
                    return GridLineItemRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, objCustomfieldData);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return EntitydataobjCreateItem;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set grid rows for plan entites
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridPlanRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsPlanEditSubordinatesAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                bool IsPlanEditAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);
                bool IsPlanEditable = true;
                string cellTextColor = string.Empty;

                cellTextColor = IsPlanEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;
                #region Set Default Columns Values

                // Add Plan Entity Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = "<div class=grid_Search id=Plan title=View ></div>" + (IsPlanEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsPlanEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.StartDate),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EndDate),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = string.Empty,
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set grid rows for campaign entites
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridCampaignRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;
                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    //value = "<div class=grid_Search id=CP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add> </div>" : "") + "<div class=honeycombbox-icon-gantt id=CampaignAdd onclick=javascript:AddRemoveEntity(this) title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' altId=" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "' taskId= " + Row.EntityId + " csvId=" + Row.EntityType + "_" + Row.EntityId + "></div>"
                    value = "<div class=grid_Search id=CP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + " title=Add> </div>" : "") + "<div class=honeycombbox-icon-gantt id=CampaignAdd onclick=javascript:AddRemoveEntity(this) title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' altId=" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + "' taskId= " + Row.EntityId + " csvId=Campaign_" + Row.EntityId + "></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set grid rows for program entites
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridProgramRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    //value = "<div class=grid_Search id=Plan title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>"
                    value = "<div class=grid_Search id=PP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Program alt=_" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div>" : "") + " <div class=honeycombbox-icon-gantt id=ProgramAdd onclick=javascript:AddRemoveEntity(this);  title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "'  TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "'  altId=_" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + "'  taskId= " + Row.EntityId + " csvId=Program_" + Row.EntityId + "></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set grid rows for tactic entites
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridTacticRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = ((Row.AnchorTacticID == Row.EntityId) ?
                              "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id=pkgIcon onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids=" + Convert.ToString(Row.PackageTacticIds) + "><i class='fa fa-object-group'></i></div>" : "")
                              + HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    //value = "<div class=grid_Search id=Plan title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>",
                    value = "<div class=grid_Search id=TP title=View></div>" + (IsEditable ? "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Tactic alt=__" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "  LinkTacticper ='" + false + "' LinkedTacticId = '" + 0 + "' tacticaddId='" + Row.EntityId + "' title=Add></div>" : "") + " <div class=honeycombbox-icon-gantt id=TacticAdd onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb'  pcptid = " + Row.TaskId + " anchortacticid='" + 0 + "' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "'  roitactictype='" + Row.AssetType + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode + "'  TacticType= '" + Row.TacticType + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' altId=__" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "' taskId=" + Row.EntityId + " csvId=Tactic_" + Row.EntityId + "></div>",
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    type = objHomeGridProp.typeEdn,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.AssetType,
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.TacticTypeId),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = (Math.Round(Convert.ToDouble(Row.ProjectedStageValue)) > 0 ? Math.Round(Convert.ToDouble(Row.ProjectedStageValue)).ToString("#,#") : "0") + " " + Row.ProjectedStage,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor,
                    type = objHomeGridProp.typeEdn
                });

                // Add Mql
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.MachineName),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                //Nullable<Int64> EntityId = Row.EntityId;
                //if (CustomFieldData.CustomFieldValues.Where(a => a.EntityId == Row.EntityId).Any())
                //{
                //    var lstCustomFieldData = (from objCustomField in CustomFieldData.CustomFields
                //                              join objCustomFieldValue in CustomFieldData.CustomFieldValues
                //                                  on objCustomField.CustomFieldId equals objCustomFieldValue.CustomFieldId
                //                                  into cust
                //                              from customval in cust.DefaultIfEmpty()
                //                              where objCustomField.EntityType.ToLower() == Row.EntityType.ToLower() &&
                //                               (customval != null ? customval.EntityId == EntityId : customval == null)
                //                              select new Plandataobj
                //                              {
                //                                  value = (customval != null ? customval.Value : string.Empty),
                //                                  locked = objHomeGridProp.lockedstateone,
                //                                  style = cellTextColor
                //                              }).ToList();

                //    EntitydataobjCreateItem.AddRange(lstCustomFieldData);
                //}
                //else
                //{
                //    var lstCustomFieldData = (from objCustomField in CustomFieldData.CustomFields
                //                              select new Plandataobj
                //                              {
                //                                  value = string.Empty,
                //                                  locked = objHomeGridProp.lockedstateone,
                //                                  style = cellTextColor
                //                              }).ToList();

                //    EntitydataobjCreateItem.AddRange(lstCustomFieldData);
                //}

                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                //foreach (var Customfield in CustomFieldData.CustomFields)
                //{
                //    string EntityValue = string.Empty;
                //    var CustomValue = CustomFieldData.CustomFieldValues.Where(cust =>
                //                            Row.EntityId == cust.EntityId
                //                            && Row.EntityType.ToLower() == Customfield.EntityType.ToLower()
                //                            && Customfield.CustomFieldId == cust.CustomFieldId)
                //                            .Select(val => val.Value).ToList();
                //    if (CustomValue != null)
                //    {
                //        if (CustomValue.Count > 0)
                //        {
                //            EntityValue = string.Join(",", CustomValue);
                //        }
                //    }

                //    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                //    {
                //        value = EntityValue,
                //        locked = objHomeGridProp.lockedstateone,
                //        style = cellTextColor
                //    });
                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Set grid rows for lineitem entites
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="EntitydataobjCreateItem"></param>
        /// <param name="CustomFieldData"></param>
        /// <param name="PlanCurrencySymbol"></param>
        /// <param name="PlanExchangeRate"></param>
        /// <param name="objCustomfieldData"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Plandataobj> GridLineItemRow(GridDefaultModel Row, ref List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    //value = "<div class=grid_Search id=" + Row.EntityType + " title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=" + Row.EntityType + " alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>",
                    value = "<div class=grid_Search id=LP title=View></div>" + (IsEditable ? "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Line alt=___" + Row.ParentEntityId + "_" + Row.EntityId + " lt=" + ((Row.LineItemType == null) ? 0 : Row.LineItemTypeId) + " dt=" + HttpUtility.HtmlEncode(Row.EntityTitle) + " per=" + IsEditable.ToString().ToLower() + " title=Add></div>" : ""),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    locked = objHomeGridProp.lockedstateone
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    locked = objHomeGridProp.lockedstateone
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    locked = ((Row.LineItemTypeId == null || Convert.ToString(Row.LineItemTypeId) == string.Empty) ? objHomeGridProp.lockedstateone : objHomeGridProp.lockedstatezero),
                    type = objHomeGridProp.typeEdn,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = !string.IsNullOrEmpty(Row.LineItemType) ? Convert.ToString(Row.LineItemType) : string.Empty,
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Mql
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Revenue
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add(new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        #endregion

        #region Method to convert number in k, m formate

        /// <summary>
        /// Mehtod for convert number to formated string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConvertNumberToRoundFormate(double num)
        {
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));
            num = num / i * i;

            if (num >= 100000000000000)
                return (num / 100000000000000D).ToString("0.##") + "Q";
            if (num >= 100000000000)
                return (num / 100000000000D).ToString("0.##") + "T";
            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.##") + "B";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";

            if (num != 0.0)
                return num < 1 ? (num.ToString().Contains(".") ? num.ToString("#,#0.00") : num.ToString("#,#")) : num.ToString("#,#");
            else
                return "0";
        }
        #endregion

        #region Grid user data
        /// <summary>
        /// Set the user data for entities
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="EntityType"></param>
        /// <param name="UniqueId"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid GridUserData(List<GridDefaultModel> DataList, string EntityType, string UniqueId, GridDefaultModel Row)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            try
            {
                if (EntityType.ToLower() == Convert.ToString(Enums.EntityType.Campaign).ToLower())
                {
                    CampaignUserData(DataList, ref objUserData, UniqueId);
                }
                else if (EntityType.ToLower() == Convert.ToString(Enums.EntityType.Program).ToLower())
                {
                    ProgramUserData(DataList, ref objUserData, UniqueId);
                }
                else if (EntityType.ToLower() == Convert.ToString(Enums.EntityType.Tactic).ToLower())
                {
                    TacticUserData(Row, ref objUserData);
                }
                else if (EntityType.ToLower() == Convert.ToString(Enums.EntityType.Lineitem).ToLower())
                {
                    LineItemUserData(Row, ref objUserData);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Campaign entities
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="EntityType"></param>
        /// <param name="UniqueId"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid CampaignUserData(List<GridDefaultModel> DataList, ref Planuserdatagrid objUserData, string UniqueId)
        {
            try
            {
                var ProgramDetail = DataList.Where(prg => prg.ParentUniqueId == UniqueId)
                                    .Select(prg => new
                                    {
                                        prg.UniqueId,
                                        prg.StartDate,
                                        prg.EndDate
                                    }).ToList();

                if (ProgramDetail.Count > 0)
                {
                    objUserData.psdate = ProgramDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                    objUserData.pedate = ProgramDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");

                    var TacticDetail = (from objPrg in ProgramDetail
                                        join objData in DataList
                                            on objPrg.UniqueId equals objData.ParentUniqueId
                                        select new
                                        {
                                            objData.StartDate,
                                            objData.EndDate
                                        }).ToList();

                    if (TacticDetail.Count > 0)
                    {
                        objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                        objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Program entities
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="EntityType"></param>
        /// <param name="UniqueId"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid ProgramUserData(List<GridDefaultModel> DataList, ref Planuserdatagrid objUserData, string UniqueId)
        {
            try
            {
                var TacticDetail = DataList.Where(tac => tac.ParentUniqueId == UniqueId)
                                    .Select(tac => new
                                    {
                                        tac.StartDate,
                                        tac.EndDate
                                    }).ToList();
                if (TacticDetail.Count > 0)
                {
                    objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                    objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Tactic entities
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="EntityType"></param>
        /// <param name="UniqueId"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid TacticUserData(GridDefaultModel Row, ref Planuserdatagrid objUserData)
        {
            try
            {
                objUserData.stage = Row.ProjectedStage;
                objUserData.tactictype = Convert.ToString(Row.TacticTypeId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Lineitem entities
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="EntityType"></param>
        /// <param name="UniqueId"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid LineItemUserData(GridDefaultModel Row, ref Planuserdatagrid objUserData)
        {
            try
            {
                string IsOther = Convert.ToString(!string.IsNullOrEmpty(Row.LineItemType) ? false : true);
                objUserData.IsOther = IsOther;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }
        #endregion

        #endregion

        #region "Calendar Related Functions"
        /// Createdy By: Viral
        /// Created On: 09/19/2016
        // Desc: Return List of Plan, Campaign, Program, Tactic
        public List<calendarDataModel> GetPlanCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string timeframe, string planYear)
        {
            #region "Declare Variables"
            SqlParameter[] para = new SqlParameter[6];
            List<calendarDataModel> calResultset = new List<calendarDataModel>();   // Return Calendar Result Data Model
            #endregion

            try
            {
                #region "Set SP Parameters"
                para[0] = new SqlParameter()
                {
                    ParameterName = "planIds",
                    Value = planIds
                };
                para[1] = new SqlParameter()
                {
                    ParameterName = "ownerIds",
                    Value = ownerIds
                };
                para[2] = new SqlParameter()
                {
                    ParameterName = "tactictypeIds",
                    Value = tactictypeIds
                };
                para[3] = new SqlParameter()
                {
                    ParameterName = "statusIds",
                    Value = statusIds
                };
                para[4] = new SqlParameter()
                {
                    ParameterName = "timeframe",
                    Value = timeframe
                };
                para[5] = new SqlParameter()
                {
                    ParameterName = "planYear",
                    Value = planYear
                };
                #endregion

                #region "Get Data"
                calResultset = objDbMrpEntities.Database
                    .SqlQuery<calendarDataModel>("spGetPlanCalendarData @planIds,@ownerIds,@tactictypeIds,@statusIds,@timeframe,@planYear", para)
                    .ToList();
                #endregion
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex); // Log error in Elmah.
            }

            return calResultset;
        }

        /// Createdy By: Viral
        /// Created On: 09/19/2016
        // Desc: Set Owner Name and Permission of entity
        public List<calendarDataModel> SetOwnerNameAndPermission(List<calendarDataModel> lstCalendarDataModel)
        {
            try
            {
                #region "Get OwnerName"
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                Dictionary<int, User> lstUsersData = new Dictionary<int, BDSService.User>();
                objBDSServiceClient.GetUserListByClientIdEx(Sessions.User.CID).ForEach(u => lstUsersData.Add(u.ID, u));
                #endregion

                #region "Get SubOrdinates"
                List<int> lstSubordinatesIds = new List<int>();
                bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);
                var IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);
                if (IsTacticAllowForSubordinates)
                {
                    lstSubordinatesIds = Common.GetAllSubordinates(Sessions.User.ID);   // Get all subordinates based on UserId.
                }
                #endregion

                #region "Set Owner Name & Permission"
                KeyValuePair<int, User> usr;
                foreach (calendarDataModel data in lstCalendarDataModel)
                {
                    #region "Set Owner Name"
                    usr = lstUsersData.Where(u => u.Key == data.CreatedBy.Value).FirstOrDefault();
                    if (usr.Value != null)
                        data.OwnerName = string.Format("{0} {1}", Convert.ToString(usr.Value.FirstName), Convert.ToString(usr.Value.LastName));
                    #endregion

                    #region "Set Permission"
                    if (IsPlanCreateAllAuthorized == false)     // check whether user has plan create permission or not
                    {
                        if ( (data.CreatedBy.HasValue) && (data.CreatedBy.Value.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(data.CreatedBy.Value))) // check whether Entity owner is own or it's subordinates.
                            data.Permission = true;
                        else
                            data.Permission = false;
                    }
                    else
                        data.Permission = true;
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstCalendarDataModel;
        }

        #endregion
    }

    #region Pivot Custom fields list for each entities
    public static class PivotList
    {
        public static HomeGridProperties objHomeGrid = new HomeGridProperties();
        /// <summary>
        /// Add By Nishant Sheth
        /// Pivot list for custom field entities values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TColumn"></typeparam>
        /// <typeparam name="TRow"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="source"></param>
        /// <param name="columnSelector"></param>
        /// <param name="rowSelector"></param>
        /// <param name="dataSelector"></param>
        /// <returns></returns>
        public static List<CustomfieldPivotData> ToPivotList<T, TColumn, TRow, TData>(
        this IEnumerable<T> source,
        Func<T, TColumn> columnSelector,
        Expression<Func<T, TRow>> rowSelector,
        Func<IEnumerable<T>, TData> dataSelector
            )
        {

            var arr = new List<CustomfieldPivotData>();
            var cols = new List<string>();
            String rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            var columns = source.Select(columnSelector).Distinct();

            cols = (new[] { rowName }).Concat(columns.Select(x => x.ToString())).ToList();
            var abc = columns.Select(x => x.ToString()).ToList().Take(1);

            var rows = source.GroupBy(rowSelector.Compile())
                             .Select(rowGroup => new
                             {
                                 Key = rowGroup.Key,
                                 Values = columns.GroupJoin(
                                     rowGroup,
                                     c => c,
                                     r => columnSelector(r),
                                     (c, columnGroup) => dataSelector(columnGroup))
                             }).ToList();


            foreach (var row in rows)
            {
                var items = row.Values.Cast<object>().ToList();
                items.Insert(0, row.Key);
                var obj = GetAnonymousObject(cols, items);
                arr.Add(obj);
            }
            return arr.ToList();
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// get values for pivoting entites
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static dynamic GetAnonymousObject(IEnumerable<string> columns, IEnumerable<object> values)
        {
            CustomfieldPivotData objCustomPivotData = new CustomfieldPivotData();
            List<Plandataobj> lstCustomFieldData = new List<Plandataobj>();
            int i;
            for (i = 0; i < columns.Count(); i++)
            {
                if (i == 0)
                {
                    objCustomPivotData.EntityId = !string.IsNullOrEmpty(Convert.ToString(values.ElementAt<object>(i))) ?
                        int.Parse(Convert.ToString(values.ElementAt<object>(i)))
                        : 0;
                }
                else
                {
                    lstCustomFieldData.Add(
                        new Plandataobj
                        {
                            locked = objHomeGrid.lockedstateone,
                            value = Convert.ToString(values.ElementAt<object>(i)),
                            style = objHomeGrid.stylecolorblack
                        });
                }
            }
            objCustomPivotData.CustomFieldData = lstCustomFieldData;
            return objCustomPivotData;
        }

    }
    #endregion

}