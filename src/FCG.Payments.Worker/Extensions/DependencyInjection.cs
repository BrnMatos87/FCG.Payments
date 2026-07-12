using FCG.Payments.Application.Abstractions.Commands;
using FCG.Payments.Application.Commands.Payments;
using FCG.Payments.Application.Commands.Payments.Handlers;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Infrastructure.Messaging;
using FCG.Payments.Infrastructure.Persistence;
using FCG.Payments.Infrastructure.Repositories;
using FCG.Payments.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkerRabbitMqOptions = FCG.Payments.Worker.Configuration.RabbitMqOptions;

namespace FCG.Payments.Worker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<WorkerRabbitMqOptions>()
            .Bind(configuration.GetSection(WorkerRabbitMqOptions.SectionName))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host não foi configurado.")
            .Validate(options =>
                    options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost não foi configurado.")
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username não foi configurado.")
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password não foi configurado.")
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.OrderPlacedQueue),
                "RabbitMq:OrderPlacedQueue não foi configurado.")
            .ValidateOnStart();

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "A connection string 'DefaultConnection' não foi configurada.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<
            IPaymentEventPublisher,
            MassTransitPaymentEventPublisher>();

        services.AddScoped<
            ICommandHandlerVoid<ProcessOrderCommand>,
            ProcessOrderCommandHandler>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context
                    .GetRequiredService<IOptions<WorkerRabbitMqOptions>>()
                    .Value;

                cfg.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(options.Username);
                        hostConfiguration.Password(options.Password);
                    });

                cfg.ReceiveEndpoint(
                    options.OrderPlacedQueue,
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<OrderPlacedConsumer>(
                            context);
                    });
            });
        });

        return services;
    }
}