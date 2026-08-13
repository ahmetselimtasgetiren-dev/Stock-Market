using System;
using System.Collections.Generic;
using UnityEngine;

namespace StockMarket.Content.Definitions
{
    [CreateAssetMenu(fileName = "NewsCatalog", menuName = "Stock Market/Catalogs/News Events")]
    public sealed class NewsCatalog : ScriptableObject
    {
        [SerializeField]
        private List<NewsDefinition> newsEvents = new List<NewsDefinition>();

        public IReadOnlyList<NewsDefinition> NewsEvents => newsEvents;

        public bool TryGetById(string id, out NewsDefinition definition)
        {
            for (int index = 0; index < newsEvents.Count; index++)
            {
                NewsDefinition candidate = newsEvents[index];

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

            for (int index = 0; index < newsEvents.Count; index++)
            {
                NewsDefinition news = newsEvents[index];

                if (news == null)
                {
                    errors.Add($"News catalog entry {index} is missing.");
                    continue;
                }

                news.CollectValidationErrors(errors);

                if (!knownIds.Add(news.Id))
                {
                    errors.Add($"News catalog contains duplicate ID '{news.Id}'.");
                }
            }
        }
    }
}
