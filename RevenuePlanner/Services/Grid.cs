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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace RevenuePlanner.Services
{
    public class Grid : IGrid
    {
        private MRPEntities objDbMrpEntities;
        List<Plandataobj> plandataobjlistCreateItem = new List<Plandataobj>();
        Dictionary<string, Plandataobj> EntitydataobjCreateItem = new Dictionary<string, Plandataobj>();
        public RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
        HomeGridProperties objHomeGridProp = new HomeGridProperties();

        public Grid()
        {
            objDbMrpEntities = new MRPEntities();
        }

        #region Method to get grid default data
        public List<GridDefaultModel> GetGridDefaultData(string PlanIds, Guid ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds)
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
        public GridCustomColumnData GetGridCustomFieldData(string PlanIds, Guid ClientId)
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
                paraClientId.DbType = DbType.Guid;
                paraClientId.Direction = ParameterDirection.Input;
                paraClientId.Value = ClientId;

                cmd.Parameters.Add(paraPlanId);
                cmd.Parameters.Add(paraClientId);
                // Run the sproc  
                var reader = cmd.ExecuteReader();

                // Read CustomField from the first result set 
                var CustomFields = ((IObjectContextAdapter)objDbMrpEntities)
                    .ObjectContext
                    .Translate<GridCustomFields>(reader).ToList();

                // Move to second result set and read Posts 
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
        public PlanMainDHTMLXGrid GetPlanGrid(string PlanIds, Guid ClientId, string ownerIds, string TacticTypeid, string StatusIds, string customFieldIds, string PlanCurrencySymbol, double PlanExchangeRate)
        {

            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();

            var GridHireachyData = GetGridDefaultData(PlanIds, ClientId, ownerIds, TacticTypeid, StatusIds, customFieldIds);

            var ModelId = GridHireachyData.Select(a => a.ModelId).Distinct().ToList();

            //List<TacticTypeModel> TacticTypeList = objDbMrpEntities.TacticTypes.Where(tactype => (tactype.IsDeleted == null || tactype.IsDeleted == false) &&
            //                                                                           tactype.IsDeployedToModel &&
            //                                                                           ModelId.Contains(tactype.ModelId)).
            //                                          Select(tacttype => new TacticTypeModel
            //                                          {
            //                                              TacticTypeId = tacttype.TacticTypeId,
            //                                              Title = tacttype.Title,
            //                                              AssetType = tacttype.AssetType
            //                                          }).ToList().OrderBy(tactype => tactype.Title).ToList();

            List<Stage> stageList = objDbMrpEntities.Stages.Where(stage => stage.ClientId == ClientId && stage.IsDeleted == false)
                .Select(stage => stage).ToList();
            string MQLTitle = stageList.Where(stage => stage.Code.ToLower() == Enums.PlanGoalType.MQL.ToString().ToLower()).Select(stage => stage.Title).FirstOrDefault();

            var ListOfDefaultColumnHeader = GenerateJsonHeader(MQLTitle);

            var ListOfCustomData = GridCustomFieldData(PlanIds, ClientId);
            GridCustomHead(ListOfCustomData.CustomFields, ref ListOfDefaultColumnHeader);


            RoundupRevnueMqlforHireachyData(ref GridHireachyData);

            var griditems = GetTopLevelRowsGrid(GridHireachyData, null)
                     .Select(row => CreateItemGrid(GridHireachyData, row, ListOfCustomData, PlanCurrencySymbol, PlanExchangeRate))
                     .ToList();

            objPlanMainDHTMLXGrid.head = ListOfDefaultColumnHeader;
            objPlanMainDHTMLXGrid.rows = griditems;
            return objPlanMainDHTMLXGrid;
        }

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

        public GridCustomColumnData GridCustomFieldData(string PlanIds, Guid ClientId)
        {
            GridCustomColumnData data = new GridCustomColumnData();
            try
            {
                data = GetGridCustomFieldData(PlanIds, ClientId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return data;
        }

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

        #region method to generate grid header
        public List<PlanHead> GenerateJsonHeader(string MQLTitle, bool IsNotLineItemListing = true)
        {
            // Modified by Arpita Soni for Ticket #2237 on 06/09/2016
            List<PlanHead> headobjlist = new List<PlanHead>();
            PlanHead headobj = new PlanHead();
            List<PlanOptions> lstOwner = new List<PlanOptions>();
            List<PlanOptions> lstTacticType = new List<PlanOptions>();
            try
            {

                // First Column Activity Type
                headobj.type = "ro";
                //headobj.align = "center";
                headobj.id = "activitytype";
                headobj.sort = "na";
                headobj.width = 0;
                headobj.value = "Activity Type";
                headobjlist.Add(headobj);

                if (!IsNotLineItemListing)
                {
                    // First Column Activity Type
                    headobj.type = "sub_row_grid";
                    //headobj.align = "center";
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

                // Modified by Arpita Soni to resolve issue in Ticket #2237 due to #2270/#2271
                if (IsNotLineItemListing)
                {
                    // Third Column : Empty
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.align = "center";
                    headobj.id = "add";
                    headobj.sort = "na";
                    headobj.width = 85; //modified by Rahul shah on 04/12/2015-to increase width for honeycomb feature
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
                    headobj.width = 45; // decreased width of column in case of line item grid
                    headobj.value = "";
                    headobjlist.Add(headobj);
                }
                // Fourth Column : Id
                headobj = new PlanHead();
                headobj.type = "ro";
                //headobj.align = "center";
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

                // Added by Arpita Soni for Ticket #2354 on 07/12/2016
                if (IsNotLineItemListing)
                {
                    headobj = new PlanHead();
                    headobj.type = "ro";
                    headobj.align = "center";
                    headobj.id = "roitactictype";
                    headobj.sort = "str";
                    headobj.width = 150;
                    headobj.value = "Tactic Category"; //Modified By Komal for #2448 on 01-08-2016
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
                    //headobj.align = "left";
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
        public void RoundupRevnueMqlforHireachyData(ref List<GridDefaultModel> DataList)
        {

            var ListofParentIds = DataList
                .Select(a => new
                {
                    a.ParentUniqueId,
                    a.EntityType
                }).Distinct()
            .Reverse()
            .ToList();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PlanDHTMLXGridDataModel CreateItemGrid(List<GridDefaultModel> DataList, GridDefaultModel Row, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            //var id = Row.UniqueId;

            Planuserdatagrid objUserData = new Planuserdatagrid();
            List<PlanDHTMLXGridDataModel> children = new List<PlanDHTMLXGridDataModel>();
            try
            {
                List<row_attrs> rows_attrData = new List<row_attrs>();

                var lstChildren = GetChildrenGrid(DataList, Row.UniqueId);

                children = lstChildren
                .Select(r => CreateItemGrid(DataList, r, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate)).ToList();
                EntitydataobjCreateItem = new Dictionary<string, Plandataobj>();
                EntitydataobjCreateItem = GridDataRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return new PlanDHTMLXGridDataModel { id = Row.UniqueId, data = EntitydataobjCreateItem.Select(a => a.Value).ToList(), rows = children, bgColor = GridBackgroundColor(Row.EntityType), open = GridEntityOpenState(Row.EntityType), userdata = GridUserData(DataList, Row.EntityType, Row.UniqueId, Row) };
        }
        #endregion

        #region Create Data object for Grid
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridDataRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            try
            {
                if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Plan).ToLower())
                {
                    return GridPlanRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Campaign).ToLower())
                {
                    return GridCampaignRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Program).ToLower())
                {
                    return GridProgramRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Tactic).ToLower())
                {
                    return GridTacticRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);
                }
                else if (Row.EntityType.ToLower() == Convert.ToString(Enums.EntityType.Lineitem).ToLower())
                {
                    return GridLineItemRow(Row, ref EntitydataobjCreateItem, CustomFieldData, PlanCurrencySymbol, PlanExchangeRate);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return EntitydataobjCreateItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridPlanRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
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
                EntitydataobjCreateItem.Add("EntityType", new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add("EntityTitle", new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add("AddRow", new Plandataobj
                {
                    value = "<div class=grid_Search id=Plan title=View ></div>" + (IsPlanEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsPlanEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add("EntityId", new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add("StartDate", new Plandataobj
                {
                    value = Convert.ToString(Row.StartDate),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add("EndDate", new Plandataobj
                {
                    value = Convert.ToString(Row.EndDate),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add("PlanCost", new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add("TacticCategory", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add("TacticType", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add("CreatedBy", new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add("TargetStage", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add("Mql", new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add("Revenue", new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add("ExternalName", new Plandataobj
                {
                    value = string.Empty,
                    locked = IsPlanEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                foreach (var Customfield in CustomFieldData.CustomFields)
                {
                    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                    {
                        value = string.Empty,
                        locked = objHomeGridProp.lockedstateone,
                        style = cellTextColor
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridCampaignRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;
                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add("EntityType", new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add("EntityTitle", new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add("AddRow", new Plandataobj
                {
                    //value = "<div class=grid_Search id=CP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add> </div>" : "") + "<div class=honeycombbox-icon-gantt id=CampaignAdd onclick=javascript:AddRemoveEntity(this) title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' altId=" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "' taskId= " + Row.EntityId + " csvId=" + Row.EntityType + "_" + Row.EntityId + "></div>"
                    value = "<div class=grid_Search id=CP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event) id=Campaign alt=" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + " title=Add> </div>" : "") + "<div class=honeycombbox-icon-gantt id=CampaignAdd onclick=javascript:AddRemoveEntity(this) title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "'  OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' altId=" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + "' taskId= " + Row.EntityId + " csvId=Campaign_" + Row.EntityId + "></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add("EntityId", new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add("StartDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add("EndDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add("PlanCost", new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add("TacticCategory", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add("TacticType", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add("CreatedBy", new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add("TargetStage", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add("Mql", new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add("Revenue", new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add("ExternalName", new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                foreach (var Customfield in CustomFieldData.CustomFields)
                {
                    string EntityValue = string.Empty;
                    var CustomValue = CustomFieldData.CustomFieldValues.Where(cust =>
                                            Row.EntityId == cust.EntityId
                                            && Row.EntityType.ToLower() == Customfield.EntityType.ToLower()
                                            && Customfield.CustomFieldId == cust.CustomFieldId)
                                            .Select(val => val.Value).ToList();
                    if (CustomValue != null)
                    {
                        if (CustomValue.Count > 0)
                        {
                            EntityValue = string.Join(",", CustomValue);
                        }
                    }

                    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                    {
                        value = EntityValue,
                        locked = objHomeGridProp.lockedstateone,
                        style = cellTextColor
                    });
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridProgramRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add("EntityType", new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add("EntityTitle", new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add("AddRow", new Plandataobj
                {
                    //value = "<div class=grid_Search id=Plan title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>"
                    value = "<div class=grid_Search id=PP title=View></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Program alt=_" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div>" : "") + " <div class=honeycombbox-icon-gantt id=ProgramAdd onclick=javascript:AddRemoveEntity(this);  title = 'Add to Honeycomb' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "' TacticType= '" + objHomeGridProp.doubledesh + "' ColorCode='" + Row.ColorCode + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "'  TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "'  altId=_" + Row.AltId + " per=" + IsEditable.ToString().ToLower() + "'  taskId= " + Row.EntityId + " csvId=Program_" + Row.EntityId + "></div>"
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add("EntityId", new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add("StartDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add("EndDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add("PlanCost", new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add("TacticCategory", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add("TacticType", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add("CreatedBy", new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add("TargetStage", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    type = objHomeGridProp.typero,
                    style = cellTextColor
                });

                // Add Mql
                EntitydataobjCreateItem.Add("Mql", new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add("Revenue", new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add("ExternalName", new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                foreach (var Customfield in CustomFieldData.CustomFields)
                {
                    string EntityValue = string.Empty;
                    var CustomValue = CustomFieldData.CustomFieldValues.Where(cust =>
                                            Row.EntityId == cust.EntityId
                                            && Row.EntityType.ToLower() == Customfield.EntityType.ToLower()
                                            && Customfield.CustomFieldId == cust.CustomFieldId)
                                            .Select(val => val.Value).ToList();
                    if (CustomValue != null)
                    {
                        if (CustomValue.Count > 0)
                        {
                            EntityValue = string.Join(",", CustomValue);
                        }
                    }

                    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                    {
                        value = EntityValue,
                        locked = objHomeGridProp.lockedstateone,
                        style = cellTextColor
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridTacticRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add("EntityType", new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add("EntityTitle", new Plandataobj
                {
                    value = ((Row.AnchorTacticID == Row.EntityId) ?
                              "<div class='package-icon package-icon-grid' style='cursor:pointer' title='Package' id=pkgIcon onclick='OpenHoneyComb(this);event.cancelBubble=true;' pkgtacids=" + Convert.ToString(Row.PackageTacticIds) + "><i class='fa fa-object-group'></i></div>" : "")
                              + HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add("AddRow", new Plandataobj
                {
                    //value = "<div class=grid_Search id=Plan title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=Plan alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>",
                    value = "<div class=grid_Search id=TP title=View></div>" + (IsEditable ? "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Tactic alt=__" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "  LinkTacticper ='" + false + "' LinkedTacticId = '" + 0 + "' tacticaddId='" + Row.EntityId + "' title=Add></div>" : "") + " <div class=honeycombbox-icon-gantt id=TacticAdd onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb'  pcptid = " + Row.TaskId + " anchortacticid='" + 0 + "' dhtmlxrowid='" + Row.EntityType + "_" + Row.EntityId + "'  roitactictype='" + Row.AssetType + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + Row.ColorCode + "'  TacticType= '" + Row.TacticType + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' altId=__" + Row.ParentEntityId + "_" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + "' taskId=" + Row.EntityId + " csvId=Tactic_" + Row.EntityId + "></div>",
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add("EntityId", new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add("StartDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.StartDate).ToString("MM/dd/yyyy"),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add("EndDate", new Plandataobj
                {
                    value = Convert.ToDateTime(Row.EndDate).ToString("MM/dd/yyyy"),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add("PlanCost", new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    type = objHomeGridProp.typeEdn,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add("TacticCategory", new Plandataobj
                {
                    value = Row.AssetType,
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add("TacticType", new Plandataobj
                {
                    value = Convert.ToString(Row.TacticTypeId),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add("CreatedBy", new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add("TargetStage", new Plandataobj
                {
                    value = (Math.Round(Convert.ToDouble(Row.ProjectedStageValue)) > 0 ? Math.Round(Convert.ToDouble(Row.ProjectedStageValue)).ToString("#,#") : "0") + " " + Row.ProjectedStage,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor,
                    type = objHomeGridProp.typeEdn
                });

                // Add Mql
                EntitydataobjCreateItem.Add("Mql", new Plandataobj
                {
                    value = Convert.ToString(ConvertNumberToRoundFormate(Convert.ToDouble(Row.MQL))),
                    actval = Convert.ToString(Convert.ToDouble(Row.MQL)),
                    style = cellTextColor
                });

                // Add Revenue
                EntitydataobjCreateItem.Add("Revenue", new Plandataobj
                {
                    value = PlanCurrencySymbol + ConvertNumberToRoundFormate(Convert.ToDouble(Row.Revenue)),
                    actval = Convert.ToString(Convert.ToDouble(Row.Revenue)),
                    style = cellTextColor
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add("ExternalName", new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.MachineName),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                foreach (var Customfield in CustomFieldData.CustomFields)
                {
                    string EntityValue = string.Empty;
                    var CustomValue = CustomFieldData.CustomFieldValues.Where(cust =>
                                            Row.EntityId == cust.EntityId
                                            && Row.EntityType.ToLower() == Customfield.EntityType.ToLower()
                                            && Customfield.CustomFieldId == cust.CustomFieldId)
                                            .Select(val => val.Value).ToList();
                    if (CustomValue != null)
                    {
                        if (CustomValue.Count > 0)
                        {
                            EntityValue = string.Join(",", CustomValue);
                        }
                    }

                    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                    {
                        value = EntityValue,
                        locked = objHomeGridProp.lockedstateone,
                        style = cellTextColor
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return EntitydataobjCreateItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, Plandataobj> GridLineItemRow(GridDefaultModel Row, ref Dictionary<string, Plandataobj> EntitydataobjCreateItem, GridCustomColumnData CustomFieldData, string PlanCurrencySymbol, double PlanExchangeRate)
        {
            try
            {
                bool IsEditable = true;
                string cellTextColor = string.Empty;
                cellTextColor = IsEditable ? objHomeGridProp.stylecolorblack : objHomeGridProp.stylecolorgray;

                #region Set Default Columns Values
                // Add Plan Entity Type
                EntitydataobjCreateItem.Add("EntityType", new Plandataobj
                {
                    value = Row.EntityType
                });

                // Add Plan Entity Title
                EntitydataobjCreateItem.Add("EntityTitle", new Plandataobj
                {
                    value = HttpUtility.HtmlEncode(Row.EntityTitle),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Add Row
                EntitydataobjCreateItem.Add("AddRow", new Plandataobj
                {
                    //value = "<div class=grid_Search id=" + Row.EntityType + " title=View ></div>" + (IsEditable ? "<div class=grid_add onclick=javascript:DisplayPopUpMenu(this,event)  id=" + Row.EntityType + " alt=" + Row.EntityId + " per=" + IsEditable.ToString().ToLower() + " title=Add></div> " : "") + "<div class=honeycombbox-icon-gantt onclick=javascript:AddRemoveEntity(this)  title = 'Add to Honeycomb' id=PlanAdd dhtmlxrowid='" + Row.EntityId + "' TacticType= '" + "--" + "' OwnerName= '" + Convert.ToString(Row.CreatedBy) + "' TaskName='" + (HttpUtility.HtmlEncode(Row.EntityTitle).Replace("'", "&#39;")) + "' ColorCode='" + "" + "' altId=" + Row.EntityId + " per=" + "true" + "' taskId=" + Row.EntityId + " csvId=Plan_" + Row.EntityId + " ></div>",
                    value = "<div class=grid_Search id=LP title=View></div>" + (IsEditable ? "<div class=grid_add  onclick=javascript:DisplayPopUpMenu(this,event)  id=Line alt=___" + Row.ParentEntityId + "_" + Row.EntityId + " lt=" + ((Row.LineItemType == null) ? 0 : Row.LineItemTypeId) + " dt=" + HttpUtility.HtmlEncode(Row.EntityTitle) + " per=" + IsEditable.ToString().ToLower() + " title=Add></div>" : ""),
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Plan Entity Id
                EntitydataobjCreateItem.Add("EntityId", new Plandataobj
                {
                    value = Convert.ToString(Row.EntityId)
                });

                // Add Plan StartDate
                EntitydataobjCreateItem.Add("StartDate", new Plandataobj
                {
                    locked = objHomeGridProp.lockedstateone
                });

                // Add Plan EndDate
                EntitydataobjCreateItem.Add("EndDate", new Plandataobj
                {
                    locked = objHomeGridProp.lockedstateone
                });

                // Add Plan Total Cost
                double PlannedCost = 0;
                double.TryParse(Convert.ToString(Row.PlannedCost), out PlannedCost);
                string Cost = Convert.ToString(objCurrency.GetValueByExchangeRate(PlannedCost, PlanExchangeRate));
                EntitydataobjCreateItem.Add("PlanCost", new Plandataobj
                {
                    value = Cost,
                    actval = Cost,
                    locked = ((Row.LineItemTypeId == null || Convert.ToString(Row.LineItemTypeId) == string.Empty) ? objHomeGridProp.lockedstateone : objHomeGridProp.lockedstatezero),
                    type = objHomeGridProp.typeEdn,
                    style = cellTextColor
                });

                // Add Tactic Category
                EntitydataobjCreateItem.Add("TacticCategory", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Tactic Type
                EntitydataobjCreateItem.Add("TacticType", new Plandataobj
                {
                    value = !string.IsNullOrEmpty(Row.LineItemType) ? Convert.ToString(Row.LineItemType) : string.Empty,
                    locked = objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });

                // Add Owner
                EntitydataobjCreateItem.Add("CreatedBy", new Plandataobj
                {
                    value = Convert.ToString(Row.CreatedBy),
                    style = cellTextColor,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone
                });

                // Add Target Stage Goal
                EntitydataobjCreateItem.Add("TargetStage", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Mql
                EntitydataobjCreateItem.Add("Mql", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                // Add Revenue
                EntitydataobjCreateItem.Add("Revenue", new Plandataobj
                {
                    value = objHomeGridProp.doubledesh,
                    style = cellTextColor,
                    type = objHomeGridProp.typero
                });

                //Add External Name Column as a last column of gridview
                EntitydataobjCreateItem.Add("ExternalName", new Plandataobj
                {
                    value = string.Empty,
                    locked = IsEditable ? objHomeGridProp.lockedstatezero : objHomeGridProp.lockedstateone,
                    style = cellTextColor
                });
                #endregion

                #region Set Customfield Columns Values
                foreach (var Customfield in CustomFieldData.CustomFields)
                {
                    string EntityValue = string.Empty;
                    var CustomValue = CustomFieldData.CustomFieldValues.Where(cust =>
                                            Row.EntityId == cust.EntityId
                                            && Row.EntityType.ToLower() == Customfield.EntityType.ToLower()
                                            && Customfield.CustomFieldId == cust.CustomFieldId)
                                            .Select(val => val.Value).ToList();
                    if (CustomValue != null)
                    {
                        if (CustomValue.Count > 0)
                        {
                            EntityValue = string.Join(",", CustomValue);
                        }
                    }

                    EntitydataobjCreateItem.Add("custom_" + Customfield.CustomFieldId, new Plandataobj
                    {
                        value = EntityValue,
                        locked = objHomeGridProp.lockedstateone,
                        style = cellTextColor
                    });
                }
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
        /// Added By devanshi gandhi/ Bhavesh Dobariya to hadle format at server side and avoide at client side - Change made to improve performance of grid view
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
                                    });

                var TacticDetail = (from objPrg in ProgramDetail
                                    join objData in DataList
                                        on objPrg.UniqueId equals objData.ParentUniqueId
                                    select new
                                    {
                                        objData.StartDate,
                                        objData.EndDate
                                    }).ToList();

                objUserData.psdate = ProgramDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                objUserData.pedate = ProgramDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");

                objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }

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
                                    });

                objUserData.tsdate = TacticDetail.Min(a => a.StartDate).Value.ToString("MM/dd/yyyy");
                objUserData.tedate = TacticDetail.Max(a => a.EndDate).Value.ToString("MM/dd/yyyy");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objUserData;
        }

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
    }
}