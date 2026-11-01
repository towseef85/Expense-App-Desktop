using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class TransactionLegRecordConfiguration : IEntityTypeConfiguration<TransactionLegRecord>
{
    public void Configure(EntityTypeBuilder<TransactionLegRecord> builder)
    {
        builder.ToTable("transaction_leg", table =>
        {
            table.HasCheckConstraint("ck_transaction_leg_nonzero", "amount_minor <> 0");
            table.HasCheckConstraint("ck_transaction_leg_rate", "quote_units_per_base > 0");
        });
        builder.HasKey(leg => leg.Id);
        builder.Property(leg => leg.Id).HasColumnName("id");
        builder.Property(leg => leg.TransactionId).HasColumnName("transaction_id");
        builder.Property(leg => leg.AccountId).HasColumnName("account_id");
        builder.Property(leg => leg.AmountMinorUnits).HasColumnName("amount_minor");
        builder.Property(leg => leg.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
        builder.Property(leg => leg.BaseAmountMinorUnits).HasColumnName("base_amount_minor");
        builder.Property(leg => leg.BaseCurrencyCode).HasColumnName("base_currency_code").HasMaxLength(3);
        builder.Property(leg => leg.QuoteUnitsPerBaseUnit).HasColumnName("quote_units_per_base");
        builder.HasOne(leg => leg.Transaction)
            .WithMany(transaction => transaction.Legs)
            .HasForeignKey(leg => leg.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(leg => leg.Account)
            .WithMany()
            .HasForeignKey(leg => leg.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CurrencyRecord>()
            .WithMany()
            .HasForeignKey(leg => leg.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CurrencyRecord>()
            .WithMany()
            .HasForeignKey(leg => leg.BaseCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(leg => new { leg.AccountId, leg.TransactionId })
            .HasDatabaseName("ix_transaction_leg_account_transaction");
    }
}
