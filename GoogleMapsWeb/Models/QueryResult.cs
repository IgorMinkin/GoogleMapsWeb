using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleMapsWeb.Models
{
    public class QueryResult
    {
        public IList<Place> Places { get; set; }
        public IList<Location> Locations { get; set; }
        public string where { get; set; }
        public string what { get; set; }
        public double radius { get; set; }
 
        public QueryResult()
        {
            Places = new List<Place>();
            Locations = new List<Location>();
            radius = 0.75;
        }
    }
}