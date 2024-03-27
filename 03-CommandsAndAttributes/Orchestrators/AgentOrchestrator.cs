using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Commands;
using DurableFunctions.SemanticKernel.Common;
using DurableFunctions.SemanticKernel.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class AgentOrchestrator
    {
        [Function(nameof(AgentOrchestrator))]
        public static async Task AgentOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
            var commandState = context.GetInput<AgentCommandState>()!;

            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} started");
            log.LogInformationWithMetadata($"{JsonHelpers.Serialize(commandState)} started");

            _ = await context.CallActivityAsync<string>($"{commandState.AgentName}_Start", commandState.Prompt);

            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} finished");
        }
    }

}
