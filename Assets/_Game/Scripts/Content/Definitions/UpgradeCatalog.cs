using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "UpgradeCatalog", menuName = "Stock Market/Catalogs/Upgrades")]
    public sealed class UpgradeCatalog : ScriptableObject
    {
        [SerializeField]
        private List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();

        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;

        public bool TryGetById(string id, out UpgradeDefinition definition)
        {
            for (int index = 0; index < upgrades.Count; index++)
            {
                UpgradeDefinition candidate = upgrades[index];

                if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < upgrades.Count; index++)
            {
                UpgradeDefinition upgrade = upgrades[index];

                if (upgrade == null)
                {
                    errors.Add($"Upgrade catalog entry {index} is missing.");
                    continue;
                }

                upgrade.CollectValidationErrors(errors);

                if (!knownIds.Add(upgrade.Id))
                {
                    errors.Add($"Upgrade catalog contains duplicate ID '{upgrade.Id}'.");
                }
            }
        }
    }
}
