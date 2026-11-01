using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Infrastructure.Persistence;

public sealed class BudgetManagerDbContext(DbContextOptions<BudgetManagerDbContext> options)
    : DbContext(options)
{
    // The financial model and first migration are intentionally deferred until Milestones 2 and 3.
}
