# Project state

_Last updated: 2026-10-23 by the Orchestrator. Status: DRAFT, pending the kickoff interview._

## Milestone
- **Current:** playable demo for testers (Act I, three classes, run encounters, skill tree and Favors). Last tagged release: `v0.5.1`.
- **Since v0.5.1 (all on `main`, committed):** UI revamp finished, data-driven balance pilot built (CSV importer, Google Sheet sync, test-build override), Unity 6.3 LTS upgrade (`6000.3.13f1`), Corruptor art.
- **Target platform:** Steam first, Xbox possibly later. No CI/CD yet.

## Git reality (checked 2026-10-23)
- Branches: `develop` (integration, created 2026-10-23) and `main` (release). No feature branches are open. See DECISIONS 2026-10-23.
- `main` was `6c083b6` before the docs commit and matched `origin/main`. Nothing pushed yet this session.
- The 5 old merged branches were deleted (local and remote).

## In progress
| Feature | Branch | Purpose | Status |
|---|---|---|---|
| Data-driven balance pilot | `main` | Designer edits enemy/wave/XP/damage numbers in Google Sheets, publishes, Unity pulls | Code done, 699 EditMode tests passed. **Waiting on the owner** deploying `BalanceSync.gs` and running the step-5 checklist in `todo.md` |
| Unity 6.3 upgrade verification | `main` | Confirm nothing regressed after the editor upgrade | Upgrade committed. 7 manual checks open in `todo.md` |

## Blocked / waiting on others
- Sheet script deployment and designer sharing (owner action).
- Art team: player re-design sprites, new level scenarios (may shrink level sizes, which can affect gameplay).

## Next up
See `BACKLOG.md`. Proposed first picks: finish the open manual tests (they gate a new tester build), then the sheet round trip, then the balance pass on enemy and player damage.

## Known open issues
- Unreproduced: "level-up picks don't raise stats".
- Frost may deal double damage in `EnemyHealth.InflictExtraDamage`.
- Ley Charge chain radius and depth stuck on a runtime-added component.
- Warrior crowd balance without Cleave.
- **High Attack Speed stops primary attacks from landing** (found 2026-10-26, tester play, Archer at Attack Speed 50). The attack animation always plays at speed 1 while the cooldown shrinks (`base / (1 + Attack Speed × 0.075)`), and each attack trigger restarts the attack state. Once the cooldown is shorter than the time to the shot or hit, the shot or hit never happens: the Archer and Augur fire at 60% of a 0.67 s clip (`BowAttackAnimBehavior`, 0.4 s), and the Warrior's hit window opens at 0.25 s (`AttackStartAnimEvent`). Breaks above about **37** Attack Speed (Archer, base 1.5 s), **20** (Augur, base 1.0 s) and **40** (Warrior, base 1.0 s). Two fixes are on the table, waiting on the team:
  - speed up the attack animation with Attack Speed (`base cooldown / current cooldown`; needs the animator's OK);
  - a per-weapon minimum cooldown above the shot / hit point, with the Attack Speed scaling changed so the cap lands higher (needs a design decision; a new Weapons column, so the balance sheet pipeline too).
- `BanditsEncounter.unity` and `CursedChestEncounter.unity` look unused (ask before deleting). `MapUtils.cs` (legacy Map) may be unused.
