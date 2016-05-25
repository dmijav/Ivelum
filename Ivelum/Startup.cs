using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ivelum.Startup))]
namespace Ivelum
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
