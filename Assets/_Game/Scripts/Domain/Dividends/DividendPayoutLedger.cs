using System;

namespace StockMarket.Domain.Dividends
{
    public sealed class DividendPayoutLedger
    {
        private readonly DividendPayoutRecord[] records;
        private int oldestIndex;
        private long nextPayoutId = 1;

        public DividendPayoutLedger(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            records = new DividendPayoutRecord[capacity];
        }

        public int Capacity => records.Length;
        public int Count { get; private set; }
        public DividendPayoutRecord Oldest => this[0];
        public DividendPayoutRecord Latest => this[Count - 1];

        public DividendPayoutRecord this[int chronologicalIndex]
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

        internal bool CanAppend(int recordCount)
        {
            return recordCount >= 0 && nextPayoutId <= long.MaxValue - recordCount;
        }

        internal long NextPayoutId => nextPayoutId;

        internal void Append(DividendPayoutRecord record)
        {
            if (record.PayoutId != nextPayoutId || !CanAppend(1))
            {
                throw new InvalidOperationException("Dividend payout cannot be appended to this ledger.");
            }

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

            nextPayoutId++;
        }
    }
}
