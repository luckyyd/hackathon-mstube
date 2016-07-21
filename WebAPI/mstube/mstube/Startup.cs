using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(mstube.Startup))]
namespace mstube
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
