# Feature: Ten Page GDD update

- **Status:** done
- **Branch:** `develop` (docs only)
- **Owner session:** gdd-update
- **Related docs:** `CLAUDE.md` (source of truth for mechanics), `game-design-docs/TenPageGDD.md`, `game-design-docs/OnePageGDD.md`, `docs/archive/design/corruption-rework.md`

## Goal
Bring the Ten Page GDD up to date: English only, mechanics that match the game as built, and a complete document (all 4 acts, scope and goals) that new team members (designer, artists) can read instead of the code.

## Scope
- Translate the Spanish sections; merge the duplicated boss Phase 1/2 text.
- Restructure on the Ten Page GDD outline from Scott Rogers' *Level Up!*: title page, story and gameplay, game flow, character and controls, main gameplay concepts and platform features, game world, interface, mechanics and power-ups, enemies, cutscenes, bonus materials, monetization.
- Fix every mechanic against the code and assets (Grace/Corruption, classes and kits, stats, debuffs, encounters, map, XP, targeting, boss).
- Add a Scope and goals section (demo / Steam release vs. later).
- Act I from what exists; Acts II–IV marked TBD / proposed.
- An open-questions list for undecided design.

## Out of scope
- `OnePageGDD.md` and the `.docx` files.
- Any code, asset or balance change.
- `docs/pm/*` (Orchestrator-owned).

## Acceptance criteria
- [x] No Spanish text left (proper names excepted).
- [x] Every mechanic described exists in the code/assets, or is marked as planned/TBD.
- [x] Stat list matches `StatType` / `StatDatabase` (retired stats excluded).
- [x] All 4 acts covered; Act I detailed.
- [x] Scope and goals section and open-questions list present.

## Files likely touched
- `game-design-docs/TenPageGDD.md`
- `docs/features/gdd-update.md` (Handoff only at the end)

## Tests
- None (docs only).

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:** `TenPageGDD.md` rewritten in English on the *Level Up!* Ten Page outline (13 sections: title page, scope and goals, story, game flow, characters and controls, gameplay concepts, world, interface, mechanics, enemies, cutscenes, bonus materials, open questions). Mechanics checked against the code and assets: `StatType`/`StatDatabase`, the effects DB (kits, 43 items, qualities), `Resources/Debuffs` + `ElementalType`, `Resources/BossAbilities` + `Scripts/Boss`, `InputSystem_Actions`, `BalanceData/*.csv`, the Act I maps and levels. Answers from the user: three separate characters (names TBD), demo = Act I and full release = Acts I-IV, premium with no MTX, Teen / PEGI 16.
- **Round 2 (2026-10-23):** integrated the user's `game-design-docs/open-items.md`. Dates (demo Dec 2026 with the full Act I, 1.0 Fall/Winter 2027), the three pilgrims (Ayteel Alamoh / Warrior, Faelin Vidal / Archer, Werlem Feth / Augur) and their shared backstory, Acts II–IV mechanics and enemies, boss names (the Ice Bull, the Forsaken, Act IV TBD), the Corruptors (Malizante, Pal'bor, Echo of Gabien), Chaos and Holy to implement, cutscenes (static images and text, start and end of each act), Steam features (achievements yes, Cloud and Deck optional), Hard mode only, localization (Spanish; Chinese and Japanese optional), no DLC. The user's answers: Act II is "Uf'Faraclario Wastes"; there is one "Pool of Corruption" mechanic (no separate Pools of Darkness); the Awoken keeps fire projectiles; Frozen makes enemy hits knock the player further.
- **Not done / follow-ups:** `OnePageGDD.md` is still Spanish and outdated. 13 open questions remain at the end of the GDD (boss mechanics II–IV, Act I Corruptor name, Frozen Enemies, Will Break rules, carry-over of act mechanics, missing enemy values, cutscene content, achievements list, key items, Hard mode, Epic/Legendary items, CJK localization, the Drekos timeline). For the Orchestrator's backlog: Chaos and Holy debuffs (decided: implement), Steam achievements, Spanish localization, Act II–IV content.
- **Commits / branch:** one docs commit on `develop`, not pushed.
- **Gotchas for the next session:** numbers in the GDD are current values (boss ability cooldowns, Favor costs, flame duration ~100 s); they live in assets and the balance sheet, so the GDD may drift. Prefer describing rules over numbers when updating. Outside the kits, the effects DB has items only in Common..Heroic (quality 0-3); Epic and Legendary are defined but have no items yet.
- **Manual tests added to todo.md:** none (docs only).
