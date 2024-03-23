using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using DurableFunctions.SemanticKernel.Services;
using DurableFunctions.SemanticKernel.Orchestrators;

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
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator), requestBody);
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }

}
