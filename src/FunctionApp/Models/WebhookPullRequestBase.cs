using System;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public abstract class WebhookPullRequestBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partition_id")]
        public string PartitionId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public abstract string Type { get; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public string Repository { get; set; }

        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }
    }
}
