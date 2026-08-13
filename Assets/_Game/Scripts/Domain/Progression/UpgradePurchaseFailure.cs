namespace StockMarket.Domain.Progression
{
    public enum UpgradePurchaseFailure
    {
        None = 0,
        InvalidUpgradeId = 1,
        UnknownUpgrade = 2,
        MaximumLevelReached = 3,
        InsufficientCash = 4,
        ArithmeticOverflow = 5
    }
}
