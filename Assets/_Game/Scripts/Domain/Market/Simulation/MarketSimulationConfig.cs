using System;

namespace StockMarket.Domain.Market.Simulation
{
    public sealed class MarketSimulationConfig
    {
        public MarketSimulationConfig(
            double globalDriftPerTick,
            double trendPersistence,
            double trendShockMagnitude,
            double maximumTrendMagnitude,
            double maximumPriceChangeRatio,
            long minimumPriceMinorUnits,
            long maximumPriceMinorUnits)
        {
            ValidateFiniteRange(globalDriftPerTick, -1d, 1d, nameof(globalDriftPerTick));
            ValidateFiniteRange(trendPersistence, 0d, 1d, nameof(trendPersistence), includeMaximum: true);
            ValidateFiniteRange(trendShockMagnitude, 0d, 1d, nameof(trendShockMagnitude), includeMaximum: true);
            ValidateFiniteRange(maximumTrendMagnitude, 0d, 1d, nameof(maximumTrendMagnitude), includeMaximum: true);
            ValidateFiniteRange(maximumPriceChangeRatio, 0d, 1d, nameof(maximumPriceChangeRatio));

            if (minimumPriceMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPriceMinorUnits),
                    "Minimum price must be positive.");
            }

            if (maximumPriceMinorUnits < minimumPriceMinorUnits)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumPriceMinorUnits),
                    "Maximum price cannot be below minimum price.");
            }

            GlobalDriftPerTick = globalDriftPerTick;
            TrendPersistence = trendPersistence;
            TrendShockMagnitude = trendShockMagnitude;
            MaximumTrendMagnitude = maximumTrendMagnitude;
            MaximumPriceChangeRatio = maximumPriceChangeRatio;
            MinimumPriceMinorUnits = minimumPriceMinorUnits;
            MaximumPriceMinorUnits = maximumPriceMinorUnits;
        }

        public double GlobalDriftPerTick { get; }

        public double TrendPersistence { get; }

        public double TrendShockMagnitude { get; }

        public double MaximumTrendMagnitude { get; }

        public double MaximumPriceChangeRatio { get; }

        public long MinimumPriceMinorUnits { get; }

        public long MaximumPriceMinorUnits { get; }

        private static void ValidateFiniteRange(
            double value,
            double minimum,
            double maximum,
            string parameterName,
            bool includeMaximum = false)
        {
            bool exceedsMaximum = includeMaximum ? value > maximum : value >= maximum;

            if (double.IsNaN(value) || double.IsInfinity(value) || value < minimum || exceedsMaximum)
            {
                string maximumDescription = includeMaximum ? "no more than" : "less than";
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"Value must be finite, at least {minimum}, and {maximumDescription} {maximum}.");
            }
        }
    }
}
