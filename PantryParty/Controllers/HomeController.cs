using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace PantryParty.Controllers
{
    public class HomeController : Controller
    {

        // Google Directions API Key: AIzaSyCAERkhlLqh6FoMMAa3PFzxn_RZeaYEsXw

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DisplayFridgeItems()
        {
            return View(); //displaying page
        }

        public ActionResult FridgeItems(string input)
        {
            HttpWebRequest request = WebRequest.CreateHttp($"https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?number=5&ranking=1&ingredients=apples%2Cflour%2Csugar"); //here we're calling the API

            //above link is for test/sample only.. use this for actual web: https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/autocomplete?number=10&query={input}

            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            request.Headers.Add("X-Mashape-Key", "B3lf5QUiIJmshYkZTOsBX2wpV3E2p1RPhROjsnr2jwlt8H1r08");

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK) //if everything goes good - ok has the value of 200 ( which = good)
            {
                StreamReader dataStream = new StreamReader(response.GetResponseStream());
                // getting data back as a stream

                string jSonData = dataStream.ReadToEnd();
                JArray recipes = JArray.Parse(jSonData); //this is taking that string and building a JSON tree 
                
                ViewBag.Data = recipes;
                return RedirectToAction("DisplayRecipes");
            }
            else // if we have something wrong
            {
                return View("../Shared/Error");
            }
        }

        public ActionResult DisplayRecipes(JArray recipes)
        {
            try
            {
                ViewBag.RecipeInfo = "";
                foreach (JObject recipe in recipes)
                {
                    HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/" + recipe["id"] + "/information");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                    request.Headers.Add("X-Mashape-Key", "B3lf5QUiIJmshYkZTOsBX2wpV3E2p1RPhROjsnr2jwlt8H1r08");

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string output = reader.ReadToEnd();
                        JObject jParser = JObject.Parse(output);
                        ViewBag.RecipeInfo += jParser;
                    }
                }
                return View("ShowResults");
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }
    }
}