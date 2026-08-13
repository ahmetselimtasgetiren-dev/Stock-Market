using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Stock Market/Definitions/Tutorial Step")]
    public sealed class TutorialStepDefinition : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string titleKey = string.Empty;
        [SerializeField] private string bodyKey = string.Empty;
        [SerializeField] private int trigger;
        [SerializeField] private TutorialStepDefinition prerequisite;

        public string Id => id;
        public string TitleKey => titleKey;
        public string BodyKey => bodyKey;
        public int Trigger => trigger;
        public TutorialStepDefinition Prerequisite => prerequisite;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null) throw new System.ArgumentNullException(nameof(errors));
            if (!DefinitionValidation.TryValidateId(id, out string idError)) errors.Add($"Tutorial '{name}': {idError}");
            if (string.IsNullOrWhiteSpace(titleKey)) errors.Add($"Tutorial '{name}': Title localization key is required.");
            if (string.IsNullOrWhiteSpace(bodyKey)) errors.Add($"Tutorial '{name}': Body localization key is required.");
            if (trigger < 0 || trigger > 5) errors.Add($"Tutorial '{name}': Trigger is invalid.");
            if (prerequisite == this) errors.Add($"Tutorial '{name}': A step cannot require itself.");
        }
    }
}
