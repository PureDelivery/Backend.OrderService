using PureDelivery.Shared.Contracts.DTOs.Session;
using SessionDto = PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto;

namespace OrderService.Application.Services;

/// <summary>
/// Сервіс для роботи з замовленнями в Redis сесії (OrderStates у SessionDto).
/// </summary>
public interface IOrderSessionService
{
    /// <summary>
    /// Зберігає стейт конкретного ресторану у словник сесії.
    /// </summary>
    Task<SessionDto> UpdateOrderStateInSessionAsync(string sessionId, OrderStateDto orderState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримує стейт конкретного ресторану. Якщо restaurantId не вказано, можна повертати останній оновлений.
    /// </summary>
    Task<OrderStateDto?> GetOrderStateFromSessionAsync(string sessionId, string? restaurantId = null, CancellationToken cancellationToken = default);

    Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Після успішної оплати: запис замовлення конкретного ресторану в БД та видалення його з сесії.
    /// </summary>
    Task<Guid> SaveOrderFromSessionAsync(string sessionId, string restaurantId, Guid? paymentId, decimal paidAmount, int paymentMethodCode, CancellationToken cancellationToken = default);
}