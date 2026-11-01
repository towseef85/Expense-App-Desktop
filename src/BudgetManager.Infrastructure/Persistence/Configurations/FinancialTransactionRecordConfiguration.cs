using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class FinancialTransactionRecordConfiguration
    : IEntityTypeConfiguration<FinancialTransactionRecord>
{
    public void Configure(EntityTypeBuilder<FinancialTransactionRecord> builder)
    {
        builder.ToTable("financial_transaction", table =>
        {
            table.HasCheckConstraint(
                "ck_financial_transaction_kind",
                "kind IN ('Income','Expense','Transfer','OpeningBalance','BalanceAdjustment')");
            table.HasCheckConstraint(
                "ck_financial_transaction_status",
                "status IN ('Draft','Posted','Voided')");
            table.HasCheckConstraint("ck_financial_transaction_row_version", "row_version >= 0");
        });
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).HasColumnName("id");
        builder.Property(transaction => transaction.Kind).HasColumnName("kind").HasMaxLength(20);
        builder.Property(transaction => transaction.Status).HasColumnName("status").HasMaxLength(10);
        builder.Property(transaction => transaction.EffectiveDate).HasColumnName("effective_date");
        builder.Property(transaction => transaction.CreatedUtc).HasColumnName("created_utc");
        builder.Property(transaction => transaction.ModifiedUtc).HasColumnName("modified_utc");
        builder.Property(transaction => transaction.PostedUtc).HasColumnName("posted_utc");
        builder.Property(transaction => transaction.VoidedUtc).HasColumnName("voided_utc");
        builder.Property(transaction => transaction.CorrectionOfTransactionId)
            .HasColumnName("correction_of_transaction_id");
        builder.Property(transaction => transaction.ReconciliationId).HasColumnName("reconciliation_id");
        builder.Property(transaction => transaction.SplitBaseRoundingDifferenceMinorUnits)
            .HasColumnName("split_base_rounding_difference_minor");
        builder.Property(transaction => transaction.IsSavingsTransfer).HasColumnName("is_savings_transfer");
        builder.Property(transaction => transaction.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken();
        builder.HasOne(transaction => transaction.CorrectionOfTransaction)
            .WithMany()
            .HasForeignKey(transaction => transaction.CorrectionOfTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(transaction => new { transaction.EffectiveDate, transaction.Status })
            .HasDatabaseName("ix_financial_transaction_effective_date_status");
    }
}
