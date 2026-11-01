using BudgetManager.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManager.Infrastructure.Persistence.Configurations;

internal sealed class CategoryRecordConfiguration : IEntityTypeConfiguration<CategoryRecord>
{
    public void Configure(EntityTypeBuilder<CategoryRecord> builder)
    {
        builder.ToTable("category", table =>
        {
            table.HasCheckConstraint("ck_category_kind", "kind IN ('Income','Expense')");
            table.HasCheckConstraint("ck_category_row_version", "row_version >= 0");
        });
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).HasColumnName("id");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(category => category.Kind).HasColumnName("kind").HasMaxLength(10);
        builder.Property(category => category.ParentId).HasColumnName("parent_id");
        builder.Property(category => category.ArchivedUtc).HasColumnName("archived_utc");
        builder.Property(category => category.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken();
        builder.HasOne(category => category.Parent)
            .WithMany()
            .HasForeignKey(category => category.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
