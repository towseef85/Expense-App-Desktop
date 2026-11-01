using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class TransactionSplitRecordConfiguration : IEntityTypeConfiguration<TransactionSplitRecord>
{
    public void Configure(EntityTypeBuilder<TransactionSplitRecord> builder)
    {
        builder.ToTable("transaction_split", table =>
        {
            table.HasCheckConstraint("ck_transaction_split_positive", "amount_minor > 0");
            table.HasCheckConstraint("ck_transaction_split_rate", "quote_units_per_base > 0");
        });
        builder.HasKey(split => split.Id);
        builder.Property(split => split.Id).HasColumnName("id");
        builder.Property(split => split.TransactionId).HasColumnName("transaction_id");
        builder.Property(split => split.CategoryId).HasColumnName("category_id");
        builder.Property(split => split.AmountMinorUnits).HasColumnName("amount_minor");
        builder.Property(split => split.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
        builder.Property(split => split.BaseAmountMinorUnits).HasColumnName("base_amount_minor");
        builder.Property(split => split.BaseCurrencyCode).HasColumnName("base_currency_code").HasMaxLength(3);
        builder.Property(split => split.QuoteUnitsPerBaseUnit).HasColumnName("quote_units_per_base");
        builder.HasOne(split => split.Transaction)
            .WithMany(transaction => transaction.Splits)
            .HasForeignKey(split => split.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(split => split.Category)
            .WithMany()
            .HasForeignKey(split => split.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CurrencyRecord>()
            .WithMany()
            .HasForeignKey(split => split.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CurrencyRecord>()
            .WithMany()
            .HasForeignKey(split => split.BaseCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(split => new { split.CategoryId, split.TransactionId })
            .HasDatabaseName("ix_transaction_split_category_transaction");
    }
}
