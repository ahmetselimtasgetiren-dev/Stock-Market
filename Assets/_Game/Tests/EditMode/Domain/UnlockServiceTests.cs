using System;
using NUnit.Framework;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;
using StockMarket.Domain.Unlocks;

namespace StockMarket.Domain.Tests
{
    public sealed class UnlockServiceTests
    {
        [Test]
        public void Purchase_SectorUnlock_AtomicallyDebitsCashAndGrantsAccess()
        {
            UnlockService unlocks = CreateService(1000, out PlayerFinancialState player, out MarketAccessState access);

            UnlockResult result = unlocks.Purchase("technology_access");

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.TargetScope, Is.EqualTo(UnlockTargetScope.Sector));
            Assert.That(result.TargetId, Is.EqualTo("technology"));
            Assert.That(result.CostMinorUnits, Is.EqualTo(200));
            Assert.That(result.CashAfterMinorUnits, Is.EqualTo(800));
            Assert.That(player.CashMinorUnits, Is.EqualTo(800));
            Assert.That(access.IsSectorUnlocked("technology"), Is.True);
        }

        [Test]
        public void Purchase_CompanyUnlock_RequiresItsSectorFirst()
        {
            UnlockService unlocks = CreateService(1000, out PlayerFinancialState player, out MarketAccessState access);

            UnlockResult blocked = unlocks.Purchase("quillbyte_access");
            UnlockResult sector = unlocks.Purchase("technology_access");
            UnlockResult company = unlocks.Purchase("quillbyte_access");

            Assert.That(blocked.FailureReason, Is.EqualTo(UnlockFailureReason.RequiredSectorLocked));
            Assert.That(sector.Succeeded, Is.True);
            Assert.That(company.Succeeded, Is.True);
            Assert.That(player.CashMinorUnits, Is.EqualTo(500));
            Assert.That(access.IsCompanyUnlocked("quillbyte_systems"), Is.True);
        }

        [Test]
        public void Purchase_WhenAlreadyUnlocked_DoesNotChargeTwice()
        {
            UnlockService unlocks = CreateService(1000, out PlayerFinancialState player, out MarketAccessState access);
            unlocks.Purchase("technology_access");
            long cashBefore = player.CashMinorUnits;

            UnlockResult result = unlocks.Purchase("technology_access");

            Assert.That(result.FailureReason, Is.EqualTo(UnlockFailureReason.AlreadyUnlocked));
            Assert.That(player.CashMinorUnits, Is.EqualTo(cashBefore));
            Assert.That(access.UnlockedSectorCount, Is.EqualTo(1));
        }

        [Test]
        public void Purchase_WhenCashIsInsufficient_LeavesAccessAndCashUnchanged()
        {
            UnlockService unlocks = CreateService(199, out PlayerFinancialState player, out MarketAccessState access);

            UnlockResult result = unlocks.Purchase("technology_access");

            Assert.That(result.FailureReason, Is.EqualTo(UnlockFailureReason.InsufficientCash));
            Assert.That(result.CostMinorUnits, Is.EqualTo(200));
            Assert.That(player.CashMinorUnits, Is.EqualTo(199));
            Assert.That(access.IsSectorUnlocked("technology"), Is.False);
        }

        [TestCase(null, UnlockFailureReason.InvalidUnlockId)]
        [TestCase("", UnlockFailureReason.InvalidUnlockId)]
        [TestCase("unknown", UnlockFailureReason.UnknownUnlock)]
        public void Purchase_WhenIdIsInvalid_ReturnsExpectedFailure(
            string unlockId,
            UnlockFailureReason expectedFailure)
        {
            UnlockService unlocks = CreateService(1000, out PlayerFinancialState player, out MarketAccessState access);

            UnlockResult result = unlocks.Purchase(unlockId);

            Assert.That(result.FailureReason, Is.EqualTo(expectedFailure));
            Assert.That(player.CashMinorUnits, Is.EqualTo(1000));
            Assert.That(access.UnlockedSectorCount, Is.Zero);
            Assert.That(access.UnlockedCompanyCount, Is.Zero);
        }

        [Test]
        public void InitialAccessSeeds_AreAvailableWithoutPurchase()
        {
            var access = new MarketAccessState(
                new[] { "technology" },
                new[] { "quillbyte_systems" });

            Assert.That(access.IsSectorUnlocked("technology"), Is.True);
            Assert.That(access.IsCompanyUnlocked("quillbyte_systems"), Is.True);
        }

        [Test]
        public void Trading_WithAccessGate_RejectsLockedCompanyThenAllowsItAfterUnlock()
        {
            var player = new PlayerFinancialState(10_000);
            var market = new MarketState(
                new[] { new CompanyMarketSeed("quillbyte_systems", 2500) },
                10);
            var access = new MarketAccessState(new[] { "technology" });
            var unlocks = new UnlockService(
                player,
                access,
                new[]
                {
                    new UnlockSpec(
                        "quillbyte_access",
                        UnlockTargetScope.Company,
                        "quillbyte_systems",
                        300,
                        "technology")
                });
            var trading = new TradingService(player, market, access);

            TradeResult locked = trading.Buy("quillbyte_systems", 1);
            UnlockResult unlocked = unlocks.Purchase("quillbyte_access");
            TradeResult purchased = trading.Buy("quillbyte_systems", 1);

            Assert.That(locked.FailureReason, Is.EqualTo(TradeFailureReason.CompanyLocked));
            Assert.That(unlocked.Succeeded, Is.True);
            Assert.That(purchased.Succeeded, Is.True);
            Assert.That(player.CashMinorUnits, Is.EqualTo(7200));
            Assert.That(player.Portfolio.GetShareQuantity("quillbyte_systems"), Is.EqualTo(1));
        }

        [Test]
        public void Constructor_RejectsDuplicateTargets()
        {
            var player = new PlayerFinancialState(0);
            var access = new MarketAccessState();

            Assert.Throws<ArgumentException>(() => new UnlockService(
                player,
                access,
                new[]
                {
                    new UnlockSpec("first", UnlockTargetScope.Sector, "technology", 1),
                    new UnlockSpec("second", UnlockTargetScope.Sector, "technology", 2)
                }));
        }

        private static UnlockService CreateService(
            long startingCash,
            out PlayerFinancialState player,
            out MarketAccessState access)
        {
            player = new PlayerFinancialState(startingCash);
            access = new MarketAccessState();
            return new UnlockService(
                player,
                access,
                new[]
                {
                    new UnlockSpec("technology_access", UnlockTargetScope.Sector, "technology", 200),
                    new UnlockSpec(
                        "quillbyte_access",
                        UnlockTargetScope.Company,
                        "quillbyte_systems",
                        300,
                        "technology")
                });
        }
    }
}
