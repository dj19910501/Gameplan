using RevenuePlanner.Helpers;
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
            if (!(Request.RawUrl.ToLower().Contains("userphoto")) && !(Request.RawUrl.ToLower().Contains("changepassword")) && (string.Compare(entity.Trim(), "login", true) != 0)) //Check of request is not for login page then redirect user to login page
            {
                if (Sessions.User != null)
                {
                    if (Sessions.User.LastLoginDate == null && Sessions.RedirectToChangePassword)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "ChangePassword", Controller = "User" }));
                    }

                    if (Sessions.RedirectToSetSecurityQuestion)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "SetSecurityQuestion", Controller = "Login" }));
                    }
                }
            }

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
            return PartialView("_SupportPartial");
        }
    }
}
