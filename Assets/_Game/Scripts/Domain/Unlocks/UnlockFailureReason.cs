namespace StockMarket.Domain.Unlocks
{
    public enum UnlockFailureReason
    {
        None = 0,
        InvalidUnlockId = 1,
        UnknownUnlock = 2,
        AlreadyUnlocked = 3,
        RequiredSectorLocked = 4,
        InsufficientCash = 5
    }
}
