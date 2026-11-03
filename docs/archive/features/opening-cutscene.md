# Feature: Opening cutscene and replay menu

- **Status:** implemented on `feature/opening-cutscene` and manual tests passed (2026-11-02). Merged into local `develop`, which isn't pushed yet. Final art is still due from the art team (page briefs below).
- **Branch:** `feature/opening-cutscene` (off `develop`) for the implementation.
- **Owner session:** design session (done), then an implementation session (done)
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
**Constraints:**
- **Short:** a player clicking Start Run wants to play. Aim for about 5 to 8 pages, and one or two short sentences per page. If a page needs more, split it.
- **The story to tell:** the Flames of Malien are out and the Years of the Miasma have reached Abidor; the Shardless Church's augurs call for the Pilgrimage of the Flame; the three pilgrims set out. Grace and the Corruption can be hinted at, if it helps the first wave make sense.
- **No class in the opening.** The player picks a class before or after it (see the open questions), so all three pilgrims appear and none is the hero.
- **Secrets:** what `Lore.md` marks _(secret)_ never appears. Gabien's truth stays hidden: the opening can show how people see him, since that's what the arcs turn on.
- **Text rules:** Pilgrimage, never "run". No em dash, middle dot or "|" (the font lacks them; "…" exists). Short lines read better in a pixel font.
- **Art format:** cutscene art is widescreen, unlike the event art (portrait, about 848 × 1264). Agree the size with the art team; 1920 × 1080 matches the UI's reference resolution.

#### The scene
- **`cutsceneId`:** `opening`. **Replay title:** The Pilgrimage of the Flame.
- **Length:** 7 pages, about 210 words, a little over a minute at a normal pace.
- **Voice:** a neutral chronicle, with no character speaking. Line breaks in the texts below are meant, and page 6's three lines appear together.

**For the art team, on every page:**
- Widescreen, 1920 × 1080, in the same style as the event art.
- The text sits in a band across the bottom, so keep the bottom quarter free of anything that must be read.
- Keep one palette for the scene: the white of the Flames is the only pure light in it, and it fades from page to page until the last. Page 7's cyan and purple match the game's Grace and Corruption colors.
- Never draw: Malizante, the Seal of Malien, or anything being thrown into an altar.

#### Page 1: The Flames
> Long ago the Drekos came from the stars to rule the world.
> Driven underground, they were held there by the Flames of Faith, white fires on the Altars of Malien.

- **Subject:** a great Altar of Malien at night, its white Flame burning tall. The Sandir, Malien's symbol (eight points and a tear), is cut into the stone.
- **Framing:** wide and low, looking up, with the altar on the upper two thirds. Below it, a cross-section of the earth: in the rock and the dark, huge shapeless forms with a few pale eyes, pressed down.
- **Mood:** old, sacred, a held breath.
- **Who appears:** no people. The Drekos are only shapes.
- **Must be readable:** the white fire on the altar, and that something is held beneath it.
- **Must make the player understand:** the Flames keep something terrible down.

#### Page 2: The forgetting
> The faithful tended them on the Pilgrimage of the Flame, from altar to altar.
> Then the faithful forgot, and the Flames went out, one by one.

- **Subject:** a road winding over hills at dusk, with altars along it into the distance. A small line of hooded pilgrims with torches walks the near stretch; further along, the road is empty and overgrown.
- **Framing:** a wide landscape, the road leading the eye from near to far. The nearest altar burns; each one beyond is dimmer, and the furthest are dark.
- **Mood:** gentle, then sad; a thing fading rather than breaking.
- **Who appears:** nameless pilgrims, small, faces hidden.
- **Must be readable:** the line of altars going dark, and that people once walked between them.
- **Must make the player understand:** what the Pilgrimage is, and that it stopped.

#### Page 3: The fall
> Sixty years ago the Drekos rose, and Ashemdrek fell, its Flame with it.
> They say Gabien Alamoh should have saved it.
> People still spit at his name.

- **Subject:** Ashemdrek City, the white citadel, burning at night under a dark sky. Its great white Flame on the highest tower is failing, small against the fire.
- **Framing:** wide, the city filling the frame. In the foreground, seen from behind, one lone silhouette with a sword stands on a ridge facing the city. No face and no detail.
- **Mood:** defeat. Not a battle in progress: it's already lost.
- **Who appears:** Gabien, as a silhouette only. No Drekos in close-up, no enemy leader.
- **Must be readable:** a white city burning, a dying Flame, one man too late or too small.
- **Must make the player understand:** the kingdom fell, and one name carries the blame. The art must not show how the Flame died.

#### Page 4: The Miasma
> Then came the Years of the Miasma.
> A dark mist spread over Sialant, twisting all it touched, and nation after nation fell.

- **Subject:** the mist rolling over a dead country: bare fields, a ruined keep, and a road of refugees with carts heading for a grey sea and a few ships.
- **Framing:** high and wide, like a view from a hilltop. The mist comes from the left and covers about half the frame; the refugees are small, on the right.
- **Mood:** slow, heavy, unstoppable.
- **Who appears:** refugees, small. Deep in the mist, one or two vague huge shapes that seem to breathe it out; don't show them clearly.
- **Must be readable:** the dark mist as a thing that advances, and people fleeing it.
- **Must make the player understand:** why the world is failing, and that the mist is still spreading.

#### Page 5: The call
> Now it has reached Abidor, across the sea: sudden plagues, food that spoils overnight.
> The augurs of the Shardless Church say the Pilgrimage must be walked again, and the altars relit.

- **Subject:** a street in Abidor at grey dawn. The sick lie on the steps of a poor stone church, beside a market stall of spoiled food. At the church door, augurs in worn robes raise a torch or a hand, and people look up at them.
- **Framing:** eye level, the church at the right, the street leading to it. A thin haze sits at the end of the street.
- **Mood:** fear, and the first small hope. The church looks poor and proud, not grand.
- **Who appears:** townsfolk and two or three nameless augurs. Not Werlem.
- **Must be readable:** a sick town, and the Church calling out to it.
- **Must make the player understand:** why now, who is asking, and what the Pilgrimage must do.

#### Page 6: The three
> Three answer.
> Ayteel Alamoh, a Crusader of the Faith, though the Church never said so.
> Faelin Vidal, a Sentinel of Abidor, who must see it with her own eyes.
> Werlem Feth, an Augur of the Shardless Church, against his family's wishes.

- **Subject:** the three pilgrims walking out of a city gate at sunrise, with the road ahead.
- **Framing:** from the side, full body, in a row at the same size and spacing, so none is at the center or in front. Their faces are visible.
- **Mood:** quiet resolve; three people who don't know each other well yet.
- **Who appears:** Ayteel with her Steeldawn sword and shield, Faelin with her Steeldawn bow and quiver, Werlem with his scepter and Augur's robes, all as in the re-designed characters.
- **Must be readable:** each pilgrim's weapon, and that they are equals. Whichever class the player chose, this page shouldn't make another look like the hero.
- **Must make the player understand:** these three walk together.

#### Page 7: The road
> They land on the Argent Coast, with the Jesterwoods ahead and the first dark altar beyond.
> Grace is all that shields them, and the Corruption will eat at it.
> The Pilgrimage begins.

- **Subject:** a small boat drawn up on a grey shore. The three stand on the sand, seen from behind, looking at a dark forest wall with mist among the trees. Far off above the treetops, the unlit shape of an altar.
- **Framing:** a wide shot from behind them, the three small and equal in the lower middle, and the forest filling the upper half.
- **Mood:** the edge of something. Lonely, but not hopeless.
- **Who appears:** the three, with no faces.
- **Must be readable:** a faint cyan light around each pilgrim (Grace), thin purple threads of mist reaching for it from the forest (Corruption), and the dark altar as the goal.
- **Must make the player understand:** where they are going, and that their Grace is under attack. This page hands over directly to the map of the Jesterwoods.

### Systems (for the implementation session, after the narrative)
- **Data:** a `CutsceneData` ScriptableObject per scene under `Resources/`, read through a database provider, per the project's data-driven pattern. It holds a stable `cutsceneId`, a title for the replay menu, and a list of pages (sprite and text). Keep the text in the asset, so Spanish localization can reach it in 1.0.
- **Starting a scene:** play by `cutsceneId` with a callback for when it ends (from the New Game path and from the replay menu). Since 2026-11-01 a Pilgrimage runs through all the acts, so 1.0 will play scenes between acts the same way, skipping the ones already seen. Don't build that now, but don't tie the player to the New Game path.
- **Player:** a `UIWindow` that shows a page and advances on click, Enter or A. The page text sits in a band across the bottom of the image, because the art briefs keep that area free. Skip must be available at once and must not fire by accident (e.g. hold to skip, or a confirm); Back or B doesn't leave a half-played state. Fades between pages run on unscaled time. Follow `UISettings`, a restyler entry and a guard test, like every window.
- **Room for voice:** keep a page's duration out of the design (the player advances by hand), so an audio clip per page can be added later without changing how pages work. Don't add the field yet.
- **Seen state:** the seen `cutsceneId`s belong to the meta-progression (`MetaProgressionManager`, `<player>_meta.dat`), not the run save: they last between Pilgrimages. Old meta files have no such list, so it must load as empty.
- **When it plays:** only on a New Game whose `opening` isn't in the seen list yet, after Start Run and before the first map, around `MainMenuController.StartNewGame` / `LoadLevelSelectorScene(true)`. Not on Continue, and not on Retry. The load path mustn't be able to hang on bad data (`CLAUDE.md`, "An exception during a load hangs the loading screen"): a missing asset skips the scene and logs.
- **Replay menu:** the **Pilgrim's Chronicle**, on the main menu only: a panel like the other panels (`MainMenuController.Switch`), that lists seen scenes and plays one, returning to the menu afterwards. Build it as a screen with room for more tabs, so the compendium can join it instead of being built apart. A scene not yet seen isn't listed.
- **Placeholder art:** the code mustn't wait on art. Use placeholder images until the art team delivers.

## Open questions (answered by the owner, 2026-10-31)
1. **When does it play?** Only on the first New Game, when the opening isn't in the seen list yet. The replay menu covers the rest; a roguelite restarts often.
2. **Before or after class select?** After Start Run, right before the map, so the story leads straight into play. On a first demo play Archer and Augur are locked, so playing it before class select would show three pilgrims and then lock two of them.
3. **The menu's name:** **Pilgrim's Chronicle**, which also fits the compendium when it joins. It's on the main menu only, not the pause menu; the compendium can reopen that question.
4. **Art delivery:** the art team will deliver every page before the demo, so there's no fallback plan.
5. **How much Gabien:** named, in one line, as people see him: he fell, the Flame died with him, the name is hated. No Seal, nothing about how or why. The three pilgrims are shown as who they are now, not as grandchildren; the family names are left to speak for themselves.

## Acceptance criteria
_Design:_
- [x] The script is in the Narrative section, with an art brief per page, and it's short enough to read in about a minute.
- [x] Every name follows `Lore.md`'s Glossary, and no secret appears.
- [x] The open questions are answered, and GDD §11 and §13 match.

_Implementation:_
- [x] The first New Game plays the opening after Start Run, right before the map. Later New Games, Continue and Retry don't.
- [x] Pages advance and the scene can be skipped with mouse, keyboard and gamepad, with the gamepad focus right throughout.
- [x] A skipped scene counts as seen.
- [x] The replay menu lists only seen scenes, and survives closing the game. An old meta file loads with an empty list.
- [x] A missing or empty cutscene never hangs the load: it logs and goes on to the map.
- [x] Adding a scene needs only a new asset, no code.
- [x] Guard tests: unique `cutsceneId`s, every page has text and art, and the window's style follows `UISettings`.

## Files likely touched
- Docs: `CLAUDE.md` (a section on the cutscene system and the Pilgrim's Chronicle).
- Code: a new cutscene area under `Scripts/`, `MainMenuController.cs`, `MetaProgressionManager.cs`.
- Prefabs / scenes: a cutscene window prefab, `MainMenu.unity`, `Prefabs/UI/Menus/MainMenuPanel` (a new button). `MainMenu.unity` is a merge risk: no other session should touch it at the same time.
- Tests: `MainMenuStyleTests` (the new button and panel), a new cutscene test file.

## Tests
- EditMode tests to add: see the acceptance criteria.
- Manual checks (go in `todo.md` "Pending manual tests"): a first New Game, a second New Game, Continue, Retry, skip with each input device, replay after restarting the game.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:** the whole Systems section. `CutsceneData` + `CutsceneDatabaseProvider`, `Resources/Cutscenes/Opening.asset` (7 pages, spec text unchanged) with placeholder art in `Sprites/Cutscenes/Opening/`; `CutscenePlayer.Play(id, onEnded)` and the `CutsceneWindowUI` window (text band, click / Enter / A, hold to skip for 1 s with mouse, Esc or B); `MetaState.seenCutscenes`; the opening on the first New Game only; the Pilgrim's Chronicle with a tab structure (one tab, Scenes). Restyler entries (`CutsceneRestyler`, `ChronicleRestyler`, run from `MainMenuRestyler`), guard tests, CLAUDE.md section. `ScrollRectMath.ScrollToShow` was extracted from `StatsPaneUI` so the Chronicle could share it.
- **Not done / follow-ups:**
  - Pushing `develop`: the owner's decision, not done.
  - The EditMode suite last ran here before the gamepad fixes (934 of 934 passed). Those fixes added one test and were compiled with `dotnet`, then checked by the owner in play; a full run after them isn't recorded here.
  - "Adding a scene needs only a new asset" holds by design (assets load by folder, scenes play by id), but no second scene has been added to prove it.
  - Placeholder art: overwrite `Opening_Page1..7.png`; they're 1920 x 1080, imported as single sprites.
  - Mouse clicks and the Next highlight depend on the current scene being loaded (see gotchas). Between-act scenes for 1.0 must play before the unload.
- **Commits / branch:** everything is committed on `feature/opening-cutscene`, off `develop`, merged into local `develop` on the owner's OK (2026-11-02). The owner pushed the feature branch up to `ab01d38`; this last docs commit and `develop` are not pushed.
- **Gotchas for the next session:**
  - The owner's test round found two gamepad bugs, now fixed: the stick couldn't reach Back (hidden panels share the spot, so `ChronicleUI.ChainNavigation` links it by hand), and B did nothing with no player spawned (`UIFocusManager` now has its own UI actions then, on every main menu panel). With that, LB/RB work on the main menu too, so the cutscene window is `UIWindow.modal`.
  - The scene plays inside the load, between `FadeIn` and the unloads, because the EventSystems live in the scenes and the Bootstrapper has none. Moved after the unload, the mouse would stop working.
  - Gate a scene with the `cutsceneOnce` id, not with `newGame`: Continue on a save with `runStarted == false` loads with `newGame` true.
  - The cutscene canvas (101) draws over the selection frame (100), so the gamepad frame doesn't show on Next; only its sprite state does. Accepted, since A is the only thing to press.
  - `ChronicleButton` is a clone of Settings in the prefab. It inherits a click slot with an empty target, and the scene override points it at `OpenChronicle`.
  - Running Restyle Main Menu reverted leftover child layout overrides in `MainMenu.unity` (about 400 removed lines) and rewrote `CharacterSelect.prefab` with a TMP field Unity added. Both are expected.
- **Manual tests added to todo.md:** "Opening cutscene and Pilgrim's Chronicle": first and second New Game, Continue, Retry, old meta file, skip and advance with each device, skip counts as seen, text fit, glyphs, the Chronicle, and the main menu on a fresh launch. All ticked by the owner on 2026-11-02.
