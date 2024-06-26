
using System.Reflection;
using DurableFunctions.SemanticKernel.Commands.Interface;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("help")]
    [CommandDescription("Shows a list of available commands.")]
    public class HelpCommand : ICommand
    {
        public async Task<CommandState> ExecuteAsync(CommandState commandState, DurableTaskClient client)
        {
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            await WebCliBridge.SendMessage("<hr><b> Available Commands</b><hr>");
            foreach (var type in commandTypes)
            {
                var descriptionAttr = type.GetCustomAttribute<CommandDescriptionAttribute>();
                var commandNameAttr = type.GetCustomAttribute<CommandNameAttribute>();

                if (descriptionAttr == null || commandNameAttr == null)
                    continue;

                await WebCliBridge.SendMessage($"#### {commandNameAttr.Name}");
                await WebCliBridge.SendMessage($"{descriptionAttr.Description}");

                var parameterAttrs = type.GetCustomAttributes<CommandParameterAttribute>();
                foreach (var paramAttr in parameterAttrs)
                {
                    await WebCliBridge.SendMessage($" {paramAttr.Parameter}: {paramAttr.Description}");
                }

                await WebCliBridge.SendMessage($"<hr>");
            }
            
            return new CommandState();
        }
    }

}


