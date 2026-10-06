namespace FinTech.Modules.Ledger.Domain;

public readonly record struct LedgerAccountId(Guid Value)
{
    public static LedgerAccountId New() => new(Guid.CreateVersion7());
}
