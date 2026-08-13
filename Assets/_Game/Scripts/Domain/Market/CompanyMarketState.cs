namespace StockMarket.Domain.Market
{
    public sealed class CompanyMarketState
    {
        internal CompanyMarketState(CompanyMarketSeed seed, int historyCapacity)
        {
            CompanyId = seed.CompanyId;
            CurrentPriceMinorUnits = seed.StartingPriceMinorUnits;
            PreviousPriceMinorUnits = seed.StartingPriceMinorUnits;
            PriceHistory = new PriceHistoryBuffer(historyCapacity);
            PriceHistory.Add(new PricePoint(0, seed.StartingPriceMinorUnits));
        }

        public string CompanyId { get; }

        public long CurrentPriceMinorUnits { get; private set; }

        public long PreviousPriceMinorUnits { get; private set; }

        public long LastUpdatedTick { get; private set; }

        public long PriceChangeMinorUnits => CurrentPriceMinorUnits - PreviousPriceMinorUnits;

        public double PriceChangeRatio =>
            (double)PriceChangeMinorUnits / PreviousPriceMinorUnits;

        public PriceHistoryBuffer PriceHistory { get; }

        internal void ApplyPrice(long tick, long priceMinorUnits)
        {
            var point = new PricePoint(tick, priceMinorUnits);

            if (tick <= LastUpdatedTick)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(tick),
                    "A company price update must use a tick later than its previous update.");
            }

            PreviousPriceMinorUnits = CurrentPriceMinorUnits;
            CurrentPriceMinorUnits = priceMinorUnits;
            LastUpdatedTick = tick;
            PriceHistory.Add(point);
        }
    }
}
