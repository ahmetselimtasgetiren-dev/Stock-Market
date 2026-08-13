using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StockMarket.Infrastructure.Persistence
{
    public sealed class JsonSaveFileService
    {
        public void Save(string path, SaveGameData data)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Save path is required.", nameof(path));
            }

            Validate(data);
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string temporaryPath = fullPath + ".tmp";
            string backupPath = fullPath + ".bak";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));

            if (File.Exists(fullPath))
            {
                File.Replace(temporaryPath, fullPath, backupPath, true);
            }
            else
            {
                File.Move(temporaryPath, fullPath);
            }
        }

        public SaveLoadResult Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new SaveLoadResult(false, null, "Save path is required.");
            }

            string fullPath = Path.GetFullPath(path);
            SaveLoadResult primary = TryLoad(fullPath);

            if (primary.Succeeded)
            {
                return primary;
            }

            SaveLoadResult backup = TryLoad(fullPath + ".bak");
            return backup.Succeeded
                ? backup
                : new SaveLoadResult(false, null, primary.Error);
        }

        private static SaveLoadResult TryLoad(string path)
        {
            if (!File.Exists(path))
            {
                return new SaveLoadResult(false, null, "Save file does not exist.");
            }

            try
            {
                SaveGameData data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
                Validate(data);
                return new SaveLoadResult(true, data, null);
            }
            catch (Exception exception)
            {
                return new SaveLoadResult(false, null, exception.Message);
            }
        }

        private static void Validate(SaveGameData data)
        {
            if (data == null)
            {
                throw new InvalidDataException("Save data is missing.");
            }

            if (data.version != SaveGameData.CurrentVersion)
            {
                throw new InvalidDataException($"Unsupported save version {data.version}.");
            }

            if (data.savedAtUnixSeconds < 0 || data.currentMarketTick < 0 ||
                data.accumulatedTickSeconds < 0d || data.cashMinorUnits < 0)
            {
                throw new InvalidDataException("Save data contains invalid core values.");
            }

            data.positions ??= new List<SavedPosition>();
            data.companies ??= new List<SavedCompanyMarket>();
            data.upgrades ??= new List<SavedUpgrade>();
            data.unlockedSectorIds ??= new List<string>();
            data.unlockedCompanyIds ??= new List<string>();
            data.automationRules ??= new List<SavedAutomationRule>();
            data.completedTutorialStepIds ??= new List<string>();
            data.earnedAchievementIds ??= new List<string>();
            data.settings ??= new SavedSettings();
        }
    }
}
