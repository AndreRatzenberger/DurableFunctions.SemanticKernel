using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using DurableFunctions.SemanticKernel.Services;


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
        public async Task<string?> Start([ActivityTrigger] string input)
        {
            await WebCliBridge.SendMessage($"## <hr><b>{nameof(SimplePrompAgent)} STARTED</b><hr>");

            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();

            await WebCliBridge.SendMessage($"<br>{result}<br><br><hr>");
            await WebCliBridge.SendMessage($"## <b>{nameof(SimplePrompAgent)}  FINISHED</b><hr>");
            return result;
        }

    }
}