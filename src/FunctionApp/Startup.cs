using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatIsTheCurrentSprint.FunctinoApp;
using WhatIsTheCurrentSprint.FunctinoApp.Helpers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace WhatIsTheCurrentSprint.FunctinoApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // builder.Services.AddLogging(loggingBuilder =>
            // {
            //     loggingBuilder.AddFilter(level => true);
            // });

            var config = (IConfiguration)builder.Services.First(d => d.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddSingleton((s) =>
            {
                CosmosClientBuilder cosmosClientBuilder = new CosmosClientBuilder(config[Constants.COSMOS_DB_CONNECTION_STRING]);

                return cosmosClientBuilder
                    .WithConnectionModeDirect()
                    // .WithApplicationRegion("Australia East")
                    .WithBulkExecution(true)
                    .Build();
            });
        }
    }
}
