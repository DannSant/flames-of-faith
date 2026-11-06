# Feature: Light Devourer's new art (isometric hit area, new sheets, no slowed windup)

- **Status:** implemented, waiting for a sheet Publish and the merge OK
- **Branch:** `feature/light-devourer-art` (off `develop`)
- **Owner session:** implementation session (Nov 5, 2026)
- **Related docs:** `docs/pm/BACKLOG.md` item 9; `CLAUDE.md` "Enemy sprite sheets & animators" (sheet layout, the Corruptor builder, the melee attack warning), "Balance data" (the pipeline rule, stale YAML), "Combat/effect pipeline" (animation events in blend trees)

## Goal
The art team delivered new sheets for the Light Devourer, with the windup drawn into the attack itself. Three changes follow:
- **The attack area:** it stays a circle (the team dropped the cone), but drawn and hit as a circle **lying on the isometric ground**, so it reads as an ellipse on screen.
- **The animations:** the clips use every frame of the new sheets.
- **The windup:** the code no longer slows the attack to fake one.

## Scope
1. **The new sheets in the game** (slicing, clips, controller), through a reusable sheet builder.
2. **No slowed windup:** the attack plays at normal speed, and the warning lasts until the hit frame.
3. **The isometric hit area:** the warning and the hitbox are the same ellipse.

## Out of scope
- Other enemies' art. The builder must be ready for them, but only the Light Devourer and the Corruptor (as a check) run through it.
- A cone or line warning.
- Damage, cooldown and range values (the designer's, in the sheet).
- The boss.

## Design details

### What's on disk now
The owner copied the new sheets over the old files in `Assets/Sprites/Enemies/LightDevourer/`; they aren't committed. Every sheet has 8 rows (directions) of 192 × 192 cells:

| File | Frames before | Frames now | Width |
|---|---|---|---|
| `LDAttack_SS_8f_192x192.png` | 9 (the name was already wrong) | **11** | 2112 px |
| `LDIdle_SS_7f_192x192.png` | 7 | **16** | 3072 px |
| `LDWalk_SS_8f_192x192.png` | 8 | 8 | 1536 px |

- **Texture size, fix first:** the importers' Max Size is **2048**, so Unity shrinks the attack and idle sheets and every frame goes blurry. Raise it to 4096 on all three. Check that the per-platform overrides follow, and keep 64 PPU, bilinear and Full Rect, as the sheet rules say.
- **File names:** rename to the real counts (`LDAttack_SS_11f_192x192.png`, `LDIdle_SS_16f_192x192.png`) together with their `.meta`, so the GUIDs and the prefab's references survive.
- **Leftovers:** `Attack.anim`, `Walk.anim` and the four old `Attack_LD_1` / `Walking_LD_1` PNGs: delete them if nothing uses them (`DataHygieneTests` spirit). Otherwise list them in the Handoff.

### 1. A reusable enemy sheet builder
- **Why:** the re-designed art is coming for every enemy, so this is the second of many. `CorruptorArtBuilder` already does it all for one enemy: slices the sheet, writes the clips per direction, and empties and refills the controller so its GUID survives.
- **What:** extract its code into a shared builder that takes a per-enemy description:
  - the sheets and their frame counts (read from the file names);
  - the pivot;
  - the clips per action;
  - the hit and end events;
  - the states' behaviours.

  Then add a Light Devourer menu item and batch method next to the Corruptor's.
- **The Corruptor must come out the same.** Rerunning its builder must leave its clips and controller unchanged (`git status` shows nothing), and `CorruptorArtTests` must still pass.
- **The Light Devourer's controller keeps its structure:** Idle, Walk and Attack 2D directional blend trees on `DirectionX` / `DirectionY`, the `MeleeAnimationAttackBehavior` on the attack state, and the `OnAnimationEventEnd` the behavior needs. Clips stay at 12 fps, with no duplicated closing key.
- **Pivot:** check that the new figure stands on the enemy's collider and shadow, and move the pivot in the builder if not. `CorruptorArtBuilder` raised its pivot to 0.46.
- **Tests:** a `LightDevourerArtTests` like `CorruptorArtTests` (frame counts per direction, 12 fps, events, the controller's GUID), and `AnimationEventTests` must still pass.

### 2. No slowed windup
- **Today:** the attack lasts `telegraphTime` (0.8 s). `AIMeleeAttacker` slows the attack state through the controller's `AttackSpeed` float to `animationHitTime / telegraphTime`. The hit fires when `MeleeAnimationAttackBehavior` passes its Attack Delay (0.4 of a 0.75 s clip, so 0.3 s).
- **From now on:** the attack plays at speed 1, and the warning fills over the real time to the hit frame, which stays `animationHitTime`.
- **The hit frame:** ask the owner which frame of the new attack lands the blow, before setting Attack Delay. Then `animationHitTime` = Attack Delay × clip length (11 frames at 12 fps = 0.917 s), and `MeleeTelegraphTests` keeps those two equal.
- **`telegraphTime` goes,** and with it the slow-down path. Keep the controller's `AttackSpeed` parameter only if something else uses it, and say so in the Handoff.
- **The pipeline:** removing the field is a balance table change. `EnemyBehaviors_MeleeAttacker` loses its Telegraph Time column, so do the whole pipeline in the same change (`CLAUDE.md` "Keep the Google Sheets pipeline in step"):
  - Export CSV, `BalanceDocs`, and Reserialize Data Assets for the stale YAML;
  - the sheet tab: **ask the owner before Push All or any sheet edit**;
  - `CLAUDE.md` "Melee attack warning".

### 3. The isometric hit area
- **The ground:** level grids are isometric with a cell of 1 × 0.5, so a circle lying on the ground is drawn **half as tall as it is wide**. Put that ratio (0.5) on `AttackTelegraphSettings`, not in code.
- **One shape for both:** the warning and the hitbox use the same ellipse: width 2 × radius, height 2 × radius × ratio. The hit area must be exactly what's drawn, or the warning lies. A pure static class (for example `IsoGroundArea`) holds the math:
  - the ellipse's size;
  - whether a point is inside;
  - the offset toward the player.

  Test it.
- **The offset:** the hitbox moves `orbitRadius` toward the player **on the ground**, so its vertical part is also scaled by the ratio. Otherwise the area drifts above the enemy when the player is north or south.
- **The warning:** scale the ring and the fill by the ratio on Y (`AttackTelegraph.ScaleFor`). The fill grows as the same ellipse.
- **The hitbox:** a `CircleCollider2D` can't be an ellipse. Make the melee collider match the drawn shape: for example a `PolygonCollider2D` built from the ellipse at runtime, or a circle broad phase filtered by the inside test before `EnemyDamage` deals damage. Choose one and say why in the Handoff.
- **Test against the player's feet:** test the ellipse where the player stands on the ground (their collider), not the sprite's center.
- **The Light Devourer's numbers stay** (Orbit Radius 1, the collider's radius). Only the shape changes. If the ellipse feels too small or too large in play, report it for the designer.

### Files and merge risks
- **Prefab and controller:** `EnemyLightDevourer.prefab` and `LightDevourer.controller`. No other open session may touch them.
- **Corruptor:** its files may only change if the rerun produces no diff.

## Acceptance criteria
- [ ] The three sheets import sharp (Max Size ≥ their width), keep their GUIDs, and their names give the real frame counts.
- [ ] Every Light Devourer clip uses all its frames in all 8 directions, at 12 fps.
- [ ] One builder makes both the Corruptor (unchanged) and the Light Devourer; both have a menu item and a batch method.
- [ ] The attack plays at normal speed, and the hit lands on the frame the owner chose, when the warning fills.
- [ ] `telegraphTime` is gone, and the sheet tab, CSV, docs and YAML are in step. The sheet was changed only with the owner's OK.
- [ ] The warning is a ground ellipse (ratio on the settings asset), and the player is hit exactly when standing inside it, in all 8 directions.
- [ ] Knockback immunity, stun and freeze behave as before (`MeleeKnockbackTests`).
- [ ] The EditMode suite passes.

## Tests
- **EditMode tests:**
  - `LightDevourerArtTests`;
  - `CorruptorArtTests` unchanged;
  - `IsoGroundArea` math (size, inside, offset, ratio 1 is a circle);
  - `MeleeTelegraphTests` updated (no slow-down, hit time equals Attack Delay × clip length);
  - `BalanceTablesTests` / `BalanceDocsTests` after the column goes.
- **Manual checks** (go in `todo.md` "Pending manual tests"):
  - the new animations in all 8 directions (idle, walk, attack), and no blur;
  - the feet on the shadow;
  - the warning's shape and timing against the hit frame;
  - standing just inside and just outside the ellipse's top, bottom and sides;
  - a stun mid-swing;
  - knockback mid-swing.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
  - **Sheets:** committed as delivered, then renamed with their `.meta` to `LDAttack_SS_11f_192x192.png` / `LDIdle_SS_16f_192x192.png` (GUIDs kept). The builder imports them at Max Size 4096 (default platform; Standalone isn't overridden, so it follows), 64 PPU, bilinear, Full Rect. The leftovers `Attack.anim`, `Walk.anim` and the four `Attack_LD_1` / `Walking_LD_1` PNGs were used only by the old controller's unreachable states and are deleted.
  - **Shared builder:** `EnemySheetArtBuilder` + `EnemyArtDefinition` / `EnemySheet` / `ClipEvent`. `CorruptorArtBuilder` is now a definition over it, with its public API kept, so `CorruptorArtTests` is untouched. `LightDevourerArtBuilder` has a menu item and `RunBatch`.
  - **Corruptor rerun:** the old builder rewrote `Corruptor.controller` with new file ids on every run. The shared builder builds a scratch controller, compares the two (`Describe`), and refills the real one only when something differs. Rerunning the Corruptor and the Light Devourer now leaves every file unchanged (checked with git status and md5).
  - **Pivot:** kept centered. The new feet sit 168 to 185 px down the cell, where the old art's did. To be confirmed in play.
  - **Controller:** Idle (default), Walk and Attack directional trees; Attack from Idle and Walk on the `Attack` trigger, back at clip end; `MeleeAnimationAttackBehavior` on Attack. The old AttackIso / WalkIso / Any State layout was replaced, and the GUID was kept.
  - **Hit frame:** the owner chose frame 9 of 11. Attack Delay 8/11, end (Collider Delay) 10/11, so the hitbox is on for the slash (frames 9 and 10). `animationHitTime` 0.6666667 (8/12 s).
  - **No slowed windup:** removed `telegraphTime`, `AIMeleeAttacker.AttackAnimationSpeed`, `EnemyAnimationController.PlayAttack(speed)` and the enemy `AttackSpeed` parameter, because nothing else used it (the player's `AttackSpeed` is separate). The warning fills over `animationHitTime`. CSV exported, `LDAttack.asset` reserialized (only that asset was kept: the tool also rewrote about 70 unrelated assets with line-wrapping and new-default churn, which I reverted). The column note comes from the `animationHitTime` tooltip, which is updated.
  - **Isometric area:** `IsoGroundArea` (pure), `AttackTelegraphSettings.groundRatio` 0.5, the ring and fill scaled by it on Y. The offset toward the player is measured on the ground: same screen direction, vertical reach × ratio.
  - **Hitbox choice: a runtime `PolygonCollider2D`** (48 points, built by `AIMeleeAttacker.CreateGroundHitbox` from the prefab's `CircleCollider2D`, which is then disabled). The alternative was a circle broad phase filtered by an inside test before `EnemyDamage`. I chose the polygon because the shape physics tests is literally the drawn ellipse, `EnemyDamage` and its collision path stay untouched, the hit is decided against the player's own collider (where they stand), and the circle on the prefab keeps holding the design radius and offset.
  - **Tests:** `LightDevourerArtTests`, `IsoGroundAreaTests` (the math, the drawn ring, and the polygon against the ellipse just inside and outside its top, bottom and sides in 8 directions), plus `MeleeTelegraphTests` updated (speed 1, hit time = Attack Delay × clip length, at least 0.5 s to react). The full EditMode suite passes: 1020 / 1020 in batchmode.
  - **CLAUDE.md:** melee warning timing and area, the sheet builder, the Light Devourer art.
- **Not done / follow-ups:**
  - **Sheet:** the `EnemyBehaviors_MeleeAttacker` tab was edited by hand with the owner's OK (Nov 5), not with Push All. Telegraph Time column deleted, Animation Hit Time set to 0.6666667, the panel's Telegraph Time row removed, and the Orbit Radius and Animation Hit Time notes set to the new tooltips. It now matches the committed CSV; the Read Me didn't mention the column. **A new Publish is still needed:** the last published version has the old column, so a Pull fails until the designer publishes again.
  - Not verified in play: the feel, the pivot on the shadow, and the ellipse size. Orbit Radius 1 and the radius 0.5 are unchanged; report the size to the designer if it feels off.
- **Commits / branch:** `feature/light-devourer-art`: `6b268b24` (sheets), `b327c352` (builder + animations), `a7909d4c` (no slowed windup), `765ce3c7` (isometric area), plus this Handoff. Not merged.
- **Gotchas for the next session:**
  - Reserialize Data Assets rewrites many unrelated assets. Keep only the ones your change touches.
  - The builders also run from their menu items with the Editor open; their batch methods, Export CSV and the tests in batchmode need the Editor closed.
  - A new redrawn enemy: a small `<Enemy>ArtBuilder` with its `EnemyArtDefinition`, a menu item, `RunBatch` and an art test. Use `MinMaxTextureSize` for sheets wider than 2048.
- **Manual tests added to todo.md:** "Light Devourer's new art (branch feature/light-devourer-art)".
