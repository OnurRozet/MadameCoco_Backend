namespace MadameCoco.Audit.Worker.Models;

public class SmtpSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public string RecipientEmail { get; set; } = default!;
}