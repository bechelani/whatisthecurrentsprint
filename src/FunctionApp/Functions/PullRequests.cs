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
        // private readonly IConfiguration _config;
        private readonly CosmosClient _cosmosClient;

        private readonly Database _database;
        private readonly Container _pullRequestsContainer;

        public PullRequests(CosmosClient cosmosClient)
        {
            // log.LogInformation("logging from constructor");
            // this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));

            this._database = _cosmosClient.GetDatabase("GitHub");
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

            //log.LogInformation($"cosmosClient: {_cosmosClient}");

            log.LogInformation("Deserializing queue message.");

            var webhookItem = JsonConvert.DeserializeObject<WebhookItem>(myQueueItem);

            log.LogInformation($"webhookItem: {webhookItem}");

            log.LogInformation($"pull-request function is processing a {webhookItem.Type} message queue item type.");

            PullRequest pullRequest = null;

            try
            {
                // look for open pull request
                log.LogInformation("reading pull request from database. 'open'");
                log.LogInformation($"pullRequest.Id:{webhookItem.PullRequest.Id}");

                pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(webhookItem.PullRequest.Id.ToString(), new PartitionKey("Open"));

                log.LogInformation($"pullRequest: {pullRequest}");
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    log.LogInformation($"pullRequest not found.");
                }
                else
                {
                    log.LogError(ex, $"Could not read item from CosmosDB: error => {ex.Message}");

                    // exit ?
                }
            }

            if (pullRequest == null)
            {
                try
                {
                    // look for closed pull request
                    log.LogInformation("reading pull request from database. 'close'");
                    log.LogInformation($"pullRequest.Id:{webhookItem.PullRequest.Id}");

                    pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(webhookItem.PullRequest.Id.ToString(), new PartitionKey("Closed"));

                    log.LogInformation($"pullRequest: {pullRequest}");
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        log.LogInformation($"pullRequest not found.");
                    }
                    else
                    {
                        log.LogError(ex, $"Could not read item from CosmosDB: error => {ex.Message}");

                        // exit ?
                    }
                }
            }

            if (pullRequest == null)
            {
                // create new pull request
                log.LogInformation("creating new pull request");
                pullRequest = new PullRequest(webhookItem.PullRequest, webhookItem.Repository);
                log.LogInformation($"pullRequest: {pullRequest}");
            }

            if (webhookItem.Type == Constants.PULL_REQUEST_TYPE)
            {
                log.LogInformation("processing pull request type");

                if (pullRequest.State != webhookItem.PullRequest.State)
                {
                    // need to change partition key for item
                }
            }
            else if (webhookItem.Type == Constants.CHECK_RUN_TYPE)
            {
                log.LogInformation("processing check run type");

                pullRequest.CheckRuns.Add(webhookItem.CheckRun);
            }
            else if (webhookItem.Type == Constants.PULL_REQUEST_REVIEW_TYPE)
            {
                log.LogInformation("processing pull request review type");

                pullRequest.Reviews.Add(webhookItem.Review);
            }

            log.LogInformation("updating pull request");
            pullRequest.Update(webhookItem.PullRequest);

            // add webhook
            log.LogInformation("adding webhook to pull request");
            pullRequest.Webhooks.Add(new PullRequestWebhook
            {
                Id = webhookItem.Id,
                Action = webhookItem.Action,
                Processed = DateTimeOffset.UtcNow
            });

            try
            {
                log.LogInformation($"pullRequest.State: {pullRequest.State}");

                if (string.IsNullOrWhiteSpace(pullRequest.State))
                {
                    pullRequest.State = "Open";
                }

                // save pull request to database
                log.LogInformation("saving pull request to cosmos db");

                await cosmosDbOut.AddAsync(pullRequest);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Could not save item into CosmosDB: error => {ex.Message}");
            }

            log.LogInformation("pull-request function finished processing request.");
        }
    }
}
