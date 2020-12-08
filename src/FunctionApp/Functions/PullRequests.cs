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
            this._webhooksContainer = _database.GetContainer(_config[Constants.COSMOS_DB_CONTAINER_NAME]);
            this._pullRequestsContainer = _database.GetContainer("PullRequests");
        }

        [FunctionName("pull-requests")]
        public async Task Run(
            [QueueTrigger("pullrequests", Connection = "AzureWebJobsStorage")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"pull-request function is processing a request.");

            log.LogInformation("Deserializing queue message.");
            var messageQueueItem = JsonConvert.DeserializeObject<PullRequestQueueMessage>(myQueueItem);

            log.LogInformation($"pull-request function is processing a {messageQueueItem.Type} message queue item type.");

            if (messageQueueItem.Type == Constants.PULL_REQUEST_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");
                var webhookItem = await _webhooksContainer.ReadItemAsync<WebhookPullRequestItem>(messageQueueItem.Id, new PartitionKey(messageQueueItem.PartitionId));

            }
            else if (messageQueueItem.Type == Constants.CHECK_RUN_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");
                var webhookItem = await _webhooksContainer.ReadItemAsync<WebhookPullRequestCheckRun>(messageQueueItem.Id, new PartitionKey(messageQueueItem.PartitionId));
            }
            else if (messageQueueItem.Type == Constants.PULL_REQUEST_REVIEW_TYPE)
            {
                log.LogInformation("Getting webhook item from cosmos db.");
                var webhookItem = await _webhooksContainer.ReadItemAsync<WebhookPullRequestReview>(messageQueueItem.Id, new PartitionKey(messageQueueItem.PartitionId));
            }

            log.LogInformation("pull-request function finished processing request.");
        }

        private static void ProcessPullRequestModel(ILogger log)
        {

        }

        private static void ProcessPullRequestModel()
        {

        }
    }
}
