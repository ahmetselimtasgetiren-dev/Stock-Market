namespace StockMarket.Domain.Dividends
{
    public readonly struct DividendPayoutRecord
    {
        internal DividendPayoutRecord(
            long payoutId,
            string policyId,
            string companyId,
            long tick,
            long shareQuantity,
            long amountPerShareMinorUnits,
            long totalAmountMinorUnits)
        {
            PayoutId = payoutId;
            PolicyId = policyId;
            CompanyId = companyId;
            Tick = tick;
            ShareQuantity = shareQuantity;
            AmountPerShareMinorUnits = amountPerShareMinorUnits;
            TotalAmountMinorUnits = totalAmountMinorUnits;
        }

        public long PayoutId { get; }
        public string PolicyId { get; }
        public string CompanyId { get; }
        public long Tick { get; }
        public long ShareQuantity { get; }
        public long AmountPerShareMinorUnits { get; }
        public long TotalAmountMinorUnits { get; }
    }
}
