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
        private IBDSService _ServiceDatabase;
        private ICurrency _ObjCurrency= new Currency();
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

        public List<Budget_Columns> GetColumns(int ColumnSetId)
        {
            List<Budget_Columns> BudgetColumns = _database.Budget_Columns.Where(a => a.Column_SetId == ColumnSetId && a.IsDeleted == false).ToList();
            return BudgetColumns;
        }


        /// <summary>
        /// Method to get marketing budget grid data for perticular budget
        /// </summary>
        
        public BudgetGridModel GetBudgetGridData(int budgetId, string viewByType, BudgetColumnFlag columnsRequested, int ClientID, int UserID, double Exchangerate, string CurSymbol,List<BDSService.User> lstUser)
        {

            BudgetGridModel objBudgetGridModel = new BudgetGridModel();
            BudgetGridDataModel objBudgetGridDataModel = new BudgetGridDataModel();
            List<int> lstUserId = lstUser.Select(a => a.ID).ToList();
            string CommaSeparatedUserIds = String.Join(",", lstUserId);
            List<string> CustomColumnNames = new List<string>();
            //Call Sp to get data.
            DataSet BudgetGridData = GetBudgetDefaultData(budgetId, viewByType,ClientID, UserID, CommaSeparatedUserIds, Exchangerate);

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
            lstData = GetTopLevelRows(StandardColumnTable, null)
                        .Select(row => CreateHierarchyItem(BudgetGridData, row, CustomColumnNames, StandardColumnNames, lstUser, CurSymbol)).ToList();

            objBudgetGridDataModel.head = objBudgetGridModel.GridDataStyleList;
            objBudgetGridDataModel.rows = lstData;
            objBudgetGridModel.objGridDataModel = objBudgetGridDataModel;

            return objBudgetGridModel;
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
        IEnumerable<DataRow> GetTopLevelRows(DataTable StandardColumnTable, int? minParentId)
        {
            return StandardColumnTable
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
                else if (ColumnName == Enums.DefaultGridColumn.Users.ToString())
                {
                    if (Permission == "View" || Permission == "None")
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
                    if (Permission == "None" && ColumnName != Enums.DefaultGridColumn.BudgetDetailId.ToString())
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
                            if (ColumnName != Enums.DefaultGridColumn.BudgetDetailId.ToString())
                            {
                                DisplayValue = string.Format("{0}{1}", CurSymbol, value.ToString(formatThousand));
                            }
                            else
                            {
                                DisplayValue = Convert.ToString(objValue);
                            }

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

        public DataSet GetBudgetDefaultData(int budgetId, string timeframe, int ClientID, int UserID, string CommaSeparatedUserIds, double Exchangerate)
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
                        headObj.value = "Id";
                    }
                    else
                    {
                        headObj.value = string.Empty;
                        sbAttachedHeaders.Append("Id,");
                    }

                    headObj.sort = "na";
                    headObj.width = 100;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = "Id";
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
                    headObj.width = 200;
                    headObj.align = "left";
                    headObj.type = "tree";
                    headObj.id = columns;
                    ListHead.Add(headObj);


                    //Add icons column after name
                    headObj = new GridDataStyle();
                    headObj.value = string.Empty;
                    headObj.sort = "na";
                    headObj.width = 50;
                    headObj.align = "center";
                    headObj.type = "ro";
                    headObj.id = "Add Row";
                    ListHead.Add(headObj);

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
                    headObj.width = 100;
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

            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileLocation, false))
            {
                //Read the first Sheet from Excel file.
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                //Get the Worksheet instance.
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                //Fetch all the rows present in the Worksheet.
                IEnumerable<Row> rowsData = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                var RowsListData = rowsData.ToList();
                RowsListData.Remove(RowsListData.LastOrDefault());

                if (viewByType == Enums.QuarterFinance.Yearly.ToString())
                {
                    RowsListData.Insert(0, RowsListData[0]);
                }
                var rows = RowsListData;
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
                                            var InnerColName = Convert.ToString(dtExcel.Rows[i][k]);
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

            var IsMonthly = (from dtMonth in MonthList
                             join defaultMonth in DefaultMonthList on dtMonth equals defaultMonth
                             select new { dtMonth }).Any();
            //end
            BudgetDetailId = _database.Budget_Detail.Where(a => a.BudgetId == BudgetDetailId).Select(a => a.Id).FirstOrDefault();

            ///If connection is closed then it will be open
            var Connection = _database.Database.Connection as SqlConnection;
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
        public MarketingBudgetHeadsUp GetFinanceHeaderValues(int BudgetId, double ExchangeRate,List<BDSService.User> lstUser)
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
            calResultset = _database.Database
                .SqlQuery<MarketingBudgetHeadsUp>("GetHeaderValuesForFinance @BudgetId,@lstUserIds,@CurrencyRate", para).FirstOrDefault();
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
                        SaveUserBudgetpermission(_budgetid,UserId);

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
        public void SaveUserBudgetpermission(int budgetId,int UserId)
        {
            _database.SaveuserBudgetPermission(budgetId, 0, UserId); //Sp to get users of all parent ids and assign to current budget item.

        }

        #endregion
    }
}