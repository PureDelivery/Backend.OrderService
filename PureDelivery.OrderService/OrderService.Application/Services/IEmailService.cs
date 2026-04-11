namespace OrderService.Application.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(
        string toEmail,
        string customerName,
        string restaurantName,
        Guid orderId,
        decimal totalAmount,
        CancellationToken ct = default);

    Task SendOrderStatusChangedAsync(
        string toEmail,
        string customerName,
        string restaurantName,
        Guid orderId,
        string newStatusLabel,
        CancellationToken ct = default);
}
