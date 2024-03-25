using DurableFunctions.SemanticKernel.Agents;
using DurableFunctions.SemanticKernel.Extentions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace DurableFunctions.SemanticKernel.Orchestrators
{
    static class AgentOrchestrator
    {
        [Function(nameof(AgentOrchestrator))]
        public static async Task AgentOrchestratorAsync([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
            var prompt = context.GetInput<string>();

            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} started");

            //_ = await context.CallActivityAsync<string>($"{nameof(HandlebarsMathAgent)}_Start", prompt);
            //_ = await context.CallActivityAsync<string>($"{nameof(StepwiseMathAgent)}_Start", prompt);
            //_ = await context.CallActivityAsync<string>($"{nameof(NativeSemanticKernelAgent)}_Start", prompt);
            //_ = await context.CallActivityAsync<string>($"{nameof(SimplePrompAgent)}_Start", prompt);
            _ = await context.CallActivityAsync<string>($"{nameof(ProjectAgent)}_Start", prompt);
            log.LogInformationWithMetadata($"{nameof(AgentOrchestrator)} finished");
        }
    }

}
