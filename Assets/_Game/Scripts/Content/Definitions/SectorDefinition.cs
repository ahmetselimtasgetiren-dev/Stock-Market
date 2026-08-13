using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "Sector", menuName = "Stock Market/Definitions/Sector")]
    public sealed class SectorDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField, TextArea]
        private string description = string.Empty;

        public string Id => id;

        public string DisplayName => displayName;

        public string Description => description;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"Sector '{name}': {idError}");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add($"Sector '{name}': Display name is required.");
            }
        }
    }
}
