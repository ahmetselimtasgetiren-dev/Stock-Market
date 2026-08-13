using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Portfolio
{
    public sealed class PortfolioState
    {
        private readonly List<PositionState> positions = new List<PositionState>();
        private readonly IReadOnlyList<PositionState> readOnlyPositions;
        private readonly Dictionary<string, PositionState> positionsByCompanyId =
            new Dictionary<string, PositionState>(StringComparer.Ordinal);

        public PortfolioState()
        {
            readOnlyPositions = positions.AsReadOnly();
        }

        public IReadOnlyList<PositionState> Positions => readOnlyPositions;

        public long GetShareQuantity(string companyId)
        {
            return TryGetPosition(companyId, out PositionState position)
                ? position.ShareQuantity
                : 0;
        }

        public bool TryGetPosition(string companyId, out PositionState position)
        {
            if (companyId == null)
            {
                position = null;
                return false;
            }

            return positionsByCompanyId.TryGetValue(companyId, out position);
        }

        internal void AddShares(string companyId, long quantity)
        {
            AddPurchasedShares(companyId, quantity, 0);
        }

        internal bool CanAddPurchasedShares(string companyId, long quantity, long costMinorUnits)
        {
            ValidateCompanyId(companyId);

            if (positionsByCompanyId.TryGetValue(companyId, out PositionState position))
            {
                return position.CanAddPurchasedShares(quantity, costMinorUnits);
            }

            var newPosition = new PositionState(companyId);
            return newPosition.CanAddPurchasedShares(quantity, costMinorUnits);
        }

        internal void AddPurchasedShares(string companyId, long quantity, long costMinorUnits)
        {
            ValidateCompanyId(companyId);

            if (!positionsByCompanyId.TryGetValue(companyId, out PositionState position))
            {
                position = new PositionState(companyId);
                position.AddPurchasedShares(quantity, costMinorUnits);
                positions.Add(position);
                positionsByCompanyId.Add(companyId, position);
                return;
            }

            position.AddPurchasedShares(quantity, costMinorUnits);
        }

        internal bool TryRemoveShares(string companyId, long quantity)
        {
            return TryRemoveShares(companyId, quantity, out _);
        }

        internal bool TryRemoveShares(
            string companyId,
            long quantity,
            out long removedCostBasisMinorUnits)
        {
            ValidateCompanyId(companyId);

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Share quantity must be positive.");
            }

            if (!positionsByCompanyId.TryGetValue(companyId, out PositionState position) ||
                !position.TryRemoveShares(quantity, out removedCostBasisMinorUnits))
            {
                removedCostBasisMinorUnits = 0;
                return false;
            }

            if (position.ShareQuantity == 0)
            {
                positionsByCompanyId.Remove(companyId);
                positions.Remove(position);
            }

            return true;
        }

        internal long CalculateCostBasisForSale(string companyId, long quantity)
        {
            ValidateCompanyId(companyId);

            if (!positionsByCompanyId.TryGetValue(companyId, out PositionState position))
            {
                throw new InvalidOperationException($"No position exists for company ID '{companyId}'.");
            }

            return position.CalculateCostBasisForSale(quantity);
        }

        private static void ValidateCompanyId(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }
        }
    }
}
