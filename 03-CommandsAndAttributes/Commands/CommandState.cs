using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;


namespace DurableFunctions.SemanticKernel.Commands
{
    public class CommandState
    {
        public Queue<string> CommandQueue { get; set; } = new();
        public string ActiveCommand { get; set; } = "";

        public string Args { get; set; } = "";

        public string ExecutingCommand { get; set; } = "";

        public string Prompt { get; set; } = "";

        public string GetPrompt()
        {
            return Prompt;
        }   

        public void SetPrompt(string prompt)
        {
            Prompt = prompt;
        }   

        public void ClearQueue()
        {
            CommandQueue.Clear();
        }

        public string GetArgs()
        {
            return Args;
        }   

        public void SetArgs(string args)
        {
            Args = args;
        }   

         public string GetExecutingCommand()
        {
            return ExecutingCommand;
        }

        public void SetExecutingCommand(string command)
        {
            ExecutingCommand = command;
        }

        public string GetActiveCommand()
        {
            return ActiveCommand;
        }

        public void SetActiveCommand(string command)
        {
            ActiveCommand = command;
        }

        public void AddCommand(string command)
        {
            CommandQueue.Enqueue(command);
        }

        public string? GetNextCommand()
        {
            return CommandQueue.Count > 0 ? CommandQueue.Dequeue() : null;
        }

        public string? ShowQueue()
        {
            return string.Join(',', [.. CommandQueue]);
        }

        [Function(nameof(CommandState))]
        public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            return dispatcher.DispatchAsync<CommandState>();
        }
    }

}
