using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Contracts;
using System.Net.Http.Json;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Payments.Infrastructure.Messaging;

public class MassTransitPaymentEventPublisher : IPaymentEventPublisher
{
    private const string PaymentProcessedRoute = "api/notifications/payment-processed";
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MassTransitPaymentEventPublisher> _logger;

    public MassTransitPaymentEventPublisher(
        IPublishEndpoint publishEndpoint,
        HttpClient httpClient,
        ILogger<MassTransitPaymentEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task PublishPaymentProcessedAsync(PaymentProcessedEvent message, CancellationToken ct = default)
    {
        await _publishEndpoint.Publish(message, ct);

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                PaymentProcessedRoute,
                message,
                ct);

            if (response.IsSuccessStatusCode)
                return;

            _logger.LogWarning(
                "Notifications indisponível para o pedido {OrderId}. StatusCode: {StatusCode}, CorrelationId: {CorrelationId}",
                message.OrderId,
                (int)response.StatusCode,
                message.CorrelationId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Não foi possível acessar Notifications para o pedido {OrderId}. CorrelationId: {CorrelationId}",
                message.OrderId,
                message.CorrelationId);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Timeout ao acessar Notifications para o pedido {OrderId}. CorrelationId: {CorrelationId}",
                message.OrderId,
                message.CorrelationId);
        }
    }
}
