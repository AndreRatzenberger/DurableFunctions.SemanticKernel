using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Extentions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class AgentOrchestrator
    {
        [Function(nameof(AgentOrchestrator))]
        public static async Task<string> AgentOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
            var prompt = context.GetInput<string>();

            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} started");
            var response = await context.CallActivityAsync<string>($"{nameof(SimplePrompAgent)}_{nameof(SimplePrompAgent.Start)}", prompt);
            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} finished");
            return response;
        }
    }

}
