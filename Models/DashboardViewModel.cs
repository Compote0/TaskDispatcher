namespace WebDispatcher.Models;

public class DashboardViewModel
{
    public List<Project> RecentProjects { get; set; } = [];
    public List<IssueActivity> RecentActivity { get; set; } = [];
    public List<Issue> AssignedIssues { get; set; } = [];
    public int AssignedCount { get; set; }
    public int OpenAssignedCount { get; set; }
}
