using FCG.Payments.Api.Middlewares;
using FCG.Payments.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FCG.Payments.Tests.Api.Middlewares;

public class CorrelationMiddlewareTests
{
    [Fact(DisplayName = "Validando criação de correlation id quando não enviado no header")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task CorrelationMiddleware_CreateCorrelationId_WhenHeaderNotExists()
    {
        Guid capturedCorrelationId = Guid.Empty;

        var correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        correlationIdAccessorMock
            .Setup(x => x.Set(It.IsAny<Guid>()))
            .Callback<Guid>(id => capturedCorrelationId = id);

        var context = new DefaultHttpContext();

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context, correlationIdAccessorMock.Object);

        Assert.NotEqual(Guid.Empty, capturedCorrelationId);
        Assert.Equal(capturedCorrelationId.ToString(), context.Request.Headers["x-correlation-id"]);
        Assert.Equal(capturedCorrelationId.ToString(), context.Response.Headers["x-correlation-id"]);
    }

    [Fact(DisplayName = "Validando uso de correlation id enviado no header")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task CorrelationMiddleware_UseCorrelationId_FromHeader()
    {
        var correlationId = Guid.NewGuid();

        var correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        var context = new DefaultHttpContext();

        context.Request.Headers["x-correlation-id"] = correlationId.ToString();

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context, correlationIdAccessorMock.Object);

        Assert.Equal(correlationId.ToString(), context.Request.Headers["x-correlation-id"]);
        Assert.Equal(correlationId.ToString(), context.Response.Headers["x-correlation-id"]);

        correlationIdAccessorMock.Verify(
            x => x.Set(correlationId),
            Times.Once);
    }
}