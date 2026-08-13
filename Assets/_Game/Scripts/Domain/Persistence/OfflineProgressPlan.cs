namespace StockMarket.Domain.Persistence
{
    public readonly struct OfflineProgressPlan
    {
        public OfflineProgressPlan(long elapsedSeconds, long simulatedSeconds, long ticks, bool wasCapped)
        {
            ElapsedSeconds = elapsedSeconds;
            SimulatedSeconds = simulatedSeconds;
            Ticks = ticks;
            WasCapped = wasCapped;
        }

        public long ElapsedSeconds { get; }
        public long SimulatedSeconds { get; }
        public long Ticks { get; }
        public bool WasCapped { get; }
    }
}
