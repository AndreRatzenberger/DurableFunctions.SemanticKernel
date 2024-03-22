using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Json.More;
using DurableFunctions.SemanticKernel.Agents.Plugins;


namespace DurableFunctions.SemanticKernel.Agents
{
    public class MathAgent : BaseAgent
    {
        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public MathAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            builder.Plugins.AddFromType<MathPlugin>();
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration()).Build();
        }

        [Function($"{nameof(MathAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {

            await WebCliBridge.SendMessage("Generating prompt...<hr>");

            var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });
            var plan = await planner.CreatePlanAsync(_kernel, input);

            await WebCliBridge.SendMessage("Prompt: " + plan.Prompt);
            await WebCliBridge.SendMessage("Generating plan...<hr>");
            await WebCliBridge.SendMessage(plan.ToString());

            await WebCliBridge.SendMessage("Executing plan...<hr>");
            
            var result = (await plan.InvokeAsync(_kernel, [])).Trim();

          
            return result;
        }
    }
}