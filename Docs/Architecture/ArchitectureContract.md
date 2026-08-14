# Stock Market Architecture Contract

Status: initial foundation, approved blueprint implementation

## Scope

This project is a single-player, offline-first 2D PC incremental game using only fictional companies and fictional currency. Real markets, banking, multiplayer, and online integrations are outside scope.

## Dependency direction

- Domain rules are plain C# and do not depend on scenes, UI, or MonoBehaviours.
- Unity-facing components may drive and present domain systems.
- Presentation reads domain state and submits explicit requests; it does not own financial truth.
- Infrastructure maps domain snapshots to persistence formats rather than making domain classes know about files.
- A future composition root may connect systems, but gameplay rules must not collect in a global `GameManager`.

## Initial decisions

- Market simulation advances on fixed, sequential ticks rather than rendered frames.
- The initial tick duration is intended to be one real-time second, supplied at composition time.
- Pausing discards real time received while paused. Offline progress will be handled separately from the live clock.
- Market state will save the current tick, fractional tick remainder, and deterministic random state.
- Company, sector, news, upgrade, tutorial, and achievement definitions will use stable string IDs.
- Player cash and accounting will eventually use a fixed-point currency representation.
- The first playable uses whole shares and weighted-average cost basis.
- ScriptableObjects define immutable authored content; versioned save data owns playthrough state.
- Direct requests perform actions; typed events announce completed state changes.

## Current implementation boundary

The persistence-neutral `MarketClock` owns only tick progress and pause state. It does not read Unity `Time`, perform offline progress, simulate prices, autosave, or update UI.

Company and sector definitions are immutable authored ScriptableObjects in a separate content assembly. Their catalogs provide stable-ID lookup and authoring validation. They do not own live prices, unlock state, holdings, or other playthrough data.

`MarketState` is the authoritative owner of live prices. It is pure C#, accepts initialization seeds rather than ScriptableObject references, and exposes company state read-only. Price updates must use a strictly increasing market tick and a positive fixed-point price. Each company owns a fixed-capacity chronological history buffer; the initial price is recorded at tick zero and the oldest samples are discarded when capacity is reached.

`MarketSimulationService` is a pure deterministic domain service. For each requested tick it advances one bounded market-wide trend, combines global drift, per-company drift, per-company volatility, optional active-news impact, and seeded noise, then applies bounded fixed-point prices through `MarketState`. Simulation order follows the stable market-company order. The live clock, Unity lifecycle, general sector trends, and offline aggregation remain separate responsibilities.

`PlayerFinancialState` is the authoritative owner of non-negative fixed-point cash and accumulated income totals, and its `PortfolioState` owns non-negative whole-share positions and weighted-average cost basis keyed by stable company ID. Mutation is restricted to domain services. Holdings value, net worth, and unrealized profit are derived on demand from current `MarketState` prices and are not stored as competing state.

`TradingService` is the sole public path for immediate whole-share buys and sells. It validates the company, optional company-access gate, positive quantity, current execution price, cash or share availability, and all arithmetic limits before mutation. Successful trades update cash and shares together and return immutable execution facts; expected player mistakes return explicit failure reasons without changing either state. Fees, pending orders, and fractional shares remain outside this trading boundary.

Positions now retain their remaining total cost basis, with average buy price derived using weighted-average accounting. Partial sales remove proportional basis rounded to the nearest minor unit; a final sale removes the exact remainder. Realized profit accumulates on `PlayerFinancialState`, while unrealized and total profit are derived from current prices. Every successful trade receives a sequential ID and is written to a fixed-capacity chronological `TransactionLedger`; failed trades are never recorded. Fees and tax-style accounting remain outside scope.

News definitions are authored immutable content with stable IDs, a company or sector target, a bounded per-tick price impact, and a duration. `NewsEventService` owns scheduled and active instances for one playthrough and advances on the same strictly increasing market ticks as price simulation. Matching impacts are additive and enter the existing price-change calculation before its safety clamp, so news cannot bypass configured market bounds. The first version activates news explicitly; random selection, cooldowns, localization, presentation, and save mapping remain later composition responsibilities.

Dividend policies are separate immutable authored definitions keyed to a fictional company. `DividendService` processes strictly sequential market ticks, reads current whole-share holdings only on scheduled payout ticks, credits cash and cumulative dividend income atomically, and records successful payments in a fixed-capacity chronological ledger. A due policy with no owned shares advances its schedule without creating a zero-value record. Yield scaling, reinvestment, offline aggregation, and upgrade modifiers remain later systems.

Upgrade definitions are immutable authored content with stable IDs, bounded levels, fixed-point base costs, deterministic basis-point cost growth, and one typed additive effect. `ProgressionState` owns purchased levels and cumulative fictional currency spent, while `UpgradeService` is the only purchase path and updates cash and progression atomically. Effect totals are derived queries for later systems; this initial boundary does not silently modify dividends, market data, automation, or unlock state.

Unlock offers are immutable authored definitions targeting one sector or company for a fixed fictional-currency cost. `MarketAccessState` owns the monotonic set of accessible sector and company IDs, seeded explicitly for a new or loaded playthrough. `UnlockService` purchases access atomically; company offers require their sector to be accessible first. Trading can receive this state as an optional gate, preserving isolated tests and compositions that intentionally omit progression. Sector access does not implicitly grant every company in that sector.

Automation rules are player-owned playthrough state rather than authored ScriptableObjects. `AutomationService` owns a bounded, ordered rule list and evaluates enabled buy-at-or-below and sell-at-or-above rules once per sequential market tick. Triggered rules submit ordinary requests to `TradingService`, so access, cash, holdings, accounting, and transaction validation remain authoritative there. Every attempt enters a bounded execution ledger and starts its cooldown even when the trade fails, preventing per-tick failure spam. Capacity can grow explicitly for later upgrade composition; scheduling, UI editing, and offline automation remain outside this first version.

Chart data is produced as immutable read-only series from bounded price and portfolio-value histories. Downsampling preserves chronological endpoints and never mutates authoritative history. Reports are derived snapshots over current financial state, market prices, and the bounded transaction ledger; report values are not separately persisted as competing truth.

Save files use a versioned JSON DTO owned by Infrastructure and contain no Unity object references. Writes use a temporary file and preserve the previous primary as a backup; loads validate the version and fall back to that backup when possible. `OfflineProgressService` converts wall-clock absence into capped whole market ticks and treats backward clock movement as zero progress. The future composition root remains responsible for mapping live domain services to DTO fields and for choosing which tick-driven systems participate offline.

`NavigationState` owns only the active top-level screen, overlay, and back history. It has no scene or visual dependencies. `TutorialService` activates ordered authored guidance steps only when their trigger and prerequisite are satisfied; completion and skip state are playthrough data. No finished UI documents or prefabs are part of these foundations.

Settings, notifications, feedback, audio requests, and achievements are separate domain services. Settings preserve normalized audio volumes, mute, reduced-motion, and notification choices. Feedback emits localization keys and optional audio cue IDs into bounded queues, without loading or playing clips itself. Achievements use stable authored IDs and explicit metric thresholds, earning each item at most once. Presentation and AudioSource/mixer adapters remain future Unity-facing composition work.

The first playable presentation uses a Unity UI Toolkit desktop shell with separate Market, Portfolio, and Upgrades controllers. A narrow `StockMarketApp` composition root advances the domain clock and connects existing market, trading, chart, portfolio, progression, and navigation services; it does not calculate prices, trades, profit, or upgrade costs. The UI reads authoritative domain state, submits explicit buy/sell, upgrade-purchase, and navigation requests, and refreshes on completed ticks or player actions. News, Reports, and later automation/settings screens remain visibly locked and are not yet composed.
