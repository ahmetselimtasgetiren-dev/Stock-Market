using NUnit.Framework;
using StockMarket.Content.Definitions;
using StockMarket.Presentation.Runtime;
using UnityEditor;

namespace StockMarket.Presentation.Tests
{
    public sealed class StockMarketRuntimeTests
    {
        [Test]
        public void FirstPlayable_AdvancesMarketAndExecutesTradeThroughDomainServices()
        {
            CompanyCatalog catalog = AssetDatabase.LoadAssetAtPath<CompanyCatalog>(
                "Assets/_Game/Data/Catalogs/CompanyCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            UpgradeCatalog upgradeCatalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>(
                "Assets/_Game/Data/Catalogs/UpgradeCatalog.asset");
            Assert.That(upgradeCatalog, Is.Not.Null);

            using (var runtime = new StockMarketRuntime(
                       catalog,
                       upgradeCatalog,
                       startingCashMinorUnits: 250_000,
                       tickDurationSeconds: 1d,
                       randomSeed: 73421))
            {
                string companyId = catalog.Companies[0].Id;
                long cashBefore = runtime.Player.CashMinorUnits;

                runtime.Advance(1d);

                Assert.That(runtime.Clock.CurrentTick, Is.EqualTo(1));
                Assert.That(runtime.Market.GetCompany(companyId).PriceHistory.Count, Is.EqualTo(2));

                var result = runtime.Buy(companyId, 2);

                Assert.That(result.Succeeded, Is.True);
                Assert.That(runtime.Player.Portfolio.GetShareQuantity(companyId), Is.EqualTo(2));
                Assert.That(runtime.Player.CashMinorUnits, Is.LessThan(cashBefore));
                Assert.That(runtime.Trading.Transactions.Count, Is.EqualTo(1));

                string upgradeId = upgradeCatalog.Upgrades[0].Id;
                var upgradeResult = runtime.PurchaseUpgrade(upgradeId);
                Assert.That(upgradeResult.Succeeded, Is.True);
                Assert.That(runtime.Progression.GetLevel(upgradeId), Is.EqualTo(1));
            }
        }
    }
}
