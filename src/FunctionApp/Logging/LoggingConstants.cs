namespace WhatIsTheCurrentSprint.FunctionApp.Logging
{
    /// <summary>
    /// Contains constants and enums for consistent structured logging
    /// </summary>
    internal static class LoggingConstants
    {
        // Template for consisted structured logging accross multiple functions, each field is described below:
        // EventDescription is a short description of the Event being logged.
        // EntityType: Business Entity Type being processed: e.g. Order, Shipment, etc.
        // EntityId: Id of the Business Entity being processed: e.g. Order Number, Shipment Id, etc.
        // Status: Status of the Log Event, e.g. Succeeded, Failed, Discarded.
        // CorrelationId: Unique identifier of the message that can be processed by more than one component.
        // CheckPoint: To classify and be able to correlate tracking events.
        // Description: A detailed description of the log event.
        internal const string Template = "{EventDescription}, {EntityType}, {EntityId}, {Status}, {CorrelationId}, {CheckPoint}, {Description}";

        internal enum EntityType
        {
            PullRequest
        }

        internal enum CheckPoint
        {
            WebhookFunc,
            PullRequestFunc
        }

        /// <summary>
        /// Enumeration of all different EventId that can be used for logging
        /// </summary>
        internal enum EventId
        {
            WebhookFunctionStart = 1000,
            WebhookFunctionDebug = 1010,
            WebhookFunctionSucceeded = 1020,
            WebhookFunctionError = 1040,
            WebhookFunctionFailed = 1050,
            WebhookFunctionFailedUnhandledException = 1055,
            PullRequestStart = 1100,
            PullRequestDebug = 1110,
            PullRequestSucceeded = 1120,
            PullRequestError = 1140,
            PullRequestFailed = 1150,
            PullRequestFailedInvalidData = 1151,
            PullRequestFailedUnhandledException = 1155
        }

        internal enum Status
        {
            Succeeded,
            Failed,
            Discarded,
            InProgress
        }
    }
}
