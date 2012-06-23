using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoogleMapsWeb.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GoogleMapsWeb.Controllers
{
    public class HomeController : Controller
    {
        protected static string ApiKey = "AIzaSyBj5xs7XOoAcTnPu2SAe4fnFU2FUHuKrmo";
        protected static string GeocodingBaseUrl = "http://maps.googleapis.com/maps/api/geocode/json";
        protected static string PlacesBaseUrl = "https://maps.googleapis.com/maps/api/place/search/json";
        protected static string PlaceDetailsBaseUrl = "https://maps.googleapis.com/maps/api/place/details/json";

        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string where, string what)
        {
            var address = where;
            var place = what;

            var locations = FindLocation(address);

            if(locations.Count == 0)
            {
                ViewBag.Message = "No locations match you search criteria.";
                return View("Results", new QueryResult());
            }
            else if(locations.Count > 1)
            {
                return View("Locations",
                            new QueryResult() {Locations = locations, where = address, what = place});
            }
                
             var places = FindPlaces(locations[0], what);
            
            
            return View("Results", new QueryResult() {Locations = locations, Places = places, where = address, what = place});

        }

        public ActionResult FindByLocation(float lat, float lng, string what)
        {
            var places = FindPlaces(new Location() {lat = lat, lng = lng}, what);
            return View("Results", new QueryResult {Places = places});
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

        private List<Place> FindPlaces(Location location, string businessName)
        {
            var client = new RestClient(PlacesBaseUrl);
            var request = new RestRequest(Method.GET);
            request.AddParameter("key", ApiKey);
            request.AddParameter("location", location.lat + "," +location.lng);
            request.AddParameter("rankby", "distance");

            if (!String.IsNullOrWhiteSpace(businessName))
            {
                request.AddParameter("name", businessName);
            }
            request.AddParameter("sensor", "false");
            request.AddParameter("types", "parking|train_station");

            IRestResponse response = client.Execute(request);

            var placesResults = JObject.Parse(response.Content).SelectToken("results");
            return placesResults.Select(result => new Place()
                                                        {
                                                            id = result.SelectToken("id").ToString(),
                                                            name = result.SelectToken("name").ToString(),
                                                            vicinity = result.SelectToken("vicinity").ToString(),
                                                            reference = result.SelectToken("reference").ToString()
                                                        }).ToList();

        }
 

    }
}
