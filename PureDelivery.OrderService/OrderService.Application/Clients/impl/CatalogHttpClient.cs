using OrderService.Application.Configuration;
using OrderService.Application.Exceptions;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;

namespace OrderService.Application.Clients.impl
{
    public class CatalogHttpClient : ICatalogClient
    {
        private readonly IHttpApiClient _apiClient;
        private readonly ExternalServicesConfig _config;

        public CatalogHttpClient(IHttpApiClient apiClient, ICustomConfigurationProvider configProvider)
        {
            _apiClient = apiClient;
            _config = configProvider.GetConfigurationAsync<ExternalServicesConfig>("ExternalServices").Result
                ?? throw new InvalidOperationException("Failed to load ExternalServicesConfig");
        }

        public async Task<List<MenuItemDetailDto>> GetMenuItemsAsync(List<string> ids, CancellationToken ct = default)
        {
            var baseUrl = _config.CatalogService.BaseUrl;
            var endpoint = _config.CatalogService.GetMenuItemsBulkEndpoint;

            var requestParams = HttpRequestParams.WithBody(baseUrl, endpoint, ids)
                        .WithCancellation(ct);

            var response = await _apiClient.PostAsync<BaseResponse<List<MenuItemDetailDto>>>(requestParams);

            if (!response.IsSuccess || response.Data == null || !response.Data.IsSuccess)
            {
                throw new OrderException("CATALOG_SERVICE_ERROR",
                    $"Failed to fetch menu items. Status: {response.StatusCode}", 500);
            }

            return response.Data.Data ?? [];
        }
    }
}
