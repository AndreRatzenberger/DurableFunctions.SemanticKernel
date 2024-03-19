using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using Microsoft.Extensions.Options;
using DurableFunctions.SemanticKernel.Ex01.Extensions;


namespace DurableFunctions.SemanticKernel.Activities
{
    public class SimplePromptQandAAgent
    {
        private readonly OpenAIOptions _openAIOptions;
        private Kernel _kernel;

        public SimplePromptQandAAgent(IOptions<OpenAIOptions> openAIOptions)
        {
            _openAIOptions = openAIOptions.Value;
            _kernel = Kernel.CreateBuilder()
               .AddOpenAIChatCompletion(modelId: _openAIOptions.ModelId, apiKey: _openAIOptions.ApiKey)
               .Build();
        }


        [Function(nameof(Start))]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            var log = context.GetLogger(nameof(SimplePromptQandAAgent));
            log.LogInformationWithMetadata($"Agent started with input: {input}");
            var response = await _kernel.InvokePromptAsync(input);
            var result = response.GetValue<string>();
            log.LogInformationWithMetadata($"Agent started with input: {input}");
            return result;
        }

    }
}