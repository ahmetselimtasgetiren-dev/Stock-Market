using System;
using System.Collections.Generic;
using StockMarket.Domain.Portfolio;

namespace StockMarket.Domain.Dividends
{
    /// <summary>
    /// Processes deterministic dividend schedules against current whole-share holdings.
    /// </summary>
    public sealed class DividendService
    {
        private readonly PlayerFinancialState player;
        private readonly DividendPolicy[] policies;
        private readonly long[] nextPayoutTicks;
        private readonly long[] dueAmounts;

        public DividendService(
            PlayerFinancialState player,
            IEnumerable<DividendPolicy> policies,
            DividendPayoutLedger ledger)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            Ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));

            if (policies == null)
            {
                throw new ArgumentNullException(nameof(policies));
            }

            var policyList = new List<DividendPolicy>();
            var companyIds = new HashSet<string>(StringComparer.Ordinal);
            var policyIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (DividendPolicy policy in policies)
            {
                if (policy == null)
                {
                    throw new ArgumentException("Dividend policies contain a missing entry.", nameof(policies));
                }

                if (!policyIds.Add(policy.Id))
                {
                    throw new ArgumentException($"Duplicate dividend policy ID '{policy.Id}'.", nameof(policies));
                }

                if (!companyIds.Add(policy.CompanyId))
                {
                    throw new ArgumentException(
                        $"Company '{policy.CompanyId}' has more than one dividend policy.",
                        nameof(policies));
                }

                policyList.Add(policy);
            }

            this.policies = policyList.ToArray();
            nextPayoutTicks = new long[this.policies.Length];
            dueAmounts = new long[this.policies.Length];

            for (int index = 0; index < this.policies.Length; index++)
            {
                nextPayoutTicks[index] = this.policies[index].FirstPayoutTick;
            }
        }

        public long LastProcessedTick { get; private set; }
        public DividendPayoutLedger Ledger { get; }

        public long GetNextPayoutTick(string companyId)
        {
            for (int index = 0; index < policies.Length; index++)
            {
                if (string.Equals(policies[index].CompanyId, companyId, StringComparison.Ordinal))
                {
                    return nextPayoutTicks[index];
                }
            }

            throw new KeyNotFoundException($"No dividend policy exists for company '{companyId}'.");
        }

        public DividendTickResult ProcessTick(long tick)
        {
            if (tick != LastProcessedTick + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "Dividend ticks must be processed sequentially.");
            }

            long totalAmount = 0;
            int payoutCount = 0;

            for (int index = 0; index < policies.Length; index++)
            {
                dueAmounts[index] = 0;

                if (tick != nextPayoutTicks[index])
                {
                    continue;
                }

                DividendPolicy policy = policies[index];
                long shares = player.Portfolio.GetShareQuantity(policy.CompanyId);

                if (shares > 0)
                {
                    long amount = checked(shares * policy.AmountPerShareMinorUnits);
                    dueAmounts[index] = amount;
                    totalAmount = checked(totalAmount + amount);
                    payoutCount++;
                }
            }

            if (payoutCount > 0)
            {
                if (!Ledger.CanAppend(payoutCount) || !player.CanReceiveDividendIncome(totalAmount))
                {
                    throw new OverflowException("Dividend payout would exceed financial or ledger limits.");
                }

                player.ReceiveDividendIncome(totalAmount);
                long payoutId = Ledger.NextPayoutId;

                for (int index = 0; index < policies.Length; index++)
                {
                    if (dueAmounts[index] == 0)
                    {
                        continue;
                    }

                    DividendPolicy policy = policies[index];
                    long shares = player.Portfolio.GetShareQuantity(policy.CompanyId);
                    Ledger.Append(new DividendPayoutRecord(
                        payoutId,
                        policy.Id,
                        policy.CompanyId,
                        tick,
                        shares,
                        policy.AmountPerShareMinorUnits,
                        dueAmounts[index]));
                    payoutId++;
                }
            }

            for (int index = 0; index < policies.Length; index++)
            {
                if (tick == nextPayoutTicks[index])
                {
                    long interval = policies[index].IntervalTicks;
                    nextPayoutTicks[index] = nextPayoutTicks[index] > long.MaxValue - interval
                        ? long.MaxValue
                        : nextPayoutTicks[index] + interval;
                }
            }

            LastProcessedTick = tick;
            return new DividendTickResult(tick, payoutCount, totalAmount);
        }
    }
}
