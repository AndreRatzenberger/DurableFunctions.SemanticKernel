using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;


namespace DurableFunctions.SemanticKernel.Extensions
{
    internal static class KernelBuilderExtensions
    {

        internal static IKernelBuilder WithOptionsConfiguration(this IKernelBuilder kernelBuilder, object options)
        {
            if (options is IOptions<OpenAIOptions> config)
            {
                kernelBuilder
               .AddOpenAIChatCompletion(modelId: config.Value.ModelId, apiKey: config.Value.ApiKey);
               _ = WebCliBridge.SendMessage($"<br>OpenAI model {config.Value.ModelId} loaded");
            }

            if (options is IOptions<AzureOpenAIOptions> azureConfig)
            {
                kernelBuilder
               .AddAzureOpenAIChatCompletion(deploymentName: azureConfig.Value.DeploymentName, endpoint: azureConfig.Value.Endpoint, apiKey: azureConfig.Value.ApiKey);
                _ = WebCliBridge.SendMessage($"<br>Azure OpenAI model {azureConfig.Value.DeploymentName} loaded");
            }

            return kernelBuilder;

        }
    }
}
