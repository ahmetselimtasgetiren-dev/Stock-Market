using System;
using System.Collections.Generic;
using StockMarket.Domain.Market;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Automation
{
    /// <summary>
    /// Evaluates player-authored trading rules once per sequential market tick.
    /// </summary>
    public sealed class AutomationService
    {
        private readonly MarketState market;
        private readonly TradingService trading;
        private readonly List<AutomationRule> rules = new List<AutomationRule>();
        private readonly IReadOnlyList<AutomationRule> readOnlyRules;
        private long nextRuleId = 1;

        public AutomationService(
            MarketState market,
            TradingService trading,
            int capacity,
            long startingTick = 0,
            AutomationExecutionLedger ledger = null)
        {
            this.market = market ?? throw new ArgumentNullException(nameof(market));
            this.trading = trading ?? throw new ArgumentNullException(nameof(trading));

            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (startingTick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingTick));
            }

            Capacity = capacity;
            LastProcessedTick = startingTick;
            Ledger = ledger ?? new AutomationExecutionLedger(100);
            readOnlyRules = rules.AsReadOnly();
        }

        public int Capacity { get; private set; }
        public long LastProcessedTick { get; private set; }
        public IReadOnlyList<AutomationRule> Rules => readOnlyRules;
        public AutomationExecutionLedger Ledger { get; }

        public void SetCapacity(int capacity)
        {
            if (capacity < rules.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    "Capacity cannot be lower than the current rule count.");
            }

            Capacity = capacity;
        }

        public AutomationRuleResult AddRule(
            string companyId,
            AutomationCondition condition,
            long triggerPriceMinorUnits,
            long quantity,
            long cooldownTicks)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.InvalidCompanyId);
            }

            if (!market.TryGetCompany(companyId, out _))
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.UnknownCompany);
            }

            if (!Enum.IsDefined(typeof(AutomationCondition), condition))
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.InvalidCondition);
            }

            if (triggerPriceMinorUnits <= 0)
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.InvalidTriggerPrice);
            }

            if (quantity <= 0)
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.InvalidQuantity);
            }

            if (cooldownTicks <= 0)
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.InvalidCooldown);
            }

            if (rules.Count >= Capacity)
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.CapacityReached);
            }

            if (nextRuleId == long.MaxValue)
            {
                return AutomationRuleResult.Failed(AutomationRuleFailure.RuleIdExhausted);
            }

            var rule = new AutomationRule(
                nextRuleId,
                companyId,
                condition,
                triggerPriceMinorUnits,
                quantity,
                cooldownTicks);
            nextRuleId++;
            rules.Add(rule);
            return AutomationRuleResult.Success(rule);
        }

        public bool RemoveRule(long ruleId)
        {
            for (int index = 0; index < rules.Count; index++)
            {
                if (rules[index].RuleId == ruleId)
                {
                    rules.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public bool SetRuleEnabled(long ruleId, bool enabled)
        {
            for (int index = 0; index < rules.Count; index++)
            {
                if (rules[index].RuleId == ruleId)
                {
                    rules[index].SetEnabled(enabled);
                    return true;
                }
            }

            return false;
        }

        public AutomationTickResult ProcessTick(long tick)
        {
            if (tick != LastProcessedTick + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "Automation ticks must be sequential.");
            }

            int attempts = 0;
            int successes = 0;

            for (int index = 0; index < rules.Count; index++)
            {
                AutomationRule rule = rules[index];

                if (!rule.IsEnabled || tick < rule.NextEligibleTick)
                {
                    continue;
                }

                CompanyMarketState company = market.GetCompany(rule.CompanyId);
                bool conditionMet = rule.Condition == AutomationCondition.BuyAtOrBelow
                    ? company.CurrentPriceMinorUnits <= rule.TriggerPriceMinorUnits
                    : company.CurrentPriceMinorUnits >= rule.TriggerPriceMinorUnits;

                if (!conditionMet)
                {
                    continue;
                }

                if (!Ledger.CanAppend)
                {
                    throw new InvalidOperationException("Automation execution ledger is exhausted.");
                }

                TradeResult tradeResult = rule.Condition == AutomationCondition.BuyAtOrBelow
                    ? trading.Buy(rule.CompanyId, rule.Quantity)
                    : trading.Sell(rule.CompanyId, rule.Quantity);
                rule.RecordAttempt(tick);
                Ledger.Append(rule.RuleId, tick, tradeResult);
                attempts++;

                if (tradeResult.Succeeded)
                {
                    successes++;
                }
            }

            LastProcessedTick = tick;
            return new AutomationTickResult(tick, attempts, successes);
        }
    }
}
