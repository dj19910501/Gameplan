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
            if (objTacticType != null)
            {
                int? InstanceId = objTacticType.Model.IntegrationInstanceMarketoID; //get marketo instance id
                int EntityID = objTacticType.TacticTypeId;
                string EntityType = Enums.FilterLabel.TacticType.ToString();
                ApiIntegration ObjApiintegration = new ApiIntegration(Enums.ApiIntegrationData.Programtype.ToString(), InstanceId);
                MarketoDataObject CampaignFolderList = ObjApiintegration.GetProgramChannellistData();

                string ProgramType = CampaignFolderList.program.Select(list => list.Key).FirstOrDefault();
                string Channel = CampaignFolderList.channels.Select(list => list.name).FirstOrDefault();

                ModelController objModelController = new ModelController();
                objModelController.SaveMarketoSettings(EntityID, InstanceId, EntityType, ProgramType, Channel);
                Assert.IsTrue(true);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + true);
            }
            else
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + "Marketo data not found.");
            }

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
            if (result != null)
            {
                Assert.AreNotEqual(null, result.Data);
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + result.Data);
            }
            else
            {

                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Fail \n The Assert Value:  " + result.Data);
            }
        }


        #endregion

    }
}
