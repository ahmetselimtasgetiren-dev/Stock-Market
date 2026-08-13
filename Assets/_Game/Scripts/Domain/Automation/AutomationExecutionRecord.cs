using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Automation
{
    public readonly struct AutomationExecutionRecord
    {
        internal AutomationExecutionRecord(
            long executionId,
            long ruleId,
            long tick,
            TradeResult tradeResult)
        {
            ExecutionId = executionId;
            RuleId = ruleId;
            Tick = tick;
            TradeResult = tradeResult;
        }

        public long ExecutionId { get; }
        public long RuleId { get; }
        public long Tick { get; }
        public TradeResult TradeResult { get; }
    }
}
