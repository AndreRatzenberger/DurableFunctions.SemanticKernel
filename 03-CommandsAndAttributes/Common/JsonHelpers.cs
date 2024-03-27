using System.Text.Json;

namespace DurableFunctions.SemanticKernel.Common
{
    public static class JsonHelperConfiguration
    {
        private static JsonSerializerOptions? _options;

        public static void Configure(JsonSerializerOptions? options)
        {
            _options = options;
        }
        internal static JsonSerializerOptions? GetOptions() => _options;
    }

    //Json serialization helpers
    public static class JsonHelpers
    {
        public static string SerializeToMarkdown(object data) => $"```json\n{JsonSerializer.Serialize(data, JsonHelperConfiguration.GetOptions())}\n```";
        public static string SerializeToMarkdown(object data, JsonSerializerOptions options) => $"```json\n{JsonSerializer.Serialize(data, options)}\n```";
        public static string Serialize(object data, JsonSerializerOptions options) => JsonSerializer.Serialize(data, options);
        public static string Serialize(object data) => JsonSerializer.Serialize(data, JsonHelperConfiguration.GetOptions());
    }
}



