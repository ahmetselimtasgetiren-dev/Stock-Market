using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Experience
{
    public sealed class AchievementService
    {
        private readonly AchievementSpec[] specs;
        private readonly HashSet<string> earned = new HashSet<string>(StringComparer.Ordinal);

        public AchievementService(IEnumerable<AchievementSpec> specs, IEnumerable<string> initiallyEarned = null)
        {
            if (specs == null) throw new ArgumentNullException(nameof(specs));
            var list = new List<AchievementSpec>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (AchievementSpec spec in specs)
            {
                if (spec == null || !ids.Add(spec.Id)) throw new ArgumentException("Achievements are missing or duplicated.");
                list.Add(spec);
            }
            this.specs = list.ToArray();
            if (initiallyEarned != null)
                foreach (string id in initiallyEarned) earned.Add(id);
        }

        public int EarnedCount => earned.Count;
        public bool IsEarned(string id) => id != null && earned.Contains(id);

        public IReadOnlyList<string> Evaluate(AchievementMetric metric, long value)
        {
            var newlyEarned = new List<string>();
            for (int index = 0; index < specs.Length; index++)
            {
                AchievementSpec spec = specs[index];
                if (spec.Metric == metric && value >= spec.Threshold && earned.Add(spec.Id))
                    newlyEarned.Add(spec.Id);
            }
            return newlyEarned.AsReadOnly();
        }
    }
}
