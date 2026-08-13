using System;
using System.Collections.Generic;
using StockMarket.Domain.Portfolio;

namespace StockMarket.Domain.Progression
{
    public sealed class UpgradeService
    {
        private readonly PlayerFinancialState player;
        private readonly ProgressionState progression;
        private readonly Dictionary<string, UpgradeSpec> specsById =
            new Dictionary<string, UpgradeSpec>(StringComparer.Ordinal);

        public UpgradeService(
            PlayerFinancialState player,
            ProgressionState progression,
            IEnumerable<UpgradeSpec> specs)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.progression = progression ?? throw new ArgumentNullException(nameof(progression));

            if (specs == null)
            {
                throw new ArgumentNullException(nameof(specs));
            }

            foreach (UpgradeSpec spec in specs)
            {
                if (spec == null)
                {
                    throw new ArgumentException("Upgrade specs contain a missing entry.", nameof(specs));
                }

                if (!specsById.TryAdd(spec.Id, spec))
                {
                    throw new ArgumentException($"Duplicate upgrade ID '{spec.Id}'.", nameof(specs));
                }
            }
        }

        public ProgressionState Progression => progression;

        public UpgradePurchaseResult Purchase(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return UpgradePurchaseResult.Failed(upgradeId, UpgradePurchaseFailure.InvalidUpgradeId);
            }

            if (!specsById.TryGetValue(upgradeId, out UpgradeSpec spec))
            {
                return UpgradePurchaseResult.Failed(upgradeId, UpgradePurchaseFailure.UnknownUpgrade);
            }

            int currentLevel = progression.GetLevel(upgradeId);

            if (currentLevel >= spec.MaxLevel)
            {
                return UpgradePurchaseResult.Failed(upgradeId, UpgradePurchaseFailure.MaximumLevelReached);
            }

            long cost;

            try
            {
                cost = spec.GetCostForCurrentLevel(currentLevel);
            }
            catch (OverflowException)
            {
                return UpgradePurchaseResult.Failed(upgradeId, UpgradePurchaseFailure.ArithmeticOverflow);
            }

            if (cost > player.CashMinorUnits)
            {
                return UpgradePurchaseResult.Failed(
                    upgradeId,
                    UpgradePurchaseFailure.InsufficientCash,
                    cost);
            }

            if (!progression.CanApplyPurchase(upgradeId, cost))
            {
                return UpgradePurchaseResult.Failed(
                    upgradeId,
                    UpgradePurchaseFailure.ArithmeticOverflow,
                    cost);
            }

            player.TryDebitCash(cost);
            progression.ApplyPurchase(upgradeId, cost);
            return UpgradePurchaseResult.Success(
                upgradeId,
                currentLevel + 1,
                cost,
                player.CashMinorUnits);
        }

        public long GetNextCost(string upgradeId)
        {
            if (!specsById.TryGetValue(upgradeId, out UpgradeSpec spec))
            {
                throw new KeyNotFoundException($"No upgrade exists for ID '{upgradeId}'.");
            }

            return spec.GetCostForCurrentLevel(progression.GetLevel(upgradeId));
        }

        public double GetEffectTotal(UpgradeEffectType effectType)
        {
            double total = 0d;

            foreach (UpgradeProgress progress in progression.Upgrades)
            {
                UpgradeSpec spec = specsById[progress.UpgradeId];

                if (spec.EffectType == effectType)
                {
                    total += spec.EffectAmountPerLevel * progress.Level;
                }
            }

            return total;
        }
    }
}
