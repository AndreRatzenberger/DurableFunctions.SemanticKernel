using DurableFunctions.SemanticKernel.Commands.Interface;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("cli")]
    [CommandDescription("CLI specific commands.")]
    [CommandParameter("-clear", "Clears the console.")]
    [CommandParameter("-welcome", "Shows the welcome message.")]
    [CommandParameter("-verbose", "Shows logs in the console.")]
    [CommandParameter("-help", "Shows this explanation of the command")]
    public class CliCommand : ICommand
    {
        public async Task<CommandState> ExecuteAsync(CommandState commandState, DurableTaskClient client)
        {
            await WebCliBridge.SendMessage($"Wrong format. Can't be used without Parameters. Try {commandState.Command} -help");
            return new CommandState();
        }

        public async Task<CommandState> ExecuteWelcomeAsync(CommandState commandState, DurableTaskClient client)
        {
            await WebCliBridge.SendMessage("## Welcome to the DurableFunction ❤️ SemanticKernel Project");
            await WebCliBridge.SendMessage("### Enter your commands below - Try 'help' for a start:");
            return new CommandState();
        }

        public async Task<CommandState> ExecuteClearAsync(CommandState commandState, DurableTaskClient client)
        {
            await WebCliBridge.SendMessage("Console cleared.");
            return new CommandState();
        }

        public async Task<CommandState> ExecuteVerboseAsync(CommandState commandState, DurableTaskClient client)
        {
            //TODO Verbose logging
            await WebCliBridge.SendMessage("Verbose logging is not yet implemented.");
            return new CommandState();
        }
    }
}


