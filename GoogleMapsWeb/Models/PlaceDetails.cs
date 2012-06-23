using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleMapsWeb.Models
{
    public class PlaceDetails
    {
        public string formatted_address { get; set; }
        public string formatted_phone_number { get; set; }
        public string website { get; set; }
    }
}