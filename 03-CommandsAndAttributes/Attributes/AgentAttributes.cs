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

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class DFSKPlugins(string plugin) : Attribute
    {
        public string Plugin { get; } = plugin;
    }

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class DFSKFunctions(string functions) : Attribute
    {
        public string Functionc { get; } = functions;
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DFSKAgentCommand(string command) : Attribute
    {
        public string Command { get; } = command;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DFSKInput(string input) : Attribute
    {
        public string Input { get; } = input;
    }
}