using Integration.Helper;
using Newtonsoft.Json;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/AuthenticateMarketo", marketoCredentialDictionary).Result;
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
            HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/GetProgramFieldsFromMarketo", marketoCredentialDictionary).Result;
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
                HttpResponseMessage response = client.PostAsJsonAsync("api/Integration/Get_Program_Channel_List", marketoCredentialDictionary).Result;
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
