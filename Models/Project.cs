using System.ComponentModel.DataAnnotations;

namespace WebDispatcher.Models;

public class Project
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(10), RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Key must be uppercase letters and digits only.")]
    public string Key { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Issue> Issues { get; set; } = [];
    public ICollection<Sprint> Sprints { get; set; } = [];
}
