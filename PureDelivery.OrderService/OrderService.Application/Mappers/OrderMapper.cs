using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Application.Mappers;

public class OrderMapper : IOrderMapper
{
    public OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.RestaurantName,
            Items = order.Items.Select(MapItemToDto).ToList(),
            DeliveryAddressId = order.DeliveryAddressId,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            DeliveryInstructions = order.DeliveryInstructions,
            PaymentId = order.PaymentId,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            CourierId = order.CourierId,
            CourierName = order.CourierName,
            Status = order.Status,
            DeliveryStatus = order.DeliveryStatus,
            SubTotal = order.SubTotal,
            DeliveryFee = order.DeliveryFee,
            Tax = order.Tax,
            Discount = order.Discount,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ConfirmedAt = order.ConfirmedAt,
            PreparationStartedAt = order.PreparationStartedAt,
            ReadyForPickupAt = order.ReadyForPickupAt,
            PickedUpAt = order.PickedUpAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt,
            EstimatedPreparationMinutes = order.EstimatedPreparationMinutes,
            EstimatedDeliveryMinutes = order.EstimatedDeliveryMinutes,
            SessionId = order.SessionId,
            SpecialInstructions = order.SpecialInstructions,
            CancellationReason = order.CancellationReason
        };
    }

    public Order MapFromCreateRequest(CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            RestaurantId = request.RestaurantId,
            DeliveryAddressId = request.DeliveryAddressId,
            Items = request.Items.Select(MapItemFromRequest).ToList(),
            DeliveryInstructions = request.DeliveryInstructions,
            SpecialInstructions = request.SpecialInstructions,
            SessionId = request.SessionId,
            Status = OrderStatus.Cart
        };

        return order;
    }

    private OrderItemDto MapItemToDto(OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            MenuItemId = item.MenuItemId,
            MenuItemName = item.MenuItemName,
            MenuItemImageUrl = item.MenuItemImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            SelectedOptions = item.SelectedOptions.Select(MapOptionToDto).ToList(),
            SpecialInstructions = item.SpecialInstructions
        };
    }

    private OrderItemOptionDto MapOptionToDto(OrderItemOption option)
    {
        return new OrderItemOptionDto
        {
            Id = option.Id,
            OptionId = option.OptionId,
            OptionName = option.OptionName,
            ChoiceId = option.ChoiceId,
            ChoiceName = option.ChoiceName,
            AdditionalPrice = option.AdditionalPrice
        };
    }

    private OrderItem MapItemFromRequest(OrderItemRequest request)
    {
        return new OrderItem
        {
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            SpecialInstructions = request.SpecialInstructions,
            SelectedOptions = request.SelectedOptions?.Select(MapOptionFromRequest).ToList() ?? new List<OrderItemOption>()
        };
    }

    private OrderItemOption MapOptionFromRequest(OrderItemOptionRequest request)
    {
        return new OrderItemOption
        {
            OptionId = request.OptionId,
            ChoiceId = request.ChoiceId
        };
    }
}

