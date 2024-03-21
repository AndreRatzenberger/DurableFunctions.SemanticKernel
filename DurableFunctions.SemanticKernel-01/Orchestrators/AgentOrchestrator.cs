using DurableFunctions.SemanticKernel.Activities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class AgentOrchestrator
    {
        [Function(nameof(AgentOrchestrator))]
        public static async Task<string> AgentOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var prompt = context.GetInput<string>();
            var response = await context.CallActivityAsync<string>($"{nameof(SimplePromptQandAAgent)}_{nameof(SimplePromptQandAAgent.Start)}", prompt);
            return response;
        }
    }

}
