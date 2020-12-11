using System;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctionApp.Models
{
    public class WebhookPullRequest
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set;}

        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; } = "open";

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "milestone")]
        public string Milestone { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public string[] Labels { get; set; }

        [JsonProperty(PropertyName = "assignees")]
        public string[] Assignees { get; set; }

        [JsonProperty(PropertyName = "requested_reviewers")]
        public string[] RequestedReviewers { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [JsonProperty(PropertyName = "merged_at")]
        public DateTimeOffset? MergedAt { get; set; }

        [JsonProperty(PropertyName = "created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "draft")]
        public bool? Draft { get; set; }

        [JsonProperty(PropertyName = "merged")]
        public bool? Merged { get; set; }

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

        public WebhookPullRequest()
        {

        }

        public WebhookPullRequest(WebhookPullRequest source)
        {
            Id = source.Id;
            Number = source.Number;
            State = source.State;
            Title = source.Title;
            Body = source.Body;
            Milestone = source.Milestone;
            Labels = source.Labels;
            Assignees = source.Assignees;
            RequestedReviewers = source.RequestedReviewers;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
            ClosedAt = source.ClosedAt;
            MergedAt = source.MergedAt;
            CreatedBy = source.CreatedBy;
            Draft = source.Draft;
            Merged = source.Merged;
            Mergable = source.Mergable;
            MergeableState = source.MergeableState;
            MergedBy = source.MergedBy;
            Url = source.Url;

            if (source.Base != null)
            {
                Base = new GitRef
                {
                    Ref = source.Base.Ref,
                    Sha = source.Base.Sha
                };
            }

            if (source.Head != null)
            {
                Head = new GitRef
                {
                    Ref = source.Head.Ref,
                    Sha = source.Head.Sha
                };
            }
        }
    }
}
