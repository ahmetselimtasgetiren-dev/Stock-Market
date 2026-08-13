namespace StockMarket.Domain.Progression
{
    public sealed class UpgradeProgress
    {
        internal UpgradeProgress(string upgradeId)
        {
            UpgradeId = upgradeId;
        }

        public string UpgradeId { get; }
        public int Level { get; private set; }
        public long TotalSpentMinorUnits { get; private set; }

        internal void ApplyPurchase(long costMinorUnits)
        {
            Level = checked(Level + 1);
            TotalSpentMinorUnits = checked(TotalSpentMinorUnits + costMinorUnits);
        }
    }
}
