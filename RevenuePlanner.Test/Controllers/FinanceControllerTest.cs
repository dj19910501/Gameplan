using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class FinanceControllerTest //: CommonController
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        #region Finance view Related Function

        #region Finance page with no parameters
        /// <summary>
        /// To check to retrieve Finance view with no parameters
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Get_Finance_View_With_No_Parameters()
        {
            Console.WriteLine("To check to retrieve Finance view with no parameters.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.Index() as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);

        }

        #endregion

        #endregion

        #region Create Budget Related Function

        #region Create new Budget with Empty Budget name

        /// <summary>
        /// To check to budget creating with Empty Budget name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Create_Budget_With_Empty_Budget_Name()
        {
            Console.WriteLine("To check to budget creating with Empty Budget name.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.CreateNewBudget("") as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }

        #endregion

        //Commented by Rahul Shah. bcz its negative test 
        //#region Create new Budget with Null Budget name

        ///// <summary>
        ///// To check to budget creating with Null Budget name
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>

        //[TestMethod]
        //public void Create_Budget_With_Null_Budget_Name()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

        //    //// Call CreateNewBudget
        //    FinanceController objFinanceController = new FinanceController();
        //    var result = objFinanceController.CreateNewBudget(null) as ViewResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result);
        //    }

        //}

        //#endregion

        #endregion

        #region Save Budget Related Function

        #region Save Budget Details with Valid Parameter

        /// <summary>
        /// save budget details
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Valid_Parameter()
        {
            Console.WriteLine("save budget details.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = Convert.ToString(DataHelper.GetBudgetDetailId());
            string BudgetDetailName = "Test" + DateTime.Now;
            string ParentId = Convert.ToString(DataHelper.GetBudgetDetailParentId(int.Parse(BudgetId)));
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetId, BudgetDetailName, ParentId, "Yearly", true) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }

        #endregion

        #region Save Budget Details with Null Values

        /// <summary>
        /// Save Budget Details with Null Values
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Null_Value()
        {
            Console.WriteLine("Save Budget Details with Null Values.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(null, null, null, "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);

            Assert.IsNull(result);

        }

        #endregion

        #region Save Budget Details with Null BudgetId

        /// <summary>
        /// Save Budget Details with Null BudgetId
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Null_BudgetId()
        {
            Console.WriteLine("Save Budget Details with Null BudgetId.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.SaveNewBudgetDetail(null, BudgetDetailName, ParentId, "", true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);

            Assert.IsNull(result);

        }

        #endregion


        #region Save Budget Details with Null Budget Name

        /// <summary>
        /// Save Budget Details with Null Budget Name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Null_BudgetName()
        {
            Console.WriteLine("Save Budget Details with Null Budget Name.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetID = "59";

            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetID, null, ParentId, "", true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }

        #endregion

        #region Save Budget Details with Empty ParentId

        /// <summary>
        /// Save Budget Details with Empty ParentID
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_ParentID()
        {
            Console.WriteLine("Save Budget Details with Empty ParentID.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            string BudgetDetailName = "Test";

            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail("", BudgetDetailName, "", null, true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }

        #endregion

        //Commented by Rahul shah. Bcz it negative testcases

        //#region Save Budget Details with Empty Values

        ///// <summary>
        ///// Save Budget Details with Empty Values
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>

        //[TestMethod]
        //public void Save_Budget_Details_with_Empty_Value()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

        //    //// Call SaveNewBudgetDetail
        //    FinanceController objFinanceController = new FinanceController();
        //    var result = objFinanceController.SaveNewBudgetDetail("", "", "", "") as JsonResult;
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result);
        //    }

        //}

        //#endregion

        #region Save Budget Details with Empty BudgetId

        /// <summary>
        /// Save Budget Details with Empty BudgetId
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_BudgetId()
        {
            Console.WriteLine("Save Budget Details with Empty BudgetId.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail("", BudgetDetailName, ParentId, "quaterly", true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }

        #endregion


        #region Save Budget Details with Empty Budget Name

        /// <summary>
        /// Save Budget Details with Empty Budget Name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_BudgetName()
        {
            Console.WriteLine("Save Budget Details with Empty Budget Name.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";

            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetId, "", ParentId, "", true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }

        #endregion

        #endregion

        #region Main Grid Related Function

        #region Refresh Main Grid Data With Empty Value
        /// <summary>
        /// To check to refresh main grid with Empty Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Refresh_MainGrid_With_Empty_Value()
        {
            Console.WriteLine("To check to refresh main grid with Empty Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(0, "") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion

        #region Refresh Main Grid Data With Empty SelectedRowID
        /// <summary>
        /// To check to refresh main grid with Null SelectedRowID
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Refresh_MainGrid_With_Empty_SelectedRowID()
        {
            Console.WriteLine("To check to refresh main grid with Null SelectedRowID.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(0, "Quaterly") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);

            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Refresh Main Grid Data With Null SelectedRowID
        /// <summary>
        /// To check to refresh main grid with Null SelectedRowID
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Refresh_MainGrid_With_Null_SelectedRowID()
        {
            Console.WriteLine("To check to refresh main grid with Null SelectedRowID.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(0, null) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }
        #endregion

        #region Refresh Main Grid Data With Invalid SelectedRowID
        /// <summary>
        /// To check to refresh main grid with Invalid SelectedRowID
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Refresh_MainGrid_With_Invalid_SelectedRowID()
        {
            Console.WriteLine("To check to refresh main grid with Invalid SelectedRowID.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(0, "Quaterly") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion

        #region Refresh Main Grid Data With Valid Parameteres
        /// <summary>
        /// To check to refresh main grid with Valid Parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Refresh_MainGrid_With_Valid_Parameter()
        {
            Console.WriteLine("To check to refresh main grid with Valid Parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(9, "Quaterly") as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        //#region get Finance Main Grid Data With Valid Values
        ///// <summary>
        ///// To check to refresh main grid with Valid Values
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>
        //[TestMethod]
        //public void Get_Finance_MainGrid_With_Valid_Values()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int budgetId = 0;
        //    string mainTimeFrame = "Yearly";
        //    //// Call CreateNewBudget
        //    FinanceController objFinanceController = new FinanceController();
        //    DhtmlXGridRowModel objDhtmlXGridRowModel = new DhtmlXGridRowModel();

        //    objDhtmlXGridRowModel = objFinanceController.GetFinanceMainGridData(budgetId, mainTimeFrame);
        //    if (objDhtmlXGridRowModel != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(objDhtmlXGridRowModel);
        //    }

        //}
        //#endregion

        //#region get Finance Main Grid Data With Null Values
        ///// <summary>
        ///// To check to refresh main grid with Null Values
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>
        //[TestMethod]
        //public void Get_Finance_MainGrid_With_Null_Values()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int budgetId = 0;
        //    string mainTimeFrame = "Yearly";
        //    //// Call CreateNewBudget
        //    FinanceController objFinanceController = new FinanceController();
        //    DhtmlXGridRowModel objDhtmlXGridRowModel = new DhtmlXGridRowModel();

        //    objDhtmlXGridRowModel = objFinanceController.GetFinanceMainGridData(budgetId, null);
        //    if (objDhtmlXGridRowModel != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(objDhtmlXGridRowModel);
        //    }

        //}
        //#endregion

        #endregion

        #region Finance Header Value Related Function

        #region Get Finance Header Value Without Parameter
        /// <summary>
        /// To Get Finance Header Value With Without Parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Finance_Header_Value_With_Without_Parameter()
        {
            Console.WriteLine("To Get Finance Header Value With Without Parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }
        #endregion

        #region Get Finance Header Value With Empty Value
        /// <summary>
        /// To Get Finance Header Value With Empty Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Finance_Header_Value_With_Empty_Value()
        {
            Console.WriteLine("To Get Finance Header Value With Empty Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(0, "", "", false) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }
        #endregion

        #region Get Finance Header Value With Null Value
        /// <summary>
        /// To Get Finance Header Value With Null Value    
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Finance_Header_Value_With_Null_Value()
        {
            Console.WriteLine("To Get Finance Header Value With Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(0, null, null, true) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);

            Assert.IsNull(result);

        }
        #endregion

        #region Get Finance Header Value With valid Value
        /// <summary>
        /// To Get Finance Header Value With Valid Value    
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Finance_Header_Value_With_Valid_Value()
        {
            Console.WriteLine("To Get Finance Header Value With Valid Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 59;
            string timeFrameOption = "";
            string isQuarterly = "Quarterly";
            bool IsMain = true;
            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(budgetId, timeFrameOption, isQuarterly, IsMain) as PartialViewResult;
            var modelData = (FinanceModelHeaders)result.Model;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.IsNotNull(result);

        }
        #endregion

        #endregion

        #region Get DropDown Value

        #region Get Child Budget DropDown Value with Valid Parameter
        /// <summary>
        /// To Get Child Budget DropDown Value with Valid Parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Child_Budget_With_Valid_Parameter()
        {
            Console.WriteLine("To Get Child Budget DropDown Value with Valid Parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetChildBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildBudget(budgetId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get Child Budget DropDown Value With No Parameter
        /// <summary>
        /// To Get Child Budget DropDown Value without parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Child_Budget_With_No_Parameter()
        {
            Console.WriteLine("To Get Child Budget DropDown Value without parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetChildBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildBudget() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Get Parent Budget DropDown Value With Valid Parameter
        /// <summary>
        /// To Get Parent Budget DropDown Value With Valid Parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Parent_Budget_With_Valid_Parameter()
        {
            Console.WriteLine("To Get Child Budget DropDown Value without parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetParentBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetParentBudget() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Get Parent Budget DropDown Value with no paramter
        /// <summary>
        /// To Get Parent Budget DropDown Value without Parameter
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Parent_Budget_With_No_Parameter()
        {
            Console.WriteLine("To Get Parent Budget DropDown Value without Parameter.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetParentBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetParentBudget(budgetId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Get Child TimeFrame
        /// <summary>
        /// To Get Child Time Frame
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Child_TimeFrame()
        {
            Console.WriteLine("To Get Child Time Frame.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetChildTimeFrame method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildTimeFrame() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #endregion

        #region Edit/Budget Forecast Griddata

        #region Edit Budget/Forecast Griddata with Valid Value
        /// <summary>
        /// To edit Budget/Forecast Griddata with Valid Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Edit_BudgetForecast_Griddata_With_Valid_Value()
        {
            Console.WriteLine("To edit Budget/Forecast Griddata with Valid Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;

            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(budgetId, IsQuaterly, string.Empty) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Edit Budget/Forecast Griddata with Empty Value
        /// <summary>
        /// To edit Budget/Forecast Griddata with Empty Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Edit_BudgetForecast_Griddata_With_Empty_Value()
        {
            Console.WriteLine("To edit Budget/Forecast Griddata with Empty Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(0, "", "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Edit Budget/Forecast Griddata with Null Value
        /// <summary>
        /// To edit Budget/Forecast Griddata with Null Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Edit_BudgetForecast_Griddata_With_Null_Value()
        {
            Console.WriteLine("To edit Budget/Forecast Griddata with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(0, "", "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Edit Budget/Forecast Griddata with Empty selectedrow Data
        /// <summary>
        /// To edit Budget/Forecast Data with Empty selected Row data
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Edit_BudgetForecast_Griddata_With_Empty_SelectedRowId()
        {
            Console.WriteLine("To edit Budget/Forecast Data with Empty selected Row data.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(budgetId, IsQuaterly, "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);


        }
        #endregion

        #endregion

        #region Update Budget/Forecast Griddata

        #region Update Budget/Forecast Griddata With Valid Value
        /// <summary>
        /// To Update Budget/Forecast Data With Valid Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Valid_Value()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Valid Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 395;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;
            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, ColumnName, Period, ParentRowId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);


        }

        #endregion

        #region Update Budget/Forecast Griddata With Empty Value
        /// <summary>
        /// To Update Budget/Forecast Data With Empty Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Empty_Value()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Empty Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string nValue = "Budget Test";
            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(0, "", nValue, "", "", "", 0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        #endregion

        #region Update Budget/Forecast Griddata With Empty nValue
        /// <summary>
        /// To Update Budget/Forecast Data With Empty nValue
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Empty_nValue()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Empty nValue.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 395;
            string IsQuaterly = "quarters";

            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, "", oValue, ColumnName, Period, ParentRowId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion

        #region Update Budget/Forecast Griddata With Empty oValue
        /// <summary>
        /// To Update Budget/Forecast Data With Empty oValue
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Empty_oValue()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Empty oValue.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 395;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";

            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, "", ColumnName, Period, ParentRowId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        #endregion

        #region Update Budget/Forecast Griddata With Empty Column Name
        /// <summary>
        /// To Update Budget/Forecast Data With Empty Column Name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Empty_ColumnName()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Empty Column Name.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 395;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";

            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, "", Period, ParentRowId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion

        #region Update Budget/Forecast Griddata With Null Value
        /// <summary>
        /// To Update Budget/Forecast Data With Null Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Null_Value()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(0, null, null, null, null, null, 0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion
        //Commented by Rahul Shah. bcz we can not handle null condition for nValue.
        //#region Update Budget/Forecast Griddata With Null nValue
        ///// <summary>
        ///// To Update Budget/Forecast Data With Null nValue
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>
        //[TestMethod]
        //public void Update_BudgetForecast_Griddata_With_Null_nValue()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int budgetId = 1138;
        //    string IsQuaterly = "quarters";

        //    string oValue = "Budget Test123";
        //    string ColumnName = "Task Name";
        //    string Period = "";
        //    int ParentRowId = 1138;

        //    //// Call UpdateBudgetGridData method
        //    FinanceController objFinanceController = new FinanceController();
        //    var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, null, oValue, ColumnName, Period, ParentRowId) as JsonResult;

        //    if (result != null)
        //    {

        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result);
        //    }

        //}

        //#endregion

        #region Update Budget/Forecast Griddata With Null oValue
        /// <summary>
        /// To Update Budget/Forecast Data With Null oValue
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Null_oValue()
        {
            Console.WriteLine("To Update Budget/Forecast Data With Null oValue.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 395;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";

            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, null, ColumnName, Period, ParentRowId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #endregion

        //Commented by Rahul Shah. bcz negative test cases
        //#region Update Budget/Forecast Griddata With Null Column Name
        ///// <summary>
        ///// To Update Budget/Forecast Data With Null Column Name
        ///// </summary>
        ///// <auther>Rahul Shah</auther>
        ///// <createddate>08Oct2015</createddate>
        //[TestMethod]
        //public void Update_BudgetForecast_Griddata_With_Null_ColumnName()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
        //    int budgetId = 1138;
        //    string IsQuaterly = "quarters";
        //    string nValue = "Budget Test";
        //    string oValue = "Budget Test123";

        //    string Period = "";
        //    int ParentRowId = 1138;

        //    //// Call UpdateBudgetGridData method
        //    FinanceController objFinanceController = new FinanceController();
        //    var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, null, Period, ParentRowId) as JsonResult;

        //    if (result != null)
        //    {

        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result);
        //    }

        //}

        //#endregion

        #endregion

        #region Update Budget Details

        #region Update Budget Details With Valid Values
        /// <summary>
        /// To Update Budget Details With Valid Values
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_Budget_Details_With_Valid_Values()
        {
            Console.WriteLine("To Update Budget Details With Valid Values.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string budgetId = "395";
            string BudgetDetailId = "1138";
            string mainTimeFrame = "Invalid";
            string BudgetDetailName = "Budget Test";
            string ParentId = "0";
            int OwnerId = Sessions.User.ID; //Changes by komal for #2243 on 16-06-16 as owner is editable in finance grid now.
            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail(budgetId, BudgetDetailId, ParentId, mainTimeFrame, "", OwnerId, BudgetDetailName) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);

        }
        #endregion

        #region Update Budget Details With Empty Values
        /// <summary>
        /// To Update Budget Details Empty Values
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_Budget_Details_With_Empty_Values()
        {
            Console.WriteLine("To Update Budget Details Empty Values.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail("", "", "", "", "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);
        }
        #endregion

        #region Update Budget Details With Null Values
        /// <summary>
        /// To Update Budget Details With Null Values
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_Budget_Details_With_Null_Values()
        {
            Console.WriteLine("To Update Budget Details With Null Values.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail(null, null, null, null) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNull(result);

        }
        #endregion

        #endregion

        #region GetAmount

        #region Get Amount Value With Valid Value
        /// <summary>
        /// To Get Amount value With Valid Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Amount_Value_With_Valid_Value()
        {
            Console.WriteLine("To Get Amount value With Valid Value.\n");
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string IsQuaterly = "quarters";
            List<Budget_Detail> BudgetDetailList = new List<Budget_Detail>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<int> BudgetDetailids = BudgetDetailList.Select(a => a.Id).ToList();
            List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

            List<int> LineItemids = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => PlanLineItemBudgetDetail.Contains(a.PlanLineItemId) && a.IsDeleted == false).Select(a => a.PlanLineItemId).ToList(); ;

            List<Budget_DetailAmount> BudgetDetailAmount = db.Budget_DetailAmount.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in db.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                 //join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Cost.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                 where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                 select Cost).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in db.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                     //join LineItemBudget in db.LineItem_Budget on Actual.PlanLineItemId equals LineItemBudget.PlanLineItemId
                                                                                     join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     join Tactic in db.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                     where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                     && tacticStatus.Contains(Tactic.Status)
                                                                                     select Actual).ToList();

            //List<string> BudgetDetailAmount = new List<string>();
            //List<string> PlanDetailAmount = new List<string>();
            //List<string> ActualDetailAmount = new List<string>();
            //// Call GetAmountValue method
            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetAmountValue(IsQuaterly, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount, LineItemidBudgetList);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value Budget:  " + obj.Budget);
            Assert.IsNotNull(obj.Budget);


        }
        #endregion

        #region Get Amount Value With Null Value
        /// <summary>
        /// To Get Amount value With Null Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_Amount_Value_With_Null_Value()
        {
            Console.WriteLine("To Get Amount value With Null Value.\n");
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetAmountValue(null, null, null, null, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value Budget:  " + obj.Budget);
            Assert.IsNotNull(obj.Budget);

        }
        #endregion

        #region Get MainGrid Amount Value With Valid Value
        /// <summary>
        /// To Get MainGrid Amount value With Valid Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_MainGrid_Amount_Value_With_Valid_Value()
        {
            Console.WriteLine("To Get MainGrid Amount value With Valid Value.\n");
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            Boolean IsQuaterly = true;
            string mainTimeFrame = "Yearly";

            List<Budget_Detail> BudgetDetailList = new List<Budget_Detail>();
            List<string> tacticStatus = Common.GetStatusListAfterApproved();
            List<int> BudgetDetailids = BudgetDetailList.Select(a => a.Id).ToList();
            List<LineItem_Budget> LineItemidBudgetList = db.LineItem_Budget.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<int> PlanLineItemBudgetDetail = LineItemidBudgetList.Select(a => a.PlanLineItemId).ToList();

            List<int> LineItemids = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => PlanLineItemBudgetDetail.Contains(a.PlanLineItemId) && a.IsDeleted == false).Select(a => a.PlanLineItemId).ToList(); ;

            List<Budget_DetailAmount> BudgetDetailAmount = db.Budget_DetailAmount.Where(a => BudgetDetailids.Contains(a.BudgetDetailId)).Select(a => a).ToList();
            List<Plan_Campaign_Program_Tactic_LineItem_Cost> PlanDetailAmount = (from Cost in db.Plan_Campaign_Program_Tactic_LineItem_Cost
                                                                                 //join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Cost.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                 where LineItemids.Contains(Cost.PlanLineItemId)
                                                                                 select Cost).ToList();

            List<Plan_Campaign_Program_Tactic_LineItem_Actual> ActualDetailAmount = (from Actual in db.Plan_Campaign_Program_Tactic_LineItem_Actual
                                                                                     //join LineItemBudget in db.LineItem_Budget on Actual.PlanLineItemId equals LineItemBudget.PlanLineItemId
                                                                                     join TacticLineItem in db.Plan_Campaign_Program_Tactic_LineItem on Actual.PlanLineItemId equals TacticLineItem.PlanLineItemId
                                                                                     join Tactic in db.Plan_Campaign_Program_Tactic on TacticLineItem.PlanTacticId equals Tactic.PlanTacticId
                                                                                     where LineItemids.Contains(Actual.PlanLineItemId)
                                                                                     && tacticStatus.Contains(Tactic.Status)
                                                                                     select Actual).ToList();

            //List<string> BudgetDetailAmount = new List<string>();
            //List<string> PlanDetailAmount = new List<string>();
            //List<string> ActualDetailAmount = new List<string>();
            //// Call GetAmountValue method
            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetMainGridAmountValue(IsQuaterly, mainTimeFrame, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount, LineItemidBudgetList);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value Budget:  " + obj.Budget);
            Assert.IsNotNull(obj.Budget);

        }
        #endregion

        #region Get MainGrid Amount Value With Null Value
        /// <summary>
        /// To Get MainGrid Amount value With Null Value
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Get_MainGrid_Amount_Value_With_Null_Value()
        {
            Console.WriteLine("To Get MainGrid Amount value With Null Value.\n");
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetMainGridAmountValue(true, null, null, null, null, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value Budget:  " + obj.Budget);
            Assert.IsNotNull(obj.Budget);


        }
        #endregion

        #region Edit Permission With Null Value
        /// <summary>
        /// Edit Permission With Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void Edit_Permission_With_Null_Value()
        {
            Console.WriteLine("Edit Permission With Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditPermission(0, "", "") as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result :  " + result);
            Assert.IsNull(result);
        }
        #endregion

        #region Delete Budget detail Permission with Null Value
        /// <summary>
        /// Delete Budget detail Permission with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void Delete_Budget_Permission_With_Null_Value()
        {
            Console.WriteLine("Delete Budget detail Permission with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.Delete(0, 0, null) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value  result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region Get specific of user record on selection of dropdown list with Null Value
        /// <summary>
        ///Get specific of user record on selection of dropdown list with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void Get_Record_With_Null_Value()
        {
            Console.WriteLine("Get specific of user record on selection of dropdown list with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.GetuserRecord(0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value  result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save record in Budget_Permission table with Null Value
        /// <summary>
        ///Save record in Budget_Permission table with Null Value with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void SaveDetail_With_Null_Value()
        {
            Console.WriteLine("Save record in Budget_Permission table with Null Value with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.SaveDetail(null, null, null, null) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get specific record based on dropdown selection value of budgetdetail id with Null Value
        /// <summary>
        ///Get specific record based on dropdown selection value of budgetdetail id with Null Value 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void DrpFilterByBudget_With_Null_Value()
        {
            Console.WriteLine("Get specific record based on dropdown selection value of budgetdetail id with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.DrpFilterByBudget(0, null, null) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get Column with Null Value
        /// <summary>
        ///Get Column with Null Value 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void GetColumn_With_Null_Value()
        {
            Console.WriteLine("Get Column with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.GetColumns(0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value  result.Data :  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region DeleteBudgetForecastData with Null Value
        /// <summary>
        ///DeleteBudgetForecastData with Null Value with Null Value 
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void DeleteBudgetForecastData_With_Null_Value()
        {
            Console.WriteLine("DeleteBudgetForecastData with Null Value with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.DeleteBudgetForecastData(null) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region EditBudget with Null Value
        /// <summary>
        /// EditBudget with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void EditBudget_With_Null_Value()
        {
            Console.WriteLine("EditBudget with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.EditBudget(0, null, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);
        }
        #endregion

        #region GetParentLineItemList with Null Value
        /// <summary>
        /// GetParentLineItemList with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void GetParentLineItemList_With_Null_Value()
        {
            Console.WriteLine("GetParentLineItemList with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.GetParentLineItemList(0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        #region GetChildLineItemList with Null Value
        /// <summary>
        /// GetChildLineItemList with Null Value
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>25Nov2015</createddate>
        [TestMethod]
        public void GetChildLineItemList_With_Null_Value()
        {
            Console.WriteLine("GetChildLineItemList with Null Value.\n");
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();

            var result = objFinanceController.GetChildLineItemList(0) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

        //#region DeleteMainGrid with Null Value
        ///// <summary>
        ///// DeleteMainGrid with Null Value
        ///// </summary>
        ///// <auther>Dashrath Prajapati</auther>
        ///// <createddate>25Nov2015</createddate>
        //[TestMethod]
        //public void DeleteMainGrid_With_Null_Value()
        //{
        //    //// Set session value
        //    System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

        //    //// Call CreateNewBudget
        //    FinanceController objFinanceController = new FinanceController();

        //    var result = objFinanceController.DeleteMainGrid(null,null,null,null);
        //    if (result != null)
        //    {
        //        //// ViewResult shoud not be null and should match with viewName
        //        Assert.IsNotNull(result);
        //    }

        //}
        //#endregion

        #endregion

        #region Import Finance Budget Data
        /// <summary>
        /// Created By Nishant Sheth
        /// Created Date : 15-Jul-2016
        /// Without file for import marketing budget
        /// </summary>
        [TestMethod]
        public void Import_Finance_Budget()
        {

            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            FinanceController controller = new FinanceController();
            int BudgetDetailId = DataHelper.GetBudgetDetailId();
            Sessions.BudgetDetailId = BudgetDetailId;
            var result = controller.ExcelFileUpload() as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result:  " + result);
            Assert.IsNotNull(result);

        }
        #endregion


    }
}
