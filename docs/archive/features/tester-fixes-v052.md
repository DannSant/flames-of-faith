# Feature: Tester fixes from v0.5.2 (knockback, Ayteel at the Shrine, sound packs)

- **Status:** done and archived (2026-11-06): merged into `develop` (`30acb5e`, 2026-11-04), manual tests passed.
- **Branch:** `fix/tester-feedback-v052` (off `develop`)
- **Owner session:** implementation session (done)
- **Related docs:** `docs/pm/BACKLOG.md` items 35, 36, 37; `CLAUDE.md` "Melee attack warning", "Run encounter events" (content rules), "Game feel" (sound table), "Balance data" (no dead data, the pipeline rule); `game-design-docs/Lore.md` (Ayteel, Gabien, the Secrets rule, the Glossary)

## Goal
Three small items from the first v0.5.2 tester feedback, plus a cleanup the owner asked for: the sound packs were put under `Resources/` by mistake, so all 736 clips (124 MB) ship in every build.

## Scope
One commit per item, in this order:
1. **Sound packs out of `Resources/`** (BACKLOG 37). First, because it's the biggest and touches no gameplay.
2. **Melee enemies resist knockback while attacking** (BACKLOG 35).
3. **An Ayteel option at the Wayside Shrine of Malien** (BACKLOG 36).

## Out of scope
- Boss health: it's in the balance pass (BACKLOG 5), with the designer.
- Deleting any clip without the owner's OK (see item 1).
- Class options for Faelin and Werlem at the Shrine. They have class lines there and no option either, but testers only flagged Ayteel. List it as a follow-up for the owner to decide.
- The melee attack's art and cone shape (BACKLOG 9).

## Design details

### 1. Sound packs
- **Today:** `Assets/Resources/Audio/SFX/` holds three packs: `Fantasy UI SFX - Lite Edition`, `Voice_Female` and `Voice_Male`. Everything under a `Resources/` folder goes into the build whether or not it's used. Only `GameSoundTable.asset` needs to stay in `Resources/`, because it's loaded by path.
- **Move** the three pack folders to `Assets/Sounds/SFX/Packs/`, next to the existing `Assets/Sounds/SFX/`. Move them with Unity (`AssetDatabase.MoveAsset`, or drag in the Project window), or with `git mv` of each file **and its `.meta`** while the Editor is closed. Either way the GUIDs stay, so `GameSoundTable` and the prefabs keep their references. Don't let Unity create new metas.
- **Check after the move:** `Report Silent Sounds` lists the same entries as before, the sound table has no missing references, and the EditMode suite passes.
- **Unused clips:** write the list of clips nothing references (not the sound table, a prefab, a scene or another asset) to the Handoff, grouped by pack folder, with a count and size. **Don't delete them**: they no longer ship, and the owner may want them for the silent sounds. Delete only if the owner says so in the session.
- **Guard test:** no `AudioClip` under any `Resources/` folder, so it can't happen again.
- **Docs:** update `CLAUDE.md` "Game feel" (where the packs live, and the rule that clips stay out of `Resources/`).

### 2. Knockback during a melee swing
- **Problem** (testers): the Light Devourer can be knocked back in the middle of its swing. The warning ring then lies about where the hit lands, and the swing looks broken.
- **Rule:** while `AIMeleeAttacker` has an attack in progress (from the windup until `OnAnimationEventEnd`, or a stun or freeze, the same span it holds `context.isShooting`), the enemy ignores knockback. Damage, debuffs and hit flashes still land. A knockback that hit **before** the swing started can still play out, but the swing's hold-still must stop the slide.
- **How:** `Knockback` (`Game.Misc`) gets an immunity the behavior turns on and off, released on every exit path, so it can never stick. The spec doesn't fix the shape (a flag, a counter, a predicate); keep it small.
- **No new design field:** don't add a toggle to the melee behavior. `EnemyBehaviors_MeleeAttacker` finds its columns from the asset's fields, so a new field would need the whole Sheets pipeline. If a toggle turns out to be needed, stop and do the pipeline work (`CLAUDE.md` "Keep the Google Sheets pipeline in step").
- **Tests:** an EditMode test that knockback is ignored during an attack and works again after it, including after a stun.

### 3. Ayteel at the Wayside Shrine
- **Today:** `CorruptionCleanEncounter.asset` (id `holyAltar`) gives each pilgrim a class line, and Ayteel's is the strongest: among the names on the stone she finds GABIEN. Its three options (Kneel and pray, leave an offering, take the old offerings) are the same for everyone. Testers say an extra line with no extra option feels off.
- **Add one Ayteel-only option** (`onlyForClass` Warrior) that answers her line. Write it from `Lore.md`. One direction that fits: she stops at her grandfather's name, and the outcome turns on whether she can face it, a check on her current Grace or her Key stat.
- **Content rules** (`CLAUDE.md` "Run encounter events", guarded by `EventEncounterTests`):
  - no numbers in the label or the hint;
  - a first-screen option always risks a cost;
  - a pure gain only in a step reached by a lore-correct answer;
  - every ending outcome is narrated, and every placeholder matches an action of its branch;
  - glossary spellings, no em dash.
- **Lore limits:** in Act I Ayteel does **not** yet learn why Gabien broke the Seal. That is her turning point, later in the Pilgrimage. Respect the Secrets rule. Use existing `OptionActions/` assets where a size fits; add a `Lose…` / `Gain…` asset only if none does.
- **Owner review:** the owner reads and approves the texts before the commit. Put them in the Handoff too.

## Acceptance criteria
- [ ] No audio clip is left under any `Resources/` folder, and a guard test enforces it.
- [ ] Every sound that played before the move still plays, and Report Silent Sounds lists the same entries.
- [ ] The Handoff lists the unused clips; nothing was deleted without the owner's OK.
- [ ] A Light Devourer in its swing isn't moved by knockback. Before and after the swing it is, and it never stays immune after a stun or freeze.
- [ ] Playing the Warrior at the Shrine shows one extra option, and the other pilgrims don't see it.
- [ ] `EventEncounterTests` pass with the new option.
- [ ] The EditMode suite passes.

## Files likely touched
- **Code:** `Knockback.cs`, `AIMeleeAttacker.cs`; a new guard test.
- **Assets:** the three pack folders (moved, metas kept), `CorruptionCleanEncounter.asset`, maybe a new `OptionActions/` asset.
- **Scenes / prefabs:** none expected.
- **Balance tables / CSVs / sheet tabs:** none, unless a melee toggle turns out to be needed (see item 2).
- **Docs:** `CLAUDE.md` "Game feel" and "Melee attack warning".

## Tests
- **EditMode tests to add:** no clips under `Resources/`; knockback immunity during a melee swing (on, off, released after a stun).
- **Manual checks** (go in `todo.md` "Pending manual tests"):
  - sounds still play after the move (UI, player voices, cutscene page turn);
  - hit a Light Devourer with knockback mid-swing (the Warrior's Crusader Charge) and before it;
  - the Shrine with each class, and both outcomes of Ayteel's option.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done** (2026-11-04; EditMode suite 985/985 in batchmode after each item):
  1. **Sound packs:** the three packs are in `Assets/Sounds/SFX/Packs/`, moved with `git mv` together with every `.meta` (1493 renames, GUIDs unchanged); `Resources/Audio/` now holds only `GameSoundTable.asset`. Guard: `AudioLocationTests`. `CLAUDE.md` "Game feel" says where clips live.
     - **Unused clips:** 736 clips in the packs, 25 referenced, **711 unused (118.1 MB)**, found by GUID over every asset, prefab, scene, controller and animation. Nothing deleted. Full list: `docs/features/tester-fixes-v052-unused-clips.md`. By folder:
       - Fantasy UI SFX - Lite Edition: 200 clips, 50.1 MB
       - Voice_Female: 242 clips, 30.6 MB (Attack 19, Breath 31, Cough 12, Effort 32, Expressions 80, Hit 24, Jump&Land 34, Throat_Cleaning 10). All of Pain is used.
       - Voice_Male: 269 clips, 37.3 MB (Attack 36, Breath 28, Cough 13, Effort 27, Expressions 71, Hit 36, Jump&Land 30, Pain 19, Throat_Cleaning 9). No male clip is used yet.
  2. **Knockback:** `Knockback.SetImmune` / `IsImmune`. `AIMeleeAttacker` turns it on when the swing starts and off at `OnAnimationEventEnd` and on a stun or a freeze (`OnStunStateChanged(true)`, which a freeze also calls), the span of `context.isShooting`. Turning it on ends a knockback already playing out, so `AIBehaviorChaserMovement`'s hold-still stops the slide. `ApplyKnockback` also gets its Rigidbody lazily (EditMode tests don't run `Awake`). No new field, so no Sheets work. Tests: `MeleeKnockbackTests`. `CLAUDE.md` "Melee attack warning".
  3. **Ayteel at the Shrine:** a Warrior-only option in `CorruptionCleanEncounter.asset`, approved by the owner. It's a Check on current Grace, Even, using existing actions (`GainGrace3To5` + `GainHealth20To30` on success, `LoseGrace2To5` on failure). `EventEncounterTests.ClassOnlyOptions_AreWhereTheSpecPutsThem` now expects it.
     - **Label:** Lay your hand on his name.
     - **Hint:** The stone is warm for pilgrims. Whether it is warm for an Alamoh is another matter.
     - **Check intro:** Ayteel kneels before the names and reaches for the letters.
     - **Success:** The stone is warm under her palm. Someone knelt here once and cut DAYLA beside his name with care, the way you only carve a name you love. Whatever Abidor says of him, someone did. Ayteel rises steadier: +{grace} Grace and +{health} health.
     - **Failure:** Her hand stops short of the stone. Every door in Abidor that closed on the Alamoh name closes again, one by one, and she can't make herself touch it. She stands, jaw set, and walks on: {grace} Grace.
- **Not done / follow-ups:**
  - Deleting the unused clips: the owner's call. Some may fill the silent sounds first; the male voices are unused because no class has a male voice wired yet.
  - Class options for Faelin and Werlem at the Shrine (out of scope, for the owner to decide).
  - The manual checks below are not run yet.
- **Commits / branch:** `fix/tester-feedback-v052` off `develop`: `4feb6fb` sound packs, `d0992f7` knockback, `1ceb36c` Shrine option, then this Handoff. Not merged.
- **Gotchas for the next session:**
  - Moving assets with the Editor closed is safe only with `git mv` of each file **and** its `.meta`. A new folder needs its own `.meta` (I wrote `Sounds/SFX/Packs.meta` by hand).
  - With the Editor closed, `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults <file>` runs the whole suite in about 15 s after startup.
  - After a stun mid-swing the interrupted animation still finishes (that's existing behavior), but without the hold-still or the immunity, like the shooters.
- **Manual tests added to todo.md:** "Tester fixes v0.5.2": sounds after the move and Report Silent Sounds; knockback mid-swing, before the swing, and after a swing, a stun and a freeze; the Shrine as each class and both of Ayteel's outcomes.
