using System;

namespace StockMarket.Domain.Market.Simulation
{
    public sealed class CompanySimulationProfile
    {
        public CompanySimulationProfile(
            string companyId,
            double volatility,
            double driftPerTick = 0d)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }

            if (!IsFinite(volatility) || volatility < 0d || volatility > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(volatility),
                    "Volatility must be finite and between zero and one.");
            }

            if (!IsFinite(driftPerTick) || driftPerTick <= -1d || driftPerTick >= 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(driftPerTick),
                    "Per-tick drift must be finite and strictly between negative one and one.");
            }

            CompanyId = companyId;
            Volatility = volatility;
            DriftPerTick = driftPerTick;
        }

        public string CompanyId { get; }

        public double Volatility { get; }

        public double DriftPerTick { get; }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
