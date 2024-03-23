
using Microsoft.Azure.Functions.Worker;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.SemanticKernel.Experimental.Agents;

namespace DurableFunctions.SemanticKernel.Agents
{
    public abstract class BaseAgent
    {
        protected async Task<string?> StartTemplate(string input, FunctionContext context)
        {
            var log = context.GetLogger(GetType().Name);
            await SendMessage($"<hr><b>{GetType().Name} STARTED</b><hr>");

            var response = await ExecuteAgent(input);

            await SendMessage($"<br>{response}<br><br>");
            await SendMessage($"<hr><b>{GetType().Name} FINISHED</b><hr>");

            await Task.WhenAll(s_agents.Select(a => a.DeleteAsync()));

            return response;
        }

        private static readonly List<IAgent> s_agents = new();

        protected abstract Task<string?> ExecuteAgent(string input);

        internal async Task SendMessage(string? message)
        {
            if (message == null) return;
            await WebCliBridge.SendMessage(message);
        }

        internal static IAgent Track(IAgent agent)
        {
            s_agents.Add(agent);

            return agent;
        }
    }
}
