using System;
using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Market.Simulation;
using StockMarket.Domain.News;

namespace StockMarket.Domain.Tests
{
    public sealed class NewsEventTests
    {
        [Test]
        public void CompanyNews_AffectsOnlyItsTargetDuringItsDuration()
        {
            var news = new NewsEventService();
            news.Activate(
                new NewsEventDefinition("quillbyte_launch", NewsTargetScope.Company, "quillbyte", 0.10d, 2),
                1);

            news.AdvanceToTick(1);
            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.EqualTo(0.10d));
            Assert.That(news.GetPriceImpact("morrowmint", "consumer_goods"), Is.Zero);

            news.AdvanceToTick(2);
            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.EqualTo(0.10d));

            news.AdvanceToTick(3);
            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.Zero);
            Assert.That(news.Events, Is.Empty);
        }

        [Test]
        public void SectorNews_AffectsEveryCompanyInThatSector()
        {
            var news = new NewsEventService();
            news.Activate(
                new NewsEventDefinition("tech_grants", NewsTargetScope.Sector, "technology", 0.04d, 1),
                1);

            news.AdvanceToTick(1);

            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.EqualTo(0.04d));
            Assert.That(news.GetPriceImpact("pineglass", "consumer_goods"), Is.Zero);
        }

        [Test]
        public void MatchingNewsImpacts_AreAdditive()
        {
            var news = new NewsEventService();
            news.Activate(new NewsEventDefinition("company", NewsTargetScope.Company, "quillbyte", 0.03d, 1), 1);
            news.Activate(new NewsEventDefinition("sector", NewsTargetScope.Sector, "technology", -0.01d, 1), 1);

            news.AdvanceToTick(1);

            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.EqualTo(0.02d).Within(0.0000001d));
        }

        [Test]
        public void Activate_AssignsSequentialInstanceIdsAndSupportsFutureScheduling()
        {
            var news = new NewsEventService();
            NewsEventDefinition definition = new NewsEventDefinition(
                "future_story",
                NewsTargetScope.Company,
                "quillbyte",
                0.05d,
                1);

            ActiveNewsEvent first = news.Activate(definition, 2);
            ActiveNewsEvent second = news.Activate(definition, 4);
            news.AdvanceToTick(1);

            Assert.That(first.InstanceId, Is.EqualTo(1));
            Assert.That(second.InstanceId, Is.EqualTo(2));
            Assert.That(news.GetPriceImpact("quillbyte", "technology"), Is.Zero);
        }

        [Test]
        public void Activate_AtProcessedTick_IsRejectedWithoutChangingState()
        {
            var news = new NewsEventService();
            news.AdvanceToTick(1);
            var definition = new NewsEventDefinition("late", NewsTargetScope.Company, "quillbyte", 0.05d, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => news.Activate(definition, 1));
            Assert.That(news.Events, Is.Empty);
        }

        [Test]
        public void Simulation_AppliesNewsBeforeExistingSafetyClamp()
        {
            var market = new MarketState(
                new[] { new CompanyMarketSeed("quillbyte", 100) },
                historyCapacity: 5);
            var news = new NewsEventService();
            news.Activate(new NewsEventDefinition("large_gain", NewsTargetScope.Company, "quillbyte", 0.25d, 1), 1);
            var simulation = new MarketSimulationService(
                market,
                new[] { new CompanySimulationProfile("quillbyte", 0d, 0d, "technology") },
                new MarketSimulationConfig(0d, 1d, 0d, 0d, 0.10d, 1, 1_000_000),
                randomSeed: 1UL,
                newsEvents: news);

            simulation.SimulateTick(1);

            Assert.That(market.GetCompany("quillbyte").CurrentPriceMinorUnits, Is.EqualTo(110));
        }
    }
}
