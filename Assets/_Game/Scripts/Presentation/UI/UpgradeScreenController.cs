using System;
using StockMarket.Content.Definitions;
using StockMarket.Domain.Progression;
using StockMarket.Presentation.Runtime;
using UnityEngine.UIElements;

namespace StockMarket.Presentation.UI
{
    internal sealed class UpgradeScreenController : IDisposable
    {
        private readonly StockMarketRuntime runtime;
        private readonly VisualElement upgradeGrid;
        private readonly Label totalLevels;
        private readonly Label totalSpent;
        private readonly Label feedback;

        public UpgradeScreenController(VisualElement root, StockMarketRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            upgradeGrid = AppShellController.Require<VisualElement>(root, "upgrade-grid");
            totalLevels = AppShellController.Require<Label>(root, "upgrade-total-levels");
            totalSpent = AppShellController.Require<Label>(root, "upgrade-total-spent");
            feedback = AppShellController.Require<Label>(root, "upgrade-feedback");
        }

        public void Dispose()
        {
        }

        public void Refresh()
        {
            int purchasedLevels = 0;

            for (int index = 0; index < runtime.Progression.Upgrades.Count; index++)
            {
                purchasedLevels += runtime.Progression.Upgrades[index].Level;
            }

            totalLevels.text = purchasedLevels.ToString("N0");
            totalSpent.text = UiFormat.Money(runtime.Progression.TotalSpentMinorUnits);
            RebuildCards();
        }

        private void RebuildCards()
        {
            upgradeGrid.Clear();

            for (int index = 0; index < runtime.UpgradeCatalog.Upgrades.Count; index++)
            {
                UpgradeDefinition definition = runtime.UpgradeCatalog.Upgrades[index];
                int level = runtime.Progression.GetLevel(definition.Id);
                bool maximumLevel = level >= definition.MaxLevel;
                long nextCost = maximumLevel ? 0 : runtime.Upgrades.GetNextCost(definition.Id);

                var card = new VisualElement();
                card.AddToClassList("upgrade-card");
                card.AddToClassList(AccentClass(index));

                var heading = new VisualElement();
                heading.AddToClassList("upgrade-card-heading");
                var icon = new Label(IconFor(definition.EffectType));
                icon.AddToClassList("upgrade-icon");
                heading.Add(icon);

                var titleBlock = new VisualElement();
                titleBlock.AddToClassList("upgrade-title-block");
                var title = new Label(definition.DisplayName);
                title.AddToClassList("upgrade-title");
                titleBlock.Add(title);
                var category = new Label(EffectLabel(definition));
                category.AddToClassList("upgrade-category");
                titleBlock.Add(category);
                heading.Add(titleBlock);
                card.Add(heading);

                var description = new Label(definition.Description);
                description.AddToClassList("upgrade-description");
                card.Add(description);

                var levelRow = new VisualElement();
                levelRow.AddToClassList("upgrade-level-row");
                levelRow.Add(new Label($"Level {level} / {definition.MaxLevel}"));
                var progress = new VisualElement();
                progress.AddToClassList("upgrade-progress");
                var progressFill = new VisualElement();
                progressFill.AddToClassList("upgrade-progress-fill");
                progressFill.style.width = Length.Percent((float)level / definition.MaxLevel * 100f);
                progress.Add(progressFill);
                levelRow.Add(progress);
                card.Add(levelRow);

                var purchase = new Button();
                purchase.AddToClassList("upgrade-purchase-button");
                purchase.text = maximumLevel ? "Maximum level" : $"Upgrade  •  {UiFormat.Money(nextCost)}";
                purchase.SetEnabled(!maximumLevel && nextCost <= runtime.Player.CashMinorUnits);
                string upgradeId = definition.Id;
                purchase.clicked += () => Purchase(upgradeId);
                card.Add(purchase);

                if (!maximumLevel && nextCost > runtime.Player.CashMinorUnits)
                {
                    var insufficient = new Label("Not enough fictional credits yet");
                    insufficient.AddToClassList("upgrade-insufficient");
                    card.Add(insufficient);
                }

                upgradeGrid.Add(card);
            }
        }

        private void Purchase(string upgradeId)
        {
            UpgradePurchaseResult result = runtime.PurchaseUpgrade(upgradeId);

            if (result.Succeeded)
            {
                UpgradeDefinition definition = runtime.GetUpgradeDefinition(upgradeId);
                feedback.text = $"{definition.DisplayName} reached level {result.NewLevel}.";
                feedback.EnableInClassList("feedback--success", true);
                feedback.EnableInClassList("feedback--error", false);
                return;
            }

            feedback.text = result.Failure == UpgradePurchaseFailure.InsufficientCash
                ? "You need more fictional credits for that upgrade."
                : "That upgrade cannot be purchased right now.";
            feedback.EnableInClassList("feedback--success", false);
            feedback.EnableInClassList("feedback--error", true);
        }

        private static string AccentClass(int index) => $"upgrade-card--accent-{index % 5}";

        private static string IconFor(StockMarket.Content.Definitions.UpgradeEffectType effectType)
        {
            switch (effectType)
            {
                case StockMarket.Content.Definitions.UpgradeEffectType.DividendYieldBonus:
                    return "D";
                case StockMarket.Content.Definitions.UpgradeEffectType.AutomationCapacity:
                    return "A";
                default:
                    return "R";
            }
        }

        private static string EffectLabel(UpgradeDefinition definition)
        {
            switch (definition.EffectType)
            {
                case StockMarket.Content.Definitions.UpgradeEffectType.DividendYieldBonus:
                    return $"+{definition.EffectAmountPerLevel * 100f:0.#}% dividend potential per level";
                case StockMarket.Content.Definitions.UpgradeEffectType.AutomationCapacity:
                    return $"+{definition.EffectAmountPerLevel:0.#} automation capacity per level";
                default:
                    return $"+{definition.EffectAmountPerLevel * 100f:0.#}% market insight per level";
            }
        }
    }
}
