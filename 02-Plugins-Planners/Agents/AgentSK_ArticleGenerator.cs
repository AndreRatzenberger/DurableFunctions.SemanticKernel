


using DurableFunctions.SemanticKernel.Agents.Plugins;
using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Agents;

namespace DurableFunctions.SemanticKernel.Agents
{
    public class AgentSK_ArticleGenerator : BaseAgent
    {
        [Function($"{nameof(AgentSK_ArticleGenerator)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }
        protected override async Task<string?> ExecuteAgent(string input)
        {

            IAgent agent = await CreateArticleGeneratorAsync();
            var agentResult = await agent.AsPlugin().InvokeAsync(input);
           ;
            return agentResult;
        }

        private static async Task<IAgent> CreateArticleGeneratorAsync()
        {
            var outline = await CreateOutlineGeneratorAsync();
            var research = await CreateResearchGeneratorAsync();

            return
                Track(
                    await new AgentBuilder()
                        .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                        .WithInstructions(@"You write concise opinionated articles that are published online.  Use an outline to generate an article with one section of prose for each top-level outline element.  
                        Each section is based on research with a minimum of 200 words.
                        Split up your workloard if you come close to token limits.
                            <RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK!
                            </RULE>")
                        .WithName("Article Author")
                        .WithDescription("Author an article on a given topic.")
                        .WithPlugin(outline.AsPlugin())
                        .WithPlugin(research.AsPlugin())
                        .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
                        .BuildAsync());
        }

        private static async Task<IAgent> CreateOutlineGeneratorAsync()
        {
            // Initialize agent so that it may be automatically deleted.
            return
                Track(
                    await new AgentBuilder()
                        .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                        .WithInstructions(@"Produce an single-level outline (no child elements) based on the given topic with at most 5 sections.<RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK!
                            </RULE>")
                        .WithName("Outline Generator")
                        .WithDescription("Generate an outline.")
                        .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
                        .BuildAsync());
        }

        private static async Task<IAgent> CreateResearchGeneratorAsync()
        {
            return
                Track(
                    await new AgentBuilder()
                        .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                        .WithInstructions(@"Provide insightful research that supports the given topic based on your knowledge of the outline topic.<RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK!
                            </RULE>")
                        .WithName("Researcher")
                        .WithDescription("Author research summary.")
                        .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
                        .BuildAsync());
        }

    }
}