using FCG.Payments.Api.Correlation;
using FCG.Payments.Application.Abstractions.Queries;
using FCG.Payments.Application.Contracts;
using FCG.Payments.Application.Queries.Payments;
using FCG.Payments.Application.Queries.Payments.Handlers;
using FCG.Payments.Application.Responses;
using FluentValidation;

namespace FCG.Payments.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        services.AddValidatorsFromAssembly(typeof(GetPaymentByOrderIdQuery).Assembly);

        services.AddScoped<IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse?>, GetPaymentByOrderIdQueryHandler>();

        return services;
    }
}