using Microsoft.Extensions.Options;

namespace DurableFunctions.SemanticKernel.Options
{
    public class FlaskLoggingOptions : IOptions<FlaskLoggingOptions>
    {
        public required string ModelId { get; set; }
        public required string ApiKey { get; set; }

        public FlaskLoggingOptions Value => this;
    }
}