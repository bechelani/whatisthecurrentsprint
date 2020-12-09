using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class WebhookItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partition_id")]
        public string PartitionId { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public WebhookRepository Repository { get; set; }

        [JsonProperty(PropertyName = "pull_request")]
        public WebhookPullRequest PullRequest { get; set; }

        [JsonProperty(PropertyName = "review")]
        public WebhookPullRequestReview Review { get; set; }

        [JsonProperty(PropertyName = "check_run")]
        public WebhookCheckRun CheckRun { get; set; }
    }
}
