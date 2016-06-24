using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.WorkFront
{


    /// <summary>
    /// Handles the client for API calls. Creates the client, maintains session ID for api authorization,
    /// verifies sign in, handles user ID, client login and logout. 
    /// Adds layer of abstraction from API call verbs -> Allows for search, create, update, etc.
    /// Packs information needed for API calls and sends to the appropriate API verb methods in the client.
    /// IDisposable allows for Disposal calls on an object if desired instead of waiting for garbage collector.
    /// Class throws ClientException exceptions.
    /// </summary>
    class ClientHandling : IDisposable
    {

        private JToken sessionResponse;
        private Client client;

        /// <summary>
        /// ClientHandling Contructor given apiPath as parameter.
        /// Verifies API path provided (though not necessarily correct) and creates a new client with said path.
        /// </summary>
        /// <param name="apiPath">
        /// The URL to the api of the WorkFront API.
        /// For example: "http://yourcompany.attask-ondemand.com/attask/api"
        /// </param>
        public ClientHandling(string apiPath)
        {
            if (string.IsNullOrEmpty(apiPath))
            {
                throw new ClientException("apiPath cannot be null or empty");
            }
            if (apiPath.EndsWith("/"))
            {
                apiPath = apiPath.Substring(0, apiPath.Length - 1);
            }
            this.client = new Client(apiPath);
        }

        /// <summary>
        /// Gets if the instance of the Client has successfully logged in
        /// </summary>
        /// <return>
        /// boolean: True if sessionresponse isn't null (will be null if user isn't signed in).
        /// </return>
        public bool IsSignedIn
        {
            get { return sessionResponse != null; }
        }

        /// <summary>
        /// Gets the session id returned by the login command
        /// If sessionResponse is null, get null; else set the sessionRepsonse value to "sessionID"
        /// This session ID is used for API verification & authorization.
        /// </summary>
        /// <return>
        /// Session ID as a string.
        /// </return>
        public string SessionID
        {
            get
            {
                if (sessionResponse == null)
                {
                    return null;
                }
                else { return sessionResponse.Value<string>("sessionID"); }
            }
        }

        /// <summary>
        /// Gets the ID of the user that is currently logged in
        /// </summary>
        /// <return>
        /// user ID, according to WorkFront, as a string.
        /// </return>
        public string UserID
        {
            get
            {
                if (sessionResponse == null)
                {
                    return null;
                }
                else { return sessionResponse.Value<string>("userID"); }
            }
        }

        /// <summary>
        /// Logs in as the given user. ClientHandling tracks the session for you so
        /// you do not need to specify the sessionID in the parameters of other commands called by ClientHandling.
        /// Throws a ClientException if you are already logged in.
        /// </summary>
        /// <param name="username">
        /// Username for the client handling for Workfront login
        /// </param>
        /// <param name="password">
        /// Password for client handling to login to Workfront
        /// </param>
        public void Login(string username, string password)
        {
            if (IsSignedIn)
            {
                return;
            }
            JToken json = client.DoPost("/login", "login", "username=" + username, "password=" + password); //sessionID not important at login, hence "login" for sessionID
            if (json == null)
            {
                throw new ClientException("No data retrieved from Server. Please check login information");
            }
            else { sessionResponse = json["data"]; }
        }

        /// <summary>
        /// Clears your current session.
        /// Throws an ClientException if you are not logged in.
        /// </summary>
        public void Logout()
        {
            if (!IsSignedIn)
            {
                throw new ClientException("Cannot log out: not signed in.");
            }
            client.DoPost("/logout", "sessionID=" + SessionID);
            sessionResponse = null;
        }

        /// <summary>
        /// If you are logged in, Dispose will attempt to log you out
        /// </summary>
        public void Dispose()
        {
            if (IsSignedIn)
            {
                this.Logout();
            }
        }

        /// <summary>
        /// Gets the object of the given ObjCode and the given id
        /// </summary>
        /// <param name="objcode">
        /// A <see cref="ObjCode"/> representing the type of object you are getting
        /// </param>
        /// <param name="id">
        /// String representing the ID of the object you are getting
        /// </param>
        /// <param name="fieldsToInclude">
        /// The name of the fields to include in the results
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/>
        /// </returns>
        public JToken Get(ObjCode objcode, string id, params string[] fieldsToInclude)
        {
            VerifySignedIn();
            List<string> parameters = new List<string>();
            StringBuilder sb = new StringBuilder();
            if (fieldsToInclude != null && fieldsToInclude.Length > 0)
            {
                fieldsToInclude.ToList().ForEach(s => sb.Append(s).Append(","));
                sb.Remove(sb.Length - 1, 1);
                string fields = "fields=" + sb.ToString();
                parameters.Add(fields);
            }
            JToken json = client.DoGet(string.Format("/{0}/{1}", objcode, id), SessionID, parameters.ToArray());
            return json;
        }

        /// <summary>
        /// Searches on the given ObjCode.
        /// </summary>
        /// <param name="objcode">
        /// A <see cref="ObjCode"/>
        /// </param>
        /// <param name="parameters">
        /// A <see cref="System.String[]"/>. Parameters included in the search.
        /// For example:
        /// "name=MyTask"
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/>
        /// </returns>
        public JToken Search(ObjCode objcode, object parameters)
        {
            VerifySignedIn();
            string[] p = parameterObjectToStringArray(parameters);
            JToken json = client.DoGet(string.Format("/{0}/search", objcode), SessionID, p);
            return json;
        }

        /// <summary>
        /// Creates a new object of the given type.
        /// </summary>
        /// <param name="objcode">
        /// ObjCode that should be limited to those listed in ObjCode class.
        /// </param>
        /// <param name="parameters">
        /// Additional parameters to be included and depend on the object type certain parameters are requiredin question.
        /// For example: 'projectID=1234&fields=updates:*' can be packed together as parameters
        /// </param>
        /// <returns>
        /// A JToken of whatever was just created (project, issue, etc)
        /// </returns>
        public JToken Create(ObjCode objcode, object parameters)
        {
            VerifySignedIn();
            string[] p = parameterObjectToStringArray(parameters);
            JToken json = client.DoPost(string.Format("/{0}", objcode), SessionID, p);
            return json;
        }

        /// <summary>
        /// Updates an object that already exists.
        /// </summary>
        /// <param name="objcode">
        /// The ObjCode of the object to update
        /// </param>
        /// <param name="parameters">
        /// Additional parameters of the object to update.
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/>
        /// </returns>
        public JToken Update(ObjCode objcode, object parameters)
        {
            try
            {
                VerifySignedIn();
                string[] p = parameterObjectToStringArray(parameters);
                JToken json = client.DoPut(string.Format("/{0}", objcode), SessionID, p);
                return json; //JSON will be null if updates fail
            }
            catch
            {
                // Modified by Arpita Soni for Ticket #2304 on 06/24/2016
                if (objcode != null && objcode.Value.Equals(ObjCode.PROJECT.Value))
                {
                    throw new ClientException("Though tactic is pushed successfully, error in updating attributes of tactic.");
                }
                else
                {
                    throw new ClientException("Error updating " + objcode + " in Section: Update");
                }
            }

        }

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="objcode">
        /// ObjCode of the object to be deleted.
        /// </param>
        /// <param name="parameters">
        /// Object of parameters that will be converted to a string array
        /// </param>
        /// <returns>
        /// JToken of JSON data returned by the server after a delete method API call.
        /// Example: { "data": { "success": true }}
        /// </returns>
        public JToken Delete(ObjCode objcode, object parameters)
        {
            VerifySignedIn();
            string[] p = parameterObjectToStringArray(parameters);
            JToken json = client.DoDelete(string.Format("/{0}", objcode), SessionID, p);
            return json;
        }

        /// <summary>
        /// Throws an exception if the client isn't logged in
        /// </summary>
        private void VerifySignedIn()
        {
            if (!IsSignedIn)
            {
                throw new ClientException("You must be signed in");
            }
        }

        /// <summary>
        /// Converts an Object to a String array. Takes in all the given parameters in 
        /// the object and converts to / returns a string array that can be used for Client api calls.
        /// Originally used (additionally) to add other, last minute, items to the parameter list.
        /// </summary>
        /// <param name="parameters">
        /// The Object of parameters.
        /// </param>
        /// <returns>
        /// String array of parameters for easier insertion into the object URI
        /// </returns>
        private string[] parameterObjectToStringArray(object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            List<string> p = new List<string>(properties.Length);
            foreach (var prop in properties)
            {
                string line = string.Format("{0}={1}", prop.Name, prop.GetValue(parameters, null));
                p.Add(line);
            }
            return p.ToArray();
        }
    }
}
