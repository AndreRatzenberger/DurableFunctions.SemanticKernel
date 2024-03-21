using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;


namespace DurableFunctions.SemanticKernel.Agents
{
    public class SimplePrompAgent
    {

        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public SimplePrompAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _kernel = Kernel.CreateBuilder()
                .WithOptionsConfiguration(_configurationService.GetCurrentConfiguration())    
                .Build();
        }


        [Function($"{nameof(SimplePrompAgent)}_{nameof(Start)}")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            var log = context.GetLogger(nameof(SimplePrompAgent));
            await log.LogToExternAsync($"## <hr><b>{nameof(SimplePrompAgent)} STARTED</b><hr>");

            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();

            await log.LogToExternAsync($"<br>{result}<br><br><hr>");
            await log.LogToExternAsync($"## <b>{nameof(SimplePrompAgent)}  FINISHED</b><hr>");
            return result;
        }

    }
}