using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Extentions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class AgentOrchestrator
    {
        [Function(nameof(AgentOrchestrator))]
        public static async Task AgentOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
            var entityId = context.GetInput<EntityInstanceId>();
            var command = await context.Entities.CallEntityAsync<string>(entityId, nameof(AgentState.GetNextCommand));
            var selectedAgent = await context.Entities.CallEntityAsync<string>(entityId, nameof(AgentState.GetCurrentAgent));
            var isAgentLoaded = await context.Entities.CallEntityAsync<bool>(entityId, nameof(AgentState.GetIsAgentLoaded));
            var isAgentRunning = await context.Entities.CallEntityAsync<bool>(entityId, nameof(AgentState.GetIsAgentRunning));
            var agentState = await context.Entities.CallEntityAsync<AgentState>(entityId, nameof(AgentState.GetAgentState));
            if(isAgentRunning)
            {
                return; //If agent is running, do nothing and let the agent handle do it's thing
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

            if(isAgentLoaded)
            {
                //Start the agent
            }
        }
    }

}
