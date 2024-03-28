namespace DurableFunctions.SemanticKernel.Commands.State
{
    public class CommandState
    {
        public string UserInput { get; set; } = "";
        public string Command { get; set; } = "";

        public List<string> Args = [];

        public string StatusMessage { get; set; } = "";
    }
}
