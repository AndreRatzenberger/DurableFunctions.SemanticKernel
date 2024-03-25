
using System.Text;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("agent.list")]
    [CommandDescription("List all available agents.")]
    public class ListAgentsCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client, IList<string> args)
        {
            var init = new StringBuilder();
            init.AppendLine("## Welcome to DurableFunction ❤️ SemanticKernel Project\n### Enter your commands below - Try 'help' for a start:");
            await WebCliBridge.SendMessage(init.ToString());
        }
    }

}


