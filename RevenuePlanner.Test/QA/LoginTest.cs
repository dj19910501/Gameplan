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
using RevenuePlanner.Test.QA;
using System.Data;
namespace RevenuePlanner.Test.QA
{
    [TestClass]
    public class LoginTest
    {
        [TestMethod()]
        public void Login()
        {
            try
            {
                Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
                
                //Call common function for login
                var result = ObjCommonFunctions.CheckLogin();

                if (result != null)
                {
                    Assert.AreEqual("Index", result.RouteValues["Action"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + result.RouteValues["Action"]);

                    Assert.AreEqual("Home", result.RouteValues["Controller"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Controller : " + result.RouteValues["Controller"]);

                    Assert.AreEqual("Home", result.RouteValues["ActiveMenu"]);
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value of Active menu : " + result.RouteValues["ActiveMenu"]);
                }
                else
                {
                    Assert.Fail();
                    Console.WriteLine("LoginController - Index With Parameters \n The assert value is null.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
