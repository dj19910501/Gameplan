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
        /// RequestId is added for security purpose ,when user reset password.
        /// </summary>
        public static Guid RequestId
        {
            get
            {
                if (HttpContext.Current.Session["RequestId"] != null)
                {
                    return new Guid(Convert.ToString(HttpContext.Current.Session["RequestId"]));
                }
                return Guid.Empty;
            }
            set
            {
                HttpContext.Current.Session["RequestId"] = value;
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
            Sessions.StartDate = null;
            Sessions.EndDate = null;            
            Sessions.ViewByValue = string.Empty;
            Sessions.ImportTimeFrame = null;
            Sessions.BudgetDetailId = 0;
            Sessions.PlanPlanIds = null;
            Sessions.ClientUsers = null;
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
                int num = 0;
                if (HttpContext.Current.Session["BudgetDetailId"] != null)
                {
                    int.TryParse(Convert.ToString(HttpContext.Current.Session["BudgetDetailId"]), out num);
                }
                return num;
            }
            set
            {
                HttpContext.Current.Session["BudgetDetailId"] = value;
            }
        }
        /// <summary>
        /// Add By Nishant Sheth
        /// Date : 26-Jul-2016
        /// Store Child label type for report card section
        /// </summary>
        public static string childlabelType
        {
            get
            {
                string childlabelType = string.Empty;
                if (HttpContext.Current.Session["childlabelType"] != null)
                {
                    childlabelType = Convert.ToString(HttpContext.Current.Session["childlabelType"]);
                }
                return childlabelType;
            }
            set
            {
                HttpContext.Current.Session["childlabelType"] = value;
            }
        }
        /// <summary>
        /// Add By Nishant Sheth
        /// Date : 26-Jul-2016
        /// Store Is ROI Package values is display or not
        /// </summary>
        public static bool IsROIPackDisplay
        {
            get
            {
                bool IsROIPackDisplay = false;
                if (HttpContext.Current.Session["IsROIPackDisplay"] != null)
                {
                    bool.TryParse(Convert.ToString(HttpContext.Current.Session["IsROIPackDisplay"]), out IsROIPackDisplay);
                }
                return IsROIPackDisplay;
            }
            set
            {
                HttpContext.Current.Session["IsROIPackDisplay"] = value;
            }
        }
        /// <summary>
        /// Add By Nishant Sheth
        /// Return plan exchange rate value
        /// </summary>
        /// <returns></returns>
        public static double PlanExchangeRate
        {
            get
            {
                double ExchangeRate = 1;
                if (HttpContext.Current.Session["PlanExchangeRate"] != null)
                {
                    double.TryParse(Convert.ToString(HttpContext.Current.Session["PlanExchangeRate"]), out ExchangeRate);
                }
                return ExchangeRate;
            }
            set
            {
                HttpContext.Current.Session["PlanExchangeRate"] = value;
            }
        }

        /// <summary>
        /// Add By Nishant Sheth
        /// Return plan currencySymbol
        /// </summary>
        /// <returns></returns>
        public static string PlanCurrencySymbol
        {
            get
            {
                string CurrencySymbol = Enums.CurrencySymbolsValues[Enums.CurrencySymbols.USD.ToString()].ToString();
                if (HttpContext.Current.Session["PlanCurrencySymbol"] != null)
                {
                    CurrencySymbol = Convert.ToString(HttpContext.Current.Session["PlanCurrencySymbol"]);
                }
                return CurrencySymbol;
            }
            set
            {
                HttpContext.Current.Session["PlanCurrencySymbol"] = value;
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
        public static List<int> ReportOwnerIds
        {
            get
            {
                if (HttpContext.Current.Session["ReportOwnerIds"] != null)
                {
                    return (List<int>)HttpContext.Current.Session["ReportOwnerIds"];
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
        //Added By devanshi to store media code permission
        public static bool IsMediaCodePermission
        {

            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["IsMediaCodePermission"]);
            }
            set
            {
                HttpContext.Current.Session["IsMediaCodePermission"] = value;
            }

        }

        public static string StartDate
        {
            get
            {
                if (HttpContext.Current.Session["StartDate"] != null)
                {
                    return Convert.ToString(HttpContext.Current.Session["StartDate"]);
                }
                return string.Empty;
            }
            set
            {
                HttpContext.Current.Session["StartDate"] = value;
            }
        }
        public static string EndDate
        {
            get
            {
                if (HttpContext.Current.Session["EndDate"] != null)
                {
                    return Convert.ToString(HttpContext.Current.Session["EndDate"]);
                }
                return string.Empty;
            }
            set
            {
                HttpContext.Current.Session["EndDate"] = value;
            }
        }
        //Added By devanshi on 19-8-2016 #2476 to store alerts permission
        public static bool IsAlertPermission
        {

            get
            {
                return Convert.ToBoolean(HttpContext.Current.Session["IsAlertPermission"]);
            }
            set
            {
                HttpContext.Current.Session["IsAlertPermission"] = value;
            }

        }
        /// <summary>
        /// Add By Kausha Somaiya
        /// Set VIew by in Session
        /// </summary>
        /// <returns></returns>
        public static string ViewByValue
        {
            get
            {
                string viewBy = string.Empty;
                if (HttpContext.Current.Session["ViewByValue"] != null)
                {
                    viewBy = Convert.ToString(HttpContext.Current.Session["ViewByValue"]);
                }
                return viewBy;
            }
            set
            {
                HttpContext.Current.Session["ViewByValue"] = value;
            }
        }
        //added by devanshi for finding no plan for clien/user
        public static bool IsNoPlanCreated
        {

            get
            {
                if (HttpContext.Current.Session["IsNoPlanCreated"] != null)
                {
                    return Convert.ToBoolean(HttpContext.Current.Session["IsNoPlanCreated"]);
                }
                else
                {
                    return false;
                }
            }
            set
            {
                HttpContext.Current.Session["IsNoPlanCreated"] = value;
            }

        }
        public static bool FirstTimeLogin
        {

            get
            {
                if (HttpContext.Current.Session["FirstTimeLogin"] != null)
                {
                    return Convert.ToBoolean(HttpContext.Current.Session["FirstTimeLogin"]);
                }
                else
                {
                    return false;
                }
            }
            set
            {
                HttpContext.Current.Session["FirstTimeLogin"] = value;
            }

        }

        public static bool IsfromMeasure
        {
            get
            {
                bool IsfromMeasure = false;
                if (HttpContext.Current.Session["IsfromMeasure"] != null)
                {
                    bool.TryParse(Convert.ToString(HttpContext.Current.Session["IsfromMeasure"]), out IsfromMeasure);
                }
                return IsfromMeasure;
            }
            set
            {
                HttpContext.Current.Session["IsfromMeasure"] = value;
            }
        }

        public static List<BDSService.UserHierarchy> UserHierarchyList
        {
            get
            {
                return (List<BDSService.UserHierarchy>)HttpContext.Current.Session["UserHierarchyList"];
            }
            set
            {
                HttpContext.Current.Session["UserHierarchyList"] = value;
            }
        }
        /// <summary>
        /// Add By Devanshi - #2804
        /// Set Time frame value in Session for checking view at the time of import marketing budget
        /// </summary>
        /// <returns></returns>
        public static string ImportTimeFrame
        {
            get
            {
                string viewBy = string.Empty;
                if (HttpContext.Current.Session["ImportTimeFrame"] != null)
                {
                    viewBy = Convert.ToString(HttpContext.Current.Session["ImportTimeFrame"]);
                }
                return viewBy;
            }
            set
            {
                HttpContext.Current.Session["ImportTimeFrame"] = value;
            }
        }
        /// <summary>
        /// Added by Arpita Soni 
        /// List of users for the specific client
        /// </summary>
        public static List<BDSService.User> ClientUsers
        {
            get
            {
                List<BDSService.User> lstUsers = new List<BDSService.User>();
                if (HttpContext.Current.Session["ClientUsers"] != null)
                {
                    lstUsers = (List<BDSService.User>)(HttpContext.Current.Session["ClientUsers"]);
                }
                return lstUsers;
            }
            set
            {
                HttpContext.Current.Session["ClientUsers"] = value;
            }
        }
        /// <summary>
        /// Added by Arpita Soni 
        /// List of users for the specific client
        /// </summary>
        public static Dictionary<Guid,int> dictUserIds
        {
            get
            {
                Dictionary<Guid, int> lstUsers = new Dictionary<Guid, int>();
                if (HttpContext.Current.Session["dictUserIds"] != null)
                {
                    lstUsers = (Dictionary<Guid, int>)(HttpContext.Current.Session["dictUserIds"]);
                }
                return lstUsers;
            }
            set
            {
                HttpContext.Current.Session["dictUserIds"] = value;
            }
        }
    }
}