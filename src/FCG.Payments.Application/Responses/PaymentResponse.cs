using FCG.BuildingBlocks.Enums;

namespace FCG.Payments.Application.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string GameTitle { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}