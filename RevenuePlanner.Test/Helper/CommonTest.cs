using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevenuePlanner.Models;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Integration.Helper;
using Integration.Eloqua;
using System.Web;
using RevenuePlanner.Test.MockHelpers;

namespace RevenuePlanner.Test.Helper
{
    [TestClass]
    public class CommonTest
    {
        #region Custom Naming Structure
        /// <summary>
        /// To Check Generate custom name with null object
        /// Added by Mitesh Vaishnav on 04/12/2014
        /// #1000 - Custom naming: Campaign name structure
        /// </summary>
        [TestMethod]
        public void Generate_Custom_Name_With_NULL_Object()
        {
            Console.WriteLine("To Check Generate custom name with null object.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int clientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).CID;
            string result = controller.TestGenerateCustomName(null, clientId);
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result:  " + result);
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// To Check Generate custom name with null object and empty GUID
        /// Added by Mitesh Vaishnav on 04/12/2014
        /// #1000 - Custom naming: Campaign name structure
        /// </summary>
        [TestMethod]
        public void Generate_Custom_Name_With_NULL_Object_And_Empty_GUID()
        {
            Console.WriteLine("To Check Generate custom name with null object and empty GUID.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            string result = controller.TestGenerateCustomName(null, 0);
           
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result:  " + result);
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// To Check Generate custom name with tactic object
        /// Added by Mitesh Vaishnav on 04/12/2014
        /// #1000 - Custom naming: Campaign name structure
        /// </summary>
        [TestMethod]
        public void Generate_Custom_Name_With_Tactic_Object()
        {
            Console.WriteLine("To Check Generate custom name with tactic object.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int clientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).CID;
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            objTactic = DataHelper.GetPlanTactic(clientId);
            string result = controller.TestGenerateCustomName(objTactic, clientId);
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result:  " + result);
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }


        #endregion

        [TestMethod]
        public void GetAllCustomFields_With_Empty_TacticIds_List()
        {
            Console.WriteLine("To Get All custom fields name with empty tactic object.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            List<int> tacticIds = new List<int>();
            List<ViewByModel> CustomFields = RevenuePlanner.Helpers.Common.GetCustomFields(tacticIds, tacticIds, tacticIds);
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value CustomFields.Count:  " + CustomFields.Count);
            Assert.AreEqual(0, CustomFields.Count);
        }

        //[TestMethod] This is a invalid test case List is can be empty but never null.
        //public void GetAllCustomFields_With_NULL_TacticIds_List()
        //{
        //    HttpContext.Current = DataHelper.SetUserAndPermission();
        //    List<ViewByModel> CustomFields = RevenuePlanner.Helpers.Common.GetCustomFields(null, null, null);
        //    Assert.AreEqual(0, CustomFields.Count);
        //}

        [TestMethod]
        public void GetAllCustomFields_With_TacticIds_List()
        {
            Console.WriteLine("To Get All custom fields name with tactic object.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            int clientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).CID;
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            objTactic = DataHelper.GetPlanTactic(clientId);
            List<int> tacticIds = new List<int>();
            tacticIds.Add(objTactic.PlanTacticId);
            List<int> programIds = new List<int>();
            tacticIds.Add(objTactic.PlanProgramId);
            List<int> campaignIds = new List<int>();
            tacticIds.Add(objTactic.Plan_Campaign_Program.PlanCampaignId);
            List<ViewByModel> CustomFields = RevenuePlanner.Helpers.Common.GetCustomFields(tacticIds, programIds, campaignIds);
            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value CustomFields.Count:  " + CustomFields.Count);
            Assert.IsNotNull(CustomFields.Count);
        }
    }
}
