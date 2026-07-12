using FCG.BuildingBlocks.Enums;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Exceptions;

namespace FCG.Payments.Tests.Domain.Entities;

public class PaymentTests
{
    [Fact(DisplayName = "Validando criação de pagamento com pedido vazio")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_OrderId_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "usuario@email.com",
                "Sonic",
                199));

        Assert.Equal("O identificador do pedido é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com usuário vazio")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_UserId_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                "usuario@email.com",
                "Sonic",
                199));

        Assert.Equal("O identificador do usuário é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com jogo vazio")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_GameId_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                "usuario@email.com",
                "Sonic",
                199));

        Assert.Equal("O identificador do jogo é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com e-mail vazio")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_UserEmail_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                string.Empty,
                "Sonic",
                199));

        Assert.Equal("O e-mail do usuário é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com título do jogo vazio")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_GameTitle_Empty()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "usuario@email.com",
                string.Empty,
                199));

        Assert.Equal("O título do jogo é obrigatório.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com valor zero")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_Price_Zero()
    {
        var result = Assert.Throws<DomainException>(() =>
            Payment.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "usuario@email.com",
                "Sonic",
                0));

        Assert.Equal("O valor do pagamento deve ser maior que zero.", result.Message);
    }

    [Fact(DisplayName = "Validando criação de pagamento com sucesso")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Create_Success()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var payment = Payment.Create(
            orderId,
            userId,
            gameId,
            " USUARIO@EMAIL.COM ",
            " Sonic ",
            199);

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(userId, payment.UserId);
        Assert.Equal(gameId, payment.GameId);
        Assert.Equal("usuario@email.com", payment.UserEmail);
        Assert.Equal("Sonic", payment.GameTitle);
        Assert.Equal(199, payment.Price);
        Assert.Equal(PaymentStatus.Pending, payment.PaymentStatus);
        Assert.Equal(StatusType.Active, payment.Status);
        Assert.True(payment.IsPending());
        Assert.False(payment.IsApproved());
        Assert.False(payment.IsRejected());
        Assert.Null(payment.ProcessedAt);
        Assert.NotEqual(default, payment.CreatedAt);
        Assert.Null(payment.UpdatedAt);
    }

    [Fact(DisplayName = "Validando aprovação de pagamento")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Approve_Success()
    {
        var payment = CreateValidPayment();

        payment.Approve();

        Assert.Equal(PaymentStatus.Approved, payment.PaymentStatus);
        Assert.True(payment.IsApproved());
        Assert.False(payment.IsPending());
        Assert.False(payment.IsRejected());
        Assert.NotNull(payment.ProcessedAt);
        Assert.NotNull(payment.UpdatedAt);
    }

    [Fact(DisplayName = "Validando rejeição de pagamento")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Reject_Success()
    {
        var payment = CreateValidPayment();

        payment.Reject();

        Assert.Equal(PaymentStatus.Rejected, payment.PaymentStatus);
        Assert.True(payment.IsRejected());
        Assert.False(payment.IsPending());
        Assert.False(payment.IsApproved());
        Assert.NotNull(payment.ProcessedAt);
        Assert.NotNull(payment.UpdatedAt);
    }

    [Fact(DisplayName = "Validando aprovação de pagamento já processado")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Approve_AlreadyProcessed()
    {
        var payment = CreateValidPayment();

        payment.Approve();

        var result = Assert.Throws<DomainException>(() =>
            payment.Approve());

        Assert.Equal("O pagamento não está pendente.", result.Message);
    }

    [Fact(DisplayName = "Validando rejeição de pagamento já processado")]
    [Trait("Categoria", "Domain - Payment")]
    public void Payment_Reject_AlreadyProcessed()
    {
        var payment = CreateValidPayment();

        payment.Reject();

        var result = Assert.Throws<DomainException>(() =>
            payment.Reject());

        Assert.Equal("O pagamento não está pendente.", result.Message);
    }

    private static Payment CreateValidPayment()
    {
        return Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "usuario@email.com",
            "Sonic",
            199);
    }
}