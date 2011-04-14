using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace RESTForceToolkit
{
    public class RESTObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public JObject data { get; set; }

        public string getTextField(string fieldname)
        {
            if ((string)data[fieldname] != null) { return (string)data[fieldname]; }
            else { return null; }
        }

        public bool getBooleanField(string fieldname)
        {
            if ( data[fieldname] != null ) { return (bool)data[fieldname]; }
            else { return false; }    
        }

        public DateTime getDateField(string fieldname)
        {
            if ((DateTime)data[fieldname] != null) { return (DateTime)data[fieldname]; }
            else { return DateTime.Now; } // proper datetime handling in WP7?
        }
    }
}
