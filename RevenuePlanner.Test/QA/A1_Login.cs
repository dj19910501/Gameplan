using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Configuration;
using RevenuePlanner.Test.MockHelpers;
using RevenuePlanner.Test.QA_Helpers;
using System.Data;
namespace RevenuePlanner.Test.QA
{
    [TestClass]
    public class A1_Login
    {
        [TestMethod()]
        public void A1_ReportLogin()
        {
            try
            {
                Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
                
                //Call common function for login
                var result = ObjCommonFunctions.CheckLogin();
                Console.WriteLine(" Testing LoginController - Index With Parameters method");
                if (result != null)
                {
                    Assert.AreEqual("Index", result.RouteValues["Action"]);
                    Console.WriteLine("\n The assert value of action is " + result.RouteValues["Action"] + ". (The expected value is Index.)");

                    Assert.AreEqual("Home", result.RouteValues["Controller"]);
                    Console.WriteLine("\n The assert value of controller is " + result.RouteValues["Controller"] + ". (The expected value is Home)");

                    Assert.AreEqual("Home", result.RouteValues["ActiveMenu"]);
                    Console.WriteLine("\n The assert value of active menu is " + result.RouteValues["ActiveMenu"]+ ". (The expected value is Home)");
                }
                else
                {
                    Assert.Fail();
                    Console.WriteLine("\n The assert value is null.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
