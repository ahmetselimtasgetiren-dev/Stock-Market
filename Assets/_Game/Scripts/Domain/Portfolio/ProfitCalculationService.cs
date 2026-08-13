using System;
using StockMarket.Domain.Market;

namespace StockMarket.Domain.Portfolio
{
    public sealed class ProfitCalculationService
    {
        public PortfolioPerformance Calculate(PlayerFinancialState player, MarketState market)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (market == null) throw new ArgumentNullException(nameof(market));

            long holdingsValue = 0;
            long costBasis = 0;

            checked
            {
                for (int index = 0; index < player.Portfolio.Positions.Count; index++)
                {
                    PositionState position = player.Portfolio.Positions[index];
                    long price = market.GetCompany(position.CompanyId).CurrentPriceMinorUnits;
                    holdingsValue += price * position.ShareQuantity;
                    costBasis += position.TotalCostBasisMinorUnits;
                }

                long unrealizedProfit = holdingsValue - costBasis;
                long totalProfit = player.RealizedProfitMinorUnits + unrealizedProfit;
                return new PortfolioPerformance(
                    holdingsValue,
                    costBasis,
                    unrealizedProfit,
                    player.RealizedProfitMinorUnits,
                    totalProfit);
            }
        }
    }
}
