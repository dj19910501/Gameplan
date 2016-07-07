using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace RevenuePlanner.Test.MockHelpers
{
    public static class DataHelper
    {
        #region Variables

        public static MRPEntities db = new MRPEntities();

        #endregion

        /// <summary>
        /// Get Value of json result property
        /// </summary>
        /// <param name="result"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetValue(this JsonResult result, string propertyName)
        {
            IDictionary<string, object> wrapper = new System.Web.Routing.RouteValueDictionary(result.Data);
            return wrapper[propertyName];
        }

        /// <summary>
        /// Get Value of json result property
        /// </summary>
        /// <param name="result"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetValue<T>(this JsonResult result, string propertyName)
        {
            return (T)GetValue(result, propertyName);
        }

        public static string ComputeSingleHash(string SimpleText)
        {
            // Convert plain text into a byte array. (here plain text would be the single hash text)
            byte[] SingleHashTextBytes = Encoding.UTF8.GetBytes(SimpleText);

            // We're using SHA512 algorithm for hashing purpose
            HashAlgorithm hash = new SHA512Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] SinglehashBytes = hash.ComputeHash(SingleHashTextBytes);

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(SinglehashBytes);

            // Return the result.
            return hashValue;
        }

        public static HttpContext SetUserAndPermission()
        {
            RevenuePlanner.BDSService.BDSServiceClient objBDSServiceClient = new RevenuePlanner.BDSService.BDSServiceClient();


            string userName = Convert.ToString(ConfigurationManager.AppSettings["Username"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
            string singlehash = DataHelper.ComputeSingleHash(password);


            HttpContext.Current = MockHelpers.FakeHttpContext();

            HttpContext.Current.Session["User"] = objBDSServiceClient.ValidateUser(applicationId, userName, singlehash);

            HttpContext.Current.Session["Permission"] = objBDSServiceClient.GetPermission(applicationId, ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).RoleId);

            return HttpContext.Current;
        }

        public static int GetModelId()
        {
            string published = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower();
            return db.Models.Where(m => m.IsDeleted == false && m.Status.ToLower() == published).Select(m => m.ModelId).FirstOrDefault();
        }

        /// <summary>
        /// Get single published plan id.
        /// </summary>
        /// <returns></returns>
        public static int GetPlanId()
        {
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            return db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.PlanId).FirstOrDefault();
        }

        /// <summary>
        /// Get Year
        /// </summary>
        /// <returns></returns>
        /// 
        public static string GetYear()
        {
            string year = DateTime.Now.Year.ToString();
            return year;
        }

        /// <summary>
        /// Get Plan Year
        /// </summary>
        /// <returns></returns>
        /// 
        public static string GetPlanYear()
        {
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            return Convert.ToString(db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.Year).FirstOrDefault());
        }

        /// <summary>
        /// Get comma separated published plan id list.
        /// </summary>
        /// <returns></returns>
        public static string GetPlanIdList()
        {
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            var planIds = db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.PlanId).Take(10).ToList();
            return string.Join(",", planIds.Select(plan => plan.ToString()));
        }

        public static Plan_Campaign_Program_Tactic GetPlanTactic(Guid clientId)
        {
            var objTactic = db.Plan_Campaign_Program_Tactic.Where(a => a.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objTactic;
        }

        public static Plan_Campaign_Program_Tactic_LineItem GetPlanLineItem(Guid clientId)
        {
            var objLineItem = db.Plan_Campaign_Program_Tactic_LineItem.Where(a => a.Plan_Campaign_Program_Tactic.Plan_Campaign_Program.Plan_Campaign.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objLineItem;
        }

        public static Plan_Campaign_Program GetPlanProgram(Guid clientId)
        {
            var objProgram = db.Plan_Campaign_Program.Where(a => a.Plan_Campaign.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objProgram;
        }

        public static Plan GetPlan(Guid clientId)
        {
            var objPlan = db.Plans.Where(a => a.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objPlan;
        }
        public static Plan_Campaign GetPlanCampaign(Guid clientId)
        {
            var objCampaign = db.Plan_Campaign.Where(a => a.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objCampaign;
        }
        public static List<Plan_Improvement_Campaign_Program_Tactic> GetPlanImprovementTacticList(Guid clientId)
        {
            var objImprovementtactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).Take(10).ToList();
            return objImprovementtactic;
        }

        public static Plan_Improvement_Campaign_Program_Tactic GetPlanImprovementTactic(Guid clientId)
        {
            var objImprovementtactic = db.Plan_Improvement_Campaign_Program_Tactic.Where(a => a.Plan_Improvement_Campaign_Program.Plan_Improvement_Campaign.Plan.Model.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objImprovementtactic;
        }
        /// <summary>
        /// Get single published multiple plan id.
        /// </summary>
        /// <returns></returns>
        public static List<int> GetMultiplePlanId()
        {
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            return db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.PlanId).Take(5).ToList();
        }

        /// <summary>
        /// Get a integrationInstanceId for the given IntegrationType
        /// </summary>
        /// <param name="integrationType">name of Integration type</param>
        /// <returns>returns an integration instance id for given integration type</returns>
        public static int GetIntegrationInstanceId(string integrationType)
        {
            var IntegrationInstanceId = (from i in db.IntegrationInstances
                                         join t in db.IntegrationTypes on i.IntegrationTypeId equals t.IntegrationTypeId
                                         where i.IsDeleted == false && t.IsDeleted == false && t.Code == integrationType
                                         select i.IntegrationInstanceId).FirstOrDefault();
            return IntegrationInstanceId;
        }

        /// <summary>
        /// Get a integrationTypeId for the given IntegrationType
        /// </summary>
        /// <param name="integrationType">name of Integration type</param>
        /// <returns>returns an integration type id for given integration type</returns>
        public static int GetIntegrationTypeId(string integrationType)
        {
            var IntegrationTypeId = (from integType in db.IntegrationTypes
                                     where integType.IsDeleted == false && integType.Code == integrationType
                                     select integType.IntegrationTypeId).FirstOrDefault();
            return IntegrationTypeId;
        }

        #region Integration

        public static Plan_Campaign_Program_Tactic Get_Plan_Campaign_Program_Tactic(int tacticId)
        {
            Plan_Campaign_Program_Tactic objPlan_Campaign_Program_Tactic = new Plan_Campaign_Program_Tactic();

            var obj = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false && tactic.PlanTacticId == tacticId).Select(tactic => tactic).FirstOrDefault();

            objPlan_Campaign_Program_Tactic = obj;

            return objPlan_Campaign_Program_Tactic;
        }

        /// <summary>
        /// Get Integration Instance Log Id 
        /// </summary>
        /// <param name="_userId">User Id.</param>
        /// <param name="_integrationInstanceId"> Integration Instance Id.</param>
        /// <returns>return Integration Instance Log Id.</returns>
        public static int GetIntegrationInstanceLogId(Guid _userId, int _integrationInstanceId)
        {
            IntegrationInstanceLog instanceLogStart = new IntegrationInstanceLog();
            instanceLogStart.IntegrationInstanceId = Convert.ToInt32(_integrationInstanceId);
            instanceLogStart.SyncStart = DateTime.Now;
            instanceLogStart.CreatedBy = _userId;
            instanceLogStart.CreatedDate = DateTime.Now;
            db.Entry(instanceLogStart).State = EntityState.Added;
            int result = db.SaveChanges();
            return instanceLogStart.IntegrationInstanceLogId;
        }

        #endregion

        #region Custom Restrictions
        /// <summary>
        /// Function to get all the Custom Restriction of the user in View/Edit format
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>returns comma separated string of custom restrictions</returns>
        public static string GetCustomRestrictionInViewEditForm(Guid userId)
        {
            StringBuilder sbCustomRestrictions = new StringBuilder(string.Empty);

            using (MRPEntities objDB = new MRPEntities())
            {
                var lstCustomRestriction = objDB.CustomRestrictions.Where(customRestriction => customRestriction.UserId == userId).Select(customRestriction => customRestriction).ToList();
                if (lstCustomRestriction.Count > 0)
                {
                    lstCustomRestriction.ForEach(customRestriction => sbCustomRestrictions.Append(2 + "_" + customRestriction.CustomFieldId + "_" + customRestriction.CustomFieldOptionId + ","));
                    return sbCustomRestrictions.ToString().TrimEnd(",".ToCharArray());
                }
            }

            return sbCustomRestrictions.ToString();
        }

        /// <summary>
        /// Function to retrieve comma separated list of custom restrictions for Search filter
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns></returns>
        public static string GetSearchFilterForCustomRestriction(Guid userId)
        {
            StringBuilder sbCustomRestrictions = new StringBuilder(string.Empty);

            using (MRPEntities objDB = new MRPEntities())
            {
                var lstCustomRestriction = objDB.CustomRestrictions.Where(customRestriction => customRestriction.UserId == userId).Select(customRestriction => customRestriction).ToList();
                if (lstCustomRestriction.Count > 0)
                {
                    lstCustomRestriction.ForEach(customRestriction => sbCustomRestrictions.Append(customRestriction.CustomFieldId + "_" + customRestriction.CustomFieldOptionId + ","));
                    return sbCustomRestrictions.ToString().TrimEnd(",".ToCharArray());
                }
            }

            return sbCustomRestrictions.ToString();
        }
        #endregion

        #region "Get tactic list"
        /// <summary>
        /// Function to retrieve comma separated list of custom restrictions for Search filter
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns></returns>
        //Get tactic list for report
        public static List<Plan_Campaign_Program_Tactic> GetTacticForReporting(bool isBugdet = false)
        {
            MRPEntities db = new MRPEntities();
            //// Getting current year's all published plan for all custom fields of clientid of director.
            List<Plan_Campaign_Program_Tactic> tacticList = new List<Plan_Campaign_Program_Tactic>();
            List<int> planIds = new List<int>();
            List<Guid> ownerIds = new List<Guid>();
            List<int> TactictypeIds = new List<int>();
            if (Sessions.ReportPlanIds != null && Sessions.ReportPlanIds.Count > 0)
            {
                planIds = Sessions.ReportPlanIds;
            }

            //// Get Tactic list.


            if (isBugdet)
            {
                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false &&
                                                              planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                                                              ).ToList();
            }
            else
            {
                List<string> tacticStatus = Common.GetStatusListAfterApproved();
                tacticList = db.Plan_Campaign_Program_Tactic.Where(tactic => tactic.IsDeleted == false &&
                                                                  tacticStatus.Contains(tactic.Status) &&
                                                                  planIds.Contains(tactic.Plan_Campaign_Program.Plan_Campaign.PlanId)
                                                                  ).ToList();
            }

            //Added by Komal Rawal
            if (Sessions.ReportOwnerIds != null && Sessions.ReportOwnerIds.Count > 0)
            {
                ownerIds = Sessions.ReportOwnerIds.Select(owner => new Guid(owner)).ToList();
                tacticList = tacticList.Where(tactic => ownerIds.Contains(tactic.CreatedBy)
                                                              ).ToList();
            }


            if (Sessions.ReportTacticTypeIds != null && Sessions.ReportTacticTypeIds.Count > 0)
            {
                TactictypeIds = Sessions.ReportTacticTypeIds;
                tacticList = tacticList.Where(tactic => TactictypeIds.Contains(tactic.TacticTypeId)
                                                              ).ToList();

            }
            //End

            if (Sessions.ReportCustomFieldIds != null && Sessions.ReportCustomFieldIds.Count() > 0)
            {
                List<int> tacticids = tacticList.Select(tactic => tactic.PlanTacticId).ToList();
                List<CustomFieldFilter> lstCustomFieldFilter = Sessions.ReportCustomFieldIds.ToList();
                tacticids = Common.GetTacticBYCustomFieldFilter(lstCustomFieldFilter, tacticids);
                tacticList = tacticList.Where(tactic => tacticids.Contains(tactic.PlanTacticId)).ToList();
            }

            return tacticList;
        }
        #endregion

        /// <summary>
        /// Get a ClientId for the given PlanId
        /// Added by Akashdeep Kadia Date:- 12/05/2016 for PL Ticket #989 & 2129
        /// </summary>
        /// <param name="PlanId">PlanId</param>
        /// <returns>returns an ClientId for given PlanId</returns>
        public static Guid GetClientId(int PlanId)
        {
            var ClientId = (from i in db.Models
                            join t in db.Plans on i.ModelId equals t.ModelId
                            where i.IsDeleted == false && t.IsDeleted == false && t.PlanId == PlanId
                            select i.ClientId).FirstOrDefault();
            return ClientId;
        }

        #region Get Dashboard Id
        public static string GetDashboardId()
        {
            int planId = DataHelper.GetPlanId();
            var ClientId = Sessions.User.UserId;
            var DashboardId = Common.GetSpDashboarData(ClientId.ToString()).Select(a => a.Id).FirstOrDefault();
            return Convert.ToString(DashboardId);
        }
        #endregion
        public static string GetPlanIdListClientWise(Guid clientId)
        {
            var planIds = db.Plans.Where(p => p.IsDeleted == false && p.Model.ClientId == clientId).Select(p => p.PlanId).Take(10).ToList();
            return string.Join(",", planIds.Select(plan => plan.ToString()));
        }

    }
}
