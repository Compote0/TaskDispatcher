namespace WebDispatcher.Models;

public static class IssueStatusHelper
{
    public static string GetLabel(IssueStatus status) => status switch
    {
        IssueStatus.Backlog    => "Backlog",
        IssueStatus.Todo       => "À faire",
        IssueStatus.InProgress => "En cours",
        IssueStatus.InReview   => "En revue",
        IssueStatus.Done       => "Terminé",
        _ => status.ToString()
    };

    public static string GetColor(IssueStatus status) => status switch
    {
        IssueStatus.Backlog    => "#6c757d",
        IssueStatus.Todo       => "#0052CC",
        IssueStatus.InProgress => "#00B8D9",
        IssueStatus.InReview   => "#FFAB00",
        IssueStatus.Done       => "#36B37E",
        _ => "#6c757d"
    };

    public static string GetCssClass(IssueStatus status) => status switch
    {
        IssueStatus.Backlog    => "status-backlog",
        IssueStatus.Todo       => "status-todo",
        IssueStatus.InProgress => "status-inprogress",
        IssueStatus.InReview   => "status-inreview",
        IssueStatus.Done       => "status-done",
        _ => "status-backlog"
    };
}
