# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

FlamesOfFaith is a 2D action roguelite built in Unity 6 (`6000.2.10f1`, URP). Runs (attempts) proceed through an act-based overworld map, into isometric combat levels with waves of enemies, elemental debuffs, boss fights, and between-run meta-progression / relic ("Effect") systems.

## Working with this repo

- **Git: always work and commit directly on `main`** unless the user says otherwise for a specific task. Don't create feature branches on your own; this overrides any default "branch first when on the default branch" behavior.

This is a Unity project — there is no CLI build/lint/test pipeline (no `package.json`, no CI config). Development happens in the Unity Editor:

- Open the project in Unity Editor **6000.2.10f1** (must match `ProjectSettings/ProjectVersion.txt` — Unity will offer to auto-upgrade if a different version is installed; don't let it silently change the version).
- There is no headless build script or Makefile in the repo. Compilation errors surface in the Editor Console; there is no `dotnet build` step that mirrors Unity's compile (the generated `.csproj`/`.sln` files are for IDE intellisense only, not authoritative builds).
- **Tests.** The only automated tests are the EditMode tests in `Assets/Scripts/Tests/Editor/`, which cover enemy damage (see "Enemy damage" below).
  - They need no asmdef: the default `Assembly-CSharp-Editor` already references NUnit (test-framework 1.6.0), so any test under an `Editor/` folder is picked up.
  - Run them from Window → General → Test Runner → EditMode. With the Editor closed, use `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml`.
  - Everything else has no coverage, so verify it by running the game in the Editor (Bootstrapper scene).
  - `Assets/Scripts/Tests/LevelTreeDebugger.cs` is a debug helper, not a test.
- All first-party code compiles into the default `Assembly-CSharp` assembly — there are no custom `.asmdef` files under `Assets/Scripts/`, so any script can reference any other without assembly-reference wiring.
- Third-party/vendored code lives under `Assets/AssetPackages/` (ClassicPixelRPGUI, DamageNumbersPro, NavMeshPlus, WalldoffStudios Indicators) and should generally be left alone rather than modified in place.
- Key packages: URP 17.2.0, new Input System 1.14.2, Cinemachine 3.1.3, `com.unity.ai.navigation` (NavMesh) plus the vendored NavMeshPlus for 2D nav, Timeline, Visual Scripting.

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
- **UI stays decoupled from game logic**: scripts under `Scripts/UI/` should never call `FindAnyObjectByType<T>()` (or hold a discovered reference) to reach into a gameplay class for data or to trigger a calculation — they should only receive plain data and delegates that gameplay pushes to them, the same way `items` already flowed from `ShopKeeper` into `ShopControllerUI`. When a UI layer needs something new from gameplay (a computed price, a reference to act on later), extend the *existing* event/callback the gameplay class already fires rather than adding a lookup on the UI side. Concrete precedent: `ShopKeeper.onShopWindowToggle` is `Action<bool, List<Effect>, ShopKeeper>` — it pushes the `ShopKeeper` reference itself alongside the items, so `ShopControllerUI` never searches the scene for it, and `ShopKeeper.GetDiscountedPrice(effect)` (not the UI) is the one place price gets computed, so display and the actual charge can never drift. `ShopIconUI`/`ItemBagControllerUI` go further and never reference a gameplay class type at all — they only take primitives (`Effect`, `int`, `Action`s) in their `Setup`/`Select` methods. The exception is a one-time `FindAnyObjectByType` purely to attach/detach an event subscription (e.g. `ShopControllerUI.SubscribeToEvents`) — that's wiring, not a data query, and isn't what this rule is about.

### Two parallel map systems — know which one you're touching

- **`Scripts/Map/`** (legacy, `MapGenerator` explicitly marked `[Obsolete]`) — layer-based procedural level-select map, rendered via uGUI with bezier connectors (`UIBezierConnector`). Being phased out; avoid extending it.
- **`Scripts/Overworld/`** (active development) — graph-based map system:
  - Data model: `MapDefinition` (ScriptableObject, asset under `Assets/Resources/Overworld/`) → `NodeDefinition` → `ConnectionDefinition` (directional edges with RNG-gated optional paths for run variety).
  - Runtime: `OverworldMapGenerator` seeds a `RunMapState`/`RunMapGraph` (`RunNode`/`RunEdge`) from definitions; `MapRunController` (singleton) owns traversal, fog-of-war reveal, and act progression, and fires events the renderer subscribes to.
  - Rendering: `Scripts/Overworld/Render/` (`OverworldMapRenderer`, `OverworldNodeView`, `OverworldEdgeView`) draws the live graph in-scene.
  - Authoring tool: `Scripts/Overworld/Editor/` — a custom `EditorWindow` (`MapEditorWindow`, menu `Tools/Flames of Faith/Map Editor`) built on UI Toolkit's `GraphView` (`MapGraphView`, `MapNodeView`, sizing constants in `MapEditorConstants`). Editor-authored node positions (`NodeDefinition.editorPosition`) are baked into normalized runtime `worldPosition` via `MapEditorWindow.BakeEditorPositionsToWorldPositions()` (divides by `MapEditorConstants.NodeSpacing`, inverts Y). Toolbar actions `AddNewNode`, `DeleteSelected` and `ValidateMap` are implemented (with Undo).
  - Gamepad/keyboard: `OverworldGamepadNavigator` moves between nodes with the UI Navigate action and enters a level with Submit. It ignores input while any `UIWindow` has gamepad focus (e.g. the map's Exit button, reached with Browse/Y).
  - Generation is deterministic from the seed and the `MapDefinition`s, except which map an act uses (`SetCurrentMapGraph` picks randomly among maps with the same `actNumber`), which is why saves store the `mapId`.

When touching map/level-select logic, confirm whether you're in the legacy `Map/` system or the new `Overworld/` system — they don't share code paths.

### Scene bootstrapping flow

`Bootstraper.cs` additively loads gameplay + UI scenes at startup → `MainSceneController` (singleton) generates the overworld run from act `MapDefinition`s and handles loading-screen transitions into gameplay levels via `LoadGameplay(levelData)` → `GameSession` (singleton) tracks the current run's selected character/difficulty/level and holds a `PlayerData` save-state container → `PlayerManager` (singleton) spawns and tracks the active player GameObject, exposing components via `GetPlayerComponent<T>()`.

- **The player is respawned on every scene load** (`PlayerManager.SpawnSelectedPlayer`). State survives only through `GameSession.PlayerData`: components write to it in `SaveState()` and read it back in `LoadState()`.
- **Player state is saved only when a level is finished.** Every level except the boss ends through the same path. That includes shop, campfire, treasure and event encounters (`Scripts/RunEncounters/`), which are their own scenes. The path is `WaveSpawner.GoToNextLevel` → `PlayerManager.HandleWaveGroupFinished`, which calls `MapRunController.OnLevelCleared`, `SaveAllPlayerComponentStates` and `LoadLevelSelectorScene(false)`. So while inside a level, `PlayerData` still holds the pre-level snapshot.
- **`GameSession.IsNewRun`** decides whether entering a level resets the player components (new run) or loads them from `PlayerData`.
- Meta-progression (unlocked effects) is separate and persisted by `MetaProgressionManager` to `persistentDataPath/<player>_meta.dat` through `IMetaProgressionStateLoader`.

### Combat/effect pipeline

Damage flows through `IDamageable`/`DamageRequest` → `DamageCalculator` (combines weapon class, active `Effect`/`EffectBehavior` modifiers via `IEffectMultiplier`, and player progression stats) → applied to `PlayerHealth`/`EnemyHealth`. Projectiles separate "what it is" (`ProjectileBase`, damage/effect) from "how it moves" (`ProjectileMovementBase` subclasses: linear/arc/homing/bounce/delayed-homing) — extend by composing a movement strategy rather than a new projectile subclass per behavior.

### Corruption & Grace (reworked Oct 2026)

The original spec is `Assets/Docs/corruption-rework.md`, but several decisions changed during implementation; this section is the source of truth. Every tunable lives in **`CorruptionSettings`** (`Assets/Resources/Combat/CorruptionSettings.asset`, loaded via `CorruptionSettings.Instance`) or on Inspector fields — never hardcode these numbers.

- **Grace** (`PlayerGrace`) is a single signed value clamped to `minGrace..maxGrace` (−50..+50), saved per run. It changes only through `SetGrace`, which fires `onGraceChanged`, `OnCorruptedStateChanged` (only on flips) and `OnGraceStatusChanged`. Damage multiplier = `max(minDamageMultiplier, 1 + damagePerGracePoint × Grace)`, applied in `DamageCalculator`.
- **Corrupted = Grace < 0.** `CorruptedLevel = max(0, −Grace)` reduces every stat flagged `affectedByCorruption` in `StatDatabase.asset` (Armor, HealingReceived) by `CorruptedLevel × corruptionReduceFactor`, through `PlayerProgression.GetCorruptionPenalty` (also used by the tooltip). Healing is reduced by a flat amount per heal, not zeroed. Corrupted players show the shared Corrupted VFX.
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
  - `GraceBar`: signed bar, pending wave Corruption label, tooltips.
  - Corruptor bar: `CorruptorPhaseBarPresenter` attaches a world-space `CorruptorPhaseBarUI` to the player.
  - Off-screen arrow: `TargetIndicatorUI`.
  - Wave hints: `WaveHintUI`, toggled by `SettingsManager.ShowTutorialHints`.
  - End-of-wave summary: `WaveCorruptionSummaryUI` waits for Submit. The end sequence waits on its callback; `waveSummaryMaxWait` 0 means no limit.
  - Corruption numbers: `NumberCorruptionGainedVFX`.

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
  - session data: class, difficulty, `levelsBeaten`, and `runStarted = !IsNewRun`;
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
  - The warnings use the reusable `ConfirmDialogUI` (`Show(message, onConfirm, onCancel)`, prefab `Prefabs/UI/Misc/ConfirmDialogUI.prefab`).

### Gamepad & UI navigation

Gamepad support for uGUI is built on `Scripts/UI/Navigation/`:

- **`UIWindow`:** goes on the panel a controller shows or hides, and requires a CanvasGroup. Keep one per window and never nest them.
  - **Role:** Primary (can take focus automatically) or Secondary (reached only with LB/RB).
  - **Open Mode:** `GameObjectActive` for panels that are activated and deactivated. Use `Manual` for panels that stay active and slide off screen, whose controller must call `SetOpen`. The main-menu panels are Manual.
  - **Focus Mode:** `AutoFocus` takes focus on open; `OnDemand` waits for Browse/Y, as in the Shop and the map's Exit button.
  - **Priority:** a newly opened window takes focus only if its priority is *strictly higher* than the focused window's.
  - **Default Selectable** and **OnCancel**.
- **`UIFocusManager`** (singleton, Bootstrapper scene):
  - It tracks open windows and moves focus. Unfocused windows get `CanvasGroup.interactable = false`, which shows their buttons in the Disabled state.
  - It cycles windows with LB/RB.
  - Cancel/B: a Secondary window returns focus to the previous window, an `OnDemand` window releases focus, and other windows invoke their OnCancel.
  - `logFocusChanges` logs every change.
- **`UIWindowOpenWhilePaused`:** lets an always-visible HUD panel join focus only while paused.
- **`UISelectionMarkerUI`:** the visible gamepad highlight. Each scene needs one, drawn above its windows (last sibling).
- **`InputDeviceManager`** (`Game.Control`): tracks keyboard/mouse vs gamepad (`IsGamepadActive`, `OnInputSchemeChanged`). With the mouse, nothing is focused and every window is interactable.
- **Common pitfalls** (all hit in practice):
  - **Selectables left on Navigation: None.** The gamepad can never reach them, and they're skipped when cycling windows.
  - **Selectables outside the window's hierarchy.**
  - **Manual panels set to `GameObjectActive`.** They count as permanently open and steal focus.
  - **The selection marker drawn under a window.**
- `PauseManager.SetPause` sets `timeScale` to 0. Upgrade selection, the item bag and the pause menu pause the game, but the wave-end sequence (including `WaveCorruptionSummaryUI`) does not. Anything time-based that must stop there checks `WaveSpawner.Instance.EndingWave` and/or `GameplayFreeze.IsActive`, as `HealthRegen` does.

### Effect data pipeline — every field change touches 5 places

`Effect` (`Assets/Scripts/Effects/Effect.cs`) is authored in a SQLite database (via `Tools/Effects/Effect Database`), not hand-edited as ScriptableObjects — the `.asset` files under `Assets/Resources/Effects/` are generated output, not source of truth. Whenever a field is added/changed/removed on `Effect`, update all of these together or the DB and the runtime SOs silently drift apart:

1. **`EffectRow`** (`Assets/Scripts/Database/EffectRow.cs`) — the SQLite row shape. Prefer a nullable type (e.g. `int?`) for a new column rather than a non-nullable value type: `SQLiteConnection.CreateTable` auto-migrates missing columns onto the existing table via `ALTER TABLE ADD COLUMN` with no default value, so pre-existing rows read back as SQLite `NULL` for that column — assigning `null` into a non-nullable value-type property via this ORM's reflection-based `SetValue` throws at load time. Treat `null` as "column didn't exist yet" and pick whatever meaning is backward-compatible (e.g. `availableForShop`'s `null` means "true", so old effects don't silently disappear from the shop).
2. **`Effect`** — add the `[SerializeField]` + property, and map it in `InitializeFromData(EffectRow row)`.
3. **`EffectLoader.CreateEffectSO(EffectRow row)`** (`Assets/Scripts/Database/EffectLoader.cs`) — a second, separate row→SO mapper used by the DB window's "Test Load" button; easy to update `Effect.InitializeFromData` and forget this one exists.
4. **`EffectLoader.GenerateAndSaveAllEffects`** — the actual DB→`.asset` generator (menu `Tools/Effects/Generate ScriptableObjects`), run after editing rows so the generated SOs pick up the change; it calls `InitializeFromData` under the hood, so it needs no edits itself, but you must re-run it.
5. **`EffectDatabaseWindow`** (`Assets/Scripts/Editor/Database/EffectDatabaseWindow.cs`) — the custom `EditorWindow` (`Tools/Effects/Effect Database`) for authoring rows; add a control for the new field in `DrawEditPanel`, mirroring its existing `unlockedByDefault`/`quality` fields.

At runtime, look effects up through **`EffectsDatabaseProvider`**. It loads every `Effect` from `Resources/Effects`, and `GetEffectById` searches available and unlockable effects. The static `EffectsDatabase` class is legacy: the `Resources/Effects/EffectsDatabase` asset it loads doesn't exist, so its lookups return null.

## Coding conventions

- Every script declares an explicit `namespace Game.<Area> { ... }` matching its folder.
- Private/serialized fields use `camelCase` (no `_` or `m_` prefix), including `[SerializeField] private` fields; public members use `PascalCase`; interfaces are prefixed `I` (e.g. `IDamageable`, `IPrimaryStateLoader`).
- C# events use the `onXChanged`/`OnXChanged` naming pair (private backing field lowercase, public event/property PascalCase) and are unsubscribed in `OnDisable`.
- Manager/service classes are singletons extending `Singleton<T>`; check `Instance != null` before use since scene load order isn't guaranteed.
