namespace DurableFunctions.SemanticKernel.Agents.Models
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

}