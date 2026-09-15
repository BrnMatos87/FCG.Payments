using FCG.Payments.Application.Contracts;
using FCG.Payments.Infrastructure.Messaging;
using FCG.Payments.Infrastructure.Persistence;
using FCG.Payments.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddRabbitMq(services, configuration);
        AddNotifications(services, configuration);

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }

    private static void AddDatabase(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada.");
        }

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }

    private static void AddRabbitMq(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host não foi configurado.")
            .Validate(
                options => options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password não foi configurado.")
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context
                    .GetRequiredService<IOptions<RabbitMqOptions>>()
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
            });
        });
    }

    private static void AddNotifications(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<NotificationsOptions>()
            .Bind(configuration.GetSection(NotificationsOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "Notifications:BaseUrl deve ser uma URL absoluta válida.")
            .ValidateOnStart();

        services.AddHttpClient<MassTransitPaymentEventPublisher>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<NotificationsOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + '/');

            if (!string.IsNullOrWhiteSpace(options.FunctionKey))
                client.DefaultRequestHeaders.Add("x-functions-key", options.FunctionKey);
        });

        services.AddScoped<IPaymentEventPublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<MassTransitPaymentEventPublisher>());
    }
}
