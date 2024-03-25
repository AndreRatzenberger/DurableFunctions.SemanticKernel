
using System.Text;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    public class InitCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId)
        {
            var init = new StringBuilder();
            init.AppendLine("Welcome to DurableFunction.SemanticKernel CLI. Enter your commands below - Try 'help' for a start:");
            await WebCliBridge.SendMessage(init.ToString());
        }
    }

}


