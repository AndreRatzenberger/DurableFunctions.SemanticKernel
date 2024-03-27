using System.Globalization;
using System.Reflection;
using DurableFunctions.SemanticKernel.Commands.Interface;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace DurableFunctions.SemanticKernel.Commands.Registry
{
    public class CommandRegistry
    {
        private CommandState _commandState = new();

        public Dictionary<string, CommandInstance> _commands { get; private set; } = [];

        public class CommandInstance
        {
            public Dictionary<string, string> CommandParameters { get; set; } = [];
            public ICommand Command { get; set; } = null!;
        }

        public CommandRegistry()
        {
            DiscoverAndRegisterCommands();
        }

        private void DiscoverAndRegisterCommands()
        {
            var commandTypes = Assembly.GetAssembly(typeof(ICommand))!.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var commandNameAttr = type.GetCustomAttribute<CommandNameAttribute>();
                if (commandNameAttr != null)
                {
                    var instance = new CommandInstance
                    {
                        Command = (ICommand)Activator.CreateInstance(type)!,
                        CommandParameters = type.GetCustomAttributes<CommandParameterAttribute>()
                                                .ToDictionary(attr => attr.Parameter, attr => attr.Description)
                    };
                    _commands[commandNameAttr.Name.ToLower()] = instance;
                }
            }
        }

        [Function(nameof(TryExecuteCommand))]
        public async Task<CommandState> TryExecuteCommand([ActivityTrigger] CommandState commandState, DurableTaskClient client)
        {
            _commandState = commandState;
            var input = commandState.Command;
            var segments = input.Split(' ', 2);
            var commandKey = segments[0].ToLower();

            if (_commands.TryGetValue(commandKey, out var commandInstance))
            {
                var args = segments.Length > 1 ? segments[1].Split(' ') : [];
                var matchedParam = commandInstance.CommandParameters.Keys.FirstOrDefault(param => args.Any(arg => arg.StartsWith(param)));

                if (matchedParam != null)
                {
                    if(matchedParam == "-help")
                    {
                        await ShowCommandHelp(commandInstance.CommandParameters);
                        return new CommandState();
                    }
                    // Prepare to invoke the correct command logic based on the matched parameter
                    commandState.Args = [.. args];
                    _commandState = await InvokeCommandLogic(commandInstance.Command, matchedParam, commandState, client);
                }
                else
                {
                    _commandState = await commandInstance.Command.ExecuteAsync(commandState, client);
                    return _commandState;
                }
            }
            else
            {
                _commandState.StatusMessage = $"Command '{input}' not found. Type 'help' for a list of available commands.";
                await ShowCommandStatus(new CommandState { StatusMessage = _commandState.StatusMessage });
            }

            return _commandState;
        }

        private async Task<CommandState> InvokeCommandLogic(ICommand commandInstance, string parameter, CommandState commandState, DurableTaskClient client)
        {
            var methodName = ConvertParameterToMethodName(parameter);
            var methodInfo = commandInstance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                var task = (Task<CommandState>)methodInfo.Invoke(commandInstance, [commandState, client])!;
                return await task;
            }
            else
            {
                commandState.StatusMessage = $"No action found for parameter {parameter}.";
                await ShowCommandStatus(commandState);
                return new CommandState();
            }
        }

        private static string ConvertParameterToMethodName(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException("Parameter cannot be null or empty.", nameof(parameter));
            }

            var cleanParameter = parameter.TrimStart('-');
            var parts = cleanParameter.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            var methodName = string.Concat(parts.Select(part => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(part))) + "Async";

            return "Execute" + methodName;
        }

        private static async Task ShowCommandStatus(CommandState commandState)
        {
            await WebCliBridge.SendMessage(commandState.StatusMessage);
        }

        public static async Task ShowCommandHelp(Dictionary<string, string> commandParameters)
        {
            var helpText = "Available parameters:\n" + string.Join("\n", commandParameters.Select(kv => $"{kv.Key}: {kv.Value}"));
            await WebCliBridge.SendMessage(helpText);
        }
    }

}
