﻿using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RevenuePlanner.Services;

/*
 *  Author: Kuber Joshi
 *  Created Date: 11/08/2013
 *  Screen: 006.000.Prefs_Admin.Profile.Account
 *  Purpose: Manage User
 */

namespace RevenuePlanner.Controllers
{
    public class UserController : CommonController
    {
        #region Variables

        private MRPEntities db = new MRPEntities();
        private BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        IAlerts objcommonalert = new Alerts();

        #endregion

        #region Team Member Listing

        /// <summary>
        /// Team Member listing
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            // Added By Sohel Pathan on 26/06/2014 for PL ticket #517
            ViewBag.NotifyBeforManagerDeletion = Common.objCached.NotifyBeforeManagerDeletion;

            //// Initialize List Variables.
            List<UserModel> teamMemberList = new List<UserModel>();
            List<BDSService.User> lstUser = null;
            List<BDSService.User> lstOtherUser = new List<BDSService.User>();   /* Added by Sohel Pathan on 10/07/2014 for PL ticket #586 */
            try
            {
                //// Get TeamMembers list by Client,Application & User Id.
                lstUser = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true).OrderBy(teamlist => teamlist.FirstName, new AlphaNumericComparer()).ToList();
                if (lstUser.Count() > 0)
                {
                    foreach (var user in lstUser)
                    {
                        UserModel objUserModel = new UserModel();
                        objUserModel.ClientId = user.ClientId;
                        objUserModel.Client = user.Client;
                        objUserModel.DisplayName = user.DisplayName;
                        objUserModel.Email = user.Email;
                        objUserModel.FirstName = user.FirstName;
                        objUserModel.JobTitle = user.JobTitle;
                        objUserModel.LastName = user.LastName;
                        objUserModel.Password = user.Password;
                        objUserModel.UserId = user.UserId;
                        objUserModel.RoleCode = user.RoleCode;
                        objUserModel.RoleId = user.RoleId;
                        objUserModel.RoleTitle = user.RoleTitle;
                        objUserModel.IsManager = user.IsManager;    // Added by Sohel Pathan on 26/06/2014 for PL ticket #517
                        teamMemberList.Add(objUserModel);
                    }
                }
                /* Added by Sohel Pathan on 10/07/2014 for PL ticket #586 */
                //// If user is Admin then Get Other Application Users list for Suggestion.
                if ((bool)ViewBag.IsUserAdminAuthorized)
                    lstOtherUser = objBDSServiceClient.GetOtherApplicationUsers(Sessions.User.ClientId, Sessions.ApplicationId).OrderBy(userlist => userlist.FirstName, new AlphaNumericComparer()).ToList();
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            /* Start - Added by Sohel Pathan on 10/07/2014 for PL ticket #586 */
            //// Suggest Other Applications Users list.
            if (lstOtherUser != null && lstOtherUser.Count > 0)
            {
                lstOtherUser.ForEach(a => a.DisplayName = a.FirstName + a.LastName);
                ViewBag.OtherUsers = lstOtherUser.OrderBy(a => a.DisplayName, new AlphaNumericComparer()).ToList();

                try
                {
                    //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                    ViewData["Roles"] = objBDSServiceClient.GetAllRoleList(Sessions.ApplicationId, Sessions.User.ClientId).OrderBy(role => role.Title, new AlphaNumericComparer());
                }
                catch (Exception e)
                {
                    ErrorSignal.FromCurrentContext().Raise(e);

                    //To handle unavailability of BDSService
                    if (e is System.ServiceModel.EndpointNotFoundException)
                    {
                        return RedirectToAction("ServiceUnavailable", "Login");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    }
                }
            }
            else
            {
                ViewBag.OtherUsers = lstOtherUser;
            }
            /* End - Added by Sohel Pathan on 10/07/2014 for PL ticket #586 */
            return View(teamMemberList.AsEnumerable());
        }

        #endregion

        #region Change Password

        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns>ChangePassword view</returns>
        public ActionResult ChangePassword()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            return View();
        }

        /// <summary>
        /// POST: Change Password
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePassword(UserChangePassword form)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            try
            {
                if (ModelState.IsValid && Sessions.User.UserId != null)
                {
                    //// Compare NewPassword and ConfirmPassword fields.
                    if (form.NewPassword != form.ConfirmNewPassword)
                    {
                        TempData["ErrorMessage"] = Common.objCached.UserPasswordDoNotMatch;
                        return View(form);
                    }

                    /* ------------------ Single hash password ----------------------*/
                    string SingleHash_CurrentPassword = Common.ComputeSingleHash(form.CurrentPassword.ToString().Trim());
                    string SingleHash_NewPassword = Common.ComputeSingleHash(form.NewPassword.ToString().Trim());
                    /* ---------------------------------------------------------------*/
                    //Added By Maitri Gandhi for #2105 on 11/4/2016
                    string returnMessage = "Success";

                    //returnMessage = objBDSServiceClient.CreatePasswordHistory(Sessions.User.UserId, SingleHash_NewPassword, Sessions.User.UserId);
                    //Modified by Maitri Gandhi on 19/4/2016
                    returnMessage = objBDSServiceClient._ChangePassword(Sessions.User.UserId, SingleHash_NewPassword, SingleHash_CurrentPassword);
                    if (returnMessage == "CurrentUserPasswordNotCorrect")
                    {
                        TempData["ErrorMessage"] = Common.objCached.CurrentUserPasswordNotCorrect;
                    }
                    else if (returnMessage != "Success")
                    {
                        TempData["ErrorMessage"] = returnMessage;
                        return View(form);
                    }
                    else if (returnMessage == "Success")
                    {
                        ChangePasswordMail();
                        //Redirect users logging in for the first time to the change password module
                        if (Sessions.User.LastLoginDate == null && Sessions.RedirectToChangePassword)
                        {
                            Sessions.RedirectToChangePassword = false;
                            //Update last login date for user
                            objBDSServiceClient.UpdateLastLoginDate(Sessions.User.UserId, Sessions.ApplicationId);

                            //Commented By Komal Rawal for #1457
                            //if (Sessions.User.SecurityQuestionId == null)
                            //{
                            //    Sessions.RedirectToSetSecurityQuestion = true;
                            //    return RedirectToAction("SetSecurityQuestion", "Login");
                            //}

                            return RedirectToAction("Index", "Home");
                        }
                        TempData["SuccessMessage"] = Common.objCached.UserPasswordChanged;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View(form);
        }

        /// <summary>
        /// Function to verify users current password.
        /// </summary>
        /// <param name="currentPassword">current password</param>
        /// <returns>Returns true if the operation is successful, 0 otherwise.</returns>
        public ActionResult CheckCurrentPassword(string currentPassword)
        {
            bool isValid = false;

            /* ------------------------------- single hash current password ------------------------------*/
            string SingleHash_CurrentPassword = Common.ComputeSingleHash(currentPassword.ToString().Trim());
            /*--------------------------------------------------------------------------------------------*/
            try
            {
                //// Verify Current Password by UserId.
                isValid = objBDSServiceClient.CheckCurrentPassword(Sessions.User.UserId, SingleHash_CurrentPassword);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
            }
            if (isValid)
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("1", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Function to send email notification to user about change password activity.
        /// </summary>
        /// <returns></returns>
        public void ChangePasswordMail()
        {
            try
            {
                if (Sessions.User != null)
                {
                    string notificationChangePassword = Enums.Custom_Notification.ChangePassword.ToString();
                    Notification notification = (Notification)db.Notifications.FirstOrDefault(notifctn => notifctn.NotificationInternalUseOnly.Equals(notificationChangePassword));
                    if (notification != null)
                    {
                        //// Set LoginUrl to pass into Email body part.
                        string strURL = Url.Action("Index", "Login", new { }, Request.Url.Scheme);
                        string replyToEmail = System.Configuration.ConfigurationManager.AppSettings.Get("ReplyToMail");
                        string emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", Sessions.User.FirstName).Replace("[URL]", strURL);
                        Common.sendMail(Sessions.User.Email, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High), string.Empty, replyToEmail);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        #endregion

        //Commented By Komal Rawal for #1457
        //#region Security Question

        ///// <summary>
        ///// Security Question
        ///// </summary>
        ///// <returns> return SecurityQuestion View</returns>
        //public ActionResult SecurityQuestion()
        //{
        //    // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
        //    ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
        //    SecurityQuestionListModel objSecurityQuestionListModel = new SecurityQuestionListModel();
        //    try
        //    {
        //        BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        //        var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();
        //        if (!string.IsNullOrEmpty(Sessions.User.Answer)) //PL #1457 Security question error during reset password :- added by dasharth prajapati 
        //        {
        //            objSecurityQuestionListModel.Answer = Common.Decrypt(Sessions.User.Answer);
        //        }

        //        objSecurityQuestionListModel.SecurityQuestionId = Convert.ToInt32(Sessions.User.SecurityQuestionId);
        //        objSecurityQuestionListModel.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);

        //        //To handle unavailability of BDSService
        //        if (e is System.ServiceModel.EndpointNotFoundException)
        //        {
        //            return RedirectToAction("ServiceUnavailable", "Login");
        //        }
        //    }

        //    return View(objSecurityQuestionListModel);
        //}

        ///// <summary>
        ///// Post : security question view
        ///// <param name="form">Security Question Data</param>
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult SecurityQuestion(SecurityQuestionListModel form)
        //{
        //    // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
        //    ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

        //    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
        //    try
        //    {
        //        BDSService.User objUser = new BDSService.User();
        //        objUser.UserId = Sessions.User.UserId;
        //        objUser.SecurityQuestionId = form.SecurityQuestionId;
        //        objUser.Answer = Common.Encrypt(form.Answer);

        //        //// Update User Security Question.
        //        int retVal = objBDSServiceClient.UpdateUserSecurityQuestion(objUser);

        //        if (retVal == -1)
        //        {
        //            TempData["ErrorMessage"] = Common.objCached.SecurityQuestionChangesNotApplied;
        //        }
        //        else if (retVal == 1)
        //        {

        //            Sessions.User.SecurityQuestionId = form.SecurityQuestionId;
        //            Sessions.User.Answer = Common.Encrypt(form.Answer);
        //            TempData["SuccessMessage"] = Common.objCached.SecurityQuestionChangesApplied;
        //        }
        //        //// return Security Question list to View.
        //    var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();
        //    form.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorSignal.FromCurrentContext().Raise(e);

        //        //To handle unavailability of BDSService
        //        if (e is System.ServiceModel.EndpointNotFoundException)
        //        {
        //            return RedirectToAction("ServiceUnavailable", "Login");
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
        //        }
        //    }

        //    return View(form);
        //}

        ///// <summary>
        ///// Method to get the Select list item
        ///// </summary>
        ///// <param name="QuestionList"> list of Questions</param>
        ///// <returns> Returns list of Questions</returns>
        //public List<SelectListItem> GetQuestionList(List<BDSService.SecurityQuestion> QuestionList)
        //{
        //    List<SelectListItem> optionslist = new List<SelectListItem>();
        //    optionslist = QuestionList.AsEnumerable().Select(questn => new SelectListItem
        //    {
        //        Value = Convert.ToString(questn.SecurityQuestionId),
        //        Text = questn.SecurityQuestion1
        //    }).ToList();
        //    return optionslist;
        //}

        //#endregion

        #region Delete User

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id">user to be deleted</param>
        /// <returns> Redirect to Index Action</returns>
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Delete(Guid id)
        {
            try
            {
                if (id != null)
                {
                    //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
                    string userRole = objBDSServiceClient.GetUserRole(id, Sessions.ApplicationId, Sessions.User.ClientId);
                    int retVal = objBDSServiceClient.DeleteUser(id, Sessions.ApplicationId);
                    if (retVal == 1)
                        TempData["SuccessMessage"] = Common.objCached.UserDeleted;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Add New User

        /// <summary>
        /// Load Create Mode Components
        /// </summary>
        /// <param name="clientId">client</param>
        /// <returns></returns>
        private void LoadCreateModeComponents(string clientId = "")
        {
            var IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);
            if (!String.IsNullOrWhiteSpace(clientId))
            {
                Guid userClientId = Guid.Parse(clientId);
                // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                if (IsUserAdminAuthorized)
                {
                    // Get All User List for Manager
                    ViewData["ManagerList"] = GetManagersList(Guid.Empty, userClientId);
                }
                // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
            }

            //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
            ViewData["Roles"] = objBDSServiceClient.GetAllRoleList(Sessions.ApplicationId, Sessions.User.ClientId).OrderBy(role => role.Title, new AlphaNumericComparer());
            ViewBag.CurrClientId = Sessions.User.ClientId;
            ViewBag.CurrClient = Sessions.User.Client;

            if (IsUserAdminAuthorized)
                ViewData["Clients"] = objBDSServiceClient.GetClientList();
        }

        /// <summary>
        /// Add New User
        /// </summary>
        /// <returns> Return Create View</returns>
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Create()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            try
            {
                LoadCreateModeComponents(Convert.ToString(Sessions.User.ClientId));
                // End: Modofied by Dharmraj, For ticket #583
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View();
        }

        /// <summary>
        /// POST: Add New User
        /// </summary>
        /// <param name="form">User Data</param>
        /// <param name="file">user photo</param>
        /// <returns>Return Create View</returns>
        [HttpPost]
        [AuthorizeUser(Enums.ApplicationActivity.UserAdmin)]  // Added by Sohel Pathan on 24/06/2014 for PL ticket #537 to implement user permission Logic
        public ActionResult Create(UserModel form, HttpPostedFileBase file)
        {
            // Added by Sohel Pathan on 25/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);

            try
            {
                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        //To check JPG or PNG file formats only & size upto 1MB
                        if ((!file.ContentType.ToLower().Contains("jpg") && !file.ContentType.ToLower().Contains("jpeg") && !file.ContentType.ToLower().Contains("png")) || (file.ContentLength > 1 * 1024 * 1024))
                        {
                            TempData["ErrorMessage"] = Common.objCached.InvalidProfileImage;
                            LoadCreateModeComponents(Convert.ToString(form.ClientId));
                            return View(form);
                        }
                    }

                    BDSService.User objUser = new BDSService.User();
                    objUser.FirstName = form.FirstName;
                    objUser.LastName = form.LastName;
                    var password = form.Password; // Here you can set default password for user
                    objUser.Password = Common.ComputeSingleHash(password); //Single hash password
                    objUser.Email = form.Email;
                    objUser.Phone = form.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                    objUser.JobTitle = form.JobTitle;
                    objUser.ClientId = form.ClientId;
                    objUser.RoleId = form.RoleId;
                    if (file != null)
                    {
                        using (MemoryStream target = new MemoryStream())
                        {
                            file.InputStream.CopyTo(target);
                            byte[] data = target.ToArray();
                            objUser.ProfilePhoto = data;
                        }
                    }

                    objUser.ManagerId = form.ManagerId;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517

                    //// Create User with default Permissions.
                    //int retVal = objBDSServiceClient.CreateUserWithPermission(objUser, Sessions.ApplicationId, Sessions.User.UserId);
                    //Modified by Maitri Gandhi on 19/4/2016
                    string retValMsg = objBDSServiceClient._CreateUserWithPermission(objUser, Sessions.ApplicationId, Sessions.User.UserId);
                    if (retValMsg == "Success")
                    {
                        UserCreatedMail(objUser, password);
                        //var UserDetails = objBDSServiceClient.GetUserDetails(objUser.Email);
                        //objBDSServiceClient.CreatePasswordHistory(UserDetails.UserId, objUser.Password,Sessions.User.UserId);
                        TempData["SuccessMessage"] = Common.objCached.UserAdded;
                        return RedirectToAction("Index");
                    }
                    else if (retValMsg == "UserDuplicate")
                    {
                        TempData["ErrorMessage"] = Common.objCached.UserDuplicate;
                    }
                    else if (retValMsg == "Error")
                    {
                        TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    }
                }
                LoadCreateModeComponents(Convert.ToString(form.ClientId));
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View(form);
        }

        /// <summary>
        /// Function to send email notification to user their account creation activity.
        /// </summary>
        /// <param name="user">user entity</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        public void UserCreatedMail(BDSService.User user, string password)
        {
            try
            {
                string notificationUserCreated = Enums.Custom_Notification.UserCreated.ToString();
                Notification notification = (Notification)db.Notifications.FirstOrDefault(notfctn => notfctn.NotificationInternalUseOnly.Equals(notificationUserCreated));
                if (notification != null)
                {
                    string applicationLink = Url.Action("Index", "Login", new { }, Request.Url.Scheme);//added by uday for #587
                    string emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", user.FirstName + " " + user.LastName).Replace("[LoginToBeReplaced]", user.Email).Replace("[PasswordToBeReplaced]", password).Replace("[ApplicationLink]", applicationLink);//application link added by uday for #587););
                    Common.sendMail(user.Email, Common.FromMail, emailBody, notification.Subject, Convert.ToString(System.Net.Mail.MailPriority.High));
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        /// <summary>
        /// Function to check whether the email exists or not.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>Returns true if the operation is successful, 0 otherwise.</returns>
        public ActionResult IsEmailExist(string email)
        {
            bool isValid = false;
            try
            {
                //// Check whether email exist or not.
                isValid = objBDSServiceClient.CheckEmail(email);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            if (isValid)
                return Json("0", JsonRequestBehavior.AllowGet);
            else
                return Json("1", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Edit Existing User

        /// <summary>
        /// Load Edit Mode Components
        /// </summary>
        /// <param name="clientId">client</param>
        /// <param name="src">source either "myaccount" or "myteam"</param>
        /// <returns></returns>
        private void LoadEditModeComponents(Guid clientId, string src)
        {
            if (!string.IsNullOrWhiteSpace(src))
            {
                ViewBag.SourceValue = src;
            }
            ViewData["Clients"] = objBDSServiceClient.GetClientList();

            //Added By : Kalpesh Sharam bifurcated Role by Client ID - 07-22-2014 
            ViewData["Roles"] = objBDSServiceClient.GetAllRoleList(Sessions.ApplicationId, Sessions.User.ClientId);

            ViewBag.CurrentUserId = Convert.ToString(Sessions.User.UserId);
            ViewBag.CurrentUserRole = Convert.ToString(Sessions.User.RoleCode);
        }

        /// <summary>
        /// Load Edit View of User Account
        /// </summary>
        /// <returns>Returns Edit View</returns>
        // Added by Viral Kadiya on 11/06/2014 for PL ticket #917 to implement Security Testing: Bulldog User Can Change Zebra User Details
        public ActionResult Edit()
        {
            // Added by Viral Kadiya on 11/13/2014 for PL ticket #945 
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);
            return View();
        }

        /// <summary>
        /// Edit Existing User
        /// </summary>
        /// <param name="usrid">user</param>
        /// <param name="src">source either "myaccount" or "myteam"</param>
        /// <param name="isForDelete">Added to give delete option, if user has selected the delete operation from user listing.</param> // Added by Sohel Pathan on 26/06/2014 for PL ticket #517
        /// <returns>Returns _UpdateUserDetails partialview with UserModel</returns>
        /// This method is common for MyAccount and Team Member edit page.
        public ActionResult EditUserDetails(string usrid = null, string src = "myaccount", string isForDelete = "false")
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            // Added by Sohel Pathan on 26/06/2014 for PL ticket #517
            ViewBag.isForDelete = isForDelete;
            Guid userId = new Guid();
            if (usrid == null)
            {
                userId = Sessions.User.UserId;
            }
            else
            {
                userId = Guid.Parse(usrid);
            }

            BDSService.User objUser = new BDSService.User();
            UserModel objUserModel = new UserModel();
            try
            {
                objUser = objBDSServiceClient.GetTeamMemberDetails(userId, Sessions.ApplicationId);
                if (objUser != null)
                {
                    //// Set User details to Model.
                    objUserModel.ClientId = objUser.ClientId;
                    objUserModel.Client = objUser.Client;
                    objUserModel.DisplayName = objUser.DisplayName;
                    objUserModel.Email = objUser.Email;
                    objUserModel.Phone = objUser.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                    objUserModel.FirstName = objUser.FirstName;
                    objUserModel.JobTitle = objUser.JobTitle;
                    objUserModel.LastName = objUser.LastName;
                    objUserModel.Password = objUser.Password;
                    objUserModel.ProfilePhoto = objUser.ProfilePhoto;
                    objUserModel.UserId = objUser.UserId;
                    objUserModel.RoleCode = objUser.RoleCode;
                    objUserModel.RoleId = objUser.RoleId;
                    objUserModel.RoleTitle = objUser.RoleTitle;
                    objUserModel.Password = objUser.Password;
                    objUserModel.ConfirmPassword = objUser.Password;
                    // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                    objUserModel.IsManager = objUser.IsManager;
                    if (objUser.ManagerId != null && objUser.ManagerId != Guid.Empty)
                        objUserModel.ManagerId = objUser.ManagerId;
                    // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                    LoadEditModeComponents(objUserModel.ClientId, src);
                    // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                    if ((bool)ViewBag.IsUserAdminAuthorized && Sessions.User.UserId != objUser.UserId && objUserModel.ClientId != Guid.Empty)
                    {
                        // Get All User List for Manager
                        ViewData["ManagerList"] = GetManagersList(objUser.UserId, objUserModel.ClientId);
                    }
                    else
                    {
                        objUserModel.ManagerName = (objUser.ManagerName == null ? "N/A" : objUser.ManagerName);
                    }
                    // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517

                    //// Get Default Image
                    byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    if (imageBytes != null)
                    {
                        //// Convert Imagebytes to Image pass to ViewBag.
                        using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                        {
                            ms.Write(imageBytes, 0, imageBytes.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                            image = Common.ImageResize(image, 60, 60, true, false);
                            imageBytes = Common.ImageToByteArray(image);
                            string imageBytesBase64String = Convert.ToBase64String(imageBytes);
                            ViewBag.DefaultImage = imageBytesBase64String;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    if (usrid == null)
                    {
                        //// Flag to indicate unavailability of web service.
                        //// Added By: Maninder Singh Wadhva on 11/24/2014.
                        //// Ticket: 942 Exception handeling in Gameplan.
                        return Json(new { serviceUnavailable = Common.RedirectOnServiceUnavailibilityPage }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }

            // Modified by Viral Kadiya on 11/04/2014 for PL ticket #917
            return PartialView("_UpdateUserDetails", objUserModel); // Client can edit user details from myaccount or team member details.
        }

        /// <summary>
        /// POST: Edit Existing User
        /// </summary>
        /// <param name="form"></param>
        /// <param name="file">user photo</param>
        /// <param name="formcollection"></param>
        /// <returns>If Page request from "MyTeam" then redirect to "Index" or "Edit" View</returns>
        [HttpPost]
        [ValidateInput(false)]////Added by Mitesh Vaishnav on 07/07/2014 for PL ticket #584
        public ActionResult Edit(UserModel form, HttpPostedFileBase file, FormCollection formcollection)//formcollection added by uday#555 )
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            var flag = formcollection["removeflag"];//added by uday #555 

            // Added by Sohel Pathan on 26/06/2014 for PL ticket #517
            ViewBag.isForDelete = "false";

            // Add By Nishant Sheth
            // Desc :: To resolve the edit data not works Not check passsword expresion due to hash encoding
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();
            foreach (var error in errors)
            {
                if (error.Key == "Password" || error.Key == "ConfirmPassword" || error.Key == "Email")
                {
                    ModelState.Remove(error.Key);
                }
            }
            // End By Nishant Sheth
            try
            {
                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        //To check JPG or PNG file formats only & size upto 1MB
                        if ((!file.ContentType.ToLower().Contains("jpg") && !file.ContentType.ToLower().Contains("jpeg") && !file.ContentType.ToLower().Contains("png")) || (file.ContentLength > 1 * 1024 * 1024))
                        {
                            TempData["ErrorMessage"] = Common.objCached.InvalidProfileImage;
                            if (form.UserId == Sessions.User.UserId)
                            {
                                LoadEditModeComponents(form.ClientId, "myaccount");
                            }
                            else
                            {
                                LoadEditModeComponents(form.ClientId, "myteam");
                            }
                            return View(form);
                        }
                    }
                    BDSService.User objUser = new BDSService.User();
                    objUser.UserId = form.UserId;
                    objUser.FirstName = form.FirstName;
                    objUser.LastName = form.LastName;
                    objUser.Email = form.Email;
                    objUser.Phone = form.Phone;     // Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517
                    objUser.JobTitle = form.JobTitle;
                    if (form.IsDeleted != null)
                    {
                        if (Convert.ToString(form.IsDeleted).ToLower() == "yes")
                        {
                            objUser.IsDeleted = true;
                            // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                            objUser.IsManager = form.IsManager;
                            objUser.NewManagerId = form.NewManagerId;
                            // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                        }
                        else
                        {
                            objUser.IsDeleted = false;
                        }
                    }
                    if (file != null)
                    {
                        using (MemoryStream target = new MemoryStream())
                        {
                            file.InputStream.CopyTo(target);
                            byte[] data = target.ToArray();
                            objUser.ProfilePhoto = data;
                        }
                    }
                    else if (flag != null && flag != "false" && flag != "" && flag != string.Empty)//added by uday #555 
                    {
                        objUser.ProfilePhoto = null;
                    }
                    else
                    {

                        //// Get User Profile photo.
                        BDSService.User objUsernew = objBDSServiceClient.GetTeamMemberDetails(form.UserId, Sessions.ApplicationId);
                        if (objUsernew != null)
                        {
                            objUser.ProfilePhoto = objUsernew.ProfilePhoto;
                        }
                    }
                    objUser.ClientId = form.ClientId;
                    objUser.RoleId = form.RoleId;
                    if (form.RoleId != null)
                    {
                        Role objRole = objBDSServiceClient.GetRoleDetails(form.RoleId);
                        if (objRole != null)
                        {
                            objUser.RoleCode = objRole.Code;
                            objUser.ManagerId = form.ManagerId;
                            // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                        }
                    }

                    // Added by Sohel Pathan on 10/07/2014 for Internal Functional Review Points #50
                    if (form.ManagerId == null)
                        form.ManagerName = "N/A";

                    //// Update User details.
                    int retVal = objBDSServiceClient.UpdateUser(objUser, Sessions.ApplicationId, Sessions.User.UserId);
                    if (retVal == 1)
                    {
                        TempData["SuccessMessage"] = Common.objCached.UserEdited;
                        if (form.UserId == Sessions.User.UserId)
                        {
                            objUser = objBDSServiceClient.GetTeamMemberDetails(form.UserId, Sessions.ApplicationId);
                            if (objUser != null)
                                Sessions.User = objUser;
                        }

                        //// Modified By Maninder Singh Wadhva to Address PL#203
                        System.Web.HttpContext.Current.Cache.Remove(form.UserId + "_photo");
                        System.Web.HttpContext.Current.Cache.Remove(form.UserId + "_name");
                        System.Web.HttpContext.Current.Cache.Remove(form.UserId + "_bu");//uday #416
                        System.Web.HttpContext.Current.Cache.Remove(form.UserId + "_jtitle");//uday #416

                        //Start Added by Mitesh Vaishnav for internal point #40 on 09-07-2014
                        if (form.IsDeleted != null)
                        {
                            if (Convert.ToString(form.IsDeleted).ToLower() == "yes")
                            {
                                int retDelete = objBDSServiceClient.DeleteUser(form.UserId, Sessions.ApplicationId);
                                return RedirectToAction("Index");   // Added by Sohel Pathan on 10/07/2014 for Internal Functional Review Points #50
                            }
                        }
                        //End Added by Mitesh Vaishnav for internal point #40 on 09-07-2014
                    }
                    else if (retVal == -2)
                    {
                        TempData["ErrorMessage"] = Common.objCached.UserDuplicate;
                    }
                    else if (retVal == -1)
                    {
                        TempData["ErrorMessage"] = Common.objCached.UserCantEdited;
                    }
                    else if (retVal == 0)
                    {
                        TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                    }
                }

                //// Check whether UserId is current loggined User or not.
                if (form.UserId == Sessions.User.UserId)
                {
                    // Start - Added by Sohel Pathan on 10/07/2014 for Internal Functional Review Points #50
                    ViewBag.SourceValue = "myaccount";
                    ViewBag.CurrentUserId = Convert.ToString(Sessions.User.UserId);
                    ViewBag.CurrentUserRole = Convert.ToString(Sessions.User.RoleCode);
                    // End - Added by Sohel Pathan on 10/07/2014 for Internal Functional Review Points #50
                }
                else
                {
                    LoadEditModeComponents(form.ClientId, "myteam");

                    // Start - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                    if ((bool)ViewBag.IsUserAdminAuthorized && form.ClientId != Guid.Empty)
                    {
                        // Get All User List for Manager
                        ViewData["ManagerList"] = GetManagersList(form.UserId, form.ClientId);
                    }
                    // End - Added by :- Sohel Pathan on 17/06/2014 for PL ticket #517 
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            // Modified by Viral Kadiya on 11/04/2014 for PL ticket #917         
            // add inner if condition to redirect organization hierarchy by Rahul Shah on 04/09/2015 fo PL Ticket #1112
            if (Request.UrlReferrer.AbsolutePath.Contains("OrganizationHierarchy"))
            {
                return RedirectToAction("OrganizationHierarchy", "Organization", new { area = "" });
            }
            else
            {
                if (form.UserId != Sessions.User.UserId)
                {
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Edit");
            }

        }

        #endregion

        #region User Notifications

        /// <summary>
        /// Load user notifications
        /// </summary>
        /// <returns>Return Notifications view</returns>
        public ActionResult Notifications()
        {
            // Added by Sohel Pathan on 19/06/2014 for PL ticket #537 to implement user permission Logic
            ViewBag.IsIntegrationCredentialCreateEditAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.IntegrationCredentialCreateEdit);
            ViewBag.IsUserAdminAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.UserAdmin);

            string typeSM = Enums.NotificationType.SM.ToString().ToLower();
            string typeAM = Enums.NotificationType.AM.ToString().ToLower();

            List<Notification> lstNotification = new List<Notification>();
            lstNotification = db.Notifications.Where(notfctn => notfctn.IsDeleted == false).ToList();
            ViewBag.SMCount = lstNotification.Where(notfctn => notfctn.NotificationType.ToLower() == typeSM).Count();
            ViewBag.AMCount = lstNotification.Where(notfctn => notfctn.NotificationType.ToLower() == typeAM).Count();

            List<UserNotification> viewModelList = new List<UserNotification>();
            foreach (var item in lstNotification)
            {
                UserNotification userNotification = new UserNotification();
                userNotification.NotificationId = item.NotificationId;

                //// Get Current loggined user notification count.
                int cntUsrNotification = db.User_Notification.Where(usrNotifctn => usrNotifctn.NotificationId == item.NotificationId && usrNotifctn.UserId == Sessions.User.UserId).Count();

                if (cntUsrNotification > 0)
                    userNotification.IsSelected = true;
                else
                    userNotification.IsSelected = false;

                userNotification.NotificationTitle = item.Title;
                userNotification.NotificationType = item.NotificationType;
                viewModelList.Add(userNotification);
            }
            return View(viewModelList.AsEnumerable());
        }

        /// <summary>
        /// Save user notifications
        /// </summary>
        /// <param name="notifications">comma seperated list of notification codes</param>
        /// <returns></returns>
        public void SaveNotifications(string notifications)
        {
            try
            {

                List<User_Notification> lstUserNotification = new List<User_Notification>();
                lstUserNotification = db.User_Notification.Where(usrNotifctn => usrNotifctn.UserId == Sessions.User.UserId).ToList();

                //// Remove all current Notifications set for User
                foreach (var item in lstUserNotification)
                {
                    db.Entry(item).State = EntityState.Modified;
                    db.User_Notification.Remove(item);
                    db.SaveChanges();
                }

                //// Save User Notifications
                if (!string.IsNullOrEmpty(notifications))
                {
                    string[] arrNotify = notifications.Split(',');
                    foreach (var notification in arrNotify)
                    {
                        int notificationId = Convert.ToInt32(notification);

                        //// Add User Notification data to table User_Notification.
                        User_Notification objUser_Notification = new User_Notification();
                        objUser_Notification.UserId = Sessions.User.UserId;
                        objUser_Notification.NotificationId = notificationId;
                        objUser_Notification.CreatedBy = Sessions.User.UserId;
                        objUser_Notification.CreatedDate = DateTime.Now;
                        db.Entry(objUser_Notification).State = EntityState.Added;
                        db.User_Notification.Add(objUser_Notification);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
            }
            TempData["SuccessMessage"] = Common.objCached.UserNotificationsSaved;
        }

        #endregion

        #region Other

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// To load user profile photo
        /// </summary>
        /// <param name="id">user</param>
        /// <param name="width">width of photo</param>
        /// <param name="height">height of photo</param>
        /// <param name="src">Load Team Member Image</param>
        /// <returns>Return User Image file</returns>
        public ActionResult LoadUserImage(string id = null, int width = 35, int height = 35, string src = null)
        {
            Guid userId = new Guid();
            byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
            try
            {
                if (id != null)
                {
                    userId = Guid.Parse(id);
                    BDSService.User objUser = new BDSService.User();
                    //// Get User profile photo.
                    objUser = objBDSServiceClient.GetTeamMemberDetails(userId, Sessions.ApplicationId);
                    if (objUser != null)
                    {
                        if (objUser.ProfilePhoto != null)
                        {
                            imageBytes = objUser.ProfilePhoto;
                        }
                    }
                }
                if (imageBytes != null)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                    {
                        ms.Write(imageBytes, 0, imageBytes.Length);
                        System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                        image = Common.ImageResize(image, width, height, true, false);
                        imageBytes = Common.ImageToByteArray(image);
                        // Modified by Viral Kadiya on 11/06/2014 for PL Ticket #917 to load team member profile image.
                        if (src == "myteam")
                            return Json(new { base64imgage = Convert.ToBase64String(imageBytes) }, JsonRequestBehavior.AllowGet);   // if src "myteam" then return json result for ajax query to display team member image.
                        return File(imageBytes, "image/jpg");
                    }
                }
            }
            catch (Exception e)
            {
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                    {
                        ms.Write(imageBytes, 0, imageBytes.Length);
                        System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                        image = Common.ImageResize(image, width, height, true, false);
                        imageBytes = Common.ImageToByteArray(image);
                        // Modified by Viral Kadiya on 11/06/2014 for PL Ticket #917 to load team member profile image.
                        if (src == "myteam")
                            return Json(new { base64imgage = Convert.ToBase64String(imageBytes) }, JsonRequestBehavior.AllowGet);   // if src "myteam" then return json result for ajax query to display team member image.
                        return File(imageBytes, "image/jpg");
                    }
                }
            }
            return View();
        }
        /// <summary>
        /// To display user profile photo
        /// </summary>
        /// <param name="id">user</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AjaxSubmit(int id = 0)
        {
            Session["ComponentContentLength"] = Request.Files[0].ContentLength;
            Session["ComponentContentType"] = Request.Files[0].ContentType;
            System.Drawing.Image imgRole = System.Drawing.Image.FromStream(Request.Files[0].InputStream);
            imgRole = Common.ImageResize(imgRole, 60, 60, true, false);
            byte[] b = Common.ImageToByteArray(imgRole);
            imgRole.Dispose();
            Request.Files[0].InputStream.Read(b, 0, Request.Files[0].ContentLength);
            Session["ComponentContentStream"] = b;
            return Content(Request.Files[0].ContentType + ";" + Request.Files[0].ContentLength);
        }

        /// <summary>
        /// To load user profile photo from current session
        /// </summary>
        /// <param name="id">userid</param>
        /// <returns>Return Image file or blank content.</returns>
        public ActionResult ImageLoad(int? id)
        {
            //// if Id is null and Session value is null then load "Not Found" image.
            if (!(Convert.ToInt32(id) > 0) && Session["ComponentContentStream"] == null)
            {
                byte[] imagebyte = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                if (imagebyte != null)
                    return File(imagebyte, "image/jpg");
            }
            if (Session["ComponentContentStream"] != null)
            {
                byte[] b = (byte[])Session["ComponentContentStream"];
                int length = (int)Session["ComponentContentLength"];
                string type = (string)Session["ComponentContentType"];
                Response.Buffer = true;
                Response.Charset = "";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = type;
                Response.BinaryWrite(b);
                Response.Flush();
                Response.End();
                return Content("");
            }
            return View();
        }

        /// <summary>
        /// Get list of managers for selected client. 
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>03/07/2014</CreatedDate>
        /// <param name="id">clientId</param>
        /// <param name="UserId">UserId</param>
        /// <returns>Return Managerlist in JsonResult</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetManagers(string id = null, string UserId = null)
        {
            Guid clientId = new Guid();
            if (id != null)
            {
                Guid.TryParse(id, out clientId);
            }
            List<UserModel> managerList = new List<UserModel>();
            if (UserId != null)
            {
                managerList = GetManagersList(Guid.Parse(UserId), clientId);
            }
            else
            {
                managerList = GetManagersList(Guid.Empty, clientId);
            }

            return Json(managerList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Userdefined Functions

        #region Get list of Managers
        /// <summary>
        /// Get list of Managers by Client and application Id.
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>11.06.2014</CreatedDate>
        /// <param name="UserId"></param>
        /// <param name="ClientId"></param>
        /// <returns>List<UserModel></returns>
        public List<UserModel> GetManagersList(Guid UserId, Guid ClientId)
        {
            if (Sessions.ApplicationId != Guid.Empty && Sessions.ApplicationId != null)
            {
                var UserList = objBDSServiceClient.GetManagerList(ClientId, Sessions.ApplicationId, UserId);
                var ManagerList = UserList.Select(a => new UserModel { ManagerId = a.UserId, ManagerName = a.ManagerName }).ToList();
                return ManagerList.OrderBy(a => a.ManagerName, new AlphaNumericComparer()).ToList();
            }
            return null;
        }
        #endregion

        #endregion

        #region Assign Other Application User

        #region Assign User Method
        /// <summary>
        /// Add other application user into current application with selected role. 
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>10/07/2014</CreatedDate>
        /// <param name="UserId">UserId</param>
        /// <param name="RoleId">RoleId</param>
        /// <returns>Return Success message.</returns>
        public ActionResult AssignUser(string UserId, string RoleId)
        {
            try
            {
                int res = objBDSServiceClient.AssignUser(Guid.Parse(UserId), Guid.Parse(RoleId), Sessions.ApplicationId, Sessions.User.UserId);
                TempData["SuccessMessage"] = Common.objCached.UserAdded;
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    return Json(new { serviceUnavailable = Url.Content("#") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(Common.objCached.ErrorOccured, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #endregion

        #region Currency
        //insertation start 09/08/2016 kausha #2492 Following  is added to get and save currency.
        public ActionResult Currency()
        {
            List<CurrencyModel> lstCurrency = new List<CurrencyModel>();
            IEnumerable<BDSService.Currency> lstCurrencydata = objBDSServiceClient.GetAllCurrency();
            IEnumerable<BDSService.Currency> lstClientCurrency = objBDSServiceClient.GetClientCurrency(Sessions.User.ClientId);

            if (lstCurrencydata != null)
            {

                foreach (var item in lstCurrencydata)
                {
                    CurrencyModel objCurrency = new CurrencyModel();
                    //    objCurrency.CurrencyId = item.CurrencyId;
                    objCurrency.CurrencySymbol = item.CurrencySymbol;
                    objCurrency.CurrencyDetail = item.CurrencyDetail;
                    objCurrency.ISOCurrencyCode = item.ISOCurrencyCode;
                    var defaultCurrency = lstClientCurrency.Where(w => w.IsDefault == true && w.ISOCurrencyCode == item.ISOCurrencyCode).FirstOrDefault();
                    if (defaultCurrency != null)
                        objCurrency.IsDefault = true;
                    else
                        objCurrency.IsDefault = false;
                    if (lstClientCurrency.Where(w => w.ISOCurrencyCode == item.ISOCurrencyCode).Any())
                        objCurrency.ClientId = Sessions.User.ClientId;
                    objCurrency.ISOCurrencyCode = item.ISOCurrencyCode;
                    lstCurrency.Add(objCurrency);
                }
            }
            return View("Currency", lstCurrency.AsEnumerable());
        }
        public JsonResult SaveClientCurrency(List<string> curruncies)
        {
            bool status = false;
            try
            {
                status = objBDSServiceClient.SaveClientCurrency(curruncies, Sessions.User.ClientId, Sessions.User.UserId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
            }
            TempData["SuccessMessage"] = Common.objCached.CurrencySaved;
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConversionRate()
        {
            //  string mode = InsepectMode;
            Plangrid objplangrid = new Plangrid();
            List<PlanHead> headobjlist = new List<PlanHead>();
            List<PlanDHTMLXGridDataModel> lineitemrowsobjlist = new List<PlanDHTMLXGridDataModel>();
            PlanDHTMLXGridDataModel lineitemrowsobj = new PlanDHTMLXGridDataModel();
            PlanController objPlanController = new PlanController();
            string customFieldEntityValue = string.Empty;
            string DropdowList = Enums.CustomFieldType.DropDownList.ToString();
            PlanMainDHTMLXGrid objPlanMainDHTMLXGrid = new PlanMainDHTMLXGrid();
            string stylecolorgray = "color:#999;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;max-width:85%;";
            try
            {

                bool isRequire = false;

                string Gridheder = string.Empty;
                string coltype = string.Empty;


                headobjlist = GenerateHeader();

                int requiredcnt = 0;
                for (int i = 0; i < 5; i++)
                {
                    string RowID = "mediacode." + i.ToString();
                    lineitemrowsobj = new PlanDHTMLXGridDataModel();
                    List<Plandataobj> lineitemdataobjlist = new List<Plandataobj>();


                    lineitemrowsobj.id = RowID;

                    Plandataobj lineitemdataobj = new Plandataobj();


                    lineitemdataobj = new Plandataobj();
                    lineitemdataobj.value = "Us dollar";
                    lineitemdataobjlist.Add(lineitemdataobj);


                    lineitemdataobj = new Plandataobj();
                    lineitemdataobj.value = "50";
                    lineitemdataobjlist.Add(lineitemdataobj);


                    //if (IsArchive == false)
                    //{
                    //    lineitemdataobj = new Plandataobj();
                    //    if (mode == Convert.ToString(Enums.InspectPopupMode.Edit) || mode == Convert.ToString(Enums.InspectPopupMode.Add))
                    //        lineitemdataobj.value = "<span alt=" + Convert.ToString(tacticId) + " class='plus-circle' title='Add Media Code'><i class='fa fa-plus-circle CodeNew' aria-hidden='true' rowid=" + RowID + " onclick='javascript:OpenGridPopup(event)' title='Add Media Code'></i></span><div class='honeycombbox-icon-gantt' rowid=" + RowID + " onclick='javascript:AddRemoveMediaCode(this);' title='Select' altid=" + item.MediaCodeId.ToString() + "></div><span class='archive' title='Archive' ><i class='fa fa-archive CodeArchive' aria-hidden='true' rowid=" + RowID + " onclick='javascript:ArchiveMediaCode(event)' title='Archive'></i></span>";
                    //    else
                    //        lineitemdataobj.value = "<span alt=" + Convert.ToString(tacticId) + " class='plus-circle disabled' title='Add Media Code'><i class='fa fa-plus-circle CodeNew' aria-hidden='true' rowid=" + RowID + " title='Add Media Code'></i></span><div class='honeycombbox-icon-gantt' rowid=" + RowID + " onclick='javascript:AddRemoveMediaCode(this);' title='Select' altid=" + item.MediaCodeId.ToString() + "></div><span class='archive disabled' title='Archive'><i class='fa fa-archive CodeArchive' aria-hidden='true' rowid=" + RowID + " title='Archive'></i></span>";

                    //    lineitemdataobjlist.Add(lineitemdataobj);
                    //}
                    //else
                    //{
                    //    lineitemdataobj = new Plandataobj();
                    //    if (mode == Convert.ToString(Enums.InspectPopupMode.Edit) || mode == Convert.ToString(Enums.InspectPopupMode.Add))
                    //        lineitemdataobj.value = "<span class='archive' title='Unarchive'><i class='fa fa-archive UnarchiveMediaCode' aria-hidden='true' rowid=" + RowID + " onclick='javascript:UnarchiveMediaCode(event)' title='Unarchive'></i></span>";
                    //    else
                    //        lineitemdataobj.value = "<span class='archive disabled' title='Unarchive'><i class='fa fa-archive UnarchiveMediaCode' aria-hidden='true' rowid=" + RowID + " title='Unarchive'/>";

                    //    lineitemdataobjlist.Add(lineitemdataobj);
                    //}

                    lineitemdataobj = new Plandataobj();
                    lineitemdataobj.value = "<span class='spnMediacode'>" + "60" + "</span>";
                    //  lineitemdataobj.value = "<span class='spnMediacode'>" + HttpUtility.HtmlEncode(item.MediaCode) + "</span>";
                    lineitemdataobj.actval = HttpUtility.HtmlEncode("60");
                    // lineitemdataobj.style = stylecolorgray;
                    lineitemdataobjlist.Add(lineitemdataobj);

                    lineitemdataobj = new Plandataobj();
                    lineitemdataobj.value = "<span class='spnMediacode'>" + "60" + "</span>";
                    //  lineitemdataobj.value = "<span class='spnMediacode'>" + HttpUtility.HtmlEncode(item.MediaCode) + "</span>";
                    lineitemdataobj.actval = HttpUtility.HtmlEncode("70");
                    //  lineitemdataobj.style = stylecolorgray;
                    lineitemdataobjlist.Add(lineitemdataobj);


                    //foreach (var Cust in lstmediaCodeCustomfield)
                    //{
                    //    if (Cust.CustomFieldTypeName.ToString() == Convert.ToString(Enums.CustomFieldType.TextBox))
                    //    {
                    //        coltype = "ro";


                    //        isRequire = Cust.IsRequired;


                    //        if (IsArchive)
                    //            coltype = "ro";

                    //        var optionValue = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).Select(o => o.CustomFieldValue).FirstOrDefault();
                    //        customFieldEntityValue = (optionValue != null) ? optionValue : string.Empty;

                    //    }
                    //    else if (Convert.ToString(Cust.CustomFieldTypeName) == Convert.ToString(Enums.CustomFieldType.DropDownList))
                    //    {
                    //        if (IsArchive == false)
                    //        {

                    //            coltype = "coro";

                    //            var optionValue = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).Select(o => o.CustomFieldValue).FirstOrDefault();
                    //            customFieldEntityValue = (optionValue != null) ? optionValue : string.Empty;
                    //            isRequire = Cust.IsRequired;

                    //        }
                    //        else
                    //        {
                    //            coltype = "ro";

                    //            var objoptionID = item.CustomFieldList.Where(o => o.CustomFieldId == Cust.CustomFieldId).FirstOrDefault();
                    //            if (objoptionID != null)
                    //            {
                    //                int intoptionID = Convert.ToInt32(objoptionID.CustomFieldValue);
                    //                string OptionValue = Cust.Option.Where(a => a.CustomFieldOptionId == intoptionID).Select(a => a.CustomFieldOptionValue).FirstOrDefault();
                    //                customFieldEntityValue = (OptionValue != null) ? OptionValue : string.Empty;
                    //            }
                    //            else
                    //                customFieldEntityValue = string.Empty;
                    //        }
                    //    }
                    //    if (isRequire)
                    //        requiredcnt = requiredcnt + 1;
                    //    lineitemdataobj = new Plandataobj();
                    //    if (!string.IsNullOrEmpty(customFieldEntityValue))
                    //        lineitemdataobj.value = HttpUtility.HtmlEncode(customFieldEntityValue);
                    //    else
                    //        lineitemdataobj.value = "--";
                    //    lineitemdataobj.locked = "1";
                    //    lineitemdataobj.actval = Convert.ToString(isRequire);
                    //    lineitemdataobj.style = stylecolorgray;

                    //    lineitemdataobjlist.Add(lineitemdataobj);
                    // }
                    lineitemrowsobj.data = lineitemdataobjlist;

                    lineitemrowsobjlist.Add(lineitemrowsobj);
                }

                objPlanMainDHTMLXGrid.head = headobjlist;
                objPlanMainDHTMLXGrid.rows = lineitemrowsobjlist;


                objplangrid.PlanDHTMLXGrid = objPlanMainDHTMLXGrid;
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return View("ConversionRate", objplangrid);
        }
        public List<PlanHead> GenerateHeader()
        {
            List<PlanHead> headobjlist = new List<PlanHead>();
            string coltype = string.Empty;
            List<PlanOptions> viewoptionlist = new List<PlanOptions>();
            // string mode = Mode;
            bool IsRequired = false;
            List<RequriedCustomField> lstRequiredcustomfield = new List<RequriedCustomField>();
            try
            {

                PlanHead headobjother = new PlanHead();


                headobjother = new PlanHead();
                headobjother.type = "ro";
                headobjother.id = "Currency";
                headobjother.sort = "na";
                headobjother.width = 100;
                headobjother.value = "Currency";
                headobjlist.Add(headobjother);

                headobjother = new PlanHead();
                headobjother.type = "ed";
                headobjother.id = "Jan";
                headobjother.sort = "na";
                headobjother.width = 100;
                headobjother.value = "Jan";
                headobjlist.Add(headobjother);


                headobjother = new PlanHead();
                headobjother.type = "ed";
                headobjother.id = "Feb";
                headobjother.sort = "na";

                headobjother.width = 100;
                //if (IsArchive == false)
                //{
                //    headobjother.value = "<input type='checkbox' id = 'MediaCodeSelectAll' value='SelectAll'  title='SelectAll'  class='selectInput'><span class='selectall'> Select All</span></input>";
                //}
                //else
                headobjother.value = "Feb";
                headobjlist.Add(headobjother);

                headobjother = new PlanHead();
                headobjother.type = "ed";
                headobjother.id = "March";
                headobjother.sort = "na";
                headobjother.width = 200;
                headobjother.value = "March";

                headobjlist.Add(headobjother);
                //var columncnt = lstmediaCodeCustomfield.Count;
                var colwidth = 200;
                //if (columncnt != 0 && columncnt < 4)
                //{
                //    colwidth = 725 / columncnt;
                //}

                if (lstRequiredcustomfield != null && lstRequiredcustomfield.Count > 0)
                    ViewBag.RequiredList = lstRequiredcustomfield;
                else
                    ViewBag.RequiredList = "";
            }
            catch (Exception objException)
            {
                ErrorSignal.FromCurrentContext().Raise(objException);
            }
            return headobjlist;

        }
        //insertation end 09/08/2016 kausha #2492 Following  is added to get and save currency.
        #endregion

        #region alerts
        #region User Alerts
        /// <summary>
        /// Load user alerts
        /// </summary>
        /// <returns>Return Alerts view</returns>
        public ActionResult Alerts()
        {

            List<SelectListItem> lstSyncFreq = new List<SelectListItem>();
            try
            {
                CacheObject objCache = new CacheObject();
                var lstentity = objcommonalert.SearchEntities(Sessions.User.ClientId);
                objCache.AddCache(Enums.CacheObject.ClientEntityList.ToString(), lstentity);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View();
        }
        #endregion

        #region method to get list of entity list as per search text
        public JsonResult ListEntity(string term)
        {
            CacheObject objCache = new CacheObject();
            List<SearchEntity> EntityList = new List<SearchEntity>();
            List<vClientWise_EntityList> lstentity = new List<vClientWise_EntityList>();
            try
            {
                var ClientEntityList = (List<vClientWise_EntityList>)objCache.Returncache(Enums.CacheObject.ClientEntityList.ToString());

                if (ClientEntityList != null)
                {
                    lstentity = ClientEntityList;
                }
                else
                {
                    lstentity = objcommonalert.SearchEntities(Sessions.User.ClientId);
                }

                EntityList = lstentity.Where(a => a.ClientId == Sessions.User.ClientId && a.EntityTitle.ToLower().Contains(term.ToLower())).Select(a => new SearchEntity
                {
                    category = a.Entity,
                    value = a.EntityId,
                    label = a.EntityTitle
                }).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            //  return Json(new { Success = true, SearchData = EntityList }, JsonRequestBehavior.AllowGet);
            return Json(EntityList, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region mfunction to get performance fector for create rule
        public SelectList GetPerformancefector()
        {
            var lstGoalTypes = Enum.GetValues(typeof(Enums.PerformanceFector)).Cast<Enums.PerformanceFector>().Select(a => a.ToString()).ToList();
            var lstGoalTypeListFromDB = db.Stages.Where(a => a.IsDeleted == false && a.ClientId == Sessions.User.ClientId && lstGoalTypes.Contains(a.Code)).Select(a => a).ToList();
            Stage objStage = new Stage();
            string revGoalType = Convert.ToString(Enums.PerformanceFector.Revenue);
            objStage.Title = revGoalType;
            objStage.Code = revGoalType.ToUpper();
            lstGoalTypeListFromDB.Add(objStage);
            objStage = new Stage();
            revGoalType = Convert.ToString(Enums.PerformanceFector.PlannedCost);
            objStage.Title = revGoalType;
            objStage.Code = revGoalType.ToUpper();
            lstGoalTypeListFromDB.Add(objStage);
            objStage = new Stage();
            objStage.Title = "Select";
            objStage.Code = "0";
            lstGoalTypeListFromDB.Insert(0, objStage);
            return new SelectList(lstGoalTypeListFromDB, "Code", "Title");
        }
        #endregion
        #region Method to save alertRule
        [HttpPost]
        public JsonResult SaveAlertRule(AlertRuleDetail RuleDetail, int RuleID = 0)
        {
            try
            {
                //Insert Rule
                if (RuleDetail != null)
                {

                    AlertRuleDetail objRule = RuleDetail;
                    int EntityID = Int32.Parse(objRule.EntityID);
                    int indicatorGoal = Int32.Parse(objRule.IndicatorGoal);
                    int Completiongoal = Int32.Parse(objRule.CompletionGoal);

                    if (objRule.EntityID != null && Convert.ToInt32(objRule.EntityID) != 0)
                    {
                        if (RuleID == 0)
                        {
                            Boolean IsExists = objcommonalert.IsAlertRuleExists(EntityID, Completiongoal, indicatorGoal, objRule.Indicator, objRule.IndicatorComparision, Sessions.User.UserId, 0);
                            if (!IsExists)
                            {
                                int result = objcommonalert.SaveAlert(objRule, Sessions.User.ClientId, Sessions.User.UserId);
                                if (result > 0)
                                    return Json(new { Success = true, SuccessMessage = Common.objCached.SuccessAlertRule }, JsonRequestBehavior.AllowGet);
                                else
                                    return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

                            }
                            else
                                return Json(new { Success = false, ErrorMessage = Common.objCached.DuplicateAlertRule }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //update rule
                            Boolean IsExists = objcommonalert.IsAlertRuleExists(EntityID, Completiongoal, indicatorGoal, objRule.Indicator, objRule.IndicatorComparision, Sessions.User.UserId, RuleID);
                            if (!IsExists)
                            {
                                int result = objcommonalert.UpdateAlertRule(objRule, Sessions.User.UserId);
                                if (result > 0)
                                    return Json(new { Success = true, SuccessMessage = Common.objCached.UpdateAlertRule }, JsonRequestBehavior.AllowGet);
                                else
                                    return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
                            }
                            else
                                return Json(new { Success = false, ErrorMessage = Common.objCached.DuplicateAlertRule }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                        return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Success = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion
        /// <summary>
        /// Get Weekdays list
        /// </summary>
        /// <returns></returns>
        public SelectList GetWeekDaysList()
        {
            List<SelectListItem> lstWeekdays = new List<SelectListItem>();
            foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
            {
                SelectListItem objTime = new SelectListItem();
                objTime.Text = item.ToString();
                objTime.Value = ((int)item).ToString();
                lstWeekdays.Add(objTime);
            }
            return new SelectList(lstWeekdays);
        }
        #region method to get list of alert rule
        public ActionResult GetAlertRuleList()
        {

            AlertRule objalert = new AlertRule();
            List<SelectListItem> lstSyncFreq = new List<SelectListItem>();
            List<SelectListItem> lstWeekdays = new List<SelectListItem>();

            try
            {
                var GoalTypeList = GetPerformancefector();
                objalert.GoalType = GoalTypeList;

                objalert.PerformanceComparison = new SelectList(Enums.DictPerformanceComparison, "Key", "Value");
                objalert.GoalNum = new SelectList(Enums.DictGoalNum.ToList(), "Value", "Value");

                SelectListItem objItem1 = new SelectListItem();

                objItem1.Text = SyncFrequencys.Daily.ToString();
                objItem1.Value = SyncFrequencys.Daily.ToString();
                lstSyncFreq.Add(objItem1);

                objItem1 = new SelectListItem();
                objItem1.Text = SyncFrequencys.Weekly.ToString();
                objItem1.Value = SyncFrequencys.Weekly.ToString();
                lstSyncFreq.Add(objItem1);

                objItem1 = new SelectListItem();
                objItem1.Text = SyncFrequencys.Monthly.ToString();
                objItem1.Value = SyncFrequencys.Monthly.ToString();
                lstSyncFreq.Add(objItem1);

                objalert.lstFrequency = new SelectList(lstSyncFreq, "Value", "Text", lstSyncFreq.First());

                foreach (var item in Enum.GetValues(typeof(DayOfWeek)))
                {
                    SelectListItem objTime = new SelectListItem();
                    objTime.Text = item.ToString();
                    objTime.Value = ((int)item).ToString();
                    lstWeekdays.Add(objTime);
                }
                objalert.lstWeekdays = new SelectList(lstWeekdays, "Value", "Text", lstWeekdays.First());
                List<AlertRuleDetail> lstRuledetail = objcommonalert.GetAletRuleList(Sessions.User.UserId, Sessions.User.ClientId);
                objalert.lstAlertRule = lstRuledetail;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("_AlertListing", objalert);
        }
        #endregion
        #region method to delete alert rule
        [HttpPost]
        public JsonResult DeleteAlertRule(int RuleId)
        {
            try
            {
                if (RuleId != 0)
                {
                    int result = objcommonalert.DeleteAlertRule(RuleId);
                    if (result > 0)
                        return Json(new { Success = true, SuccessMessage = Common.objCached.DeleteAlertRule }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { Success = false, ErrorMessage = Common.objCached.ErrorOccured }, JsonRequestBehavior.AllowGet);

            }

        }
        #endregion
        #region method to disable alert rule
        public JsonResult DisableAlertRule(int RuleId, bool RuleOn)
        {
            try
            {
                if (RuleId != 0)
                {
                    int result = objcommonalert.DisableAlertRule(RuleId, RuleOn);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion
        #endregion
    }
}
