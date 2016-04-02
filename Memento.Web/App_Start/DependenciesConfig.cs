using Memento.DependecyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Memento
{
    public class DependenciesConfig
    {
        public static void RegisterDependencies()
        {
            var assembly = typeof(MvcApplication).Assembly;
            DependencyInjector.Initialize(assembly);

            var mvcResolver = DependencyInjector.GetMvcResolver();
            var webApiResolver = DependencyInjector.GetWebApiResolver();

            DependencyResolver.SetResolver(mvcResolver);
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
        }
    }
}
