# Decisions

Newest first. Format: date, decision, reason.

## 2026-11-06
- **Light Devourer art merged** into `develop` (`e48d21e`) after the last check (the Corruptor) passed.
- **The class rename goes all the way** (owner): Warrior becomes Crusader and Archer becomes Sentinel in code, assets, prefabs and the sheet's keys, and the internal Mage becomes Augur. Old meta saves load through aliases. Done once, before the Steam page shows class names.
- **Class select tabs:** the implementation session drafts the Lore (from `Lore.md` §6, no spoilers past an arc's start) and Playstyle texts; the owner reviews them.
- **The pilgrim on the map walks, then waits** (owner): picking the next node walks the class's pilgrim along the path; entering the level stays a second click or A.
- **Cutscene scope cut to 8 scenes** (owner): one at the start and one at the end of each act, for the party as a whole; the opening counts as Act I's start. Replaces the 26 scenes of 2026-10-29 (one per pilgrim before and after each act). Each pilgrim's arc stays in the events. A future party (a DLC) would get its own act-start and act-end scenes.
- **Items 38 to 42 are demo musts** (owner): the Dreko Eye outline, the class select gamepad step, the class rename, the class select tabs and the pilgrim on the map. "We have time."
- **Class names: Warrior becomes Crusader, Archer becomes Sentinel** (owner). Augur stays. BACKLOG 40; it needs a spec, because names are saved and are balance sheet keys.
- **Light Devourer art, from the owner's play-tests:** the hit lands on frame 9 of 11; ranges and areas are measured on the ground from the feet; every enemy body sorts on order 10 with the player and trees; the player's knockback is a steady push (force x time), not an added impulse. The owner's tuning: Orbit Radius 1.3, Max Range 1.8, Attack Cooldown 1, LDChase Min Range 1. Published as sheet version 15.
- **Push All to Sheet** waits up to 180 s per batch and retries a timed-out batch twice (it stopped partway on large pushes).
- **Specs whose last checks wait on clips are archived:** game feel's three sound checks can't run until those sounds have clips, so they moved to `todo.md` "Sounds waiting for clips" and the spec is in the archive.

## 2026-11-05
- **Light Devourer attack area** (owner and art team): the circle stays, no cone, but it's drawn and hit as a circle on the isometric ground (an ellipse, half as tall as wide). The windup is now in the animation, so the code stops slowing it.
- **v0.5.3 released from `develop`** (owner): tagged on `develop`. `bundleVersion` was then set to 0.5.3 and `main` fast-forwarded to `develop` (owner's OK, 2026-11-05), as v0.5.2 was. The tag stays where it is.
- **Boss balance** (owner, from tester feedback): Taio'Sandir's health 2000 → 8000; contact damage tier 1 → 2, projectile tier 0 → 2.

## 2026-11-04
- **Unused sound clips are kept for now** (owner): the 711 clips the fixes session found unused (118 MB, listed in `docs/archive/features/tester-fixes-v052-unused-clips.md`) stay in `Assets/Sounds/SFX/Packs/`. Out of `Resources/`, they no longer ship in the build, and some may fill the sounds that are still silent (no male voice is wired yet).
- **Game feel merged into `develop`** (owner, fast-forward). Decided along the way:
  - **Clips come from sound packs** the owner added (a UI pack and two voice packs), so the sound list's brief needs no other source for now.
  - **Player sounds get per-class voices** (owner, during review): a sound can have clips per class, with the shared clips as the fallback.
  - **The heartbeat starts at intensity 0.8** (owner, play-testing), so it marks only the lowest health, not every moment the vignette shows.
- **Tester feedback triage:** boss health goes into the balance pass (BACKLOG 5); knockback during melee swings and Ayteel's Shrine option are new items (35, 36).

## 2026-11-01
- **Run structure (GDD Q14), owner:** one Pilgrimage through all four acts in a row; death starts over from Act I. Why: builds keep growing, the boss reward has a use, and it needs no act select or starting packages. The demo's Act I ending stays as it is. Cost: about 1.5 hours for a full clear (estimated) and one difficulty curve to about 32 levels beaten. Full comparison: `docs/features/act-ii-design.md` section 1.
- **Act II design, owner** (same spec):
  - **Frozen is in:** the third Frost stack freezes the player, and Dash with a charge ready breaks the ice.
  - **Frozen Enemies:** optional, with a guaranteed item pickup.
  - **Act II only:** Frozen and Frozen Enemies are not rules of Acts III–IV.
  - **Roster:** all six enemies plus the Echo stay; the Ice Sprite was recommended for a cut and kept.
  - **The Ice Bull:** the player fills the flame by baiting his charge into pillars, Frozen Axes are phase 1's pressure and Deep Freeze is phase 2's. He is one of the first Plerians the Drekos corrupted, saved and buried by Malizante.
- **Act II branches (Orchestrator):** chunk 1 (the curve) on `develop`, everything else on one branch, `feature/act-ii`, one session at a time. Why: `DataHygieneTests` stops the content landing piece by piece, and chunks 2 and 9 share the boss flow. Act II isn't needed for the demo, so it's built after it unless time is free.
- **GDD corrected:** the map's "you can never go back" was wrong. `MapTravelRules` allows walking back along found edges, and the side paths that end in a shop or a treasure depend on it.

## 2026-10-31
- **Opening cutscene, owner:** only on the first New Game, after Start Run, right before the map. The replay menu is the **Pilgrim's Chronicle**, on the main menu only. Gabien is named once, as people see him. The art team delivers all 7 pages before the demo, with no fallback.

## 2026-10-30
- **Attack Speed final numbers:** scale 0.02 on every weapon, soft cap 40, hard cap 80, rate 0.5 (the owner's, after play-testing; the spec's proposal felt far too strong). At most 2.2× speed.
- **Release v0.5.2** (owner): not 0.6.0, because most of the changes are refactors testers won't see; 0.6.0 is held for later. `bundleVersion` set to 0.5.2 (it said 1.0). While testers play, work on the opening cutscene's design and a small replay menu.
- **Lore compendium added to the backlog:** items, enemies and encounter events the player has interacted with. Placed in the demo update for now.
- **Attack Speed spec written** (`docs/archive/features/attack-speed-caps.md`). Primary attack animations scale with the cooldown; specials stay at speed 1, with a guard test that each special's cooldown stays longer than its action at the hard cap. Cap values are a starting proposal for the designer.
- **A finished spec stays in `docs/features/` until its manual tests pass**, then moves to `docs/archive/features/`.

## 2026-10-29
- **Demo scope trimmed** (owner, after the scope risk was flagged): the demo keeps the events rework, the Attack Speed fix, the opening cutscene and its design, the balance pass, the re-designed characters, the new scenarios **with the companions in non-combat levels** (the owner kept them), the boss shield and Steam prep. Skill-tree item unlocks, more items, the map redesign, the other cutscenes' design, voice research, the SQLite deletion and the one-page GDD move to a post-launch **demo update**. Reason: protect the December date; five of the musts depend on art.
- **High Attack Speed fix:** the attack animation speeds up with Attack Speed (the art team agreed), plus a soft cap and a hard cap on Attack Speed, like Shop Discount's. Replaces the two options left open on 2026-10-26.
- **Cutscene scope:** 26 scenes for 1.0 (an opening, an ending, and one per pilgrim before and after each act), static images and text. The Steam demo gets the opening one. Voice narration to be researched. Replaces the 8 scenes of 2026-10-23.
- **Demo scope grows:** skill-tree item unlocks, more and higher-quality items, the re-designed characters, the new scenarios with companions in non-combat levels, the map redesign, the opening cutscene, the boss's invulnerability shield (art). See BACKLOG "Before the Steam demo".
- **For 1.0:** a pick-1-of-3 boss reward of boss-only items, and a boss architecture refactor before the second boss.
- **Lore canon:** `game-design-docs/Lore.md` is the story bible and its glossary sets every name. The owner's novels and notes in `lore-docs/` (gitignored, Spanish) are reference only; where they differ, `Lore.md` wins. The lore decisions themselves are recorded in `docs/archive/features/run-encounter-events.md`, "Lore decisions".
- **Lore choices from the narrative session:**
  - The game is set about **60 years after Gabien** (`lore-docs/Timeline.docx` said 150).
  - The English for the Amanecidos is **"Awoken"** (not "Dawner").
  - Renames: Shop → **Plerian Shop**, Holy Altar → **Wayside Shrine of Malien**, Centinel → **Sentinel**.
  - Act III boss: **Forsaken Daviro the Unholy**. Corruptors: Dreko Spawn (Act I, working name), Echo of Malizante, Echo of Pal'bor, Echo of Gabien.
- **Event design rule: pure gains are earned.** A first-step option always risks a cost; a pure gain only appears in a step reached by a lore-correct answer. The Wayside Shrine's "Kneel and pray" is the one exception.
- **Event scope for the demo:** the 5 events and the Shrine reworked, plus 4 new ones (Liah, the Aylinean, the Star Reader, the Last Torch).
- **Docs cleanup:** closed specs moved to `docs/archive/features/`; the old handoffs, the corruption rework spec, the data-driven analysis and the 2026-10-09 tester feedback moved from `Assets/Docs/` to `docs/archive/` (out of the Unity project). The `.docx` GDDs were deleted (the Markdown versions replace them; git history keeps them). Applied handoffs are deleted once their content is in the spec and `docs/pm/`.

## 2026-10-26
- **Narrative before systems for the events rework.** A design session writes the lore bible (`game-design-docs/Lore.md`) and the event content into the Narrative section of `docs/archive/features/run-encounter-events.md`; the implementation session builds the systems from it afterwards. Reason: the rework's goal is lore delivery, and the lore has contradictions (listed in the spec) that would leak into event text. A design session may edit `Lore.md`, the GDD's story sections and that spec's Narrative section, on `develop`.
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
- 2026-10: Balance data goes through Google Sheets + CSV importer, with a test-build override. Why: designer works without Unity. Details: `docs/archive/design/data-driven-analysis.md`.
- 2026-10: Corruption and Grace reworked (Grace changes at wave end, Corruption per wave). Spec: `docs/archive/design/corruption-rework.md`.
- 2026-10: Enemies use local context steering, not a NavMesh.
- 2026-10: Cleave stat retired. The `StatType` index stays because stats are stored by index.
