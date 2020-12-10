using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;
using WhatIsTheCurrentSprint.FunctionApp.Helpers;
using WhatIsTheCurrentSprint.FunctionApp.Logging;

namespace WhatIsTheCurrentSprint.FunctionApp.Functions
{
    public class WebHook
    {
        /// <summary>
        /// Sample Function to show structured and correlated logging on Azure Functions using ILogger.
        /// Triggered by an Http post and drops a message into a Azure Storage message queue.
        /// Checks whether an order is valid and logs an event accordingly
        /// </summary>
        /// <param name="req">Expects an order in the JSON format, with the 'orderNumber' property</param>
        /// <param name="outMessage"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "GitHub",
                collectionName: "Webhooks",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Models.WebhookItem> cosmosDbOut,
            [Queue("pullrequests"), StorageAccount("AzureWebJobsStorage")] IAsyncCollector<Models.WebhookItem> queueOut,
            ILogger log)
        {
            string correlationId = Guid.NewGuid().ToString();

            try
            {
                log.LogInformation(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.WebhookFunctionProcessingStart.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    null,
                    correlationId,
                    LoggingConstants.CheckPoint.Publisher.ToString(),
                    "webhook function is processing a request.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                // // check secret
                // if (req.Headers.TryGetValue("X-Hub-Signature-256", out var signatures))
                // {
                //     var signature = signatures.First();

                //     if (!CalculateSignature(requestBody, config.GetValue<string>("GitHubSecret")))
                //     {
                //         return (ActionResult) new BadRequestResult();
                //     }
                // }

                // check what event was triggered
                if (!req.Headers.TryGetValue("X-GitHub-Event", out var eventNames))
                {
                    log.LogError(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.WebhookFunctionProcessingFailed.ToString(),
                        LoggingConstants.EntityType.PullRequest.ToString(),
                        null,
                        null,
                        correlationId,
                        LoggingConstants.CheckPoint.Publisher.ToString(),
                        "Did not find X-GitHub-Event header.");

                    return (ActionResult) new BadRequestResult();
                }

                var eventName = eventNames.First();

                log.LogDebug($"webhook function is processing a {eventName} event.");

                if (eventName == "pull_request")
                {
                    var payload = DeserializePullRequestPayloadAsync(requestBody, log);

                    if (payload != null)
                    {
                        var model = CreateWebhookItemModel(payload, correlationId, log);

                        log.LogDebug("adding model to cosmosDb");
                        await cosmosDbOut.AddAsync(model);

                        log.LogDebug("adding model to queue");
                        await queueOut.AddAsync(model);
                    }
                    else
                    {
                        log.LogError(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                            LoggingConstants.Template,
                            LoggingConstants.EventId.WebhookFunctionProcessingFailed.ToString(),
                            LoggingConstants.EntityType.PullRequest.ToString(),
                            null,
                            LoggingConstants.Status.Failed.ToString(),
                            correlationId,
                            LoggingConstants.CheckPoint.Publisher.ToString(),
                            $"Payload was incorrect.");

                        return (ActionResult) new BadRequestResult();
                    }
                }
                else if (eventName == "pull_request_review")
                {
                    var payload = DeserializePullRequestReviewPayloadAsync(requestBody, log);

                    if (payload != null)
                    {
                        var model = CreateWebhookItemModel(payload, correlationId, log);

                        log.LogDebug("adding model to cosmosDb");
                        await cosmosDbOut.AddAsync(model);

                        log.LogDebug("adding model to queue");
                        await queueOut.AddAsync(model);
                    }
                    else
                    {
                        log.LogError(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                            LoggingConstants.Template,
                            LoggingConstants.EventId.WebhookFunctionProcessingFailed.ToString(),
                            LoggingConstants.EntityType.PullRequest.ToString(),
                            null,
                            LoggingConstants.Status.Failed.ToString(),
                            correlationId,
                            LoggingConstants.CheckPoint.Publisher.ToString(),
                            $"Payload was incorrect.");

                        return (ActionResult) new BadRequestResult();
                    }
                }
                else if (eventName == "check_run")
                {
                    var payload = DeserializeCheckRunPayloadAsync(requestBody, log);

                    if (payload != null)
                    {
                        var models = CreateWebhookItemModel(payload, correlationId, log);

                        foreach (var model in models)
                        {
                            log.LogDebug("adding model to cosmosDb");
                            await cosmosDbOut.AddAsync(model);

                            log.LogDebug("adding model to queue");
                            await queueOut.AddAsync(model);
                        }
                    }
                    else
                    {
                        log.LogError(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                            LoggingConstants.Template,
                            LoggingConstants.EventId.WebhookFunctionProcessingFailed.ToString(),
                            LoggingConstants.EntityType.PullRequest.ToString(),
                            null,
                            LoggingConstants.Status.Failed.ToString(),
                            correlationId,
                            LoggingConstants.CheckPoint.Publisher.ToString(),
                            $"Payload was incorrect.");

                        return (ActionResult) new BadRequestResult();
                    }
                }
                else if (eventName == "ping")
                {
                    var payload = DeserializePingPayloadAsync(requestBody, log);

                    log.LogDebug($"Zen: {payload.zen}");
                }
                else
                {
                    log.LogError($"{eventName} did not match any event.");
                    return (ActionResult) new BadRequestResult();
                }

                log.LogInformation(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingStart),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.WebhookFunctionProcessingSucceeded.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    LoggingConstants.Status.Succeeded.ToString(),
                    correlationId,
                    LoggingConstants.CheckPoint.Publisher.ToString(),
                    "webhook function returned 200.");

                return (ActionResult) new OkResult();
            }
            catch (Exception ex)
            {
                // log an error for an unexcepted exception
                log.LogError(new EventId((int)LoggingConstants.EventId.WebhookFunctionProcessingFailed),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.WebhookFunctionProcessingFailed.ToString(),
                    LoggingConstants.EntityType.PullRequest.ToString(),
                    null,
                    LoggingConstants.Status.Failed.ToString(),
                    correlationId,
                    LoggingConstants.CheckPoint.Publisher.ToString(),
                    $"An unexcepted exception occurred. {ex.Message}");

                throw;
            }
        }

        private static PullRequestEventPayload DeserializePullRequestPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializePullRequestPayloadAsync");

            try
            {
                var serializer = new SimpleJsonSerializer();
                var payload = serializer.Deserialize<PullRequestEventPayload>(json);

                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static PullRequestReviewEventPayload DeserializePullRequestReviewPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializePullRequestReviewPayloadAsync");

            try
            {
                var serializer = new SimpleJsonSerializer();
                var payload = serializer.Deserialize<PullRequestReviewEventPayload>(json);

                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static CheckRunEventPayload DeserializeCheckRunPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializeCheckRunPayloadAsync");

            try
            {
                var serializer = new SimpleJsonSerializer();
                var payload = serializer.Deserialize<CheckRunEventPayload>(json);

                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static dynamic DeserializePingPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializePingPayloadAsync");

            var serializer = new SimpleJsonSerializer();

            log.LogDebug($"json:{json}");

            try
            {
                var payload = serializer.Deserialize<dynamic>(json);

                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static Models.WebhookItem CreateWebhookItemModel(PullRequestEventPayload payload, string correlationId, ILogger log)
        {
            log.LogDebug("CreateWebhookItemModel: PullRequestEventPayload");

            if (payload.PullRequest == null)
            {
                return null;
            }

            var model = new Models.WebhookItem
            {
                Id = Guid.NewGuid().ToString(),
                PartitionId = $"{payload.Repository?.Name}_{payload.Number}",
                CorrelationId = correlationId,
                Action = payload.Action,
                Event = Constants.PULL_REQUEST_TYPE,
                Repository = CreateWebhookRepositoryModel(payload.Repository, log),
                PullRequest = CreateWebhookPullRequestModel(payload.PullRequest, log)
            };

            return model;
        }

        private static Models.WebhookItem CreateWebhookItemModel(PullRequestReviewEventPayload payload, string correlationId, ILogger log)
        {
            log.LogDebug("CreateWebhookItemModel: PullRequestReviewEventPayload");

            if (payload.PullRequest == null || payload.Review == null)
            {
                return null;
            }

            var model = new Models.WebhookItem
            {
                Id = Guid.NewGuid().ToString(),
                PartitionId = $"{payload.Repository?.Name}_{payload.PullRequest.Number}",
                CorrelationId = correlationId,
                Action = payload.Action,
                Event = Constants.PULL_REQUEST_REVIEW_TYPE,
                Repository = CreateWebhookRepositoryModel(payload.Repository, log),
                PullRequest = CreateWebhookPullRequestModel(payload.PullRequest, log),
                Review = CreateWebhookPullRequestReviewModel(payload.Review, log)
            };

            return model;
        }

        private static List<Models.WebhookItem> CreateWebhookItemModel(CheckRunEventPayload payload, string correlationId, ILogger log)
        {
            log.LogDebug("CreateWebhookItemModel: CheckRunEventPayload");

            if (payload.CheckRun == null)
            {
                log.LogDebug("payload.CheckRun is null");
                return null;
            }

            var models = new List<Models.WebhookItem>();

            log.LogDebug($"payload.CheckRun.PullRequests: {payload.CheckRun.PullRequests.Count}");

            foreach (var pullRequest in payload.CheckRun.PullRequests)
            {
                log.LogDebug("creating model");

                var model = new Models.WebhookItem
                {
                    Id = Guid.NewGuid().ToString(),
                    PartitionId = $"{payload.Repository?.Name}_{pullRequest.Number}",
                    CorrelationId = correlationId,
                    Action = payload.Action,
                    Event = Constants.CHECK_RUN_TYPE,
                    Repository = CreateWebhookRepositoryModel(payload.Repository, log),
                    PullRequest = CreateWebhookPullRequestModel(pullRequest, log),
                    CheckRun = CreateWebhookCheckRunModel(payload.CheckRun, log)
                };

                models.Add(model);
            }

            return models;
        }

        private static Models.WebhookRepository CreateWebhookRepositoryModel(Repository repository, ILogger log)
        {
            log.LogDebug("CreateWebhookRepositoryModel");

            if (repository== null)
            {
                return null;
            }

            var model = new Models.WebhookRepository
            {
                Name = repository.Name,
                Url = repository.HtmlUrl
            };

            return model;
        }

        private static Models.WebhookPullRequest CreateWebhookPullRequestModel(PullRequest pullRequest, ILogger log)
        {
            log.LogDebug("CreateWebhookPullRequestModel");

            if (pullRequest== null)
            {
                return null;
            }

            var model = new Models.WebhookPullRequest
            {
                Id = pullRequest.Id.ToString(),
                Number = pullRequest.Number,
                Title = pullRequest.Title,
                Milestone = pullRequest.Milestone?.Title,
                Labels = pullRequest.Labels?.Select(l => l.Name).ToArray(),
                Assignees = pullRequest.Assignees?.Select(a => a.Login).ToArray(),
                RequestedReviewers = pullRequest.RequestedReviewers?.Select(r => r.Login).ToArray(),
                CreatedAt = pullRequest.CreatedAt == default(DateTimeOffset) ? (DateTimeOffset?)null : pullRequest.CreatedAt,
                UpdatedAt = pullRequest.UpdatedAt == default(DateTimeOffset) ? (DateTimeOffset?)null : pullRequest.UpdatedAt,
                ClosedAt = pullRequest.ClosedAt,
                MergedAt = pullRequest.MergedAt,
                CreatedBy = pullRequest.User?.Login,
                Draft = pullRequest.Draft,
                Merged = pullRequest.Merged,
                Mergable = pullRequest.Mergeable,
                MergeableState = pullRequest.MergeableState?.StringValue,
                MergedBy = pullRequest.MergedBy?.Login,
                Base = new Models.GitRef { Ref = pullRequest.Base?.Ref, Sha = pullRequest.Base?.Sha },
                Head = new Models.GitRef { Ref = pullRequest.Head?.Ref, Sha = pullRequest.Head?.Sha },
                Url = pullRequest.HtmlUrl,
                State = pullRequest.State.StringValue,
                Body = pullRequest.Body
            };

            return model;
        }

        private static Models.WebhookPullRequestReview CreateWebhookPullRequestReviewModel(PullRequestReview pullRequestReview, ILogger log)
        {
            log.LogDebug("CreateWebhookPullRequestReviewModel");

            if (pullRequestReview== null)
            {
                return null;
            }

            var model = new Models.WebhookPullRequestReview
            {
                Url = pullRequestReview.HtmlUrl,
                State = pullRequestReview.State.StringValue,
                Body = pullRequestReview.Body
            };

            return model;
        }

        private static Models.WebhookCheckRun CreateWebhookCheckRunModel(CheckRun checkRun, ILogger log)
        {
            log.LogDebug("CreateWebhookCheckRunModel");

            if (checkRun== null)
            {
                return null;
            }

            var model = new Models.WebhookCheckRun
            {
                Name = checkRun.Name,
                StartedAt = checkRun.StartedAt,
                CompletedAt = checkRun.CompletedAt,
                Status = checkRun.Status.StringValue,
                Conclusion = checkRun.Conclusion?.StringValue,
                Url = checkRun.HtmlUrl
            };

            return model;
        }

        private static bool CalculateSignature(string body, string secret)
        {
            return true;
        }
    }
}
