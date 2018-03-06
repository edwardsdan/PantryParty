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

            //the application needs to make the HTTP request 
            //useragent means which browser is making this request
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0";

            request.Headers.Add("X-Mashape-Key", "B3lf5QUiIJmshYkZTOsBX2wpV3E2p1RPhROjsnr2jwlt8H1r08");

            //STEP 2. Get response back 
            //(GetResponse will give us a lot of responses... we're casting here in the '()' to tell it its a specific HTTPWeb response)
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK) //if everything goes good - ok has the value of 200 ( which = good)
            {
                StreamReader dataStream = new StreamReader(response.GetResponseStream());
                //getting data back as a stream - we're getting it back as a stream because it makes it more efficient to read instead of a blob

                string jSonData = dataStream.ReadToEnd(); // read the entire response back (this will include all JSON data)


                //parse data (to go)
                //transform from JSON to HTML
                JArray JParser = JArray.Parse(jSonData); //this is taking that string and building a JSON tree 


                //playing with this.. not working yet
                ViewBag.Apple = JParser[0]["title"];//["temperature"]; //go inside viewbag with for loop
                                                    //ViewBag.Time = JParser["time"]["startPeriodName"];



                return View("ShowResults");


            }
            else // if we have something wrong
            {
                return View("../Shared/Error");
            }


            //200 = all good
            //4_ _+ (ex:404) = problem on our side
            //5_ _ = servers fault
        }

    }
}