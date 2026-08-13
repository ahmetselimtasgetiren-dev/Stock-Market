using System;
using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Market.Simulation;

namespace StockMarket.Domain.Tests
{
    public sealed class MarketSimulationTests
    {
        [Test]
        public void SimulateTick_WithSameSeedAndInputs_IsDeterministic()
        {
            MarketSimulationService first = CreateSimulation(12345UL, out MarketState firstMarket);
            MarketSimulationService second = CreateSimulation(12345UL, out MarketState secondMarket);

            for (long tick = 1; tick <= 100; tick++)
            {
                first.SimulateTick(tick);
                second.SimulateTick(tick);

                Assert.That(first.CurrentMarketTrend, Is.EqualTo(second.CurrentMarketTrend));
                Assert.That(first.RandomState, Is.EqualTo(second.RandomState));

                for (int index = 0; index < firstMarket.Companies.Count; index++)
                {
                    Assert.That(
                        firstMarket.Companies[index].CurrentPriceMinorUnits,
                        Is.EqualTo(secondMarket.Companies[index].CurrentPriceMinorUnits));
                }
            }
        }

        [Test]
        public void SimulateTick_WithDifferentSeeds_ProducesDifferentSequence()
        {
            MarketSimulationService first = CreateSimulation(1UL, out MarketState firstMarket);
            MarketSimulationService second = CreateSimulation(2UL, out MarketState secondMarket);

            for (long tick = 1; tick <= 10; tick++)
            {
                first.SimulateTick(tick);
                second.SimulateTick(tick);
            }

            Assert.That(
                firstMarket.GetCompany("quillbyte_systems").CurrentPriceMinorUnits,
                Is.Not.EqualTo(secondMarket.GetCompany("quillbyte_systems").CurrentPriceMinorUnits));
        }

        [Test]
        public void SimulateTick_BoundsEveryPriceMovement()
        {
            MarketState market = CreateMarket(startingPrice: 100000, historyCapacity: 10);
            var simulation = new MarketSimulationService(
                market,
                CreateProfiles(volatility: 1d),
                CreateConfig(maximumChange: 0.05d),
                44UL);

            for (long tick = 1; tick <= 100; tick++)
            {
                simulation.SimulateTick(tick);

                foreach (CompanyMarketState company in market.Companies)
                {
                    Assert.That(Math.Abs(company.PriceChangeRatio), Is.LessThanOrEqualTo(0.05d));
                }
            }
        }

        [Test]
        public void SimulateTick_ClampsToConfiguredPriceFloorAndCeiling()
        {
            MarketState fallingMarket = CreateMarket(startingPrice: 100, historyCapacity: 3);
            var falling = new MarketSimulationService(
                fallingMarket,
                CreateProfiles(volatility: 0d, drift: -0.5d),
                CreateConfig(maximumChange: 0.9d, minimumPrice: 95, maximumPrice: 105),
                7UL);
            MarketState risingMarket = CreateMarket(startingPrice: 100, historyCapacity: 3);
            var rising = new MarketSimulationService(
                risingMarket,
                CreateProfiles(volatility: 0d, drift: 0.5d),
                CreateConfig(maximumChange: 0.9d, minimumPrice: 95, maximumPrice: 105),
                7UL);

            falling.SimulateTick(1);
            rising.SimulateTick(1);

            Assert.That(fallingMarket.Companies[0].CurrentPriceMinorUnits, Is.EqualTo(95));
            Assert.That(risingMarket.Companies[0].CurrentPriceMinorUnits, Is.EqualTo(105));
        }

        [Test]
        public void SimulateTick_KeepsTrendWithinConfiguredMagnitude()
        {
            MarketSimulationService simulation = CreateSimulation(6789UL, out _);

            for (long tick = 1; tick <= 1000; tick++)
            {
                simulation.SimulateTick(tick);
                Assert.That(Math.Abs(simulation.CurrentMarketTrend), Is.LessThanOrEqualTo(0.01d));
            }
        }

        [Test]
        public void SimulateTick_UpdatesEveryCompanyAndUsesBoundedHistory()
        {
            MarketSimulationService simulation = CreateSimulation(42UL, out MarketState market, historyCapacity: 4);

            for (long tick = 1; tick <= 10; tick++)
            {
                simulation.SimulateTick(tick);
            }

            foreach (CompanyMarketState company in market.Companies)
            {
                Assert.That(company.LastUpdatedTick, Is.EqualTo(10));
                Assert.That(company.PriceHistory.Count, Is.EqualTo(4));
                Assert.That(company.PriceHistory.Oldest.Tick, Is.EqualTo(7));
                Assert.That(company.PriceHistory.Latest.Tick, Is.EqualTo(10));
            }
        }

        [Test]
        public void SimulateTick_WhenTickIsRepeated_DoesNotAdvanceRandomOrMarketState()
        {
            MarketSimulationService simulation = CreateSimulation(42UL, out MarketState market);
            simulation.SimulateTick(1);
            ulong randomState = simulation.RandomState;
            long price = market.Companies[0].CurrentPriceMinorUnits;

            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.SimulateTick(1));

            Assert.That(simulation.RandomState, Is.EqualTo(randomState));
            Assert.That(market.Companies[0].CurrentPriceMinorUnits, Is.EqualTo(price));
        }

        [Test]
        public void Constructor_WhenACompanyProfileIsMissing_Throws()
        {
            MarketState market = CreateMarket(startingPrice: 2500, historyCapacity: 10);
            var profiles = new[]
            {
                new CompanySimulationProfile("quillbyte_systems", 0.02d)
            };

            Assert.Throws<ArgumentException>(
                () => new MarketSimulationService(market, profiles, CreateConfig(), 1UL));
        }

        private static MarketSimulationService CreateSimulation(
            ulong seed,
            out MarketState market,
            int historyCapacity = 120)
        {
            market = CreateMarket(startingPrice: 2500, historyCapacity);
            return new MarketSimulationService(market, CreateProfiles(), CreateConfig(), seed);
        }

        private static MarketState CreateMarket(long startingPrice, int historyCapacity)
        {
            return new MarketState(
                new[]
                {
                    new CompanyMarketSeed("quillbyte_systems", startingPrice),
                    new CompanyMarketSeed("morrowmint_foods", startingPrice)
                },
                historyCapacity);
        }

        private static CompanySimulationProfile[] CreateProfiles(double volatility = 0.02d, double drift = 0d)
        {
            return new[]
            {
                new CompanySimulationProfile("quillbyte_systems", volatility, drift),
                new CompanySimulationProfile("morrowmint_foods", volatility, drift)
            };
        }

        private static MarketSimulationConfig CreateConfig(
            double maximumChange = 0.10d,
            long minimumPrice = 1,
            long maximumPrice = 1_000_000_000)
        {
            return new MarketSimulationConfig(
                globalDriftPerTick: 0.0001d,
                trendPersistence: 0.95d,
                trendShockMagnitude: 0.002d,
                maximumTrendMagnitude: 0.01d,
                maximumPriceChangeRatio: maximumChange,
                minimumPriceMinorUnits: minimumPrice,
                maximumPriceMinorUnits: maximumPrice);
        }
    }
}
