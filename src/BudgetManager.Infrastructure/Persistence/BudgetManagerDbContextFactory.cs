using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class BudgetManagerDbContextFactory : IDesignTimeDbContextFactory<BudgetManagerDbContext>
{
    public BudgetManagerDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<BudgetManagerDbContext> options = new();
        options.UseSqlite("Data Source=budget-manager.design.db;Foreign Keys=True;Default Timeout=10");
        return new BudgetManagerDbContext(options.Options);
    }
}
