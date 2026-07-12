using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Abstractions.Commands;
using FCG.Payments.Application.Commands.Payments;
using FCG.Payments.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.Tests.Worker.Consumers;

public class OrderPlacedConsumerTests
{
    private readonly Mock<ICommandHandlerVoid<ProcessOrderCommand>> _handlerMock;
    private readonly Mock<ILogger<OrderPlacedConsumer>> _loggerMock;
    private readonly OrderPlacedConsumer _consumer;

    public OrderPlacedConsumerTests()
    {
        _handlerMock = new Mock<ICommandHandlerVoid<ProcessOrderCommand>>();
        _loggerMock = new Mock<ILogger<OrderPlacedConsumer>>();

        _consumer = new OrderPlacedConsumer(
            _handlerMock.Object,
            _loggerMock.Object);
    }

    [Fact(DisplayName = "Validando consumo do evento OrderPlacedEvent")]
    [Trait("Categoria", "Worker - OrderPlacedConsumer")]
    public async Task Consume_Success()
    {
        var message = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        var contextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();

        contextMock
            .Setup(x => x.Message)
            .Returns(message);

        contextMock
            .Setup(x => x.CancellationToken)
            .Returns(CancellationToken.None);

        await _consumer.Consume(contextMock.Object);

        _handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<ProcessOrderCommand>(command =>
                    command.OrderId == message.OrderId &&
                    command.UserId == message.UserId &&
                    command.GameId == message.GameId &&
                    command.UserEmail == message.UserEmail &&
                    command.GameTitle == message.GameTitle &&
                    command.Price == message.Price &&
                    command.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}