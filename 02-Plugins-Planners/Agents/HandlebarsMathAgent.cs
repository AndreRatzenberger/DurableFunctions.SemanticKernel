using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.SemanticKernel.Planning.Handlebars;
using DurableFunctions.SemanticKernel.Agents.Plugins;
using Microsoft.SemanticKernel.Planning;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


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

            await WebCliBridge.SendMessage("Generating prompt...<hr>");
         
            var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });
            var plan = await planner.CreatePlanAsync(_kernel, input);

            await WebCliBridge.SendMessage(plan.Prompt);
            await WebCliBridge.SendMessage("Generating plan...<hr>");
            await WebCliBridge.SendMessage(plan.ToString());
            await WebCliBridge.SendMessage("Executing plan...<hr>");
          
            var result = (await plan.InvokeAsync(_kernel, [])).Trim();

            return result;
        }
    }
}