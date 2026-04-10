using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Application.Exceptions;
using OrderService.Application.Services;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace PureDelivery.OrderService.Controllers;

[ApiController]
[Route("api/v1/order/[controller]")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Получение заказа по ID
    /// </summary>
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<BaseResponse<OrderDto>>> GetOrderById(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(orderId, ct);
            return Ok(result);
        }
        catch (OrderException ex)
        {
            return StatusCode(ex.StatusCode, BaseResponse<OrderDto>.Failure(ex.UserMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", orderId);
            return StatusCode(500, BaseResponse<OrderDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Получение списка заказов клиента (с пагинацией)
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<BaseResponse<PagedResult<OrderDto>>>> GetCustomerOrders(
        [FromRoute] Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
            return StatusCode(500, BaseResponse<PagedResult<OrderDto>>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Обновление статуса заказа (принимает UpdateOrderStatusRequest, но передает параметры в сервис раздельно)
    /// </summary>
    [HttpPut("{orderId:guid}/status")]
    public async Task<ActionResult<BaseResponse<OrderDto>>> UpdateStatus(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct = default)
    {
        try
        {
            // Здесь мапим объект запроса в параметры твоего интерфейса (Guid, OrderStatus, string, ct)
            var result = await _orderService.UpdateStatusAsync(orderId, request.Status, request.Notes, ct);
            return Ok(result);
        }
        catch (OrderException ex)
        {
            return StatusCode(ex.StatusCode, BaseResponse<OrderDto>.Failure(ex.UserMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for {OrderId}", orderId);
            return StatusCode(500, BaseResponse<OrderDto>.Failure("Internal server error"));
        }
    }
}