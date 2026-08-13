using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Progression
{
    public sealed class ProgressionState
    {
        private readonly List<UpgradeProgress> upgrades = new List<UpgradeProgress>();
        private readonly IReadOnlyList<UpgradeProgress> readOnlyUpgrades;
        private readonly Dictionary<string, UpgradeProgress> upgradesById =
            new Dictionary<string, UpgradeProgress>(StringComparer.Ordinal);

        public ProgressionState()
        {
            readOnlyUpgrades = upgrades.AsReadOnly();
        }

        public IReadOnlyList<UpgradeProgress> Upgrades => readOnlyUpgrades;
        public long TotalSpentMinorUnits { get; private set; }

        public int GetLevel(string upgradeId)
        {
            return upgradeId != null && upgradesById.TryGetValue(upgradeId, out UpgradeProgress progress)
                ? progress.Level
                : 0;
        }

        internal bool CanApplyPurchase(string upgradeId, long costMinorUnits)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || costMinorUnits <= 0)
            {
                return false;
            }

            try
            {
                checked
                {
                    _ = TotalSpentMinorUnits + costMinorUnits;

                    if (upgradesById.TryGetValue(upgradeId, out UpgradeProgress progress))
                    {
                        _ = progress.Level + 1;
                        _ = progress.TotalSpentMinorUnits + costMinorUnits;
                    }
                }

                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        internal void ApplyPurchase(string upgradeId, long costMinorUnits)
        {
            if (!CanApplyPurchase(upgradeId, costMinorUnits))
            {
                throw new OverflowException("Upgrade purchase cannot be applied.");
            }

            if (!upgradesById.TryGetValue(upgradeId, out UpgradeProgress progress))
            {
                progress = new UpgradeProgress(upgradeId);
                upgrades.Add(progress);
                upgradesById.Add(upgradeId, progress);
            }

            progress.ApplyPurchase(costMinorUnits);
            TotalSpentMinorUnits = checked(TotalSpentMinorUnits + costMinorUnits);
        }
    }
}
