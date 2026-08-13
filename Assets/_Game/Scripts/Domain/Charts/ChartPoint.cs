using System;

namespace StockMarket.Domain.Charts
{
    public readonly struct ChartPoint
    {
        public ChartPoint(long tick, double value)
        {
            if (tick < 0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            Tick = tick;
            Value = value;
        }

        public long Tick { get; }
        public double Value { get; }
    }
}
