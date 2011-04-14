using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RESTForceToolkit
{
    public class RESTRequest
    {
        public delegate void RESTCallback(RESTResult result);
        public RESTCallback callback { get; set; }
        public RESTCallback errorCallback { get; set; }
        
    }
}
