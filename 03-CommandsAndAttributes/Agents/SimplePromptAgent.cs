using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extensions;


namespace DurableFunctions.SemanticKernel.Agents
{
    [DFSKAgentName("SimplePromptAgent")]
    [DFSKAgentDescription("A simple agent that just reacts on prompts like ChatGPT.")]
    public class SimplePrompAgent : BaseAgent
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

        [DFSKAgentCommand($"{nameof(SimplePrompAgent)}_Start")]
        [DFSKInput($"A simple prompt")]
        [Function($"{nameof(SimplePrompAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();
            return result;
        }
    }
}