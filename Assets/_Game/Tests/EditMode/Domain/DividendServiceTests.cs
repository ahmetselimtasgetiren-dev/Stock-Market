using System;
using NUnit.Framework;
using StockMarket.Domain.Dividends;
using StockMarket.Domain.Portfolio;

namespace StockMarket.Domain.Tests
{
    public sealed class DividendServiceTests
    {
        [Test]
        public void ProcessTick_WhenPolicyIsDue_CreditsCashAndRecordsPassiveIncome()
        {
            DividendService dividends = CreateService(
                startingCash: 100,
                shares: 4,
                amountPerShare: 3,
                interval: 5,
                firstTick: 2,
                out PlayerFinancialState player);

            DividendTickResult first = dividends.ProcessTick(1);
            DividendTickResult second = dividends.ProcessTick(2);

            Assert.That(first.PayoutCount, Is.Zero);
            Assert.That(second.PayoutCount, Is.EqualTo(1));
            Assert.That(second.TotalAmountMinorUnits, Is.EqualTo(12));
            Assert.That(player.CashMinorUnits, Is.EqualTo(112));
            Assert.That(player.DividendIncomeMinorUnits, Is.EqualTo(12));
            Assert.That(dividends.GetNextPayoutTick("quillbyte"), Is.EqualTo(7));
            Assert.That(dividends.Ledger.Latest.ShareQuantity, Is.EqualTo(4));
            Assert.That(dividends.Ledger.Latest.Tick, Is.EqualTo(2));
        }

        [Test]
        public void ProcessTick_WhenNoSharesAreOwned_AdvancesWithoutZeroValueRecord()
        {
            DividendService dividends = CreateService(
                100,
                0,
                3,
                interval: 2,
                firstTick: 1,
                out PlayerFinancialState player);

            DividendTickResult result = dividends.ProcessTick(1);

            Assert.That(result.PayoutCount, Is.Zero);
            Assert.That(player.CashMinorUnits, Is.EqualTo(100));
            Assert.That(player.DividendIncomeMinorUnits, Is.Zero);
            Assert.That(dividends.Ledger.Count, Is.Zero);
            Assert.That(dividends.GetNextPayoutTick("quillbyte"), Is.EqualTo(3));
        }

        [Test]
        public void ProcessTick_UsesHoldingsAtThePayoutTick()
        {
            DividendService dividends = CreateService(
                100,
                1,
                5,
                interval: 2,
                firstTick: 1,
                out PlayerFinancialState player);
            dividends.ProcessTick(1);
            player.Portfolio.AddPurchasedShares("quillbyte", 2, 20);
            dividends.ProcessTick(2);

            DividendTickResult secondPayout = dividends.ProcessTick(3);

            Assert.That(secondPayout.TotalAmountMinorUnits, Is.EqualTo(15));
            Assert.That(player.DividendIncomeMinorUnits, Is.EqualTo(20));
            Assert.That(dividends.Ledger.Latest.ShareQuantity, Is.EqualTo(3));
        }

        [Test]
        public void ProcessTick_WhenSeveralPoliciesAreDue_AppliesThemAsOneAtomicCredit()
        {
            var player = new PlayerFinancialState(10);
            player.Portfolio.AddPurchasedShares("quillbyte", 2, 20);
            player.Portfolio.AddPurchasedShares("morrowmint", 3, 30);
            var dividends = new DividendService(
                player,
                new[]
                {
                    new DividendPolicy("quillbyte_dividend", "quillbyte", 4, 5, 1),
                    new DividendPolicy("morrowmint_dividend", "morrowmint", 2, 5, 1)
                },
                new DividendPayoutLedger(10));

            DividendTickResult result = dividends.ProcessTick(1);

            Assert.That(result.PayoutCount, Is.EqualTo(2));
            Assert.That(result.TotalAmountMinorUnits, Is.EqualTo(14));
            Assert.That(player.CashMinorUnits, Is.EqualTo(24));
            Assert.That(player.DividendIncomeMinorUnits, Is.EqualTo(14));
            Assert.That(dividends.Ledger[0].PayoutId, Is.EqualTo(1));
            Assert.That(dividends.Ledger[1].PayoutId, Is.EqualTo(2));
        }

        [Test]
        public void ProcessTick_WhenCashWouldOverflow_LeavesAllStateUnchanged()
        {
            DividendService dividends = CreateService(
                long.MaxValue,
                1,
                1,
                interval: 5,
                firstTick: 1,
                out PlayerFinancialState player);

            Assert.Throws<OverflowException>(() => dividends.ProcessTick(1));

            Assert.That(player.CashMinorUnits, Is.EqualTo(long.MaxValue));
            Assert.That(player.DividendIncomeMinorUnits, Is.Zero);
            Assert.That(dividends.LastProcessedTick, Is.Zero);
            Assert.That(dividends.GetNextPayoutTick("quillbyte"), Is.EqualTo(1));
            Assert.That(dividends.Ledger.Count, Is.Zero);
        }

        [Test]
        public void ProcessTick_MustBeSequential()
        {
            DividendService dividends = CreateService(100, 1, 1, 5, 5, out _);

            Assert.Throws<ArgumentOutOfRangeException>(() => dividends.ProcessTick(2));

            Assert.That(dividends.LastProcessedTick, Is.Zero);
        }

        [Test]
        public void Ledger_WhenCapacityIsReached_DropsTheOldestPayout()
        {
            var player = new PlayerFinancialState(0);
            player.Portfolio.AddPurchasedShares("quillbyte", 1, 1);
            var dividends = new DividendService(
                player,
                new[] { new DividendPolicy("quillbyte_dividend", "quillbyte", 1, 1, 1) },
                new DividendPayoutLedger(2));

            dividends.ProcessTick(1);
            dividends.ProcessTick(2);
            dividends.ProcessTick(3);

            Assert.That(dividends.Ledger.Count, Is.EqualTo(2));
            Assert.That(dividends.Ledger.Oldest.PayoutId, Is.EqualTo(2));
            Assert.That(dividends.Ledger.Latest.PayoutId, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_RejectsMoreThanOnePolicyForACompany()
        {
            var player = new PlayerFinancialState(0);

            Assert.Throws<ArgumentException>(() => new DividendService(
                player,
                new[]
                {
                    new DividendPolicy("first", "quillbyte", 1, 2, 1),
                    new DividendPolicy("second", "quillbyte", 2, 3, 1)
                },
                new DividendPayoutLedger(5)));
        }

        private static DividendService CreateService(
            long startingCash,
            long shares,
            long amountPerShare,
            long interval,
            long firstTick,
            out PlayerFinancialState player)
        {
            player = new PlayerFinancialState(startingCash);

            if (shares > 0)
            {
                player.Portfolio.AddPurchasedShares("quillbyte", shares, shares * 10);
            }

            return new DividendService(
                player,
                new[]
                {
                    new DividendPolicy(
                        "quillbyte_dividend",
                        "quillbyte",
                        amountPerShare,
                        interval,
                        firstTick)
                },
                new DividendPayoutLedger(10));
        }
    }
}
