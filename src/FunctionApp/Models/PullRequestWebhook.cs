using System;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class PullRequestWebhook
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "processed")]
        public DateTimeOffset Processed { get; set; }
    }
}
