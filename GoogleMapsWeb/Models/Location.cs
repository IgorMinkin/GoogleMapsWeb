using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleMapsWeb.Models
{
    public class Location
    {
        public string formatted_address { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
    }
}