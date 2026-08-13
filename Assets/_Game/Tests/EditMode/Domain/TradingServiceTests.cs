using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Tests
{
    public sealed class TradingServiceTests
    {
        [Test]
        public void DefaultTradeResult_IsNotSuccessful()
        {
            Assert.That(default(TradeResult).Succeeded, Is.False);
        }

        [Test]
        public void Buy_WhenValid_AtomicallyExchangesCashForShares()
        {
            TradingService trading = CreateTrading(10_000, out PlayerFinancialState player, out _);

            TradeResult result = trading.Buy("quillbyte_systems", 3);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.TradeType, Is.EqualTo(TradeType.Buy));
            Assert.That(result.UnitPriceMinorUnits, Is.EqualTo(2500));
            Assert.That(result.TotalValueMinorUnits, Is.EqualTo(7500));
            Assert.That(result.PriceTick, Is.Zero);
            Assert.That(result.CashAfterMinorUnits, Is.EqualTo(2500));
            Assert.That(result.SharesAfter, Is.EqualTo(3));
            Assert.That(player.CashMinorUnits, Is.EqualTo(2500));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(3));
        }

        [Test]
        public void Sell_WhenValid_AtomicallyExchangesSharesForCashAtCurrentPrice()
        {
            TradingService trading = CreateTrading(10_000, out PlayerFinancialState player, out MarketState market);
            Assert.That(trading.Buy("quillbyte_systems", 2).Succeeded, Is.True);
            market.ApplyPrice("quillbyte_systems", 4, 3000);

            TradeResult result = trading.Sell("quillbyte_systems", 1);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.TradeType, Is.EqualTo(TradeType.Sell));
            Assert.That(result.UnitPriceMinorUnits, Is.EqualTo(3000));
            Assert.That(result.TotalValueMinorUnits, Is.EqualTo(3000));
            Assert.That(result.PriceTick, Is.EqualTo(4));
            Assert.That(player.CashMinorUnits, Is.EqualTo(8000));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(1));
        }

        [Test]
        public void Buy_WhenCashIsInsufficient_LeavesCashAndSharesUnchanged()
        {
            TradingService trading = CreateTrading(2499, out PlayerFinancialState player, out _);

            TradeResult result = trading.Buy("quillbyte_systems", 1);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.InsufficientCash));
            Assert.That(player.CashMinorUnits, Is.EqualTo(2499));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.Zero);
        }

        [Test]
        public void Sell_WhenSharesAreInsufficient_LeavesCashAndSharesUnchanged()
        {
            TradingService trading = CreateTrading(1000, out PlayerFinancialState player, out _);
            player.Portfolio.AddShares("quillbyte_systems", 2);

            TradeResult result = trading.Sell("quillbyte_systems", 3);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.InsufficientShares));
            Assert.That(player.CashMinorUnits, Is.EqualTo(1000));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(2));
        }

        [TestCase(null, TradeFailureReason.InvalidCompanyId)]
        [TestCase("", TradeFailureReason.InvalidCompanyId)]
        [TestCase("unknown_company", TradeFailureReason.UnknownCompany)]
        public void Buy_WhenCompanyIsInvalid_ReturnsExpectedFailure(
            string companyId,
            TradeFailureReason expectedFailure)
        {
            TradingService trading = CreateTrading(10_000, out PlayerFinancialState player, out _);

            TradeResult result = trading.Buy(companyId, 1);

            Assert.That(result.FailureReason, Is.EqualTo(expectedFailure));
            Assert.That(player.CashMinorUnits, Is.EqualTo(10_000));
            Assert.That(player.Portfolio.Positions, Is.Empty);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Buy_WhenQuantityIsInvalid_ReturnsFailureWithoutMutation(long quantity)
        {
            TradingService trading = CreateTrading(10_000, out PlayerFinancialState player, out _);

            TradeResult result = trading.Buy("quillbyte_systems", quantity);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.InvalidQuantity));
            Assert.That(player.CashMinorUnits, Is.EqualTo(10_000));
            Assert.That(player.Portfolio.Positions, Is.Empty);
        }

        [Test]
        public void Buy_WhenTotalValueOverflows_ReturnsFailureWithoutMutation()
        {
            TradingService trading = CreateTrading(long.MaxValue, out PlayerFinancialState player, out _);

            TradeResult result = trading.Buy("quillbyte_systems", long.MaxValue);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(long.MaxValue));
            Assert.That(player.Portfolio.Positions, Is.Empty);
        }

        [Test]
        public void Buy_WhenShareQuantityWouldOverflow_ReturnsFailureWithoutMutation()
        {
            TradingService trading = CreateTrading(long.MaxValue, out PlayerFinancialState player, out _);
            player.Portfolio.AddShares("quillbyte_systems", long.MaxValue);

            TradeResult result = trading.Buy("quillbyte_systems", 1);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(long.MaxValue));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void Sell_WhenCashWouldOverflow_ReturnsFailureWithoutMutation()
        {
            TradingService trading = CreateTrading(long.MaxValue - 1000, out PlayerFinancialState player, out _);
            player.Portfolio.AddShares("quillbyte_systems", 1);

            TradeResult result = trading.Sell("quillbyte_systems", 1);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(long.MaxValue - 1000));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(1));
        }

        private static TradingService CreateTrading(
            long startingCash,
            out PlayerFinancialState player,
            out MarketState market)
        {
            player = new PlayerFinancialState(startingCash);
            market = new MarketState(
                new[] { new CompanyMarketSeed("quillbyte_systems", 2500) },
                historyCapacity: 10);
            return new TradingService(player, market);
        }
    }
}
