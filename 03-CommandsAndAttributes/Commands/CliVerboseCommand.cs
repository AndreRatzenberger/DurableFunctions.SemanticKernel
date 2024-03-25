using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("cli.verbose")]
    [CommandDescription("Turns logging on or off")]
    public class CliVerboseCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId)
        {
            if (WebCliConfiguration.GetIsVerbose() == true)
                WebCliConfiguration.Configure(WebCliConfiguration.GetEndpoint(), false);
            else
                WebCliConfiguration.Configure(WebCliConfiguration.GetEndpoint(), true);
            await WebCliBridge.SendMessage("Verbose mode is now " + (WebCliConfiguration.GetIsVerbose() == true ? "on" : "off"));
        }
    }
}


