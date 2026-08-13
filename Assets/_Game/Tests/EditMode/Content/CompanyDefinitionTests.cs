using System.Collections.Generic;
using NUnit.Framework;
using StockMarket.Content.Definitions;
using UnityEditor;
using UnityEngine;

namespace StockMarket.Content.Tests
{
    public sealed class CompanyDefinitionTests
    {
        private readonly List<Object> temporaryObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < temporaryObjects.Count; index++)
            {
                Object.DestroyImmediate(temporaryObjects[index]);
            }

            temporaryObjects.Clear();
        }

        [TestCase("technology", true)]
        [TestCase("green_energy_2", true)]
        [TestCase("Technology", false)]
        [TestCase("green-energy", false)]
        [TestCase("2technology", false)]
        [TestCase("", false)]
        public void DefinitionIdValidation_EnforcesStableIdFormat(string id, bool expected)
        {
            Assert.That(DefinitionValidation.TryValidateId(id, out _), Is.EqualTo(expected));
        }

        [Test]
        public void CompanyDefinition_WithValidAuthoredData_HasNoValidationErrors()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            var errors = new List<string>();

            company.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void CompanyCatalog_ReportsDuplicateIdsAndTickers()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition first = CreateCompany("nova_circuitry", "Nova Circuitry", "NOVA", sector, 2500, 0.025f);
            CompanyDefinition second = CreateCompany("nova_circuitry", "Nova Systems", "NOVA", sector, 1800, 0.03f);
            CompanyCatalog catalog = CreateObject<CompanyCatalog>();
            SetObjectArray(catalog, "companies", first, second);
            var errors = new List<string>();

            catalog.CollectValidationErrors(errors);

            Assert.That(errors, Has.Some.Contains("duplicate ID"));
            Assert.That(errors, Has.Some.Contains("duplicate ticker"));
        }

        [Test]
        public void CompanyCatalog_TryGetById_UsesStableId()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition expected = CreateCompany("nova_circuitry", "Nova Circuitry", "NOVA", sector, 2500, 0.025f);
            CompanyCatalog catalog = CreateObject<CompanyCatalog>();
            SetObjectArray(catalog, "companies", expected);

            bool found = catalog.TryGetById("nova_circuitry", out CompanyDefinition actual);

            Assert.That(found, Is.True);
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(catalog.TryGetById("NOVA_CIRCUITRY", out _), Is.False);
        }

        [Test]
        public void AuthoredCatalogs_ContainTheValidFirstPlayableDefinitionSet()
        {
            SectorCatalog sectorCatalog = AssetDatabase.LoadAssetAtPath<SectorCatalog>(
                "Assets/_Game/Data/Catalogs/SectorCatalog.asset");
            CompanyCatalog companyCatalog = AssetDatabase.LoadAssetAtPath<CompanyCatalog>(
                "Assets/_Game/Data/Catalogs/CompanyCatalog.asset");
            var errors = new List<string>();

            Assert.That(sectorCatalog, Is.Not.Null);
            Assert.That(companyCatalog, Is.Not.Null);

            sectorCatalog.CollectValidationErrors(errors);
            companyCatalog.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
            Assert.That(sectorCatalog.Sectors, Has.Count.EqualTo(2));
            Assert.That(companyCatalog.Companies, Has.Count.EqualTo(3));

            for (int index = 0; index < companyCatalog.Companies.Count; index++)
            {
                CompanyDefinition company = companyCatalog.Companies[index];
                Assert.That(
                    sectorCatalog.TryGetById(company.Sector.Id, out SectorDefinition sector),
                    Is.True,
                    $"Company '{company.Id}' references a sector outside the authored sector catalog.");
                Assert.That(sector, Is.SameAs(company.Sector));
            }
        }

        [Test]
        public void NewsDefinition_WithCompanyTarget_HasNoValidationErrors()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            NewsDefinition news = CreateObject<NewsDefinition>();
            var serializedObject = new SerializedObject(news);
            serializedObject.FindProperty("id").stringValue = "nova_product_launch";
            serializedObject.FindProperty("headline").stringValue = "Nova unveils pocket processor";
            serializedObject.FindProperty("summary").stringValue = "Early fictional demand exceeds expectations.";
            serializedObject.FindProperty("targetType").enumValueIndex = (int)NewsTargetType.Company;
            serializedObject.FindProperty("company").objectReferenceValue = company;
            serializedObject.FindProperty("priceImpactPerTick").floatValue = 0.03f;
            serializedObject.FindProperty("durationTicks").intValue = 4;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            var errors = new List<string>();

            news.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
            Assert.That(news.TargetId, Is.EqualTo("nova_circuitry"));
        }

        [Test]
        public void NewsCatalog_ReportsDuplicateIdsAndInvalidEntries()
        {
            NewsDefinition first = CreateObject<NewsDefinition>();
            NewsDefinition second = CreateObject<NewsDefinition>();
            SetString(first, "id", "duplicate_news");
            SetString(second, "id", "duplicate_news");
            NewsCatalog catalog = CreateObject<NewsCatalog>();
            SetObjectArray(catalog, "newsEvents", first, second);
            var errors = new List<string>();

            catalog.CollectValidationErrors(errors);

            Assert.That(errors, Has.Some.Contains("duplicate ID"));
            Assert.That(errors, Has.Some.Contains("Headline is required"));
            Assert.That(errors, Has.Some.Contains("Company target is required"));
        }

        [Test]
        public void DividendPolicyDefinition_WithValidData_HasNoValidationErrors()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            DividendPolicyDefinition policy = CreateObject<DividendPolicyDefinition>();
            var serializedObject = new SerializedObject(policy);
            serializedObject.FindProperty("id").stringValue = "nova_quarterly_dividend";
            serializedObject.FindProperty("company").objectReferenceValue = company;
            serializedObject.FindProperty("amountPerShareMinorUnits").longValue = 8;
            serializedObject.FindProperty("intervalTicks").intValue = 60;
            serializedObject.FindProperty("firstPayoutTick").intValue = 60;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            var errors = new List<string>();

            policy.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void DividendPolicyCatalog_ReportsDuplicateCompanies()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            DividendPolicyDefinition first = CreateDividendPolicy("nova_first", company);
            DividendPolicyDefinition second = CreateDividendPolicy("nova_second", company);
            DividendPolicyCatalog catalog = CreateObject<DividendPolicyCatalog>();
            SetObjectArray(catalog, "policies", first, second);
            var errors = new List<string>();

            catalog.CollectValidationErrors(errors);

            Assert.That(errors, Has.Some.Contains("more than one policy"));
        }

        [Test]
        public void UpgradeDefinition_WithValidData_HasNoValidationErrors()
        {
            UpgradeDefinition upgrade = CreateUpgrade("dividend_research");
            var errors = new List<string>();

            upgrade.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
            Assert.That(upgrade.EffectType, Is.EqualTo(UpgradeEffectType.DividendYieldBonus));
        }

        [Test]
        public void UpgradeCatalog_ReportsDuplicateIds()
        {
            UpgradeDefinition first = CreateUpgrade("duplicate_upgrade");
            UpgradeDefinition second = CreateUpgrade("duplicate_upgrade");
            UpgradeCatalog catalog = CreateObject<UpgradeCatalog>();
            SetObjectArray(catalog, "upgrades", first, second);
            var errors = new List<string>();

            catalog.CollectValidationErrors(errors);

            Assert.That(errors, Has.Some.Contains("duplicate ID"));
        }

        [Test]
        public void UnlockDefinition_WithCompanyTarget_HasNoValidationErrors()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            UnlockDefinition unlock = CreateUnlock("nova_access", company);
            var errors = new List<string>();

            unlock.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
            Assert.That(unlock.TargetId, Is.EqualTo("nova_circuitry"));
            Assert.That(unlock.RequiredSectorId, Is.EqualTo("technology"));
        }

        [Test]
        public void UnlockCatalog_ReportsDuplicateTargets()
        {
            SectorDefinition sector = CreateSector("technology", "Technology");
            CompanyDefinition company = CreateCompany(
                "nova_circuitry",
                "Nova Circuitry",
                "NOVA",
                sector,
                2500,
                0.025f);
            UnlockDefinition first = CreateUnlock("nova_first", company);
            UnlockDefinition second = CreateUnlock("nova_second", company);
            UnlockCatalog catalog = CreateObject<UnlockCatalog>();
            SetObjectArray(catalog, "unlocks", first, second);
            var errors = new List<string>();

            catalog.CollectValidationErrors(errors);

            Assert.That(errors, Has.Some.Contains("more than one offer"));
        }

        [Test]
        public void TutorialAndAchievementDefinitions_ValidateLocalizationAndThresholdData()
        {
            TutorialStepDefinition tutorial = CreateObject<TutorialStepDefinition>();
            var tutorialObject = new SerializedObject(tutorial);
            tutorialObject.FindProperty("id").stringValue = "welcome";
            tutorialObject.FindProperty("titleKey").stringValue = "tutorial.welcome.title";
            tutorialObject.FindProperty("bodyKey").stringValue = "tutorial.welcome.body";
            tutorialObject.FindProperty("trigger").intValue = 0;
            tutorialObject.ApplyModifiedPropertiesWithoutUndo();
            AchievementDefinition achievement = CreateObject<AchievementDefinition>();
            var achievementObject = new SerializedObject(achievement);
            achievementObject.FindProperty("id").stringValue = "first_trade";
            achievementObject.FindProperty("titleKey").stringValue = "achievement.first_trade.title";
            achievementObject.FindProperty("descriptionKey").stringValue = "achievement.first_trade.body";
            achievementObject.FindProperty("metric").intValue = 0;
            achievementObject.FindProperty("threshold").longValue = 1;
            achievementObject.ApplyModifiedPropertiesWithoutUndo();
            var errors = new List<string>();

            tutorial.CollectValidationErrors(errors);
            achievement.CollectValidationErrors(errors);

            Assert.That(errors, Is.Empty);
        }

        private SectorDefinition CreateSector(string id, string displayName)
        {
            SectorDefinition sector = CreateObject<SectorDefinition>();
            SetString(sector, "id", id);
            SetString(sector, "displayName", displayName);
            return sector;
        }

        private CompanyDefinition CreateCompany(
            string id,
            string displayName,
            string ticker,
            SectorDefinition sector,
            long startingPriceMinorUnits,
            float baseVolatility)
        {
            CompanyDefinition company = CreateObject<CompanyDefinition>();
            var serializedObject = new SerializedObject(company);
            serializedObject.FindProperty("id").stringValue = id;
            serializedObject.FindProperty("displayName").stringValue = displayName;
            serializedObject.FindProperty("ticker").stringValue = ticker;
            serializedObject.FindProperty("sector").objectReferenceValue = sector;
            serializedObject.FindProperty("startingPriceMinorUnits").longValue = startingPriceMinorUnits;
            serializedObject.FindProperty("baseVolatility").floatValue = baseVolatility;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return company;
        }

        private DividendPolicyDefinition CreateDividendPolicy(string id, CompanyDefinition company)
        {
            DividendPolicyDefinition policy = CreateObject<DividendPolicyDefinition>();
            var serializedObject = new SerializedObject(policy);
            serializedObject.FindProperty("id").stringValue = id;
            serializedObject.FindProperty("company").objectReferenceValue = company;
            serializedObject.FindProperty("amountPerShareMinorUnits").longValue = 1;
            serializedObject.FindProperty("intervalTicks").intValue = 5;
            serializedObject.FindProperty("firstPayoutTick").intValue = 5;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return policy;
        }

        private UpgradeDefinition CreateUpgrade(string id)
        {
            UpgradeDefinition upgrade = CreateObject<UpgradeDefinition>();
            var serializedObject = new SerializedObject(upgrade);
            serializedObject.FindProperty("id").stringValue = id;
            serializedObject.FindProperty("displayName").stringValue = "Dividend Research";
            serializedObject.FindProperty("description").stringValue = "Improves fictional passive income.";
            serializedObject.FindProperty("maxLevel").intValue = 5;
            serializedObject.FindProperty("baseCostMinorUnits").longValue = 100;
            serializedObject.FindProperty("costGrowthBasisPoints").intValue = 15000;
            serializedObject.FindProperty("effectType").enumValueIndex = (int)UpgradeEffectType.DividendYieldBonus;
            serializedObject.FindProperty("effectAmountPerLevel").floatValue = 0.1f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return upgrade;
        }

        private UnlockDefinition CreateUnlock(string id, CompanyDefinition company)
        {
            UnlockDefinition unlock = CreateObject<UnlockDefinition>();
            var serializedObject = new SerializedObject(unlock);
            serializedObject.FindProperty("id").stringValue = id;
            serializedObject.FindProperty("targetType").enumValueIndex = (int)UnlockTargetType.Company;
            serializedObject.FindProperty("company").objectReferenceValue = company;
            serializedObject.FindProperty("costMinorUnits").longValue = 500;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return unlock;
        }

        private T CreateObject<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            temporaryObjects.Add(instance);
            return instance;
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(Object target, string propertyName, params Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;

            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
