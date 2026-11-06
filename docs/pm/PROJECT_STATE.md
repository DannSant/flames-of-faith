# Project state

_Last updated: 2026-11-06 by the Orchestrator._

## Milestone
- **Current:** tester round on v0.5.3, then the **Steam demo (full Act I) in December 2026**. 1.0 (Acts I–IV) Fall/Winter 2027 (`TenPageGDD.md` §1–2).
- **Last tagged release:** `v0.5.3` (2026-11-04, tag pushed, on `50631ef`). `main` was fast-forwarded to `develop` on 2026-11-05, past the tag, so it also carries the patch notes and the `bundleVersion` 0.5.3 fix (`58bb55d`). Patch notes: `PatchNotes/notas-desde-v0.5.2.md`.
- **In v0.5.3:** the opening cutscene and Pilgrim's Chronicle; game feel (low-health warning, sounds, shake, hit stop, rumble, Settings sliders); the three tester fixes; the boss at 4× health (2000 → 8000) with contact and projectile damage tier 2; Bolt of Darkness fragments now deal half the bolt's damage; Dreko Eye hit and death sounds.
- **In v0.5.2:** Unity 6.3 upgrade; data-driven steps 1–5 (everything balance-related is in the Google Sheet); English Ten Page GDD; tester-feedback pass; the lore bible and the events narrative; item and level texts made lore-accurate; the run encounter events rework; the Attack Speed fix.

## Git reality (checked 2026-11-06)
- `main` = `origin/main` = `a9a381e` (v0.5.3 plus the version fix; the tag `v0.5.3` is on `50631ef`).
- `develop` has `feature/light-devourer-art` merged (`e48d21e`, 2026-11-06, owner's OK) and the spec for items 38 to 42 on top; check `git status` for unpushed commits.
- `feature/demo-musts` (items 38 to 42) is created by its implementation session, off `develop`.
- `feature/opening-cutscene`, `feature/game-feel`, `fix/tester-feedback-v052` and `feature/light-devourer-art` are merged and can be deleted, locally and on `origin`, with the owner's OK.

## Resume here (Orchestrator, 2026-11-06)
- **Items 38 to 42** (demo musts) run in one implementation session on `feature/demo-musts`, spec `docs/features/demo-musts.md`, in the order 38, 40, 41, 39, 42. The class rename stops for the owner's OK before Push All to Sheet; after it, the owner publishes a new sheet version, or test builds' overrides fail on the old keys.
- **When it finishes:** read its Handoff, update these files, rebuild the tester guide (`.docx` / `.pdf`) with the new class names, and get the owner's review of the drafted Lore and Playstyle texts.
- **Next code after that:** CI/CD (BACKLOG 18, needs a spec).
- **Waiting on the owner:** class options for Faelin and Werlem at the Shrine; the 7 page briefs to the art team and their size; the five lore questions in `docs/features/act-ii-design.md`.
- **Tester feedback on v0.5.3:** triage each batch into the backlog; the balance pass (5) and the SQLite deletion (16) follow this round.
- **Sound checks** wait for clips (`todo.md` "Sounds waiting for clips"); no spec is open for them.

## In progress
| Feature | Branch | Spec | Status |
|---|---|---|---|
| Tester build v0.5.3 | `main` | BACKLOG item 1 | **With testers** |
| Demo musts (BACKLOG 38 to 42): Dreko Eye outline, class rename, class select tabs, gamepad to Start Run, the pilgrim on the map | `feature/demo-musts` | `docs/features/demo-musts.md` | **Ready**, implementation session starting |
| Act II build | `feature/act-ii` (chunk 1 on `develop`) | `docs/features/act-ii-design.md` | **Designed, not started.** Waits for the demo |

## Done recently
- **Tester fixes from v0.5.2** (merged 2026-11-04, `30acb5e`; manual tests passed 2026-11-06): sound packs out of `Resources/`, no knockback mid-swing, Ayteel's Shrine option. Spec archived: `docs/archive/features/tester-fixes-v052.md`.
- **Game feel** (merged 2026-11-04; manual tests passed 2026-11-06, except three sound checks that wait for clips, kept in `todo.md`): spec archived, `docs/archive/features/game-feel.md`.
- **Light Devourer's new art** (merged 2026-11-06, `e48d21e`; manual tests passed; spec archived, `docs/archive/features/light-devourer-art.md`): new sheets (attack 11 frames, idle 16) through a shared enemy sheet builder that the Corruptor uses too; no slowed windup (the hit on frame 9 of 11); the attack area is a circle on the isometric ground for the warning and the hitbox; ranges measured on the ground from the feet; enemies sort with the player and trees (order 10); the player's knockback is a steady push. The reusable pieces are listed in the spec's Handoff.
- **Tester guide** (2026-11-04, Spanish): `PatchNotes/guia-del-tester.md`, for the tester joining now: the game, every system, controls, Act I, what we want feedback on and how to report. Update it when a build adds features (the 🆕 marks are what came after v0.5.2).
- **Game feel** (merged 2026-11-04, fast-forward to `12318a9`): a low-health vignette with a heartbeat; a sound table with hooks everywhere the spec listed, one voice per sound, per-class player voices and clips from the owner's packs; shake, hit stop, slow motion and rumble on five moments, with Settings sliders. `PauseManager` is now the only writer of `Time.timeScale`.
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
- **Balance sheet:** published version 15 (2026-11-06, the Light Devourer's tuning) matches the project's CSVs; `snapshot.json` records it.
- **Designer:** Min Levels Beaten per item (all 0), Luck Scale 2 looks steep in `Report_LootOdds`, Move Speed and Frost per-point values look too small.

## Open manual tests
- **Sounds waiting for clips:** the heartbeat, and the once-per-event sounds (dodge, shield, dash, wave start and end, Corruptor, boss); one optional check (enemy attack and boss cast sounds).

Finished tests are in `docs/archive/todo-2026-11-06.md`.

## Known open issues
- **Light Devourer pivot vs body collider:** the figure stands about 1.3 below its transform while its body collider (its ground point) is at -0.5. Fine in play; a pivot at the feet would make sorting and ground points exact, but that's a larger prefab change.
- **DarkDrone** (the boss's summon) is still on sorting order 0, outside the characters' order 10. Left alone on purpose.
- **The v0.5.3 tag predates the `bundleVersion` fix:** a build made from the tag itself reports 0.5.2. Build from `main` (or `58bb55d` or later) instead.
- **12 game sounds have no clip yet** (checked 2026-11-05): Dash, LevelUp, WaveStart, WaveEnd, LowHealthHeartbeat, CorruptorAppears / Killed / Escaped, BossPhaseChange, BossDeath, PlayerDodge, ShieldAbsorb. No class has its own voice (every `classClips` is empty), so all three pilgrims share the hurt and death voices. The v0.5.3 notes and the tester guide say so.
- Unreproduced: "level-up picks don't raise stats".
- `MapUtils.cs` (legacy Map) may be unused.
- `CharacterClassData.animatorController` is effectively unused (the prefab's controller plays), so a new character must set both; a test keeps them the same.
- Events: edge texts the guard tests can't see: "+0 health" when healed at full health (Liah's fish), and "weighs your 0 Golden Chronos" with an empty purse. The check tunables and amount sizes aren't a balance table yet (agreed follow-up).
- Obsolete SQLite item pipeline still in the repo; deletion deferred by the owner until after a play-test round.
- Open lore (not needed for the demo): how Forsaken Daviro the Unholy has lived sixty years past Gabien's time; five Act II questions (spec, "Lore questions for the owner").
- Act II flavor lines (Winter Shamans raised the Ice Golems, sprites as snow spirits, Death Witches as former scouts) are in the spec only, not in `Lore.md`, until the owner keeps them.
- `event-test` is a node id on `Act1MapB`. It works, but the name is left over from testing. Node ids are saved, so renaming it would invalidate saved runs on that map.
- Fewest fights to reach the boss vary by map (3 / 7 / 6); see BACKLOG 5.
