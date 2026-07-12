using FCG.Payments.Application.Abstractions.Queries;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Application.Responses;

namespace FCG.Payments.Application.Queries.Payments.Handlers;

public class GetPaymentByOrderIdQueryHandler : IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponse?> HandleAsync(GetPaymentByOrderIdQuery query, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(query.OrderId, ct);

        if (payment is null)
            return null;

        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            GameId = payment.GameId,
            UserEmail = payment.UserEmail,
            GameTitle = payment.GameTitle,
            Price = payment.Price,
            PaymentStatus = payment.PaymentStatus,
            ProcessedAt = payment.ProcessedAt,
            CreatedAt = payment.CreatedAt
        };
    }
}