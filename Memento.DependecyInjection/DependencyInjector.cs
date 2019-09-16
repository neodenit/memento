using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Memento.Core;
using Memento.Core.Converter;
using Memento.Core.Evaluators;
using Memento.Core.Scheduler;
using Memento.Core.Validators;
using Memento.DataAccess.Repository;
using Memento.Interfaces;
using Memento.Models;
using Memento.Services;

namespace Memento.DependecyInjection
{
    public static class DependencyInjector
    {
        private static IContainer container;

        public static void Initialize(Assembly assembly)
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(assembly);
            builder.RegisterApiControllers(assembly);

            builder.RegisterType<EFMementoRepository>().As<IMementoRepository>().InstancePerRequest();

            builder.RegisterType<DecksService>().As<IDecksService>();
            builder.RegisterType<CardsService>().As<ICardsService>();
            builder.RegisterType<StatisticsService>().As<IStatisticsService>();
            builder.RegisterType<ExportImportService>().As<IExportImportService>();
            builder.RegisterType<SchedulerService>().As<ISchedulerService>();            

            builder.RegisterType<PhraseEvaluator>().As<IEvaluator>();
            builder.RegisterType<Converter>().As<IConverter>();
            builder.RegisterType<ExistenceValidator>().As<IValidator>().WithParameter("baseValidator", null);
            builder.RegisterType<Scheduler>().As<IScheduler>();
            builder.RegisterType<NewClozesManager>().As<INewClozesManager>();
            builder.RegisterType<SiblingsManager>().As<ISiblingsManager>();

            container = builder.Build();
        }

        public static System.Web.Mvc.IDependencyResolver GetMvcResolver() =>
            new AutofacDependencyResolver(container);

        public static System.Web.Http.Dependencies.IDependencyResolver GetWebApiResolver() =>
            new AutofacWebApiDependencyResolver(container);
    }
}
