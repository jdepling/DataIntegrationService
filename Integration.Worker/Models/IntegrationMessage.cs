using System.Text.Json;

namespace Integration.Worker.Models
{
    public class IntegrationMessage
    {
        public string SourceId { get; set; } = string.Empty;

        public JsonElement Data { get; set; }
    }
}