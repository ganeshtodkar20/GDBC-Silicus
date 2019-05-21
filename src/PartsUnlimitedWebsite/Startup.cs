// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PartsUnlimited.Areas.Admin;
using PartsUnlimited.Models;
using PartsUnlimited.Queries;
using PartsUnlimited.Recommendations;
using PartsUnlimited.Search;
using PartsUnlimited.Security;
using PartsUnlimited.Telemetry;
using PartsUnlimited.WebsiteConfiguration;
using PartsUnlimitedWebsite.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;

namespace PartsUnlimited
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IServiceCollection service { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            service = services;

            services.Configure<ConfigSettings>(Configuration);
            //If this type is present - we're on mono
            var runningOnMono = Type.GetType("Mono.Runtime") != null;
            var sqlConnectionString = Configuration[ConfigurationPath.Combine("ConnectionStrings", "DefaultConnectionString")];
            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);

            if (useInMemoryDatabase || runningOnMono)
            {
                sqlConnectionString = "";
            }

            // Add EF services to the services container
            services.AddDbContext<PartsUnlimitedContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddDbContext<HavokContext>(opt => opt.UseInMemoryDatabase("Havoklist"));


            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<PartsUnlimitedContext>()
                .AddDefaultTokenProviders();

            // Configure admin policies
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AdminConstants.Role,
                    authBuilder =>
                    {
                        authBuilder.RequireClaim(AdminConstants.ManageStore.Name, AdminConstants.ManageStore.Allowed);
                    });

            });

            // Add implementations
            services.AddSingleton<IMemoryCache, NoMemoryCache>();
            services.AddTransient<IOrdersQuery, OrdersQuery>();
            services.AddTransient<IRaincheckQuery, RaincheckQuery>();

            services.AddSingleton<ITelemetryProvider, EmptyTelemetryProvider>();
            services.AddSingleton<PartsUnlimitedScriptsIncluder, PartsUnlimitedScriptsIncluder>();
            services.AddTransient<IProductSearch, StringContainsProductSearch>();

            SetupRecommendationService(services);

            services.AddScoped<IWebsiteOptions>(p =>
            {
                var telemetry = p.GetRequiredService<ITelemetryProvider>();

                return new ConfigurationWebsiteOptions(Configuration.GetSection("WebsiteOptions"), telemetry);
            });

            services.AddScoped<IApplicationInsightsSettings>(p =>
            {
                return new ConfigurationApplicationInsightsSettings(Configuration.GetSection(ConfigurationPath.Combine("Keys", "ApplicationInsights")));
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            // Associate IPartsUnlimitedContext and PartsUnlimitedContext with context
            services.AddTransient<IPartsUnlimitedContext, PartsUnlimitedContext>();
            services.AddTransient<PartsUnlimitedContext, PartsUnlimitedContext>();

            // We need access to these settings in a static extension method, so DI does not help us :(
            ContentDeliveryNetworkExtensions.Configuration = new ContentDeliveryNetworkConfiguration(Configuration.GetSection("CDN"));

            // Add MVC services to the services container
            services.AddMvc();

            //Add InMemoryCache
            services.AddSingleton<IMemoryCache, NoMemoryCache>();

            // Add session related services.
            //services.AddCaching();
            services.AddSession();
        }

        private void SetupRecommendationService(IServiceCollection services)
        {
            var azureMlConfig = new AzureMLFrequentlyBoughtTogetherConfig(Configuration.GetSection(ConfigurationPath.Combine("Keys", "AzureMLFrequentlyBoughtTogether")));

            // If keys are not available for Azure ML recommendation service, register an empty recommendation engine
            if (string.IsNullOrEmpty(azureMlConfig.AccountKey) || string.IsNullOrEmpty(azureMlConfig.ModelName))
            {
                services.AddSingleton<IRecommendationEngine, EmptyRecommendationsEngine>();
            }
            else
            {
                services.AddSingleton<IAzureMLAuthenticatedHttpClient, AzureMLAuthenticatedHttpClient>();
                services.AddSingleton<IAzureMLFrequentlyBoughtTogetherConfig>(azureMlConfig);
                services.AddScoped<IRecommendationEngine, AzureMLFrequentlyBoughtTogetherRecommendationEngine>();
            }
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
        //The allowed values are Development,Staging and Production
        public void ConfigureDevelopment(IApplicationBuilder app,IServiceProvider serviceProvider)
        {
            //Display custom error page in production when error occurs
            //During development use the ErrorPage middleware to display error information in the browser
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();

            Configure(app,serviceProvider);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Staging'
        //The allowed values are Development,Staging and Production
        public void ConfigureStaging(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseExceptionHandler("/Home/Error");
            Configure(app,serviceProvider);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Production'
        //The allowed values are Development,Staging and Production
        public void ConfigureProduction(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseExceptionHandler("/Home/Error");
            Configure(app,serviceProvider);
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {

            var havokcontext = serviceProvider.GetService<HavokContext>();
            if (havokcontext.Havoks.Count() == 0)
            {
                // Create a new TodoItem if collection is empty,
                // which means you can't delete all TodoItems.
                havokcontext.Havoks.Add(new Havok { Name = "Item1" });
                havokcontext.SaveChanges();
            }
            Func<HttpContext, bool> isApiRequest = (HttpContext context) => context.Request.Path.ToString().StartsWith("/api/");

            app.UseWhen(context => !isApiRequest(context), appbuilder => { appbuilder.UseHavokMiddleware(min: TimeSpan.FromMilliseconds(10000), max: TimeSpan.FromMilliseconds(20000)); });



            // Configure Session.
            app.UseSession();

            // Add static files to the request pipeline
            app.UseStaticFiles();

            // Allow static files within the .well-known directory to allow for automatic SSL renewal
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = true, // this was needed as IIS would not serve extensionless URLs from the directory without it
                FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",@".well-known")),
                RequestPath = new PathString("/.well-known")
            });

            // Add cookie-based authentication to the request pipeline
            app.UseAuthentication();

            AppBuilderLoginProviderExtensions.AddLoginProviders(service, new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));
            // Add login providers (Microsoft/AzureAD/Google/etc).  This must be done after `app.UseIdentity()`
            //app.AddLoginProviders( new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });
        }
    }
}