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

namespace RevenuePlanner.Test.MockHelpers
{
    public static class DataHelper
    {
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

            HttpContext.Current.Session["User"] =  objBDSServiceClient.ValidateUser(applicationId, userName, singlehash);

            HttpContext.Current.Session["Permission"] = objBDSServiceClient.GetPermission(applicationId, ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).RoleId); ;

            return HttpContext.Current;
        }

        public static int GetModelId()
        {
            MRPEntities db = new MRPEntities();
            string published = Convert.ToString(Enums.ModelStatusValues.Single(s => s.Key.Equals(Enums.ModelStatus.Published.ToString())).Value).ToLower();
            return db.Models.Where(m => m.IsDeleted == false && m.Status.ToLower() == published).Select(m => m.ModelId).FirstOrDefault();
        }

        /// <summary>
        /// Get single published plan id.
        /// </summary>
        /// <returns></returns>
        public static int GetPlanId()
        {
            MRPEntities db = new MRPEntities();
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            return db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.PlanId).FirstOrDefault();
        }

        /// <summary>
        /// Get single published multiple plan id.
        /// </summary>
        /// <returns></returns>
        public static List<int> GetMultiplePlanId()
        {
            MRPEntities db = new MRPEntities();
            string published = Convert.ToString(Enums.PlanStatusValues.Single(s => s.Key.Equals(Enums.PlanStatus.Published.ToString())).Value).ToLower();
            return db.Plans.Where(p => p.IsDeleted == false && p.Status.ToLower() == published).Select(p => p.PlanId).Take(5).ToList();
        }
    }
}
