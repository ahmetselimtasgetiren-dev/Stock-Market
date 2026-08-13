using System;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Unlocks;

namespace StockMarket.Domain.Trading
{
    /// <summary>
    /// Executes immediate whole-share trades at the current fictional market price.
    /// </summary>
    public sealed class TradingService
    {
        private readonly PlayerFinancialState player;
        private readonly MarketState market;
        private readonly TransactionLedger transactions;
        private readonly MarketAccessState access;

        public TradingService(PlayerFinancialState player, MarketState market)
            : this(player, market, new TransactionLedger(100), null)
        {
        }

        public TradingService(
            PlayerFinancialState player,
            MarketState market,
            MarketAccessState access)
            : this(player, market, new TransactionLedger(100), access)
        {
        }

        public TradingService(
            PlayerFinancialState player,
            MarketState market,
            TransactionLedger transactions)
            : this(player, market, transactions, null)
        {
        }

        public TradingService(
            PlayerFinancialState player,
            MarketState market,
            TransactionLedger transactions,
            MarketAccessState access)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.market = market ?? throw new ArgumentNullException(nameof(market));
            this.transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            this.access = access;
        }

        public TransactionLedger Transactions => transactions;

        public TradeResult Buy(string companyId, long quantity)
        {
            return Execute(TradeType.Buy, companyId, quantity);
        }

        public TradeResult Sell(string companyId, long quantity)
        {
            return Execute(TradeType.Sell, companyId, quantity);
        }

        private TradeResult Execute(TradeType tradeType, string companyId, long quantity)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return TradeResult.Failure(
                    tradeType,
                    companyId,
                    quantity,
                    TradeFailureReason.InvalidCompanyId);
            }

            if (quantity <= 0)
            {
                return TradeResult.Failure(
                    tradeType,
                    companyId,
                    quantity,
                    TradeFailureReason.InvalidQuantity);
            }

            if (!market.TryGetCompany(companyId, out CompanyMarketState company))
            {
                return TradeResult.Failure(
                    tradeType,
                    companyId,
                    quantity,
                    TradeFailureReason.UnknownCompany);
            }

            if (access != null && !access.IsCompanyUnlocked(companyId))
            {
                return TradeResult.Failure(
                    tradeType,
                    companyId,
                    quantity,
                    TradeFailureReason.CompanyLocked,
                    company.CurrentPriceMinorUnits,
                    priceTick: company.LastUpdatedTick);
            }

            long unitPriceMinorUnits = company.CurrentPriceMinorUnits;
            long totalValueMinorUnits;

            try
            {
                totalValueMinorUnits = checked(unitPriceMinorUnits * quantity);
            }
            catch (OverflowException)
            {
                return TradeResult.Failure(
                    tradeType,
                    companyId,
                    quantity,
                    TradeFailureReason.ArithmeticOverflow,
                    unitPriceMinorUnits,
                    priceTick: company.LastUpdatedTick);
            }

            return tradeType == TradeType.Buy
                ? ExecuteBuy(company, quantity, totalValueMinorUnits)
                : ExecuteSell(company, quantity, totalValueMinorUnits);
        }

        private TradeResult ExecuteBuy(
            CompanyMarketState company,
            long quantity,
            long totalValueMinorUnits)
        {
            if (totalValueMinorUnits > player.CashMinorUnits)
            {
                return CreateFailure(
                    TradeType.Buy,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.InsufficientCash);
            }

            if (!player.Portfolio.CanAddPurchasedShares(
                    company.CompanyId,
                    quantity,
                    totalValueMinorUnits) ||
                !transactions.CanAppend)
            {
                return CreateFailure(
                    TradeType.Buy,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.ArithmeticOverflow);
            }

            player.TryDebitCash(totalValueMinorUnits);
            player.Portfolio.AddPurchasedShares(company.CompanyId, quantity, totalValueMinorUnits);

            TradeResult result = CreateSuccess(
                TradeType.Buy,
                company,
                quantity,
                totalValueMinorUnits,
                transactions.NextTransactionId,
                costBasisRemovedMinorUnits: 0,
                realizedProfitMinorUnits: 0);
            transactions.Append(result);
            return result;
        }

        private TradeResult ExecuteSell(
            CompanyMarketState company,
            long quantity,
            long totalValueMinorUnits)
        {
            long currentShares = player.Portfolio.GetShareQuantity(company.CompanyId);

            if (quantity > currentShares)
            {
                return CreateFailure(
                    TradeType.Sell,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.InsufficientShares);
            }

            if (totalValueMinorUnits > long.MaxValue - player.CashMinorUnits)
            {
                return CreateFailure(
                    TradeType.Sell,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.ArithmeticOverflow);
            }

            long removedCostBasisMinorUnits = player.Portfolio.CalculateCostBasisForSale(
                company.CompanyId,
                quantity);
            long realizedProfitMinorUnits;

            try
            {
                realizedProfitMinorUnits = checked(totalValueMinorUnits - removedCostBasisMinorUnits);
            }
            catch (OverflowException)
            {
                return CreateFailure(
                    TradeType.Sell,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.ArithmeticOverflow);
            }

            if (!player.CanApplyRealizedProfit(realizedProfitMinorUnits) || !transactions.CanAppend)
            {
                return CreateFailure(
                    TradeType.Sell,
                    company,
                    quantity,
                    totalValueMinorUnits,
                    TradeFailureReason.ArithmeticOverflow);
            }

            player.Portfolio.TryRemoveShares(company.CompanyId, quantity, out long appliedCostBasisMinorUnits);
            player.CreditCash(totalValueMinorUnits);
            player.ApplyRealizedProfit(realizedProfitMinorUnits);

            TradeResult result = CreateSuccess(
                TradeType.Sell,
                company,
                quantity,
                totalValueMinorUnits,
                transactions.NextTransactionId,
                appliedCostBasisMinorUnits,
                realizedProfitMinorUnits);
            transactions.Append(result);
            return result;
        }

        private TradeResult CreateSuccess(
            TradeType tradeType,
            CompanyMarketState company,
            long quantity,
            long totalValueMinorUnits,
            long transactionId,
            long costBasisRemovedMinorUnits,
            long realizedProfitMinorUnits)
        {
            return TradeResult.Success(
                tradeType,
                company.CompanyId,
                quantity,
                company.CurrentPriceMinorUnits,
                totalValueMinorUnits,
                company.LastUpdatedTick,
                player.CashMinorUnits,
                player.Portfolio.GetShareQuantity(company.CompanyId),
                transactionId,
                costBasisRemovedMinorUnits,
                realizedProfitMinorUnits);
        }

        private static TradeResult CreateFailure(
            TradeType tradeType,
            CompanyMarketState company,
            long quantity,
            long totalValueMinorUnits,
            TradeFailureReason failureReason)
        {
            return TradeResult.Failure(
                tradeType,
                company.CompanyId,
                quantity,
                failureReason,
                company.CurrentPriceMinorUnits,
                totalValueMinorUnits,
                company.LastUpdatedTick);
        }
    }
}
