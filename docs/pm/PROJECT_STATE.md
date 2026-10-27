# Project state

_Last updated: 2026-10-26 by the Orchestrator._

## Milestone
- **Current:** next tester build, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.1` (on `main`). Everything since is on `develop`.
- **Since v0.5.1 (on `develop`):** Unity 6.3 upgrade; data-driven steps 1–5 (enemies, waves, XP, debuffs, Grace/Corruption, player stats, item drops, items, abilities and weapons all in the Google Sheet, round trip and test-build override verified 2026-10-25); English Ten Page GDD; tester-feedback pass (tooltips, pause rules, Shift view, Whirlwind rework, lock-on rework, Favors display, IMMUNE numbers, melee attack warning).

## Git reality (checked 2026-10-26)
- `develop` is 3 commits ahead of `origin/develop` (e0b4a72, cfd61f7, 36f6cff), not pushed.
- `main` = `ff779dc` = `origin/main`; 50 commits behind `develop`. No release merge yet.
- **Stale branches** (fully merged into `develop`, 0 commits ahead): `feature/data-driven-step3`, `feature/item-drops` (also on origin), `feature/items-abilities`. Delete on your OK.
- **Uncommitted, the owner's:** `Assets/Scenes/Act1/Level1_Iso.unity` (your edits) and `Assets/Prefabs/Enemies/EnemyLightDevourer.prefab` (a reserialize-only diff). Not committed by any session.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Run encounter events rework | `feature/run-encounter-events` (not created yet) | `docs/features/run-encounter-events.md` | Draft spec. **Next:** a narrative design session (lore bible + event content), then implementation |

## Done recently (specs closed)
- Ten Page GDD update (`docs/features/gdd-update.md`), data-driven step 3, item drops, items and abilities (their specs' Handoffs are the record).
- Tester-feedback pass, 2026-10-25/26 (archived in `docs/archive/todo-2026-10-26.md`).

## Waiting on others / decisions
- **High Attack Speed stops primary attacks from landing** (found 2026-10-26). Breaks above about 37 Attack Speed (Archer), 53 (Augur) and 67 (Warrior): the attack animation plays at speed 1 while the cooldown shrinks, so once the cooldown is shorter than the time to the shot or hit, the attack never lands. Two fixes, waiting on the team:
  - speed up the attack animation with Attack Speed (needs the animator's OK);
  - a per-weapon minimum cooldown (a design decision, plus a new Weapons column in the balance sheet).
- **Run structure across acts** (GDD open question 14): one run through all acts, or one act per run. Blocks Act II planning, not the demo.
- **Art team:** player re-design, level scenarios (may shrink levels), event art, the Favor icon, the boss's invulnerable look, the 2 s Whirlwind animation, melee telegraph sprites.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
`todo.md`: the melee attack warning (7) and the event art panel (1).

## Known open issues
- Unreproduced: "level-up picks don't raise stats".
- `BanditsEncounter.unity` and `CursedChestEncounter.unity` look unused (ask before deleting). `MapUtils.cs` (legacy Map) may be unused.
- Obsolete SQLite item pipeline still in the repo; deletion deferred by the owner (2026-10-25) until after a play-test round.
