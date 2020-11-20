using System;
using Newtonsoft.Json;

namespace CloudNinja.GitHub.Models
{
    public class PullRequest
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "repository")]
        public string Repository { get; set; }
        
        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }
        
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
        
        [JsonProperty(PropertyName = "milestone")]
        public string Milestone { get; set; }
        
        [JsonProperty(PropertyName = "assignees")]
        public string[] Assignees { get; set; }
        
        [JsonProperty(PropertyName = "created_at")]
        public DateTime Created { get; set; }
        
        [JsonProperty(PropertyName = "created_by")]
        public string CreatedBy { get; set; }
    }
}
