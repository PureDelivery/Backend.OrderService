using Microsoft.Extensions.Logging;
using OrderService.Application.Integration;
using OrderService.Application.Services;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Обробник події завершення оплати від Payment Service.
/// </summary>
public class PaymentCompletedEventHandler
{
    private readonly IOrderSessionService _orderSessionService;
    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(
        IOrderSessionService orderSessionService,
        ILogger<PaymentCompletedEventHandler> logger)
    {
        _orderSessionService = orderSessionService;
        _logger = logger;
    }

    public async Task HandlePaymentCompletedAsync(
        string sessionId,
        string restaurantId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Payment completed for session {SessionId}, restaurant {RestaurantId}, payment {PaymentId}",
                sessionId, restaurantId, paymentId);

            var orderId = await _orderSessionService.SaveOrderFromSessionAsync(sessionId, restaurantId, paymentId, cancellationToken);

            _logger.LogInformation("Order {OrderId} persisted for restaurant {RestaurantId} after payment", orderId, restaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment completed event for session {SessionId}, restaurant {RestaurantId}",
                sessionId, restaurantId);
            throw;
        }
    }
}
