﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class BoostControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        #region Best In Class
        /// <summary>
        /// To Return BestInClass View.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Best_In_Class()
        {
            var routes = new RouteCollection();
            Console.WriteLine("To Return BestInClass View.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            var result = objBoostController.BestInClass() as ViewResult;
            List<BestInClassModel> objModelList = (List<BestInClassModel>)result.Model;
            BestInClassModel objModel = objModelList.FirstOrDefault();
            var serializedData = new RouteValueDictionary(objModel);
            var resultvalue = serializedData["StageName"];
            Assert.IsNotNull(result.Model);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value: result.Model  " + result.Model);
            Assert.IsNotNull(resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value: resultvalue " + resultvalue.ToString());
        }
        #endregion

        #region Action to Save BIC
        /// <summary>
        /// Action to Save BIC.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Save_BIC()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Action to Save BIC.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            int StageId = DataHelper.GetStageId(Sessions.User.CID);
            var TaskData = DataHelper.GetStageData(StageId);
            List<BestInClassModel> objBestinClassModelList = new List<BestInClassModel>();
            BestInClassModel objBestinClassModel = new BestInClassModel();
            objBestinClassModel.StageID_CR = StageId;
            objBestinClassModel.StageID_SV = StageId;
            objBestinClassModel.StageID_Size = StageId;
            objBestinClassModel.StageName = "CR";
            objBestinClassModel.StageType = Enums.StageType.CR.ToString();
            objBestinClassModel.ConversionValue = 10.0;
            objBestinClassModel.VelocityValue = 10.0;

            objBestinClassModelList.Add(objBestinClassModel);

            if (objBestinClassModelList != null && objBestinClassModelList.Count > 0)
            {
                var result = objBoostController.SaveBIC(objBestinClassModelList) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }
        }

        #endregion

        #region Action to show Improvement Tactic Type list.
        /// <summary>
        /// Action to show Improvement Tactic Type list..
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Index()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Action to show Improvement Tactic Type list..\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            var result = objBoostController.Index() as ActionResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result:  " + result);
            Assert.IsNotNull(result);

        }
        #endregion

        #region Get Improvement Tactic Type list for Boost
        /// <summary>
        /// Get Improvement Tactic Type list for Boost.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Get_Improvement_TacticType_List_Boost()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Improvement Tactic Type list for Boost.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            var result = objBoostController.ImprovementTacticList() as JsonResult;
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["Count"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.IsNotNull(result.Data);
            Assert.IsNotNull(resultvalue.ToString());

        }
        #endregion

        #region Get Deatail Improvement Tactic Type for Boost
        /// <summary>
        /// Get Deatail Improvement Tactic Type for Boost.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Get_Detail_Improvement_TacticType_Boost()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Deatail Improvement Tactic Type for Boost.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            int ImprovementTacticTypeId = DataHelper.GetImprovementTacticTypeId(Sessions.User.CID);

            var result = objBoostController.DetailImprovementTacticData(ImprovementTacticTypeId) as PartialViewResult;
            Assert.IsNotNull(result.ViewName);
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["Title"];
            var resultvalue1 = serializedData["CanDelete"];
            var resultvalue2 = serializedData["IsCreated"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " Title: " + resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " CanDelete: " + resultvalue1.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " IsCreated: " + resultvalue2.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Assert.IsNotNull(resultvalue1.ToString());
            Assert.IsNotNull(resultvalue2.ToString());

        }

        /// <summary>
        /// Get Deatail Improvement Tactic Type for Boost.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Get_Detail_Improvement_TacticType_Boost_for_New_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Deatail Improvement Tactic Type for Boost.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);

            var result = objBoostController.DetailImprovementTacticData(0) as PartialViewResult;

            Assert.IsNotNull(result.ViewName);
            var serializedData = new RouteValueDictionary(result.ViewData);
            var resultvalue = serializedData["Title"];
            var resultvalue1 = serializedData["CanDelete"];
            var resultvalue2 = serializedData["IsCreated"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " Title: " + resultvalue.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " CanDelete: " + resultvalue1.ToString());
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value:  " + " IsCreated: " + resultvalue2.ToString());
            Assert.IsNotNull(resultvalue.ToString());
            Assert.IsNotNull(resultvalue1.ToString());
            Assert.IsNotNull(resultvalue2.ToString());
            
        }
        #endregion

        #region Save Improvement Tactic Type Details for Boost
        /// <summary>
        /// Save Improvement Tactic Type Details for Existing Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Save_Improvement_TacticType_Old()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Improvement Tactic Type Details for Existing Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);

            ImprovementTacticType ImprovementTacticTypeData = DataHelper.GetImprovementTacticType(Sessions.User.CID);
            if (ImprovementTacticTypeData != null)
            {
                int improvementId = ImprovementTacticTypeData.ImprovementTacticTypeId;
                string improvementDetails = "[{\"StageId\":\"106\",\"StageType\":\"SV\",\"Value\":\"5\"}]";
                bool status = ImprovementTacticTypeData.IsDeleted;
                double cost = ImprovementTacticTypeData.Cost;
                string desc = ImprovementTacticTypeData.Description;
                string title = ImprovementTacticTypeData.Title;
                bool deployToIntegrationStatus = ImprovementTacticTypeData.IsDeployedToIntegration;
                int UserId = ImprovementTacticTypeData.CreatedBy;
                var result = objBoostController.saveImprovementTacticData(improvementId, improvementDetails, status, cost, desc, title, deployToIntegrationStatus, UserId) as JsonResult;
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.Data:  " + result.Data);
                Assert.IsNotNull(result.Data);
            }

        }

        /// <summary>
        ///  Save Improvement Tactic Type Details for New Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Save_Improvement_TacticType_New()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Improvement Tactic Type Details New Existing Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            Sessions.User.ID = DataHelper.GetUserId(0, ModelId);

            int improvementId = 0;
            string improvementDetails = "[{\"StageId\":\"106\",\"StageType\":\"SV\",\"Value\":\"5\"}]";
            bool status = true;
            double cost = 25000;
            string desc = "";
            string title = "New Improvement Tactic Type Test 1234";
            bool deployToIntegrationStatus = false;
            int UserId = Sessions.User.ID;
            var result = objBoostController.saveImprovementTacticData(improvementId, improvementDetails, status, cost, desc, title, deployToIntegrationStatus, UserId) as JsonResult;            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            
        }
        #endregion

        #region Save Deployed To Integration Status for Improvement Tactic Type
        /// <summary>
        /// Save Deployed To Integration Status for Improvement Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Save_Deploye_To_Integration_Improvement_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Deployed To Integration Status for Improvement Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);

            ImprovementTacticType ImprovementTacticTypeData = DataHelper.GetImprovementTacticType(Sessions.User.CID);

            if (ImprovementTacticTypeData != null)
            {
                Sessions.User.ID = ImprovementTacticTypeData.CreatedBy;
                int improvementId = ImprovementTacticTypeData.ImprovementTacticTypeId;
                bool deployToIntegrationStatus = ImprovementTacticTypeData.IsDeployedToIntegration;
                int UserId = ImprovementTacticTypeData.CreatedBy;
                var result = objBoostController.SaveDeployedToIntegrationStatus(improvementId, deployToIntegrationStatus, UserId) as JsonResult;
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["message"];
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data :  " + result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value resultvalue:  " + resultvalue.ToString());
                Assert.IsNotNull(resultvalue.ToString());
                Assert.IsNotNull(result.Data);
            }
        }
        #endregion

        #region Save Deployed Status for Improvement Tactic Type
        /// <summary>
        /// Save Deployed Status for Improvement Tactic Type.
        /// </summary>
        /// <auther>Rahul Shah</auther>
        /// <createddate>15July2016</createddate>
        [TestMethod]
        public void Save_Deploye_Improvement_TacticType()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Save Deployed Status for Improvement Tactic Type.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            BoostController objBoostController = new BoostController();
            objBoostController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objBoostController);
            objBoostController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            int ModelId = DataHelper.GetModelId();
            Sessions.User.CID = DataHelper.GetClientId(0, ModelId);
            ImprovementTacticType ImprovementTacticTypeData = DataHelper.GetImprovementTacticType(Sessions.User.CID);
            if (ImprovementTacticTypeData != null)
            {
                Sessions.User.ID= ImprovementTacticTypeData.CreatedBy;
                int improvementId = ImprovementTacticTypeData.ImprovementTacticTypeId;
                bool deploye = ImprovementTacticTypeData.IsDeployed;
                int UserId = ImprovementTacticTypeData.CreatedBy;
                var result = objBoostController.SaveDeployedToIntegrationStatus(improvementId, deploye, UserId) as JsonResult;
                var serializedData = new RouteValueDictionary(result.Data);
                var resultvalue = serializedData["message"];                
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value result.data:  " + result.Data);
                Assert.IsNotNull(result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value resultvalue :  " + resultvalue.ToString());
                Assert.IsNotNull(resultvalue.ToString());
            }
        }
        #endregion

    }
}
