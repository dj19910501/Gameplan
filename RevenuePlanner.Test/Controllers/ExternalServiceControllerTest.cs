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
using System.Linq;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ExternalServiceControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.GetIntegrationFolder(0, 1) as ViewResult;            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value Year:  " + controller.ViewBag.Year);
            Assert.IsNotNull(controller.ViewBag.Year);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value integrationTypeCode:  " + result.ViewName);
            Assert.AreEqual("IntegrationFolder", result.ViewName);                     

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();            
            var result = controller.GetIntegrationFolder(1) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("IntegrationFolder", result.ViewName);

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.GetIntegrationFolder(1, 0) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewBag.integrationTypeCode:  " + result.ViewBag.integrationTypeCode);
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
            Console.WriteLine("To check that it returns a proper partial view for plan listing or not.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.GetIntegrationFolderPlanList(DateTime.Now.Year.ToString()) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
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
            var resultvalue = result.GetValue("status");
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue " + result.GetValue("status").ToString());
            Assert.AreEqual("Error", resultvalue.ToString(), true);

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
            var resultValue = result.GetValue("status").ToString();
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultValue:  " + resultValue.ToString());
            Assert.IsNotNull(resultValue.ToString());
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
            var resultValue = result.GetValue("status").ToString();
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultValue:  " + resultValue.ToString());
            Assert.IsNotNull(resultValue.ToString());

        }
        #endregion

        #region Sync InterationInstance with InterationInstanceId of Marketo
        /// <summary>
        /// To check sync method of Marketo intrgration
        /// <author>Nishant Sheth</author>
        /// <createdDate>24-May-2016</createdDate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_With_Interation_Instance_Id_Marketo()
        {
            Console.WriteLine("To check sync method of marketo intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Marketo.ToString());
            var result = controller.SyncNow(IntegrationInstanceId) as JsonResult;
            var resultValue = result.GetValue("status").ToString();
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultValue:  " + resultValue.ToString());
            Assert.IsNotNull(resultValue.ToString());
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();


            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.editIntegration(0, IntegrationTypeId) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value integrationTypeId:  " + result.ViewBag.integrationTypeId);
            Assert.IsNotNull(result.ViewBag.integrationTypeId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("edit", result.ViewName, true);
            

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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();


            IList<GameplanDataTypePullModel> lstGameplanDataTypePullModel = null;

            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            //// Set Parameter IntegrationInstanceId for Eloqua
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.SaveDataMappingPulling(lstGameplanDataTypePullModel, IntegrationInstanceId, Enums.IntegrationType.Eloqua.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
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
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();


            //// Set form data parameter for save method
            GameplanDataTypePullModel objGameplanDataTypePullModel = new GameplanDataTypePullModel();
            IList<GameplanDataTypePullModel> lstGameplanDataTypePullModel = new List<GameplanDataTypePullModel>();
            lstGameplanDataTypePullModel.Add(objGameplanDataTypePullModel);

            //// Set Parameter IntegrationTypeId for Eloqua
            int IntegrationTypeId = DataHelper.GetIntegrationTypeId(Enums.IntegrationType.Eloqua.ToString());
            //// Set Parameter IntegrationInstanceId for Eloqua
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.Eloqua.ToString());
            var result = controller.SaveDataMappingPulling(lstGameplanDataTypePullModel, IntegrationInstanceId, Enums.IntegrationType.Eloqua.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value  result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.GetValue:  " + result.GetValue("status").ToString());
            Assert.IsNotNull(result.GetValue("status").ToString(), "1");

        }
        #endregion

        #endregion

        #region Display Campaign Folder Selection under integration

        /// <summary>
        /// To get Marketo folder
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void GetmarketoFolder()
        {
            Console.WriteLine("To get Marketo integration folder.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault(); //marketo type id
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault(); //marketo instance id
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.GetMarketoFolder(MarketoInstanceTypeId, IntegrationInstanceId) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("MarketoFolder", result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value Year:  " + result.ViewBag.Year);
            Assert.IsNotNull(controller.ViewBag.Year);

        }

        /// <summary>
        /// To check that it returns a proper partial view for plan listing or not
        /// <author>Komal Rawal</author>
        /// <createddate>24May2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Marketo_Folder_Plan_List_WithNoInstanceValue()
        {
            Console.WriteLine("To check that it returns a proper partial view for plan listing of marketo or not.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            int planId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(planId);
            var result = controller.GetMarketoFolderPlanList(DateTime.Now.Year.ToString()) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("_MarketoFolderPlanList", result.ViewName);

        }

        /// <summary>
        /// To check that it returns a proper partial view for plan listing or not
        /// <author>Komal Rawal</author>
        /// <createddate>24May2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Marketo_Folder_Plan_List_WithInstanceValue()
        {
            Console.WriteLine("To check that it returns a proper partial view for plan listing of marketo or not.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            MRPEntities db = new MRPEntities();
            int planId = DataHelper.GetPlanId();
            Sessions.User.CID = DataHelper.GetClientId(planId);
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault();
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var result = controller.GetMarketoFolderPlanList(DateTime.Now.Year.ToString(), IntegrationInstanceId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("_MarketoFolderPlanList", result.ViewName);
        }


        /// <summary>
        /// To Save Campaign folder plan list without list
        /// <author>Komal Rawal</author>
        /// <createddate>24May2016</createddate>
        /// </summary>
        [TestMethod]
        public void SaveMarketoCampaignFolderPlanList_WithoutList()
        {
            Console.WriteLine("To Save Campaign foldeer without plan list .\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            MRPEntities db = new MRPEntities();
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault();
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            List<IntegrationPlanList> IntegrationPlanList = new List<IntegrationPlanList>();
            var result = controller.SaveMarketoCampaignFolderPlanList(IntegrationPlanList, IntegrationInstanceId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        /// <summary>
        /// To Save Campaign folder with plan list
        /// <author>Komal Rawal</author>
        /// <createddate>24May2016</createddate>
        /// </summary>
        [TestMethod]
        public void SaveMarketoCampaignFolderPlanList_WithList()
        {
            Console.WriteLine("To Save Campaign foldeer with plan list .\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            controller.Url = MockHelpers.FakeUrlHelper.UrlHelper();

            MRPEntities db = new MRPEntities();
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault();
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            List<IntegrationPlanList> IntegrationPlanList = new List<IntegrationPlanList>();

            IntegrationPlanList objIntegrationPlanList = new IntegrationPlanList();

            foreach (var item in DataHelper.GetMultiplePlanId())
            {
                objIntegrationPlanList = new IntegrationPlanList();
                objIntegrationPlanList.PlanId = item;
                objIntegrationPlanList.Year = DateTime.Now.Year.ToString();
                IntegrationPlanList.Add(objIntegrationPlanList);
            }


            var result = controller.SaveMarketoCampaignFolderPlanList(IntegrationPlanList, IntegrationInstanceId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        #endregion

        #region Display and Save general settings of marketo instance

        /// <summary>
        /// To get general settings of an instance
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void GetExternalServiceIntegrationsforMarketo()
        {
            Console.WriteLine("To get general settings of an instance.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault();
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.editIntegration(IntegrationInstanceId, MarketoInstanceTypeId) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);

            Assert.AreEqual("edit", result.ViewName);

        }

        /// <summary>
        /// To get general settings of an instance with no value
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void GetExternalServiceIntegrations_withnovalue()
        {
            Console.WriteLine("To get general settings of an instance with no value.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.editIntegration() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("edit", result.ViewName);


        }


        /// <summary>
        /// To test marketo integration credentials
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void TestMarketoIntegrationCredentials_withvaliddata()
        {
            Console.WriteLine("To test marketo integration credentials.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType ObjMarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").FirstOrDefault();
            int MarketoInstanceTypeId = ObjMarketoInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var record = db.IntegrationInstances
                                   .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.IntegrationInstanceId == IntegrationInstanceId)
                                   .Select(ii => ii).FirstOrDefault();

            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();
            if (record != null)
            {
                //// Add IntegrationType Attributes data to List.
                List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
                foreach (var item in recordAttribute)
                {

                    IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                    objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                    objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                    objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                    objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                    objIntegrationTypeAttributeModel.Value = item.Value;
                    lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
                }

                if (lstObjIntegrationTypeAttributeModel.Count == 0)
                {
                    lstObjIntegrationTypeAttributeModel = null;
                }
                //Valid credentials of an instance
                form.Instance = record.Instance;
                form.Username = record.Username;
                form.Password = Common.Decrypt(record.Password);
                form.IntegrationInstanceId = record.IntegrationInstanceId;
                form.IntegrationTypeId = record.IntegrationTypeId;
                form.IsActive = record.IsActive;
                form.ClientId = Sessions.User.CID;
                form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
                IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
                objIntegrationTypeModel.Title = ObjMarketoInstanceTypeId.Title;
                objIntegrationTypeModel.Code = ObjMarketoInstanceTypeId.Code;
                form.IntegrationType = objIntegrationTypeModel;
                var result = controller.TestIntegration(form) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }

        /// <summary>
        /// To test marketo integration credentials
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void TestMarketoIntegrationCredentials_withInvaliddata()
        {
            Console.WriteLine("To test marketo integration credentials.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType ObjMarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").FirstOrDefault();
            int MarketoInstanceTypeId = ObjMarketoInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();

            //// Add IntegrationType Attributes data to List.
            List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
            foreach (var item in recordAttribute)
            {

                IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                objIntegrationTypeAttributeModel.Value = item.Value;
                lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
            }
            if (lstObjIntegrationTypeAttributeModel.Count == 0)
            {
                lstObjIntegrationTypeAttributeModel = null;
            }
            form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = ObjMarketoInstanceTypeId.Title;
            objIntegrationTypeModel.Code = ObjMarketoInstanceTypeId.Code;
            form.IntegrationType = objIntegrationTypeModel;
            var result = controller.TestIntegration(form) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);


        }


        /// <summary>
        /// To Save marketo settings
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void SaveGeneralMarketoSettings()
        {
            Console.WriteLine("To Save general marketo settings.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType ObjMarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").FirstOrDefault();
            int MarketoInstanceTypeId = ObjMarketoInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var record = db.IntegrationInstances
                                   .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.IntegrationInstanceId == IntegrationInstanceId)
                                   .Select(ii => ii).FirstOrDefault();

            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();
            if (record != null)
            {
                //// Add IntegrationType Attributes data to List.
                List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
                foreach (var item in recordAttribute)
                {

                    IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                    objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                    objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                    objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                    objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                    objIntegrationTypeAttributeModel.Value = item.Value;
                    lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
                }

                if (lstObjIntegrationTypeAttributeModel.Count == 0)
                {
                    lstObjIntegrationTypeAttributeModel = null;
                }

                //Credentials of an instance
                form.Instance = record.Instance;
                form.Username = record.Username;
                form.Password = Common.Decrypt(record.Password);
                form.IntegrationInstanceId = record.IntegrationInstanceId;
                form.IntegrationTypeId = record.IntegrationTypeId;
                form.IsActive = record.IsActive;
                form.ClientId = Sessions.User.CID;
                form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;

                IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
                objIntegrationTypeModel.Title = ObjMarketoInstanceTypeId.Title;
                objIntegrationTypeModel.Code = ObjMarketoInstanceTypeId.Code;

                form.IntegrationType = objIntegrationTypeModel;

                var recordSync = db.SyncFrequencies
                                          .Where(freq => freq.IntegrationInstanceId == IntegrationInstanceId && freq.IntegrationInstance.ClientId == Sessions.User.CID)
                                          .Select(freq => freq).FirstOrDefault();

                SyncFrequencyModel objSync = new SyncFrequencyModel();
                if (recordSync != null)
                {
                    objSync.Day = !string.IsNullOrEmpty(recordSync.Day) ? recordSync.Day : string.Empty;
                    objSync.DayofWeek = !string.IsNullOrEmpty(recordSync.DayofWeek) ? recordSync.DayofWeek : string.Empty;
                    objSync.Frequency = recordSync.Frequency;

                    // Set Time data to SyncFrequencyModel Object.
                    if (recordSync.Time.HasValue)
                    {
                        if (recordSync.Time.Value.Hours > 12)
                            objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "PM";
                        else
                            objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "AM";
                    }
                    objSync.IntegrationInstanceId = recordSync.IntegrationInstanceId;
                }
                form.SyncFrequency = objSync;

                var result = controller.SaveGeneralSetting(form) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }
        }

        #endregion

        #region Display and Save general settings of Salesforce instance

        /// <summary>
        /// To get general settings of an instance
        /// </summary>
        /// <auther>Viral</auther>
        /// <createddate>17June2016</createddate>
        [TestMethod]
        public void GetExternalServiceIntegrationsforSalesforce()
        {
            Console.WriteLine("To get general settings of an instance.\n");
            string strSFDCTitle = Enums.IntegrationType.Salesforce.ToString();
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int sfdcIntegrationTypeId = db.IntegrationTypes.Where(inst => inst.Title == strSFDCTitle).Select(id => id.IntegrationTypeId).FirstOrDefault();
            int sfdcIntegrationInstanceId = DataHelper.GetIntegrationInstanceId(strSFDCTitle);
            //int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            ExternalServiceController controller = new ExternalServiceController();
            var result = controller.editIntegration(sfdcIntegrationInstanceId, sfdcIntegrationTypeId) as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.ViewName:  " + result.ViewName);
            Assert.AreEqual("edit", result.ViewName);

        }

        /// <summary>
        /// To test Salesforce integration credentials
        /// </summary>
        /// <auther>Viral</auther>
        /// <createddate>17June2016</createddate>
        [TestMethod]
        public void TestSalesforceIntegrationCredentials_withvaliddata()
        {
            Console.WriteLine("To test salesforce integration credentials.\n");
            string strSFDCTitle = Enums.IntegrationType.Salesforce.ToString();
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType objSFDCInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == strSFDCTitle).FirstOrDefault();
            int sfdcInstanceTypeId = objSFDCInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == sfdcInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var record = db.IntegrationInstances
                                   .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.IntegrationInstanceId == IntegrationInstanceId)
                                   .Select(ii => ii).FirstOrDefault();

            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();

            //// Add IntegrationType Attributes data to List.
            List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
            foreach (var item in recordAttribute)
            {

                IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                objIntegrationTypeAttributeModel.Value = item.Value;
                lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
            }

            if (lstObjIntegrationTypeAttributeModel.Count == 0)
            {
                lstObjIntegrationTypeAttributeModel = null;
            }
            //Valid credentials of an instance
            if (record != null)
            {
                form.Instance = record.Instance;
                form.Username = record.Username;
                form.Password = Common.Decrypt(record.Password);
                form.IntegrationInstanceId = record.IntegrationInstanceId;
                form.IntegrationTypeId = record.IntegrationTypeId;
                form.IsActive = record.IsActive;
                form.ClientId = Sessions.User.CID;
                form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
            }

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = objSFDCInstanceTypeId.Title;
            objIntegrationTypeModel.Code = objSFDCInstanceTypeId.Code;

            form.IntegrationType = objIntegrationTypeModel;

            var result = controller.TestIntegration(form) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);

            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To test Salesforce integration credentials
        /// </summary>
        /// <auther>Viral</auther>
        /// <createddate>17June2016</createddate>
        [TestMethod]
        public void TestSalesforceIntegrationCredentials_withInvaliddata()
        {
            Console.WriteLine("To test salesforce integration credentials.\n");
            string strSFDCTitle = Enums.IntegrationType.Salesforce.ToString();
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType objSFDCInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == strSFDCTitle).FirstOrDefault();
            int sfdcInstanceTypeId = objSFDCInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == sfdcInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();

            //// Add IntegrationType Attributes data to List.
            List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
            foreach (var item in recordAttribute)
            {

                IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                objIntegrationTypeAttributeModel.Value = item.Value;
                lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
            }

            if (lstObjIntegrationTypeAttributeModel.Count == 0)
            {
                lstObjIntegrationTypeAttributeModel = null;
            }


            form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = objSFDCInstanceTypeId.Title;
            objIntegrationTypeModel.Code = objSFDCInstanceTypeId.Code;

            form.IntegrationType = objIntegrationTypeModel;

            var result = controller.TestIntegration(form) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);

            Assert.IsNotNull(result.Data);
          
        }


        /// <summary>
        /// To Save salesforce settings
        /// </summary>
        /// <auther>Viral</auther>
        /// <createddate>17June2016</createddate>
        [TestMethod]
        public void SaveGeneralSalesforceSettings()
        {
            Console.WriteLine("To Save general salesforce settings.\n");
            string strSFDCTitle = Enums.IntegrationType.Salesforce.ToString();
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();
            IntegrationModel form = new IntegrationModel();
            IntegrationType objSFDCInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == strSFDCTitle).FirstOrDefault();
            int sfdcInstanceTypeId = objSFDCInstanceTypeId.IntegrationTypeId;
            int IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == sfdcInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            var record = db.IntegrationInstances
                                   .Where(ii => ii.IsDeleted.Equals(false) && ii.ClientId == Sessions.User.CID && ii.IntegrationInstanceId == IntegrationInstanceId)
                                   .Select(ii => ii).FirstOrDefault();

            var recordAttribute = db.IntegrationInstance_Attribute
                         .Where(attr => attr.IntegrationInstanceId == IntegrationInstanceId && attr.IntegrationInstance.ClientId == Sessions.User.CID)
                         .Select(attr => attr).ToList();

            //// Add IntegrationType Attributes data to List.
            List<IntegrationTypeAttributeModel> lstObjIntegrationTypeAttributeModel = new List<IntegrationTypeAttributeModel>();
            foreach (var item in recordAttribute)
            {

                IntegrationTypeAttributeModel objIntegrationTypeAttributeModel = new IntegrationTypeAttributeModel();
                objIntegrationTypeAttributeModel.Attribute = item.IntegrationTypeAttribute.Attribute;
                objIntegrationTypeAttributeModel.AttributeType = item.IntegrationTypeAttribute.AttributeType;
                objIntegrationTypeAttributeModel.IntegrationTypeAttributeId = item.IntegrationTypeAttribute.IntegrationTypeAttributeId;
                objIntegrationTypeAttributeModel.IntegrationTypeId = item.IntegrationTypeAttribute.IntegrationTypeId;
                objIntegrationTypeAttributeModel.Value = item.Value;
                lstObjIntegrationTypeAttributeModel.Add(objIntegrationTypeAttributeModel);
            }

            if (lstObjIntegrationTypeAttributeModel.Count == 0)
            {
                lstObjIntegrationTypeAttributeModel = null;
            }

            //Credentials of an instance
            if (record != null)
            {
                form.Instance = record.Instance;
                form.Username = record.Username;
                form.Password = Common.Decrypt(record.Password);
                form.IntegrationInstanceId = record.IntegrationInstanceId;
                form.IntegrationTypeId = record.IntegrationTypeId;
                form.IsActive = record.IsActive;
                form.ClientId = Sessions.User.CID;
                form.IntegrationTypeAttributes = lstObjIntegrationTypeAttributeModel;
            }

            IntegrationTypeModel objIntegrationTypeModel = new IntegrationTypeModel();
            objIntegrationTypeModel.Title = objSFDCInstanceTypeId.Title;
            objIntegrationTypeModel.Code = objSFDCInstanceTypeId.Code;

            form.IntegrationType = objIntegrationTypeModel;

            var recordSync = db.SyncFrequencies
                                      .Where(freq => freq.IntegrationInstanceId == IntegrationInstanceId && freq.IntegrationInstance.ClientId == Sessions.User.CID)
                                      .Select(freq => freq).FirstOrDefault();

            SyncFrequencyModel objSync = new SyncFrequencyModel();
            if (recordSync != null)
            {
                objSync.Day = !string.IsNullOrEmpty(recordSync.Day) ? recordSync.Day : string.Empty;
                objSync.DayofWeek = !string.IsNullOrEmpty(recordSync.DayofWeek) ? recordSync.DayofWeek : string.Empty;
                objSync.Frequency = recordSync.Frequency;

                // Set Time data to SyncFrequencyModel Object.
                if (recordSync.Time.HasValue)
                {
                    if (recordSync.Time.Value.Hours > 12)
                        objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "PM";
                    else
                        objSync.Time = recordSync.Time.Value.Hours.ToString().PadLeft(2, '0') + ":00 " + "AM";
                }
                objSync.IntegrationInstanceId = recordSync.IntegrationInstanceId;
            }
            form.SyncFrequency = objSync;
            
            var result = controller.SaveGeneralSetting(form) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value  result.Data:  " + result.Data);

            Assert.IsNotNull(result.Data);

        }

        #endregion

        #region WorkFront Integration
        /// <summary>
        /// To sync data into work front by integration instance
        /// <author>Arpita Soni</author>
        /// <createddate>22Jun2016</createddate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_With_Interation_Instance_Id_WorkFront()
        {
            Console.WriteLine("To check sync method of work front intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ExternalServiceController controller = new ExternalServiceController();

            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.WorkFront.ToString());
            var result = controller.SyncNow(IntegrationInstanceId) as JsonResult;
            var resultValue = result.GetValue("status").ToString();
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);          

        }

        /// <summary>
        /// To sync data into work front from the review tab 
        /// of tactic inspect popup for use case - program to portfolio
        /// <author>Arpita Soni</author>
        /// <createddate>29Jun2016</createddate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_WorkFront_From_Review_Tab_For_Program_As_Portfolio()
        {
            Console.WriteLine("To check sync method of work front intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            MRPEntities db = new MRPEntities();
            List<string> statusList = Common.GetStatusListAfterApproved();
            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.WorkFront.ToString());
            List<int> lstTacIds = new List<int>();
            lstTacIds = db.Plan_Campaign_Program_Tactic.
                                Where(tac => tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt == IntegrationInstanceId
                                && tac.IntegrationWorkFrontProjectID != null
                                && statusList.Contains(tac.Status) && tac.IsDeployedToIntegration && !tac.IsDeleted).Select(t => t.PlanTacticId).ToList();

            int planTacticId = db.IntegrationWorkFrontTacticSettings.Where(x => lstTacIds.Contains(x.TacticId) &&
                                x.TacticApprovalObject.Equals("Project")).Select(t => t.TacticId).FirstOrDefault();
            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.ID, EntityType.Tactic);

            externalIntegration.Sync();

            var result = externalIntegration;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result._isResultError:  " + result._isResultError);
            Assert.IsNotNull(result._isResultError);
            //// Check sync status is success or not
            //if (result._isResultError)
            //{
            //    Assert.AreEqual("true", result._isResultError.ToString(), true);
            //}
            //else
            //{
            //    Assert.AreEqual("false", result._isResultError.ToString(), true);
            //}
      
        }


        /// <summary>
        /// To sync data into work front from the review tab 
        /// of tactic inspect popup for use case - request
        /// <author>Arpita Soni</author>
        /// <createddate>29Jun2016</createddate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_WorkFront_From_Review_Tab_For_Request()
        {
            Console.WriteLine("To check sync method of work front intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            MRPEntities db = new MRPEntities();
            List<string> statusList = Common.GetStatusListAfterApproved();
            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.WorkFront.ToString());
            List<int> lstTacIds = new List<int>();
            lstTacIds = db.Plan_Campaign_Program_Tactic.
                                Where(tac => tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt == IntegrationInstanceId
                                && tac.IntegrationWorkFrontProjectID == null
                                && statusList.Contains(tac.Status) && tac.IsDeployedToIntegration && !tac.IsDeleted).Select(t => t.PlanTacticId).ToList();

            int planTacticId = db.IntegrationWorkFrontRequests.Where(x => lstTacIds.Contains(x.PlanTacticId)).
                                    Select(t => t.PlanTacticId).FirstOrDefault();
            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.ID, EntityType.Tactic);

            externalIntegration.Sync();

            var result = externalIntegration;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value  result._isResultError:  " + result._isResultError);
            // Check Json result data object is null or not
            Assert.IsNotNull(result._isResultError);
            // Check sync status is success or not
            //if (result._isResultError)
            //{
            //    Assert.AreEqual("true", result._isResultError.ToString(), true);
            //}
            //else
            //{
            //    Assert.AreEqual("false", result._isResultError.ToString(), true);
            //}         
        }


        /// <summary>
        /// To sync data into work front from the review tab 
        /// of tactic inspect popup for use case - plan to portfolio
        /// <author>Arpita Soni</author>
        /// <createddate>29Jun2016</createddate>
        /// </summary>
        [TestMethod]
        public void Sync_Interation_Instance_WorkFront_From_Review_Tab_For_Plan_As_Portfolio()
        {
            Console.WriteLine("To check sync method of work front intrgration.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            MRPEntities db = new MRPEntities();
            List<string> statusList = Common.GetStatusListAfterApproved();
            // Set Parameter IntegrationInstancesId
            int IntegrationInstanceId = DataHelper.GetIntegrationInstanceId(Enums.IntegrationType.WorkFront.ToString());
            List<int> lstTacIds = new List<int>();
            lstTacIds = db.Plan_Campaign_Program_Tactic.
                                Where(tac => tac.Plan_Campaign_Program.Plan_Campaign.Plan.Model.IntegrationInstanceIdProjMgmt == IntegrationInstanceId
                                && tac.IntegrationWorkFrontProjectID != null
                                && statusList.Contains(tac.Status) && tac.IsDeployedToIntegration && !tac.IsDeleted).Select(t => t.PlanTacticId).ToList();

            int planTacticId = db.IntegrationWorkFrontTacticSettings.Where(x => lstTacIds.Contains(x.TacticId) &&
                                x.TacticApprovalObject.Equals("Project2")).Select(t => t.TacticId).FirstOrDefault();
            ExternalIntegration externalIntegration = new ExternalIntegration(planTacticId, Sessions.ApplicationId, Sessions.User.ID, EntityType.Tactic);

            externalIntegration.Sync();
                        
            var result = externalIntegration;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value  result._isResultError:  " + result._isResultError);
            Assert.IsNotNull(result._isResultError);

            // Check sync status is success or not
            //if (result._isResultError)
            //{
            //    Assert.AreEqual("true", result._isResultError.ToString(), true);
            //}
            //else
            //{
            //    Assert.AreEqual("false", result._isResultError.ToString(), true);
            //}
         
        }

        #endregion
    }
}
