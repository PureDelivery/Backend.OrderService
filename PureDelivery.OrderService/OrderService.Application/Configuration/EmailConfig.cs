namespace OrderService.Application.Configuration;

public class EmailConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "PureDelivery";
    public bool EnableSsl { get; set; } = true;
    public string AppBaseUrl { get; set; } = "http://localhost:3000";
}
