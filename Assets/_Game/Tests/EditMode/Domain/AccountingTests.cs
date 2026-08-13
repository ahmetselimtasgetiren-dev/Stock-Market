using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Tests
{
    public sealed class AccountingTests
    {
        [Test]
        public void Buys_UseWeightedAverageCostBasis()
        {
            TradingService trading = CreateTrading(100_000, 2500, out PlayerFinancialState player, out MarketState market);
            Assert.That(trading.Buy("quillbyte_systems", 2).Succeeded, Is.True);
            market.ApplyPrice("quillbyte_systems", 1, 3500);

            Assert.That(trading.Buy("quillbyte_systems", 2).Succeeded, Is.True);

            PositionState position = player.Portfolio.Positions[0];
            Assert.That(position.ShareQuantity, Is.EqualTo(4));
            Assert.That(position.TotalCostBasisMinorUnits, Is.EqualTo(12_000));
            Assert.That(position.AverageBuyPriceMinorUnits, Is.EqualTo(3000m));
        }

        [Test]
        public void PartialSale_RemovesWeightedCostAndRecordsRealizedProfit()
        {
            TradingService trading = CreateTrading(100_000, 2500, out PlayerFinancialState player, out MarketState market);
            trading.Buy("quillbyte_systems", 2);
            market.ApplyPrice("quillbyte_systems", 1, 3500);
            trading.Buy("quillbyte_systems", 2);
            market.ApplyPrice("quillbyte_systems", 2, 4000);

            TradeResult sale = trading.Sell("quillbyte_systems", 1);

            PositionState remaining = player.Portfolio.Positions[0];
            Assert.That(sale.CostBasisRemovedMinorUnits, Is.EqualTo(3000));
            Assert.That(sale.RealizedProfitMinorUnits, Is.EqualTo(1000));
            Assert.That(player.RealizedProfitMinorUnits, Is.EqualTo(1000));
            Assert.That(remaining.ShareQuantity, Is.EqualTo(3));
            Assert.That(remaining.TotalCostBasisMinorUnits, Is.EqualTo(9000));
            Assert.That(trading.Transactions.Latest.RealizedProfitMinorUnits, Is.EqualTo(1000));
        }

        [Test]
        public void FinalSale_RemovesExactRemainingCostBasisAfterRounding()
        {
            TradingService trading = CreateTrading(10_000, 100, out PlayerFinancialState player, out MarketState market);
            trading.Buy("quillbyte_systems", 1);
            market.ApplyPrice("quillbyte_systems", 1, 101);
            trading.Buy("quillbyte_systems", 2);

            TradeResult partial = trading.Sell("quillbyte_systems", 1);
            TradeResult final = trading.Sell("quillbyte_systems", 2);

            Assert.That(partial.CostBasisRemovedMinorUnits, Is.EqualTo(101));
            Assert.That(final.CostBasisRemovedMinorUnits, Is.EqualTo(201));
            Assert.That(player.Portfolio.Positions, Is.Empty);
        }

        [Test]
        public void SaleBelowAveragePrice_RecordsNegativeRealizedProfit()
        {
            TradingService trading = CreateTrading(10_000, 2500, out PlayerFinancialState player, out MarketState market);
            trading.Buy("quillbyte_systems", 2);
            market.ApplyPrice("quillbyte_systems", 1, 2000);

            TradeResult sale = trading.Sell("quillbyte_systems", 1);

            Assert.That(sale.RealizedProfitMinorUnits, Is.EqualTo(-500));
            Assert.That(player.RealizedProfitMinorUnits, Is.EqualTo(-500));
        }

        [Test]
        public void ProfitCalculation_CombinesRealizedAndUnrealizedProfit()
        {
            TradingService trading = CreateTrading(100_000, 2500, out PlayerFinancialState player, out MarketState market);
            trading.Buy("quillbyte_systems", 2);
            market.ApplyPrice("quillbyte_systems", 1, 3500);
            trading.Buy("quillbyte_systems", 2);
            market.ApplyPrice("quillbyte_systems", 2, 4000);
            trading.Sell("quillbyte_systems", 1);

            PortfolioPerformance performance = new ProfitCalculationService().Calculate(player, market);

            Assert.That(performance.HoldingsValueMinorUnits, Is.EqualTo(12_000));
            Assert.That(performance.CostBasisMinorUnits, Is.EqualTo(9000));
            Assert.That(performance.UnrealizedProfitMinorUnits, Is.EqualTo(3000));
            Assert.That(performance.RealizedProfitMinorUnits, Is.EqualTo(1000));
            Assert.That(performance.TotalProfitMinorUnits, Is.EqualTo(4000));
        }

        [Test]
        public void TransactionLedger_WhenFull_DropsOldestRecordAndKeepsSequentialIds()
        {
            var ledger = new TransactionLedger(2);
            TradingService trading = CreateTrading(100_000, 100, out _, out MarketState market, ledger);

            TradeResult first = trading.Buy("quillbyte_systems", 1);
            market.ApplyPrice("quillbyte_systems", 1, 110);
            TradeResult second = trading.Buy("quillbyte_systems", 1);
            TradeResult third = trading.Sell("quillbyte_systems", 1);

            Assert.That(first.TransactionId, Is.EqualTo(1));
            Assert.That(second.TransactionId, Is.EqualTo(2));
            Assert.That(third.TransactionId, Is.EqualTo(3));
            Assert.That(ledger.Count, Is.EqualTo(2));
            Assert.That(ledger.Oldest.TransactionId, Is.EqualTo(2));
            Assert.That(ledger.Latest.TransactionId, Is.EqualTo(3));
        }

        [Test]
        public void FailedTrade_IsNotAddedToTransactionLedger()
        {
            TradingService trading = CreateTrading(99, 100, out _, out _);

            TradeResult failure = trading.Buy("quillbyte_systems", 1);

            Assert.That(failure.Succeeded, Is.False);
            Assert.That(failure.TransactionId, Is.Zero);
            Assert.That(trading.Transactions.Count, Is.Zero);
        }

        [Test]
        public void Buy_WhenAccumulatedCostBasisWouldOverflow_LeavesAllStateUnchanged()
        {
            TradingService trading = CreateTrading(10_000, 100, out PlayerFinancialState player, out _);
            player.Portfolio.AddPurchasedShares("quillbyte_systems", 1, long.MaxValue);

            TradeResult result = trading.Buy("quillbyte_systems", 1);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(10_000));
            Assert.That(player.Portfolio.Positions[0].ShareQuantity, Is.EqualTo(1));
            Assert.That(player.Portfolio.Positions[0].TotalCostBasisMinorUnits, Is.EqualTo(long.MaxValue));
            Assert.That(trading.Transactions.Count, Is.Zero);
        }

        [Test]
        public void Sell_WhenCumulativeRealizedProfitWouldOverflow_LeavesAllStateUnchanged()
        {
            TradingService trading = CreateTrading(10_000, 100, out PlayerFinancialState player, out MarketState market);
            trading.Buy("quillbyte_systems", 1);
            player.ApplyRealizedProfit(long.MaxValue);
            market.ApplyPrice("quillbyte_systems", 1, 200);
            long cashBefore = player.CashMinorUnits;
            int recordsBefore = trading.Transactions.Count;

            TradeResult result = trading.Sell("quillbyte_systems", 1);

            Assert.That(result.FailureReason, Is.EqualTo(TradeFailureReason.ArithmeticOverflow));
            Assert.That(player.CashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(1));
            Assert.That(player.RealizedProfitMinorUnits, Is.EqualTo(long.MaxValue));
            Assert.That(trading.Transactions.Count, Is.EqualTo(recordsBefore));
        }

        private static TradingService CreateTrading(
            long startingCash,
            long startingPrice,
            out PlayerFinancialState player,
            out MarketState market,
            TransactionLedger ledger = null)
        {
            player = new PlayerFinancialState(startingCash);
            market = new MarketState(
                new[] { new CompanyMarketSeed("quillbyte_systems", startingPrice) },
                historyCapacity: 20);
            return ledger == null
                ? new TradingService(player, market)
                : new TradingService(player, market, ledger);
        }
    }
}
