using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "CompanyCatalog", menuName = "Stock Market/Catalogs/Companies")]
    public sealed class CompanyCatalog : ScriptableObject
    {
        [SerializeField]
        private List<CompanyDefinition> companies = new List<CompanyDefinition>();

        public IReadOnlyList<CompanyDefinition> Companies => companies;

        public bool TryGetById(string id, out CompanyDefinition definition)
        {
            for (int index = 0; index < companies.Count; index++)
            {
                CompanyDefinition candidate = companies[index];

                if (candidate != null && string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            var knownTickers = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < companies.Count; index++)
            {
                CompanyDefinition company = companies[index];

                if (company == null)
                {
                    errors.Add($"Company catalog entry {index} is missing.");
                    continue;
                }

                company.CollectValidationErrors(errors);

                if (!knownIds.Add(company.Id))
                {
                    errors.Add($"Company catalog contains duplicate ID '{company.Id}'.");
                }

                if (!knownTickers.Add(company.Ticker))
                {
                    errors.Add($"Company catalog contains duplicate ticker '{company.Ticker}'.");
                }
            }
        }
    }
}
