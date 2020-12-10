using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatIsTheCurrentSprint.FunctionApp;
using WhatIsTheCurrentSprint.FunctionApp.Helpers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace WhatIsTheCurrentSprint.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //var config = (IConfiguration)builder.Services.First(d => d.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddSingleton((s) =>
            {
                // CosmosClientBuilder cosmosClientBuilder = new CosmosClientBuilder(config[Constants.COSMOS_DB_CONNECTION_STRING]);
                var cosmosClientBuilder = new CosmosClientBuilder("AccountEndpoint=https://mbb-cosmodb.documents.azure.com:443/;AccountKey=3cNUvfuZmEprq7XC0HxJh3ucfW5PUB3zs00MFDZFMW4oXxQAKNpa1WLgkjteSJA03eGrNh9123da6vQIHIFmTA==;");

                return cosmosClientBuilder
                    .WithConnectionModeDirect()
                    // .WithApplicationRegion("Australia East")
                    .WithBulkExecution(true)
                    .Build();
            });

            builder.Services.AddLogging();
        }
    }
}
