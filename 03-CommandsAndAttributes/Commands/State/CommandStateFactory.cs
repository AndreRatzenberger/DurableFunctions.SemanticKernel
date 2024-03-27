namespace DurableFunctions.SemanticKernel.Commands.State
{
    public static class CommandStateFactory
    {
        public static CommandState Create(string commandName)
        {
            switch (commandName)
            {
                case "agent":
                    return new AgentCommandState();
                default:
                    return new CommandState();
            }
        }

        public static CommandState CreateFromState(string commandName, CommandState state)
        {
            switch (commandName)
            {
                case "agent":
                    return state as AgentCommandState ?? new AgentCommandState();
                default:
                    return state ?? new CommandState();
            }
        }
    }
}
