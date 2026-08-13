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
