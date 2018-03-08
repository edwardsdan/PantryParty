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
    using Newtonsoft.Json.Linq;

    public partial class Recipe
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Recipe()
        {
            this.UserRecipes = new HashSet<UserRecipe>();
        }
    
        public string ID { get; set; }
        public string ImageURL { get; set; }
        public string Title { get; set; }
        public string CookTime { get; set; }
        public string ImageType { get; set; }
        public string Instructions { get; set; }
        public string RecipeURL { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecipe> UserRecipes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecipeIngredient> RecipeIngredient { get; set; }

        public static Recipe Parse(JObject input)
        {
            Recipe output = new Recipe();
            output.ID = input["id"].ToString();
            output.ImageURL = input["image"].ToString();
            output.CookTime = input["readyInMinutes"].ToString();
            output.ImageType = input["imageType"].ToString();
            output.Instructions = input["instructions"].ToString();
            output.Title = input["title"].ToString();
            output.RecipeURL = input["spoonacularSourceUrl"].ToString();
            return output;
        }

        // Checks if Recipe exists in DB, if not, adds to DB, and adds User->Recipe relationship
        public static void SaveRecipes(Recipe ThisRecipe, string UserID)
        {
            pantrypartyEntities ORM = new pantrypartyEntities();
            AspNetUser CurrentUser = ORM.AspNetUsers.Find(UserID);
            UserRecipe ToAdd = new UserRecipe();
            ToAdd.UserID = UserID;
            ToAdd.RecipeID = ThisRecipe.ID;
            if (ORM.Recipes.Find(ThisRecipe.ID) == null)
            {
                ORM.Recipes.Add(ThisRecipe);
                ORM.SaveChanges();
            }
            CurrentUser.UserRecipes.Add(ToAdd);
            ORM.SaveChanges();
        }
    }
}
