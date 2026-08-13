using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "DividendPolicyCatalog", menuName = "Stock Market/Catalogs/Dividend Policies")]
    public sealed class DividendPolicyCatalog : ScriptableObject
    {
        [SerializeField]
        private List<DividendPolicyDefinition> policies = new List<DividendPolicyDefinition>();

        public IReadOnlyList<DividendPolicyDefinition> Policies => policies;

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            var knownCompanies = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < policies.Count; index++)
            {
                DividendPolicyDefinition policy = policies[index];

                if (policy == null)
                {
                    errors.Add($"Dividend policy catalog entry {index} is missing.");
                    continue;
                }

                policy.CollectValidationErrors(errors);

                if (!knownIds.Add(policy.Id))
                {
                    errors.Add($"Dividend policy catalog contains duplicate ID '{policy.Id}'.");
                }

                if (policy.Company != null && !knownCompanies.Add(policy.Company.Id))
                {
                    errors.Add($"Dividend policy catalog contains more than one policy for company '{policy.Company.Id}'.");
                }
            }
        }
    }
}
