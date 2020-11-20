using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CloudNinja.GitHub.Models;

namespace CloudNinja.GitHub.Functions
{
    public static class WebHook
    {
        [FunctionName("web-hook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "GitHub",
                collectionName: "PullRequests",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<PullRequest> pullRequestsOut,
            ILogger log)
        {
            log.LogInformation("PullRequest function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var zen = (string)data?.zen;
            var repository = (string)data?.repository?.name;
            var action = (string)data?.action;
            var number = (int?)data?.number;

            if (!string.IsNullOrWhiteSpace(zen))
            {
                log.LogInformation($"GitHub Zen: {zen}");
                log.LogInformation("PullRequest function returned 200.");

                return (ActionResult)new OkResult();
            }
            else if (!string.IsNullOrWhiteSpace(repository) && !string.IsNullOrWhiteSpace(action) && number.HasValue)
            {
                var pullRequest = new PullRequest
                {
                    Id = $"{repository}_{number.Value}",
                    Repository = repository,
                    Number = number.Value,
                    Action = action,
                    Milestone = data?.milestone,
                    CreatedBy = data?.pull_request?.user?.login,
                    Created = data?.pull_request?.created_at,
                    Assignees = data?.pull_request?.assignees.ToObject<string[]>()
                };

                await pullRequestsOut.AddAsync(pullRequest);

                log.LogInformation("PullRequest function returned 200.");

                return (ActionResult)new OkResult();
            }
            else
            {
                log.LogInformation("PullRequest function returned 400.");

                return (ActionResult)new BadRequestResult();
            }
        }
    }
}
