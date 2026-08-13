using System;

namespace StockMarket.Domain.Persistence
{
    public sealed class OfflineProgressService
    {
        public OfflineProgressPlan Calculate(
            long savedAtUnixSeconds,
            long currentUnixSeconds,
            long tickDurationSeconds,
            long maximumOfflineSeconds)
        {
            if (savedAtUnixSeconds < 0 || currentUnixSeconds < 0)
            {
                throw new ArgumentOutOfRangeException("Unix timestamps cannot be negative.");
            }

            if (tickDurationSeconds <= 0 || maximumOfflineSeconds < 0)
            {
                throw new ArgumentOutOfRangeException("Offline timing configuration is invalid.");
            }

            long elapsed = currentUnixSeconds <= savedAtUnixSeconds
                ? 0
                : currentUnixSeconds - savedAtUnixSeconds;
            long simulated = Math.Min(elapsed, maximumOfflineSeconds);
            return new OfflineProgressPlan(
                elapsed,
                simulated,
                simulated / tickDurationSeconds,
                elapsed > maximumOfflineSeconds);
        }

        public long Apply(long startingTick, OfflineProgressPlan plan, Action<long> processTick)
        {
            if (startingTick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingTick));
            }

            if (processTick == null)
            {
                throw new ArgumentNullException(nameof(processTick));
            }

            if (plan.Ticks > long.MaxValue - startingTick)
            {
                throw new OverflowException("Offline ticks exceed the market tick range.");
            }

            for (long offset = 1; offset <= plan.Ticks; offset++)
            {
                processTick(startingTick + offset);
            }

            return startingTick + plan.Ticks;
        }
    }
}
