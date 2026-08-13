namespace StockMarket.Domain.Automation
{
    public readonly struct AutomationTickResult
    {
        public AutomationTickResult(long tick, int attemptedTrades, int successfulTrades)
        {
            Tick = tick;
            AttemptedTrades = attemptedTrades;
            SuccessfulTrades = successfulTrades;
        }

        public long Tick { get; }
        public int AttemptedTrades { get; }
        public int SuccessfulTrades { get; }
    }
}
