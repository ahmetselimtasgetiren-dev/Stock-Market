using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Unlocks
{
    public sealed class MarketAccessState
    {
        private readonly HashSet<string> unlockedSectorIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> unlockedCompanyIds = new HashSet<string>(StringComparer.Ordinal);

        public MarketAccessState(
            IEnumerable<string> initiallyUnlockedSectorIds = null,
            IEnumerable<string> initiallyUnlockedCompanyIds = null)
        {
            AddSeeds(initiallyUnlockedSectorIds, unlockedSectorIds, nameof(initiallyUnlockedSectorIds));
            AddSeeds(initiallyUnlockedCompanyIds, unlockedCompanyIds, nameof(initiallyUnlockedCompanyIds));
        }

        public int UnlockedSectorCount => unlockedSectorIds.Count;
        public int UnlockedCompanyCount => unlockedCompanyIds.Count;

        public bool IsSectorUnlocked(string sectorId)
        {
            return sectorId != null && unlockedSectorIds.Contains(sectorId);
        }

        public bool IsCompanyUnlocked(string companyId)
        {
            return companyId != null && unlockedCompanyIds.Contains(companyId);
        }

        internal void Unlock(UnlockTargetScope scope, string targetId)
        {
            HashSet<string> targets = scope == UnlockTargetScope.Sector
                ? unlockedSectorIds
                : unlockedCompanyIds;

            if (!targets.Add(targetId))
            {
                throw new InvalidOperationException($"Target '{targetId}' is already unlocked.");
            }
        }

        private static void AddSeeds(
            IEnumerable<string> seeds,
            HashSet<string> destination,
            string parameterName)
        {
            if (seeds == null)
            {
                return;
            }

            foreach (string id in seeds)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Initial unlock IDs cannot be empty.", parameterName);
                }

                destination.Add(id);
            }
        }
    }
}
