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
            List<string> IngList = input.Split(',').ToList();

            SaveIngredients(EditIngredients(IngList), UserID);
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
        public ActionResult DisplayRecipes(JArray recipes)
        {
            //try
            //{
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

        public static void SaveIngredients(List<Ingredient> IngList, string UserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser User = ORM.AspNetUsers.Find(UserID);
            foreach (Ingredient ing in IngList)
            {
                UserIngredient NewUserIngredient = new UserIngredient();
                NewUserIngredient.UserID = UserID;
                NewUserIngredient.IngredientID = ing.Name;
                if (!ORM.Ingredients.ToList().Contains(ing))
                {
                    ORM.Ingredients.Add(ing);
                }
                User.UserIngredients.Add(NewUserIngredient);
                ORM.SaveChanges();
                // check DB interaction/relationship ings->users? users->ings?
            }
        }

        public static List<Ingredient> EditIngredients(List<string> IngList)
        {
            List<Ingredient> newList = new List<Ingredient>(IngList.Capacity);
            foreach (string ing in IngList)
            {
                Ingredient ToAdd = new Ingredient();
                ToAdd.Name = ing;
                newList.Add(ToAdd);
            }
            return newList;
        }
    }
}