using FCG.Payments.Application.Commands.Payments;
using FCG.Payments.Application.Validators;

namespace FCG.Payments.Tests.Application.Validators;

public class ProcessOrderCommandValidatorTests
{
    private readonly ProcessOrderCommandValidator _validator;

    public ProcessOrderCommandValidatorTests()
    {
        _validator = new ProcessOrderCommandValidator();
    }

    [Fact(DisplayName = "Validando comando de processamento de pedido válido")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validando identificador do pedido obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_OrderId_Required()
    {
        var command = CreateValidCommand();
        command.OrderId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do pedido é obrigatório.");
    }

    [Fact(DisplayName = "Validando identificador do usuário obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_UserId_Required()
    {
        var command = CreateValidCommand();
        command.UserId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do usuário é obrigatório.");
    }

    [Fact(DisplayName = "Validando identificador do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_GameId_Required()
    {
        var command = CreateValidCommand();
        command.GameId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando e-mail inválido")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_Email_Invalid()
    {
        var command = CreateValidCommand();
        command.UserEmail = "email-invalido";

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O e-mail do usuário está em formato inválido.");
    }

    [Fact(DisplayName = "Validando título do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_GameTitle_Required()
    {
        var command = CreateValidCommand();
        command.GameTitle = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O título do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando valor do pagamento maior que zero")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_Price_Invalid()
    {
        var command = CreateValidCommand();
        command.Price = 0;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O valor do pagamento deve ser maior que zero.");
    }

    [Fact(DisplayName = "Validando identificador de correlação obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessOrderCommandValidator_CorrelationId_Required()
    {
        var command = CreateValidCommand();
        command.CorrelationId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador de correlação é obrigatório.");
    }

    private static ProcessOrderCommand CreateValidCommand()
    {
        return new ProcessOrderCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            CorrelationId = Guid.NewGuid()
        };
    }
}