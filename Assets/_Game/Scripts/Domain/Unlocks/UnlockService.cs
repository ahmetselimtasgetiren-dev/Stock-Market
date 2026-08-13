using System;
using System.Collections.Generic;
using StockMarket.Domain.Portfolio;

namespace StockMarket.Domain.Unlocks
{
    public sealed class UnlockService
    {
        private readonly PlayerFinancialState player;
        private readonly MarketAccessState access;
        private readonly Dictionary<string, UnlockSpec> specsById =
            new Dictionary<string, UnlockSpec>(StringComparer.Ordinal);
        private readonly HashSet<string> targetKeys = new HashSet<string>(StringComparer.Ordinal);

        public UnlockService(
            PlayerFinancialState player,
            MarketAccessState access,
            IEnumerable<UnlockSpec> specs)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.access = access ?? throw new ArgumentNullException(nameof(access));

            if (specs == null)
            {
                throw new ArgumentNullException(nameof(specs));
            }

            foreach (UnlockSpec spec in specs)
            {
                if (spec == null)
                {
                    throw new ArgumentException("Unlock specs contain a missing entry.", nameof(specs));
                }

                if (!specsById.TryAdd(spec.Id, spec))
                {
                    throw new ArgumentException($"Duplicate unlock ID '{spec.Id}'.", nameof(specs));
                }

                string targetKey = $"{spec.TargetScope}:{spec.TargetId}";

                if (!targetKeys.Add(targetKey))
                {
                    throw new ArgumentException($"Duplicate unlock target '{targetKey}'.", nameof(specs));
                }
            }
        }

        public MarketAccessState Access => access;

        public UnlockResult Purchase(string unlockId)
        {
            if (string.IsNullOrWhiteSpace(unlockId))
            {
                return UnlockResult.Failure(unlockId, UnlockFailureReason.InvalidUnlockId);
            }

            if (!specsById.TryGetValue(unlockId, out UnlockSpec spec))
            {
                return UnlockResult.Failure(unlockId, UnlockFailureReason.UnknownUnlock);
            }

            bool alreadyUnlocked = spec.TargetScope == UnlockTargetScope.Sector
                ? access.IsSectorUnlocked(spec.TargetId)
                : access.IsCompanyUnlocked(spec.TargetId);

            if (alreadyUnlocked)
            {
                return UnlockResult.Failure(unlockId, UnlockFailureReason.AlreadyUnlocked, spec);
            }

            if (spec.TargetScope == UnlockTargetScope.Company &&
                !access.IsSectorUnlocked(spec.RequiredSectorId))
            {
                return UnlockResult.Failure(unlockId, UnlockFailureReason.RequiredSectorLocked, spec);
            }

            if (spec.CostMinorUnits > player.CashMinorUnits)
            {
                return UnlockResult.Failure(unlockId, UnlockFailureReason.InsufficientCash, spec);
            }

            player.TryDebitCash(spec.CostMinorUnits);
            access.Unlock(spec.TargetScope, spec.TargetId);
            return UnlockResult.Success(spec, player.CashMinorUnits);
        }
    }
}
