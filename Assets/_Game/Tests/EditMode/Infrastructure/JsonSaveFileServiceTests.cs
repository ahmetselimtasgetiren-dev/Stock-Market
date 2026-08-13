using System.IO;
using NUnit.Framework;
using StockMarket.Infrastructure.Persistence;

namespace StockMarket.Infrastructure.Tests
{
    public sealed class JsonSaveFileServiceTests
    {
        private string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "StockMarketSaveTests", TestContext.CurrentContext.Test.ID);
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void SaveAndLoad_RoundTripsVersionedData()
        {
            string path = Path.Combine(directory, "save.json");
            var data = new SaveGameData
            {
                savedAtUnixSeconds = 1000,
                currentMarketTick = 42,
                cashMinorUnits = 12345
            };
            data.positions.Add(new SavedPosition
            {
                companyId = "quillbyte",
                quantity = 3,
                totalCostBasisMinorUnits = 300
            });
            var service = new JsonSaveFileService();

            service.Save(path, data);
            SaveLoadResult result = service.Load(path);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.version, Is.EqualTo(SaveGameData.CurrentVersion));
            Assert.That(result.Data.currentMarketTick, Is.EqualTo(42));
            Assert.That(result.Data.cashMinorUnits, Is.EqualTo(12345));
            Assert.That(result.Data.positions[0].companyId, Is.EqualTo("quillbyte"));
        }

        [Test]
        public void Load_WhenPrimaryIsCorrupt_UsesLastBackup()
        {
            string path = Path.Combine(directory, "save.json");
            var service = new JsonSaveFileService();
            service.Save(path, new SaveGameData { cashMinorUnits = 100 });
            service.Save(path, new SaveGameData { cashMinorUnits = 200 });
            File.WriteAllText(path, "not-json");

            SaveLoadResult result = service.Load(path);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data.cashMinorUnits, Is.EqualTo(100));
        }

        [Test]
        public void Load_RejectsUnsupportedVersion()
        {
            string path = Path.Combine(directory, "save.json");
            File.WriteAllText(path, "{\"version\":999}");

            SaveLoadResult result = new JsonSaveFileService().Load(path);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("Unsupported save version"));
        }
    }
}
