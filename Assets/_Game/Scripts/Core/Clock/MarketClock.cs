using System;

namespace StockMarket.Core.Clock
{
    /// <summary>
    /// Converts elapsed real time into deterministic, fixed-duration market ticks.
    /// This class deliberately has no Unity lifecycle or wall-clock dependency.
    /// </summary>
    public sealed class MarketClock
    {
        private const double TickComparisonTolerance = 1e-9d;

        private double accumulatedSeconds;

        public MarketClock(double tickDurationSeconds)
        {
            if (!IsFinitePositive(tickDurationSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickDurationSeconds),
                    "Tick duration must be finite and positive.");
            }

            TickDurationSeconds = tickDurationSeconds;
        }

        public event Action<MarketTick> TickOccurred;

        public double TickDurationSeconds { get; }

        public long CurrentTick { get; private set; }

        public double AccumulatedSeconds => accumulatedSeconds;

        public bool IsPaused { get; private set; }

        public double ElapsedSimulationSeconds => CurrentTick * TickDurationSeconds;

        /// <summary>
        /// Advances the clock and returns the number of ticks emitted.
        /// Elapsed time supplied while paused is intentionally discarded.
        /// </summary>
        public long Advance(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(elapsedSeconds),
                    "Elapsed time must be finite and non-negative.");
            }

            if (IsPaused || elapsedSeconds == 0d)
            {
                return 0;
            }

            accumulatedSeconds += elapsedSeconds;
            long emittedTicks = 0;
            double tolerance = TickDurationSeconds * TickComparisonTolerance;

            while (accumulatedSeconds + tolerance >= TickDurationSeconds)
            {
                accumulatedSeconds -= TickDurationSeconds;

                if (accumulatedSeconds < 0d)
                {
                    accumulatedSeconds = 0d;
                }

                CurrentTick++;
                emittedTicks++;
                TickOccurred?.Invoke(new MarketTick(CurrentTick, ElapsedSimulationSeconds));
            }

            return emittedTicks;
        }

        public void SetPaused(bool isPaused)
        {
            IsPaused = isPaused;
        }

        public MarketClockSnapshot CaptureSnapshot()
        {
            return new MarketClockSnapshot(CurrentTick, accumulatedSeconds, IsPaused);
        }

        public void Restore(MarketClockSnapshot snapshot)
        {
            double tolerance = TickDurationSeconds * TickComparisonTolerance;

            if (snapshot.AccumulatedSeconds + tolerance >= TickDurationSeconds)
            {
                throw new ArgumentException(
                    "Accumulated snapshot time must be less than the clock's tick duration.",
                    nameof(snapshot));
            }

            CurrentTick = snapshot.CurrentTick;
            accumulatedSeconds = snapshot.AccumulatedSeconds;
            IsPaused = snapshot.IsPaused;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0d;
        }
    }
}
