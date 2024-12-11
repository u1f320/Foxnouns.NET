using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using NodaTime;
using NodaTime.Extensions;

namespace Foxnouns.Backend.Services;

public class EmailRateLimiter
{
    private readonly ConcurrentDictionary<string, RateLimiter> _limiters = new();

    private readonly FixedWindowRateLimiterOptions _limiterOptions =
        new() { Window = TimeSpan.FromHours(2), PermitLimit = 3 };

    private RateLimiter GetLimiter(string bucket) =>
        _limiters.GetOrAdd(bucket, _ => new FixedWindowRateLimiter(_limiterOptions));

    public bool IsLimited(string bucket, out Duration retryAfter)
    {
        RateLimiter limiter = GetLimiter(bucket);
        RateLimitLease lease = limiter.AttemptAcquire();

        if (!lease.IsAcquired)
        {
            retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan timeSpan)
                ? timeSpan.ToDuration()
                : default;
        }
        else
        {
            retryAfter = Duration.Zero;
        }

        return !lease.IsAcquired;
    }
}
