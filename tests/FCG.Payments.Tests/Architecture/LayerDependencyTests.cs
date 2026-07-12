using FCG.Payments.Api.Controllers;
using FCG.Payments.Application.Commands.Payments;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Infrastructure.Persistence;
using FCG.Payments.Worker.Consumers;
using NetArchTest.Rules;

namespace FCG.Payments.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact(DisplayName = "Domain não deve depender de Application")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types
            .InAssembly(typeof(Payment).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Payment).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(ProcessOrderCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(ProcessOrderCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Infrastructure não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(PaymentsDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Api não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Api_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(PaymentsController).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Worker não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Worker_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(OrderPlacedConsumer).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Payments.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}