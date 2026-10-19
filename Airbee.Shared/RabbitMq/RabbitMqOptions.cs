using System.ComponentModel.DataAnnotations;

namespace Airbee.Shared.RabbitMq;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string Host { get; set; } = string.Empty;

    // 5672 is the standard AMQP port
    [Range(1, 65535)]
    public int Port { get; set; } = 5672;

    // UserName and Password come from user secrets and must match RABBITMQ_USER / RABBITMQ_PASSWORD in .env
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
