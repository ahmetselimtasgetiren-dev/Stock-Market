using System;

namespace StockMarket.Domain.Market
{
    /// <summary>
    /// Persistence- and Unity-neutral input used to initialize one live company state.
    /// </summary>
    public sealed class CompanyMarketSeed
    {
        public CompanyMarketSeed(string companyId, long startingPriceMinorUnits)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }

            if (startingPriceMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingPriceMinorUnits),
                    "Starting price must be greater than zero.");
            }

            CompanyId = companyId;
            StartingPriceMinorUnits = startingPriceMinorUnits;
        }

        public string CompanyId { get; }

        public long StartingPriceMinorUnits { get; }
    }
}
