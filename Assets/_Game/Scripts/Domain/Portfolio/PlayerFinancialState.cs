using System;

namespace StockMarket.Domain.Portfolio
{
    /// <summary>
    /// Authoritative owner of player cash and whole-share positions.
    /// </summary>
    public sealed class PlayerFinancialState
    {
        public PlayerFinancialState(long startingCashMinorUnits)
        {
            if (startingCashMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingCashMinorUnits),
                    "Starting cash cannot be negative.");
            }

            CashMinorUnits = startingCashMinorUnits;
            Portfolio = new PortfolioState();
        }

        public long CashMinorUnits { get; private set; }

        public long RealizedProfitMinorUnits { get; private set; }

        public long DividendIncomeMinorUnits { get; private set; }

        public PortfolioState Portfolio { get; }

        internal bool TryDebitCash(long amountMinorUnits)
        {
            ValidatePositiveAmount(amountMinorUnits);

            if (amountMinorUnits > CashMinorUnits)
            {
                return false;
            }

            CashMinorUnits -= amountMinorUnits;
            return true;
        }

        internal void CreditCash(long amountMinorUnits)
        {
            ValidatePositiveAmount(amountMinorUnits);
            CashMinorUnits = checked(CashMinorUnits + amountMinorUnits);
        }

        internal bool CanApplyRealizedProfit(long profitMinorUnits)
        {
            try
            {
                checked
                {
                    _ = RealizedProfitMinorUnits + profitMinorUnits;
                }

                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        internal void ApplyRealizedProfit(long profitMinorUnits)
        {
            RealizedProfitMinorUnits = checked(RealizedProfitMinorUnits + profitMinorUnits);
        }

        internal bool CanReceiveDividendIncome(long amountMinorUnits)
        {
            if (amountMinorUnits <= 0)
            {
                return false;
            }

            try
            {
                checked
                {
                    _ = CashMinorUnits + amountMinorUnits;
                    _ = DividendIncomeMinorUnits + amountMinorUnits;
                }

                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        internal void ReceiveDividendIncome(long amountMinorUnits)
        {
            if (!CanReceiveDividendIncome(amountMinorUnits))
            {
                throw new OverflowException("Dividend income cannot be applied.");
            }

            CashMinorUnits = checked(CashMinorUnits + amountMinorUnits);
            DividendIncomeMinorUnits = checked(DividendIncomeMinorUnits + amountMinorUnits);
        }

        private static void ValidatePositiveAmount(long amountMinorUnits)
        {
            if (amountMinorUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amountMinorUnits),
                    "Cash amount must be positive.");
            }
        }
    }
}
