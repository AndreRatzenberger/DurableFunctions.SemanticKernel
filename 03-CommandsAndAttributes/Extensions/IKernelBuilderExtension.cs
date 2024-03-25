using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.IO; // Add this using statement


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

        internal static IKernelBuilderPlugins AddFromFunctionDirectory(this IKernelBuilderPlugins kernelBuilder, 
                                        string folderPath,
                                        IPromptTemplateFactory? promptTemplateFactory = null,
                                        IServiceProvider? services = null)
        {
            const string ConfigFile = "config.json";
            const string PromptFile = "skprompt.txt";

            ILoggerFactory loggerFactory = services?.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            var factory = promptTemplateFactory ?? new KernelPromptTemplateFactory(loggerFactory);

            string lastFolder = Path.GetFileName(folderPath);
            string promptText = File.ReadAllText(Path.Combine(folderPath, PromptFile));
            string configText = File.ReadAllText(Path.Combine(folderPath, ConfigFile));

            var name = new DirectoryInfo(folderPath).Name;

            var config = PromptTemplateConfig.FromJson(File.ReadAllText(configText));
            config.Template = promptText;
            IPromptTemplate promptTemplateInstance = factory.Create(config);

            var function = KernelFunctionFactory.CreateFromPrompt(promptTemplateInstance, config, loggerFactory);
            var functionList = new List<KernelFunction> { function };
            var plugin = KernelPluginFactory.CreateFromFunctions(lastFolder, null, functionList);

            kernelBuilder.Add(plugin);
            return kernelBuilder;
        }
    }
}
