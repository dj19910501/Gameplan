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
    public class ClonehelperTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        /// <summary>
        ///  To check to identify the clone type with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ToClone_with_empty()
        {
            Console.WriteLine("To check to identify the clone type with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ToClone(string.Empty, string.Empty, 0, 0);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
           

        }

        /// <summary>
        ///  To check to identify the clone type with Invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ToClone_with_InvalidParameter()
        {
            Console.WriteLine("To check to identify the clone type with Invalid parameter.\n");
            string _invalidcopyparam = "invalid";
            string _invalidsuffixyparam = "invalid";
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ToClone(_invalidsuffixyparam, _invalidcopyparam, 0, 0);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);

        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_empty()
        {
            Console.WriteLine("To check to identify the Clone the Plan and it's All Child element with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.PlanClone(0, string.Empty, 0, string.Empty, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_planId_userid()
        {
            Console.WriteLine("To check to identify the Clone the Plan and it's All Child element with planid,userid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.PlanClone(planid, string.Empty, UserId, string.Empty, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.IsNotNull(_clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Plan and it's All Child element with some invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void PlanClone_with_invalidParam()
        {
            Console.WriteLine("To check to identify the Clone the Plan and it's All Child element with some invalid parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            string _invalidparam = "invalid";
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.PlanClone(planid, _invalidparam, UserId, _invalidparam, _invalidparam);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.IsNotNull(_clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with empty paramter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_empty()
        {
            Console.WriteLine("To check to identify the Clone the Campaign and it's All Child element with empty paramter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CampaignClone(0, string.Empty, 0, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_planid_userid()
        {
            Console.WriteLine("To check to identify the Clone the Campaign and it's All Child element with userid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.CampaignClone(planid, string.Empty, UserId, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Campaign and it's All Child element with invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CampaignClone_with_invalidParam()
        {
            Console.WriteLine("To check to identify the Clone the Campaign and it's All Child element with invalid parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            string invalidparam = "invalid";
            int planid = DataHelper.GetPlanId();
            int _clone = Clonehelper.CampaignClone(planid, invalidparam, UserId, 0, invalidparam);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }


        /// <summary>
        ///  To check to identify the Clone the Program and it's All Child element with empty
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ProgramClone_with_empty()
        {
            Console.WriteLine("To check to identify the Clone the Program and it's All Child element with empty.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.ProgramClone(0, string.Empty, 0, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Program and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void ProgramClone_with_planid_userid()
        {
            Console.WriteLine("To check to identify the Clone the Program and it's All Child element with userid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int _clone = Clonehelper.ProgramClone(planid, string.Empty, UserId, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the Tactic and it's All Child element with empty paramater
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void TacticClone_with_empty()
        {
            Console.WriteLine("To check to identify the Clone the Tactic and it's All Child element with empty paramater.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.TacticClone(0, string.Empty, 0, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the Clone the LineItem and it's All Child element with empty paramater
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LineItemClone_with_empty()
        {
            Console.WriteLine("To check to identify the Clone the LineItem and it's All Child element with empty paramater.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LineItemClone(0, string.Empty, 0, 0, string.Empty);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the clone type with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify the clone type with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneToOtherPlan(null, string.Empty, 0, 0, 0, false);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify the clone type with entityid,clonetype,userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneToOtherPlan_with_entityid()
        {
            Console.WriteLine("To check to identify the clone type with entityid,clonetype,userid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            //string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();

            string clonetype = Enums.EntityType.Tactic.ToString();

            //List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> lstPlanids = db.Plans.Where(pl => pl.CreatedBy == UserId).Select(pl => pl.PlanId).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();
            int _clone = Clonehelper.CloneToOtherPlan(null, clonetype, Convert.ToInt32(entityId.Split('_')[1]), Convert.ToInt32(entityId.Split('_')[0]), 0, false);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify Clone the Tactic and it's All Child element with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneTacticToOtherPlan(0, 0, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_userid_planid()
        {
            Console.WriteLine("To check to identify Clone the Tactic and it's All Child element with planid,userid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int _clone = Clonehelper.CloneTacticToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Tactic and it's All Child element with planid,userid,entityid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneTacticToOtherPlan_with_entityid()
        {
            Console.WriteLine("To check to identify Clone the Tactic and it's All Child element with planid,userid,entityid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            //string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            //List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> lstPlanids = db.Plans.Where(pl => pl.CreatedBy == UserId && pl.IsDeleted == false).Select(pl => pl.PlanId).Take(1).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId) && id.IsDeleted == false).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();

            int _clone = Clonehelper.CloneTacticToOtherPlan(Convert.ToInt32(entityId.Split('_')[0]), UserId, Convert.ToInt32(entityId.Split('_')[1]), 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Campaign and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneCampaignToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify Clone the Campaign and it's All Child element with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneCampaignToOtherPlan(0, 0, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Campaign and it's All Child element with planid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneCampaignToOtherPlan_with_userid_planid()
        {
            Console.WriteLine("To check to identify Clone the Campaign and it's All Child element with planid,userid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int _clone = Clonehelper.CloneCampaignToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Program and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneProgramToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify Clone the Program and it's All Child element with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.CloneProgramToOtherPlan(0, 0, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Clone the Program and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void CloneProgramToOtherPlan_with_userid_planid()
        {
            Console.WriteLine("To check to identify Clone the Program and it's All Child element with userid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int _clone = Clonehelper.CloneProgramToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify identify the clone type  with emmpty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify identify the clone type  with emmpty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LinkToOtherPlan(null, string.Empty, 0, 0, 0, false);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify identify the clone type  with entityid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkToOtherPlan_with_entityid()
        {
            Console.WriteLine("To check to identify identify the clone type  with entityid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            //string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            //List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> lstPlanids = db.Plans.Where(pl => pl.CreatedBy == UserId).Select(pl => pl.PlanId).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();
            int _clone = Clonehelper.LinkToOtherPlan(null, string.Empty, Convert.ToInt32(entityId.Split('_')[1]), Convert.ToInt32(entityId.Split('_')[0]), 0, false);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with empty parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_empty()
        {
            Console.WriteLine("To check to identify Link the Tactic and it's All Child element with empty parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int _clone = Clonehelper.LinkTacticToOtherPlan(0, 0, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with userid,planid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_userid_planid()
        {
            Console.WriteLine("To check to identify Link the Tactic and it's All Child element with userid,planid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            int _clone = Clonehelper.LinkTacticToOtherPlan(planid, UserId, 0, 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }
        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with invalid parameter
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_invalidValue()
        {
            Console.WriteLine("To check to identify Link the Tactic and it's All Child element with invalid parameter.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            int planid = DataHelper.GetPlanId();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            string _invalid = "invalid";
            int _clone = Clonehelper.LinkTacticToOtherPlan(planid, UserId, 0, 0, _invalid, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }

        /// <summary>
        ///  To check to identify Link the Tactic and it's All Child element with planid,entityid,userid
        /// </summary>
        /// <auther>Dashrath Prajapati</auther>
        /// <createddate>29/01/2015</createddate>
        [TestMethod]
        public void LinkTacticToOtherPlan_with_entityid()
        {
            Console.WriteLine("To check to identify Link the Tactic and it's All Child element with planid,entityid,userid.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            RevenuePlanner.Helpers.Clonehelper Clonehelper = new Helpers.Clonehelper();
            MRPEntities db = new MRPEntities();
            Plan_Campaign_Program_Tactic objTactic = new Plan_Campaign_Program_Tactic();
            int UserId = ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).ID;
            //string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();
            //List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> lstPlanids = db.Plans.Where(pl => pl.CreatedBy == UserId).Select(pl => pl.PlanId).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string entityId = lstPlanids.FirstOrDefault().ToString() + "_" + tactic.FirstOrDefault().ToString();

            int _clone = Clonehelper.LinkTacticToOtherPlan(Convert.ToInt32(entityId.Split('_')[0]), UserId, Convert.ToInt32(entityId.Split('_')[1]), 0, string.Empty, false, null);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "\n The Assert Value clone:  " + _clone);
            Assert.AreEqual(0, _clone);
        }
    }
}
