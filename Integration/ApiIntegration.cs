using Integration.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevenuePlanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Dynamic;
using System.IO;
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
        public List<SalesForceFieldsDetails> salesForceFieldsDetails = new List<SalesForceFieldsDetails>();
        public List<LogDetails> logs = new List<LogDetails>();
        public List<SFDCAPICampaignResult> campaignIds = new List<SFDCAPICampaignResult>();
    }
    public class SalesForceFieldsDetails
    {
        public bool Custom { get; set; }
        public string DefaultValue { get; set; }
        public bool AutoNumber { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public string Type { get; set; }
        public string SoapType { get; set; }
    }
    public class LogDetails
    {
        public int? SourceId { get; set; }
        public int? EntityId { get; set; }
        public DateTime StartTimeStamp { get; set; }
        public DateTime EndTimeStamp { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Mode { get; set; }
        public string Url { get; set; }
        public string ObjectType { get; set; }
    }
    public class Parameters
    {
        public Dictionary<string, string> credentials { get; set; }       
        public string applicationId { get; set; }
        public string clientId { get; set; }
        public List<fieldMapping> fieldMapList;
        public string spName { get; set; }
        public List<SpParameters> lstParameterList;
    }
    public class SFDCAPICampaignResult
    {
        public string SourceId { get; set; }
        public string CampaignId { get; set; }
        public string ObjectType { get; set; }
        public string Description { get; set; }
        public string Mode { get; set; }
        public DateTime EndTimeStamp { get; set; }
    }
    public class fieldMapping
    {
        public string sourceFieldName { get; set; }
        public string destinationFieldName { get; set; }
        public string marketoFieldType { get; set; }
        public string fieldType { get; set; }
    }
    public class ApiIntegration
    {
        string _TypeofData;
        string strToken = Enums.MarketoAPIEventNames.Token.ToString();
        string strAuthentication = Enums.MarketoAPIEventNames.Authentication.ToString();
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
            if (integrationInstanceId > 0)
            {
            SetIntegrationInstanceDetail();
            }

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
            
            //IntegrationInstance integrationInstance = db.IntegrationInstances.Where(instance => instance.IntegrationInstanceId == _integrationInstanceId).FirstOrDefault();// Commented By Nishant Sheth // Host URL will be diffrent for every client
            Dictionary<string, string> attributeKeyPair = db.IntegrationInstance_Attribute.Where(attribute => attribute.IntegrationInstanceId == _integrationInstanceId).Select(attribute => new { attribute.IntegrationTypeAttribute.Attribute, attribute.Value }).ToDictionary(attribute => attribute.Attribute, attribute => attribute.Value);

            if (attributeKeyPair.Count > 0) //Modified By komal to handle error while it there is no data
            {
            this._clientid = attributeKeyPair[ClientId];
            this._clientsecret = attributeKeyPair[ClientSecret];
            // Add By Nishant Sheth // Host URL will be diffrent for every client
            this._host = attributeKeyPair[Enums.IntegrationTypeAttribute.Host.ToString()];
            }
            else
            {
                this._clientid = null;
                this._clientsecret = null;
            }
            //this._host = integrationInstance.IntegrationType.APIURL;// Commented By Nishant Sheth // Host URL will be diffrent for every client
            if (HttpContext.Current != null && HttpContext.Current.Session["MarketoToken"] != null)
            {
                this._marketoToken = HttpContext.Current.Session["MarketoToken"].ToString();
            }
            else
            {
                _marketoToken = "";
            }
        }
        /// <summary>
        /// Added by Rahul Shah
        /// 21-05-2016
        /// to check the authentication for Marketo
        /// </summary>
        public void AuthenticateforMarketo()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session["MarketoToken"] != null)
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
            if (marketoIntegrstionApi != null) {
                Uri baseAddress = new Uri(marketoIntegrstionApi);
                client.BaseAddress = baseAddress;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpResponseMessage response = client.PostAsJsonAsync("api/Marketo/Marketo_Authentication", marketoCredentialDictionary).Result;
                if (response.IsSuccessStatusCode)
                {
                    ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                    if (objData.status == true)
                    {
                        _isAuthenticated = true;
                        if (HttpContext.Current != null)
                        {
                            HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("strToken")).Select(th => th.Description).FirstOrDefault();
                        }

                    }
                    else
                    {
                        _isAuthenticated = false;
                        //_ErrorMessage = objData.lstLogDetails.Select(err => err.Description.ToString()).FirstOrDefault();
                    }
                }
            }
        }
        /// <summary>
        /// Created By Rahul Shah
        /// Created Date : 22-May-2016
        /// Get list of marketo custom tag with api
        /// </summary>
        /// <returns></returns>
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
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpResponseMessage response = client.PostAsJsonAsync("api/Marketo/Marketo_GetProgramFields", marketoCredentialDictionary).Result;
            if (response.IsSuccessStatusCode)
            {
                ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                ListOfData = objData.data;
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("strToken")).Select(th => th.Description).FirstOrDefault();
                }
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
            if (_TypeofData == Enums.ApiIntegrationData.Programtype.ToString())
            {
                if (HttpContext.Current != null && HttpContext.Current.Session["MarketoToken"] != null)
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
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpResponseMessage response = client.PostAsJsonAsync("api/Marketo/Marketo_Get_Program_Channels", marketoCredentialDictionary).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseobj = response.Content.ReadAsStringAsync().Result;
                    MarketoDataObject objData = JsonConvert.DeserializeObject<MarketoDataObject>(responseobj);
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();
                    }
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
                if (HttpContext.Current != null && HttpContext.Current.Session["MarketoToken"] != null)
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
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpResponseMessage response = client.PostAsJsonAsync("api/Marketo/Marketo_GetFolders_MarketingActivity", marketoCredentialDictionary).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseobj = response.Content.ReadAsStringAsync().Result;
                    ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(responseobj);
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Session["MarketoToken"] = objData.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals("Token")).Select(th => th.Description).FirstOrDefault();
                    }
                    ListOfData = objData;
                }
            }
            return ListOfData;
        }

        /// <summary>
        /// Created By Rahul Shah
        /// Created Date : 24-May-2016
        /// to push the data to the marketo and get the loglist from marketo
        /// </summary>
        /// <returns></returns>
        public List<LogDetails> MarketoData_Push(string spName, List<fieldMapping> lstFieldsMap, Guid _clientId, List<SpParameters> spParams)
        {
            
            bool isAuthenticate = false;
            string strwebAPITimeout = System.Configuration.ConfigurationManager.AppSettings["CommonIntegrationWebAPITimeOut"];
            int CommonWebAPITimeout = 0;
            if (!string.IsNullOrEmpty(strwebAPITimeout))
                CommonWebAPITimeout = Convert.ToInt32(strwebAPITimeout);
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
            objParams.lstParameterList = spParams;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(CommonWebAPITimeout);  //set timeout for Common Integration API call
            client.Timeout = TimeSpan.FromHours(3);  //set timeout for Common Integration API call
            string marketoIntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(marketoIntegrstionApi);
            //Uri baseAddress = new Uri("http://localhost:54371/");
            client.BaseAddress = baseAddress;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpResponseMessage response = client.PostAsJsonAsync("api/Marketo/Marketo_PushMarketoPrograms ", objParams).Result;
            ReturnObject ro = new ReturnObject();
            if (response.IsSuccessStatusCode)
            {
                ReturnObject obj2 = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                logdetails = obj2.lstLogDetails;
                isAuthenticate = obj2.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals(strAuthentication)).Select(th => th.Status).Any();
                if (isAuthenticate)                {
                    _isAuthenticated = true;
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Session["MarketoToken"] = obj2.lstLogDetails.Where(tkn => tkn.EventName.ToString().Equals(strToken)).Select(th => th.Description).FirstOrDefault();
                    }
                }
                

            }
            return logdetails;
        }


        #region " SFDC related API functions"

        /// <summary>
        /// Added by Viral
        /// Added On 13 June 2016
        /// to check the authentication for Saleforce
        /// </summary>
        public bool AuthenticateforSFDC(Dictionary<string,string> sfdcCredentials, string AppId,string clientID)
        {
            HttpClient client = new HttpClient();
            bool isAuthenticated = false;   
            string IntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            if (IntegrstionApi != null)
            {
                JObject obj = new JObject();
                obj.Add("credentials", JsonConvert.SerializeObject(sfdcCredentials));
                obj.Add("applicationId", AppId);
                obj.Add("clientId", clientID);

                Uri baseAddress = new Uri(IntegrstionApi);
                client.BaseAddress = baseAddress;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpResponseMessage response = client.PostAsJsonAsync("api/Sfdc/SalesForce_Authentication", obj).Result;
                if (response.IsSuccessStatusCode)
                {
                    ReturnObject objData = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                    isAuthenticated = objData.status;
                }
            }
            return isAuthenticated;
        }

        /// <summary>
        ///  Get list of Fields Name,Type, Length from SFDC.
        /// </summary>
        /// <param name="salesforceCredentials"></param>
        /// <returns></returns>
        public List<SalesForceFieldsDetails> GetSFDCmetaDataFields(Dictionary<string, string> salesforceCredentials,string AppId,string clientid)
        {
            string objectname = "Campaign";
            int CommonWebAPITimeout = 0;
            string strwebAPITimeout = System.Configuration.ConfigurationManager.AppSettings["CommonIntegrationWebAPITimeOut"];
            List<SalesForceFieldsDetails> lstSFDCFieldDetails = new List<SalesForceFieldsDetails>();
            JObject obj = new JObject();
            obj.Add("credentials", JsonConvert.SerializeObject(salesforceCredentials));
            obj.Add("applicationId", AppId);
            obj.Add("clientId", clientid);
            obj.Add("object", objectname);

            if (!string.IsNullOrEmpty(strwebAPITimeout))
                CommonWebAPITimeout = Convert.ToInt32(strwebAPITimeout);
            else
                CommonWebAPITimeout = 4;    // if user has not defined Integration WEB API timeout then set static value to 4.
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(CommonWebAPITimeout);  //set timeout for Common Integration API call
            string IntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(IntegrstionApi);
            client.BaseAddress = baseAddress;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpResponseMessage response = client.PostAsJsonAsync("api/Sfdc/SalesForce_ReadMetadataDetails", obj).Result;
            ReturnObject ro = new ReturnObject();
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                ReturnObject obj2 = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
                ro.status = obj2.status;
                //ro.logs = obj2.logs;
                lstSFDCFieldDetails = obj2.salesForceFieldsDetails;
            }
            return lstSFDCFieldDetails;

        }

        /// <summary>
        /// Created By Viral
        /// Created Date : 13-June-2016
        /// to push the data to the Salesforce and get the loglist from Salesforce
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage SFDCData_Push(string spName, List<fieldMapping> lstFieldsMap, string AppId, Guid _clientId, List<SpParameters> spParams, Dictionary<string, string> SFDCCredentials)
        {
            #region "Declare local varibles"
            int CommonWebAPITimeout = 0;
            string strwebAPITimeout = System.Configuration.ConfigurationManager.AppSettings["CommonIntegrationWebAPITimeOut"];
            Parameters objParams = new Parameters();
            List<LogDetails> logdetails = new List<LogDetails>();
            #endregion

            if (!string.IsNullOrEmpty(strwebAPITimeout))
                CommonWebAPITimeout = Convert.ToInt32(strwebAPITimeout);
            else
                CommonWebAPITimeout = 4;    // if user has not defined Integration WEB API timeout then set static value to 4.

            //objParams.credentials = SFDCCredentials;
            //objParams.applicationId = AppId;
            //objParams.clientId = _clientId.ToString();
            //objParams.fieldMapList = lstFieldsMap;
            //objParams.spName = spName;
            //objParams.lstParameterList = spParams;

            JObject obj = new JObject();
            obj.Add("credentials", JsonConvert.SerializeObject(SFDCCredentials));
            obj.Add("applicationId", AppId);
            obj.Add("clientId", _clientId.ToString());
            obj.Add("spName", spName);
            obj.Add("fieldMapList", JsonConvert.SerializeObject(lstFieldsMap));
            obj.Add("spParameterList", JsonConvert.SerializeObject(spParams));


            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(CommonWebAPITimeout);  //set timeout for Common Integration API call
            string IntegrstionApi = System.Configuration.ConfigurationManager.AppSettings.Get("IntegrationApi");
            Uri baseAddress = new Uri(IntegrstionApi);
            //Uri baseAddress = new Uri("http://localhost:54371/");
            client.BaseAddress = baseAddress;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = client.PostAsJsonAsync("api/Sfdc/SalesForce_PushObject", obj).Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //ReturnObject ro = new ReturnObject();
            //if (response.IsSuccessStatusCode)
            //    ro = JsonConvert.DeserializeObject<ReturnObject>(response.Content.ReadAsStringAsync().Result);
            return response;
        }

        #endregion
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
