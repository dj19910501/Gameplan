using RevenuePlanner.Models;
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
            Sessions.ReportPlanId = 0;
            Sessions.PlanUserSavedViews = null;
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
        // Add By Nishant Sheth
        // Desc :: Get Or Set multiples PlanIds
        public static List<int> PlanPlanIds
        {
            get
            {
                if (HttpContext.Current.Session["PlanPlanIds"] != null)
                {
                    return (List<int>)HttpContext.Current.Session["PlanPlanIds"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["PlanPlanIds"] = value;
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
        /// Add By Nishant Sheth
        /// Date : 15-Jul-2016
        /// Store Budgetid/Budget Detail Id
        /// </summary>
        public static Int32 BudgetDetailId
        {
            get
            {
                if (HttpContext.Current.Session["BudgetDetailId"] != null)
                {
                    return Convert.ToInt32(HttpContext.Current.Session["BudgetDetailId"]);
                }
                return 0;
            }
            set
            {
                HttpContext.Current.Session["BudgetDetailId"] = value;
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

                return string.Empty;
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

        /// <summary>
        /// Added By: Mitesh Vaishnav for PL ticket #846
        /// Date: 08/10/2014
        /// Store multiple selected plan's Ids 
        /// </summary>
        public static List<int> ReportPlanIds
        {
            get
            {
                if (HttpContext.Current.Session["ReportPlanIds"] != null)
                {
                    return (List<int>)HttpContext.Current.Session["ReportPlanIds"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["ReportPlanIds"] = value;
            }
        }
        //Added By Komal Rawal
        public static List<string> ReportOwnerIds
        {
            get
            {
                if (HttpContext.Current.Session["ReportOwnerIds"] != null)
                {
                    return (List<string>)HttpContext.Current.Session["ReportOwnerIds"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["ReportOwnerIds"] = value;
            }
        }

        public static List<int> ReportTacticTypeIds
        {
            get
            {
                if (HttpContext.Current.Session["ReportTacticTypeIds"] != null)
                {
                    return (List<int>)HttpContext.Current.Session["ReportTacticTypeIds"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["ReportTacticTypeIds"] = value;
            }
        }
        //End
        public static bool IsDisplayDataInconsistencyMsg
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["DisplayDataInconsistencyMsg"]);
            }
            set
            {
                HttpContext.Current.Session["DisplayDataInconsistencyMsg"] = value;
            }
        }

        /// <summary>
        /// Added By: Arpita Soni for PL ticket #1148
        /// Date: 01/23/2015
        /// Store multiple selected Custom Field Ids 
        /// </summary>
        public static RevenuePlanner.Models.CustomFieldFilter[] ReportCustomFieldIds
        {
            get
            {
                if (HttpContext.Current.Session["ReportCustomFieldIds"] != null)
                {
                    return (RevenuePlanner.Models.CustomFieldFilter[])HttpContext.Current.Session["ReportCustomFieldIds"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["ReportCustomFieldIds"] = value;
            }
        }

        public static bool IsBudgetShow
        {
            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["IsBudgetShow"]);
            }
            set
            {
                HttpContext.Current.Session["IsBudgetShow"] = value;
            }
        }

        public static string FilterPresetName
        {
            get
            {
                if (HttpContext.Current.Session["FilterPresetName"] != null)
                {
                    return (string)HttpContext.Current.Session["FilterPresetName"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["FilterPresetName"] = value;
            }
        }

        //Added By komal Rawal for #1959 to handle last viewed data in session
        public static List<Plan_UserSavedViews> PlanUserSavedViews
        {

            get
            {
                if (HttpContext.Current.Session["PlanUserSavedViews"] != null)
                {
                    return (List<Plan_UserSavedViews>)HttpContext.Current.Session["PlanUserSavedViews"];
                }
                return null;
            }
            set
            {
                HttpContext.Current.Session["PlanUserSavedViews"] = value;
            }

        }

    }
}