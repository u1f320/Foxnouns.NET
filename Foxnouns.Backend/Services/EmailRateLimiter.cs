// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using NodaTime;
using NodaTime.Extensions;

namespace Foxnouns.Backend.Services;

public class EmailRateLimiter
{
    private readonly ConcurrentDictionary<string, RateLimiter> _limiters = new();

    private readonly FixedWindowRateLimiterOptions _limiterOptions = new()
    {
        Window = TimeSpan.FromHours(2),
        PermitLimit = 3,
    };

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
