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
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

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
        public bool destroySessions()
        {
            Session.Abandon();
            return true;
        }

        public ActionResult DBServiceUnavailable()
        {
            //// Flag to indicate unavailability of web service.
            //// Added By: Maninder Singh Wadhva on 11/24/2014.
            //// Ticket: 942 Exception handeling in Gameplan.
            Sessions.Clear();
            System.Web.Security.FormsAuthentication.SignOut();
            TempData["ErrorMessage"] = Common.objCached.DatabaseServiceUnavailableMessage;
            return RedirectToAction("Index");
        }

        public ActionResult ServiceUnavailable()
        {
            //// Flag to indicate unavailability of web service.
            //// Added By: Maninder Singh Wadhva on 11/24/2014.
            //// Ticket: 942 Exception handeling in Gameplan.
            Sessions.Clear();
            System.Web.Security.FormsAuthentication.SignOut();
            TempData["ErrorMessage"] = Common.objCached.ServiceUnavailableMessage;
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Login view
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string returnUrl = "", bool sessionTimeout=false)
        {
            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            bool isErrorMessageAddedToModel = false;
            if (!string.IsNullOrEmpty(Convert.ToString(TempData["ErrorMessage"])))
            {
                ModelState.AddModelError("", Convert.ToString(TempData["ErrorMessage"]));
                TempData["ErrorMessage"] = null;
                isErrorMessageAddedToModel = true;
            }
            TempData["SessionTimeOut"] = sessionTimeout.ToString();
            /* Bug 25:Unavailability of BDSService leads to no error shown to user */

            ViewBag.ReturnUrl = returnUrl;

            try
            {
                Common.SetSessionVariableApplicationName();

                //// Start - Added by :- Sohel Pathan on 22/05/2014 for PL ticket #469 to display release version from database
                string applicationReleaseVersion = Common.GetCurrentApplicationReleaseVersion();
                ViewBag.ApplicationReleaseVersion = applicationReleaseVersion;
                //// End - Added by :- Sohel Pathan on 22/05/2014 for PL ticket #469 to display release version from database
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    Sessions.Clear();
                    System.Web.Security.FormsAuthentication.SignOut();
                    if (!isErrorMessageAddedToModel)
                    {
                        ModelState.AddModelError("", Common.objCached.ServiceUnavailableMessage);
                    }
                }
                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            //// Start - Added by :- Pratik Chauhan on 22/09/2014 for PL ticket #468 to display maintenance page
            if (Common.IsOffline)
            {
                return View("MaintenanceSite");
            }
            else
            {
                return View();
            }
            //// End - Added by :- Pratik Chauhan on 22/09/2014 for PL ticket #468 to display maintenance page
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
                    if (obj == null)
                        ModelState.AddModelError("", Common.objCached.InvalidLogin);
                    
                    //Start  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
                    System.Web.Security.FormsAuthentication.SetAuthCookie(obj.UserId.ToString(), false);
                    //End  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
                    Sessions.User = obj;
                    
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

                    //// Set user activity permission session.
                    SetUserActivityPermission();

                    // Start - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic
                    if (Sessions.AppMenus != null)
                    {
                        var isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ModelCreateEdit);
                        var item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Model.ToString().ToUpper());
                        if (item != null && !isAuthorized)
                        {
                            Sessions.AppMenus.Remove(item);
                        }

                        isAuthorized = (AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostImprovementTacticCreateEdit) ||
                            AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.BoostBestInClassNumberEdit));
                        item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Boost.ToString().ToUpper());
                        if (item != null && !isAuthorized)
                        {
                            Sessions.AppMenus.Remove(item);
                        }

                        isAuthorized = AuthorizeUserAttribute.IsAuthorized(Enums.ApplicationActivity.ReportView);
                        item = Sessions.AppMenus.Find(a => a.Code.ToString().ToUpper() == Enums.ActiveMenu.Report.ToString().ToUpper());
                        if (item != null && !isAuthorized)
                        {
                            Sessions.AppMenus.Remove(item);
                        }
                    }
                    // End - Added by Sohel Pathan on 19/06/2014 for PL ticket #519 to implement user permission Logic

                    // Added by bhavesh Dobariya @Date: 26/11/2014
                    Sessions.IsDisplayDataInconsistencyMsg = false;
                    Guid ClientId = obj.ClientId;
                    List<int> deletedStageId = db.Stages.Where(s => s.ClientId == ClientId && s.IsDeleted == true).Select(s => s.StageId).ToList();
                    if (deletedStageId.Count > 0)
                    {
                        var tacticlist = db.Plan_Campaign_Program_Tactic.Where(t => deletedStageId.Contains(t.StageId) && t.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == ClientId && t.IsDeleted == false).ToList();
                        if (tacticlist.Count > 0)
                        {
                            Common.SetCookie("DataClientId" + ClientId.ToString().ToLower(), ClientId.ToString().ToLower(), true);
                            Sessions.IsDisplayDataInconsistencyMsg = true;
                        }
                        else
                        {
                            Common.RemoveCookie("DataClientId" + ClientId.ToString().ToLower());
                        }
                    }
                    //End Bhavesh

                    //Redirect users logging in for the first time to the change password module
                    if (obj.LastLoginDate == null)
                    {
                        Sessions.RedirectToChangePassword = true;
                        return RedirectToAction("ChangePassword", "User");
                    }

                    //Commented By Komal Rawal for #1457
                    //if (obj.SecurityQuestionId == null)
                    //{
                    //    Sessions.RedirectToSetSecurityQuestion = true;
                    //    return RedirectToAction("SetSecurityQuestion", "Login");
                    //}
                    //else
                    //{
                    //    Sessions.RedirectToSetSecurityQuestion = false;
                    //}

                    //Update last login date for user
                    objBDSServiceClient.UpdateLastLoginDate(Sessions.User.UserId, Sessions.ApplicationId);

                    if ((!string.IsNullOrWhiteSpace(returnUrl)) && IsLocalUrl(returnUrl))
                    {
                        return RedirectLocal(returnUrl);
                    }
                    else
                    {
                        MVCUrl defaultURL = Common.DefaultRedirectURL(Enums.ActiveMenu.None);
                        if (defaultURL == null)
                            return RedirectToAction("Index", "Home");
                        
                        if (!string.IsNullOrEmpty(defaultURL.queryString))
                        {
                            return RedirectToAction(defaultURL.actionName, defaultURL.controllerName, new { activeMenu = defaultURL.queryString });
                        }
                        else
                        {
                            return RedirectToAction(defaultURL.actionName, defaultURL.controllerName);
                        }
                    }
                }
                else
                {
                    if (form.Password != null && form.Password.Length < 8)
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }

                if (e is System.Data.EntityException || e is System.Data.SqlClient.SqlException)
                {
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("DBServiceUnavailable", "Login");                    
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            try
            {
                //// Start - Added by :- Sohel Pathan on 22/05/2014 for PL ticket #469 to display release version from database
                string applicationReleaseVersion = Common.GetCurrentApplicationReleaseVersion();
                ViewBag.ApplicationReleaseVersion = applicationReleaseVersion;
                //// End - Added by :- Sohel Pathan on 22/05/2014 for PL ticket #469 to display release version from database
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(form);
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 06/18/2014
        /// Function to set user activity permissions in session.
        /// </summary>
        private void SetUserActivityPermission()
        {
            BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();

            // Get user application activity permission using BDSService
            var lstUserActivityPermissions = objBDSServiceClient.GetUserActivityPermission(Sessions.User.UserId, Sessions.ApplicationId);
            Sessions.UserActivityPermission = new Enums.ApplicationActivity();
            foreach (string permission in lstUserActivityPermissions)
            {
                Sessions.UserActivityPermission |= Common.GetKey<Enums.ApplicationActivity>(permission);
            }
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

            // Modified by Viral Kadiya on 10/15/14 for #795 Cannot Log In After Session Expiration.
            BDSService.Menu currentMenuOfUrl = (BDSService.Menu)AppMenus.Where(am => am.ControllerName.ToLower().Equals(controllerName)).FirstOrDefault();
            if (currentMenuOfUrl == null)
                return RedirectToAction("Index", "Home");

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
                case Enums.ActiveMenu.Organization:
                    //// For actions with organization controller 06/30/2014 Added By Maninder Singh Wadhva to handle return URL view.
                    return RedirectToAction("ViewEditPermission", currentMenuOfUrl.ControllerName, new { id = Sessions.User.UserId, Mode = "MyPermission" });
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
            SecurityQuestionListModel objSecurityQuestionListModel = new SecurityQuestionListModel();

            try
            {
                BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                var lstSecurityQuestion = objBDSServiceClient.GetSecurityQuestion();


                objSecurityQuestionListModel.SecurityQuestionList = GetQuestionList(lstSecurityQuestion);
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
                    return RedirectToAction("ServiceUnavailable", "Login");
                }
            }

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

                string encryptedAnswer = Common.Encrypt(form.Answer);

                objUser.Answer = encryptedAnswer;
                objBDSServiceClient.UpdateUserSecurityQuestion(objUser);

                Sessions.User.Answer = encryptedAnswer;
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
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
            //LoginSession l = new LoginSession();
            //l.RemoveSession(Session.SessionID, Sessions.User.UserId.ToString());
            Sessions.Clear();
            //Start Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
            System.Web.Security.FormsAuthentication.SignOut();
            //End  Manoj Limbachiya : 10/23/2013 - Auto login if coockie is presented
            return RedirectToAction("Index", "Login");
        }

        /// <summary>
        /// Modified BY : Kalpesh Sharma
        /// #453: Support request Issue field needs to be bigger
        /// Send image in Email template 
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="CompanyName"></param>
        /// <param name="Issue"></param>
        /// <returns></returns>
        public JsonResult ContactSupport(string emailId, string CompanyName, string Issue)
        {
            try
            {
            string notificationContactSupport = Enums.Custom_Notification.ContactSupport.ToString();
            string emailSubject = Sessions.ApplicationName + "/" + CompanyName;
            Notification notification = (Notification)db.Notifications.Single(n => n.NotificationInternalUseOnly.Equals(notificationContactSupport));
            //this is to decode the html content which we have encoded into text on client side.added by uday on 28-5-2014 for editor in contact support.
            Issue = HttpUtility.UrlDecode(Issue, System.Text.Encoding.Default);

            //Added for send the images in the Email section
            //Added By : Kalpesh Sharma
            // #453: Support request Issue field needs to be bigger

            MatchCollection mc = Regex.Matches(Issue, @"\<img(.*?)\"">");

            AlternateView tempAlternateView = AlternateView.CreateAlternateViewFromString((""), null, "text/html");

            for (int i = 0; i < mc.Count; i++)
            {
                var extractBase64String = mc[i].Groups[0].Value;

                if (!string.IsNullOrEmpty(extractBase64String))
                {
                    string RandomNumber = Common.GenerateRandomNumber();

                    Issue = Issue.Replace(extractBase64String, "<img src='cid:" + RandomNumber + "'>");

                    var metadataStart = extractBase64String.IndexOf("base64,");

                    // Remove the string if match is found.
                    extractBase64String = extractBase64String.Remove(0, metadataStart + 7);

                    int lastPosition = Common.GetNthIndex(extractBase64String, '"', 1);

                    if (lastPosition == extractBase64String.Length)
                    {
                        extractBase64String = extractBase64String.Remove(lastPosition, 1);
                    }
                    else
                    {
                        extractBase64String = extractBase64String.Remove(lastPosition, (extractBase64String.Length - lastPosition));
                    }

                    byte[] imageBytes = Convert.FromBase64String(extractBase64String);

                    LinkedResource linkedResource = new LinkedResource(new MemoryStream(imageBytes));
                    linkedResource.ContentId = RandomNumber;
                    linkedResource.TransferEncoding = TransferEncoding.Base64;
                    tempAlternateView.LinkedResources.Add(linkedResource);
                }
            }

            //End : Added for send the images in the Email section

            string emailBody = notification.EmailContent.Replace("[EmailToBeReplaced]", emailId).Replace("[IssueToBeReplaced]", Issue);
            AlternateView htmltextview = AlternateView.CreateAlternateViewFromString(("<html><body>" + emailBody + "</body></html>"), null, "text/html");

            foreach (LinkedResource item in tempAlternateView.LinkedResources)
            {
                htmltextview.LinkedResources.Add(item);
            }

            var success = Common.sendMail(Common.SupportMail, Common.FromSupportMail, emailBody, emailSubject, string.Empty, Common.FromAlias, string.Empty, true, htmltextview); //email will be sent to Support email Id defined in web.config
            if (success == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */

                //To handle unavailability of BDSService
                if (e is System.Data.EntityException || e is System.Data.SqlClient.SqlException)
                {
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return Json(new { DBServiceUnavailable = "#" }, JsonRequestBehavior.AllowGet);
                }

                /* Bug 25:Unavailability of BDSService leads to no error shown to user */
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSupportPartialOnLogin()     //// Method Signature Modified by Sohel Pathan on 23/05/2014 for internal review points.
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
                        BDSService.PasswordResetRequest objPasswordResetRequest = new BDSService.PasswordResetRequest();
                        objPasswordResetRequest.PasswordResetRequestId = Guid.NewGuid();
                        objPasswordResetRequest.UserId = objUser.UserId;
                        objPasswordResetRequest.AttemptCount = 0;
                        objPasswordResetRequest.CreatedDate = DateTime.Now;

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

                            //Changes made by Komal rawal for #1328
                            TempData["UserId"] = objUser.UserId;
                            string PasswordResetLink = Url.Action("ResetPassword", "Login", new { id = PasswordResetRequestId }, Request.Url.Scheme);
                            emailBody = notification.EmailContent.Replace("[PasswordResetLinkToBeReplaced]", "<a href='" + PasswordResetLink + "'>" + PasswordResetLink + "</a>")
                                                                 .Replace("[ExpireDateToBeReplaced]", objPasswordResetRequest.CreatedDate.AddHours(int.Parse(ConfigurationManager.AppSettings["ForgotPasswordLinkExpiration"])).ToString());

                            //string tempUrl = "http://localhost:57856/Login/SecurityQuestion/" + PasswordResetRequestId;

                            Common.sendMail(objUser.Email, Common.FromMail, emailBody, notification.Subject, string.Empty);


                            form.IsSuccess = true;
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
                }

                if (ex is System.Data.EntityException || ex is System.Data.SqlClient.SqlException)
                {
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("DBServiceUnavailable", "Login");
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
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
                    if (Common.Encrypt(form.Answer) != objUser.Answer)
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
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
        public ActionResult ResetPassword(string id)
        {
            try
            {
                //Changes made by Komal rawal for #1328
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

                if (string.IsNullOrEmpty(Convert.ToString(TempData["UserId"])))
                {
                    return RedirectToAction("Index", "Login", new { returnUrl = "" });
                }
                else
                {
                            objPasswordResetRequest.PasswordResetRequestId = PasswordResetRequestId;
                            objPasswordResetRequest.IsUsed = true;
                            objBDSServiceClient.UpdatePasswordResetRequest(objPasswordResetRequest);
                    Guid UserId = Guid.Parse(TempData["UserId"].ToString());

                    TempData["UserId"] = null;

                    ResetPasswordModel objResetPasswordModel = new ResetPasswordModel();

                    objResetPasswordModel.UserId = UserId;

                    return View(objResetPasswordModel);
                        }
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
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
                    //// Flag to indicate unavailability of web service.
                    //// Added By: Maninder Singh Wadhva on 11/24/2014.
                    //// Ticket: 942 Exception handeling in Gameplan.
                    return RedirectToAction("ServiceUnavailable", "Login");
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

                //// Flag to indicate unavailability of web service.
                //// Added By: Maninder Singh Wadhva on 11/24/2014.
                //// Ticket: 942 Exception handeling in Gameplan.
                if (e is System.ServiceModel.EndpointNotFoundException)
                {
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

        #endregion

        #region "Site Maintenance"

        /// <summary>
        /// Set site maintenance view.
        /// </summary>
        /// <returns></returns>
        public ActionResult MaintenanceSite()
        {
            if (Common.IsOffline)
            {
                return View("MaintenanceSite");
            }
            else
            {
                return View("Index");
            }
        }

        #endregion
    }
}
