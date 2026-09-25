using System.Text.Json;

namespace Integration.Api
{
    public class IntegrationMessageRequest
    {
        public string SourceId { get; set; }
        public JsonElement Data { get; set; }
    }
}
