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
using System.Text.RegularExpressions;
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
        List<CustomfieldPivotData> EntityCustomDataValues = new List<CustomfieldPivotData>(); // Set Custom fields value for entities
        List<PlandataobjColumn> EmptyCustomValues; // Variable for assigned blank values for custom field
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        HomeGridProperties objHomeGridProp = new HomeGridProperties();
        double PlanExchangeRate = 1; // set currency plan exchange rate. it's variable value update from 'GetPlanGrid' method
        string PlanCurrencySymbol = "$"; // set currency symbol. it's variable value update from 'GetPlanGrid' method
        public string ColumnManagmentIcon = Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon);
        int _ClientId;
        int _UserId;
        bool IsUserView = false;
        #endregion

        // Constructor
        public Grid()
        {
            objDbMrpEntities = new MRPEntities(); // Create Entities object
            objCache = new CacheObject(); // Create Cache object for stored data
            objColumnView = new ColumnView();
        }

        #region Plan Grid Methods

        #region Method to get grid default data
        /// <summary>
        /// Add By Nishant Sheth
        /// call stored procedure to get list of plan and all related entities for home grid, based on client and filters selected by user 
        /// <summary>
        public List<GridDefaultModel> GetGridDefaultData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string viewBy)
        {
            List<GridDefaultModel> EntityList = new List<GridDefaultModel>();
            SqlParameter[] para = new SqlParameter[6];

            para[0] = new SqlParameter { ParameterName = "PlanId", Value = PlanIds };

            para[1] = new SqlParameter { ParameterName = "ClientId", Value = ClientId };

            para[2] = new SqlParameter { ParameterName = "OwnerIds", Value = ownerIds };

            para[3] = new SqlParameter { ParameterName = "TacticTypeIds", Value = TacticTypeid };

            para[4] = new SqlParameter { ParameterName = "StatusIds", Value = StatusIds };

            para[5] = new SqlParameter { ParameterName = "ViewBy", Value = viewBy };

            EntityList = objDbMrpEntities.Database
                .SqlQuery<GridDefaultModel>("GetGridData @PlanId,@ClientId,@OwnerIds,@TacticTypeIds,@StatusIds,@ViewBy", para)
                .OrderBy(a => a.EntityTitle).ToList();
            return EntityList;
        }
        #endregion

        #region Mtehod to get grid customfield and it's entity value
        /// <summary>
        /// Add By Nishant Sheth
        /// call stored procedure to get all related custom fields and their values for selected plans and entities , based on client and filters selected by user 
        /// There are 2 results set from the GridCustomFieldData sproc- 1.CustomField master list 2. Values of custom fields for entities with in selected plans 
        /// TODO :: Working to combine 'GridCustomFieldData' Sproc and 'GetGridData' Sproc so that we can get result in single db call it's covered in #2572 PL ticket.
        /// <summary>
        private GridCustomColumnData GetGridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, List<string> customColumnslist, int UserId)
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
                cmd.CommandTimeout = 0;
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

                DbParameter paraUserId = cmd.CreateParameter();
                paraUserId.ParameterName = "UserId";
                paraUserId.DbType = DbType.Int32;
                paraUserId.Direction = ParameterDirection.Input;
                paraUserId.Value = UserId;

                DbParameter paraSelectedCustomField = cmd.CreateParameter();
                paraSelectedCustomField.ParameterName = "SelectedCustomField";
                paraSelectedCustomField.DbType = DbType.String;
                paraSelectedCustomField.Direction = ParameterDirection.Input;
                paraSelectedCustomField.Value = string.Join(",", customColumnslist);

                cmd.Parameters.Add(paraPlanId);
                cmd.Parameters.Add(paraClientId);
                cmd.Parameters.Add(paraOwnerIds);
                cmd.Parameters.Add(paraTacticTypeIds);
                cmd.Parameters.Add(paraStatusIds);
                cmd.Parameters.Add(paraUserId);
                cmd.Parameters.Add(paraSelectedCustomField);

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
        public PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string CurrencySymbol, double ExchangeRate, int UserId, EntityPermission objPermission, List<int> lstSubordinatesIds, string viewBy)
        {
            _ClientId = ClientId;
            _UserId = UserId;
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            PlanExchangeRate = ExchangeRate; // Set client currency plan exchange rate 
            PlanCurrencySymbol = CurrencySymbol; // Set user currency symbol

            // Get MQL title label client wise
            string MQLTitle = GetMqlTitle(ClientId);

            // Get list of user wise columns or default columns of grid view it's refrencely update from GenerateJsonHeader Method
            List<string> HiddenColumns = new List<string>(); // List of hidden columns of plan grid
            List<string> UserDefinedColumns = new List<string>(); // List of User selected or default columns list
            List<string> customColumnslist = new List<string>(); // List of custom field columns
            // Get user column management data
            User_CoulmnView objUserView = objDbMrpEntities.User_CoulmnView.Where(a => a.CreatedBy == UserId).FirstOrDefault();
            List<AttributeDetail> UserColitems = null;
            Dictionary<int, string> usercolindex = new Dictionary<int, string>();
            if (objUserView != null && !string.IsNullOrEmpty(objUserView.GridAttribute))
            {
                XDocument doc = XDocument.Parse(objUserView.GridAttribute);
                // Read the xml data from user column view
                UserColitems = objColumnView.UserSavedColumnAttribute(doc);
                // Add hidden columns 
                lstHomeGrid_Hidden_And_Default_Columns().Select(a => a.Key.ToString()).ToList().ForEach(a =>
                {
                    usercolindex.Add((usercolindex.Count + 1), a);
                });
                // Add common/standard columns 
                UserColitems.Select(a => a).ToList().ForEach(a =>
                {
                    // Add Column id same as attribute id where attribute type is common
                    if (string.Compare(a.AttributeType, Convert.ToString(Enums.HomeGridColumnAttributeType.Common), true) == 0 && !(lstHomeGrid_Hidden_And_Default_Columns().Select(hidden => hidden.Key.ToString()).Contains(a.AttributeId)))
                    { usercolindex.Add((usercolindex.Count + 1), a.AttributeId); }
                    else if (string.Compare(a.AttributeType, Convert.ToString(Enums.HomeGridColumnAttributeType.Common), true) == 1)
                    { usercolindex.Add((usercolindex.Count + 1), ("custom_" + a.AttributeId + ":" + a.AttributeType)); } // Add Column id with custom_CustomFieldId:EntityType combination
                });
            }
            // Generate header columns for grid
            List<PlanHead> ListOfDefaultColumnHeader = GenerateJsonHeader(MQLTitle, ref HiddenColumns, ref UserDefinedColumns, ref customColumnslist, UserId, ref IsUserView);

            //Get list of entities for plan grid
            List<GridDefaultModel> GridHireachyData = GetGridDefaultData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds, viewBy);

            //Filter custom field
            if (GridHireachyData != null && GridHireachyData.Count > 0 && !string.IsNullOrEmpty(customFieldIds))
            {
                GridHireachyData = FilterCustomField(GridHireachyData, customFieldIds);
            }

            // Update Plan Start and end date
            GridHireachyData = UpdatePlanStartEndDate(GridHireachyData);
            // Get List of custom fields and it's entity's values
            GridCustomColumnData ListOfCustomData = new GridCustomColumnData();
            if (customColumnslist != null && customColumnslist.Count > 0)
            {
                ListOfCustomData = GridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customColumnslist, UserId, IsUserView);
            }
            List<Int64> lsteditableEntityIds = GetEditableTacticIds(GridHireachyData, ListOfCustomData, UserId, ClientId);
            // Set Row wise permission
            GridHireachyData = GridRowPermission(GridHireachyData, objPermission, lstSubordinatesIds, lsteditableEntityIds, UserId);

            List<EntityPermissionRowWise> EntityRowPermission = GridHireachyData.Select(a => new EntityPermissionRowWise
            {
                IsRowPermission = a.IsRowPermission,
                EntityId = a.EntityId.ToString(),
                EntityType = a.EntityType,
                LineItemType = a.LineItemType
            }).ToList();

            PivotcustomFieldData(ref customColumnslist, ListOfCustomData, ref IsUserView, EntityRowPermission);

            // Check is user select Planned Cost column in column saved view
            bool IsPlanCostColumn = UserDefinedColumns.Where(a =>
               a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost).ToLower()).Any();
            if (IsPlanCostColumn)
            {
                // Round up the Planned Cost value for Program/Campaign/Plan
                GridHireachyData = RoundupValues(GridHireachyData, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost));
            }

            // Check is user select Revenue column in column saved view
            bool IsRevenueColumn = UserDefinedColumns.Where(a =>
                a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue).ToLower()).Any();
            if (IsRevenueColumn)
            {
                // Round up the Revenue value for Program/Campaign/Plan
                GridHireachyData = RoundupValues(GridHireachyData, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue));
            }

            // Check is user select MQL column in column saved view
            bool IsMQLColumn = UserDefinedColumns.Where(a =>
               a.ToLower() == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL).ToLower()).Any();
            if (IsMQLColumn)
            {
                // Round up the MQL value for Program/Campaign/Plan
                GridHireachyData = RoundupValues(GridHireachyData, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL));
            }

            // Get selected columns data
            List<PlanGridColumnData> lstSelectedColumnsData = GridHireachyData.Select(a => Projection(a, UserDefinedColumns, viewBy)).ToList();

            // Merge header of plan grid with custom fields
            ListOfDefaultColumnHeader.AddRange(GridCustomHead(ListOfCustomData.CustomFields, customColumnslist));

            if (UserColitems != null && UserColitems.Count > 0)
            {
                ListOfDefaultColumnHeader = (from colsort in usercolindex
                                             join HeaderCol in ListOfDefaultColumnHeader
                                                 on colsort.Value equals HeaderCol.id
                                             orderby colsort.Key
                                             select HeaderCol).ToList();

            }
            // Generate Hierarchy of Plan grid
            List<PlanDHTMLXGridDataModel> griditems = GetTopLevelRowsGrid(lstSelectedColumnsData, null)
                     .Select(row => CreateItemGrid(lstSelectedColumnsData, row, ListOfCustomData, PlanCurrencySymbol, PlanExchangeRate, customColumnslist, GridHireachyData, usercolindex))
                     .ToList();

            objPlanMainDHTMLXGrid.head = ListOfDefaultColumnHeader;
            objPlanMainDHTMLXGrid.rows = griditems;
            return objPlanMainDHTMLXGrid;
        }

        /// <summary>
        /// Get list of Editable tactic ids list
        /// </summary>
        private List<Int64> GetEditableTacticIds(List<GridDefaultModel> GridHireachyData, GridCustomColumnData ListOfCustomData, int UserId, int ClientId)
        {
            List<int> lstTacticIds = GridHireachyData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower()).Select(a => int.Parse(a.EntityId.ToString())).ToList();
            List<CustomField_Entity> customfieldEntitylist = new List<CustomField_Entity>();
            List<Int64> lsteditableEntityIds = new List<Int64>();
            if (ListOfCustomData != null && ListOfCustomData.CustomFields != null && ListOfCustomData.CustomFieldValues != null)
            {
                // Split the comma separated values to list
                var lstCommaSplitCustomfield = ListOfCustomData.CustomFieldValues.Where(a => a.Value != null).Select(a => new
                {
                    CustomFieldId = a.CustomFieldId,
                    EntityId = a.EntityId,
                    ListofValues = a.Value.Split(',')
                }).ToList();

                // Add item to custom field entity list
                int ParseCustomValue = 0;
                foreach (var data in lstCommaSplitCustomfield.Where(a => a.ListofValues.Count() > 1).ToList())
                {
                    foreach (var val in data.ListofValues)
                    {
                        int.TryParse(val, out ParseCustomValue);
                        customfieldEntitylist.Add(new CustomField_Entity { CustomFieldId = int.Parse(data.CustomFieldId.ToString()), EntityId = int.Parse(data.EntityId.ToString()), Value = ParseCustomValue.ToString() });
                    }
                }

                // Add items which have only single value for entity
                customfieldEntitylist.AddRange(lstCommaSplitCustomfield.Where(a => a.ListofValues.Count() == 1)
                    .Select(a => new CustomField_Entity { CustomFieldId = int.Parse(a.CustomFieldId.ToString()), EntityId = int.Parse(a.EntityId.ToString()), Value = Convert.ToString(a.ListofValues[0]) }).ToList());

                ListOfCustomData.CustomFieldValues.Where(a => a.EntityId != null && a.CustomFieldId != null)
                .Select(a => new CustomField_Entity { CustomFieldId = int.Parse(a.CustomFieldId.ToString()), EntityId = int.Parse(a.EntityId.ToString()), Value = a.Value }).ToList();

                // Get list of editable list of tactic ids for permission
                lsteditableEntityIds = Common.GetEditableTacticList(UserId, ClientId, lstTacticIds, false, customfieldEntitylist)
                   .Select(a => Int64.Parse(a.ToString())).ToList();
            }
            return lsteditableEntityIds;
        }


        /// <summary>
        /// Update plan start and end date based on campaign start date and end date
        /// </summary>
        public List<GridDefaultModel> UpdatePlanStartEndDate(List<GridDefaultModel> GridData)
        {
            List<GridDefaultModel> CopyGridData = GridData; // Copy the grid data to get it's Childs data
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
            return CopyGridData;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Get list of custom fields values for each entities
        /// </summary>
        private GridCustomColumnData GridCustomFieldData(string PlanIds, int ClientId, string ownerIds, string TacticTypeid, string StatusIds, List<string> customColumnslist, int UserId, bool IsUserView)
        {
            GridCustomColumnData data = new GridCustomColumnData();
            if (IsUserView)
            {
                // Call the method of stored procedure that return list of custom field and it's entities values
                data = GetGridCustomFieldData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customColumnslist, UserId);
            }
            return data;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Pivot Selected custom field columns data 
        /// </summary>
        private void PivotcustomFieldData(ref List<string> selectedCustomColumns, GridCustomColumnData data, ref bool IsUserView, List<EntityPermissionRowWise> EntityRowPermission)
        {
            if (IsUserView)
            {
                // Update selectedCustomColumns variable with list of user selected/all custom fields columns name
                if (data.CustomFields != null)
                {
                    selectedCustomColumns = data.CustomFields.Select(a => Convert.ToString(a.CustomFieldId)).ToList();
                }
            }
            // Pivoting the custom fields entities values //EntityCustomDataValues declare globally at class level and it's use with hierarchy process

            EntityCustomDataValues = data.CustomFieldValues.ToPivotList(item => item.CustomFieldId,
                     item => item.UniqueId,
                        items => items.Max(a => a.Text)
                        , selectedCustomColumns
                        , data.CustomFields
                        , EntityRowPermission)
                        .ToList();

            // Create empty list of custom field values for entity where there is no any custom fields value on entity
            List<PlandataobjColumn> lstCustomPlanData = new List<PlandataobjColumn>();
            if (selectedCustomColumns != null && selectedCustomColumns.Count > 0)
            {
                selectedCustomColumns.ForEach(a =>
                {
                    lstCustomPlanData.Add(new PlandataobjColumn
                    {
                        value = string.Empty,
                        locked = objHomeGridProp.lockedstateone,
                        style = objHomeGridProp.stylecolorgray,
                        column = data.CustomFields.Where(cust => a == cust.CustomFieldId.ToString()).Select(cust => cust.CustomUniqueId).FirstOrDefault()
                    });
                });
            }
            EmptyCustomValues = lstCustomPlanData;
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Create header of custom fields for plan grid
        /// </summary>
        private List<PlanHead> GridCustomHead(List<GridCustomFields> lstCustomFields, List<string> customColumnslist)
        {
            List<PlanHead> ListHead = new List<PlanHead>();
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
            return ListHead;
        }

        /// <summary>
        /// Add by Nishant Sheth
        /// set open/close entity state for respective entity
        /// </summary>
        public string GridEntityOpenState(string EntityType, int ChildernCount)
        {
            if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Plan), true) == 0)
            {
                if (ChildernCount > 0)
                    return objHomeGridProp.openstateone;
            }
            else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Campaign), true) == 0)
            {
                if (ChildernCount > 0)
                    return objHomeGridProp.openstateone;
            }
            return string.Empty;
        }
        #endregion

        #region Method to generate grid header

        /// <summary>
        /// Add By Nishant Sheth
        /// Create Plan Grid header for default columns
        /// </summary>
        private List<PlanHead> GenerateJsonHeader(string MQLTitle, ref List<string> HiddenColumns, ref List<string> UserDefinedColumns, ref List<string> customColumnslist, int UserId, ref bool IsUserView)
        {
            List<PlanHead> headobjlist = new List<PlanHead>(); // List of headers detail of plan grid
            // Get user column view
            User_CoulmnView userview = objDbMrpEntities.User_CoulmnView.Where(a => a.CreatedBy == UserId).FirstOrDefault();
            // Add Default and hidden required columns into list
            headobjlist = lstHomeGrid_Hidden_And_Default_Columns().Select(a => a.Value).ToList();
            if (headobjlist != null && headobjlist.Count > 0)
            {
                HiddenColumns.AddRange(headobjlist.Select(a => a.value).ToList());
            }
            // Below condition for when user have no any specific column view of plan grid
            if (userview == null)
            {
                IsUserView = false;
                // Set option values for dropdown columns like Tactic type/ Owner/ Asset type
                List<PlanHead> lstDefaultColumns = new List<PlanHead>();
                string MqlString = MQLTitle + Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon); // set client wise mql title and column management icon;
                Dictionary<string, PlanHead> DictDefaultCoulmns = lstHomeGrid_Default_Columns();
                DictDefaultCoulmns.Where(a => a.Key == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL)).FirstOrDefault().Value.value = MqlString;
                lstDefaultColumns.AddRange(DictDefaultCoulmns.Select(a => a.Value).ToList());
                // Update UserDefinedColumns variable with default columns list
                UserDefinedColumns = lstDefaultColumns.Select(a => a.id).ToList();
                // Merge list with default columns list
                headobjlist.AddRange(lstDefaultColumns);
            }
            else
            {
                if (userview.GridAttribute == null)
                {
                    IsUserView = false;
                    // Set option values for dropdown columns like Tactic type/ Owner/ Asset type
                    List<PlanHead> lstDefaultColumns = new List<PlanHead>();
                    string MqlString = MQLTitle + Enums.GetEnumDescription(Enums.HomeGrid_Header_Icons.columnmanagementicon); // set client wise mql title and column management icon;
                    Dictionary<string, PlanHead> DictDefaultCoulmns = lstHomeGrid_Default_Columns();
                    DictDefaultCoulmns.Where(a => a.Key == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL)).FirstOrDefault().Value.value = MqlString;
                    lstDefaultColumns.AddRange(DictDefaultCoulmns.Select(a => a.Value).ToList());
                    // Update UserDefinedColumns variable with default columns list
                    UserDefinedColumns = lstDefaultColumns.Select(a => a.id).ToList();
                    // Merge list with default columns list
                    headobjlist.AddRange(lstDefaultColumns);
                }
                else
                {
                    // Get user grid view columns list
                    string attributexml = userview.GridAttribute;
                    IsUserView = true;
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
            return headobjlist;
        }

        // Below dictionary for default columns list of home grid // We are set the dhtmlx header properties 
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

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Status), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Status),
                sort = "str",
                width = 150,
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.Status) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost), new PlanHead
            {
                type = "edn",
                align = "center",
                id = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost),
                sort = "str",
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
                options = GetOwnerListForHeader(),
                value = Enums.GetEnumDescription(Enums.HomeGrid_Default_Hidden_Columns.Owner) + ColumnManagmentIcon
            });

            lstColumns.Add(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal), new PlanHead
            {
                type = "ed",
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
                value = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL) // Here we not set ColumnManagmentIcon because MQl Title will be different for clients it will be set when list get
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
            #region add integration ids
            lstColumns.Add(Convert.ToString(Enums.IntegrationIdType.Eloquaid), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.IntegrationIdType.Eloquaid),
                sort = "str",
                width = 250,
                value = Enums.Integration_Column[Enums.IntegrationIdType.Eloquaid.ToString()] + ColumnManagmentIcon
            });
            lstColumns.Add(Convert.ToString(Enums.IntegrationIdType.Salesforceid), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.IntegrationIdType.Salesforceid),
                sort = "str",
                width = 250,
                value = Enums.Integration_Column[Enums.IntegrationIdType.Salesforceid.ToString()] + ColumnManagmentIcon
            });
            lstColumns.Add(Convert.ToString(Enums.IntegrationIdType.Marketoid), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.IntegrationIdType.Marketoid),
                sort = "str",
                width = 250,
                value = Enums.Integration_Column[Enums.IntegrationIdType.Marketoid.ToString()] + ColumnManagmentIcon
            });
            lstColumns.Add(Convert.ToString(Enums.IntegrationIdType.WorkFrontid), new PlanHead
            {
                type = "ro",
                align = "center",
                id = Convert.ToString(Enums.IntegrationIdType.WorkFrontid),
                sort = "str",
                width = 250,
                value = Enums.Integration_Column[Enums.IntegrationIdType.WorkFrontid.ToString()] + ColumnManagmentIcon
            });
            #endregion
            return lstColumns;
        }

        // Below dictionary list default for every user. it's user have is any specific view or not.
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
                //Assgin "Activity" as header to get header name in export to excel.
                value = "Activity"
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

        #region Calculate Revenue/Mql/ Planned Cost for hierarchy
        /// <summary>
        /// Below method use for round up values of 
        /// </summary>
        public List<GridDefaultModel> RoundupValues(List<GridDefaultModel> DataList, string ColumnName)
        {
            List<string> EntityTypeOrder = new List<string>();// Variable use for ordering data list
            EntityTypeOrder.Add(Enums.EntityType.Lineitem.ToString().ToLower());
            EntityTypeOrder.Add(Enums.EntityType.Tactic.ToString().ToLower());
            EntityTypeOrder.Add(Enums.EntityType.Program.ToString().ToLower());
            EntityTypeOrder.Add(Enums.EntityType.Campaign.ToString().ToLower());
            EntityTypeOrder.Add(Enums.EntityType.Plan.ToString().ToLower());

            // Order by entity type from 'Plan -> Lineitem' to 'Lineitem -> Plan'
            // Here is anonymous variable so need to use var type
            var ListofParentIds = DataList
                .Select(a => new
                {
                    a.ParentUniqueId,
                    a.EntityType
                }).Distinct()
            .OrderBy(a => EntityTypeOrder.IndexOf(a.EntityType.ToLower()))
            .ToList();

            if (string.Compare(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue), ColumnName, true) == 0)
            {
                //Roundup the values from child to parent
                List<decimal?> RevenueList = new List<decimal?>();

                foreach (var ParentDetail in ListofParentIds)
                {
                    if (string.Compare(ParentDetail.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) != 0)
                    {
                        // Get list of Revenues for that Childs
                        RevenueList = DataList.Where(a => a.ParentUniqueId == ParentDetail.ParentUniqueId)
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
            else if (string.Compare(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MQL), ColumnName, true) == 0)
            {
                //Roundup the values from child to parent
                List<Int64?> MqlList = new List<Int64?>();
                foreach (var ParentDetail in ListofParentIds)
                {
                    if (string.Compare(ParentDetail.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) != 0)
                    {
                        // Get list of Mql for that Childs
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
            else if (string.Compare(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.PlannedCost), ColumnName, true) == 0)
            {
                //Roundup the values from child to parent
                List<double?> CostList = new List<double?>();
                foreach (var ParentDetail in ListofParentIds)
                {
                    if (string.Compare(ParentDetail.EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) != 0)
                    {
                        // Get list of Mql for that Childs
                        CostList = DataList.Where(a => a.ParentUniqueId == ParentDetail.ParentUniqueId && a.PlannedCost != null)
                           .Select(a => a.PlannedCost).ToList();

                        // Check there is any parent or not
                        bool CheckisParent = DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).Any();
                        if (CheckisParent && CostList.Count > 0)
                        {
                            // Assign the sum of value to parent
                            DataList.Where(a => a.UniqueId == ParentDetail.ParentUniqueId).FirstOrDefault()
                               .PlannedCost = CostList.Sum(ab => ab.Value);
                        }
                    }
                }
            }
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
            return DataList
              .Where(row => row.ParentUniqueId == parentId);
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Generate items for hierarchy 
        /// </summary>
        PlanDHTMLXGridDataModel CreateItemGrid(List<PlanGridColumnData> DataList, PlanGridColumnData Row, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<string> customColumnslist, List<GridDefaultModel> GridDefaultData, Dictionary<int, string> usercolindex)
        {
            List<PlanDHTMLXGridDataModel> children = new List<PlanDHTMLXGridDataModel>();
            try
            {
                // Get entity Childs records
                IEnumerable<PlanGridColumnData> lstChildren = GetChildrenGrid(DataList, Row.UniqueId);

                // Call recursive if any other child entity
                children = lstChildren
                .Select(r => CreateItemGrid(DataList, r, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, customColumnslist, GridDefaultData, usercolindex)).ToList();

                EntitydataobjItem = new List<Plandataobj>();

                // Get list of custom field values for particular entity based on pivoted entities list
                List<PlandataobjColumn> lstCustomfieldData = EntityCustomDataValues.Where(a => a.UniqueId == (Row.EntityType + "_" + Row.EntityId))
                                           .Select(a => a.CustomFieldData).FirstOrDefault();
                if (lstCustomfieldData == null && CustomFieldData.CustomFields != null)
                {
                    List<PlandataobjColumn> ItemEmptylist = new List<PlandataobjColumn>(); // Variable for empty list of custom fields value to assign entity 
                    // Get list of custom fields by entity type
                    List<string> EntityCustomFields = CustomFieldData.CustomFields.Where(a => a.EntityType.ToLower() == Row.EntityType.ToLower()).Select(a => a.CustomFieldId.ToString()).ToList();
                    // Get list of custom column indexes from custom field list
                    List<int> Colindexes = customColumnslist.Select((s, k) => new { Str = s, Index = k })
                                                .Where(x => EntityCustomFields.Contains(x.Str))
                                                .Select(x => x.Index).ToList();
                    // Set custom field is editable or not for respective entities
                    if (string.Compare(Row.EntityType, Enums.EntityType.Lineitem.ToString().ToLower(), true) == 0 && string.IsNullOrEmpty(Row.LineItemType))
                    {
                        Row.IsRowPermission = false;
                    }
                    if (Row.IsRowPermission)
                    {
                        for (int j = 0; j < EmptyCustomValues.Count; j++)
                        {
                            if (Colindexes.Contains(j))
                            {
                                ItemEmptylist.Add(new PlandataobjColumn { value = string.Empty, locked = objHomeGridProp.lockedstatezero, style = objHomeGridProp.stylecolorblack, column = EmptyCustomValues[j].column });
                            }
                            else
                            { ItemEmptylist.Add(EmptyCustomValues[j]); }
                        }
                    }
                    else
                    { ItemEmptylist.AddRange(EmptyCustomValues); }
                    lstCustomfieldData = ItemEmptylist;
                }

                // Set the values of row
                EntitydataobjItem = GridDataRow(Row, EntitydataobjItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate, lstCustomfieldData, usercolindex);
            }
            catch
            {
                throw;
            }
            return new PlanDHTMLXGridDataModel { id = (Row.TaskId), data = EntitydataobjItem.Select(a => a).ToList(), rows = children, open = GridEntityOpenState(Row.EntityType, children.Count), userdata = GridUserData(Row.EntityType, Row.UniqueId, GridDefaultData) };
        }
        #endregion

        #region Create Data object for Grid
        /// <summary>
        /// Add By Nishant Sheth
        /// Set the grid rows for entities
        /// </summary>
        public List<Plandataobj> GridDataRow(PlanGridColumnData Row, List<Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate, List<PlandataobjColumn> objCustomfieldData, Dictionary<int, string> usercolindex)
        {
            bool IsEditable = true;
            string cellTextColor = string.Empty;
            cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;
            List<PlandataobjColumn> lstOrderData = new List<PlandataobjColumn>();
            #region Set Default Columns Values
            lstOrderData.AddRange(Row.lstdata);
            #endregion
            #region Set Customfield Columns Values
            if (objCustomfieldData != null)
            {
                lstOrderData.AddRange(objCustomfieldData);
            }
            #endregion
            if (usercolindex != null && usercolindex.Count > 0)
            {
                lstOrderData = (from colsort in usercolindex
                                join lstData in lstOrderData
                                    on colsort.Value equals lstData.column
                                orderby colsort.Key
                                select lstData).ToList();
            }
            EntitydataobjCreateItem.AddRange(lstOrderData.Select(a => new Plandataobj { value = a.value, type = a.type, style = a.style, locked = a.locked, actval = a.actval }));
            return EntitydataobjCreateItem;
        }

        #endregion

        #region Check Row wise Permission

        private List<GridDefaultModel> GridRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, List<Int64> lsteditableEntityIds, int UserId)
        {
            #region set plan permission
            lstData = GridPlanRowPermission(lstData, objPermission, lstSubordinatesIds, UserId);
            #endregion

            #region set campaign permission
            lstData = GridCampaignRowPermission(lstData, objPermission, lstSubordinatesIds, lsteditableEntityIds, UserId);
            #endregion

            #region set program permission
            lstData = GridProgramRowPermission(lstData, objPermission, lstSubordinatesIds, lsteditableEntityIds, UserId);
            #endregion

            #region set tactic permission
            lstData = GridTacticRowPermission(lstData, objPermission, lstSubordinatesIds, lsteditableEntityIds, UserId);
            #endregion

            #region set line item permission
            lstData = GridLineItemRowPermission(lstData, objPermission, lstSubordinatesIds, UserId);
            #endregion

            return lstData;
        }

        /// <summary>
        /// Set grid plan row permission
        /// </summary>
        private List<GridDefaultModel> GridPlanRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, int UserId)
        {
            if (objPermission.PlanCreate == true)
            {
                // Update create plan permission 
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower()).ToList()
                    .ForEach(a => a.IsCreatePermission = true);
            }
            else
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower() &&
                    (((a.Owner.HasValue) && lstSubordinatesIds.Contains(a.Owner.Value)) || a.Owner == UserId))
                    .ToList().ForEach(a => a.IsCreatePermission = true);
            }
            // Update row permission for plan for created by

            if (lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower() && a.Owner == UserId).Any())
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower() && a.Owner == UserId).ToList().ForEach(a => a.IsRowPermission = true);
            }
            else if (objPermission.PlanEditAll == true)
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower()).ToList()
                    .ForEach(a => a.IsRowPermission = true);
            }
            else if (objPermission.PlanEditSubordinates == true)
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower() && ((a.Owner.HasValue) && lstSubordinatesIds.Contains(a.Owner.Value)))
                    .ToList().ForEach(a => a.IsRowPermission = true);
            }
            return lstData;
        }

        /// <summary>
        /// Set grid campaign row permission
        /// </summary>
        private List<GridDefaultModel> GridCampaignRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, List<Int64> lsteditableEntityIds, int UserId)
        {
            if (objPermission.PlanCreate == false)
            {
                // Update create campaign permission 
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Campaign.ToString().ToLower() &&
                    (a.Owner == UserId || ((a.Owner.HasValue) && lstSubordinatesIds.Contains(a.Owner.Value)))).ToList()
                    .ForEach(a => a.IsCreatePermission = true);
            }
            else
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Campaign.ToString().ToLower())
                    .ToList().ForEach(a => a.IsCreatePermission = true);
            }
            // Update campaign edit permission
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Campaign.ToString().ToLower() && a.Owner == UserId).ToList()
                .ForEach(a => a.IsRowPermission = true);

            // Set edit permission if it's child's tactic is editable
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Campaign.ToString().ToLower()).ToList().ForEach(a =>
            {
                var CampProgramList = lstData.Where(camp => camp.ParentUniqueId == a.UniqueId).Select(camp => camp.UniqueId).ToList(); // Get Campaign's Program List 
                var ProgramTacticList = lstData.Where(prg => CampProgramList.Contains(prg.ParentUniqueId)).Select(prg => prg.EntityId).ToList(); // Get Program's Tactic
                var AllowEntityIds = lsteditableEntityIds.Where(en => ProgramTacticList.Contains(en)).Count(); // Get list of tactic which have edit rights
                if (ProgramTacticList.Count > 0 && ProgramTacticList.Count == AllowEntityIds && lstSubordinatesIds.Contains(int.Parse(a.Owner.ToString())))
                {
                    a.IsRowPermission = true;
                }
            });
            return lstData;
        }

        /// <summary>
        /// Set grid program row permission
        /// </summary>
        private List<GridDefaultModel> GridProgramRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, List<Int64> lsteditableEntityIds, int UserId)
        {
            if (objPermission.PlanCreate == false)
            {
                // Update create program permission 
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Program.ToString().ToLower() &&
                    ((a.Owner.HasValue) && ((a.Owner.Value == UserId) || lstSubordinatesIds.Contains(a.Owner.Value)))).ToList()
                    .ForEach(a => a.IsCreatePermission = true);
            }
            else
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Program.ToString().ToLower())
                    .ToList().ForEach(a => a.IsCreatePermission = true);
            }
            //Update program edit permission
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Program.ToString().ToLower() && a.Owner == UserId).ToList()
               .ForEach(a => a.IsRowPermission = true);
            // Set edit permission if it's child's tactic is editable
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Program.ToString().ToLower()).ToList().ForEach(a =>
               {
                   var ProgramTacticList = lstData.Where(prg => prg.ParentUniqueId == a.UniqueId).Select(prg => prg.EntityId).ToList();// Get Program's Tactic
                   var AllowEntityIds = lsteditableEntityIds.Where(en => ProgramTacticList.Contains(en)).Count(); // Get list of tactic which have edit rights
                   if (ProgramTacticList.Count > 0 && ProgramTacticList.Count == AllowEntityIds && lstSubordinatesIds.Contains(int.Parse(a.Owner.ToString())))
                   {
                       a.IsRowPermission = true;
                   }
               });
            return lstData;
        }

        /// <summary>
        /// Set grid tactic row permission
        /// </summary>
        private List<GridDefaultModel> GridTacticRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, List<Int64> lsteditableEntityIds, int UserId)
        {
            if (objPermission.PlanCreate == false)
            {
                // Update create tactic permission 
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower() &&
                    ((a.Owner.HasValue) && ((a.Owner.Value == UserId) || lstSubordinatesIds.Contains(a.Owner.Value)))).ToList()
                    .ForEach(a => a.IsCreatePermission = true);
            }
            else
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower())
                    .ToList().ForEach(a => a.IsCreatePermission = true);
            }
            //Update tactic edit permission
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower() &&
                   ((a.Owner.HasValue) && ((a.Owner.Value == UserId) || lstSubordinatesIds.Contains(a.Owner.Value)))).ToList()
                   .ForEach(a => a.IsRowPermission = true);

            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower()
                && ((a.Owner.HasValue) && lstSubordinatesIds.Contains(a.Owner.Value))
                && ((a.EntityId.HasValue) && lsteditableEntityIds.Contains(a.EntityId.Value)))
                .ToList().ForEach(a => a.IsRowPermission = true);
            return lstData;
        }

        /// <summary>
        /// Set grid line item row permission
        /// </summary>
        private List<GridDefaultModel> GridLineItemRowPermission(List<GridDefaultModel> lstData, EntityPermission objPermission, List<int> lstSubordinatesIds, int UserId)
        {
            if (objPermission.PlanCreate == false)
            {
                // Update line item create permission 
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower() &&
                    ((a.Owner.HasValue) && ((a.Owner.Value == UserId) || lstSubordinatesIds.Contains(a.Owner.Value)))).ToList()
                    .ForEach(a => a.IsCreatePermission = true);
            }
            else
            {
                lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower())
                    .ToList().ForEach(a => a.IsCreatePermission = true);
            }
            //Update line item edit permission
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower() && a.Owner == UserId).ToList()
                   .ForEach(a => a.IsRowPermission = true);

            // Update line item edit permission if tactic is editable
            lstData.Where(a => a.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower())
                .ToList().ForEach(a =>
                {
                    var IsTacticEditable = lstData.Where(ab => ab.UniqueId == a.ParentUniqueId).Select(ab => ab.IsRowPermission).FirstOrDefault();
                    if (IsTacticEditable != null)
                    {
                        a.IsRowPermission = IsTacticEditable;
                    }
                });
            return lstData;
        }

        #endregion

        #region Method to convert number format
        /// <summary>
        /// Method for convert number to formatted string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormatNumber<T>(T number, int maxDecimals = 1)
        {
            return Regex.Replace(String.Format("{0:n" + maxDecimals + "}", number),
                                 @"[" + System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "]?0+$", "");
        }
        #endregion

        #region Grid user data
        /// <summary>
        /// Set the user data for entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Planuserdatagrid GridUserData(string EntityType, string UniqueId, List<GridDefaultModel> DataList)
        {
            Planuserdatagrid objUserData = new Planuserdatagrid();
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
                    var Row = DataList.Where(a => a.UniqueId == UniqueId)
                        .Select(a => new { a.ProjectedStage, a.TacticTypeId }).FirstOrDefault();
                    if (Row != null)
                    {
                        objUserData.stage = Row.ProjectedStage;
                        objUserData.tactictype = Convert.ToString(Row.TacticTypeId);
                    }
                }
            }
            else if (string.Compare(EntityType, Convert.ToString(Enums.EntityType.Lineitem), true) == 0)
            {
                if (!string.IsNullOrEmpty(UniqueId))
                {
                    var LineItemType = DataList.Where(a => a.UniqueId == UniqueId).Select(a => a.LineItemType).FirstOrDefault();
                    string IsOther = Convert.ToString(!string.IsNullOrEmpty(LineItemType) ? false : true);
                    objUserData.IsOther = IsOther;
                }
            }

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
            var ProgramDetail = DataList.Where(prg => prg.ParentUniqueId == UniqueId)
                                .Select(prg => new
                                {
                                    prg.UniqueId,
                                    prg.StartDate,
                                    prg.EndDate
                                });

            if (ProgramDetail.Count() > 0)
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
                                    });

                if (TacticDetail.Count() > 0)
                {
                    objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                    objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
                }
            }

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
            var TacticDetail = DataList.Where(tac => tac.ParentUniqueId == UniqueId)
                                .Select(tac => new
                                {
                                    tac.StartDate,
                                    tac.EndDate
                                });
            if (TacticDetail.Count() > 0)
            {
                objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
            }
            return objUserData;
        }

        #endregion


        #region Get Mql Title
        /// <summary>
        /// Get the client wise mql stage title 
        /// </summary>
        private string GetMqlTitle(int ClientId)
        {
            string MQLTitle = string.Empty;
            string MQLCode = Convert.ToString(Enums.PlanGoalType.MQL).ToLower();
            // Get MQL title label client wise
            MQLTitle = objDbMrpEntities.Stages.Where(stage => stage.Code.ToLower() == MQLCode && stage.IsDeleted == false && stage.ClientId == ClientId).Select(stage => stage.Title).FirstOrDefault();
            return MQLTitle;
        }
        #endregion

        #region Select Specific Columns dynamic
        // From this method we pass the array of column list and select it's values
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlanGridColumnData Projection(object RowData, IEnumerable<string> props, string viewBy)
        {
            PlanGridColumnData objres = new PlanGridColumnData();
            if (RowData == null)
            {
                return null;
            }
            PlandataobjColumn objPlanData = new PlandataobjColumn();
            List<PlandataobjColumn> lstPlanData = new List<PlandataobjColumn>();
            Type type = RowData.GetType();
            string IsEditable = string.Empty;
            string cellTextColor = string.Empty;

            // Set attribute values for add columns string as html and maintain hierarchy
            objres = InsertAttributeValueforAddColumns(RowData, objres);

            // Insert Hidden field values
            List<PlandataobjColumn> lstHiddenColData = InsertHiddenDefaultColumnsValues(RowData, objres);

            lstPlanData.AddRange(lstHiddenColData);

            // Set user selected columns values
            foreach (var pair in props.Select(n => new
            {
                Name = n,
                Property = type.GetProperty(n)
            }))
            {
                objPlanData = new PlandataobjColumn();
                if (pair.Property != null)
                {
                    objPlanData.column = pair.Name;
                    objres.EntityType = GetvalueFromObject(RowData, "EntityType");
                    if (objres.IsRowPermission == true)
                    {
                        IsEditable = objHomeGridProp.lockedstatezero;
                        cellTextColor = objHomeGridProp.stylecolorblack;
                    }
                    else
                    {
                        IsEditable = objHomeGridProp.lockedstateone;
                        cellTextColor = objHomeGridProp.stylecolorgray;
                    }

                    // Check Entity Type is line item and is other line item or not
                    if (string.Compare(objres.EntityType, Enums.EntityType.Lineitem.ToString(), true) == 0 && string.IsNullOrEmpty(objres.LineItemType))
                    {
                        IsEditable = objHomeGridProp.lockedstateone;
                        cellTextColor = objHomeGridProp.stylecolorgray;
                    }

                    if ((pair.Name == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.StartDate) || pair.Name == Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.EndDate)) &&
                            (objres.EntityType.ToUpper().ToString() == Enums.EntityType.Lineitem.ToString().ToUpper()) ||
                            ((viewBy.ToUpper() != PlanGanttTypes.Tactic.ToString().ToUpper()) && (objres.EntityType.ToUpper().ToString() == viewBy.ToUpper()))
                           )
                    {
                        objPlanData.value = "-";
                        objPlanData.actval = "-";
                        objPlanData.locked = objHomeGridProp.lockedstateone;
                        cellTextColor = objHomeGridProp.stylecolorgray;
                    }
                    else
                    {
                        objPlanData.value = GetvalueFromObject(RowData, pair.Name);
                        // set ids for line item type and tactic type
                        if (objres.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString()) // Consider as line item type
                            {
                                objPlanData.actval = GetvalueFromObject(RowData, "LineItemTypeId");
                            }
                            else
                            {
                                objPlanData.actval = GetvalueFromObject(RowData, pair.Name);
                            }
                        }
                        else if (objres.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString())
                            {
                                objPlanData.actval = GetvalueFromObject(RowData, "TacticTypeId");
                            }
                            else
                            {
                                objPlanData.actval = GetvalueFromObject(RowData, pair.Name);
                            }
                        }
                        else
                        {
                            objPlanData.actval = GetvalueFromObject(RowData, pair.Name);
                        }

                        if (objres.EntityType.ToLower() == Enums.EntityType.Lineitem.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString())
                            {
                                objPlanData.locked = !string.IsNullOrEmpty(objres.LineItemType) ? IsEditable : objHomeGridProp.lockedstateone;
                            }
                            else if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.StartDate.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.EndDate.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString()
                                || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Owner.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Status.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.AssetType.ToString())
                            {
                                objPlanData.locked = objHomeGridProp.lockedstateone;
                                cellTextColor = objHomeGridProp.stylecolorgray;
                            }
                            else
                            {
                                objPlanData.locked = IsEditable;
                            }
                        }
                        else if (objres.EntityType.ToLower() == Enums.EntityType.Tactic.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Status.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.AssetType.ToString())
                            {
                                objPlanData.locked = objHomeGridProp.lockedstateone;
                                cellTextColor = objHomeGridProp.stylecolorgray;
                            }
                            else
                            {
                                objPlanData.locked = IsEditable;
                            }
                        }
                        else if (objres.EntityType.ToLower() == Enums.EntityType.Program.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString()
                                || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Status.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.AssetType.ToString())
                            {
                                objPlanData.locked = objHomeGridProp.lockedstateone;
                                cellTextColor = objHomeGridProp.stylecolorgray;
                            }
                            else
                            {
                                objPlanData.locked = IsEditable;
                            }
                        }
                        else if (objres.EntityType.ToLower() == Enums.EntityType.Campaign.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString()
                                || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Status.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.AssetType.ToString())
                            {
                                objPlanData.locked = objHomeGridProp.lockedstateone;
                                cellTextColor = objHomeGridProp.stylecolorgray;
                            }
                            else
                            {
                                objPlanData.locked = IsEditable;
                            }
                        }
                        else if (objres.EntityType.ToLower() == Enums.EntityType.Plan.ToString().ToLower())
                        {
                            if (pair.Name == Enums.HomeGrid_Default_Hidden_Columns.PlannedCost.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TacticType.ToString()
                                || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.TargetStageGoal.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.StartDate.ToString()
                                || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.EndDate.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.Status.ToString() || pair.Name == Enums.HomeGrid_Default_Hidden_Columns.AssetType.ToString())
                            {
                                objPlanData.locked = objHomeGridProp.lockedstateone;
                                cellTextColor = objHomeGridProp.stylecolorgray;
                            }
                            else
                            {
                                objPlanData.locked = IsEditable;
                            }
                        }
                        if (pair.Name == Convert.ToString(Enums.IntegrationIdType.Eloquaid) || pair.Name == Convert.ToString(Enums.IntegrationIdType.WorkFrontid) || pair.Name == Convert.ToString(Enums.IntegrationIdType.Marketoid)
                            || pair.Name == Convert.ToString(Enums.IntegrationIdType.Salesforceid))
                        {
                            cellTextColor = objHomeGridProp.stylecolorgray;
                        }
                        objPlanData.style = cellTextColor;
                    }
                }
                lstPlanData.Add(objPlanData);
            }

            objres.lstdata = lstPlanData;
            return objres;
        }

        /// <summary>
        /// Return the hidden columns data for particular Entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<PlandataobjColumn> InsertHiddenDefaultColumnsValues(object RowData, PlanGridColumnData objres)
        {
            // Insert Hidden field values
            List<PlandataobjColumn> lstPlanData = new List<PlandataobjColumn>();
            lstHomeGrid_Hidden_And_Default_Columns().Select(col => col.Key).ToList().ForEach(coldata =>
            {
                if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ActivityType), true) == 0)
                {
                    lstPlanData.Add(new PlandataobjColumn
                    {
                        value = objres.EntityType, // Set Entity Type like Plan/Campaign etc...
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ActivityType)
                    });
                }
                else if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ColourCode), true) == 0)
                {
                    lstPlanData.Add(new PlandataobjColumn
                    {
                        style = "background-color:#" + objres.ColorCode, // Set colour Column
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.ColourCode)
                    });
                }
                else if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.id), true) == 0)
                {
                    lstPlanData.Add(new PlandataobjColumn
                    {
                        value = Convert.ToString(objres.EntityId), // Set column ids
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.id)
                    });
                }
                else if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName), true) == 0)
                {

                    lstPlanData.Add(new PlandataobjColumn
                    {
                        value = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType()
                                                                .GetProperty(Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName))
                                                                .GetValue(RowData, new object[0]))), // set machine name for tactics
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.MachineName)
                    });
                }
                else if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TaskName), true) == 0)
                {
                    string Roistring = string.Empty;
                    string Linkedstring = string.Empty;
                    string IsEditable = string.Empty;
                    string cellTextColor = string.Empty;

                    if (objres.IsRowPermission == true)
                    {
                        IsEditable = objHomeGridProp.lockedstatezero;
                        cellTextColor = objHomeGridProp.stylecolorblack;
                    }
                    else
                    {
                        IsEditable = objHomeGridProp.lockedstateone;
                        cellTextColor = objHomeGridProp.stylecolorgray;
                    }
                    // Check Entity Type is line item and is other line item or not
                    if (string.Compare(objres.EntityType, Enums.EntityType.Lineitem.ToString(), true) == 0 && string.IsNullOrEmpty(objres.LineItemType))
                    {
                        IsEditable = objHomeGridProp.lockedstateone;
                        cellTextColor = objHomeGridProp.stylecolorgray;
                    }

                    if (string.Compare(objres.EntityType, Convert.ToString(Enums.EntityType.Tactic), true) == 0)
                    {
                        // Get Anchor Tactic Id
                        string AnchorTacticId = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType().GetProperty("AnchorTacticID").GetValue(RowData, new object[0])));

                        // Get Tactic Id
                        string EntityId = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType().GetProperty("EntityId").GetValue(RowData, new object[0])));

                        if (!string.IsNullOrEmpty(AnchorTacticId) && !string.IsNullOrEmpty(EntityId))
                        {
                            if (string.Compare(AnchorTacticId, EntityId, true) == 0) // If Anchor tacticid and Entity id both same then set ROI package icon
                            {
                                // Get list of package tactic ids
                                string PackageTacticIds = Convert.ToString(HttpUtility.HtmlEncode(RowData.GetType().GetProperty("PackageTacticIds").GetValue(RowData, new object[0])));

                                Roistring = "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id=pkgIcon onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids='" + PackageTacticIds + "'><i class='fa fa-object-group'></i></div>";
                            }
                        }
                        DateTime TacticStartDate = Convert.ToDateTime(Convert.ToString(RowData.GetType().GetProperty("StartDate").GetValue(RowData, new object[0])));
                        DateTime TacticEndDate = Convert.ToDateTime(Convert.ToString(RowData.GetType().GetProperty("EndDate").GetValue(RowData, new object[0])));

                        bool IsExtendedTactic = (TacticEndDate.Year - TacticStartDate.Year) > 0 ? true : false;

                        int? LinkedTacticId = int.Parse(Convert.ToString(RowData.GetType().GetProperty("LinkedTacticId")
                                        .GetValue(RowData, new object[0])));
                        if (LinkedTacticId == 0)
                        {
                            LinkedTacticId = null;
                        }
                        string LinkedPlanName = Convert.ToString(RowData.GetType().GetProperty("LinkedPlanName")
                                                                .GetValue(RowData, new object[0]));
                        Linkedstring = ((IsExtendedTactic == true && LinkedTacticId == null) ?
                                                        "<div class='unlink-icon unlink-icon-grid'><i class='fa fa-chain-broken'></i></div>" :
                                                        ((IsExtendedTactic == true && LinkedTacticId != null) || (LinkedTacticId != null)) ? "<div class='unlink-icon unlink-icon-grid'  LinkedPlanName='"
                                                      + (string.IsNullOrEmpty(LinkedPlanName) ? null
                                                        : HttpUtility.HtmlEncode(LinkedPlanName).Replace("'", "&#39;"))
                                                       + "' id = 'LinkIcon' ><i class='fa fa-link'></i></div>" : "");
                        objres.IsExtendTactic = IsExtendedTactic;
                        objres.LinkedTacticId = LinkedTacticId;
                    }
                    lstPlanData.Add(new PlandataobjColumn
                    {
                        value = Roistring + Linkedstring + HttpUtility.HtmlEncode(objres.EntityTitle), //Set Entity title
                        locked = IsEditable,
                        style = cellTextColor,
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TaskName)
                    });
                }
                else if (string.Compare(coldata, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Add), true) == 0)
                {
                    lstPlanData.Add(new PlandataobjColumn
                    {
                        value = AddColumnString(objres), //Set Add icon html string for plan grid
                        column = Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Add)
                    });
                }
            });

            return lstPlanData;
        }

        /// <summary>
        /// Update reference variable values for Add columns icon's html attribute values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PlanGridColumnData InsertAttributeValueforAddColumns(object RowData, PlanGridColumnData objres)
        {
            Int64 ParentEntityId = 0;
            int LineItemTypeId = 0;
            int AnchorTacticID = 0;
            Int64 entityid = 0;
            int ownerId;
            Int64.TryParse(GetvalueFromObject(RowData, "EntityId"), out entityid);
            objres.EntityId = entityid;
            int.TryParse(GetvalueFromObject(RowData, "Owner"), out ownerId);
            objres.Owner = ownerId;
            objres.AltId = GetvalueFromObject(RowData, "AltId");
            objres.ColorCode = GetvalueFromObject(RowData, "ColorCode");
            objres.TaskId = GetvalueFromObject(RowData, "TaskId");
            objres.ParentTaskId = GetvalueFromObject(RowData, "ParentTaskId");
            objres.UniqueId = GetvalueFromObject(RowData, "UniqueId");
            objres.ParentUniqueId = GetvalueFromObject(RowData, "ParentUniqueId");
            objres.EntityType = GetvalueFromObject(RowData, "EntityType");
            objres.OwnerName = GetvalueFromObject(RowData, "OwnerName");

            Int64.TryParse(GetvalueFromObject(RowData, "ParentEntityId"), out ParentEntityId);
            objres.ParentEntityId = ParentEntityId;

            objres.AssetType = GetvalueFromObject(RowData, "AssetType");
            objres.TacticType = GetvalueFromObject(RowData, "TacticType");
            if (objres.EntityType.ToString().ToUpper() != Enums.EntityType.Lineitem.ToString().ToUpper())
            {
                objres.StartDate = Convert.ToDateTime(GetvalueFromObject(RowData, "StartDate"));
                objres.EndDate = Convert.ToDateTime(GetvalueFromObject(RowData, "EndDate"));
            }
            objres.Status = GetvalueFromObject(RowData, "Status");
            objres.EntityTitle = GetvalueFromObject(RowData, "EntityTitle");

            int.TryParse(GetvalueFromObject(RowData, "LineItemTypeId"), out LineItemTypeId);
            objres.LineItemTypeId = LineItemTypeId;
            objres.LineItemType = GetvalueFromObject(RowData, "LineItemType");

            int.TryParse(GetvalueFromObject(RowData, "AnchorTacticID"), out AnchorTacticID);
            objres.AnchorTacticID = AnchorTacticID;

            objres.IsCreatePermission = bool.Parse(GetvalueFromObject(RowData, "IsCreatePermission"));
            objres.IsRowPermission = bool.Parse(GetvalueFromObject(RowData, "IsRowPermission"));
            return objres;
        }

        /// <summary>
        /// return cell value for plan grid data
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetvalueFromObject(object RowData, string ColumnName)
        {
            string objVal = null;
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
                objVal = PlanCurrencySymbol + FormatNumber(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate), 2);
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
                objVal = FormatNumber(PlannedMQL, 1);
            }
            else if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.Revenue), true) == 0)
            {
                string Revenue = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                double PlannedRevenue = 0;
                double.TryParse(Convert.ToString(Revenue), out PlannedRevenue);
                objVal = PlanCurrencySymbol + FormatNumber(PlannedRevenue, 2);
            }
            else if (string.Compare(Convert.ToString(RowData.GetType().GetProperty("EntityType").GetValue(RowData, new object[0])).ToLower(), Enums.EntityType.Lineitem.ToString().ToLower()) == 0)
            {
                if (string.Compare(ColumnName, Convert.ToString(Enums.HomeGrid_Default_Hidden_Columns.TacticType), true) == 0)
                {
                    objVal = Convert.ToString(RowData.GetType().GetProperty("LineItemType").GetValue(RowData, new object[0]));
                }
                else
                {
                    objVal = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
                }
            }
            else
            {
                objVal = Convert.ToString(RowData.GetType().GetProperty(ColumnName).GetValue(RowData, new object[0]));
            }
            return objVal;
        }

        #region Add Column string
        /// <summary>
        ///  Return plan grid add column's icon html value
        /// </summary>
        private string AddColumnString(PlanGridColumnData Row)
        {
            string addColumn = string.Empty;
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

            return addColumn;
        }

        // Set the Plan add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PlanAddString(PlanGridColumnData Row)
        {
            /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
            string grid_add = string.Empty;
            if (Row.IsCreatePermission == true)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId +
                " per=" + Convert.ToString(Row.IsCreatePermission).ToLower() +
                " title=Add><i class='fa fa-plus-circle'></i></div>";
            }

            string addColumn = @" <div class=grid_Search id=Plan onclick=javascript:DisplayPopup(this) title='View'> <i Class='fa fa-external-link-square'> </i> </div>" +
                grid_add
                + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Select' id=Plan TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.Owner)
                + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode + "' altId=" + Row.TaskId
                + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>";
            return addColumn;
        }

        // Set the Campaign add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string CampaignAddString(PlanGridColumnData Row)
        {
            /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
            string grid_add = string.Empty;
            if (Row.IsCreatePermission == true)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.AltId +
                " per=" + Convert.ToString(Row.IsCreatePermission).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
            }

            string addColumn = @" <div class=grid_Search id=CP onclick=javascript:DisplayPopup(this) title='View'> <i Class='fa fa-external-link-square'> </i> </div>"
                + grid_add
                + "<div class=honeycombbox-icon-gantt id=Campaign onclick=javascript:AddRemoveEntity(this) title = 'Select'  TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '"
                + Convert.ToString(Row.Owner) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;"))
                + "' altId=" + Row.TaskId + " per=" + Convert.ToString(Row.IsCreatePermission).ToLower() + "' taskId= " + Row.EntityId + " csvId=Campaign_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the program add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ProgroamAddString(PlanGridColumnData Row)
        {
            /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
            string grid_add = string.Empty;
            if (Row.IsCreatePermission == true)
            {
                grid_add = "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Program alt=_" + Row.AltId +
                " per=" + Convert.ToString(Row.IsCreatePermission).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
            }
            string addColumn = @" <div class=grid_Search id=PP onclick=javascript:DisplayPopup(this) title='View'> <i Class='fa fa-external-link-square'> </i> </div>"
                + grid_add
                + " <div class=honeycombbox-icon-gantt id=Program onclick=javascript:AddRemoveEntity(this);  title = 'Select'  TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "' OwnerName= '" + Convert.ToString(Row.Owner)
                + "'  TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "'  altId= " + Row.TaskId +
                " per=" + Row.IsCreatePermission.ToString().ToLower() + "'  taskId= " + Row.EntityId + " csvId=Program_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the tactic add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string TacticAddString(PlanGridColumnData Row)
        {
            /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
            string grid_add = string.Empty;
            if (Row.IsCreatePermission == true)
            {
                grid_add = "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Tactic alt=__" + Row.ParentEntityId + "_" + Row.EntityId +
                " per=" + Row.IsCreatePermission.ToString().ToLower() + "  LinkTacticper ='" + Row.IsExtendTactic + "' LinkedTacticId = '" + 0
                + "' tacticaddId='" + Row.EntityId + "' title=Add><i class='fa fa-plus-circle'></i></div>";
            }
            string addColumn = @" <div class=grid_Search id=TP onclick=javascript:DisplayPopup(this) title='View'> <i Class='fa fa-external-link-square'> </i> </div>"
                + grid_add
                + " <div class=honeycombbox-icon-gantt id=Tactic onclick=javascript:AddRemoveEntity(this)  title = 'Select' anchortacticid='" + Row.AnchorTacticID + "' roitactictype='" + Row.AssetType
                + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode
                + "'  TacticType= '" + Row.TacticType + "' OwnerName= '" + Convert.ToString(Row.Owner) + "' altId=" + Row.TaskId
                + " per=" + Row.IsCreatePermission.ToString().ToLower() + "' taskId=" + Row.EntityId + " csvId=Tactic_" + Row.EntityId + "></div>";
            return addColumn;
        }

        // Set the line item add icon html string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string LineItemAddString(PlanGridColumnData Row)
        {
            /// TODO :: We need to Move HTML code in HTML HELPER As A part of code refactoring it's covered in #2676 PL ticket.
            string grid_add = string.Empty;
            if (!string.IsNullOrEmpty(Row.LineItemType))
            {
                if (Row.IsCreatePermission == true)
                {
                    int LineItemTypeId = 0;
                    if (Row.LineItemType != null)
                    {
                        int.TryParse(Convert.ToString(Row.LineItemTypeId), out LineItemTypeId);
                    }

                    grid_add = "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Line alt=___" + Row.ParentEntityId + "_" + Row.EntityId
                    + " lt=" + LineItemTypeId
                    + " dt=" + HttpUtility.HtmlEncode(Row.EntityTitle) + " per=" + Convert.ToString(Row.IsCreatePermission).ToLower() + " title=Add><i class='fa fa-plus-circle'></i></div>";
                }
                string addColumn = @" <div class=grid_Search id=LP onclick=javascript:DisplayPopup(this) title='View'> <i Class='fa fa-external-link-square'> </i> </div>"
                    + grid_add;
                return addColumn;
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
        #endregion

        private List<GridDefaultModel> FilterCustomField(List<GridDefaultModel> allData, string fltrCustomfields)
        {
            List<GridDefaultModel> resultData = new List<GridDefaultModel>();
            try
            {
                if (allData != null && allData.Count > 0)
                {
                    #region "Declare & Initialize local Variables"
                    List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                    List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                    string tacticType = Enums.EntityType.Tactic.ToString().ToUpper();
                    string[] filteredCustomFields = string.IsNullOrWhiteSpace(fltrCustomfields) ? null : fltrCustomfields.Split(',');
                    List<GridDefaultModel> tacData = allData.Where(tac => tac.EntityType != null && tac.EntityType.ToUpper() == tacticType).ToList();
                    List<int> lstTacticIds = tacData.Select(tactic => (int)tactic.EntityId).ToList();
                    #endregion

                    resultData = allData.Where(tac => tac.EntityType != null && tac.EntityType.ToUpper() != tacticType).ToList(); // Set Plan,Campaign,Program data to result dataset.
                    if (filteredCustomFields != null)
                    {
                        string[] splittedCustomField;
                        // Splitting filter Customfield values Ex. 71_104 to CustomFieldId: 71 & OptionId: 104
                        foreach (string customField in filteredCustomFields)
                        {
                            splittedCustomField = new string[2];
                            splittedCustomField = customField.Split('_');
                            lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                            lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                        };

                        lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds); // Filter Tactics list by selected Custofields in filter. 
                        tacData = tacData.Where(tactic => lstTacticIds.Contains((int)tactic.EntityId)).ToList();
                    }
                    //// get Allowed Entity Ids
                    List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, lstTacticIds, false);
                    tacData = tacData.Where(tactic => lstAllowedEntityIds.Contains((int)tactic.EntityId)).ToList();    //filter tactics with allowed entity.
                    resultData.AddRange(tacData);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return resultData;
        }




        #endregion

        #region "Calendar Related Functions"
        /// Createdy By: Viral
        /// Created On: 09/19/2016
        // Desc: Return List of Plan, Campaign, Program, Tactic
        public List<calendarDataModel> GetPlanCalendarData(string planIds, string ownerIds, string tactictypeIds, string statusIds, string customFieldIds, string timeframe, string planYear, string viewby)
        {
            #region "Declare Variables"
            SqlParameter[] para = new SqlParameter[7];
            List<calendarDataModel> calResultset = new List<calendarDataModel>();   // Return Calendar Result Data Model
            #endregion

            #region "Set SP Parameters"
            para[0] = new SqlParameter() { ParameterName = "planIds", Value = planIds };
            para[1] = new SqlParameter() { ParameterName = "ownerIds", Value = ownerIds };
            para[2] = new SqlParameter() { ParameterName = "tactictypeIds", Value = tactictypeIds };
            para[3] = new SqlParameter() { ParameterName = "statusIds", Value = statusIds };
            para[4] = new SqlParameter() { ParameterName = "timeframe", Value = timeframe };
            para[5] = new SqlParameter() { ParameterName = "planYear", Value = planYear };
            para[6] = new SqlParameter() { ParameterName = "viewBy", Value = viewby };
            #endregion

            #region "Get Data"
            calResultset = objDbMrpEntities.Database
                .SqlQuery<calendarDataModel>("spGetPlanCalendarData @planIds,@ownerIds,@tactictypeIds,@statusIds,@timeframe,@planYear,@viewBy", para)
                .ToList();
            #endregion

            #region "Filter data based on customfields selected under filter"
            if (calResultset != null && calResultset.Count > 0 && !string.IsNullOrEmpty(customFieldIds))
                calResultset = FilterCustomField(calResultset, customFieldIds); // Get filtered tactics based on customfield selection under Filter.
            #endregion


            #region "Filter data based on timeframe"
            if (calResultset != null && calResultset.Count > 0 && !string.IsNullOrEmpty(timeframe))
                calResultset = FilterPlanByTimeFrame(calResultset, timeframe); // Get filtered tactics based on timeframe.
            #endregion

            return calResultset;
        }

        /// Createdy By: Viral
        /// Created On: 09/19/2016
        // Desc: Set Owner Name and Permission of entity
        public List<calendarDataModel> SetOwnerNameAndPermission(List<calendarDataModel> lstCalendarDataModel)
        {


            #region "Get SubOrdinates"
            List<int> lstSubordinatesIds = new List<int>();
            bool IsTacticAllowForSubordinates = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditSubordinates);    // Check that user has subordinates permission or not. 
            bool IsPlanCreateAllAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanCreate);                  // Check that user has plan create permission or not.
            bool IsPlanEditAll = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.PlanEditAll);                            // Check that user has permission to edit plan permission or not.
            int userId = Sessions.User.ID;
            if (IsTacticAllowForSubordinates)
            {
                lstSubordinatesIds = Common.GetAllSubordinates(userId);   // Get all subordinates based on UserId.
            }
            #endregion

            #region "Set Permission"

            if (IsPlanCreateAllAuthorized)          // check whether user has plan create permission or not
            {
                lstCalendarDataModel.ForEach(data => data.Permission = true);
            }
            else
            {
                lstCalendarDataModel.Where(data =>
                                                (data.CreatedBy.HasValue) &&
                                                (data.CreatedBy.Value.Equals(userId) || lstSubordinatesIds.Contains(data.CreatedBy.Value))
                                          ).ToList()
                                    .ForEach(data => data.Permission = true);
            }

            #region "Set SubOrdinate Permission"

            #region "Plan Entities Permission"

            string strPlanEntity = Enums.EntityType.Plan.ToString().ToLower();

            if (lstCalendarDataModel.Where(a => a.CreatedBy.HasValue && a.CreatedBy.Value == userId).Any())  // check that user is owner or not
            {
                lstCalendarDataModel.Where(a => a.CreatedBy.HasValue && a.CreatedBy.Value == userId).ToList().ForEach(a => a.IsRowPermission = true);   // set Edit permission for all entities.
            }
            else if (IsPlanEditAll)                         // Check that user has permission to edit plan permission or not.
            {
                lstCalendarDataModel.Where(a => a.type != null && a.type.ToLower() == strPlanEntity).ToList().ForEach(a => a.IsRowPermission = true);
            }
            else if (IsTacticAllowForSubordinates)         // Check that user has subordinates permission or not. 
            {
                lstCalendarDataModel.Where(a => a.type != null && a.type.ToLower() == strPlanEntity &&
                                          ((a.CreatedBy.HasValue) && lstSubordinatesIds.Contains(a.CreatedBy.Value))
                                          ).ToList()
                                    .ForEach(a => a.IsRowPermission = true);
            }
            #endregion

            #region "Tactic Entities Permission"
            lstCalendarDataModel.Where(a => a.type != null && a.type.ToLower() == Enums.EntityType.Tactic.ToString().ToLower() &&
                                            a.CreatedBy.HasValue && ((a.CreatedBy.Value == userId) || lstSubordinatesIds.Contains(a.CreatedBy.Value))).ToList()
                                .ForEach(a => a.IsRowPermission = true);
            #endregion

            #endregion


            #endregion

            return lstCalendarDataModel;
        }

        /// <summary>
        /// Created by: Viral
        /// Created On: 09/19/2016
        /// Desc: Filter Calendar Model data based on custom field selected under filter screen. 
        /// </summary>
        /// <returns> Return List<calendarDataModel> dataset</returns>
        private List<calendarDataModel> FilterCustomField(List<calendarDataModel> allData, string fltrCustomfields)
        {
            List<calendarDataModel> resultData = new List<calendarDataModel>();
            if (allData != null && allData.Count > 0)
            {
                #region "Declare & Initialize local Variables"
                List<CustomFieldFilter> lstCustomFieldFilter = new List<CustomFieldFilter>();
                List<string> lstFilteredCustomFieldOptionIds = new List<string>();
                string tacticType = Enums.EntityType.Tactic.ToString().ToUpper();
                string[] filteredCustomFields = string.IsNullOrWhiteSpace(fltrCustomfields) ? null : fltrCustomfields.Split(',');
                List<calendarDataModel> tacData = allData.Where(tac => tac.type != null && tac.type.ToUpper() == tacticType).ToList();
                List<int> lstTacticIds = tacData.Select(tactic => tactic.PlanTacticId.Value).ToList();
                #endregion

                resultData = allData.Where(tac => tac.type != null && tac.type.ToUpper() != tacticType).ToList(); // Set Plan,Campaign,Program data to result dataset.
                if (filteredCustomFields != null)
                {
                    string[] splittedCustomField;
                    // Splitting filter Customfield values Ex. 71_104 to CustomFieldId: 71 & OptionId: 104
                    foreach (string customField in filteredCustomFields)
                    {
                        splittedCustomField = new string[2];
                        splittedCustomField = customField.Split('_');
                        lstCustomFieldFilter.Add(new CustomFieldFilter { CustomFieldId = int.Parse(splittedCustomField[0]), OptionId = splittedCustomField[1] });
                        lstFilteredCustomFieldOptionIds.Add(splittedCustomField[1]);
                    };

                    lstTacticIds = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, lstTacticIds); // Filter Tactics list by selected Custofields in filter. 
                    tacData = tacData.Where(tactic => lstTacticIds.Contains(tactic.PlanTacticId.Value)).ToList();
                }
                //// get Allowed Entity Ids
                List<int> lstAllowedEntityIds = Common.GetViewableTacticList(Sessions.User.ID, Sessions.User.CID, lstTacticIds, false);
                tacData = tacData.Where(tactic => lstAllowedEntityIds.Contains(tactic.PlanTacticId.Value)).ToList();    //filter tactics with allowed entity.
                resultData.AddRange(tacData);
            }
            return resultData;
        }

        /// <summary>
        /// Added by Viral Kadiya for PL ticket 2585
        /// </summary>
        /// <param name="calendarDataModel"></param>
        /// <param name="TimeFrame"></param>
        /// <returns></returns>
        private List<calendarDataModel> FilterPlanByTimeFrame(List<calendarDataModel> calendarDataModel, string TimeFrame)
        {

            bool isMultiYear = Common.IsMultiyearTimeframe(TimeFrame);  // Identify that Timeframe is multiyear or not.
            string entPlan = ActivityType.ActivityPlan.ToLower();
            foreach (calendarDataModel objPlan in calendarDataModel.Where(p => p.type != null && p.type.ToLower() == entPlan).ToList())
            {
                if (!calendarDataModel.Where(ent => ent.parent == objPlan.id).Any())
                {
                    int planId = Convert.ToInt32(objPlan.PlanId);//
                    bool isChildExist = objDbMrpEntities.Plan_Campaign.Where(p => p.PlanId == planId && p.IsDeleted == false).Any();
                    if (isChildExist)
                    {
                        calendarDataModel.Remove(objPlan);
                    }
                    else
                    {
                        string firstYear = Common.GetInitialYearFromTimeFrame(TimeFrame);
                        string lastYear = firstYear;

                        if (isMultiYear)
                        {
                            lastYear = Convert.ToString(Convert.ToInt32(firstYear) + 1);
                        }
                        if (objPlan.PYear.Value.ToString() != firstYear && objPlan.PYear.Value.ToString() != lastYear)
                        {
                            calendarDataModel.Remove(objPlan);
                        }
                    }
                }
            }
            return calendarDataModel;
        }

        #endregion

        /// <summary>
        /// This is owner list (client wise) used in plan grid, this list will be bind in grid header and will be available at the time of cell editing
        /// </summary>
        /// <returns></returns>
        private List<PlanOptions> GetOwnerListForHeader()
        {
            IBDSService objAuthService = new BDSServiceClient();
            List<User> lstUsers = objAuthService.GetUserListByClientIdEx(_ClientId);
            List<int> lstClientUsers = Common.GetClientUserListUsingCustomRestrictions(_ClientId, lstUsers.Where(i => i.IsDeleted == false).ToList());
            return lstUsers.Where(u => lstClientUsers.Contains(u.ID)).Select(owner => new PlanOptions
            {
                id = owner.ID,
                value = owner.FirstName + " " + owner.LastName
            }).ToList().OrderBy(tactype => tactype.value).ToList(); ;
        }
        /// <summary>
        /// This is tactic type list (client wise) used in plan grid
        /// </summary>
        /// <returns></returns>
        public List<PlanOptionsTacticType> GetTacticTypeListForHeader(string strPlanIds, int ClientId)
        {
            List<int> lstPlanIds = new List<int>();
            if (!string.IsNullOrEmpty(strPlanIds))
            {
                lstPlanIds = strPlanIds.Split(',').Select(int.Parse).ToList();
            }
            List<PlanOptionsTacticType> lstTacticTypes = (from tactictypes in objDbMrpEntities.TacticTypes
                                                          join model in objDbMrpEntities.Models on tactictypes.ModelId equals model.ModelId
                                                          join plan in objDbMrpEntities.Plans on model.ModelId equals plan.ModelId
                                                          where (tactictypes.IsDeleted == null || tactictypes.IsDeleted == false) && tactictypes.IsDeployedToModel
                                                          && model.ClientId == ClientId && model.IsDeleted == false && lstPlanIds.Contains(plan.PlanId)
                                                          select new PlanOptionsTacticType
                                                          {
                                                              PlanId = plan.PlanId,
                                                              id = tactictypes.TacticTypeId,
                                                              value = tactictypes.Title
                                                          }
                                 ).ToList();
            return lstTacticTypes;
        }
        /// <summary>
        /// This is Line Item type list (client wise) used in plan grid
        /// </summary>
        /// <returns></returns>
        public List<PlanOptionsTacticType> GetLineItemTypeListForHeader(string strPlanIds, int ClientId)
        {
            List<int> lstPlanIds = new List<int>();
            if (!string.IsNullOrEmpty(strPlanIds))
            {
                lstPlanIds = strPlanIds.Split(',').Select(int.Parse).ToList();
            }
            List<PlanOptionsTacticType> lstLineItemTypes = (from lineitemtypes in objDbMrpEntities.LineItemTypes
                                                            join model in objDbMrpEntities.Models on lineitemtypes.ModelId equals model.ModelId
                                                            join plan in objDbMrpEntities.Plans on model.ModelId equals plan.ModelId
                                                            where (lineitemtypes.IsDeleted == false) && model.ClientId == ClientId && model.IsDeleted == false && lstPlanIds.Contains(plan.PlanId)
                                                            select new PlanOptionsTacticType
                                                            {
                                                                PlanId = plan.PlanId,
                                                                id = lineitemtypes.LineItemTypeId,
                                                                value = lineitemtypes.Title
                                                            }
                                 ).ToList();
            return lstLineItemTypes;
        }
    }

    #region Pivot Custom fields list for each entities
    public static class PivotList
    {
        public static HomeGridProperties objHomeGrid = new HomeGridProperties();
        /// <summary>
        /// Add By Nishant Sheth
        /// Pivot list for custom field entities values
        /// </summary>
        public static List<CustomfieldPivotData> ToPivotList<T, TColumn, TRow, TData>(
        this IEnumerable<T> source,
        Func<T, TColumn> columnSelector,
        Expression<Func<T, TRow>> rowSelector,
        Func<IEnumerable<T>, TData> dataSelector,
         List<string> selectedColumns,
            List<GridCustomFields> CustomFields,
            List<EntityPermissionRowWise> EntityRowPermission
            )
        {

            List<CustomfieldPivotData> arr = new List<CustomfieldPivotData>();
            List<string> cols = new List<string>();
            String rowName = ((MemberExpression)rowSelector.Body).Member.Name;
            if (source != null)
            {
                IEnumerable<TColumn> columns = source.Select(columnSelector).Distinct()
                                        .ToList().Where(a => selectedColumns.Contains(a.ToString())).ToList()
                                        .OrderBy(a => selectedColumns.IndexOf(a.ToString()));


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
                    CustomfieldPivotData obj = GetAnonymousObject(cols, items, CustomFields, EntityRowPermission);
                    arr.Add(obj);
                }
            }
            return arr.ToList();
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// get values for pivoting entities
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static dynamic GetAnonymousObject(IEnumerable<string> columns, IEnumerable<object> values, List<GridCustomFields> CustomFields, List<EntityPermissionRowWise> EntityRowPermission)
        {
            CustomfieldPivotData objCustomPivotData = new CustomfieldPivotData();
            List<PlandataobjColumn> lstCustomFieldData = new List<PlandataobjColumn>();
            int i;
            string EntityType = string.Empty;
            string EntityId = string.Empty;
            string lockedval = string.Empty;
            string styleval = string.Empty;
            bool isRowPermission = false;
            if (values != null)
            {
                string[] EntityValues = Convert.ToString(values.ElementAt<object>(0)).Split('_');
                if (EntityValues != null && EntityValues.Count() > 0)
                {
                    EntityType = EntityValues[0];
                }
                if (EntityValues != null && EntityValues.Count() > 1)
                {
                    EntityId = EntityValues[1];
                }
                if (EntityRowPermission != null)
                {
                    isRowPermission = EntityRowPermission.Where(a => a.EntityType.ToLower() == EntityType.ToLower() && a.EntityId == EntityId)
                        .Select(a => a.IsRowPermission).FirstOrDefault();
                }
            }
            // Get list of custom fields by entity type
            List<string> EntityCustomFields = CustomFields.Where(a => a.EntityType.ToLower() == EntityType.ToLower()).Select(a => a.CustomFieldId.ToString()).ToList();
            // Get list of custom column indexes from custom field list
            IEnumerable<int> Colindexes = columns.Select((s, k) => new { Str = s, Index = k })
                                        .Where(x => EntityCustomFields.Contains(x.Str))
                                        .Select(x => x.Index);
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
                    if (isRowPermission)
                    {
                        if (Colindexes.Contains(i))
                        {
                            lockedval = objHomeGrid.lockedstatezero;
                            styleval = objHomeGrid.stylecolorblack;
                        }
                        else
                        {
                            lockedval = objHomeGrid.lockedstateone;
                            styleval = objHomeGrid.stylecolorgray;
                            DataValue = string.Empty;
                        }
                    }
                    else
                    {
                        lockedval = objHomeGrid.lockedstateone;
                        styleval = objHomeGrid.stylecolorgray;
                        DataValue = string.Empty;
                    }
                    lstCustomFieldData.Add(
                        new PlandataobjColumn
                        {
                            locked = lockedval,
                            value = DataValue,
                            style = styleval,
                            column = CustomFields.Where(a => columns.ElementAt(i) == a.CustomFieldId.ToString()).Select(a => a.CustomUniqueId).FirstOrDefault()
                        });
                }
            }
            objCustomPivotData.CustomFieldData = lstCustomFieldData;
            return objCustomPivotData;
        }

    }
    #endregion

}