using Qonote.Core.Domain.Enums;

namespace Qonote.Core.Application.Common.Subscriptions;

public static class SubscriptionPeriodHelper
{
    // Compute the subscription period that contains the given 'now' instant, starting from an anchorStart and interval.
    // If hardEnd is provided and the computed end exceeds it, clamp to hardEnd.
    public static (DateTime start, DateTime end) ComputeContainingPeriod(
        DateTime anchorStartUtc,
        BillingInterval interval,
        DateTime nowUtc,
        DateTime? hardEndUtc = null)
    {
        if (anchorStartUtc.Kind != DateTimeKind.Utc || nowUtc.Kind != DateTimeKind.Utc || (hardEndUtc.HasValue && hardEndUtc.Value.Kind != DateTimeKind.Utc))
        {
            // Normalize to UTC to avoid subtle boundary errors
            anchorStartUtc = DateTime.SpecifyKind(anchorStartUtc, DateTimeKind.Utc);
            nowUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
            if (hardEndUtc.HasValue)
            {
                hardEndUtc = DateTime.SpecifyKind(hardEndUtc.Value, DateTimeKind.Utc);
            }
        }

        var start = anchorStartUtc;

        if (interval == BillingInterval.Monthly)
        {
            // Advance start by whole months until the next period would exceed 'now'
            while (start.AddMonths(1) <= nowUtc)
            {
                start = start.AddMonths(1);
            }
            var end = start.AddMonths(1);
            if (hardEndUtc.HasValue && end > hardEndUtc.Value)
            {
                end = hardEndUtc.Value;
            }
            return (start, end);
        }
        else // Yearly
        {
            while (start.AddYears(1) <= nowUtc)
            {
                start = start.AddYears(1);
            }
            var end = start.AddYears(1);
            if (hardEndUtc.HasValue && end > hardEndUtc.Value)
            {
                end = hardEndUtc.Value;
            }
            return (start, end);
        }
    }
}