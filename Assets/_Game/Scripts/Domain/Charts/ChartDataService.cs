using System;
using System.Collections.Generic;
using StockMarket.Domain.Market;

namespace StockMarket.Domain.Charts
{
    public sealed class ChartDataService
    {
        public ChartSeries BuildPriceSeries(CompanyMarketState company, int maximumPoints)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            return BuildSeries(
                company.CompanyId,
                company.PriceHistory.Count,
                maximumPoints,
                index => new ChartPoint(
                    company.PriceHistory[index].Tick,
                    company.PriceHistory[index].PriceMinorUnits));
        }

        public ChartSeries BuildPortfolioSeries(PortfolioValueHistory history, int maximumPoints)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            return BuildSeries(
                "portfolio_net_worth",
                history.Count,
                maximumPoints,
                index => new ChartPoint(history[index].Tick, history[index].NetWorthMinorUnits));
        }

        private static ChartSeries BuildSeries(
            string id,
            int sourceCount,
            int maximumPoints,
            Func<int, ChartPoint> read)
        {
            if (maximumPoints < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumPoints));
            }

            if (sourceCount <= maximumPoints)
            {
                var all = new ChartPoint[sourceCount];

                for (int index = 0; index < sourceCount; index++)
                {
                    all[index] = read(index);
                }

                return new ChartSeries(id, all);
            }

            var points = new List<ChartPoint>(maximumPoints) { read(0) };
            double interval = (double)(sourceCount - 1) / (maximumPoints - 1);

            for (int outputIndex = 1; outputIndex < maximumPoints - 1; outputIndex++)
            {
                int sourceIndex = (int)Math.Round(outputIndex * interval, MidpointRounding.AwayFromZero);
                points.Add(read(sourceIndex));
            }

            points.Add(read(sourceCount - 1));
            return new ChartSeries(id, points.ToArray());
        }
    }
}
