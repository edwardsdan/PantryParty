using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PantryParty.Models;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace PantryParty.Controllers
{
    public class HomeController : Controller
    {
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
           // try
           //// {
                if (Regex.IsMatch(input, @"^([A-Za-z\s]{1,})$"))
                {
                    Ingredient.EditIngredients(input, UserID);
                }
                else if (Regex.IsMatch(input, @"^([A-Za-z\s\,]{1,})$"))
                {
                    List<string> IngList = input.Split(',').ToList();
                    Ingredient.EditIngredients(IngList, UserID);
                    input = input.Replace(",", "%2C");
                }
                //else
                //{
                //    return View("../Shared/Error");
                //}

                // Gets list of recipes based on ingredients input
                HttpWebRequest request = WebRequest.CreateHttp("https://spoonacular-recipe-food-nutrition-v1.p.mashape.com/recipes/findByIngredients?fillIngredients=false&ingredients=" + input + "&limitLicense=false&number=5&ranking=1");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                string Header = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Header"];
                string APIkey = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Key"];
                request.Headers.Add(Header, APIkey);

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
                    return View("../Shared/Error");
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
                string Header = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Header"];
                string APIkey = System.Configuration.ConfigurationManager.AppSettings["Spoonacular API Key"];
                request.Headers.Add(Header, APIkey);

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
                    UserIngredient.SaveNewRecipeIngredient(jParser, ToAdd);
                }
            }
            ViewBag.RecipeInfo = RecipeList;
            return View("ShowResults");
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        public ActionResult CompareMissingIngredients(string ToCompare, string UserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            List<Ingredient> RecipesIngredientsList = new List<Ingredient>();
            List<Ingredient> MyIngredients = new List<Ingredient>();

            // Creates list of RecipeIngredient objects and initializes RecipeIngredientsList with all matching values
            List<RecipeIngredient> ChangeToRecipesIng = ORM.RecipeIngredients.Where(x => x.RecipeID == ToCompare).ToList();
            foreach (RecipeIngredient x in ChangeToRecipesIng)
            {
                if (!RecipesIngredientsList.Contains(ORM.Ingredients.Find(x.IngredientID)))
                {
                    RecipesIngredientsList.Add(ORM.Ingredients.Find(x.IngredientID));
                }
            }

            // Creates list of UserIngredient objects and initializes UserIngredientList with all matching values
            List<UserIngredient> ChangeToUserIngredients = ORM.UserIngredients.Where(x => x.UserID == UserID).ToList();
            foreach (UserIngredient x in ChangeToUserIngredients)
            {
                if (!MyIngredients.Contains(ORM.Ingredients.Find(x.IngredientID)))
                {
                    MyIngredients.Add(ORM.Ingredients.Find(x.IngredientID));
                }
            }

            // Edits recipe's ingredients list to contain only missing ingredients
            foreach (Ingredient x in MyIngredients)
            {
                if (RecipesIngredientsList.Contains(x))
                {
                    RecipesIngredientsList.Remove(x);
                }
            }

            // Creates list of Users with any/all of your missing ingredients
            List<AspNetUser> CheckNearby = UserIngredient.FindUsersWith(RecipesIngredientsList);

            // Sends list of nearby users with your missing ingredients to page

            List<AspNetUser> NearbyUsers = FindNearbyUsers(CheckNearby, UserID);
            ViewBag.NearbyUsers = NearbyUsers;

            ViewBag.CurrentUserLatLong = Geocode(UserID);
            ViewBag.LatLongArray = Geocode(NearbyUsers, UserID);

            // Sends list of your missing ingredients to page
            // ViewBag.MissingIngredients = RecipesIngredientsList;

            List<UserIngredient> Test = new List<UserIngredient>();
            foreach (Ingredient item in RecipesIngredientsList)
            {
                Test.AddRange(ORM.UserIngredients.Where(x => x.IngredientID == item.Name));
            }
            ViewBag.UserIngredients = Test.Distinct().ToList();

            string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Marker API KEY"];
            ViewData["APIkey"] = APIkey;

            return View("NearbyUsers");
            // Geocode(UserID);
        }

        public static LatLong Geocode(string CurrentUserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser CurrentUser = ORM.AspNetUsers.Find(CurrentUserID);
            LatLong ToReturn = new LatLong();
            string googleplus = Plus(CurrentUser.Address, CurrentUser.City, CurrentUser.State);
            string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Geocode API KEY"];

            HttpWebRequest request = WebRequest.CreateHttp($"https://maps.googleapis.com/maps/api/geocode/json?address={googleplus}&key={APIkey}");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader rd = new StreamReader(response.GetResponseStream());
                string output = rd.ReadToEnd();
                JObject Jparser = JObject.Parse(output);

                ToReturn.Lat = Jparser["results"][0]["geometry"]["location"]["lat"].ToString();
                ToReturn.Long = Jparser["results"][0]["geometry"]["location"]["lng"].ToString();
                return ToReturn;
            }
            return ToReturn;
        }

        public static LatLong[] Geocode(List<AspNetUser> NearByUsers, string UserID)
        {
            LatLong[] ToReturn = new LatLong[NearByUsers.Count()];
          
            int i = 0;
            foreach (AspNetUser Person in NearByUsers)
            {
                ToReturn[i] = new LatLong();
                if (Person.ID == UserID)
                {
                    continue;
                }
                string googleplus = Plus(Person.Address, Person.City, Person.State);
                string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Geocode API KEY"];

                HttpWebRequest request = WebRequest.CreateHttp($"https://maps.googleapis.com/maps/api/geocode/json?address={googleplus}&key={APIkey}");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader rd = new StreamReader(response.GetResponseStream());
                    string output = rd.ReadToEnd();
                    JObject Jparser = JObject.Parse(output);
                    
                    ToReturn[i].Lat = Jparser["results"][0]["geometry"]["location"]["lat"].ToString();
                    ToReturn[i].Long = Jparser["results"][0]["geometry"]["location"]["lng"].ToString();
                    i++;
                }
            }
            return ToReturn;
        }

        public static string Plus(string a, string b, string c)
        {
            string street = a.Replace(" ", "+");
            string city = b.Replace(" ", "+");
            string state = c;
            string google = street + ",+" + city + ",+" + state;
            return google;
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

        // Move  this method to User class and refactor
        public List<AspNetUser> FindNearbyUsers(List<AspNetUser> Users, string UserId)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser CurrentUser = ORM.AspNetUsers.Find(UserId);
            string city = CurrentUser.City;

            //List<string> DistinctNearbyCities = new List<string>();
            List<AspNetUser> NearbyUsers = new List<AspNetUser>();

            // Checks distance between you and all users with your missing ingredients
            foreach (AspNetUser User in Users)
            {
                if (!NearbyUsers.Contains(User))
                {
                    string APIkey = System.Configuration.ConfigurationManager.AppSettings["Google Distance Matrix API KEY"];

                    HttpWebRequest request = WebRequest.CreateHttp("https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + city + ",MI&destinations=" + User.City + ",MI&key=" + APIkey);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        StreamReader rd = new StreamReader(response.GetResponseStream());
                        string output = rd.ReadToEnd();
                        rd.Close();
                        JObject JParser = JObject.Parse(output);

                        // Gets the distance between your city and another user's city and converts to a floating-point
                        string DistanceAsString = JParser["rows"][0]["elements"][0]["distance"]["text"].ToString();

                        // also removes " mi" from end of DistanceAsString
                        float DistanceAsFloat = float.Parse(DistanceAsString.Remove(DistanceAsString.Length - 3));

                        #region Distance if // Add distance input functionality
                        if (DistanceAsFloat <= 20)
                        {
                            // populates list with users within 20 miles of your city
                            NearbyUsers.Add(User);
                        }
                        else
                            continue;//no one in your area yet
                        #endregion
                        // end method
                    }
                    else
                    // something is wrong
                    {
                        //return View("../Shared/Error");
                    }
                }
            } // end of foreach
            return NearbyUsers;
        }

        public ActionResult FindNearbyUsersButton()
        {
            return View();
        }

        public ActionResult EditIngred(string UserID)
        {

            pantrypartyEntities ORM = new pantrypartyEntities(); //creating new object to use SQL
            List<UserIngredient> EditThisList = ORM.UserIngredients.Where(x => x.UserID == UserID).ToList(); //looking for all ingred. that have given (current) UserID, saving it into viewbag
            List<Ingredient> ToSend = new List<Ingredient>();
            foreach (UserIngredient x in EditThisList)
            {
                ToSend.Add(ORM.Ingredients.Find(x.IngredientID));
            }
            ViewBag.UsersListOfIngred = ToSend;
            return View("EditIngred");
        }

        public ActionResult Delete(string CurrentUser, string ItemToDelete)
        {
            //try
            //{
                pantrypartyEntities ORM = new pantrypartyEntities();
                ORM.UserIngredients.RemoveRange(ORM.UserIngredients.Where(x => (x.UserID == CurrentUser && x.IngredientID == ItemToDelete)));
                ORM.SaveChanges();
                return EditIngred(CurrentUser);
            //}
            //catch (Exception)
            //{
            //    return View("../Shared/Error");
            //}
        }

        //EDIT PROFILE -- not working yet
        public ActionResult UpdateProfile(string UserID)
        {
            try
            {
                pantrypartyEntities ORM = new pantrypartyEntities();
                AspNetUser ToBeUpdated = ORM.AspNetUsers.Find(UserID);
                ViewBag.UpdateProf = ToBeUpdated;

                return View();
            }
            catch (Exception)
            {
                return View("../Shared/Error");
            }
        }

        //SAVED EDIT PROFILE
        public ActionResult SaveProfChanges(AspNetUser NUser)
        {
            ////if (!ModelState.IsValid)
            //{
            //    return View("../Shared/Error");
            //}
            pantrypartyEntities ORM = new pantrypartyEntities();

            AspNetUser CurrentUser = ORM.AspNetUsers.Find(NUser.ID);

            CurrentUser.FirstName = NUser.FirstName;
            CurrentUser.LastName = NUser.LastName;
            CurrentUser.PhoneNumber = NUser.PhoneNumber;
            CurrentUser.Address = NUser.Address;
            CurrentUser.City = NUser.City;
            CurrentUser.State = NUser.State;
            CurrentUser.Zipcode = NUser.Zipcode;
            CurrentUser.EmailConfirmed = NUser.EmailConfirmed;



            ORM.Entry(ORM.AspNetUsers.Find(NUser.ID)).CurrentValues.SetValues(CurrentUser); //finding old object, replacing it with new information

            ORM.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
