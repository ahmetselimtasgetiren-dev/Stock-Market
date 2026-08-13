namespace StockMarket.Domain.Progression
{
    public readonly struct UpgradePurchaseResult
    {
        private UpgradePurchaseResult(
            bool succeeded,
            string upgradeId,
            UpgradePurchaseFailure failure,
            int newLevel,
            long costMinorUnits,
            long cashAfterMinorUnits)
        {
            Succeeded = succeeded;
            UpgradeId = upgradeId;
            Failure = failure;
            NewLevel = newLevel;
            CostMinorUnits = costMinorUnits;
            CashAfterMinorUnits = cashAfterMinorUnits;
        }

        public bool Succeeded { get; }
        public string UpgradeId { get; }
        public UpgradePurchaseFailure Failure { get; }
        public int NewLevel { get; }
        public long CostMinorUnits { get; }
        public long CashAfterMinorUnits { get; }

        internal static UpgradePurchaseResult Success(
            string upgradeId,
            int newLevel,
            long costMinorUnits,
            long cashAfterMinorUnits)
        {
            return new UpgradePurchaseResult(
                true,
                upgradeId,
                UpgradePurchaseFailure.None,
                newLevel,
                costMinorUnits,
                cashAfterMinorUnits);
        }

        internal static UpgradePurchaseResult Failed(
            string upgradeId,
            UpgradePurchaseFailure failure,
            long costMinorUnits = 0)
        {
            return new UpgradePurchaseResult(false, upgradeId, failure, 0, costMinorUnits, 0);
        }
    }
}
