namespace FCG.Payments.Infrastructure.Messaging;

public sealed class NotificationsOptions
{
    public const string SectionName = "Notifications";

    public string BaseUrl { get; init; } = string.Empty;

    public string FunctionKey { get; init; } = string.Empty;
}
