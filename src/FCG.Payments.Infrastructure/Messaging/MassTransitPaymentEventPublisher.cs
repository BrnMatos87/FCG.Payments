using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Contracts;
using MassTransit;

namespace FCG.Payments.Infrastructure.Messaging;

public class MassTransitPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitPaymentEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishPaymentProcessedAsync(PaymentProcessedEvent message, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(message, ct);
    }
}