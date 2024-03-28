using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using DurableFunctions.SemanticKernel.Orchestrators;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.DurableTask;
using DurableFunctions.SemanticKernel.Common;

namespace DurableFunctions.SemanticKernel.Functions
{
    static class HttpStart
    {
        [Function(nameof(StartSemanticKernel))]
        public static async Task<HttpResponseData> StartSemanticKernel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(StartSemanticKernel));
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var instanceId = Environment.GetEnvironmentVariable("OrchestratorId")!;

            var status = client.CreateCheckStatusResponseAsync(req, instanceId);
            var orchestrator = await client.GetInstanceAsync(instanceId);

            if (orchestrator != null && orchestrator.RuntimeStatus == OrchestrationRuntimeStatus.Running)
                if (IsActiveOrchestrator(orchestrator))
                {
                    await client.RaiseEventAsync(instanceId, EventListener.CommandReceived, requestBody);
                    return client.CreateCheckStatusResponse(req, instanceId);
                }

            var startOrchestrationOptions = new StartOrchestrationOptions { InstanceId = instanceId };
            instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(CommandOrchestrator), requestBody, startOrchestrationOptions);
            return client.CreateCheckStatusResponse(req, instanceId);
        }

        public static bool IsActiveOrchestrator(OrchestrationMetadata existingInstanceStatus)
        {
            if (existingInstanceStatus != null &&
                existingInstanceStatus.RuntimeStatus != OrchestrationRuntimeStatus.Completed &&
                existingInstanceStatus.RuntimeStatus != OrchestrationRuntimeStatus.Failed &&
                existingInstanceStatus.RuntimeStatus != OrchestrationRuntimeStatus.Terminated)
                return true;
            return false;
        }


        [Function(nameof(SanityCheck))]
        public static async Task<string> SanityCheck(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
           [DurableClient] DurableTaskClient client,
           FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(StartSemanticKernel));
            await WebCliBridge.SendMessage("Sanity");
            return "Sanity!";
        }
    }

}
