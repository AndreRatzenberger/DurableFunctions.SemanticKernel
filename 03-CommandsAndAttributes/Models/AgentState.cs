using Microsoft.Azure.Functions.Worker;


namespace DurableFunctions.SemanticKernel.Orchestrators
{
    public class AgentState 
    {
        private string currentAgent = string.Empty;

        private bool isAgentLoaded = false;

        private bool isAgentRunning = false;

        public string GetCurrentAgent()
        {
            return currentAgent;
        }

        public void SetCurrentAgent(string value)
        {
            currentAgent = value;
        }

        public Queue<string> CommandQueue { get; set; } = new();

        public bool GetIsAgentLoaded()
        {
            return isAgentLoaded;
        }

        public void SetIsAgentLoaded(bool value)
        {
            isAgentLoaded = value;
        }

        public bool GetIsAgentRunning()
        {
            return isAgentRunning;
        }

        public void SetIsAgentRunning(bool value)
        {
            isAgentRunning = value;
        }

        public void AddCommand(string command)
        {
            CommandQueue.Enqueue(command);
        }

        public AgentState GetAgentState()
        {
            return this;
        }   

        public string? GetNextCommand()
        {
            return CommandQueue.Count > 0 ? CommandQueue.Dequeue() : null;
        }

        public string? ShowQueue()
        {
            return string.Join(',', [.. CommandQueue]);
        }

        [Function(nameof(AgentState))]
        public static Task Run([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<AgentState>();
    }

}
