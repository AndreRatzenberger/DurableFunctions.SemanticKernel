using DurableFunctions.SemanticKernel.Commands.State;
using Microsoft.DurableTask.Client;

namespace DurableFunctions.SemanticKernel.Commands.Interface
{
    public interface ICommand
    {
        Task<CommandState> ExecuteAsync(CommandState commandState, DurableTaskClient client);
    }
}


