# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-10-30.md`). Backlog: `docs/pm/BACKLOG.md`.

## Opening cutscene and Pilgrim's Chronicle (branch feature/opening-cutscene)

To see the first New Game again: in Play mode, right-click `MetaProgressionManager` (Bootstrapper) > Debug: Forget Seen Cutscenes. Art is placeholder.

- [ ] First New Game: Start Run fades to black, the opening plays (7 pages), then the map loads with no flash of the main menu.
- [ ] Second New Game: no opening, straight to the map.
- [ ] Continue: no opening. Also with a save made on the map before entering any level, after using Forget Seen Cutscenes.
- [ ] Retry from the death screen: no opening.
- [ ] Old save: a meta file from before this branch loads, the Chronicle is empty, and the first New Game plays the opening.
- [ ] Mouse: a click anywhere and Next both advance one page. Holding the skip bar fills it and skips after 1 s; letting go or dragging off it resets the bar.
- [ ] Keyboard: Enter advances. A tap of Esc does nothing; holding it skips.
- [ ] Gamepad: A advances one page per press (not two). A tap of B does nothing; holding it skips. Next looks selected, and the stick and LB/RB can't reach the menu behind the scene.
- [ ] A button still held when the scene opens doesn't skip it.
- [ ] Skip counts as seen: skip the opening, go back to the menu, and it's in the Chronicle and doesn't play on the next New Game.
- [ ] The text fits the band on every page (page 6 has four lines), at 16:9 and on a non-16:9 window (the art letterboxes).
- [ ] The skip glyph shows Esc on keyboard, B on Xbox and Circle on PlayStation.
- [ ] Main menu: Pilgrim's Chronicle sits between Skill Tree and Settings, and the panel slides in like the others. Nothing seen yet shows "Nothing is written here yet."
- [ ] Chronicle: lists The Pilgrimage of the Flame only after it was seen, plays it over the panel, and returns to the panel with gamepad focus on it. The list survives closing the game.
- [ ] Chronicle, gamepad: Back with A. B to go back: try it on a fresh launch too (see the spec's Handoff, B may do nothing until a player has existed).
