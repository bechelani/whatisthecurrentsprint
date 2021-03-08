using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public class SprintInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get => "currentSprint"; }

        [Required]
        [JsonProperty(PropertyName = "codeCompleteDate")]
        public DateTime? CodeCompleteDate { get; set; }

        [Required]
        [JsonProperty(PropertyName = "codeFreezeDate")]
        public DateTime? CodeFreezeDate { get; set; }

        [Required]
        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate { get; set; }

        [Required]
        [JsonProperty(PropertyName = "releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        [JsonProperty(PropertyName = "sprint")]
        public string Sprint { get; set; }

        [Required]
        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [Required]
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
