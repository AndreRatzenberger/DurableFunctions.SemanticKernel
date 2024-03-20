using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Ex01.Extensions;


namespace DurableFunctions.SemanticKernel.Activities
{
    public class SimplePromptQandAAgent
    {

        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public SimplePromptQandAAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _kernel = Kernel.CreateBuilder()
                .WithOptionsConfiguration(_configurationService.GetCurrentConfiguration())    
                .Build();
        }


        [Function(nameof(Start))]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            var log = context.GetLogger(nameof(SimplePromptQandAAgent));
            await log.LogToExternAsync($"Agent started with input:<br> {input}");
            log.LogInformationWithMetadata($"Agent started with input: {input}");
            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();
            log.LogInformationWithMetadata($"Agent returned with output: {result}");
            await log.LogToExternAsync($"Agent returned with output:<br> {result}");
            return result;
        }

    }
}