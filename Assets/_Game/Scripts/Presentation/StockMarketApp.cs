using System;
using StockMarket.Content.Definitions;
using StockMarket.Presentation.Runtime;
using StockMarket.Presentation.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace StockMarket.Presentation
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class StockMarketApp : MonoBehaviour
    {
        [SerializeField]
        private CompanyCatalog companyCatalog;

        [SerializeField]
        private UpgradeCatalog upgradeCatalog;

        [SerializeField, Min(0)]
        private long startingCashMinorUnits = 250_000;

        [SerializeField, Min(0.05f)]
        private float tickDurationSeconds = 1f;

        [SerializeField]
        private uint simulationSeed = 73421;

        private StockMarketRuntime runtime;
        private AppShellController shell;

        private void Awake()
        {
            if (companyCatalog == null || upgradeCatalog == null)
            {
                Debug.LogError("Stock Market UI requires company and upgrade catalog references.", this);
                enabled = false;
                return;
            }

            try
            {
                runtime = new StockMarketRuntime(
                    companyCatalog,
                    upgradeCatalog,
                    startingCashMinorUnits,
                    tickDurationSeconds,
                    simulationSeed);
                shell = new AppShellController(GetComponent<UIDocument>(), runtime);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }

        private void Update()
        {
            runtime?.Advance(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            shell?.Dispose();
            runtime?.Dispose();
        }
    }
}
