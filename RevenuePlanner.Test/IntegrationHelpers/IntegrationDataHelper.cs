using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using RevenuePlanner.Test.MockHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace RevenuePlanner.Test.IntegrationHelpers
{
    public static class IntegrationDataHelper
    {
        #region Variables

        public static MRPEntities db = new MRPEntities();

        #endregion

        public static HttpContext SetUserAndPermission(bool IsLogin = false, string Username = "", string Password = "")
        {
            RevenuePlanner.BDSService.BDSServiceClient objBDSServiceClient = new RevenuePlanner.BDSService.BDSServiceClient();
            string userName, password;
            if (IsLogin && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                userName = Username;
                password = Password;
            }
            else
            {
                userName = Convert.ToString(ConfigurationManager.AppSettings["Username"]);
                password = Convert.ToString(ConfigurationManager.AppSettings["Password"]);
            }

            Guid applicationId = Guid.Parse(ConfigurationManager.AppSettings["BDSApplicationCode"]);
            string singlehash = DataHelper.ComputeSingleHash(password);

            HttpContext.Current = MockHelpers.MockHelpers.FakeHttpContext();

            var Users = objBDSServiceClient.ValidateUser(applicationId, userName, singlehash);
            if (Users != null)
            {
                HttpContext.Current.Session["User"] = Users;
                HttpContext.Current.Session["Permission"] = objBDSServiceClient.GetPermission(applicationId, ((RevenuePlanner.BDSService.User)(HttpContext.Current.Session["User"])).RoleId);
            }

            Message msg = new Message();
            //var xmlMsgFilePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.FullName + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonMsgFilePath");
            var xmlMsgFilePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.FullName + "\\" + System.Configuration.ConfigurationManager.AppSettings.Get("XMLCommonIntegrationMsgFilePath");
            msg.loadMsg(xmlMsgFilePath);
            HttpContext.Current.Cache["CommonMsg"] = msg;
            CacheDependency dependency = new CacheDependency(xmlMsgFilePath);
            HttpContext.Current.Cache.Insert("CommonMsg", msg, dependency);
            Common.objCached = msg;


            return HttpContext.Current;
        }
    }
}
