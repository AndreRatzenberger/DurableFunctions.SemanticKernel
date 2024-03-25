using DurableFunctions.SemanticKernel.Extensions;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("cli.clear")]
    [CommandDescription("Clears the console.")]
    public class CliClearCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client, IList<string> args)
        {
             // Happens on the client side
            await Task.CompletedTask;
        }
    }

}


