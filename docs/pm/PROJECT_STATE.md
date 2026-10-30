# Project state

_Last updated: 2026-10-30 by the Orchestrator._

## Milestone
- **Current:** next tester build, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.1` (on `main`). Everything since is on `develop`.
- **Since v0.5.1 (on `develop`):** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate; the run encounter events rework.

## Git reality (checked 2026-10-30)
- `develop` = `origin/develop` (pushed 2026-10-30), including the events rework merge (`ac9a22a`).
- `main` = `ff779dc` = `origin/main`, well behind `develop`. No release merge yet.
- No feature branches open. `feature/run-encounter-events` was deleted 2026-10-30 (fully merged).
- Uncommitted: `todo.md` (the owner's file, holding the events' manual tests).

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| High Attack Speed fix | `feature/attack-speed-caps` (to create off `develop`) | `docs/features/attack-speed-caps.md` | **Ready for implementation.** Next: an implementation session |

## Done recently
- **Run encounter events rework** (merged 2026-10-30, `ac9a22a`; spec Handoff has the details): 10 events with hints, narrated results, stat checks, steps up to 3 deep and class content; no repeats in a Pilgrimage, saved with the run. Quiver of the Sentinel renamed. Manual tests pending.
- **Narrative design** (2026-10-26 to 10-29): `game-design-docs/Lore.md` (story bible), the events' Narrative section, GDD §3 and §13 aligned. The owner then updated item and level texts to match (`9f82994`: Sacred Writings typo fixed, level names, Plerian Shop, Wayside Shrine node).
- Closed specs are in `docs/archive/features/` (GDD update, data-driven step 3, item drops, items and abilities).

## Waiting on others / decisions
- **Demo scope trimmed (2026-10-29)** to 8 musts (BACKLOG "Before the Steam demo"); the rest is in "Demo update". Four musts wait on art: the re-designed characters, the scenarios with companion idle animations, the boss shield, and the opening cutscene's art. **Ask the art team for delivery dates**; they set the demo's critical path.
- **Run structure across acts** (GDD open question 14). Blocks Act II planning, not the demo.
- **Art team:** re-designed characters, scenarios and companion idle animations, map decorations, the boss shield, cutscene art, event art, the Plerian trader, the Favor icon, the 2 s Whirlwind animation, melee telegraph sprites.
- **Owner: Push All to Sheet** for the Quiver of the Sentinel rename, if not done yet: the sheet's Items / ItemStats / ItemDrops rows said "Centinel", and a Pull fails on a row naming a missing item.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
`todo.md`: the run encounter events rework (35), the melee attack warning (7) and the event art panel (1).

## Known open issues
- Unreproduced: "level-up picks don't raise stats".
- `MapUtils.cs` (legacy Map) may be unused.
- Events: edge texts the guard tests can't see: "+0 health" when healed at full health (Liah's fish), and "weighs your 0 Golden Chronos" with an empty purse. The check tunables and amount sizes aren't a balance table yet (agreed follow-up).
- Obsolete SQLite item pipeline still in the repo; deletion deferred by the owner until after a play-test round.
- Open lore (not needed for the demo): how Forsaken Daviro the Unholy has lived sixty years past Gabien's time.
