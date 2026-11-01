using BudgetManager.Domain.Accounts;
using BudgetManager.Domain.Common;
using BudgetManager.Domain.Money;
using BudgetManager.Domain.Transactions;
using Xunit;
using MonetaryAmount = BudgetManager.Domain.Money.Money;

namespace BudgetManager.Domain.Tests;

public sealed class FinancialTransactionTests
{
    private static readonly Currency Sar = new("SAR", 2);
    private static readonly DateTimeOffset UtcNow = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ExpensePostsWhenSplitsEqualAbsoluteAccountAmount()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = CreateExpense(accountId, -10_000, 4_000, 6_000);

        transaction.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1));

        Assert.Equal(TransactionStatus.Posted, transaction.Status);
        Assert.Equal(0, transaction.SplitBaseRoundingDifferenceMinorUnits);
    }

    [Fact]
    public void ExpenseRejectsSplitTotalMismatch()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = CreateExpense(accountId, -10_000, 4_000, 5_999);

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(
            () => transaction.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1)));

        Assert.Equal("transaction.split_total_mismatch", exception.Rule);
        Assert.Equal(TransactionStatus.Draft, transaction.Status);
    }

    [Fact]
    public void SameCurrencyTransferPreservesTotalAcrossManyAmounts()
    {
        for (long minorUnits = 1; minorUnits <= 10_000; minorUnits += 137)
        {
            Guid sourceAccountId = Guid.NewGuid();
            Guid destinationAccountId = Guid.NewGuid();
            FinancialTransaction transfer = CreateTransfer(
                sourceAccountId,
            new MonetaryAmount(-minorUnits, Sar),
                destinationAccountId,
                new MonetaryAmount(minorUnits, Sar));

            transfer.Post(
                AccountProfiles((sourceAccountId, Sar), (destinationAccountId, Sar)),
                UtcNow.AddMinutes(1));

            Assert.Equal(0, transfer.Legs.Sum(leg => leg.Amount.MinorUnits));
        }
    }

    [Fact]
    public void SameCurrencyTransferRejectsUnequalLegs()
    {
        Guid sourceAccountId = Guid.NewGuid();
        Guid destinationAccountId = Guid.NewGuid();
        FinancialTransaction transfer = CreateTransfer(
            sourceAccountId,
            new MonetaryAmount(-1_000, Sar),
            destinationAccountId,
            new MonetaryAmount(999, Sar));

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(
            () => transfer.Post(
                AccountProfiles((sourceAccountId, Sar), (destinationAccountId, Sar)),
                UtcNow.AddMinutes(1)));

        Assert.Equal("transaction.same_currency_transfer_mismatch", exception.Rule);
    }

    [Fact]
    public void CrossCurrencyTransferPreservesBothAmountsAndAppliedRate()
    {
        Currency usd = new("USD", 2);
        Guid sourceAccountId = Guid.NewGuid();
        Guid destinationAccountId = Guid.NewGuid();
        FinancialTransaction transfer = FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Transfer,
            new DateOnly(2026, 11, 1),
            [
                new TransactionLeg(
                    Guid.NewGuid(),
                    sourceAccountId,
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(-3_750, Sar))),
                new TransactionLeg(
                    Guid.NewGuid(),
                    destinationAccountId,
                    HistoricalValuation.Convert(new MonetaryAmount(1_000, usd), Sar, 0.2666666667m)),
            ],
            [],
            UtcNow);

        transfer.Post(
            AccountProfiles((sourceAccountId, Sar), (destinationAccountId, usd)),
            UtcNow.AddMinutes(1));

        Assert.Equal(-3_750, transfer.Legs[0].Amount.MinorUnits);
        Assert.Equal(1_000, transfer.Legs[1].Amount.MinorUnits);
        Assert.Equal(0.2666666666666666666666666667m, transfer.TransferRateDestinationUnitsPerSourceUnit);
        Assert.Equal(0, transfer.TransferBaseRoundingDifferenceMinorUnits);
    }

    [Fact]
    public void TransferRejectsSameAccount()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transfer = CreateTransfer(
            accountId,
            new MonetaryAmount(-1_000, Sar),
            accountId,
            new MonetaryAmount(1_000, Sar));

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(
            () => transfer.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1)));

        Assert.Equal("transaction.transfer_distinct_accounts", exception.Rule);
    }

    [Fact]
    public void AccountLegRejectsCurrencyDifferentFromAccountCurrency()
    {
        Currency usd = new("USD", 2);
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = CreateExpense(accountId, -1_000, 1_000);

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(
            () => transaction.Post(AccountProfiles((accountId, usd)), UtcNow.AddMinutes(1)));

        Assert.Equal("transaction.account_currency_mismatch", exception.Rule);
    }

    [Fact]
    public void ReconciledTransactionCannotBeVoidedDirectly()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = CreateExpense(accountId, -1_000, 1_000);
        transaction.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1));
        transaction.ProtectByReconciliation(Guid.NewGuid());

        DomainRuleViolationException exception = Assert.Throws<DomainRuleViolationException>(
            () => transaction.Void(UtcNow.AddMinutes(2)));

        Assert.Equal("transaction.reconciled_history_protected", exception.Rule);
        Assert.Equal(TransactionStatus.Posted, transaction.Status);
    }

    [Fact]
    public void VoidedTransactionRetainsOriginalLegsAndAuditState()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction transaction = CreateExpense(accountId, -1_000, 1_000);
        transaction.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1));

        transaction.Void(UtcNow.AddMinutes(2));

        Assert.Equal(TransactionStatus.Voided, transaction.Status);
        Assert.Single(transaction.Legs);
        Assert.Equal(-1_000, transaction.Legs[0].Amount.MinorUnits);
        Assert.Equal(UtcNow.AddMinutes(2), transaction.VoidedUtc);
        Assert.False(transaction.AffectsAccountBalances);
        Assert.False(transaction.ConsumesBudget);
    }

    [Fact]
    public void OnlyPostedExpensesConsumeBudgets()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction expense = CreateExpense(accountId, -1_000, 1_000);

        Assert.False(expense.AffectsAccountBalances);
        Assert.False(expense.ConsumesBudget);

        expense.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1));

        Assert.True(expense.AffectsAccountBalances);
        Assert.True(expense.ConsumesBudget);
        Assert.False(expense.CountsAsIncome);
    }

    [Fact]
    public void PostedTransferAffectsBalancesWithoutConsumingBudget()
    {
        Guid sourceAccountId = Guid.NewGuid();
        Guid destinationAccountId = Guid.NewGuid();
        FinancialTransaction transfer = CreateTransfer(
            sourceAccountId,
            new MonetaryAmount(-1_000, Sar),
            destinationAccountId,
            new MonetaryAmount(1_000, Sar));

        transfer.Post(
            AccountProfiles((sourceAccountId, Sar), (destinationAccountId, Sar)),
            UtcNow.AddMinutes(1));

        Assert.True(transfer.AffectsAccountBalances);
        Assert.False(transfer.ConsumesBudget);
        Assert.False(transfer.CountsAsIncome);
    }

    [Fact]
    public void TransferIntoSavingsIsSavingsFlowAndNotExpense()
    {
        Guid sourceAccountId = Guid.NewGuid();
        Guid savingsAccountId = Guid.NewGuid();
        FinancialTransaction transfer = CreateTransfer(
            sourceAccountId,
            new MonetaryAmount(-1_000, Sar),
            savingsAccountId,
            new MonetaryAmount(1_000, Sar));
        Dictionary<Guid, AccountPostingProfile> accounts = AccountProfiles(
            (sourceAccountId, Sar),
            (savingsAccountId, Sar));
        accounts[savingsAccountId] = new AccountPostingProfile(savingsAccountId, Sar, isSavings: true);

        transfer.Post(accounts, UtcNow.AddMinutes(1));

        Assert.True(transfer.IsSavingsTransfer);
        Assert.False(transfer.ConsumesBudget);
        Assert.False(transfer.CountsAsIncome);
    }

    [Fact]
    public void OnlyUnprotectedDraftCanBeHardDeleted()
    {
        Guid accountId = Guid.NewGuid();
        FinancialTransaction draft = CreateExpense(accountId, -1_000, 1_000);

        Assert.True(draft.CanBeHardDeleted(hasProtectedDependencies: false));
        Assert.False(draft.CanBeHardDeleted(hasProtectedDependencies: true));

        draft.Post(AccountProfiles((accountId, Sar)), UtcNow.AddMinutes(1));
        Assert.False(draft.CanBeHardDeleted(hasProtectedDependencies: false));
    }

    [Fact]
    public void SplitRoundingDifferenceIsPreserved()
    {
        Currency usd = new("USD", 2);
        Guid accountId = Guid.NewGuid();
        HistoricalValuation legValuation = HistoricalValuation.Convert(new MonetaryAmount(-2, usd), Sar, 3m);
        HistoricalValuation splitOne = HistoricalValuation.Convert(new MonetaryAmount(1, usd), Sar, 3m);
        HistoricalValuation splitTwo = HistoricalValuation.Convert(new MonetaryAmount(1, usd), Sar, 3m);
        FinancialTransaction transaction = FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Expense,
            new DateOnly(2026, 11, 1),
            [new TransactionLeg(Guid.NewGuid(), accountId, legValuation)],
            [
                new CategorySplit(Guid.NewGuid(), Guid.NewGuid(), splitOne),
                new CategorySplit(Guid.NewGuid(), Guid.NewGuid(), splitTwo),
            ],
            UtcNow);

        transaction.Post(AccountProfiles((accountId, usd)), UtcNow.AddMinutes(1));

        Assert.Equal(1, transaction.SplitBaseRoundingDifferenceMinorUnits);
    }

    private static FinancialTransaction CreateExpense(Guid accountId, long legAmount, params long[] splits)
        => FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Expense,
            new DateOnly(2026, 11, 1),
            [
                new TransactionLeg(
                    Guid.NewGuid(),
                    accountId,
                    HistoricalValuation.InBaseCurrency(new MonetaryAmount(legAmount, Sar))),
            ],
            splits.Select(split => new CategorySplit(
                Guid.NewGuid(),
                Guid.NewGuid(),
                HistoricalValuation.InBaseCurrency(new MonetaryAmount(split, Sar)))),
            UtcNow);

    private static FinancialTransaction CreateTransfer(
        Guid sourceAccountId,
        MonetaryAmount sourceAmount,
        Guid destinationAccountId,
        MonetaryAmount destinationAmount)
        => FinancialTransaction.CreateDraft(
            Guid.NewGuid(),
            TransactionKind.Transfer,
            new DateOnly(2026, 11, 1),
            [
                new TransactionLeg(
                    Guid.NewGuid(),
                    sourceAccountId,
                    HistoricalValuation.InBaseCurrency(sourceAmount)),
                new TransactionLeg(
                    Guid.NewGuid(),
                    destinationAccountId,
                    HistoricalValuation.InBaseCurrency(destinationAmount)),
            ],
            [],
            UtcNow);

    private static Dictionary<Guid, AccountPostingProfile> AccountProfiles(
        params (Guid Id, Currency Currency)[] accounts)
        => accounts.ToDictionary(
            account => account.Id,
            account => new AccountPostingProfile(account.Id, account.Currency, isSavings: false));
}
