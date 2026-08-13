using System;

namespace StockMarket.Domain.Market
{
    public readonly struct PricePoint
    {
        public PricePoint(long tick, long priceMinorUnits)
        {
            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "Price point tick cannot be negative.");
            }

            if (priceMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(priceMinorUnits),
                    "Price must be greater than zero.");
            }

            Tick = tick;
            PriceMinorUnits = priceMinorUnits;
        }

        public long Tick { get; }

        public long PriceMinorUnits { get; }
    }
}
