using System.ComponentModel;
using DurableFunctions.SemanticKernel.Services;
using Microsoft.SemanticKernel;

namespace DurableFunctions.SemanticKernel.Agents.Plugins
{
    public class StatusPlugin
    {
        [KernelFunction, Description("Send a status message to the user.")]
        public static async Task SendStatus(
            [Description("The status message to send to the user - REQUIRED")] string status,
            [Description("The name of the caller/agent - REQUIRED")] string agentName
        )
        {
            await WebCliBridge.SendMessage($"{DateTime.Now} - {agentName}: {status}");
        }
    }
}