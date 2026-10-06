namespace FinTech.SharedKernel;

public readonly record struct Currency
{
    public string Code { get; }
    public int MinorUnits { get; }

    private Currency(string code, int minorUnits)
    {
        Code = code;
        MinorUnits = minorUnits;
    }

    public static readonly Currency VND = new("VND", 0);
    public static readonly Currency USD = new("USD", 2);
    public static readonly Currency EUR = new("EUR", 2);
    public static readonly Currency JPY = new("JPY", 0);

    public static Currency FromCode(string code) => code?.Trim().ToUpperInvariant() switch
    {
        "VND" => VND,
        "USD" => USD,
        "EUR" => EUR,
        "JPY" => JPY,
        _ => throw new ArgumentException($"Currency code '{code}' is not supported.", nameof(code))
    };

    public override string ToString() => Code;
}

public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        Amount = Math.Round(amount, currency.MinorUnits, MidpointRounding.ToEven);
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator -(Money a) => new(-a.Amount, a.Currency);

    public bool IsZero => Amount == 0m;
    public bool IsPositive => Amount > 0m;
    public bool IsNegative => Amount < 0m;

    /// <summary>
    /// Thuật toán chia tiền Largest Remainder Allocation bảo toàn 100% phần dư.
    /// Tổng các phần sau khi phân bổ LUÔN BẰNG ĐÚNG số tiền gốc ban đầu.
    /// </summary>
    public IReadOnlyList<Money> Allocate(params int[] ratios)
    {
        if (ratios is null || ratios.Length == 0)
        {
            throw new ArgumentException("Ratios cannot be empty.", nameof(ratios));
        }

        var totalRatio = ratios.Sum();
        if (totalRatio <= 0)
        {
            throw new ArgumentException("Total ratio must be greater than zero.", nameof(ratios));
        }

        var currentAmount = Amount;
        var currentCurrency = Currency;
        var unit = 1m / (decimal)Math.Pow(10, currentCurrency.MinorUnits);
        
        var shares = ratios
            .Select(r => Math.Floor(currentAmount * r / totalRatio / unit) * unit)
            .ToArray();

        var remainder = currentAmount - shares.Sum();
        var sortedIndices = Enumerable.Range(0, ratios.Length)
            .OrderByDescending(i => (currentAmount * ratios[i] / totalRatio) - shares[i])
            .ToArray();

        foreach (var index in sortedIndices)
        {
            if (remainder < unit && remainder > -unit)
            {
                break;
            }

            if (remainder > 0)
            {
                shares[index] += unit;
                remainder -= unit;
            }
            else if (remainder < 0)
            {
                shares[index] -= unit;
                remainder += unit;
            }
        }

        return shares.Select(s => new Money(s, currentCurrency)).ToArray();
    }

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException($"Cannot operate on different currencies: {a.Currency} and {b.Currency}");
        }
    }

    public override string ToString() => $"{Amount:N4} {Currency}";
}
