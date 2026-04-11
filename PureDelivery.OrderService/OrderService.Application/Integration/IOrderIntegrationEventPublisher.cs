using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Integration;

public interface IOrderIntegrationEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishOrderStatusChangedAsync(OrderStatusChangedEvent integrationEvent, CancellationToken cancellationToken = default);
}
