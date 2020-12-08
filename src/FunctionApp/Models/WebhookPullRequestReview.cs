using System;
using Newtonsoft.Json;
using WhatIsTheCurrentSprint.FunctinoApp.Helpers;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class WebhookPullRequestReview : WebhookPullRequestBase
    {
        public override string Type => Constants.PULL_REQUEST_REVIEW_TYPE;

        [JsonProperty(PropertyName = "commit_id")]
        public string CommitId { get; set; }

        [JsonProperty(PropertyName = "submitted_at")]
        public DateTimeOffset SubmittedAt { get; set; }

        [JsonProperty(PropertyName = "submitted_by")]
        public string SubmittedBy { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}
