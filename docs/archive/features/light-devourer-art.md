# Feature: Light Devourer's new art (isometric hit area, new sheets, no slowed windup)

- **Status:** done and archived (2026-11-06): merged into `develop` (`e48d21e`), manual tests passed.
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

Sessions on Nov 5 and Nov 6, 2026: the spec's three parts, then three rounds of fixes from the owner's play-tests, and a fix to Push All to Sheet.

- **Done (the spec):**
  - **Sheets:** committed as delivered, then renamed with their `.meta` to `LDAttack_SS_11f_192x192.png` / `LDIdle_SS_16f_192x192.png` (GUIDs kept). They import at Max Size 4096 (the idle is 3072 wide), 64 PPU, bilinear, Full Rect. Standalone isn't overridden, so it follows. Deleted `Attack.anim`, `Walk.anim` and the four `Attack_LD_1` / `Walking_LD_1` PNGs; only the old controller's unreachable states used them.
  - **Shared sheet builder:** see "Reusable code" below. The Corruptor now builds through it, and a rerun leaves all its files unchanged. The old builder rewrote `Corruptor.controller` with new file ids on every run.
  - **Light Devourer controller:** Idle (default), Walk and Attack directional trees, GUID kept. Attack is reached from Idle and Walk on the `Attack` trigger and returns at clip end, with `MeleeAnimationAttackBehavior` on it.
  - **Pivot:** kept centered, because the new feet sit 168 to 185 px down the cell, where the old art's did.
  - **Hit frame:** frame 9 of 11 (owner's choice). Attack Delay 8/11, end 10/11, `animationHitTime` 0.6666667 (8/12 s).
  - **No slowed windup:** removed `telegraphTime`, `AIMeleeAttacker.AttackAnimationSpeed`, `PlayAttack(speed)` and the enemy `AttackSpeed` parameter, which nothing else used (the player's is separate). The warning fills over `animationHitTime`. The Telegraph Time column is gone from the CSV and the sheet.
  - **Isometric hit area:** the warning and the hitbox are the same ground ellipse.
    - **Hitbox choice:** a runtime 48-point `PolygonCollider2D` built from the prefab's circle, rather than a circle filtered by an inside test.
    - **Why:** physics tests exactly the drawn shape, `EnemyDamage` stays as it was, and the circle keeps holding the design radius.
- **Done (follow-ups from the owner's play-tests):**
  1. **It never hit from the south.** The stop distance and Max Range were screen distances measured center to center, so vertically the enemy stopped twice as far on the ground. On top of that, the area orbited from the sprite's middle, where the old -0.5 offset cancelled the orbit going north.
     - **Fix:** everything is now measured on the ground from the feet (`IsoGround`, `AIMeleeAttacker.AreaCenter`).
     - **New balance column:** `AIBehaviorChaserLogic.measureOnGround`, on for `LDChase` and off for the FlameShooter.
  2. **The player drew over an enemy while standing behind it.** No script was missing: the renderer already sorts by Y (Custom Axis 0,1,0), but only within one sorting order. Every enemy body was on order 3, the player and trees on 10. All enemy bodies (`EnemyBase` and the variants' overrides) are now on 10.
  3. **The push ranged from slight to half the level.** Three causes:
     - the hitbox was solid, so physics shoved the player out of it;
     - the push was added to the player's velocity and kept sliding;
     - it pointed away from the hitbox's transform, which could aim into the enemy.

     **Fix:** the hitbox is a trigger, the push runs feet to feet along the ground, and the player's `Knockback` has **Consistent Push**: it always travels Force x Time (3 x 0.3 s).
  4. **Push All to Sheet stopped partway (after 30 tabs, then after 6).** One batch can outlast the HTTP timeout, and a timed-out run keeps the script's lock.
     - **Fix:** the timeout is 180 s, a batch that times out or loses the connection is retried twice, 10 s apart (`WithRetries`), and the error goes to the Console. Explicit refusals aren't retried.
     - **Confirmed:** the owner then pushed, published version 15 and pulled with no changes.
- **Current Light Devourer tuning (the owner's):**
  - **Body capsule:** 0.52 x 0.68.
  - **`LDAttack`:** Orbit Radius 1.3, Max Range 1.8, Attack Cooldown 1.
  - **`LDChase`:** Min Range 1, Measure On Ground on.
  - **Sheet:** the CSVs, the sheet and published version 15 all match.
- **Reusable code (for new enemies and attacks):**
  - **`IsoGroundArea`** (`Scripts/AI/IsoGroundArea.cs`, `Game.AI`, pure, all tunables passed in). The ground is drawn squashed: a circle lying on it is `groundRatio` (0.5) times as tall as it is wide on screen.
    - `ToGround(screenOffset, ratio)`: a screen offset in ground units (vertical / ratio).
    - `GroundDistance(a, b, ratio)`: the distance on the ground, the same from every side. Use it for any range or stop distance.
    - `GroundOffset(direction, distance, ratio)`: the point `distance` away on the ground in a screen direction. It keeps the direction on screen and shortens the vertical reach. Use it to place an area or a hitbox toward a target.
    - `Contains(center, radius, ratio, point)`, `Size(radius, ratio)`, `Outline(center, radius, ratio, segments)`: the ground circle's inside test, size on screen and outline (for a `PolygonCollider2D`).
  - **`IsoGround`** (`Scripts/AI/IsoGround.cs`, the scene side):
    - `Ratio`: from `AttackTelegraphSettings.GroundRatio`.
    - `PointOf(transform)`: a character's ground point, the center of its solid body collider (the first enabled non-trigger `Collider2D` on its root), else its position. Characters' transforms aren't at their feet (the Light Devourer's is the sprite's middle), so measure from this.
    - `BodyOf(transform)` and `Distance(a, b)`: the ground distance between two characters.
  - **`AIMeleeAttacker`:** `AreaCenter(enemyGround, direction, orbitRadius, ratio)` and `CreateGroundHitbox(circle, ratio, state)`, which swaps a `CircleCollider2D` for a trigger polygon outlining the same ground circle. A new melee enemy only needs the behavior asset, a `MeleeCollider` child with a circle (its radius is the area's size), and a matching `animationHitTime`. Check its tuning with the rule in `IsoGroundTests`: wherever it ends up beside the player must be within Max Range and inside the area.
  - **`AttackTelegraph.ScaleFor(sprite, width, ratio)`:** scales a round sprite into the ground ellipse. Cone or line warnings can follow the same approach.
  - **`AIBehaviorChaserLogic.measureOnGround`:** tick it on any chaser that has to reach the player in melee.
  - **`Knockback`:**
    - `ApplyKnockbackFrom(sourceGroundPoint, force)`: pushes away from an attacker's feet, along the ground.
    - `PushVelocity` / `GroundPushVelocity`: the pure math.
    - `consistentPush`: on the player only, so enemies keep the additive impulse.
  - **`EnemySheetArtBuilder`** (`Scripts/Enemies/Editor/`), for the coming enemy redraws. Describe the enemy in an `EnemyArtDefinition`:
    - folders, controller and prefab paths, pivot, `MinMaxTextureSize`;
    - `EnemySheet`s: state, file named `Name_SS_<frames>f_<w>x<h>`, loop, and `ClipEvent`s on a frame or `ClipEvent.LastFrame`;
    - the attack trigger, `ConfigureAttackState(controller, state)` for behaviours and parameters, and `ConfigurePrefab(root)`.

    It slices (keeping sprite ids), writes the 24 clips in place, builds the three directional trees and wires the prefab. The controller is built in a scratch copy and compared (`Describe`), so a rerun with nothing new changes no file.

    **Recipe:** copy `LightDevourerArtBuilder` (definition, menu item, `RunBatch`) and `LightDevourerArtTests`.
- **Tests added or changed:**
  - **New:** `LightDevourerArtTests`, `IsoGroundAreaTests`, `IsoGroundTests` (the reach rule against every player prefab), `KnockbackAndSortingTests` (push math, consistent push, every character body on the trees' layer and order).
  - **Changed:** `MeleeTelegraphTests` (speed 1, hit time = Attack Delay x clip length, at least 0.5 s), `BalanceSyncTests` (retries). `CorruptorArtTests` is unchanged.
  - **Last batchmode run:** 1031 of 1032 passed. The one failure was the stale CSVs from `47d68d14`, fixed by exporting in `c76540d4`.
- **Not done / follow-ups:**
  - **Not play-tested:** the checks in `todo.md` ("Light Devourer's new art") for the art in 8 directions, the timing, the reach from every side, draw order, knockback, stun and the Corruptor.
  - **Pivot vs body collider:** the Light Devourer's figure stands about 1.3 below its transform, while its body collider (its gameplay ground point) is at -0.5. It reads fine in play, but a pivot at the feet would make sorting and ground points exact. That would be a larger prefab change (colliders, health bar, stun point).
  - **DarkDrone:** the boss's summon is still on sorting order 0, outside the characters' order 10. Left alone on purpose.
  - **Merge:** not merged into `develop`; waiting for the owner's OK.
- **Commits / branch:** `feature/light-devourer-art`, pushed to `origin`.
  - **The spec:** `6b268b24` sheets, `b327c352` builder + animations, `a7909d4c` no slowed windup, `765ce3c7` isometric area.
  - **Fixes:** `68d64bbc` ground measuring, `d433a608` sorting + knockback, `c76540d4` Push All retries.
  - **Owner's tuning:** `053a3f15`, `47d68d14`.
  - **Docs:** the rest.
- **Gotchas for the next session:**
  - **Y sort:** it only works between sprites on the same sorting layer **and** order. Characters and trees are on 10.
  - **Feet, not transforms:** measure ranges, areas and pushes from `IsoGround.PointOf`. A screen distance straight up or down is half the ground distance.
  - **Solid hitboxes:** a hitbox that appears over a character should be a trigger, or physics shoves the character out of it.
  - **Reserialize Data Assets:** it rewrites many unrelated assets. Keep only the ones your change touches.
  - **Balance edits in the Inspector:** they need Export CSV in the same commit, or `BalanceTablesTests` fails (it happened with `47d68d14`).
  - **Batchmode:** builder batch methods, Export CSV and batchmode tests need the Editor closed; the menu items work with it open.
- **Manual tests added to todo.md:** "Light Devourer's new art (branch feature/light-devourer-art)".
