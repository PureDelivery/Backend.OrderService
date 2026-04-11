using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using OrderService.Application.Configuration;

namespace OrderService.Application.Services.impl;

public class EmailService(EmailConfig config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendOrderConfirmationAsync(
        string toEmail, string customerName, string restaurantName,
        Guid orderId, decimal totalAmount, CancellationToken ct = default)
    {
        var subject = "Your order has been placed — PureDelivery";
        var orderUrl = $"{config.AppBaseUrl}/orders/{orderId}";

        var body = GetBaseTemplate("Order Confirmed!", $@"
            <p>Hi <b>{customerName}</b>,</p>
            <p>Your order from <b>{restaurantName}</b> has been received and confirmed.</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                <tr>
                    <td style='padding:8px;color:#6c757d;'>Order ID</td>
                    <td style='padding:8px;font-weight:600;'>{orderId}</td>
                </tr>
                <tr style='background:#f8f9fa;'>
                    <td style='padding:8px;color:#6c757d;'>Restaurant</td>
                    <td style='padding:8px;font-weight:600;'>{restaurantName}</td>
                </tr>
                <tr>
                    <td style='padding:8px;color:#6c757d;'>Total</td>
                    <td style='padding:8px;font-weight:600;color:#28a745;'>${totalAmount:F2}</td>
                </tr>
            </table>
            <div style='text-align:center;margin:24px 0;'>
                <a href='{orderUrl}' style='background:#ff6b35;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:600;font-size:15px;'>
                    Track Your Order
                </a>
            </div>
            <p style='color:#6c757d;font-size:13px;'>We'll notify you as your order progresses.</p>");

        await SendAsync(toEmail, subject, body, ct);
    }

    public async Task SendOrderStatusChangedAsync(
        string toEmail, string customerName, string restaurantName,
        Guid orderId, string newStatusLabel, CancellationToken ct = default)
    {
        var subject = $"Order update: {newStatusLabel} — PureDelivery";
        var orderUrl = $"{config.AppBaseUrl}/orders/{orderId}";

        var (icon, message) = newStatusLabel switch
        {
            "In Preparation" => ("🍳", "The restaurant has started preparing your order."),
            "Ready for Pickup" => ("✅", "Your order is ready and waiting for the courier."),
            "Out for Delivery" => ("🛵", "Your order is on the way!"),
            "Delivered" => ("🎉", "Your order has been delivered. Enjoy your meal!"),
            "Cancelled" => ("❌", "Unfortunately your order has been cancelled."),
            _ => ("📦", $"Your order status has been updated to <b>{newStatusLabel}</b>.")
        };

        var body = GetBaseTemplate($"Order Update: {newStatusLabel}", $@"
            <p>Hi <b>{customerName}</b>,</p>
            <p>{icon} {message}</p>
            <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                <tr>
                    <td style='padding:8px;color:#6c757d;'>Restaurant</td>
                    <td style='padding:8px;font-weight:600;'>{restaurantName}</td>
                </tr>
                <tr style='background:#f8f9fa;'>
                    <td style='padding:8px;color:#6c757d;'>Status</td>
                    <td style='padding:8px;font-weight:600;'>{newStatusLabel}</td>
                </tr>
            </table>
            <div style='text-align:center;margin:24px 0;'>
                <a href='{orderUrl}' style='background:#ff6b35;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:600;font-size:15px;'>
                    View Order
                </a>
            </div>");

        await SendAsync(toEmail, subject, body, ct);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config.FromName, config.FromEmail));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            client.CheckCertificateRevocation = false;

            await client.ConnectAsync(
                config.SmtpHost,
                config.SmtpPort,
                config.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                ct);

            if (!string.IsNullOrEmpty(config.SmtpUsername))
                await client.AuthenticateAsync(config.SmtpUsername, config.SmtpPassword, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation("Email '{Subject}' sent to {Email}", subject, toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email '{Subject}' to {Email}", subject, toEmail);
            throw;
        }
    }

    private static string GetBaseTemplate(string title, string content) => $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'/></head>
<body style='margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f4;padding:40px 0;'>
    <tr><td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);'>
        <tr>
          <td style='background:#ff6b35;padding:28px 40px;text-align:center;'>
            <h1 style='color:#fff;margin:0;font-size:24px;letter-spacing:1px;'>PureDelivery</h1>
          </td>
        </tr>
        <tr>
          <td style='padding:32px 40px;'>
            <h2 style='color:#333;margin-top:0;'>{title}</h2>
            {content}
          </td>
        </tr>
        <tr>
          <td style='background:#f8f9fa;padding:16px 40px;text-align:center;color:#adb5bd;font-size:12px;'>
            © {DateTime.UtcNow.Year} PureDelivery. All rights reserved.
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
}
