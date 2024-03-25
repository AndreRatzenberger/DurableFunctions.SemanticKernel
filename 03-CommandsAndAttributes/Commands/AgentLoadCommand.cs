using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("agent.load")]
    [CommandDescription("Loads a specific agent.")]
    [CommandParameter("agent.load 'agentName'", "Loads the agent 'agentName' into the current session.")]
    public class AgentLoadCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client)
        {
            await Task.CompletedTask;
        }

        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client, IList<string> args)
        {
            if (args == null || args.Count != 1)
            {
                await WebCliBridge.SendMessage("Usage: agent.load 'agentName'");
                return;
            }
            entityId = new EntityInstanceId(nameof(CommandState), "singleton");
            await client.Entities.SignalEntityAsync(entityId, nameof(CommandState.SetArgs), args[0]);
            await client.Entities.SignalEntityAsync(entityId, nameof(CommandState.SetActiveCommand), "agent.load");
        }
    }

}