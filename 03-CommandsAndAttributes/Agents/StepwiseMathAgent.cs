using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using Microsoft.SemanticKernel.Planning;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core;
using DurableFunctions.SemanticKernel.Extensions;


namespace DurableFunctions.SemanticKernel.Agents
{
    [DFSKAgentName("StepwiseMathAgent")]
    [DFSKAgentDescription("A stepwise math agent that can solve math problems step by step.")]
    [DFSKAgentDescription("It uses the FunctionCallingStepwisePlanner and also saves the plan to a json file.")]
    public class StepwiseMathAgent : BaseAgent
    {
        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public StepwiseMathAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;

            var builder = Kernel.CreateBuilder();
            //builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            //Use this extension method to load functions from a directory as plugins
            builder.Plugins.AddFromFunctionDirectory("Agents/Plugins/ChainOfThought");
            builder.Plugins.AddFromType<MathPlugin>();
            builder.Plugins.AddFromType<FileIOPlugin>();
            builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration()).Build();
         
        }

        [DFSKAgentCommand($"{nameof(StepwiseMathAgent)}_Start")]
        [DFSKInput($"A math problem to solve step by step.")]
        [Function($"{nameof(StepwiseMathAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            var stepwisePlanner = new FunctionCallingStepwisePlanner();
            var result = await stepwisePlanner.ExecuteAsync(_kernel, input);

            var jsonHistory = JsonConvert.SerializeObject(result.ChatHistory, Formatting.Indented);

            await _kernel.InvokeAsync("FileIOPlugin", "Write", new() {
                { "content", jsonHistory },
                { "path", "../../../stepwise.json" }
            });

            await SendMessage("## Final answer:");
            return result.FinalAnswer;
        }
    }
}