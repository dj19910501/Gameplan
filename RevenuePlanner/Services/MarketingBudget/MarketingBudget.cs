using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;
using RevenuePlanner.BDSService;
using System.Runtime.CompilerServices;


namespace RevenuePlanner.Services.MarketingBudget
{
    public class MarketingBudget : IMarketingBudget
    {
        private MRPEntities _database;
        string TripleDash = "---";
        private const string formatThousand = "#,#0.##";
        private IBDSService _ServiceDatabase;
        public MarketingBudget(MRPEntities database, IBDSService ServiceDatabase)
        {
            _database = database;
            _ServiceDatabase = ServiceDatabase;
        }

        public List<BindDropdownData> GetBudgetlist(int ClientId)
        {
            //get budget name list for budget drop-down data binding.
            List<BindDropdownData> ddlBudget = new List<BindDropdownData>();
            List<Models.Budget> lstBudget = _database.Budgets.Where(bdgt => bdgt.ClientId == ClientId && (bdgt.IsDeleted == false || bdgt.IsDeleted == null) && !string.IsNullOrEmpty(bdgt.Name)).ToList();
            ddlBudget = lstBudget.Select(budget => new BindDropdownData { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return ddlBudget;
        }

        public List<BindDropdownData> GetColumnSet(int ClientId)
        {
            List<BindDropdownData> lstColumnset = (from ColumnSet in _database.Budget_ColumnSet
                                                   join Columns in _database.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                                                   where ColumnSet.ClientId == ClientId && ColumnSet.IsDeleted == false
                                                   && Columns.IsDeleted == false
                                                   select new
                                                   {
                                                       ColumnSet.Name,
                                                       ColumnSet.Id,
                                                       ColId = Columns.Id
                                                   }).ToList().GroupBy(g => new { Id = g.Id, Name = g.Name })
                                              .Select(a => new { a.Key.Id, a.Key.Name, Count = a.Count() })
                                              .Where(a => a.Count > 0)
                                              .Select(a => new BindDropdownData { Text = a.Name, Value = Convert.ToString(a.Id) }).ToList();
            return lstColumnset;
        }

        public List<BindDropdownData> GetColumns(int ColumnSetId)
        {
            List<BindDropdownData> lstColumns = _database.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false)
               .Select(a => new { a.CustomField.Name, a.CustomField.CustomFieldId }).ToList()
               .Select(a => new BindDropdownData { Text = a.Name, Value = Convert.ToString(a.CustomFieldId) }).ToList();
            return lstColumns;
        }



        public BudgetGridModel GetBudgetGridData(int budgetId, string viewByType, BudgetColumnFlag columnsRequested, int ClientID, int UserID, double Exchangerate, string CurSymbol)
        {
            List<BDSService.User> lstUser = _ServiceDatabase.GetUserListByClientIdEx(Sessions.User.CID).ToList();

            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            BudgetGridDataModel objBudgetGridDataModel = new BudgetGridDataModel();
            List<int> lstUserId = lstUser.Select(a => a.ID).ToList();
            string CommaSeparatedUserIds = String.Join(",", lstUserId);
            List<string> CustomColumnNames = new List<string>();
            //Call Sp to get data.
            DataSet BudgetGridData = GetBudgetDefaultData(budgetId, viewByType, columnsRequested, ClientID, UserID, CommaSeparatedUserIds, Exchangerate);

            if (BudgetGridData.Tables.Count > 1)
            {
                DataTable CustomColumnsTable = BudgetGridData.Tables[1];
                CustomColumnNames = CustomColumnsTable.Columns.Cast<DataColumn>() //list to get custom column names
                  .Select(x => x.ToString())
                  .ToList();
            }
            DataTable StandardColumnTable = BudgetGridData.Tables[0];
            //list to get standard column names
            List<string> StandardColumnNames = StandardColumnTable.Columns.Cast<DataColumn>().Where(name => name.ToString().ToLower() != Enums.DefaultGridColumn.Permission.ToString().ToLower() && name.ToString().ToLower() != Enums.DefaultGridColumn.ParentId.ToString().ToLower())
                  .Select(x => x.ToString())
                  .ToList();

            //Set Header Object.
            SetHeaderObject(CustomColumnNames, StandardColumnNames, viewByType, objBudgetGridModel);

            //Call recursive function to bind the hierarchy.
            List<BudgetGridRowModel> lstData = new List<BudgetGridRowModel>();
            lstData = GetTopLevelRows(BudgetGridData, null)
                        .Select(row => CreateHierarchyItem(BudgetGridData, row, CustomColumnNames, StandardColumnNames, lstUser, CurSymbol)).ToList();

            objBudgetGridDataModel.head = objBudgetGridModel.GridDataStyleList;
            objBudgetGridDataModel.rows = lstData;
            objBudgetGridModel.objGridDataModel = objBudgetGridDataModel;
            return objBudgetGridModel;
        }
        IEnumerable<DataRow> GetTopLevelRows(DataSet DataSet, int? minParentId)
        {
            return DataSet.Tables[0]
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("ParentId") == minParentId);
        }

        private BudgetGridRowModel CreateHierarchyItem(DataSet DataSet, DataRow row, List<string> CustomColumnNames, List<string> StandardColumnNames, List<BDSService.User> lstUser, string CurSymbol)
        {
            string istitleedit = "1";
            string stylecolorgray = string.Empty; // if no permission set style to gray
            int id = Convert.ToInt32(row[Enums.DefaultGridColumn.BudgetDetailId.ToString()]);
            string Permission = row[Enums.DefaultGridColumn.Permission.ToString()].ToString(); // variable to check permission
            string rowId = Regex.Replace((row[Enums.DefaultGridColumn.Name.ToString()].ToString() == null ? "" : row[Enums.DefaultGridColumn.Name.ToString().Trim().Replace("_", "")]).ToString(), @"[^0-9a-zA-Z]+", "") + "_" + id + "_" + (row[Enums.DefaultGridColumn.ParentId.ToString()].ToString() == "" ? "0" : row[Enums.DefaultGridColumn.ParentId.ToString()]); //row id for each budget item
            List<string> Data = new List<string>(); // list to bind all each row data.
            UserData objuserData = new UserData();
            List<string> BindColumnDataatend = new List<string>(); //list will bind data of user owner and lineitems as we have to bind these data at the end.

            #region Bind Standard Columns
            foreach (var ColumnName in StandardColumnNames)
            {
                if (ColumnName == Enums.DefaultGridColumn.Name.ToString())
                {
                    Data.Add(row[ColumnName.ToString()].ToString() == null ? string.Empty : row[ColumnName.ToString()].ToString());

                    //Add icons data after name
                    if (Permission == "None" || Permission == "View")
                    {
                        Data.Add(string.Empty);
                        istitleedit = "0";
                    }
                    else
                    {
                        Data.Add("<div id='dv" + rowId + "' row-id='" + rowId + "' onclick='AddRow(this)' class='finance_grid_add' title='Add New Row'></div><div id='cb" + rowId + "' row-id='" + rowId + "' name='" + row[Enums.DefaultGridColumn.Name.ToString()].ToString() + "' LICount='" + row[Enums.DefaultGridColumn.LineItems.ToString()].ToString() + "' onclick='DeleteBudgetIconClick(this)' title='Delete' title='Delete' class='grid_Delete'></div>");
                    }

                }
                else if (ColumnName == Enums.DefaultGridColumn.LineItems.ToString())
                {
                    BindColumnDataatend.Add(row[ColumnName.ToString()].ToString());
                }
                else if (ColumnName == Enums.DefaultGridColumn.User.ToString())
                {
                    if (Permission == "View" || Permission == "None")
                    {
                        BindColumnDataatend.Add(string.Format("<div onclick=Edit({0},false,{1},'" + rowId + "',this) class='finance_link' Rowid='" + rowId + "'><a>" + Convert.ToInt32(row[ColumnName]) + "</a><span style='border-left:1px solid #000;height:20px'></span><span><span style='text-decoration: underline;'>View</div>", id, HttpUtility.HtmlEncode(Convert.ToString("'User'"))));
                    }
                    else
                    {
                        BindColumnDataatend.Add(string.Format("<div onclick=Edit({0},false,{1},'" + rowId + "',this) class='finance_link' Rowid='" + rowId + "'><a>" + Convert.ToInt32(row[ColumnName]) + "</a><span style='border-left:1px solid #000;height:20px'></span><span><span style='text-decoration: underline;'>Edit</div>", id, HttpUtility.HtmlEncode(Convert.ToString("'User'"))));
                    }
                }
                else if (ColumnName == Enums.DefaultGridColumn.Owner.ToString())
                {
                    BindColumnDataatend.Add(lstUser.Where(a => a.ID == Convert.ToInt32(row[ColumnName])).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault());
                }
                else
                {
                    if (Permission == "None")
                    {
                        Data.Add(TripleDash); // if none permission show tripe dash.
                    }
                    else
                    {
                        
                        object objValue = row[Convert.ToString(ColumnName)];
                        string DisplayValue = string.Empty;
                        if (objValue != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(objValue)))
                        {
                            double value;
                            double.TryParse(Convert.ToString(objValue), out value);
                            DisplayValue = string.Format("{0}{1}", CurSymbol, value.ToString(formatThousand));
                        }
                        Data.Add(DisplayValue);
                    }

                }

            }

            #endregion

            #region Bind Custom Columns
            //Get Custom Column row
            IEnumerable<DataRow> CustomColumnRow = null;
            if (CustomColumnNames.Count > 0)
            {
                CustomColumnRow = GetCustomColumnRow(DataSet, id);
            }
            if (Permission == "None")
            {
                Data.AddRange(CustomColumnNames.Skip(1).Select(item => TripleDash).ToList());
            }
            else if (CustomColumnRow != null && CustomColumnRow.Count() > 0)
            {
                //Add custom row values to the list.
                var Listofvalues = CustomColumnRow.Select(dr => dr.ItemArray.Skip(1).Select(x => x.ToString())
                .ToArray()).FirstOrDefault();


                Data.AddRange(Listofvalues);
            }
            else
            {
                Data.AddRange(CustomColumnNames.Skip(1).Select(item => string.Empty).ToList());
            }
            #endregion

            Data.AddRange(BindColumnDataatend);

            ////call recursively untill there are no childs
            List<BudgetGridRowModel> children = new List<BudgetGridRowModel>();
            IEnumerable<DataRow> lstChildren = null;
            lstChildren = GetChildren(DataSet, id);

            #region Handle Permissions in grid
            if (Permission == "None")
            {
                stylecolorgray = "color:#999";
                objuserData.lo = "1"; // checks if the cell is locked or not
                objuserData.isTitleEdit = istitleedit;//checks if title is editable or not
                objuserData.per = Permission;
            }
            else
            {
                int rwcount = DataSet.Tables[0] != null ? DataSet.Tables[0].Rows.Count : 0;
                if (rwcount == 1 || lstChildren.Count() == 0)
                {
                    objuserData.lo = Permission == "View" ? "1" : "0";
                    objuserData.isTitleEdit = istitleedit;
                    objuserData.per = Permission;
                }
                else
                {
                    objuserData.lo = "1";
                    objuserData.isTitleEdit = istitleedit;
                    objuserData.per = Permission;
                }
            }

            #endregion

            children = lstChildren
                  .Select(r => CreateHierarchyItem(DataSet, r, CustomColumnNames, StandardColumnNames, lstUser, CurSymbol))
                  .ToList();

            return new BudgetGridRowModel { id = rowId, data = Data, rows = children, style = stylecolorgray, Detailid = Convert.ToString(id), userdata = objuserData };
        }

        IEnumerable<DataRow> GetCustomColumnRow(DataSet DataSet, int? BudgetDetailId)
        {
            return DataSet.Tables[1]
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("BudgetDetailId") == BudgetDetailId);
        }

        IEnumerable<DataRow> GetChildren(DataSet DataSet, int? parentId)
        {
            return DataSet.Tables[0]
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("ParentId") == parentId);
        }

        public DataSet GetBudgetDefaultData(int budgetId, string timeframe, BudgetColumnFlag columnsRequested, int ClientID, int UserID, string CommaSeparatedUserIds, double Exchangerate)
        {
            DataSet EntityList = new DataSet();
            try
            {
                ///If connection is closed then it will be open
                var Connection = _database.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                {
                    Connection.Open();
                }
                SqlCommand command = new SqlCommand("MV.GetFinanceGridData", Connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BudgetId", budgetId);
                command.Parameters.AddWithValue("@ClientId", ClientID);
                command.Parameters.AddWithValue("@timeframe", timeframe);
                command.Parameters.AddWithValue("@lstUserIds", CommaSeparatedUserIds);
                command.Parameters.AddWithValue("@UserId", UserID);
                command.Parameters.AddWithValue("@CurrencyRate", Exchangerate);

                SqlDataAdapter adp = new SqlDataAdapter(command);
                adp.Fill(EntityList);
                if (Connection.State == System.Data.ConnectionState.Open)
                {
                    Connection.Close();
                }
            }
            catch { throw; }
            return EntityList;
        }


        BudgetGridModel SetHeaderObject(List<string> CustomColumnNames, List<string> StandardColumnNames, string timeframe, BudgetGridModel mdlGridHeader)
        {
            List<GridDataStyle> ListHead = new List<GridDataStyle>(); //list to bind all header data
            StringBuilder sbAttachedHeaders = new StringBuilder(); //used to get comma separated values to attach 2 headers
            List<GridDataStyle> ListAppendAtLast = new List<GridDataStyle>();//list will have data oof user,owner,line items as we need to attach them at the end as per UI
            GridDataStyle headObj = new GridDataStyle();

            #region Bind Standard Columns
            foreach (var columns in StandardColumnNames)
            {
                if (columns == Enums.DefaultGridColumn.BudgetDetailId.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = columns;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append(columns + ",");
                    }

                    headObj.sort = "na";
                    headObj.width = 0;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = columns;
                    ListHead.Add(headObj);

                }
                else if (columns == Enums.DefaultGridColumn.Name.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = "Task Name";
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append("Task Name" + ",");
                        sbAttachedHeaders.Append(string.Empty + ",");
                    }
                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "tree";
                    headObj.id = columns;
                    ListHead.Add(headObj);


                    //Add icons column after name
                    headObj = new GridDataStyle();
                    headObj.value = string.Empty;
                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = "Add Row";
                    ListHead.Add(headObj);

                }
                else if (columns == Enums.DefaultGridColumn.User.ToString() ||
                       columns == Enums.DefaultGridColumn.Owner.ToString())
                {
                    headObj = new GridDataStyle();

                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = columns;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                    }
                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = columns;
                    ListAppendAtLast.Add(headObj);
                }
                else if (columns == Enums.DefaultGridColumn.LineItems.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = "Line Items";
                    }
                    else
                    {
                        headObj.value = string.Empty;
                    }
                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = columns;
                    ListAppendAtLast.Add(headObj);
                }
                else
                {
                    headObj = new GridDataStyle();

                    headObj.id = columns;
                    headObj.sort = "na";
                    if (columns.Contains(Enums.DefaultGridColumn.Forecast.ToString()))
                    {
                        headObj.width = 0;
                    }
                    else
                    {
                        headObj.width = 100;
                    }

                    headObj.align = "center";

                    // columns will have data like eg.Y1_Budget in case of time frame like This year(Monthly)
                    // so we will split them and attach the values to string builder to get double headers.
                    if (columns.Contains('_') && columns.Split('_').Length > 0)
                    {
                        if (timeframe != Enums.QuarterFinance.quarters.ToString() && !columns.Contains("Total"))
                        {
                            headObj.value = Enums.monthList.FirstOrDefault(x => x.Value == columns.Split('_')[0]).Key; //so this will have Jan for Y1
                        }
                        else
                        {
                            headObj.value = columns.Split('_')[0];
                        }
                        sbAttachedHeaders.Append(columns.Split('_')[1] + ","); // this will be budget as per example
                    }
                    else
                    {
                        headObj.value = columns;
                        sbAttachedHeaders.Append(columns + ",");
                    }

                    if (columns.Contains(Enums.DefaultGridColumn.Planned.ToString()) ||
                       columns.Contains(Enums.DefaultGridColumn.Actual.ToString()))
                    {
                        headObj.type = "ro";
                    }
                    else
                    {
                        headObj.type = "ed";
                    }

                    ListHead.Add(headObj);
                }

            }
            #endregion
            #region Bind Header for custom columns
            foreach (var columns in CustomColumnNames)
            {
                if (columns != Enums.DefaultGridColumn.BudgetDetailId.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = columns;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append(columns + ",");
                    }
                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "ed";
                    headObj.id = columns;
                    ListHead.Add(headObj);
                }
            }
            #endregion

            ListHead.AddRange(ListAppendAtLast);
            if (timeframe != Enums.QuarterFinance.Yearly.ToString())
            {
                //attach double header in case of time frame options other than default view
                sbAttachedHeaders.Append(String.Join(",", ListAppendAtLast.Select(name => name.id).ToList()));
                mdlGridHeader.attachedHeader = sbAttachedHeaders.ToString();
            }
            mdlGridHeader.GridDataStyleList = ListHead;
            return mdlGridHeader;
        }

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

        public List<LineItemAllocatingAccount> GetAccountsForLineItem(int lineItemId)
        {
            throw new NotImplementedException();
        }

        public List<PlanAllocatingAccount> GetAccountsForPlan(int planId)
        {
            throw new NotImplementedException();
        }

        public List<AllocatedLineItemForAccount> GetAllocatedLineItemsForAccount(int accountId)
        {
            throw new NotImplementedException();
        }

        public List<BudgetItem> GetBudgetData(int budgetId, ViewByType viewByType, BudgetColumnFlag columnsRequested)
        {
            throw new NotImplementedException();
        }

        public BudgetSummary GetBudgetSummary(int budgetId)
        {
            throw new NotImplementedException();
        }

        public List<UserBudgetPermission> GetUserPermissionsForAccount(int accountId)
        {
            throw new NotImplementedException();
        }

        public void LinkLineItemsToAccounts(List<LineItemAccountAssociation> lineItemAccountAssociations)
        {
            throw new NotImplementedException();
        }

        public void LinkPlansToAccounts(List<PlanAccountAssociation> planAccountAssociations)
        {
            throw new NotImplementedException();
        }

        public Dictionary<BudgetCloumn, double> UpdateBudgetCell(int budgetId, BudgetCloumn columnIndex, double oldValue, double newValue)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Function to Call sp. DeleteMarketingBudget for deleting budget data and its child heirarchy.
        /// Added By: Rahul Shah on 11/30/2016.
        /// </summary>
        /// <param name="selectedBudgetId">Budget Detail Id.</param>
        /// <param name="ClientId">Client Id.</param>        
        /// <returns>Return Budget Id.</returns>
        public int DeleteBudget(int selectedBudgetId, int ClientId)
        {
            int NextBudgetId = 0; // 
            try
            {
                ///If connection is closed then it will be open
                var Connection = _database.Database.Connection as SqlConnection;
                if (Connection.State == System.Data.ConnectionState.Closed)
                {
                    Connection.Open();
                }
                SqlCommand command = new SqlCommand("DeleteMarketingBudget", Connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BudgetDetailId", selectedBudgetId);
                command.Parameters.AddWithValue("@ClientId", ClientId);

                SqlParameter returnParameter = command.Parameters.Add("NewParentId", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
                NextBudgetId = (int)returnParameter.Value;
                
                if (Connection.State == System.Data.ConnectionState.Open)
                {
                    Connection.Close();
                }
            }
            catch { throw; }
            return NextBudgetId; //return new budgetid if user delete root/Parent budget.
        }
    }
}