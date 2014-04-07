using Elmah;
using RevenuePlanner.BDSService;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

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

        #endregion

        #region Team Member Listing

        /// <summary>
        /// Team Member listing
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            string permRoleCodesForDel = string.Empty;
            List<UserModel> teamMemberList = new List<UserModel>();
            List<BDSService.User> lstUser = null;
            try
            {
                if (Sessions.IsSystemAdmin)
                {
                    lstUser = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, true);
                }
                else
                {
                    lstUser = objBDSServiceClient.GetTeamMemberList(Sessions.User.ClientId, Sessions.ApplicationId, Sessions.User.UserId, false);
                }
                if (lstUser.Count() > 0)
                {
                    foreach (var user in lstUser)
                    {
                        UserModel objUserModel = new UserModel();
                        objUserModel.BusinessUnitId = user.BusinessUnitId;
                        objUserModel.BusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == objUserModel.BusinessUnitId).Select(b => b.Title).FirstOrDefault();
                        objUserModel.ClientId = user.ClientId;
                        objUserModel.Client = user.Client;
                        objUserModel.DisplayName = user.DisplayName;
                        objUserModel.Email = user.Email;
                        objUserModel.FirstName = user.FirstName;
                        objUserModel.JobTitle = user.JobTitle;
                        objUserModel.LastName = user.LastName;
                        objUserModel.Password = user.Password;
                        objUserModel.GeographyId = user.GeographyId;
                        objUserModel.Geography = db.Geographies.Where(geo => geo.GeographyId == objUserModel.GeographyId).Select(g => g.Title).FirstOrDefault();
                        objUserModel.UserId = user.UserId;
                        objUserModel.RoleCode = user.RoleCode;
                        objUserModel.RoleId = user.RoleId;
                        objUserModel.RoleTitle = user.RoleTitle;
                        teamMemberList.Add(objUserModel);
                    }
                }
                if (Sessions.IsSystemAdmin || Sessions.IsClientAdmin || Sessions.IsDirector)
                {
                    ViewBag.IsAdmin = "true";
                }
                Enums.Role role = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, Sessions.User.RoleCode);
                switch (role)
                {
                    case Enums.Role.SystemAdmin:
                        permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.CA) + "," + Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                        break;
                    case Enums.Role.ClientAdmin:
                        permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                        break;
                    case Enums.Role.Director:
                        permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.P);
                        break;
                    case Enums.Role.Planner:
                        permRoleCodesForDel = string.Empty;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            ViewBag.PermRoleCodesForDel = permRoleCodesForDel;
            return View(teamMemberList.AsEnumerable());
        }

        #endregion

        #region Change Password

        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }
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
            try
            {
                if (ModelState.IsValid)
                {
                    if (Sessions.User.UserId != null)
                    {
                        if (form.NewPassword != form.ConfirmNewPassword)
                        {
                            TempData["ErrorMessage"] = Common.objCached.UserPasswordDoNotMatch;
                            return View(form);
                        }

                        /* ------------------ Single hash password ----------------------*/
                        string SingleHash_CurrentPassword = Common.ComputeSingleHash(form.CurrentPassword.ToString().Trim());
                        string SingleHash_NewPassword = Common.ComputeSingleHash(form.NewPassword.ToString().Trim());
                        /* ---------------------------------------------------------------*/

                        int retVal = objBDSServiceClient.ChangePassword(Sessions.User.UserId, SingleHash_NewPassword, SingleHash_CurrentPassword);

                        if (retVal == -1)
                        {
                            TempData["ErrorMessage"] = Common.objCached.CurrentUserPasswordNotCorrect;
                        }
                        else if (retVal == 1)
                        {
                            ChangePasswordMail();
                            //Redirect users logging in for the first time to the change password module
                            if (Sessions.User.LastLoginDate == null && Sessions.RedirectToChangePassword)
                            {
                                Sessions.RedirectToChangePassword = false;
                                //Update last login date for user
                                objBDSServiceClient.UpdateLastLoginDate(Sessions.User.UserId, Sessions.ApplicationId);

                                if (Sessions.User.SecurityQuestionId == null)
                                {
                                    Sessions.RedirectToSetSecurityQuestion = true;
                                    return RedirectToAction("SetSecurityQuestion", "Login");
                                }

                                return RedirectToAction("Index", "Home");
                            }
                            TempData["SuccessMessage"] = Common.objCached.UserPasswordChanged;
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
                isValid = objBDSServiceClient.CheckCurrentPassword(Sessions.User.UserId, SingleHash_CurrentPassword);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
                    Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationChangePassword));
                    if (notification != null)
                    {
                        string replyToEmail = System.Configuration.ConfigurationManager.AppSettings.Get("ReplyToMail");
                        string emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", Sessions.User.FirstName);
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

        #region Security Question

        /// <summary>
        /// Security Question
        /// </summary>
        /// <returns></returns>
        public ActionResult SecurityQuestion()
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();

            SecurityQuestionListModel objSecurityQuestionListModel = new SecurityQuestionListModel();
            objSecurityQuestionListModel.Answer = Sessions.User.Answer;
            objSecurityQuestionListModel.SecurityQuestionId = Convert.ToInt32(Sessions.User.SecurityQuestionId);
            objSecurityQuestionListModel.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);

            return View(objSecurityQuestionListModel);
        }

        /// <summary>
        /// Post : security question view
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SecurityQuestion(SecurityQuestionListModel form)
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            try
            {
                
                BDSService.User objUser = new BDSService.User();
                objUser.UserId = Sessions.User.UserId;
                objUser.SecurityQuestionId = form.SecurityQuestionId;
                objUser.Answer = form.Answer;
                int retVal = objBDSServiceClient.UpdateUserSecurityQuestion(objUser);

                if (retVal == -1)
                {
                    TempData["ErrorMessage"] = Common.objCached.SecurityQuestionChangesNotApplied;
                }
                else if (retVal == 1)
                {

                    Sessions.User.SecurityQuestionId = form.SecurityQuestionId;
                    Sessions.User.Answer = form.Answer;

                    TempData["SuccessMessage"] = Common.objCached.SecurityQuestionChangesApplied;
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }

            var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();
            form.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);

            return View(form);

        }

        /// <summary>
        /// Method to get the Select list item
        /// </summary>
        /// <param name="QuestionList"></param>
        /// <returns></returns>
        public List<SelectListItem> GetQuestionList(List<BDSService.SecurityQuestion> QuestionList)
        {
            List<SelectListItem> optionslist = new List<SelectListItem>();

            optionslist = QuestionList.AsEnumerable().Select(x => new SelectListItem
            {
                Value = Convert.ToString(x.SecurityQuestionId),
                Text = x.SecurityQuestion1
            }).ToList();

            return optionslist;
        }

        #endregion

        #region Delete User

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id">user to be deleted</param>
        /// <returns></returns>
        public ActionResult Delete(Guid id)
        {
            try
            {
                if (id != null)
                {
                    string userRole = objBDSServiceClient.GetUserRole(id, Sessions.ApplicationId);
                    Enums.Role delUserRole = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, userRole);
                    Enums.Role currUserRole = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, Sessions.User.RoleCode);
                    switch (currUserRole)
                    {
                        case Enums.Role.SystemAdmin:
                            if (!(delUserRole == Enums.Role.ClientAdmin) && !(delUserRole == Enums.Role.Director) && !(delUserRole == Enums.Role.Planner))
                            {
                                TempData["ErrorMessage"] = Common.objCached.UserCantDeleted;
                                return RedirectToAction("Index");
                            }
                            break;
                        case Enums.Role.ClientAdmin:
                            if (!(delUserRole == Enums.Role.Director) && !(delUserRole == Enums.Role.Planner))
                            {
                                TempData["ErrorMessage"] = Common.objCached.UserCantDeleted;
                                return RedirectToAction("Index");
                            }
                            break;
                        case Enums.Role.Director:
                            if (!(delUserRole == Enums.Role.Planner))
                            {
                                TempData["ErrorMessage"] = Common.objCached.UserCantDeleted;
                                return RedirectToAction("Index");
                            }
                            break;
                    }
                    int retVal = objBDSServiceClient.DeleteUser(id);
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
            string permRoleCodesForIns = string.Empty;
            Enums.Role role = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, Sessions.User.RoleCode);
            switch (role)
            {
                case Enums.Role.SystemAdmin:
                    permRoleCodesForIns = Convert.ToString(Enums.RoleCodes.CA) + "," + Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    break;
                case Enums.Role.ClientAdmin:
                    permRoleCodesForIns = Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    break;
                case Enums.Role.Director:
                    permRoleCodesForIns = Convert.ToString(Enums.RoleCodes.P);
                    break;
                case Enums.Role.Planner:
                    permRoleCodesForIns = string.Empty;
                    break;
                default:
                    break;
            }
            if (!String.IsNullOrWhiteSpace(clientId))
            {
                Guid userClientId = Guid.Parse(clientId);
                ViewData["BusinessUnits"] = db.BusinessUnits.Where(bu => bu.ClientId == userClientId && bu.IsDeleted == false).OrderBy(q => q.Title).ToList();
                ViewData["Geographies"] = db.Geographies.Where(r => r.IsDeleted == false && r.ClientId == userClientId).OrderBy(q => q.Title).ToList();
            }
            else
            {
                ViewData["BusinessUnits"] = null;
                ViewData["Geographies"] = null;
            }

            ViewData["Roles"] = objBDSServiceClient.GetRoleList(permRoleCodesForIns);
            ViewBag.CurrClientId = Sessions.User.ClientId;
            ViewBag.CurrClient = Sessions.User.Client;

            if (Sessions.IsSystemAdmin)
            {
                ViewBag.IsSysAdmin = true;
                ViewData["Clients"] = objBDSServiceClient.GetClientList();
            }
            else
            {
                ViewBag.IsSysAdmin = false;
            }
        }

        /// <summary>
        /// Add New User
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            try
            {
                if (Sessions.RolePermission != null)
                {
                    Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                    switch (permission)
                    {
                        case Common.Permission.FullAccess:
                            break;
                        case Common.Permission.NoAccess:
                            return RedirectToAction("Index", "NoAccess");
                        case Common.Permission.NotAnEntity:
                            break;
                        case Common.Permission.ViewOnly:
                            ViewBag.IsViewOnly = "true";
                            break;
                    }
                }

                if (Sessions.IsSystemAdmin)
                {
                    LoadCreateModeComponents();
                }
                else
                {
                    LoadCreateModeComponents(Convert.ToString(Sessions.User.ClientId));
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
        /// <param name="form"></param>
        /// <param name="file">user photo</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(UserModel form, HttpPostedFileBase file)
        {
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
                    objUser.JobTitle = form.JobTitle;
                    objUser.ClientId = form.ClientId;
                    objUser.RoleId = form.RoleId;
                    if (file != null)
                    {
                        MemoryStream target = new MemoryStream();
                        file.InputStream.CopyTo(target);
                        byte[] data = target.ToArray();
                        objUser.ProfilePhoto = data;
                    }
                    objUser.BusinessUnitId = form.BusinessUnitId;
                    objUser.GeographyId = form.GeographyId;

                    int retVal = objBDSServiceClient.CreateUser(objUser, Sessions.ApplicationId, Sessions.User.UserId);
                    if (retVal == 1)
                    {
                        UserCreatedMail(objUser, password);
                        TempData["SuccessMessage"] = Common.objCached.UserAdded;
                        return RedirectToAction("Index");
                    }
                    else if (retVal == -1)
                    {
                        TempData["ErrorMessage"] = Common.objCached.UserDuplicate;
                    }
                    else if (retVal == 0)
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
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
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
                Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationUserCreated));
                if (notification != null)
                {
                    string emailBody = notification.EmailContent.Replace("[NameToBeReplaced]", user.FirstName + " " + user.LastName).Replace("[LoginToBeReplaced]", user.Email).Replace("[PasswordToBeReplaced]", password);
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
                isValid = objBDSServiceClient.CheckEmail(email);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
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

        #endregion

        #region Edit Existing User

        /// <summary>
        /// Load Edit Mode Components
        /// </summary>
        /// <param name="clientId">client</param>
        /// <returns></returns>
        private void LoadEditModeComponents(Guid clientId, string src)
        {
            string permRoleCodesForDel = string.Empty;
            string permRoleCodesForUpd = string.Empty;
            Enums.Role role = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, Sessions.User.RoleCode);
            switch (role)
            {
                case Enums.Role.SystemAdmin:
                    permRoleCodesForUpd = Convert.ToString(Enums.RoleCodes.CA) + "," + Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    if (src.ToLower() == "myaccount")
                    {
                        permRoleCodesForUpd = Convert.ToString(Enums.RoleCodes.SA) + "," + permRoleCodesForUpd;
                    }
                    permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.CA) + "," + Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    break;
                case Enums.Role.ClientAdmin:
                    permRoleCodesForUpd = Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.P) + "," + Convert.ToString(Enums.RoleCodes.D);
                    break;
                case Enums.Role.Director:
                    permRoleCodesForUpd = Convert.ToString(Enums.RoleCodes.P);
                    permRoleCodesForDel = Convert.ToString(Enums.RoleCodes.P);
                    break;
                case Enums.Role.Planner:
                    permRoleCodesForUpd = string.Empty;
                    permRoleCodesForDel = string.Empty;
                    break;
                default:
                    break;
            }

            if (clientId != null)
            {
                ViewData["BusinessUnits"] = db.BusinessUnits.Where(bu => bu.ClientId == clientId && bu.IsDeleted == false).OrderBy(q => q.Title).ToList();
                ViewData["Geographies"] = db.Geographies.Where(r => r.IsDeleted == false && r.ClientId == clientId).OrderBy(q => q.Title).ToList();
            }
            if (!string.IsNullOrWhiteSpace(src))
            {
                ViewBag.SourceValue = src;
            }
            ViewData["Clients"] = objBDSServiceClient.GetClientList();
            ViewData["Roles"] = objBDSServiceClient.GetRoleList(permRoleCodesForUpd);
            ViewBag.CurrentUserId = Convert.ToString(Sessions.User.UserId);
            ViewBag.CurrentUserRole = Convert.ToString(Sessions.User.RoleCode);
            ViewBag.PermRoleCodesForUpd = permRoleCodesForUpd;
            ViewBag.PermRoleCodesForDel = permRoleCodesForDel;
        }

        /// <summary>
        /// Edit Existing User
        /// </summary>
        /// <param name="usrid">user</param>
        /// <param name="src">source either "myaccount" or "myteam"</param>
        /// <returns></returns>
        public ActionResult Edit(string usrid = null, string src = "myaccount")
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }
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
                    objUserModel.BusinessUnitId = objUser.BusinessUnitId;
                    objUserModel.BusinessUnit = db.BusinessUnits.Where(bu => bu.BusinessUnitId == objUserModel.BusinessUnitId).Select(b => b.Title).FirstOrDefault();
                    objUserModel.ClientId = objUser.ClientId;
                    objUserModel.Client = objUser.Client;
                    objUserModel.DisplayName = objUser.DisplayName;
                    objUserModel.Email = objUser.Email;
                    objUserModel.FirstName = objUser.FirstName;
                    objUserModel.JobTitle = objUser.JobTitle;
                    objUserModel.LastName = objUser.LastName;
                    objUserModel.Password = objUser.Password;
                    objUserModel.ProfilePhoto = objUser.ProfilePhoto;
                    objUserModel.GeographyId = objUser.GeographyId;
                    objUserModel.Geography = db.Geographies.Where(geo => geo.GeographyId == objUserModel.GeographyId).Select(g => g.Title).FirstOrDefault();
                    objUserModel.UserId = objUser.UserId;
                    objUserModel.RoleCode = objUser.RoleCode;
                    objUserModel.RoleId = objUser.RoleId;
                    objUserModel.RoleTitle = objUser.RoleTitle;
                    objUserModel.Password = objUser.Password;
                    objUserModel.ConfirmPassword = objUser.Password;

                    if (Sessions.IsSystemAdmin)
                    {
                        objUserModel.IsSystemAdmin = true;
                    }
                    else if (Sessions.IsClientAdmin)
                    {
                        objUserModel.IsClientAdmin = true;
                    }
                    else if (Sessions.IsDirector)
                    {
                        objUserModel.IsDirector = true;
                    }
                    else if (Sessions.IsPlanner)
                    {
                        objUserModel.IsPlanner = true;
                    }
                    LoadEditModeComponents(objUserModel.ClientId, src);
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View(objUserModel);
        }

        /// <summary>
        /// POST: Edit Existing User
        /// </summary>
        /// <param name="form"></param>
        /// <param name="file">user photo</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(UserModel form, HttpPostedFileBase file)
        {
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
                    objUser.JobTitle = form.JobTitle;
                    if (form.IsDeleted != null)
                    {
                        if (Convert.ToString(form.IsDeleted).ToLower() == "yes")
                        {
                            objUser.IsDeleted = true;
                        }
                        else
                        {
                            objUser.IsDeleted = false;
                        }
                    }
                    if (file != null)
                    {
                        MemoryStream target = new MemoryStream();
                        file.InputStream.CopyTo(target);
                        byte[] data = target.ToArray();
                        objUser.ProfilePhoto = data;
                    }
                    objUser.ClientId = form.ClientId;
                    objUser.BusinessUnitId = form.BusinessUnitId;
                    objUser.GeographyId = form.GeographyId;
                    objUser.RoleId = form.RoleId;
                    if (form.RoleId != null)
                    {
                        Role objRole = objBDSServiceClient.GetRoleDetails(form.RoleId);
                        if (objRole != null)
                        {
                            objUser.RoleCode = objRole.Code;
                        }
                    }
                    int retVal = objBDSServiceClient.UpdateUser(objUser, Sessions.ApplicationId, Sessions.User.UserId);
                    if (retVal == 1)
                    {
                        TempData["SuccessMessage"] = Common.objCached.UserEdited;
                        if (form.UserId == Sessions.User.UserId)
                        {
                            objUser = objBDSServiceClient.GetTeamMemberDetails(form.UserId, Sessions.ApplicationId);
                            if (objUser != null)
                            {
                                Sessions.User = objUser;
                            }
                        }

                        //// Modified By Maninder Singh Wadhva to Address PL#203
                        System.Web.HttpContext.Current.Cache.Remove(form.UserId + "_photo");
                        return RedirectToAction("Index");
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
                if (form.UserId == Sessions.User.UserId)
                {
                    LoadEditModeComponents(form.ClientId, "myaccount");
                }
                else
                {
                    LoadEditModeComponents(form.ClientId, "myteam");
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = Common.objCached.ErrorOccured;
                }
            }
            return View(form);
        }

        #endregion

        #region User Notifications

        /// <summary>
        /// Load user notifications
        /// </summary>
        /// <returns></returns>
        public ActionResult Notifications()
        {
            if (Sessions.RolePermission != null)
            {
                Common.Permission permission = Common.GetPermission(ActionItem.Pref);
                switch (permission)
                {
                    case Common.Permission.FullAccess:
                        break;
                    case Common.Permission.NoAccess:
                        return RedirectToAction("Index", "NoAccess");
                    case Common.Permission.NotAnEntity:
                        break;
                    case Common.Permission.ViewOnly:
                        ViewBag.IsViewOnly = "true";
                        break;
                }
            }

            string typeSM = Enums.NotificationType.SM.ToString().ToLower();
            string typeAM = Enums.NotificationType.AM.ToString().ToLower();
            ViewBag.SMCount = db.Notifications.Where(n => n.IsDeleted == false && n.NotificationType.ToLower() == typeSM).Count();
            ViewBag.AMCount = db.Notifications.Where(n => n.IsDeleted == false && n.NotificationType.ToLower() == typeAM).Count();

            List<UserNotification> viewModelList = new List<UserNotification>();
            foreach (var item in db.Notifications.Where(n => n.IsDeleted == false).ToList())
            {
                UserNotification userNotification = new UserNotification();
                userNotification.NotificationId = item.NotificationId;

                if (db.User_Notification.Where(r => r.NotificationId == item.NotificationId && r.UserId == Sessions.User.UserId).Count() > 0)
                {
                    userNotification.IsSelected = true;
                }
                else
                {
                    userNotification.IsSelected = false;
                }
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
                // Remove all current Notifications set for User
                foreach (var item in db.User_Notification.Where(u => u.UserId == Sessions.User.UserId).ToList())
                {
                    db.Entry(item).State = EntityState.Modified;
                    db.User_Notification.Remove(item);
                    db.SaveChanges();
                }

                // Save User Notifications
                if (notifications != null)
                {
                    if (notifications != string.Empty)
                    {
                        if (notifications.Length > 0)
                        {
                            string[] arrNotify = notifications.Split(',');
                            foreach (var notification in arrNotify)
                            {
                                int notificationId = Convert.ToInt32(notification);
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
        /// <returns></returns>
        public ActionResult LoadUserImage(string id = null, int width = 60, int height = 60)
        {
            Guid userId = new Guid();
            byte[] imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
            try
            {
                if (id != null)
                {
                    userId = Guid.Parse(id);
                    BDSService.User objUser = new BDSService.User();
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
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, width, height, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                    return File(imageBytes, "image/jpg");
                }
            }
            catch (Exception e)
            {
                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ErrorSignal.FromCurrentContext().Raise(e);
                    imageBytes = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    image = Common.ImageResize(image, width, height, true, false);
                    imageBytes = Common.ImageToByteArray(image);
                    return File(imageBytes, "image/jpg");
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
        /// <param name="id">user</param>
        /// <returns></returns>
        public ActionResult ImageLoad(int? id)
        {
            if (!(Convert.ToInt32(id) > 0))
            {
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
                else
                {

                    byte[] b = Common.ReadFile(Server.MapPath("~") + "/content/images/user_image_not_found.png");
                    if (b != null)
                        return File(b, "image/jpg");
                }
            }
            else
            {
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
            }
            return View();
        }

        /// <summary>
        /// Get list of business units for selected client. 
        /// </summary>
        /// <param name="id">client</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetBusinessUnit(string id = null)
        {
            Guid clientId = new Guid();
            if (id != null)
            {
                Guid.TryParse(id, out clientId);
            }

            var businessUnitList = (from bu in db.BusinessUnits
                                    where bu.ClientId == clientId && bu.IsDeleted == false
                                    select new { bu.BusinessUnitId, bu.Title }).ToList();

            var businessUnitData = businessUnitList.Select(q => new SelectListItem()
            {
                Text = q.Title,
                Value = Convert.ToString(q.BusinessUnitId),
            }).OrderBy(t => t.Text);

            return Json(businessUnitData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get list of geographic locations for selected client. 
        /// </summary>
        /// <param name="id">client</param>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetGeography(string id = null)
        {
            Guid clientId = new Guid();
            if (id != null)
            {
                Guid.TryParse(id, out clientId);
            }

            var geographyList = (from g in db.Geographies
                                 where g.ClientId == clientId && g.IsDeleted == false
                                 select new { g.GeographyId, g.Title }).ToList();

            var geographyData = geographyList.Select(q => new SelectListItem()
            {
                Text = q.Title,
                Value = Convert.ToString(q.GeographyId),
            }).OrderBy(g => g.Text);

            return Json(geographyData, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
