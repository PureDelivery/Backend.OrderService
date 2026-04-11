using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Clients;
using OrderService.Application.Clients.impl;
using OrderService.Application.Configuration;
using OrderService.Application.Consumers;
using PureDelivery.Common.Configuration.Services;
using OrderService.Application.EventHandlers;
using OrderService.Application.Exceptions;
using OrderService.Application.Integration;
using OrderService.Application.Mappers;
using OrderService.Application.Repositories;
using OrderService.Application.Services;
using OrderService.Application.Services.impl;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using PureDelivery.Common.Configuration.Extensions;
using PureDelivery.Common.Http.Extensions;
using PureDelivery.Infrastructure.Redis.Extensions;
using Serilog;
using OrderAppService = OrderService.Application.Services.impl.OrderService;
using OrderSessionService = OrderService.Application.Services.impl.OrderSessionService;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Service", "OrderService");
});

// Add services to the container
builder.Services.AddRedisServices("Redis");
builder.Services.AddConfigurationProvider(builder.Configuration);
builder.Services.AddApiClient("HttpClient");

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register mappers
builder.Services.AddScoped<IOrderMapper, OrderMapper>();

// Register application services
builder.Services.AddScoped<IOrderService, OrderAppService>();
builder.Services.AddScoped<IOrderSessionService, OrderSessionService>();
builder.Services.AddScoped<PaymentCompletedEventHandler>();
builder.Services.AddScoped<IOrderIntegrationEventPublisher, LoggingOrderIntegrationEventPublisher>();

builder.Services.AddScoped<IRestaurantClient, RestaurantHttpClient>();
builder.Services.AddScoped<ICatalogClient, CatalogHttpClient>();

builder.Services.AddConfigurationProvider(builder.Configuration);

builder.Services.AddSingleton<RabbitMqConfiguration>(sp =>
{
    var provider = sp.GetRequiredService<ICustomConfigurationProvider>();
    var cfg = provider.GetConfigurationAsync<RabbitMqConfiguration>("RabbitMQ").Result;
    cfg.Validate();
    return cfg;
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPaidConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitCfg = context.GetRequiredService<RabbitMqConfiguration>();
        cfg.Host(rabbitCfg.Host, rabbitCfg.VirtualHost, h =>
        {
            h.Username(rabbitCfg.Username);
            h.Password(rabbitCfg.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PureDelivery Order Service API",
        Version = "v1",
        Description = "API для управління замовленнями",
        Contact = new()
        {
            Name = "PureDelivery Team",
            Email = "support@puredelivery.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

