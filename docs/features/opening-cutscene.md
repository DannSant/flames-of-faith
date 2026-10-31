# Feature: Opening cutscene and replay menu

- **Status:** ready for the design session. Design first, then the systems work (an implementation session), then the art team.
- **Branch:** `feature/opening-cutscene` (off `develop`) for the implementation. The design session only writes docs, on `develop`.
- **Owner session:** design session (not started), then an implementation session (not started)
- **Related docs:** `game-design-docs/Lore.md` (§2 Timeline, §5 Gabien, §6 The three pilgrims, the Glossary, the Secrets rule), `game-design-docs/TenPageGDD.md` §11 Cutscenes and §13; `CLAUDE.md` Glossary, "Gamepad & UI navigation", "UI standards", "Scene bootstrapping flow"; `docs/pm/DECISIONS.md` 2026-10-29 and 2026-10-30

## Goal
Players start the demo with no context. One short opening scene tells them what the Pilgrimage of the Flame is, why the world is failing and who the three pilgrims are, so the first map and the events make sense. A small menu lets them watch it again.

## Scope
1. **The opening scene's content** (design session): the script, page by page, and an art brief for each page.
2. **A small cutscene system** (implementation): static images and text, skippable, working with mouse, keyboard and gamepad.
3. **A replay menu** (implementation): reachable from the main menu, it lists the cutscenes the player has seen and plays them.

## Out of scope
- The other 25 cutscenes, in content or art (demo update and 1.0). The system must be able to play them later, with no code change.
- Voice narration. The research is in the demo update. Leave room for it (see Systems), but build nothing for it.
- The lore compendium. The replay menu should be easy to extend into it, but no compendium code goes in now.
- Animation, video and music composition.

## Design details

### Narrative (owned by the design session)
_To be filled. Write it so an implementation session can author it directly, and the art team can draw from it without asking._

For the scene: how many pages, its running time when read at a normal pace, and for each page:
- the text, in English, following the Glossary;
- the art brief: subject, framing, mood, who appears, and what must be readable;
- what the page must make the player understand.

**Constraints:**
- **Short:** a player clicking Start Run wants to play. Aim for about 5 to 8 pages, and one or two short sentences per page. If a page needs more, split it.
- **The story to tell:** the Flames of Malien are out and the Years of the Miasma have reached Abidor; the Shardless Church's augurs call for the Pilgrimage of the Flame; the three pilgrims set out. Grace and the Corruption can be hinted at, if it helps the first wave make sense.
- **No class in the opening.** The player picks a class before or after it (see the open questions), so all three pilgrims appear and none is the hero.
- **Secrets:** what `Lore.md` marks _(secret)_ never appears. Gabien's truth stays hidden: the opening can show how people see him, since that's what the arcs turn on.
- **Text rules:** Pilgrimage, never "run". No em dash, middle dot or "|" (the font lacks them; "…" exists). Short lines read better in a pixel font.
- **Art format:** cutscene art is widescreen, unlike the event art (portrait, about 848 × 1264). Agree the size with the art team; 1920 × 1080 matches the UI's reference resolution.

### Systems (for the implementation session, after the narrative)
- **Data:** a `CutsceneData` ScriptableObject per scene under `Resources/`, read through a database provider, per the project's data-driven pattern. It holds a stable `cutsceneId`, a title for the replay menu, and a list of pages (sprite and text). Keep the text in the asset, so Spanish localization can reach it in 1.0.
- **Player:** a `UIWindow` that shows a page and advances on click, Enter or A. Skip must be available at once and must not fire by accident (e.g. hold to skip, or a confirm); Back or B doesn't leave a half-played state. Fades between pages run on unscaled time. Follow `UISettings`, a restyler entry and a guard test, like every window.
- **Room for voice:** keep a page's duration out of the design (the player advances by hand), so an audio clip per page can be added later without changing how pages work. Don't add the field yet.
- **Seen state:** the seen `cutsceneId`s belong to the meta-progression (`MetaProgressionManager`, `<player>_meta.dat`), not the run save: they last between Pilgrimages. Old meta files have no such list, so it must load as empty.
- **When it plays:** before the first map of a New Game, around `MainMenuController.StartNewGame` / `LoadLevelSelectorScene(true)`. Not on Continue, and not on Retry. The load path mustn't be able to hang on bad data (`CLAUDE.md`, "An exception during a load hangs the loading screen"): a missing asset skips the scene and logs.
- **Replay menu:** a main-menu panel, like the other panels (`MainMenuController.Switch`), that lists seen scenes and plays one, returning to the menu afterwards. Build it as a screen with room for more tabs, so the compendium can join it instead of being built apart. A scene not yet seen isn't listed.
- **Placeholder art:** the code mustn't wait on art. Use placeholder images until the art team delivers.

## Open questions (for the design session, with the owner)
1. **When does it play?** Every New Game, or only the first one? _Proposal: only the first, with the replay menu for the rest; a roguelite restarts often._
2. **Before or after class select?** _Proposal: after Start Run, right before the map, so the class choice comes first and the story leads straight into play._
3. **The menu's name.** Players see it, and the compendium may join it. _Proposal: a lore-fitting name in `Lore.md`'s voice, chosen by the owner._ Should it be reachable from the pause menu too?
4. **Whether the art team can deliver the pages before the demo.** If not, what's the fallback: fewer pages, or re-used event art?
5. **How much Gabien** the opening carries, given that each pilgrim's arc depends on what the player doesn't yet know.

## Acceptance criteria
_Design:_
- [ ] The script is in the Narrative section, with an art brief per page, and it's short enough to read in about a minute.
- [ ] Every name follows `Lore.md`'s Glossary, and no secret appears.
- [ ] The open questions are answered, and GDD §11 and §13 match.

_Implementation:_
- [ ] A New Game plays the opening at the agreed moment; Continue and Retry don't.
- [ ] Pages advance and the scene can be skipped with mouse, keyboard and gamepad, with the gamepad focus right throughout.
- [ ] A skipped scene counts as seen.
- [ ] The replay menu lists only seen scenes, and survives closing the game. An old meta file loads with an empty list.
- [ ] A missing or empty cutscene never hangs the load: it logs and goes on to the map.
- [ ] Adding a scene needs only a new asset, no code.
- [ ] Guard tests: unique `cutsceneId`s, every page has text and art, and the window's style follows `UISettings`.

## Files likely touched
- Docs: this spec's Narrative section, `TenPageGDD.md` §11 and §13, `Lore.md` only if the script settles something new.
- Code: a new cutscene area under `Scripts/`, `MainMenuController.cs`, `MetaProgressionManager.cs`.
- Prefabs / scenes: a cutscene window prefab, `MainMenu.unity`, `Prefabs/UI/Menus/MainMenuPanel` (a new button). `MainMenu.unity` is a merge risk: no other session should touch it at the same time.
- Tests: `MainMenuStyleTests` (the new button and panel), a new cutscene test file.

## Tests
- EditMode tests to add: see the acceptance criteria.
- Manual checks (go in `todo.md` "Pending manual tests"): a first New Game, a second New Game, Continue, Retry, skip with each input device, replay after restarting the game.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
- **Not done / follow-ups:**
- **Commits / branch:**
- **Gotchas for the next session:**
- **Manual tests added to todo.md:**
