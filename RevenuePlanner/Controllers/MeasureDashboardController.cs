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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RevenuePlanner.Controllers
{
    public class MeasureDashboardController : CommonController
    {
        public CacheObject objCache = new CacheObject();
        public ActionResult Index(string DashboardId)
        {
            try
            {
                int DashId = 0;
                if (int.TryParse(Convert.ToString(DashboardId), out DashId) && Sessions.AppMenus.Where(w => w.Description == "javascript:void(0)" && w.MenuApplicationId == DashId).Any())
                {
                    if (string.IsNullOrEmpty(Sessions.StartDate))
                    {
                        Sessions.StartDate = DateTime.Now.AddMonths(6).ToString("MM/dd/yyyy");
                        Sessions.EndDate = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy");
                        if (Convert.ToDateTime(Sessions.StartDate) > Convert.ToDateTime(Sessions.EndDate))
                        {
                            Sessions.StartDate = Convert.ToDateTime(Sessions.EndDate).AddMonths(-6).ToString("MM/dd/yyyy");
                        }
                    }
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
                    string url = ApiUrl + "api/Dashboard/GetDashboardContent?DashboardId=" + DashId + "&UserId=" + Sessions.User.UserId + "&ConnectionString=" + ReportDBConnString + "&UserName=" + AuthorizedReportAPIUserName + "&Password=" + AuthorizedReportAPIPassword;
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
                    //Insertation start #2416 mark Selected=true if Sessions.ViewByValue is not null
                    if (!string.IsNullOrEmpty(Sessions.ViewByValue))
                    {
                        var selectedViewBy = li.Where(x => x.Value == Sessions.ViewByValue).First();
                        selectedViewBy.Selected = true;
                    }
                    else
                    {
                        var selectedViewBy = li.Where(x => x.Value == Convert.ToString(Enums.viewByOption.Q)).First();
                        selectedViewBy.Selected = true;
                    }
                    //Insertation end #2416 mark Selected=true if Sessions.ViewByValue is not null
                    ViewData["ViewBy"] = li;
                    ViewBag.ViewBy = li;
                    ViewBag.DashboardID = DashId;
                    ViewBag.AuthorizedReportAPIUserName = AuthorizedReportAPIUserName;
                    ViewBag.AuthorizedReportAPIPassword = AuthorizedReportAPIPassword;
                    ViewBag.ApiUrl = ApiUrl;
                    ViewBag.DashboardList = Common.GetSpDashboarData(Sessions.User.UserId.ToString());
                    ViewBag.DashboardAccess = true;

                    return View("Index", model);
                }
                else
                {
                    ViewBag.DashboardID = DashId;
                    ViewBag.AuthorizedReportAPIUserName = string.Empty;
                    ViewBag.AuthorizedReportAPIPassword = string.Empty;
                    ViewBag.ApiUrl = string.Empty;
                    ViewBag.DashboardList = Common.GetSpDashboarData(Sessions.User.UserId.ToString());
                    ViewBag.DashboardAccess = false;
                    Custom_Dashboard model = new Custom_Dashboard();
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        /// <summary>
        /// Add By Nandish Shah
        /// Get Custom Report List
        /// </summary>
        /// <returns>List<CurrencyModel.ClientCurrency></returns>
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

        /// <summary>
        /// Add By Nandish Shah
        /// Set Date Range
        /// </summary>
        /// <returns>List<CurrencyModel.ClientCurrency></returns>
        public JsonResult SetDateRange(string StartDate = "01/01/1900", string EndDate = "01/01/2100")
        {
            Sessions.StartDate = StartDate;
            Sessions.EndDate = EndDate;
            return Json(new { isSuccess = true }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateViewBy(string viewBy)
        {
            if (Convert.ToString(Enums.PlanAllocatedBy.quarters) == viewBy)
                viewBy = Convert.ToString(Enums.viewByOption.Q);
            if (Convert.ToString(Enums.PlanAllocatedBy.months) == viewBy)
                viewBy = Convert.ToString(Enums.viewByOption.M);
            Sessions.ViewByValue = viewBy;
            TempData["viewbyValue"] = viewBy;
            return Json(new { viewbyValue = viewBy }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Add By Nandish Shah
        /// Get Chart Data
        /// </summary>
        /// <returns>List<CurrencyModel.ClientCurrency></returns>
        public async Task<JsonResult> GetChart(int Id, string DbName, string Container, string[] SDV, bool TopOnly = true, string ViewBy = "Q", string StartDate = "01/01/1900", string EndDate = "01/01/2100")
        {
            RevenuePlanner.Services.ICurrency objCurrency = new RevenuePlanner.Services.Currency();
            HttpResponseMessage response = new HttpResponseMessage();
            string result = string.Empty;
            string AuthorizedReportAPIUserName = string.Empty;
            string AuthorizedReportAPIPassword = string.Empty;
            string ApiUrl = string.Empty;
            string ConnectionString = string.Empty;
            Sessions.ViewByValue = ViewBy;
            if (!string.IsNullOrEmpty(DbName) && DbName == Convert.ToString(Enums.ApplicationCode.RPC))
            {
                ConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(DbName) && DbName == Convert.ToString(Enums.ApplicationCode.MRP))
            {
                var efConnectionString = ConfigurationManager.ConnectionStrings["MRPEntities"].ToString();
                var builder = new EntityConnectionStringBuilder(efConnectionString);
                string regularConnectionString = builder.ProviderConnectionString;

                if (!string.IsNullOrEmpty(Convert.ToString(regularConnectionString)))
                {
                    ConnectionString = Convert.ToString(regularConnectionString.ToString().Replace(@"\", @"\\"));
                }
            }

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
                List<RevenuePlanner.Models.CurrencyModel.ClientCurrency> MonthWiseUserReportCurrency = objCurrency.GetUserCurrencyMonthwise(StartDate, EndDate);
                string[] CurrencyRate = null;
                if (MonthWiseUserReportCurrency != null)
                {
                    CurrencyRate = new string[MonthWiseUserReportCurrency.Count];
                    int i = 0;
                    foreach (var item in MonthWiseUserReportCurrency)
                    {
                        CurrencyRate[i] = item.StartDate.ToString("MM/dd/yyyy") + ":" + item.EndDate.ToString("MM/dd/yyyy") + ":" + item.ExchangeRate + ":" + item.CurrencySymbol;
                        i++;
                    }
                }
                
                try
                {
                    HttpClient client = new HttpClient();
                    
                    int CommonWebAPITimeout = 0;
                    string strwebAPITimeout = System.Configuration.ConfigurationManager.AppSettings["CommonIntegrationWebAPITimeOut"];
                    if (!string.IsNullOrEmpty(strwebAPITimeout))
                        CommonWebAPITimeout = Convert.ToInt32(strwebAPITimeout);
                    
                    client.Timeout = TimeSpan.FromHours(CommonWebAPITimeout);  //set timeout for Common Integration API call
                    client.Timeout = TimeSpan.FromHours(3);  //set timeout for Common Integration API call
                    
                    Uri baseAddress = new Uri(ApiUrl);
                    client.BaseAddress = baseAddress;
                    
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    
                    ReportParameters objParams = new ReportParameters();
                    objParams.Id = Id;
                    objParams.ConnectionString = ConnectionString;
                    objParams.Container = Container;
                    objParams.SDV = SDV;
                    objParams.TopOnly = TopOnly;
                    objParams.ViewBy = ViewBy;
                    objParams.StartDate = StartDate;
                    objParams.EndDate = EndDate;
                    objParams.UserName = AuthorizedReportAPIUserName;
                    objParams.Password = AuthorizedReportAPIPassword;
                    objParams.CurrencyRate = CurrencyRate;

                    response = await client.PostAsJsonAsync("api/Report/Chart ", objParams);
                    result = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
            return Json(new { isSuccess = true, data = result }, JsonRequestBehavior.AllowGet);
        }        
    }
}
