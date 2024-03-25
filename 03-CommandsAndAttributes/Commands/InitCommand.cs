using System.Text;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    //Hidden Command
    //Handshake between client and server
    public class InitCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client, IList<string> args)
        {
            await WebCliBridge.SendMessage( "## Welcome to the DurableFunction ❤️ SemanticKernel Project");
            await WebCliBridge.SendMessage("### Enter your commands below - Try 'help' for a start:");
        }
    }

}


