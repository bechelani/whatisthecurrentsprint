using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using WhatIsTheCurrentSprint.FunctinoApp.Helpers;
using WhatIsTheCurrentSprint.FunctinoApp.Models;

namespace WhatIsTheCurrentSprint.FunctinoApp.Functions
{
    public class WebHook
    {
        [FunctionName("webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "GitHub",
                collectionName: "Webhooks",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<dynamic> cosmosDbOut,
            [Queue("pullrequests"), StorageAccount("AzureWebJobsStorage")] IAsyncCollector<PullRequestQueueMessage> queueOut,
            ILogger log)
        {
            log.LogInformation("webhook function is processing a request.");

            // log.LogInformation($"github secret: {config[Constants.GITHUB_WEBHOOK_SECRET]}");

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
                log.LogError("Did not find X-GitHub-Event header.");
                return (ActionResult) new BadRequestResult();
            }

            var eventName = eventNames.First();

            log.LogInformation($"webhook function is processing a {eventName} event.");

            if (eventName == "pull_request")
            {
                var payload = DeserializePullRequestPayloadAsync(requestBody, log);

                if (payload != null)
                {
                    log.LogDebug("building model");

                    var model = CreatePullRequestModel(payload, log);
                    await cosmosDbOut.AddAsync(model);
                    await queueOut.AddAsync(new PullRequestQueueMessage(model.Id, model.PartitionId, model.Type));
                }
                else
                {
                    log.LogError($"Payload was incorrect.");
                    return (ActionResult) new BadRequestResult();
                }
            }
            else if (eventName == "pull_request_review")
            {
                var payload = DeserializePullRequestReviewPayloadAsync(requestBody, log);

                if (payload != null)
                {
                    log.LogDebug("building model");

                    var model = CreatePullRequestReviewModel(payload, log);
                    await cosmosDbOut.AddAsync(model);
                    await queueOut.AddAsync(new PullRequestQueueMessage(model.Id, model.PartitionId, model.Type));
                }
                else
                {
                    log.LogError($"Payload was incorrect.");
                    return (ActionResult) new BadRequestResult();
                }
            }
            else if (eventName == "check_run")
            {
                var payload = DeserializeCheckRunPayloadAsync(requestBody, log);

                if (payload != null)
                {
                    log.LogDebug("building model");

                    var models = CreateCheckRunModel(payload, log);
                    foreach (var model in models)
                    {
                        await cosmosDbOut.AddAsync(model);
                        await queueOut.AddAsync(new PullRequestQueueMessage(model.Id, model.PartitionId, model.Type));
                    }
                }
                else
                {
                    log.LogError($"Payload was incorrect.");
                    return (ActionResult) new BadRequestResult();
                }
            }
            else if (eventName == "ping")
            {
                var payload = DeserializePingPayloadAsync(requestBody, log);

                log.LogInformation($"Zen: {payload.zen}");
            }
            else
            {
                log.LogError($"{eventName} did not match any event.");
                return (ActionResult) new BadRequestResult();
            }

            log.LogInformation("webhook function returned 200.");
            return (ActionResult) new OkResult();
        }

        private static PullRequestEventPayload DeserializePullRequestPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializePullRequestPayloadAsync");

            var serializer = new SimpleJsonSerializer();

            log.LogDebug($"json:{json}");

            try
            {

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

            var serializer = new SimpleJsonSerializer();

            log.LogDebug($"json:{json}");

            try
            {

                var payload = serializer.Deserialize<PullRequestReviewEventPayload>(json);
                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static StatusEventPayload DeserializeStatusPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializeStatusPayloadAsync");

            var serializer = new SimpleJsonSerializer();

            log.LogDebug($"json:{json}");

            try
            {

                var payload = serializer.Deserialize<StatusEventPayload>(json);
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

        private static CheckRunEventPayload DeserializeCheckRunPayloadAsync(string json, ILogger log)
        {
            log.LogDebug("DeserializeCheckRunPayloadAsync");

            var serializer = new SimpleJsonSerializer();

            log.LogDebug($"json:{json}");

            try
            {
                var payload = serializer.Deserialize<CheckRunEventPayload>(json);
                return payload;
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            return null;
        }

        private static Models.WebhookPullRequestItem CreatePullRequestModel(PullRequestEventPayload payload, ILogger log)
        {
            log.LogDebug("CreatePullRequestModel");

            if (payload.PullRequest == null)
            {
                return null;
            }

            var model = new Models.WebhookPullRequestItem
            {
                Id = Guid.NewGuid().ToString(),
                PartitionId = $"{payload.Repository?.Name}_{payload.Number}",
                Repository = payload.Repository?.Name,
                Number = payload.Number,
                Action = payload.Action,
                Title = payload.PullRequest.Title,
                Milestone = payload.PullRequest.Milestone?.Description,
                Labels = payload.PullRequest.Labels.Select(l => l.Name).ToArray(),
                Assignees = payload.PullRequest.Assignees.Select(a => a.Login).ToArray(),
                RequestedReviewers = payload.PullRequest.RequestedReviewers.Select(r => r.Login).ToArray(),
                CreatedAt = payload.PullRequest.CreatedAt,
                UpdatedAt = payload.PullRequest.UpdatedAt,
                ClosedAt = payload.PullRequest.ClosedAt,
                MergedAt = payload.PullRequest.MergedAt,
                CreatedBy = payload.PullRequest.User?.Login,
                Draft = payload.PullRequest.Draft,
                Merged = payload.PullRequest.Merged,
                Mergable = payload.PullRequest.Mergeable,
                MergeableState = payload.PullRequest.MergeableState?.StringValue,
                MergedBy = payload.PullRequest.MergedBy?.Login,
                Base = new Models.GitRef { Ref = payload.PullRequest.Base.Ref, Sha = payload.PullRequest.Base.Sha },
                Head = new Models.GitRef { Ref = payload.PullRequest.Head.Ref, Sha = payload.PullRequest.Head.Sha },
                Url = payload.PullRequest.HtmlUrl,
                State = payload.PullRequest.State.StringValue,
                Body = payload.PullRequest.Body
            };

            return model;
        }

        private static Models.WebhookPullRequestReview CreatePullRequestReviewModel(PullRequestReviewEventPayload payload, ILogger log)
        {
            log.LogDebug("CreatePullRequestReviewModel");

            if (payload.Review == null || payload.PullRequest == null)
            {
                return null;
            }

            var model = new Models.WebhookPullRequestReview
            {
                Id = Guid.NewGuid().ToString(),
                PartitionId = $"{payload.Repository?.Name}_{payload.PullRequest.Number}",
                Repository = payload.Repository?.Name,
                Number = payload.PullRequest.Number,
                Action = payload.Action,
                CommitId = payload.Review.CommitId,
                SubmittedAt = payload.Review.SubmittedAt,
                SubmittedBy = payload.Review.User.Login,
                Url = payload.PullRequest.HtmlUrl,
                State = payload.PullRequest.State.StringValue,
                Body = payload.PullRequest.Body
            };

            return model;
        }

        private static List<Models.WebhookPullRequestCheckRun> CreateCheckRunModel(CheckRunEventPayload payload, ILogger log)
        {
            log.LogDebug("CreateCheckRunModel");

            if (payload.CheckRun == null)
            {
                return null;
            }

            var models = new List<Models.WebhookPullRequestCheckRun>();

            foreach (var pr in payload.CheckRun.PullRequests)
            {
                var checkRun = new Models.WebhookPullRequestCheckRun
                {
                    Id = Guid.NewGuid().ToString(),
                    PartitionId = $"{payload.Repository?.Name}_{pr.Number}",
                    Repository = payload.Repository?.Name,
                    Number = pr.Number,
                    Action = payload.Action,
                    Name = payload.CheckRun.Name,
                    StartedAt = payload.CheckRun.StartedAt,
                    CompletedAt = payload.CheckRun.CompletedAt,
                    Status = payload.CheckRun.Status.StringValue,
                    Conclusion = payload.CheckRun.Conclusion?.StringValue,
                    Url = payload.CheckRun.HtmlUrl
                };

                models.Add(checkRun);
            }

            return models;
        }

        private static bool CalculateSignature(string body, string secret)
        {
            return true;
        }
    }
}
