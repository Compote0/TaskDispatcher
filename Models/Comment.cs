using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebDispatcher.Models;

public class Comment
{
    public int Id { get; set; }
    public int IssueId { get; set; }

    [ValidateNever]
    public Issue Issue { get; set; } = null!;

    public string AuthorId { get; set; } = "";

    [ValidateNever]
    public IdentityUser Author { get; set; } = null!;

    [Required, MaxLength(5000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
}
