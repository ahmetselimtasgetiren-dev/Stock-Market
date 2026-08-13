using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "SectorCatalog", menuName = "Stock Market/Catalogs/Sectors")]
    public sealed class SectorCatalog : ScriptableObject
    {
        [SerializeField]
        private List<SectorDefinition> sectors = new List<SectorDefinition>();

        public IReadOnlyList<SectorDefinition> Sectors => sectors;

        public bool TryGetById(string id, out SectorDefinition definition)
        {
            for (int index = 0; index < sectors.Count; index++)
            {
                SectorDefinition candidate = sectors[index];

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

            for (int index = 0; index < sectors.Count; index++)
            {
                SectorDefinition sector = sectors[index];

                if (sector == null)
                {
                    errors.Add($"Sector catalog entry {index} is missing.");
                    continue;
                }

                sector.CollectValidationErrors(errors);

                if (!knownIds.Add(sector.Id))
                {
                    errors.Add($"Sector catalog contains duplicate ID '{sector.Id}'.");
                }
            }
        }
    }
}
