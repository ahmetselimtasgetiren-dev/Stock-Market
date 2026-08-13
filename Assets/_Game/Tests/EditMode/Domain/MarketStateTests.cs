using System;
using System.Collections.Generic;
using NUnit.Framework;
using StockMarket.Domain.Market;

namespace StockMarket.Domain.Tests
{
    public sealed class MarketStateTests
    {
        [Test]
        public void Constructor_CreatesInitialPriceAndTickZeroHistory()
        {
            MarketState market = CreateMarket(historyCapacity: 3);

            CompanyMarketState company = market.GetCompany("quillbyte_systems");

            Assert.That(market.Companies, Has.Count.EqualTo(2));
            Assert.That(company.CurrentPriceMinorUnits, Is.EqualTo(2500));
            Assert.That(company.PreviousPriceMinorUnits, Is.EqualTo(2500));
            Assert.That(company.LastUpdatedTick, Is.Zero);
            Assert.That(company.PriceHistory.Count, Is.EqualTo(1));
            Assert.That(company.PriceHistory[0].Tick, Is.Zero);
            Assert.That(company.PriceHistory[0].PriceMinorUnits, Is.EqualTo(2500));
        }

        [Test]
        public void ApplyPrice_UpdatesCurrentPreviousChangeAndHistory()
        {
            MarketState market = CreateMarket(historyCapacity: 3);

            market.ApplyPrice("quillbyte_systems", 1, 2600);

            CompanyMarketState company = market.GetCompany("quillbyte_systems");
            Assert.That(company.CurrentPriceMinorUnits, Is.EqualTo(2600));
            Assert.That(company.PreviousPriceMinorUnits, Is.EqualTo(2500));
            Assert.That(company.PriceChangeMinorUnits, Is.EqualTo(100));
            Assert.That(company.PriceChangeRatio, Is.EqualTo(0.04d).Within(1e-12d));
            Assert.That(company.LastUpdatedTick, Is.EqualTo(1));
            Assert.That(company.PriceHistory.Latest.Tick, Is.EqualTo(1));
        }

        [Test]
        public void PriceHistory_WhenCapacityIsExceeded_DropsOldestSamplesOnly()
        {
            MarketState market = CreateMarket(historyCapacity: 3);

            market.ApplyPrice("quillbyte_systems", 1, 2510);
            market.ApplyPrice("quillbyte_systems", 2, 2520);
            market.ApplyPrice("quillbyte_systems", 3, 2530);
            market.ApplyPrice("quillbyte_systems", 4, 2540);

            PriceHistoryBuffer history = market.GetCompany("quillbyte_systems").PriceHistory;
            Assert.That(history.Count, Is.EqualTo(3));
            Assert.That(history.Oldest.Tick, Is.EqualTo(2));
            Assert.That(history[1].Tick, Is.EqualTo(3));
            Assert.That(history.Latest.Tick, Is.EqualTo(4));
        }

        [Test]
        public void PriceHistory_CopyTo_PreservesChronologicalOrder()
        {
            MarketState market = CreateMarket(historyCapacity: 2);
            market.ApplyPrice("quillbyte_systems", 1, 2510);
            market.ApplyPrice("quillbyte_systems", 2, 2520);
            var copy = new List<PricePoint>();

            market.GetCompany("quillbyte_systems").PriceHistory.CopyTo(copy);

            Assert.That(copy, Has.Count.EqualTo(2));
            Assert.That(copy[0].Tick, Is.EqualTo(1));
            Assert.That(copy[1].Tick, Is.EqualTo(2));
        }

        [Test]
        public void ApplyPrice_WhenTickDoesNotAdvance_ThrowsWithoutChangingState()
        {
            MarketState market = CreateMarket(historyCapacity: 3);
            market.ApplyPrice("quillbyte_systems", 2, 2600);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => market.ApplyPrice("quillbyte_systems", 2, 2700));

            CompanyMarketState company = market.GetCompany("quillbyte_systems");
            Assert.That(company.CurrentPriceMinorUnits, Is.EqualTo(2600));
            Assert.That(company.PriceHistory.Count, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ApplyPrice_WhenPriceIsNotPositive_ThrowsWithoutChangingState(long invalidPrice)
        {
            MarketState market = CreateMarket(historyCapacity: 3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => market.ApplyPrice("quillbyte_systems", 1, invalidPrice));

            Assert.That(market.GetCompany("quillbyte_systems").LastUpdatedTick, Is.Zero);
        }

        [Test]
        public void Constructor_WhenCompanyIdIsDuplicated_Throws()
        {
            var seeds = new[]
            {
                new CompanyMarketSeed("duplicate", 1000),
                new CompanyMarketSeed("duplicate", 2000)
            };

            Assert.Throws<ArgumentException>(() => new MarketState(seeds, 10));
        }

        [Test]
        public void GetCompany_WhenIdIsUnknown_Throws()
        {
            MarketState market = CreateMarket(historyCapacity: 3);

            Assert.Throws<KeyNotFoundException>(() => market.GetCompany("unknown_company"));
        }

        [Test]
        public void Constructor_WhenNoCompaniesAreProvided_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new MarketState(Array.Empty<CompanyMarketSeed>(), 10));
        }

        [Test]
        public void Constructor_WhenACompanySeedIsMissing_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new MarketState(new CompanyMarketSeed[] { null }, 10));
        }

        private static MarketState CreateMarket(int historyCapacity)
        {
            return new MarketState(
                new[]
                {
                    new CompanyMarketSeed("quillbyte_systems", 2500),
                    new CompanyMarketSeed("morrowmint_foods", 1850)
                },
                historyCapacity);
        }
    }
}
