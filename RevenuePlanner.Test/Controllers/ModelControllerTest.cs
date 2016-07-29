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
using System.Diagnostics;
using Integration;


namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ModelControllerTest
    {
        #region Display channel and Program type selection for marketo under Model-Tactictype
        /// <summary>
        /// To Save Marketo Settings on Model-TacticType scrren
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void SaveMarketoSettings()
        {
            Console.WriteLine("To Save Channel and Program type on Model-TacticType screen.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            TacticType objTacticType = db.TacticTypes.Where(Id => Id.Model.IntegrationInstanceMarketoID != null).Select(obj => obj).FirstOrDefault();
            int? InstanceId = objTacticType.Model.IntegrationInstanceMarketoID; //get marketo instance id
            int EntityID = objTacticType.TacticTypeId;
            string EntityType = Enums.FilterLabel.TacticType.ToString();
            ApiIntegration ObjApiintegration = new ApiIntegration(Enums.ApiIntegrationData.Programtype.ToString(), InstanceId);
            MarketoDataObject CampaignFolderList = ObjApiintegration.GetProgramChannellistData();
            string ProgramType = CampaignFolderList.program.Select(list => list.Key).FirstOrDefault();
            string Channel = CampaignFolderList.channels.Select(list => list.name).FirstOrDefault();
            ModelController objModelController = new ModelController();
            objModelController.SaveMarketoSettings(EntityID, InstanceId, EntityType, ProgramType, Channel);
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value objModelController:  " + objModelController);
            Assert.IsNotNull(objModelController);
        }

        /// <summary>
        /// To Save Marketo integration data
        /// </summary>
        /// <auther>Komal Rawal</auther>
        /// <createddate>23May2016</createddate>
        [TestMethod]
        public void SaveMarketoIntegration()
        {
            Console.WriteLine("To Save Marketo integration data.\n");
            MRPEntities db = new MRPEntities();
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int ModelId = DataHelper.GetModelId();
            int MarketoInstanceTypeId = db.IntegrationTypes.Where(inst => inst.Title == "Marketo").Select(id => id.IntegrationTypeId).FirstOrDefault();
            int? IntegrationInstanceId = db.IntegrationInstances.Where(id => id.IntegrationTypeId == MarketoInstanceTypeId && id.IsDeleted == false).Select(id => id.IntegrationInstanceId).FirstOrDefault();
            BaselineModel objBaselineModel = new BaselineModel();
            objBaselineModel.IntegrationInstanceMarketoID = IntegrationInstanceId;
            ModelController objModelController = new ModelController();
            var result = objModelController.SaveIntegration(ModelId, objBaselineModel, false, false, true);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.AreNotEqual(null, result.Data);
        }


        #endregion        

        #region Create a new Model

        /// <summary>
        /// To Create a New Model
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Create_Model()
        {

            var routes = new RouteCollection();
            Console.WriteLine("To Create a New Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objModelController.CreateModel() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
            

        }
        #endregion

        #region Check Target Stage        
        /// <summary>
        /// To Check Target Stage with valid data.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Check_Target_Stage()
        {

            var routes = new RouteCollection();
            Console.WriteLine("To Check Target Stage with valid data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            int StageId = DataHelper.GetStageId(Sessions.User.ClientId);
            var result = objModelController.CheckTargetStage(ModelId, StageId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Check Target Stage with invalid data.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Check_Target_Stage_inValid()
        {

            var routes = new RouteCollection();
            Console.WriteLine("To Check Target Stage with invalid data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = 0;
            int StageId = 0;
            var result = objModelController.CheckTargetStage(ModelId, StageId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Check Target Stage BenchMark
        /// <summary>
        /// To Check Target Stage BenchMark.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Check_Target_Stage_BenchMark()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check Target Stage BenchMark.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objModelController.Url = new UrlHelper(
                new RequestContext(
                    objModelController.HttpContext, new RouteData()
                    ),
                routes);
            var result = objModelController.CheckTargetStageBenchMark() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Check Duplicate Model Title
        /// <summary>
        /// To Check Duplicate Model Title Exist.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Check_Duplicate_Model_Title_Exist()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check Duplicate Model Title Exist.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string ModelTitle = DataHelper.GetModel(ModelId).Title;
            var result = objModelController.CheckDuplicateModelTitle(ModelTitle) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }

        /// <summary>
        /// To Check Duplicate Model Title Not Exist.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Check_Duplicate_Model_Title_NotExist()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Check Duplicate Model Title Not Exist.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string ModelTitle = DataHelper.GetModel(ModelId).Title + "Not Exist";
            var result = objModelController.CheckDuplicateModelTitle(ModelTitle) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get Model Data
        /// <summary>
        /// To Get Model Data with Passing ModelId.    
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Model_Data_with_Passing_ModelId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Model Data with Passing ModelId  .\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.GetModelData(ModelId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Get Model Data without Passing ModelId.     
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Model_Data_without_Passing_ModelId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Model Data without Passing ModelId..\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.GetModelData() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region No model exist in case of current year
        /// <summary>
        /// To Action method that reurns view for no model exist in case of current year, business unit of logged-in user.    
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Model_Zero()
        {
            var routes = new RouteCollection();
            Console.WriteLine("no model exist in case of current year.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.ModelZero() as ViewResult;

            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ActiveMenu"];
            var resultvalue1 = serializedData["ModelExists"];
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue1:  " + resultvalue1.ToString());
            Assert.IsNotNull(resultvalue1.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName:  " + result.ViewName);

        }
        #endregion

        #region Get Model List
        /// <summary>
        /// To Get Active Model List.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Active_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Active Model List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string listtype = "active";
            var result = objModelController.GetModelList(listtype) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Get All Model List.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_All_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get All Model List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string listtype = "all";
            var result = objModelController.GetModelList(listtype) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Get Invalid Model List.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Invalid_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Invalid Model List.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string listtype = "Invalid";
            var result = objModelController.GetModelList(listtype) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Load Model Overview
        /// <summary>
        /// To Load Model Overview for New Model.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Load_Model_Overview_New_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Load Model Overview for New Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string ModelTitle = "Baseline Model";
            int New_ModelId = 0;
            var result = objModelController.LoadModelOverview(ModelTitle, New_ModelId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
          
        }

        /// <summary>
        /// To Load Model Overview for Existing Model.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Load_Model_Overview_Existing_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Load Model Overview for Existing Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            string ModelTitle = DataHelper.GetModel(ModelId).Title;
            var result = objModelController.LoadModelOverview(ModelTitle, ModelId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }
        #endregion

        #region Load Contact Inquiry
        /// <summary>
        /// To Load Contact Inquiry.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Load_Contact_Inquiry()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Load Contact Inquiry.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            double Msize = DataHelper.GetModel(ModelId).AverageDealSize;
            var result = objModelController.LoadContactInquiry(Msize) as PartialViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["MarketingDealSize"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);            
            Assert.IsNotNull(result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());           
        }

        #endregion

        #region Get TacticType List for Model
        /// <summary>
        /// To Get TacticType List for Model without Passing Parameter.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_TacticTypeList_Without_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get TacticType List for Model without Passing Parameter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.Tactics() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }

        /// <summary>
        /// To Get TacticType List for Model with Passing Parameter.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_TacticTypeList_With_Parameter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get TacticType List for Model with Passing Parameter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.Tactics() as ViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
        }

        #endregion

        #region Fill Model Version
        /// <summary>
        /// To Fill Model Version.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Fill_Model_Version()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Fill Model Version.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.FillVersion(ModelId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get Tactic Type Data by ModelId
        /// <summary>
        /// To Get Tactic Type Data by ModelId.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_TacticType_Data()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Tactic Type Data by ModelId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.GetTacticDatabyid(ModelId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data :  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Get Detail Tactic Type Data.
        /// <summary>
        /// To Get Tactic Type Data.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Detail_TacticType_Without_TacticTypeId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Tactic Type Data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            int TacticTypeId = DataHelper.GetTacticTypeId(ModelId);
            var result = objModelController.DetailTacticData(TacticTypeId, ModelId) as PartialViewResult;
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ModelStatus"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
        }

        #endregion

        #region Create New Tactic Type For Model.
        /// <summary>
        /// To Create New Tactic Type For Model.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Create_New_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Create New Tactic Type For Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.CreateTacticData(ModelId) as PartialViewResult;


            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["IsCreated"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName :  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.AreEqual("True", resultvalue.ToString(), true);
           
        }
        #endregion

        #region Delete Tactic Type From Model.
        /// <summary>
        /// To Delete Tactic Type From Model.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Delete_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Delete Tactic Type From Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetDeletedModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            int TacticTypeId = DataHelper.GetdeletedTacticTypeId(ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.DeleteTactic(TacticTypeId, Sessions.User.UserId.ToString()) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Delete Tactic Type From Model without Passing UserId.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Delete_TacticType_without_Passing_UserId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Delete Tactic Type From Model without Passing UserId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetDeletedModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            int TacticTypeId = DataHelper.GetdeletedTacticTypeId(ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.DeleteTactic(TacticTypeId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }
        #endregion

        #region Save Tactic Type.
        /// <summary>
        /// To Save Existing Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Save_Existing_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Save Existing Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            TacticType TaskData = DataHelper.GetTacticType(ModelId);
            if (TaskData != null)
            {
                string Title = TaskData.Title;
                string Description = TaskData.Description;
                int Stageid = Convert.ToInt32(TaskData.StageId);
                double ProjectedStageValue = Convert.ToDouble(TaskData.ProjectedStageValue);
                double ProjectedRevenue = Convert.ToDouble(TaskData.ProjectedRevenue);
                int TacticTypeId = Convert.ToInt32(TaskData.TacticTypeId);
                bool isDeployedToIntegration = TaskData.IsDeployedToIntegration;
                bool isDeployedToModel = TaskData.IsDeployedToModel;
                string WorkFrontTemplate = TaskData.WorkFrontTemplateId.ToString();
                string AssetType = TaskData.AssetType;
                string ProgramType = "";
                string Channel = "";
                bool DeleteAllPackage = false;

                var result = objModelController.SaveTactic(Title, Description, Stageid, ProjectedStageValue, ProjectedRevenue, TacticTypeId, ModelId.ToString(), isDeployedToIntegration, isDeployedToModel, WorkFrontTemplate, AssetType, ProgramType, Channel, DeleteAllPackage) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }
           
        }

        /// <summary>
        /// To Save New Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Save_New_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Save New Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            TacticType TaskData = DataHelper.GetTacticType(ModelId);
            if (TaskData != null)
            {
                string Title = TaskData.Title + "Test";
                string Description = TaskData.Description + "Test";
                int Stageid = Convert.ToInt32(TaskData.StageId);
                double ProjectedStageValue = Convert.ToDouble(TaskData.ProjectedStageValue);
                double ProjectedRevenue = Convert.ToDouble(TaskData.ProjectedRevenue);
                int TacticTypeId = 0;
                bool isDeployedToIntegration = TaskData.IsDeployedToIntegration;
                bool isDeployedToModel = TaskData.IsDeployedToModel;
                string WorkFrontTemplate = TaskData.WorkFrontTemplateId.ToString();
                string AssetType = "";

                var result = objModelController.SaveTactic(Title, Description, Stageid, ProjectedStageValue, ProjectedRevenue, TacticTypeId, ModelId.ToString(), isDeployedToIntegration, isDeployedToModel, WorkFrontTemplate, AssetType) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }
        }
        #endregion

        #region Save All Tactic Type.
        /// <summary>
        /// To Save All Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Save_All_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Save All Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Model ModelData = DataHelper.GetModel(ModelId);
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            
            List<TacticType> TaskData = DataHelper.GetTacticTypeList(ModelId);
            Sessions.User.UserId = ModelData.CreatedBy;
            if (TaskData != null && TaskData.Count > 0)
            {
                List<string> tacticIds = new List<string>();
                List<string> tacticIds1 = new List<string>();
                List<string> alltacticIds = new List<string>();
                List<string> rejtacticIds = new List<string>();
                tacticIds = TaskData.Where(tt => tt.IsDeployedToModel == true && tt.IsDeployedToIntegration == false).Select(tt => tt.TacticTypeId + "_false").ToList();
                tacticIds1 = TaskData.Where(tt => tt.IsDeployedToModel == true && tt.IsDeployedToIntegration == true).Select(tt => tt.TacticTypeId + "_true").ToList();
                List<string> test = TaskData.Where(tt => tt.IsDeployedToModel == false).Select(tt => tt.TacticTypeId + "_false").ToList();
                alltacticIds.AddRange(tacticIds);
                alltacticIds.AddRange(tacticIds1);
                string ids = string.Join(",", alltacticIds);
                string rejIds = string.Join(",", rejtacticIds);
                var ModelStatus = ModelData.Status;
                bool isModelPublished = true;
                string EffectiveDate = ModelData.EffectiveDate.ToString();
                if (ModelStatus == Enums.ModelStatus.Published.ToString())
                {
                    isModelPublished = true;
                }
                var result = objModelController.saveAllTactic(ids, rejIds, ModelId, isModelPublished, EffectiveDate) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
              
            }
        }
        #endregion

        #region Get Plan Tactic(s)
        /// <summary>
        /// To Get Plan Tactic(s).
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Plan_Tactic()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Get Plan Tactic(s).\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Model ModelData = DataHelper.GetModel(ModelId);
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            List<TacticType> TaskData = DataHelper.GetTacticTypeList(ModelId);
            if (TaskData != null && TaskData.Count > 0)
            {
                List<int> tacticIds = new List<int>();
                tacticIds = TaskData.Where(tt => tt.IsDeployedToModel == true).Select(tt => tt.TacticTypeId).ToList();
                List<string> test = TaskData.Where(tt => tt.IsDeployedToModel == false).Select(tt => tt.TacticTypeId + "_false").ToList();
                var result = objModelController.GetPlanTacticsByTacticType(ModelId, tacticIds);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data result:  " + result);
                Assert.IsNotNull(result);               
            }
        }
        #endregion

        #region Action method to show model integration screen
        /// <summary>
        /// To Action method to show model integration screen.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Integration()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Action method to show model integration screen.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.Integration(ModelId) as ViewResult;

            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ActiveMenu"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
           
        }
        #endregion

        #region Get Integration Data by Model Id
        /// <summary>
        /// Action method to show Integrations List by ModelId.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Integration_Data()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Action method to show Integrations List by ModelId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            var result = objModelController.GetIntegrationDatabyid(ModelId) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion              

        #region Publish Model
        /// <summary>
        /// To Publish Model.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Model_Pubish()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Publish Model.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            Model TaskData = DataHelper.GetModel(ModelId);
            if (TaskData != null)
            {
                string EffectiveDate = TaskData.EffectiveDate.ToString();
                var result = objModelController.ModelPublish(ModelId, EffectiveDate) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }
        #endregion

        #region Check Model Publish or Not
        /// <summary>
        /// To Check Model Publish or Not.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Pubish_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check Model Publish or Not.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            Model TaskData = DataHelper.GetModel(ModelId);
            if (TaskData != null)
            {
                string EffectiveDate = TaskData.EffectiveDate.ToString();
                bool isTacticTypeExist = false;
                bool isModelPublished = objModelController.PublishModel(ModelId, EffectiveDate, out isTacticTypeExist);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value isModelPublished:  " + isModelPublished);
                Assert.IsNotNull(isModelPublished);
                
            }

        }
        #endregion

        #region Check Duplicate Model or Not
        /// <summary>
        /// To Check Duplicate Model or Not.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Duplicate_Model()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check Duplicate Model or Not.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            Model TaskData = DataHelper.GetModel(ModelId);
            if (TaskData != null)
            {
                string Title = TaskData.Title;

                var result = objModelController.DuplicateModel(ModelId, Title) as JsonResult;
                
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["msg"];
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
             
            }

        }

        /// <summary>
        /// To Check Duplicate Model or Not with Invalid Data.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Duplicate_Model_Invalid()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check Duplicate Model or Not with Invalid Data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            Model TaskData = DataHelper.GetModel(ModelId);
            if (TaskData != null)
            {
                string Title = TaskData.Title;

                var result = objModelController.DuplicateModel(0, Title) as JsonResult;
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["msg"];
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
                Assert.IsNotNull(resultvalue.ToString());
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
              
            }

        }
        #endregion

        #region Get Default Duplicate Model Name
        /// <summary>
        /// To Get Default Duplicate Model Name.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Default_Duplicate_Model_Name()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Default Duplicate Model Name.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.GetDefaultDuplicateModelName(ModelId) as JsonResult;

            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["msg"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
           
        }

        /// <summary>
        /// To Get Default Duplicate Model Name with Invalid ModelId.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Get_Default_Duplicate_Model_Name_Invalid_ModelId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Default Duplicate Model Name with Invalid ModelId.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.GetDefaultDuplicateModelName(0) as JsonResult;

            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["msg"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
           
        }
        #endregion

        #region Check Duplicate Model Title or Not
        /// <summary>
        /// To Check Duplicate Model Title or Not.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Duplicate_Model_Title_ModelId()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Check Duplicate Model Title or Not.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            Model TaskData = DataHelper.GetModel(ModelId);
            if (TaskData != null)
            {
                string Title = TaskData.Title;

                var result = objModelController.CheckDuplicateModelTitleByID(Title, ModelId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);

            }

        }
        #endregion

        #region Get Integration Instances for Integration Selection
        /// <summary>
        /// To Get Integration Instances for Integration Selection.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Integration_Selection()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Integration Instances for Integration Selection.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.IntegrationSelection(ModelId) as ViewResult;

            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ActiveMenu"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);
           
        }
        #endregion

        #region Get Integration Overview for Integration Selection
        /// <summary>
        /// To Get Integration Overview for Integration Selection.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>14July2016</createddate>
        [TestMethod]
        public void Integration_Overview()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Integration Overview for Integration Selection.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ModelController objModelController = new ModelController();
            objModelController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objModelController);
            objModelController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.ClientId = DataHelper.GetClientId(0, ModelId);
            Sessions.User.UserId = DataHelper.GetUserId(0, ModelId);
            var result = objModelController.IntegrationOverview(ModelId) as ViewResult;

            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["ActiveMenu"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value ViewName:  " + result.ViewName);
            Assert.IsNotNull(result.ViewName);          
        }
        #endregion
    }
}


