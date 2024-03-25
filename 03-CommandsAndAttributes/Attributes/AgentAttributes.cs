namespace DurableFunctions.SemanticKernel.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DFSKAgentName(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DFSKAgentDescription(string description) : Attribute
    {
        public string Description { get; } = description;
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DFSKAgentCommand(string name) : Attribute
    {
        public string Command { get; } = name;
    }
}