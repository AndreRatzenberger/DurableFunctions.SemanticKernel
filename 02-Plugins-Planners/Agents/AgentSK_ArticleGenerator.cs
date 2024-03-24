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
                        .WithInstructions(@"You write very long opinionated articles that are published online, and are funny, sarcastic, and showcasing a engaging writing style that makes the reader want to read more and laugh. Even if the topic is serious.
                            Use an outline to generate an article with one section of prose for each top-level outline element.  
                            Each section is based on research with a minimum of 200 words.
                            <RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK! USE 'ArticleGenerator' AS THE AGENT NAME.
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
            return
                Track(
                    await new AgentBuilder()
                        .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                        .WithInstructions(@"Produce an single-level outline (no child elements) based on the given topic with at most 5 sections.<RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK! USE 'OutlineGenerator' AS THE AGENT NAME.
                            </RULE>")
                        .WithName("Outline Generator")
                        .WithDescription("Generates an outline.")
                        .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
                        .BuildAsync());
        }

        private static async Task<IAgent> CreateResearchGeneratorAsync()
        {
            return
                Track(
                    await new AgentBuilder()
                        .WithOpenAIChatCompletion(Environment.GetEnvironmentVariable("OpenAIOptions__ModelId"), Environment.GetEnvironmentVariable("OpenAIOptions__ApiKey"))
                        .WithInstructions(@"Provide insightful research that supports the given topic based on your knowledge of the outline topic.
                            <RULE>
                            BEFORE EVERY STEP SEND A DETAILED STATUS MESSAGE WITH THE STATUSPLUGIN SO THE USER KNOWS IF YOU'RE STILL AT WORK! USE 'Researcher' AS THE AGENT NAME.
                            </RULE>")
                        .WithName("Researcher")
                        .WithDescription("Researches information to support the topic.")
                        .WithPlugin(KernelPluginFactory.CreateFromType<StatusPlugin>())
                        .BuildAsync());
        }

    }
}