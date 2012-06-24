using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleMapsWeb.Models
{
    public class Place
    {
        public string id { get; set; }
        public string name { get; set; }
        public string vicinity { get; set; }
        public string reference { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
}
}