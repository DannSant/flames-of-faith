# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-10-30.md`). Backlog: `docs/pm/BACKLOG.md`.

## Game feel, chunk 1: low-health warning (branch feature/game-feel)

First run Tools > Flames of Faith > UI Revamp > Restyle HUD once, so UIMain gets the vignette. Tunables: `Resources/Feel/GameFeelSettings.asset`. The vignette art is a placeholder. `DevTools/Stats` can lower Max Health to get there faster.

- [ ] Combat level: dropping to 30% health or below fades in a red pulse at the screen edges. The middle of the screen stays clear, and the pulse never reads as flashing.
- [ ] It gets stronger and a little faster as health drops toward 10%.
- [ ] Healing to 31-35% keeps it on (no flicker around 30%); healing above 35% fades it out.
- [ ] Boss level: same as a combat level.
- [ ] No warning in a shop, campfire, treasure or event, even at low health; none on the map.
- [ ] Death: the warning fades out as the death screen comes up and doesn't show over it.
- [ ] Enter a combat level already at low health: the warning is there from the start (after the fade). Enter one at full health: no flash of it.
- [ ] Retry from the death screen, then enter a level: no warning.
- [ ] Pause and level-up windows: the pulse keeps going behind them, under the window, and every button still takes clicks (the vignette takes none).
- [ ] The HUD (health, Grace, abilities, XP) draws on top of the vignette.
- [ ] Untick Low Health Warning Enabled on the settings asset in Play mode: it fades out.

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
