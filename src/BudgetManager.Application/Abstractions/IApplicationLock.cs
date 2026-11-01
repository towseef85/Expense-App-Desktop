namespace BudgetManager.Application.Abstractions;

/// <summary>
/// Boundary for the optional application lock. The Windows Hello implementation is deferred
/// to Milestone 10 and must never expose or store a Windows credential.
/// </summary>
public interface IApplicationLock
{
    Task<ApplicationLockAvailability> GetAvailabilityAsync(CancellationToken cancellationToken = default);

    Task<ApplicationUnlockResult> RequestUnlockAsync(CancellationToken cancellationToken = default);
}

public enum ApplicationLockAvailability
{
    Available,
    Unavailable,
}

public enum ApplicationUnlockResult
{
    Verified,
    Canceled,
    Unavailable,
    Failed,
}
