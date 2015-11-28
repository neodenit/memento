using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Memento.DomainModel.Repository;
using Memento.Core.Evaluators;
using Memento.Core;
using Memento.Core.Validators;

namespace Memento
{
    public class DependenciesConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterType<EFMementoRepository>().As<IMementoRepository>();
            builder.RegisterType<PhraseEvaluator>().As<IEvaluator>().WithParameter("permissibleError", 0.2);
            builder.RegisterType<Converter>().As<IConverter>();
            builder.RegisterType<CombinedValidator>().As<IValidator>();
            builder.RegisterType<Scheduler>().As<IScheduler>();
            builder.RegisterType<NewCardsManager>().As<INewCardsManager>();
            builder.RegisterType<SiblingsManager>().As<ISiblingsManager>();
            
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
