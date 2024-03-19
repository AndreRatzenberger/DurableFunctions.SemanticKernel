using DurableFunctions.SemanticKernel.Activities;
using DurableFunctions.SemanticKernel.Ex01.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class SemanticKernel
    {
        [Function(nameof(SemanticKernelOrchestrator))]
        public static async Task<string> SemanticKernelOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(SemanticKernelOrchestrator));
            log.LogInformationWithMetadata("Orchestrator started");

            var prompt = context.GetInput<string>();
            var response = await context.CallActivityAsync<string>(nameof(SimplePromptQandAAgent.Start), prompt);
            
            log.LogInformationWithMetadata($"Answer Received: {response}");
            return response;
        }
    }

}
