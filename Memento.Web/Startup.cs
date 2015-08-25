using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Memento.Startup))]
namespace Memento
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
