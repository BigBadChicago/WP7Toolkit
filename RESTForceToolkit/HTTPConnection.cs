using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RESTForceToolkit
{
    public class HTTPConnection
    {
        public HTTPConnection() { }

        public JObject getHTTPResult(IAsyncResult  result, string method) {
                HttpWebRequest asyncReq = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)asyncReq.EndGetResponse(result);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                string responseString = streamRead.ReadToEnd();
                streamResponse.Close();
                streamRead.Close();
                response.Close();
                if (responseString == "" && (method == "PATCH" || method == "DELETE") )
                {
                    responseString = "{ success: true }";
                }
                JObject resultObject = JObject.Parse(responseString);
                return resultObject;
        }
        
        public string handleHTTPError(WebException wex)
        {
            Console.WriteLine(wex.Status);
            if (wex.Response != null)
            {
                if (wex.Response.ContentLength != 0)
                {
                    using (var stream = wex.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string responseError = reader.ReadToEnd();
                            Console.WriteLine(reader.ReadToEnd());
                            return responseError;
                        }
                    }
                }
            }//
            return "Error"; //ick
        }

        public static string UrlEncode(string value)
        {
            if (value == null)
            {
                return null;
            }
            StringBuilder result = new StringBuilder();
            foreach (char symbol in value)
            {
                if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~".IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + string.Format("{0:X2}", (int)symbol));
                }
            }
            return result.ToString();
        }
    }
}
