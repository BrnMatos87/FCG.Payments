using FCG.BuildingBlocks.Enums;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Application.Queries.Payments;
using FCG.Payments.Application.Queries.Payments.Handlers;
using FCG.Payments.Domain.Entities;
using Moq;

namespace FCG.Payments.Tests.Application.Queries;

public class GetPaymentByOrderIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly GetPaymentByOrderIdQueryHandler _handler;

    public GetPaymentByOrderIdQueryHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentByOrderIdQueryHandler(_paymentRepositoryMock.Object);
    }

    [Fact(DisplayName = "Validando busca de pagamento por pedido com sucesso")]
    [Trait("Categoria", "Application - GetPaymentByOrderId")]
    public async Task GetPaymentByOrderId_HandleAsync_Success()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "usuario@email.com",
            "Sonic",
            199);

        payment.Approve();

        var query = new GetPaymentByOrderIdQuery
        {
            OrderId = payment.OrderId
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(query.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var response = await _handler.HandleAsync(query);

        Assert.NotNull(response);
        Assert.Equal(payment.Id, response!.Id);
        Assert.Equal(payment.OrderId, response.OrderId);
        Assert.Equal(payment.UserId, response.UserId);
        Assert.Equal(payment.GameId, response.GameId);
        Assert.Equal(payment.UserEmail, response.UserEmail);
        Assert.Equal(payment.GameTitle, response.GameTitle);
        Assert.Equal(payment.Price, response.Price);
        Assert.Equal(PaymentStatus.Approved, response.PaymentStatus);
        Assert.Equal(payment.ProcessedAt, response.ProcessedAt);
        Assert.Equal(payment.CreatedAt, response.CreatedAt);
    }

    [Fact(DisplayName = "Validando busca de pagamento inexistente")]
    [Trait("Categoria", "Application - GetPaymentByOrderId")]
    public async Task GetPaymentByOrderId_HandleAsync_NotFound()
    {
        var query = new GetPaymentByOrderIdQuery
        {
            OrderId = Guid.NewGuid()
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(query.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var response = await _handler.HandleAsync(query);

        Assert.Null(response);
    }
}