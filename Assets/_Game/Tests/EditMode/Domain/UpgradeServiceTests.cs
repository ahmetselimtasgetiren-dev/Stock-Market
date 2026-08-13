using System;
using NUnit.Framework;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Progression;

namespace StockMarket.Domain.Tests
{
    public sealed class UpgradeServiceTests
    {
        [Test]
        public void Purchase_WhenAffordable_AtomicallyDebitsCashAndAddsLevel()
        {
            UpgradeService upgrades = CreateService(1000, out PlayerFinancialState player, out ProgressionState state);

            UpgradePurchaseResult result = upgrades.Purchase("dividend_research");

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.NewLevel, Is.EqualTo(1));
            Assert.That(result.CostMinorUnits, Is.EqualTo(100));
            Assert.That(result.CashAfterMinorUnits, Is.EqualTo(900));
            Assert.That(player.CashMinorUnits, Is.EqualTo(900));
            Assert.That(state.GetLevel("dividend_research"), Is.EqualTo(1));
            Assert.That(state.TotalSpentMinorUnits, Is.EqualTo(100));
        }

        [Test]
        public void Purchase_UsesDeterministicRoundedCostGrowth()
        {
            UpgradeService upgrades = CreateService(1000, out _, out _);

            Assert.That(upgrades.GetNextCost("dividend_research"), Is.EqualTo(100));
            upgrades.Purchase("dividend_research");
            Assert.That(upgrades.GetNextCost("dividend_research"), Is.EqualTo(150));
            upgrades.Purchase("dividend_research");
            Assert.That(upgrades.GetNextCost("dividend_research"), Is.EqualTo(225));
        }

        [Test]
        public void Purchase_WhenCashIsInsufficient_LeavesStateUnchanged()
        {
            UpgradeService upgrades = CreateService(99, out PlayerFinancialState player, out ProgressionState state);

            UpgradePurchaseResult result = upgrades.Purchase("dividend_research");

            Assert.That(result.Failure, Is.EqualTo(UpgradePurchaseFailure.InsufficientCash));
            Assert.That(result.CostMinorUnits, Is.EqualTo(100));
            Assert.That(player.CashMinorUnits, Is.EqualTo(99));
            Assert.That(state.GetLevel("dividend_research"), Is.Zero);
            Assert.That(state.TotalSpentMinorUnits, Is.Zero);
        }

        [Test]
        public void Purchase_WhenMaximumLevelIsReached_ReturnsFailureWithoutMutation()
        {
            UpgradeService upgrades = CreateService(1000, out PlayerFinancialState player, out ProgressionState state);
            upgrades.Purchase("dividend_research");
            upgrades.Purchase("dividend_research");
            upgrades.Purchase("dividend_research");
            long cashBefore = player.CashMinorUnits;

            UpgradePurchaseResult result = upgrades.Purchase("dividend_research");

            Assert.That(result.Failure, Is.EqualTo(UpgradePurchaseFailure.MaximumLevelReached));
            Assert.That(player.CashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(state.GetLevel("dividend_research"), Is.EqualTo(3));
        }

        [TestCase(null, UpgradePurchaseFailure.InvalidUpgradeId)]
        [TestCase("", UpgradePurchaseFailure.InvalidUpgradeId)]
        [TestCase("unknown", UpgradePurchaseFailure.UnknownUpgrade)]
        public void Purchase_WhenIdIsInvalid_ReturnsExpectedFailure(
            string upgradeId,
            UpgradePurchaseFailure expectedFailure)
        {
            UpgradeService upgrades = CreateService(1000, out PlayerFinancialState player, out ProgressionState state);

            UpgradePurchaseResult result = upgrades.Purchase(upgradeId);

            Assert.That(result.Failure, Is.EqualTo(expectedFailure));
            Assert.That(player.CashMinorUnits, Is.EqualTo(1000));
            Assert.That(state.Upgrades, Is.Empty);
        }

        [Test]
        public void GetEffectTotal_AddsAllPurchasedLevelsWithMatchingType()
        {
            var player = new PlayerFinancialState(10_000);
            var state = new ProgressionState();
            var upgrades = new UpgradeService(
                player,
                state,
                new[]
                {
                    new UpgradeSpec("dividend_a", 5, 100, 10000, UpgradeEffectType.DividendYieldBonus, 0.10d),
                    new UpgradeSpec("dividend_b", 5, 100, 10000, UpgradeEffectType.DividendYieldBonus, 0.25d),
                    new UpgradeSpec("insight", 5, 100, 10000, UpgradeEffectType.MarketInsight, 1d)
                });
            upgrades.Purchase("dividend_a");
            upgrades.Purchase("dividend_a");
            upgrades.Purchase("dividend_b");
            upgrades.Purchase("insight");

            Assert.That(upgrades.GetEffectTotal(UpgradeEffectType.DividendYieldBonus), Is.EqualTo(0.45d).Within(0.0000001d));
            Assert.That(upgrades.GetEffectTotal(UpgradeEffectType.MarketInsight), Is.EqualTo(1d));
            Assert.That(upgrades.GetEffectTotal(UpgradeEffectType.AutomationCapacity), Is.Zero);
        }

        [Test]
        public void Purchase_WhenFutureCostOverflows_ReturnsFailureWithoutMutation()
        {
            var player = new PlayerFinancialState(long.MaxValue);
            var state = new ProgressionState();
            var upgrades = new UpgradeService(
                player,
                state,
                new[]
                {
                    new UpgradeSpec(
                        "expensive",
                        2,
                        long.MaxValue / 2,
                        30000,
                        UpgradeEffectType.MarketInsight,
                        1d)
                });
            Assert.That(upgrades.Purchase("expensive").Succeeded, Is.True);
            long cashBefore = player.CashMinorUnits;

            UpgradePurchaseResult result = upgrades.Purchase("expensive");

            Assert.That(result.Failure, Is.EqualTo(UpgradePurchaseFailure.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(state.GetLevel("expensive"), Is.EqualTo(1));
        }

        [Test]
        public void Constructor_RejectsDuplicateUpgradeIds()
        {
            UpgradeSpec first = CreateSpec();
            UpgradeSpec second = CreateSpec();

            Assert.Throws<ArgumentException>(() => new UpgradeService(
                new PlayerFinancialState(0),
                new ProgressionState(),
                new[] { first, second }));
        }

        private static UpgradeService CreateService(
            long startingCash,
            out PlayerFinancialState player,
            out ProgressionState state)
        {
            player = new PlayerFinancialState(startingCash);
            state = new ProgressionState();
            return new UpgradeService(player, state, new[] { CreateSpec() });
        }

        private static UpgradeSpec CreateSpec()
        {
            return new UpgradeSpec(
                "dividend_research",
                maxLevel: 3,
                baseCostMinorUnits: 100,
                costGrowthBasisPoints: 15000,
                UpgradeEffectType.DividendYieldBonus,
                effectAmountPerLevel: 0.10d);
        }
    }
}
