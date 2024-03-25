using Microsoft.DurableTask.Entities;

namespace DurableFunctions.SemanticKernel.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(EntityInstanceId entityId);
    }

}


