using System;

namespace StockMarket.Domain.Progression
{
    public sealed class UpgradeSpec
    {
        public UpgradeSpec(
            string id,
            int maxLevel,
            long baseCostMinorUnits,
            int costGrowthBasisPoints,
            UpgradeEffectType effectType,
            double effectAmountPerLevel)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Upgrade ID is required.", nameof(id));
            }

            if (maxLevel <= 0 || maxLevel > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevel));
            }

            if (baseCostMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCostMinorUnits));
            }

            if (costGrowthBasisPoints < 10000 || costGrowthBasisPoints > 1000000)
            {
                throw new ArgumentOutOfRangeException(nameof(costGrowthBasisPoints));
            }

            if (!Enum.IsDefined(typeof(UpgradeEffectType), effectType))
            {
                throw new ArgumentOutOfRangeException(nameof(effectType));
            }

            if (double.IsNaN(effectAmountPerLevel) || double.IsInfinity(effectAmountPerLevel) ||
                effectAmountPerLevel <= 0d || effectAmountPerLevel > 1000d)
            {
                throw new ArgumentOutOfRangeException(nameof(effectAmountPerLevel));
            }

            Id = id;
            MaxLevel = maxLevel;
            BaseCostMinorUnits = baseCostMinorUnits;
            CostGrowthBasisPoints = costGrowthBasisPoints;
            EffectType = effectType;
            EffectAmountPerLevel = effectAmountPerLevel;
        }

        public string Id { get; }
        public int MaxLevel { get; }
        public long BaseCostMinorUnits { get; }
        public int CostGrowthBasisPoints { get; }
        public UpgradeEffectType EffectType { get; }
        public double EffectAmountPerLevel { get; }

        public long GetCostForCurrentLevel(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= MaxLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLevel));
            }

            decimal cost = BaseCostMinorUnits;
            decimal growth = (decimal)CostGrowthBasisPoints / 10000m;

            for (int level = 0; level < currentLevel; level++)
            {
                cost = decimal.Round(cost * growth, 0, MidpointRounding.AwayFromZero);

                if (cost > long.MaxValue)
                {
                    throw new OverflowException("Upgrade cost exceeds fixed-point currency limits.");
                }
            }

            return checked((long)cost);
        }
    }
}
