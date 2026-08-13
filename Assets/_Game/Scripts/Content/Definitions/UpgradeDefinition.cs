using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Stock Market/Definitions/Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField, TextArea]
        private string description = string.Empty;

        [SerializeField, Min(1)]
        private int maxLevel = 1;

        [SerializeField, Min(1)]
        private long baseCostMinorUnits = 100;

        [SerializeField, Min(10000)]
        private int costGrowthBasisPoints = 15000;

        [SerializeField]
        private UpgradeEffectType effectType;

        [SerializeField, Min(0.0001f)]
        private float effectAmountPerLevel = 0.05f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public int MaxLevel => maxLevel;
        public long BaseCostMinorUnits => baseCostMinorUnits;
        public int CostGrowthBasisPoints => costGrowthBasisPoints;
        public UpgradeEffectType EffectType => effectType;
        public float EffectAmountPerLevel => effectAmountPerLevel;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"Upgrade '{name}': {idError}");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"Upgrade '{name}': Display name is required.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add($"Upgrade '{name}': Description is required.");
            }

            if (maxLevel <= 0 || maxLevel > 1000)
            {
                errors.Add($"Upgrade '{name}': Max level must be between 1 and 1000.");
            }

            if (baseCostMinorUnits <= 0)
            {
                errors.Add($"Upgrade '{name}': Base cost must be positive.");
            }

            if (costGrowthBasisPoints < 10000 || costGrowthBasisPoints > 1000000)
            {
                errors.Add($"Upgrade '{name}': Cost growth must be between 10000 and 1000000 basis points.");
            }

            if (float.IsNaN(effectAmountPerLevel) || float.IsInfinity(effectAmountPerLevel) ||
                effectAmountPerLevel <= 0f || effectAmountPerLevel > 1000f)
            {
                errors.Add($"Upgrade '{name}': Effect per level must be finite, positive, and no more than 1000.");
            }
        }
    }
}
