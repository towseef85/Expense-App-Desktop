using BudgetManager.Application.Abstractions;
using BudgetManager.Domain.Transactions;
using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class EfFinancialTransactionStore(BudgetManagerDbContext dbContext)
    : IFinancialTransactionStore
{
    public async Task SavePostedAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (transaction.Status != TransactionStatus.Posted)
        {
            throw new InvalidOperationException("Only a validated posted transaction can be persisted.");
        }

        FinancialTransactionRecord record = Map(transaction);

        await using IDbContextTransaction databaseTransaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        dbContext.FinancialTransactions.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await databaseTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static FinancialTransactionRecord Map(FinancialTransaction transaction)
    {
        FinancialTransactionRecord record = new()
        {
            Id = transaction.Id,
            Kind = transaction.Kind.ToString(),
            Status = transaction.Status.ToString(),
            EffectiveDate = transaction.EffectiveDate,
            CreatedUtc = transaction.CreatedUtc,
            ModifiedUtc = transaction.ModifiedUtc,
            PostedUtc = transaction.PostedUtc,
            VoidedUtc = transaction.VoidedUtc,
            CorrectionOfTransactionId = transaction.CorrectionOfTransactionId,
            ReconciliationId = transaction.ReconciliationId,
            SplitBaseRoundingDifferenceMinorUnits = transaction.SplitBaseRoundingDifferenceMinorUnits,
            IsSavingsTransfer = transaction.IsSavingsTransfer,
        };

        record.Legs.AddRange(transaction.Legs.Select(leg => new TransactionLegRecord
        {
            Id = leg.Id,
            TransactionId = transaction.Id,
            AccountId = leg.AccountId,
            AmountMinorUnits = leg.Amount.MinorUnits,
            CurrencyCode = leg.Amount.Currency.Code,
            BaseAmountMinorUnits = leg.Valuation.BaseAmountMinorUnits,
            BaseCurrencyCode = leg.Valuation.BaseCurrency.Code,
            QuoteUnitsPerBaseUnit = leg.Valuation.QuoteUnitsPerBaseUnit,
        }));
        record.Splits.AddRange(transaction.Splits.Select(split => new TransactionSplitRecord
        {
            Id = split.Id,
            TransactionId = transaction.Id,
            CategoryId = split.CategoryId,
            AmountMinorUnits = split.Amount.MinorUnits,
            CurrencyCode = split.Amount.Currency.Code,
            BaseAmountMinorUnits = split.Valuation.BaseAmountMinorUnits,
            BaseCurrencyCode = split.Valuation.BaseCurrency.Code,
            QuoteUnitsPerBaseUnit = split.Valuation.QuoteUnitsPerBaseUnit,
        }));

        return record;
    }
}
