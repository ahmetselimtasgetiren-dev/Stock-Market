using System;

namespace StockMarket.Domain.Portfolio
{
    public sealed class PositionState
    {
        internal PositionState(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                throw new ArgumentException("Company ID is required.", nameof(companyId));
            }

            CompanyId = companyId;
        }

        public string CompanyId { get; }

        public long ShareQuantity { get; private set; }

        public long TotalCostBasisMinorUnits { get; private set; }

        public decimal AverageBuyPriceMinorUnits => ShareQuantity == 0
            ? 0m
            : (decimal)TotalCostBasisMinorUnits / ShareQuantity;

        internal void AddShares(long quantity)
        {
            AddPurchasedShares(quantity, 0);
        }

        internal bool CanAddPurchasedShares(long quantity, long costMinorUnits)
        {
            ValidatePositiveQuantity(quantity);

            if (costMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costMinorUnits));
            }

            return quantity <= long.MaxValue - ShareQuantity &&
                   costMinorUnits <= long.MaxValue - TotalCostBasisMinorUnits;
        }

        internal void AddPurchasedShares(long quantity, long costMinorUnits)
        {
            if (!CanAddPurchasedShares(quantity, costMinorUnits))
            {
                throw new OverflowException("Position quantity or cost basis would overflow.");
            }

            ShareQuantity += quantity;
            TotalCostBasisMinorUnits += costMinorUnits;
        }

        internal bool TryRemoveShares(long quantity)
        {
            return TryRemoveShares(quantity, out _);
        }

        internal bool TryRemoveShares(long quantity, out long removedCostBasisMinorUnits)
        {
            ValidatePositiveQuantity(quantity);

            if (quantity > ShareQuantity)
            {
                removedCostBasisMinorUnits = 0;
                return false;
            }

            removedCostBasisMinorUnits = CalculateCostBasisForSale(quantity);
            ShareQuantity -= quantity;
            TotalCostBasisMinorUnits -= removedCostBasisMinorUnits;
            return true;
        }

        internal long CalculateCostBasisForSale(long quantity)
        {
            ValidatePositiveQuantity(quantity);

            if (quantity > ShareQuantity)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot calculate basis for unavailable shares.");
            }

            if (quantity == ShareQuantity)
            {
                return TotalCostBasisMinorUnits;
            }

            decimal proportionalCost =
                ((decimal)TotalCostBasisMinorUnits / ShareQuantity) * quantity;
            return decimal.ToInt64(decimal.Round(proportionalCost, 0, MidpointRounding.AwayFromZero));
        }

        private static void ValidatePositiveQuantity(long quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Share quantity must be positive.");
            }
        }
    }
}
