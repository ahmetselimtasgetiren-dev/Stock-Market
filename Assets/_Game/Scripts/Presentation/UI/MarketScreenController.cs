using System;
using System.Globalization;
using StockMarket.Content.Definitions;
using StockMarket.Domain.Charts;
using StockMarket.Domain.Market;
using StockMarket.Domain.Trading;
using StockMarket.Presentation.Runtime;
using UnityEngine.UIElements;

namespace StockMarket.Presentation.UI
{
    internal sealed class MarketScreenController : IDisposable
    {
        private readonly StockMarketRuntime runtime;
        private readonly VisualElement companyList;
        private readonly Label companyName;
        private readonly Label companyMeta;
        private readonly Label price;
        private readonly Label change;
        private readonly Label ownedShares;
        private readonly Label positionValue;
        private readonly TextField quantity;
        private readonly Button buyButton;
        private readonly Button sellButton;
        private readonly Label tradeEstimate;
        private readonly Label feedback;
        private readonly PriceChartElement chart = new PriceChartElement();
        private string selectedCompanyId;

        public MarketScreenController(VisualElement root, StockMarketRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            companyList = AppShellController.Require<VisualElement>(root, "market-company-list");
            companyName = AppShellController.Require<Label>(root, "selected-company-name");
            companyMeta = AppShellController.Require<Label>(root, "selected-company-meta");
            price = AppShellController.Require<Label>(root, "selected-company-price");
            change = AppShellController.Require<Label>(root, "selected-company-change");
            ownedShares = AppShellController.Require<Label>(root, "owned-shares");
            positionValue = AppShellController.Require<Label>(root, "position-value");
            quantity = AppShellController.Require<TextField>(root, "trade-quantity");
            buyButton = AppShellController.Require<Button>(root, "buy-button");
            sellButton = AppShellController.Require<Button>(root, "sell-button");
            tradeEstimate = AppShellController.Require<Label>(root, "trade-estimate");
            feedback = AppShellController.Require<Label>(root, "trade-feedback");

            AppShellController.Require<VisualElement>(root, "price-chart-host").Add(chart);
            selectedCompanyId = runtime.CompanyCatalog.Companies[0].Id;
            BuildCompanyList();

            buyButton.clicked += Buy;
            sellButton.clicked += Sell;
            quantity.RegisterValueChangedCallback(QuantityChanged);
        }

        public void Dispose()
        {
            buyButton.clicked -= Buy;
            sellButton.clicked -= Sell;
            quantity.UnregisterValueChangedCallback(QuantityChanged);
        }

        public void Refresh()
        {
            for (int index = 0; index < companyList.childCount; index++)
            {
                Button row = (Button)companyList[index];
                string companyId = row.userData as string;
                CompanyDefinition definition = runtime.GetCompanyDefinition(companyId);
                CompanyMarketState state = runtime.Market.GetCompany(companyId);
                row.text = $"{definition.Ticker}\n{definition.DisplayName}\n{UiFormat.Money(state.CurrentPriceMinorUnits)}   {UiFormat.Percent(state.PriceChangeRatio)}";
                row.EnableInClassList("company-row--selected", companyId == selectedCompanyId);
            }

            CompanyDefinition selected = runtime.GetCompanyDefinition(selectedCompanyId);
            CompanyMarketState marketState = runtime.Market.GetCompany(selectedCompanyId);
            long shares = runtime.Player.Portfolio.GetShareQuantity(selectedCompanyId);
            long value = checked(shares * marketState.CurrentPriceMinorUnits);

            companyName.text = selected.DisplayName;
            companyMeta.text = $"{selected.Ticker}  •  {selected.Sector.DisplayName}";
            price.text = UiFormat.Money(marketState.CurrentPriceMinorUnits);
            change.text = UiFormat.Percent(marketState.PriceChangeRatio);
            change.EnableInClassList("value--gain", marketState.PriceChangeMinorUnits >= 0);
            change.EnableInClassList("value--loss", marketState.PriceChangeMinorUnits < 0);
            ownedShares.text = shares.ToString("N0", CultureInfo.InvariantCulture);
            positionValue.text = UiFormat.Money(value);

            ChartSeries series = runtime.Charts.BuildPriceSeries(marketState, maximumPoints: 80);
            chart.SetSeries(series, marketState.PriceChangeMinorUnits >= 0);
            RefreshEstimate();
        }

        private void BuildCompanyList()
        {
            companyList.Clear();

            for (int index = 0; index < runtime.CompanyCatalog.Companies.Count; index++)
            {
                CompanyDefinition company = runtime.CompanyCatalog.Companies[index];
                var row = new Button { userData = company.Id };
                row.AddToClassList("company-row");
                row.AddToClassList(index % 3 == 0
                    ? "company-row--lavender"
                    : index % 3 == 1
                        ? "company-row--mint"
                        : "company-row--peach");
                row.clicked += () => SelectCompany((string)row.userData);
                companyList.Add(row);
            }
        }

        private void SelectCompany(string companyId)
        {
            selectedCompanyId = companyId;
            feedback.text = string.Empty;
            Refresh();
        }

        private void Buy()
        {
            SubmitTrade(isBuy: true);
        }

        private void Sell()
        {
            SubmitTrade(isBuy: false);
        }

        private void SubmitTrade(bool isBuy)
        {
            if (!TryReadQuantity(out long amount))
            {
                ShowFeedback("Enter a whole-share quantity of 1 or more.", isSuccess: false);
                return;
            }

            TradeResult result = isBuy
                ? runtime.Buy(selectedCompanyId, amount)
                : runtime.Sell(selectedCompanyId, amount);

            if (result.Succeeded)
            {
                string verb = isBuy ? "Bought" : "Sold";
                ShowFeedback($"{verb} {amount:N0} shares for {UiFormat.Money(result.TotalValueMinorUnits)}.", true);
                return;
            }

            ShowFeedback(FailureMessage(result.FailureReason), false);
        }

        private void QuantityChanged(ChangeEvent<string> changeEvent)
        {
            RefreshEstimate();
        }

        private void RefreshEstimate()
        {
            if (!TryReadQuantity(out long amount))
            {
                tradeEstimate.text = "Enter a whole-share amount.";
                buyButton.SetEnabled(false);
                sellButton.SetEnabled(false);
                return;
            }

            long unitPrice = runtime.Market.GetCompany(selectedCompanyId).CurrentPriceMinorUnits;
            long total;

            try
            {
                total = checked(unitPrice * amount);
            }
            catch (OverflowException)
            {
                tradeEstimate.text = "That order is too large.";
                buyButton.SetEnabled(false);
                sellButton.SetEnabled(false);
                return;
            }

            tradeEstimate.text = $"Estimated order value  {UiFormat.Money(total)}";
            buyButton.SetEnabled(true);
            sellButton.SetEnabled(true);
        }

        private bool TryReadQuantity(out long amount)
        {
            return long.TryParse(quantity.value, NumberStyles.None, CultureInfo.InvariantCulture, out amount) && amount > 0;
        }

        private void ShowFeedback(string message, bool isSuccess)
        {
            feedback.text = message;
            feedback.EnableInClassList("feedback--success", isSuccess);
            feedback.EnableInClassList("feedback--error", !isSuccess);
        }

        private static string FailureMessage(TradeFailureReason reason)
        {
            switch (reason)
            {
                case TradeFailureReason.InsufficientCash:
                    return "Not enough fictional credits for this purchase.";
                case TradeFailureReason.InsufficientShares:
                    return "You do not own enough shares to sell that amount.";
                case TradeFailureReason.CompanyLocked:
                    return "This company is still locked.";
                default:
                    return "The trade could not be completed. Check the quantity and try again.";
            }
        }
    }
}
