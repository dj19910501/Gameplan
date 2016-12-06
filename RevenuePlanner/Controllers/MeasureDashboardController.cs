using Elmah;
using Newtonsoft.Json;
using RevenuePlanner.BAL;
using RevenuePlanner.Helpers;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        public ActionResult Index(string DashboardId, string DashboardPageId)
        {
            try
            {
                int DashId = 0;
                int DashPageId = 0;
                if (int.TryParse(Convert.ToString(DashboardId), out DashId) && Sessions.AppMenus.Where(w => w.Description == "javascript:void(0)" && w.MenuApplicationId == DashId).Any())
                {
                    int.TryParse(Convert.ToString(DashboardPageId), out DashPageId);
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
                    string url = ApiUrl + "api/Dashboard/GetDashboardContent?DashboardId=" + DashId + "&DashboardPageId=" + DashPageId + "&UserId=" + Sessions.User.UserId + "&ConnectionString=" + ReportDBConnString + "&UserName=" + AuthorizedReportAPIUserName + "&Password=" + AuthorizedReportAPIPassword;
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
                    //Insertion start #2416 mark Selected=true if Sessions.ViewByValue is not null
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
                    //Insertion end #2416 mark Selected=true if Sessions.ViewByValue is not null
                    ViewData["ViewBy"] = li;
                    ViewBag.ViewBy = li;
                    ViewBag.DashboardID = DashId;
                    ViewBag.DashboardPageID = DashPageId;
                    ViewBag.AuthorizedReportAPIUserName = AuthorizedReportAPIUserName;
                    ViewBag.AuthorizedReportAPIPassword = AuthorizedReportAPIPassword;
                    ViewBag.ApiUrl = ApiUrl;
                    ViewBag.DashboardList = Common.GetSpDashboarData(Sessions.User.ID);
                    ViewBag.DashboardAccess = true;

                    return View("Index", model);
                }
                else
                {
                    ViewBag.DashboardID = DashId;
                    ViewBag.DashboardPageID = DashPageId;
                    ViewBag.AuthorizedReportAPIUserName = string.Empty;
                    ViewBag.AuthorizedReportAPIPassword = string.Empty;
                    ViewBag.ApiUrl = string.Empty;
                    ViewBag.DashboardList = Common.GetSpDashboarData(Sessions.User.ID);
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
        public async Task<ActionResult> GetChart(int Id, string DbName, string Container, string[] SDV, bool TopOnly = true, string ViewBy = "Q", string StartDate = "01/01/1900", string EndDate = "01/01/2100", bool IsViewData = false)
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
                    //Following will be return chart or table based on passed parameter IsViewData
                    if (IsViewData == false)
                        response = await client.PostAsJsonAsync("api/Report/Chart ", objParams);
                    else
                        response = await client.PostAsJsonAsync("api/Report/GetChartTable", objParams);

                    result = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
            return Json(new { isSuccess = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add By Nandish Shah
        /// Get Report table Data
        /// </summary>
        public async Task<string> GetReportTable(int Id, string DbName, string Container, string[] SDV, bool TopOnly = true, string ViewBy = "Q", string StartDate = "01/01/1900", string EndDate = "01/01/2100", int DashboardId = 0, int DashboardPageid = 0, int DashboardContentId = 0)
        {
          

            string AuthorizedReportAPIUserName = string.Empty;
            string AuthorizedReportAPIPassword = string.Empty;
            string ApiUrl = string.Empty;
            string ConnectionString = string.Empty;
            Sessions.ViewByValue = ViewBy;
            if(SDV!=null)
            {
                if(SDV.Count()==1)
                {
                    if(string.IsNullOrEmpty(SDV[0]))
                    {
                        SDV = null;
                    }
                }
            }
            if (!string.IsNullOrEmpty(DbName) && DbName == Convert.ToString(Enums.ApplicationCode.RPC))
            {
                // Get Measure Connection String
                ConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
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
                    ConnectionString = ConnectionString + " multipleactiveresultsets=True;";
                    ReportTableParameters objParams = new ReportTableParameters();
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
                    objParams.DashboardId = DashboardId;
                    objParams.DashboardPageid = DashboardPageid;
                    objParams.DashboardContentId = DashboardContentId;
                    objParams.UserId = Convert.ToString(Sessions.User.UserId);
                    objParams.RoleId = Convert.ToString(Sessions.User.RoleId);

                    HttpResponseMessage response = await client.PostAsJsonAsync("api/Report/GetReportTable ", objParams);
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
            return string.Empty;
        }
     
        
        /// <summary>
        /// Date:25/11/2016 #2819 Following method is created to load _ViewAllReportTable partial view(pop-up)
        /// </summary>
      
        public async  Task<PartialViewResult> LoadReportTablePartial(int Id, string Container, string[] SDV, string ViewBy = "Q", string StartDate = "01/01/1900", string EndDate = "01/01/2100", int DashboardId = 0, int DashboardPageid = 0, int DashboardContentId = 0, string DisplayName = "")     
        {
            ReportTableParameters objReportTable = new ReportTableParameters();
            objReportTable.Id = Id;
            objReportTable.DisplayName = DisplayName;
            objReportTable.Container = Container;
            //SDV is parameter to pass selected filter
            objReportTable.SDV = SDV;
            objReportTable.TopOnly = false;
            objReportTable.ViewBy = ViewBy;
            objReportTable.StartDate = StartDate;
            objReportTable.EndDate = EndDate;
            objReportTable.DashboardId = DashboardId;
            objReportTable.DashboardPageid = DashboardPageid;
            objReportTable.DashboardContentId = DashboardContentId;
            await Task.Delay(1);
            return  PartialView("_ViewAllReportTable", objReportTable);

        }
        /// <summary>
        /// Date:30/11/2016 #2818 Following method is created to load _ViewAllReportGraph partial view(pop-up) for chart
        /// </summary>
        public async Task<PartialViewResult> LoadChartTablePartial(int Id, string DbName, string Container, string[] SDV, bool TopOnly = true, string ViewBy = "Q", string StartDate = "01/01/1900", string EndDate = "01/01/2100", string DisplayName = "", int DashboardId = 0, int DashboardPageid = 0, int DashboardContentId = 0, string Customquery = "", string Charttype = "")
        {
            ReportParameters objReportTable = new ReportParameters();
            objReportTable.Id = Id;
            objReportTable.DisplayName = DisplayName;
            objReportTable.Container = Container;
            //SDV is parameter to pass selected filter
            objReportTable.SDV = SDV;
            objReportTable.TopOnly = false;
            objReportTable.ViewBy = ViewBy;
            objReportTable.StartDate = StartDate;
            objReportTable.EndDate = EndDate;

            objReportTable.DashboardContentId = DashboardContentId;
            objReportTable.HelpTextId = 0;
            objReportTable.DashboardId = DashboardId;
            objReportTable.IsSortByValue = false;
            objReportTable.SortOrder = "asc";
            objReportTable.DisplayName = DisplayName;
            objReportTable.CustomQuery = Customquery;
            objReportTable.ChartType = Charttype;
            await Task.Delay(1);
            return PartialView("_ViewAllReportGraph", objReportTable);

        }

        /// <summary>
        /// Action method to load DrillDownDetail partial view
        /// </summary>
        /// //Start- By Nandish Shah For Ticket #2820
        public async Task<PartialViewResult> LoadDrillDownData(string DbName, int Id, string DisplayName, string DimensionValueName, string DimensionValueCount, string DimensionActualValueCount, string HeaderDimensionValue, string DashboardContentId, string DashboardId, string MeasureName, string childchartid, string CustomQuery, int HelpTextId = 0, bool IsSortByValue = false, string SortOrder = "desc", string ChartType = "")
        {
            DrillDownDetails objDrillDownDetails = new DrillDownDetails();
            objDrillDownDetails.ChartId = Id;
            objDrillDownDetails.DisplayName = DisplayName;
            objDrillDownDetails.DimensionValueName = DimensionValueName;
            objDrillDownDetails.DimensionValueCount = DimensionValueCount;
            objDrillDownDetails.DimensionActualValueCount = DimensionActualValueCount;
            objDrillDownDetails.HeaderDimensionValue = HeaderDimensionValue;
            objDrillDownDetails.childchartid = childchartid;
            int _dashboardContentId;
            int.TryParse(DashboardContentId, out _dashboardContentId);
            objDrillDownDetails.DashboardContentId = _dashboardContentId;
            int _dashboardId;
            int.TryParse(DashboardId, out _dashboardId);
            objDrillDownDetails.DashboardId = _dashboardId;
            objDrillDownDetails.MeasureName = MeasureName;
            if (string.IsNullOrEmpty(CustomQuery))
            {
                CustomQuery = "0";
            }
            objDrillDownDetails.CustomQuery = CustomQuery;
            objDrillDownDetails.HelpTextId = HelpTextId;
            objDrillDownDetails.IsSortByValue = IsSortByValue;
            objDrillDownDetails.SortOrder = SortOrder;
            objDrillDownDetails.ChartType = ChartType;

            string AuthorizedReportAPIUserName = string.Empty;
            string AuthorizedReportAPIPassword = string.Empty;
            string ApiUrl = string.Empty;
            string ConnectionString = string.Empty;
            if (!string.IsNullOrEmpty(DbName) && DbName == Convert.ToString(Enums.ApplicationCode.RPC))
            {
                // Get Measure Connection String
                ConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
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
                    ConnectionString = ConnectionString + " multipleactiveresultsets=True;";
                    ReportTableParameters objParams = new ReportTableParameters();
                    objParams.DashboardId = Convert.ToInt32(DashboardId);
                    objParams.DashboardContentId = objDrillDownDetails.DashboardContentId;
                    objParams.UserName = AuthorizedReportAPIUserName;
                    objParams.Password = AuthorizedReportAPIPassword;
                    objParams.ConnectionString = ConnectionString;


                    HttpResponseMessage response = await client.PostAsJsonAsync("api/Report/LoadDrillDownData ", objParams);

                    if (response != null && response.Content.ReadAsStringAsync().Result != null)
                    {
                        List<SelectListItem> lstDispBy = JsonConvert.DeserializeObject<List<SelectListItem>>(response.Content.ReadAsStringAsync().Result);
                        ViewData["DisplayBy"] = lstDispBy;
                    }
                }
                catch (Exception ex)
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return PartialView("_DrillDownDetails", objDrillDownDetails);
        }

        /// <summary>
        /// Action method to load drill deeper data
        /// <returns></returns>
        public async Task<ActionResult> GetDrillDownReportTable(string[] SelectedOthersDimension, string[] SelectedDimensionValue, string DbName, int ChartId, string DimensionValueName, string HeaderDimensionValue, string DisplayBy, string SortBy, string SortDirection, int PageIndex, int PageSize, string mTotalRecords, string MeasureName, int ReportDashboardID = 0, string childchartid = "0", string CustomQuery = "")
        {
            try
            {
                string _DrillDownConfigExists = string.Empty;
                string[] DimensionValues = new string[] { System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(DimensionValueName)), HeaderDimensionValue };
                int _TotalReocrds = 0;
                int _NoOfPrimaryCols = 0;
                string htmlTable = string.Empty;
                string AuthorizedReportAPIUserName = string.Empty;
                string AuthorizedReportAPIPassword = string.Empty;
                string ApiUrl = string.Empty;
                string ConnectionString = string.Empty;
                if (!string.IsNullOrEmpty(DbName) && DbName == Convert.ToString(Enums.ApplicationCode.RPC))
                {
                    // Get Measure Connection String
                    ConnectionString = Sessions.User.UserApplicationId.Where(o => o.ApplicationTitle == Enums.ApplicationCode.RPC.ToString()).Select(o => o.ConnectionString).FirstOrDefault();
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
                        ConnectionString = ConnectionString + " multipleactiveresultsets=True;";

                        string DimensionTable = string.Empty;
                        List<string> sDimensionTable = new List<string>();
                        if (SelectedDimensionValue != null && SelectedDimensionValue.Length > 0)
                        {
                            foreach (string s in SelectedDimensionValue)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    if (!sDimensionTable.Contains(s))
                                    {
                                        sDimensionTable.Add(s);
                                    }
                                }
                            }
                        }
                        string[] sSelectedOthersDimension = new string[] { };
                        if (SelectedOthersDimension != null && SelectedOthersDimension.Length > 0)
                        {
                            sSelectedOthersDimension = SelectedOthersDimension.ToArray();
                        }
                        
                        if (sSelectedOthersDimension != null)
                        {
                            if (sSelectedOthersDimension.Length >= 1)
                            {
                                Array.Sort(sSelectedOthersDimension);

                                foreach (var item in sSelectedOthersDimension)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                        DimensionTable += "D" + item;
                                }
                            }
                        }

                        string SubDashboardOtherDimensionTable = string.Empty;
                        string[] SelectedDimension = new string []{ };
                        if (DimensionValues != null)
                        {
                            if (!string.IsNullOrEmpty(DimensionTable))
                            {
                                SubDashboardOtherDimensionTable = DimensionTable.Replace("D", "");
                            }
                            if (sDimensionTable != null && sDimensionTable.Count > 0)
                            {
                                SelectedDimension = sDimensionTable.ToArray();
                            }                            
                        }
                        else if (SelectedOthersDimension.Count() == 1)
                        {
                            if (SelectedOthersDimension != null && SelectedOthersDimension.Length > 0)
                            {
                                SubDashboardOtherDimensionTable = SelectedOthersDimension[0].ToString().Replace("D", "");
                            }
                            SelectedDimension = null;
                        }

                        string ViewBy = Sessions.ViewByValue;

                        DrillDownParameters objParams = new DrillDownParameters();
                        objParams.ChartId = ChartId;
                        objParams.DimensionValues = DimensionValues;
                        objParams.DimensionValueName = DimensionValueName;
                        objParams.HeaderDimensionValue = HeaderDimensionValue;
                        objParams.DisplayBy = DisplayBy;
                        objParams.ViewByValue = ViewBy;
                        objParams.SortBy = SortBy;
                        objParams.SortDirection = SortDirection;
                        objParams.PageIndex = PageIndex;
                        objParams.PageSize = PageSize;
                        objParams.mTotalRecords = mTotalRecords;
                        objParams.MeasureName = MeasureName;
                        objParams.childchartid = childchartid;
                        objParams.CustomQuery = CustomQuery;
                        objParams.DimensionTable = DimensionTable;
                        objParams.SelectedDimension = SelectedDimension;
                        objParams.StartDate = Sessions.StartDate;
                        objParams.EndDate = Sessions.EndDate;
                        objParams.DashboardId = ReportDashboardID;
                        objParams.DashboardPageId = 0;
                        objParams.UserName = AuthorizedReportAPIUserName;
                        objParams.Password = AuthorizedReportAPIPassword;
                        objParams.ConnectionString = ConnectionString;

                        HttpResponseMessage response = await client.PostAsJsonAsync("api/Report/GetDrillDownReportTable ", objParams);
                        List<string> lstDrillData = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
                        htmlTable = lstDrillData[0];
                        _DrillDownConfigExists = lstDrillData[1];
                        _NoOfPrimaryCols = Convert.ToInt32(lstDrillData[2]);
                        _TotalReocrds = Convert.ToInt32(lstDrillData[3]);
                    }
                    catch (Exception ex)
                    {
                        ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                }
                return Json(new { isSuccess = true, HtmlTable = htmlTable, TotalRecords = _TotalReocrds, DrillDownConfigExists = _DrillDownConfigExists, NoOfPrimaryCols = _NoOfPrimaryCols }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return Json(new { isSuccess = false, HtmlTable = "Error" }, JsonRequestBehavior.AllowGet);

            }
        }

    }
}
