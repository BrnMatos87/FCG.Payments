using FCG.Payments.Application.Commands.Payments;
using FluentValidation;

namespace FCG.Payments.Application.Validators;

public class ProcessOrderCommandValidator : AbstractValidator<ProcessOrderCommand>
{
    public ProcessOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("O identificador do pedido é obrigatório.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("O identificador do usuário é obrigatório.");

        RuleFor(x => x.GameId)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("O e-mail do usuário é obrigatório.")
            .EmailAddress()
            .WithMessage("O e-mail do usuário está em formato inválido.");

        RuleFor(x => x.GameTitle)
            .NotEmpty()
            .WithMessage("O título do jogo é obrigatório.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("O valor do pagamento deve ser maior que zero.");

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("O identificador de correlação é obrigatório.");
    }
}