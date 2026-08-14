using System;
using StockMarket.Content.Definitions;
using StockMarket.Domain.Market;
using StockMarket.Domain.Portfolio;
using StockMarket.Domain.Trading;
using StockMarket.Presentation.Runtime;
using UnityEngine.UIElements;

namespace StockMarket.Presentation.UI
{
    internal sealed class PortfolioScreenController : IDisposable
    {
        private readonly StockMarketRuntime runtime;
        private readonly Label totalValue;
        private readonly Label cash;
        private readonly Label holdingsValue;
        private readonly Label totalProfit;
        private readonly Label unrealizedProfit;
        private readonly Label realizedProfit;
        private readonly VisualElement holdings;
        private readonly VisualElement transactions;
        private readonly Label holdingsEmpty;
        private readonly Label transactionsEmpty;

        public PortfolioScreenController(VisualElement root, StockMarketRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            totalValue = AppShellController.Require<Label>(root, "portfolio-total-value");
            cash = AppShellController.Require<Label>(root, "portfolio-cash");
            holdingsValue = AppShellController.Require<Label>(root, "portfolio-holdings-value");
            totalProfit = AppShellController.Require<Label>(root, "portfolio-total-profit");
            unrealizedProfit = AppShellController.Require<Label>(root, "portfolio-unrealized-profit");
            realizedProfit = AppShellController.Require<Label>(root, "portfolio-realized-profit");
            holdings = AppShellController.Require<VisualElement>(root, "holdings-list");
            transactions = AppShellController.Require<VisualElement>(root, "transaction-list");
            holdingsEmpty = AppShellController.Require<Label>(root, "holdings-empty");
            transactionsEmpty = AppShellController.Require<Label>(root, "transactions-empty");
        }

        public void Dispose()
        {
        }

        public void Refresh()
        {
            PortfolioValuation valuation = runtime.Valuation;
            PortfolioPerformance performance = runtime.Performance;
            totalValue.text = UiFormat.Money(valuation.NetWorthMinorUnits);
            cash.text = UiFormat.Money(valuation.CashMinorUnits);
            holdingsValue.text = UiFormat.Money(valuation.HoldingsValueMinorUnits);
            SetSignedValue(totalProfit, performance.TotalProfitMinorUnits);
            SetSignedValue(unrealizedProfit, performance.UnrealizedProfitMinorUnits);
            SetSignedValue(realizedProfit, performance.RealizedProfitMinorUnits);
            RebuildHoldings();
            RebuildTransactions();
        }

        private void RebuildHoldings()
        {
            holdings.Clear();
            holdingsEmpty.style.display = runtime.Player.Portfolio.Positions.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            for (int index = 0; index < runtime.Player.Portfolio.Positions.Count; index++)
            {
                PositionState position = runtime.Player.Portfolio.Positions[index];
                CompanyDefinition company = runtime.GetCompanyDefinition(position.CompanyId);
                CompanyMarketState market = runtime.Market.GetCompany(position.CompanyId);
                long marketValue = checked(position.ShareQuantity * market.CurrentPriceMinorUnits);
                long profit = marketValue - position.TotalCostBasisMinorUnits;

                var row = new VisualElement();
                row.AddToClassList("table-row");
                row.Add(CreateCell($"{company.Ticker}\n{company.DisplayName}", "table-cell--company"));
                row.Add(CreateCell(position.ShareQuantity.ToString("N0")));
                row.Add(CreateCell(UiFormat.Money(decimal.ToInt64(decimal.Round(position.AverageBuyPriceMinorUnits)))));
                row.Add(CreateCell(UiFormat.Money(market.CurrentPriceMinorUnits)));
                row.Add(CreateCell(UiFormat.Money(marketValue)));
                Label profitCell = CreateCell(UiFormat.SignedMoney(profit));
                profitCell.EnableInClassList("value--gain", profit >= 0);
                profitCell.EnableInClassList("value--loss", profit < 0);
                row.Add(profitCell);
                holdings.Add(row);
            }
        }

        private void RebuildTransactions()
        {
            transactions.Clear();
            TransactionLedger ledger = runtime.Trading.Transactions;
            transactionsEmpty.style.display = ledger.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            int firstIndex = Math.Max(0, ledger.Count - 6);

            for (int index = ledger.Count - 1; index >= firstIndex; index--)
            {
                TransactionRecord transaction = ledger[index];
                CompanyDefinition company = runtime.GetCompanyDefinition(transaction.CompanyId);
                var row = new VisualElement();
                row.AddToClassList("transaction-row");

                var badge = new Label(transaction.TradeType == TradeType.Buy ? "BUY" : "SELL");
                badge.AddToClassList("transaction-badge");
                badge.AddToClassList(transaction.TradeType == TradeType.Buy ? "transaction-badge--buy" : "transaction-badge--sell");
                row.Add(badge);

                var details = new Label($"{company.Ticker}  •  {transaction.Quantity:N0} shares\nTick {transaction.PriceTick:N0}");
                details.AddToClassList("transaction-details");
                row.Add(details);

                var value = new Label(UiFormat.Money(transaction.TotalValueMinorUnits));
                value.AddToClassList("transaction-value");
                row.Add(value);
                transactions.Add(row);
            }
        }

        private static Label CreateCell(string text, string extraClass = null)
        {
            var label = new Label(text);
            label.AddToClassList("table-cell");

            if (!string.IsNullOrEmpty(extraClass))
            {
                label.AddToClassList(extraClass);
            }

            return label;
        }

        private static void SetSignedValue(Label label, long value)
        {
            label.text = UiFormat.SignedMoney(value);
            label.EnableInClassList("value--gain", value >= 0);
            label.EnableInClassList("value--loss", value < 0);
        }
    }
}
