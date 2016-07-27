using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.BAL;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class MeasureDashboardController : CommonController
    {
        public ActionResult Index(int DashboardId)
        {
            try
            {
                WebClient client = new WebClient();
                string regularConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
                string ReportDBConnString = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(regularConnectionString)))
                {
                    ReportDBConnString = Convert.ToString(regularConnectionString);
                }
                string AuthorizedReportAPIUserName = string.Empty;
                string AuthorizedReportAPIPassword = string.Empty;
                string ApiUrl = string.Empty;
                if (ConfigurationManager.AppSettings.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIUserName"])))
                    {
                        AuthorizedReportAPIUserName = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIUserName");
                    }
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIPassword"])))
                    {
                        AuthorizedReportAPIPassword = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIPassword");
                    }
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["IntegrationApi"])))
                    {
                        ApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
                        if (!string.IsNullOrEmpty(ApiUrl) && !ApiUrl.EndsWith("/"))
                        {
                            ApiUrl += "/";
                        }
                    }
                }
                Custom_Dashboard model = new Custom_Dashboard();
                string url = ApiUrl + "api/Dashboard/GetDashboardContent?DashboardId=" + DashboardId + "&UserId=" + Sessions.User.UserId + "&ConnectionString=" + ReportDBConnString + "&UserName=" + AuthorizedReportAPIUserName + "&Password=" + AuthorizedReportAPIPassword;
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    string result = client.DownloadString(url);
                    List<DashboardContentModel> list = JsonConvert.DeserializeObject<List<DashboardContentModel>>(result);
                    model.DashboardContent = list;
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
                
                List<SelectListItem> li = new List<SelectListItem>();
                li.Add(new SelectListItem { Text = "Years", Value = "Y" });
                li.Add(new SelectListItem { Text = "Quarters", Value = "Q" });
                li.Add(new SelectListItem { Text = "Months", Value = "M" });
                li.Add(new SelectListItem { Text = "Weeks", Value = "W" });
                ViewData["ViewBy"] = li;

                ViewBag.DashboardID = DashboardId;
                ViewBag.ReportDBConnString = ReportDBConnString;
                ViewBag.AuthorizedReportAPIUserName = AuthorizedReportAPIUserName;
                ViewBag.AuthorizedReportAPIPassword = AuthorizedReportAPIPassword;
                ViewBag.ApiUrl = ApiUrl;
                ViewBag.DashboardList = Common.GetSpDashboarData(Sessions.User.UserId.ToString());

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }
        public ActionResult GetCustomReport(string DashboardId = "", string ConnectionString = "")
        {
            Custom_Dashboard model = new Custom_Dashboard();
            CustomDashboard cd = new CustomDashboard();

            if (!string.IsNullOrEmpty(DashboardId))
            {
                int DashId = int.Parse(DashboardId.ToString());
                model = cd.GetMainDashBoardInfo(DashId);

                string regularConnectionString = ConnectionString;

                string ReportDBConnString = string.Empty;
                if (!string.IsNullOrEmpty(Convert.ToString(regularConnectionString)))
                {
                    ReportDBConnString = Convert.ToString(regularConnectionString.ToString().Replace(@"\", @"\\"));
                }

                string AuthorizedReportAPIUserName = string.Empty;
                if (ConfigurationManager.AppSettings.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIUserName"])))
                    {
                        AuthorizedReportAPIUserName = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIUserName");
                    }
                }

                string AuthorizedReportAPIPassword = string.Empty;
                if (ConfigurationManager.AppSettings.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["AuthorizedReportAPIPassword"])))
                    {
                        AuthorizedReportAPIPassword = System.Configuration.ConfigurationManager.AppSettings.Get("AuthorizedReportAPIPassword");
                    }
                }
                string ApiUrl = string.Empty;
                if (ConfigurationManager.AppSettings.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["IntegrationApi"])))
                    {
                        ApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
                    }
                }

                ViewBag.ReportDBConnString = ReportDBConnString;
                ViewBag.AuthorizedReportAPIUserName = AuthorizedReportAPIUserName;
                ViewBag.AuthorizedReportAPIPassword = AuthorizedReportAPIPassword;
                ViewBag.ApiUrl = ApiUrl;
            }

            return View("Index", model);
        }
    }
}
