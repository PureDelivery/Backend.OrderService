using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task AddHistoryAsync(OrderHistory history, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid orderId, CancellationToken cancellationToken = default);
}

