using System;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Automation
{
    public sealed class AutomationExecutionLedger
    {
        private readonly AutomationExecutionRecord[] records;
        private int oldestIndex;
        private long nextExecutionId = 1;

        public AutomationExecutionLedger(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            records = new AutomationExecutionRecord[capacity];
        }

        public int Capacity => records.Length;
        public int Count { get; private set; }
        public AutomationExecutionRecord Oldest => this[0];
        public AutomationExecutionRecord Latest => this[Count - 1];

        public AutomationExecutionRecord this[int chronologicalIndex]
        {
            get
            {
                if (chronologicalIndex < 0 || chronologicalIndex >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(chronologicalIndex));
                }

                return records[(oldestIndex + chronologicalIndex) % records.Length];
            }
        }

        internal bool CanAppend => nextExecutionId < long.MaxValue;

        internal void Append(long ruleId, long tick, TradeResult tradeResult)
        {
            if (!CanAppend)
            {
                throw new InvalidOperationException("Automation execution IDs are exhausted.");
            }

            var record = new AutomationExecutionRecord(nextExecutionId, ruleId, tick, tradeResult);

            if (Count < records.Length)
            {
                records[(oldestIndex + Count) % records.Length] = record;
                Count++;
            }
            else
            {
                records[oldestIndex] = record;
                oldestIndex = (oldestIndex + 1) % records.Length;
            }

            nextExecutionId++;
        }
    }
}
