using System;

namespace StockMarket.Domain.Dividends
{
    public sealed class DividendPolicy
    {
        public DividendPolicy(
            string id,
            string companyId,
            long amountPerShareMinorUnits,
            long intervalTicks,
            long firstPayoutTick)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Dividend policy ID is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }

            if (amountPerShareMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amountPerShareMinorUnits));
            }

            if (intervalTicks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalTicks));
            }

            if (firstPayoutTick <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstPayoutTick));
            }

            Id = id;
            CompanyId = companyId;
            AmountPerShareMinorUnits = amountPerShareMinorUnits;
            IntervalTicks = intervalTicks;
            FirstPayoutTick = firstPayoutTick;
        }

        public string Id { get; }
        public string CompanyId { get; }
        public long AmountPerShareMinorUnits { get; }
        public long IntervalTicks { get; }
        public long FirstPayoutTick { get; }
    }
}
