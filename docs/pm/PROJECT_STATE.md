# Project state

_Last updated: 2026-10-30 by the Orchestrator._

## Milestone
- **Current:** next tester build, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.1` (on `main`). Everything since is on `develop`.
- **Since v0.5.1 (on `develop`):** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate; the run encounter events rework; the Attack Speed fix.

## Git reality (checked 2026-10-30)
- `develop` includes the events rework (`ac9a22a`), the Attack Speed fix (`789d4bc`) and the event art (`5b4938a`), all on origin. Docs commits after them may not be pushed; check `git status`.
- `main` = `ff779dc` = `origin/main`, 88+ commits behind `develop`. No release merge yet: the Attack Speed fix is on `develop` only.
- **Stale branch:** local `feature/attack-speed-caps` is fully merged (it was never on origin). Delete on your OK.
- Uncommitted, the owner's: `LiahEncounter.asset` and `LastTorchEncounter.asset` (their art), plus the `todo.md` cleanup and `docs/archive/todo-2026-10-30.md`.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Next tester build | `develop` → `main` | BACKLOG item 1 | **Next.** Waiting on the owner's go |

## Done recently
- **High Attack Speed fix** (merged 2026-10-30, `789d4bc`; manual tests passed): the attack animation speeds up with the cooldown, soft cap 40, hard cap 80, scale 0.02, at most 2.2× speed.
- **Run encounter events rework** (merged 2026-10-30, `ac9a22a`; manual tests passed): 10 events with hints, narrated results, stat checks, steps up to 3 deep and class content; no repeats in a Pilgrimage, saved with the run. Quiver of the Sentinel renamed.
- **Event art:** every event has art except The Star Reader.
- **Narrative design** (2026-10-26 to 10-29): `game-design-docs/Lore.md` (story bible), the events' Narrative section, GDD §3 and §13 aligned. The owner then updated item and level texts to match (`9f82994`: Sacred Writings typo fixed, level names, Plerian Shop, Wayside Shrine node).
- Closed specs are in `docs/archive/features/`.

## Waiting on others / decisions
- **Demo scope trimmed (2026-10-29)** to 8 musts (BACKLOG "Before the Steam demo"); the rest is in "Demo update". Four musts wait on art: the re-designed characters, the scenarios with companion idle animations, the boss shield, and the opening cutscene's art. **Ask the art team for delivery dates**; they set the demo's critical path.
- **Run structure across acts** (GDD open question 14). Blocks Act II planning, not the demo.
- **Art team:** re-designed characters, scenarios and companion idle animations, map decorations, the boss shield, cutscene art, The Star Reader's event art, the Plerian trader, the Favor icon, the 2 s Whirlwind animation, melee telegraph sprites.
- **Owner: confirm the sheet is published.** The working tabs have the final Attack Speed numbers (checked 2026-10-30), but `snapshot.json` records version 13, which the Attack Speed handoff says has the old ones. A Pull, or a test build with `balance-override.json`, uses the latest published version, so Publish first if version 13 is still the latest.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
None. All 52 passed (archived in `docs/archive/todo-2026-10-30.md`).

## Known open issues
- Unreproduced: "level-up picks don't raise stats".
- `MapUtils.cs` (legacy Map) may be unused.
- `CharacterClassData.animatorController` is effectively unused (the prefab's controller plays), so a new character must set both; a test keeps them the same.
- Events: edge texts the guard tests can't see: "+0 health" when healed at full health (Liah's fish), and "weighs your 0 Golden Chronos" with an empty purse. The check tunables and amount sizes aren't a balance table yet (agreed follow-up).
- Obsolete SQLite item pipeline still in the repo; deletion deferred by the owner until after a play-test round.
- Open lore (not needed for the demo): how Forsaken Daviro the Unholy has lived sixty years past Gabien's time.
