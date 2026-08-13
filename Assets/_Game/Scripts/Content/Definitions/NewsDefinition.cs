using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "News", menuName = "Stock Market/Definitions/News Event")]
    public sealed class NewsDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string headline = string.Empty;

        [SerializeField, TextArea]
        private string summary = string.Empty;

        [SerializeField]
        private NewsTargetType targetType;

        [SerializeField]
        private CompanyDefinition company;

        [SerializeField]
        private SectorDefinition sector;

        [SerializeField, Range(-0.25f, 0.25f)]
        private float priceImpactPerTick;

        [SerializeField, Min(1)]
        private int durationTicks = 5;

        public string Id => id;
        public string Headline => headline;
        public string Summary => summary;
        public NewsTargetType TargetType => targetType;
        public CompanyDefinition Company => company;
        public SectorDefinition Sector => sector;
        public float PriceImpactPerTick => priceImpactPerTick;
        public int DurationTicks => durationTicks;

        public string TargetId => targetType == NewsTargetType.Company
            ? company != null ? company.Id : string.Empty
            : sector != null ? sector.Id : string.Empty;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"News '{name}': {idError}");
            }

            if (string.IsNullOrWhiteSpace(headline))
            {
                errors.Add($"News '{name}': Headline is required.");
            }

            if (string.IsNullOrWhiteSpace(summary))
            {
                errors.Add($"News '{name}': Summary is required.");
            }

            if (targetType == NewsTargetType.Company && company == null)
            {
                errors.Add($"News '{name}': Company target is required.");
            }
            else if (targetType == NewsTargetType.Sector && sector == null)
            {
                errors.Add($"News '{name}': Sector target is required.");
            }

            if (float.IsNaN(priceImpactPerTick) || float.IsInfinity(priceImpactPerTick) ||
                priceImpactPerTick < -0.25f || priceImpactPerTick > 0.25f)
            {
                errors.Add($"News '{name}': Price impact must be finite and between -0.25 and 0.25.");
            }

            if (durationTicks <= 0)
            {
                errors.Add($"News '{name}': Duration must be at least one tick.");
            }
        }
    }
}
