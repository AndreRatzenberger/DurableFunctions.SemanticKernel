namespace DurableFunctions.SemanticKernel.Common
{
    public static class OrchestratorNames
    {
        public const string AgentOrchestrator = nameof(AgentOrchestrator);
    }


    public static class AgentNames
    {
        // Agent name: SimplePromptQandAAgent
        // Decription: This agent is a simple question and answer agent that uses the OpenAI chat completion model.
        // UseCase: This agent can be used to answer simple questions or provide information based on a prompt.
        // AdditionalInfo: This agent should be chosen if the user's question can be answered with a short response.
        public const string SimplePromptAgent = nameof(SimplePromptAgent);
    }
}
