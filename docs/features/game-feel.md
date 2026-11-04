# Feature: Game feel (low-health warning, sound, hit feel)

- **Status:** ready
- **Branch:** `feature/game-feel` (off `develop`)
- **Owner session:** implementation session, not started
- **Related docs:** `docs/pm/BACKLOG.md` items 31, 32, 33 (34, the VFX pass, is out of scope); `CLAUDE.md` "UI stays decoupled from game logic", "UI standards", "Gamepad & UI navigation", "Scene bootstrapping flow", "Balance data"

## Goal
Make the game tell the player what is happening without asking them to look at the HUD, and make hits land. Testers often miss their low health, because the bar is top left and their eyes are on the middle, and most actions make no sound. None of this needs the art team, so it can land while their work is pending, and a demo sells in its first minutes.

Keep it restrained. The Corrupted outline exists because VFX had already got too noisy. A player fighting a screen full of enemies should feel the big moments, not a constant rattle.

## Scope
Three chunks, in this order. Each one is playable and committed on its own.

1. **Low-health warning** (BACKLOG 31): a pulsing blood vignette at the screen edges. Can merge alone, ahead of the rest, if the owner wants it in the next tester build.
2. **Sound** (BACKLOG 33): one data-driven sound table, a safer `AudioManager`, and every hook in the "Sound list" below. Hooks ship silent until clips exist. The heartbeat joins chunk 1's warning here.
3. **Hit feel** (BACKLOG 32): camera shake, hit stop, slow motion, gamepad rumble, and two Settings sliders.

## Out of scope
- The VFX pass (BACKLOG 34): it waits for the re-designed characters and scenarios. No new particle effects here.
- Sourcing or making sound clips: the owner decides that. Don't download any.
- Music changes (muffling at low health, new tracks), an AudioMixer, footsteps.
- A red flash on every hit: try it after play-testing the warning.
- A setting for the vignette itself.
- Shake per boss ability: the boss refactor (BACKLOG 20) will reshape abilities.
- Any change to damage, health or other balance values.

## Design details

### Shared rules
- **New area:** `Scripts/Feel/`, namespace `Game.Feel`.
- **Tunables:** `GameFeelSettings`, an SO at `Resources/Feel/GameFeelSettings.asset`, loaded through `GameFeelSettings.Instance` like the other settings assets. Every number below is a starting value on that asset, never hardcoded. It's **not** a balance table, so there's no Sheets pipeline work: look and feel stay in Unity, as the Corruption table's camera and visuals do.
- **Math is pure,** static and tested, with its tunables passed in, like the rest of the project.
- **Master switches:** each of the warning, shake, hit stop and rumble has an on / off on the settings asset.
- **Don't edit level scenes or the three player prefabs.** The art team's new scenarios and characters will replace or reshape them. Add what's needed at runtime (see Camera shake).
- **The load must not throw** (`CLAUDE.md` "Scene bootstrapping flow"). Anything subscribed during a load logs and falls back.

### Chunk 1: low-health warning
- **Rule:** the warning turns on at or below `lowHealthEnter` (0.30 of max health) and off above `lowHealthExit` (0.35), so a small heal doesn't make it flicker. Intensity rises from `minIntensity` at the threshold to 1 at `fullIntensityAt` (0.10 of max health). The pure class is `LowHealthRules`.
- **Pulse:** the vignette's alpha pulses, faster at higher intensity, from `pulseRateMin` to `pulseRateMax` beats per second. Keep the fastest under 2 per second: this must not read as flashing. It runs on unscaled time and fades in and out rather than popping.
- **Art:** a full-screen `Image` with a generated placeholder, `Sprites/UI/Feedback/LowHealthVignette.png` (red at the edges, fully transparent in the middle, about 960 × 540). The middle must stay clear: the player has to see what's killing them. Tint and maximum alpha are on the settings asset. The art team can overwrite the PNG later with a blood-like one.
- **Where:** in UIMain's canvas, stretched, drawn under the HUD and under every window, `raycastTarget` off. `HudRestyler` owns it, per the UI recipe.
- **When it shows:** combat and boss levels only (`LevelType` Combat and Boss), never in a shop, campfire, treasure or event, where the player can't act on it. Never at 0 health: the death screen takes over. Under a pause or level-up window it simply stays behind the window.
- **Wiring:** the UI script subscribes once to `PlayerHealth.onHealthChanged` and reads the first value, as `HealthBar` does (wiring, not a data query). For "is this a fight", extend an existing gameplay event rather than adding a lookup on the UI side, per the UI rule.
- **Respawn:** the player is respawned on every scene load, so check that the warning is right on entering a level already at low health, and that it's off after a Retry.
- **Heartbeat:** once chunk 2 exists, `LowHealthHeartbeat` plays on each pulse above `heartbeatFrom` intensity, except while paused.

### Chunk 2: sound
**`AudioManager` fixes**
- `PlaySFX` sets `pitch` on one shared `AudioSource`, which re-pitches everything that source is already playing. Build a small pool of SFX sources at runtime from `sfxSource` (no prefab edits) and give each sound its own, so pitch is per sound. Both volume setters must reach the whole pool.
- A clip that is null plays nothing and logs nothing.

**`GameSoundTable`** (`Resources/Audio/GameSoundTable.asset`): one entry per `GameSound` enum value: `clips` (one is picked at random), `volume`, `randomizePitch` and `minInterval` (the same sound won't restart sooner; the default is above 0 for anything that can fire many times a frame).
- `AudioManager.Play(GameSound)` is the entry point, safe to call with no clips.
- Add new `GameSound` values at the end of the enum.
- An editor menu, `Tools/Flames of Faith/Audio/Report Silent Sounds`, lists entries with no clips, so the owner has a live to-do list. A silent entry is **not** a test failure.

**XP chain:** `PickupPitchChain` (pure) raises the pitch per pickup within `xpChainWindow` (0.6 s), by `xpChainStep` (0.03), capped at `xpChainMaxPitch` (1.5), then starts over. `CollectAllInstantly` can sweep many tokens in one frame, so one sound must cover them, not one each.

**Enemy attacks:** an optional `attackSound` on `EnemyData`, played where the attack starts and limited by a new `ThrottledSfxQueue` beside the hit and death ones, or a screen of shooters turns into noise. Check that the `Enemies` table lists its fields by hand, so a new field doesn't reach the sheet. If it doesn't, no pipeline work is needed.

**Boss abilities:** an optional `castSound` on the ability asset, played from `BossController.OnAbilityCasted`.

**UI sounds:** one place each, not one call per window.
- `UiClick`: in `UIStyleButton`, which is on every styled button.
- `UiMove`: where `UISelectionMarkerUI` sees the selection change (gamepad only). Not on a window's first selection, or every window opens with a click. No hover sound for the mouse.
- `UiBack`: in `UIFocusManager`'s Cancel.

### Sound list
This is also the audio brief: whoever sources the clips works from this table. Priority 1 is what the demo needs. Existing sounds (weapon attacks, enemy hit and death, player death) stay as they are.

| Sound | Hook | Feel | Priority |
|---|---|---|---|
| `PlayerHurt` | `PlayerHealth.TakeDamage`, after damage lands; not on a dodge, an absorbed shot or `noDamage` | Short, physical, unmistakable over the fight | 1 |
| `Dash` | `DashBase.onDashStarted` | A quick whoosh | 1 |
| `PickupXp` | `ExperienceToken.CollectInstantly`, with the pitch chain | Tiny, bright, pleasant at high repetition | 1 |
| `PickupGrace` | `GracePickup.OnPickup` | Soft, holy | 1 |
| `PickupHealth` | `HealthPickup.OnPickup` | Warm | 1 |
| `PickupItem` | `RandomItemPickup.OnPickup` | A reward | 1 |
| `LevelUp` | the real level-up in `PlayerExperience.AddExperience` (see gotcha) | Rising, a clear reward | 1 |
| `WaveStart` | `WaveSpawner.OnWaveStarted` | A horn or low drum | 1 |
| `WaveEnd` | when the end sequence starts | Release | 1 |
| `UiClick`, `UiMove`, `UiBack` | see UI sounds | Quiet, dry | 1 |
| `ShopBuy` | `ShopKeeper.BuyItem` succeeding | Golden Chronos changing hands | 1 |
| `ShopError` | the "Not enough Golden Chronos" path | A soft denial | 1 |
| `LowHealthHeartbeat` | see chunk 1 | Low heartbeat | 1 |
| `CorruptorAppears` | Corruptor phase start | Dread | 2 |
| `CorruptorKilled` | Corruptor killed in time | A satisfying break | 2 |
| `CorruptorEscaped` | Corruptor escapes | A failure sting | 2 |
| `BossPhaseChange` | `BossWaveHandler.OnPhaseTwoStarted` | Big | 2 |
| `BossDeath` | `BossWaveHandler.NotifyBossDied` | Bigger | 2 |
| `PlayerDodge` | `PlayerHealth.TryDodge` succeeding | A quick slip | 2 |
| `ShieldAbsorb` | `TryAbsorbProjectile` succeeding | Holy ping | 2 |
| Enemy `attackSound` | per enemy | Per enemy, quiet | 2 |
| Boss `castSound` | per ability | Per ability | 2 |

### Chunk 3: hit feel
**Rule of use: only the moments below. Never on routine hits or kills:** with auto-attack, Attack Speed and the Whirlwind, they would never stop.

| Moment | Source | Shake | Hit stop | Slow motion | Rumble |
|---|---|---|---|---|---|
| Heavy hit on the player: at least `heavyHitFraction` (0.20) of max health | `PlayerHealth.onDamageTaken` | Small | 0.05 s | | Medium |
| Player death | `PlayerHealth.Die` | Medium | | 0.4× for 0.6 s | Strong |
| Corruptor killed | `CorruptorPhaseController`: needs its own event, since `OnPhaseEnded` fires on every exit | Medium | 0.06 s | | Medium |
| Boss second phase | `BossWaveHandler.OnPhaseTwoStarted` | Large | | | Strong |
| Boss killed | `BossWaveHandler.NotifyBossDied` | Large | | 0.3× for 1 s | Strong |

**One entry point:** `GameFeel.Play(FeelMoment)` looks the moment's row up on the settings asset, so moments are tuned and added in one place. Each row has a `cooldown`, so a burst of heavy hits can't chain.

**Time scale**
- `PauseManager` writes `Time.timeScale` directly and sets 1 on resume, so a hit stop running under a pause would be cut or left stuck. Add a single owner: `TimeDilation` holds the active request, and `PauseManager` applies `paused ? 0 : TimeDilation's current scale`. Nothing else writes `timeScale`.
- Durations are real time. If a request expires during a pause, resume gives 1. The lowest active scale wins. The pure part is `TimeDilationRules`.
- Reset to 1 on every scene load and on leaving a level, so a slow motion can never carry over. The boss kill's slow motion must end cleanly before `EndscreenControllerUI` pauses. Read how it waits first (its comment around line 84 relies on time scale 1).
- Don't change `fixedDeltaTime`: these are short, and physics speed depends on it.

**Camera shake**
- Use Cinemachine Impulse (3.1.3). Add the `CinemachineImpulseListener` at runtime in `CameraController.SetPlayerCameraFollow`, level cameras only, so no scene changes. No shake on the map.
- Check that it runs during a hit stop (Cinemachine's impulse manager has a time-scale option) and that `CameraFocusController`'s focus and restore aren't disturbed.
- Shake strength is multiplied by the Settings slider; 0 sends nothing.

**Rumble**
- `Gamepad.current.SetMotorSpeeds`, only while `InputDeviceManager.IsGamepadActive`, multiplied by the Settings slider.
- **Motors must never stay on:** stop them on a real-time timer and on pause, scene load, losing focus and quitting.

**Settings:** two sliders, Screen Shake and Rumble, 0 to 1, default 1, in the Gameplay column. They follow the volume sliders in `SettingsManager` (PlayerPrefs keys, an old file gets the default) and `SettingsPanelController`. Layout goes through `SettingsRestyler`, and they need working gamepad navigation in the panel.

### Gotchas already known
- **`PlayerExperience.onLevelUp` also fires outside `AddExperience`** (lines 54 and 90, on load and reset), so hooking the event would play the level-up sound on every level load. Call it only from the real level-up.
- **Some `AudioManager.Instance` calls have no null check.** New calls must have one.
- **`ThrottledSfxQueue` uses `Time.time`,** so it waits during a pause. That's fine for enemies. UI sounds must not go through it.
- **`AudioSource` plays at `timeScale` 0,** so UI sounds work in pause windows with no extra step.

## Acceptance criteria
- [ ] Below the threshold in a fight, the screen edges pulse red and the middle stays clear; it stops after healing above the exit threshold, with no flicker around it.
- [ ] No warning in shops, campfires, treasures, events, on the map, or after death.
- [ ] The warning is right on entering a level already at low health, and off after a Retry.
- [ ] The vignette takes no clicks and draws under the HUD and windows.
- [ ] Every sound in the list has a hook and a table entry; an entry with no clips is silent and logs nothing.
- [ ] A sound's pitch no longer changes sounds already playing.
- [ ] Sweeping many XP tokens at once plays no pile of sounds, and chained pickups rise in pitch.
- [ ] Level loads play no level-up sound, and opening a window plays no `UiMove`.
- [ ] Each moment in the table fires its feedback, and no routine hit or kill does.
- [ ] `Time.timeScale` is written in one place only, and a pause during a hit stop or slow motion resumes at the right speed.
- [ ] No slow motion survives a scene load, and the boss kill reaches the end screen normally.
- [ ] Both sliders work at 0 (no shake, no rumble), save, and are reachable by gamepad.
- [ ] The gamepad's motors never stay on after a pause, a scene load or quitting.
- [ ] No level scene or player prefab changed.

## Files likely touched
- **Code:** new `Scripts/Feel/`; `AudioManager.cs`; `PlayerHealth.cs`, `DashBase.cs`, `ExperienceToken.cs`, `PlayerExperience.cs`, the pickups, `ShopKeeper.cs`, `ShopControllerUI.cs`, `WaveSpawner.cs`, `CorruptorPhaseController.cs`, `BossWaveHandler.cs`, `BossController.cs`; `PauseManager.cs`, `CameraController.cs`, `SettingsManager.cs`, `SettingsPanelController.cs`, `UIStyleButton.cs`, `UISelectionMarkerUI.cs`, `UIFocusManager.cs`, `HudRestyler.cs`, `SettingsRestyler.cs`; `EnemyData.cs` and the boss ability base.
- **Prefabs / scenes / assets:** `UIMain.unity` and `SettingsPanel.prefab` are the merge risks: no other session should touch them at the same time. New `GameFeelSettings.asset`, `GameSoundTable.asset`, the placeholder vignette.
- **Balance tables / CSVs / sheet tabs:** none. If `EnemyData` turns out to reach a table automatically, stop and add the pipeline work (`CLAUDE.md` "Keep the Google Sheets pipeline in step").
- **Docs:** a "Game feel" section in `CLAUDE.md`.

## Tests
- **EditMode tests to add:**
  - `LowHealthRules`: enter and exit thresholds, intensity at both ends, 0 health, a max health of 0.
  - `PickupPitchChain`: rises, caps, resets after the window.
  - `TimeDilationRules`: lowest scale wins, expiry, pause then resume, reset.
  - Sound table guard: one entry per `GameSound`, no duplicates, sane volume and interval.
  - Settings guard: every moment has a row, every value sane, durations short.
  - Guard that `Time.timeScale` is assigned only in its owner, by scanning `Assets/Scripts`.
  - Style tests: the vignette (stretched, no raycasts, drawn first), the sliders (`SettingsStyleTests`).
- **Manual checks** (go in `todo.md` "Pending manual tests"): each row of the moments table; the warning across a fight, a heal, a Retry and a shop; pause during a hit stop and during the boss kill's slow motion; both sliders at 0; rumble on a real gamepad, then unplugged mid-rumble; the whole thing at high Attack Speed with the Whirlwind, to judge whether it's too much.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
  - Chunk 1, low-health warning: `LowHealthRules`, `LowHealthVignetteUI` in UIMain (placed by Restyle HUD), placeholder art, the heartbeat. `OnGameplayUISetupRequested` now pushes the `LevelData`.
  - Chunk 2, sound: `GameSound` / `GameSoundTable` (one entry each), `SfxVoicePool` (one voice per sound), the XP pitch chain, every hook in the Sound list, enemy `attackSound` and boss `castSound`, Report Silent Sounds. Added after review: per-class voices (`classClips`, `PlayerVoice`), `PlayerDeath` (wins over the prefab's death clip), `CutscenePageTurn` (Next no longer also clicks). The owner added clips and two voice packs.
  - Chunk 3, hit feel: the five moments of the table, `GameFeel.Play`, `TimeDilation` with `PauseManager` as the only `timeScale` writer, Cinemachine impulse shake on level cameras, gamepad rumble with every stop case, Screen shake and Rumble sliders.
- **Not done / follow-ups:**
  - Chunk 3 is untested in play (compile-checked only); the EditMode tests need a run, and Restyle Settings must be run once for the sliders.
  - Silent sounds: whatever Report Silent Sounds lists after the owner's clip pass (the table had clips added by the owner; run the report for the current list).
  - All 736 clips of the sound packs sit under `Resources/`, so all ship in the build. Moving them out of `Resources/` keeps only the referenced ones.
  - Shake strengths are first guesses in world units; tune them in play.
- **Commits / branch:** `feature/game-feel`. Chunk 1: 5310ac8, 36f0c27, b317a9d. Chunk 2: fa08aa0, afc42b9, 316de07, b6727bd, ba95910 (chunks 1-2 merged into `develop` as a fast-forward and pushed). Chunk 3: the commit after ba95910.
- **Gotchas for the next session:**
  - Never write `Time.timeScale` outside `PauseManager`: request a `TimeDilation` instead (`HitFeelTests` fails otherwise).
  - `TimeDilation` clears on every scene load and unload, additive ones included.
  - A `CinemachineImpulseListener` added with `AddComponent` skips `Reset`, so `GameFeel.AttachShakeListener` sets its channel and gain by hand.
  - `Scene` and `Navigation` clash with the `Game.Scene` and `Game.UI.Navigation` namespaces; fully qualify them.
- **Manual tests added to todo.md:** the three "Game feel" sections (chunk 1 low-health warning, chunk 2 sound, chunk 3 hit feel).
