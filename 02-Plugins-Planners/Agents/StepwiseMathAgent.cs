using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.SemanticKernel.Planning;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core;


namespace DurableFunctions.SemanticKernel.Agents
{
    public class StepwiseMathAgent : BaseAgent
    {
        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;

        public StepwiseMathAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;
            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            builder.Plugins.AddFromType<Plugins.MathPlugin>();
            builder.Plugins.AddFromType<FileIOPlugin>();
            builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration()).Build();
        }

        [Function($"{nameof(StepwiseMathAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            var pluginMarkdown = await _kernel.InvokeAsync("Plugins", "JsonToMarkdown", new() {
                { "input", JsonConvert.SerializeObject(_kernel.Plugins) },
                { "jsonContext", "The Plugins the Kernel has access too" }
            });

            await WebCliBridge.SendMessage($"## Used Plugins: <br>{pluginMarkdown.GetValue<string>()} <hr>");
            
            var stepwisePlanner = new FunctionCallingStepwisePlanner();
            var result = await stepwisePlanner.ExecuteAsync(_kernel, input);

            var jsonHistory = JsonConvert.SerializeObject(result.ChatHistory, Formatting.Indented);

            await _kernel.InvokeAsync("FileIOPlugin", "Write", new() {
                { "content", jsonHistory },
                { "path", "../../../.dump/stepwise.json" }
            });

            var markdownHistory = await _kernel.InvokeAsync("Plugins", "JsonToMarkdown", new() {
                { "input", jsonHistory },
                { "importantValues", "Content, ChatResponseMessage.FunctionToolCalls, ModelId, Role" },
                { "jsonContext", "Describe the execution plan of an AI trying to solve a problem" }
            });

          
            await WebCliBridge.SendMessage(markdownHistory.GetValue<string>() + "<hr>");
            await WebCliBridge.SendMessage("## FInal answer:");
            return result.FinalAnswer;
        }
    }
}