using System.Reflection;
using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Commands.Interface;
using DurableFunctions.SemanticKernel.Commands.State;
using DurableFunctions.SemanticKernel.Extensions;
using DurableFunctions.SemanticKernel.Orchestrators;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    public class AgentCommandState : CommandState
    {
        public string AgentName { get; set; } = "";

        public string ActiveAgentOrchestratorId { get; set; } = "";

        public string Prompt { get; set; } = "";
    }

    [CommandName("agent")]
    [CommandDescription("Loads a specific agent.")]
    [CommandParameter("-load <AgentName>", "Loads the Agent into the current session.")]
    [CommandParameter("-list", "Lists all available agents.")]
    [CommandParameter("-help", "Shows this explanation of the command")]
    public class AgentCommand : ICommand
    {
        public async Task<CommandState> ExecuteAsync(CommandState commandState, DurableTaskClient client)
        {
            await WebCliBridge.SendMessage($"Wrong format. Can't be used without Parameters. Try {commandState.Command} -help");
            return new CommandState();
        }

        public async Task<CommandState>  ExecuteListAsync(CommandState commandState, DurableTaskClient client)
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
            return new CommandState();
        }

        private async Task<CommandState> ExecuteLoadAsync(CommandState commandState, DurableTaskClient client)
        {

            if (commandState is not AgentCommandState agentCommandState)
                throw new ArgumentException("Invalid command state type");
            if (!CheckForRequiremenets(agentCommandState))
            {
                await WebCliBridge.SendMessage(agentCommandState!.StatusMessage);
                return agentCommandState;
            }

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator), agentCommandState);
            agentCommandState.ActiveAgentOrchestratorId = instanceId;
            return agentCommandState!;
        }

        private static bool CheckForRequiremenets(CommandState commandState)
        {
            if (commandState == null)
                return false;
            return commandState.Args.Count == 1;
        }
    }

}