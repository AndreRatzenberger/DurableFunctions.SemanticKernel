using DurableFunctions.SemanticKernel.Options;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;


namespace DurableFunctions.SemanticKernel.Extentions
{
    internal static class KernelBuilderExtensions
    {

        internal static IKernelBuilder WithOptionsConfiguration(this IKernelBuilder kernelBuilder, object options)
        {
            if (options is IOptions<OpenAIOptions> config)
            {
                kernelBuilder
               .AddOpenAIChatCompletion(modelId: config.Value.ModelId, apiKey: config.Value.ApiKey);
            }

            if (options is IOptions<AzureOpenAIOptions> azureConfig)
            {
                kernelBuilder
               .AddAzureOpenAIChatCompletion(deploymentName: azureConfig.Value.DeploymentName, endpoint: azureConfig.Value.Endpoint, apiKey: azureConfig.Value.ApiKey);
            }

            return kernelBuilder;

        }
    }
}
