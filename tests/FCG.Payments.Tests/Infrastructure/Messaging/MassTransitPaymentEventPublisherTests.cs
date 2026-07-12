using FCG.BuildingBlocks.Events;
using FCG.Payments.Infrastructure.Messaging;
using MassTransit;
using Moq;

namespace FCG.Payments.Tests.Infrastructure.Messaging;

public class MassTransitPaymentEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly MassTransitPaymentEventPublisher _publisher;

    public MassTransitPaymentEventPublisherTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _publisher = new MassTransitPaymentEventPublisher(_publishEndpointMock.Object);
    }

    [Fact(DisplayName = "Validando publicação do evento PaymentProcessedEvent")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishPaymentProcessedAsync_Success()
    {
        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            Status = FCG.BuildingBlocks.Enums.PaymentStatus.Approved,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        await _publisher.PublishPaymentProcessedAsync(message);

        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PaymentProcessedEvent>(e =>
                    e.OrderId == message.OrderId &&
                    e.UserId == message.UserId &&
                    e.GameId == message.GameId &&
                    e.UserEmail == message.UserEmail &&
                    e.GameTitle == message.GameTitle &&
                    e.Price == message.Price &&
                    e.Status == message.Status &&
                    e.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}