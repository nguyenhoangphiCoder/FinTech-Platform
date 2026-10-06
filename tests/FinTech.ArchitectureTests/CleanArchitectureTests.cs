using NetArchTest.Rules;
using Xunit;

namespace FinTech.ArchitectureTests;

public class CleanArchitectureTests
{
    [Fact]
    public void Domain_ShouldNot_DependOn_Infrastructure()
    {
        var domainAssembly = typeof(FinTech.Modules.Ledger.Domain.LedgerAccountId).Assembly;

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("FinTech.Modules.Ledger.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_ShouldNot_DependOn_Application()
    {
        var domainAssembly = typeof(FinTech.Modules.Ledger.Domain.LedgerAccountId).Assembly;

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("FinTech.Modules.Ledger.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_ShouldNot_DependOn_Infrastructure()
    {
        var appAssembly = typeof(FinTech.BuildingBlocks.Application.IUnitOfWork).Assembly;

        var result = Types.InAssembly(appAssembly)
            .ShouldNot()
            .HaveDependencyOn("FinTech.BuildingBlocks.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
