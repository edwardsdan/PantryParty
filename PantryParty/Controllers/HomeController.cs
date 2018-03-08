using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PantryParty.Models;
using System.Net;
using System.IO;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace PantryParty.Controllers
{
    public class HomeController : Controller
    {

        // Google Directions API Key: AIzaSyCAERkhlLqh6FoMMAa3PFzxn_RZeaYEsXw
        // Google Embed API Key: AIzaSyBDEoOqYsBV3hGVbktNMKulDnheQgm0vK8


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

        [Authorize]
        public ActionResult Welcome()
        {
            return View();
        }

        // displays recipes based on ingredients and saves ingredients to database
        [Authorize] //you're only allowed here if you're logged in
        public ActionResult FridgeItems(string input, string UserID)
        {
            //try
            //{
            if (Regex.IsMatch(input, @"([A-Za-z\s])"))
            {
                Ingredient.EditIngredients(input, UserID);
            }
            else if (input.Contains(","))
            {
                // Overload methods Edit/SaveIngredients
                List<string> IngList = input.Split(',').ToList();
                Ingredient.EditIngredients(IngList, UserID);
                input = input.Replace(",", "%2C");
            }
            
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
                dataStream.Close();
                return RedirectToAction("DisplayRecipes");
            }
            else // if we have something wrong
            {
                //return View("../Shared/Error");
                return View("Index");
            }
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        [Authorize]
        public ActionResult DisplayRecipes(JArray recipes, string UserID)
        {
            //try
            //{
            List<Recipe> RecipeList = new List<Recipe>();
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
                    reader.Close();
                    Recipe ToAdd = Recipe.Parse(jParser);
                    Recipe.SaveRecipes(ToAdd, UserID);
                }
            }
            return View("ShowResults");
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }



        [Authorize]
        public static void SendToGMaps()
        {
            // get Lat & Lon?
            // search in maps by User's address
            // search grocery stores
        }


        public static void FindRecipeInDB(Recipe Selected)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            if (ORM.Recipes.Find(Selected.ID) == null)
            {
                ORM.Recipes.Add(Selected);
                ORM.SaveChanges();
            }
        }
    }
}