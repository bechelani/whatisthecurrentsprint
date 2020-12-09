using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WhatIsTheCurrentSprint.FunctinoApp.Helpers;
using WhatIsTheCurrentSprint.FunctinoApp.Models;

namespace WhatIsTheCurrentSprint.FunctinoApp.Functions
{
    public class PullRequests
    {
        private readonly IConfiguration _config;
        private readonly CosmosClient _cosmosClient;

        private readonly Database _database;
        private readonly Container _webhooksContainer;
        private readonly Container _pullRequestsContainer;

        public PullRequests(IConfiguration config, CosmosClient cosmosClient)
        {
            this._config = config;
            this._cosmosClient = cosmosClient;

            this._database = _cosmosClient.GetDatabase(_config[Constants.COSMOS_DB_DATABASE_NAME]);
            this._pullRequestsContainer = _database.GetContainer("PullRequests");
        }

        [FunctionName("pull-requests")]
        public async Task Run(
            [QueueTrigger("pullrequests", Connection = "AzureWebJobsStorage")] string myQueueItem,
            [CosmosDB(
                databaseName: "GitHub",
                collectionName: "PullRequests",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Models.PullRequest> cosmosDbOut,
            ILogger log)
        {
            log.LogInformation($"pull-request function is processing a request.");

            log.LogInformation("Deserializing queue message.");
            var webhookItem = JsonConvert.DeserializeObject<WebhookItem>(myQueueItem);

            log.LogInformation($"pull-request function is processing a {webhookItem.Type} message queue item type.");

            PullRequest pullRequest;

            // look for open pull request
            pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(webhookItem.PullRequest.Id.ToString(), new PartitionKey("Open"));

            if (pullRequest == null)
            {
                // look for closed pull request
                pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(webhookItem.PullRequest.Id.ToString(), new PartitionKey("Closed"));
            }

            if (pullRequest == null)
            {
                // create new pull request
                pullRequest = new PullRequest(webhookItem.PullRequest, webhookItem.Repository);
            }

            if (webhookItem.Type == Constants.PULL_REQUEST_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");

                if (pullRequest.State != webhookItem.PullRequest.State)
                {
                    // need to change partition key for item
                }
            }
            else if (webhookItem.Type == Constants.CHECK_RUN_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");

                pullRequest.CheckRuns.Add(webhookItem.CheckRun);
            }
            else if (webhookItem.Type == Constants.PULL_REQUEST_REVIEW_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");

                pullRequest.Reviews.Add(webhookItem.Review);
            }

            pullRequest.Update(webhookItem.PullRequest);

            // add webhook
            pullRequest.Webhooks.Add(new PullRequestWebhook
            {
                Id = webhookItem.Id,
                Action = webhookItem.Action,
                Processed = DateTimeOffset.UtcNow
            });

            // save pull request to database
            await cosmosDbOut.AddAsync(pullRequest);

            log.LogInformation("pull-request function finished processing request.");
        }
    }
}
