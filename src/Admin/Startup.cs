using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatIsTheCurrentSprint.Core.Data;

namespace WhatIsTheCurrentSprint.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
            //     .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services.AddControllersWithViews(options =>
            {
                // var policy = new AuthorizationPolicyBuilder()
                //     .RequireAuthenticatedUser()
                //     .Build();
                // options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.AllowedHosts = Configuration.GetValue<string>("AllowedHosts")?.Split(';').ToList<string>();
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHealthChecks();

            InitializeCosmosClientInstanceAsync(services, Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogError("This is not an error. Configure started.");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // if you've configured it at /admin or /whatever, set that pathbase so ~ will generate correctly
            Uri rootUri = new Uri(Configuration.GetValue<string>("SiteConfiguration:Root"));
            string path = rootUri.AbsolutePath;

            // Deal with path base and proxies that change the request path
            if (path != "/")
            {
                app.Use((context, next) =>
                {
                    context.Request.PathBase = new PathString(path);
                    return next.Invoke();
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHealthChecks("/health");
            });
        }

        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key.
        /// </summary>
        /// <returns></returns>
        private static async Task InitializeCosmosClientInstanceAsync(IServiceCollection services, IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;

            CosmosClient client = new CosmosClient(account, key);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

            // Sprints
            string sprintsContainerName = configurationSection.GetSection("SprintsContainerName").Value;
            SprintInfoService cosmosDbService = new SprintInfoService(client, databaseName, sprintsContainerName);

            await database.Database.CreateContainerIfNotExistsAsync(sprintsContainerName, "/id");

            services.AddSingleton<ISprintInfoService>(cosmosDbService);

            // Triggers
            string triggersContainerName = configurationSection.GetSection("TriggersContainerName").Value;
            TriggerService triggerService = new TriggerService(client, databaseName, triggersContainerName);

            await database.Database.CreateContainerIfNotExistsAsync(triggersContainerName, "/id");

            services.AddSingleton<ITriggerService>(triggerService);
        }
    }
}
