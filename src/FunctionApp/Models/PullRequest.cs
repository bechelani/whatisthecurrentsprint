using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class PullRequest : WebhookPullRequest
    {
        [JsonProperty(PropertyName = "repository")]
        public WebhookRepository Repository { get; set; }

        [JsonProperty(PropertyName = "check_runs")]
        public List<WebhookCheckRun> CheckRuns { get; set; }

        [JsonProperty(PropertyName = "reviews")]
        public List<WebhookPullRequestReview> Reviews { get; set; }

        [JsonProperty(PropertyName = "webhooks")]
        public List<PullRequestWebhook> Webhooks { get; set; }

        public PullRequest()
        {

        }

        public PullRequest(WebhookPullRequest source, WebhookRepository repository)
            : base(source)
        {
            Repository = new WebhookRepository
            {
                Name = repository.Name,
                Url = repository.Url
            };

            CheckRuns = new List<WebhookCheckRun>();
            Reviews = new List<WebhookPullRequestReview>();
            Webhooks = new List<PullRequestWebhook>();
        }

        public void Update(WebhookPullRequest source)
        {
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
