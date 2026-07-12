using FCG.BuildingBlocks.Enums;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Payment>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(x => x.Status == StatusType.Active)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task CreateAsync(Payment entity, CancellationToken ct = default)
    {
        await _context.Payments.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CreateManyAsync(IEnumerable<Payment> entities, CancellationToken ct = default)
    {
        await _context.Payments.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment entity, CancellationToken ct = default)
    {
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(id, ct);

        if (payment is null)
            return;

        payment.Inactivate();

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct);
    }
}