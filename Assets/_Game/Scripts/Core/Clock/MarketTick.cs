using System;

namespace StockMarket.Core.Clock
{
    /// <summary>
    /// Describes one completed, sequential market simulation step.
    /// </summary>
    public readonly struct MarketTick
    {
        public MarketTick(long index, double elapsedSimulationSeconds)
        {
            if (index < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "A market tick index must be at least one.");
            }

            if (double.IsNaN(elapsedSimulationSeconds) ||
                double.IsInfinity(elapsedSimulationSeconds) ||
                elapsedSimulationSeconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(elapsedSimulationSeconds),
                    "Elapsed simulation time must be finite and positive.");
            }

            Index = index;
            ElapsedSimulationSeconds = elapsedSimulationSeconds;
        }

        public long Index { get; }

        public double ElapsedSimulationSeconds { get; }
    }
}
