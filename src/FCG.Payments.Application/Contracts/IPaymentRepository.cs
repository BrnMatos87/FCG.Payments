using FCG.Payments.Domain.Entities;

namespace FCG.Payments.Application.Contracts;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}