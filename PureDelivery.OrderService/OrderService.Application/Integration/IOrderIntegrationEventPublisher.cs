using PureDelivery.Shared.Contracts.Events.Loyalty;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Integration;

public interface IOrderIntegrationEventPublisher
{
    Task PublishOrderProcessedAsync(OrderProcessedEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishOrderStatusChangedAsync(OrderStatusChangedEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishLoyaltyEarnedAsync(LoyaltyEarnedByOrderEvent integrationEvent, CancellationToken cancellationToken = default);
}
