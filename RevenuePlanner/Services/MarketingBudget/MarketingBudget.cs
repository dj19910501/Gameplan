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
using System.Xml;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;



namespace RevenuePlanner.Services.MarketingBudget
{
    public class MarketingBudget : IMarketingBudget
    {
        private MRPEntities _database;
        string TripleDash = "---";
        private const string formatThousand = "#,#0.##";
        private const string PeriodPrefix = "Y";
        private IBDSService _ServiceDatabase;
        private ICurrency _ObjCurrency = new Currency();

        List<string> QuaterPeriod = new List<string>();
        public string dataPeriod = "";
        public MarketingBudget(MRPEntities database, IBDSService ServiceDatabase)
        {
            _database = database;
            _ServiceDatabase = ServiceDatabase;
        }

        /// <summary>
        /// Function to Get Budget List
        /// Added By: Rahul Shah on 11/30/2016.
        /// </summary>        
        /// <param name="ClientId">Client Id.</param>        
        /// <returns>Return Budget List.</returns>
        public List<BindDropdownData> GetBudgetlist(int ClientId)
        {
            //get budget name list for budget drop-down data binding.
            List<BindDropdownData> ddlBudget = new List<BindDropdownData>();
            List<Models.Budget> lstBudget = _database.Budgets.Where(bdgt => bdgt.ClientId == ClientId
            && (bdgt.IsDeleted == false || bdgt.IsDeleted == null)
            && !string.IsNullOrEmpty(bdgt.Name)).ToList();
            ddlBudget = lstBudget.Select(budget => new BindDropdownData { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return ddlBudget;
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Returns list of column set for a particular client
        /// </summary>
        /// <param name="ClientId">Id of the Client</param>
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

        /// <summary>
        /// Added by Komal Rawal
        /// Returns list of columns based on column set id to bind the column management dropdown
        /// </summary>
        /// <param name="ColumnSetId">Id of the ColumnSet</param>
        public List<Budget_Columns> GetColumns(int ColumnSetId)
        {
            List<Budget_Columns> BudgetColumns = _database.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false).ToList();
            return BudgetColumns;
        }


        /// <summary>
        /// Added by Komal Rawal
        /// Returns model with the hierarchy that need to be displayed.
        /// </summary>
        /// <param name="budgetId">Id of the Budget</param>
        /// <param name="TimeFrame">TimeFrame Selected</param>
        /// <param name="ClientID">Id of the Client</param>
        /// <param name="UserID">Id of the User</param>
        /// <param name="Exchangerate">Exachange rate of the currency</param>
        /// <param name="CurSymbol">preferred currency by the user</param>
        public BudgetGridModel GetBudgetGridData(int budgetId, string TimeFrame, int ClientID, int UserID, double Exchangerate, string CurSymbol, List<BDSService.User> lstUser)
        {

            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            BudgetGridDataModel objBudgetGridDataModel = new BudgetGridDataModel(); //model to bind rows and header
            List<int> lstUserId = lstUser.Select(a => a.ID).ToList();
            string CommaSeparatedUserIds = String.Join(",", lstUserId);  //comma separated ids
            List<string> CustomColumnNames = new List<string>();
            List<CustomFieldOption> lstCustomFieldOptions = new List<CustomFieldOption>();
            //Call Sp to get data.
            DataSet BudgetGridData = GetBudgetDefaultData(budgetId, TimeFrame, ClientID, UserID, CommaSeparatedUserIds, Exchangerate);
            if (BudgetGridData != null) // checks if data set returned is not null
            {
                if (BudgetGridData.Tables.Count > 2) // checks if custom column table exists
                {
                    DataTable CustomColumnsTable = BudgetGridData.Tables[1]; // get list of custom fields
                    CustomColumnNames = CustomColumnsTable.Columns.Cast<DataColumn>() //list to get custom column names
                      .Select(x => x.ToString())
                      .ToList();

                    // Get all custom field options list
                    lstCustomFieldOptions = (from cfo in BudgetGridData.Tables[2].AsEnumerable()
                                             select new CustomFieldOption
                                             {
                                                 CustomFieldId = cfo.Field<int>("CustomFieldId"),
                                                 CustomFieldOptionId = cfo.Field<int>("CustomFieldOptionId"),
                                                 Value = cfo.Field<string>("Value")
                                             }).ToList();
                }
                DataTable StandardColumnTable = BudgetGridData.Tables[0];
                //list to get standard column names
                List<string> StandardColumnNames = StandardColumnTable.Columns.Cast<DataColumn>().Where(name => name.ToString().ToLower() != Enums.DefaultGridColumn.Permission.ToString().ToLower()
                && name.ToString().ToLower() != Enums.DefaultGridColumn.ParentId.ToString().ToLower())
                  .Select(x => x.ToString())
                  .ToList();

                //Set Header Object.
                SetHeaderObject(CustomColumnNames, StandardColumnNames, TimeFrame, objBudgetGridModel, lstCustomFieldOptions);
                List<string> NonePermissionIds = new List<string>(); //Get rowid of non permisson rows to show three dash in rollup columns 
                NonePermissionIds = BudgetGridData.Tables[0].AsEnumerable().Where(p => p.Field<string>(Enums.DefaultGridColumn.Permission.ToString()).ToLower() == "none").Select(p => Regex.Replace((p[Enums.DefaultGridColumn.Name.ToString()].ToString() == null ? "" : p[Enums.DefaultGridColumn.Name.ToString().Trim().Replace("_", "")]).ToString(), @"[^0-9a-zA-Z]+", "") + "_" + Convert.ToInt32(p[Enums.DefaultGridColumn.BudgetDetailId.ToString()]) + "_" + (p[Enums.DefaultGridColumn.ParentId.ToString()].ToString() == "" ? "0" : p[Enums.DefaultGridColumn.ParentId.ToString()])).ToList();
                //Call recursive function to bind the hierarchy.
                List<BudgetGridRowModel> lstData = new List<BudgetGridRowModel>();
                lstData = GetTopLevelRows(StandardColumnTable)
                        .Select(row => CreateHierarchyItem(BudgetGridData, row, CustomColumnNames, StandardColumnNames, lstUser, CurSymbol,TimeFrame)).ToList();


                objBudgetGridDataModel.head = objBudgetGridModel.GridDataStyleList;
                objBudgetGridDataModel.rows = lstData;
                objBudgetGridModel.nonePermissonIDs = NonePermissionIds;
                objBudgetGridModel.objGridDataModel = objBudgetGridDataModel;

            }

            return objBudgetGridModel;
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Get the top level row i.e the parent row.
        /// </summary>
        /// <param name="StandardColumnTable">Standard table to get the row data</param>
        /// <param name="minParentId">Parent id </param>
        private IEnumerable<DataRow> GetTopLevelRows(DataTable StandardColumnTable)
        {
            return StandardColumnTable
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("ParentId") == null);
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Returns row model to bind the budget grid
        /// </summary>
        /// <param name="DataSet">Set of tables containg custom field data and other standard data</param>
        /// <param name="row">row wise data</param>
        /// <param name="CustomColumnNames">List of names of custom column</param>
        /// <param name="StandardColumnNames">List of names of standard column</param>
        /// <param name="lstUser">List of users</param>
        /// <param name="CurSymbol">Preferred currency symbol by user</param>
        private BudgetGridRowModel CreateHierarchyItem(DataSet DataSet, DataRow row, List<string> CustomColumnNames, List<string> StandardColumnNames, List<BDSService.User> lstUser, string CurSymbol,string TimeFrame)
        {
            string istitleedit = "1";
            string stylecolorgray = string.Empty; // if no permission set style to gray
            int id = Convert.ToInt32(row[Enums.DefaultGridColumn.BudgetDetailId.ToString()]);
            string Permission = row[Enums.DefaultGridColumn.Permission.ToString()].ToString(); // variable to check permission
            string rowId = Regex.Replace((row[Enums.DefaultGridColumn.Name.ToString()].ToString() == null ? "" : row[Enums.DefaultGridColumn.Name.ToString().Trim().Replace("_", "")]).ToString(), @"[^0-9a-zA-Z]+", "") + "_" + id + "_" + (row[Enums.DefaultGridColumn.ParentId.ToString()].ToString() == "" ? "0" : row[Enums.DefaultGridColumn.ParentId.ToString()]); //row id for each budget item
            List<string> Data = new List<string>(); // list to bind all each row data.
            UserData objuserData = new UserData();
            List<string> BindColumnDataatend = new List<string>(); //list will bind data of user owner and lineitems as we have to bind these data at the end.
            ////call recursively untill there are no childs
            List<BudgetGridRowModel> children = new List<BudgetGridRowModel>();
            IEnumerable<DataRow> lstChildren = null;
            lstChildren = GetChildren(DataSet, id);

            #region Bind Standard Columns
            foreach (var ColumnName in StandardColumnNames)
            {
                if (ColumnName == Enums.DefaultGridColumn.Name.ToString())
                {
                    Data.Add(row[ColumnName].ToString() == null ? string.Empty : row[ColumnName].ToString());

                    //Add icons data after name
                    if (Permission == Enums.Permission.None.ToString() || Permission == Enums.Permission.View.ToString())
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
                    if (Permission == "None" || Permission == "View" )
                    {
                        BindColumnDataatend.Add(row[ColumnName.ToString()].ToString());
                    }
                    else
                    {
                        if (lstChildren.Count() == 0 && row[Enums.DefaultGridColumn.ParentId.ToString()].ToString() != "")
                        {
                            string strLineItemLink = string.Format("<div onclick='LoadLineItemGrid({0})' class='finance_lineItemlink'>{1}</div>", id, row[ColumnName.ToString()].ToString());
                            BindColumnDataatend.Add(strLineItemLink);
                        }
                        else
                            BindColumnDataatend.Add(row[ColumnName.ToString()].ToString());
                    }

                }
                else if (ColumnName == Enums.DefaultGridColumn.Users.ToString())
                {
                    if (Permission == Enums.Permission.View.ToString() || Permission == Enums.Permission.None.ToString())
                    {
                        BindColumnDataatend.Add(string.Format("<div onclick=Edit({0},false,{1},'" + rowId + "',this) class='finance_link' Rowid='" + rowId + "'><a class='marketing-tbl-link'>" + Convert.ToInt32(row[ColumnName]) + "</a><span class='pipeLine'></span><span class='marketing-tbl-link'>View</span></div>", id, HttpUtility.HtmlEncode(Convert.ToString("'User'"))));
                    }
                    else
                    {
                        BindColumnDataatend.Add(string.Format("<div onclick=Edit({0},false,{1},'" + rowId + "',this) class='finance_link' Rowid='" + rowId + "'><a class='marketing-tbl-link'>" + Convert.ToInt32(row[ColumnName]) + "</a><span class='pipeLine'></span><span class='marketing-tbl-link'>Edit</span></div>", id, HttpUtility.HtmlEncode(Convert.ToString("'User'"))));
                    }
                }
                else if (ColumnName == Enums.DefaultGridColumn.Owner.ToString())
                {
                    BindColumnDataatend.Add(lstUser.Where(a => a.ID == Convert.ToInt32(row[ColumnName])).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault());
                }
                else
                {
                    if (Permission == Enums.Permission.None.ToString() && ColumnName != Enums.DefaultGridColumn.BudgetDetailId.ToString())
                    {
                        Data.Add(string.Empty); // if none permission add empty value so that we can make the values roll up.
                    }
                    else
                    {

                        object objValue = row[Convert.ToString(ColumnName)];
                        string DisplayValue = string.Empty;
                        if (objValue != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(objValue)))
                        {
                            DisplayValue = Convert.ToString(objValue);
                        }
                        Data.Add(DisplayValue);
                    }

                }

            }

            #endregion

            if (TimeFrame != Enums.QuarterFinance.Yearly.ToString())
            {
                #region Add Blank Values to Unallocated Columns
                Data.Add(string.Empty); //unallocated budget
                Data.Add(string.Empty); //unallocated forecast
                #endregion
            }

            #region Bind Custom Columns
            //Get Custom Column row
            IEnumerable<DataRow> CustomColumnRow = null;
            if (CustomColumnNames.Count > 0)
            {
                CustomColumnRow = GetCustomColumnRow(DataSet, id);
            }
            if (Permission == Enums.Permission.None.ToString())
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



            #region Handle Permissions in grid
            if (Permission == Enums.Permission.None.ToString())
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
                    objuserData.lo = Permission == Enums.Permission.View.ToString() ? "1" : "0";
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
                  .Select(r => CreateHierarchyItem(DataSet, r, CustomColumnNames, StandardColumnNames, lstUser, CurSymbol,TimeFrame))
                  .ToList();

            return new BudgetGridRowModel { id = rowId, data = Data, rows = children, style = stylecolorgray, Detailid = Convert.ToString(id), userdata = objuserData };
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Get row data from custom table as per detail id.
        /// </summary>
        /// <param name="DataSet">Set of tables including standard and custom data</param>
        /// <param name="BudgetDetailId">Budget detail id </param>
        private IEnumerable<DataRow> GetCustomColumnRow(DataSet DataSet, int? BudgetDetailId)
        {
            return DataSet.Tables[1]
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("BudgetDetailId") == BudgetDetailId);
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Get child data from parent id.
        /// </summary>
        /// <param name="DataSet">Set of tables including standard and custom data</param>
        /// <param name="parentId">Parent id to get child data from the table </param>
        private IEnumerable<DataRow> GetChildren(DataSet DataSet, int? parentId)
        {
            return DataSet.Tables[0]
              .Rows
              .Cast<DataRow>()
              .Where(row => row.Field<Nullable<Int32>>("ParentId") == parentId);
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Returns data from Sp
        /// </summary>
        /// <param name="budgetId">Id of the Budget</param>
        /// <param name="TimeFrame">TimeFrame Selected</param>
        /// <param name="ClientID">Id of the Client</param>
        /// <param name="UserID">Id of the User</param>
        /// <param name="Exchangerate">Exachange rate of the currency</param>
        /// <param name="CommaSeparatedUserIds">list of user ids </param>
        public DataSet GetBudgetDefaultData(int budgetId, string timeframe, int ClientID, int UserID, string CommaSeparatedUserIds, double Exchangerate)
        {
            DataSet EntityList = new DataSet();

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

            return EntityList;
        }

        /// <summary>
        /// Added by Komal Rawal
        /// Method to set header for the grid
        /// <param name="CustomColumnNames">List of names of custom column</param>
        /// <param name="StandardColumnNames">List of names of standard column</param>
        /// <param name="TimeFrame">Selected Time Frame</param>
        /// <param name="mdlGridHeader">Model grid header</param>
        /// <param name="lstCustomFieldOptions">List of custom field options</param>
        private BudgetGridModel SetHeaderObject(List<string> CustomColumnNames, List<string> StandardColumnNames, string timeframe, BudgetGridModel mdlGridHeader, List<CustomFieldOption> lstCustomFieldOptions)
        {
            List<GridDataStyle> ListHead = new List<GridDataStyle>(); //list to bind all header data
            StringBuilder sbAttachedHeaders = new StringBuilder(); //used to get comma separated values to attach 2 headers
            List<GridDataStyle> ListAppendAtLast = new List<GridDataStyle>();//list will have data oof user,owner,line items as we need to attach them at the end as per UI
            GridDataStyle headObj = new GridDataStyle();
            string Readonly = "ro";
            string Editable = "ed";
            string Combo = "coro"; // combo type in the grid
            string ColumnId = "Id";
            string TaskName = "Task Name";
            string aligncenter = "center";
            string sort = "na";
            List<int> colIndexes = new List<int>(); //List index of column used for given numberformat to rollup columns and also used in aggregation in unallocated columns

            int colindex = 0;//find the index of column used for given numberformat to rollup columns and also used in aggregation in unallocated columns  
            StringBuilder Allocatedforcastcolums = new StringBuilder(); ;//store indexes of all forcast columns used to create unallocated column foumula
            StringBuilder Allocatedbudgetcolums = new StringBuilder();//store indexes of all Budget columns used to create unallocated column foumula
            string totalforcastcolumns = string.Empty;//store indexe of total forcast column used to create unallocated column foumula
            string totalbudgetcolumns = string.Empty;////store indexe of total Budget columns used to create unallocated column foumula
            char plus = '+';
            string c = "c";
            string customFieldName = string.Empty;
            int customFieldId = 0;

            #region Bind Standard Columns
            foreach (var columns in StandardColumnNames)
            {
                if (columns == Enums.DefaultGridColumn.BudgetDetailId.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = ColumnId;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append(ColumnId + ",");
                    }

                    headObj.sort = sort;
                    headObj.width = 100;
                    headObj.align = aligncenter;
                    headObj.type = Readonly;
                    headObj.id = ColumnId;
                    ListHead.Add(headObj);
                    colindex++;
                }
                else if (columns == Enums.DefaultGridColumn.Name.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = TaskName;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append(TaskName + ",");
                        sbAttachedHeaders.Append(string.Empty + ",");
                    }
                    headObj.sort = sort;
                    headObj.width = 200;
                    headObj.align = "left";
                    headObj.type = "tree";
                    headObj.id = columns;
                    ListHead.Add(headObj);
                    colindex++;

                    //Add icons column after name
                    headObj = new GridDataStyle();
                    headObj.value = string.Empty;
                    headObj.sort = sort;
                    headObj.width = 50;
                    headObj.align = aligncenter;
                    headObj.type = Readonly;
                    headObj.id = "Add Row";
                    ListHead.Add(headObj);
                    colindex++;

                }
                else if (columns == Enums.DefaultGridColumn.Users.ToString() ||
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
                    headObj.sort = sort;
                    headObj.width = 100;
                    headObj.align = aligncenter;
                    if(columns == Enums.DefaultGridColumn.Owner.ToString())
                    {
                        headObj.type = Combo; //set type for the drop down in the dhtmlxgrid
                    }
                    else
                    {
                        headObj.type = Readonly;
                    }

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
                    headObj.sort = sort;
                    headObj.width = 100;
                    headObj.align = aligncenter;
                    headObj.type = Readonly;
                    headObj.id = "Line Items";
                    ListAppendAtLast.Add(headObj);
                }
                else
                {
                    headObj = new GridDataStyle();

                    headObj.id = columns;
                    headObj.sort = sort;
                    headObj.width = 100;
                    headObj.align = aligncenter;

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
                    if (columns.ToString().Contains(Enums.DefaultGridColumn.Budget.ToString()) || columns.ToString().Contains(Enums.DefaultGridColumn.Forecast.ToString()) || columns.ToString().Contains(Enums.DefaultGridColumn.Planned.ToString()) || columns.ToString().Contains(Enums.DefaultGridColumn.Actual.ToString()))
                    {
                        colIndexes.Add(colindex);//add index of rollup column
                        headObj.type = "edn[=sum]"; // set coltype for rollup columns

                        #region  set formaula for unallocated columns

                        if (timeframe != Enums.QuarterFinance.Yearly.ToString())
                        {
                            if (columns.ToString().Contains(Enums.DefaultGridColumn.Forecast.ToString()))
                            {
                                if (columns.ToString().ToLower().Contains("total"))
                                {
                                    totalforcastcolumns = c + colindex;
                                }
                                else
                                {

                                    Allocatedforcastcolums.Append(c + colindex + plus);

                                }
                            }
                            else if (columns.ToString().Contains(Enums.DefaultGridColumn.Budget.ToString()))
                            {
                                if (columns.ToString().ToLower().Contains("total"))
                                {
                                    totalbudgetcolumns = c + colindex;
                                }
                                else
                                {
                                    Allocatedbudgetcolums.Append(c + colindex + plus);

                                }
                            }
                        }

                        #endregion

                    }
                    else
                    {
                        headObj.type = Editable;
                    }

                    ListHead.Add(headObj);
                    colindex++;

                }

            }
            #endregion

            #region Add Unallocated Columns for views other than yearly

            if (timeframe != Enums.QuarterFinance.Yearly.ToString())
            {
                headObj = new GridDataStyle();
                headObj.value = "Unallocated";
                headObj.sort = "";
                headObj.width = 100;
                headObj.align = "center";
                headObj.type = "edn[=" + totalforcastcolumns + "-(" + Allocatedforcastcolums.ToString().TrimEnd(plus) + ")]";
                headObj.id = "Unallocated_Forecast";
                ListHead.Add(headObj);
                colIndexes.Add(colindex);
                sbAttachedHeaders.Append("Forecast" + ",");

                headObj = new GridDataStyle();
                headObj.value = "Unallocated";
                headObj.sort = "";
                headObj.width = 100;
                headObj.align = "center";
                headObj.type = "edn[=" + totalbudgetcolumns + "-(" + Allocatedbudgetcolums.ToString().TrimEnd(plus) + ")]";
                headObj.id = "Unallocated_Budget";
                ListHead.Add(headObj);
                colIndexes.Add(colindex + 1);
                sbAttachedHeaders.Append("Budget" + ",");
            }

            #endregion

            #region Bind Header for custom columns
            foreach (string column in CustomColumnNames)
            {
                // Split column name with "_" to get name and id. e.g. EMA_123 -> Name:EMA and ID:123
                string[] arrColumn = column.Split('_');
                if (arrColumn != null && arrColumn.Count() > 0)
                {
                    customFieldName = arrColumn.First();
                    int.TryParse(arrColumn.Last(), out customFieldId);
                }

                if (customFieldName != Enums.DefaultGridColumn.BudgetDetailId.ToString())
                {
                    headObj = new GridDataStyle();
                    if (timeframe == Enums.QuarterFinance.Yearly.ToString())
                    {
                        headObj.value = customFieldName;
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append(customFieldName + ",");
                    }
                    headObj.sort = sort;
                    headObj.width = 100;
                    headObj.align = aligncenter;
                    // Bind custom field options into header
                    if (lstCustomFieldOptions.Where(cf => cf.CustomFieldId == customFieldId).Any())
                    {
                        headObj.type = Combo;
                        headObj.options = lstCustomFieldOptions.Where(cf => cf.CustomFieldId == customFieldId).Select(x => new Options { id = x.CustomFieldOptionId, value = x.Value }).ToList();
                    }
                    else
                    {
                        headObj.type = Editable;
                    }
                    headObj.id = string.Format("Cust_{0}",customFieldId);
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
            mdlGridHeader.colIndexes = colIndexes;
            mdlGridHeader.GridDataStyleList = ListHead;
            return mdlGridHeader;
        }


        /// <summary>
        /// Get list of users for specific client
        /// </summary>
        /// <param name="ClientID">Client Id</param>
        /// <returns>Returns list of users for the client</returns>
        public List<BDSService.User> GetUserListByClientId(int ClientID)
        {
            List<BDSService.User> lstUser = _ServiceDatabase.GetUserListByClientIdEx(ClientID).ToList();
            return lstUser;
        }

        /// <summary>
        /// This is owner list (client wise), and it will be bind into dropdown at the time of editing
        /// </summary>
        /// <param name="ClientId">Client Id</param>
        /// <param name="ApplicationId">Application Id</param>
        /// <param name="lstUsers">List of users for current client</param>
        /// <returns>Returns list of PlanOptions contains user ids and names</returns>
        public List<PlanOptions> GetOwnerListForDropdown(int ClientId, Guid ApplicationId, List<BDSService.User> lstUsers)
        {
            return Common.GetOwnerListForDropdown(ClientId, ApplicationId, lstUsers);
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


            return NextBudgetId; //return new budgetid if user delete root/Parent budget.
        }

        #region Import Marketing Budget

        /// <summary>
        /// Read Data from excel 2007/(.xlsx) and above version format file to xml
        /// </summary>
        /// <param name="fileLocation">Location of file to read the uploaded data</param>
        /// <param name="BudgetDetailId">For which budget user want to import data</param>
        /// <param name="PlanExchangeRate">exchange rate for client</param>
        /// <returns></returns>
        public BudgetImportData GetXLSXData(string viewByType, string fileLocation, int ClientId, int BudgetDetailId = 0, double PlanExchangeRate = 0, string CurrencySymbol = "$")
        {
            BudgetImportData objImportData = new BudgetImportData();
            DataTable dtColumns = new DataTable();
            XmlDocument xmlDoc = new XmlDocument();
            dtColumns.Columns.Add("Month", typeof(string));
            dtColumns.Columns.Add("ColumnName", typeof(string));
            dtColumns.Columns.Add("ColumnIndex", typeof(Int64));
            BudgetDetailId = _database.Budget_Detail.Where(a => a.BudgetId == BudgetDetailId).Select(a => a.Id).FirstOrDefault();
            ValidateBudget(BudgetDetailId, ClientId);

            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileLocation, false))
            {
                //Read the first Sheet from Excel file.
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                //Get the Worksheet instance.
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                //Fetch all the rows present in the Worksheet.
                IEnumerable<Row> rowsData = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                List<Row> RowsListData = rowsData.ToList();
                RowsListData.Remove(RowsListData.LastOrDefault());

                if (viewByType == Enums.QuarterFinance.Yearly.ToString())
                {
                    RowsListData.Insert(0, RowsListData[0]);
                }
                List<Row> rows = RowsListData;
                //Loop through the Worksheet rows.
                XmlNode rootNode = xmlDoc.CreateElement("data");
                xmlDoc.AppendChild(rootNode);

                List<XmlColumns> listColumnIndex = new List<XmlColumns>();
                List<CustomColumnModel> ListCustomCols = GetCustomColumns(ClientId);// Get List of Custom Columns 
                int matchrowforbudget = 3;  // RowIndex 3 is for first row
                int firstrowindex = 2;
                if (viewByType == Enums.QuarterFinance.Yearly.ToString())
                {
                    matchrowforbudget = 2;
                    firstrowindex = 1;
                }
                string colName = string.Empty, colValue = string.Empty;
                double coldata;
                foreach (Row row in rows)
                {
                    //Use the first row to add columns to DataTable.
                    if (row.RowIndex.Value > firstrowindex)
                    {
                        XmlNode childnode = xmlDoc.CreateElement("row");
                        rootNode.AppendChild(childnode);

                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            XmlColumns CellData = listColumnIndex.Where(a => a.ColumnIndex == i).Select(a => a).FirstOrDefault();
                            if (CellData != null)
                            {
                                //  To handle the multi currency for budget,foracast and custom columns which have currency validation type
                                colName = Convert.ToString(CellData.ColumName);
                                colValue = GetCellValue(doc, cell).Trim();
                                colValue = colValue.Replace(CurrencySymbol, "");
                                double.TryParse(colValue, out coldata);

                                if (colName == Convert.ToString(Enums.FinanceHeader_Label.Budget) || colName == Convert.ToString(Enums.FinanceHeader_Label.Forecast))
                                {
                                    colValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(coldata, PlanExchangeRate));
                                }
                                else if (ListCustomCols != null)
                                {
                                    CustomColumnModel CustomCol = ListCustomCols
                                        .Where(a => a.ColName == colName)
                                        .Select(a => a).FirstOrDefault();

                                    if (CustomCol != null)
                                    {
                                        if (CustomCol.ValidationType == Convert.ToString(Enums.ColumnValidation.ValidCurrency))
                                        {
                                            colValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(coldata, PlanExchangeRate));
                                        }
                                    }
                                }
                                XmlNode datanode = xmlDoc.CreateElement("value");
                                XmlAttribute attribute = xmlDoc.CreateAttribute("code");
                                attribute.Value = colName;
                                datanode.Attributes.Append(attribute);
                                datanode.InnerText = colValue;

                                if (row.RowIndex == matchrowforbudget)
                                {
                                    if (!string.IsNullOrEmpty(attribute.Value) && Convert.ToString(attribute.Value).ToLower() == "id")
                                    {
                                        int n;
                                        bool isNumeric = int.TryParse(datanode.InnerText, out n);
                                        if (isNumeric)
                                        {
                                            if (BudgetDetailId != n)
                                            {
                                                objImportData = new BudgetImportData();
                                                objImportData.ErrorMsg = "Data getting uploaded does not relate to specific Budget/Forecast.";
                                                return objImportData;
                                            }
                                        }
                                    }
                                }
                                childnode.AppendChild(datanode);
                            }
                            i++;
                        }
                    }
                    else
                    {
                        // Get list of columns and its time frame
                        if (row.RowIndex.Value == 1 && dtColumns.Rows.Count == 0)
                        {
                            string columnName = string.Empty;
                            string columnNameLower = string.Empty;
                            string InnerColName = string.Empty;
                            List<Row> HeaderRows = RowsListData.Where(a => a.RowIndex.Value == 1 || a.RowIndex.Value == 2).ToList();
                            if (HeaderRows.Count > 0)
                            {
                                List<Cell> InnerHeader = HeaderRows[0].Descendants<Cell>().Select(a => a).ToList();
                                int p = 0;
                                int j = 1;
                                foreach (Cell cell in HeaderRows[1].Descendants<Cell>())
                                {
                                    columnName = GetCellValue(doc, cell);
                                    if (!string.IsNullOrEmpty(columnName))
                                    {
                                        columnNameLower = columnName.ToLower();
                                        if (columnNameLower != Convert.ToString(Enums.FinanceHeader_Label.Planned).ToLower() && columnNameLower != Convert.ToString(Enums.FinanceHeader_Label.Actual).ToLower())
                                        {
                                            Cell InnerCol = InnerHeader[p];
                                            InnerColName = GetCellValue(doc, InnerCol);
                                            if (viewByType == Enums.QuarterFinance.Yearly.ToString())
                                            {
                                                if (InnerColName == Enums.FinanceHeader_Label.Budget.ToString() || InnerColName == Enums.FinanceHeader_Label.Forecast.ToString())
                                                    InnerColName = "Total";
                                                else
                                                    InnerColName = string.Empty;
                                            }

                                            listColumnIndex.Add(new XmlColumns { ColumName = columnName, ColumnIndex = p });
                                            dtColumns.Rows.Add();
                                            dtColumns.Rows[j - 1]["Month"] = InnerColName;
                                            dtColumns.Rows[j - 1]["ColumnIndex"] = j;
                                            dtColumns.Rows[j - 1]["ColumnName"] = columnName;
                                            j++;
                                        }
                                    }
                                    p++;
                                }
                            }
                        }
                    }
                }
            }
            // check if import monthly/quaterly data file for total(yearly) view
            List<string> monthcolumn = dtColumns.Rows.Cast<DataRow>().Select(x => x.Field<string>("Month").ToLower()).Distinct().ToList();
            if (viewByType == Convert.ToString(Enums.QuarterFinance.Yearly))
            {
                if (monthcolumn.Count() == 1 && string.IsNullOrEmpty(monthcolumn.FirstOrDefault()))
                    objImportData.ErrorMsg = "Data getting uploaded does not related to specific view.";
            }
            else if (!string.IsNullOrEmpty(monthcolumn[0].ToString()) || monthcolumn[0].ToLower() == "id")
            {
                objImportData.ErrorMsg = "Data getting uploaded does not related to specific view.";
            }
            //end
            objImportData.MarketingBudgetColumns = dtColumns;
            objImportData.XmlData = xmlDoc;

            return objImportData;
        }



        /// <summary>
        /// Method to read XLS file which user import
        /// </summary>
        /// <param name="viewByType">selected Time frame type </param>
        /// <param name="ds"> dataset of data which get import</param>
        /// <param name="ClientId">client id detail</param>
        /// <param name="BudgetDetailId">Budget id for which user wants to import data</param>
        /// <param name="PlanExchangeRate">Exchange rate of client</param>
        /// <param name="CurrencySymbol">prefred currency</param>
        /// <returns></returns>
        public BudgetImportData GetXLSData(string viewByType, DataSet ds, int ClientId, int BudgetDetailId = 0, double PlanExchangeRate = 0, string CurrencySymbol = "$")
        {
            List<XmlColumns> listColumnIndex = new List<XmlColumns>();
            DataTable dtExcel = new DataTable();
            BudgetImportData objImportData = new BudgetImportData();
            DataTable dtColumns = new DataTable();
            XmlDocument xmlDoc = new XmlDocument();
            dtColumns.Columns.Add("Month", typeof(string));
            dtColumns.Columns.Add("ColumnName", typeof(string));
            dtColumns.Columns.Add("ColumnIndex", typeof(Int64));
            int RowCount = 0, ColumnCount = 0;
            string colName = string.Empty, colValue = string.Empty;

            BudgetDetailId = _database.Budget_Detail.Where(a => a.BudgetId == BudgetDetailId).Select(a => a.Id).FirstOrDefault();
            ValidateBudget(BudgetDetailId, ClientId);

            try
            {
                List<CustomColumnModel> ListCustomCols = GetCustomColumns(ClientId);// Get List of Custom Columns 
                if (ds != null && ds.Tables.Count > 0)
                {
                    dtExcel = ds.Tables[0];
                    if (dtExcel != null)
                    {
                        if (dtExcel.Rows.Count > 0)
                        {
                            RowCount = dtExcel.Rows.Count;
                        }
                        if (dtExcel.Columns.Count > 0)
                        {
                            ColumnCount = dtExcel.Columns.Count;
                        }
                    }
                }

                XmlNode childnode = null;
                if (RowCount > 0)
                {
                    dtExcel.Rows[RowCount - 1].Delete();
                    dtExcel.AcceptChanges();

                    XmlNode rootNode = xmlDoc.CreateElement("data");
                    xmlDoc.AppendChild(rootNode);
                    // In case of yearly view add extra row to match with all other view
                    if (viewByType == Convert.ToString(Enums.QuarterFinance.Yearly))
                    {
                        DataRow dr = dtExcel.NewRow();
                        for (int k = 0; k < ColumnCount; k++)
                        {
                            string columnname = dtExcel.Rows[0][k].ToString();
                            if (columnname.ToLower() == Convert.ToString(Enums.FinanceHeader_Label.Budget).ToLower() || columnname.ToLower() == Convert.ToString(Enums.FinanceHeader_Label.Forecast).ToLower())
                            {
                                dr[k] = "Total";
                            }
                            else
                                dr[k] = string.Empty;
                        }
                        dtExcel.Rows.InsertAt(dr, 0);
                        dtExcel.AcceptChanges();
                    }
                    //end
                    RowCount = dtExcel.Rows.Count;
                    string columnName = string.Empty;
                    string columnNameLower = string.Empty;
                    for (int i = 0; i < RowCount; i++)
                    {

                        int p = 0;
                        int j = 1;
                        // Create Child Node For Data
                        if (i > 1)
                        {
                            childnode = xmlDoc.CreateElement("row");
                            rootNode.AppendChild(childnode);
                        }
                        for (int k = 0; k < ColumnCount; k++)
                        {
                            #region Create Data Table For Column name and it's TimeFrame
                            if (i == 0)
                            {
                                //  Get list of columns and its time frame
                                if (RowCount > (i + 2)) // Set Condition for invalid file where only first two rows (Timeframe and column names) without data
                                {
                                    columnName = Convert.ToString(dtExcel.Rows[i + 1][k]);
                                    if (!string.IsNullOrEmpty(columnName))
                                    {
                                        columnNameLower = columnName.ToLower();
                                        if (columnNameLower != Convert.ToString(Enums.FinanceHeader_Label.Planned).ToLower() && columnNameLower != Convert.ToString(Enums.FinanceHeader_Label.Actual).ToLower())
                                        {
                                            string InnerColName = Convert.ToString(dtExcel.Rows[i][k]);
                                            listColumnIndex.Add(new XmlColumns { ColumName = columnName, ColumnIndex = p });
                                            dtColumns.Rows.Add();
                                            dtColumns.Rows[j - 1]["Month"] = InnerColName;
                                            dtColumns.Rows[j - 1]["ColumnIndex"] = j;
                                            dtColumns.Rows[j - 1]["ColumnName"] = columnName;
                                            j++;
                                        }
                                    }
                                    p++;
                                }
                            }
                            #endregion

                            #region Insert Record to XML
                            if (i > 1)
                            {
                                XmlColumns CellData = listColumnIndex.Where(a => a.ColumnIndex == k).Select(a => a).FirstOrDefault();

                                if (CellData != null)
                                {
                                    // To handle the multi currency for budget,foracast and custom columns which have currency validation type
                                    colName = CellData.ColumName;
                                    colValue = Convert.ToString(dtExcel.Rows[i][k]).Trim();
                                    colValue = colValue.Replace(CurrencySymbol, "");
                                    double coldata;
                                    double.TryParse(colValue, out coldata);

                                    if (colName == Convert.ToString(Enums.FinanceHeader_Label.Budget) || colName == Convert.ToString(Enums.FinanceHeader_Label.Forecast))
                                    {
                                        colValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(coldata, PlanExchangeRate));
                                    }
                                    else if (ListCustomCols != null)
                                    {
                                        CustomColumnModel CustomCol = ListCustomCols
                                            .Where(a => a.ColName == colName)
                                            .Select(a => a).FirstOrDefault();

                                        if (CustomCol != null)
                                        {
                                            if (CustomCol.ValidationType == Convert.ToString(Enums.ColumnValidation.ValidCurrency))
                                            {
                                                colValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(coldata, PlanExchangeRate));
                                            }
                                        }
                                    }
                                    XmlNode datanode = xmlDoc.CreateElement("value");
                                    XmlAttribute attribute = xmlDoc.CreateAttribute("code");
                                    attribute.Value = colName;
                                    datanode.Attributes.Append(attribute);
                                    datanode.InnerText = colValue;

                                    // RowIndex 2 is for first row
                                    if (i == 2)
                                    {
                                        if (!string.IsNullOrEmpty(attribute.Value) && Convert.ToString(attribute.Value).ToLower() == "id")
                                        {
                                            int n;
                                            bool isNumeric = int.TryParse(datanode.InnerText, out n);
                                            if (isNumeric)
                                            {
                                                if (BudgetDetailId != n)
                                                {
                                                    objImportData = new BudgetImportData();
                                                    objImportData.ErrorMsg = "Data getting uploaded does not relate to specific Budget/Forecast.";
                                                    return objImportData;
                                                }
                                            }
                                        }
                                    }
                                    childnode.AppendChild(datanode);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
            // check if import monthly/quaterly data file  for total(yearly) view
            List<string> monthcolumn = dtColumns.Rows.Cast<DataRow>().Select(x => x.Field<string>("Month").ToLower()).Distinct().ToList();
            if (viewByType == Convert.ToString(Enums.QuarterFinance.Yearly))
            {
                if (monthcolumn.Count() == 1 && string.IsNullOrEmpty(monthcolumn.FirstOrDefault()))
                    objImportData.ErrorMsg = "Data getting uploaded does not related to specific view.";
            }
            else if (!string.IsNullOrEmpty(monthcolumn[0].ToString()) || monthcolumn[0].ToLower() == "id")
            {
                objImportData.ErrorMsg = "Data getting uploaded does not related to specific view.";
            }
            //end
            objImportData.MarketingBudgetColumns = dtColumns;
            objImportData.XmlData = xmlDoc;

            return objImportData;
        }

        /// Get the value of cell from excel sheet.
        private string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = string.Empty;
            if (cell.CellValue != null)
            {
                value = cell.CellValue.InnerText;
            }
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }

        /// <summary>
        /// Method to get list of custom column for budget
        /// </summary>
        /// <returns></returns>
        private List<CustomColumnModel> GetCustomColumns(int ClientId)
        {
            List<CustomColumnModel> lstColumns = (from objColumnSet in _database.Budget_ColumnSet
                                                  join objColumn in _database.Budget_Columns on objColumnSet.Id equals objColumn.Column_SetId
                                                  join objCustomField in _database.CustomFields on objColumn.CustomFieldId equals objCustomField.CustomFieldId
                                                  where objColumnSet.ClientId == ClientId
                                                  && objColumn.IsTimeFrame == false
                                                  && objColumnSet.IsDeleted == false && objColumn.IsDeleted == false && objCustomField.IsDeleted == false
                                                  select new CustomColumnModel
                                                  {
                                                      ColName = objCustomField.Name,
                                                      CustomColumSetId = objColumnSet.Id,
                                                      CustomFieldId = objCustomField.CustomFieldId,
                                                      ValidationType = objColumn.ValidationType
                                                  }).ToList();
            return lstColumns;
        }

        /// <summary>
        /// Desc:: Import Marketing finance Data from excel and save to database.
        /// </summary>
        public int ImportMarketingFinance(XmlDocument XMLData, DataTable ImportBudgetCol, int UserID, int ClientID, int BudgetDetailId = 0)
        {
            // Check the file data is monthly or quarterly
            List<string> MonthList = new List<string>();
            List<string> DefaultMonthList = new List<string>();
            MonthList = ImportBudgetCol.Rows.Cast<DataRow>().Select(x => x.Field<string>("Month").ToLower()).Distinct().ToList();

            DefaultMonthList = Enums.ReportMonthDisplayValuesWithPeriod.Select(a => a.Key.ToLower()).ToList();

            bool IsMonthly = (from dtMonth in MonthList
                             join defaultMonth in DefaultMonthList on dtMonth equals defaultMonth
                             select new { dtMonth }).Any();
            //end
            BudgetDetailId = _database.Budget_Detail.Where(a => a.BudgetId == BudgetDetailId).Select(a => a.Id).FirstOrDefault();
            ValidateBudget(BudgetDetailId, ClientID);

            ///If connection is closed then it will be open
            SqlConnection Connection = _database.Database.Connection as SqlConnection;
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection.Open();
            }
            SqlCommand command = new SqlCommand();
            int ExecuteCommand = 0;
            string spname = string.Empty;

            if (IsMonthly)
            {
                spname = "ImportMarketingBudgetMonthly";
            }
            else
            {
                spname = "ImportMarketingBudgetQuarter";
            }
            using (command = new SqlCommand(spname, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", UserID);
                command.Parameters.AddWithValue("@ClientId", ClientID);
                command.Parameters.AddWithValue("@XMLData", XMLData.InnerXml);
                command.Parameters.AddWithValue("@ImportBudgetCol", ImportBudgetCol);
                command.Parameters.AddWithValue("@BudgetDetailId", BudgetDetailId);
                SqlDataAdapter adp = new SqlDataAdapter(command);
                command.CommandTimeout = 0;
                ExecuteCommand = command.ExecuteNonQuery();
            }
            Connection.Close();
            return ExecuteCommand;

        }

        #endregion

        /// <summary>
        /// Get values of Budget, Forecast, Planned and Actual for HUD
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <param name="ExchangeRate">Currency exchange rate</param>
        /// <returns>Returns datatable having 4 values(Budget,Forecast,Planned,Actual)</returns>
        public MarketingBudgetHeadsUp GetFinanceHeaderValues(int BudgetId, double ExchangeRate, List<BDSService.User> lstUser,bool IsLineItem=false)
        {
            #region "Declare Variables"
            SqlParameter[] para = new SqlParameter[3];
            MarketingBudgetHeadsUp calResultset = new MarketingBudgetHeadsUp();   // Return Marketing Budget Header Result Data Model
            List<int> lstUserId = lstUser.Select(a => a.ID).ToList();
            string CommaSeparatedUserIds = String.Join(",", lstUserId);
            #endregion

            #region "Set SP Parameters"
            para[0] = new SqlParameter() { ParameterName = "BudgetId", Value = BudgetId };
            para[1] = new SqlParameter() { ParameterName = "lstUserIds", Value = CommaSeparatedUserIds };
            para[2] = new SqlParameter() { ParameterName = "CurrencyRate", Value = ExchangeRate };
            #endregion

            #region "Get Data"
            if (IsLineItem)
            {
                calResultset = _database.Database
                       .SqlQuery<MarketingBudgetHeadsUp>("GetHeaderValuesForFinanceLineItems @BudgetId,@lstUserIds,@CurrencyRate", para).FirstOrDefault();
            }
            else
            {
                calResultset = _database.Database
                .SqlQuery<MarketingBudgetHeadsUp>("GetHeaderValuesForFinance @BudgetId,@lstUserIds,@CurrencyRate", para).FirstOrDefault();
            }
            #endregion

            return calResultset; // Returns Model having 4 values(Budget, Forecast, Planned, Actual)

        }

        #region "Save new Budget related methods"
        /// <summary>
        /// Added by Komal on 12/05/2016
        /// Method to save new budget
        /// </summary>
        ///  /// <param name="budgetName">Name of the Budget</param>
        /// <returns>Returns budget id in json format</returns>
        public int SaveNewBudget(string BudgetName, int ClientId, int UserId)
        {
            int budgetId = 0;
            if (!string.IsNullOrEmpty(BudgetName))
            {
                //save budget data and get budget id 
                RevenuePlanner.Models.Budget objBudget = new RevenuePlanner.Models.Budget();
                objBudget.ClientId = ClientId;
                objBudget.Name = BudgetName;
                objBudget.Desc = string.Empty;
                objBudget.CreatedBy = UserId;
                objBudget.CreatedDate = DateTime.Now;
                objBudget.IsDeleted = false;
                _database.Entry(objBudget).State = EntityState.Added;
                _database.SaveChanges();
                budgetId = objBudget.Id;

                //save data in budget detail table to display row wise data
                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail.BudgetId = budgetId;
                objBudgetDetail.Name = BudgetName;
                objBudgetDetail.IsDeleted = false;
                objBudgetDetail.CreatedBy = UserId;
                objBudgetDetail.CreatedDate = DateTime.Now;
                _database.Entry(objBudgetDetail).State = EntityState.Added;
                _database.SaveChanges();
                int _budgetid = objBudgetDetail.Id;

                //save permission for newly created budget.
                SaveUserBudgetpermission(_budgetid, UserId);
            }
            return budgetId;
        }

        /// <summary>
        /// Added by Komal on 12/05/2016
        /// Method to save new item /child item
        /// </summary>
        /// <param name="BudgetId">Id of the Budget</param>
        /// <param name="BudgetDetailName">Name of the item</param>
        /// <param name="ParentId">ParentId of the Budget item</param>
        /// <param name="mainTimeFrame">Selected time frame value </param>
        public void SaveNewBudgetDetail(int BudgetId, string BudgetDetailName, int ParentId, int ClientId, int UserId, string mainTimeFrame = "Yearly")
        {

            if (BudgetId != 0)
            {
                //Save budget detail data for newly added item to database
                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail.BudgetId = BudgetId;
                objBudgetDetail.Name = BudgetDetailName;
                objBudgetDetail.ParentId = ParentId;
                objBudgetDetail.CreatedBy = UserId;
                objBudgetDetail.CreatedDate = DateTime.Now;
                objBudgetDetail.IsDeleted = false;
                _database.Entry(objBudgetDetail).State = EntityState.Added;
                _database.SaveChanges();
                int _budgetid = objBudgetDetail.Id;
                SaveUserBudgetpermission(_budgetid, UserId);

                #region Update LineItem with child item
                int? BudgetDetailParentid = objBudgetDetail.ParentId;
                IQueryable<LineItem_Budget> objLineItem = _database.LineItem_Budget
                .Where(x => x.BudgetDetailId == BudgetDetailParentid);
                foreach (LineItem_Budget LineItem in objLineItem)
                {
                    LineItem.BudgetDetailId = _budgetid;
                }
                _database.SaveChanges();
                #endregion
            }

        }
        /// <summary>
        /// Added by Komal on 12/05/2016
        /// Method Execute sp to get users of all parent ids and assign to current budget item.
        public void SaveUserBudgetpermission(int budgetId, int UserId)
        {
            _database.SaveuserBudgetPermission(budgetId, 0, UserId); //Sp to get users of all parent ids and assign to current budget item.

        }

        #endregion

        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Get Budget Detail List
        /// </summary>
        /// <param name="ListItems">List of BudgetDetaildIds of the Budget</param>        
        private List<Budget_Detail> GetBudgetDetailList(List<string> ListItems)
        {
            List<Budget_Detail> listobjBudgetDetail = new List<Budget_Detail>();
            listobjBudgetDetail = _database.Budget_Detail.ToList().Where(budgtDtl => ListItems.Contains(budgtDtl.Id.ToString()) && budgtDtl.IsDeleted == false).ToList();
            return listobjBudgetDetail;
        }

        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Update Name Of the budget 
        /// </summary>
        /// <param name="budgetId">List of BudgetDetail</param> 
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param> 
        /// <param name="parentId">parent id of selected budget detaild id</param> 
        /// <param name="clientId">Client id</param> 
        /// <param name="ListItems">List of BudgetDetaildIds of the Budget and its child budget</param>
        /// <param name="nValue">new Value</param>

        public void UpdateTaskName(int budgetId, int budgetDetailId, int parentId, int clientId, string nValue)
        {
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException("clientId", "A clientId less than or equal to zero is invalid, and likely indicates the clientId was not set properly");
            }
            else
            {
                Budget_Detail objBudgetDetail = new Budget_Detail();
                objBudgetDetail  = _database.Budget_Detail.Where(budgtDtl => budgtDtl.Id == budgetDetailId && budgtDtl.IsDeleted == false).FirstOrDefault();
                if (objBudgetDetail != null)
                {
                    if (budgetDetailId > 0 && parentId > 0)
                    {
                        //check objBudgetDetail is not null.
                        if (objBudgetDetail != null)
                        {
                            if (!string.IsNullOrEmpty(nValue))
                            {
                                objBudgetDetail.Name = nValue.Trim(); // assign new budget name to objBudgetDetail.
                            }
                            _database.Entry(objBudgetDetail).State = EntityState.Modified;
                        }
                    }
                    else if (budgetDetailId > 0 && parentId <= 0)
                    {
                        //Update name into Budget Table.
                        RevenuePlanner.Models.Budget objBudget = new RevenuePlanner.Models.Budget();
                        objBudget = _database.Budgets.Where(budgt => budgt.Id == budgetId && budgt.IsDeleted == false && budgt.ClientId == clientId).FirstOrDefault();
                        if (objBudget != null)
                        {
                            if (!string.IsNullOrEmpty(nValue))
                            {
                                objBudget.Name = nValue.Trim(); // assign new budget name to objBudget.
                            }
                            _database.Entry(objBudget).State = EntityState.Modified;
                        }

                        //Update name into Budget Detail Table.
                        if (objBudgetDetail != null)
                        {
                            if (!string.IsNullOrEmpty(nValue))
                            {
                                objBudgetDetail.Name = nValue.Trim(); // assign new budget name to objBudgetDetail.
                            }
                            _database.Entry(objBudgetDetail).State = EntityState.Modified;
                        }

                    }
                    _database.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Update OwnerName Of the budget 
        /// </summary>        
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param>                 
        /// <param name="ListItems">List of BudgetDetaildIds of the Budget and its child budget</param>
        /// <param name="ownerId">new Owner Id</param>

        public void UpdateOwnerName(int budgetDetailId, List<string> listItems, int ownerId)
        {
            List<Budget_Detail> listobjBudgetDetail = GetBudgetDetailList(listItems); // get Budget Detail List
            if (listobjBudgetDetail != null && listobjBudgetDetail.Count > 0 && ownerId != 0)
            {
                //update budget detail with new owner id.
                listobjBudgetDetail.Select(item =>
                {
                    if (item.CreatedBy != ownerId)
                        item.CreatedBy = ownerId;

                    return item;

                }).ToList();
                //get budget permission data for selected budget detailids.
                List<Budget_Permission> BudgetPermissionData = _database.Budget_Permission.ToList().Where(item => listItems.Contains(item.BudgetDetailId.ToString())).ToList();

                //check permission is exist or not for new owner for selected budget detail if exist the delete that entry from budgetdetail_Permission table
                List<Budget_Permission> BudgetPermissionDataExists = BudgetPermissionData.Where(item => item.UserId == ownerId && item.IsOwner == false).ToList();

                if (BudgetPermissionDataExists.Count > 0)
                {
                    BudgetPermissionDataExists.ForEach(a => _database.Entry(a).State = EntityState.Deleted);
                }

                //add entry in budgetdetail_permission table to identify permission.
                List<Budget_Permission> OwnerBudgetPermissionData = BudgetPermissionData.Where(item => item.IsOwner == true).ToList();
                OwnerBudgetPermissionData.Select(id =>
                {
                    id.UserId = ownerId;
                    return id;
                }).ToList();
            }
            _database.SaveChanges();

        }


        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Update Total Budget and Total Forecast Column data Of the budget 
        /// </summary>
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param> 
        /// <param name="nValue">new Value</param>
        /// <param name="columnName">Name of the Column (i.e Budget, Forecast, ..etc)</param> 
        /// <param name="planExchangeRate">Apply multicurrency exchange rate</param> 

        public void UpdateTotalAmount(int budgetDetailId, string nValue, string columnName, double planExchangeRate = 1.0)
        {

            if (budgetDetailId > 0)
            {

                Budget_Detail objMainBudgetDetail = new Budget_Detail();
                objMainBudgetDetail = _database.Budget_Detail.Where(budgtDtl => budgtDtl.Id == budgetDetailId && budgtDtl.IsDeleted == false).FirstOrDefault(); //get budget detail of selected budget.

                if (objMainBudgetDetail != null)
                {
                    double NewValue = 0;
                    double.TryParse(nValue, out NewValue);

                    if (!string.IsNullOrEmpty(nValue))
                    {
                        //check column is budget or forecast
                        if ((string.Compare(columnName, Enums.DefaultGridColumn.Budget.ToString(), true) == 0)
                            || (string.Compare(columnName, Enums.DefaultGridColumn.Total_Budget.ToString(), true) == 0))
                        {
                            objMainBudgetDetail.TotalBudget = Convert.ToDouble(_ObjCurrency.SetValueByExchangeRate(NewValue, planExchangeRate)); // apply currency rate and save new value
                        }
                        else
                        {
                            objMainBudgetDetail.TotalForecast = Convert.ToDouble(_ObjCurrency.SetValueByExchangeRate(NewValue, planExchangeRate)); // apply currency rate and save new value
                        }
                    }
                    _database.Entry(objMainBudgetDetail).State = EntityState.Modified;
                    _database.SaveChanges();

                }

            }
        }


        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Update Budget and Forecast Column data monthly or quaterly view and also update customfield data Of the budget 
        /// </summary>
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param> 
        /// <param name="nValue">new Value</param>
        /// <param name="isForecast">check editable field value is for forecast or budget</param>
        /// <param name="allocationType">Type of allocation i.e(quarter, monthly, yearly)</param>
        /// <param name="Period">period (i.e Jan, Feb.. etc)</param>
        /// <param name="planExchangeRate">Apply multicurrency exchange rate</param> 

        public void SaveBudgetorForecast(int budgetDetailId, string nValue, bool isForecast = false, string allocationType = "quarters", string period = "", double planExchangeRate = 1.0)
        {
            // get budget amount according to budgetdetail id and its period
            Budget_DetailAmount objBudAmount = new Budget_DetailAmount();
            objBudAmount = GetBudgetAmount(budgetDetailId, allocationType, period);

            // check budget amount of the periticular period is exist or not. 
            //If objBudAmount is null then add new budget amount data into Budget_DetailAmount table
            if (objBudAmount == null)
            {
                objBudAmount = new Budget_DetailAmount();
                if (budgetDetailId > 0)
                {
                    double newValue;
                    bool isNumeric = double.TryParse(nValue, out newValue);
                    if (isForecast)
                    {
                        //assign value to forecast field
                        objBudAmount.Forecast = Convert.ToDouble(_ObjCurrency.SetValueByExchangeRate(newValue, planExchangeRate));
                    }
                    else
                    {
                        //assign value to budget field
                        objBudAmount.Budget = Convert.ToDouble(_ObjCurrency.SetValueByExchangeRate(newValue, planExchangeRate));
                    }
                    objBudAmount.Period = dataPeriod;
                    objBudAmount.BudgetDetailId = budgetDetailId;
                    _database.Entry(objBudAmount).State = EntityState.Added;
                    _database.SaveChanges();
                }
            }
            else
            {
                // If not null then update budget amount data into Budget_DetailAmount table
                UpdateBudgetorForecast(budgetDetailId, allocationType, nValue, isForecast, planExchangeRate);
            }

        }

        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Save Budget and Forecast Column data monthly or quaterly view.
        /// </summary>
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param> 
        /// <param name="allocationType">Type of allocation i.e(quarter, monthly, yearly)</param>
        /// <param name="isForecast">check the field is forecast or not</param>
        /// <param name="nValue">new Value</param>        
        /// <param name="planExchangeRate">Apply multicurrency exchange rate</param> 
        private void UpdateBudgetorForecast(int budgetDetailId, string allocationType, string nValue, bool isForecast = false, double planExchangeRate = 1.0)
        {
            if (string.Compare(allocationType, Enums.PlanAllocatedBy.quarters.ToString(), true) == 0)
            {
                // save budget and forecast value in quarterly case
                List<Budget_DetailAmount> BudgetAmountList = new List<Budget_DetailAmount>();
                BudgetAmountList = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && QuaterPeriod.Contains(a.Period)).OrderBy(a => a.Period).ToList();
                double? QuaterSum = 0;
                double? Maindiff = 0;
                Budget_DetailAmount objBudgetDetailAmt;
                if (BudgetAmountList.Count > 0)
                {
                    if (isForecast)
                    {
                        //here we perform sum of the forecast column and assign it to QuaterSum variable
                        QuaterSum = Convert.ToDouble(BudgetAmountList.Select(a => a.Forecast).Sum());
                    }
                    else
                    {
                        //here we perform sum of the budget column and assign it to QuaterSum variable
                        QuaterSum = Convert.ToDouble(BudgetAmountList.Select(a => a.Budget).Sum());
                    }
                    //apply Exchange rate on new value
                    nValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(Convert.ToDouble(nValue), planExchangeRate));

                    //here nValue is Grater than the sum of the quarter the add new value to first month of the quarter
                    if (Convert.ToDouble(nValue) > QuaterSum)
                    {
                        // Add cost from first months of the quarter e.g. For Q1 -> Add cost in Y1 as per requirement
                        objBudgetDetailAmt = BudgetAmountList.FirstOrDefault();
                        Maindiff = Convert.ToDouble(nValue) - QuaterSum;
                        if (!isForecast)
                        {
                            objBudgetDetailAmt.Budget += Maindiff;
                        }
                        else
                        {
                            objBudgetDetailAmt.Forecast += Maindiff;

                        }
                        _database.Entry(objBudgetDetailAmt).State = EntityState.Modified;
                    }
                    //here nValue is less than the sum of the quarter the subtrac new value from the last month to first month of the quarter
                    else if (Convert.ToDouble(nValue) < QuaterSum)
                    {
                        QuaterPeriod.Reverse(); // Reversed list to subtract from last months
                        double? curPeriodVal = 0, needToSubtract = 0;
                        needToSubtract = QuaterSum - Convert.ToDouble(nValue);

                        // Subtract cost from each months of the quarter e.g. For Q1 -> subtract from Y3, Y2 and Y1 as per requirement
                        foreach (string quarter in QuaterPeriod)
                        {
                            objBudgetDetailAmt = BudgetAmountList.Where(pcptc => pcptc.Period == quarter).FirstOrDefault();
                            if (objBudgetDetailAmt != null)
                            {
                                if (isForecast)
                                {
                                    curPeriodVal = objBudgetDetailAmt.Forecast;
                                    curPeriodVal = curPeriodVal - needToSubtract;
                                    needToSubtract = -curPeriodVal;
                                    objBudgetDetailAmt.Forecast = curPeriodVal < 0 ? 0 : curPeriodVal;
                                }
                                else
                                {
                                    curPeriodVal = objBudgetDetailAmt.Budget;
                                    curPeriodVal = curPeriodVal - needToSubtract;
                                    needToSubtract = -curPeriodVal;
                                    objBudgetDetailAmt.Budget = curPeriodVal < 0 ? 0 : curPeriodVal;
                                }
                                if (curPeriodVal >= 0)
                                {
                                    _database.Entry(objBudgetDetailAmt).State = EntityState.Modified;
                                    break;  // break when quarterly allocated value subtracted from months
                                }
                                _database.Entry(objBudgetDetailAmt).State = EntityState.Modified;
                            }
                        }
                    }
                    _database.SaveChanges();
                }
            }
            else
            {
                // save budget and forecast value in monthly case
                Budget_DetailAmount objBudAmountUpdate = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && a.Period == dataPeriod).FirstOrDefault();
                if (objBudAmountUpdate != null)
                {
                    if (isForecast)
                    {
                        objBudAmountUpdate.Forecast = Convert.ToDouble(nValue);
                    }
                    else
                    {
                        objBudAmountUpdate.Budget = Convert.ToDouble(nValue);
                    }
                    _database.Entry(objBudAmountUpdate).State = EntityState.Modified;
                    _database.SaveChanges();
                }
            }
        }


        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Save Custom field data.
        /// </summary>        
        /// <param name="columnName">Name of the Column (i.e Budget, Forecast, ..etc)</param>
        /// <param name="customCol">custom Column object</param>
        /// <param name="budgetDetailId">BudgetDetaildIds of the Budget</param> 
        /// <param name="nValue">new Value</param>        
        /// <param name="userId">User Id</param>        
        /// <param name="planExchangeRate">Apply multicurrency exchange rate</param> 
        public void SaveCustomColumnValues(int customfieldId, Budget_Columns customCol, int budgetDetailId, string nValue, int userId, double planExchangeRate = 1.0)
        {
            if (customfieldId == customCol.CustomField.CustomFieldId)
            {
                string[] DoubleColumnValidation = new string[] { Enums.ColumnValidation.ValidCurrency.ToString(), Enums.ColumnValidation.ValidNumeric.ToString() };
                CustomField_Entity objCustomFieldEnity = new CustomField_Entity();
                //get Customfield value from CustomField_Entity table of perticular column.

                objCustomFieldEnity = _database.CustomField_Entity.Where(a => a.EntityId == (budgetDetailId != 0 ? budgetDetailId : 0) && a.CustomFieldId == customCol.CustomFieldId).FirstOrDefault();

                if (DoubleColumnValidation.Contains(customCol.ValidationType))
                {
                    double newValue;
                    bool isNumeric = double.TryParse(nValue, out newValue);
                    //if customfield is numeric the apply multi currency ExchangeRate.
                    if (isNumeric)
                    {
                        nValue = Convert.ToString(_ObjCurrency.SetValueByExchangeRate(newValue, planExchangeRate));
                    }
                }
                //here we need to check customfied value is exist or not in Customfield_Entity Table
                if (objCustomFieldEnity != null)
                {
                    //customfied value is exist so we need to update the value in Customfield_Entity Table
                    objCustomFieldEnity.Value = nValue;
                    _database.Entry(objCustomFieldEnity).State = EntityState.Modified;
                }
                else
                {
                    //customfied value is not exist so we need to add the value in Customfield_Entity Table
                    objCustomFieldEnity = new CustomField_Entity();
                    objCustomFieldEnity.CustomFieldId = customCol.CustomFieldId;
                    objCustomFieldEnity.EntityId = budgetDetailId;
                    objCustomFieldEnity.Value = nValue;
                    objCustomFieldEnity.CreatedBy = userId;
                    objCustomFieldEnity.CreatedDate = System.DateTime.Now;
                    _database.Entry(objCustomFieldEnity).State = EntityState.Added;
                }

            }
            _database.SaveChanges();
        }

        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Get Budget Column List.
        /// </summary>                
        /// <param name="clientId">Client Id</param>                
        /// <returns>Return Budget Column List.</returns>
        public List<Budget_Columns> GetBudgetColumn(int clientId)
        {
            //get column of the Marketing budget.
            List<Budget_Columns> objColumns = new List<Budget_Columns>();
            objColumns = (from ColumnSet in _database.Budget_ColumnSet
                          join Columns in _database.Budget_Columns on ColumnSet.Id equals Columns.Column_SetId
                          where ColumnSet.IsDeleted == false && Columns.IsDeleted == false
                          && ColumnSet.ClientId == clientId
                          select Columns).ToList();
            return objColumns;
        }
        /// <summary>
        /// Added by Rahul Shah on 12/07/2016
        /// Method to Get Budget amount.
        /// </summary>                
        /// <param name="budgetDetailId">budget detail Id</param>
        /// <param name="allocationType">Type of allocation i.e(quarter, monthly, yearly)</param>
        ///  <param name="Period">period (i.e Jan, Feb.. etc)</param>
        ///  <returns>Return Budget amount object.</returns>
        private Budget_DetailAmount GetBudgetAmount(int budgetDetailId, string allocationType, string period)
        {
            Budget_DetailAmount objBudAmount = new Budget_DetailAmount();

            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

            if (string.Compare(allocationType, Enums.PlanAllocatedBy.quarters.ToString(), true) == 0)
            {
                if (period == "Q1")
                {
                    dataPeriod = "Y1";
                    QuaterPeriod = Q1;
                    objBudAmount = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && Q1.Contains(a.Period)).FirstOrDefault();
                }
                else if (period == "Q2")
                {
                    dataPeriod = "Y4";
                    QuaterPeriod = Q2;
                    objBudAmount = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && Q2.Contains(a.Period)).FirstOrDefault();
                }
                else if (period == "Q3")
                {
                    dataPeriod = "Y7";
                    QuaterPeriod = Q3;
                    objBudAmount = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && Q3.Contains(a.Period)).FirstOrDefault();
                }
                else if (period == "Q4")
                {
                    dataPeriod = "Y10";
                    QuaterPeriod = Q4;
                    objBudAmount = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && Q4.Contains(a.Period)).FirstOrDefault();
                }
            }
            else
            {
                period = Enums.ReportMonthDisplayValuesWithPeriod.Where(a => a.Key == period).Select(a => a.Value).FirstOrDefault();
                dataPeriod = period;
                objBudAmount = _database.Budget_DetailAmount.Where(a => a.BudgetDetailId == budgetDetailId && a.Period == period).FirstOrDefault();
            }

            return objBudAmount;
        }

        #region methods to bind  line items for budget
        #region "Finance: Get Parent & Child dropdown list for LineItem page "
        /// <summary>
        /// Get the list of Parent dropdown for LineItem screen
        /// </summary>
        /// <returns>Return the list of Budget Details list</returns>
        public LineItemDropdownModel GetParentLineItemBudgetDetailslist(int BudgetDetailId = 0, int ClientID=0)
        {
            ValidateBudget(BudgetDetailId, ClientID);
            List<Budget_Detail> lstBudgetDetails = new List<Budget_Detail>();
            lstBudgetDetails = _database.Budget_Detail.Where(a => a.Budget.ClientId == ClientID && a.IsDeleted == false).ToList();
            List<ViewByModel> lstParentItems = new List<ViewByModel>();
            LineItemDropdownModel objParentListModel = new LineItemDropdownModel();
            int? ParentId = 0, mostParentId = 0; // variable for parent and most parent for budget
            ParentId = lstBudgetDetails.Where(dtl => dtl.Id == BudgetDetailId).Select(dtl => dtl.ParentId).FirstOrDefault();
            mostParentId = lstBudgetDetails.Where(dtl => dtl.Id == ParentId).Select(dtl => dtl.ParentId).FirstOrDefault();
            var filterParentList = (from BD in lstBudgetDetails
                                    where BD.ParentId == mostParentId && BD.IsDeleted == false && !string.IsNullOrEmpty(BD.Name)
                                    select new { BD.Name, BD.Id }).Distinct().ToList();
            lstParentItems = filterParentList.Select(budget => new ViewByModel { Text = HttpUtility.HtmlEncode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();

            objParentListModel.list = lstParentItems;
            objParentListModel.parentId = ParentId.HasValue ? ParentId.Value : 0;
            return objParentListModel;
        }
        #endregion
        /// <summary>
        /// Get the list of Child dropdown for LineItem screen
        /// </summary>
        /// <returns>Return the list of Budget Details list</returns>
        public List<ViewByModel> GetChildLineItemBudgetDetailslist(int ParentBudgetDetailId = 0, int ClientId=0)
        {
            ValidateBudget(ParentBudgetDetailId, ClientId);
            List<ViewByModel> lstChildItems = new List<ViewByModel>();
            var filterChildList = (from detail1 in _database.Budget_Detail
                                   where detail1.ParentId == ParentBudgetDetailId && detail1.IsDeleted == false && !string.IsNullOrEmpty(detail1.Name) && detail1.Budget.ClientId == ClientId
                                   select new { detail1.Name, detail1.Id }).Distinct().ToList();
            lstChildItems = filterChildList.Select(budget => new ViewByModel { Text = HttpUtility.HtmlDecode(budget.Name), Value = budget.Id.ToString() }).OrderBy(bdgt => bdgt.Text, new AlphaNumericComparer()).ToList();
            return lstChildItems;
        }
        //Method to get line item grid for budget and time frame
        public LineItemDetail GetLineItemGrid(int BudgetDetailId,int ClientId, string IsQuaterly = "quarters", double PlanExchangeRate=1.0)
        {
            ValidateBudget(BudgetDetailId, ClientId);
            LineItemDetail AllLineItemDetail = new LineItemDetail();
            BudgetGridModel lineItemGridData = new BudgetGridModel();

            BudgetGridDataModel objLineItemGridDataModel = new BudgetGridDataModel();
            Budget_Detail objBudgetDetail = _database.Budget_Detail.Where(a => a.Id == (BudgetDetailId > 0 ? BudgetDetailId : a.BudgetId) && a.IsDeleted == false).FirstOrDefault();
            List<LineItem_Budget> LineItemidBudgetList = new List<LineItem_Budget>();
            if (objBudgetDetail == null)
                objBudgetDetail = new Budget_Detail();

            LineItemidBudgetList = _database.LineItem_Budget.Where(a => objBudgetDetail.Id == a.BudgetDetailId).Select(a => a).ToList();
            if (LineItemidBudgetList.Count() > 0)
            {
                LineItem_Budget LinkedTactic;
                for (int i = 0; i < LineItemidBudgetList.Count(); i++)
                {
                    LinkedTactic = new LineItem_Budget();
                    LinkedTactic = LineItemidBudgetList.Where(l => l.Plan_Campaign_Program_Tactic_LineItem.PlanLineItemId == LineItemidBudgetList[i].Plan_Campaign_Program_Tactic_LineItem.LinkedLineItemId).FirstOrDefault();
                    if (LinkedTactic != null && LineItemidBudgetList.Any(l => l == LinkedTactic))
                    {
                            LineItemidBudgetList.RemoveAt(i);
                            i--;
                        
                    }
                }
            }
            List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem> lstPlanLineItems = _database.Plan_Campaign_Program_Tactic_LineItem.Where(a => PlanLineItemBudgetDetail.Contains(a.PlanLineItemId) && a.IsDeleted == false).ToList();
            List<int> LineItemids = lstPlanLineItems != null ? lstPlanLineItems.Select(line => line.PlanLineItemId).ToList() : new List<int>();

            List<string> tacticStatus = Common.GetStatusListAfterApproved();

            List<Budget_DetailAmount> BudgetDetailAmount = _database.Budget_DetailAmount.Where(a => objBudgetDetail.Id == a.BudgetDetailId).Select(a => a).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in _database.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                 where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                 select Cost).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in _database.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                     join TacticLineItem in _database.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     join Tactic in _database.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                     where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                     && tacticStatus.Contains(Tactic.Status)
                                                                                     select Actual).ToList();

            #region "Create Child Data Model"

            #region "Declare local variables"
            List<BudgetGridRowModel> childModelList = new List<BudgetGridRowModel>();
            BudgetGridRowModel childModel = new BudgetGridRowModel();
            List<string> childData = new List<string>();
            BudgetAmount objBudgetAmount;
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> lstPlannedValue;
            List<Plan_Campaign_Program_Tactic_LineItem_Actual> lstActualValue;
            List<LineItem_Budget> lstLineItemBudget;
            double planVal = 0, actualVal = 0, forecstVal = 0, totalPlanned = 0, totalActual = 0;
            string strlineItemPopupUrl;
            int planId;
            #endregion

            #region "Foreach LineItem list to set Value into Model"
            foreach (Plan_Campaign_Program_Tactic_LineItem lineitem in lstPlanLineItems)
            {
                totalPlanned = totalActual = 0;
                childModel = new BudgetGridRowModel();
                childData = new List<string>();
                planId = 0;
                strlineItemPopupUrl = string.Empty;
                // Name,View,Forecast,Planned,Actual
                childData.Add(HttpUtility.HtmlDecode(lineitem.Title));
                planId = lineitem.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.PlanId;
                childData.Add(string.Format("#SEARCHHTML#_{0}_{1}", lineitem.PlanLineItemId, planId));
                //strlineItemPopupUrl = GetNotificationURLbyStatus(planId, lineitem.PlanLineItemId, Enums.Section.LineItem.ToString());
                //childData.Add(string.Format("<div id='{0}' onclick=OpenLineItemPopup('{1}') title='View' class='grid_Search'></div>", lineitem.PlanLineItemId, HttpUtility.HtmlDecode(strlineItemPopupUrl)));

                #region "Filter Planned Value"
                lstPlannedValue = new List<Plan_Campaign_Program_Tactic_LineItem_Cost>();
                lstPlannedValue = PlanDetailAmount.Where(plan => plan.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Filter Actual Value"
                lstActualValue = new List<Plan_Campaign_Program_Tactic_LineItem_Actual>();
                lstActualValue = ActualDetailAmount.Where(actual => actual.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Filter LineItem_Budget List"
                lstLineItemBudget = new List<LineItem_Budget>();
                lstLineItemBudget = LineItemidBudgetList.Where(line => line.PlanLineItemId == lineitem.PlanLineItemId).ToList();
                #endregion

                #region "Get Planned & Actual value based on Timeframe selected value"
                objBudgetAmount = new BudgetAmount();
                objBudgetAmount = GetAmountValue(IsQuaterly, new List<Budget_DetailAmount>(), lstPlannedValue, lstActualValue, lstLineItemBudget,PlanExchangeRate);
                #endregion

                if (objBudgetAmount != null)
                {
                    #region "Insert data as per timeframe wise"
                    int timeframeRowCount = objBudgetAmount.Plan != null ? objBudgetAmount.Plan.Count : 0;
                    for (int row = 0; row < timeframeRowCount; row++)
                    {
                        planVal = objBudgetAmount.Plan[row] != null ? Convert.ToDouble(objBudgetAmount.Plan[row]) : 0;
                        actualVal = objBudgetAmount.Actual[row] != null ? Convert.ToDouble(objBudgetAmount.Actual[row]) : 0;
                        totalPlanned = totalPlanned + planVal;
                        totalActual = totalActual + actualVal;
                        childData.Add("<div style='color: gray;'>0</div>"); // Insert Forecast Column Value
                        childData.Add(planVal.ToString(formatThousand));    // Insert Planned Column Value
                        childData.Add(actualVal.ToString(formatThousand));  // Insert Actual Column Value
                    }
                    #endregion
                }

                #region "Add Child Items Total values to Model"
                childData.Add("<div style='color: gray;'>0</div>");
                childData.Add(totalPlanned.ToString(formatThousand));    // Insert Planned Column Total Value
                childData.Add(totalActual.ToString(formatThousand));  // Insert Actual Column Total Value 
                #endregion

                #region "Set LinetItem Value in Model"
                childModel.id = lineitem.PlanLineItemId.ToString();
                childModel.data = childData;
                childModelList.Add(childModel);
                #endregion
            }
            #endregion

            #endregion

            #region "Create Parent Data Model"
            List<BudgetGridRowModel> parentModelList = new List<BudgetGridRowModel>();
            List<string> parentData = new List<string>();
            BudgetGridRowModel parentDataModel = new BudgetGridRowModel();
            double totalParentForecast = 0, totalParentPlanned = 0, totalParentActual = 0;
            parentData.Add(HttpUtility.HtmlDecode(objBudgetDetail.Name));   // Set TaskName column value
            parentData.Add(string.Empty);           // Set View column value
            objBudgetAmount = new BudgetAmount();
            objBudgetAmount = GetAmountValue(IsQuaterly, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount, LineItemidBudgetList,PlanExchangeRate);

            if (objBudgetAmount != null)
            {
                #region "Insert data as per timeframe wise"
                int timeframeRowCount = objBudgetAmount.Plan != null ? objBudgetAmount.Plan.Count : 0;

                for (int row = 0; row < timeframeRowCount; row++)
                {
                    forecstVal = objBudgetAmount.ForeCast[row] != null ? Convert.ToDouble(objBudgetAmount.ForeCast[row]) : 0;
                    planVal = objBudgetAmount.Plan[row] != null ? Convert.ToDouble(objBudgetAmount.Plan[row]) : 0;
                    actualVal = objBudgetAmount.Actual[row] != null ? Convert.ToDouble(objBudgetAmount.Actual[row]) : 0;

                    totalParentForecast = totalParentForecast + forecstVal;
                    totalParentPlanned = totalParentPlanned + planVal;
                    totalParentActual = totalParentActual + actualVal;

                    parentData.Add(forecstVal.ToString(formatThousand)); // Insert Forecast Column Value
                    parentData.Add(planVal.ToString(formatThousand));    // Insert Planned Column Value
                    parentData.Add(actualVal.ToString(formatThousand));  // Insert Actual Column Value
                }
                #endregion
            }

            #region "Add Child Items Total values to Model"
            parentData.Add(totalParentForecast.ToString(formatThousand));   // Insert Forecast Column Total Value
            parentData.Add(totalParentPlanned.ToString(formatThousand));    // Insert Planned Column Total Value
            parentData.Add(totalParentActual.ToString(formatThousand));  // Insert Actual Column Total Value 
            #endregion

            parentDataModel.id = BudgetDetailId.ToString();
            parentDataModel.data = parentData;
            parentDataModel.rows = childModelList;
            parentModelList.Add(parentDataModel);

            //ViewBag.HasLineItems = childModelList != null && childModelList.Count > 0 ? true : false;
            AllLineItemDetail.childLineItemCount = childModelList != null && childModelList.Count > 0 ? true : false;
            #endregion
            var temp = parentModelList.Where(a => a.id == Convert.ToString(BudgetDetailId)).Select(a => a.data).FirstOrDefault();
            FinanceModelHeaders objFinanceHeader = new FinanceModelHeaders();
            if (temp != null)
            {
                objFinanceHeader.Budget = 0;
                objFinanceHeader.Forecast = Convert.ToDouble(temp[temp.Count - 3]);
                objFinanceHeader.Planned = Convert.ToDouble(temp[temp.Count - 2]);
                objFinanceHeader.Actual = Convert.ToDouble(temp[temp.Count - 1]);
            }
            else
            {
                objFinanceHeader.Budget = objFinanceHeader.Forecast = objFinanceHeader.Planned = objFinanceHeader.Actual = 0;
            }
            objFinanceHeader.ActualTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Actual.ToString()].ToString();
            objFinanceHeader.ForecastTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Forecast.ToString()].ToString();
            objFinanceHeader.PlannedTitle = Enums.FinanceHeader_LabelValues[Enums.FinanceHeader_Label.Planned.ToString()].ToString();

            objLineItemGridDataModel.rows = parentModelList;
            objLineItemGridDataModel.FinanemodelheaderObj = objFinanceHeader;

            lineItemGridData.objGridDataModel = objLineItemGridDataModel;
            AllLineItemDetail.LineItemGridData = lineItemGridData;
            return AllLineItemDetail;

        }


        #region Get Budget/ForeCast/Plan/Actual Value
        public BudgetAmount GetAmountValue(string isQuaterly, List<Budget_DetailAmount> Budget_DetailAmountList, List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount, List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount, List<LineItem_Budget> LineItemidBudgetList, double PlanExchangeRate)
        {
            #region Declartion
            BudgetAmount objbudget = new BudgetAmount();
            List<double?> _budgetlist = new List<double?>();
            List<double?> _forecastlist = new List<double?>();
            List<double?> _planlist = new List<double?>();
            List<double?> _actuallist = new List<double?>();
            int currentEndMonth = 12;
            double? _Budget = 0, _ForeCast = 0, _Plan = 0, _Actual = 0;
            #endregion

            List<string> Q1 = new List<string>() { "Y1", "Y2", "Y3" };
            List<string> Q2 = new List<string>() { "Y4", "Y5", "Y6" };
            List<string> Q3 = new List<string>() { "Y7", "Y8", "Y9" };
            List<string> Q4 = new List<string>() { "Y10", "Y11", "Y12" };

            List<string> _curentBudget = new List<string>();
            List<string> _curentForeCast = new List<string>();
            List<string> _curentPlan = new List<string>();
            List<string> _curentActual = new List<string>();

            List<string> _commonQuarters = new List<string>();
            if (LineItemidBudgetList != null)
            {
                List<int> LineItemIds = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();
            }
            if (isQuaterly == Enums.PlanAllocatedBy.quarters.ToString())
            {
                #region "Get Quarter list based on loop value"
                for (int i = 1; i <= 4; i++)
                {
                    if (i == 1)
                    {
                        _curentBudget = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q1.Where(q1 => Convert.ToInt32(q1.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 2)
                    {
                        _curentBudget = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q2.Where(q2 => Convert.ToInt32(q2.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 3)
                    {
                        _curentBudget = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q3.Where(q3 => Convert.ToInt32(q3.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    else if (i == 4)
                    {
                        _curentBudget = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentForeCast = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentPlan = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                        _curentActual = Q4.Where(q4 => Convert.ToInt32(q4.Replace(PeriodPrefix, "")) <= Convert.ToInt32(currentEndMonth)).ToList();
                    }
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where _curentPlan.Contains(plandetail.Period)
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where _curentActual.Contains(actualdetail.Period)
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);
                    //Moidified by Rahul Shah for PL #2505 to Apply multicurrency.
                    _Budget = Budget_DetailAmountList.Where(a => _curentBudget.Contains(a.Period)).Sum(a => a.Budget);
                    _budgetlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Budget)), PlanExchangeRate));

                    _ForeCast = Budget_DetailAmountList.Where(a => _curentForeCast.Contains(a.Period)).Sum(a => a.Forecast);
                    _forecastlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_ForeCast)), PlanExchangeRate));
                    _planlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Plan)), PlanExchangeRate));
                    _actuallist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Actual)), PlanExchangeRate));
                }
                #endregion
            }
            else if (isQuaterly == Enums.PlanAllocatedBy.months.ToString())
            {
                for (int i = 1; i <= 12; i++)
                {
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where plandetail.Period == PeriodPrefix + i.ToString()
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where actualdetail.Period == PeriodPrefix + i.ToString()
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);
                    //Moidified by Rahul Shah for PL #2505 to Apply multicurrency.
                    _Budget = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Budget);
                    _budgetlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Budget)), PlanExchangeRate));
                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == PeriodPrefix + i.ToString()).Sum(a => a.Forecast);
                    _forecastlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_ForeCast)), PlanExchangeRate));
                    _planlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Plan)), PlanExchangeRate));
                    _actuallist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Actual)), PlanExchangeRate));
                }
            }
            else
            {
                if (isQuaterly == Enums.QuarterWithSpace.Quarter1.ToString())
                {
                    _commonQuarters = Q1;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter2.ToString())
                {
                    _commonQuarters = Q2;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter3.ToString())
                {
                    _commonQuarters = Q3;
                }
                else if (isQuaterly == Enums.QuarterWithSpace.Quarter4.ToString())
                {
                    _commonQuarters = Q4;
                }
                foreach (var item in _commonQuarters)
                {
                    _Plan = (from plandetail in PlanDetailAmount
                             join budgetweightage in LineItemidBudgetList on plandetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftplan
                             where plandetail.Period == item
                             from leftplanweightage in leftplan.DefaultIfEmpty()
                             select new
                             {
                                 Value = (plandetail.Value * (leftplanweightage == null ? 100 : leftplanweightage.Weightage)) / 100,
                             }).ToList().Sum(a => a.Value);

                    _Actual = (from actualdetail in ActualDetailAmount
                               join budgetweightage in LineItemidBudgetList on actualdetail.PlanLineItemId equals budgetweightage.PlanLineItemId into leftactual
                               where actualdetail.Period == item
                               from leftactualweightage in leftactual.DefaultIfEmpty()
                               select new
                               {
                                   Value = (actualdetail.Value * (leftactualweightage == null ? 100 : leftactualweightage.Weightage)) / 100,
                               }).ToList().Sum(a => a.Value);

                    _Budget = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Budget);
                    //Moidified by Rahul Shah for PL #2505 to Apply multicurrency.
                    _budgetlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Budget)), PlanExchangeRate));
                    _ForeCast = Budget_DetailAmountList.Where(a => a.Period == item).Sum(a => a.Forecast);
                    _forecastlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_ForeCast)), PlanExchangeRate));
                    _planlist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Plan)), PlanExchangeRate));
                    _actuallist.Add(_ObjCurrency.GetValueByExchangeRate(double.Parse(Convert.ToString(_Actual)), PlanExchangeRate));
                }
            }
            objbudget.Budget = _budgetlist;
            objbudget.ForeCast = _forecastlist;
            objbudget.Plan = _planlist;
            objbudget.Actual = _actuallist;
            return objbudget;
        }
        #endregion
        #endregion

        #region method to bind User Permission for budget
        public List<ViewByModel> GetChildBudget(int BudgetId = 0)
        {
            List<ViewByModel> lstchildbudget = Common.GetParentBudgetlist(BudgetId);
            if (lstchildbudget.Count > 0)
            {
                return lstchildbudget;
            }
            else
            {
                lstchildbudget.Add(new ViewByModel { Text = "Please Select", Value = "0" });
                return lstchildbudget;
            }
        }
        //Method to get list of user for budget
        public List<Budget_Permission> GetUserList(int BudgetId)
        {
            List<Budget_Permission> UserList = (from c in _database.Budget_Permission
                                                where c.BudgetDetailId == BudgetId
                                                orderby c.UserId
                                                select c).GroupBy(g => g.UserId).Select(x => x.FirstOrDefault()).ToList();
            return UserList;
        }
        //Method to get user permission list for budget
        public FinanceModel EditPermission(int BudgetId, Guid ApplicationId, List<Budget_Permission> UserList, int UserId)
        {
            FinanceModel objFinanceModel = new FinanceModel();
            BDSService.User objUser = new BDSService.User();

            UserPermission user = new UserPermission();
            List<BDSService.User> BDSuserList = _ServiceDatabase.GetMultipleTeamMemberDetailsEx(UserList.Select(x => x.UserId).ToList(), ApplicationId);
            List<UserPermission> _user = new List<UserPermission>();
            for (int i = 0; i < BDSuserList.Count; i++)
            {
                user = new UserPermission();
                user.budgetID = BudgetId;
                user.UserId = BDSuserList[i].ID;
                user.FirstName = BDSuserList[i].FirstName;
                user.LastName = BDSuserList[i].LastName;
                user.Role = BDSuserList[i].RoleTitle;
                user.Permission = UserList[i].PermisssionCode;
                user.createdby = UserList[i].CreatedBy.ToString();
                user.IsOwner = UserList[i].IsOwner;
                _user.Add(user);
            }
            objFinanceModel.Userpermission = _user.OrderBy(i => i.FirstName, new AlphaNumericComparer()).ToList();
            var index = _user.FindIndex(x => x.UserId == UserId);
            if (index != -1)
            {
                var item = _user[index];
                _user[index] = _user[0];
                _user[0] = item;
            }
            objFinanceModel.Userpermission = _user.ToList();
            return objFinanceModel;
        }
        //method to check permission for budget
        public string CheckUserPermission(int BudgetId, int ClientId, int UserId)
        {
            List<BDSService.User> lstUser = new List<BDSService.User>();
            lstUser = _ServiceDatabase.GetUserListByClientIdEx(ClientId).ToList();
            List<int> lstUserId = lstUser.Select(a => a.ID).ToList();
            List<Budget_Permission> ListOfUserPermission = _database.Budget_Permission.Where(a => lstUserId.Contains(a.UserId)).ToList();
            List<Budget_Permission> CheckUserPermission = ListOfUserPermission.Where(a => a.BudgetDetailId == (Int32)BudgetId && a.UserId == UserId).ToList();
            string EditView = "";
            string strUserAction = string.Empty;
            if (CheckUserPermission.Count > 0)
            {
                if (CheckUserPermission.First().PermisssionCode == 0)
                {
                    EditView = "Edit";
                }
                else
                {
                    EditView = "View";
                }
            }
            else
            {
                EditView = "View";
            }
            return EditView;
        }

        public List<UserPermission> FilterByBudget(int BudgetId, Guid ApplicationId)
        {
            List<UserPermission> _user = new List<UserPermission>();
            BDSService.User objUser = new BDSService.User();
            var UserList = (from c in _database.Budget_Permission
                            where c.BudgetDetailId == BudgetId
                            orderby c.UserId
                            select c).GroupBy(g => g.UserId).Select(x => x.FirstOrDefault()).ToList();

            for (int i = 0; i < UserList.Count; i++)
            {
                UserPermission user = new UserPermission();
                objUser = _ServiceDatabase.GetTeamMemberDetailsEx(UserList[i].UserId, ApplicationId);
                user.budgetID = BudgetId;
                user.UserId = objUser.ID;
                user.FirstName = objUser.FirstName;
                user.LastName = objUser.LastName;
                user.Role = objUser.RoleTitle;
                user.Permission = UserList[i].PermisssionCode;
                user.createdby = Convert.ToString(UserList[i].CreatedBy);
                user.IsOwner = UserList[i].IsOwner;
                _user.Add(user);
            }
            return _user;
        }
        public UserModel GetuserRecord(int Id, int UserId, Guid ApplicationId)
        {
            int uId = 0;
            if (Id == 0)
            {
                uId = UserId;
            }
            else
            {
                uId = Id;
            }
            BDSService.User objUser = new BDSService.User();
            UserModel objUserModel = new UserModel();

            objUser = _ServiceDatabase.GetTeamMemberDetailsEx(uId, ApplicationId);
            if (objUser != null)
            {
                objUserModel.DisplayName = objUser.DisplayName;
                objUserModel.Email = objUser.Email;
                objUserModel.Phone = objUser.Phone;
                objUserModel.FirstName = objUser.FirstName;
                objUserModel.JobTitle = objUser.JobTitle;
                objUserModel.LastName = objUser.LastName;
                objUserModel.UserId = objUser.ID;
                objUserModel.RoleTitle = objUser.RoleTitle;
            }
            return objUserModel;
        }
        //method to get all userlist for client for search in edit permission
        public List<BDSService.User> GetAllUserList(int ClientId, int UserId, Guid ApplicationId)
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            List<BDSService.User> lstUserDetail = objBDSServiceClient.GetTeamMemberListEx(ClientId, ApplicationId, UserId, true).ToList();

            return lstUserDetail;
        }
        //method to delete user for budget and its parent item
        public void DeleteUserForBudget(List<int> BudgetDetailIds, int UserID)
        {
            List<Budget_Permission> BudgetDetailList = _database.Budget_Permission.Where(i => BudgetDetailIds.Contains(i.BudgetDetailId) && i.UserId == UserID).ToList();
            //END
            foreach (var BudgetDetail in BudgetDetailList)
            {
                _database.Entry(BudgetDetail).State = EntityState.Deleted;
            }
            _database.SaveChanges();
        }
        //method to save user permission
        public void SaveUSerPermission(List<UserBudgetPermissionDetail> UserData, string ChildItems, string ParentID,  int UserId)
        {
            List<string> ListItems = new List<string>();
            if (ChildItems != "" && ChildItems != null)
            {
                ListItems = ChildItems.Split(',').ToList();

            }
            if (ParentID != "" && ParentID != null)
            {
                ListItems.Add(ParentID);
            }

            string ItemID = string.Empty;

            List<UserBudgetPermissionDetail> FinalUserData = new List<UserBudgetPermissionDetail>();
            Budget_Permission objBudget_Permission = new Budget_Permission();
            List<Budget_Permission> BudgetDetailList = _database.Budget_Permission.Select(list => list).ToList();
            Budget_Permission CurrentobjBudget_Permission = new Budget_Permission();
            List<UserBudgetPermissionDetail> TempUserData = new List<UserBudgetPermissionDetail>();

            for (int i = 0; i < ListItems.Count; i++)
            {

                if (ListItems[i].Contains("_"))
                {
                    ItemID = ListItems[i].Split('_')[1];
                }
                TempUserData = (from User in UserData
                                select new UserBudgetPermissionDetail
                                {
                                    BudgetDetailId = Convert.ToInt32(ItemID),
                                    PermisssionCode = User.PermisssionCode,
                                    UserId = User.UserId
                                }).ToList();
                FinalUserData.AddRange(TempUserData);
            }

            try
            {
                for (int i = 0; i < FinalUserData.Count; i++)
                {
                    _database.Configuration.AutoDetectChangesEnabled = false;
                    objBudget_Permission = new Budget_Permission();
                    int id = FinalUserData[i].UserId;
                    CurrentobjBudget_Permission = BudgetDetailList.Where(t => t.BudgetDetailId.Equals(FinalUserData[i].BudgetDetailId) && t.UserId.Equals(id)).FirstOrDefault();
                    if (CurrentobjBudget_Permission == null)
                    {
                        objBudget_Permission.UserId = FinalUserData[i].UserId;
                        objBudget_Permission.BudgetDetailId = Convert.ToInt32(FinalUserData[i].BudgetDetailId);
                        objBudget_Permission.CreatedBy = UserId;
                        objBudget_Permission.CreatedDate = DateTime.Now;
                        objBudget_Permission.PermisssionCode = FinalUserData[i].PermisssionCode;
                        _database.Entry(objBudget_Permission).State = EntityState.Added;
                    }
                    else
                    {
                        if (!CurrentobjBudget_Permission.IsOwner)
                        {
                            CurrentobjBudget_Permission.UserId = FinalUserData[i].UserId;
                            CurrentobjBudget_Permission.BudgetDetailId = Convert.ToInt32(FinalUserData[i].BudgetDetailId);
                            CurrentobjBudget_Permission.CreatedDate = DateTime.Now;
                            CurrentobjBudget_Permission.PermisssionCode = FinalUserData[i].PermisssionCode;
                            _database.Entry(CurrentobjBudget_Permission).State = EntityState.Modified;
                        }



                    }

                }
            }
            finally
            {
                _database.Configuration.AutoDetectChangesEnabled = true;
            }
            _database.SaveChanges();
        }

        #endregion

//method to validate budget for cross client
        private void ValidateBudget(int BudgetDetailId, int ClientID)
        {
           Budget_Detail BudgetDetail = _database.Budget_Detail.Where(a => a.Id == BudgetDetailId).SingleOrDefault();
           if (BudgetDetail == null || BudgetDetail.Budget.ClientId != ClientID) throw new Exception(string.Format("BudgetDetailId: {0} not valid", BudgetDetailId));
        }

    }
}
