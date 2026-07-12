namespace FCG.Payments.Worker.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = string.Empty;

    public ushort Port { get; init; }

    public string VirtualHost { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string OrderPlacedQueue { get; init; } = string.Empty;
}