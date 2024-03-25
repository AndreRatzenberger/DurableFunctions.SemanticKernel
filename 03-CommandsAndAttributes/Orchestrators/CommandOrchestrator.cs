using DurableFunctions.SemanticKernel.Commands;
using IronPython.Runtime.Operations;
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
            var command = await context.Entities.CallEntityAsync<string>(entityId, nameof(CommandState.GetNextCommand));
            var executingCommand = await context.Entities.CallEntityAsync<string>(entityId, nameof(CommandState.GetExecutingCommand));
            if (!string.IsNullOrEmpty(executingCommand))
            {
                await context.CallSendMessageAsync("Agent is running, please wait for it to finish");
                return;
            }

            var args = await context.Entities.CallEntityAsync<string>(entityId, nameof(CommandState.GetArgs));
            if (!string.IsNullOrEmpty(args))
            {
                var agentName = args.replace("'","");
                await context.CallActivityAsync($"{agentName}_Start", command);
                await context.CallCleanCommandAsync(new CommandExecutionInput
                {
                    CommandString = "",
                    EntityId = entityId
                });
                return;
            }

            await context.CallExecuteCommandAsync(new CommandExecutionInput
            {
                CommandString = command,
                EntityId = entityId
            });

            var activeCommand = await context.Entities.CallEntityAsync<string>(entityId, nameof(CommandState.GetActiveCommand));
            if (!string.IsNullOrEmpty(activeCommand))
            {
                await context.CallSendMessageAsync("Please enter prompt:");
                return;
            }
        }
    }

}
