using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using RevenuePlanner.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using RevenuePlanner.Test.MockHelpers;
using System.Data;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System.Reflection;
using System.IO;

namespace RevenuePlanner.Test.QA_Helpers
{

    public class Hive9CommonFunctions
    {
        public TestContext TestContext { get; set; }
        LoginController objLoginController;
        int PlanId;

        public Hive9CommonFunctions()
        {
            HttpContext.Current = MockHelpers.MockHelpers.FakeHttpContext();
            objLoginController = new LoginController();
            objLoginController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objLoginController);
            objLoginController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            PlanId = DataHelper.GetPlanId();
        }

        //Read excel file and returns data
        public DataSet GetExcelData(string connectionString, string sheetName)
        {
            try
            {
                OleDbConnection ExcelConnection; DataSet ds; OleDbDataAdapter Command;
                var ExcelPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" + ConfigurationManager.AppSettings.Get(connectionString);
                var path = connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ExcelPath + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                ExcelConnection = new OleDbConnection(path);
                ExcelConnection.Open();
                Command = new OleDbDataAdapter("select * from " + sheetName, ExcelConnection);
                Command.TableMappings.Add("Table", "TestTable");
                ds = new DataSet();
                Command.Fill(ds);
                ExcelConnection.Close();
                return ds;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public RedirectToRouteResult CheckLogin()
        {
            try
            {
                DataSet ds = GetExcelData("LoginExcelConn", "[LogIn$]");
                string UserEmail = ds.Tables[0].Rows[0]["USER"].ToString();
                string Password = ds.Tables[0].Rows[0]["Password"].ToString();

                //Set user permission 
                HttpContext.Current = QA_DataHelper.SetUserAndPermission(true, UserEmail, Password);

                DataTable dt = GetExcelData("GamePlanExcelConn", "[Plan$]").Tables[0];
                if (dt.Rows[0]["PlanId"].ToString() != "")
                {
                    PlanId = Convert.ToInt32(dt.Rows[0]["PlanId"].ToString());
                }
                Sessions.User = new BDSService.User();
                Sessions.User.CID = DataHelper.GetClientId(PlanId);
                string returnURL = string.Empty;

                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(UserEmail))
                {
                    LoginModel login = new LoginModel();
                    login.Password = Password;
                    login.UserEmail = UserEmail;
                    return objLoginController.Index(login, returnURL) as RedirectToRouteResult;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
