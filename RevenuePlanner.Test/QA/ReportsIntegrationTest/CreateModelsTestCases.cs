
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevenuePlanner.Controllers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.QA;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RevenuePlanner.Helpers;
using System.Collections.ObjectModel;
using Moq;
using System.Collections.Specialized;

namespace RevenuePlanner.Test.QA
{
    [TestClass]
    public class CreateModelsTestCases
    {
        public TestContext TestContext { get; set; }
        ModelController objModelController;
        MRPEntities db = new MRPEntities();
        FormCollection fc = new FormCollection();
        ICollection<string> stageIds = null;
        ICollection<string> TargetStage = null;
        ICollection<string> MCR = null;
        ICollection<string> MSV = null;
        
        [TestMethod()]
        [DataSource("CreateandSaveModel")]
        public void CreateAndSaveModel()
        {
            Hive9CommonFunctions ObjCommonFunctions = new Hive9CommonFunctions();
            objModelController = new ModelController();
            var result = ObjCommonFunctions.CheckLogin();
            if (result != null)
            {
                Assert.AreEqual("Index", result.RouteValues["Action"]);
                Console.WriteLine("LoginController - Index With Parameters \n The assert value of Action : " + result.RouteValues["Action"]);

                var routes = new RouteCollection();
                Console.WriteLine("To Create a New Model.\n");

                var retresult = objModelController.CreateModel() as ViewResult;
                Assert.AreEqual("Create", retresult.ViewName);
                Console.WriteLine("ModelController - CreateModel \n The assert value of view name:  " + retresult.ViewName);

                DataTable dt = this.TestContext.DataRow.Table;

                setDataFromExcel(dt);

                //Creation of model
                var Createresult = objModelController.Create(fc, stageIds, TargetStage, MCR, MSV) as RedirectToRouteResult;
                Assert.AreEqual("Tactics", result.RouteValues["Action"]);
                Console.WriteLine("ModelController - CreateModel \n The assert value of action:  " + result.RouteValues["Action"]);
                Assert.AreEqual("Model", result.RouteValues["Controller"]);
                Console.WriteLine("ModelController - CreateModel \n The assert value of controller:  " + result.RouteValues["Controller"]);

                //Set Tactic for model


            }
            else
            {
                Assert.Fail();
                Console.WriteLine("LoginController - Index With Parameters \n The assert value is null.");
            }
        }

        public void setDataFromExcel(DataTable dt)
        {
            if (dt != null)
            {
                string effectiveDateForModel = DateTime.UtcNow.Date.ToShortDateString();
                //Set Values for Form Collection 
                fc.Add("ValidateForEmptyField ", "Make sure all fields are filled with valid values and try again.");
                fc.Add("ErrorOccured", "An error has occured.");
                fc.Add("ModelId", "0");
                fc.Add("whichButton", "save");
                fc.Add("EffectiveDate", effectiveDateForModel);
                fc.Add("IsBenchmarked", "true");
                fc.Add("CurrentModelId", "");
                if (dt.Rows.Count > 0)
                {
                    string modelTitle = dt.Rows[0]["Title"].ToString();
                    string modelprice = dt.Rows[0]["Avg_Deal_Size"].ToString();
                    fc.Add("Title", modelTitle);
                    fc.Add("txtMarketing", modelprice);
                    stageIds = new Collection<string>();
                    TargetStage = new Collection<string>();
                    MCR = new Collection<string>();
                    MSV = new Collection<string>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        //To get list of stage Id
                        List<Stage> stages = QA_DataHelper.GetAllClientStages(Sessions.User.CID);
                        string stageId = stages.Where(s => s.Code == dr["Stage_Name"].ToString()).Select(s => s.StageId).FirstOrDefault().ToString();
                        stageIds.Add(stageId);
                        MSV.Add(dr["Valocity"].ToString());
                        MCR.Add(dr["Conversion_Rate"].ToString());
                        TargetStage.Add(dr["Allowed_Target_Stage"].ToString());
                    }
                }
            }
        }
    }
}
