using DurableFunctions.SemanticKernel.Commands.Interface;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;

namespace DurableFunctions.SemanticKernel.Commands
{
    //To implement a command set these attributes and implement the ICommand interface
    [CommandName("cli")]
    [CommandDescription("CLI specific commands.")]
    [CommandParameter("-clear", "Clears the console.")]
    [CommandParameter("-welcome", "Shows the welcome message.")]
    [CommandParameter("-verbose", "Shows logs in the console.")]
    [CommandParameter("-markdown", "on/off switch. Renders CLI content as markdown.")]
    [CommandParameter("-help", "Shows this explanation of the command")]
    public class CliCommand : ICommand
    {
        //ExecuteAsync is the method that will be called when the command is invoked without parameters
        public async Task<CommandState> ExecuteAsync(CommandState commandState, DurableTaskClient client)
        {
            await WebCliBridge.SendMessage($"Wrong or missing parameters. Try {commandState.Command} -help");
            return new CommandState();
        }

        //Those following Execute methods are called when the command is invoked with the respective parameter
        //To make the runtime binding work the method has to be of this name:
        //Format: Execute{ParameterName}Async
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


