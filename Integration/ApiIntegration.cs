using Integration.Helper;
using Newtonsoft.Json;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Integration
{
    public class ReturnObject
    {
        public bool status { get; set; }
        public List<LogDetails> lstLogDetails;
        public Dictionary<string, string> data;
    }

    public class LogDetails
    {
        public int? SourceId { get; set; }
        public int? ProgramId { get; set; }
        public DateTime StartTimeStamp { get; set; }
        public DateTime EndTimeStamp { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Mode { get; set; }
    }
    public class Parameters
    {
        public Dictionary<string, string> credentials { get; set; }       
        public string applicationId { get; set; }
        public string clientId { get; set; }
        public List<fieldMapping> fieldMapList;
        public string spName { get; set; }
        public List<SpParameters> lstPramtereLilst;
    }

    public class fieldMapping
    {
        public string sourceFieldName { get; set; }
        public string destinationFieldName { get; set; }
        public string marketoFieldType { get; set; }
    }
    public class ApiIntegration
    {
        string _TypeofData;
        private MRPEntities db = new MRPEntities();
        public string _host { get; set; }
        public string _clientid { get; set; }
        public string _clientsecret { get; set; }
        public string _marketoToken { get; set; }
        private bool _isAuthenticated { get; set; }
        private int? _integrationInstanceId { get; set; }
        private int _id { get; set; }
        private Guid _userId { get; set; }
        private int _integrationInstanceLogId { get; set; }
        private EntityType _entityType { get; set; }
        Guid _applicationId = Guid.Empty;
        public string _ErrorMessage { get; set; }

        public ApiIntegration(string Dropdowntype, int? integrationInstanceId)
        {
            _TypeofData = Dropdowntype;
            _integrationInstanceId = integrationInstanceId;
            SetIntegrationInstanceDetail();

        }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }

        public ApiIntegration()
        {
        }


        public ApiIntegration(int integrationInstanceId, int id, EntityType entityType, Guid userId, int integrationInstanceLogId, Guid applicationId)
        {
            _integrationInstanceId = integrationInstanceId;
            _id = id;
            _entityType = entityType;
            _userId = userId;
            _integrationInstanceLogId = integrationInstanceLogId;
            _applicationId = applicationId;

            SetIntegrationInstanceDetail();

            //this.AuthenticateforMarketo();
        }

        private void SetIntegrationInstanceDetail()
        {
            string ClientId = "ClientId";
            string ClientSecret = "ClientSecret";

            IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();
            Dictionary<string, string> attributeKeyPair = db.IntegrationInstance_Attribute.Where(attribute => attribute.IntegrationInstanceId == _integrationInstanceId).Select(attribute => new { attribute.IntegrationTypeAttribute.Attribute, attribute.Value }).ToDictionary(attribute => attribute.Attribute, attribute => attribute.Value);

            this._clientid = attributeKeyPair[ClientId];
            this._clientsecret = attributeKeyPair[ClientSecret];
            this._host = integrationInstance.IntegrationType.APIURL;
            if (HttpContext.Current.Session["MarketoToken"] != null)
            {
                this._marketoToken = HttpContext.Current.Session["MarketoToken"].ToString();
            }
            else
            {
                _marketoToken = "";
            }
        }

        public void AuthenticateforMarketo()
        {
            if (HttpContext.Current.Session["MarketoToken"] != null)
            {
                _marketoToken = HttpContext.Current.Session["MarketoToken"].ToString();
            }
            else
            {
                _marketoToken = "";
            }
            Dictionary<string, string> marketoCredentialDictionary = new Dictionary<string, string>();
            marketoCredentialDictionary.Add("host", _host);
            marketoCredentialDictionary.Add("clientid", _clientid);
            marketoCredentialDictionary.Add("clientsecret", _clientsecret);
            marketoCredentialDictionary.Add("token", _marketoToken);
            HttpClient client = new HttpClient();
            string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(marketoIntegrstionApi);
            //Uri baseAddress = new Uri("http://121.244.200.162:8085/IntegrationApi/");

            client.BaseAddress = baseAddress;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Marketo_Authentication", marketoCredentialDictionary).Result;
            if (response.IsSuccessStatusCode)
            {
                ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                if (objData.status == true)
                {
                    _isAuthenticated = true;
                    HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();

                }
                else
                {
                    _isAuthenticated = false;
                    //_ErrorMessage = objData.lstLogDetails.Select(err => err.Description.ToString()).FirstOrDefault();
                }
            }

        }

        public Dictionary<string, string> GetTargetCustomTags()
        {
            Dictionary<string, string> ListOfData = new Dictionary<string, string>();

            Dictionary<string, string> marketoCredentialDictionary = new Dictionary<string, string>();
            marketoCredentialDictionary.Add("host", _host);
            marketoCredentialDictionary.Add("clientid", _clientid);
            marketoCredentialDictionary.Add("clientsecret", _clientsecret);
            marketoCredentialDictionary.Add("token", _marketoToken);

            HttpClient client = new HttpClient();
            string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(marketoIntegrstionApi);
            //Uri baseAddress = new Uri("http://121.244.200.162:8085/IntegrationApi/");
            client.BaseAddress = baseAddress;
            HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Marketo_GetProgramFields", marketoCredentialDictionary).Result;
            if (response.IsSuccessStatusCode)
            {
                ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                ListOfData = objData.data;
                HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();
            }
            return ListOfData;
        }

        /// <summary>
        /// Added by Komal Rawal
        /// 13-05-2016
        /// To Get data with Api for dropdowns.
        /// </summary>
        public MarketoDataObject GetProgramChannellistData()
        {
            MarketoDataObject ListOfData = new MarketoDataObject();
            if (_TypeofData == Enums.ApiIntegrationData.Progrmatype.ToString())
            {
                if (HttpContext.Current.Session["MarketoToken"] != null)
                {
                    _marketoToken = HttpContext.Current.Session["MarketoToken"].ToString();
                }
                else
                {
                    _marketoToken = "";
                }
                Dictionary<string, string> marketoCredentialDictionary = new Dictionary<string, string>();
                marketoCredentialDictionary.Add("host", _host);
                marketoCredentialDictionary.Add("clientid", _clientid);
                marketoCredentialDictionary.Add("clientsecret", _clientsecret);
                marketoCredentialDictionary.Add("token", _marketoToken);
                //    marketoCredentialDictionary.Add("rootfolder", "Marketing Activities");


                HttpClient client = new HttpClient();
                string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
                Uri baseAddress = new Uri(marketoIntegrstionApi);
                client.BaseAddress = baseAddress;
                HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Marketo_Get_Program_Channels", marketoCredentialDictionary).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseobj = response.Content.ReadAsStringAsync().Result;
                    MarketoDataObject objData = JsonConvert.DeserializeObject<MarketoDataObject>(responseobj);
                    HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();
                    ListOfData = objData;
                }
            }
            return ListOfData;
        }

        /// <summary>
        /// Created By Nishant Sheth
        /// Created Date : 21-May-2016
        /// Get list of marketo campaign folder with api
        /// </summary>
        /// <returns></returns>
        public ReturnObject GetMarketoCampaignFolderList()
        {
            ReturnObject ListOfData = new ReturnObject();
            if (_TypeofData == Enums.ApiIntegrationData.CampaignFolderList.ToString())
            {
                if (HttpContext.Current.Session["MarketoToken"] != null)
                {
                    _marketoToken = HttpContext.Current.Session["MarketoToken"].ToString();
                }
                else
                {
                    _marketoToken = "";
                }
                Dictionary<string, string> marketoCredentialDictionary = new Dictionary<string, string>();
                marketoCredentialDictionary.Add("host", _host);
                marketoCredentialDictionary.Add("clientid", _clientid);
                marketoCredentialDictionary.Add("clientsecret", _clientsecret);
                marketoCredentialDictionary.Add("token", _marketoToken);
                marketoCredentialDictionary.Add("rootfolder", "Marketing Activities");


                HttpClient client = new HttpClient();
                string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
                Uri baseAddress = new Uri(marketoIntegrstionApi);
                // Uri baseAddress = new Uri("http://121.244.200.162:8085/IntegrationApi/");
                client.BaseAddress = baseAddress;
                HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Marketo_GetFolders_MarketingActivity", marketoCredentialDictionary).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseobj = response.Content.ReadAsStringAsync().Result;
                    ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(responseobj);
                    HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();
                    ListOfData = objData;
                }
            }
            return ListOfData;
        }

        //Added by Rahul Shah for PL #2194 
        public List<LogDetails> MarketoData_Push(string spName, List<fieldMapping> lstFieldsMap, Guid _clientId, List<SpParameters> spParams)
        {
            Parameters objParams = new Parameters();
            List<LogDetails> logdetails = new List<LogDetails>();
            Dictionary<string, string> marketoCredentialDictionary = new Dictionary<string, string>();
            marketoCredentialDictionary.Add("host", _host);
            marketoCredentialDictionary.Add("clientid", _clientid);
            marketoCredentialDictionary.Add("clientsecret", _clientsecret);
            marketoCredentialDictionary.Add("token", _marketoToken);
            objParams.credentials = marketoCredentialDictionary;
            objParams.applicationId = _applicationId.ToString();
            objParams.clientId = _clientId.ToString();
            objParams.fieldMapList = lstFieldsMap;
            objParams.spName = spName;
            objParams.lstPramtereLilst = spParams;
            HttpClient client = new HttpClient();
            string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(marketoIntegrstionApi);
            //Uri baseAddress = new Uri("http://localhost:54371/");
            client.BaseAddress = baseAddress;
            HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Marketo_PushMarketoPrograms ", objParams).Result;
            ReturnObject ro = new ReturnObject();
            if (response.IsSuccessStatusCode)
            {
                ReturnObject obj2 = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);

                logdetails = obj2.lstLogDetails;
                //string json = JsonConvert.DeserializeObject(response.);

            }
            return logdetails;
        }
    }




    public class MarketoDataObject
    {
        public bool status { get; set; }
        public List<LogDetails> lstLogDetails;
        public List<channel> channels;
        public Dictionary<string, string> program;
    }

    public class channel
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string applicableProgramType { get; set; }

    }



}
