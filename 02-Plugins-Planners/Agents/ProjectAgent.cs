using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using System.Text.Json;
using DurableFunctions.SemanticKernel.Common;
using DurableFunctions.SemanticKernel.Agents.Models;
using DurableFunctions.SemanticKernel.Extensions;


namespace DurableFunctions.SemanticKernel.Agents
{
    public class ProjectAgent : BaseAgent
    {
        private readonly string _baseFolderPath = Path.Combine("..", "..", "..", ".generated");
        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;
        private string? _projectPlannerResponse;
        private List<UserStory> _userStories = [];
        private List<ProjectTask> _projectTasks = [];
        private List<ProjectFiles> _projectFiles = [];

        public ProjectAgent(ConfigurationService configurationService)
        {
            _configurationService = configurationService;

            var builder = Kernel.CreateBuilder();
            builder.Plugins.AddFromPromptDirectory("Agents/Plugins");
            _kernel = builder.WithOptionsConfiguration(_configurationService.GetCurrentConfiguration())
            .Build();
        }

        [Function($"{nameof(ProjectAgent)}_Start")]
        public async Task<string?> Start([ActivityTrigger] string input, FunctionContext context)
        {
            return await StartTemplate(input, context);
        }

        protected override async Task<string?> ExecuteAgent(string input)
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            //If the input is a valid guid, we will load the files from the disk 
            //to continue the process where it was left off
            var guid = await ProcessInputGuid(input);

            await SendMessage($"**Your Agent Run ID: {guid}**");

            await SendMessage("**ProjectPlanner STARTED...**");
            await EnsureProjectPlannerResponse(input);

            await SendMessage(_projectPlannerResponse);
            await WriteProjectFiles(guid.ToString(), _projectPlannerResponse, "ProjectPlan.md");

            await SendMessage("**UserStory designer STARTED...**");
            await EnsureUserStoriesGenerated();

            await SendMessage(JsonHelpers.SerializeJsonToMarkdown(_userStories, jsonOptions));
            await WriteProjectFiles(guid.ToString(), JsonHelpers.SerializeJson(_userStories, jsonOptions), "userstories.json");

            await ProcessUserStories(guid, jsonOptions);

            await SendMessage("**Repo Initializer STARTED...**");
            await ProcessRepoInitializer(guid, jsonOptions);

            return $"**Your Agent Run ID: {guid}**";
        }

        private async Task<Guid> ProcessInputGuid(string input)
        {
            if (Guid.TryParse(input, out var guid))
            {
                await LoadFiles(guid);
                return guid;
            }
            return Guid.NewGuid();
        }

        private async Task EnsureProjectPlannerResponse(string input)
        {
            if (string.IsNullOrEmpty(_projectPlannerResponse))
            {
                var response = await InvokeFunction("ProjectPlanner", new KernelArguments { { "input", input } });
                _projectPlannerResponse = response.GetValue<string>();
            }
        }

        private async Task EnsureUserStoriesGenerated()
        {
            if (_userStories.Count == 0)
            {
                var response = await InvokeFunction("UserStoryDesigner", new KernelArguments { { "input", _projectPlannerResponse } });
                var responseString = response.GetValue<string>();
                _userStories = await GenerateValidUserStories(responseString);
            }
        }

        private async Task ProcessUserStories(Guid guid, JsonSerializerOptions jsonOptions)
        {
            foreach (var userStory in _userStories)
            {
                await SendMessage($"**Task designer for '{userStory.Id} - {userStory.Title}' STARTED...**");
                var taskList = _projectTasks.Where(task => task.UserStoryId == userStory.Id).ToList();
                if (taskList.Count != 0)
                {
                    await SendMessage(JsonHelpers.SerializeJsonToMarkdown(taskList, jsonOptions));
                    continue;
                }

                var tasks = await GenerateAndProcessTasks(userStory);
                await SendMessage(JsonHelpers.SerializeJsonToMarkdown(tasks, jsonOptions));
                await WriteProjectFiles(guid.ToString(), JsonHelpers.SerializeJson(tasks, jsonOptions), $"task_{userStory.Id}.json");
                _projectTasks.AddRange(tasks);
            }
        }

        private async Task<List<ProjectTask>> GenerateAndProcessTasks(UserStory userStory)
        {
            var response = await InvokeFunction("TaskDesigner", new KernelArguments
            {
                { "inputProjectPlan", _projectPlannerResponse },
                { "inputAlreadyCreatedItems", $"{JsonHelpers.SerializeJson(_userStories)} \n {JsonHelpers.SerializeJson(_projectTasks)}" },
                { "inputUserStory", JsonHelpers.SerializeJson(userStory) }
            });

            var responseString = response.GetValue<string>();
            return await GenerateValidTasks(responseString);
        }

        private async Task ProcessRepoInitializer(Guid guid, JsonSerializerOptions jsonOptions)
        {
            if (_projectFiles.Count == 0)
            {
                var response = await InvokeFunction("GenerateRepositoryStructure", new KernelArguments
                {
                    { "inputHighLevelProjectPlan", _projectPlannerResponse },
                    { "inputUserStories", JsonHelpers.SerializeJson(_userStories) },
                    { "inputTasks", JsonHelpers.SerializeJson(_projectTasks) }
                });

                var responseString = response.GetValue<string>();
                _projectFiles = await GenerateValidRepo(responseString);
            }

            await SendMessage(JsonHelpers.SerializeJsonToMarkdown(_projectFiles, jsonOptions));
            await WriteProjectFiles(guid.ToString(), JsonHelpers.SerializeJson(_projectFiles, jsonOptions), "repo.json");
            await GenerateFiles(_projectFiles, guid);
        }

        private async Task<dynamic> InvokeFunction(string functionName, KernelArguments parameters)
        {
            return await _kernel.InvokeAsync("Plugins", functionName, parameters);
        }

        private async Task GenerateFiles(List<ProjectFiles> projectFiles, Guid guid)
        {
            string baseFolderPath = Path.Combine(_baseFolderPath, guid.ToString(), "repo");
            Directory.CreateDirectory(baseFolderPath);
            foreach (var file in projectFiles)
            {
                string filePath = Path.Combine(baseFolderPath, file.FileName.Replace('/', Path.DirectorySeparatorChar));
                string? fileDirectory = Path.GetDirectoryName(filePath);
                if (fileDirectory != null)
                    Directory.CreateDirectory(fileDirectory);

                try
                {
                    await File.WriteAllTextAsync(filePath, file.Content);
                }
                catch (Exception ex)
                {
                    await SendMessage($"Error: {ex.Message}");
                }
            }
        }
        private async Task LoadFiles(Guid guid)
        {
            string folderPath = Path.Combine(_baseFolderPath, guid.ToString());
            _projectTasks = [];
            _userStories = [];

            try
            {
                string projectPlanPath = Path.Combine(folderPath, "ProjectPlan.md");
                if (File.Exists(projectPlanPath))
                    _projectPlannerResponse = await File.ReadAllTextAsync(projectPlanPath);

                string repoJsonPath = Path.Combine(folderPath, "repo.json");
                if (File.Exists(repoJsonPath))
                {
                    var repoContent = await File.ReadAllTextAsync(repoJsonPath);
                    var repoFiles = JsonSerializer.Deserialize<List<ProjectFiles>>(repoContent);
                    if (repoFiles != null)
                        _projectFiles = repoFiles;
                }

                string userStoriesPath = Path.Combine(folderPath, "userstories.json");
                if (File.Exists(userStoriesPath))
                {
                    var userStoriesContent = await File.ReadAllTextAsync(userStoriesPath);
                    var userStories = JsonSerializer.Deserialize<List<UserStory>>(userStoriesContent);
                    if (userStories != null)
                        _userStories = userStories;
                }

                foreach (UserStory item in _userStories)
                {
                    string taskFilePath = Path.Combine(folderPath, $"task_{item.Id}.json");
                    if (File.Exists(taskFilePath))
                    {
                        var taskContent = await File.ReadAllTextAsync(taskFilePath);
                        var tasks = JsonSerializer.Deserialize<List<ProjectTask>>(taskContent);
                        if (tasks != null)
                            _projectTasks.AddRange(tasks);
                    }
                }
            }
            catch (Exception ex)
            {
                await SendMessage($"Error in LoadFiles: {ex.Message}");
            }
        }
        private async Task WriteProjectFiles(string guid, string? input, string name)
        {
            string folderPath = Path.Combine(_baseFolderPath, guid);
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, name);
            await File.WriteAllTextAsync(filePath, input);
        }
        private async Task<List<ProjectFiles>> GenerateValidRepo(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            const int maxRetries = 5;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var jsonResult = JsonSerializer.Deserialize<List<ProjectFiles>>(input);
                    if (jsonResult != null) return jsonResult;

                    var jsonResultList = JsonSerializer.Deserialize<ProjectFilesList>(input);
                    if (jsonResultList != null) return jsonResultList.ProjectFiles;

                    throw new InvalidOperationException("Deserialization returned null.");
                }
                catch (JsonException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException("Failed to parse input as List<UserStory> after retries.", ex);
                    }

                    // Attempt to fix the input JSON.
                    try
                    {
                        var responseCleanup = await _kernel.InvokeAsync("Plugins", "JsonFixer", new() {
                        { "inputTargetFormat", @"a json array of Files.
                                                Definition of a File:
                                                public class File
                                                {
                                                    public string FileName { get; set; }
                                                    public string Content { get; set; }
                                                }
                                                So the output should be [{File1},{File2}....]" },
                            { "inputData", input },
                            { "inputError", ex.Message}
                        });
                        input = responseCleanup.GetValue<string>() ?? string.Empty;
                    }
                    catch (Exception fixException)
                    {
                        throw new InvalidOperationException("Error fixing the JSON input.", fixException);
                    }
                }
            }
            return [];
        }
        private async Task<List<ProjectTask>> GenerateValidTasks(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            const int maxRetries = 5;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var jsonResult = JsonSerializer.Deserialize<List<ProjectTask>>(input);
                    if (jsonResult != null) return jsonResult;

                    var jsonResultList = JsonSerializer.Deserialize<ProjectTaskList>(input);
                    if (jsonResultList != null) return jsonResultList.ProjectTasks;

                    throw new InvalidOperationException("Deserialization returned null.");
                }
                catch (JsonException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException("Failed to parse input as List<UserStory> after retries.", ex);
                    }

                    // Attempt to fix the input JSON.
                    try
                    {
                        var responseCleanup = await _kernel.InvokeAsync("Plugins", "JsonFixer", new() {
                        { "inputTargetFormat", @"a json array of Tasks.
                                                Definition of a Task:
                                                public class Task
                                                {
                                                    public string Id { get; set; }
                                                    public string UserStoryId { get; set; }
                                                    public string Title { get; set; }
                                                    public int EstimatedLinesOfCode { get; set; }
                                                    public int EstimatedHoursNeeded { get; set; }
                                                }
                                                So the output should be [{Task1},{Task2}....]" },
                            { "inputData", input },
                            { "inputError", ex.Message}
                        });
                        input = responseCleanup.GetValue<string>() ?? string.Empty;
                    }
                    catch (Exception fixException)
                    {
                        throw new InvalidOperationException("Error fixing the JSON input.", fixException);
                    }
                }
            }
            return [];
        }
        private async Task<List<UserStory>> GenerateValidUserStories(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            const int maxRetries = 5;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    var jsonResult = JsonSerializer.Deserialize<List<UserStory>>(input);
                    if (jsonResult != null) return jsonResult;

                    var jsonResultList = JsonSerializer.Deserialize<UserStoryList>(input);
                    if (jsonResultList != null) return jsonResultList.UserStories;

                    throw new InvalidOperationException("Deserialization returned null.");
                }
                catch (JsonException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException("Failed to parse input as List<UserStory> after retries.", ex);
                    }

                    // Attempt to fix the input JSON.
                    try
                    {
                        var responseCleanup = await _kernel.InvokeAsync("Plugins", "JsonFixer", new() {
                            { "inputTargetFormat", @"a json array of UserStories.
                                                    Definition of UserStory:
                                                    public class UserStory
                                                    {
                                                        public string Id { get; set; }
                                                        public string Title { get; set; }
                                                        public int StoryPoints { get; set; }
                                                    }
                                                    So the output should be [{UserStory1},{UserStory2}....]" },
                            { "inputData", input },
                            { "inputError", ex.Message}
                        });
                        input = responseCleanup.GetValue<string>() ?? string.Empty;
                    }
                    catch (Exception fixException)
                    {
                        throw new InvalidOperationException("Error fixing the JSON input.", fixException);
                    }
                }
            }
            return [];
        }
    }
}