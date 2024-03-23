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
            var responseProjectPlanner = await _kernel.InvokeAsync("Plugins", "ProjectPlanner", new() {
                { "input", input }
            });

            var html = await _kernel.InvokeAsync("Plugins", "AnythingToHtml", new() {
                { "inputFile", responseProjectPlanner.GetValue<string>() }
            });

            var htmlString =html.GetValue<string>();
            await SendMessage(htmlString);

            var responseComplexity = await _kernel.InvokeAsync("Plugins", "ComplexityChecker", new() {
                { "input", responseProjectPlanner }
            });

            var result = responseComplexity.GetValue<string>();

            html = await _kernel.InvokeAsync("Plugins", "AnythingToHtml", new() {
                { "inputFile", result}
            });
            result = html.GetValue<string>();
            return result;
        }
    }
}