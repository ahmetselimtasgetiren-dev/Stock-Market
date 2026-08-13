namespace StockMarket.Domain.Trading
{
    public enum TradeFailureReason
    {
        None = 0,
        InvalidCompanyId = 1,
        UnknownCompany = 2,
        InvalidQuantity = 3,
        InsufficientCash = 4,
        InsufficientShares = 5,
        ArithmeticOverflow = 6,
        CompanyLocked = 7
    }
}
