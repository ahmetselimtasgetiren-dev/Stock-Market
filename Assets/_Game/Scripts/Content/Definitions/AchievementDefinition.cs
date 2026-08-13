using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "Achievement", menuName = "Stock Market/Definitions/Achievement")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string titleKey = string.Empty;
        [SerializeField] private string descriptionKey = string.Empty;
        [SerializeField] private int metric;
        [SerializeField, Min(1)] private long threshold = 1;

        public string Id => id;
        public string TitleKey => titleKey;
        public string DescriptionKey => descriptionKey;
        public int Metric => metric;
        public long Threshold => threshold;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null) throw new System.ArgumentNullException(nameof(errors));
            if (!DefinitionValidation.TryValidateId(id, out string idError)) errors.Add($"Achievement '{name}': {idError}");
            if (string.IsNullOrWhiteSpace(titleKey)) errors.Add($"Achievement '{name}': Title localization key is required.");
            if (string.IsNullOrWhiteSpace(descriptionKey)) errors.Add($"Achievement '{name}': Description localization key is required.");
            if (metric < 0 || metric > 3) errors.Add($"Achievement '{name}': Metric is invalid.");
            if (threshold <= 0) errors.Add($"Achievement '{name}': Threshold must be positive.");
        }
    }
}
