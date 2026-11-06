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

    [Fact(DisplayName = "Mantendo RabbitMQ sem falhar quando Notifications retorna erro")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishPaymentProcessedAsync_NotificationFailure_ShouldNotThrow()
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

        await publisher.PublishPaymentProcessedAsync(message);

        _publishEndpointMock.Verify(
            endpoint => endpoint.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Mantendo RabbitMQ sem falhar quando Notifications está indisponível")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishPaymentProcessedAsync_NotificationsUnavailable_ShouldNotThrow()
    {
        var publisher = CreatePublisherThatThrows(
            new HttpRequestException("Notifications indisponível"));
        var message = CreateMessage();

        await publisher.PublishPaymentProcessedAsync(message);

        _publishEndpointMock.Verify(
            endpoint => endpoint.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Mantendo RabbitMQ sem falhar em timeout do Notifications")]
    [Trait("Categoria", "Infrastructure - Messaging")]
    public async Task PublishPaymentProcessedAsync_NotificationsTimeout_ShouldNotThrow()
    {
        var publisher = CreatePublisherThatThrows(
            new OperationCanceledException("Timeout do Notifications"));
        var message = CreateMessage();

        await publisher.PublishPaymentProcessedAsync(message);

        _publishEndpointMock.Verify(
            endpoint => endpoint.Publish(message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private MassTransitPaymentEventPublisher CreatePublisherThatThrows(Exception exception)
    {
        var httpClient = new HttpClient(new ThrowingHttpMessageHandler(exception))
        {
            BaseAddress = new Uri("https://notifications.test/")
        };

        return new MassTransitPaymentEventPublisher(
            _publishEndpointMock.Object,
            httpClient,
            Mock.Of<ILogger<MassTransitPaymentEventPublisher>>());
    }

    private static PaymentProcessedEvent CreateMessage()
    {
        return new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid()
        };
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

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(_exception);
        }
    }
}
