
using System.Text;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    public class HelpCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId)
        {
            var helpText = new StringBuilder();
            helpText.AppendLine("**cli.clear** - Clear the console.");
            helpText.AppendLine("**agent.load 'agentname'** - Load an agent by name.");
            helpText.AppendLine("**agent.list** - List all available agents.");


            await WebCliBridge.SendMessage(helpText.ToString());
        }
    }

}


