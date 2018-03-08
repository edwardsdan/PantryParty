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
                List<string> IngList = input.Split(',').ToList();
                Ingredient.EditIngredients(IngList, UserID);
                input = input.Replace(",", "%2C");
            }
            //else      UNCOMMENT WHEN BUGS ARE FIXED
            //{
            //    return View("../Shared/Error");
            //}

            // Gets list of recipes based on ingredients input
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
                return DisplayRecipes(recipes, UserID);
            }
            else // if we have something wrong
            {
                //RedirectToAction("../Shared/Error");
                return View("Index");
            }
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        //  [Authorize]
        public ActionResult DisplayRecipes(JArray recipes, string UserID)
        {

            //try
            //{
            List<Recipe> RecipeList = new List<Recipe>();
            for (int i = 0; i < recipes.Count; i++)
            {
                // gets specific recipe information
                HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/" + recipes[i]["id"] + "/information");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                request.Headers.Add("X-Mashape-Key", "B3lf5QUiIJmshYkZTOsBX2wpV3E2p1RPhROjsnr2jwlt8H1r08");

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string output = reader.ReadToEnd();
                    JObject jParser = JObject.Parse(output);
                    reader.Close();
                    Recipe ToAdd = Recipe.Parse(jParser);
                    RecipeList.Add(ToAdd);
                    Recipe.SaveRecipes(ToAdd, UserID);
                }
            }
            ViewBag.RecipeInfo = RecipeList;
            return View("ShowResults"); // remove when bugs are fixed
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        public ActionResult CompareMissingIngredients(Recipe ToCompare)
        {

            return View(); // can be changed accordingly
        }

        [Authorize]
        public static void SendToGMaps()
        {
            // get Lat & Lon?
            // search in maps by User's address
            // search grocery stores
        }

        // This method may be unnecessary
        public static void FindRecipeInDB(Recipe Selected)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            if (ORM.Recipes.Find(Selected.ID) == null)
            {
                ORM.Recipes.Add(Selected);
                ORM.SaveChanges();
            }
        }

        public ActionResult Things(string UserId) //FOR CALLING APIS!!!!!!!!!
        {
            pantrypartyEntities ORM = new pantrypartyEntities();

            AspNetUser CurrentUser = ORM.AspNetUsers.Find(UserId);
            string city = CurrentUser.City;

            List<AspNetUser> Users = ORM.AspNetUsers.ToList();
            List<string> DistinctCities = new List<string>();
            
            // Checking Distance between user logged in and all other users.
            foreach (AspNetUser User in Users)
            {
                if (!DistinctCities.Contains(User.City))
                {
                    // call method here
                    HttpWebRequest request = WebRequest.CreateHttp("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + city + ",DC&destinations=" + User.City + ",MI&key=AIzaSyASi83XCFM3YXo19ydq82vZ5i4tZ7rW1CQ");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader rd = new StreamReader(response.GetResponseStream());
                        string output = rd.ReadToEnd(); //reads all the response back
                        //parsing the data

                        JObject JParser = JObject.Parse(output);
                        ViewBag.RawData = JParser["rows"][0]["elements"][0]["distance"]["text"];
                        string x = ViewBag.RawData;
                        //string y = x.Remove(x.Length-1,4);
                        char[] charArray = { '.' };
                        string[] y = x.Split(charArray[0]);
                        int z = Convert.ToInt32(y[0]);
                        //int z = int.Parse(y);

                        #region Distance if
                        if (z <= 50)
                        {
                            DistinctCities.Add(User.City);
                        }
                        else
                            continue;//no one in your area.
                        #endregion
                        // end method
                    }
                    else
                    // something is wrong
                    {
                        return View("../Shared/Error");
                    }
                }
            } // end of foreach

            // another method
            // List of nearby Users
            List<AspNetUser> NearbyUsers = new List<AspNetUser>();
            foreach (string CityName in DistinctCities)
            {
                List<AspNetUser> UserList = ORM.AspNetUsers.Where(x => x.City == CityName).ToList();
                foreach (AspNetUser Person in UserList)
                {
                    if (Person.City == CityName)
                    {
                        NearbyUsers.Add(Person);
                    }
                }
            }


            return View();
        }

        public ActionResult Button()
        {
            return View();
        }

    }
}