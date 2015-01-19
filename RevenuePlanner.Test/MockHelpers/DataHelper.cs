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

        public static Plan_Campaign_Program_Tactic GetPlanTactic(Guid clientId)
        {
            var objTactic = db.Plan_Campaign_Program_Tactic.Where(a => a.BusinessUnit.ClientId == clientId && a.IsDeleted == false).OrderBy(a => Guid.NewGuid()).FirstOrDefault();
            return objTactic;
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
        #endregion
    }
}
