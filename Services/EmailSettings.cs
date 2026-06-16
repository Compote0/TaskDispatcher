namespace WebDispatcher.Services;

public class EmailSettings
{
    public string Provider { get; set; } = "Console";
    public string From { get; set; } = "noreply@webdispatcher.local";
    public string FromName { get; set; } = "TaskDispatcher";
    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
}
