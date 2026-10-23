# Decisions

Newest first. Format: date, decision, reason.

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
