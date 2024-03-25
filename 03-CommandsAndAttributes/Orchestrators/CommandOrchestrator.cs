using DurableFunctions.SemanticKernel.Commands;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class CommandOrchestrator
    {
        [Function(nameof(CommandOrchestrator))]
        public static async Task CommandOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(CommandOrchestrator));
            var entityId = context.GetInput<EntityInstanceId>();
            var command = await context.Entities.CallEntityAsync<string>(entityId, nameof(AgentState.GetNextCommand));
            var agentState = await context.Entities.CallEntityAsync<AgentState>(entityId, nameof(AgentState.GetAgentState));

            if (agentState.GetIsAgentRunning())
            {
                await context.CallSendMessageAsync("Agent is running, please wait for it to finish");
                return;
            }

            await context.CallExecuteCommandAsync(new CommandExecutionInput
            {
                CommandString = command,
                EntityId = entityId
            });
        }
    }

}
