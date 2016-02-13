using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Memento.DependecyInjection;

namespace Memento
{
    public class DependenciesConfig
    {
        public static void RegisterDependencies()
        {
            var assembly = typeof(MvcApplication).Assembly;
            var resolver = DependencyInjector.GetResolver(assembly);

            DependencyResolver.SetResolver(resolver);
        }
    }
}
