using System;
using System.Collections.Generic;
using StockMarket.Domain.News;

namespace StockMarket.Domain.Market.Simulation
{
    /// <summary>
    /// Produces bounded fictional price movement from drift, trend, volatility, and seeded noise.
    /// </summary>
    public sealed class MarketSimulationService
    {
        private readonly MarketState marketState;
        private readonly MarketSimulationConfig config;
        private readonly SeededRandomSource random;
        private readonly CompanySimulationProfile[] orderedProfiles;
        private readonly long[] nextPrices;
        private readonly NewsEventService newsEvents;

        public MarketSimulationService(
            MarketState marketState,
            IEnumerable<CompanySimulationProfile> profiles,
            MarketSimulationConfig config,
            ulong randomSeed)
            : this(marketState, profiles, config, randomSeed, null)
        {
        }

        public MarketSimulationService(
            MarketState marketState,
            IEnumerable<CompanySimulationProfile> profiles,
            MarketSimulationConfig config,
            ulong randomSeed,
            NewsEventService newsEvents)
        {
            this.marketState = marketState ?? throw new ArgumentNullException(nameof(marketState));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            random = new SeededRandomSource(randomSeed);
            this.newsEvents = newsEvents;

            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            var profilesById = new Dictionary<string, CompanySimulationProfile>(StringComparer.Ordinal);

            foreach (CompanySimulationProfile profile in profiles)
            {
                if (profile == null)
                {
                    throw new ArgumentException("Simulation profiles contain a missing entry.", nameof(profiles));
                }

                if (!profilesById.TryAdd(profile.CompanyId, profile))
                {
                    throw new ArgumentException(
                        $"Simulation profiles contain duplicate company ID '{profile.CompanyId}'.",
                        nameof(profiles));
                }
            }

            orderedProfiles = new CompanySimulationProfile[marketState.Companies.Count];
            nextPrices = new long[marketState.Companies.Count];

            for (int index = 0; index < marketState.Companies.Count; index++)
            {
                CompanyMarketState company = marketState.Companies[index];
                string companyId = company.CompanyId;

                if (company.CurrentPriceMinorUnits < config.MinimumPriceMinorUnits ||
                    company.CurrentPriceMinorUnits > config.MaximumPriceMinorUnits)
                {
                    throw new ArgumentException(
                        $"Company '{companyId}' starts outside the configured price bounds.",
                        nameof(marketState));
                }

                if (!profilesById.TryGetValue(companyId, out CompanySimulationProfile profile))
                {
                    throw new ArgumentException(
                        $"Simulation profile is missing for company ID '{companyId}'.",
                        nameof(profiles));
                }

                orderedProfiles[index] = profile;
            }

            if (profilesById.Count != orderedProfiles.Length)
            {
                throw new ArgumentException(
                    "Simulation profiles contain companies that are not present in the market state.",
                    nameof(profiles));
            }
        }

        public long LastSimulatedTick { get; private set; }

        public double CurrentMarketTrend { get; private set; }

        public ulong RandomState => random.State;

        public void SimulateTick(long tick)
        {
            if (tick <= LastSimulatedTick)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tick),
                    "Simulation ticks must increase strictly.");
            }

            for (int index = 0; index < marketState.Companies.Count; index++)
            {
                if (tick <= marketState.Companies[index].LastUpdatedTick)
                {
                    throw new InvalidOperationException(
                        $"Company '{marketState.Companies[index].CompanyId}' already has a price at tick {tick} or later.");
                }
            }

            if (newsEvents != null && tick <= newsEvents.CurrentTick)
            {
                throw new InvalidOperationException(
                    "News state has already processed this simulation tick or a later one.");
            }

            CurrentMarketTrend = Clamp(
                (CurrentMarketTrend * config.TrendPersistence) +
                (random.NextSignedUnitDouble() * config.TrendShockMagnitude),
                -config.MaximumTrendMagnitude,
                config.MaximumTrendMagnitude);

            newsEvents?.AdvanceToTick(tick);

            for (int index = 0; index < marketState.Companies.Count; index++)
            {
                CompanyMarketState company = marketState.Companies[index];
                CompanySimulationProfile profile = orderedProfiles[index];
                double randomMovement = random.NextSignedUnitDouble() * profile.Volatility;
                double changeRatio = Clamp(
                    config.GlobalDriftPerTick +
                    profile.DriftPerTick +
                    CurrentMarketTrend +
                    (newsEvents?.GetPriceImpact(company.CompanyId, profile.SectorId) ?? 0d) +
                    randomMovement,
                    -config.MaximumPriceChangeRatio,
                    config.MaximumPriceChangeRatio);

                nextPrices[index] = CalculateBoundedPrice(company.CurrentPriceMinorUnits, changeRatio);
            }

            for (int index = 0; index < marketState.Companies.Count; index++)
            {
                marketState.ApplyPrice(marketState.Companies[index].CompanyId, tick, nextPrices[index]);
            }

            LastSimulatedTick = tick;
        }

        private long CalculateBoundedPrice(long currentPriceMinorUnits, double changeRatio)
        {
            double calculatedPrice = currentPriceMinorUnits * (1d + changeRatio);
            long roundedPrice = (long)Math.Round(calculatedPrice, MidpointRounding.AwayFromZero);
            long minimumTickPrice = (long)Math.Ceiling(
                currentPriceMinorUnits * (1d - config.MaximumPriceChangeRatio));
            long maximumTickPrice = (long)Math.Floor(
                currentPriceMinorUnits * (1d + config.MaximumPriceChangeRatio));
            long effectiveMinimum = Math.Max(config.MinimumPriceMinorUnits, minimumTickPrice);
            long effectiveMaximum = Math.Min(config.MaximumPriceMinorUnits, maximumTickPrice);

            return Math.Max(effectiveMinimum, Math.Min(effectiveMaximum, roundedPrice));
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
