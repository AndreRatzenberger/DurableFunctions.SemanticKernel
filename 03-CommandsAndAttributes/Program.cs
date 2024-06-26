using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Services;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Common;
using System.Text.Json;
using DurableFunctions.SemanticKernel.Commands.Registry;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostingContext, services) =>
    {

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.Configure<OpenAIOptions>(hostingContext.Configuration.GetSection(nameof(OpenAIOptions)));
        services.Configure<AzureOpenAIOptions>(hostingContext.Configuration.GetSection(nameof(AzureOpenAIOptions)));
        services.AddSingleton<SimplePrompAgent>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<CommandRegistry>();
        services.AddLogging();

        JsonHelperConfiguration.Configure(new JsonSerializerOptions { WriteIndented = true });
        LoggerConfiguration.ConfigureLogger("DURABLE AI");
        WebCliConfiguration.Configure(Environment.GetEnvironmentVariable("ExternalLogEndpoint"));
    })
    .Build();

host.Run();
