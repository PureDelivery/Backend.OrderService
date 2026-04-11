using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace OrderService.Application.Services;

public interface IOrderService
{
    Task<Order> SaveNewOrderAsync(Order order, CancellationToken ct);

    Task<BaseResponse<OrderDto>> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default);

    Task<BaseResponse<PagedResult<OrderDto>>> GetCustomerOrdersAsync(Guid customerId, int page, int pageSize, CancellationToken ct);

    Task<BaseResponse<PagedResult<OrderDto>>> GetRestaurantOrdersAsync(Guid restaurantId, int page, int pageSize, CancellationToken ct);

    Task<BaseResponse<OrderDto>> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, string changedBy, string comment, CancellationToken ct);
}