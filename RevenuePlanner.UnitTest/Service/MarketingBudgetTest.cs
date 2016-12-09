using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.MarketingBudget;
using System.Data;
using System.IO;
using System.Reflection;
using System.Data.OleDb;
using System.Linq;
using System;
using RevenuePlanner.Models;


namespace RevenuePlanner.UnitTest.Service
{
    [TestClass]
    public class MarketingBudgetTest
    {
        private IMarketingBudget _marketingBudget;

        #region Test Data 
        private const int ClientId = 24; //demo client     
        private const string BudgetTitle = "ZebraAdmin";
        private const int BudgetId = 80;
        private const int BudgetDetailId = 752;
        private const double ExchangeRate = 1.0; // Currency exchange rate
        private const string Importviewby = "Yearly";//TimeFrame for import budget
        private const string ImportXLSFileLocation = "\\ImportTest_Data\\grid_MarketingBudget.xls"; // xls file location for import marketing budget
        private const string ImportXLSX_FileLocation = "\\ImportTest_Data\\grid_MarketingBudgetXLSXFile.xlsx"; // xlsx file location for import marketing budget
        private Guid ApplicationId =  new Guid("1c10d4b9-7931-4a7c-99e9-a158ce158951");
        private const int UserId = 627;
        #endregion Test Data

        public MarketingBudgetTest()
        {
            _marketingBudget = ObjectFactory.GetInstance<IMarketingBudget>();
        }

        [TestMethod]
        public void Test_MarketingBudget_GetBudgetlist()
        {
            var res = _marketingBudget.GetBudgetlist(ClientId);
            Assert.IsTrue(res.Count > 0);
        }

        public void Test_MarketingBudget_DeleteBudget()
        {
            var res = _marketingBudget.DeleteBudget(BudgetDetailId, ClientId);
            Assert.IsTrue(res >= 0);
        }

        /// <summary>
        /// Test case of the method of getting finance header values
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_GetFinanceHeaderValues()
        {
            // Get users list for current client
            List<BDSService.User> lstUsers = _marketingBudget.GetUserListByClientId(ClientId);

            // Get header values 
            MarketingBudgetHeadsUp headerValues = _marketingBudget.GetFinanceHeaderValues(BudgetId, ExchangeRate, lstUsers);

            Assert.IsNotNull(headerValues);
            Assert.IsNotNull(headerValues.Budget);
            Assert.IsNotNull(headerValues.Forecast);
            Assert.IsNotNull(headerValues.Planned);
            Assert.IsNotNull(headerValues.Actual);
        }
        /// <summary>
        /// Test case for import marketing budget with .XLS file.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_Import()
        {
            Console.WriteLine("To Import Marketing budget with .XLS file.\n");
            //read excel file from path for import and return dataset
            OleDbConnection ExcelConnection; DataSet ds; OleDbDataAdapter Command;
            var ExcelPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + ImportXLSFileLocation;
            var path = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ExcelPath + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
            ExcelConnection = new OleDbConnection(path);
            ExcelConnection.Open();
            string sheetName = "[First_Sheet$]";
            Command = new OleDbDataAdapter("select * from " + sheetName, ExcelConnection);
            Command.TableMappings.Add("Table", "TestTable");
            ds = new DataSet();
            Command.Fill(ds);
            ExcelConnection.Close();
            //end
            var res = _marketingBudget.GetXLSData(Importviewby, ds, ClientId, BudgetDetailId, 1);
            Assert.IsTrue(res != null);
            if (string.IsNullOrEmpty(res.ErrorMsg))
            {
                Assert.IsTrue(res.MarketingBudgetColumns != null && res.MarketingBudgetColumns.Columns.Count > 0);
                Assert.IsTrue(res.XmlData != null);

            }

        }
        /// <summary>
        /// Test case for import marketing budget with .XLSX file.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_Import_XLSXFile()
        {
            Console.WriteLine("To Import Marketing budget with .XLSX file.\n");
            string FileLocation = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + ImportXLSX_FileLocation;
            var res = _marketingBudget.GetXLSXData(Importviewby, FileLocation, ClientId, BudgetDetailId, 1);
            Assert.IsTrue(res != null);
            if (string.IsNullOrEmpty(res.ErrorMsg))
            {
                Assert.IsTrue(res.MarketingBudgetColumns != null && res.MarketingBudgetColumns.Columns.Count > 0);
                Assert.IsTrue(res.XmlData != null);

            }
        }

			 /// <summary>
        /// Test Case to save new budget.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_SaveNewBudget()
        {
            Console.WriteLine("To Save New Budget.\n");
            int res = _marketingBudget.SaveNewBudget("Test Budget", 30, 627);
            Assert.IsTrue(res >= 0);

            var delres = _marketingBudget.DeleteBudget(res, ClientId);
            Assert.IsTrue(delres >= 0);
        }

        /// <summary>
        /// Test Case to save budget with empty name.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_SaveNewBudgetWithemptybudgetname()
        {
            Console.WriteLine("To Save New Budget With Empty Value.\n");
            int res = _marketingBudget.SaveNewBudget(string.Empty, 30, 627);
            Assert.AreEqual(0, res);
        }

        /// <summary>
        /// Test Case to save budget detail when we add new item/child item.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_SaveNewItemBudgetDetail()
        {
            Console.WriteLine("To Save New Budget Detail Item.\n");

            int BudgetID = _marketingBudget.SaveNewBudget("Budget Test", 30, 627);

            DataSet BudgetDetails = _marketingBudget.GetBudgetDefaultData(BudgetID, "yearly", 30, 627, "627", 0);
            DataTable BudgetDetailTable = BudgetDetails.Tables[0];
            int DataCount = BudgetDetailTable.Rows.Count;

            DataRow ParentRow = BudgetDetailTable.Rows.Cast<DataRow>()
                                       .Where(row => row.Field<Nullable<Int32>>("ParentId") == null).FirstOrDefault();
            int BudgetDetailID = Convert.ToInt32(ParentRow["BudgetDetailID"]);
            _marketingBudget.SaveNewBudgetDetail(BudgetID, "Item 1", BudgetDetailID, 30, 627);

            DataSet BudgetDetailsafterSaving = _marketingBudget.GetBudgetDefaultData(BudgetID, "yearly", 30, 627, "627", 0);
            int DataCountAfterSaving = BudgetDetailsafterSaving.Tables[0].Rows.Count;

            Assert.IsTrue(DataCountAfterSaving > DataCount);

            var res = _marketingBudget.DeleteBudget(BudgetDetailID, ClientId);
            Assert.IsTrue(res >= 0);

        }



        [TestMethod]
        public void Test_MarketingBudget_GetColumnSet()
        {
            Console.WriteLine("To Get Columnset Values.\n");
            var res = _marketingBudget.GetColumnSet(ClientId);
            Assert.IsTrue(res.Count > 0);
        }

        [TestMethod]
        public void Test_MarketingBudget_Columns()
        {
            Console.WriteLine("To get Columns based on column set id.\n");
            var result = _marketingBudget.GetColumnSet(ClientId);
            Assert.IsTrue(result.Count > 0);

            string ColumnSetID = result.SingleOrDefault().Value;
            var res = _marketingBudget.GetColumns(Convert.ToInt32(ColumnSetID));
            Assert.IsTrue(res.Count > 0);
        }

        [TestMethod]
        public void Test_MarketingBudget_GetBudgetGridData()
        {
            Console.WriteLine("To get budget grid data.\n");
            // Get users list for current client
            List<BDSService.User> lstUsers = _marketingBudget.GetUserListByClientId(30);

            int res = _marketingBudget.SaveNewBudget("Test Budget", 30, 627);
            Assert.IsTrue(res >= 0);

            BudgetGridModel Data = _marketingBudget.GetBudgetGridData(res, "yearly", 30, 627, 1, "$", lstUsers);
            Assert.IsNotNull(Data);
            Assert.IsTrue(Data.objGridDataModel.rows.Count > 0);
            Assert.IsTrue(Data.GridDataStyleList.Count > 0);

            var delres = _marketingBudget.DeleteBudget(res, ClientId);
            Assert.IsTrue(delres >= 0);

        }

        
        [TestMethod]
        public void Test_MarketingBudget_GetUserList()
        {
            Console.WriteLine("To get particular budget's user list.\n");

            List<Budget_Permission> Data = _marketingBudget.GetUserList(BudgetId);
            Assert.IsNotNull(Data);
            Assert.IsTrue(Data.Count > 0);
        }
        [TestMethod]
        public void Test_MarketingBudget_EditPermission()
        {
            Console.WriteLine("To get User permission detail for budget.\n");
            List<Budget_Permission> UserData = _marketingBudget.GetUserList(BudgetId);
            RevenuePlanner.Services.MarketingBudget.FinanceModel Data = _marketingBudget.EditPermission(BudgetId, ApplicationId, UserData,UserId);
            Assert.IsNotNull(Data);
            Assert.IsTrue(Data.Userpermission.Count > 0);
        }
        [TestMethod]
        public void Test_MarketingBudget_CheckUserPermission()
        {
            Console.WriteLine("To Check user permission for budget.\n");
            string Data = _marketingBudget.CheckUserPermission(BudgetId, ClientId, UserId);
            Assert.IsNotNull(Data);
        }
        [TestMethod]
        public void Test_MarketingBudget_FilterByBudget()
        {
            Console.WriteLine("To get user as per filter budget.\n");
            List<RevenuePlanner.Services.MarketingBudget.UserPermission> Data = _marketingBudget.FilterByBudget(BudgetId, ApplicationId);
            Assert.IsNotNull(Data);
            Assert.IsTrue(Data.Count > 0);
        }
        [TestMethod]
        public void Test_MarketingBudget_GetuserRecord()
        {
            Console.WriteLine("To get user record.\n");
            UserModel Data = _marketingBudget.GetuserRecord(0,UserId, ApplicationId);
            Assert.IsNotNull(Data);
            Assert.IsNotNull(Data.Email);
        }
        
        [TestMethod]
        public void Test_MarketingBudget_GetParentLineItemBudgetDetailslist()
        {
            Console.WriteLine("To get parent Line item detail for budget to bind dropdown.\n");
            RevenuePlanner.Services.MarketingBudget.LineItemDropdownModel Data = _marketingBudget.GetParentLineItemBudgetDetailslist(BudgetDetailId);
            Assert.IsNotNull(Data);
            Assert.IsNotNull(Data.parentId);
        }
        public void Test_MarketingBudget_GetBudgetColumn()
        {
            var res = _marketingBudget.GetBudgetColumn(ClientId);
            Assert.IsTrue(res.Count > 0);
        }
    }
    


    /// <summary>
    /// This extension makes code more readable!
    /// </summary>

}
