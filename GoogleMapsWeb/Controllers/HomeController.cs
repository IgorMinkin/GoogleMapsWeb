using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using GoogleMapsWeb.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GoogleMapsWeb.Controllers
{
    public class HomeController : Controller
    {
        protected static string ApiKey = ConfigurationManager.AppSettings["ApiKey"];
        protected static string GeocodingBaseUrl = "http://maps.googleapis.com/maps/api/geocode/json";
        protected static string PlacesBaseUrl = "https://maps.googleapis.com/maps/api/place/search/json";
        protected static string PlaceDetailsBaseUrl = "https://maps.googleapis.com/maps/api/place/details/json";
        protected static double DefaultSearchRadius = 1000.0;

        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string where, string what, string radius)
        {
            var address = where;
            var place = what;
            var radiusMi = Double.Parse(radius.Substring(0, radius.IndexOf('m')));
            var radiusMeter = radiusMi * 1.6 * 1000;

            var locations = FindLocation(address);

            if(locations.Count == 0)
            {
                ViewBag.Message = "No locations match you search criteria.";
                return View("Results", new QueryResult());
            }
             
            if(locations.Count > 1)
            {
                return View("Locations",
                            new QueryResult() {Locations = locations, where = address, what = place, radius = radiusMi});
            }
                
             var places = FindPlaces(locations[0], what, radiusMeter);
            
            
            return View("Results", new QueryResult() {Locations = locations, Places = places, where = address, what = place, radius = radiusMi});

        }

        public ActionResult FindByLocation(float lat, float lng, string what)
        {
            var places = FindPlaces(new Location() {lat = lat, lng = lng}, what, DefaultSearchRadius);
            return View("Results", new QueryResult {Places = places, Locations = new List<Location>()
                                                                                     {
                                                                                         new Location() {lat = lat, lng = lng}
                                                                                     }});
        }

        public JsonResult GetPlaceDetails(string RefCode)
        {
            return RefCode == null ? null : Json(GetPlaceDetailsInternal(RefCode), JsonRequestBehavior.AllowGet);
        }



        private PlaceDetails GetPlaceDetailsInternal(string refCode)
        {
            var client = new RestClient(PlaceDetailsBaseUrl);
            var request = new RestRequest(Method.GET);
            request.AddParameter("key", ApiKey);
            request.AddParameter("reference", refCode);
            request.AddParameter("sensor", "false");
            var response = client.Execute(request);

            var result = JObject.Parse(response.Content).SelectToken("result");
            var details = new PlaceDetails()
                              {
                                  formatted_phone_number = result.SelectToken("formatted_phone_number") == null ? "" : result.SelectToken("formatted_phone_number").ToString(),
                                  website = result.SelectToken("website") == null ? "" : result.SelectToken("website").ToString()
                              };
                

            return details;
        }

        private List<Location> FindLocation(string address)
        {
            //find matching locations
            var client = new RestClient(GeocodingBaseUrl);
            var request = new RestRequest(Method.GET);
            //request.AddParameter("key", ApiKey);
            request.AddParameter("address", address);
            request.AddParameter("sensor", "false");
            var response = client.Execute(request);

            var status = JObject.Parse(response.Content).SelectToken("status");
            ViewBag.LocStatus = status;

            var results = JObject.Parse(response.Content).SelectToken("results");

            var locations = results.Select(
                result => new Location()
                              {
                                  formatted_address = result.SelectToken("formatted_address").ToString(),
                                  lat = (float) result.SelectToken("geometry.location.lat"),
                                  lng = (float) result.SelectToken("geometry.location.lng")
                              }).ToList();

            return locations;
        }

        private List<Place> FindPlaces(Location location, string businessName, double radius)
        {
            var client = new RestClient(PlacesBaseUrl);
            var request = new RestRequest(Method.GET);
            request.AddParameter("key", WebConfigurationManager.AppSettings["ApiKey"]);
            request.AddParameter("location", location.lat + "," +location.lng);
            request.AddParameter("radius", radius);

            if (!String.IsNullOrWhiteSpace(businessName))
            {
                request.AddParameter("name", businessName);
            }
            request.AddParameter("sensor", "false");
            request.AddParameter("types", "parking");
            //request.AddParameter("keyword", "parking garage");

            IRestResponse response = client.Execute(request);

            var placesResults = JObject.Parse(response.Content).SelectToken("results");
            return placesResults.Select(result => new Place()
                                                        {
                                                            id = result.SelectToken("id").ToString(),
                                                            name = result.SelectToken("name").ToString(),
                                                            vicinity = result.SelectToken("vicinity").ToString(),
                                                            reference = result.SelectToken("reference").ToString(),
                                                            lat = (float) result.SelectToken("geometry.location.lat"),
                                                            lng = (float) result.SelectToken("geometry.location.lng")
                                                        }).ToList();

        }
 

    }
}
