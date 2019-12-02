using System.Reflection;
using Memento.DataAccess;
using Memento.DataAccess.Repository;
using Memento.Interfaces;
using Memento.Services;
using Memento.Services.Converter;
using Memento.Services.Evaluators;
using Memento.Services.Scheduler;
using Memento.Services.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Neodenit.Memento.Web.Controllers;
using Neodenit.Memento.Web.Data;

namespace Neodenit.Memento.DependecyInjection
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MementoContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IdentityContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddRazorPages();
            services.AddControllersWithViews();

            services.AddScoped<IMementoRepository, EFMementoRepository>();

            services.AddTransient<IDecksService, DecksService>();

            services.AddTransient<ICardsService, CardsService>();
            services.AddTransient<ICardOperationService, CardOperationService>();
            services.AddTransient<IRawCardOperationService, RawCardOperationService>();
            services.AddTransient<IConverterService, ConverterService>();

            services.AddTransient<ISchedulerService, SchedulerService>();
            services.AddTransient<ISchedulerOperationService, SchedulerOperationService>();
            services.AddTransient<ISchedulerUtilsService, SchedulerUtilsService>();

            services.AddTransient<IStatisticsService, StatisticsService>();
            services.AddTransient<IExportImportService, ExportImportService>();
            services.AddTransient<IEvaluatorService, PhraseEvaluatorService>();
            services.AddTransient<IValidatorService, ExistenceValidatorService>();

            services.AddTransient<INewClozesManagerService, NewClozesManagerService>();
            services.AddTransient<ISiblingsManagerService, SiblingsManagerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            var personAssembly = typeof(HomeController).GetTypeInfo().Assembly;
            var personEmbeddedFileProvider = new EmbeddedFileProvider(
                personAssembly,
                "Neodenit.Memento.Web.wwwroot"
            );

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = personEmbeddedFileProvider
            });
        }
    }
}
