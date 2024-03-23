using DurableFunctions.SemanticKernel.Agents.Plugins;
using DurableFunctions.SemanticKernel.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Scripting.Generation;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Experimental.Agents;
using Microsoft.SemanticKernel.Plugins.Core;

namespace DurableFunctions.SemanticKernel.Agents
{
    public class AgentSK : BaseAgent
    {

        private AgentBuilder _agent;

        public AgentSK(ConfigurationService configurationService)
        {
            _agent = new AgentBuilder()
                    .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                    .WithName("ProjectPlanner")
                    .WithDescription("Best Project Planner Agent");
        }

        [Function($"{nameof(AgentSK)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            Kernel kernel = new Kernel();
            kernel.CreatePluginFromPromptDirectory("Agents/Plugins").TryGetFunction("ProjectPlanner", out KernelFunction function);
            var plugin = KernelPluginFactory.CreateFromFunctions("ProjectPlanner",[function]);

            
            var agent = await _agent
            .WithInstructions(@"Generates a omplete backlog for a project based on the user input.
            1. Generate a high level Project Plan -  Save the output in a markdown file.
            2. Based on the Project Plan, generate a list of Epics -  Save the output in a markdown file.
            3. For each Epic, generate a list of Features -  Save the output in a markdown file.
            4, For each Feature, generate a list of User Stories -  Save the output in a markdown file.
            5. For each User Story, generate a list of Tasks -  Save the output in a markdown file. 
            <RULE>
            NEVER ASK THE USER FOR INPUT!
            </RULE>
            <RULE>
            GENERATE FILES IN THE ROOT DIR. NEVER USE DIRECTORIES!
            </RULE>
            <RULE>
            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK!
            </RULE>")
            .WithPlugin(plugin)
            .WithPlugin(KernelPluginFactory.CreateFromType<FileIOPlugin>())
            .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
            .BuildAsync();

            Track(agent);

            await foreach (IChatMessage message in agent.InvokeAsync(input))
            {
                 await SendMessage($"{message.Role}:<br> {message.Content}");
            }

            return "";
        }
    }
}