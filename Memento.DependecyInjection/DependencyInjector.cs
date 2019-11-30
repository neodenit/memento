using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Memento.DataAccess.Repository;
using Memento.Interfaces;
using Memento.Services;
using Memento.Services.Converter;
using Memento.Services.Evaluators;
using Memento.Services.Scheduler;
using Memento.Services.Validators;

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
            builder.RegisterType<CardOperationService>().As<ICardOperationService>();
            builder.RegisterType<RawCardOperationService>().As<IRawCardOperationService>();
            builder.RegisterType<ConverterService>().As<IConverterService>();

            builder.RegisterType<SchedulerService>().As<ISchedulerService>();
            builder.RegisterType<SchedulerOperationService>().As<ISchedulerOperationService>();
            builder.RegisterType<SchedulerUtilsService>().As<ISchedulerUtilsService>();

            builder.RegisterType<StatisticsService>().As<IStatisticsService>();
            builder.RegisterType<ExportImportService>().As<IExportImportService>();
            builder.RegisterType<PhraseEvaluatorService>().As<IEvaluatorService>();
            builder.RegisterType<ExistenceValidatorService>().As<IValidatorService>().WithParameter("baseValidator", null);

            builder.RegisterType<NewClozesManagerService>().As<INewClozesManagerService>();
            builder.RegisterType<SiblingsManagerService>().As<ISiblingsManagerService>();

            container = builder.Build();
        }

        public static System.Web.Mvc.IDependencyResolver GetMvcResolver() =>
            new AutofacDependencyResolver(container);

        public static System.Web.Http.Dependencies.IDependencyResolver GetWebApiResolver() =>
            new AutofacWebApiDependencyResolver(container);
    }
}
