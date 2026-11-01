namespace BudgetManager.Domain.Common;

public sealed class DomainRuleViolationException(string rule, string message) : Exception(message)
{
    public string Rule { get; } = rule;
}
