using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASC.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using ASC.Web.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authentication.Google;
using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using System.Reflection;
using ASC.Business.Interfaces;
using ASC.Business;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using ASC.Web.Data.Cache;
using Microsoft.Extensions.Logging;
using ASC.Web.Logger;
using ASC.Web.Filters;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace ASC.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // sql server
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDefaultIdentity<IdentityUser>()
            //    .AddDefaultUI(UIFramework.Bootstrap4)                
            //    .AddEntityFrameworkStores<ApplicationDbContext>();



            services.Configure<PasswordHasherOptions>(options =>
                  options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            );
            // Add Elcamino Azure Table Identity services.
            services.AddIdentity<ApplicationUser, ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>((options) =>
            {
                // User settings.
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = String.Empty; //  "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            //or use .AddAzureTableStores with your ApplicationUser extends IdentityUser if your code depends on the Role, Claim and Token collections on the user object.
            //You can safely switch between .AddAzureTableStores and .AddAzureTableStoresV2. Just make sure the Application User extends the correct IdentityUser/IdentityUserV2
            .AddAzureTableStoresV2<ApplicationDbContext>(new Func<IdentityConfiguration>(() =>
            {
                IdentityConfiguration idconfig = new IdentityConfiguration();
                idconfig.TablePrefix = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:TablePrefix").Value;
                idconfig.StorageConnectionString = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:StorageConnectionString").Value;
                idconfig.LocationMode = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:LocationMode").Value;
                return idconfig;
            }))
            .AddDefaultTokenProviders()
            .CreateAzureTablesIfNotExists<ApplicationDbContext>(); //can remove after first run;

            // O código a seguir altera todos os tokens de proteção de dados período de tempo limite para 3 horas:
            //services.Configure<DataProtectionTokenProviderOptions>(o =>
            //           o.TokenLifespan = TimeSpan.FromMinutes(10)); /*TimeSpan.FromHours(3));*/


            services.AddAuthentication().AddGoogle(options =>
            {
                IConfigurationSection googleAuthNSection = Configuration.GetSection("Google:Identity");

                options.ClientId = googleAuthNSection["ClientId"];
                options.ClientSecret = googleAuthNSection["ClientSecret"];

                //options.ClientId = Configuration["Google:Identity:ClientId"];
                //options.ClientSecret = Configuration["Google:Identity:ClientSecret"];
            });

            services.ConfigureApplicationCookie(options =>
            {

                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;

            });


            // Add functionality to inject IOptions<T>
            services.AddOptions();
            // Add our Config object so it can be injected
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));

            // ISession needs IDistributedCache to store and retrieve items to a persistent medium. 
            // AddDistributedMemoryCache is used to store items in the server memory.
            //services.AddDistributedMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetSection("CacheSettings:CacheConnectionString").Value;
                options.InstanceName = Configuration.GetSection("CacheSettings:CacheInstance").Value;
            });

            services.AddSession();

            // Adding Internationalization Support - 
            // Globalization and Localization.Globalization is the process of extending an application to support multiple cultures
            // (languages and regions). Localization is the process of customization of application for a particular culture.
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            //var applicationSettings = new ApplicationSettings();
            //new ConfigureFromConfigurationOptions<ApplicationSettings>(Configuration.GetSection("AppSettings")).Configure(applicationSettings);
            //services.AddSingleton(applicationSettings);           

            // Add application services.            
            // Resolve HttpContextAccessor dependency
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Resolving IUnitOfWork dependency - SCOPED in the entire request cycle
            services.AddScoped<IUnitOfWork>(p => new UnitOfWork(Configuration.GetSection("ConnectionStrings:DefaultConnection").Value));
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();
            services.AddScoped<ILogDataOperations, LogDataOperations>();
            services.AddScoped<CustomExceptionFilter>();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            services.AddScoped<IServiceRequestMessageOperations, ServiceRequestMessageOperations>();
            services.AddScoped<IOnlineUsersOperations, OnlineUsersOperations>();
            services.AddScoped<IPromotionOperations, PromotionOperations>();

            // Resolving IIdentitySeed dependency in Startup class
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // services.Add(new ServiceDescriptor(typeof(IEmailSender), typeof(AuthMessageSender), ServiceLifetime.Transient));

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(typeof(CustomExceptionFilter)); // Add CustomExceptionFilter to filters collection

            })
              // By default, ASP.NET Core MVC serializes JSON data by using camel casing. To prevent that, we need
              // to add the following configuration to MVC in the ConfigureServices method of the Startup class
              .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
              .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => options.ResourcesPath = "Resources")
              .AddDataAnnotationsLocalization(options => { options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResources)); })
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("pt-BR"),
                    new CultureInfo("en")
                };

                options.DefaultRequestCulture = new RequestCulture(culture: "pt-BR", uiCulture: "pt-BR");
                // Formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                options.SupportedUICultures = supportedCultures;

            });


            // If you are using AspNet Core 2.2 and AutoMapper.Extensions.Microsoft.DependencyInjection v6.1 You need to use in Startup file
            // Automapper
            services.AddAutoMapper(typeof(Startup));

            // To consume the left navigation view component, we need to install the Microsoft.Extensions.FileProviders.Embedded NuGet package to the ASC.Web project
            // Add support to embedded views from ASC.Utilities project.
            var assembly = typeof(ASC.Utilities.Navigation.LeftNavigationViewComponent).GetTypeInfo().Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "ASC.Utilities");
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app,
                                    IHostingEnvironment env,
                                    ILoggerFactory loggerFactory,
                                    IIdentitySeed storageSeed,
                                    IUnitOfWork unitOfWork,
                                    IMasterDataCacheOperations masterDataCacheOperations,
                                    INavigationCacheOperations navigationCacheOperations,
                                    ILogDataOperations logDataOperations)
        {


            // Configure Azure Logger to log all events except the ones that are generated by
            //default by ASP.NET Core.
            loggerFactory.AddAzureTableStorageLog(logDataOperations,
                                                 (categoryName, logLevel) => !categoryName.Contains("Microsoft") && logLevel >= LogLevel.Information);

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
                //app.UseDirectoryBrowser();
            }
            else
            {
                // app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // RequestLocalization - Globalization and Localization
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");

            // configure the HTTP pipeline to use the session,
            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Once you've registered all your classes with the DI container (using IServiceCollection), 
            // pretty much all a DI container needs to do is allow you to retrieve an instance of an object using GetService().
            using (var scope = app.ApplicationServices.CreateScope())
            {
                //Resolve ASP .NET Core Identity with DI help
                var userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>));
                var userRole = (RoleManager<ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>)scope.ServiceProvider.GetService(typeof(RoleManager<ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>));

                // do you things here
                await storageSeed.Seed(userManager, userRole, app.ApplicationServices.GetService<IOptions<ApplicationSettings>>());
            }

            // Error: Cannot resolve scoped service 'Microsoft.AspNetCore.Identity.UserManager ApplicationUser from root provider         
            //await storageSeed.Seed(app.ApplicationServices.GetService<UserManager<ApplicationUser>>(),
            //                       app.ApplicationServices.GetService<RoleManager<ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>>(),
            //                       app.ApplicationServices.GetService<IOptions<ApplicationSettings>>());

            var models = Assembly.Load(new AssemblyName("ASC.Models")).GetTypes().Where(type => type.Namespace == "ASC.Models.Models");

            foreach (var model in models)
            {
                var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(model), unitOfWork);
                MethodInfo method = typeof(Repository<>).MakeGenericType(model).GetMethod("CreateTableAsync");
                method.Invoke(repositoryInstance, new object[0]);
            }

            await masterDataCacheOperations.CreateMasterDataCacheAsync();

            await navigationCacheOperations.CreateNavigationCacheAsync();


        }
    }
}
