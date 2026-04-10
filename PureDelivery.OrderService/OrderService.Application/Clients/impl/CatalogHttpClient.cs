using Microsoft.Extensions.Options;
using OrderService.Application.Configuration;
using OrderService.Application.Exceptions;
using PureDelivery.Common.Http.Models;
using PureDelivery.Common.Http.Services;
using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Clients.impl
{
    public class CatalogHttpClient : ICatalogClient
    {
        private readonly IHttpApiClient _apiClient;
        private readonly ExternalServicesConfig _config;

        public CatalogHttpClient(IHttpApiClient apiClient, IOptions<ExternalServicesConfig> config)
        {
            _apiClient = apiClient;
            _config = config.Value;
        }

        public async Task<List<MenuItemDetailDto>> GetMenuItemsAsync(List<string> ids, CancellationToken ct = default)
        {
            var baseUrl = _config.CatalogService.BaseUrl;
            var endpoint = _config.CatalogService.GetBulkItemsEndpoint;

            var requestParams = HttpRequestParams.WithBody(baseUrl, endpoint, ids)
                        .WithCancellation(ct);

            var response = await _apiClient.PostAsync<List<MenuItemDetailDto>>(requestParams);

            if (!response.IsSuccess)
            {
                throw new OrderException("CATALOG_SERVICE_ERROR",
                    $"Failed to fetch menu items. Status: {response.StatusCode}", 500);
            }

            return response.Data;
        }
    }
}
