# Other issues

# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-11-06.md`). Backlog: `docs/pm/BACKLOG.md`.

## Light Devourer's new art (branch feature/light-devourer-art)

Spec: `docs/features/light-devourer-art.md`. Put a Light Devourer in a level (or wait for one) and watch it from every side.

- [ ] The Corruptor looks and shoots as before (it was rebuilt through the same tool with no file changes).

## Sounds waiting for clips (game feel, merged)

These sounds are hooked up but still have no clip (BACKLOG 33). Check them once their clips are in.

Every new sound is silent until it has a clip. To hear them, drop any test clip into a few entries of `Resources/Audio/GameSoundTable.asset` (don't commit it), e.g. PickupXp, UiMove, LevelUp and PlayerHurt. Tools > Flames of Faith > Audio > Report Silent Sounds lists the empty ones.

- [ ] Heartbeat: at low health in a fight it beats with the pulse, and stops while paused. With Min Intensity 0.6 and Heartbeat From 0.6 it beats as soon as the warning shows; raise Heartbeat From (e.g. 0.8) to keep it for the lowest health only.
- [ ] Dodge, Whirlwind shield absorb, dash, Grace / health / item pickups, wave start and end, Corruptor appears / killed / escaped, boss second phase and boss death: each plays its sound once.
- [ ] Optional: set an `attackSound` on an enemy and a `castSound` on a boss ability; a crowd of shooters stays readable (throttled), and the boss plays it as the cast starts.
