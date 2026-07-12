using FCG.BuildingBlocks.Events;
using FCG.Payments.Application.Abstractions.Commands;
using FCG.Payments.Application.Commands.Payments;
using MassTransit;

namespace FCG.Payments.Worker.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ICommandHandlerVoid<ProcessOrderCommand> _handler;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        ICommandHandlerVoid<ProcessOrderCommand> handler,
        ILogger<OrderPlacedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Evento OrderPlacedEvent recebido. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, CorrelationId: {CorrelationId}",
            message.OrderId,
            message.UserId,
            message.GameId,
            message.CorrelationId);

        await _handler.HandleAsync(new ProcessOrderCommand
        {
            OrderId = message.OrderId,
            UserId = message.UserId,
            GameId = message.GameId,
            UserEmail = message.UserEmail,
            GameTitle = message.GameTitle,
            Price = message.Price,
            CorrelationId = message.CorrelationId
        }, context.CancellationToken);

        _logger.LogInformation(
            "Evento OrderPlacedEvent processado com sucesso. OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            message.OrderId,
            message.CorrelationId);
    }
}