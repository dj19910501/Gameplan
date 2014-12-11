﻿using System;
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
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            Guid clientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ClientId;
            string result = controller.TestGenerateCustomName(null, clientId);
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
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            string result = controller.TestGenerateCustomName(null, Guid.Empty);
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
            IntegrationEloquaClient controller = new IntegrationEloquaClient();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            Guid clientId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ClientId;
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            objTactic=DataHelper.GetPlanTactic(clientId);
            string result = controller.TestGenerateCustomName(objTactic, clientId);
            Assert.IsTrue(!string.IsNullOrEmpty(result));
            
        }
        #endregion
    }
}
