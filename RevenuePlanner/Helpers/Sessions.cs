using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace RevenuePlanner.Helpers
{
    //[Serializable]
    public abstract class Sessions
    {
        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 06/18/2014
        /// Session variable for user activity permissions.
        /// </summary>
        public static Enums.ApplicationActivity UserActivityPermission
        {
            get
            {
                if (HttpContext.Current.Session["UserActivityPermission"] != null)
                {
                    return ((Enums.ApplicationActivity)HttpContext.Current.Session["UserActivityPermission"]);
                }

                return new Enums.ApplicationActivity();
            }
            set
            {
                HttpContext.Current.Session["UserActivityPermission"] = value;
            }
        }

        ///// <summary>
        ///// MRP Application Id
        ///// </summary>
        //public static List<Enums.ApplicationActivity> UserActivityPermission
        //{
        //    get
        //    {
        //        return ((List<Enums.ApplicationActivity>)HttpContext.Current.Session["UserActivityPermission"]);
        //    }
        //    set
        //    {
        //        HttpContext.Current.Session["UserActivityPermission"] = value;
        //    }
        //}

        /// <summary>
        /// MRP Application Id
        /// </summary>
        public static Guid ApplicationId
        {
            get
            {
                return Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]); ;
            }
        }

        /// <summary>
        /// Logged in user details
        /// </summary>
        public static BDSService.User User
        {
            get
            {
                if (HttpContext.Current.Session["User"] != null)
                {
                    return (BDSService.User)HttpContext.Current.Session["User"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["User"] = value;
            }
        }

        ///// <summary>
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// Flag to indicate current user is System Admin.
        ///// </summary>
        //public static bool IsSystemAdmin
        //{
        //    get
        //    {
        //        return Convert.ToBoolean(HttpContext.Current.Session["IsSystemAdmin"]);
        //    }
        //    set
        //    {
        //        HttpContext.Current.Session["IsSystemAdmin"] = value;
        //    }
        //}

        ///// <summary>
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// Flag to indicate current user is Client Admin.
        ///// </summary>
        //public static bool IsClientAdmin
        //{
        //    get
        //    {
        //        return Convert.ToBoolean(HttpContext.Current.Session["IsClientAdmin"]);
        //    }
        //    set
        //    {
        //        HttpContext.Current.Session["IsClientAdmin"] = value;
        //    }
        //}

        ///// <summary>
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// Flag to indicate current user is Director.
        ///// </summary>
        //public static bool IsDirector
        //{
        //    get
        //    {
        //        return Convert.ToBoolean(HttpContext.Current.Session["IsDirector"]);
        //    }
        //    set
        //    {
        //        HttpContext.Current.Session["IsDirector"] = value;
        //    }
        //}

        ///// <summary>
        ///// Added By: Maninder Singh Wadhva.
        ///// Date: 11/27/2013
        ///// Flag to indicate current user is Planner.
        ///// </summary>
        //public static bool IsPlanner
        //{
        //    get
        //    {
        //        return Convert.ToBoolean(HttpContext.Current.Session["IsPlanner"]);
        //    }
        //    set
        //    {
        //        HttpContext.Current.Session["IsPlanner"] = value;
        //    }
        //}

        /// <summary>
        /// Added By: Manoj Limbachiya.
        /// Date: 11/29/2013
        /// Store menus in session for MRP Application.
        /// </summary>
        public static List<BDSService.Menu> AppMenus
        {
            get
            {
                return (List<BDSService.Menu>)HttpContext.Current.Session["Menus"];
            }
            set
            {
                HttpContext.Current.Session["Menus"] = value;
            }
        }
        /// <summary>
        /// Added By: Manoj Limbachiya.
        /// Date: 11/29/2013
        /// Store Permission in session for MRP Application.
        /// </summary>
        public static List<BDSService.Permission> RolePermission
        {
            get
            {
                return (List<BDSService.Permission>)HttpContext.Current.Session["Permission"];
            }
            set
            {
                HttpContext.Current.Session["Permission"] = value;
            }
        }

        /// <summary>
        /// Added By: Manoj Limbachiya.
        /// Date: 11/29/2013
        /// To remove all the session variables.
        /// </summary>
        public static void Clear()
        {
            Sessions.User = null;
            //Sessions.IsSystemAdmin = false;
            //Sessions.IsClientAdmin = false;
            //Sessions.IsDirector = false;
            //Sessions.IsPlanner = false;
            Sessions.AppMenus = null;
            Sessions.RolePermission = null;
            HttpContext.Current.Session["PlanId"] = 0;
            HttpContext.Current.Session["PublishedPlanId"] = 0;
            Sessions.ModelId = 0;
            Sessions.BusinessUnitId = Guid.Empty;
            Sessions.ReportPlanId = 0;
        }

        /// <summary>
        /// Added By: Maninder Singh Wadhva.
        /// Date: 12/02/2013
        /// Store Plan Id.
        /// </summary>
        public static Int32 PlanId
        {
            get
            {
                return Convert.ToInt32(HttpContext.Current.Session["PlanId"]);
            }
            set
            {
                HttpContext.Current.Session["PlanId"] = value;
                if (Common.IsPlanPublished(value))
                {
                    HttpContext.Current.Session["PublishedPlanId"] = value;
                }
            }
        }
        public static Int32 PublishedPlanId
        {
            get
            {
                return Convert.ToInt32(HttpContext.Current.Session["PublishedPlanId"]);
            }
            set
            {
                HttpContext.Current.Session["PublishedPlanId"] = value;
            }
        }
        /// <summary>
        /// Added By: Kunal
        /// Store Model Id.
        /// </summary>
        public static Int32 ModelId
        {
            get
            {
                return Convert.ToInt32(HttpContext.Current.Session["ModelId"]);
            }
            set
            {
                HttpContext.Current.Session["ModelId"] = value;
            }

        }

        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 02/12/2014
        /// To redirect the user to change password module
        /// </summary>
        public static bool RedirectToChangePassword
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["RedirectToChangePassword"]);
            }
            set
            {
                HttpContext.Current.Session["RedirectToChangePassword"] = value;
            }
        }

        /// <summary>
        /// Added By: Juned Katariya.
        /// Date: 24/02/2014
        /// Store Business Unit Id.
        /// </summary>
        public static Guid BusinessUnitId
        {
            get
            {
                if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["BusinessUnitId"])))
                {
                    return Guid.Parse(Convert.ToString(HttpContext.Current.Session["BusinessUnitId"]));
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set
            {
                HttpContext.Current.Session["BusinessUnitId"] = value;
            }
        }

        /// <summary>
        /// Added By: Juned Katariya.
        /// Date: 25/02/2014
        /// Stores Plan Id for Report filter section.
        /// </summary>
        public static Int32 ReportPlanId
        {
            get
            {
                return Convert.ToInt32(HttpContext.Current.Session["ReportPlanId"]);
            }
            set
            {
                HttpContext.Current.Session["ReportPlanId"] = value;
            }
        }

        /// <summary>
        /// Added By: Kuber Joshi
        /// Date: 25/02/2014
        /// To fetch the application name
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (HttpContext.Current.Session["ApplicationName"] != null)
                {
                    return Convert.ToString(HttpContext.Current.Session["ApplicationName"]);
                }
                else
                {
                    Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
                    BDSService.BDSServiceClient objBDSServiceClient = new BDSService.BDSServiceClient();
                    string applicationName = objBDSServiceClient.GetApplicationName(applicationId);
                    HttpContext.Current.Session["ApplicationName"] = applicationName;
                    return Convert.ToString(HttpContext.Current.Session["ApplicationName"]);
                }
            }
        }

        /// <summary>
        /// Added By: Dharmraj Mangukiya
        /// Date: 28/3/2014
        /// To redirect the user to set security question module
        /// </summary>
        public static bool RedirectToSetSecurityQuestion
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["RedirectToSetSecurityQuestion"]);
            }
            set
            {
                HttpContext.Current.Session["RedirectToSetSecurityQuestion"] = value;
            }
        }
    }
}