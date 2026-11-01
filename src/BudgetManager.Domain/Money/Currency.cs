using System.Diagnostics;
using BudgetManager.Domain.Common;

namespace BudgetManager.Domain.Money;

public sealed record Currency
{
    public Currency(string code, int minorUnitDigits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        string normalizedCode = code.Trim().ToUpperInvariant();
        if (normalizedCode.Length != 3 || normalizedCode.Any(character => character is < 'A' or > 'Z'))
        {
            throw new DomainRuleViolationException(
                "currency.invalid_code",
                "An MVP currency code must contain exactly three ISO 4217 letters.");
        }

        if (minorUnitDigits is < 0 or > 3)
        {
            throw new DomainRuleViolationException(
                "currency.invalid_minor_unit_digits",
                "Currency minor-unit precision must be between zero and three digits.");
        }

        Code = normalizedCode;
        MinorUnitDigits = minorUnitDigits;
    }

    public string Code { get; }

    public int MinorUnitDigits { get; }

    public long MinorUnitsPerMajorUnit => MinorUnitDigits switch
    {
        0 => 1L,
        1 => 10L,
        2 => 100L,
        3 => 1_000L,
        _ => throw new UnreachableException(),
    };

    public override string ToString() => Code;
}
