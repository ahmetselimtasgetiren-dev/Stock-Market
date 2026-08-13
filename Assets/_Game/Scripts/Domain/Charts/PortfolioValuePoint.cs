using System;

namespace StockMarket.Domain.Charts
{
    public readonly struct PortfolioValuePoint
    {
        public PortfolioValuePoint(long tick, long netWorthMinorUnits)
        {
            if (tick < 0 || netWorthMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            Tick = tick;
            NetWorthMinorUnits = netWorthMinorUnits;
        }

        public long Tick { get; }
        public long NetWorthMinorUnits { get; }
    }
}
