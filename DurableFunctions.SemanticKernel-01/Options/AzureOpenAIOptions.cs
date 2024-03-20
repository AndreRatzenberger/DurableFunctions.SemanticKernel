using Microsoft.Extensions.Options;

namespace DurableFunctions.SemanticKernel.Options
{
    public class AzureOpenAIOptions : IOptions<AzureOpenAIOptions>
    {
        public required string ModelId { get; set; }
        public required string ApiKey { get; set; }
        public required string DeploymentName { get; set; }
        public required string Endpoint { get; set; }

        public AzureOpenAIOptions Value => this;
    }
}