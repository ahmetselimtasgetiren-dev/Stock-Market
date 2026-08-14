using System;
using System.Collections.Generic;
using StockMarket.Content.Definitions;
using StockMarket.Core.Clock;
using StockMarket.Domain.Charts;
using StockMarket.Domain.Market;
using StockMarket.Domain.Market.Simulation;
using StockMarket.Domain.Navigation;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Progression;
using StockMarket.Domain.Trading;

namespace StockMarket.Presentation.Runtime
{
    /// <summary>
    /// Composition boundary for the first playable. It connects existing domain services
    /// without moving any market, accounting, or trading rules into the UI layer.
    /// </summary>
    internal sealed class StockMarketRuntime : IDisposable
    {
        private readonly PortfolioValuationService valuationService = new PortfolioValuationService();
        private readonly ProfitCalculationService profitService = new ProfitCalculationService();
        private readonly MarketSimulationService simulation;

        public StockMarketRuntime(
            CompanyCatalog companyCatalog,
            UpgradeCatalog upgradeCatalog,
            long startingCashMinorUnits,
            double tickDurationSeconds,
            ulong randomSeed)
        {
            CompanyCatalog = companyCatalog ?? throw new ArgumentNullException(nameof(companyCatalog));
            UpgradeCatalog = upgradeCatalog ?? throw new ArgumentNullException(nameof(upgradeCatalog));

            if (companyCatalog.Companies.Count == 0)
            {
                throw new ArgumentException("The company catalog must contain at least one company.", nameof(companyCatalog));
            }

            var seeds = new List<CompanyMarketSeed>(companyCatalog.Companies.Count);
            var profiles = new List<CompanySimulationProfile>(companyCatalog.Companies.Count);

            for (int index = 0; index < companyCatalog.Companies.Count; index++)
            {
                CompanyDefinition company = companyCatalog.Companies[index];

                if (company == null)
                {
                    throw new ArgumentException("The company catalog contains a missing company.", nameof(companyCatalog));
                }

                seeds.Add(new CompanyMarketSeed(company.Id, company.StartingPriceMinorUnits));
                profiles.Add(new CompanySimulationProfile(
                    company.Id,
                    company.BaseVolatility,
                    driftPerTick: 0.00015d,
                    sectorId: company.Sector != null ? company.Sector.Id : string.Empty));
            }

            Clock = new MarketClock(tickDurationSeconds);
            Market = new MarketState(seeds, historyCapacity: 240);
            Player = new PlayerFinancialState(startingCashMinorUnits);
            Trading = new TradingService(Player, Market, new TransactionLedger(100));
            Progression = new ProgressionState();
            Upgrades = new UpgradeService(Player, Progression, BuildUpgradeSpecs(upgradeCatalog));
            Charts = new ChartDataService();
            Navigation = new NavigationState(GameScreen.Market);

            simulation = new MarketSimulationService(
                Market,
                profiles,
                new MarketSimulationConfig(
                    globalDriftPerTick: 0.0001d,
                    trendPersistence: 0.88d,
                    trendShockMagnitude: 0.0015d,
                    maximumTrendMagnitude: 0.006d,
                    maximumPriceChangeRatio: 0.05d,
                    minimumPriceMinorUnits: 10,
                    maximumPriceMinorUnits: 100_000_000),
                randomSeed);

            Clock.TickOccurred += HandleTick;
        }

        public event Action StateChanged;

        public CompanyCatalog CompanyCatalog { get; }

        public UpgradeCatalog UpgradeCatalog { get; }

        public MarketClock Clock { get; }

        public MarketState Market { get; }

        public PlayerFinancialState Player { get; }

        public TradingService Trading { get; }

        public ProgressionState Progression { get; }

        public UpgradeService Upgrades { get; }

        public ChartDataService Charts { get; }

        public NavigationState Navigation { get; }

        public PortfolioValuation Valuation => valuationService.Calculate(Player, Market);

        public PortfolioPerformance Performance => profitService.Calculate(Player, Market);

        public void Advance(double elapsedSeconds)
        {
            Clock.Advance(elapsedSeconds);
        }

        public TradeResult Buy(string companyId, long quantity)
        {
            TradeResult result = Trading.Buy(companyId, quantity);
            StateChanged?.Invoke();
            return result;
        }

        public TradeResult Sell(string companyId, long quantity)
        {
            TradeResult result = Trading.Sell(companyId, quantity);
            StateChanged?.Invoke();
            return result;
        }

        public UpgradePurchaseResult PurchaseUpgrade(string upgradeId)
        {
            UpgradePurchaseResult result = Upgrades.Purchase(upgradeId);
            StateChanged?.Invoke();
            return result;
        }

        public CompanyDefinition GetCompanyDefinition(string companyId)
        {
            if (!CompanyCatalog.TryGetById(companyId, out CompanyDefinition definition))
            {
                throw new KeyNotFoundException($"No company definition exists for ID '{companyId}'.");
            }

            return definition;
        }

        public UpgradeDefinition GetUpgradeDefinition(string upgradeId)
        {
            if (!UpgradeCatalog.TryGetById(upgradeId, out UpgradeDefinition definition))
            {
                throw new KeyNotFoundException($"No upgrade definition exists for ID '{upgradeId}'.");
            }

            return definition;
        }

        public void Dispose()
        {
            Clock.TickOccurred -= HandleTick;
        }

        private void HandleTick(MarketTick tick)
        {
            simulation.SimulateTick(tick.Index);
            StateChanged?.Invoke();
        }

        private static IEnumerable<UpgradeSpec> BuildUpgradeSpecs(UpgradeCatalog catalog)
        {
            var specs = new List<UpgradeSpec>(catalog.Upgrades.Count);

            for (int index = 0; index < catalog.Upgrades.Count; index++)
            {
                UpgradeDefinition definition = catalog.Upgrades[index];

                if (definition == null)
                {
                    throw new ArgumentException("The upgrade catalog contains a missing upgrade.", nameof(catalog));
                }

                specs.Add(new UpgradeSpec(
                    definition.Id,
                    definition.MaxLevel,
                    definition.BaseCostMinorUnits,
                    definition.CostGrowthBasisPoints,
                    (StockMarket.Domain.Progression.UpgradeEffectType)(int)definition.EffectType,
                    definition.EffectAmountPerLevel));
            }

            return specs;
        }
    }
}
