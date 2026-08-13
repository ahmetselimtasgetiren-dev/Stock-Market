using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Market
{
    /// <summary>
    /// Authoritative owner of live company prices for one playthrough.
    /// </summary>
    public sealed class MarketState
    {
        private readonly List<CompanyMarketState> companies = new List<CompanyMarketState>();
        private readonly Dictionary<string, CompanyMarketState> companiesById =
            new Dictionary<string, CompanyMarketState>(StringComparer.Ordinal);
        private readonly IReadOnlyList<CompanyMarketState> readOnlyCompanies;

        public MarketState(IEnumerable<CompanyMarketSeed> seeds, int historyCapacity)
        {
            if (seeds == null)
            {
                throw new ArgumentNullException(nameof(seeds));
            }

            if (historyCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(historyCapacity), "History capacity must be positive.");
            }

            foreach (CompanyMarketSeed seed in seeds)
            {
                if (seed == null)
                {
                    throw new ArgumentException("Market seed contains a missing company entry.", nameof(seeds));
                }

                if (companiesById.ContainsKey(seed.CompanyId))
                {
                    throw new ArgumentException(
                        $"Market seed contains duplicate company ID '{seed.CompanyId}'.",
                        nameof(seeds));
                }

                var companyState = new CompanyMarketState(seed, historyCapacity);
                companies.Add(companyState);
                companiesById.Add(seed.CompanyId, companyState);
            }

            if (companies.Count == 0)
            {
                throw new ArgumentException("At least one company is required to create market state.", nameof(seeds));
            }

            readOnlyCompanies = companies.AsReadOnly();
        }

        public IReadOnlyList<CompanyMarketState> Companies => readOnlyCompanies;

        public bool TryGetCompany(string companyId, out CompanyMarketState companyState)
        {
            if (companyId == null)
            {
                companyState = null;
                return false;
            }

            return companiesById.TryGetValue(companyId, out companyState);
        }

        public CompanyMarketState GetCompany(string companyId)
        {
            if (!TryGetCompany(companyId, out CompanyMarketState companyState))
            {
                throw new KeyNotFoundException($"No live market state exists for company ID '{companyId}'.");
            }

            return companyState;
        }

        public void ApplyPrice(string companyId, long tick, long priceMinorUnits)
        {
            GetCompany(companyId).ApplyPrice(tick, priceMinorUnits);
        }
    }
}
