using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public class Trigger
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [Required]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        [Required]
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }
    }
}
