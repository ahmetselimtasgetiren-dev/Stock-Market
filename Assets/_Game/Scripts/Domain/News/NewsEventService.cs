using System;
using System.Collections.Generic;

namespace StockMarket.Domain.News
{
    /// <summary>
    /// Owns scheduled and active news instances for one playthrough.
    /// </summary>
    public sealed class NewsEventService
    {
        private readonly List<ActiveNewsEvent> events = new List<ActiveNewsEvent>();
        private readonly IReadOnlyList<ActiveNewsEvent> readOnlyEvents;
        private long nextInstanceId = 1;

        public NewsEventService()
        {
            readOnlyEvents = events.AsReadOnly();
        }

        public long CurrentTick { get; private set; }
        public IReadOnlyList<ActiveNewsEvent> Events => readOnlyEvents;

        public ActiveNewsEvent Activate(NewsEventDefinition definition, long startTick)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (startTick <= CurrentTick)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startTick),
                    "News must start after the latest processed tick.");
            }

            if (nextInstanceId == long.MaxValue)
            {
                throw new InvalidOperationException("No further news instance IDs are available.");
            }

            var activeEvent = new ActiveNewsEvent(nextInstanceId, definition, startTick);
            nextInstanceId++;
            events.Add(activeEvent);
            return activeEvent;
        }

        public void AdvanceToTick(long tick)
        {
            if (tick <= CurrentTick)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "News ticks must increase strictly.");
            }

            CurrentTick = tick;

            for (int index = events.Count - 1; index >= 0; index--)
            {
                if (events[index].EndTickExclusive <= tick)
                {
                    events.RemoveAt(index);
                }
            }
        }

        public double GetPriceImpact(string companyId, string sectorId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }

            double totalImpact = 0d;

            for (int index = 0; index < events.Count; index++)
            {
                ActiveNewsEvent activeEvent = events[index];

                if (!activeEvent.IsEffectiveAt(CurrentTick))
                {
                    continue;
                }

                NewsEventDefinition definition = activeEvent.Definition;
                bool matches = definition.TargetScope == NewsTargetScope.Company
                    ? string.Equals(definition.TargetId, companyId, StringComparison.Ordinal)
                    : string.Equals(definition.TargetId, sectorId, StringComparison.Ordinal);

                if (matches)
                {
                    totalImpact += definition.PriceImpactPerTick;
                }
            }

            return totalImpact;
        }
    }
}
