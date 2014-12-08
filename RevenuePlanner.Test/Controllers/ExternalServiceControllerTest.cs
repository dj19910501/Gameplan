using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.BDSService;
using RevenuePlanner.Controllers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ExternalServiceControllerTest
    {
        #region PL#998 Eloqua Folders: Way to specifiy folder paths on Gameplan

        #region Get Integratoin Folder

        /// <summary>
        /// To check that it returns a proper view for the main screen or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_View_Name_With_Param()
        {
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(0, 1) as ViewResult;
            Assert.AreEqual("IntegrationFolder", result.ViewName);

            //Check for the Year view bag, if it is null then view can give an error
            Assert.IsNotNull(controller.ViewBag.Year);
        }

        /// <summary>
        /// To check that it returns a proper view for the main screen or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_View_Name_With_Out_Param()
        {
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(1) as ViewResult;
            Assert.AreEqual("IntegrationFolder", result.ViewName);

            //Check for the Year view bag, if it is null then view can give an error
            Assert.IsNotNull(controller.ViewBag.Year);
        }

        /// <summary>
        /// To check that it returns a proper integration code for view in the main screen or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_Check_Integration_Type_Code()
        {
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(1, 0) as ViewResult;
            Assert.AreEqual("IntegrationFolder", result.ViewName);

            //Check for the Integration Type Code view bag, if it is null then view can give an error
            Assert.IsNotNull(controller.ViewBag.IntegrationTypeCode);
        }

        #endregion

        #region Get Integration Folder Plan List

        /// <summary>
        /// To check that it returns a proper partial view for plan listing or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_Plan_List_View_Name()
        {
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            var result = controller.GetIntegrationFolderPlanList(DateTime.Now.Year.ToString()) as PartialViewResult;
            Assert.AreEqual("_IntegrationFolderPlanList", result.ViewName);
        }

        #endregion

        #region Save Integration Folder Plan List

        ///// <summary>
        ///// To check that it returns a proper result after save with zero list in plan
        ///// <author>Pratik</author>
        ///// <createddate>04Dec2014</createddate>
        ///// </summary>
        //[TestMethod]
        //public void Save_Integration_Folder_Plan_List_With_Zero_List()
        //{
        //    List<IntegrationPlanList> lst = new List<IntegrationPlanList>();

        //    ExternalServiceController controller = new ExternalServiceController();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();

        //    var result = controller.SaveIntegrationFolderPlanList(lst) as JsonResult;
        //    Assert.IsNotNull(result);
        //}

        ///// <summary>
        ///// To check that it returns a proper result after save with one list in plan
        ///// <author>Pratik</author>
        ///// <createddate>04Dec2014</createddate>
        ///// </summary>
        //[TestMethod]
        //public void Save_Integration_Folder_Plan_List_With_One_List()
        //{
        //    List<IntegrationPlanList> lst = new List<IntegrationPlanList>();

        //    IntegrationPlanList objIntegrationPlanList = new IntegrationPlanList();

        //    objIntegrationPlanList.PlanId = DataHelper.GetPlanId();
        //    objIntegrationPlanList.FolderPath = "E:/PlanFolder/" + objIntegrationPlanList.PlanId + "/" + DateTime.Now.ToString();

        //    lst.Add(objIntegrationPlanList);

        //    ExternalServiceController controller = new ExternalServiceController();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();

        //    var result = controller.SaveIntegrationFolderPlanList(lst) as JsonResult;
        //    Assert.IsNotNull(result);
        //}

        ///// <summary>
        ///// To check that it returns a proper result after save with multiple list  data in plan
        ///// <author>Pratik</author>
        ///// <createddate>04Dec2014</createddate>
        ///// </summary>
        //[TestMethod]
        //public void Save_Integration_Folder_Plan_List_With_Multiple_List()
        //{
        //    List<IntegrationPlanList> lst = new List<IntegrationPlanList>();

        //    IntegrationPlanList objIntegrationPlanList = new IntegrationPlanList();

        //    foreach (var item in DataHelper.GetMultiplePlanId())
        //    {
        //        objIntegrationPlanList = new IntegrationPlanList();
        //        objIntegrationPlanList.PlanId = item;
        //        objIntegrationPlanList.FolderPath = "E:/PlanFolder/" + item + "/" + DateTime.Now.ToString();
        //        lst.Add(objIntegrationPlanList);
        //    }

        //    ExternalServiceController controller = new ExternalServiceController();
        //    HttpContext.Current = DataHelper.SetUserAndPermission();

        //    var result = controller.SaveIntegrationFolderPlanList(lst) as JsonResult;
        //    Assert.IsNotNull(result);
        //}

        #endregion

        #endregion
    }
}
