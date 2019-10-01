﻿using System;
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

            // Add Elcamino Azure Table Identity services.
            services.AddIdentity<ApplicationUser, ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>((options) =>
            {
                options.User.RequireUniqueEmail = true;
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

            // Add functionality to inject IOptions<T>
            services.AddOptions();
            // Add our Config object so it can be injected
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));

            // ISession needs IDistributedCache to store and retrieve items to a persistent medium. 
            // AddDistributedMemoryCache is used to store items in the server memory.
            services.AddDistributedMemoryCache();
            services.AddSession();

            //var applicationSettings = new ApplicationSettings();
            //new ConfigureFromConfigurationOptions<ApplicationSettings>(Configuration.GetSection("AppSettings")).Configure(applicationSettings);
            //services.AddSingleton(applicationSettings);           
            
            // Add application services.
            // aqui

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

         
            // Resolving IIdentitySeed dependency in Startup class
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, IIdentitySeed storageSeed)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // configure the HTTP pipeline to use the session,
            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
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
        }
    }
}
