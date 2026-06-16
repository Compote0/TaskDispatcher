using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebDispatcher.Models;

public class Sprint
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    [ValidateNever]
    public Project Project { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public bool IsActive { get; set; }
    public string? Goal { get; set; }
}
