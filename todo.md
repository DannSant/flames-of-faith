# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-10-30.md`). Backlog: `docs/pm/BACKLOG.md`.

## Game feel, chunk 1: low-health warning (branch feature/game-feel)

First run Tools > Flames of Faith > UI Revamp > Restyle HUD once, so UIMain gets the vignette. Tunables: `Resources/Feel/GameFeelSettings.asset`. The vignette art is a placeholder. `DevTools/Stats` can lower Max Health to get there faster.

- [x ] Combat level: dropping to 30% health or below fades in a red pulse at the screen edges. The middle of the screen stays clear, and the pulse never reads as flashing.
- [x ] It gets stronger and a little faster as health drops toward 10%.
- [x ] Healing to 31-35% keeps it on (no flicker around 30%); healing above 35% fades it out.
- [x ] Boss level: same as a combat level.
- [x ] No warning in a shop, campfire, treasure or event, even at low health; none on the map.
- [x ] Death: the warning fades out as the death screen comes up and doesn't show over it.
- [x ] Enter a combat level already at low health: the warning is there from the start (after the fade). Enter one at full health: no flash of it.
- [x ] Retry from the death screen, then enter a level: no warning.
- [x ] Pause and level-up windows: the pulse keeps going behind them, under the window, and every button still takes clicks (the vignette takes none).
- [x ] The HUD (health, Grace, abilities, XP) draws on top of the vignette.
- [x ] Untick Low Health Warning Enabled on the settings asset in Play mode: it fades out.

## Game feel, chunk 2: sound (branch feature/game-feel)

Every new sound is silent until it has a clip. To hear them, drop any test clip into a few entries of `Resources/Audio/GameSoundTable.asset` (don't commit it), e.g. PickupXp, UiMove, LevelUp and PlayerHurt. Tools > Flames of Faith > Audio > Report Silent Sounds lists the empty ones.

- [x ] With the table as committed (no clips): a whole level plays exactly as before, with no new errors or warnings in the Console.
- [x ] Existing sounds still play: weapon attacks, enemy hits and deaths, player death, music. Their volume sounds the same as before.
- [x ] Master and SFX volume sliders change every sound, also one already playing.
- [x ] Pitch: with the Archer firing fast (random pitch) while a long sound plays, the long sound doesn't wobble.
- [x ] XP: picking up a stream of tokens rises in pitch, then starts over; the last-wave sweep that collects every token plays one sound, not a pile.
- [x ] Level-up: one sound per level-up (also when one gain gives two levels). Entering a level, Continue and Retry play none.
- [x ] Gamepad: moving between buttons in a window plays UiMove; opening a window (level-up, pause, item bag) doesn't. B plays UiBack. A click on any styled button plays UiClick, also in the pause menu.
- [x ] Shop: buying plays ShopBuy; clicking an item you can't afford plays ShopError with the red line.
- [ ] Heartbeat: at low health in a fight it beats with the pulse, and stops while paused. With Min Intensity 0.6 and Heartbeat From 0.6 it beats as soon as the warning shows; raise Heartbeat From (e.g. 0.8) to keep it for the lowest health only.
- [ ] Dodge, Whirlwind shield absorb, dash, Grace / health / item pickups, wave start and end, Corruptor appears / killed / escaped, boss second phase and boss death: each plays its sound once.
- [x ] Getting hit plays PlayerHurt, but not on a dodge, an absorbed shot or the killing blow.
- [x ] Class voices: give PlayerHurt a shared clip and a Warrior Class Clips entry with another clip. The Warrior plays its own, the Archer and Augur the shared one. Report Silent Sounds lists a player sound that has class voices but no shared clip, per class left silent.
- [x ] Death: with PlayerDeath empty the prefab's death sound plays as before; with a clip for the class, that one plays instead (once).
- [ x] Cutscene page turn: with a clip on CutscenePageTurn, every page change (click anywhere, Next, Enter, A) plays it once, and Next doesn't click as well. Continue on the last page and a skip play no page turn.
- [ ] Optional: set an `attackSound` on an enemy and a `castSound` on a boss ability; a crowd of shooters stays readable (throttled), and the boss plays it as the cast starts.

## Game feel, chunk 3: hit feel (branch feature/game-feel)

First run Tools > Flames of Faith > UI Revamp > Restyle Settings once, so the panel gets the two sliders. Tunables: the Hit feel rows on `Resources/Feel/GameFeelSettings.asset`. `DevTools/Stats` can lower Max Health to make heavy hits easy.

- [ ] Heavy hit (20% of max health or more): a small shake, a very short freeze and a medium rumble on a gamepad. Small hits do nothing. Several heavy hits in a row don't chain (0.5 s cooldown).
- [ ] Event damage (a non-combat option) never shakes.
- [ ] Death: a medium shake, slow motion (40%) for a moment, a strong rumble; the death screen then works normally, and Retry starts at full speed.
- [ ] Corruptor killed in time: a medium shake, a short freeze and a rumble. An escape does nothing.
- [ ] Boss second phase: a large shake and a strong rumble. Boss killed: a large shake, slow motion (30%) for 1 s, then the end screen; Main Menu from it, then a new run, plays at full speed.
- [ ] Routine hits and kills never shake, even at high Attack Speed with the Whirlwind.
- [ ] Pause during a hit stop or the boss's slow motion: resuming plays at the right speed (still slowed if it hasn't ended, full speed if it ended during the pause).
- [ ] The Corruptor camera focus still pans and restores normally around a shake.
- [ ] No shake on the map.
- [ ] Settings (main menu and pause): Screen shake and Rumble sliders in Gameplay, reachable with the gamepad. At 0 there is no shake / rumble; the values survive a restart.
- [ ] Rumble on a real gamepad never stays on: after it ends, after pausing mid-rumble, after a scene load, after Alt-Tab, and after unplugging the gamepad mid-rumble.
- [ ] With the keyboard and mouse active, no rumble.

## Opening cutscene and Pilgrim's Chronicle (branch feature/opening-cutscene)

To see the first New Game again or an empty Chronicle: in Play mode, DevTools > Cutscenes > Forget Seen Cutscenes. Art is placeholder.

- [x ] First New Game: Start Run fades to black, the opening plays (7 pages), then the map loads with no flash of the main menu.
- [x ] Second New Game: no opening, straight to the map.
- [x ] Continue: no opening. Also with a save made on the map before entering any level, after using Forget Seen Cutscenes.
- [x ] Retry from the death screen: no opening.
- [x ] Old save: a meta file from before this branch loads, the Chronicle is empty, and the first New Game plays the opening.
- [x ] Mouse: a click anywhere and Next both advance one page. Holding the skip bar fills it and skips after 1 s; letting go or dragging off it resets the bar.
- [x ] Keyboard: Enter advances. A tap of Esc does nothing; holding it skips.
- [x ] Gamepad: A advances one page per press (not two). A tap of B does nothing; holding it skips. Next looks selected, and the stick and LB/RB can't reach the menu behind the scene.
- [x ] A button still held when the scene opens doesn't skip it.
- [x ] Skip counts as seen: skip the opening, go back to the menu, and it's in the Chronicle and doesn't play on the next New Game.
- [x ] The text fits the band on every page (page 6 has four lines), at 16:9 and on a non-16:9 window (the art letterboxes).
- [x ] The skip glyph shows Esc on keyboard, B on Xbox and Circle on PlayStation.
- [x ] Main menu: Pilgrim's Chronicle sits between Skill Tree and Settings, and the panel slides in like the others. Nothing seen yet shows "Nothing is written here yet."
- [x ] Chronicle: lists The Pilgrimage of the Flame only after it was seen, plays it over the panel, and returns to the panel with gamepad focus on it. The list survives closing the game.
- [x] Chronicle, gamepad: the stick moves down the rows to Back, and up again, also with an empty list. A on Back and B both return to the menu, on a fresh launch and after a Pilgrimage.
- [x] Main menu, gamepad, fresh launch: B goes back from the class select, and LB/RB do nothing odd on any panel. During a cutscene LB/RB do nothing.
