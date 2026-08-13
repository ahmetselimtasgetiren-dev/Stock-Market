using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "UnlockCatalog", menuName = "Stock Market/Catalogs/Unlocks")]
    public sealed class UnlockCatalog : ScriptableObject
    {
        [SerializeField]
        private List<UnlockDefinition> unlocks = new List<UnlockDefinition>();

        public IReadOnlyList<UnlockDefinition> Unlocks => unlocks;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            var knownTargets = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < unlocks.Count; index++)
            {
                UnlockDefinition unlock = unlocks[index];

                if (unlock == null)
                {
                    errors.Add($"Unlock catalog entry {index} is missing.");
                    continue;
                }

                unlock.CollectValidationErrors(errors);

                if (!knownIds.Add(unlock.Id))
                {
                    errors.Add($"Unlock catalog contains duplicate ID '{unlock.Id}'.");
                }

                string targetKey = $"{unlock.TargetType}:{unlock.TargetId}";

                if (!string.IsNullOrEmpty(unlock.TargetId) && !knownTargets.Add(targetKey))
                {
                    errors.Add($"Unlock catalog contains more than one offer for {targetKey}.");
                }
            }
        }
    }
}
