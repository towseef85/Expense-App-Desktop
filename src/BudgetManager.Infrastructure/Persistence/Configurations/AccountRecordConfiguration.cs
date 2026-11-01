using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class AccountRecordConfiguration : IEntityTypeConfiguration<AccountRecord>
{
    public void Configure(EntityTypeBuilder<AccountRecord> builder)
    {
        builder.ToTable("account", table =>
        {
            table.HasCheckConstraint(
                "ck_account_type",
                "account_type IN ('Cash','Checking','Savings','CreditCard','EWallet','Loan','Other')");
            table.HasCheckConstraint("ck_account_row_version", "row_version >= 0");
        });
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasColumnName("id");
        builder.Property(account => account.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(account => account.AccountType).HasColumnName("account_type").HasMaxLength(20);
        builder.Property(account => account.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
        builder.Property(account => account.IsSavings).HasColumnName("is_savings");
        builder.Property(account => account.ArchivedUtc).HasColumnName("archived_utc");
        builder.Property(account => account.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken();
        builder.HasOne(account => account.Currency)
            .WithMany()
            .HasForeignKey(account => account.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(account => account.Name)
            .IsUnique()
            .HasFilter("archived_utc IS NULL")
            .HasDatabaseName("ux_account_active_name");
    }
}
