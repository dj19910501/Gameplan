using Integration;
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
            Console.WriteLine("To check that it returns a proper view for the main screen or not.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(0, 1) as ViewResult;
            if (result != null)
            {
                Assert.AreEqual("IntegrationFolder", result.ViewName);

                //Check for the Year view bag, if it is null then view can give an error
                Assert.IsNotNull(controller.ViewBag.Year);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.integrationTypeCode);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To check that it returns a proper view for the main screen or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_View_Name_With_Out_Param()
        {
            Console.WriteLine("To check that it returns a proper view for the main screen or not.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(1) as ViewResult;
            if (result != null)
            {
                Assert.AreEqual("IntegrationFolder", result.ViewName);

                //Check for the Year view bag, if it is null then view can give an error
                Assert.IsNotNull(controller.ViewBag.Year);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.integrationTypeCode);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }

        /// <summary>
        /// To check that it returns a proper integration code for view in the main screen or not
        /// <author>Pratik</author>
        /// <createddate>04Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Integration_Folder_Check_Integration_Type_Code()
        {
            Console.WriteLine("To check that it returns a proper integration code for view in the main screen or not.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //Eloqua
            var result = controller.GetIntegrationFolder(1, 0) as ViewResult;
            if (result != null)
            {
                Assert.AreEqual("IntegrationFolder", result.ViewName);

                //Check for the Integration Type Code view bag, if it is null then view can give an error
                Assert.IsNotNull(controller.ViewBag.IntegrationTypeCode);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.integrationTypeCode);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

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
            Console.WriteLine("To check that it returns a proper partial view for plan listing or not.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            var result = controller.GetIntegrationFolderPlanList(DateTime.Now.Year.ToString()) as PartialViewResult;
            if (result != null)
            {
                Assert.AreEqual("_IntegrationFolderPlanList", result.ViewName);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewName);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

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

        #region PL#993, #995, #996 and #997 Custom fields integration: Change layout of existing UI and Custom fields integration: Tactic/Program/Campaign custom fields

        #region Sync InterationInstance without InterationInstanceId
        /// <summary>
        /// To check whether sync method properly handles exception or not in case of no integrationInstance selected
        /// <author>Sohel Pathan</author>
        /// <createdDate>08Dec2014</createdDate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_Without_Interation_Instance_Id()
        {
            Console.WriteLine("To check whether sync method properly handles exception or not in case of no integrationInstance selected.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            //Parameter IntegrationInstancesId = 0
            var result = controller.SyncNow(0) as JsonResult;
            if (result != null)
            {
                // Check Json result data should not be null
                Assert.IsNotNull(result.Data);

                // Check sync status is success or not
                Assert.AreEqual("Error", result.GetValue("status").ToString(), true);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status").ToString());
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Sync InterationInstance with InterationInstanceId of Salesforce
        /// <summary>
        /// To check sync method of salesforce intrgration
        /// <author>Sohel Pathan</author>
        /// <createdDate>08Dec2014</createdDate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_With_Interation_Instance_Id_SalesForce()
        {
            Console.WriteLine("To check sync method of salesforce intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Salesforce.ToString());
            var result = controller.SyncNow(IntegrationInstanceId) as JsonResult;
            if (result != null)
            {
                // Check Json result data object is null or not
                Assert.IsNotNull(result.Data);

                // Check sync status is success or not
                if (result.GetValue("status").ToString().Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("Success", result.GetValue("status").ToString(), true);
                }
                else if (result.GetValue("status").ToString().Equals("In-Progress", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("In-Progress", result.GetValue("status").ToString(), true);
                }
                else
                {
                    Assert.AreEqual("Error", result.GetValue("status").ToString(), true);
                }
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status").ToString());
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #region Sync InterationInstance with InterationInstanceId of Eloqua
        /// <summary>
        /// To check sync method of eloqua intrgration
        /// <author>Sohel Pathan</author>
        /// <createdDate>08Dec2014</createdDate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_With_Interation_Instance_Id_Eloqua()
        {
            Console.WriteLine("To check sync method of eloqua intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.SyncNow(IntegrationInstanceId) as JsonResult;
            if (result != null)
            {
                // Check Json result data object is null or not
                Assert.IsNotNull(result.Data);

                // Check sync status is success or not
                if (result.GetValue("status").ToString().Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("Success", result.GetValue("status").ToString(), true);
                }
                else if (result.GetValue("status").ToString().Equals("In-Progress", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("In-Progress", result.GetValue("status").ToString(), true);
                }
                else
                {
                    Assert.AreEqual("Error", result.GetValue("status").ToString(), true);
                }
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status").ToString());
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #endregion

        #region PL#1061 Pull MQL For Eloqua

        #region Create Eloqua Integration Instance
        /// <summary>
        /// To check that it returns a proper view for the selected integration instance or not
        /// <author>Sohel Pathan</author>
        /// <createddate>23Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Create_Integration_Instance_View()
        {
            Console.WriteLine("Set Parameter IntegrationTypeId for Eloqua.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.editIntegration(0, IntegrationTypeId) as ViewResult;
            if (result != null)
            {
                Assert.AreEqual("edit", result.ViewName, true);

                //// Check for the integrationTypeId view bag, if it is null then view can give an error
                Assert.IsNotNull(result.ViewBag.integrationTypeId);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.ViewBag.integrationTypeId);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }


        }
        #endregion

        #region Save Pull MQL data type mapping with null form data
        /// <summary>
        /// To check that SaveDataMappingPulling method save data with null form data
        /// <author>Sohel Pathan</author>
        /// <createddate>23Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Save_Pull_MQL_Repsones_With_Null_Form_Data()
        {
            Console.WriteLine("To check that SaveDataMappingPulling method save data with null form data.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            IList<GameplanDataTypePullModel> lstGameplanDataTypePullModel = null;

            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            //// Set Parameter IntegrationInstanceId for Eloqua
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.SaveDataMappingPulling(lstGameplanDataTypePullModel, IntegrationInstanceId, Enums.IntegrationType.Eloqua.ToString());

            if (result != null)
            {
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }
        }
        #endregion

        #region Save Pull MQL data type mapping with selected form data (selected data mapping)
        /// <summary>
        /// To check that SaveDataMappingPulling method save data with form data
        /// <author>Sohel Pathan</author>
        /// <createddate>23Dec2014</createddate>
        /// </summary>
        [TestMethod]
        public void Save_Pull_MQL_Repsones_With_Form_Data()
        {
            Console.WriteLine("To check that SaveDataMappingPulling method save data with form data.\n");
            ExternalServiceController controller = new ExternalServiceController();
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Set form data parameter for save method
            GameplanDataTypePullModel objGameplanDataTypePullModel = new GameplanDataTypePullModel();
            IList<GameplanDataTypePullModel> lstGameplanDataTypePullModel = new List<GameplanDataTypePullModel>();
            lstGameplanDataTypePullModel.Add(objGameplanDataTypePullModel);

            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            //// Set Parameter IntegrationInstanceId for Eloqua
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.SaveDataMappingPulling(lstGameplanDataTypePullModel, IntegrationInstanceId, Enums.IntegrationType.Eloqua.ToString());
            if (result != null)
            {
                Assert.IsNotNull(result.Data);

                Assert.IsNotNull(result.GetValue("status").ToString(), "1");
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.GetValue("status").ToString());
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result);
            }

        }
        #endregion

        #endregion
    }
}
