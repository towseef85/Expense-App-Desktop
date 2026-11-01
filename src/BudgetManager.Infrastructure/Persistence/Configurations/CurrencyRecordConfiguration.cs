using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class CurrencyRecordConfiguration : IEntityTypeConfiguration<CurrencyRecord>
{
    public void Configure(EntityTypeBuilder<CurrencyRecord> builder)
    {
        builder.ToTable("currency", table =>
        {
            table.HasCheckConstraint("ck_currency_code", "length(code) = 3 AND code = upper(code)");
            table.HasCheckConstraint("ck_currency_minor_unit_digits", "minor_unit_digits BETWEEN 0 AND 3");
        });
        builder.HasKey(currency => currency.Code);
        builder.Property(currency => currency.Code).HasColumnName("code").HasMaxLength(3);
        builder.Property(currency => currency.MinorUnitDigits).HasColumnName("minor_unit_digits");
    }
}
