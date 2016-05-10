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
    public class ClonehelperTest
    {
        /// <summary>
        ///  To check to identify the clone type with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ToClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ToClone(string.Empty, string.Empty, 0, 0);

            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
            
        }

        /// <summary>
        ///  To check to identify the clone type with Invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ToClone_with_InvalidParameter()
        {
            string _invalidcopyparam = "invalid";
            string _invalidsuffixyparam = "invalid";
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ToClone(_invalidsuffixyparam, _invalidcopyparam, 0, 0);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.PlanClone(0, string.Empty, Guid.Empty, string.Empty, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_planId_userid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.PlanClone(planid, string.Empty, UserId, string.Empty, string.Empty);
            Assert.IsNotNull(_clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with some invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_invalidParam()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            string _invalidparam = "invalid";
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.PlanClone(planid, _invalidparam, UserId, _invalidparam, _invalidparam);
            Assert.IsNotNull(_clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with empty paramter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CampaignClone(0, string.Empty, Guid.Empty, 0, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_planid_userid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.CampaignClone(planid, string.Empty, UserId, 0, string.Empty);
            Assert.IsNotNull(_clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_invalidParam()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string invalidparam = "invalid";
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.CampaignClone(planid, invalidparam, UserId, 0, invalidparam);
            Assert.IsNotNull(_clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }


        /// <summary>
        ///  To check to identify the Clone the Program and it's All Child element with empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ProgramClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ProgramClone(0, string.Empty, Guid.Empty, 0, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Program and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ProgramClone_with_planid_userid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int _clone = Clonehelper.ProgramClone(planid, string.Empty, UserId, 0, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Tactic and it's All Child element with empty paramater
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void TacticClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.TacticClone(0, string.Empty, Guid.Empty, 0, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the LineItem and it's All Child element with empty paramater
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LineItemClone_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LineItemClone(0, string.Empty, Guid.Empty, 0, string.Empty);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the clone type with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneToOtherPlan(null, string.Empty, 0, 0, 0, false);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify the clone type with entityid,clonetype,userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneToOtherPlan_with_entityid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();

            string clonetype = Enums.EntityType.Tactic.ToString();

            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();
            int _clone = Clonehelper.CloneToOtherPlan(null, clonetype, Convert.ToInt32(entityId.Split('_')[1]), Convert.ToInt32(entityId.Split('_')[0]), 0, false);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneTacticToOtherPlan(0, Guid.Empty, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_userid_planid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int _clone = Clonehelper.CloneTacticToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with planid,userid,entityid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_entityid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();

            int _clone = Clonehelper.CloneTacticToOtherPlan(Convert.ToInt32(entityId.Split('_')[0]), UserId, Convert.ToInt32(entityId.Split('_')[1]), 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Campaign and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneCampaignToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneCampaignToOtherPlan(0, Guid.Empty, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Campaign and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneCampaignToOtherPlan_with_userid_planid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int _clone = Clonehelper.CloneCampaignToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Program and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneProgramToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneProgramToOtherPlan(0, Guid.Empty, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Program and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneProgramToOtherPlan_with_userid_planid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int _clone = Clonehelper.CloneProgramToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify identify the clone type  with emmpty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LinkToOtherPlan(null, string.Empty, 0, 0, 0, false);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify identify the clone type  with entityid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkToOtherPlan_with_entityid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();
            int _clone = Clonehelper.LinkToOtherPlan(null, string.Empty, Convert.ToInt32(entityId.Split('_')[1]), Convert.ToInt32(entityId.Split('_')[0]), 0, false);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_empty()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LinkTacticToOtherPlan(0, Guid.Empty, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_userid_planid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            int _clone = Clonehelper.LinkTacticToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }
        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_invalidValue()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string _invalid = "invalid";
            int _clone = Clonehelper.LinkTacticToOtherPlan(planid, UserId, 0, 0, _invalid, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with planid,entityid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_entityid()
        {
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            Guid UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).UserId;
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();

            int _clone = Clonehelper.LinkTacticToOtherPlan(Convert.ToInt32(entityId.Split('_')[0]), UserId, Convert.ToInt32(entityId.Split('_')[1]), 0, string.Empty, false, null);
            Assert.AreEqual(0, _clone);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Pass – And result value is " + _clone);
        }
    }
}
