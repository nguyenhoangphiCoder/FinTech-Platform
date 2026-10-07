using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace FinTech.ArchitectureTests;

public class CleanArchitectureTests
{
    private static readonly Assembly LedgerAssembly =
        typeof(FinTech.Modules.Ledger.Domain.LedgerAccountId).Assembly;

    private static readonly Assembly SharedKernelAssembly =
        typeof(FinTech.SharedKernel.IUnitOfWork).Assembly;

    [Fact]
    public void Domain_ShouldNot_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(LedgerAssembly)
            .That()
            .ResideInNamespace("FinTech.Modules.Ledger.Domain")
            .ShouldNot()
            .HaveDependencyOn("FinTech.Modules.Ledger.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_ShouldNot_DependOn_Application()
    {
        var result = Types.InAssembly(LedgerAssembly)
            .That()
            .ResideInNamespace("FinTech.Modules.Ledger.Domain")
            .ShouldNot()
            .HaveDependencyOn("FinTech.Modules.Ledger.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOn_Modules()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn("FinTech.Modules")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
