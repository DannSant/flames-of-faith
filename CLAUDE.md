# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

FlamesOfFaith is a 2D action roguelite built in Unity 6 (`6000.2.10f1`, URP). Runs (attempts) proceed through an act-based overworld map, into isometric combat levels with waves of enemies, elemental debuffs, boss fights, and between-run meta-progression / relic ("Effect") systems.

## Working with this repo

This is a Unity project — there is no CLI build/lint/test pipeline (no `package.json`, no CI config). Development happens in the Unity Editor:

- Open the project in Unity Editor **6000.2.10f1** (must match `ProjectSettings/ProjectVersion.txt` — Unity will offer to auto-upgrade if a different version is installed; don't let it silently change the version).
- There is no headless build script or Makefile in the repo. Compilation errors surface in the Editor Console; there is no `dotnet build` step that mirrors Unity's compile (the generated `.csproj`/`.sln` files are for IDE intellisense only, not authoritative builds).
- `com.unity.test-framework` is a declared package dependency, but `Assets/Scripts/Tests/` only contains a debug helper (`LevelTreeDebugger.cs`), not actual EditMode/PlayMode test suites. Treat this project as having no automated test coverage today — verify changes by running the game in the Editor (Bootstrapper scene) rather than expecting tests to catch regressions.
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

### Two parallel map systems — know which one you're touching

- **`Scripts/Map/`** (legacy, `MapGenerator` explicitly marked `[Obsolete]`) — layer-based procedural level-select map, rendered via uGUI with bezier connectors (`UIBezierConnector`). Being phased out; avoid extending it.
- **`Scripts/Overworld/`** (active development) — graph-based map system:
  - Data model: `MapDefinition` (ScriptableObject, asset under `Assets/Resources/Overworld/`) → `NodeDefinition` → `ConnectionDefinition` (directional edges with RNG-gated optional paths for run variety).
  - Runtime: `OverworldMapGenerator` seeds a `RunMapState`/`RunMapGraph` (`RunNode`/`RunEdge`) from definitions; `MapRunController` (singleton) owns traversal, fog-of-war reveal, and act progression, and fires events the renderer subscribes to.
  - Rendering: `Scripts/Overworld/Render/` (`OverworldMapRenderer`, `OverworldNodeView`, `OverworldEdgeView`) draws the live graph in-scene.
  - Authoring tool: `Scripts/Overworld/Editor/` — a custom `EditorWindow` (`MapEditorWindow`, menu `Tools/Flames of Faith/Map Editor`) built on UI Toolkit's `GraphView` (`MapGraphView`, `MapNodeView`, sizing constants in `MapEditorConstants`). Editor-authored node positions (`NodeDefinition.editorPosition`) are baked into normalized runtime `worldPosition` via `MapEditorWindow.BakeEditorPositionsToWorldPositions()` (divides by `MapEditorConstants.NodeSpacing`, inverts Y). Several toolbar actions (`AddNewNode`, `DeleteSelected`, `ValidateMap`) are still TODO stubs — check current state before assuming they're implemented.

When touching map/level-select logic, confirm whether you're in the legacy `Map/` system or the new `Overworld/` system — they don't share code paths.

### Scene bootstrapping flow

`Bootstraper.cs` additively loads gameplay + UI scenes at startup → `MainSceneController` (singleton) generates the overworld run from act `MapDefinition`s and handles loading-screen transitions into gameplay levels via `LoadGameplay(levelData)` → `GameSession` (singleton) tracks the current run's selected character/difficulty/level and holds a `PlayerData` save-state container → `PlayerManager` (singleton) spawns and tracks the active player GameObject, exposing components via `GetPlayerComponent<T>()`.

### Combat/effect pipeline

Damage flows through `IDamageable`/`DamageRequest` → `DamageCalculator` (combines weapon class, active `Effect`/`EffectBehavior` modifiers via `IEffectMultiplier`, and player progression stats) → applied to `PlayerHealth`/`EnemyHealth`. Projectiles separate "what it is" (`ProjectileBase`, damage/effect) from "how it moves" (`ProjectileMovementBase` subclasses: linear/arc/homing/bounce/delayed-homing) — extend by composing a movement strategy rather than a new projectile subclass per behavior.

## Coding conventions

- Every script declares an explicit `namespace Game.<Area> { ... }` matching its folder.
- Private/serialized fields use `camelCase` (no `_` or `m_` prefix), including `[SerializeField] private` fields; public members use `PascalCase`; interfaces are prefixed `I` (e.g. `IDamageable`, `IPrimaryStateLoader`).
- C# events use the `onXChanged`/`OnXChanged` naming pair (private backing field lowercase, public event/property PascalCase) and are unsubscribed in `OnDisable`.
- Manager/service classes are singletons extending `Singleton<T>`; check `Instance != null` before use since scene load order isn't guaranteed.
