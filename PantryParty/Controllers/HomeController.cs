using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PantryParty.Models;
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
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        // displays recipes based on ingredients and saves ingredient IDs to database
        public ActionResult FridgeItems(string input, string UserID)
        {
            try
            {
                List<string> IngList = input.Split(',').ToList();
                SaveIngredients(IngList);
                input = input.Replace(",", "%2C");
                HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=" + input + "&limitLicense=false&number=5&ranking=1");

                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                request.Headers.Add("X-Mashape-Key", "B3lf5QUiIJmshYkZTOsBX2wpV3E2p1RPhROjsnr2jwlt8H1r08");

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader dataStream = new StreamReader(response.GetResponseStream());

                    string jSonData = dataStream.ReadToEnd();
                    JArray recipes = JArray.Parse(jSonData);
                    ViewBag.Data = recipes;
                    return RedirectToAction("DisplayRecipes");
                }
                else // if we have something wrong
                {
                    return View("../Shared/Error");
                }
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        public ActionResult DisplayRecipes(JArray recipes)
        {
            try
            {
                ViewBag.RecipeInfo = "";
                for (int i = 0; i < recipes.Count; i++)
                {
                    HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/" + recipes[i]["id"] + "/information");
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

        public static void SaveIngredients(List<string> IngList)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            Ingredient newIngredient = new Ingredient();
            foreach (string Ingredient in IngList)
            {
                newIngredient.Name = Ingredient;
                ORM.Ingredients.Add(newIngredient);
            }
        }
    }
}