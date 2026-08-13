namespace StockMarket.Domain.Trading
{
    public readonly struct TransactionRecord
    {
        internal TransactionRecord(TradeResult result)
        {
            TransactionId = result.TransactionId;
            TradeType = result.TradeType;
            CompanyId = result.CompanyId;
            Quantity = result.Quantity;
            UnitPriceMinorUnits = result.UnitPriceMinorUnits;
            TotalValueMinorUnits = result.TotalValueMinorUnits;
            PriceTick = result.PriceTick;
            CashAfterMinorUnits = result.CashAfterMinorUnits;
            SharesAfter = result.SharesAfter;
            CostBasisRemovedMinorUnits = result.CostBasisRemovedMinorUnits;
            RealizedProfitMinorUnits = result.RealizedProfitMinorUnits;
        }

        public long TransactionId { get; }
        public TradeType TradeType { get; }
        public string CompanyId { get; }
        public long Quantity { get; }
        public long UnitPriceMinorUnits { get; }
        public long TotalValueMinorUnits { get; }
        public long PriceTick { get; }
        public long CashAfterMinorUnits { get; }
        public long SharesAfter { get; }
        public long CostBasisRemovedMinorUnits { get; }
        public long RealizedProfitMinorUnits { get; }
    }
}
