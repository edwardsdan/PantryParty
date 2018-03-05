using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PantryParty.Startup))]
namespace PantryParty
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
