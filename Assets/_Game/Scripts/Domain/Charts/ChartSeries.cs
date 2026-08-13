using System;
using System.Collections.Generic;

namespace StockMarket.Domain.Charts
{
    public sealed class ChartSeries
    {
        internal ChartSeries(string seriesId, ChartPoint[] points)
        {
            SeriesId = seriesId ?? throw new ArgumentNullException(nameof(seriesId));
            Points = Array.AsReadOnly(points ?? throw new ArgumentNullException(nameof(points)));

            if (points.Length == 0)
            {
                MinimumValue = 0d;
                MaximumValue = 0d;
                return;
            }

            MinimumValue = points[0].Value;
            MaximumValue = points[0].Value;

            for (int index = 1; index < points.Length; index++)
            {
                MinimumValue = Math.Min(MinimumValue, points[index].Value);
                MaximumValue = Math.Max(MaximumValue, points[index].Value);
            }
        }

        public string SeriesId { get; }
        public IReadOnlyList<ChartPoint> Points { get; }
        public double MinimumValue { get; private set; }
        public double MaximumValue { get; private set; }
    }
}
