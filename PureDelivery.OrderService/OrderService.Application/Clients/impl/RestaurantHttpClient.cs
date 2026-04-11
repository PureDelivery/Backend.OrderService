using Microsoft.Extensions.Options;
using OrderService.Application.Clients;
using OrderService.Application.Configuration; // Твой класс ExternalServicesConfig
using OrderService.Application.Exceptions;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;

namespace OrderService.Infrastructure.Clients;

public class RestaurantHttpClient : IRestaurantClient
{
    private readonly IHttpApiClient _apiClient;
    private readonly ExternalServicesConfig _config;

    public RestaurantHttpClient(IHttpApiClient apiClient, ICustomConfigurationProvider configProvider)
    {
        _apiClient = apiClient;
        _config = configProvider.GetConfigurationAsync<ExternalServicesConfig>("ExternalServices").Result;
    }

    public async Task<RestaurantDetailDto> GetRestaurantAsync(string restaurantId, CancellationToken ct = default)
    {
        var endpoint = _config.RestaurantService.GetRestaurantEndpoint
            .Replace("{id}", restaurantId);

        var requestParams = HttpRequestParams.Create(_config.RestaurantService.BaseUrl, endpoint)
            .WithCancellation(ct);

        var response = await _apiClient.GetAsync<RestaurantDetailDto>(requestParams);

        if (!response.IsSuccess)
        {
            throw new OrderException("RESTAURANT_SERVICE_ERROR",
                $"Failed to get restaurant {restaurantId}. Status: {response.StatusCode}", 500);
        }

        return response.Data;
    }
}