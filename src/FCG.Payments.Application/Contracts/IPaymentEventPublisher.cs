using FCG.BuildingBlocks.Events;

namespace FCG.Payments.Application.Contracts;

public interface IPaymentEventPublisher
{
    Task PublishPaymentProcessedAsync(PaymentProcessedEvent message, CancellationToken ct = default);
}