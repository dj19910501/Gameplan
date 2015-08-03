using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Integration.WorkFront

{
    /// <summary>
    /// Client class creates the actual client with the client URL and controls the API 
    /// calls through use of HTTP verbs. API call made with DoRequest method. Specific 
    /// verbs used for the call is determined in other class methods, then sent accordingly 
    /// to the DoRequest method.
    /// </summary>
    public class Client
    {
        private string url;

        /// <summary>
        /// Creates a new Client that sends requests to the given root URL.
        /// </summary>
        /// <param name="url">
        /// A string of the url for the client. Ex: "http://yourcompany.attask-ondemand.com/attask/api"
        /// Note: sandbox should be used for testing purposes: "http://yourcompany.attasksandbox.com/attask/api"
        /// </param>
        public Client(string url)
        {
            if (url.EndsWith("/"))
            {
                this.url = url.Substring(0, url.Length - 1);
            }
            else
            {
                this.url = url;
            }
        }

        /// <summary>
        /// Main template used for requests (by default a GET request) to the given path with the given 
        /// parameters sent as a querystring.
        /// WorkFront server authorization complied with by adding session ID to requests as HTTP header.
        /// </summary>
        /// <param name="path">
        /// The path to the URL you want requested. This path is appended to the URL provided in the constructor.
        /// If ""http://yourcompany.attask-ondemand.com/attask/api"" is provided to the constructor, "/search" 
        /// is provided as the path parameter, and "name=ProjectName" and "day=today" are sent as parameters
        /// then an HTTP Request will be sent to "http://yourcompany.attask-ondemand.com/attask/api/search?q=mysearch&day=today"
        /// </param>
        /// <param name="parameters">
        /// Parameters to be added to the querystring. Each parameter is added as it is given
        /// starting with a "?" character and separated by an "&" character. If "q=mySearch" and 
        /// "day=today" are sent as parameters then "?q=mysearch&day=today" will be added to the query string
        /// </param>
        /// <returns>
        /// JToken containing the json data returned by the server.
        /// </returns>
        public JToken DoRequest(string path, string sessionID, params string[] parameters) 
        {
            try
            {
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                string fullUrl = url + path + ToQueryString(parameters);
                WebRequest request = HttpWebRequest.CreateDefault(new Uri(fullUrl));
                request.Headers.Add("SessionID", sessionID);
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        return ReadResponse(responseStream);
                    }
                }
            }
            catch (UriFormatException)
            {
                throw new ClientException("Invalid URL. Please ensure the company name matches the name in the WorkFront URL.");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Calls DoRequest as a GET request.
        /// <seealso cref="DoRequest"/
        /// </summary>
        /// <returns>
        /// JToken containing the json data returned by the server.
        /// </returns>
        public JToken DoGet(string path, string sessionID, params string[] parameters)
        {
            return DoRequest(path, sessionID, parameters);
        }

        /// <summary>
        /// Calls DoRequest as a POST request.
        /// </summary>
        /// <returns>
        /// JToken containing the json data returned by the server.
        /// </returns>
        public JToken DoPost(string path, string sessionID, params string[] parameters)
        {
            List<string> list = parameters.ToList();
            list.Insert(0, "method=post");
            return DoRequest(path, sessionID, list.ToArray());
        }

        /// <summary>
        /// Calls Client.DoRequest as a PUT request.
        /// </summary>
        /// <returns>
        /// JToken containing the json data returned by the server.
        /// </returns>
        public JToken DoPut(string path, string sessionID, params string[] parameters)
        {
            List<string> list = parameters.ToList();
            list.Insert(0, "method=put");
            return DoRequest(path, sessionID, list.ToArray());
        }

        /// <summary>
        /// Calls Client.DoRequest as a DELETE request.
        /// </summary>
        /// <returns>
        /// JToken containing the json data returned by the server.
        /// </returns>
        public JToken DoDelete(string path, string sessionID, params string[] parameters)
        {
            List<string> list = parameters.ToList();
            list.Insert(0, "method=delete");
            return DoRequest(path, sessionID, list.ToArray());
        }

        /// <summary>
        /// Converts the given string array to query string format.
        /// </summary>
        /// <returns>
        /// The paramters as a string formated for the needed object URI. If the parameters array 
        /// contains ["item1", "item2"] the result will be "?item1&item2"
        /// </returns>
        private string ToQueryString(string[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            parameters.ToList().ForEach(s => sb.Append(s).Append("&"));
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return "?" + sb.ToString();
        }

        /// <summary>
        /// Reads the given stream to the end then creates a new JToken containing all the data read.
        /// </summary>
        /// <param name="stream">
        /// A stream that provides JSON data.
        /// </param>
        /// <returns>
        /// JToken with read data.
        /// </returns>
        private JToken ReadResponse(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string body = reader.ReadToEnd();
            return JObject.Parse(body);
        }
    }
}
