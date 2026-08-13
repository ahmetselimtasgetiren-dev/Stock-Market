namespace StockMarket.Domain.Unlocks
{
    public readonly struct UnlockResult
    {
        private UnlockResult(
            bool succeeded,
            string unlockId,
            UnlockTargetScope targetScope,
            string targetId,
            UnlockFailureReason failureReason,
            long costMinorUnits,
            long cashAfterMinorUnits)
        {
            Succeeded = succeeded;
            UnlockId = unlockId;
            TargetScope = targetScope;
            TargetId = targetId;
            FailureReason = failureReason;
            CostMinorUnits = costMinorUnits;
            CashAfterMinorUnits = cashAfterMinorUnits;
        }

        public bool Succeeded { get; }
        public string UnlockId { get; }
        public UnlockTargetScope TargetScope { get; }
        public string TargetId { get; }
        public UnlockFailureReason FailureReason { get; }
        public long CostMinorUnits { get; }
        public long CashAfterMinorUnits { get; }

        internal static UnlockResult Success(UnlockSpec spec, long cashAfterMinorUnits)
        {
            return new UnlockResult(
                true,
                spec.Id,
                spec.TargetScope,
                spec.TargetId,
                UnlockFailureReason.None,
                spec.CostMinorUnits,
                cashAfterMinorUnits);
        }

        internal static UnlockResult Failure(
            string unlockId,
            UnlockFailureReason reason,
            UnlockSpec spec = null)
        {
            return new UnlockResult(
                false,
                unlockId,
                spec?.TargetScope ?? default,
                spec?.TargetId,
                reason,
                spec?.CostMinorUnits ?? 0,
                0);
        }
    }
}
