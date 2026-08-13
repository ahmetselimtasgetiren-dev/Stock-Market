using System;
using StockMarket.Domain.Market;

namespace StockMarket.Domain.Portfolio
{
    public sealed class PortfolioValuationService
    {
        public PortfolioValuation Calculate(PlayerFinancialState player, MarketState market)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (market == null)
            {
                throw new ArgumentNullException(nameof(market));
            }

            long holdingsValueMinorUnits = 0;

            checked
            {
                for (int index = 0; index < player.Portfolio.Positions.Count; index++)
                {
                    PositionState position = player.Portfolio.Positions[index];
                    long currentPriceMinorUnits = market.GetCompany(position.CompanyId).CurrentPriceMinorUnits;
                    long positionValueMinorUnits = currentPriceMinorUnits * position.ShareQuantity;
                    holdingsValueMinorUnits += positionValueMinorUnits;
                }

                long netWorthMinorUnits = player.CashMinorUnits + holdingsValueMinorUnits;
                return new PortfolioValuation(
                    player.CashMinorUnits,
                    holdingsValueMinorUnits,
                    netWorthMinorUnits);
            }
        }
    }
}
