using Elmah;
using RestSharp;
using RevenuePlanner.Helpers;
using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
/*
 *  Author: Manoj Limbachiya
 *  Created Date: 10/22/2013
 *  Screen: Common controller
 *  Purpose: Handles authentication and authorization for all the controller which inherits
  */
namespace RevenuePlanner.Controllers
{
    public class CommonController : Controller
    {
        /// <summary>
        /// Initialize controller - load common messages
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // Set/get common messages cache
            if (System.Web.HttpContext.Current.Cache["CommonMsg"] == null)
            {
                Common.objCached.loadMsg(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache["CommonMsg"] = Common.objCached;
                CacheDependency dependency = new CacheDependency(Common.xmlMsgFilePath);
                System.Web.HttpContext.Current.Cache.Insert("CommonMsg", Common.objCached, dependency);
            }
            else
            {
                Common.objCached = (Message)System.Web.HttpContext.Current.Cache["CommonMsg"];
            }

        }

        /// <summary>
        /// Check permissions for each action
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // to add client id into elmah error log
            if (Sessions.User != null)
            {
                if (Sessions.User.CID != 0)
                {
                    Common.SetCookie("ClientID", Convert.ToString(Sessions.User.CID));
                }
            }

            Session["LastViewURL"] = Request.RawUrl;//Get the current requested URL
            string controller = filterContext.Controller.ToString();//Get the name of controller
            string[] arr = controller.Split('.');//Splet controller name
            string entity = arr[arr.Length - 1];
            entity = entity.Substring(0, entity.IndexOf("Controller"));//Get the name of entity from controller name
            Common.SetCookie("IsSessionExist", "1");

            #region Check for Session
            if (filterContext.HttpContext.Request.IsAjaxRequest()) //It will check for ajax request and session is null
            {
                if (Sessions.User == null)
                {
                    if (entity.ToString().ToLower().Trim() != "login")
                    {
                        Common.SetCookie("IsSessionExist", "0");
                        filterContext.HttpContext.Response.StatusCode = 403;
                        Common.SetCookie("IsSessionExist", "0");
                        Common.SetCookie("ReturnURL", Request.Url.ToString());
                    }
                    else
                    {
                        Common.SetCookie("ReturnURL","");
                    }
                }
            }
            else //It will check for request and session is null
            {
                if (entity.ToLower().Trim() != "login")
                {
                    if (Sessions.User == null)
                    {
                        if (string.Compare(entity.Trim(), "login", true) != 0)//Check of request is not for login page then redirect user to login page
                        {
                            Common.SetCookie("IsSessionExist", "0");
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", Controller = "Login", ReturnUrl = Request.Url.ToString() }));
                        }
                    }
                }
            }
            #endregion

            //Redirect users logging in for the first time to the change password module 
            // Modified by Dharmraj for issue with support popup when first time login on change password screen
            if (!(Request.RawUrl.ToLower().Contains("loadsupportpartial")) && !(Request.RawUrl.ToLower().Contains("userphoto")) && !(Request.RawUrl.ToLower().Contains("changepassword")) && (string.Compare(entity.Trim(), "login", true) != 0)) //Check of request is not for login page then redirect user to login page
            {
                if (Sessions.User != null)
                {
                    if (Sessions.User.LastLoginDate == null && Sessions.RedirectToChangePassword)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ChangePassword", Controller = "User" }));
                    }
                    //Commented by Rahul Shah to improve code coverage 
                    //if (Sessions.RedirectToSetSecurityQuestion)
                    //{
                    //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "SetSecurityQuestion", Controller = "Login" }));
                    //}
                }
            }
            ////Start Manoj PL #490 Date:27May2014
            //System.Collections.Generic.List<LoginSession> a = (System.Collections.Generic.List<LoginSession>)filterContext.HttpContext.Application["CurrentSession"];
            //if (a != null)
            //{
            //    if (string.Compare(entity.Trim(), "login", true) != 0)
            //    {
            //        if (a.Find(l => l.SessionId == Session.SessionID && l.UserId != Sessions.User.UserId.ToString()) != null)
            //        {
            //            TempData["ErrorMessage"] = "Another user already logged-in with the same session";
            //            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", Controller = "Login" }));
            //        }
            //    }
            //}
            ////End Manoj PL #490 Date:27May2014
            base.OnActionExecuting(filterContext);//Call the method of base class
        }

        /// <summary>
        /// Open Contact support pop-up
        /// </summary>
        /// <CreatedBy>Sohel Pathan</CreatedBy>
        /// <CreatedDate>22/05/2014</CreatedDate>
        /// <returns></returns>
        public PartialViewResult LoadSupportPartial()
        {
            ViewBag.login = "login";//uday for #453 7-7-14
            return PartialView("_SupportPartial");
        }
        /// <summary>
        /// Open NoModel pop-up if there is not any model in application 
        /// </summary>
        /// <CreatedBy>Mitesh Vaishnav</CreatedBy>
        /// <CreatedDate>16/07/2014</CreatedDate>
        /// <returns></returns>
        public PartialViewResult LoadNoModelPartial()
        {
            return PartialView("~/Views/Plan/_NoModel.cshtml");
        }
        /// <summary>
        /// open popup for session timeout warning
        /// </summary>
        /// <CreatedBy>Mitesh Vaishnav</CreatedBy>
        /// <CreatedDate>01/05/2015</CreatedDate>
        /// <returns>Partial view</returns>
        public PartialViewResult LoadSessionWarning()
        {
            return PartialView("~/Views/Shared/_SessionWarning.cshtml");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                System.Exception e = filterContext.Exception;
                filterContext.ExceptionHandled = true;
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                if (filterContext.HttpContext.Request.IsAjaxRequest()) //It will check for ajax request and session is null
                {
                    filterContext.Result = this.RedirectToAction("ElmahError", "Error");
                }
                else //It will check for request and session is null
                {
                    filterContext.Result = this.RedirectToAction("Error", "Error");
                }
                base.OnException(filterContext);
                return;
            }
            if (new System.Web.HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                System.Exception e = filterContext.Exception;
                filterContext.ExceptionHandled = true;
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                if (filterContext.HttpContext.Request.IsAjaxRequest()) //It will check for ajax request and session is null
                {
                    filterContext.Result = this.RedirectToAction("ElmahError", "Error");
                }
                else //It will check for request and session is null
                {
                    filterContext.Result = this.RedirectToAction("Error", "Error");
                }
                base.OnException(filterContext);
                return;
            }
            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {

                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = "Sorry, an error occurred while processing your request." //filterContext.Exception.Message
                    }
                };
            }
            else
            {
                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
            }
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
