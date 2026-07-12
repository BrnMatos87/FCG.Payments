using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Abstractions.Commands;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Domain.Entities;

namespace FCG.Payments.Application.Commands.Payments.Handlers;

public class ProcessOrderCommandHandler : ICommandHandlerVoid<ProcessOrderCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentEventPublisher _paymentEventPublisher;

    public ProcessOrderCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentEventPublisher paymentEventPublisher)
    {
        _paymentRepository = paymentRepository;
        _paymentEventPublisher = paymentEventPublisher;
    }

    public async Task HandleAsync(ProcessOrderCommand command, CancellationToken ct = default)
    {
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(command.OrderId, ct);

        if (existingPayment is not null)
            throw new InvalidOperationException("Já existe um pagamento para este pedido.");

        var payment = Payment.Create(
            command.OrderId,
            command.UserId,
            command.GameId,
            command.UserEmail,
            command.GameTitle,
            command.Price);

        var status = SimulatePaymentStatus(command.Price);

        if (status == PaymentStatus.Approved)
            payment.Approve();
        else
            payment.Reject();

        await _paymentRepository.CreateAsync(payment, ct);

        var paymentProcessedEvent = new PaymentProcessedEvent
        {
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            GameId = payment.GameId,
            UserEmail = payment.UserEmail,
            GameTitle = payment.GameTitle,
            Price = payment.Price,
            Status = payment.PaymentStatus,
            ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow,
            CorrelationId = command.CorrelationId
        };

        await _paymentEventPublisher.PublishPaymentProcessedAsync(paymentProcessedEvent, ct);
    }

    private static PaymentStatus SimulatePaymentStatus(decimal price)
    {
        return price > 100
            ? PaymentStatus.Approved
            : PaymentStatus.Rejected;
    }
}