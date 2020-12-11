using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WhatIsTheCurrentSprint.FunctionApp.Helpers;
using WhatIsTheCurrentSprint.FunctionApp.Models;
using WhatIsTheCurrentSprint.FunctionApp.Logging;

namespace WhatIsTheCurrentSprint.FunctionApp.Functions
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
            string correlationId = null;

            try
            {
                log.LogDebug("Deserializing queue message.");

                var webhookItem = JsonConvert.DeserializeObject<WebhookItem>(myQueueItem);

                correlationId = webhookItem.CorrelationId;

                log.LogInformation(new EventId((int)LoggingConstants.EventId.PullRequestProcessingStart),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.PullRequestProcessingStart.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    LoggingConstants.Status.InProgress.ToString(),
                    correlationId,
                    LoggingConstants.CheckPoint.Subscriber.ToString(),
                    "pull-request function is processing a request.");

                log.LogDebug($"webhookItem: {webhookItem}");

                log.LogDebug($"pull-request function is processing a {webhookItem.Event} message queue item type.");

                PullRequest pullRequest = await GetPullRequestFromCosmosDb(webhookItem.PullRequest.Id, log);
                if (pullRequest == null)
                {
                    pullRequest = CreateNewPullRequestFromWebhookItem(webhookItem, log);
                }

                if (webhookItem.Event == Constants.PULL_REQUEST_TYPE)
                {
                    ProcessPullRequestEvent(webhookItem, pullRequest, log);
                }
                else if (webhookItem.Event == Constants.CHECK_RUN_TYPE)
                {
                    ProcessCheckRunEvent(webhookItem.CheckRun, pullRequest, log);
                }
                else if (webhookItem.Event == Constants.PULL_REQUEST_REVIEW_TYPE)
                {
                    ProcessPullRequestReviewEvent(webhookItem.Review, pullRequest, log);
                }

                AddWebhookToHistory(webhookItem, pullRequest, log);

                // log.LogDebug($"pullRequest.State: {pullRequest.State}");
                // if (string.IsNullOrWhiteSpace(pullRequest.State))
                // {
                //     pullRequest.State = "open";
                // }

                // save pull request to database
                log.LogDebug("saving pull request to cosmos db");
                await cosmosDbOut.AddAsync(pullRequest);

                log.LogInformation(new EventId((int)LoggingConstants.EventId.PullRequestProcessingSucceeded),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.PullRequestProcessingSucceeded.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    LoggingConstants.Status.Succeeded.ToString(),
                    correlationId,
                    LoggingConstants.CheckPoint.Publisher.ToString(),
                    "pull-request function finished processing request.");
            }
            catch (Exception ex)
            {
                // log an error for an unexcepted exception
                log.LogError(new EventId((int)LoggingConstants.EventId.PullRequestFailedUnhandledException),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.PullRequestFailedUnhandledException.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    LoggingConstants.Status.Failed.ToString(),
                    correlationId,
                    LoggingConstants.CheckPoint.Subscriber.ToString(),
                    $"An unexcepted exception occurred. {ex.Message}");

                throw;
            }
        }

        private async Task<PullRequest> GetPullRequestFromCosmosDb(string pullRequestId, ILogger log)
        {
            log.LogDebug("GetPullRequest enter");
            log.LogDebug($"pullRequestId:{pullRequestId}");

            PullRequest pullRequest = null;

            try
            {
                // look for open pull request
                log.LogDebug("reading pull request from database. 'open'");

                pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(pullRequestId, new PartitionKey("open"));
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    log.LogDebug($"pullRequest not found.");
                }
                else
                {
                    log.LogError(ex, $"Could not read item from CosmosDB: error => {ex.Message}");
                    throw;
                }
            }

            if (pullRequest != null)
            {
                return pullRequest;
            }

            try
            {
                // look for closed pull request
                log.LogDebug("reading pull request from database. 'close'");

                pullRequest = await _pullRequestsContainer.ReadItemAsync<PullRequest>(pullRequestId, new PartitionKey("closed"));
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    log.LogDebug($"pullRequest not found.");
                }
                else
                {
                    log.LogError(ex, $"Could not read item from CosmosDB: error => {ex.Message}");
                    throw;
                }
            }

            return pullRequest;
        }

        private PullRequest CreateNewPullRequestFromWebhookItem(WebhookItem webhookItem, ILogger log)
        {
            log.LogDebug("CreateNewPullRequestFromWebhookItem enter.");

            var pullRequest = new PullRequest(webhookItem.PullRequest, webhookItem.Repository);

            return pullRequest;
        }

        private void ProcessPullRequestEvent(WebhookItem webhookItem, PullRequest pullRequest, ILogger log)
        {
            log.LogDebug("ProcessPullRequestEvent enter.");

            if (pullRequest.State != webhookItem.PullRequest.State)
            {
                // need to change partition key for item

                // delete pullRequest object from Cosmos DB
            }

            log.LogDebug("updating pull request");
            pullRequest.Update(webhookItem.PullRequest);
        }

        private void ProcessPullRequestReviewEvent(WebhookPullRequestReview review, PullRequest pullRequest, ILogger log)
        {
            log.LogDebug("ProcessPullRequestReviewEvent enter.");

            // look for same check run
            var existingReview = pullRequest.Reviews
                            .Where(c => c.Id == review.Id)
                            .FirstOrDefault();

            if (review != null)
            {
                // update existing check run
                existingReview.CommitId = review.CommitId ?? review.CommitId;
                existingReview.SubmittedAt = review.SubmittedAt;
                existingReview.SubmittedBy = review.SubmittedBy ?? review.SubmittedBy;
                existingReview.State = review.State ?? review.State;
                existingReview.Body = review.Body ?? review.Body;
                existingReview.Url = review.Url ?? review.Url;
            }
            else
            {
                // simply add new review
                pullRequest.Reviews.Add(review);
            }
        }

        private void ProcessCheckRunEvent(WebhookCheckRun checkRun, PullRequest pullRequest, ILogger log)
        {
            log.LogDebug("ProcessCheckRunEvent enter.");

            // look for same check run
            var existingCheckRun = pullRequest.CheckRuns
                            .Where(c => c.Id == checkRun.Id)
                            .FirstOrDefault();

            if (checkRun != null)
            {
                // update existing check run
                existingCheckRun.HeadSha = checkRun.HeadSha ?? checkRun.HeadSha;
                existingCheckRun.Name = checkRun.Name ?? checkRun.Name;
                existingCheckRun.CompletedAt = checkRun.CompletedAt ?? (DateTimeOffset?)null;
                existingCheckRun.Status = checkRun.Status ?? checkRun.Status;
                existingCheckRun.Conclusion = checkRun.Conclusion ?? checkRun.Conclusion;
                existingCheckRun.Url = checkRun.Url ?? checkRun.Url;
            }
            else
            {
                // simply add new check run
                pullRequest.CheckRuns.Add(checkRun);
            }
        }

        private void AddWebhookToHistory(WebhookItem webhookItem, PullRequest pullRequest, ILogger log)
        {
            // add webhook to history
            log.LogDebug("AddWebhookToHistory enter");

            pullRequest.Webhooks.Add(new PullRequestWebhook
            {
                Id = webhookItem.Id,
                PartitionId = webhookItem.PartitionId,
                CorrelationId = webhookItem.CorrelationId,
                Event = webhookItem.Event,
                Action = webhookItem.Action,
                Processed = DateTimeOffset.UtcNow
            });
        }
    }
}
