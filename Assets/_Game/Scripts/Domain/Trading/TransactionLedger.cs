using System;

namespace StockMarket.Domain.Trading
{
    public sealed class TransactionLedger
    {
        private readonly TransactionRecord[] records;
        private int oldestIndex;
        private long nextTransactionId = 1;

        public TransactionLedger(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Transaction capacity must be positive.");
            }

            records = new TransactionRecord[capacity];
        }

        public int Capacity => records.Length;
        public int Count { get; private set; }
        public bool CanAppend => nextTransactionId < long.MaxValue;
        internal long NextTransactionId => nextTransactionId;

        public TransactionRecord Oldest => this[0];
        public TransactionRecord Latest => this[Count - 1];

        public TransactionRecord this[int chronologicalIndex]
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

        internal void Append(TradeResult result)
        {
            if (!result.Succeeded || result.TransactionId != nextTransactionId || !CanAppend)
            {
                throw new InvalidOperationException("Transaction result cannot be appended to this ledger.");
            }

            var record = new TransactionRecord(result);

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

            nextTransactionId++;
        }
    }
}
