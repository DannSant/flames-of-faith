# Project state

_Last updated: 2026-11-04 by the Orchestrator._

## Milestone
- **Current:** tester round on v0.5.2, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.2` (on `main`, 2026-10-30).
- **In v0.5.2:** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate; the run encounter events rework; the Attack Speed fix.

## Git reality (checked 2026-11-04)
- `main` = `origin/main` = `ef237a6`, tagged `v0.5.2` (pushed 2026-10-30).
- `develop` carries the opening cutscene (merge `d5d6b1e`), game feel (fast-forward to `12318a9`, test fix `aa70ba5`) and the v0.5.2 tester fixes (merge `30acb5e`) on top of the docs commits. Check `git status` for unpushed commits.
- `feature/opening-cutscene`, `feature/game-feel` and `fix/tester-feedback-v052` are merged and can be deleted, locally and on `origin`, with the owner's OK.
- `origin/HEAD` was a corrupted local ref (empty bytes), which made git print "ignoring broken ref"; reset to `origin/main` on 2026-11-04.

## Resume here (Orchestrator, 2026-11-04)
- **Waiting on the owner:** class options for Faelin and Werlem at the Shrine; sending the 7 page briefs to the art team and agreeing the size; the two open sound checks in `todo.md`; the five lore questions in `docs/features/act-ii-design.md`.
- **Next code:** CI/CD (BACKLOG 18, needs a spec).
- **Manual checks to run on `develop`:** the tester fixes (merged before their checks, on the owner's OK) and the two game feel sound checks. When each section passes, move its spec to the archive. The VFX pass (34) waits for the new art. Act II's build waits for the demo, except chunk 1.
- **Tester feedback on v0.5.2** has started arriving (top of `todo.md`). Triage each batch into the backlog; the balance pass (5) and the SQLite deletion (16) follow this round.
- **When the two sound checks pass:** move `docs/features/game-feel.md` to the archive.
- **EditMode suite:** run by the owner on 2026-11-04 after the game feel merge; two failures (float tolerance in the sound restart interval and the heavy hit rule) fixed in `aa70ba5`.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Tester build v0.5.2 | `main` | BACKLOG item 1 | **With testers.** First feedback triaged 2026-11-04 (BACKLOG 5, 35, 36) |
| Tester fixes from v0.5.2 | `develop` (merged) | `docs/features/tester-fixes-v052.md` | **Merged 2026-11-04** (`30acb5e`): sound packs out of `Resources/`, knockback ignored mid-swing, Ayteel's Shrine option (texts approved by the owner); EditMode 985/985. Manual checks in `todo.md` not run yet; the spec moves to the archive after them |
| Game feel | `develop` (merged) | `docs/features/game-feel.md` | **Merged 2026-11-04.** Two sound checks left in `todo.md`; spec moves to the archive after them |
| Act II build | `feature/act-ii` (chunk 1 on `develop`) | `docs/features/act-ii-design.md` | **Designed, not started.** Waits for the demo |

## Done recently
- **Tester guide** (2026-11-04, Spanish): `PatchNotes/guia-del-tester.md`, for the tester joining now: the game, every system, controls, Act I, what we want feedback on and how to report. Update it when a build adds features (the 🆕 marks are what came after v0.5.2).
- **Game feel** (merged 2026-11-04, fast-forward to `12318a9`; manual tests passed except two sound checks): a low-health vignette with a heartbeat; a sound table with hooks everywhere the spec listed, one voice per sound, per-class player voices and clips from the owner's packs; shake, hit stop, slow motion and rumble on five moments, with Settings sliders. `PauseManager` is now the only writer of `Time.timeScale`.
- **Opening cutscene and Pilgrim's Chronicle** (merged 2026-11-02, `d5d6b1e`; manual tests passed): `CutscenePlayer.Play(id, onEnded)`, played on the first New Game only, on placeholder art. Spec and art briefs: `docs/archive/features/opening-cutscene.md`. Between-act scenes for 1.0 must play before the scene unload (its Handoff explains why).
- **Opening cutscene design** (2026-11-01): 7 pages, a little over a minute, with an art brief per page. The replay menu is the Pilgrim's Chronicle.
- **Act II design and the run structure** (2026-11-01): one Pilgrimage through all four acts; Frozen, Frozen Enemies, six enemies, the Echo of Malizante, the Ice Bull, an art brief and a 12-chunk build order. GDD and `Lore.md` updated.
- **High Attack Speed fix** (merged 2026-10-30, `789d4bc`; manual tests passed): the attack animation speeds up with the cooldown, soft cap 40, hard cap 80, scale 0.02, at most 2.2× speed.
- **Run encounter events rework** (merged 2026-10-30, `ac9a22a`; manual tests passed): 10 events with hints, narrated results, stat checks, steps up to 3 deep and class content; no repeats in a Pilgrimage, saved with the run. Quiver of the Sentinel renamed.
- **Event art:** every event has art except The Star Reader.
- **Narrative design** (2026-10-26 to 10-29): `game-design-docs/Lore.md` (story bible), the events' Narrative section, GDD §3 and §13 aligned. The owner then updated item and level texts to match (`9f82994`: Sacred Writings typo fixed, level names, Plerian Shop, Wayside Shrine node).
- Closed specs are in `docs/archive/features/`.

## Waiting on others / decisions
- **Demo scope trimmed (2026-10-29)** to 8 musts (BACKLOG "Before the Steam demo"); the rest is in "Demo update". Four musts wait on art: the re-designed characters, the scenarios with companion idle animations, the boss shield, and the opening cutscene's art. **Ask the art team for delivery dates**; they set the demo's critical path.
- **Art team:** the opening cutscene's 7 pages (promised before the demo; size still to agree), re-designed characters, scenarios and companion idle animations, map decorations, the boss shield, cutscene art, The Star Reader's event art, the Plerian trader, the Favor icon, the 2 s Whirlwind animation, melee telegraph sprites, a blood-like low-health vignette (the current one is a placeholder).
- **Owner: confirm the sheet is published.** The working tabs have the final Attack Speed numbers (checked 2026-10-30), but `snapshot.json` records version 13, which the Attack Speed handoff says has the old ones. A Pull, or a test build with `balance-override.json`, uses the latest published version, so Publish first if version 13 is still the latest.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
Game feel, chunk 2: the heartbeat, and the once-per-event sounds (dodge, shield, dash, pickups, wave start and end, Corruptor, boss); one optional check (enemy attack and boss cast sounds). Everything else in `todo.md` is ticked.

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
