using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Exceptions;
using OrderService.Application.Services;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Session;
using SessionDto = PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto;

namespace PureDelivery.OrderService.Controllers;

[ApiController]
[Route("api/v1/order/[controller]")]
[Produces("application/json")]
public class OrderSessionController : ControllerBase
{
    private readonly IOrderSessionService _orderSessionService;
    private readonly ILogger<OrderSessionController> _logger;

    public OrderSessionController(
        IOrderSessionService orderSessionService,
        ILogger<OrderSessionController> logger)
    {
        _orderSessionService = orderSessionService;
        _logger = logger;
    }

    /// <summary>
    /// Повна сесія з Redis (клієнт, OrderState / корзина).
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(BaseResponse<SessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<SessionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResponse<SessionDto>>> GetSession(
        [FromRoute] string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _orderSessionService.GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(BaseResponse<SessionDto>.Failure("Session not found"));
            }

            return Ok(BaseResponse<SessionDto>.Success(session));
        }
        catch (OrderException ex)
        {
            _logger.LogWarning(ex, "Order session error: {ErrorCode}", ex.ErrorCode);
            return StatusCode(ex.StatusCode, BaseResponse<SessionDto>.Failure(ex.UserMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading session {SessionId}", sessionId);
            return StatusCode(500, BaseResponse<SessionDto>.Failure("Internal server error"));
        }
    }

    /// <summary>
    /// Зберігає OrderState у сесії (корзина, ресторан, доставка — з фронта або гейтвея).
    /// </summary>
    [HttpPut("{sessionId}/order-state")]
    [ProducesResponseType(typeof(BaseResponse<SessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<SessionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<SessionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResponse<SessionDto>>> UpdateOrderState(
        [FromRoute] string sessionId,
        [FromBody] OrderStateDto orderState,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseResponse<SessionDto>.Failure("Invalid request data"));
            }

            var result = await _orderSessionService.UpdateOrderStateInSessionAsync(sessionId, orderState, cancellationToken);

            return Ok(BaseResponse<SessionDto>.Success(result));
        }
        catch (OrderException ex)
        {
            _logger.LogWarning(ex, "Order session error: {ErrorCode}", ex.ErrorCode);
            return StatusCode(ex.StatusCode, BaseResponse<SessionDto>.Failure(ex.UserMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order state for session {SessionId}", sessionId);
            return StatusCode(500, BaseResponse<SessionDto>.Failure("Internal server error"));
        }
    }

    [HttpGet("{sessionId}/order-state")]
    [ProducesResponseType(typeof(BaseResponse<List<OrderStateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<BaseResponse<List<OrderStateDto>>>> GetOrderState(
    [FromRoute] string sessionId,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _orderSessionService.GetSessionAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(BaseResponse<List<OrderStateDto>>.Failure("Session not found"));
            }

            var allOrderStates = session.OrderStates.Values.ToList();

            return Ok(BaseResponse<List<OrderStateDto>>.Success(allOrderStates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all order states for session {SessionId}", sessionId);
            return StatusCode(500, BaseResponse<List<OrderStateDto>>.Failure("Internal server error"));
        }
    }
}
