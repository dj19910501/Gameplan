using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class FilterControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
        #region Filter with no parameters
        /// <summary>
        /// To check to retrieve Filter view with no parameters
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>26Sep2016</createddate>
        [TestMethod]
        public void Get_Filter_With_No_Parameters()
        {
            Console.WriteLine("To check to retrieve Home view with no parameters.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            string userName = Convert.ToString(ConfigurationManager.AppSettings["Username"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            string singlehash = DataHelper.ComputeSingleHash(password);
            RevenuePlanner.BDSService.BDSServiceClient objBDSServiceClient = new RevenuePlanner.BDSService.BDSServiceClient();
            Sessions.User = objBDSServiceClient.Validate_UserOverAll(userName, singlehash);

            //// Call index method
            FilterController objFilterController = new FilterController();
            var result = objFilterController.Index() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ActiveMenu :  " + result.ViewBag.ActiveMenu);
            Assert.IsNotNull(result.ViewBag.ActiveMenu);
        }
        #endregion

        #region Filter page with plan id
        /// <summary>
        /// To check to retrieve Filter view with plan id
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>26Sep2016</createddate>
        [TestMethod]
        public void Get_Filter_With_PlanId()
        {
            Console.WriteLine("To check to retrieve Home view with plan id.\n");
            //// Set session value
            HttpContext.Current = DataHelper.SetUserAndPermission();

            //// Call index method
            FilterController objFilterController = new FilterController();
            List<int> planId = new List<int>();
            planId.Add(DataHelper.GetPlanId());
            string userName = Convert.ToString(ConfigurationManager.AppSettings["Username"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            string singlehash = DataHelper.ComputeSingleHash(password);
            RevenuePlanner.BDSService.BDSServiceClient objBDSServiceClient = new RevenuePlanner.BDSService.BDSServiceClient();
            Sessions.User = objBDSServiceClient.Validate_UserOverAll(userName, singlehash);
            var result = objFilterController.Index(Enums.ActiveMenu.Home, planId) as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ActiveMenu:  " + result.ViewBag.ActiveMenu);
            Assert.IsNotNull(result.ViewBag.ActiveMenu);
        }
        #endregion

        #region Get TacticType List For Filter
        /// <summary>
        /// To check to Get TacticType List For Filter
        /// <author>Nandish Shah</author>
        /// <createddate>20Sep2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_TacticType_List_For_Filter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get TacticType List For Filter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();
            objFilterController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objFilterController);
            objFilterController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            objFilterController.Url = new UrlHelper(new RequestContext(objFilterController.HttpContext, new RouteData()), routes);
            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            var result = objFilterController.GetTacticTypeListForFilter(PlanId.ToString()) as JsonResult;
            var serializedData = new RouteValueDictionary(result.Data);
            var resultvalue = serializedData["isSuccess"];
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value resultvalue:  " + resultvalue.ToString());
            Assert.AreEqual("true", resultvalue.ToString(), true);
        }
        #endregion

        /// <summary>
        /// To Save last set data
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>20Sep2016</createddate>
        [TestMethod]
        public void SaveLastSetOfViews()
        {
            Console.WriteLine("To Save last set data.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();
            string CommaSeparatedPlanId = DataHelper.GetPlanId().ToString();

            string ViewBy = PlanGanttTypes.Tactic.ToString();
            List<int> lstPlanids = CommaSeparatedPlanId.Split(',').ToList().Select(id => Convert.ToInt32(id)).ToList();
            List<int> tactic = db.Plan_Campaign_Program_Tactic.Where(id => lstPlanids.Contains(id.Plan_Campaign_Program.Plan_Campaign.PlanId)).Select(tactictype => tactictype.TacticTypeId).ToList();
            string tactictypeids = string.Join(",", tactic);

            List<int> Owner = db.Plans.Where(id => lstPlanids.Contains(id.PlanId)).Select(plan => plan.CreatedBy).ToList();
            string Ownerids = string.Join(",", Owner);

            var UserID = Sessions.User.ID;

            string CommaSeparatedCustomFields = DataHelper.GetSearchFilterForCustomRestriction(UserID);

            List<string> lststatus = new List<string>();
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Created.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Submitted.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.InProgress.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Approved.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Complete.ToString()].ToString());
            lststatus.Add(Enums.TacticStatusValues[Enums.TacticStatus.Decline.ToString()].ToString());

            string Status = string.Join(",", lststatus);

            var result = objFilterController.SaveLastSetofViews(CommaSeparatedPlanId, CommaSeparatedCustomFields, Ownerids, tactictypeids, Status, "", "", "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// To Save last set data with empty planid
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>20Sep2016</createddate>
        [TestMethod]
        public void SaveLastSetOfViews_With_EmptyValues()
        {
            Console.WriteLine("To Save last set data with empty planid.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();
            Sessions.PlanUserSavedViews = db.Plan_UserSavedViews.Where(t => t.Userid == Sessions.User.ID).ToList();
            var result = objFilterController.SaveLastSetofViews("", "", "", "", "", "", "", "") as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value isSuccess:  " + result.GetValue("isSuccess"));
            Assert.AreEqual(true, result.GetValue("isSuccess"));
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ViewName:  " + result.GetValue("ViewName"));
            Assert.AreEqual("", result.GetValue("ViewName"));

        }

        /// <summary>
        /// To Save last set data with null CustomFields and tactictypeid
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>20Sep2016</createddate>
        [TestMethod]
        public void SaveLastSetOfViews_With_Null_CustomFields_Tactictypeid()
        {
            Console.WriteLine("To Save last set data with null CustomFields and tactictypeid.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();

            var result = objFilterController.SaveLastSetofViews(null, null, null, null, null, null, null, null) as JsonResult;

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value ViewName:  " + result.GetValue("ViewName"));
            Assert.AreEqual(null, result.GetValue("ViewName"));
        }

        /// <summary>
        /// To Render last set of view
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>20Sep2016</createddate>
        [TestMethod]
        public void Render_LastSetofViews()
        {
            Console.WriteLine("To Render last set of view.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();

            var result = objFilterController.LastSetOfViews() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// Get Plan Based On Year
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>29Sep2016</createddate>
        [TestMethod]
        public void GetPlanBasedOnYear_EmptyParameters()
        {
            Console.WriteLine("To Get Plan Based On Year.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();

            var result = objFilterController.GetPlanBasedOnYear() as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        /// <summary>
        /// Get Plan Based On Year
        /// </summary>
        /// <auther>Nandish Shah</auther>
        /// <createddate>29Sep2016</createddate>
        [TestMethod]
        public void GetPlanBasedOnYear_WithParameters()
        {
            Console.WriteLine("To Get Plan Based On Year.\n");
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();
            string Year = Convert.ToString(DateTime.Now.Year);

            var result = objFilterController.GetPlanBasedOnYear(Year) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result.Data:  " + result.Data);
            Assert.IsNotNull(result.Data);
        }

        #region Get Owner List For Filter
        /// <summary>
        /// To Get Owner List For Filter
        /// <author>Nandish Shah</author>
        /// <createddate>20Sep2016</createddate>
        /// </summary>
        [TestMethod]
        public void Get_Owner_List_For_Filter()
        {
            var routes = new RouteCollection();
            Console.WriteLine("Get Owner List For Filter.\n");
            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            FilterController objFilterController = new FilterController();
            objFilterController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objFilterController);

            int PlanId = DataHelper.GetPlanId();
            Sessions.PlanId = PlanId;
            string UserId = Sessions.User.UserId.ToString();
            string ActiveMenu = Enums.ActiveMenu.Home.ToString();
            string viewBy = Enums.EntityType.Tactic.ToString();
            var result = objFilterController.GetOwnerListForFilter(PlanId.ToString(), viewBy, ActiveMenu) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value:  " + result);
            Assert.IsNotNull(result);

        }
        #endregion
    }
}
