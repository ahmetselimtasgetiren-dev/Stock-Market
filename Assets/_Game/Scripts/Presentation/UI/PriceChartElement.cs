using StockMarket.Domain.Charts;
using UnityEngine;
using UnityEngine.UIElements;

namespace StockMarket.Presentation.UI
{
    internal sealed class PriceChartElement : VisualElement
    {
        private ChartSeries series;
        private Color lineColor = new Color(0.17f, 0.76f, 0.64f, 1f);

        public PriceChartElement()
        {
            name = "live-price-chart";
            AddToClassList("live-chart");
            generateVisualContent += Draw;
        }

        public void SetSeries(ChartSeries value, bool isGain)
        {
            series = value;
            lineColor = isGain
                ? new Color(0.17f, 0.76f, 0.64f, 1f)
                : new Color(0.96f, 0.44f, 0.44f, 1f);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect bounds = contentRect;

            if (bounds.width <= 1f || bounds.height <= 1f)
            {
                return;
            }

            Painter2D painter = context.painter2D;
            painter.lineWidth = 1f;
            painter.strokeColor = new Color(0.88f, 0.90f, 0.95f, 1f);

            for (int line = 1; line < 4; line++)
            {
                float y = bounds.yMin + (bounds.height * line / 4f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(bounds.xMin, y));
                painter.LineTo(new Vector2(bounds.xMax, y));
                painter.Stroke();
            }

            if (series == null || series.Points.Count == 0)
            {
                return;
            }

            double range = series.MaximumValue - series.MinimumValue;
            double safeRange = range <= 0d ? 1d : range;
            float horizontalStep = series.Points.Count == 1
                ? 0f
                : bounds.width / (series.Points.Count - 1);

            painter.lineWidth = 3f;
            painter.strokeColor = lineColor;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            painter.BeginPath();

            for (int index = 0; index < series.Points.Count; index++)
            {
                float x = bounds.xMin + (horizontalStep * index);
                double normalized = (series.Points[index].Value - series.MinimumValue) / safeRange;
                float y = bounds.yMax - 12f - ((float)normalized * (bounds.height - 24f));
                var point = new Vector2(x, y);

                if (index == 0)
                {
                    painter.MoveTo(point);
                }
                else
                {
                    painter.LineTo(point);
                }
            }

            painter.Stroke();
        }
    }
}
