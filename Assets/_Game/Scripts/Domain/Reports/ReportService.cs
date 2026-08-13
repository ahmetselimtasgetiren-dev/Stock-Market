using System;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;

namespace StockMarket.Domain.Reports
{
    public sealed class ReportService
    {
        private readonly PlayerFinancialState player;
        private readonly MarketState market;
        private readonly TransactionLedger transactions;
        private readonly ProfitCalculationService profit;

        public ReportService(PlayerFinancialState player, MarketState market, TransactionLedger transactions)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.market = market ?? throw new ArgumentNullException(nameof(market));
            this.transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            profit = new ProfitCalculationService();
        }

        public ReportSnapshot Build()
        {
            PortfolioPerformance performance = profit.Calculate(player, market);
            int buys = 0;
            int sells = 0;
            int profitableSells = 0;

            for (int index = 0; index < transactions.Count; index++)
            {
                TransactionRecord record = transactions[index];

                if (record.TradeType == TradeType.Buy)
                {
                    buys++;
                }
                else
                {
                    sells++;

                    if (record.RealizedProfitMinorUnits > 0)
                    {
                        profitableSells++;
                    }
                }
            }

            string bestCompany = null;
            long bestGain = 0;

            foreach (PositionState position in player.Portfolio.Positions)
            {
                long value = checked(position.ShareQuantity * market.GetCompany(position.CompanyId).CurrentPriceMinorUnits);
                long gain = checked(value - position.TotalCostBasisMinorUnits);

                if (bestCompany == null || gain > bestGain)
                {
                    bestCompany = position.CompanyId;
                    bestGain = gain;
                }
            }

            return new ReportSnapshot(
                player.CashMinorUnits,
                performance.HoldingsValueMinorUnits,
                checked(player.CashMinorUnits + performance.HoldingsValueMinorUnits),
                performance.CostBasisMinorUnits,
                performance.RealizedProfitMinorUnits,
                performance.UnrealizedProfitMinorUnits,
                player.DividendIncomeMinorUnits,
                transactions.Count,
                buys,
                sells,
                profitableSells,
                bestCompany,
                bestGain);
        }
    }
}
