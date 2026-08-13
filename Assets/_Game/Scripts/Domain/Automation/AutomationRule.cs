using System;

namespace StockMarket.Domain.Automation
{
    public sealed class AutomationRule
    {
        internal AutomationRule(
            long ruleId,
            string companyId,
            AutomationCondition condition,
            long triggerPriceMinorUnits,
            long quantity,
            long cooldownTicks)
        {
            RuleId = ruleId;
            CompanyId = companyId;
            Condition = condition;
            TriggerPriceMinorUnits = triggerPriceMinorUnits;
            Quantity = quantity;
            CooldownTicks = cooldownTicks;
            IsEnabled = true;
        }

        public long RuleId { get; }
        public string CompanyId { get; }
        public AutomationCondition Condition { get; }
        public long TriggerPriceMinorUnits { get; }
        public long Quantity { get; }
        public long CooldownTicks { get; }
        public bool IsEnabled { get; private set; }
        public long LastAttemptTick { get; private set; }
        public long NextEligibleTick { get; private set; }

        internal void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        internal void RecordAttempt(long tick)
        {
            LastAttemptTick = tick;
            NextEligibleTick = tick > long.MaxValue - CooldownTicks
                ? long.MaxValue
                : tick + CooldownTicks;
        }
    }
}
