using System;

namespace StockMarket.Core.Clock
{
    /// <summary>
    /// A persistence-neutral snapshot of market clock progress.
    /// Save infrastructure can map this value to its own versioned data model later.
    /// </summary>
    public readonly struct MarketClockSnapshot
    {
        public MarketClockSnapshot(long currentTick, double accumulatedSeconds, bool isPaused)
        {
            if (currentTick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTick), "Current tick cannot be negative.");
            }

            if (double.IsNaN(accumulatedSeconds) ||
                double.IsInfinity(accumulatedSeconds) ||
                accumulatedSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(accumulatedSeconds),
                    "Accumulated time must be finite and non-negative.");
            }

            CurrentTick = currentTick;
            AccumulatedSeconds = accumulatedSeconds;
            IsPaused = isPaused;
        }

        public long CurrentTick { get; }

        public double AccumulatedSeconds { get; }

        public bool IsPaused { get; }
    }
}
