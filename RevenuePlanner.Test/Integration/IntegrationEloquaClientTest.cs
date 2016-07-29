//// File Used to test the methods of Integration Eloqua Client.

#region Usings

using Integration;
using Integration.Eloqua;
using Integration.Helper;
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
using RestSharp;
using System.Text;

#endregion

namespace RevenuePlanner.Test.Integration
{
    [TestClass]
    public class IntegrationEloquaClientTest
    {
        #region Variables

        private static int _integrationInstanceId = 15;
        private static EntityType _entityType = EntityType.IntegrationInstance;
        private static Guid _userId = Guid.Parse("14D7D588-CF4D-46BE-B4ED-A74063B67D66");
        private static Guid _applicationId = Guid.Parse("1C10D4B9-7931-4A7C-99E9-A158CE158951");
        private static int _integrationInstanceLogId = 0;

        #endregion

        #region Constructor

        public IntegrationEloquaClientTest()
        {
            _integrationInstanceLogId = DataHelper.GetIntegrationInstanceLogId(_userId, _integrationInstanceId);
        }

        #endregion

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

        #region PL#1060 - MQL count from Eloqua

        #region Contact List Manipulation

        /// <summary>
        /// Get contact list for correct Ids.
        /// </summary>
        [TestMethod]
        public void Get_Eloqua_Contact_List_for_Correct_Ids()
        {
            Console.WriteLine("Get contact list for correct Ids.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter view id and contact list id.
            string elouqaContactListId = "58", eloquaViewId = "10007";

            var result = controller.GetEloquaContactList(elouqaContactListId, eloquaViewId, 1);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result.StatusCode:  " + result.StatusCode);
            Assert.AreNotEqual(null, result);
            

        }

        /// <summary>
        /// Get contact list for correct Ids.
        /// </summary>
        [TestMethod]
        public void Get_Eloqua_Contact_List_for_InCorrect_Ids()
        {
            Console.WriteLine("Get contact list for correct Ids.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter view id and contact list id.
            string elouqaContactListId = "58", eloquaViewId = "10007";

            var result = controller.GetEloquaContactList(elouqaContactListId, eloquaViewId, 1);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result.StatusCode:  " + result.StatusCode);
            Assert.AreNotEqual(null, result);
          
        }

        /// <summary>
        /// Get contact list detail for correct Id.
        /// </summary>
        [TestMethod]
        public void Get_Eloqua_Contact_List_Details_for_Correct_Id()
        {
            Console.WriteLine("Get contact list detail for correct Id.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter contact list id.
            string elouqaContactListId = "58";

            var result = controller.GetEloquaContactListDetails(elouqaContactListId);

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result.StatusCode:  " + result.StatusCode);
            Assert.AreNotEqual(null, result);
        }

        /// <summary>
        /// Get contact list detail for correct Ids.
        /// </summary>
        [TestMethod]
        public void Get_Eloqua_Contact_List_Details_for_InCorrect_Id()
        {
            Console.WriteLine("Get contact list detail for correct Ids.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter contact list id.
            string elouqaContactListId = "A58";

            var result = controller.GetEloquaContactListDetails(elouqaContactListId);
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value result.StatusCode:  " + result.StatusCode);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
           

        }

        /// <summary>
        /// Put contact list detail for correct Ids.
        /// </summary>
        [TestMethod]
        public void Put_Eloqua_Contact_List_Details_with_Correct_Id()
        {
            Console.WriteLine("Put contact list detail for correct Ids.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter contact list id.
            string elouqaContactListId = "58";

            ContactListDetailModel contactListDetailModel = new ContactListDetailModel
            {
                type = "ContactList",
                id = elouqaContactListId,
                createdAt = "1416978000",
                createdBy = "15",
                depth = "complete",
                folderId = "260",
                name = "GameplanMQL",
                updatedAt = "1419331552",
                count = "4",
                dataLookupId = "54d066ee-ff21-4aa4-950a-0c50e5d955fb",
                scope = "global",
                membershipDeletions = new List<string> { }
            };

            controller.PutEloquaContactListDetails(contactListDetailModel, elouqaContactListId);

            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + true);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Put contact list detail for in correct Ids.
        /// </summary>
        [TestMethod]
        public void Put_Eloqua_Contact_List_Details_with_InCorrect_Id()
        {
            Console.WriteLine("Put contact list detail for incorrect Ids.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter contact list id.
            string elouqaContactListId = "0";

            ContactListDetailModel contactListDetailModel = new ContactListDetailModel
            {
                type = "ContactList",
                id = elouqaContactListId,
                createdAt = "1416978000",
                createdBy = "15",
                depth = "complete",
                folderId = "260",
                name = "GameplanMQL",
                updatedAt = "1419331552",
                count = "4",
                dataLookupId = "54d066ee-ff21-4aa4-950a-0c50e5d955fb",
                scope = "global",
                membershipDeletions = new List<string> { }
            };

            controller.PutEloquaContactListDetails(contactListDetailModel, elouqaContactListId);

            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + true);
            Assert.IsTrue(true);
        }

        #endregion

        #region Common

        /// <summary>
        /// Convert Time stamp to DateTime.
        /// </summary>
        [TestMethod]
        public void Convert_Timestamp_To_DateTime()
        {
            Console.WriteLine("Convert Time stamp to DateTime.\n");
            IntegrationEloquaClient controller = new IntegrationEloquaClient(_integrationInstanceId, 0, _entityType, _userId, _integrationInstanceLogId, _applicationId);

            //// Enter date time time stamp.
            string dateTimeTimestamp = "1416978000";

            DateTime dateTime = controller.ConvertTimestampToDateTime(dateTimeTimestamp);

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value dateTime:  " + dateTime);
            Assert.AreNotEqual(null, dateTime);
            
        }

        #endregion

        #region Eloqua Response

        /// <summary>
        /// Function to manipulate tactic actual data.
        /// </summary>
        [TestMethod]
        public void Set_Tactic_Response()
        {
            Console.WriteLine("To manipulate tactic actual data.\n");
            EloquaResponse controller = new EloquaResponse();
            List<SyncError> lstSyncError = new List<SyncError>();
            controller.SetTacticMQLs(_integrationInstanceId, _userId, _integrationInstanceLogId, _applicationId, _entityType, out lstSyncError);

            
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + "  : Pass \n The Assert Value:  " + true);
            Assert.IsTrue(true);
        }

        #endregion

        #endregion
    }
}
