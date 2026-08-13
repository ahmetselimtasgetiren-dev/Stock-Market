using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "Unlock", menuName = "Stock Market/Definitions/Unlock")]
    public sealed class UnlockDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private UnlockTargetType targetType;

        [SerializeField]
        private SectorDefinition sector;

        [SerializeField]
        private CompanyDefinition company;

        [SerializeField, Min(1)]
        private long costMinorUnits = 1000;

        public string Id => id;
        public UnlockTargetType TargetType => targetType;
        public SectorDefinition Sector => sector;
        public CompanyDefinition Company => company;
        public long CostMinorUnits => costMinorUnits;

        public string TargetId => targetType == UnlockTargetType.Sector
            ? sector != null ? sector.Id : string.Empty
            : company != null ? company.Id : string.Empty;

        public string RequiredSectorId => targetType == UnlockTargetType.Company && company != null && company.Sector != null
            ? company.Sector.Id
            : string.Empty;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"Unlock '{name}': {idError}");
            }

            if (targetType == UnlockTargetType.Sector && sector == null)
            {
                errors.Add($"Unlock '{name}': Sector target is required.");
            }
            else if (targetType == UnlockTargetType.Company && company == null)
            {
                errors.Add($"Unlock '{name}': Company target is required.");
            }

            if (costMinorUnits <= 0)
            {
                errors.Add($"Unlock '{name}': Cost must be positive.");
            }
        }
    }
}
