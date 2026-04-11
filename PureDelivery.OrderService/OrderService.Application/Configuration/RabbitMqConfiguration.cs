using PureDelivery.Common.Configuration.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Configuration;

public class RabbitMqConfiguration : IConfiguration<RabbitMqConfiguration>
{
    public const string SectionName = "RabbitMQ";

    [Required]
    public string Host { get; set; } = "localhost";

    public string VirtualHost { get; set; } = "/";

    [Required]
    public string Username { get; set; } = "guest";

    [Required]
    public string Password { get; set; } = "guest";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new ArgumentException("RabbitMQ Host cannot be empty", nameof(Host));

        if (string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("RabbitMQ Username cannot be empty", nameof(Username));

        if (string.IsNullOrWhiteSpace(Password))
            throw new ArgumentException("RabbitMQ Password cannot be empty", nameof(Password));
    }
}
