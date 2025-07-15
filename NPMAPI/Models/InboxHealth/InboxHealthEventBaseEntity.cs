using System;
using Newtonsoft.Json;
using NPMAPI.Enums;

namespace NPMAPI.Models.InboxHealth
{
    public abstract class InboxHealthEventBaseEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("livemode")]
        public bool LiveMode { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("event_type")]
        public eInboxHealthEvent EventType { get; set; }
    }
}