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
        public string Id { get; set; }
        public string Title { get; set; }
        public int StoryPoints { get; set; }
    }

    public class ProjectFiles
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class ProjectFilesList
    {
        public List<ProjectFiles> ProjectFiles { get; set; }
    }   

    public class ProjectTask
    {
        public string Id { get; set; }
        public string UserStoryId { get; set; }
        public string Title { get; set; }
        public int EstimatedLinesOfCode { get; set; }
        public int EstimatedHoursNeeded { get; set; }
    }

    public class UserStoryList
    {
        public List<UserStory> UserStories { get; set; }
    }

    public class ProjectTaskList
    {
        public List<ProjectTask> ProjectTasks { get; set; }
    }

    public class ProjectAgent : BaseAgent
    {

        private readonly ConfigurationService _configurationService;
        private Kernel _kernel;
        private string? _projectPlannerResponse;
        private string? _userStoryDesignerResponse;
        private string? _taskDesignerResponse;
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
            Guid guid;
            if (Guid.TryParse(input, out var parsedGuid))
            {
                guid = parsedGuid;
                await LoadFiles(guid);
            }
            else
            {
                guid = Guid.NewGuid();
            }
            var guidString = guid.ToString();
          

            await SendMessage("**ProjectPlanner STARTED...**");
            if(string.IsNullOrEmpty(_projectPlannerResponse))
            {
                var responseProjectPlanner = await _kernel.InvokeAsync("Plugins", "ProjectPlanner", new() {
                    { "input", input }
                });
                _projectPlannerResponse = responseProjectPlanner.GetValue<string>();
            }

            await SendMessage(_projectPlannerResponse);
            await WriteProjectFiles(guidString,_projectPlannerResponse,"ProjectPlan.md");
          

            

            await SendMessage("**UserStory designer STARTED...**");
            if(!_userStories.Any()){
                 var responseUserStoryDesigner = await _kernel.InvokeAsync("Plugins", "UserStoryDesigner", new() {
                    { "input", _projectPlannerResponse }
                });
                _userStoryDesignerResponse = responseUserStoryDesigner.GetValue<string>();
                _userStories = await GenerateValidUserStories(_userStoryDesignerResponse);
            }
           
            await SendMessage(JsonSerializer.Serialize(_userStories));
            await WriteProjectFiles(guidString,JsonSerializer.Serialize(_userStories),"userstories.json");

        
            foreach (var userStory in _userStories)
            {
                await SendMessage($"**Task designer for '{userStory.Id} - {userStory.Title}' STARTED...**");
                if(_projectTasks.Any(task => task.UserStoryId == userStory.Id))
                {
                    var taskList = _projectTasks.Where(task => task.UserStoryId == userStory.Id).ToList();
                    await SendMessage(JsonSerializer.Serialize(taskList));
                    continue;
                }
                
                var responseTaskDesigner = await _kernel.InvokeAsync("Plugins", "TaskDesigner", new() {
                    { "inputProjectPlan", _projectPlannerResponse },
                    { "inputAlreadyCreatedItems", $"{JsonSerializer.Serialize(_userStories)} \n {JsonSerializer.Serialize(_projectTasks)}" },
                    { "inputUserStory", JsonSerializer.Serialize(userStory) }
                });

                _taskDesignerResponse = responseTaskDesigner.GetValue<string>();
                await SendMessage(_taskDesignerResponse);

                var tasks = await GenerateValidTasks(_taskDesignerResponse);
                await WriteProjectFiles(guidString,JsonSerializer.Serialize(tasks),$"task_{userStory.Id}.json");
                _projectTasks.AddRange(tasks);
            }

            var responseRepo = await _kernel.InvokeAsync("Plugins", "GenerateRepositoryStructure", new() {
                    { "inputHighLevelProjectPlan", _projectPlannerResponse },
                    { "inputUserStories", JsonSerializer.Serialize(_userStories) },
                    { "inputTasks", JsonSerializer.Serialize(_projectTasks) }
                });

            var repoStructure = responseRepo.GetValue<string>();
            _projectFiles = await GenerateValidRepo(repoStructure);

            await GenerateFiles(_projectFiles,guid);


            return JsonSerializer.Serialize(_projectFiles);
        }

        private async Task GenerateFiles(List<ProjectFiles> projectFiles, Guid guid)
        {
            string folderPath = Path.Combine("..", "..", "..", ".dump", guid.ToString(), "repo");
            Directory.CreateDirectory(folderPath);
            foreach (var file in projectFiles)
            {
                string filePath = Path.Combine(folderPath, file.FileName);
                string fileDirectory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(fileDirectory);
                await File.WriteAllTextAsync(filePath, file.Content);
            }
        }

        private async Task<List<ProjectFiles>> GenerateValidRepo(string? input)
        {
            var stringResponse = input;
            try
            {
                var jsonResult = JsonSerializer.Deserialize<List<ProjectFiles>>(stringResponse);
                return jsonResult;
            }
            catch (Exception ex)
            {
                var error = "Error: The response is not in the expected format." + ex.Message;
                while (true)
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
                        { "inputData", stringResponse },
                        { "inputError", error}
                    });
                    stringResponse = responseCleanup.GetValue<string>();
                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<List<ProjectFiles>>(stringResponse);
                        return jsonResult;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            var jsonResult = JsonSerializer.Deserialize<ProjectFilesList>(stringResponse);
                            return jsonResult.ProjectFiles;
                        }
                        catch (Exception)
                        {

                        }
                        error = "Error: The response is not in the expected format." + e.Message;
                    }
                }
            }
        }

        private async Task LoadFiles(Guid guid)
        {
            try
            {
                _projectTasks = new List<ProjectTask>();
           
                string folderPath = Path.Combine("..", "..", "..", ".dump", guid.ToString());

                string filePath = Path.Combine(folderPath, "ProjectPlan.md");
                _projectPlannerResponse = File.ReadAllText(filePath);

                filePath = Path.Combine(folderPath, "userstories.json");
                _userStories = JsonSerializer.Deserialize<List<UserStory>>(File.ReadAllText(filePath));

                foreach (UserStory item in _userStories)
                {
                    filePath = Path.Combine(folderPath, $"task_{item.Id}.json");
                    var tasks = JsonSerializer.Deserialize<List<ProjectTask>>(File.ReadAllText(filePath));
                    _projectTasks.AddRange(tasks);
                }
            }
            catch (Exception)
            {
              
            }
        }

        private static async Task WriteProjectFiles(string guid, string? input, string name)
        {
            string folderPath = Path.Combine("..", "..", "..", ".dump", guid);
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, name);
            await File.WriteAllTextAsync(filePath, input);
        }

        private async Task<List<ProjectTask>> GenerateValidTasks(string? input) 
        {
            var stringResponse = input;
            try
            {
                var jsonResult = JsonSerializer.Deserialize<List<ProjectTask>>(stringResponse);
                return jsonResult;
            }
            catch (Exception ex)
            {
                var error = "Error: The response is not in the expected format." + ex.Message;
                while (true)
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
                        { "inputData", stringResponse },
                        { "inputError", error}
                    });
                    stringResponse = responseCleanup.GetValue<string>();
                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<List<ProjectTask>>(stringResponse);
                        return jsonResult;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            var jsonResult = JsonSerializer.Deserialize<ProjectTaskList>(stringResponse);
                            return jsonResult.ProjectTasks;
                        }
                        catch (Exception)
                        {

                        }
                        error = "Error: The response is not in the expected format." + e.Message;
                    }
                }
            }
        }

        private async Task<List<UserStory>> GenerateValidUserStories(string? input)
        {
            var stringResponse = input;
            try
            {
                var jsonResult = JsonSerializer.Deserialize<List<UserStory>>(stringResponse);
                return jsonResult;
            }
            catch (Exception ex)
            {
                var error = "Error: The response is not in the expected format." + ex.Message;
                while (true)
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
                        { "inputData", stringResponse },
                        { "inputError", error}
                    });
                    stringResponse = responseCleanup.GetValue<string>();
                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<List<UserStory>>(stringResponse);
                        return jsonResult;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            var jsonResult = JsonSerializer.Deserialize<UserStoryList>(stringResponse);
                            return jsonResult.UserStories;
                        }
                        catch (Exception)
                        {

                        }
                        error = "Error: The response is not in the expected format." + e.Message;
                    }
                }
            }
        }
    }
}