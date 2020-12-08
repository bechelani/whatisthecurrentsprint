using System;
using Newtonsoft.Json;

namespace WhatIsTheCurrentSprint.FunctinoApp.Models
{
    public class GitRef
    {
        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        [JsonProperty(PropertyName = "sha")]
        public string Sha { get; set; }
    }
}
