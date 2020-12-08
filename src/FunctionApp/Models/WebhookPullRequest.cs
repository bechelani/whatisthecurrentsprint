using System;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class WebhookPullRequest
    {
        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "milestone")]
        public string Milestone { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public string[] Labels { get; set; }

        [JsonProperty(PropertyName = "assignees")]
        public string[] Assignees { get; set; }

        [JsonProperty(PropertyName = "requested_reviewers")]
        public string[] RequestedReviewers { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonProperty(PropertyName = "merged_at")]
        public DateTimeOffset? MergedAt { get; set; }

        [JsonProperty(PropertyName = "created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "draft")]
        public bool Draft { get; set; }

        [JsonProperty(PropertyName = "merged")]
        public bool Merged { get; set; }

        [JsonProperty(PropertyName = "mergable")]
        public bool? Mergable { get; set; }

        [JsonProperty(PropertyName = "mergeable_state")]
        public string MergeableState { get; set; }

        [JsonProperty(PropertyName = "merged_by")]
        public string MergedBy { get; set; }

        [JsonProperty(PropertyName = "base")]
        public GitRef Base { get; set; }

        [JsonProperty(PropertyName = "head")]
        public GitRef Head { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}
