namespace StockMarket.Domain.Portfolio
{
    public readonly struct PortfolioPerformance
    {
        internal PortfolioPerformance(
            long holdingsValueMinorUnits,
            long costBasisMinorUnits,
            long unrealizedProfitMinorUnits,
            long realizedProfitMinorUnits,
            long totalProfitMinorUnits)
        {
            HoldingsValueMinorUnits = holdingsValueMinorUnits;
            CostBasisMinorUnits = costBasisMinorUnits;
            UnrealizedProfitMinorUnits = unrealizedProfitMinorUnits;
            RealizedProfitMinorUnits = realizedProfitMinorUnits;
            TotalProfitMinorUnits = totalProfitMinorUnits;
        }

        public long HoldingsValueMinorUnits { get; }
        public long CostBasisMinorUnits { get; }
        public long UnrealizedProfitMinorUnits { get; }
        public long RealizedProfitMinorUnits { get; }
        public long TotalProfitMinorUnits { get; }
    }
}
