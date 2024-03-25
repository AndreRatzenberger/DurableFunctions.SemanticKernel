using Microsoft.Extensions.Options;

namespace DurableFunctions.SemanticKernel.Options
{
    public class OpenAIOptions : IOptions<OpenAIOptions>
    {
        public required string ModelId { get; set; }
        public required string ApiKey { get; set; }

        public OpenAIOptions Value => this;
    }
}