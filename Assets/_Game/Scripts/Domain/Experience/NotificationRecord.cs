namespace StockMarket.Domain.Experience
{
    public sealed class NotificationRecord
    {
        internal NotificationRecord(long id, FeedbackType type, string messageKey, long amountMinorUnits)
        {
            Id = id;
            Type = type;
            MessageKey = messageKey;
            AmountMinorUnits = amountMinorUnits;
        }

        public long Id { get; }
        public FeedbackType Type { get; }
        public string MessageKey { get; }
        public long AmountMinorUnits { get; }
        public bool IsRead { get; private set; }
        internal void MarkRead() => IsRead = true;
    }
}
