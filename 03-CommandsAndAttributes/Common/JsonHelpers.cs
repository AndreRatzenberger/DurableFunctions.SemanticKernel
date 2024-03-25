using System.Text.Json;

namespace DurableFunctions.SemanticKernel.Common
{
    //Json serialization helpers
    public static class JsonHelpers
    {
        public static string SerializeJsonToMarkdown(object data, JsonSerializerOptions options) => $"```json\n{JsonSerializer.Serialize(data, options)}\n```";
        public static string SerializeJson(object data, JsonSerializerOptions options) => JsonSerializer.Serialize(data, options);
        public static string SerializeJson(object data) => JsonSerializer.Serialize(data);
    }
}



