using System;

namespace StockMarket.Domain.News
{
    public sealed class NewsEventDefinition
    {
        public NewsEventDefinition(
            string id,
            NewsTargetScope targetScope,
            string targetId,
            double priceImpactPerTick,
            int durationTicks)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("News ID is required.", nameof(id));
            }

            if (!Enum.IsDefined(typeof(NewsTargetScope), targetScope))
            {
                throw new ArgumentOutOfRangeException(nameof(targetScope));
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("News target ID is required.", nameof(targetId));
            }

            if (double.IsNaN(priceImpactPerTick) || double.IsInfinity(priceImpactPerTick) ||
                priceImpactPerTick < -0.25d || priceImpactPerTick > 0.25d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(priceImpactPerTick),
                    "News impact must be finite and between -0.25 and 0.25.");
            }

            if (durationTicks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationTicks), "Duration must be positive.");
            }

            Id = id;
            TargetScope = targetScope;
            TargetId = targetId;
            PriceImpactPerTick = priceImpactPerTick;
            DurationTicks = durationTicks;
        }

        public string Id { get; }
        public NewsTargetScope TargetScope { get; }
        public string TargetId { get; }
        public double PriceImpactPerTick { get; }
        public int DurationTicks { get; }
    }
}
