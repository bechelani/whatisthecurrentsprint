namespace WhatIsTheCurrentSprint.FunctionApp.Helpers
{
    public class Constants
    {
        public const string COSMOS_DB_CONNECTION_STRING = "CosmosDBConnection";
        public const string COSMOS_DB_DATABASE_NAME = "CosmosDBContainerName";
        public const string COSMOS_DB_CONTAINER_NAME = "CosmosDBContainerName";
        public const string GITHUB_WEBHOOK_SECRET = "GitHubWebhookSecret";

        public const string PULL_REQUEST_TYPE = "pull_request";
        public const string PULL_REQUEST_REVIEW_TYPE = "pull_request_review";
        public const string CHECK_RUN_TYPE = "check_run";
    }
}
