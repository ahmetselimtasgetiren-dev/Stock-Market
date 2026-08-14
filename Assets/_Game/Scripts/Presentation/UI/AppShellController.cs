using System;
using StockMarket.Domain.Navigation;
using StockMarket.Presentation.Runtime;
using UnityEngine.UIElements;

namespace StockMarket.Presentation.UI
{
    internal sealed class AppShellController : IDisposable
    {
        private readonly StockMarketRuntime runtime;
        private readonly Button marketButton;
        private readonly Button portfolioButton;
        private readonly Button upgradesButton;
        private readonly VisualElement marketPage;
        private readonly VisualElement portfolioPage;
        private readonly VisualElement upgradesPage;
        private readonly Label cashLabel;
        private readonly Label netWorthLabel;
        private readonly Label tickLabel;
        private readonly MarketScreenController marketScreen;
        private readonly PortfolioScreenController portfolioScreen;
        private readonly UpgradeScreenController upgradeScreen;

        public AppShellController(UIDocument document, StockMarketRuntime runtime)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            VisualElement root = document.rootVisualElement;
            marketButton = Require<Button>(root, "nav-market");
            portfolioButton = Require<Button>(root, "nav-portfolio");
            upgradesButton = Require<Button>(root, "nav-upgrades");
            marketPage = Require<VisualElement>(root, "market-page");
            portfolioPage = Require<VisualElement>(root, "portfolio-page");
            upgradesPage = Require<VisualElement>(root, "upgrades-page");
            cashLabel = Require<Label>(root, "top-cash");
            netWorthLabel = Require<Label>(root, "top-net-worth");
            tickLabel = Require<Label>(root, "top-tick");

            marketScreen = new MarketScreenController(root, runtime);
            portfolioScreen = new PortfolioScreenController(root, runtime);
            upgradeScreen = new UpgradeScreenController(root, runtime);

            marketButton.clicked += NavigateToMarket;
            portfolioButton.clicked += NavigateToPortfolio;
            upgradesButton.clicked += NavigateToUpgrades;
            runtime.Navigation.Changed += RefreshNavigation;
            runtime.StateChanged += RefreshState;

            RefreshNavigation();
            RefreshState();
        }

        public void Dispose()
        {
            marketButton.clicked -= NavigateToMarket;
            portfolioButton.clicked -= NavigateToPortfolio;
            upgradesButton.clicked -= NavigateToUpgrades;
            runtime.Navigation.Changed -= RefreshNavigation;
            runtime.StateChanged -= RefreshState;
            marketScreen.Dispose();
            portfolioScreen.Dispose();
            upgradeScreen.Dispose();
        }

        private void NavigateToMarket() => runtime.Navigation.NavigateTo(GameScreen.Market);

        private void NavigateToPortfolio() => runtime.Navigation.NavigateTo(GameScreen.Portfolio);

        private void NavigateToUpgrades() => runtime.Navigation.NavigateTo(GameScreen.Upgrades);

        private void RefreshNavigation()
        {
            GameScreen current = runtime.Navigation.CurrentScreen;
            marketPage.style.display = current == GameScreen.Market ? DisplayStyle.Flex : DisplayStyle.None;
            portfolioPage.style.display = current == GameScreen.Portfolio ? DisplayStyle.Flex : DisplayStyle.None;
            upgradesPage.style.display = current == GameScreen.Upgrades ? DisplayStyle.Flex : DisplayStyle.None;
            marketButton.EnableInClassList("nav-button--active", current == GameScreen.Market);
            portfolioButton.EnableInClassList("nav-button--active", current == GameScreen.Portfolio);
            upgradesButton.EnableInClassList("nav-button--active", current == GameScreen.Upgrades);
        }

        private void RefreshState()
        {
            cashLabel.text = UiFormat.Money(runtime.Player.CashMinorUnits);
            netWorthLabel.text = UiFormat.Money(runtime.Valuation.NetWorthMinorUnits);
            tickLabel.text = $"Market tick {runtime.Clock.CurrentTick:N0}";
            marketScreen.Refresh();
            portfolioScreen.Refresh();
            upgradeScreen.Refresh();
        }

        internal static T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            T element = root.Q<T>(name);
            return element ?? throw new InvalidOperationException($"UI element '{name}' is missing.");
        }
    }
}
