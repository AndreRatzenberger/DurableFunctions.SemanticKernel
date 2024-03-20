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
            await log.LogToExternAsync($"<hr><b>{nameof(SimplePromptQandAAgent)} STARTED</b><hr>");

            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();

            await log.LogToExternAsync($"<br>{result}<hr>");
            await log.LogToExternAsync($"<b>{nameof(SimplePromptQandAAgent)}  FINISHED</b><hr>");
            return result;
        }

    }
}