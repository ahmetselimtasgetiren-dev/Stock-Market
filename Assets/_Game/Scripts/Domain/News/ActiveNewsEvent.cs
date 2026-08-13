using System;

namespace StockMarket.Domain.News
{
    public sealed class ActiveNewsEvent
    {
        internal ActiveNewsEvent(long instanceId, NewsEventDefinition definition, long startTick)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            StartTick = startTick;
            EndTickExclusive = checked(startTick + definition.DurationTicks);
        }

        public long InstanceId { get; }
        public NewsEventDefinition Definition { get; }
        public long StartTick { get; }
        public long EndTickExclusive { get; }

        public bool IsEffectiveAt(long tick)
        {
            return tick >= StartTick && tick < EndTickExclusive;
        }
    }
}
