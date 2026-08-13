using System;

namespace StockMarket.Domain.Experience
{
    public sealed class AchievementSpec
    {
        public AchievementSpec(string id, AchievementMetric metric, long threshold)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (!Enum.IsDefined(typeof(AchievementMetric), metric) || threshold <= 0)
                throw new ArgumentOutOfRangeException();
            Id = id;
            Metric = metric;
            Threshold = threshold;
        }
        public string Id { get; }
        public AchievementMetric Metric { get; }
        public long Threshold { get; }
    }
}
