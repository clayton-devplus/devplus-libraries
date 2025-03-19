using System.Text.Json.Serialization;

namespace Devplus.Messaging.Models
{
    public class CloudEvent<T>
    {
        [JsonPropertyName("specversion")]
        public string SpecVersion { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}