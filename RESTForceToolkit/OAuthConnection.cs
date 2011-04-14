using System;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;

namespace RESTForceToolkit
{
    public class OAuthConnection : HTTPConnection
    {
        public WebBrowser LoginBrowser { get; set; }

        public string AuthorizeURL { get; set; }
        public string RequestToken { get; set; }
        public RESTConnection restConnection { get; set; }
        
        private string accessToken;
        private string connectionURL;
        
        public string AccessToken
        {
            get
            {
                if (accessToken == null && OAuthConnection.GetKeyValue<string>("access_token") != null)
                {
                    accessToken = OAuthConnection.GetKeyValue<string>("access_token");
                }
                return accessToken;
            }
            set
            {
                OAuthConnection.SetKeyValue<string>("access_token", value);
                accessToken = value;
            }
        }

        public string ConnectionURL
        {
            get
            {
                if (connectionURL == null && OAuthConnection.GetKeyValue<string>("instance_url") != null)
                {
                    connectionURL = OAuthConnection.GetKeyValue<string>("instance_url");
                }
                return connectionURL;
            }
            set
            {
                OAuthConnection.SetKeyValue<string>("instance_url", value);
                connectionURL = value;
            }
        }

        /* These should all be Settings / Config */

        public static string RequestTokenUri = "https://login.salesforce.com/services/oauth2/token";
        public static string AuthorizeUri = "https://login.salesforce.com/services/oauth2/authorize?display=touch&response_type=code&client_id=";
        public static string AccessTokenUri = "https://login.salesforce.com/services/oauth2/token";

        public static string CallbackUri = "https://www.salesforce.com";

        public static string consumerKey = "3MVG9zeKbAVObYjPJixRj0EVnsBo5wZIgYicFa1qGLlZsATv0l3TqgxQHxOXTvDHWnkTipXWY5PpVMR5ZaaSL";
        public static string consumerKeySecret = "4014667509740828038";
        public static string oAuthVersion = "2.0";

        public delegate void OAuthCallback(string message);
        public OAuthCallback callback { get; set; }
        public OAuthCallback errorCallback { get; set; }
        
        private string username;
        private string password;

        public void createOAuthLogin(WebBrowser loginBrowser) /* Get instance of Web Browser component */
        {
            this.LoginBrowser = loginBrowser;
            this.LoginBrowser.Visibility = Visibility.Visible;
                
            var authorizeUrl = OAuthConnection.AuthorizeUri + OAuthConnection.consumerKey + "&redirect_uri=" + OAuthConnection.UrlEncode(OAuthConnection.CallbackUri);
            this.LoginBrowser.Navigating += new EventHandler<NavigatingEventArgs>(LoginBrowser_Navigating);
            this.LoginBrowser.Navigate(new Uri(authorizeUrl));
        }

        public void loginWithCredentials(string _username, string _password)
        {
            username = _username;
            password = _password;
            var AuthorizeResult = RequestTokenUri;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(OAuthConnection.AccessTokenUri);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.BeginGetRequestStream(new AsyncCallback(PerformAuthorizationCredentialsRequest), myRequest);
        }

        public void PerformAuthorizationCredentialsRequest(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            System.IO.Stream postStream = request.EndGetRequestStream(asynchronousResult);
            string postData = "code=" + RequestToken;
            postData += ("&grant_type=password");
            postData += ("&userername="+username);
            postData += ("&password="+password);

            postData += ("&client_id=" + OAuthConnection.consumerKey);
            postData += ("&client_secret=" + OAuthConnection.consumerKeySecret);
            postData += ("&redirect_uri=" + HTTPConnection.UrlEncode(OAuthConnection.CallbackUri));
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);

            // Write to the request stream.
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();
            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void LoginBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(CallbackUri))
            {
                var AuthorizeResult = e.Uri.ToString();
                string authResponse = AuthorizeResult.Replace(CallbackUri + "/?display=touch&code=", "");
                RequestToken = authResponse;
                this.LoginBrowser.Visibility = Visibility.Collapsed;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(OAuthConnection.AccessTokenUri);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.BeginGetRequestStream(new AsyncCallback(PerformAuthorizationCodeRequest), myRequest);
            }
        }

        private void PerformAuthorizationCodeRequest(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            System.IO.Stream postStream = request.EndGetRequestStream(asynchronousResult);
            string postData = "code=" + RequestToken;
            postData += ("&grant_type=authorization_code");
            postData += ("&client_id=" + OAuthConnection.consumerKey);
            postData += ("&client_secret=" + OAuthConnection.consumerKeySecret);
            postData += ("&redirect_uri=" + HTTPConnection.UrlEncode(OAuthConnection.CallbackUri));
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);

            // Write to the request stream.
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();
            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                JObject resultObject = getHTTPResult(asynchronousResult, "POST");
                AccessToken = (string)resultObject["access_token"];
                ConnectionURL = (string)resultObject["instance_url"];
                if (this.restConnection != null) { this.restConnection.Endpoint = this.ConnectionURL; }
                if (callback != null) { callback("OAuth Connected"); }
            }
            catch (WebException wex)
            {
                string result = handleHTTPError(wex);
                if (errorCallback != null) { errorCallback("OAuth error: " + result); }
            }

        }


        internal static T GetKeyValue<T>(string key)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                return (T)IsolatedStorageSettings.ApplicationSettings[key];
            else
                return default(T);
        }

        internal static void SetKeyValue<T>(string key, T value)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                IsolatedStorageSettings.ApplicationSettings[key] = value;
            else
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public bool tokenExists()
        {
            return GetKeyValue<string>("access_token") != null;
        }

        public void ClearStorage()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }
    }
}
