using System;

namespace StockMarket.Domain.Experience
{
    public sealed class NotificationCenter
    {
        private readonly NotificationRecord[] records;
        private int oldestIndex;
        private long nextId = 1;

        public NotificationCenter(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            records = new NotificationRecord[capacity];
        }

        public int Count { get; private set; }
        public int UnreadCount { get; private set; }
        public NotificationRecord Latest => this[Count - 1];
        public NotificationRecord this[int index] =>
            index < 0 || index >= Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : records[(oldestIndex + index) % records.Length];

        public NotificationRecord Publish(FeedbackType type, string messageKey, long amountMinorUnits = 0)
        {
            if (string.IsNullOrWhiteSpace(messageKey)) throw new ArgumentException(nameof(messageKey));
            if (nextId == long.MaxValue) throw new InvalidOperationException("Notification IDs exhausted.");
            var record = new NotificationRecord(nextId++, type, messageKey, amountMinorUnits);

            if (Count < records.Length)
            {
                records[(oldestIndex + Count) % records.Length] = record;
                Count++;
            }
            else
            {
                if (!records[oldestIndex].IsRead) UnreadCount--;
                records[oldestIndex] = record;
                oldestIndex = (oldestIndex + 1) % records.Length;
            }

            UnreadCount++;
            return record;
        }

        public bool MarkRead(long id)
        {
            for (int index = 0; index < Count; index++)
            {
                NotificationRecord record = this[index];
                if (record.Id == id && !record.IsRead)
                {
                    record.MarkRead();
                    UnreadCount--;
                    return true;
                }
            }

            return false;
        }
    }
}
