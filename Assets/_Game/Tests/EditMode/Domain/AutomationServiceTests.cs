using System;
using NUnit.Framework;
using StockMarket.Domain.Automation;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Tests
{
    public sealed class AutomationServiceTests
    {
        [Test]
        public void ProcessTick_BuyThresholdMet_UsesOrdinaryTradingPath()
        {
            AutomationService automation = CreateAutomation(
                startingCash: 1000,
                capacity: 1,
                out PlayerFinancialState player,
                out _);
            AutomationRuleResult added = automation.AddRule(
                "quillbyte",
                AutomationCondition.BuyAtOrBelow,
                triggerPriceMinorUnits: 100,
                quantity: 2,
                cooldownTicks: 1);

            AutomationTickResult result = automation.ProcessTick(1);

            Assert.That(added.Succeeded, Is.True);
            Assert.That(result.AttemptedTrades, Is.EqualTo(1));
            Assert.That(result.SuccessfulTrades, Is.EqualTo(1));
            Assert.That(player.CashMinorUnits, Is.EqualTo(800));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte"), Is.EqualTo(2));
            Assert.That(automation.Ledger.Latest.TradeResult.TradeType, Is.EqualTo(TradeType.Buy));
        }

        [Test]
        public void ProcessTick_SellThresholdMet_SellsOwnedShares()
        {
            AutomationService automation = CreateAutomation(1000, 1, out PlayerFinancialState player, out _);
            player.Portfolio.AddPurchasedShares("quillbyte", 3, 300);
            automation.AddRule("quillbyte", AutomationCondition.SellAtOrAbove, 100, 2, 1);

            AutomationTickResult result = automation.ProcessTick(1);

            Assert.That(result.SuccessfulTrades, Is.EqualTo(1));
            Assert.That(player.CashMinorUnits, Is.EqualTo(1200));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte"), Is.EqualTo(1));
        }

        [Test]
        public void ProcessTick_WhenThresholdIsNotMet_DoesNothing()
        {
            AutomationService automation = CreateAutomation(1000, 1, out PlayerFinancialState player, out _);
            automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 99, 1, 1);

            AutomationTickResult result = automation.ProcessTick(1);

            Assert.That(result.AttemptedTrades, Is.Zero);
            Assert.That(player.CashMinorUnits, Is.EqualTo(1000));
            Assert.That(automation.Ledger.Count, Is.Zero);
        }

        [Test]
        public void ProcessTick_CooldownPreventsRepeatedAttemptsUntilEligibleTick()
        {
            AutomationService automation = CreateAutomation(1000, 1, out _, out _);
            AutomationRule rule = automation.AddRule(
                "quillbyte",
                AutomationCondition.BuyAtOrBelow,
                100,
                1,
                cooldownTicks: 2).Rule;

            AutomationTickResult first = automation.ProcessTick(1);
            AutomationTickResult second = automation.ProcessTick(2);
            AutomationTickResult third = automation.ProcessTick(3);

            Assert.That(first.AttemptedTrades, Is.EqualTo(1));
            Assert.That(second.AttemptedTrades, Is.Zero);
            Assert.That(third.AttemptedTrades, Is.EqualTo(1));
            Assert.That(rule.LastAttemptTick, Is.EqualTo(3));
            Assert.That(rule.NextEligibleTick, Is.EqualTo(5));
        }

        [Test]
        public void FailedTrade_IsRecordedAndStillStartsCooldown()
        {
            AutomationService automation = CreateAutomation(50, 1, out PlayerFinancialState player, out _);
            AutomationRule rule = automation.AddRule(
                "quillbyte",
                AutomationCondition.BuyAtOrBelow,
                100,
                1,
                3).Rule;

            AutomationTickResult result = automation.ProcessTick(1);

            Assert.That(result.AttemptedTrades, Is.EqualTo(1));
            Assert.That(result.SuccessfulTrades, Is.Zero);
            Assert.That(automation.Ledger.Latest.TradeResult.FailureReason, Is.EqualTo(TradeFailureReason.InsufficientCash));
            Assert.That(rule.NextEligibleTick, Is.EqualTo(4));
            Assert.That(player.CashMinorUnits, Is.EqualTo(50));
        }

        [Test]
        public void DisabledRule_DoesNotExecuteUntilEnabled()
        {
            AutomationService automation = CreateAutomation(1000, 1, out _, out _);
            AutomationRule rule = automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 100, 1, 1).Rule;
            Assert.That(automation.SetRuleEnabled(rule.RuleId, false), Is.True);

            AutomationTickResult disabled = automation.ProcessTick(1);
            automation.SetRuleEnabled(rule.RuleId, true);
            AutomationTickResult enabled = automation.ProcessTick(2);

            Assert.That(disabled.AttemptedTrades, Is.Zero);
            Assert.That(enabled.SuccessfulTrades, Is.EqualTo(1));
        }

        [Test]
        public void AddRule_EnforcesCapacityAndCapacityCanGrowExplicitly()
        {
            AutomationService automation = CreateAutomation(1000, 1, out _, out _);
            Assert.That(automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 100, 1, 1).Succeeded, Is.True);

            AutomationRuleResult blocked = automation.AddRule("quillbyte", AutomationCondition.SellAtOrAbove, 100, 1, 1);
            automation.SetCapacity(2);
            AutomationRuleResult added = automation.AddRule("quillbyte", AutomationCondition.SellAtOrAbove, 100, 1, 1);

            Assert.That(blocked.Failure, Is.EqualTo(AutomationRuleFailure.CapacityReached));
            Assert.That(added.Succeeded, Is.True);
            Assert.That(automation.Rules, Has.Count.EqualTo(2));
        }

        [Test]
        public void AddRule_ValidatesCompanyAndNumericInputs()
        {
            AutomationService automation = CreateAutomation(1000, 5, out _, out _);

            Assert.That(automation.AddRule("unknown", AutomationCondition.BuyAtOrBelow, 1, 1, 1).Failure,
                Is.EqualTo(AutomationRuleFailure.UnknownCompany));
            Assert.That(automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 0, 1, 1).Failure,
                Is.EqualTo(AutomationRuleFailure.InvalidTriggerPrice));
            Assert.That(automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 1, 0, 1).Failure,
                Is.EqualTo(AutomationRuleFailure.InvalidQuantity));
            Assert.That(automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 1, 1, 0).Failure,
                Is.EqualTo(AutomationRuleFailure.InvalidCooldown));
        }

        [Test]
        public void ProcessTick_MustBeSequentialFromConfiguredStartingTick()
        {
            AutomationService automation = CreateAutomation(1000, 0, out _, out _, startingTick: 10);

            Assert.Throws<ArgumentOutOfRangeException>(() => automation.ProcessTick(12));
            Assert.That(automation.LastProcessedTick, Is.EqualTo(10));
            Assert.That(automation.ProcessTick(11).Tick, Is.EqualTo(11));
        }

        [Test]
        public void ExecutionLedger_WhenFull_DropsOldestRecord()
        {
            AutomationService automation = CreateAutomation(
                1000,
                1,
                out _,
                out _,
                ledger: new AutomationExecutionLedger(2));
            automation.AddRule("quillbyte", AutomationCondition.BuyAtOrBelow, 100, 1, 1);

            automation.ProcessTick(1);
            automation.ProcessTick(2);
            automation.ProcessTick(3);

            Assert.That(automation.Ledger.Count, Is.EqualTo(2));
            Assert.That(automation.Ledger.Oldest.ExecutionId, Is.EqualTo(2));
            Assert.That(automation.Ledger.Latest.ExecutionId, Is.EqualTo(3));
        }

        private static AutomationService CreateAutomation(
            long startingCash,
            int capacity,
            out PlayerFinancialState player,
            out MarketState market,
            long startingTick = 0,
            AutomationExecutionLedger ledger = null)
        {
            player = new PlayerFinancialState(startingCash);
            market = new MarketState(
                new[] { new CompanyMarketSeed("quillbyte", 100) },
                historyCapacity: 10);
            var trading = new TradingService(player, market);
            return new AutomationService(market, trading, capacity, startingTick, ledger);
        }
    }
}
