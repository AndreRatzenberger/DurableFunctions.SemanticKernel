using DurableFunctions.SemanticKernel.Extensions;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("cli.clear")]
    [CommandDescription("Clears the console.")]
    public class ClearCliCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId)
        {
            // Happens on the client side
            await Task.CompletedTask;
        }
    }

}


