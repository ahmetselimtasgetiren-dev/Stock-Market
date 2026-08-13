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

`MarketSimulationService` is a pure deterministic domain service. For each requested tick it advances one bounded market-wide trend, combines global drift, per-company drift, per-company volatility, and seeded noise, then applies bounded fixed-point prices through `MarketState`. Simulation order follows the stable market-company order. The live clock, Unity lifecycle, sector effects, news effects, and offline aggregation remain separate responsibilities.
