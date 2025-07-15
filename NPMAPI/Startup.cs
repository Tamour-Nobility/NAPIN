using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(NPMAPI.Startup))]

namespace NPMAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
