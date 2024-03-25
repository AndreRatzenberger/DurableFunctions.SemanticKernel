using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Extensions;
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
            if(agentState.GetIsAgentRunning())
            {
                await context.CallSendMessageAsync("Agent is running, please wait for it to finish");
                return;
            }
            
            if(command.Equals("HELP", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.CallSendMessageAsync("cli.clear - clear the console");
                await context.CallSendMessageAsync("agent.load 'agentname' - load an agent");
                return;
            }

            if(command.StartsWith("agent.load", StringComparison.CurrentCultureIgnoreCase))
            {
                var agentName = command.Split(' ')[1];
                await context.CallSendMessageAsync($"Loading agent {agentName}");
                await context.CallSendMessageAsync("Please enter prompt");
                await context.Entities.CallEntityAsync<string>(entityId, nameof(AgentState.SetCurrentAgent), agentName);
                return;
            }

            if(agentState.GetIsAgentLoaded())
            {
                await context.CallSendMessageAsync("Agent is already loaded, please enter prompt");
                return;
            }
        }
    }

}
