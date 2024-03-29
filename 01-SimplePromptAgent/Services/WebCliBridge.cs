using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

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

        public async static Task SendMessage(string input)
        {
            try
            {
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
