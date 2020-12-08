using System;
using Newtonsoft.Json;
using WhatIsTheCurrentSprint.FunctinoApp.Helpers;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class WebhookPullRequestCheckRun : WebhookPullRequestBase
    {
        public override string Type => Constants.CHECK_RUN_TYPE;

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "started_at")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonProperty(PropertyName = "completed_at")]
        public DateTimeOffset? CompletedAt { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "conclusion")]
        public string Conclusion { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
