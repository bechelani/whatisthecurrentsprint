using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class PullRequestQueueMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partition_id")]
        public string PartitionId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public PullRequestQueueMessage()
        {

        }

        public PullRequestQueueMessage(string id, string partitionId, string type)
        {
            Id = id;
            PartitionId = partitionId;
            Type = type;
        }
    }
}
