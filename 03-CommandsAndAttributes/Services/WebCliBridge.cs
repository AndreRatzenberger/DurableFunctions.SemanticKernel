using Microsoft.Azure.Functions.Worker;

namespace DurableFunctions.SemanticKernel.Services
{
    public static class WebCliConfiguration
    {
        private static string? _endpoint;
        private static bool? _isVerbose;

        public static void Configure(string? endpoint, bool isVerbose = false)
        {
            _endpoint = endpoint;
            _isVerbose = isVerbose;
        }

        internal static string? GetEndpoint() => _endpoint;
        internal static bool? GetIsVerbose() => _isVerbose;

    }

    public static class WebCliBridge
    {
        private static readonly HttpClient httpClient = new();

        [Function($"{nameof(SendLogMessage)}")]
        public async static Task SendLogMessage([ActivityTrigger] string? input)
        {
            try
            {
                if(WebCliConfiguration.GetIsVerbose() == true)
                   await SendMessage($"<p style='color:Tomato;'>{input}</p>");
            }
            catch (System.Exception)
            {

            }
        }

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
