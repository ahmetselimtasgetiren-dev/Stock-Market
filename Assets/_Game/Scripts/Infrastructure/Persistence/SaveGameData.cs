using System;
using System.Collections.Generic;

namespace StockMarket.Infrastructure.Persistence
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public long savedAtUnixSeconds;
        public long currentMarketTick;
        public double accumulatedTickSeconds;
        public long cashMinorUnits;
        public long realizedProfitMinorUnits;
        public long dividendIncomeMinorUnits;
        public List<SavedPosition> positions = new List<SavedPosition>();
        public List<SavedCompanyMarket> companies = new List<SavedCompanyMarket>();
        public List<SavedUpgrade> upgrades = new List<SavedUpgrade>();
        public List<string> unlockedSectorIds = new List<string>();
        public List<string> unlockedCompanyIds = new List<string>();
        public List<SavedAutomationRule> automationRules = new List<SavedAutomationRule>();
        public List<string> completedTutorialStepIds = new List<string>();
        public List<string> earnedAchievementIds = new List<string>();
        public SavedSettings settings = new SavedSettings();
    }

    [Serializable]
    public sealed class SavedPosition
    {
        public string companyId;
        public long quantity;
        public long totalCostBasisMinorUnits;
    }

    [Serializable]
    public sealed class SavedCompanyMarket
    {
        public string companyId;
        public long currentPriceMinorUnits;
        public long lastUpdatedTick;
        public List<SavedPricePoint> history = new List<SavedPricePoint>();
    }

    [Serializable]
    public sealed class SavedPricePoint
    {
        public long tick;
        public long priceMinorUnits;
    }

    [Serializable]
    public sealed class SavedUpgrade
    {
        public string upgradeId;
        public int level;
        public long totalSpentMinorUnits;
    }

    [Serializable]
    public sealed class SavedAutomationRule
    {
        public long ruleId;
        public string companyId;
        public int condition;
        public long triggerPriceMinorUnits;
        public long quantity;
        public long cooldownTicks;
        public bool isEnabled;
        public long nextEligibleTick;
    }

    [Serializable]
    public sealed class SavedSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float effectsVolume = 1f;
        public bool muteAll;
        public bool reducedMotion;
        public bool notificationsEnabled = true;
    }
}
