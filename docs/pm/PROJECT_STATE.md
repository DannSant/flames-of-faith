# Project state

_Last updated: 2026-10-29 by the Orchestrator._

## Milestone
- **Current:** next tester build, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.1` (on `main`). Everything since is on `develop`.
- **Since v0.5.1 (on `develop`):** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate.

## Git reality (checked 2026-10-29)
- `develop` = `origin/develop` before this session's docs commit (`9f82994`, the owner's lore text edits, was pushed).
- `main` = `ff779dc` = `origin/main`, about 60 commits behind `develop`. No release merge yet.
- No feature branches open. Working tree clean apart from this session's docs changes.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Run encounter events rework | `feature/run-encounter-events` (to create off `develop`) | `docs/features/run-encounter-events.md` | **Ready for implementation.** Narrative done 2026-10-29 (10 events, authoring rules, Systems and Acceptance criteria written). Next: an implementation session |

## Done recently
- **Narrative design** (2026-10-26 to 10-29): `game-design-docs/Lore.md` (story bible), the events' Narrative section, GDD §3 and §13 aligned. The owner then updated item and level texts to match (`9f82994`: Sacred Writings typo fixed, level names, Plerian Shop, Wayside Shrine node).
- Closed specs are in `docs/archive/features/` (GDD update, data-driven step 3, item drops, items and abilities).

## Waiting on others / decisions
- **High Attack Speed stops primary attacks from landing** (found 2026-10-26). Breaks above about 37 Attack Speed (Archer), 53 (Augur) and 67 (Warrior). Two fixes, waiting on the team: speed up the attack animation with Attack Speed (animator's OK), or a per-weapon minimum cooldown (design decision plus a Weapons column in the sheet).
- **Run structure across acts** (GDD open question 14). Blocks Act II planning, not the demo.
- **Art team:** player re-design, level scenarios, event art (the events now have texts to illustrate), the Plerian trader, the Favor icon, the boss's invulnerable look, the 2 s Whirlwind animation, melee telegraph sprites.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
`todo.md`: the melee attack warning (7) and the event art panel (1).

## Known open issues
- Unreproduced: "level-up picks don't raise stats".
- `BanditsEncounter.unity` and `CursedChestEncounter.unity` look unused (ask before deleting). `MapUtils.cs` (legacy Map) may be unused.
- Obsolete SQLite item pipeline still in the repo; deletion deferred by the owner until after a play-test round.
- Open lore (not needed for the demo): how Forsaken Daviro the Unholy has lived sixty years past Gabien's time.
