using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctionApp.Models
{
    public class WebhookRepository
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
