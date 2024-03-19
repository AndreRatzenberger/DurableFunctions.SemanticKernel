
# .NET 8 Isolated - Durable Functions + Semantic Kernel Integration Project - Ex01

This example shows how to build a Kernel and send a prompt to it via an Orchestration of an Agent (which are basically just renamed Durable Functions Activities)

## Prerequisites

Before diving into the project, ensure you have the following tools and accounts set up:

- **.NET 8 SDK**: Ensure the latest version is installed for compatibility. [Download .NET 8 SDK](https://dotnet.microsoft.com/download)
- **Azure Functions Core Tools**: Necessary for local development and testing. [Download Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- **Visual Studio or VS Code**: With Azure development and Azure Function Tools extensions installed for a streamlined development experience.
- **An OpenAI API key**: Required for accessing LLM services. Obtain this from the [OpenAI website](https://openai.com) after registration.
- **Durable Function Monitor**: For detailed insights into function orchestrations and activities.
- **Azurite or Azure Storage Account**: Durable Functions require storage to manage state. Azurite offers a local storage emulator suitable for development.
- **Azure Storage Explorer**: Optional but recommended for managing and inspecting Azure Storage resources.

## Setup and Configuration

1. **Clone the Repository**: Start by cloning this repository to your local development environment.

2. **Secure Your API Key**: Store your OpenAI API key securely. For local development, `local.settings.json` is recommended. For production, consider Azure Key Vault.

3. **Install Dependencies**: Execute `dotnet restore` to install the required NuGet packages.

4. **Local Development Setup**:
    - Start the Azure Functions runtime using `func start`.
    - Optionally, use the Durable Function Monitor for a visual interface to your orchestrations and activities.

5. **Configuration File**: Set up your `local.settings.json` as follows, replacing placeholders with actual values:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "OpenAIOptions__ApiKey": "<Your_OpenAI_API_Key>",
    "OpenAIOptions__ModelId": "gpt-4-turbo-preview"
  }
}
```

## Running the Project

- Execute the function app locally using the Azure Functions Core Tools. Navigate to the project directory and run `func start`.
- To test the HTTP-triggered function, send a request via Postman, curl, or any HTTP client to the function's endpoint with appropriate input.

## Project Structure

- `Program.cs`: Initializes and configures the function app.
- `Options/OpenAIOptions.cs`: Holds configuration options for OpenAI integration.
- `Agents/SimplePromptQandAAgent.cs`: Implements an agent capable of processing prompts and returning responses.
- `Triggers/HttpStart.cs`: Defines an HTTP trigger for initiating the orchestrator function.
- `Orchestrators/SemanticKernel.cs`: Orchestrates agent interactions and manages the lifecycle of kernel tasks.
- `README.md`: Provides project documentation.



## License

This project is licensed under the MIT License - see the LICENSE file for details.
