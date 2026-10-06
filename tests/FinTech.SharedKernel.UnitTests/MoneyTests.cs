using FinTech.SharedKernel;
using Xunit;

namespace FinTech.SharedKernel.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Money_Addition_SameCurrency_Succeeds()
    {
        var m1 = new Money(100_000m, Currency.VND);
        var m2 = new Money(50_000m, Currency.VND);

        var result = m1 + m2;

        Assert.Equal(150_000m, result.Amount);
        Assert.Equal(Currency.VND, result.Currency);
    }

    [Fact]
    public void Money_Addition_DifferentCurrencies_ThrowsException()
    {
        var m1 = new Money(100_000m, Currency.VND);
        var m2 = new Money(10m, Currency.USD);

        Assert.Throws<InvalidOperationException>(() => m1 + m2);
    }

    [Fact]
    public void Allocate_LargestRemainder_PreservesTotalExactly()
    {
        // Chia 100.000 VND cho 3 phần tỷ lệ (1:1:1)
        var total = new Money(100_000m, Currency.VND);
        
        var shares = total.Allocate(1, 1, 1);

        // 33.334 + 33.333 + 33.333 = 100.000
        Assert.Equal(3, shares.Count);
        Assert.Equal(total.Amount, shares.Sum(s => s.Amount));
    }

    [Fact]
    public void Allocate_CentCurrencies_PreservesCentsExactly()
    {
        // Chia 100 USD thành 3 phần tỷ lệ (1:1:1)
        var total = new Money(100.00m, Currency.USD);

        var shares = total.Allocate(1, 1, 1);

        // 33.34 + 33.33 + 33.33 = 100.00
        Assert.Equal(3, shares.Count);
        Assert.Equal(total.Amount, shares.Sum(s => s.Amount));
    }
}
