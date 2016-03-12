using Autofac;
using Autofac.Integration.Mvc;
using Memento.Core;
using Memento.Core.Converter;
using Memento.Core.Evaluators;
using Memento.Core.Validators;
using Memento.DomainModel.Repository;
using Memento.Interfaces;
using Memento.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Memento.DependecyInjection
{
    public static class DependencyInjector
    {
        public static IDependencyResolver GetResolver(Assembly assembly)
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(assembly);

            builder.RegisterType<EFMementoRepository>().As<IMementoRepository>().SingleInstance();

            builder.RegisterType<DecksService>().As<IDecksService>().SingleInstance();
            builder.RegisterType<CardsService>().As<ICardsService>().SingleInstance();
            builder.RegisterType<StatisticsService>().As<IStatisticsService>().SingleInstance();
            builder.RegisterType<ExportImportService>().As<IExportImportService>().SingleInstance();
            builder.RegisterType<SchedulerService>().As<ISchedulerService>().SingleInstance();            

            builder.RegisterType<PhraseEvaluator>().As<IEvaluator>();
            builder.RegisterType<Converter>().As<IConverter>();
            builder.RegisterType<ExistenceValidator>().As<IValidator>().WithParameter("baseValidator", null);
            builder.RegisterType<Scheduler>().As<IScheduler>();
            builder.RegisterType<NewClozesManager>().As<INewClozesManager>();
            builder.RegisterType<SiblingsManager>().As<ISiblingsManager>();

            var container = builder.Build();

            return new AutofacDependencyResolver(container);
        }
    }
}
