# Decisions

Newest first. Format: date, decision, reason.

## 2026-10-26
- **Narrative before systems for the events rework.** A design session writes the lore bible (`game-design-docs/Lore.md`) and the event content into the Narrative section of `docs/features/run-encounter-events.md`; the implementation session builds the systems from it afterwards. Reason: the rework's goal is lore delivery, and the lore has contradictions (listed in the spec) that would leak into event text. A design session may edit `Lore.md`, the GDD's story sections and that spec's Narrative section, on `develop`.
- **`OnePageGDD.md` marked outdated** rather than deleted; rewrite is in the backlog. The Ten Page GDD is the current design.
- **Closed specs:** data-driven step 3, item drops, items and abilities (merged into `develop` 2026-10-25), GDD update.
- **Recorded from the 2026-10-25/26 sessions:**
  - The tester-feedback pass was done directly on `develop`, one commit per item, by the owner's choice (no feature branches for small fixes).
  - High Attack Speed issue parked as a known open issue until the team picks a fix.
  - Obsolete SQLite pipeline: delete after a play-test round (owner, 2026-10-25).
  - Glossary: the player sees **Pilgrimage** (run) and **Golden Chronos** (coins); see `CLAUDE.md`.

## 2026-10-23 (GDD, from the gdd-update session)
- **Three separate pilgrims, one per class:** Ayteel Alamoh (Warrior), Faelin Vidal (Archer), Werlem Feth (Augur).
- **Scope:** Steam demo = full Act I, December 2026; 1.0 = Acts I–IV, Fall/Winter 2027. Premium, no microtransactions, no DLC planned. Teen / PEGI 16.
- **1.0 extras:** Chaos and Holy debuffs, Steam achievements, Spanish localization (Chinese and Japanese optional), cutscenes as 8 static scenes, Hard mode only if any.
- Open: run structure across acts (GDD Q14).

## 2026-10-23
- **Branch model: `develop` (integration) + `main` (release), feature branches off `develop`.** Created today from `main` at the docs commit. Reason: planned CI/CD builds and deploys on merge to `main`, so `main` must only move on purpose. `CLAUDE.md` updated (it used to say commit on `main`). The first docs commit went straight to `main` before the split.
- **Next milestone: the next tester build.** The backlog is ordered around it.
- **Deleted the 5 stale branches** (local and remote), all fully merged into `main` (0 commits ahead): `feature-overworldEditor`, `feature/corruption-rework`, `feature/corruption-ui`, `fix/tester-feedback`, `feature/manual-attack-auto-attack-toggle`.
- **`todo.md` reduced to pending manual tests only.** Backlog moved to `BACKLOG.md`. Done sections, checked items and the old Priority 1/2/3 notes archived verbatim in `docs/archive/todo-2026-10-23.md`. Reason: the file had grown to 640 lines and hid the 80 open tests. A Trello migration may replace it later.
- **Game design docs converted from `.docx` to Markdown** in `game-design-docs/` (`OnePageGDD.md`, `TenPageGDD.md`). The `.docx` files were kept. They are in Spanish, and parts predate the Corruption rework (e.g. Grace drains per wave, Corruption per level), so `CLAUDE.md` is the source of truth where they disagree.

## Earlier (from `CLAUDE.md` and handoffs, for reference)
- 2026-10: Balance data goes through Google Sheets + CSV importer, with a test-build override. Why: designer works without Unity. Details: `Assets/Docs/data-driven-analysis.md`.
- 2026-10: Corruption and Grace reworked (Grace changes at wave end, Corruption per wave). Spec: `Assets/Docs/corruption-rework.md`.
- 2026-10: Enemies use local context steering, not a NavMesh.
- 2026-10: Cleave stat retired. The `StatType` index stays because stats are stored by index.
