﻿using Elmah;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Collections.Generic;

/*
 *  Author: Manoj Limbachiya
 *  Created Date: 10/22/2013
 *  Screen: 00.00.01 - Login
 *  Purpose: To authenticate user before accessing the application
 */
namespace RevenuePlanner.Controllers
{
    public class LoginController : CommonController
    {
        #region Variables
        MRPEntities db = new MRPEntities();
        #endregion

        #region Login

        /// <summary>
        /// Login view
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string returnUrl = "")
        {
            //Start Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
            //HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName.ToString()];
            //if (authCookie != null)
            //{
            //    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            //    if (authTicket != null & !authTicket.Expired)
            //    {
            //        string cookieValue = authTicket.Name;
            //        int userID = 0;
            //        int.TryParse(cookieValue,out userID);
            //        var obj = db.Users.Where(u => u.UserId == userID && u.IsDeleted == false).FirstOrDefault();
            //        if (obj != null)
            //        {
            //            Sessions.User = obj;
            //            return RedirectToAction("Index", "Home");
            //        }
            //    }
            //}
            //End  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented

            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            if (!string.IsNullOrEmpty(Convert.ToString(TempData["ErrorMessage"])))
            {
                ModelState.AddModelError("", Convert.ToString(TempData["ErrorMessage"]));
                TempData["ErrorMessage"] = null;
            }

            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        /// <summary>
        /// POST: Login view
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(LoginModel form, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    BDSService.User obj = new BDSService.User();
                    Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                    string singlehash = Common.ComputeSingleHash(form.Password.ToString().Trim());

                    obj = objBDSServiceClient.ValidateUser(applicationId, form.UserEmail.Trim(), singlehash);

                    //var obj = db.Users.Where(u => u.Email.Trim().ToLower() == form.UserEmail.Trim().ToLower() && u.Password.Trim().ToLower() == form.Password.Trim().ToLower() &&  u.IsDeleted == false).FirstOrDefault();
                    if (obj != null)
                    {
                        //Start  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
                        System.Web.Security.FormsAuthentication.SetAuthCookie(obj.UserId.ToString(), false);
                        //End  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
                        Sessions.User = obj;

                        //Start Maninder Singh Wadhva : 11/27/2013 - Setting role flag.
                        Sessions.IsSystemAdmin = Sessions.IsClientAdmin = Sessions.IsDirector = Sessions.IsPlanner = false;

                        Enums.Role role = Common.GetKey<Enums.Role>(Enums.RoleCodeValues, Sessions.User.RoleCode);
                        switch (role)
                        {
                            case Enums.Role.SystemAdmin:
                                Sessions.IsSystemAdmin = true;
                                break;
                            case Enums.Role.ClientAdmin:
                                Sessions.IsClientAdmin = true;
                                break;
                            case Enums.Role.Director:
                                Sessions.IsDirector = true;
                                break;
                            case Enums.Role.Planner:
                                Sessions.IsPlanner = true;
                                break;
                            default:
                                break;
                        }
                        //End Maninder Singh Wadhva : 11/27/2013 - Setting role flag.
                        //Start Manoj Limbachiya : 11/23/2013 - Menu filling and Role Permission
                        if (Sessions.AppMenus == null)
                        {
                            Sessions.AppMenus = objBDSServiceClient.GetMenu(Sessions.ApplicationId, Sessions.User.RoleId);
                        }
                        if (Sessions.RolePermission == null)
                        {
                            Sessions.RolePermission = objBDSServiceClient.GetPermission(Sessions.ApplicationId, Sessions.User.RoleId);
                        }
                        //End Manoj Limbachiya : 11/23/2013 - Menu filling  and Role Permission

                        //Redirect users logging in for the first time to the change password module
                        if (obj.LastLoginDate == null)
                        {
                            Sessions.RedirectToChangePassword = true;
                            return RedirectToAction("ChangePassword", "User");
                        }

                        if (obj.SecurityQuestionId == null)
                        {
                            Sessions.RedirectToSetSecurityQuestion = true;
                            return RedirectToAction("SetSecurityQuestion", "Login");
                        }
                        else
                        {
                            Sessions.RedirectToSetSecurityQuestion = false;
                        }

                        //Update last login date for user
                        objBDSServiceClient.UpdateLastLoginDate(Sessions.User.UserId, Sessions.ApplicationId);

                        if ((!string.IsNullOrWhiteSpace(returnUrl)) && IsLocalUrl(returnUrl))
                        {
                            return RedirectLocal(returnUrl);
                        }
                        else
                        {
                            MVCUrl defaultURL = Common.DefaultRedirectURL(Enums.ActiveMenu.None);
                            if (defaultURL != null)
                            {
                                if (!string.IsNullOrEmpty(defaultURL.queryString))
                                {
                                    return RedirectToAction(defaultURL.actionName, defaultURL.controllerName, new { activeMenu = defaultURL.queryString });
                                }
                                else
                                {
                                    return RedirectToAction(defaultURL.actionName, defaultURL.controllerName);
                                }
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", Common.objCached.InvalidLogin);
                    }
                }
                else
                {
                    if (form.Password != null)
                    {
                        ModelState.AddModelError("", Common.objCached.InvalidPassword);
                    }
                    else
                    {
                        ModelState.AddModelError("", Common.objCached.InvalidLogin);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(form);
        }

        private ActionResult RedirectLocal(string returnUrl)
        {
            var questionMarkIndex = returnUrl.IndexOf('?');
            string queryString = null;
            string url = returnUrl;
            if (questionMarkIndex != -1) // There is a QueryString
            {
                url = returnUrl.Substring(0, questionMarkIndex);
                queryString = returnUrl.Substring(questionMarkIndex + 1);
            }

            // Arranges
            var request = new HttpRequest(null, url, queryString);
            var response = new HttpResponse(new System.IO.StringWriter());
            var httpContext = new HttpContext(request, response);
            var routeData = System.Web.Routing.RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            // Extract the data    
            var values = routeData.Values;
            string controllerName = Convert.ToString(values["controller"]).ToLower();
            string actionName = Convert.ToString(values["action"]);
            //var areaName = values["area"];

            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            List<BDSService.Menu> AppMenus = new List<BDSService.Menu>();
            try
            {
                AppMenus = objBDSServiceClient.GetAllMenu(Sessions.ApplicationId, Sessions.User.RoleId);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                }
            }
            BDSService.Menu currentMenuOfUrl = (BDSService.Menu)AppMenus.Single(am => am.ControllerName.ToLower().Equals(controllerName));
            Enums.ActiveMenu activeMenu = Common.GetKey<Enums.ActiveMenu>(Enums.ActiveMenuValues, controllerName);
            switch (activeMenu)
            {
                case Enums.ActiveMenu.Home:
                    if (actionName.Equals("Index") && !string.IsNullOrWhiteSpace(queryString))
                    {
                        //// For share tactic popup.
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        //// For actions other than Index.
                        return RedirectToAction(currentMenuOfUrl.ActionName, currentMenuOfUrl.ControllerName);
                    }
                default:
                    return RedirectToAction(currentMenuOfUrl.ActionName, currentMenuOfUrl.ControllerName);
            }
        }

        //Note: This has been copied from the System.Web.WebPages RequestExtensions class
        private bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
            {
                return String.Equals(this.Request.Url.Host, absoluteUri.Host,
                    StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                bool isLocal = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                    && Uri.IsWellFormedUriString(url, UriKind.Relative);
                return isLocal;
            }
        }

        /// <summary>
        /// Set security question view
        /// </summary>
        /// <returns></returns>
        public ActionResult SetSecurityQuestion()
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();

            SecurityQuestionListModel objSecurityQuestionListModel = new SecurityQuestionListModel();
            objSecurityQuestionListModel.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);

            return View(objSecurityQuestionListModel);
        }

        /// <summary>
        /// Post : Set security question view
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetSecurityQuestion(SecurityQuestionListModel form)
        {
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                BDSService.User objUser = new BDSService.User();
                objUser.UserId = Sessions.User.UserId;
                objUser.SecurityQuestionId = form.SecurityQuestionId;
                objUser.Answer = form.Answer;
                objBDSServiceClient.UpdateUserSecurityQuestion(objUser);

                Sessions.User.Answer = form.Answer;
                Sessions.User.SecurityQuestionId = form.SecurityQuestionId;

                Sessions.RedirectToSetSecurityQuestion = false;

                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

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

        #region Logout

        /// <summary>
        /// Logoff the user from application
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Sessions.Clear();
            //Start Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
            System.Web.Security.FormsAuthentication.SignOut();
            //End  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
            return RedirectToAction("Index", "Login");
        }

        public JsonResult ContactSupport(string emailId, string CompanyName, string Issue)
        {
            string notificationContactSupport = Enums.Custom_Notification.ContactSupport.ToString();
            string emailSubject = Sessions.ApplicationName + "/" + CompanyName;
            Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationContactSupport));
            string emailBody = notification.EmailContent.Replace("[EmailToBeReplaced]", emailId).Replace("[IssueToBeReplaced]", Issue);
            var success = Common.sendMail(Common.SupportMail, Common.FromSupportMail, emailBody, emailSubject, string.Empty, Common.FromAlias, string.Empty, true); //email will be sent to Support email Id defined in web.config
            if (success == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSupportPartial()
        {
            return PartialView("_SupportPartial");
        }

        #endregion

        #region ForgotPassword

        /// <summary>
        /// Forgot Password View
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            if (!string.IsNullOrEmpty(Convert.ToString(TempData["ErrorMessage"])))
            {
                ModelState.AddModelError("", Convert.ToString(TempData["ErrorMessage"]));
                TempData["ErrorMessage"] = null;
            }

            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            ForgotPasswordModel objForgotPasswordModel = new ForgotPasswordModel()
            {
                IsSuccess = false
            };

            return View(objForgotPasswordModel);
        }

        /// <summary>
        /// Post : Forgor Password View
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel form)
        {
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

                var objUser = objBDSServiceClient.GetUserDetails(form.UserEmail);

                if (objUser == null)
                {
                    ModelState.AddModelError("", Common.objCached.EmailNotExistInDatabse);
                }
                else
                {
                    if (objUser.SecurityQuestionId != null)
                    {
                        BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
                        objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
                        objPasswordResetRequest.UserId = objUser.UserId;
                        objPasswordResetRequest.AttemptCount = 0;
                        objPasswordResetRequest.CreatedDate=DateTime.Now;

                        string PasswordResetRequestId = objBDSServiceClient.CreatePasswordResetRequest(objPasswordResetRequest);

                        if (PasswordResetRequestId == string.Empty)
                        {
                            ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                        }
                        else
                        {
                            // Send email
                            string notificationShare = "";
                            string emailBody = "";
                            Notification notification = new Notification();
                            notificationShare = Enums.Custom_Notification.ResetPasswordLink.ToString();
                            notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationShare));

                            string PasswordResetLink = Url.Action("SecurityQuestion", "Login", new { id = PasswordResetRequestId }, Request.Url.Scheme);
                            emailBody = notification.EmailContent.Replace("[PasswordResetLinkToBeReplaced]", "<a href='" + PasswordResetLink + "'>" + PasswordResetLink + "</a>")
                                                                 .Replace("[ExpireDateToBeReplaced]", objPasswordResetRequest.CreatedDate.AddHours(int.Parse(ConfigurationManager.AppSettings["ForgotPasswordLinkExpiration"])).ToString());

                            //string tempUrl = "http://localhost:57856/Login/SecurityQuestion/" + PasswordResetRequestId;

                            Common.sendMail(objUser.Email, Common.FromMail, emailBody, notification.Subject, string.Empty);


                            form.IsSuccess = true;
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = Common.objCached.SecurityQuestionNotFound;
                        return RedirectToAction("Index", "Login", new { returnUrl = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            return View(form);
        }

        /// <summary>
        /// Security Question View
        /// </summary>
        /// <param name="PasswordResetRequestId"></param>
        /// <returns></returns>
        public ActionResult SecurityQuestion(string id)
        {
            try
            {
                Guid PasswordResetRequestId = Guid.Parse(id);

                SecurityQuestionModel objSecurityQuestionModel = new SecurityQuestionModel();

                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                var objPasswordResetRequest = objBDSServiceClient.GetPasswordResetRequest(PasswordResetRequestId);

                if (objPasswordResetRequest == null)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                    return RedirectToAction("Index", "Login", new { returnUrl = "" });
                }
                else
                {
                    int interval = int.Parse(ConfigurationManager.AppSettings["ForgotPasswordLinkExpiration"]); // Link expiration duration in hour.

                    if (objPasswordResetRequest.IsUsed)
                    {
                        TempData["ErrorMessage"] = Common.objCached.PasswordResetLinkAlreadyUsed;
                        return RedirectToAction("Index", "Login", new { returnUrl = "" });
                    }
                    else if ((DateTime.Now - objPasswordResetRequest.CreatedDate).Hours >= interval)
                    {
                        TempData["ErrorMessage"] = Common.objCached.PasswordResetLinkExpired;
                        return RedirectToAction("Index", "Login", new { returnUrl = "" });
                    }
                    else
                    {
                        Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                        var objUser = objBDSServiceClient.GetTeamMemberDetails(objPasswordResetRequest.UserId, applicationId);

                        objSecurityQuestionModel.PasswordResetRequestId = objPasswordResetRequest.PasswordResetRequestId;
                        objSecurityQuestionModel.UserId = objPasswordResetRequest.UserId;
                        objSecurityQuestionModel.AttemptCount = objPasswordResetRequest.AttemptCount;
                        objSecurityQuestionModel.SecurityQuestion = objUser.SecurityQuestion;

                    }
                }

                return View(objSecurityQuestionModel);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                }

                return RedirectToAction("Index", "Login", new { returnUrl = "" });

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }
        }

        /// <summary>
        /// Post : Security Question View
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SecurityQuestion(SecurityQuestionModel form)
        {
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();

                objPasswordResetRequest.PasswordResetRequestId = form.PasswordResetRequestId;

                int PossibleAttemptCount = int.Parse(ConfigurationManager.AppSettings["PossibleAttemptCount"]);
                if (form.AttemptCount < PossibleAttemptCount)
                {
                    var objUser = objBDSServiceClient.GetTeamMemberDetails(form.UserId, Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]));
                    if (form.Answer != objUser.Answer)
                    {
                        form.AttemptCount = form.AttemptCount + 1;
                        objPasswordResetRequest.AttemptCount = form.AttemptCount;
                        objPasswordResetRequest.IsUsed = true;
                        objBDSServiceClient.UpdatePasswordResetRequest(objPasswordResetRequest);
                        ModelState.AddModelError("", Common.objCached.AnswerNotMatched);
                    }
                    else
                    {
                        objPasswordResetRequest.AttemptCount = form.AttemptCount + 1;
                        objPasswordResetRequest.IsUsed = true;
                        objBDSServiceClient.UpdatePasswordResetRequest(objPasswordResetRequest);
                        TempData["UserId"] = form.UserId;
                        return RedirectToAction("ResetPassword", "Login");
                    }
                }
                else
                {

                    objPasswordResetRequest.IsUsed = true;
                    objBDSServiceClient.UpdatePasswordResetRequest(objPasswordResetRequest);
                    TempData["ErrorMessage"] = Common.objCached.PossibleAttemptLimitExceed;
                    return RedirectToAction("Index", "Login", new { returnUrl = "" });

                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            return View(form);
        }

        /// <summary>
        /// Reset Password View
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult ResetPassword()
        {
            try
            {
                //TempData["UserId"] = "F37A855C-9BF4-4A1F-AB7F-B21AF43EB2BF";

                if (string.IsNullOrEmpty(Convert.ToString(TempData["UserId"])))
                {
                    return RedirectToAction("Index", "Login", new { returnUrl = "" });
                }
                else
                {
                    Guid UserId = Guid.Parse(TempData["UserId"].ToString());

                    TempData["UserId"] = null;

                    ResetPasswordModel objResetPasswordModel = new ResetPasswordModel();

                    objResetPasswordModel.UserId = UserId;

                    return View(objResetPasswordModel);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                }

                return RedirectToAction("Index", "Login", new { returnUrl = "" });

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }
        }

        /// <summary>
        /// post : Reset Password View
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel form)
        {
            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                var objUser = objBDSServiceClient.GetTeamMemberDetails(form.UserId, applicationId);

                /* ------------------------------- single hash current password ------------------------------*/
                string SingleHash_CurrentPassword = Common.ComputeSingleHash(form.NewPassword.ToString().Trim());
                /*--------------------------------------------------------------------------------------------*/

                if (objBDSServiceClient.CheckCurrentPassword(form.UserId, SingleHash_CurrentPassword))
                {
                    ModelState.AddModelError("", "New and current password cannot be same.");
                }
                else
                {
                    /* ------------------ Single hash password ----------------------*/
                    string SingleHash_NewPassword = Common.ComputeSingleHash(form.NewPassword.ToString().Trim());
                    /* ---------------------------------------------------------------*/

                    objBDSServiceClient.ResetPassword(form.UserId, SingleHash_NewPassword);

                    form.IsSuccess = true;
                }


                return View(form);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (ex is System.ServiceModel.EndpointNotFoundException)
                {
                    TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
                }

                return RedirectToAction("Index", "Login", new { returnUrl = "" });

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }
        }

        /// <summary>
        /// Function to verify users current password.
        /// </summary>
        /// <param name="currentPassword">current password</param>
        /// <returns>Returns true if the operation is successful, 0 otherwise.</returns>
        public ActionResult CheckCurrentPassword(string currentPassword, string userId)
        {
            bool isValid = false;
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
            /* ------------------------------- single hash current password ------------------------------*/
            string SingleHash_CurrentPassword = Common.ComputeSingleHash(currentPassword.ToString().Trim());
            /*--------------------------------------------------------------------------------------------*/
            try
            {
                isValid = objBDSServiceClient.CheckCurrentPassword(new Guid(userId), SingleHash_CurrentPassword);
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

        #endregion
    }
}
