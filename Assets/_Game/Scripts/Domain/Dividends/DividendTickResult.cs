namespace StockMarket.Domain.Dividends
{
    public readonly struct DividendTickResult
    {
        public DividendTickResult(long tick, int payoutCount, long totalAmountMinorUnits)
        {
            Tick = tick;
            PayoutCount = payoutCount;
            TotalAmountMinorUnits = totalAmountMinorUnits;
        }

        public long Tick { get; }
        public int PayoutCount { get; }
        public long TotalAmountMinorUnits { get; }
    }
}
