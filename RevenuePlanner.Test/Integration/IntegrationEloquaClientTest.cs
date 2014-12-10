//// File Used to test the methods of Integration Eloqua Client.

#region Usings

using Integration;
using Integration.Eloqua;
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

#endregion

namespace RevenuePlanner.Test.Integration
{
    [TestClass]
    public class IntegrationEloquaClientTest
    {
        //#region PL#999 - Eloqua folders: Integration of tactics to Folders

        //#region Get Eloqua Folder Id

        ///// <summary>
        ///// Get Eloqua Folder Id when Folder Path of plan doesn't specified.
        ///// </summary>
        //[TestMethod]
        //public void Get_Eloqua_Folder_Id_Folder_Path_Not_Specified()
        //{
        //    IntegrationEloquaClient controller = new IntegrationEloquaClient();

        //    //// Enter tactic Id such that it's plan doesn't have any path specified.
        //    int tacticId = 9931;

        //    int result = controller.GetEloquaFolderId(DataHelper.Get_Plan_Campaign_Program_Tactic(tacticId));

        //    Assert.Equals(0, result);
        //}

        ///// <summary>
        ///// Get Eloqua Folder Id when specified Folder Path of plan doesn't exist.
        ///// </summary>
        //[TestMethod]
        //public void Get_Eloqua_Folder_Id_Folder_Path_Not_Exist()
        //{
        //    IntegrationEloquaClient controller = new IntegrationEloquaClient();

        //    //// Enter tactic Id such that it's plan folder path doesn't exist on eloqua.
        //    int tacticId = 805;

        //    int result = controller.GetEloquaFolderId(DataHelper.Get_Plan_Campaign_Program_Tactic(tacticId));

        //    Assert.Equals(0, result);
        //}
        
        ///// <summary>
        ///// Get Eloqua Folder Id when specified Folder Path of plan having one folder exist in eloqua.
        ///// </summary>
        //[TestMethod]
        //public void Get_Eloqua_Folder_Id_Folder_Path_One_Folder_Exist()
        //{
        //    IntegrationEloquaClient controller = new IntegrationEloquaClient();

        //    //// Enter tactic Id such that it's plan folder path doesn't exist on eloqua.
        //    int tacticId = 9930;

        //    int result = controller.GetEloquaFolderId(DataHelper.Get_Plan_Campaign_Program_Tactic(tacticId));

        //    Assert.AreNotEqual(0, result);
        //}

        ///// <summary>
        ///// Get Eloqua Folder Id when specified Folder Path of plan having more than one folder exist in eloqua.
        ///// </summary>
        //[TestMethod]
        //public void Get_Eloqua_Folder_Id_Folder_Path_More_Than_One_Folder_Exist()
        //{
        //    IntegrationEloquaClient controller = new IntegrationEloquaClient();

        //    //// Enter tactic Id such that it's plan folder path doesn't exist on eloqua.
        //    int tacticId = 805;

        //    int result = controller.GetEloquaFolderId(DataHelper.Get_Plan_Campaign_Program_Tactic(tacticId));

        //    Assert.AreNotEqual(0, result);
        //}
        
        //#endregion

        //#region Search Folder In Eloqua

        ///// <summary>
        ///// Get Eloqua Folder list in json when folder name specified.
        ///// </summary>
        //[TestMethod]
        //public void Search_Folder_In_Eloqua_Folder_Name_Specified()
        //{
        //    IntegrationEloquaClient controller = new IntegrationEloquaClient();

        //    //// Enter tactic Id such that it's plan folder path doesn't exist on eloqua.
        //    string FolderPath = "";

        //   // var result = controller.GetEloquaFolderId(FolderPath) as I;

        //   // Assert.AreNotEqual(0, result);
        //}

        ///// <summary>
        ///// Get Eloqua Folder list in json when folder name not specified.
        ///// </summary>
        //[TestMethod]
        //public void Search_Folder_In_Eloqua_Folder_Name_Not_Specified()
        //{

        //}

        ///// <summary>
        ///// Get all Eloqua Folder list in json response.
        ///// </summary>
        //[TestMethod]
        //public void Search_Folder_In_Eloqua_Get_All_Folders()
        //{

        //}

        //#endregion
        
        //#endregion
    }
}
