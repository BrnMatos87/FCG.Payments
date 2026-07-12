using FCG.Payments.Api.Controllers;
using FCG.Payments.Application.Abstractions.Queries;
using FCG.Payments.Application.Queries.Payments;
using FCG.Payments.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG.Payments.Tests.Api.Controllers;

public class PaymentsControllerTests
{
    [Fact(DisplayName = "Validando busca de pagamento por pedido")]
    [Trait("Categoria", "API - PaymentsController")]
    public async Task GetByOrderId_Success()
    {
        var orderId = Guid.NewGuid();

        var response = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199
        };

        var handlerMock = new Mock<IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse?>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetPaymentByOrderIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var controller = new PaymentsController();

        var result = await controller.GetByOrderId(
            orderId,
            handlerMock.Object,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);

        handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<GetPaymentByOrderIdQuery>(query => query.OrderId == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando busca de pagamento inexistente")]
    [Trait("Categoria", "API - PaymentsController")]
    public async Task GetByOrderId_NotFound()
    {
        var orderId = Guid.NewGuid();

        var handlerMock = new Mock<IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse?>>();

        handlerMock
            .Setup(x => x.HandleAsync(
                It.IsAny<GetPaymentByOrderIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentResponse?)null);

        var controller = new PaymentsController();

        var result = await controller.GetByOrderId(
            orderId,
            handlerMock.Object,
            CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(404, notFoundResult.StatusCode);
    }
}