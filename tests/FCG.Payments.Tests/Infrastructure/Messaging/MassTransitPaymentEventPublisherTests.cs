using FCG.BuildingBlocks.Events;
using FCG.Payments.Infrastructure.Messaging;
using System.Net;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Payments.Tests.Infrastructure.Messaging;

public class MassTransitPaymentEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly RecordingHttpMessageHandler _httpHandler;
    private readonly MassTransitPaymentEventPublisher _publisher;

    public MassTransitPaymentEventPublisherTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _httpHandler = new RecordingHttpMessageHandler(HttpStatusCode.NoContent);
        var httpClient = new HttpClient(_httpHandler)
        {
            BaseAddress = new Uri("https://notifications.test/")
        };
        var logger = new Mock<ILogger<MassTransitPaymentEventPublisher>>();

        _publisher = new MassTransitPaymentEventPublisher(
            _publishEndpointMock.Object,
            httpClient,
            logger.Object);
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

        Assert.Equal(
            new Uri("https://notifications.test/api/notifications/payment-processed"),
            _httpHandler.Request?.RequestUri);
        Assert.Equal(HttpMethod.Post, _httpHandler.Request?.Method);

        var request = JsonSerializer.Deserialize<PaymentProcessedEvent>(
            _httpHandler.Body!,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(message.OrderId, request?.OrderId);
        Assert.Equal(message.CorrelationId, request?.CorrelationId);
    }

    [Fact(DisplayName = "Mantendo RabbitMQ e propagando falha HTTP do Notifications")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishPaymentProcessedAsync_NotificationFailure_ShouldThrow()
    {
        var httpClient = new HttpClient(
            new RecordingHttpMessageHandler(HttpStatusCode.InternalServerError))
        {
            BaseAddress = new Uri("https://notifications.test/")
        };
        var publisher = new MassTransitPaymentEventPublisher(
            _publishEndpointMock.Object,
            httpClient,
            Mock.Of<ILogger<MassTransitPaymentEventPublisher>>());

        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            publisher.PublishPaymentProcessedAsync(message));

        _publishEndpointMock.Verify(
            endpoint => endpoint.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public RecordingHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public HttpRequestMessage? Request { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_statusCode);
        }
    }
}
