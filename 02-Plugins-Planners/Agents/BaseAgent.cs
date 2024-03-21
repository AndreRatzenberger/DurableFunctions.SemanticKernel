
using Microsoft.Azure.Functions.Worker;
using DurableFunctions.SemanticKernel.Services;

namespace DurableFunctions.SemanticKernel.Agents
{
    public abstract class BaseAgent
    {
        protected async Task<string?> StartTemplate(string input, FunctionContext context)
        {
            var log = context.GetLogger(GetType().Name);
            await SendMessage($"## <hr><b>{GetType().Name} STARTED</b><hr>");

            var response = await ExecuteAgent(input);
            
            await SendMessage($"<br>{response}<br><br>");
            await SendMessage($"## <hr><b>{GetType().Name} FINISHED</b><hr>");
            
            return response;
        }

        protected abstract Task<string?> ExecuteAgent(string input);

        private async Task SendMessage(string message)
        {
            await WebCliBridge.SendMessage(message);
        }
    }
}
