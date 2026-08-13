namespace StockMarket.Domain.Trading
{
    public readonly struct TradeResult
    {
        private TradeResult(
            bool succeeded,
            TradeType tradeType,
            string companyId,
            long quantity,
            TradeFailureReason failureReason,
            long unitPriceMinorUnits,
            long totalValueMinorUnits,
            long priceTick,
            long cashAfterMinorUnits,
            long sharesAfter,
            long transactionId,
            long costBasisRemovedMinorUnits,
            long realizedProfitMinorUnits)
        {
            Succeeded = succeeded;
            TradeType = tradeType;
            CompanyId = companyId;
            Quantity = quantity;
            FailureReason = failureReason;
            UnitPriceMinorUnits = unitPriceMinorUnits;
            TotalValueMinorUnits = totalValueMinorUnits;
            PriceTick = priceTick;
            CashAfterMinorUnits = cashAfterMinorUnits;
            SharesAfter = sharesAfter;
            TransactionId = transactionId;
            CostBasisRemovedMinorUnits = costBasisRemovedMinorUnits;
            RealizedProfitMinorUnits = realizedProfitMinorUnits;
        }

        public bool Succeeded { get; }

        public TradeType TradeType { get; }

        public string CompanyId { get; }

        public long Quantity { get; }

        public TradeFailureReason FailureReason { get; }

        public long UnitPriceMinorUnits { get; }

        public long TotalValueMinorUnits { get; }

        public long PriceTick { get; }

        public long CashAfterMinorUnits { get; }

        public long SharesAfter { get; }

        public long TransactionId { get; }

        public long CostBasisRemovedMinorUnits { get; }

        public long RealizedProfitMinorUnits { get; }

        internal static TradeResult Success(
            TradeType tradeType,
            string companyId,
            long quantity,
            long unitPriceMinorUnits,
            long totalValueMinorUnits,
            long priceTick,
            long cashAfterMinorUnits,
            long sharesAfter,
            long transactionId,
            long costBasisRemovedMinorUnits = 0,
            long realizedProfitMinorUnits = 0)
        {
            return new TradeResult(
                true,
                tradeType,
                companyId,
                quantity,
                TradeFailureReason.None,
                unitPriceMinorUnits,
                totalValueMinorUnits,
                priceTick,
                cashAfterMinorUnits,
                sharesAfter,
                transactionId,
                costBasisRemovedMinorUnits,
                realizedProfitMinorUnits);
        }

        internal static TradeResult Failure(
            TradeType tradeType,
            string companyId,
            long quantity,
            TradeFailureReason failureReason,
            long unitPriceMinorUnits = 0,
            long totalValueMinorUnits = 0,
            long priceTick = 0)
        {
            return new TradeResult(
                false,
                tradeType,
                companyId,
                quantity,
                failureReason,
                unitPriceMinorUnits,
                totalValueMinorUnits,
                priceTick,
                0,
                0,
                0,
                0,
                0);
        }
    }
}
