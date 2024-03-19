using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
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
            string requestBody;
            using (StreamReader reader = new(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(OrchestratorNames.SemanticKernelOrchestrator, requestBody);
            logger.LogInformation("Started orchestrator with instance ID = {instanceId}", instanceId);
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }

}
