using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BudgetManager.Domain.Accounts;
using BudgetManager.Domain.Common;
using BudgetManager.Domain.Money;

namespace BudgetManager.Domain.Transactions;

public sealed class FinancialTransaction
{
    private readonly List<TransactionLeg> _legs;
    private readonly List<CategorySplit> _splits;

    private FinancialTransaction(
        Guid id,
        TransactionKind kind,
        DateOnly effectiveDate,
        IEnumerable<TransactionLeg> legs,
        IEnumerable<CategorySplit> splits,
        DateTimeOffset createdUtc,
        Guid? correctionOfTransactionId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A transaction identifier cannot be empty.", nameof(id));
        }

        EnsureUtc(createdUtc, nameof(createdUtc));

        Id = id;
        Kind = kind;
        EffectiveDate = effectiveDate;
        CreatedUtc = createdUtc;
        ModifiedUtc = createdUtc;
        CorrectionOfTransactionId = correctionOfTransactionId;
        Status = TransactionStatus.Draft;
        _legs = [.. legs];
        _splits = [.. splits];
    }

    public Guid Id { get; }

    public TransactionKind Kind { get; }

    public TransactionStatus Status { get; private set; }

    public DateOnly EffectiveDate { get; }

    public DateTimeOffset CreatedUtc { get; }

    public DateTimeOffset ModifiedUtc { get; private set; }

    public DateTimeOffset? PostedUtc { get; private set; }

    public DateTimeOffset? VoidedUtc { get; private set; }

    public Guid? CorrectionOfTransactionId { get; }

    public Guid? ReconciliationId { get; private set; }

    public IReadOnlyList<TransactionLeg> Legs => _legs;

    public IReadOnlyList<CategorySplit> Splits => _splits;

    public long SplitBaseRoundingDifferenceMinorUnits { get; private set; }

    public bool AffectsAccountBalances => Status == TransactionStatus.Posted;

    public bool ConsumesBudget => Status == TransactionStatus.Posted && Kind == TransactionKind.Expense;

    public bool CountsAsIncome => Status == TransactionStatus.Posted && Kind == TransactionKind.Income;

    public bool IsSavingsTransfer { get; private set; }

    public long? TransferBaseRoundingDifferenceMinorUnits
        => Kind == TransactionKind.Transfer && _legs.Count == 2
            ? checked(_legs[0].Valuation.BaseAmountMinorUnits + _legs[1].Valuation.BaseAmountMinorUnits)
            : null;

    public decimal? TransferRateDestinationUnitsPerSourceUnit
    {
        get
        {
            if (Kind != TransactionKind.Transfer || _legs.Count != 2)
            {
                return null;
            }

            TransactionLeg source = _legs.Single(leg => leg.Amount.MinorUnits < 0);
            TransactionLeg destination = _legs.Single(leg => leg.Amount.MinorUnits > 0);
            return destination.Amount.ToMajorUnits() / Math.Abs(source.Amount.ToMajorUnits());
        }
    }

    public static FinancialTransaction CreateDraft(
        Guid id,
        TransactionKind kind,
        DateOnly effectiveDate,
        IEnumerable<TransactionLeg> legs,
        IEnumerable<CategorySplit>? splits,
        DateTimeOffset createdUtc,
        Guid? correctionOfTransactionId = null)
    {
        ArgumentNullException.ThrowIfNull(legs);
        return new FinancialTransaction(
            id,
            kind,
            effectiveDate,
            legs,
            splits ?? [],
            createdUtc,
            correctionOfTransactionId);
    }

    public void Post(
        IReadOnlyDictionary<Guid, AccountPostingProfile> accounts,
        DateTimeOffset postedUtc)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        EnsureUtc(postedUtc, nameof(postedUtc));

        if (Status != TransactionStatus.Draft)
        {
            ThrowRule("transaction.only_draft_can_post", "Only a draft transaction can be posted.");
        }

        ValidateCommonRules(accounts);

        switch (Kind)
        {
            case TransactionKind.Income:
                ValidateIncomeOrExpense(expectedPositiveLeg: true);
                break;
            case TransactionKind.Expense:
                ValidateIncomeOrExpense(expectedPositiveLeg: false);
                break;
            case TransactionKind.Transfer:
                ValidateTransfer();
                TransactionLeg destination = _legs.Single(leg => leg.Amount.MinorUnits > 0);
                IsSavingsTransfer = accounts[destination.AccountId].IsSavings;
                break;
            case TransactionKind.OpeningBalance:
            case TransactionKind.BalanceAdjustment:
                ValidateSingleLegWithoutSplits();
                break;
            default:
                throw new UnreachableException();
        }

        Status = TransactionStatus.Posted;
        PostedUtc = postedUtc;
        ModifiedUtc = postedUtc;
    }

    public void Void(DateTimeOffset voidedUtc)
    {
        EnsureUtc(voidedUtc, nameof(voidedUtc));

        if (Status != TransactionStatus.Posted)
        {
            ThrowRule("transaction.only_posted_can_void", "Only a posted transaction can be voided.");
        }

        if (ReconciliationId is not null)
        {
            ThrowRule(
                "transaction.reconciled_history_protected",
                "A reconciled transaction must be reopened or corrected instead of directly voided.");
        }

        Status = TransactionStatus.Voided;
        VoidedUtc = voidedUtc;
        ModifiedUtc = voidedUtc;
    }

    public void ProtectByReconciliation(Guid reconciliationId)
    {
        if (reconciliationId == Guid.Empty)
        {
            throw new ArgumentException("A reconciliation identifier cannot be empty.", nameof(reconciliationId));
        }

        if (Status != TransactionStatus.Posted)
        {
            ThrowRule(
                "transaction.only_posted_can_reconcile",
                "Only a posted transaction can be protected by reconciliation.");
        }

        ReconciliationId = reconciliationId;
    }

    public bool CanBeHardDeleted(bool hasProtectedDependencies)
        => Status == TransactionStatus.Draft && !hasProtectedDependencies;

    private void ValidateCommonRules(IReadOnlyDictionary<Guid, AccountPostingProfile> accounts)
    {
        if (_legs.Count == 0)
        {
            ThrowRule("transaction.leg_required", "A posted transaction requires at least one account leg.");
        }

        if (_legs.Any(leg => leg.Amount.MinorUnits == 0))
        {
            ThrowRule("transaction.zero_leg", "A posted transaction cannot contain a zero-value account leg.");
        }

        if (_legs.Select(leg => leg.Id).Distinct().Count() != _legs.Count)
        {
            ThrowRule("transaction.duplicate_leg_id", "Transaction-leg identifiers must be unique.");
        }

        Currency baseCurrency = _legs[0].Valuation.BaseCurrency;
        if (_legs.Any(leg => leg.Valuation.BaseCurrency != baseCurrency)
            || _splits.Any(split => split.Valuation.BaseCurrency != baseCurrency))
        {
            ThrowRule(
                "transaction.base_currency_mismatch",
                "All historical valuations in a transaction must use the same base currency.");
        }

        foreach (TransactionLeg leg in _legs)
        {
            if (!accounts.TryGetValue(leg.AccountId, out AccountPostingProfile? account))
            {
                ThrowRule("transaction.account_not_found", "Every transaction leg must reference a known account.");
            }

            if (account.Currency != leg.Amount.Currency)
            {
                ThrowRule(
                    "transaction.account_currency_mismatch",
                    $"The {leg.Amount.Currency.Code} leg does not match its {account.Currency.Code} account.");
            }
        }
    }

    private void ValidateIncomeOrExpense(bool expectedPositiveLeg)
    {
        if (_legs.Count != 1)
        {
            ThrowRule(
                "transaction.income_expense_single_leg",
                "Income and expense transactions require exactly one account leg.");
        }

        TransactionLeg leg = _legs[0];
        bool signIsValid = expectedPositiveLeg
            ? leg.Amount.MinorUnits > 0
            : leg.Amount.MinorUnits < 0;
        if (!signIsValid)
        {
            ThrowRule(
                "transaction.income_expense_sign",
                expectedPositiveLeg
                    ? "An income account leg must be positive."
                    : "An expense account leg must be negative.");
        }

        if (_splits.Count == 0)
        {
            ThrowRule("transaction.split_required", "Posted income and expenses require at least one category split.");
        }

        if (_splits.Any(split => split.Amount.Currency != leg.Amount.Currency))
        {
            ThrowRule(
                "transaction.split_currency_mismatch",
                "Every category split must use the account-leg currency.");
        }

        if (_splits.Any(split => split.Amount.MinorUnits <= 0))
        {
            ThrowRule("transaction.split_must_be_positive", "Category split amounts must be positive.");
        }

        long splitTotal = _splits.Aggregate(0L, (total, split) => checked(total + split.Amount.MinorUnits));
        if (splitTotal != leg.Amount.Absolute().MinorUnits)
        {
            ThrowRule(
                "transaction.split_total_mismatch",
                "Category split totals must equal the absolute account amount.");
        }

        long splitBaseTotal = _splits.Aggregate(
            0L,
            (total, split) => checked(total + Math.Abs(split.Valuation.BaseAmountMinorUnits)));
        SplitBaseRoundingDifferenceMinorUnits = checked(
            Math.Abs(leg.Valuation.BaseAmountMinorUnits) - splitBaseTotal);
    }

    private void ValidateTransfer()
    {
        if (_legs.Count != 2)
        {
            ThrowRule("transaction.transfer_two_legs", "A transfer requires exactly two linked account legs.");
        }

        if (_splits.Count != 0)
        {
            ThrowRule("transaction.transfer_has_no_splits", "A transfer cannot contain category splits.");
        }

        if (_legs[0].AccountId == _legs[1].AccountId)
        {
            ThrowRule("transaction.transfer_distinct_accounts", "Transfer accounts must be different.");
        }

        int outflowCount = _legs.Count(leg => leg.Amount.MinorUnits < 0);
        int inflowCount = _legs.Count(leg => leg.Amount.MinorUnits > 0);
        if (outflowCount != 1 || inflowCount != 1)
        {
            ThrowRule(
                "transaction.transfer_opposite_signs",
                "A transfer requires one outflow leg and one inflow leg.");
        }

        TransactionLeg source = _legs.Single(leg => leg.Amount.MinorUnits < 0);
        TransactionLeg destination = _legs.Single(leg => leg.Amount.MinorUnits > 0);
        if (source.Amount.Currency == destination.Amount.Currency
            && source.Amount.Absolute().MinorUnits != destination.Amount.Absolute().MinorUnits)
        {
            ThrowRule(
                "transaction.same_currency_transfer_mismatch",
                "Same-currency transfer legs must have equal absolute amounts.");
        }

        if (source.Amount.Currency != destination.Amount.Currency
            && TransferRateDestinationUnitsPerSourceUnit <= 0m)
        {
            ThrowRule(
                "transaction.invalid_transfer_rate",
                "A cross-currency transfer must preserve a positive applied rate.");
        }
    }

    private void ValidateSingleLegWithoutSplits()
    {
        if (_legs.Count != 1)
        {
            ThrowRule(
                "transaction.single_leg_required",
                "Opening balances and balance adjustments require exactly one account leg.");
        }

        if (_splits.Count != 0)
        {
            ThrowRule(
                "transaction.non_spending_has_no_splits",
                "Opening balances and balance adjustments cannot contain expense splits.");
        }
    }

    private static void EnsureUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Audit timestamps must use UTC.", parameterName);
        }
    }

    [DoesNotReturn]
    private static void ThrowRule(string rule, string message)
        => throw new DomainRuleViolationException(rule, message);
}
