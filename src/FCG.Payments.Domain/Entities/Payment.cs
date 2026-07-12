using FCG.BuildingBlocks.Domain;
using FCG.BuildingBlocks.Enums;
using FCG.Payments.Domain.Exceptions;

namespace FCG.Payments.Domain.Entities;

public class Payment : EntityBase
{
    public Guid OrderId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid GameId { get; private set; }

    public string UserEmail { get; private set; } = string.Empty;

    public string GameTitle { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    protected Payment()
    {
    }

    public static Payment Create(
        Guid orderId,
        Guid userId,
        Guid gameId,
        string userEmail,
        string gameTitle,
        decimal price)
    {
        var normalizedUserEmail = userEmail?.Trim().ToLower() ?? string.Empty;
        var normalizedGameTitle = gameTitle?.Trim() ?? string.Empty;

        ValidateOrderId(orderId);
        ValidateUserId(userId);
        ValidateGameId(gameId);
        ValidateUserEmail(normalizedUserEmail);
        ValidateGameTitle(normalizedGameTitle);
        ValidatePrice(price);

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            GameId = gameId,
            UserEmail = normalizedUserEmail,
            GameTitle = normalizedGameTitle,
            Price = price,
            PaymentStatus = PaymentStatus.Pending,
            Status = StatusType.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        EnsurePending();

        PaymentStatus = PaymentStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        EnsurePending();

        PaymentStatus = PaymentStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPending()
    {
        return PaymentStatus == PaymentStatus.Pending;
    }

    public bool IsApproved()
    {
        return PaymentStatus == PaymentStatus.Approved;
    }

    public bool IsRejected()
    {
        return PaymentStatus == PaymentStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (PaymentStatus != PaymentStatus.Pending)
            throw new DomainException("O pagamento não está pendente.");
    }

    private static void ValidateOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("O identificador do pedido é obrigatório.");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("O identificador do usuário é obrigatório.");
    }

    private static void ValidateGameId(Guid gameId)
    {
        if (gameId == Guid.Empty)
            throw new DomainException("O identificador do jogo é obrigatório.");
    }

    private static void ValidateUserEmail(string userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
            throw new DomainException("O e-mail do usuário é obrigatório.");
    }

    private static void ValidateGameTitle(string gameTitle)
    {
        if (string.IsNullOrWhiteSpace(gameTitle))
            throw new DomainException("O título do jogo é obrigatório.");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("O valor do pagamento deve ser maior que zero.");
    }
}