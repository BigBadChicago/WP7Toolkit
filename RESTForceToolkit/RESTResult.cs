using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RESTForceToolkit
{
    public class RESTResult
    {
        public RESTRequest request { get; set; }
        public JObject data { get; set; }
        public string message { get; set; }
       
        public RESTResult() { }
    }
}
