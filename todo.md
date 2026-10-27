# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-10-26.md`). Backlog: `docs/pm/BACKLOG.md`. The RunEncounter events rework notes moved to `docs/features/run-encounter-events.md`.

## Melee attack warning (LightDevourer) (2026-10-26)
When a LightDevourer starts its swing, a red ring appears on the spot it will hit and a disc grows inside it; the hit lands when the disc is full, 0.8 s later (it was 0.3 s with no warning). The direction is decided when the swing starts, and the enemy stands still while winding up, so stepping out of the ring dodges it. The attack animation plays slower to match. Placeholders: the ring and disc sprites (Sprites/Enemies/Telegraph) and colors (Resources/AI/AttackTelegraphSettings).
- [ ] First: Balance Data > Push All to Sheet (EnemyBehaviors_MeleeAttacker has two new columns: Telegraph Time, Animation Hit Time).
- [ ] Let a LightDevourer reach you: the ring shows where the hit lands, the disc fills, the hit lands right when it's full. Standing in it: you're hit. Stepping out before it fills: you're not.
- [ ] The enemy doesn't slide or turn during the windup; the swing animation looks slower but not broken.
- [ ] The ring draws above the ground, under you when you stand in it, and doesn't flicker over or under other enemies badly.
- [ ] Stun it mid-windup (e.g. a stun item): the ring disappears; after the stun it finishes the swing at the same spot.
- [ ] Kill it mid-windup: the ring goes with it.
- [ ] Tune Telegraph Time in the sheet (e.g. 1.0), Pull: the warning and the animation both take longer.

## UI revamp step 8: run encounter windows
- [ ] No event has art yet, so no side panel shows. (To try it: assign any sprite to an event's **Art** field; a framed panel appears to the left of the event window.)
