using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using DurableFunctions.SemanticKernel.Services;


namespace DurableFunctions.SemanticKernel.Agents
{

    public class ProjectManagerAgent : BaseAgent
    {

        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;


        public ProjectManagerAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;

            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration()).Build();
        }

        [Function($"{nameof(ProjectManagerAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            await SendMessage("ProjectPlanner STARTED...");
            var responseProjectPlanner = await _kernel.InvokeAsync("Plugins", "ProjectPlanner", new() {
                { "input", input }
            });

            await SendMessage("ComplexityEvaluator STARTED...");
            var responseComplexity = await _kernel.InvokeAsync("Plugins", "ComplexityChecker", new() {
                { "input", responseProjectPlanner }
            });

            await SendMessage(responseProjectPlanner.GetValue<string>());

            return responseComplexity.GetValue<string>();
        }
    }
}