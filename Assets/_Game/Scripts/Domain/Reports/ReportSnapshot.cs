namespace StockMarket.Domain.Reports
{
    public readonly struct ReportSnapshot
    {
        public ReportSnapshot(
            long cash,
            long holdings,
            long netWorth,
            long costBasis,
            long realizedProfit,
            long unrealizedProfit,
            long dividendIncome,
            int transactionCount,
            int buyCount,
            int sellCount,
            int profitableSellCount,
            string bestHoldingCompanyId,
            long bestHoldingGain)
        {
            CashMinorUnits = cash;
            HoldingsValueMinorUnits = holdings;
            NetWorthMinorUnits = netWorth;
            CostBasisMinorUnits = costBasis;
            RealizedProfitMinorUnits = realizedProfit;
            UnrealizedProfitMinorUnits = unrealizedProfit;
            DividendIncomeMinorUnits = dividendIncome;
            TransactionCount = transactionCount;
            BuyCount = buyCount;
            SellCount = sellCount;
            ProfitableSellCount = profitableSellCount;
            BestHoldingCompanyId = bestHoldingCompanyId;
            BestHoldingGainMinorUnits = bestHoldingGain;
        }

        public long CashMinorUnits { get; }
        public long HoldingsValueMinorUnits { get; }
        public long NetWorthMinorUnits { get; }
        public long CostBasisMinorUnits { get; }
        public long RealizedProfitMinorUnits { get; }
        public long UnrealizedProfitMinorUnits { get; }
        public long DividendIncomeMinorUnits { get; }
        public long TotalProfitMinorUnits => RealizedProfitMinorUnits + UnrealizedProfitMinorUnits + DividendIncomeMinorUnits;
        public int TransactionCount { get; }
        public int BuyCount { get; }
        public int SellCount { get; }
        public int ProfitableSellCount { get; }
        public double ProfitableSellRatio => SellCount == 0 ? 0d : (double)ProfitableSellCount / SellCount;
        public string BestHoldingCompanyId { get; }
        public long BestHoldingGainMinorUnits { get; }
    }
}
