//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace PantryParty.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json.Linq;
    public partial class UserIngredient
    {
        public string UserID { get; set; }
        public string IngredientID { get; set; }
        public int keyvalue { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Ingredient Ingredient { get; set; }

        public static List<AspNetUser> FindUsersWith(List<Ingredient> MissingIngredients)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            List<UserIngredient> UIList = new List<UserIngredient>();
            List<AspNetUser> ToReturn = new List<AspNetUser>();

            foreach (Ingredient ing in MissingIngredients)
            {
                UIList = ORM.UserIngredients.Where(x => x.IngredientID == ing.Name).ToList();
                if (UIList == null)
                {
                    continue;
                }
                else
                {
                    foreach (UserIngredient item in UIList)
                    {
                        if (!ToReturn.Exists(x => x.ID == item.UserID))
                        {
                            ToReturn.Add(ORM.AspNetUsers.Find(item.UserID));
                        }
                    }
                    UIList.Clear();
                }
            }
            ToReturn = ToReturn.Distinct<AspNetUser>().ToList();
            return ToReturn;
        }
        public static void SaveNewRecipeIngredient(JObject IngArray, Recipe ThisRecipe)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            if (ORM.Recipes.Find(ThisRecipe.ID) == null)
            {
                ORM.Recipes.Add(ThisRecipe);
                ORM.SaveChanges();
            }
            for (int i = 0; i < IngArray["extendedIngredients"].Count(); i++)
            {
                if (ORM.Ingredients.Find(IngArray["extendedIngredients"][i]["name"].ToString()) == null)
                {
                    Ingredient newIngredient = new Ingredient
                    {
                        Name = IngArray["extendedIngredients"][i]["name"].ToString()
                    };
                    ORM.Ingredients.Add(newIngredient);
                    ORM.SaveChanges();
                }
                RecipeIngredient ObjToCheck = new RecipeIngredient();
                ObjToCheck.RecipeID = ThisRecipe.ID;
                ObjToCheck.IngredientID = IngArray["extendedIngredients"][i]["name"].ToString();
                if (ORM.RecipeIngredients.Where(x => x.RecipeID == ObjToCheck.RecipeID).ToList().Count == 0)
                {
                    ORM.Recipes.Find(ThisRecipe.ID).RecipeIngredients.Add(ObjToCheck);
                    ORM.SaveChanges();
                }
            }
        }
    }
}