using Microsoft.Azure.Functions.Worker;
using Microsoft.SemanticKernel;
using DurableFunctions.SemanticKernel.Options;
using DurableFunctions.SemanticKernel.Extentions;
using System.Text.Json;
using Microsoft.SemanticKernel.Plugins.Core;

namespace DurableFunctions.SemanticKernel.Agents
{
    public class UserStory
    {
        public string Id { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty; 
        public int StoryPoints { get; set; }
    }
    public class ProjectFiles
    {
        public string FileName { get; set; } = string.Empty; 
        public string Content { get; set; } = string.Empty; 
    }
    public class ProjectFilesList
    {
        public List<ProjectFiles> ProjectFiles { get; set; } = []; 
    }
    public class ProjectTask
    {
        public string Id { get; set; } = string.Empty; 
        public string UserStoryId { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty; 
        public int EstimatedLinesOfCode { get; set; }
        public int EstimatedHoursNeeded { get; set; }
    }
    public class UserStoryList
    {
        public List<UserStory> UserStories { get; set; } = []; 
    }
    public class ProjectTaskList
    {
        public List<ProjectTask> ProjectTasks { get; set; } = []; 
    }

    public class ProjectAgent : BaseAgent
    {
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
            builder.Plugins.AddFromType<FileIOPlugin>();
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

            Guid guid;
            if (Guid.TryParse(input, out var parsedGuid))
            {
                guid = parsedGuid;
                await LoadFiles(guid);
            }
            else
                guid = Guid.NewGuid();

            var guidString = guid.ToString();

            await SendMessage("**ProjectPlanner STARTED...**");
            if (string.IsNullOrEmpty(_projectPlannerResponse))
            {
                var responseProjectPlanner = await _kernel.InvokeAsync("Plugins", "ProjectPlanner", new() {
                    { "input", input }
                });
                _projectPlannerResponse = responseProjectPlanner.GetValue<string>();
            }

            await SendMessage(_projectPlannerResponse);
            await WriteProjectFiles(guidString, _projectPlannerResponse, "ProjectPlan.md");

            await SendMessage("**UserStory designer STARTED...**");
            if (_userStories.Count == 0)
            {
                var responseUserStoryDesigner = await _kernel.InvokeAsync("Plugins", "UserStoryDesigner", new() {
                    { "input", _projectPlannerResponse }
                });
                _userStories = await GenerateValidUserStories(responseUserStoryDesigner.GetValue<string>());
            }

            await SendMessage($"```json\n{JsonSerializer.Serialize(_userStories, jsonOptions)}\n```");
            await WriteProjectFiles(guidString, JsonSerializer.Serialize(_userStories, jsonOptions), "userstories.json");

            foreach (var userStory in _userStories)
            {
                await SendMessage($"**Task designer for '{userStory.Id} - {userStory.Title}' STARTED...**");
                if (_projectTasks.Any(task => task.UserStoryId == userStory.Id))
                {
                    var taskList = _projectTasks.Where(task => task.UserStoryId == userStory.Id).ToList();
                    await SendMessage($"```json\n{JsonSerializer.Serialize(taskList, jsonOptions)}\n```");
                    continue;
                }

                var responseTaskDesigner = await _kernel.InvokeAsync("Plugins", "TaskDesigner", new() {
                    { "inputProjectPlan", _projectPlannerResponse },
                    { "inputAlreadyCreatedItems", $"{JsonSerializer.Serialize(_userStories)} \n {JsonSerializer.Serialize(_projectTasks)}" },
                    { "inputUserStory", JsonSerializer.Serialize(userStory) }
                });

                var tasks = await GenerateValidTasks(responseTaskDesigner.GetValue<string>());
                await SendMessage($"```json\n{JsonSerializer.Serialize(tasks, jsonOptions)}\n```");
                await WriteProjectFiles(guidString, JsonSerializer.Serialize(tasks, jsonOptions), $"task_{userStory.Id}.json");
                _projectTasks.AddRange(tasks);
            }

            await SendMessage("**Repo Initializer STARTED...**");
            var responseRepo = await _kernel.InvokeAsync("Plugins", "GenerateRepositoryStructure", new() {
                    { "inputHighLevelProjectPlan", _projectPlannerResponse },
                    { "inputUserStories", JsonSerializer.Serialize(_userStories) },
                    { "inputTasks", JsonSerializer.Serialize(_projectTasks) }
                });

            var repoStructure = responseRepo.GetValue<string>();
            _projectFiles = await GenerateValidRepo(repoStructure);
            await SendMessage($"```json\n{JsonSerializer.Serialize(_projectFiles, jsonOptions)}\n```");
            await WriteProjectFiles(guidString, JsonSerializer.Serialize(_projectFiles, jsonOptions), $"repo.json");
            await GenerateFiles(_projectFiles, guid);

            return JsonSerializer.Serialize(_projectFiles, jsonOptions);
        }

        private async Task GenerateFiles(List<ProjectFiles> projectFiles, Guid guid)
        {
            string baseFolderPath = Path.Combine("..", "..", "..", ".dump", guid.ToString(), "repo");
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
            string folderPath = Path.Combine("..", "..", "..", ".dump", guid.ToString());
            _projectTasks = [];
            _userStories = [];

            try
            {
                string projectPlanPath = Path.Combine(folderPath, "ProjectPlan.md");
                if (File.Exists(projectPlanPath))
                    _projectPlannerResponse = await File.ReadAllTextAsync(projectPlanPath);

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
        private static async Task WriteProjectFiles(string guid, string? input, string name)
        {
            string folderPath = Path.Combine("..", "..", "..", ".dump", guid);
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