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

            //Clean up agents - Only needed for SemanticKernel native agents
            await Task.WhenAll(_agents.Select(a => a.DeleteAsync()));

            return response;
        }

        private static readonly List<IAgent> _agents = [];
        protected abstract Task<string?> ExecuteAgent(string input);

        internal static async Task SendMessage(string? message)
        {
            if (message == null) return;
            await WebCliBridge.SendMessage(message);
        }

        //For SemanicKernel native agents
        //You need to keep track of the agents so you can clean them up when you're done
        internal static IAgent Track(IAgent agent)
        {
            _agents.Add(agent);
            return agent;
        }
    }
}
