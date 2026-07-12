using FCG.Payments.Application.Abstractions.Queries;
using FCG.Payments.Application.Queries.Payments;
using FCG.Payments.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Administrator")]
public class PaymentsController : ControllerBase
{
    [HttpGet("orders/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(
        [FromRoute] Guid orderId,
        [FromServices] IQueryHandler<GetPaymentByOrderIdQuery, PaymentResponse?> handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetPaymentByOrderIdQuery
        {
            OrderId = orderId
        }, ct);

        if (response is null)
            return NotFound(new { message = "Pagamento não encontrado." });

        return Ok(response);
    }
}