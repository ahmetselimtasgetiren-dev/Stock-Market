namespace StockMarket.Domain.Portfolio
{
    public readonly struct PortfolioValuation
    {
        internal PortfolioValuation(
            long cashMinorUnits,
            long holdingsValueMinorUnits,
            long netWorthMinorUnits)
        {
            CashMinorUnits = cashMinorUnits;
            HoldingsValueMinorUnits = holdingsValueMinorUnits;
            NetWorthMinorUnits = netWorthMinorUnits;
        }

        public long CashMinorUnits { get; }

        public long HoldingsValueMinorUnits { get; }

        public long NetWorthMinorUnits { get; }
    }
}
