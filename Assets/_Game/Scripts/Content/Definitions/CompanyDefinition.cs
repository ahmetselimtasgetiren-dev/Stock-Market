using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "Company", menuName = "Stock Market/Definitions/Company")]
    public sealed class CompanyDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField]
        private string ticker = string.Empty;

        [SerializeField]
        private SectorDefinition sector;

        [SerializeField]
        private long startingPriceMinorUnits = 1000;

        [SerializeField, Range(0.001f, 1f)]
        private float baseVolatility = 0.02f;

        public string Id => id;

        public string DisplayName => displayName;

        public string Ticker => ticker;

        public SectorDefinition Sector => sector;

        public long StartingPriceMinorUnits => startingPriceMinorUnits;

        public float BaseVolatility => baseVolatility;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"Company '{name}': {idError}");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"Company '{name}': Display name is required.");
            }

            if (!DefinitionValidation.TryValidateTicker(ticker, out string tickerError))
            {
                errors.Add($"Company '{name}': {tickerError}");
            }

            if (sector == null)
            {
                errors.Add($"Company '{name}': Sector reference is required.");
            }

            if (startingPriceMinorUnits <= 0)
            {
                errors.Add($"Company '{name}': Starting price must be greater than zero.");
            }

            if (float.IsNaN(baseVolatility) || float.IsInfinity(baseVolatility) ||
                baseVolatility <= 0f || baseVolatility > 1f)
            {
                errors.Add($"Company '{name}': Base volatility must be greater than zero and no more than one.");
            }
        }
    }
}
