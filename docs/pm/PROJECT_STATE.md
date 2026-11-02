# Project state

_Last updated: 2026-11-01 by the Orchestrator._

## Milestone
- **Current:** tester round on v0.5.2, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.2` (on `main`, 2026-10-30).
- **In v0.5.2:** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate; the run encounter events rework; the Attack Speed fix.

## Git reality (checked 2026-11-01)
- `main` = `origin/main` = `ef237a6`, tagged `v0.5.2` (pushed 2026-10-30).
- `develop` is ahead of `main` by docs commits only (cutscene and Act II specs, GDD, lore, these state updates). Check `git status` for unpushed ones.
- No feature branches open.

## Resume here (Orchestrator, 2026-11-01)
- **Waiting on the owner:** the Windows build of v0.5.2 and sending it to testers; starting the implementation session for `feature/opening-cutscene` (kickoff prompt given 2026-11-01); sending the 7 page briefs to the art team and agreeing the size; the five lore questions in `docs/features/act-ii-design.md`.
- **When the cutscene session ends:** read its Handoff, update these files, and move the spec to the archive once its manual tests pass.
- **When tester feedback arrives:** triage it into the backlog; the balance pass and the SQLite deletion follow this round.
- **If code runs out before art arrives:** CI/CD (BACKLOG 18) comes first, because it has a December date. Act II's build waits for the demo, except chunk 1.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Tester build v0.5.2 | `main` | BACKLOG item 1 | **Released.** Next: the owner makes the Windows build and sends it with the patch notes |
| Opening cutscene and Pilgrim's Chronicle | `feature/opening-cutscene` | `docs/features/opening-cutscene.md` | **Design done, ready for the implementation session.** Art briefs are in the spec |
| Act II build | `feature/act-ii` (chunk 1 on `develop`) | `docs/features/act-ii-design.md` | **Designed, not started.** Waits for the demo |

## Done recently
- **Opening cutscene design** (2026-11-01): 7 pages, a little over a minute, with an art brief per page. The replay menu is the Pilgrim's Chronicle.
- **Act II design and the run structure** (2026-11-01): one Pilgrimage through all four acts; Frozen, Frozen Enemies, six enemies, the Echo of Malizante, the Ice Bull, an art brief and a 12-chunk build order. GDD and `Lore.md` updated.
- **High Attack Speed fix** (merged 2026-10-30, `789d4bc`; manual tests passed): the attack animation speeds up with the cooldown, soft cap 40, hard cap 80, scale 0.02, at most 2.2× speed.
- **Run encounter events rework** (merged 2026-10-30, `ac9a22a`; manual tests passed): 10 events with hints, narrated results, stat checks, steps up to 3 deep and class content; no repeats in a Pilgrimage, saved with the run. Quiver of the Sentinel renamed.
- **Event art:** every event has art except The Star Reader.
- **Narrative design** (2026-10-26 to 10-29): `game-design-docs/Lore.md` (story bible), the events' Narrative section, GDD §3 and §13 aligned. The owner then updated item and level texts to match (`9f82994`: Sacred Writings typo fixed, level names, Plerian Shop, Wayside Shrine node).
- Closed specs are in `docs/archive/features/`.

## Waiting on others / decisions
- **Demo scope trimmed (2026-10-29)** to 8 musts (BACKLOG "Before the Steam demo"); the rest is in "Demo update". Four musts wait on art: the re-designed characters, the scenarios with companion idle animations, the boss shield, and the opening cutscene's art. **Ask the art team for delivery dates**; they set the demo's critical path.
- **Art team:** the opening cutscene's 7 pages (promised before the demo; size still to agree), re-designed characters, scenarios and companion idle animations, map decorations, the boss shield, cutscene art, The Star Reader's event art, the Plerian trader, the Favor icon, the 2 s Whirlwind animation, melee telegraph sprites.
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
- Open lore (not needed for the demo): how Forsaken Daviro the Unholy has lived sixty years past Gabien's time; five Act II questions (spec, "Lore questions for the owner").
- Act II flavor lines (Winter Shamans raised the Ice Golems, sprites as snow spirits, Death Witches as former scouts) are in the spec only, not in `Lore.md`, until the owner keeps them.
- `event-test` is a node id on `Act1MapB`. It works, but the name is left over from testing. Node ids are saved, so renaming it would invalidate saved runs on that map.
- Fewest fights to reach the boss vary by map (3 / 7 / 6); see BACKLOG 5.
