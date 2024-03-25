using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using Microsoft.SemanticKernel.Planning.Handlebars;
using DurableFunctions.SemanticKernel.Agents.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DurableFunctions.SemanticKernel.Extensions;


namespace DurableFunctions.SemanticKernel.Agents
{
    public class HandlebarsMathAgent : BaseAgent
    {
        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public HandlebarsMathAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            builder.Plugins.AddFromType<MathPlugin>();
            builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration()).Build();
        }

        [Function($"{nameof(HandlebarsMathAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {

            await SendMessage("Generating prompt...<hr>");
         
            var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });
            var plan = await planner.CreatePlanAsync(_kernel, input);

            await SendMessage(plan.Prompt);
            await SendMessage("Generating plan...<hr>");
            await SendMessage(plan.ToString());
            await SendMessage("Executing plan...<hr>");
          
            var result = (await plan.InvokeAsync(_kernel, [])).Trim();

            return result;
        }
    }
}