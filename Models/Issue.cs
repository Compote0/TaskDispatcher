using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebDispatcher.Models;

public enum IssueStatus { Backlog, Todo, InProgress, InReview, Done }
public enum IssuePriority { Low, Medium, High, Critical }
public enum IssueType { Task, Bug, Feature }

public class Issue
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    [ValidateNever]
    public Project Project { get; set; } = null!;

    public int IssueNumber { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public IssueStatus Status { get; set; } = IssueStatus.Todo;

    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public IssueType Type { get; set; } = IssueType.Task;

    public string? AssigneeId { get; set; }

    [ValidateNever]
    public IdentityUser? Assignee { get; set; }

    public string? ReporterId { get; set; }

    [ValidateNever]
    public IdentityUser? Reporter { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }
}
