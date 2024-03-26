using System.Globalization;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;


namespace DurableFunctions.SemanticKernel.Commands
{
    public class CommandExecutionInput
    {
        public string CommandString { get; set; } = default!;
        public EntityInstanceId EntityId { get; set; }
    }
    public class CommandManager
    {
        private readonly Dictionary<string, ICommand> _commands = new()
        {
            { "cli.clear", new CliClearCommand() },
            { "agent.load", new AgentLoadCommand() },
            { "agent.list", new ListAgentsCommand() },
            { "help", new HelpCommand() },
            { "cli.welcome", new InitCommand() },
            { "cli.verbose", new CliVerboseCommand() }
        };

        [Function(nameof(ExecuteCommand))]
        public async Task ExecuteCommand([ActivityTrigger] CommandExecutionInput input, [DurableClient] DurableTaskClient client)
        {
            var segments = input.CommandString.Split([' '], 2);
            var commandName = segments[0].ToLower(CultureInfo.InvariantCulture).Trim();
            var args = segments.Length > 1 ? ParseArguments(segments[1]) : [];

            if (_commands.TryGetValue(commandName, out var cmd))
                await cmd.ExecuteAsync(input.EntityId, client, args);
            else
                await WebCliBridge.SendMessage($"Command '{input.CommandString}' not found. Type 'help' for a list of available commands.");
        }

        [Function(nameof(CleanCommand))]
        public async Task CleanCommand([ActivityTrigger] CommandExecutionInput input, [DurableClient] DurableTaskClient client)
        {
            await client.Entities.SignalEntityAsync(input.EntityId, nameof(CommandState.SetArgs), "");
            await client.Entities.SignalEntityAsync(input.EntityId, nameof(CommandState.SetExecutingCommand), "");
            await client.Entities.SignalEntityAsync(input.EntityId, nameof(CommandState.SetPrompt), "");
            await client.Entities.SignalEntityAsync(input.EntityId, nameof(CommandState.ClearQueue));
            await client.Entities.SignalEntityAsync(input.EntityId, nameof(CommandState.SetActiveCommand), "");
        }

        private static IList<string> ParseArguments(string args)
        {
            return [.. args.Split(' ')];
        }
    }

}
