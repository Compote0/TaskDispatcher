using Microsoft.AspNetCore.Identity;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Services;

public static class IssueActivityService
{
    public static async Task LogAsync(
        ApplicationDbContext context,
        int issueId,
        string userId,
        ActivityType type,
        string? oldValue = null,
        string? newValue = null)
    {
        context.IssueActivities.Add(new IssueActivity
        {
            IssueId = issueId,
            UserId = userId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}
