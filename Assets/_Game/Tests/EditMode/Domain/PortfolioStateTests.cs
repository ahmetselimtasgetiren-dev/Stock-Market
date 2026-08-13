using System;
using System.Collections.Generic;
using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;

namespace StockMarket.Domain.Tests
{
    public sealed class PortfolioStateTests
    {
        [Test]
        public void Constructor_CreatesCashBalanceAndEmptyPortfolio()
        {
            var player = new PlayerFinancialState(100_000);

            Assert.That(player.CashMinorUnits, Is.EqualTo(100_000));
            Assert.That(player.Portfolio.Positions, Is.Empty);
        }

        [Test]
        public void Constructor_WhenStartingCashIsNegative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerFinancialState(-1));
        }

        [Test]
        public void CashOperations_CreditAndDebitWithoutAllowingNegativeBalance()
        {
            var player = new PlayerFinancialState(10_000);

            Assert.That(player.TryDebitCash(4_000), Is.True);
            player.CreditCash(1_500);
            bool excessiveDebit = player.TryDebitCash(8_000);

            Assert.That(excessiveDebit, Is.False);
            Assert.That(player.CashMinorUnits, Is.EqualTo(7_500));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CashOperations_WhenAmountIsNotPositive_Throw(long amount)
        {
            var player = new PlayerFinancialState(10_000);

            Assert.Throws<ArgumentOutOfRangeException>(() => player.TryDebitCash(amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => player.CreditCash(amount));
            Assert.That(player.CashMinorUnits, Is.EqualTo(10_000));
        }

        [Test]
        public void CreditCash_WhenBalanceWouldOverflow_ThrowsWithoutChangingBalance()
        {
            var player = new PlayerFinancialState(long.MaxValue);

            Assert.Throws<OverflowException>(() => player.CreditCash(1));
            Assert.That(player.CashMinorUnits, Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void ShareOperations_AddRemoveAndDeleteEmptyPosition()
        {
            var portfolio = new PortfolioState();

            portfolio.AddShares("quillbyte_systems", 10);
            portfolio.AddShares("quillbyte_systems", 5);
            bool removedSome = portfolio.TryRemoveShares("quillbyte_systems", 4);
            bool removedRest = portfolio.TryRemoveShares("quillbyte_systems", 11);

            Assert.That(removedSome, Is.True);
            Assert.That(removedRest, Is.True);
            Assert.That(portfolio.GetShareQuantity("quillbyte_systems"), Is.Zero);
            Assert.That(portfolio.Positions, Is.Empty);
        }

        [Test]
        public void TryRemoveShares_WhenQuantityIsUnavailable_DoesNotChangePosition()
        {
            var portfolio = new PortfolioState();
            portfolio.AddShares("quillbyte_systems", 3);

            bool removed = portfolio.TryRemoveShares("quillbyte_systems", 4);

            Assert.That(removed, Is.False);
            Assert.That(portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(3));
        }

        [Test]
        public void AddShares_WhenQuantityWouldOverflow_ThrowsWithoutChangingPosition()
        {
            var portfolio = new PortfolioState();
            portfolio.AddShares("quillbyte_systems", long.MaxValue);

            Assert.Throws<OverflowException>(() => portfolio.AddShares("quillbyte_systems", 1));
            Assert.That(portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void Valuation_UsesCurrentMarketPricesAndPlayerCash()
        {
            MarketState market = CreateMarket();
            var player = new PlayerFinancialState(10_000);
            player.Portfolio.AddShares("quillbyte_systems", 3);
            player.Portfolio.AddShares("morrowmint_foods", 2);
            var valuationService = new PortfolioValuationService();

            PortfolioValuation initial = valuationService.Calculate(player, market);
            market.ApplyPrice("quillbyte_systems", 1, 3000);
            PortfolioValuation updated = valuationService.Calculate(player, market);

            Assert.That(initial.HoldingsValueMinorUnits, Is.EqualTo(11_200));
            Assert.That(initial.NetWorthMinorUnits, Is.EqualTo(21_200));
            Assert.That(updated.HoldingsValueMinorUnits, Is.EqualTo(12_700));
            Assert.That(updated.NetWorthMinorUnits, Is.EqualTo(22_700));
        }

        [Test]
        public void Valuation_WhenOwnedCompanyHasNoMarketState_Throws()
        {
            MarketState market = CreateMarket();
            var player = new PlayerFinancialState(10_000);
            player.Portfolio.AddShares("unknown_company", 1);

            Assert.Throws<KeyNotFoundException>(
                () => new PortfolioValuationService().Calculate(player, market));
        }

        [Test]
        public void Valuation_WhenPositionValueWouldOverflow_Throws()
        {
            MarketState market = CreateMarket();
            var player = new PlayerFinancialState(0);
            player.Portfolio.AddShares("quillbyte_systems", long.MaxValue);

            Assert.Throws<OverflowException>(
                () => new PortfolioValuationService().Calculate(player, market));
        }

        private static MarketState CreateMarket()
        {
            return new MarketState(
                new[]
                {
                    new CompanyMarketSeed("quillbyte_systems", 2500),
                    new CompanyMarketSeed("morrowmint_foods", 1850)
                },
                historyCapacity: 10);
        }
    }
}
