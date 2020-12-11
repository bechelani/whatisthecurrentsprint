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
            Publisher,
            Subscriber
        }

        /// <summary>
        /// Enumeration of all different EventId that can be used for logging
        /// </summary>
        internal enum EventId
        {
            WebhookFunctionProcessingStart = 1000,
            WebhookFunctionProcessingSucceeded = 1001,
            WebhookFunctionProcessingFailed = 1005,
            PullRequestProcessingStart = 1100,
            PullRequestProcessingSucceeded = 1101,
            PullRequestFailedInvalidData = 1105,
            PullRequestFailedUnhandledException = 1106
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
