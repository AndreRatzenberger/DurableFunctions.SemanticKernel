
using System.Reflection;
using System.Text;
using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Services;
using Google.Protobuf.WellKnownTypes;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    [CommandName("agent.list")]
    [CommandDescription("List all available agents.")]
    public class ListAgentsCommand : ICommand
    {
        public async Task ExecuteAsync(EntityInstanceId entityId, DurableTaskClient client, IList<string> args)
        {
            var agents = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(BaseAgent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            await WebCliBridge.SendMessage("<hr><b> Available Agents</b><hr>");
            foreach (var agent in agents)
            {
                var agentNameAttr = agent.GetCustomAttribute<DFSKAgentName>();
                if (agentNameAttr == null)
                    continue;

                var descriptionAttr = agent.GetCustomAttributes<DFSKAgentDescription>();
                await WebCliBridge.SendMessage($"### {agentNameAttr.Name}");

                if (descriptionAttr != null)
                    foreach (var desc in descriptionAttr)
                        await WebCliBridge.SendMessage($"{desc.Description}");

                foreach (var method in agent.GetMethods())
                {
                    var startCommand = method.GetCustomAttributes<DFSKAgentCommand>();
                    if (startCommand != null)
                        foreach (var start in startCommand)
                            await WebCliBridge.SendMessage($"StartCommand: {start.Command}");

                    var inputAttr = method.GetCustomAttributes<DFSKInput>();
                    if (inputAttr != null)
                        foreach (var input in inputAttr)
                            await WebCliBridge.SendMessage($"Input: {input.Input}");
                }


                await WebCliBridge.SendMessage($"<hr>");

            }
        }
    }

}


