using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "DividendPolicy", menuName = "Stock Market/Definitions/Dividend Policy")]
    public sealed class DividendPolicyDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private CompanyDefinition company;

        [SerializeField, Min(1)]
        private long amountPerShareMinorUnits = 1;

        [SerializeField, Min(1)]
        private int intervalTicks = 60;

        [SerializeField, Min(1)]
        private int firstPayoutTick = 60;

        public string Id => id;
        public CompanyDefinition Company => company;
        public long AmountPerShareMinorUnits => amountPerShareMinorUnits;
        public int IntervalTicks => intervalTicks;
        public int FirstPayoutTick => firstPayoutTick;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new System.ArgumentNullException(nameof(errors));
            }

            if (!DefinitionValidation.TryValidateId(id, out string idError))
            {
                errors.Add($"Dividend policy '{name}': {idError}");
            }

            if (company == null)
            {
                errors.Add($"Dividend policy '{name}': Company reference is required.");
            }

            if (amountPerShareMinorUnits <= 0)
            {
                errors.Add($"Dividend policy '{name}': Amount per share must be positive.");
            }

            if (intervalTicks <= 0)
            {
                errors.Add($"Dividend policy '{name}': Interval must be positive.");
            }

            if (firstPayoutTick <= 0)
            {
                errors.Add($"Dividend policy '{name}': First payout tick must be positive.");
            }
        }
    }
}
