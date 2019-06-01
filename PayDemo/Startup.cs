using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PayDemo.Startup))]
namespace PayDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
