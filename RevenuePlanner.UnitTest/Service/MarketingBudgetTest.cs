using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;
using RevenuePlanner.Services.MarketingBudget;
using System.Data;
using System.IO;
using System.Reflection;
using System.Data.OleDb;

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
	    private const string Importviewby="Yearly";//TimeFrame for import budget
        private const string ImportXLSFileLocation = "\\ImportTest_Data\\grid_MarketingBudget.xls"; // xls file location for import marketing budget
        private const string ImportXLSX_FileLocation = "\\ImportTest_Data\\grid_MarketingBudgetXLSXFile.xlsx"; // xlsx file location for import marketing budget
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
            // Get header values 
            DataTable headerValues = _marketingBudget.GetFinanceHeaderValues(BudgetId, ExchangeRate);

            Assert.IsTrue(headerValues.Rows.Count > 0);
            Assert.IsTrue(headerValues.Columns.IndexOf("Budget") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Forecast") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Planned") > -1);
            Assert.IsTrue(headerValues.Columns.IndexOf("Actual") > -1);
        }
		    /// <summary>
        /// Test case for import marketing budget with .XLS file.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_Import()
        {
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
            var res = _marketingBudget.GetXLSData(Importviewby, ds,ClientId, BudgetDetailId, 1);
            Assert.IsTrue(res != null);

        }
        /// <summary>
        /// Test case for import marketing budget with .XLSX file.
        /// </summary>
        [TestMethod]
        public void Test_MarketingBudget_Import_XLSXFile()
        {
            string FileLocation = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + ImportXLSX_FileLocation;
            var res = _marketingBudget.GetXLSXData(Importviewby, FileLocation, ClientId, BudgetDetailId, 1);
            Assert.IsTrue(res!=null);

        }
      

    }
    

    /// <summary>
    /// This extension makes code more readable!
    /// </summary>

}
