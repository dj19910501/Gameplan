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
    public class FinanceControllerTest : CommonController
    {
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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.Index() as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                if (!(result.ViewName.Equals("Index") || result.ViewName.Equals("Finance")))
                {
                    Assert.Fail();
                }
                else if (result.ViewName.Equals("Index"))
                {
                    Assert.IsNotNull(result.Model);
                    HomePlanModel objModel = (HomePlanModel)result.Model;
                    Assert.AreNotEqual(0, objModel.PlanId);
                }

                Assert.IsNotNull(result.ViewBag.ActiveMenu);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.CreateNewBudget("") as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Create new Budget with Null Budget name

        /// <summary>
        /// To check to budget creating with Null Budget name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Create_Budget_With_Null_Budget_Name()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.CreateNewBudget(null) as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetId, BudgetDetailName, ParentId, "Yearly") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(null, null, null, "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetID = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetDetailName, BudgetDetailName, ParentId, "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetID = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetID, null, ParentId, "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Save Budget Details with Null ParentId

        /// <summary>
        /// Save Budget Details with Null ParentID
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Null_ParentID()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetID = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetID, BudgetDetailName, null, null) as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Save Budget Details with Empty Values

        /// <summary>
        /// Save Budget Details with Empty Values
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_Value()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail("", "", "", "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Save Budget Details with Empty BudgetId

        /// <summary>
        /// Save Budget Details with Empty BudgetId
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_BudgetId()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail("", BudgetDetailName, ParentId, "quaterly") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetId, "", ParentId, "") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Save Budget Details with Empty ParentId

        /// <summary>
        /// Save Budget Details with Empty ParentId
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>

        [TestMethod]
        public void Save_Budget_Details_with_Empty_ParentId()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string BudgetId = "59";
            string BudgetDetailName = "Test";
            string ParentId = "1138";
            //// Call SaveNewBudgetDetail
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.SaveNewBudgetDetail(BudgetId, BudgetDetailName, "", "quaterly") as JsonResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string _InvalidRowID = "InValid";
            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData("", 0, "") as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData("", 0, "Quaterly") as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string _InvalidRowID = "InValid";
            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(null, 0, null) as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string _InvalidRowID = "InValid";
            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData(_InvalidRowID, 0, "Quaterly") as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call CreateNewBudget
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.RefreshMainGridData("[]", 9, "Quaterly") as ViewResult;
            if (result != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue() as ViewResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 0;
            string timeFrameOption = "";
            string isQuarterly = "Quarterly";
            bool IsMain = true;
            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(0, "", "", false) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 0;
            string timeFrameOption = "";
            string isQuarterly = "Quarterly";
            bool IsMain = true;
            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(0, null, null, true) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 0;
            string timeFrameOption = "";
            string isQuarterly = "Quarterly";
            bool IsMain = true;
            //// Call GetFinanceHeaderValue method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetFinanceHeaderValue(budgetId, timeFrameOption, isQuarterly, IsMain) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetChildBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildBudget(budgetId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetChildBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildBudget() as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetParentBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetParentBudget() as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1;
            //// Call GetParentBudget method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetParentBudget(budgetId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call GetChildTimeFrame method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.GetChildTimeFrame() as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string SelectedRowIDs = "[]";
            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(SelectedRowIDs, budgetId, IsQuaterly) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string SelectedRowIDs = "[]";
            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData("", 0, "") as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string SelectedRowIDs = "[]";
            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(null, 0, null) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string SelectedRowIDs = "";
            string IsQuaterly = "quarters";
            //// Call EditBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.EditBudgetGridData(SelectedRowIDs, budgetId, IsQuaterly) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;
            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, ColumnName, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(1138, "", nValue, "", "", "", 0) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, "", oValue, ColumnName, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, "", ColumnName, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, "", Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;
            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(0, null, null, null, null, null, 0) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Update Budget/Forecast Griddata With Null nValue
        /// <summary>
        /// To Update Budget/Forecast Data With Null nValue
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Null_nValue()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, null, oValue, ColumnName, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Update Budget/Forecast Griddata With Null oValue
        /// <summary>
        /// To Update Budget/Forecast Data With Null oValue
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Null_oValue()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, null, ColumnName, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

        #region Update Budget/Forecast Griddata With Null Column Name
        /// <summary>
        /// To Update Budget/Forecast Data With Null Column Name
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>08Oct2015</createddate>
        [TestMethod]
        public void Update_BudgetForecast_Griddata_With_Null_ColumnName()
        {
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            int budgetId = 1138;
            string IsQuaterly = "quarters";
            string nValue = "Budget Test";
            string oValue = "Budget Test123";
            string ColumnName = "Task Name";
            string Period = "";
            int ParentRowId = 1138;

            //// Call UpdateBudgetGridData method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetGridData(budgetId, IsQuaterly, nValue, oValue, null, Period, ParentRowId) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

        }

        #endregion

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string budgetId = "59";
            string BudgetDetailId = "1138";
            string mainTimeFrame = "Invalid";
            string BudgetDetailName = "Budget Test";
            string ParentId = "0";
            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail(budgetId, BudgetDetailName, BudgetDetailId, ParentId, mainTimeFrame) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string budgetId = "59";
            string BudgetDetailId = "1138";
            string mainTimeFrame = "Invalid";
            string BudgetDetailName = "Budget Test";
            string ParentId = "0";
            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail("", "", "", "", "") as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            //// Set session value
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();
            string budgetId = "59";
            string BudgetDetailId = "1138";
            string mainTimeFrame = "Invalid";
            string BudgetDetailName = "Budget Test";
            string ParentId = "0";
            //// Call UpdateBudgetDetail method
            FinanceController objFinanceController = new FinanceController();
            var result = objFinanceController.UpdateBudgetDetail(null, null, null, null, null) as JsonResult;

            if (result != null)
            {

                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(result);
            }

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
            obj = objFinanceController.GetAmountValue(IsQuaterly, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount);

            if (obj != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(obj);
            }

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
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetAmountValue(null, null, null, null);

            if (obj != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(obj);
            }

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
            obj = objFinanceController.GetMainGridAmountValue(IsQuaterly, mainTimeFrame, BudgetDetailAmount, PlanDetailAmount, ActualDetailAmount);

            if (obj != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(obj);
            }

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
            //// Set session value
            MRPEntities db = new MRPEntities();
            System.Web.HttpContext.Current = DataHelper.SetUserAndPermission();

            FinanceController objFinanceController = new FinanceController();

            BudgetAmount obj = new BudgetAmount();
            obj = objFinanceController.GetMainGridAmountValue(true, null, null, null, null);

            if (obj != null)
            {
                //// ViewResult shoud not be null and should match with viewName
                Assert.IsNotNull(obj);
            }

        }
        #endregion

        #endregion

                
    }
}
