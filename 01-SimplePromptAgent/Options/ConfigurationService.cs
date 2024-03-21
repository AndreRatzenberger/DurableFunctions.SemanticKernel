using Microsoft.Extensions.Options;

namespace DurableFunctions.SemanticKernel.Options
{
    public class ConfigurationService(IOptions<OpenAIOptions> openAIOptions, IOptions<AzureOpenAIOptions> azureOpenAIOptions)
    {
        private readonly IOptions<OpenAIOptions> _openAIOptions = openAIOptions;
        private readonly IOptions<AzureOpenAIOptions> _azureOpenAIOptions = azureOpenAIOptions;

        public object GetCurrentConfiguration()
        {
            _ = bool.TryParse(Environment.GetEnvironmentVariable("UseAzureOpenAIOptions"), out bool isAzureOpenAI);
            if (isAzureOpenAI)
                return _azureOpenAIOptions.Value;
            else
                return _openAIOptions.Value;
        }
    }

  
}