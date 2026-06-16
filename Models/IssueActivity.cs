using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebDispatcher.Models;

public enum ActivityType { Created, StatusChanged, Updated, Assigned }

public class IssueActivity
{
    public int Id { get; set; }
    public int IssueId { get; set; }

    [ValidateNever]
    public Issue Issue { get; set; } = null!;

    public string UserId { get; set; } = "";

    [ValidateNever]
    public IdentityUser User { get; set; } = null!;

    public ActivityType Type { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
