namespace WebDispatcher.Models;

public class StatusPickerModel
{
    public int IssueId { get; set; }
    public IssueStatus Status { get; set; }
    public string Size { get; set; } = "sm";
}
