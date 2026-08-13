using NUnit.Framework;
using StockMarket.Domain.Charts;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Reports;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Tests
{
    public sealed class ChartAndReportTests
    {
        [Test]
        public void PriceSeries_DownsamplesAndPreservesEndpoints()
        {
            var market = new MarketState(new[] { new CompanyMarketSeed("quillbyte", 100) }, 20);

            for (long tick = 1; tick <= 9; tick++)
            {
                market.ApplyPrice("quillbyte", tick, 100 + tick);
            }

            ChartSeries series = new ChartDataService().BuildPriceSeries(market.GetCompany("quillbyte"), 4);

            Assert.That(series.Points, Has.Count.EqualTo(4));
            Assert.That(series.Points[0].Tick, Is.Zero);
            Assert.That(series.Points[3].Tick, Is.EqualTo(9));
            Assert.That(series.MinimumValue, Is.EqualTo(100));
            Assert.That(series.MaximumValue, Is.EqualTo(109));
        }

        [Test]
        public void PortfolioHistory_IsBoundedAndBuildsSeries()
        {
            var history = new PortfolioValueHistory(3);
            history.Add(1, 100);
            history.Add(2, 120);
            history.Add(3, 110);
            history.Add(4, 140);

            ChartSeries series = new ChartDataService().BuildPortfolioSeries(history, 3);

            Assert.That(history.Oldest.Tick, Is.EqualTo(2));
            Assert.That(series.Points, Has.Count.EqualTo(3));
            Assert.That(series.MaximumValue, Is.EqualTo(140));
        }

        [Test]
        public void Report_DerivesFinancialAndTradingStatistics()
        {
            var player = new PlayerFinancialState(10_000);
            var market = new MarketState(new[] { new CompanyMarketSeed("quillbyte", 100) }, 10);
            var ledger = new TransactionLedger(20);
            var trading = new TradingService(player, market, ledger);
            trading.Buy("quillbyte", 10);
            market.ApplyPrice("quillbyte", 1, 150);
            trading.Sell("quillbyte", 2);

            ReportSnapshot report = new ReportService(player, market, ledger).Build();

            Assert.That(report.TransactionCount, Is.EqualTo(2));
            Assert.That(report.BuyCount, Is.EqualTo(1));
            Assert.That(report.SellCount, Is.EqualTo(1));
            Assert.That(report.ProfitableSellCount, Is.EqualTo(1));
            Assert.That(report.ProfitableSellRatio, Is.EqualTo(1d));
            Assert.That(report.UnrealizedProfitMinorUnits, Is.EqualTo(400));
            Assert.That(report.RealizedProfitMinorUnits, Is.EqualTo(100));
            Assert.That(report.BestHoldingCompanyId, Is.EqualTo("quillbyte"));
        }
    }
}
