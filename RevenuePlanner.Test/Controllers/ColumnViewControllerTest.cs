using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using RevenuePlanner.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;

namespace RevenuePlanner.Test.Controllers
{
    [TestClass]
    public class ColumnViewControllerTest
    {
        [TestInitialize]
        public void LoadCacheMessage()
        {
            HttpContext.Current = RevenuePlanner.Test.MockHelpers.MockHelpers.FakeHttpContext();
        }
    
        #region method to get attribute list for column management
        /// <summary>
        /// To Get the list of attribute
        /// <author>Devanshi gandhi</author>
        /// <createddate>15-9-2016</createddate>
        /// </summary>
        [TestMethod]
        public void GetAttributeList()
        {

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ColumnViewController objUserController = new ColumnViewController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();


            var result = objUserController.GetAttributeList_ColumnView() as PartialViewResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.ViewName);
            Assert.IsNotNull(result.Model);
            Assert.AreEqual("_AddColumnView", result.ViewName);

        }
        #endregion

        #region method to save column view for user
        /// <summary>
        /// To Save the column view
        /// <author>Devanshi gandhi</author>
        /// <createddate>17-8-2016</createddate>
        /// </summary>
        [TestMethod]
        public void SaveColumnview()
        {

            MRPEntities db = new MRPEntities();
            HttpContext.Current = DataHelper.SetUserAndPermission();
            ColumnViewController objUserController = new ColumnViewController();
            objUserController.ControllerContext = new ControllerContext(MockHelpers.FakeUrlHelper.FakeHttpContext(), new RouteData(), objUserController);
            objUserController.Url = MockHelpers.FakeUrlHelper.UrlHelper();
            List<AttributeDetail> lstattribute = new List<AttributeDetail>();
            AttributeDetail objattribute = new AttributeDetail();
            objattribute.AttributeId = "Actual";

            objattribute.AttributeType = "Actual";
            lstattribute.Add(objattribute);
            var result = objUserController.SaveColumnView(lstattribute) as JsonResult;
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " \n The Assert Value result : " + result.Data);
            Assert.IsNotNull(result.Data);

        }
        #endregion

    }
}
