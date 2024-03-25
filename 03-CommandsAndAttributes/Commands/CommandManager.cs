using DurableFunctions.SemanticKernel.Services;
using Microsoft.Azure.Functions.Worker;
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
            { "cli.clear", new ClearCliCommand() },
            // { "agent.load", new LoadAgentCommand() },
            { "agent.list", new ListAgentsCommand() },
            { "help", new HelpCommand() },
            { "cli.welcome", new InitCommand() }
        };

        [Function(nameof(ExecuteCommand))]
        public async Task ExecuteCommand([ActivityTrigger] CommandExecutionInput input)
        {
            if (_commands.TryGetValue(input.CommandString, out var cmd))
            {
                await cmd.ExecuteAsync(input.EntityId);
            }
            else
            {
                await WebCliBridge.SendMessage($"Command '{input.CommandString}' not found. Type 'help' for a list of available commands.");
            }
        }
    }

}
