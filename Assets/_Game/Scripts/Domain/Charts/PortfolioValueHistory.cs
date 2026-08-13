using System;

namespace StockMarket.Domain.Charts
{
    public sealed class PortfolioValueHistory
    {
        private readonly PortfolioValuePoint[] points;
        private int oldestIndex;

        public PortfolioValueHistory(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            points = new PortfolioValuePoint[capacity];
        }

        public int Capacity => points.Length;
        public int Count { get; private set; }
        public PortfolioValuePoint Oldest => this[0];
        public PortfolioValuePoint Latest => this[Count - 1];

        public PortfolioValuePoint this[int chronologicalIndex]
        {
            get
            {
                if (chronologicalIndex < 0 || chronologicalIndex >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(chronologicalIndex));
                }

                return points[(oldestIndex + chronologicalIndex) % points.Length];
            }
        }

        public void Add(long tick, long netWorthMinorUnits)
        {
            var point = new PortfolioValuePoint(tick, netWorthMinorUnits);

            if (Count > 0 && tick <= Latest.Tick)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), "Portfolio history ticks must increase strictly.");
            }

            if (Count < points.Length)
            {
                points[(oldestIndex + Count) % points.Length] = point;
                Count++;
            }
            else
            {
                points[oldestIndex] = point;
                oldestIndex = (oldestIndex + 1) % points.Length;
            }
        }
    }
}
