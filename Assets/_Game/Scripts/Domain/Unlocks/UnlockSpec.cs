using System;

namespace StockMarket.Domain.Unlocks
{
    public sealed class UnlockSpec
    {
        public UnlockSpec(
            string id,
            UnlockTargetScope targetScope,
            string targetId,
            long costMinorUnits,
            string requiredSectorId = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Unlock ID is required.", nameof(id));
            }

            if (!Enum.IsDefined(typeof(UnlockTargetScope), targetScope))
            {
                throw new ArgumentOutOfRangeException(nameof(targetScope));
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Unlock target ID is required.", nameof(targetId));
            }

            if (costMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costMinorUnits));
            }

            if (targetScope == UnlockTargetScope.Company && string.IsNullOrWhiteSpace(requiredSectorId))
            {
                throw new ArgumentException("Company unlocks require a sector ID.", nameof(requiredSectorId));
            }

            Id = id;
            TargetScope = targetScope;
            TargetId = targetId;
            CostMinorUnits = costMinorUnits;
            RequiredSectorId = requiredSectorId ?? string.Empty;
        }

        public string Id { get; }
        public UnlockTargetScope TargetScope { get; }
        public string TargetId { get; }
        public long CostMinorUnits { get; }
        public string RequiredSectorId { get; }
    }
}
