using DurableFunctions.SemanticKernel.Commands;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class CommandOrchestrator
    {
        [Function(nameof(CommandOrchestrator))]
        public static async Task CommandOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(CommandOrchestrator));
            var commandState = new CommandState{UserInput = context.GetInput<string>()!};
            commandState = await context.CallTryExecuteCommandAsync(commandState); 
            context.SetCustomStatus(commandState);

            while (true)
            {
                await context.CallSendMessageAsync(commandState.StatusMessage);
                commandState.UserInput = await context.WaitForExternalEvent<string>(EventListener.CommandReceived);
                commandState = await context.CallTryExecuteCommandAsync(commandState); 
                context.SetCustomStatus(commandState);
                
                if (commandState is AgentCommandState agentCommandState)
                    agentCommandState = await context.CallSubOrchestratorAsync<AgentCommandState>(nameof(AgentOrchestrator.AgentOrchestratorAsync), agentCommandState);
            };
        }
    }

}
