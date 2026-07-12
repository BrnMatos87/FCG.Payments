using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Commands.Payments;
using FCG.Payments.Application.Commands.Payments.Handlers;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Domain.Entities;
using Moq;

namespace FCG.Payments.Tests.Application.Commands;

public class ProcessOrderCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentEventPublisher> _paymentEventPublisherMock;
    private readonly ProcessOrderCommandHandler _handler;

    public ProcessOrderCommandHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentEventPublisherMock = new Mock<IPaymentEventPublisher>();

        _handler = new ProcessOrderCommandHandler(
            _paymentRepositoryMock.Object,
            _paymentEventPublisherMock.Object);
    }

    [Fact(DisplayName = "Validando processamento de pedido com sucesso")]
    [Trait("Categoria", "Application - ProcessOrder")]
    public async Task ProcessOrder_HandleAsync_Success()
    {
        var command = new ProcessOrderCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            CorrelationId = Guid.NewGuid()
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        await _handler.HandleAsync(command);

        _paymentRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Payment>(payment =>
                    payment.OrderId == command.OrderId &&
                    payment.UserId == command.UserId &&
                    payment.GameId == command.GameId &&
                    payment.UserEmail == command.UserEmail &&
                    payment.GameTitle == command.GameTitle &&
                    payment.Price == command.Price &&
                    payment.IsApproved()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _paymentEventPublisherMock.Verify(
            x => x.PublishPaymentProcessedAsync(
                It.Is<PaymentProcessedEvent>(message =>
                    message.OrderId == command.OrderId &&
                    message.UserId == command.UserId &&
                    message.GameId == command.GameId &&
                    message.UserEmail == command.UserEmail &&
                    message.GameTitle == command.GameTitle &&
                    message.Price == command.Price &&
                    message.CorrelationId == command.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando processamento de pedido já existente")]
    [Trait("Categoria", "Application - ProcessOrder")]
    public async Task ProcessOrder_HandleAsync_AlreadyExists()
    {
        var existingPayment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "usuario@email.com",
            "Sonic",
            199);

        var command = new ProcessOrderCommand
        {
            OrderId = existingPayment.OrderId,
            UserId = existingPayment.UserId,
            GameId = existingPayment.GameId,
            UserEmail = existingPayment.UserEmail,
            GameTitle = existingPayment.GameTitle,
            Price = existingPayment.Price,
            CorrelationId = Guid.NewGuid()
        };

        _paymentRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(command));

        Assert.Equal("Já existe um pagamento para este pedido.", result.Message);

        _paymentRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _paymentEventPublisherMock.Verify(
            x => x.PublishPaymentProcessedAsync(
                It.IsAny<PaymentProcessedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}