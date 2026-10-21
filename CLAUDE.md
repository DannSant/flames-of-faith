# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

FlamesOfFaith is a 2D action roguelite built in Unity 6 (`6000.2.10f1`, URP). Runs (attempts) proceed through an act-based overworld map, into isometric combat levels with waves of enemies, elemental debuffs, boss fights, and between-run meta-progression / relic ("Effect") systems.

## Working with this repo

- **Git: always work and commit directly on `main`** unless the user says otherwise for a specific task. Don't create feature branches on your own; this overrides any default "branch first when on the default branch" behavior.

This is a Unity project — there is no CLI build/lint/test pipeline (no `package.json`, no CI config). Development happens in the Unity Editor:

- Open the project in Unity Editor **6000.2.10f1** (must match `ProjectSettings/ProjectVersion.txt` — Unity will offer to auto-upgrade if a different version is installed; don't let it silently change the version).
- There is no headless build script or Makefile in the repo. Compilation errors surface in the Editor Console; there is no `dotnet build` step that mirrors Unity's compile (the generated `.csproj`/`.sln` files are for IDE intellisense only, not authoritative builds).
- **Tests.** The only automated tests are the EditMode tests in `Assets/Scripts/Tests/Editor/`. They come in two kinds:
  - **Pure tests** of the math (static functions with their tunables passed in): enemy damage, stats and stat bonuses, Pierce, projectile steering, enemy obstacle steering, wave progress, Favors, skill tree rules, ability tiers.
  - **Guard tests over the real assets:** enemy damage tuning, class kits and skill trees, ability descriptions, unique map ids, starting-only items never sold or rewarded.
  - They need no asmdef: the default `Assembly-CSharp-Editor` already references NUnit (test-framework 1.6.0), so any test under an `Editor/` folder is picked up.
  - Run them from Window → General → Test Runner → EditMode. With the Editor closed, use `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml`.
  - Gameplay itself (feel, timing, visuals) has no coverage, so verify it by running the game in the Editor (Bootstrapper scene).
  - `Assets/Scripts/Tests/LevelTreeDebugger.cs` is a debug helper, not a test.
- All first-party code compiles into the default `Assembly-CSharp` assembly — there are no custom `.asmdef` files under `Assets/Scripts/`, so any script can reference any other without assembly-reference wiring.
- Third-party/vendored code lives under `Assets/AssetPackages/` (ClassicPixelRPGUI, DamageNumbersPro, NavMeshPlus, WalldoffStudios Indicators) and should generally be left alone rather than modified in place.
- Key packages: URP 17.2.0, new Input System 1.14.2, Cinemachine 3.1.3, `com.unity.ai.navigation` (NavMesh) plus the vendored NavMeshPlus for 2D nav (both currently unused: enemies use local steering, see "Enemy obstacle avoidance"), Timeline, Visual Scripting.

## Architecture

### Namespace/folder convention

Code is organized by feature under `Assets/Scripts/<Area>/`, each area mapped to a `Game.<Area>` namespace (e.g. `Game.Combat`, `Game.Overworld`, `Game.Scene`). Editor-only tooling lives in nested `Editor/` folders (Unity auto-excludes these from player builds regardless of nesting depth), e.g. `Scripts/Overworld/Editor/`, `Scripts/Editor/`.

### Cross-cutting patterns

- **Singleton services**: `Game.Common.Singleton<T>` (`Assets/Scripts/Common/Singleton.cs`) is the base for most manager-style MonoBehaviours (`GameSession`, `MainSceneController`, `PlayerManager`, `MapRunController`, `AudioManager`, `WaveSpawner`, etc.). Always null-check `X.Instance` before use — many systems check `WaveSpawner.Instance != null` etc. because singletons may not exist yet depending on scene.
- **State-loader interfaces** (`Assets/Scripts/Common/`): `IPrimaryStateLoader`, `IDependentStateLoader` (`LoadState`/`SaveState`/`ResetState`), and `Utils/IInitializeAfterStateReady.cs` (`InitializeAfterStateReady`) define an ordered initialization contract — primary state (progression, currency, health) loads first, dependent components load/initialize after. Follow this contract rather than initializing player-related state directly in `Awake`/`Start` for anything that needs saved data.
- **`ISceneCleanupHandler`**: contract for objects that need explicit teardown between scene loads (used heavily by run-scoped singletons like `WaveSpawner`, `CameraController`, `CurrencyGenerator`).
- **`IMapComponentDisabler`**: contract for player/enemy components that must be disabled while the player is on the overworld map scene (e.g. `PlayerController.DisableComponentsOnMap`).
- **Data-driven ScriptableObjects**: most content (weapons, enemies, boss abilities, effects/relics, elemental debuffs, wave compositions, map/level definitions) is authored as ScriptableObject assets under `Assets/Resources/`, loaded at runtime via `Resources.Load` through a `*DatabaseProvider` singleton (e.g. `EnemyDatabaseProvider`, `EffectsDatabaseProvider`). When adding new content types, prefer this ScriptableObject + database-provider pattern over hardcoding.
- **Behavior composition over inheritance**: enemy AI (`Scripts/AI/Behaviors/`) and boss abilities (`Scripts/Boss/Abilities/` + `Conditions/`) are built from small, pluggable ScriptableObject "behavior" pieces attached/driven by a runtime controller (`BehaviorController`, `BossController`), rather than one monolithic script per enemy/boss. Follow this pattern when adding new enemy or boss behavior rather than subclassing a base AI class.
- **UI stays decoupled from game logic**: scripts under `Scripts/UI/` should never call `FindAnyObjectByType<T>()` (or hold a discovered reference) to reach into a gameplay class for data or to trigger a calculation — they should only receive plain data and delegates that gameplay pushes to them, the same way `items` already flowed from `ShopKeeper` into `ShopControllerUI`. When a UI layer needs something new from gameplay (a computed price, a reference to act on later), extend the *existing* event/callback the gameplay class already fires rather than adding a lookup on the UI side. Concrete precedent: `ShopKeeper.onShopWindowToggle` is `Action<bool, List<Effect>, ShopKeeper>` — it pushes the `ShopKeeper` reference itself alongside the items, so `ShopControllerUI` never searches the scene for it, and `ShopKeeper.GetDiscountedPrice(effect)` (not the UI) is the one place price gets computed, so display and the actual charge can never drift. `ShopIconUI`/`ItemBagControllerUI` go further and never reference a gameplay class type at all — they only take primitives (`Effect`, `int`, `Action`s) in their `Setup`/`Select` methods. The exception is a one-time `FindAnyObjectByType` purely to attach/detach an event subscription (e.g. `ShopControllerUI.SubscribeToEvents`) — that's wiring, not a data query, and isn't what this rule is about. **Known, accepted exception:** `StatsPaneUI` still reads `PlayerProgression` and `EffectStore` directly (it predates the rule, and pushing every stat change through events was judged too much wiring at the time). Leave it as is unless a refactor is scheduled; don't copy the pattern.

### Enemy obstacle avoidance (Oct 2026)

Levels have only a few small, convex obstacles (tree trunks on the `Obstacle` layer, there to block enemy projectiles), so enemies don't use a NavMesh. They use local **context steering**, in `Scripts/AI/Steering/ObstacleSteering`. The vendored NavMeshPlus is unused.
- **How it works:** a movement behavior passes its wanted direction to `ObstacleSteering.SteerAroundObstacles(context, rb, desired, expectedSpeed, dt)`. It scores the wanted direction and a ring of candidates as `dot(dir, wanted) − dangerWeight × danger(dir)`. Danger comes from `Obstacle` colliders within `lookAhead` of the enemy's collider.
- **Side commitment:** the enemy keeps the side it chose for `sideCommitTime`, so it doesn't flip-flop.
- **Stuck recovery:** if over a `stuckCheckWindow` it covers less than `stuckSpeedRatio` of the expected distance next to an obstacle, it switches sides.
- **Wired into:** `AIBehaviorChaserMovement` (chasers and the Corruptor's flee), `AIBehaviorMoveToSnapshot`, `AIBehaviorMoveToObject` and `AIBehaviorChaseOnAnimIntervals` (per hop, no stuck check). Each has an `avoidObstacles` toggle. Roamers already bounce off collisions.
- **`context.moveDirection` stays the wanted direction,** because it drives facing. Only the applied velocity is steered.
- **Tunables:** `ObstacleAvoidanceSettings` (`Resources/AI/ObstacleAvoidanceSettings.asset`), including `debugDraw` (white = wanted, cyan = steered) and a master switch.
- **New movement behaviors** should route their final direction through it. The math is pure and covered by `ObstacleSteeringTests`.

### Two parallel map systems — know which one you're touching

- **`Scripts/Map/`** (legacy, `MapGenerator` explicitly marked `[Obsolete]`) — layer-based procedural level-select map, rendered via uGUI with bezier connectors (`UIBezierConnector`). Being phased out; avoid extending it.
- **`Scripts/Overworld/`** (active development) — graph-based map system:
  - Data model: `MapDefinition` (ScriptableObject, asset under `Assets/Resources/Overworld/`) → `NodeDefinition` → `ConnectionDefinition` (directional edges with RNG-gated optional paths for run variety).
  - Runtime: `OverworldMapGenerator` seeds a `RunMapState`/`RunMapGraph` (`RunNode`/`RunEdge`) from definitions; `MapRunController` (singleton) owns traversal, fog-of-war reveal, and act progression, and fires events the renderer subscribes to.
  - Rendering: `Scripts/Overworld/Render/` (`OverworldMapRenderer`, `OverworldNodeView`, `OverworldEdgeView`) draws the live graph in-scene.
  - Authoring tool: `Scripts/Overworld/Editor/` — a custom `EditorWindow` (`MapEditorWindow`, menu `Tools/Flames of Faith/Map Editor`) built on UI Toolkit's `GraphView` (`MapGraphView`, `MapNodeView`, sizing constants in `MapEditorConstants`). Editor-authored node positions (`NodeDefinition.editorPosition`) are baked into normalized runtime `worldPosition` via `MapEditorWindow.BakeEditorPositionsToWorldPositions()` (divides by `MapEditorConstants.NodeSpacing`, inverts Y). Toolbar actions `AddNewNode`, `DeleteSelected` and `ValidateMap` are implemented (with Undo).
  - Gamepad/keyboard: `OverworldGamepadNavigator` moves between nodes with the UI Navigate action and enters a level with Submit. It ignores input while any `UIWindow` has gamepad focus (e.g. the map's Exit button, reached with Browse/Y). The map's HUD (act / level / coins window, prompts, the Stats pane opened by `OverworldPlayerStatePresenter`) is under "Map HUD" in UI standards.
  - Generation is deterministic from the seed and the `MapDefinition`s, except which map an act uses (`SetCurrentMapGraph` picks randomly among maps with the same `actNumber`), which is why saves store the `mapId`.
  - **Developer map cheats:**
    - **Where:** the Ctrl+Shift+D window, whose prefab is also in `LevelSelector.unity` because UIMain is unloaded on the map, and `DevTools/Map`.
    - **What:** reveal the full map, free travel to any revealed node, beat the current node (a real clear: reveals its children, a boss advances the act, it counts toward `levelsBeaten` only for fights), and go to act N.
    - **Code:** `MapRunController.Debug*` / `FreeTravel`, with the pure rules in `MapTravelRules` (`MapTravelRulesTests`). They fire `OnMapStateChanged` so the renderer redraws, and they autosave.
    - **Window buttons:** the map buttons are cloned from the Skip button at runtime. The level-only buttons (skip, add item) are disabled on the map.
  - **Every map needs a unique `mapId`** (`MapDefinitionTests`). Continue looks the map up by it, so a copied map that keeps its source's id restores the wrong map, and the save is deleted as "doesn't match this build".

When touching map/level-select logic, confirm whether you're in the legacy `Map/` system or the new `Overworld/` system — they don't share code paths.

### Scene bootstrapping flow

`Bootstraper.cs` additively loads gameplay + UI scenes at startup → `MainSceneController` (singleton) generates the overworld run from act `MapDefinition`s and handles loading-screen transitions into gameplay levels via `LoadGameplay(levelData)` → `GameSession` (singleton) tracks the current run's selected character/difficulty/level and holds a `PlayerData` save-state container → `PlayerManager` (singleton) spawns and tracks the active player GameObject, exposing components via `GetPlayerComponent<T>()`.

- **The player is respawned on every scene load** (`PlayerManager.SpawnSelectedPlayer`). State survives only through `GameSession.PlayerData`: components write to it in `SaveState()` and read it back in `LoadState()`.
- **Player state is saved only when a level is finished.** Every level except the boss ends through the same path. That includes shop, campfire, treasure and event encounters (`Scripts/RunEncounters/`), which are their own scenes. The path is `WaveSpawner.GoToNextLevel` → `PlayerManager.HandleWaveGroupFinished`, which calls `MapRunController.OnLevelCleared`, `SaveAllPlayerComponentStates` and `LoadLevelSelectorScene(false)`. So while inside a level, `PlayerData` still holds the pre-level snapshot.
- **`GameSession.IsNewRun`** decides whether entering a level resets the player components (new run) or loads them from `PlayerData`.
- Meta-progression is separate from the run and persisted by `MetaProgressionManager` to `persistentDataPath/<player>_meta.dat` (through `SaveFileIO`). It holds unlocked effects (through `IMetaProgressionStateLoader`), Favors, class unlocks and skill tree tiers; see "Class kits, skill trees & Favors".

### Run encounter events

The data lives under `Resources/RunEncounters/EventEncounter/`. An `EventEncounterData` (title, text, options) lists `EventOptionsList` assets (a label plus actions). The actions (`OptionActions/`) are `Change{Currency,Grace,Health,Favors}Option` and `GiveRandomItemOptionAction`.
- **Random pool:** `encountersList` on `Prefabs/RunEncounters/EventEncounter.prefab` holds the Arcane Box, Bandits, Cursed Chest, Faraclarian Clan and House of Stars. The Holy Altar has its own map node and scene, which overrides the list.
- **No Ignore options** (reworked Oct 2026): every event has 2–3 options, and each one costs and gives something.
- **Action names:** an action is named `Lose…` or `Gain…`, and it must lose or gain accordingly. `EventEncounterTests` checks this; the lever's "Lose1To5Grace" used to give +3.
- **Rolls:** ranged rolls (`StatBiasedRoll`) lean toward the range's high end with Luck. For losses, that means losing less.
- **Art:** `EventEncounterData.art` (optional sprite). When set, `EventEncounterUI` shows it in a framed panel to the left of the event window; no event has art yet. The window itself uses the standard frame (see "Run encounter windows" under UI standards).
- **Treasure** (`TreasureEncounterController`, `Act1/Treasure.unity`): walking into the chest rolls one reward. An item goes to the player's `ItemBag` and the encounter ends, so the item bag window opens (take or sell), like event items. Currency and experience are given at once and shown in the Results window (`OnTreasureResolved`). There is no treasure window.

### Combat/effect pipeline

Damage flows through `IDamageable`/`DamageRequest` → `DamageCalculator` (combines weapon class, active `Effect`/`EffectBehavior` modifiers via `IEffectMultiplier`, and player progression stats) → applied to `PlayerHealth`/`EnemyHealth`. Projectiles separate "what it is" (`ProjectileBase`, damage/effect) from "how it moves" (`ProjectileMovementBase` subclasses: linear/arc/homing/bounce/delayed-homing) — extend by composing a movement strategy rather than a new projectile subclass per behavior.

- **`DamageSourceBase` hit modes:**
  - **Trigger enter:** one hit on contact.
  - **Trigger stay** (e.g. the Augur's flame trail): each target has its own timer, is hit on contact, then every `tickInterval` while it stays inside. One shared timer used to need 0.4 s of contact, so fleeing enemies such as the Corruptor never got hit.
- **Explosions:** `ExplosionDamage.SetExplosionDamage` lets a spawner set the damage of the explosion it sets off; without it the explosion prefab's own damage is used.

### Corruption & Grace (reworked Oct 2026)

The original spec is `Assets/Docs/corruption-rework.md`, but several decisions changed during implementation; this section is the source of truth. Every tunable lives in **`CorruptionSettings`** (`Assets/Resources/Combat/CorruptionSettings.asset`, loaded via `CorruptionSettings.Instance`) or on Inspector fields — never hardcode these numbers.

- **Grace** (`PlayerGrace`) is a single signed value clamped to `minGrace..maxGrace` (−50..+50), saved per run. It changes only through `SetGrace`, which fires `onGraceChanged`, `OnCorruptedStateChanged` (only on flips) and `OnGraceStatusChanged`. Damage multiplier = `max(minDamageMultiplier, 1 + damagePerGracePoint × Grace)`, applied in `DamageCalculator`.
- **Corrupted = Grace < 0.** `CorruptedLevel = max(0, −Grace)` reduces every stat flagged `affectedByCorruption` in `StatDatabase.asset` (Armor, HealingReceived) by `CorruptedLevel × corruptionReduceFactor`, through `PlayerProgression.GetCorruptionPenalty` (also used by the tooltip). Healing is reduced by a flat amount per heal, not zeroed. Corrupted players, enemies and the boss are marked by `CorruptedVisual` (see Corrupted outline below).
- **Corruption** (`PlayerCorruption`) is **per wave only**, never saved, and reset when a wave starts. It comes from two sources:
  - **Corrupted Damage:** post-armor damage taken from anything tagged with `CorruptedDamageSource`, converted at +1 per `corruptedDamagePerCorruption`, capped at `maxCorruptionFromDamage`.
  - **The Corruptor:** see below.
- **Wave end** (`WaveEndSequenceController.ResolveWaveCorruption`): `Grace = Grace + Grace Affinity − Corruption`, then Corruption resets. Grace no longer drains per wave, and levels/events no longer add Corruption. The Cursed Chest and Holy Altar events change Grace instead.
- **Grace Affinity** (`StatType.GraceAffinity`, enum index 5, formerly MaxGrace/GracePerWave) adds Grace at every wave end and raises the Grace pickup drop chance. The Grace pickup is rolled separately from `PickupSpawner`'s weighted pool (`gracePickupBaseChance + affinity × perPoint`, capped).
- **Corrupted enemies:**
  - **Spawn chance**, rolled in `EnemySpawnCoordinator`: `(LevelData.taintLevel × chancePerTaint + CorruptedLevel × chancePerNegativeGrace) × WaveData.corruptedChanceMultiplier`, capped.
  - **The flag** lives on `BehaviorContext.isCorrupted`. `AIBehavior.GetDamageAmount`/`GetRangedDamageAmount` apply the damage multiplier, so every attack path is covered in one place. Projectiles and explosions they spawn are tagged with `CorruptedDamageSource`.
  - **Extra health:** global `corruptedHealthBonus`, overridable per `EnemyData`. `EnemyData.alwaysCorrupted` is set for Corruptors.
- **Taint Level** (`LevelData.taintLevel`) is shown on overworld map nodes with a tooltip.
- **Corruptor phase** (`CorruptorPhaseController`, driven by `WaveSpawner.EndCurrentWave`), running between the wave timer and the end sequence:
  1. Spawning stops, but living enemies stay.
  2. The level's Corruptor spawns (`WaveDatabase.corruptorType`, overridable per `WaveData`; one `EnemyType.CorruptorAct<N>` per act).
  3. The camera focuses on it under a `GameplayFreeze`.
  4. The timer starts. The Corruptor's Corruption grows with its lifetime (base value for a fast kill, then +1 per interval, capped). If it isn't killed in time it escapes through a portal and grants the escape value.
- **Phase cancellation:** the phase is cancelled immediately on player death via `PlayerHealth.IsDead()`. Don't rely on `onDeath` here: it fires only after the death animation.
- **Boss:** `BossController.isCorrupted` (default on). Ability damage goes through `BossController.GetAbilityDamage`. Boss damage is *not* converted into Corruption, because boss levels have no wave end. `BossWaveHandler` is unchanged.
- **UI** (all fed by gameplay events, per the UI rule above):
  - `GraceBar`: signed bar, pending wave Corruption label, tooltips. The wave summary can take over what it shows (`BeginPresentation` / `PresentGrace` / `PresentPendingCorruption` / `EndPresentation`, display only).
  - Corruptor bar: `CorruptorPhaseBarPresenter` attaches a world-space `CorruptorPhaseBarUI` to the player.
  - Off-screen arrow: `TargetIndicatorUI`.
  - Wave hints: `WaveHintUI`, toggled by `SettingsManager.ShowTutorialHints`.
  - End-of-wave summary (`WaveCorruptionSummaryUI`), a receipt of what happened to Grace:
    - **Rows:** each Corruption source is `-N` in purple, then the total, then Grace Affinity `+N` in cyan. Then the formula, which counts over `countDuration` (2 s) on a Grace bar in the window, then Favors in gold.
    - **The HUD:** Grace is already applied when the summary opens, so it takes over the HUD `GraceBar`, which shows the old values again. With `flyingNumbers`:
      1. The Corruption total flies from the HUD label into its row, and the label drops to 0.
      2. Grace flies into the formula.
      3. The result flies back, and the HUD bar moves when it lands (`UIFlight` path).
    - Without `flyingNumbers`, the HUD follows the count instead.
    - **Button:** it reads Skip while playing (jumps to the end) and Continue after (closes). It's the UIWindow default.
    - **Waiting:** the end sequence waits on its callback; `waveSummaryMaxWait` 0 means no limit.
  - Corruption numbers: `NumberCorruptionGainedVFX`.

**Corrupted outline** (Oct 2026): many VFX on screen got noisy, so Corrupted things get a pulsing purple outline instead.
- **Mode:** `CorruptionSettings.corruptedVisualMode` is Outline (default), Vfx or Both. The material, color, thickness, pulse and draw order are on the same asset.
- **How it's drawn:** `SpriteOutlineRenderer` (`Game.Common`, reusable) adds a child SpriteRenderer that copies the body sprite (sprite, flip through scale, sorting + offset, visibility, alpha). It draws that sprite with `Shaders/CorruptionOutline.shader` (unlit, ring only, values in a MaterialPropertyBlock). The body keeps its own 2D-lit material.
- **Which sprite:** the one on the root, else the `CharacterVisual` one, else the highest sorting order (`CorruptedVisual.FindMainSpriteRenderer`). On the map, `SetAnchor` rebuilds it on the marker's sprite.
- **Sprite margin:** the ring is drawn outside the silhouette, so the sprite mesh needs that much empty margin. A 1 texel ring fits Tight meshes with the default Extrude Edges of 1.
- **Tests:** `CorruptionOutlineTests`.

Reusable pieces that came out of this work:
- **`GameplayFreeze`** (`Game.Common`): a semi-pause (time keeps running). Enemy behaviors with `stopDuringGameplayFreeze` skip ticks and animation-event starts, enemy projectiles stop, and player input and damage are blocked.
- **`CameraFocusController`:** pan/zoom to a target and restore.
- **`TooltipTriggerUI`:** a hover/select tooltip on `GeneralTooltipPaneUI`.
- **`UIWindowOpenWhilePaused`:** an always-visible HUD panel that joins gamepad focus only while paused.

### Experience & XP tokens (reworked Oct 2026)

XP needed per level is a fixed curve, `PlayerExperience.GetXPRequired` = `10 × 1.2^(level−1)`. Drops are sized so that the base tokens give about `levelsPerWaveTarget` levels per wave at any level. All tunables live in **`ExperienceSettings`** (`Assets/Resources/Progression/ExperienceSettings.asset`, loaded via `ExperienceSettings.Instance`). XP values are `float`.

- **Per-wave base XP** is computed once at wave start (`WaveSpawner.CalculateWaveExperience` → `WaveExperienceCalculator.CalculateBaseXp`) from the player's level at that moment:
  - `expectedSpawns = waveDuration / lerp(regularCooldown, longCooldown, cooldownBlend)`, or `WaveData.expectedEnemyCountOverride` when it's > 0. The blend exists because `EnemySpawnCoordinator` uses the long cooldown while few enemies are alive, so fast killers get fewer spawns.
  - `expectedDrops = expectedSpawns × dropChance × expectedKillRatio`.
  - `targetXp = GetXPRequired(level) × levelsPerWaveTarget × (1 + ExperienceToLevelUpReduction points × xpBonusPerReductionPoint)`. The Experience Reduction stat is a flat XP bonus here and does **not** change the curve. Bending the curve made its effect grow exponentially with level.
  - `baseXp = targetXp / expectedDrops`, exposed as `WaveSpawner.CurrentWaveBaseXp`.
- **Denominations:** `DropExperienceOnDeathBehavior` rolls `dropChance`. The token starts at tier 1, then for each tier up to `EnemyData.xpTier` it rolls that tier's `promoteChance` and stops at the first failure. The token is worth `baseXp × multiplier` and uses the tier's sprite (`ExperienceToken.Setup`). Enhanced tokens are bonus XP on top of the target, which is deliberate.
- `EnemyData.xpBase`/`xpPerLevel` are gone. `xpTier` (default 1) decides which denominations an enemy can drop.
- Enemies still alive at wave end are killed by the end sequence and drop tokens too.
- **Last wave of a level** (`WaveSpawner.IsLastWaveOfLevel`, not `WaveDatabase.lastWave`, which marks the game's last level): `WaveEndSequenceController.CollectAllExperienceRoutine` pulls every token to the player (`MagnetToPlayer.ForceAttract`). Tokens still out after `xpCollectMaxDuration` are granted instantly, and one more instant sweep runs after the summary closes. On other waves, tokens stay into the next wave.
- **End sequence order:** cleanse, kill the remaining enemies, collect the gems (last wave only), then resolve Corruption and open the summary. The summary opens last so that closing it quickly can't end the level before the gems are collected.
- `logWaveXpDebug` logs expected vs actual spawns, drops and XP for every wave, after the end sequence.

### Enemy damage (reworked Oct 2026)

Enemy → player damage is sized against an *expected* player, never the real one. That way a player with above-expected health or armor really does outscale enemies. Player → enemy damage and enemy health are unchanged.

- **Tunables** live in **`EnemyDamageSettings`** (`Assets/Resources/Combat/EnemyDamageSettings.asset`, loaded via `EnemyDamageSettings.Instance`):
  - `expectedCurve`: expected max health and armor by levels beaten, interpolated between rows.
  - `waveProgressStep`: how much each wave inside a level counts as progress.
  - `tierHitsToKill`: hits per tier.
  - The armor constants.
- **Each `EnemyData`** has `contactDamageTier` and `projectileDamageTier`. Projectile covers projectiles and explosions. Fractional tiers interpolate; 0 means no damage. Tier N kills a player with the expected stats in `tierHitsToKill[N-1]` hits.
- **`EnemyDamageCalculator`** (pure and static, no `GameSession`) holds the formula: `raw = expectedHP / hits / armorMultiplier(expectedArmor)`. `AIBehavior.GetDamageAmount`/`GetRangedDamageAmount` call it and then apply the corrupted multiplier.
- **Damage is fractional end to end.**
  - `EnemyDamage`/`EnemyTriggerDamage` take a `float`.
  - `PlayerHealth.ApplyArmor` no longer rounds.
  - The damage-to-player number is rounded for display only (minimum 1), in `DamageNumberSpawner.SpawnDamageToPlayerNumber`.
  - `HealthBar` rounds health up for display.
- **Armor math** lives in **`ArmorMitigation`** and is shared by `PlayerHealth` and the calculator. Negative armor increases damage taken, mirroring the positive curve: −5 = +17%, −25 = +50%, never +100%. Corruption lowers armor, so it can push the player below 0. Its constants moved from `PlayerHealth`'s Inspector to `EnemyDamageSettings`.
- **Tuning:**
  - `Tools/Flames of Faith/Enemy Damage Preview` shows raw damage and hits-to-die per tier or per enemy at every point of the run. It can compare against a custom player's health and armor.
  - `EnemyDamageTuningTests` guard the real asset: the curve is sorted and never gets weaker, tiers are strictly stronger, and every enemy attack takes 1–60 hits.
  - `EnemyDamageCalculatorTests` cover the formula with their own settings.
- Boss abilities still use their own damage (`BossController.GetAbilityDamage`) and aren't part of this system.

### Adding a player stat

- **Append to `StatType`, never insert.** Effect assets, the effects DB (`statModifiersJson`), the Emblem level-up behaviors (`MainStatOnLevelUpBehavior.stat`) and event `biasStat` store stats by **index**. Run saves store them by **name**, so a rename needs an alias in the load path or saves get deleted.
- **Add an entry to `Resources/Progression/StatDatabase.asset`.** A stat without one stays at 0, items included. `EvasionAndStatTests` guards this.
- **Add a display name** in `StatDisplayNameHelper`.
- **Put the formula in `StatsCalculations`** as a pure function that takes its tunables as parameters. The settings live on an SO; for example, Evasion reads `EnemyDamageSettings.GetEvasionChance`.
- **Old saves:** `PlayerProgression.LoadState` fills stats missing from a save with their initial value (`StatBackfill`).
- **Add a benefit line** in `StatBenefitDescriber`, or list the stat in `StatBenefitTests.StatsWithoutComputedBenefit`; a guard test fails otherwise.
  - The tooltip shows the static description from `StatDatabase` plus that computed line, e.g. "Take 28.6% less damage".
  - Keep numbers out of the static text, so it can't drift from the formulas.

**Stat descriptions:** `StatBenefitDescriber.Describe(stat, points, StatBenefitContext)` is pure and uses the same formulas as gameplay.
- The context is built on the gameplay side by `PlayerProgression.BuildStatBenefitContext` from the settings SOs and player components (weapon data, dash, movement, `RetributionHandler.Curve`).
- `StatsPaneUI` shows the line in the tooltip's second panel (`GeneralTooltipPaneUI` sources section). While Shift is held, that panel shows the items granting the stat instead.
- Level-up choices show "Now / After": `UpgradeManager.OnUpgradeOptionsAvailable` pushes a preview function along with the choices, per the UI rule.
  - "After" comes from `PlayerProgression.PreviewFinalStats`, which runs the same pure math as the real stats (`FinalStatsCalculator`: flat and % item bonuses, corruption, then stat bonus rules). It can't disagree with the result.
  - An "Also:" line lists other stats the pick changes through stat bonus rules (e.g. the Archer Emblem: Evasion also adds Pierce).
- **Formerly hardcoded numbers live on `PlayerStatSettings`** (`Resources/Progression/PlayerStatSettings.asset`): Health Regen, Life Steal, dash recovery scale, shop discount caps, pickup range, and the level-up tier weights for Luck.
- **Negative speed stats:** attack and dash speed divide by at least 0.1 (`StatsCalculations.SafeSpeedDivisor`). Large negative Attack Speed used to wrap around into near-instant attacks; it now slows them down by at most 10×.
- **Evasion** (index 22) is rolled in `PlayerHealth.TakeDamage` before armor.
  - Non-lethal event damage can't be dodged.
  - A dodge returns before `onDamageTaken`, so it adds no Corruption and triggers no Retribution or on-damage effects.
  - It grants `dodgeInvulnerabilityDuration` and shows `DamageNumberSpawner.SpawnDodgeNumber`.

**Class stat guide** (Oct 2026), so new players know what to pick:
- **Data:** `CharacterClassData.statPriorities`, a list of `{stat, role (Key / Complementary), justification}`. Each class has one Key stat.
- **Rules:** the pure `ClassStatGuide` (`Scripts/Progression/`) handles role lookup, display order and tab, and the tooltip line ("Key stat for the Warrior: ...").
- **Stats pane:** `StatsPaneUI.ArrangeRows` puts the Key stat and then the Complementary ones on top of the **Primary** tab, even when their category is Secondary (Pierce, Skill Duration). It re-sorts on every show, because the pane outlives a run.
- **Stars:** a gold (Key) or silver (Complementary) star (`StatRoleStar`, sprite `Sprites/UI/StatRoleStar.png`, colors on the prefabs). The justification goes in the row's existing tooltip.
- **Upgrade choices:** the same star and tooltip line. `UpgradeManager.OnUpgradeOptionsAvailable` now pushes one `UpgradeChoices` object (groups, preview, first level, class stats, class name).
- **Tests:** `ClassStatGuideTests` guard each class's list.

### Pierce (reworked Oct 2026)

`StatType.Pierce` (index 11, formerly `PierceAmount`; old saves load through `StatTypeNames`). Tunables are on `PierceSettings` (`Resources/Combat/PierceSettings.asset`).
- **Extra targets:** `floor(points × 0.25)`, added to the weapon's own `pierceAmount`. Used by Bow primary/special/barrage, the Scepter special, and Strafe arrows (prefabs spawned without a weapon opt in with `DamageSourceBase.addPierceStat`). The Augur's basic fireball ignores the stat.
- **Split arrows:** `ProjectileSplitOnHit`, only on linear arrow prefabs (`ArrowProjectile`, `ArrowEffectProjectile`).
  - Each enemy hit has a 3%-per-point chance (capped at 40%) to clone the arrow at half damage, in a random direction.
  - The clone has a single hit, a capped travel distance, no steering, and ignores the enemy it split from (`DamageSourceBase.IgnoreTarget`). Split arrows never split again.

### Targeting & lock-on

`WeaponManager` owns the player's **marked target** (`CurrentTarget`, `OnTargetChanged`). This is separate from the weapon's per-shot target (`WeaponBase.currentTarget` / `GetCurrentTarget()`), which manual aim assist can set without marking anything. Player facing and the Sword orbit use the per-shot target.

- **Lock-on input:** the `LockTarget` action (F / gamepad LT; Left Shift is taken by `ShowStatSources` and the Ctrl+Shift+D cheat window), read by `PlayerController`. Its mode is `SettingsManager.LockOnToggleMode`, saved in PlayerPrefs and shown in `SettingsPanelController.lockOnToggleModeToggle`.
  - **Toggle mode (default):** each press calls `WeaponManager.ToggleLock`. It locks the current target, or the nearest one in `weapon range × lockOnRangeMultiplier`, and does nothing if there is none. Pressing again unlocks. The lock also ends when that enemy dies.
  - **Hold mode:** `SetLockHeld` every frame. While held, a dead target is replaced by the nearest one in lock range.
- **Rules**, applied every frame in `UpdateCurrentTarget`:
  1. **Locked:** the target never changes, even out of range.
  2. **Auto-attack on:** the nearest enemy in weapon range, or none.
  3. **Otherwise:** no target.
- **Attacks:** auto-attack, and manual attacks while locked, fire at the marked target only while it's in weapon range (`EnemyTargeting.IsWithinRange`).
- **Specials** read the marked target through `WeaponBase.GetMarkedTarget()`:
  - The Archer's barrage (`BowWeapon`, `BarrageArrow.prefab`, homing).
  - The Augur's timed explosions on the target (`ScepterWeapon`, `FireballExplotion.prefab`, damage initialized as a % of the special's damage).
  - All tunables are on those weapon components.
- **Marker:** `TargetMarkerUI` is a triangle on the enemy's world-space canvas, added once in `EnemyBase.prefab` (plus `BossAct1`). It shows **only on a locked target**, so the player can see the lock. `WeaponManager.RefreshLockMarker` shows and hides it directly; gameplay pushes to UI.

### Run save / load (single slot)

Code is in `Scripts/Saving/`. `RunSaveService` (static) writes `RunSaveData` to `persistentDataPath/run.sav`: plain JSON in the Editor, XOR-obfuscated in builds, written to a temp file and then swapped in.

- **When it writes:** autosave on every map arrival (end of `MainSceneController.LoadLevelSelectorSceneRoutine` and the Retry routine) and on every map move (`MapRunController.TryMoveTo`). It never writes from inside a level. The file always holds the pre-level snapshot, so Save & Exit, a crash or Alt-F4 inside a level all return the player to the map as it was before entering that level, standing on the uncleared node.
- **When it deletes:**
  - in `PlayerHealth.Die`, at the moment of death and not on `onDeath`, which fires after the animation;
  - on boss kill (`BossWaveHandler.NotifyBossDied`), on `WaveSpawner.OnAllLevelsFinished` and when the last act is cleared (`MapRunController.IsRunComplete`, which also blocks re-saving);
  - on New Game.
- **What it stores:**
  - session data: class, difficulty, `levelsBeaten`, the run's unbanked Favors (`runFavors`), and `runStarted = !IsNewRun`;
  - `PlayerData`: effects by `EffectID`, stats and node states by enum **name**;
  - the current act's map: seed, `mapId`, node states, edges and current node.
- **Mapping lives with the owners:** `GameSession.ToSaveData`/`ApplySaveData`/`CanApplySaveData`, and `MapRunController.CaptureSaveData`/`CanRestore`/`RestoreFromSave`.
- **Continue** (`MainSceneController.ContinueRun`):
  - It validates first, by regenerating the map from the saved seed and checking that the effects, stats, `mapId` and node ids exist. An invalid save is deleted.
  - It then restores through the normal non-new path (`LoadAllPlayerComponentStates`).
  - A save with `runStarted == false` restarts that run fresh with the same class and seed.
- **Seeds:** new runs and Retry roll a random seed. `MainSceneController.useFixedSeed` forces the Inspector seed for testing.
- Bump `RunSaveData.CurrentVersion` when the save format changes incompatibly. Mismatched versions are ignored.
- **When adding run state:** if it lives on a player component, saving it into `PlayerData` from `SaveState()` isn't enough. It also has to be added to `RunSaveData` and to the `GameSession` mapping, or it won't survive a restart.
- **UI:**
  - Main menu: Load Game is disabled and tinted when there's no save, and New Game warns before deleting the save.
  - Pause menu: `SaveAndExit` warns that the level's progress is lost.
  - Map: `ExitGame` saves, then goes to the main menu.
  - The warnings use the reusable `ConfirmDialogUI` (`Show(message, onConfirm, onCancel)`, or the OK-only `ShowInfo(message, onClose)`; prefab `Prefabs/UI/Misc/ConfirmDialogUI.prefab`).

### Gamepad & UI navigation

Gamepad support for uGUI is built on `Scripts/UI/Navigation/`:

- **`UIWindow`:** goes on the panel a controller shows or hides, and requires a CanvasGroup. Keep one per window and never nest them.
  - **Role:** Primary (can take focus automatically) or Secondary (reached only with LB/RB).
  - **Open Mode:** `GameObjectActive` for panels that are activated and deactivated. Use `Manual` for panels that stay active and slide off screen, whose controller must call `SetOpen`. The main-menu panels are Manual.
  - **Focus Mode:** `AutoFocus` takes focus on open; `OnDemand` waits for Browse/Y, as in the map's Exit button. (The shop is AutoFocus: it's modal.)
  - **Priority:** a newly opened window takes focus only if its priority is *strictly higher* than the focused window's.
  - **Default Selectable** and **OnCancel**.
- **`UIFocusManager`** (singleton, Bootstrapper scene):
  - It tracks open windows and moves focus. Unfocused windows get `CanvasGroup.interactable = false`, which shows their buttons in the Disabled state.
  - It cycles windows with LB/RB.
  - Cancel/B: a Secondary window returns focus to the previous window, an `OnDemand` window releases focus, and other windows invoke their OnCancel.
  - `logFocusChanges` logs every change.
- **`UIWindowOpenWhilePaused`:** lets an always-visible HUD panel join focus only while paused.
- **`UISelectionMarkerUI`:** the visible gamepad highlight, a 9-sliced bracket frame that wraps the selected element (see "UI standards" below). Each scene with windows needs one (MainMenu, UIMain, LevelSelector). Its own Canvas (override sorting 100) draws it above every window, wherever it sits in the hierarchy.
- **`InputDeviceManager`** (`Game.Control`): tracks keyboard/mouse vs gamepad (`IsGamepadActive`, `OnInputSchemeChanged`). With the mouse, nothing is focused and every window is interactable.
- **Common pitfalls** (all hit in practice):
  - **Selectables left on Navigation: None.** The gamepad can never reach them, and they're skipped when cycling windows.
  - **Selectables outside the window's hierarchy.**
  - **Manual panels set to `GameObjectActive`.** They count as permanently open and steal focus.
  - **The selection marker drawn under a window** (fixed: the marker now has its own sorting Canvas).
- **Player death** blocks only the Player action map (`PlayerInputHandler` gameplay block). The UI map stays on, so the death screen's Stats pane and Inventory still get Shift, LB/RB and gamepad navigation. `PauseMenuController` ignores Pause while dead.
- `PauseManager.SetPause` sets `timeScale` to 0. Upgrade selection, the item bag and the pause menu pause the game, but the wave-end sequence (including `WaveCorruptionSummaryUI`) does not. Anything time-based that must stop there checks `WaveSpawner.Instance.EndingWave` and/or `GameplayFreeze.IsActive`, as `HealthRegen` does.

### UI standards & revamp (Oct 2026, in progress)

The UI was redone window by window to one standard (`todo.md`, Priority 2); every window is done: the foundation, the gameplay HUD, the Inventory, the center windows (level-up, pause, death screen, Settings, item bag), the Stats pane, the shared tooltip, the confirm dialog, the end screen, the run encounter windows, the wave results, the shop, the skill tree's colors (its layout stays the builder's), the overworld map HUD and the main menu.

- **`UISettings`** (`Scripts/UI/Style/`, asset `Resources/UI/UISettings.asset`, `UISettings.Instance`) holds the standard. Never hardcode these values in UI scripts or prefabs:
  - **Fonts:** Small 12 and Big 20. Titles are Big and bold. `Display` (40) exists only for the agreed headline exception, YOU DIED. `font` is PIXEARG_ SDF; `outlinedFont` (PIXEARG_OUT) is for text over bars or straight over gameplay.
  - **Icons:** Small 50, Medium 80, Tiny 28 (input glyphs, the coin next to a number, stars in small buttons).
  - **Buttons:** Small (32 high, small font) and Big (48 high, big font). `UIStyleButton` also applies the 9-sliced button art and its sprite-swap states.
  - **Layout:** `margin` 15 (screen edge to window, frame to content), `spacing` 8.
  - **Colors:** Grace cyan, Corruption purple, Favors gold, text white, muted grey, and three UI roles. Rich text uses `UISettings.ToHex(role)`.
    - **Highlight (yellow):** neutral highlights such as PAUSE, section headers and previews.
    - **Positive (light green):** improved stats and gains not related to Grace.
    - **Negative (red):** negative stats, warnings, YOU DIED and auto-attack OFF.
  - **Art:** panel frame, bar frame, item slot frame, button and pressed button, neutral bar fill (tinted with the colors above), solid fill (cooldown sweeps), selection frame. These are 9-sliced copies in `Sprites/UI/Frames/`; the vendored packs stay untouched.
- **Canvases** (Bootstrapper, MainMenu, UIMain, LevelSelector) scale with the screen from 1920×1080, match 0.5. All sizes are canvas units at that resolution.
- **Style components** (`UIStyleText`, `UIStyleIcon`, `UIStyleButton`, `UIStyleFrame`, `UIStyleGrid`) mark an element's role (Small/Big text, Medium icon, Panel/Bar/Slot frame, a grid of Small cells...) and apply `UISettings` in `Awake`, so the asset stays the source of truth.
  - `UIStyleText` with color role None leaves the color to a script (e.g. `GraceBar`). It also turns off TMP vertex gradients, which tint whatever color a script sets. Its `bold` flag only touches the Bold bit.
  - `UIStyleGrid` sets a GridLayoutGroup's cells to an icon size and its spacing to `UISettings.Spacing`.
  - `UIStyleColor` applies only a color role, for texts in windows not restyled yet (`Tools/.../Apply Color Roles`). No window needs it since the shop's restyle.
  - Scripts that color text at runtime read `UISettings` too: `GraceBar`, `WaveUI`, `AutoAttackIndicatorUI`, `StatsPaneUI` (values and the selected tab), the skill tree (MAX, next tier, pips, the Favors counter in Favors gold; node costs (`SkillTreeNodeUI.GetCostColor`) and the details footer (`SkillTreeUI.IsFooterPositive`) green when affordable or available and red when not, like the shop's prices; and the class tabs like the Stats pane's: yellow when selected, muted while locked; `SkillTreeUI.GetTabLabelColor`) and the upgrade preview.
- **Tools** (`Tools/Flames of Faith/UI Revamp/`, code in `Scripts/UI/Style/Editor/`). They edit the existing prefabs and scenes in place and can be run again; they find what they made before.
  - **Apply Foundation** (`UIRevampTools`): fills `UISettings`' asset references, sets the canvas scalers, rebuilds the selection marker prefab and adds it to LevelSelector.
  - **Re-apply UI Styles:** after editing `UISettings`, it pushes the values into every prefab so the Editor shows them.
  - **Restylers that own a window's layout:** Restyle HUD (`HudRestyler`), Restyle Inventory (`InventoryRestyler`), Restyle Level-up Window (`UpgradeRestyler`), Restyle Pause and Death Screen (`PauseDeathRestyler`), Restyle Settings (`SettingsRestyler`), Restyle Stats Pane (`StatsRestyler`), Restyle Item Bag (`ItemBagRestyler`), Restyle Tooltip (`TooltipRestyler`), Restyle Confirm Dialog and Endscreen (`ConfirmEndscreenRestyler`), Restyle Run Encounter Windows (`RunEncounterRestyler`), Restyle Wave Results (`WaveSummaryRestyler`), Restyle Shop (`ShopRestyler`), Restyle Map HUD (`LevelSelectorRestyler`) and Restyle Main Menu (`MainMenuRestyler`). Make layout changes there, not by hand, or a re-run overwrites them.
  - **Scene instances:** restylers never revert a whole instance; they only replace what they own.
    - Unity keeps a prefab instance root's anchors, size and position as overrides that a revert does **not** remove, so a restyled prefab can keep its old spot in a scene.
    - Restylers copy the prefab's root layout onto the scene instance (`UIBuildHelpers.MatchPrefabRootLayout`).
    - `SceneInstanceLayout.AssertMatchesPrefab` (tests) guards it.
  - **Batchmode:** `-executeMethod Game.UI.Style.Editor.UIRevampTools.ApplyFoundation` (or `<Restyler>.Restyle`).
  - **Revert before you snap:** `PrefabUtility.RevertPropertyOverride` on a scene instance reloads it from its recorded overrides. Revert first, then snap. `MatchPrefabRootLayout` now also records its values as instance modifications, so a revert in the same pass can't drop them.
  - **Children's leftover layout overrides:** a removed layout group can leave its children's anchors and positions as scene overrides (they pinned the wave results title to the bottom). `UIBuildHelpers.RevertChildLayoutOverrides(scene, prefabPath)` reverts just those layout properties on an instance's children (wave results, shop). `SceneInstanceLayout.AssertNoChildLayoutOverrides` guards it.
  - **Helpers:** `UIBuildHelpers` has find-or-create, place/stretch and style helpers for the next windows' restylers.
- **Gamepad selection frame:** `UISelectionMarkerUI` wraps the selected element. The frame's position is computed in its parent's local space from the element's corners (`SelectionFrameMath`), so it works at any canvas scale and on Screen Space Camera canvases. It slides between elements and pulses.
  - A `UIWindow` can only override the padding now. The old per-window arrow offsets are gone.
- **HUD layout** (1920×1080 reference):
  - **Top left:** HealthPane and the wave hint, stacked in `TopLeftHud` in UIMain (a VerticalLayoutGroup). The hint takes no room while hidden.
  - **Top right:** experience. **Bottom left:** IconsPane. **Bottom center:** boss flame bar.
  - **HealthPane, IconsPane and the wave hint** are framed.
  - **Grace bar:** split at 50% by anchors. Fills use the neutral sprite tinted by `GraceBar` from `UISettings`.
  - **Grace label:** cyan at 0 and above, purple below 0 (`GraceBar.GetLabelColor`). The pending label reads "Corruption -N".
  - **Ability slots:** Medium icons. Each slot's input glyph is its child, centered on the bottom edge.
- **Inventory** (`InventoryPane`, `InventoryControllerUI`, `EffectIconUI`):
  - **Window:** framed, on the left between the HUD's top-left stack and IconsPane. The title row has "Inventory" (Big, bold) and the coins (Small, 28px icon).
  - **Grid:** a `Grid` child holds 8 columns × 10 rows of Small slots, and icons spawn there (`iconContainer`).
  - **Slots:** each slot is a Slot frame tinted by item quality, the icon inside it, and a count badge shown only above 1.
  - **Tooltip:** item tooltips use the scene's shared `GeneralTooltipPane`. The title is the quality-colored name (`EffectQualityDisplayHelper.ColorizeName`, also used by `TreasureUI`).
  - **Hiding:** `EffectIconUI.OnDisable` hides its tooltip, because icons are destroyed on refresh and hidden with the window without a pointer exit. The controller only hides a tooltip it opened.
- **Center windows** (`CenterWindowLayout`): level-up, pause, death screen and Settings.
  - **Size and position:** the Inventory's height (645) and vertical position, centered, 400 wide (Settings 640). The tops and bottoms line up with the Inventory.
  - **Title row:** centered, Big and bold.
  - **Progress button** (Continue, Resume, Retry, Back): a Small 160×32 button at the bottom center. It is the same spot in every window; other buttons stack above it.
  - **Focus:** the windows don't tint their frame for focus (`UIWindow.focusFrame` is cleared); the selection frame shows focus.
- **Level-up:**
  - **Header:** "Select an upgrade", then a yellow "You are now Level N" line (`levelText`), then green "Coins gained".
  - **Choices:** spawn in an `Options` column as Small full-width buttons, with the role star Tiny.
- **Pause:**
  - PAUSE is yellow.
  - Buttons from the bottom: Resume, Save & Exit, Settings.
  - **Settings:** `PauseMenuController.OpenSettings` hides the pause panel, Stats and Inventory, then opens the scene's SettingsPanel (`SetActive` + `SetOpen`, priority 102). Back, Cancel or Pause close it back to the pause menu.
- **Death screen:** YOU DIED in Display red; Retry, then Exit above it.
- **Settings** (`SettingsPanel.prefab`, used in MainMenu and UIMain):
  - **Layout:** two columns.
    - Left: Gameplay (auto-attack, lock-on mode, tutorial hints) and Audio.
    - Right: Graphics and Language.
  - **Close:** Back and Cancel call `SettingsPanelController.RequestClose`. Whoever opened it subscribes to `OnCloseRequested` (`MainMenuController`, `PauseMenuController`), so neither scene wires it.
  - **Main menu:** it rests at its shown spot on the right, x = −(15 + width/2) (`SettingsRestyler.MainMenuShownX`, set on the instance), and `MainMenuController` fades and slides it in and out like the other menu panels.
- **Shared tooltip** (`GeneralTooltipPane`, `GeneralTooltipPaneUI`, one per scene):
  - **API:** `ShowTooltip(description, title, detail, list, anchor)` / `HideTooltip()`.
  - **Layout:**
    - **Title:** Big, bold.
    - **Left column:** `GeneralTooltipTxt` (main text), then `DetailedTooltipTxt` (`detail`: stat sources and benefit, upgrade Now/After).
    - **Right column:** `ListTxt` (`list`: an item's stat lines).
    - Empty parts hide.
  - **Size:** the pane is 600 wide; its height follows the content (ContentSizeFitter). The frame is at `UISettings.tooltipOpacity`.
  - **Raycasts:** the pane never takes raycasts (CanvasGroup), so it can't steal hover or flicker.
  - **Placement:**
    - **Anchor:** the given `anchor`, else the gamepad selection, else the pointer.
    - **Side:** to its right if it fits, else to its left.
    - **Height:** top-aligned with the anchor, and kept on screen with the margin (pure `TooltipPlacement.TopLeft`).
    - **Item bag:** anchors to its selected slot.
  - **Item text:** `Effect.GetDescriptionText()` (description + ability tier lines) and `Effect.GetStatLines()` (modifier + bonus rule lines). `GetFormattedDescription()` still returns both joined, for the shop, main menu and skill tree.
- **Item bag** (`ItemBagPane`, `ItemBagControllerUI`), the end-of-wave window where each picked-up item is taken or sold:
  - **Header:** "Items found", and "Take or sell each item" in muted text.
  - **Grid:** a 6-column grid of the **Inventory's slot** (`EffectIconUI`, the only item slot prefab; `ItemBagIconButton` is gone).
  - **Selection:** the selected slot shows a yellow `SelectedMark` ring (`EffectIconUI.SetSelected`, sprite `UISettings.selectedOutline`).
  - **Tooltip:** the shared tooltip keeps showing the selected item. Hovering another item previews it, and moving off returns to the selected one. With the gamepad, moving onto an item selects it.
  - **Buttons:** Take sits in the progress spot, with "Sell (N)" above it.
  - **Panes:** opens the Stats pane and the Inventory beside it (`inventoryUI`, wired in UIMain).
- **Stats pane** (`StatsPane`, `StatsPaneUI`, `StatRow`):
  - **Window:** mirrors the Inventory on the right (same size, height and row). Title "Stats".
  - **Tabs:** Main/Secondary use the tab art (`UISettings.GetTabSprite`). `StatsPaneUI.ApplyTabs` gives the selected tab the picked sprite and a yellow label.
  - **Scrolling list:** `Scroll` holds the ScrollRect, `View` is the RectMask2D viewport, and Primary/Secondary are the content.
    - `StatsPaneUI.ShowStatsWindow` points the ScrollRect at the visible tab and resets it to the top.
    - With the gamepad, `StatRowUI.OnSelect` → `StatsPaneUI.NotifySelected` scrolls just enough to show the row (`ScrollRectMath.NormalizedPositionToShow`, pure, tested).
    - The scrollbar auto-hides and has navigation None, so the gamepad never lands on it.
  - **Gotcha:** every graphic inside scrolled content must be **maskable**. The row texts were not, so they drew outside the list. `StatsStyleTests` guards it.
  - **StatRow:** a full-width, 32-high band (the slot frame at low alpha that brightens on hover and select; the Selectable's target graphic, so the gamepad frame wraps the whole row), a Tiny star, a name, and a green/red value. The name stays white.
- **Confirm dialog** (`ConfirmDialogUI`, prefab `Prefabs/UI/Misc/ConfirmDialogUI.prefab`):
  - **Layout:** a 400×160 framed `Window` with a Small centered message, and OK | Cancel as Small 160×32 buttons in a centered `Buttons` row (a HorizontalLayoutGroup). No title: the message is the question.
  - **Modal backdrop:** the `Panel` the script toggles is a 4000×4000 dim image (`UISettings.modalBackdropColor`) centered on the dialog. It blocks clicks on the windows behind. It is fixed-size because the dialog's parent differs in each place.
  - **API:** `Show(message, onConfirm, onCancel)` asks a question. `ShowInfo(message, onClose)` hides Cancel, so OK is centered and the gamepad lands on it (the main menu's locked-class message).
  - **Gamepad:** the Panel's `UIWindow` is Primary, AutoFocus, priority 101, default Cancel, and B → `Cancel`.
  - **Instances:** the MainMenu scene, plus nested in `PauseMenu.prefab` and `ShopPane.prefab`, where it is the last sibling. The restyler snaps the scene instance and the nested ones (their root layout overrides live in those prefabs).
- **End screen** (`Endscreen.prefab` in UIMain, `EndscreenControllerUI`), shown after the boss or the last level:
  - **Layout:** a center window like the death screen. VICTORY is the second Display headline exception, in yellow, followed by a Small "Thanks for playing the demo!" line. Main Menu sits in the progress spot.
  - **Main Menu:** wired in code (`mainMenuButton`). It used to be a UIMain override pointing at another prefab's `PauseMenuController`. It unpauses before `LoadMainMenu`, because the end screen paused the game.
  - **Gamepad:** a `UIWindow` on MainPanel (Primary, AutoFocus, priority 50, default Main Menu). It opens the Stats pane and the Inventory beside it, like the death screen (`showStatsAndInventory`).
  - **Pause:** `PauseMenuController` ignores Pause while it's up (`EndscreenControllerUI.IsShown`), since Resume would unpause the game behind it.
- **Run encounter windows** (`Prefabs/UI/RunEncounters/`, all in UIMain): center windows with a Big bold **yellow** title. `UIWindow` priority 50.
  - **Campfire** (`CampfireUI`):
    - One line from `restedFormat`: "You feel rested. You gained 5 Grace and 10 health.", with the Grace in cyan and the health in green (`FormatRestedText`).
    - Continue (wired in code) is in the progress spot.
    - It opens the Stats pane and the Inventory beside it; `CampfireGraceGenerator` no longer touches the UI.
  - **Results** (`RunEncounterResultUI`): the result lines of an event option or a currency / experience treasure. "Gained…" lines are green and "Lost…" lines red (`ColorizeResultLines`). Continue ends the encounter.
  - **Event** (`EventEncounterUI`):
    - **Body:** a `Body` column holds the text (flexible height) above `OptionsContainer`, a bottom-aligned column.
    - **Options:** each option (`EventOptionButton.prefab`) is a full-width Small button that grows with its wrapped label. Its root VerticalLayoutGroup sizes it, with no root LayoutElement, so `UIStyleButton` doesn't pin it to 32. The label's min height keeps it at least a Small button high.
    - **Tooltips:** an option's tooltip anchors beside it.
    - **Art panel:** to the left, the window's size, shown only for events with art.
  - **Shared helper:** `StatsAndInventoryPanes` (`Game.UI`) opens and closes the two panes beside a window; the death screen, end screen and campfire use it.
- **Wave results** (`WaveCorruptionSummaryUI`):
  - **Layout:** a center window. "Wave Results" is Big, bold, yellow. A `Content` column holds the Small receipt rows (label left, value right via `FormatRow`; hidden rows collapse), the Big formula, the `GraceMeter` (built like the HUD bar, with a tick at 0) and the Big gold Favors row.
  - **Button:** Continue in the progress spot.
  - **Removed:** the old click / press A prompt.
- **Shop** (`ShopPane`, `ShopControllerUI`, `ShopIconUI`):
  - **Layout:** a center window between the Inventory and the Stats pane, which both open with it. "Shop" is Big, bold, yellow. Under it is a `Grid` of 5 fixed columns with no scroll: `ShopKeeper.maxItemCount` (20) fits in 4 rows, and `ShopStyleTests` guards that.
  - **Cells:** each cell (`ShopIconUI.prefab`) nests the **Inventory slot** (`EffectIconUI`: quality frame, icon, its own Button and tooltip events) over a price row (a Tiny coin and the Small price).
  - **Affordability:** `ShopKeeper.CanAfford` decides it and pushes it to the UI. Items the player can't afford are dimmed (`EffectIconUI.SetDimmed`) with a red price; they can still be clicked, which shows the error.
  - **Item details:** the shared tooltip, anchored beside the slot, with a price line in its detail section (`ShopControllerUI.FormatPriceLine`, red with "Not enough coins"). The shop's own description panel and the "(A) Buy" prompt row are gone.
  - **Bottom:** the "Not enough Golden Chronos" error sits just above Continue, and Continue is wired in code.
  - **Focus:** the window is AutoFocus at priority 50; the shop is modal (player input blocked until Continue).
- **Main menu** (`MainMenu.unity`, prefabs `Prefabs/UI/Menus/MainMenuPanel` and `CharacterSelect`, `MainMenuController`):
  - **Panels:** every one rests at its shown spot on the right at the standard height (645), as does Settings.
    - **Menu panel:** 400 wide. Its title "Flames of Faith" is Big, bold, yellow (an Inspector text; the scene also has the logo art). The buttons are **Big** (48), and `SaveInfo`, Small muted, sits under Load Game: "Warrior - Act I - Level 7" from `RunSaveService.TryLoad`, or "No saved run" (`FormatSaveInfo`).
    - **Class select:** 640 wide.
      - **Cards:** three on the left (`ClassSelectHoverButton`): a portrait, the class name, a red "Locked" note, and the item bag's yellow ring on the chosen one.
      - **Details:** on the right, like the skill tree's: the name, the description, a yellow "Starting weapon" header, the weapon (`ClassKits.GetWeapon`, quality color) and its text, a green "Unlocked" / red unlock-cost footer, and **Skill Tree**, which opens the tree on that class (`SkillTreeController.Open(class)`).
      - **Buttons:** Start Run in the progress spot, Back above it.
  - **Class select flow:**
    - **Choosing:** a click chooses a class; with the gamepad, moving onto a card chooses it. Hovering previews another class (`ClassToShow`).
    - **Start Run** starts the run; on a locked class it shows the unlock dialog. Nothing starts from a single click any more.
    - **Back:** B goes back (the window's `onCancel`).
    - **Order:** `classesByPlayerIndex` is Warrior, Archer, Augur, the `GameSession.SelectedPlayerIndex` order.
  - **Transitions:**
    - **How:** `MainMenuController.Switch` fades and slides the old panel out (0.15 s, `slideDistance` to the right) and the new one in (0.25 s), eased (`UIFlight.EaseOutCubic`) on unscaled time. It only touches alpha, `blocksRaycasts` and position; interactability is left to `UIFocusManager`.
    - **Input:** clicks are ignored while a panel moves.
    - **Removed:** the old Animator, its six clips and the 1 s waits.
- **Map HUD** (LevelSelector scene, prefabs in `Prefabs/UI/LevelSelector/`):
  - **Map window** (`LevelSelectorUI.prefab`, `OverworldMapInfoUI`): top right, 486 wide, the Stats pane's width and right edge, ending above it. It shows:
    - the act title (Big, bold, yellow; `OverworldPlayerStatePresenter.FormatActTitle`, "Act I - Jesterwoods", from `RunMapGraph.displayName`);
    - the level under the player (Small, the current node's `LevelData.DisplayName`; it changes as the player moves);
    - the coins (Tiny coin, Small);
    - Save & Exit (Small) with its (Y) prompt.

    The coins were a separate CurrencyPane, merged in. The window stays the Primary, OnDemand `UIWindow`.
  - **Prompts** (`InputPrompts.prefab`): a framed window bottom left, gamepad only, sized to its rows.
    - `MapRows` (Move, Enter level, Menu / Stats) show while no window has focus (`GlobalFocusRequirement.NoWindowFocused`).
    - `MenuRows` (LB/RB Swap windows, B Back) show while the gamepad is in the UI.
  - **Stats pane:**
    - **Display:** it shows on every map visit. `OverworldPlayerStatePresenter` opens it on `PlayerManager.OnPlayerDisabledOnMap`, after the player state loads. The map's UI setup event fires earlier, while the stats are still empty, which was the bug.
    - **Gamepad:** Y enters the UI (the map window), LB/RB reaches the Stats pane, and B goes back to moving on the map.
  - **Canvas plane:** this scene's canvas is Screen Space - Camera. Its children must stay on the canvas plane: `LevelSelectorRestyler` zeroes their z (the Stats pane sat ~8000 units toward the camera and was clipped on wide screens).
  - **Level names:** `LevelData.DisplayName` is shown to the player, so keep it readable. `LevelSelectorStyleTests` rejects CamelCase or numbered placeholders.
- **Tests:**
  - `UISettingsTests`: the standard, colors, sliced art, canvas scalers, marker.
  - `CenterWindowStyleTests`: the shared height, frame, titles, Small buttons, the progress button spot, scene placement, the level-up header, and the end screen (thanks line, gamepad window, Main Menu wired in code).
  - `ConfirmDialogStyleTests`: size, frame, backdrop, message, the button row, the gamepad window, and the three instances (layout, drawn on top).
  - `RunEncounterStyleTests`: the Campfire line and wiring, the Results colors, the Event body / options / art panel, and the Treasure having no window. The three windows are also `CenterWindowStyleTests` cases; the Event has no fixed progress button.
  - `WaveSummaryUITests`: row sizes, gold Favors, the column order, Continue as a button, the meter, no leftover child layout overrides in UIMain, the formatting, `GraceBar.GetFills` and the `UIFlight` path.
  - `MainMenuStyleTests`: both panels (place, frame, title, Big menu buttons, the saved-run line, cards, details, Start Run / Back), the scene (no Animator, B and Skill Tree wired, controller fields and class order, layouts), the saved-run text and the hover preview rule.
  - `LevelSelectorStyleTests`: the map window (place, sizes, title, coins, Exit, OnDemand window), the prompts (frame, gamepad only, the focus rules), the scene (no CurrencyPane, presenter wired, no z offsets, layouts), act titles and Roman numerals, readable level names.
  - `ShopStyleTests`: no leftovers (description panel, prompt row, scroll view), the 5-column grid fits the item cap, the cell nests the Inventory slot with a Tiny coin and Small price, the error above Continue, AutoFocus, no child layout overrides in UIMain, and the price line.
  - `SettingsStyleTests`: columns, fonts, the tutorial hints toggle, the close flow, the pause integration and the slide.
  - `StatsStyleTests`: the pane mirrors the Inventory, tabs, legend, the scroll setup, the rows (band, maskable) and the scroll math.
  - `ItemBagStyleTests`: the Inventory slot in a 6-column grid, no description panel, subtitle, Sell label, selected outline, and the Inventory wired.
  - `TooltipStyleTests`: see-through frame, no raycasts, the text and list columns, scene placement, the placement math, and the item text split.
  - `HudStyleTests`: every HUD text is a styled Small/Big text, frames, icon sizes, Grace split and colors, margins, scene placement.
  - `InventoryStyleTests`: frame, bold title, coins, grid, slot, shared tooltip, scene placement.
  - `CooldownSweepTests`.
- **Next windows** follow the same recipe: a restyler entry, style components and a guard test.

### Class kits, skill trees & Favors (reworked Oct 2026)

Each class's abilities and bonuses are **items** (its kit), and each item has **tiers** bought with **Favors** in a per-class skill tree.

- **Kit:** 6 starting-only items per class (`Effect.startingOnly`): weapon, off-hand, Emblem, and 3 Glyphs.
  - **Glyphs carry an ability** (`Scripts/Effects/Abilities/`): `AbilityBehavior<TValues>` is a data-only `EffectBehavior` with a per-tier table, one per slot (`AbilitySlot` Primary/Special/Dash).
  - **The class components still perform the ability** and read their numbers with `EffectStore.TryGetAbility<T>`: `SwordWeapon`/`BowWeapon`/`ScepterWeapon` and the `*Dash` classes.
  - **Hooks:** `WeaponBase.ApplyAbilityItems` (on initialize and on item changes); `DashBase.ApplyAbilityItems` (before each dash and on item changes, so a new duration counts for that dash and charges are right from the start).
  - **Without the item**, each falls back to its own `WeaponData`/Inspector values.
  - **Upgrade-only mechanics** (tier 0 has none):
    - Holy Shot extra cone arrows; they don't raise `onBowAttackLaunched`, so on-hit spawn items trigger once per volley.
    - Fireball impact explosion.
    - Fire Tornado explosion damage.
    - Whirlwind holy shield: `PlayerHealth.ActivateProjectileShield`. While it's up, enemy projectiles (`PlayerHealth.IsEnemyProjectile`) are absorbed in `TakeDamage` before invulnerability, Evasion and `onDamageTaken`, destroyed, and heal a fraction of their damage. Melee and contact hits still land.
  - **Emblem:** `MainStatOnLevelUpBehavior` (+1 main stat per level, baseline = level when the item is added) plus the class's stat bonus rules.
  - **Tooltips describe the owned tier:** each ability overrides `Describe(row)` (`AbilityBehavior.DescribeTier`), and `Effect.GetFormattedDescription` adds those lines under the item's text, next to the stat lines.
    - The text comes from the tier's own numbers, so it can't drift: e.g. "Explodes on impact for 4 base damage" appears from Fireball +1 only.
    - Keep the DB descriptions generic: no numbers, nothing that only some tiers have. `AbilityDescriptionTests` guards this.
- **Tiers:** each tier is its own DB row (`"<item> +N"`, `Effect.tier` = N, own `EffectID`).
  - Every tier of a Glyph uses the **same** ability asset; `Effect.tier` picks the table row (a tier past the table uses the last row).
  - Mark fields where smaller is stronger with `[LowerIsBetter]`.
- **Skill trees:** `SkillTreeDefinition`, one per class in `Resources/Metaprogression/SkillTrees`, read through `SkillTrees`.
  - Root: `unlockedByDefault` (Warrior only) and `unlockCost`.
  - Nodes (`weapon`, `offhand`, `emblem`, `primary`, `special`, `dash`) list their tier items and `upgradeCosts`. Node ids are saved, so don't rename them.
  - Each node is a linear path: tier N is bought from tier N-1 (`SkillTreeRules`).
- **Starting kit:** the skill tree is the **only** source of starting items. `ClassKits.Resolve(class)` takes each node at its bought tier, and `PlayerManager.ResetAllPlayerComponentStates` pushes it into `EffectStore.SetStartingKit` before every new-run reset. Upgrades apply from the next new run.
- **Favors** (`MetaProgressionManager`, saved per class in `<player>_meta.dat` through `SaveFileIO`):
  - **Earned:** +1 per wave that ends with Grace > 0, +5 for the boss kill, and from events via `ChangeFavorsOption`. Values on `MetaProgressionSettings` (`Resources/Metaprogression/`).
  - **Three stages, so Favors can only be spent between runs:**
    1. **Level:** `AddRunFavors` adds to the current level's Favors. `LoadGameplay` discards them (`DiscardLevelFavors`), so leaving a level without finishing it (Save & Exit, quitting) drops them, and replaying from the pre-level save can't farm Favors.
    2. **Run:** a finished level adds them to the run's total (`CommitLevelFavors` in `PlayerManager.HandleWaveGroupFinished` → `GameSession.RunFavors`). The total is saved with the run (`RunSaveData.runFavors`), so Save & Exit on the map and Continue keep it.
    3. **Banked:** only when the run ends does `BankRunFavors` move the run's total, plus the current level's Favors, to the class being played and save it. That happens on death (`PlayerHealth.Die`), the boss kill, the last level, and clearing the last act. Abandoning a run (New Game over a save) loses its Favors.
  - **Spending:** node upgrades spend the class's own Favors, or all classes' when the developer toggle `sharedFavorPool` is on (`FavorRules`). Class unlocks always spend from all classes combined, because a locked class can't earn its own.
- **Developer:**
  - `MetaProgressionSettings.unlockAllClasses` (on for tester builds) makes every class playable.
  - The debug cheat window swaps an owned kit item for another tier of its line (`SkillTrees.EquipTier`).
  - `MetaProgressionManager`'s Inspector context menu (Play mode only) adds Favors, upgrades a node, unlocks a class or resets everything.
- **Main menu:** locked classes are tinted with a red "Locked" note (`ClassSelectHoverButton.SetLocked`). Their details footer shows the unlock cost in red, and Start Run shows the unlock dialog instead of starting. The details show the weapon at its bought tier, and the Skill Tree button opens the tree on that class.
- **Class icon:** `CharacterClassData.icon` is the class's menu icon (skill tree root). Don't reuse `defaultSprite` for it: `CharacterVisual` assigns that one to the player's SpriteRenderer on spawn.
- **Skill tree screen** (main menu, prefab `Prefabs/UI/Menus/SkillTreePanel.prefab`):
  - `SkillTreeController` (gameplay side, on the always-active prefab root) builds a plain `SkillTreeView` and handles upgrade and unlock requests.
  - `SkillTreeUI` / `SkillTreeNodeUI` (`Scripts/UI/SkillTree/`) only render it and report clicks.
  - Clicking a node buys its next tier; clicking the class root unlocks the class; both go through the scene's `ConfirmDialogUI`. It's a `UIWindow` with priority 10.
  - The prefab is generated by `Tools/Flames of Faith/Build Skill Tree Panel Prefab` (`SkillTreePanelBuilder`). Rebuilding overwrites hand edits, so make layout changes in the builder.
- **The shared Favor pool is on:** players see and spend one total, which is less confusing than per-class pools.
- **Tests:** `ClassKitTests`, `SkillTreeTests`, `SkillTreeRulesTests`, `FavorTests`, `AbilityTierTests`, `AbilityDescriptionTests`, `ProjectileShieldTests`.
- **Current costs:** each node's tiers cost 10 / 15 / 20 Favors; Archer and Augur unlocks cost 50. A normal lost run earns about 12–18 Favors.
- **Adding kit content:**
  1. Add the DB rows: tier 0 with Starting Only, then `+1..+N` with `tier` set.
  2. Generate ScriptableObjects.
  3. Add the items to the class's tree node.
  4. For a Glyph, give its ability asset at least as many table rows as the node has tiers (a test checks this).
  5. A new ability type also needs a `Describe(row)` override, or its tooltips show no tier line (`AbilityDescriptionTests` fails).

### Effect data pipeline — every field change touches 5 places

`Effect` (`Assets/Scripts/Effects/Effect.cs`) is authored in a SQLite database (via `Tools/Effects/Effect Database`), not hand-edited as ScriptableObjects — the `.asset` files under `Assets/Resources/Effects/` are generated output, not source of truth. Whenever a field is added/changed/removed on `Effect`, update all of these together or the DB and the runtime SOs silently drift apart:

1. **`EffectRow`** (`Assets/Scripts/Database/EffectRow.cs`) — the SQLite row shape. Prefer a nullable type (e.g. `int?`) for a new column rather than a non-nullable value type: `SQLiteConnection.CreateTable` auto-migrates missing columns onto the existing table via `ALTER TABLE ADD COLUMN` with no default value, so pre-existing rows read back as SQLite `NULL` for that column — assigning `null` into a non-nullable value-type property via this ORM's reflection-based `SetValue` throws at load time. Treat `null` as "column didn't exist yet" and pick whatever meaning is backward-compatible (e.g. `availableForShop`'s `null` means "true", so old effects don't silently disappear from the shop).
2. **`Effect`** — add the `[SerializeField]` + property, and map it in `InitializeFromData(EffectRow row)`.
3. **`EffectLoader.CreateEffectSO(EffectRow row)`** (`Assets/Scripts/Database/EffectLoader.cs`) — a second, separate row→SO mapper used by the DB window's "Test Load" button; easy to update `Effect.InitializeFromData` and forget this one exists.
4. **`EffectLoader.GenerateAndSaveAllEffects`** — the actual DB→`.asset` generator (menu `Tools/Effects/Generate ScriptableObjects`), run after editing rows so the generated SOs pick up the change; it calls `InitializeFromData` under the hood, so it needs no edits itself, but you must re-run it.
5. **`EffectDatabaseWindow`** (`Assets/Scripts/Editor/Database/EffectDatabaseWindow.cs`) — the custom `EditorWindow` (`Tools/Effects/Effect Database`) for authoring rows; add a control for the new field in `DrawEditPanel`, mirroring its existing `unlockedByDefault`/`quality` fields.

**Stat bonuses (stat dependency rules, `StatDependencyRule`):** "every N points of stat X give ±amount of stat Y", stored in `EffectRow.statDependenciesJson` and edited under "Stat Bonuses" in the window.
- `PlayerProgression.RecalculateFinalStats` applies them in a second pass, from the totals before any bonus (corruption included), through the pure `StatDependencyCalculator`. Rules never chain, and one that feeds its own stat just doubles it.
- Items with rules are **starting items only**: `ShopKeeper` filters them out, and `StatDependencyTests` guards the hand-authored treasure, event and drop lists.
- The stats pane's Shift view shows them as "(+N from Retribution)".

At runtime, look effects up through **`EffectsDatabaseProvider`**. It loads every `Effect` from `Resources/Effects`, and `GetEffectById` searches available and unlockable effects. The static `EffectsDatabase` class is legacy: the `Resources/Effects/EffectsDatabase` asset it loads doesn't exist, so its lookups return null.

## Coding conventions

- Every script declares an explicit `namespace Game.<Area> { ... }` matching its folder.
- Private/serialized fields use `camelCase` (no `_` or `m_` prefix), including `[SerializeField] private` fields; public members use `PascalCase`; interfaces are prefixed `I` (e.g. `IDamageable`, `IPrimaryStateLoader`).
- C# events use the `onXChanged`/`OnXChanged` naming pair (private backing field lowercase, public event/property PascalCase) and are unsubscribed in `OnDisable`.
- Manager/service classes are singletons extending `Singleton<T>`; check `Instance != null` before use since scene load order isn't guaranteed.
