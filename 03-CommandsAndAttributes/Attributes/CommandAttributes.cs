
namespace DurableFunctions.SemanticKernel.Extensions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandDescriptionAttribute(string description) : Attribute
    {
        public string Description { get; } = description;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CommandParameterAttribute(string parameter, string description) : Attribute
    {
        public string Parameter { get; } = parameter;
        public string Description { get; } = description;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandNameAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}




