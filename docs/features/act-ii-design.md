# Feature: Act II design and the run structure

- **Status:** design done and reviewed (2026-11-01). GDD and `Lore.md` changes applied. Implementation comes after the demo unless a chunk says otherwise (section 6).
- **Branch:** the design was docs only, on `develop`. Implementation: see section 6.
- **Owner session:** design session (done 2026-11-01)
- **Related docs:** `game-design-docs/TenPageGDD.md` §4, §7, §10 and §13 (questions 1, 3, 5, 6 and 14), `game-design-docs/Lore.md`; `CLAUDE.md` "Enemy damage", "Corruption & Grace", "Run save / load", "Two parallel map systems", "Balance data"; `docs/pm/BACKLOG.md` "For 1.0"

## Goal
Turn Act II from a concept into something buildable: settle how a Pilgrimage spans the acts, then design the act's mechanics, enemies and boss in enough detail that implementation sessions can start without asking, and the art team gets its next brief early.

Why now: the demo's remaining code waits on art and tester feedback, so this is free time. Design conflicts with nothing, and it produces the next round of coding work.

## Scope
1. **Run structure** (GDD Q14), decided by the owner, with what the choice costs.
2. **Act II rules:** Frozen and Frozen Enemies, including GDD Q3 and Q5.
3. **Act II enemies:** a behavior spec for each enemy and the Corruptor.
4. **The Ice Bull:** the boss's lore, phases, abilities and animations.
5. **Art brief:** what Act II needs from the art team.
6. **Build order:** implementation chunks, each marked by whether it can start on placeholder art.

## Out of scope
- Code, prefabs, scenes, ScriptableObjects, CSVs and the balance sheet.
- Final numbers. Use low / medium / high and damage tiers; the designer tunes them in the sheet.
- Acts III and IV, beyond noting what a decision here forces on them. Don't design their bosses.
- Anything in the demo. If this session finds a demo problem, write it under "Follow-ups" below.
- Cutscene content, which belongs to the opening cutscene session.

## Design details
All of this goes in the sections below. **This session edits only this file.** The opening cutscene session is working in the same checkout and may edit the GDD, so GDD and `Lore.md` changes go under "Changes for the Orchestrator", who applies them afterwards.

### 1. Run structure
**Decision (owner, 2026-11-01): Option A, one Pilgrimage through all four acts in a row.** Death means starting over from Act I.

**What it changes:**
- **Boss flow:** the boss kill goes through the level-finished path (`OnLevelCleared`, `SaveAllPlayerComponentStates`, `CommitLevelFavors`, `MarkLevelBeaten`, then the map). The save is deleted, Favors banked and the end screen shown only when no next act exists, so the demo's Act I ending stays as it is.
- **Saves:** an autosave on arriving at the next act's map. `RunSaveData` needs no new fields for this.
- **Difficulty:** `expectedCurve` extends to about 32 levels beaten (about 8 per act), and health per level gets checked across the whole game. The designer sets the numbers. Item `minLevelsBeaten` starts to matter.
- **Favors:** costs stay. A boss kill's +5 joins the run's total like any level's Favors and is banked at death or at the final victory, so no Favors are lost by continuing.
- **Boss reward:** the pick-1-of-3 fits here: it happens after the boss, before the next act's map.
- **Cutscenes:** the after-act and before-act scenes play in that gap, one after the other. Scenes seen before need a skip, since Act I's get replayed.
- **Achievements:** clearing each act, plus finishing the whole Pilgrimage.

The comparison behind the decision:

**What the code does today** (checked `MapRunController`, `OverworldMapGenerator`, `MainSceneController`, `BossController`, `BossWaveHandler`, `EndscreenControllerUI`, `PlayerManager`, `GameSession`, `RunSaveData`, `MetaProgressionManager`, `EnemyDamageCalculator`, `Enemy`, `LootRoller`, `HealthPickup`):
- **The act change exists but real play can't reach it.** `AdvanceToNextAct` only runs from `OnLevelCleared`, which runs from `PlayerManager.HandleWaveGroupFinished` (the `WaveSpawner` path) or the developer cheat. A boss level has `WaveSpawner` disabled: `BossController.HandleBossDefeated` → `BossWaveHandler.NotifyBossDied` deletes the save, banks the Favors and fires `OnBossFightEnded`, which opens the end screen. The boss's node is never marked cleared, and `levelsBeaten` doesn't count it.
- **Saving only the current act is enough for A.** Earlier acts are never revisited, and later acts regenerate from the seed. The map an act uses is picked with `UnityEngine.Random` at the act change, but it's saved by `mapId` from then on.
- **Without Act II maps, nothing changes for the demo.** "No next act" already means `IsRunComplete`, so Act I's boss stays the end of the Pilgrimage if the new boss path treats it that way.
- **Maps:** `actDefinitions` (Bootstrapper scene) holds the 3 Act I maps. A list index sets each map's seed (`seed + index × 1000`), so inserting maps changes the generation of those after it. That's harmless for a saved act, whose edges are saved.
- **`levelsBeaten`** feeds enemy damage, enemy health, health pickups and item `minLevelsBeaten`. The expected curve ends at 8 levels and then **clamps to its last row**, so damage stops growing past Act I until rows are added. Enemy health keeps growing linearly.
- **XP** needs no change: each wave's drops are sized from the player's level, so the curve can't run away.
- **Length (estimate, no measured data):** a route has at most 7 combat nodes plus the boss. At about 80 s per wave, plus nodes and windows, a long Act I is about 20–25 minutes, so a full Pilgrimage is about 1.5 hours.
- **Favors:** one class's tree costs 270, everything costs 910 including the two unlocks, or about 60 lost Pilgrimages at 12–18 each.

| | **A: one Pilgrimage, Acts I to IV** | **B: one act per Pilgrimage** |
|---|---|---|
| Fit with the design | Builds keep growing. The glossary already defines a Pilgrimage this way ("until... defeats the final boss") | Keeps sessions short, but each build ends at one act's size |
| Length | About 1.5 h for a full clear, split by Save & Exit on the map. Against "short runs" (GDD §2), and Act I gets replayed a lot | About 25 minutes, later acts reachable directly |
| Boss reward (BACKLOG 21) | Works: the pick is used in the next act | Has no use, because the Pilgrimage ends there. It needs a carry-over rule or a cut |
| Boss flow | **Small:** route the boss kill through the level-finished path, and end only on the last act | Small: keep today's flow, add a clear record |
| New systems | None | **Large:** act select, act clears in the meta save, and a starting package per act (level, stats, items, Golden Chronos, Grace) |
| Saves | Almost nothing: save after the boss, `RunSaveData` already fits | New meta data. The run save also needs the starting act |
| Difficulty | Extend one curve to about 32 levels beaten, and check health per level across the whole game. Long-range balance is the main risk | Each act has its own curve, but `levelsBeaten` needs an offset, and every starting package needs balancing per class |
| Favors | No cost change: deep Pilgrimages pay about 60–70, so reward grows with skill | 12–23 each. Act IV pays the same as Act I unless paced per act |
| Cutscenes | After-act and before-act scenes play back to back, so each needs the "seen, skip it" rule from the replay data | Clean: one pair around each Pilgrimage |
| Achievements | Clear each act, plus "finish the Pilgrimage", a real long-term goal | Simpler, but no one-build achievement |

**Acts III and IV inherit:** the same boss flow and curve rows, and each act must stay about Act I's size or smaller, or the length grows. A later "start from a relit altar" shortcut stays possible, but it costs B's starting packages, so it isn't part of 1.0.

### 2. Act II rules
**How it works today** (checked `DebuffHandler`, `DebuffFrost`, `DamageSourceBase`, `EnemyDamage`, `Knockback`, `PlayerHealth`, `PlayerCorruption`, `CorruptedDamageSource`, `DashBase`, `PlayerController`):
- Debuffs reach enemies only. `DamageSourceBase` is the player's damage path, and `DebuffFrost` only acts on `BehaviorController` and `EnemyHealth`, so on the player it would do nothing. Enemy hits use `EnemyDamage` and have no debuff path.
- **The cheapest way in is a copy of the Corruption path.** `PlayerCorruption` listens to `PlayerHealth.onDamageTaken` and checks a `CorruptedDamageSource` marker on the attacker. A `FrostDamageSource` marker and a `PlayerFrost` component can do the same. `onDamageTaken` fires only for hits that land, so Evasion, dash invulnerability, the hit invulnerability window and the Whirlwind shield all block Frost with no extra code.
- `Knockback` ignores a new knockback while one is running (0.3 s on the player), and `PlayerController` ignores movement during it.
- Dashes make the player invulnerable. Cooldowns are 1.5 s (Archer, 1 charge), 3 s (Augur, 2 charges) and 4 s (Warrior, 1 charge).

#### Frozen
**Decision (owner, 2026-11-01): Frozen is in, and Dash breaks the ice.**
- **Building up:** each landed Frost hit adds 1 Frost stack, shown as pips above the player and a growing blue tint. Each stack slows the player a little (low). Stacks fall off one at a time after a few seconds without a new one.
- **Frozen:** the third stack encases the player in ice for a short time (medium, about 1.5 s to start from). No moving, attacking, specials or items that need input. An ability in progress is cut off, like a dash cuts off the Whirlwind (`OnActionInterrupted`).
- **Breaking free:** pressing Dash with a charge ready shatters the ice at once and dashes as normal, so a saved dash is the counter. With no charge, the player waits it out.
- **Sliding:** a hit while Frozen knocks the player further than normal (the GDD's block of ice). Sliding ignores the "already in knockback" rule.
- **No chains:** when Frozen ends, the player can't gain stacks for a few seconds, shown by a pale flash.
- **Ends at once:** on wave end, on `GameplayFreeze` and on death. Stacks and Frozen aren't saved.

**Interactions:**
| System | Rule |
|---|---|
| Grace, Corruption | No direct link. A Corrupted enemy's Frost hit adds Corruption as any Corrupted hit does |
| Evasion | A dodged hit adds no stack |
| Armor | Reduces the damage only, not the stack |
| Dash | Invulnerable, so no stack. Also the way out of Frozen |
| Retribution | Still reflects while Frozen, because the hit landed |
| Whirlwind shield | An absorbed Frost shot adds no stack |
| Barrage, Fire Tornadoes | Invulnerable, so no stack |

#### Frozen Enemies
**How it works today** (checked `Destructable`, `EnemyTargeting`, `DamageSourceBase`, `EnemySpawnCoordinator`, `ItemProvider`): `Destructable` is already an `IDamageable` with health. Auto-attack, aim assist and lock-on only look at the `Enemy` and `Boss` layers, so a block on another layer is never picked as a target. `SpawnEnemy` always uses a random spawn zone and a portal, so releasing an enemy at a set point needs a new entry point.

**Decision (owner, 2026-11-01, GDD Q3): optional, and the reward is a guaranteed item pickup.**
- **Optional.** Ignoring every block costs nothing.
- **What it is:** a block of ice with a visible enemy shape inside, so the player knows the fight they're choosing. Blocks are `Obstacle`-layer objects: they stop enemy projectiles like tree trunks, and enemies steer around them. Breaking one also removes cover.
- **How many:** a few per combat level (low, 1 to 3), at positions authored in each scene, with the count set per level. They appear at level start and stay between waves.
- **Breaking:** the block has health and takes any player hit, but it's never auto-targeted, so the player has to aim or walk up to it. Stray shots can break it, and that's fine.
- **What comes out:** one enemy from a list set per level, heavier than the wave's usual mix. It starts awake, after a short shatter pause. Its Corrupted roll is the normal one.
- **Reward:** killing it always drops a random item pickup (`LootSource` Enemy Pickup, so a health pickup when no item can drop). It also keeps its normal XP.
- **Timing:** a block can't be broken once the wave timer ends. An enemy killed by the wave's end sequence drops no reward, so a last-second break earns nothing.
- **Leftover blocks** vanish with the level.

#### Carry-over into Acts III and IV
**Decision (owner, 2026-11-01, GDD Q5): Act II only.** Neither is a rule of Acts III and IV. Both belong to the frozen north, and Acts III and IV already add their own mechanics. A 1.5-hour Pilgrimage also needs each act to feel different, and Frost plus Pools plus Will Break plus Darkness would be a lot to read at once.
- **Frozen Enemies:** Act II scenes only. Later acts place no blocks.
- **Frozen:** the code stays general, because it comes with the enemy (`FrostDamageSource`), not with the act. So Frost only appears later if an enemy from Act II does.
- **What this leaves open for Act IV:** the Awoken Summoner summons "an enemy from a previous act", so a Frozen Slime could appear there. That's allowed and needs no extra work. The Act IV design decides whether its list includes Frost enemies.
- **GDD §7** says each act adds a mechanic "on top of the previous ones". That line needs changing to match.

### 3. Act II enemies
Checked: `AIBehaviorShooter`, `AIBehaviorShootBurst`, `AIBehaviorChaserLogic`, `AIBehaviorFleeLogic`, `AIBehavior`, `BehaviorContext`, `CorruptorPhaseController`, `EnemySpawnCoordinator`, `TargetIndicatorUI`, and the `EnemyBehaviors_*`, `Enemies`, `EnemyDamageTiers` and `Corruption` tables.

**Shared by every entry:**
- **Tiers** are starting points for the designer. Today's table: tier 1 = 10 hits to kill the expected player, 2 = 7, 3 = 5, 4 = 3, 5 = 2. Act I reference: slimes contact 1, Light Devourer contact 4, Dreko Spawn contact 2.5, projectile 2.
- **Health** is relative to Act I: low = Dreko Eye (35), medium = Slime Chaser (55), high = Light Devourer (100).
- **Sheets** follow "Enemy sprite sheets & animators": 8 directions, 64 PPU, 12 fps, one blend tree per action.
- **Frost** means a `FrostDamageSource` marker on the prefab that deals the hit (section 2).
- **A burst can add only 1 stack per 0.5 s,** because `onDamageTaken` respects the hit invulnerability window. Frost is built up by several enemies, not one volley.
- **`EnemyType` values are appended,** never inserted. Enemy projectiles are new prefabs in the style of `EnemyProjectile`.

#### New behavior pieces
| Piece | Used by | What it does | New balance tab |
|---|---|---|---|
| `AIBehaviorOrbitPlayer` | Death Witch | Sets `moveDirection` to circle the player at a set radius, and routes through `ObstacleSteering` | `EnemyBehaviors_OrbitPlayer` |
| `AIBehaviorBeamAttack` | Death Witch, could serve bosses | Locks a line, shows a telegraph, then deals one hit along it, stopped by the first `Obstacle` | `EnemyBehaviors_BeamAttack` |
| `AIBehaviorBlink` | Echo of Malizante, could serve later echoes | Hides, moves, telegraphs, reappears on a timed cycle | `EnemyBehaviors_Blink` |

#### Frozen Slime
- **Role:** the act's basic enemy, teaching that Frost comes from a distance. Pairs with the Frost Ghoul, which punishes whoever stops to dodge shards.
- **Why not a reskin:** the Act I chaser has no ranged attack.
- **Behavior:** hops toward the player like the Slime Chaser. Every so often it stops between hops and fires one ice shard. The shot is telegraphed by a short swell in its clip. Shards stop at obstacles. Inflicts Frost with shards only.
- **Built from:** `AIBehaviorChaseOnAnimIntervals`, `DamageOnCollisionBehavior`, `AIBehaviorShooter` (Hold Still off, since it fires between hops), `DropExperienceOnDeathBehavior`. No new piece.
- **Numbers:** contact 0.75, projectile 3. Health low, speed like the Slime Chaser.
- **Animations:** Idle, Hop, Shoot (`OnAnimationEventStart` on the release frame), Die.
- **Lore:** "A slime of the Miasma that drifted north and froze from the inside. It spits what the cold left of it."

#### Ice Sprite
- **Role:** a weak ranged pest that adds Frost pressure. Pairs with the Ice Golem, which blocks the way while it shoots.
- **Closest Act I enemy:** the Dreko Eye, which uses the same attack piece. The difference is a much smaller body, less health and a fast, narrow volley. This is the closest thing to a reskin in the roster, kept by the owner's choice (see "Roster").
- **Behavior:** floats, keeping medium range, stops, and fires a quick burst of many weak shots in a narrow fan. Shots stop at obstacles. Inflicts Frost.
- **Built from:** `AIBehaviorChaserLogic` (minimum range), `AIBehaviorMoveToSnapshot`, `AIBehaviorShootBurst`. No new piece.
- **Numbers:** contact 0.5, projectile 0.5. Health low, speed medium.
- **Animations:** Idle, Move, Attack (start event per volley), Die.
- **Lore:** "Sprites were the small spirits of the northern snow, until the Miasma reached them. They still dance. They no longer play."

#### Awoken
- **Role:** the long-range threat that makes cover matter. Pairs with Frozen Enemies, since breaking a block near it removes cover.
- **Why not a reskin:** it's the Flame Shooter's opposite: very slow, very heavy shots from far away. Dodging one is a decision, not a reflex.
- **Behavior:** walks to long range and stands still to cast. A long windup with a flame growing in its hands warns the player, then one big, slow fireball flies straight. It stops at obstacles and ice blocks. Fire, no Frost.
- **Built from:** `AIBehaviorChaserLogic` (large minimum range), `AIBehaviorChaserMovement`, `AIBehaviorShooter` (Hold Still on, large max range). No new piece.
- **Numbers:** contact 1, projectile 4.5. Health medium, speed slow.
- **Animations:** Idle, Walk, Attack (start event late in the clip, end event on the last frame), Die.
- **Lore:** "A Forsaken buried for a night, who rose without will and without death. The fire in its hands is the only warm thing left in it."

#### Ice Golem
- **Role:** a wall that takes space away, and the safest way to feel Frozen coming. Pairs with the Ice Sprite and the Awoken.
- **Why not a reskin:** the Light Devourer stops, telegraphs and swings. The Golem never stops and has no attack, so the danger is being cornered by it.
- **Behavior:** walks straight at the player, slowly and without pause, and steers around obstacles. Contact deals heavy damage, inflicts Frost and knocks the player back hard.
- **Built from:** `AIBehaviorChaserLogic`, `AIBehaviorChaserMovement`, `DamageOnCollisionBehavior`, `KnockbackBehavior`. No new piece.
- **Numbers:** contact 3.5. Health high (above the Light Devourer), speed very slow.
- **Animations:** Idle, Walk, Die. No attack clip, so it's the cheapest sheet to draw after the Frozen Slime.
- **Lore:** "Faraclarians say the Winter Shamans raised these to guard the passes. Nobody gives them orders now, and they guard nothing. They only keep walking."

#### Frost Ghoul
- **Role:** the fast melee threat that forces a dash, which is also the way out of Frozen. Pairs with the Frozen Slime.
- **Why not a reskin:** the Light Devourer's attack piece, tuned the other way: small, fast, a short warning.
- **Behavior:** runs at the player, then a quick claw swipe in a direction locked at the start, with a short telegraph. It holds still during the swipe, which is the window to punish it. Inflicts Frost.
- **Built from:** `AIBehaviorChaserLogic`, `AIBehaviorChaserMovement`, `AIMeleeAttacker`. No new piece, but its animator needs the `AttackSpeed` parameter and its own `animationHitTime` (`MeleeTelegraphTests`).
- **Numbers:** contact 2.5. Health medium, speed fast.
- **Animations:** Idle, Run, Attack (`MeleeAnimationAttackBehavior`), Die.
- **Lore:** "The dead of the scattered clans do not rest in Fara Uf'Clario. The cold keeps the body, and the Miasma keeps the hunger."

#### Death Witch
- **Role:** the priority target. The player has to break off and kill it, or respect its beam. Pairs with anything that crowds the player.
- **Why not a reskin:** nothing in Act I circles the player or fires a line attack.
- **Behavior:** flies in a circle around the player at medium range. Every so often it stops and locks a line toward the player, and a thin line grows thicker on the ground for about 1 second. The beam then fires once along that line. Obstacles and ice blocks stop it, so cover works. No Frost: its threat is the hit itself.
- **Built from:** new `AIBehaviorOrbitPlayer` and `AIBehaviorBeamAttack`; existing `DamageOnCollisionBehavior`. The line uses the `AttackTelegraph` look.
- **Numbers:** contact 1, beam 5 (projectile tier). Health low, speed fast.
- **Animations:** Idle / Fly, Cast (loop while the line grows), Release (start event fires the beam, end event ends it), Die.
- **Lore (see question for the owner in "Changes for the Orchestrator"):** "They were scouts once, and they knew every pass in the north. What flies over the snow now remembers the way, and nothing else."

#### Echo of Malizante (Corruptor)
**What `CorruptorPhaseController` assumes:** one GameObject from spawn to end. `corruptor == null` means it's gone, `EnemyHealth.onDeath` means killed, and it escapes through `DespawnThroughPortal`. Camera focus and the off-screen arrow use its transform. So the Echo must **stay one object and hide**, never be destroyed and respawned. Then the timer, the Corruption scoring and the phase bar need no change. No `CorruptorPhaseController` edit is planned.

- **Role:** a kill-speed check that asks for positioning, not a chase. Its visible time is limited, so burst damage and Attack Speed both matter.
- **Why not a reskin:** the Dreko Spawn runs away. The Echo comes to the player.
- **Behavior:**
  1. It leaves its portal normally, and the usual camera focus shows it. `AIBehaviorBlink` is stopped during the freeze.
  2. **Hidden:** renderers and colliders are off and `EnemyHealth` is immune. Lock-on, aim assist and auto-attack skip it, and damage over time does nothing, while the lock itself stays on it.
  3. **Warning:** a ground mark shows where it will appear (about 0.5 s). The spot is on a ring around the player, on screen, and not inside an obstacle or out of bounds.
  4. **Visible:** it appears, fires an ice projectile that inflicts Frost, stays vulnerable for a few seconds, then fades and repeats.
- **Off-screen arrow:** it matters only before the first blink. After that the Echo always appears on screen.
- **Escape:** if the timer ends while it's hidden, the portal opens at its last spot, which is fine because the Echo is leaving anyway.
- **Built from:** new `AIBehaviorBlink`; existing `AIBehaviorShooter` (Hold Still on), `FlipTowardPlayerBehavior`. New `EnemyType.CorruptorAct2`, set per `WaveDatabase`. Always Corrupted, so its hits count as Corrupted Damage.
- **Numbers:** projectile 2, no contact damage. Health must suit limited visible time: tune so a normal build kills it within the fast kill time (15 s), which means lower than the Dreko Spawn's 200.
- **Animations:** Appear (end event starts the visible state), Idle, Attack (start event fires), Vanish. No walk. No Die sheet needed if Vanish can play on death.
- **Lore:** "Malizante burned in the last altar, and still her echo walks the snow. It comes close, it strikes, and it is gone, as if it were looking for something."

#### Roster
**Decision (owner, 2026-11-01): keep all six enemies plus the Echo.** The Ice Sprite was recommended for a cut and the owner kept it. It's the last to be built, since it needs no new piece.

If art runs short later, cut in this order: Ice Sprite, Ice Golem (the Light Devourer can hold the "heavy" slot in some waves), then Frozen Slime. Keep the Death Witch and the Echo, because they carry the act's new attacks.

### 4. The Ice Bull
**Decision (owner, 2026-11-01): the bait charge, plus a growing pressure ability in each phase.** Checked: `BossController`, `Act1BossBehavior`, `BossBehavior`, `BossMovement`, `BossRenderer`, `BossAltar`, `AltarAttacker`, `BossWaveHandler`, `BossAbilityBase`, `BossAbilityRuntime`, `VoltOfDarknessAbility`, `CorruptionBurst`, `FlameBarUI`, `ProjectileMoveBounce`.

**Structure: same two phases, a different first phase.** The GDD's proposal would repeat Act I: wait for a timer while dodging. Here the first phase keeps the Altar, the flame bar and the invulnerable boss, but **the player fills the flame by baiting the Bull's charge into ice pillars**, while **Frozen Axes** threaten both the player and the Altar. The flame also fills slowly by itself. The Act I lesson is "survive", and this one is "use the boss's strength against it". Both phases escalate the longer they last, as Taio'Sandir's enrage does.

**Lore (owner, 2026-11-01).** The Ice Bull is a Plerian, one of the first Plerians the Drekos corrupted. He was defeated during the Dark Age, but Malizante saved him and buried him in the frozen north, transforming him into what he is now. The name comes from locals who saw his horns and took him for a bull. He isn't a Dreko, which sets him apart from Taio'Sandir. It also ties him to the Echo of Malizante, the act's Corruptor: her echo walks the same snow.

**Player-facing line (draft):** "The clans named it for its horns. They saw a beast in the ice, and they called it the Ice Bull. Nobody who came close enough to see its face ever told them otherwise."

#### Phase 1: Break the ice
The Bull stands before the dark Altar, armored in ice and invulnerable (hits show IMMUNE). Ice pillars stand around the arena, each with an enemy shape inside. Enemies spawn from portals as in Act I.
- **Ice Charge** (main ability): it turns to the player and paws the ground while a line grows on the floor (about 1 s), locks the line, then charges along it. If it hits a pillar, the pillar shatters, the Bull is stunned for a few seconds, its armor cracks and **the flame jumps up by a large share**. If it hits nothing, it skids to a stop. The Altar stops a charge like a wall and takes no harm from it. Hit: heavy damage, Frost and hard knockback.
- **Frozen Axes** (after every charge): the pressure of this phase.
  - **When:** after a miss, once it has skidded to a stop. After a crash, once the stun ends, so the stun stays a safe moment to reposition.
  - **The throw:** it hurls the axes at the player, spread in a small fan.
  - **Bouncing:** an axe bounces off the player, enemies, pillars and the arena's walls. The 3rd bounce is its last, and its next contact destroys it. A max lifetime is the safety net. Axes never hurt enemies and never shatter pillars.
  - **Hit:** projectile tier 2 (about 7 hits to kill the expected player), with Frost. A dash passes through an axe. A dodged hit with Evasion still bounces it.
  - **Escalation:** each throw adds one axe and one bounce: 1 axe with 3 bounces, then 2 with 4, then 3 with 5, up to a cap (about 4 axes, 6 bounces). Every throw counts, so missed charges make the fight harder. It doesn't reset within the phase.
  - **The Altar:** an axe that touches the Altar **shatters and takes flame away**. So the player's choice is the Act I one, in a new form: stand in the axe's path and take the hit, or lose flame.
- **The flame's rule:** it can't go below empty, and **a crash is always worth more than a capped throw landing entirely on the Altar** (a guard test checks the asset), so baiting well always moves the fight forward. A player who only ever misses gets the slow fill and rising danger.
- **Pillars:** the player can't break them. A shattered pillar releases its enemy, with no item reward. Boss-level pillars aren't the Frozen Enemies blocks of normal levels, but they share the prefab and the release code.
- **Glacial Stomp** (when few pillars are left): it rears and stamps, and new pillars rise at marked spots after a short warning. So the Bull always has targets.
- **Frost Breath** (cooldown): a short channel spraying Frost shards in an arc toward the player. It's the player's reason not to stand still behind a pillar. Pillars block the shards. If play-tests find phase 1 too busy now that it has axes, this is the first thing to remove from it.

**End of phase:** 3 crashes fill the flame if no axe reached the Altar (tunable). The transition uses the existing `OnPhaseTwoStarted` flow: the Flame strikes the Bull, the last ice falls off, the enemies stop spawning, and any axe still flying shatters. Player death clears them too.

#### Phase 2: The real fight
No armor, no pillars left and no adds. The Bull is vulnerable and moves between patrol points like Taio'Sandir, but casts more often.
- **Double Charge:** two Ice Charges in a row, the second re-aimed, each with its line.
- **Shatter Ring:** it stamps, the ground cracks around it, then a ring of ice shards flies outward with gaps to pass through. Frost.
- **Frost Breath:** as in phase 1, longer.
- **Deep Freeze** (the pressure of this phase):
  - **The mark:** a large circle appears on the ground at the player's position and stays there. A disc grows to fill it over a warning (about 1.5 s), in the `AttackTelegraph` style.
  - **The freeze:** when the disc fills, a player inside is **Frozen at once, with no stacks needed**. Anyone who walked out, or is dashing at that moment, is safe. It deals no damage: the danger is what lands next.
  - **Rules:** the normal Frozen rules apply, so a dash with a charge ready breaks the ice, and the immunity after Frozen also blocks a Deep Freeze.
  - **Escalation:** it starts with a long interval, and **the interval gets shorter the longer the phase lasts**, down to a minimum. Time drives it, not the Bull's health, so a slow fight gets harder. A guard test keeps the minimum longer than the warning plus the Frozen time plus the immunity, so the player always gets a free moment.

Corrupted by default, like Taio'Sandir. Killing it grants +5 Favors, then the boss reward and the map of Act III (section 1). In the 1.0 build; it never appears in the demo.

**Fairness with Frozen:** Frost builds on the Bull's hits as usual. The stack-immunity after Frozen (section 2) stops a chain, and a saved dash breaks the ice, so Frozen in the charge line is a mistake, not a death sentence. Keeping a dash for Deep Freeze is the phase's main decision.

**Animations** (8 directions, as Taio'Sandir's): Idle, Walk, Paw (charge windup, loop), Charge (run, loop), Crash (stunned, loop), Throw (start event releases the axes), Stamp (also casts Deep Freeze), Breath (start, loop, end), Damage (phase transition), Die. No fade, because it never hides.

#### What the code can't do yet (scope of the boss refactor, BACKLOG 20)
| Need | Today | Change |
|---|---|---|
| Continue after a boss kill | `NotifyBossDied` always ends the run; `EndscreenControllerUI` listens to `OnBossFightEnded` | Section 1's boss flow |
| A boss that doesn't start hidden | `BossRenderer.Awake` hides it; abstract fade names on `BossBehavior` | A "starts visible" option; fade optional |
| Shared boss logic | Patrol and summon cleanup live in `Act1BossBehavior` | Move to a shared base or component; add `IceBullBehavior` |
| Flame filled by actions | `BossWaveHandler` has `ReduceFlameProgress` only, and a timer | Add `AddFlameProgress`; timer stays as the slow fill. Flame loss needs nothing new: `BossAltar` already reads `AltarAttacker` from whatever touches it, so the axe prefab carries one |
| Bouncing enemy projectiles | `ProjectileMoveBounce` is built for player projectiles: it derives from `ProjectileMovementBase`, skips trigger colliders, and has no way to set bounces at runtime. Enemy shots use `ProjectileMovement` | A bouncing mode for enemy shots with a settable bounce count and a layer mask for what it bounces on, reusing `ProjectileMoveBounce`'s reflection. Also a max lifetime, and it shatters on the Altar. Check at build time which of the player's and enemies' colliders are triggers |
| Abilities that escalate by use or time | `BossController.GetEnrageLevel` rises only when eyes die | Per-fight counters on the ability's runtime: throw count for axes, phase time for Deep Freeze's interval. `BossAbilityRuntime` reads a fixed `cooldown`, so it needs a way to ask for a varying one |
| Forced Frozen | Frozen is built from stacks | `PlayerFrost` gets a direct "freeze now" that respects Dash invulnerability and the immunity (section 2) |
| Boss damage that scales | `GetAbilityDamage` takes a flat number and rounds it to an int, unrelated to the expected player. At 16 levels beaten that's wrong | Ability damage by tier through `EnemyDamageCalculator`, fractional |
| Charges | `BossMovement` sets `transform.position`, so it ignores colliders, and has no speed change | A charge mode that detects pillars and walls, stops, and reports what it hit |
| Body hit only while charging | No body hitbox tied to an ability | A trigger with `EnemyDamage` and `FrostDamageSource`, on during a charge |
| New abilities | Bolt, burst, summons | `IceChargeAbility`, `FrozenAxeAbility`, `GlacialStompAbility`, `ShatterRingAbility`, `DeepFreezeAbility`. Frost Breath is a new `CorruptionBurst` asset with a Frost projectile |
| Numbers in the sheet | Boss abilities and `flameDuration` are Inspector only, not balance data | New `BossAbilities` table, plus flame values |
| Boss identity | `EnemyType.Boss` is one row | Append `BossAct2`, plus an `Enemies` row |

### 5. Art brief
For the art team, in priority order. Everything marked **Essential** is needed for Act II to ship; **Nice to have** can go if the queue runs long. Enemy sheets follow the usual format: one row per direction (S, SW, W, NW, N, NE, E, SE), 12 fps, file name `Name_SS_<frames>f_<w>x<h>.png`, with room around the figure for the Corrupted outline. Death needs a frame in every direction only if the enemy's Die clip shows one.

**1. The world (Essential, first, because everything else is judged against it)**
| Asset | Notes | Reuse |
|---|---|---|
| Snow ground tileset | Isometric, for about 9 combat levels, like Act I's | New |
| Obstacles | Small, convex, like Act I's tree trunks: dead pines, rocks, frozen stumps. They block enemy projectiles | New |
| Decor | Faraclarian camps, broken banners, fences | Recolor of Act I's where it works |
| Encounter scenes | Campfire, Plerian Shop, Treasure, Wayside Shrine of Malien, event background | Same layouts as Act I, snow version. The shopkeeper and the chest can stay |
| Boss arena | Open floor around the Altar, with clear edges (axes bounce off them) | New, sized like Act I's |
| Altar of Malien | Frozen and dark, then lit, with the same flame stages as Act I | Recolor of `Altar_ss_128x128` |
| Overworld map | Act II background, node look | New background, nodes stay |

**2. Ice Bull (Essential)**
- A large figure, about Taio'Sandir's 256×256 cell. A tall, horned, ice-armored humanoid: a Plerian transformed, not an animal. From far away the horns must read as a bull's, and up close it must look like something that was once a person.
- Two looks: **armored** in ice (phase 1) and **broken free** (phase 2). If that's too much, keep one sheet and show the armor as an overlay that breaks at the transition.
- Sheets: Idle, Walk, Paw (loop), Charge (loop), Crash (loop), Throw, Stamp, Breath (start, loop, end), Damage, Die.
- Pieces: Frozen Axe (spinning, about 8 frames, one direction), its shatter.

**3. Ice block and pillars (Essential)**
- One block of ice with a dark enemy shape inside: intact, cracked, shatter animation. Normal levels and the boss's pillars share it. The shape needs to hint at the enemy, so make it swappable, or one silhouette per enemy if it's cheap.
- A taller pillar version, if the block looks too small for a charge to hit.

**4. Enemies, in build order (Essential unless noted)**
| Enemy | Cell | Clips | Notes |
|---|---|---|---|
| Ice Golem | Large | Idle, Walk, Die | Cheapest sheet: no attack |
| Frost Ghoul | Medium | Idle, Run, Attack, Die | The swipe's hit frame must be clear |
| Frozen Slime | Small | Idle, Hop, Shoot, Die | Can start from the Slime Chaser's shape |
| Awoken | Medium | Idle, Walk, Attack, Die | Flame growing in its hands during the windup. Shares a look with Forsaken art if any exists |
| Death Witch | Medium | Fly, Cast (loop), Release, Die | Flying, so no walk |
| Echo of Malizante | Medium | Appear, Idle, Attack, Vanish | Winged, spectral, recognizably the Sea Witch. The opening cutscene never draws her, so this is her first look. Vanish can double as its death |
| Ice Sprite | Small | Idle, Move, Attack, Die | Last in line, and the first to cut (section 3) |

**5. Projectiles and VFX (Essential; simple versions are fine)**
- Ice shard, big fireball (Awoken), Frost Breath shard, Shatter Ring shard, Echo's ice projectile. One ice shard can serve the first, third and fourth with recolors.
- Death Witch beam: the line and the hit.
- Player Frost: Frost pips, a growing blue tint (a shader or color can do it), **Frozen** ice around the player, and the shatter when a dash breaks it. It must work on all three pilgrims, so draw it as a generic block.
- Stack immunity: a pale flash.
- Ground warnings: the charge line and the Deep Freeze circle and disc. Placeholders exist in `Sprites/Enemies/Telegraph/`.
- Echo's ground mark.
- Bull: hit sparks on crash, ice falling at the transition.

**6. Nice to have**
- A portrait or card for the Bull in menus, an Act II event art set (the events themselves are out of this spec), snowfall in levels, footprints.

**Reuse without new art:** the Frost debuff icon, health and item pickups, spawn portals, XP tokens, all UI frames. Audio isn't in this brief.

### 6. Build order
**Rule for the December demo:** nothing here is needed for it, so the safest order is to build all of this after the demo ships. If there's free time before that, chunks marked **Safe on `develop`** add no risk. Chunks marked **Branch** change the demo's flow or add content the build would pick up, so they stay on a branch until the demo is out. `DataHygieneTests` allows no dead data: every behavior asset must be used by an enemy prefab, every `LevelData` must be on a map and its scene in the build. That's why the content chunks share one branch: they can't land piece by piece without Act II scenes entering the demo build.

**Branches (Orchestrator, 2026-11-01):** chunk 1 goes on `develop`. Everything else goes on one branch, `feature/act-ii`, one session at a time, because chunks 2 and 9 both touch the boss flow and the content chunks can't land piece by piece. Chunk 3 stays there too unless the demo's code is frozen first: it touches the player prefab, which the re-designed characters also change.

| # | Chunk | Depends on | Placeholder art? | Balance tabs | Where | Guard tests |
|---|---|---|---|---|---|---|
| 1 | **Curve and long-range check:** extend `expectedCurve` to about 32 levels, check enemy health against it, list which items gain a `minLevelsBeaten` | Nothing | Not needed | No new tab; new rows in `EnemyDamageCurve` | **Safe on `develop`:** the demo never passes level 8 | `EnemyDamageTuningTests` already cover it |
| 2 | **Boss flow:** the boss kill goes through the level-finished path; ending only when no next act exists; autosave at the next act's map | Nothing | Not needed | None | **Branch:** it changes how the demo's run ends, though the result must look identical | Pure rule "is this the last act?"; Act I ending unchanged, checked by hand too |
| 3 | **Frozen:** `FrostDamageSource`, `PlayerFrost` (stacks, Frozen, Dash break, sliding, immunity, freeze now), pips and tint | Nothing | Yes, a tinted block | New `Frozen` settings tab | **Safe on `develop`:** dormant, since no demo enemy has the marker. It does touch the player prefab, `DashBase` and `Knockback`, so land it only after the demo's code freeze or test those by hand | `PlayerFrostTests` (pure rules); pips follow the UI recipe (restyler entry, style guard test) |
| 4 | **Act II shell:** appended `EnemyType` values, Act II `MapDefinition`s, placeholder levels and encounter scenes, map art, one `WaveDatabase` | 2 | Yes, Act I tiles recolored | Rows in `Levels`, `WaveLevels`, `Waves`, `WaveEnemies` | **Branch** | `MapDefinitionTests` (unique ids), `DataHygieneTests`, `LevelSelectorStyleTests` (readable names) |
| 5 | **First enemies, no new pieces:** Ice Golem, Frost Ghoul, Frozen Slime, Awoken | 3, 4 | Yes, tinted Act I sheets | Rows in `Enemies`, `EnemyBehaviors_*` | **Branch** | `AnimationEventTests`, `MeleeTelegraphTests` (Ghoul), `EnemyDamageTuningTests` |
| 6 | **New pieces:** Death Witch with `AIBehaviorOrbitPlayer` and `AIBehaviorBeamAttack`; Echo of Malizante with `AIBehaviorBlink` and `CorruptorAct2` | 4 | Yes | `EnemyBehaviors_OrbitPlayer`, `EnemyBehaviors_BeamAttack`, `EnemyBehaviors_Blink` | **Branch** | Pure orbit and blink math; a test that the Echo stays one object through the cycle |
| 7 | **Frozen Enemies:** the block, spawn at a point, guaranteed item pickup, the no-reward rule at wave end | 4, 5 | Yes, a blue box | Count and enemy list per level, a new `LevelFrozenEnemies` tab or columns in `Levels` | **Branch** | A rule test for the reward; a scene check that blocks sit on valid spots |
| 8 | **Ice Sprite** | 5 | Yes | Rows only | **Branch** | As chunk 5 |
| 9 | **Boss refactor** (BACKLOG 20): the section 4 table, without Bull content. Taio'Sandir must play the same after it | 2 | Not needed | New `BossAbilities` table, flame values | **Branch:** it touches the demo's boss | Boss damage by tier; the demo boss's numbers mustn't change by more than rounding (`Report` before and after) |
| 10 | **Ice Bull:** abilities, pillars, arena, `BossAct2` | 3, 7 (shared block), 9 | Yes, a tinted Taio'Sandir | Rows in `BossAbilities` and `Enemies` | **Branch** | A crash beats a capped throw; Deep Freeze's minimum interval leaves a free moment; every Bull ability has a tier |
| 11 | **Boss reward** (BACKLOG 21) | 2 | Yes | New drop source in `LootSources` and `ItemDrops` | **Branch** | `ItemDropTableTests` |
| 12 | **Act II pass:** real art swaps, wave compositions, a designer's balance round, then remove placeholder sheets | All | No | None | **Branch**, then merge | `DataHygieneTests` |

**Pipeline cost:** every new tab above needs the whole "Balance data" rule (table, CSV, notes, workbook tab, Push All). Chunks 3, 6, 7 and 9 each carry that cost, so include it in their estimates.

**Order against art:** nothing is blocked by art except chunk 12, so code can run ahead of the art queue. Art for the world (section 5, group 1) should still start early, because it decides how readable the enemies, the shards and the ice block will be.

### Open questions for the owner
_Filled during the session. Ask one at a time, each with a recommendation._
1. **Run structure (GDD Q14):** A or B? **Decided: A** (owner, 2026-11-01).
2. **Frozen:** in, with Dash breaking the ice? **Decided: yes** (owner, 2026-11-01).
3. **Frozen Enemies (GDD Q3):** optional, with a guaranteed item pickup as the reward? **Decided: yes** (owner, 2026-11-01).
4. **Carry-over (GDD Q5):** do Frozen and Frozen Enemies stay as rules in Acts III and IV? **Decided: no, Act II only** (owner, 2026-11-01).
5. **Roster:** cut the Ice Sprite from 1.0? **Decided: no, all six stay** (owner, 2026-11-01).
6. **Ice Bull first phase:** bait the charge into pillars to fill the flame, instead of Act I's wait? **Decided: yes, with Frozen Axes as phase 1's pressure, Deep Freeze as phase 2's, and the owner's lore** (owner, 2026-11-01).

### Follow-ups
- **Demo check, short routes (checked by the Orchestrator, 2026-11-01):** the fear of 1 combat node was wrong, since those were side paths that end before the boss. The fewest fights to reach the boss are 3 on `act1` (when its optional paths open), 7 on `act1B` and 6 on `act1C`. Moved to the balance pass (BACKLOG 5).
- **Boss-level adds' damage:** `BossWaveHandler` spawns adds with `WaveNumber = 1`, so they don't grow within the fight. Fine for the demo; chunk 9 should decide it for later bosses.
- **Play check for the Echo:** `DespawnThroughPortal` from a hidden state needs a look in play once built.
- **Boss-only damage scaling:** `GetAbilityDamage` rounds to an int and ignores the expected player. Harmless at level 8, wrong at level 16 (chunk 9).
- **Effect on GDD §2:** a 1.5-hour full clear sits against "short runs". The goal may need rewording to "a Pilgrimage in short stages".

### Changes for the Orchestrator
**`game-design-docs/TenPageGDD.md`:**
- **§3, line starting "Each run is a pilgrimage through an act":** change to say a Pilgrimage runs through all four acts in a row, and each act is a branching map.
- **§4 flow diagram:** replace "next act? (open question 14)" with: boss killed, boss reward, cutscenes, next act's map; after the last act, the ending.
- **§7, "Each act adds one new mechanic on top of the previous ones":** change to "Each act adds its own mechanic." Add that Pools of Corruption carry into Act IV, while Frozen and Frozen Enemies stay in Act II (Frost can reappear only through an Act II enemy, such as one the Awoken Summoner summons).
- **§7 table, Act II row:** boss status becomes "Designed". 
- **§10 Act II, "Frozen":** remove "_(optional)_" and add section 2's rules (a third stack freezes, Dash with a charge breaks the ice, immunity afterward).
- **§10 Act II, "Frozen Enemies":** replace the open note with section 2's answer: optional, guaranteed item pickup, no reward at the wave's end.
- **§10 Act II enemies:** add each enemy's tiers only if the team wants them there. Behavior text already matches, apart from the Death Witch's beam, which cover now blocks.
- **§10 Bosses, Act II:** replace "Mechanics **TBD**" with section 4's lore, phases and animations.
- **§10 Bosses, proposal paragraph:** Acts III and IV keep "an invulnerable phase while the Altar of Malien fills", but note that each boss may fill it differently.
- **§13:** Q1, mark the Ice Bull as answered. Q3, Q5 and Q14 answered: remove or mark them, pointing to this spec. Leave Q6 (Crawler and Cursed Sword values) alone: it's Act III and IV work.
- **§5 and "Glossary" table:** the Pilgrimage's definition already fits, so no change.

**`game-design-docs/Lore.md`:**
- **§3 peoples, under the Plerians or the enemies list:** add: "**The Ice Bull:** one of the first Plerians the Drekos corrupted. He was defeated in the Dark Age, but Malizante saved him and buried him in the frozen north, transforming him. Locals who saw his horns took him for a bull, and the name stuck."
- **§4 table, Act II row:** "The Ice Bull (TBD)" becomes "The Ice Bull".
- **Glossary, new rows:** Ice Bull (Act II boss; a corrupted Plerian named by locals for his horns), Frozen Slime, Ice Sprite, Ice Golem, Frost Ghoul, Death Witch. The Spanish column is for the owner.
- **Act II flavor lines (section 3):** these add small facts: Winter Shamans raised the Ice Golems to guard the passes, and the sprites were spirits of the northern snow. Add them if the owner keeps them.

**Lore questions for the owner** (none block the build):
1. **Why is the Bull at the Altar?** Burial came in the Dark Age and the Altars came later, in the Chaos Age. Proposal: the clans built their Altar over the place he lay, unknowingly, and the Flame held him down as it held the Drekos. When it died, he woke. That needs the owner's OK before it enters `Lore.md`.
2. **Is the Bull's origin known to anyone?** This spec treats it as true but little known, so it isn't marked _(secret)_, and the player-facing line only hints at it. If nobody in the world knows, mark it _(secret)_; the line needs no change.
3. **Death Witches:** the draft line makes them former Faraclarian scouts, which hints at the **Crystal Maidens**. Should it say so, or be another origin? The line needs the owner's OK either way.
4. **Who beat the Bull in the Dark Age?** The Golden Riders would fit, since they broke the Rising Darkness, but it's left open.
5. **How did Malizante save him?** (Added in the Orchestrator's review.) `Lore.md` §2 has Gyra driving her into the sea in the Dark Age, so either she saved him before that, or the Bull was beaten early in the age. It needs a line before his story enters any game text.

## Acceptance criteria
- [x] The owner has chosen the run structure, and its effects are listed.
- [x] Frozen, Frozen Enemies, Q3 and Q5 are decided, or cut.
- [x] Each Act II enemy has a complete entry, with its new behavior pieces named.
- [x] The Ice Bull can be built from its section without asking the owner about its design.
- [x] Every name follows `Lore.md`'s Glossary, and no _(secret)_ leaks into player-facing text.
- [x] The art brief and the build order are finished.
- [x] Only this file changed.

## Files likely touched
- Docs: this file only.
- Code, prefabs, scenes, assets, CSVs, sheet tabs: none.

## Tests
None: this session changes no code. Its build order should name the guard tests each chunk needs.

## Handoff
_Written by the design session when it finishes. Nobody else edits this section._

- **Done:** every design section. Six owner decisions are recorded in "Open questions": run structure A (all four acts in one Pilgrimage), Frozen with the Dash break, optional Frozen Enemies with a guaranteed item pickup, Act II only for both mechanics, all six enemies kept, and the Ice Bull (bait charge, Frozen Axes, Deep Freeze, the owner's Plerian lore).
- **Not done / follow-ups:** the GDD and `Lore.md` edits are listed in "Changes for the Orchestrator", not applied. Four lore questions there wait for the owner, and none blocks the build. "Follow-ups" lists five items, the first a possible demo issue with short routes. Final numbers belong to the designer.
- **Commits / branch:** uncommitted on `develop`, by instruction. Only `docs/features/act-ii-design.md` changed in this session. `docs/features/opening-cutscene.md` also shows as modified, by the cutscene session.
- **Gotchas for the next session:**
  - **Deep Freeze's interval:** it gets shorter over the phase, so the ability comes more often (confirmed by the owner, 2026-11-01).
  - **Frozen Axes:** the bounce rule, what the axes bounce on beyond enemies and the player, and the flame rule are this session's reading of the owner's idea. `ProjectileMoveBounce` can't be used as it stands (see the section 4 table).
  - **Echo of Malizante** must stay one GameObject, or `CorruptorPhaseController` breaks.
  - **The boss flow (chunk 2) touches the demo's ending,** so keep it on a branch until December's build is out.
  - **`DataHygieneTests`** forces the content chunks onto one branch.
- **Manual tests added to todo.md:** none
