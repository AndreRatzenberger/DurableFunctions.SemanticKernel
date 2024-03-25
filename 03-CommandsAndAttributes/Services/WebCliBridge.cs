using Microsoft.Azure.Functions.Worker;

namespace DurableFunctions.SemanticKernel.Services
{
    public static class WebCliConfiguration
    {
        private static string? _endpoint;

        public static void Configure(string? endpoint)
        {
            _endpoint = endpoint;
        }

        internal static string? GetEndpoint() => _endpoint;
    }

    public static class WebCliBridge
    {
        private static readonly HttpClient httpClient = new();

        [Function($"{nameof(SendMessage)}")]
        public async static Task SendMessage([ActivityTrigger] string? input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return;
                    
                var endpoint = WebCliConfiguration.GetEndpoint();
                if (string.IsNullOrEmpty(endpoint))
                    return;

                var content = new StringContent(input);

                _ = await httpClient.PostAsync(endpoint, content);
            }
            catch (System.Exception)
            {

            }
        }
    }
}
