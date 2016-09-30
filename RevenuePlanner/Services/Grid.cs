﻿using Elmah;
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
using System.Xml.Linq;

namespace RevenuePlanner.Services
{
    public class Grid : IGrid
    {
        #region Declartion
        private MRPEntities objDbMrpEntities;
        private CacheObject objCache;
        private ColumnView objColumnView;
        List<Plandataobj> plandataobjlistCreateItem = new List<Plandataobj>();
        List<Plandataobj> EntitydataobjItem = new List<Plandataobj>(); // Plan grid entity row's data
        List<CustomfieldPivotData> EntityCustomDataValues = new List<CustomfieldPivotData>(); // Set Custom fields value for entites
        List<Plandataobj> EmptyCustomValues; // Variable for assigned blank values for custom field
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        HomeGridProperties objHomeGridProp = new HomeGridProperties();
        double PlanExchangeRate = 1; // set curenncy plan exchange rate. it's varibale value update from 'GetPlanGrid' method
        string PlanCurrencySymbol = "$"; // set curenncy symbol. it's varibale value update from 'GetPlanGrid' method
        public string ColumnManagmentIcon = Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon);
        #endregion

        // Constructor
        public Grid()
        {
            objDbMrpEntities = new MRPEntities(); // Create Enitites object
            objCache = new CacheObject(); // Create Cache object for stored data
            objColumnView = new ColumnView();
        }

        #region Plan Grid Methods

        #region Method to get grid default data
        /// <summary>
        /// Add By Nishant Sheth
        /// call stored procedure to get list of plan and all related entities for home grid, based on client and filters selected by user 
        /// <summary>
        public List<GridDefaultModel> GetGridDefaultData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {
            List<GridDefaultModel> EntityList = new List<GridDefaultModel>();
            try
            {
                SqlParameter[] para = new SqlParameter[5];

                para[0] = new SqlParameter { ParameterName = "PlanId", Value = PlanIds };

                para[1] = new SqlParameter { ParameterName = "ClientId", Value = ClientId };

                para[2] = new SqlParameter { ParameterName = "OwnerIds", Value = ownerIds };

                para[3] = new SqlParameter { ParameterName = "TacticTypeIds", Value = TacticTypeid };

                para[4] = new SqlParameter { ParameterName = "StatusIds", Value = StatusIds };

                EntityList = objDbMrpEntities.Database
                    .SqlQuery<GridDefaultModel>("GetGridData @PlanId,@ClientId,@OwnerIds,@TacticTypeIds,@StatusIds", para)
                    .ToList();
            }
            catch { throw; }
            return EntityList;
        }
        #endregion

        #region Mtehod to get grid customfield and it's entity value
        /// <summary>
        /// Add By Nishant Sheth
        /// call stored procedure to get all related custom fields and their values for selected plans and entities , based on client and filters selected by user 
        /// There are 2 results set from the GridCustomFieldData sproc- 1.CustomField master list 2. Vlaues of customfields for entities with in selected plans 
        /// TODO :: Working to combine 'GridCustomFieldData' Sproc and 'GetGridData' Sproc so that we can get result in single db call.
        /// <summary>
        public GridCustomColumnData GetGridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
        {
            GridCustomColumnData EntityList = new GridCustomColumnData();
            SqlConnection Connection = objDbMrpEntities.Database.Connection as SqlConnection;
            try
            {
                //If connection is closed then it will be open
                if (Connection.State == System.Data.ConnectionState.Closed)
                    Connection.Open();
                // Create a SQL command to execute the sproc 
                DbCommand cmd = objDbMrpEntities.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GridCustomFieldData]";
                cmd.CommandType = CommandType.StoredProcedure;

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
                paraOwnerIds.Value = ownerIds;

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

                DbDataReader reader = cmd.ExecuteReader();
                // Read CustomField from the first result set 
                EntityList.CustomFields = ((IObjectContextAdapter)objDbMrpEntities)
                   .ObjectContext
                   .Translate<GridCustomFields>(reader).ToList();
                // Move to second result set and read custom field entities values
                reader.NextResult();
                EntityList.CustomFieldValues = ((IObjectContextAdapter)objDbMrpEntities)
                    .ObjectContext
                    .Translate<GridCustomFieldEntityValues>(reader).ToList();
            }
            catch { throw; }
            finally
            {
                if (Connection.State == System.Data.ConnectionState.Open)
                    Connection.Close();
            }

            return EntityList;
        }
        #endregion

        #region Get Model for Grid Data
        /// <summary>
        /// Add By Nishant Sheth
        /// Get plan grid data with default and custom fields columns 
        /// </summary>
        public PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string CurrencySymbol, double ExchangeRate, int UserId)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            try
            {
                PlanExchangeRate = ExchangeRate; // Set client currency plan exchange rate 
                PlanCurrencySymbol = CurrencySymbol; // Set user currency symbol

                // Get MQL title label client wise
                string MQLTitle = GetMqlTitle(ClientId);

                // Get list of user wise columns or default columns of gridview it's refrencely update from GenerateJsonHeader Method
                List<string> HiddenColumns = new List<string>(); // List of hidden columns of plan grid
                List<string> UserDefinedColumns = new List<string>(); // List of User selected or default columns list
                List<string> customColumnslist = new List<string>(); // List of custom field columns

                // Generate header columns for grid
                List<PlanHead> ListOfDefaultColumnHeader = GenerateJsonHeader(MQLTitle, ref HiddenColumns, ref UserDefinedColumns, ref customColumnslist, UserId);

                //Get list of entities for plan grid
                List<GridDefaultModel> GridHireachyData = GetGridDefaultData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);
                // Update Plan Start and end date
                GridHireachyData = UpdatePlanStartEndDate(GridHireachyData);
                // Add Plan grid default column data to cache object
                objCache.AddCache(Convert.ToString(Enums.CacheObject.ListPlanGridDefaultData), GridHireachyData);

                // Get List of customfields and it's entity's values
                GridCustomColumnData ListOfCustomData = GridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds, ref customColumnslist);
                // Add Plan grid default column data to cache object
                objCache.AddCache(Convert.ToString(Enums.CacheObject.ListPlanGridCustomColumnData), ListOfCustomData);

                // Check is user select Revnue column in column saved view
                bool IsRevenueColumn = UserDefinedColumns.Where(a =>
                    a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue).ToLower()).Any();
                if (IsRevenueColumn)
                {
                    // Round up the Revnue value for Progam/Campaign/Plan
                    GridHireachyData = RoundupRevenueforHireachyData(GridHireachyData);
                }

                // Check is user select MQL column in column saved view
                bool IsMQLColumn = UserDefinedColumns.Where(a =>
                   a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL).ToLower()).Any();
                if (IsMQLColumn)
                {
                    // Round up the Revnue value for Progam/Campaign/Plan
                    GridHireachyData = RoundupMqlforHireachyData(GridHireachyData);
                }

                // Get selected columns data
                List<PlanGridColumnData> lstSelectedColumnsData = GridHireachyData.Select(a => Projection(a, UserDefinedColumns)).ToList();

                // Merge header of plan grid with custom fields
                ListOfDefaultColumnHeader.AddRange(GridCustomHead(ListOfCustomData.CustomFields, customColumnslist));

                // Genrate Hireachy of Plan grid
                List<PlanDHTMLXGridDataModel> griditems = GetTopLevelRowsGrid(lstSelectedColumnsData, null)
                         .Select(row => CreateItemGrid(lstSelectedColumnsData, row, ListOfCustomData, PlanCurrencySymbol, PlanExchangeRate))
                         .ToList();

                objPlanMainDHTMLXGrid.head = ListOfDefaultColumnHeader;
                objPlanMainDHTMLXGrid.rows = griditems;
            }
            catch
            {
                throw;
            }
            return objPlanMainDHTMLXGrid;
        }

        /// <summary>
        /// Update plan start and end date based on campaign start date and end date
        /// </summary>
        public List<GridDefaultModel> UpdatePlanStartEndDate(List<GridDefaultModel> GridData)
        {
            List<GridDefaultModel> CopyGridData = GridData; // Copy the grid data to get it's childs data
            try
            {
                CopyGridData.ForEach(a =>
                {
                    if (string.Compare(a.EntityType, Convert.ToString(Enums.EntityType.Plan), true) == 0)
                    {
                        int year = int.Parse(a.PlanYear);
                        DateTime firstDay = new DateTime(year, 1, 1);
                        DateTime? lastDay = new DateTime(year, 12, 31);

                        // Check is any campaign or not in plan data
                        DateTime? CampaignMaxDate = GridData.Where(cmp => cmp.ParentUniqueId == a.UniqueId)
                                                .Max(b => b.EndDate);
                        if (CampaignMaxDate != null)
                        {
                            lastDay = CampaignMaxDate;
                        }
                        // Set Plan Dates
                        a.StartDate = firstDay;
                        a.EndDate = lastDay;
                    }
                });
            }
            catch { throw; }
            return CopyGridData;
        }

        /// <summary>
        /// Get plan grid data from cache memory
        /// </summary>
        public PlanMainDHTMLXGrid GetPlanGridDataFromCache(int ClientId, int UserId)
        {
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            try
            {
                // Get MQL title label client wise
                List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false)
                    .Select(stage => stage).ToList();
                string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Convert.ToString(Enums.PlanGoalType.MQL).ToLower()).Select(stage => stage.Title).FirstOrDefault();

                // Get list of user wise columns or default columns of gridview it's refrencely update from GenerateJsonHeader Method
                List<string> HiddenColumns = new List<string>(); // List of hidden columns of plan grid
                List<string> UserDefinedColumns = new List<string>(); // List of User selected or default columns list
                List<string> customColumnslist = new List<string>(); // List of custom field columns

                // Generate header methods for default columns
                List<PlanHead> ListOfDefaultColumnHeader = GenerateJsonHeader(MQLTitle, ref HiddenColumns, ref UserDefinedColumns, ref customColumnslist, UserId);

                //Get list of entities for plan grid from Cache object
                List<GridDefaultModel> GridHireachyData = (List<GridDefaultModel>)objCache.Returncache(Convert.ToString(Enums.CacheObject.ListPlanGridDefaultData));
                if (GridHireachyData == null)
                {
                    GridHireachyData = new List<GridDefaultModel>();
                }

                // Check is user select Revnue column in column saved view
                bool IsRevenueColumn = UserDefinedColumns.Where(a =>
                    a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue).ToLower()).Any();
                if (IsRevenueColumn)
                {
                    // Round up the Revnue value for Progam/Campaign/Plan
                    GridHireachyData = RoundupRevenueforHireachyData(GridHireachyData);
                }

                // Check is user select MQL column in column saved view
                bool IsMQLColumn = UserDefinedColumns.Where(a =>
                   a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL).ToLower()).Any();
                if (IsMQLColumn)
                {
                    // Round up the Revnue value for Progam/Campaign/Plan
                    GridHireachyData = RoundupMqlforHireachyData(GridHireachyData);
                }

                // Get selected columns data
                List<PlanGridColumnData> lstSelectedColumnsData = GridHireachyData.Select(a => Projection(a, UserDefinedColumns)).ToList();

                // Get List of customfields and it's entity's values
                GridCustomColumnData ListOfCustomData = (GridCustomColumnData)objCache.Returncache(Convert.ToString(Enums.CacheObject.ListPlanGridCustomColumnData));
                if (ListOfCustomData == null)
                {
                    ListOfCustomData = new GridCustomColumnData();
                }

                // Pivot Custom fields data with selected columns
                PivotcustomFieldData(ref customColumnslist, ListOfCustomData);

                // Merge header of plan grid with custom fields
                ListOfDefaultColumnHeader.AddRange(GridCustomHead(ListOfCustomData.CustomFields, customColumnslist));

                // Genrate Hireachy of Plan grid
                List<PlanDHTMLXGridDataModel> griditems = GetTopLevelRowsGrid(lstSelectedColumnsData, null)
                         .Select(row => CreateItemGrid(lstSelectedColumnsData, row, ListOfCustomData, PlanCurrencySymbol, PlanExchangeRate))
                         .ToList();

                objPlanMainDHTMLXGrid.head = ListOfDefaultColumnHeader;
                objPlanMainDHTMLXGrid.rows = griditems;
            }
            catch { throw; }
            return objPlanMainDHTMLXGrid;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get list of custom fields values for each entities
        /// </summary>
        public GridCustomColumnData GridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, ref List<string> selectedCustomColumns)
        {
            GridCustomColumnData data = new GridCustomColumnData();
            try
            {
                // Call the method of stored procedure that return list of custom field and it's entities values
                data = GetGridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);
                PivotcustomFieldData(ref selectedCustomColumns, data);
            }
            catch { throw; }
            return data;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Pivot Selected custom field columns data 
        /// </summary>
        public void PivotcustomFieldData(ref List<string> selectedCustomColumns, GridCustomColumnData data)
        {
            if (selectedCustomColumns.Count == 0)
            {
                // Update selectedCustomColumns variable with list of user selected/all custom fields columns name
                if (data.CustomFields != null)
                {
                    selectedCustomColumns = data.CustomFields.Select(a => Convert.ToString(a.CustomFieldId)).ToList();
                }
            }
            // Pivoting the customfields entities values //EntityCustomDataValues decalre globally at class level and it's use with hireachy process

            EntityCustomDataValues = data.CustomFieldValues.ToPivotList(item => item.CustomFieldId,
                     item => item.UniqueId,
                        items => items.Max(a => a.Value)
                        , selectedCustomColumns)
                        .ToList();

            // Create empty list of custom field values for entity where there is no any custom fields value on entity
            List<Plandataobj> lstCustomPlanData = new List<Plandataobj>();
            selectedCustomColumns.ForEach(a =>
            {
                lstCustomPlanData.Add(new Plandataobj
                {
                    value = string.Empty,
                    locked = objHomeGridProp.lockedstateone,
                    style = objHomeGridProp.stylecolorblack
                });
            });
            EmptyCustomValues = lstCustomPlanData;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Create header of custom fields for plan grid
        /// </summary>
        public List<PlanHead> GridCustomHead(List<GridCustomFields> lstCustomFields, List<string> customColumnslist)
        {
            List<PlanHead> ListHead = new List<PlanHead>();
            try
            {
                // Set Column management icon on grid view header
                string manageviewicon = Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon);
                PlanHead headobj = new PlanHead();
                foreach (string customFieldId in customColumnslist)
                {
                    GridCustomFields CustomfieldDetail = lstCustomFields.Where(a => a.CustomFieldId == int.Parse(customFieldId)).FirstOrDefault();

                    if (CustomfieldDetail != null)
                    {
                        headobj = new PlanHead();
                        // Check custom field type is Text box or Dropdown
                        if (string.Compare(CustomfieldDetail.CustomFieldType, Convert.ToString(Enums.CustomFieldType.TextBox), true) == 0)
                        {
                            headobj.type = "ed";
                        }
                        else
                        {
                            headobj.type = "clist";
                        }
                        headobj.id = "custom_" + CustomfieldDetail.CustomFieldId + ":" + CustomfieldDetail.EntityType;
                        headobj.sort = "str";
                        headobj.width = 150;
                        headobj.value = CustomfieldDetail.CustomFieldName + manageviewicon;
                        ListHead.Add(headobj);
                    }
                }
            }
            catch { throw; }
            return ListHead;
        }

        /// <summary>
        /// Add by Nishant Sheth
        /// set open/close entity state for respective entity
        /// </summary>
        public string GridEntityOpenState(string EntityType)
        {
            try
            {
                if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Plan), true) == 0)
                {
                    return objHomeGridProp.openstateone;
                }
                else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Campaign), true) == 0)
                {
                    return objHomeGridProp.openstateone;
                }
                else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Program), true) == 0)
                {
                    return objHomeGridProp.openstateone;
                }

            }
            catch { throw; }
            return string.Empty;
        }
        #endregion

        #region Method to generate grid header

        /// <summary>
        /// Add By Nishant Sheth
        /// Create Plan Grid header for default columns
        /// </summary>
        public List<PlanHead> GenerateJsonHeader(string MQLTitle, ref List<string> HiddenColumns, ref List<string> UserDefinedColumns, ref List<string> customColumnslist, int UserId)
        {
            List<PlanHead> headobjlist = new List<PlanHead>(); // List of headers detail of plan grid
            List<PlanOptions> lstOwner = new List<PlanOptions>(); // List of owner
            List<PlanOptions> lstTacticType = new List<PlanOptions>(); // List of tactic type
            try
            {
                // Get user column view
                User_CoulmnView userview = objDbMrpEntities.User_CoulmnView.Where(a => a.CreatedBy == UserId).FirstOrDefault();

                // Add Default and hidden reuqired columns into list
                headobjlist = lstHomeGrid_Hidden_And_Default_Columns().Select(a => a.Value).ToList();
                if (headobjlist != null && headobjlist.Count > 0)
                {
                    HiddenColumns.AddRange(headobjlist.Select(a => a.value).ToList());
                }

                // Below condition for when user have no any specfic column view of plan grid
                if (userview == null)
                {
                    // Set option values for dropdown columns like Tactic type/ Owner/ Asset type
                    List<PlanHead> lstDefaultColumns = new List<PlanHead>();

                    PlanHead DefaultObjects;
                    lstHomeGrid_Default_Columns().TryGetValue(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Owner), out DefaultObjects);
                    DefaultObjects.options = lstOwner;

                    lstHomeGrid_Default_Columns().TryGetValue(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType), out DefaultObjects);
                    DefaultObjects.options = lstTacticType;

                    lstHomeGrid_Default_Columns().TryGetValue(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType), out DefaultObjects);
                    DefaultObjects.value = MQLTitle + Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon); // set client wise mql title and column management icon;

                    lstDefaultColumns.AddRange(lstHomeGrid_Default_Columns().Select(a => a.Value).ToList());

                    //Enums.lstHomeGrid_Default_Columns.Select(a => a).ToList()
                    //    .ForEach(a =>
                    //    {
                    //        if (string.Compare(a.Key, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Owner), true) == 0)
                    //        {
                    //            a.Value.options = lstOwner;// pass the owner list to plan grid header
                    //        }
                    //        if (string.Compare(a.Key, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType), true) == 0)
                    //        {
                    //            a.Value.options = lstTacticType; // pass the tactictype list to plan grid header
                    //        }
                    //        if (string.Compare(a.Key, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL), true) == 0)
                    //        {
                    //            a.Value.value = MQLTitle + Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon); // set client wise mql title and column management icon
                    //        }
                    //        lstDefaultColumns.Add(a.Value);
                    //    });

                    // Update UserDefinedColumns variable with default columns list
                    UserDefinedColumns = lstDefaultColumns.Select(a => a.id).ToList();

                    // Merge list with default columns list
                    headobjlist.AddRange(lstDefaultColumns);
                }
                else
                {
                    // Get user gridview columns list
                    string attributexml = userview.GridAttribute;

                    if (!string.IsNullOrEmpty(attributexml))
                    {
                        XDocument doc = XDocument.Parse(attributexml);
                        // Read the xml data from user column view
                        List<AttributeDetail> items = objColumnView.UserSavedColumnAttribute(doc);

                        // Set default columns values
                        headobjlist.AddRange(GetUserDefaultColumnsProp(items, MQLTitle, ref  UserDefinedColumns));

                        // Add custom field columns to that user have selected from column manage view
                        customColumnslist = items.Where(a => a.AttributeType.ToLower() != Convert.ToString(Enums.HomeGridColumnAttributeType.Common).ToLower())
                                            .Select(a => a.AttributeId).ToList();
                    }
                }

            }
            catch
            {
                throw;
            }
            return headobjlist;
        }


        // Below dictionary list use when user have not any specific columns view and as default columns for home grid and set it's dhtmlx grid header properties
        private Dictionary<string, PlanHead> lstHomeGrid_Default_Columns()
        {
            Dictionary<string, PlanHead> lstColumns = new Dictionary<string, PlanHead>();

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.StartDate), new PlanHead
            {
                type = "dhxCalendar",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.StartDate),
                sort = "date",
                width = 110,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.StartDate) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.EndDate), new PlanHead
            {
                type = "dhxCalendar",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.EndDate),
                sort = "date",
                width = 100,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.EndDate) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost), new PlanHead
            {
                type = "ron",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost),
                sort = "int",
                width = 160,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.AssetType), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.AssetType),
                sort = "str",
                width = 150,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.AssetType) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType), new PlanHead
            {
                type = "coro",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType),
                sort = "sort_TacticType",
                width = 150,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.TacticType) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Owner), new PlanHead
            {
                type = "coro",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Owner),
                sort = "sort_Owner",
                width = 115,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.Owner) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal), new PlanHead
            {
                type = "ron",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal),
                sort = "int",
                width = 150,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL), new PlanHead
            {
                type = "ron",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL),
                sort = "int",
                width = 150,
                value = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL) // Here we not set ColumnManagmentIcon because MQl Title will be diffrent for clients it will be set when list get
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue), new PlanHead
            {
                type = "ron",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue),
                sort = "int",
                width = 150,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.Revenue) + ColumnManagmentIcon
            });
            return lstColumns;
        }

        // Below dictionary list defaulty for every user. it's user have is any sepecifc view or not.
        private Dictionary<string, PlanHead> lstHomeGrid_Hidden_And_Default_Columns()
        {
            Dictionary<string, PlanHead> lstColumns = new Dictionary<string, PlanHead>();

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ActivityType), new PlanHead
            {
                type = "ro",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ActivityType),
                sort = "na",
                width = 0,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.ActivityType)
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ColourCode), new PlanHead
            {
                type = "ro",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ColourCode),
                sort = "na",
                width = 10,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.ColourCode)
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName), new PlanHead
            {
                type = "ro",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName),
                sort = "na",
                width = 0,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.MachineName)
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TaskName), new PlanHead
            {
                type = "tree",
                align = "left",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TaskName),
                sort = "str",
                width = 330,
                value = string.Empty
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.id), new PlanHead
            {
                type = "ro",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.id),
                sort = "na",
                width = 0,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.id)
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Add), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Add),
                sort = "na",
                width = 85,
                value = string.Empty
            });
            return lstColumns;
        }


        /// <summary>
        /// Set default columns header values and it's properties
        /// </summary>
        private List<PlanHead> GetUserDefaultColumnsProp(List<AttributeDetail> items, string MQLTitle, ref List<string> UserDefinedColumns)
        {
            List<PlanHead> lstDefaultCols = new List<PlanHead>();

            // Get the default common column list except custom fields columns
            List<string> UserSelctedDefaultcolumnsList = items
                                .Where(a => a.AttributeType.ToLower() == Convert.ToString(Enums.HomeGridColumnAttributeType.Common).ToLower())
                                .Select(a => a.AttributeId.ToLower()).ToList();

            // Add Default Columns to that user have selected from column manage view
            if (UserSelctedDefaultcolumnsList != null && UserSelctedDefaultcolumnsList.Count > 0)
            {
                // Get the default/common list of header object 
                lstDefaultCols = lstHomeGrid_Default_Columns()
                    .Where(a => UserSelctedDefaultcolumnsList.Contains(a.Value.id.ToLower()))
                    .Select(a => a.Value).OrderBy(a => UserSelctedDefaultcolumnsList.IndexOf(a.id.ToLower()))
                        .ToList();

                if (lstDefaultCols != null && lstDefaultCols.Count > 0)
                {
                    // Update MQL Label Client wise and set column management icon
                    bool IsMqlCol = lstDefaultCols.Where(a => a.id.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL).ToLower()).Any();
                    if (IsMqlCol)
                    {
                        lstDefaultCols.Where(a => a.id.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL).ToLower()).ToList()
                            .ForEach(a =>
                            {
                                a.value = MQLTitle + Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon); // set client wise mql title and column management icon
                            });
                    }

                    UserDefinedColumns.AddRange(lstDefaultCols.Select(a => a.id).ToList());
                }
            }

            return lstDefaultCols;
        }

        #endregion

        #region Calculate Revenue Mql for hierarchy
        /// <summary>
        /// Add By Nishant Sheth
        /// Calculate Revenue and MQL value for Program,Campaign, and Plan based on Tactic values
        /// </summary>
        public List<GridDefaultModel> RoundupMqlforHireachyData(List<GridDefaultModel> DataList)
        {
            try
            {
                // Reverse the list from Plan -> Lineitem to Lineitem -> Plan
                // Here is anyonymos variable so need to use var type
                var ListofParentIds = DataList
                    .Select(a => new
                    {
                        a.ParentUniqueId,
                        a.EntityType
                    }).Distinct()
                .Reverse()
                .ToList();
                //Rounup the values from child to parent
                List<Int64?> MqlList = new List<Int64?>();
                foreach (var ParentDetail in ListofParentIds)
                {
                    if (string.Compare(ParentDetail.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) != 0)
                    {
                        // Get list of Mql for that childs
                        MqlList = DataList.Where(a => a.ParentUniqueId == ParentDetail.ParentUniqueId && a.MQL != null)
                           .Select(a => a.MQL).ToList();

                        // Check there is any parent or not
                        bool CheckisParent = DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).Any();
                        if (CheckisParent && MqlList.Count > 0)
                        {
                            // Assign the sum of value to parent
                            DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).FirstOrDefault()
                               .MQL = MqlList.Sum(ab => ab.Value);

                        }
                    }

                }
            }
            catch { throw; }
            return DataList;
        }

        /// <summary>
        /// Calculate Revenue and MQL value for Program,Campaign, and Plan based on Tactic values
        /// </summary>
        public List<GridDefaultModel> RoundupRevenueforHireachyData(List<GridDefaultModel> DataList)
        {
            try
            {
                // Reverse the list from Plan -> Lineitem to Lineitem -> Plan
                // Here is anyonymos variable so need to use var type
                var ListofParentIds = DataList
                    .Select(a => new
                    {
                        a.ParentUniqueId,
                        a.EntityType
                    }).Distinct()
                .Reverse()
                .ToList();

                //Roundup the values from child to parent
                List<decimal?> RevenueList = new List<decimal?>();
                foreach (var ParentDetail in ListofParentIds)
                {
                    if (string.Compare(ParentDetail.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) != 0)
                    {
                        // Get list of Revenues for that childs
                        RevenueList = DataList.Where(a => a.ParentUniqueId == ParentDetail.ParentUniqueId && a.Revenue != null)
                           .Select(a => a.Revenue).ToList();

                        // Check there is any parent or not
                        bool CheckisParent = DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).Any();
                        if (CheckisParent && RevenueList.Count > 0)
                        {
                            // Assign the sum of value to parent
                            DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).FirstOrDefault()
                                .Revenue = RevenueList.Sum(ab => ab.Value);
                        }
                    }
                }
            }
            catch { throw; }
            return DataList;
        }
        #endregion

        #region Hireachy Methods
        /// <summary>
        /// Add By Nishant Sheth
        /// Get first row of plan grid or parent row 
        /// </summary>
        IEnumerable<PlanGridColumnData> GetTopLevelRowsGrid(List<PlanGridColumnData> DataList, string minParentId)
        {
            try
            {
                return DataList
                  .Where(row => row.ParentUniqueId == minParentId || row.ParentUniqueId == string.Empty);
            }
            catch
            { throw; }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get Childs records of parent entity
        /// </summary>
        IEnumerable<PlanGridColumnData> GetChildrenGrid(List<PlanGridColumnData> DataList, string parentId)
        {
            try
            {
                return DataList
                  .Where(row => row.ParentUniqueId == parentId);
            }
            catch { throw; }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Genrate items for hireachy 
        /// </summary>
        PlanDHTMLXGridDataModel CreateItemGrid(List<PlanGridColumnData> DataList, PlanGridColumnData Row, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            List<PlanDHTMLXGridDataModel> children = new List<PlanDHTMLXGridDataModel>();
            try
            {
                // Get entity childs records
                IEnumerable<PlanGridColumnData> lstChildren = GetChildrenGrid(DataList, Row.UniqueId);

                // Call recursive if any other child entity
                children = lstChildren
                .Select(r => CreateItemGrid(DataList, r, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate)).ToList();

                EntitydataobjItem = new List<Plandataobj>();

                // Get list of custom field values for particular entity based on pivoted entities list
                List<Plandataobj> lstCustomfieldData = EntityCustomDataValues.Where(a => a.UniqueId == (Row.EntityType + "_" + Row.EntityId))
                                           .Select(a => a.CustomFieldData).FirstOrDefault();

                if (lstCustomfieldData == null)
                {
                    lstCustomfieldData = EmptyCustomValues;
                }

                // Set the values of row
                EntitydataobjItem = GridDataRow(Row, EntitydataobjItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, lstCustomfieldData);

            }
            catch
            {
                throw;
            }
            return new PlanDHTMLXGridDataModel { id = (Row.EntityType + "_" + Row.EntityId), data = EntitydataobjItem.Select(a => a).ToList(), rows = children, bgColor = string.Empty, open = GridEntityOpenState(Row.EntityType), userdata = GridUserData(Row.EntityType, Row.UniqueId) };
        }
        #endregion

        #region Create Data object for Grid
        /// <summary>
        /// Add By Nishant Sheth
        /// Set the grid rows for entities
        /// </summary>
        public List<Plandataobj> GridDataRow(PlanGridColumnData Row, List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<Plandataobj> objCustomfieldData)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                EntitydataobjCreateItem.AddRange(Row.lstdata);
                #endregion

                #region Set Customfield Columns Values
                EntitydataobjCreateItem.AddRange(objCustomfieldData);
                #endregion
            }
            catch { throw; }
            return EntitydataobjCreateItem;
        }

        #endregion

        #region Method to convert number in k, m formate

        /// <summary>
        /// Mehtod for convert number to formated string
        /// </summary>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid GridUserData(string EntityType, string UniqueId)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            try
            {
                // Get data of plan grid default columns data from cache 
                List<GridDefaultModel> DataList = (List<GridDefaultModel>)objCache.Returncache(Convert.ToString(Enums.CacheObject.ListPlanGridDefaultData));
                if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Campaign), true) == 0)
                {
                    objUserData = CampaignUserData(DataList, UniqueId);
                }
                else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Program), true) == 0)
                {
                    objUserData = ProgramUserData(DataList, UniqueId);
                }
                else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
                {
                    if (!string.IsNullOrEmpty(UniqueId))
                    {
                        GridDefaultModel Row = DataList.Where(a => a.UniqueId == UniqueId).FirstOrDefault();
                        if (Row != null)
                        {
                            objUserData = TacticUserData(Row);
                        }
                    }
                }
                else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) == 0)
                {
                    if (!string.IsNullOrEmpty(UniqueId))
                    {
                        GridDefaultModel Row = DataList.Where(a => a.UniqueId == UniqueId).FirstOrDefault();
                        if (Row != null)
                        {
                            objUserData = LineItemUserData(Row);
                        }
                    }
                }
            }
            catch { throw; }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Campaign entities this user data will be manage from ui side when we change the entities start date and end date
        /// When we change the end date max compare to it's parent entity that time we will not refresh the whole grid update from JS side
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Planuserdatagrid CampaignUserData(List<GridDefaultModel> DataList, string UniqueId)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
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
            catch { throw; }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Program entities
        /// When we change the end date max compare to it's parent entity that time we will not refresh the whole grid update from JS side
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Planuserdatagrid ProgramUserData(List<GridDefaultModel> DataList, string UniqueId)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
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
            catch { throw; }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Tactic entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Planuserdatagrid TacticUserData(GridDefaultModel Row)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            try
            {
                objUserData.stage = Row.ProjectedStage;
                objUserData.tactictype = Convert.ToString(Row.TacticTypeId);
            }
            catch { throw; }
            return objUserData;
        }

        /// <summary>
        /// Set the user data for Lineitem entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Planuserdatagrid LineItemUserData(GridDefaultModel Row)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
            try
            {
                string IsOther = Convert.ToString(!string.IsNullOrEmpty(Row.LineItemType) ? false : true);
                objUserData.IsOther = IsOther;
            }
            catch { throw; }
            return objUserData;
        }
        #endregion


        #region Get Mql Title
        /// <summary>
        /// Get the client wise mql stage title 
        /// </summary>
        private string GetMqlTitle(int ClientId)
        {
            string MQLTitle = Convert.ToString(Enums.PlanGoalType.MQL);
            // Get MQL title label client wise
            List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false)
                .Select(stage => stage).ToList();
            if (!(stageList.Count > 0))
            {
                MQLTitle = stageList.Where(stage => stage
                       .Code.ToLower() == Convert.ToString(Enums.PlanGoalType.MQL).ToLower()).Select(stage => stage.Title).FirstOrDefault();
            }
            return MQLTitle;
        }
        #endregion

        #region Select Specific Columns dynamic
        // From this method we pass the array of column list and select it's values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlanGridColumnData Projection(object RowData, IEnumerable<string> props)
        {
            PlanGridColumnData objres = new PlanGridColumnData();
            if (RowData == null)
            {
                return null;
            }
            try
            {
                //List<PlanGridColumnData> res = new List<PlanGridColumnData>();
                Plandataobj objPlanData = new Plandataobj();
                List<Plandataobj> lstPlanData = new List<Plandataobj>();
                Type type = RowData.GetType();

                // Set attribute values for add columns string as html and maintain hireachy
                objres = InsertAttributeValueforAddColumns(RowData, objres);

                // Insert Hidden field values
                lstPlanData.AddRange(InsertHiddenDefaultColumnsValues(RowData, objres));

                // Set user selected columns values
                foreach (var pair in props.Select(n => new
                {
                    Name = n,
                    Property = type.GetProperty(n)
                }))
                {
                    objPlanData = new Plandataobj();
                    if (pair.Property != null)
                    {
                        objPlanData.value = GetvalueFromObject(RowData, pair.Name);
                        objPlanData.actval = GetvalueFromObject(RowData, pair.Name);
                    }
                    lstPlanData.Add(objPlanData);
                }

                objres.lstdata = lstPlanData;
            }
            catch { throw; }
            return objres;
        }

        /// <summary>
        /// Return the hidden columns data for particular Entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<Plandataobj> InsertHiddenDefaultColumnsValues(object RowData, PlanGridColumnData objres)
        {
            // Insert Hidden field values
            List<Plandataobj> lstPlanData = new List<Plandataobj>();
            try
            {
                lstHomeGrid_Hidden_And_Default_Columns().Select(col => col.Key).ToList().ForEach(coldata =>
                {
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ActivityType), true) == 0)
                    {
                        lstPlanData.Add(new Plandataobj
                        {
                            value = objres.EntityType // Set Entity Type like Plan/Campaign etc...
                        });
                    }
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ColourCode), true) == 0)
                    {
                        lstPlanData.Add(new Plandataobj
                        {
                            style = "background-color:#" + objres.ColorCode // Set color Column
                        });
                    }
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.id), true) == 0)
                    {
                        lstPlanData.Add(new Plandataobj
                        {
                            value = Convert.ToString(objres.EntityId) // Set column ids
                        });
                    }
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName), true) == 0)
                    {

                        lstPlanData.Add(new Plandataobj
                        {
                            value = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType()
                                                                    .GetProperty(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName))
                                                                    .GetValue(RowData, new object[0]))) // set machine name for tactics
                        });
                    }
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TaskName), true) == 0)
                    {
                        string Roistring = string.Empty;

                        if (string.Compare(objres.EntityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
                        {
                            // Get Anchor Tactic Id
                            string AnchorTacticId = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType()
                                            .GetProperty("AnchorTacticID")
                                            .GetValue(RowData, new object[0])));

                            // Get Tactic Id
                            string EntityId = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType()
                                            .GetProperty("EntityId")
                                            .GetValue(RowData, new object[0])));

                            if (!string.IsNullOrEmpty(AnchorTacticId) && !string.IsNullOrEmpty(EntityId))
                            {
                                if (string.Compare(AnchorTacticId, EntityId, true) == 0) // If Anchor tacticid and Entity id both same then set ROI package icon
                                {
                                    // Get list of package tactic ids
                                    string PackageTacticIds = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType()
                                            .GetProperty("PackageTacticIds")
                                            .GetValue(RowData, new object[0])));

                                    Roistring = "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id=pkgIcon onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids=" + PackageTacticIds + "><i class='fa fa-object-group'></i></div>";
                                }
                            }
                        }
                        lstPlanData.Add(new Plandataobj
                        {
                            value = Roistring + HttpUtility.HtmlEncode(objres.EntityTitle) //Set Entity title
                        });
                    }
                    if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Add), true) == 0)
                    {
                        lstPlanData.Add(new Plandataobj
                        {
                            value = AddColumnString(objres) //Set Add icon html string for plan grid
                        });
                    }
                });
            }
            catch { throw; }
            return lstPlanData;
        }

        /// <summary>
        /// Update refence variable values fo Add columns icon's html attribute values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PlanGridColumnData InsertAttributeValueforAddColumns(object RowData, PlanGridColumnData objres)
        {
            Int64 ParentEntityId = 0;
            int LineItemTypeId = 0;
            int AnchorTacticID = 0;
            try
            {
                objres.EntityId = Int64.Parse(GetvalueFromObject(RowData, "EntityId"));
                objres.Owner = int.Parse(GetvalueFromObject(RowData, "Owner"));
                objres.AltId = GetvalueFromObject(RowData, "AltId");
                objres.ColorCode = GetvalueFromObject(RowData, "ColorCode");
                objres.TaskId = GetvalueFromObject(RowData, "TaskId");
                objres.UniqueId = GetvalueFromObject(RowData, "UniqueId");
                objres.ParentUniqueId = GetvalueFromObject(RowData, "ParentUniqueId");
                objres.EntityType = GetvalueFromObject(RowData, "EntityType");

                Int64.TryParse(GetvalueFromObject(RowData, "ParentEntityId"), out ParentEntityId);
                objres.ParentEntityId = ParentEntityId;

                objres.AssetType = GetvalueFromObject(RowData, "AssetType");
                objres.TacticType = GetvalueFromObject(RowData, "TacticType");
                objres.StartDate = Convert.ToDateTime(GetvalueFromObject(RowData, "StartDate"));
                objres.EndDate = Convert.ToDateTime(GetvalueFromObject(RowData, "EndDate"));
                objres.EntityTitle = GetvalueFromObject(RowData, "EntityTitle");

                int.TryParse(GetvalueFromObject(RowData, "LineItemTypeId"), out LineItemTypeId);
                objres.LineItemTypeId = LineItemTypeId;
                objres.LineItemType = GetvalueFromObject(RowData, "LineItemType");

                int.TryParse(GetvalueFromObject(RowData, "AnchorTacticID"), out AnchorTacticID);
                objres.AnchorTacticID = AnchorTacticID;

            }
            catch { throw; }
            return objres;
        }

        /// <summary>
        /// return cell value for plan grid data
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetvalueFromObject(object RowData, string ColumnName)
        {
            string objVal = null;
            try
            {
                if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.StartDate), true) == 0 ||
                    string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.EndDate), true) == 0)
                {
                    objVal = Convert.ToDateTime(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0])).ToString("MM/dd/yyyy");
                }
                else if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost), true) == 0)
                {
                    string Cost = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                    double PlannedCost = 0;
                    double.TryParse(Convert.ToString(Cost), out PlannedCost);
                    objVal = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                }
                else if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal), true) == 0)
                {
                    string ProjectedStageValue = Convert.ToString(RowData.GetType().GetProperty("ProjectedStageValue").GetValue(RowData, new object[0]));
                    string ProjectedStage = Convert.ToString(RowData.GetType().GetProperty("ProjectedStage").GetValue(RowData, new object[0]));

                    if (!string.IsNullOrEmpty(ProjectedStageValue) && !string.IsNullOrEmpty(ProjectedStage))
                    {
                        objVal = Convert.ToString((Math.Round(Convert.ToDouble(ProjectedStageValue)) > 0 ?
                            Math.Round(Convert.ToDouble(ProjectedStageValue)).ToString("#,#") : "0") + " " + ProjectedStage);
                    }
                }
                else if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL), true) == 0)
                {
                    string MQL = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                    double PlannedMQL = 0;
                    double.TryParse(Convert.ToString(MQL), out PlannedMQL);
                    objVal = ConvertNumberToRoundFormate(PlannedMQL);
                }
                else if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue), true) == 0)
                {
                    string Revenue = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                    double PlannedRevenue = 0;
                    double.TryParse(Convert.ToString(Revenue), out PlannedRevenue);
                    objVal = PlanCurrencySymbol + ConvertNumberToRoundFormate(PlannedRevenue);
                }
                else
                {
                    objVal = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                }
            }
            catch { throw; }
            return objVal;
        }

        #region Add Column string
        /// <summary>
        ///  Return plan grid add column's icon html value
        /// </summary>
        private string AddColumnString(PlanGridColumnData Row)
        {
            string addColumn = string.Empty;
            try
            {
                if (string.Compare(Row.EntityType, Convert.ToString(Enums.EntityType.Plan), true) == 0)
                {
                    return PlanAddString(Row);
                }
                else if (string.Compare(Row.EntityType, Convert.ToString(Enums.EntityType.Campaign), true) == 0)
                {
                    return CampaignAddString(Row);
                }
                else if (string.Compare(Row.EntityType, Convert.ToString(Enums.EntityType.Program), true) == 0)
                {
                    return ProgroamAddString(Row);
                }
                else if (string.Compare(Row.EntityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
                {
                    return TacticAddString(Row);
                }
                else if (string.Compare(Row.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) == 0)
                {
                    return LineItemAddString(Row);
                }
            }
            catch { throw; }
            return addColumn;
        }

        // Set the Plan add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PlanAddString(PlanGridColumnData Row, bool IsEditable = true)
        {
            string grid_add = string.Empty;
            if (IsEditable)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId +
                " per=" + Convert.ToString(IsEditable).ToLower() +
                " title=Add><i class='fa fa-plus-circle'></i></div>";
            }

            string addColumn = "<div class=grid_Search id=Plan title=View><i class='fa fa-search'></i></div>" +
                grid_add
                + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=Plan dhtmlxrowid='"
                + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.Owner)
                + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode + "' altId=" + Row.EntityId
                + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>";
            return addColumn;
        }

        // Set the Campaign add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string CampaignAddString(PlanGridColumnData Row, bool IsEditable = true)
        {
            string grid_add = string.Empty;
            if (IsEditable)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.AltId +
                " per=" + Convert.ToString(IsEditable).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
            }

            string addColumn = "<div class=grid_Search id=CP title=View><i class='fa fa-search'></i></div>"
                + grid_add
                + "<div class=honeycombbox-icon-gantt id=Campaign onclick=javascript:AddRemoveEntity(this) title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType
                + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '"
                + Convert.ToString(Row.Owner) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;"))
                + "' altId=" + Row.AltId + " per=" + Convert.ToString(IsEditable).ToLower() + "' taskId= " + Row.EntityId + " csvId=Campaign_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the program add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ProgroamAddString(PlanGridColumnData Row, bool IsEditable = true)
        {
            string grid_add = string.Empty;
            if (IsEditable)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Program alt=_" + Row.AltId +
                " per=" + Convert.ToString(IsEditable).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
            }
            string addColumn = "<div class=grid_Search id=PP title=View><i class='fa fa-search'></i></div>"
                + grid_add
                + " <div class=honeycombbox-icon-gantt id=Program onclick=javascript:AddRemoveEntity(this);  title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId
                + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "' OwnerName= '" + Convert.ToString(Row.Owner)
                + "'  TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "'  altId=_" + Row.AltId +
                " per=" + IsEditable.ToString().ToLower() + "'  taskId= " + Row.EntityId + " csvId=Program_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the tactic add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string TacticAddString(PlanGridColumnData Row, bool IsEditable = true)
        {
            string grid_add = string.Empty;
            if (IsEditable)
            {
                grid_add = "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Tactic alt=__" + Row.ParentEntityId + "_" + Row.EntityId +
                " per=" + IsEditable.ToString().ToLower() + "  LinkTacticper ='" + false + "' LinkedTacticId = '" + 0
                + "' tacticaddId='" + Row.EntityId + "' title=Add><i class='fa fa-plus-circle'></i></div>";
            }
            string addColumn = "<div class=grid_Search id=TP title=View><i class='fa fa-search'></i></div>"
                + grid_add
                + " <div class=honeycombbox-icon-gantt id=Tactic onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb'  pcptid = " + Row.TaskId
                + " anchortacticid='" + Row.AnchorTacticID + "' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "'  roitactictype='" + Row.AssetType
                + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode
                + "'  TacticType= '" + Row.TacticType + "' OwnerName= '" + Convert.ToString(Row.Owner) + "' altId=__" + Row.ParentEntityId + "_" + Row.EntityId
                + " per=" + IsEditable.ToString().ToLower() + "' taskId=" + Row.EntityId + " csvId=Tactic_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the line item add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string LineItemAddString(PlanGridColumnData Row, bool IsEditable = true)
        {
            string grid_add = string.Empty;
            if (IsEditable)
            {
                int LineItemTypeId = 0;
                if (Row.LineItemType != null)
                {
                    int.TryParse(Convert.ToString(Row.LineItemTypeId), out LineItemTypeId);
                }

                grid_add = "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Line alt=___" + Row.ParentEntityId + "_" + Row.EntityId
                + " lt=" + LineItemTypeId
                + " dt=" + HttpUtility.HtmlEncode(Row.EntityTitle) + " per=" + Convert.ToString(IsEditable).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
            }
            string addColumn = "<div class=grid_Search id=LP title=View><i class='fa fa-search'></i></div>"
                + grid_add;
            return addColumn;
        }

        #endregion
        #endregion
        #endregion

        #region "Calendar Related Functions"
        /// Createdy By: Viral
        /// Created On: 09/19/2016
        // Desc: Return List of Plan, Campaign, Program, Tactic
        public List<calendarDataModel> GetPlanCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string timeframe, string planYear, string viewby)
        {
            #region "Declare Variables"
            SqlParameter[] para = new SqlParameter[7];
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
                para[6] = new SqlParameter()
                {
                    ParameterName = "viewBy",
                    Value = viewby
                };
                #endregion

                #region "Get Data"
                calResultset = objDbMrpEntities.Database
                    .SqlQuery<calendarDataModel>("spGetPlanCalendarData @planIds,@ownerIds,@tactictypeIds,@statusIds,@timeframe,@planYear,@viewBy", para)
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
                    usr = lstUsersData.Where(u => data.CreatedBy.HasValue && u.Key == data.CreatedBy.Value).FirstOrDefault();
                    if (usr.Value != null)
                        data.OwnerName = string.Format("{0} {1}", Convert.ToString(usr.Value.FirstName), Convert.ToString(usr.Value.LastName));
                    #endregion

                    #region "Set Permission"
                    if (IsPlanCreateAllAuthorized == false)     // check whether user has plan create permission or not
                    {
                        if ((data.CreatedBy.HasValue) && (data.CreatedBy.Value.Equals(Sessions.User.ID) || lstSubordinatesIds.Contains(data.CreatedBy.Value))) // check whether Entity owner is own or it's subordinates.
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
        Func<IEnumerable<T>, TData> dataSelector,
         List<string> selectedColumns
            )
        {

            List<CustomfieldPivotData> arr = new List<CustomfieldPivotData>();
            List<string> cols = new List<string>();
            String rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            if (source != null)
            {
                IEnumerable<TColumn> columns = source.Select(columnSelector).Distinct()
                                        .ToList().Where(a => selectedColumns.Contains(a.ToString())).ToList();

                cols = (new[] { rowName }).Concat(selectedColumns).ToList();

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
                    CustomfieldPivotData obj = GetAnonymousObject(cols, items);
                    arr.Add(obj);
                }
            }
            return arr.ToList();
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// get values for pivoting entites
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static dynamic GetAnonymousObject(IEnumerable<string> columns, IEnumerable<object> values)
        {
            CustomfieldPivotData objCustomPivotData = new CustomfieldPivotData();
            List<Plandataobj> lstCustomFieldData = new List<Plandataobj>();
            int i;
            for (i = 0; i < columns.Count(); i++)
            {
                if (i == 0)
                {
                    objCustomPivotData.UniqueId = Convert.ToString(values.ElementAt<object>(i));
                }
                else
                {
                    string DataValue = string.Empty;
                    // Check the index of columns is greater or not // to handle index exception error
                    if (values.Count() > i)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(values.ElementAt<object>(i))))
                        {
                            DataValue = Convert.ToString(values.ElementAt<object>(i));
                        }
                    }
                    lstCustomFieldData.Add(
                        new Plandataobj
                        {
                            locked = objHomeGrid.lockedstateone,
                            value = DataValue,
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