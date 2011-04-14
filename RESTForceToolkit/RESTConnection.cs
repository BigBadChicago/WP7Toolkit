using System;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;


namespace RESTForceToolkit
{
    public class RESTConnection : HTTPConnection
    {
        public OAuthConnection OAuthConnection {get; set;}
        public string Endpoint {get; set;}
       
        public RESTConnection() {
               this.OAuthConnection = new OAuthConnection();
               this.OAuthConnection.restConnection = this;
               if(this.OAuthConnection.ConnectionURL != null) { this.Endpoint = this.OAuthConnection.ConnectionURL; }
        }

        public void getObjectById(string type, string id, RESTRequest.RESTCallback _callback)
        {
            RESTRequest req = new RESTRequest();
            req.callback = _callback;
            createRESTRequest(req, this.Endpoint + "/services/data/v20.0/sobjects/" + type + "/" + id, "GET", null);
        }

        public void query(string query, RESTRequest.RESTCallback callback, RESTRequest.RESTCallback errorCallback)
        {
            RESTRequest req = new RESTRequest();
            req.callback = callback;
            req.errorCallback = errorCallback;
            createRESTRequest(req, this.Endpoint + "/services/data/v20.0/query/?q=" + query, "GET", null);
        }

        public JObject createNew(string type)
        {
            JObject newObject = new JObject(new JProperty("type", type), new JProperty("fields", new JObject()));
            return newObject;
        }

        public void insert(JObject newObject, RESTRequest.RESTCallback callback, RESTRequest.RESTCallback errorCallback)
        {
            RESTRequest req = new RESTRequest();
            req.callback = callback;
            req.errorCallback = errorCallback;
            string type = (string)newObject["type"];
            createRESTRequest(req, this.Endpoint + "/services/data/v20.0/sobjects/" + type + "/", "POST", newObject["fields"].ToString());
        }

        public void update(JObject jobject, RESTRequest.RESTCallback callback, RESTRequest.RESTCallback errorCallback)
        {
            RESTRequest req = new RESTRequest();
            req.callback = callback;
            req.errorCallback = errorCallback;
            string type = (string)jobject["type"];
            createRESTRequest(req, this.Endpoint + "/services/data/v20.0/sobjects/" + type + "/" + (string)jobject["id"], "PATCH", jobject["fields"].ToString());
        }

        public void delete(JObject jobject, RESTRequest.RESTCallback callback, RESTRequest.RESTCallback errorCallback)
        {
            RESTRequest req = new RESTRequest();
            req.callback = callback;
            req.errorCallback = errorCallback;
            string type = (string)jobject["type"];
            createRESTRequest(req, this.Endpoint + "/services/data/v20.0/sobjects/" + (string)jobject["attributes"]["type"] + "/" + (string)jobject["Id"], "DELETE", null); //BEWARE
        }

        public void createRESTRequest(RESTRequest rest_request, string URI, string method, string body)
        {
            HttpWebRequest req = WebRequest.Create(new Uri(URI))
                           as HttpWebRequest;
            req.Method = method;
            req.Headers["Authorization"] = "OAuth " + OAuthConnection.AccessToken;
            req.Headers["User-Agent"] = "salesforce-toolkit/Windows7/20";
            if (method == "POST" || method == "PATCH")
            {
                req.ContentType = "application/json";
                req.BeginGetRequestStream(
                    (result) =>
                    {
                        HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                        System.IO.Stream postStream = request.EndGetRequestStream(result);
                        string postData = body;
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);

                        // Write to the request stream.
                        postStream.Write(byteArray, 0, postData.Length);
                        postStream.Close();
                        // Start the asynchronous operation to get the response
                        request.BeginGetResponse(
                                        (postresult) =>
                                        {
                                            try
                                            {
                                                RESTResult restresult = new RESTResult();
                                                restresult.request = rest_request;
                                                restresult.data = getHTTPResult(postresult,method);
                                                restresult.request.callback( restresult );
                                            }
                                            catch (WebException wex)
                                            {
                                                RESTResult restresult = new RESTResult();
                                                restresult.request = rest_request;
                                                string error = handleHTTPError(wex);
                                                restresult.data = JObject.Parse(error.Substring(1, error.Length - 1));
                                                restresult.message = (string)restresult.data["message"];
                                                restresult.request.errorCallback( restresult );
                                            }

                                        }, req);
                    }, req);

            }
            else
            {
                req.BeginGetResponse(
                    (result) =>
                    {
                        try
                        {
                            RESTResult restresult = new RESTResult();
                            restresult.request = rest_request;
                            restresult.data = getHTTPResult(result, method);
                            restresult.request.callback(restresult);
                        }
                        catch (WebException wex)
                        {
                            RESTResult restresult = new RESTResult();
                            restresult.request = rest_request;
                            string error = handleHTTPError(wex);
                            restresult.data = JObject.Parse(error.Substring(1, error.Length - 1));
                            restresult.message = (string)restresult.data["message"];
                                               
                            restresult.request.errorCallback(restresult);
                        }

                    }, req);

            }
        }
 
     }
}
