using DurableFunctions.SemanticKernel.Commands;
using DurableFunctions.SemanticKernel.Common;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
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

            log.LogInformationWithMetadata($"Agent... started");
            await WebCliBridge.SendMessage("<hr>START Agent....<hr>");

            while(true)
            {
                await context.CallSendMessageAsync(commandState.StatusMessage);
                commandState.UserInput = await context.WaitForExternalEvent<string>(EventListener.CommandReceived);

                if (commandState.IsExecutable)
                    break;
            }


            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} started");
            _ = await context.CallActivityAsync<string>($"{commandState.AgentName}_Start", commandState.Prompt);

            await WebCliBridge.SendMessage("<hr>END Agent....<hr>");
            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} finished");
        }
    }

}
