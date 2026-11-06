# Feature: Demo musts from the owner's list (BACKLOG 38 to 42)

- **Status:** ready
- **Branch:** `feature/demo-musts` (off `develop`, after the Light Devourer merge `e48d21e`)
- **Owner session:** implementation session, not started
- **Related docs:** `docs/pm/BACKLOG.md` items 38 to 42; `CLAUDE.md` "Corrupted outline", "UI standards & revamp" (main menu, class select flow, restylers), "Gamepad & UI navigation", "Class kits, skill trees & Favors", "Class stat guide", "Effect data pipeline", "Balance data" (the pipeline rule), "Two parallel map systems" (Overworld), "Run save / load"; `game-design-docs/Lore.md` §6 and the glossary

## Goal
Five things the owner wants in the Steam demo (December 2026), all on the screens a new player sees first: the class select and the map. The classes get their lore names, the class select explains each class before the player commits to it, and the map shows the chosen pilgrim walking instead of a fixed Warrior picture.

## Scope
Do the parts in this order and commit each one on its own (each commit leaves the EditMode suite green):
1. **Item 38:** Dreko Eye's Corrupted outline fits its sprite in every direction.
2. **Item 40:** rename the classes everywhere, once: Warrior becomes **Crusader**, Archer becomes **Sentinel**, and the internal Mage becomes **Augur**.
3. **Item 41:** class select details get three tabs: **Abilities, Lore, Playstyle**.
4. **Item 39:** with the gamepad, choosing a class moves the selection straight to Start Run.
5. **Item 42:** the pilgrim on the map is the played class, animated, and walks along the path to the next node.

Why this order: 40 renames the methods, assets and texts that 41 and 39 build on, and 39 is easiest to get right once the panel has its tabs.

## Out of scope
- New art. Item 42 uses the current player sprites and animators. The re-designed characters (BACKLOG 6) must drop in later through the class data, with no code change.
- Translating the game. Only the English names change in the game; the Spanish tester guide is a doc.
- Changing what the classes do, their numbers, or the skill trees.
- The legacy `Scripts/Map/` system.
- Pushing to the balance sheet or publishing it without the owner's OK (see part 2).

## Design details

### 1. Dreko Eye's Corrupted outline (item 38)
- **The bug (owner's report):** a Corrupted Dreko Eye's purple outline doesn't match its sprite in some directions. Reproduce it first: force a Corrupted Dreko Eye (and a DrekoEyeAdd) in a level and turn it through all 8 directions.
- **Where to look**, in this order:
  - **Which renderer the outline copies** (`CorruptedVisual.FindMainSpriteRenderer`: the root's, else `CharacterVisual`'s, else the highest sorting order). The Light Devourer merge moved enemy bodies to sorting order 10, so check that the outline still picks the body and not another renderer.
  - **How the flip is copied:** `SpriteOutlineRenderer` copies flip through scale. If the Dreko Eye's animation or `EnemyAnimationController` flips with `flipX` (or the clips key flip or scale) in some directions, the copy goes wrong only there.
  - **The sprite's mesh and pivot:** the sheet must be Full Rect with enough empty margin for the ring (`CLAUDE.md` "Sprite margin"), and the outline child must keep the body's local position.
- **Fix the cause in the shared code,** not with a Dreko Eye special case, unless the cause is the Dreko Eye's own data (its import settings or clips).
- **Test:** add the case to `CorruptionOutlineTests` (for example, the outline follows a `flipX` body, or picks the body renderer on every enemy prefab).

### 2. Rename the classes (item 40)
Owner's decision (2026-11-06): everything, once. Player-facing names: **Crusader**, **Sentinel**, **Augur**. Lore titles: "Crusader of the Faith", "Sentinel of Abidor" (`Lore.md` §3, §6).

**The enum** (`CharacterClass`, `CharacterClassData.cs`):
- Rename the values in place: `Warrior` → `Crusader`, `Archer` → `Sentinel`, `Mage` → `Augur`. **Never reorder them.** Assets store the enum by index (class data, skill trees, events' `onlyForClass` and class lines), so the indexes must stay 1, 2, 3. `Rogue` stays.
- **The meta save stores classes by name** (`MetaState.favors`, `unlockedClasses`, `nodeTiers` in `<player>_meta.dat`). Old files say "Warrior", "Archer", "Mage". Map them on load through one alias table (old name → new value), so a tester's Favors, unlocks and bought tiers survive. The next save writes the new names. Test it with an old-format meta JSON.
- **The run save** stores the class as `selectedPlayerIndex` and items by `effectID`, so it needs nothing. Check it anyway: Continue on a save made before the rename must work.

**Assets** (rename in Unity or move with their `.meta`, so every GUID and reference survives):
- **Class data:** `CharacterData/Warrior.asset` → `Crusader.asset`, `Archer.asset` → `Sentinel.asset`, and their `characterName`.
- **Skill trees:** `WarriorSkillTree` → `CrusaderSkillTree`, `ArcherSkillTree` → `SentinelSkillTree`. Node ids stay as they are: they're saved.
- **Emblems:** `Warrior Emblem` (+1..+3) → `Crusader Emblem`, `Archer Emblem` → `Sentinel Emblem`: the asset name and `effectName`. `effectID` never changes.
  - Their icons: `Icons/WarriorEmblem.png` → `CrusaderEmblem.png`, `ArcherEmblem.png` → `SentinelEmblem.png`.
  - Their behaviors: `WarriorEmblemLevelUp` → `CrusaderEmblemLevelUp`, `ArcherEmblemLevelUp` → `SentinelEmblemLevelUp`. File name and `behaviorId` must stay equal (`EffectAssetTests`).
- **Quiver of the Centinel** → **Quiver of the Sentinel** (+1..+3): the rename `Lore.md` §8 still lists as left. It goes through the same sheet step, so do it here.
- **Player prefabs:** `PlayerWarrior` → `PlayerCrusader`, `PlayerArcher` → `PlayerSentinel`, plus `PlayerWarrior_DashHitbox` and its `DamageSourceData` asset (named after its prefab, `DamageSourceDataTests`).
- **Scripts named after a class:** `WarriorDash` → `CrusaderDash`, `ArcherDash` → `SentinelDash`, and any other. Rename the `.cs` and its `.meta` together, so prefabs keep the script.
- **Folders, animator controllers and sprite folders** with Warrior or Archer in their name (e.g. `Sprites/Player/WarriorHeroIso`): rename them too, keeping GUIDs. The art team's delivery paths change, so list them in the Handoff.
- **Serialized fields** named after a class: add `[FormerlySerializedAs]`, or the value is lost.

**Code and wiring:**
- **Method names wired in UnityEvents:** `MainMenuController.SelectWarrior` / `SelectArcher` / `SelectAugur` are called by the class cards' buttons (`CharacterSelect.prefab`). A persistent listener stores the method's name, so after a rename the button silently does nothing. Rename the methods and rewire them, preferably in code or in `MainMenuRestyler`, not by hand.
- **Hardcoded names:** `SkillTreePanelBuilder` ("Warrior", "Archer", "Augur"), `BalanceReports.ClassData("Warrior")`, `ReserializeDataAssets` (prefab paths), `MainMenuRestyler`'s sample save line, and the tooltips and comments. Grep for `Warrior`, `Archer` and `\bMage\b` in `Assets/Scripts` and the tests. Lore uses of "Warriors" (e.g. the Faraclarian event's text) stay.
- **Rebuild** the skill tree panel prefab and re-run the main menu restyler, if they bake names.

**The balance pipeline** (`CLAUDE.md` "Keep the Google Sheets pipeline in step"). Asset names are the sheet's keys, so the rename changes keys in the Classes, Items, ItemStats, ItemStatBonuses, ItemBehaviors, ItemBehaviors_MainStatOnLevelUp, DamageSources and ItemDrops tabs (the quiver), and the report tabs. Steps:
1. **Before renaming,** Pull from Sheet. It only changes the project and needs no OK. It brings in anything the designer published, so nothing published is lost.
2. Rename, then Export CSV. Update `BalanceDocs` texts that name the classes. `BalanceTablesTests` and `BalanceDocsTests` must pass.
3. **Stop and ask the owner** before Push All to Sheet. It replaces every data tab, so the designer must have no unpublished edits. The dropdowns that list asset names (Icon, behaviors, Damage Sources) may hold the old names: they're sheet edits too, in the same OK.
4. After the push, the owner publishes a new version (Balance > Publish). Test builds with `balance-override.json` read the latest published version by key. Until it has the new names, their import fails and applies nothing. Say so in the Handoff.

**Docs:**
- `CLAUDE.md` (every Warrior / Archer / Mage meaning a class), `game-design-docs/TenPageGDD.md`, `Lore.md` (§6 headings, §8 table, glossary rows).
- The tester guide `PatchNotes/guia-del-tester.md`: Cruzado / Centinela, as in the glossary. Leave its `.docx` / `.pdf` to the Orchestrator.
- Archived docs and old patch notes stay as they were.

### 3. Class select tabs: Abilities, Lore, Playstyle (item 41)
- **Where:** the class select's details column (right side of `CharacterSelect`, `MainMenuRestyler`). Today it shows the name, the description, "Starting weapon" with the weapon and its text, the unlock footer and Skill Tree.
- **Layout:**
  - Above the tabs, unchanged: the class name and its one-line description.
  - Under the tabs, unchanged: the footer (Unlocked / unlock cost) and Skill Tree.
  - The tabs between them, with the Stats pane's tab look (`UISettings.GetTabSprite`, yellow label when selected).
  - Abilities is the default tab. The tab stays selected when the player previews or chooses another class.
  - Build it in `MainMenuRestyler`, not by hand.
- **Abilities tab:** the class's starting kit at its bought tier (`ClassKits.Resolve`): weapon, off-hand, Emblem, and the three Glyphs (primary, special, dash).
  - Each item is a row: its Small icon in the Inventory's slot frame, and its quality-colored name.
  - The full text (`GetDescriptionText` and `GetStatLines`) shows in the shared tooltip, beside the row, on hover or gamepad select, as in the Inventory.
  - If the main menu has no shared tooltip, add one, the way the other scenes have it.
  - The rows replace today's "Starting weapon" block.
- **Lore tab:** a new `lore` text on `CharacterClassData`. It starts with the pilgrim's name and title (e.g. "Ayteel Alamoh, Crusader of the Faith"), then two or three short paragraphs.
  - **The session drafts these texts and the owner reviews them** (owner's decision, 2026-11-06). Draft from `Lore.md` §6 **Who** and **Start** only: the turning points and resolutions are spoilers.
  - Rules: glossary spellings, no em dash (the font lacks it), plain English.
- **Playstyle tab:** a new `playstyle` text on `CharacterClassData` (two or three sentences: range, rhythm, what to watch out for), drafted the same way. Under it, the class's stat guide: the Key stat with the gold star, then the Complementary stats with silver stars (`ClassStatGuide`, `StatRoleStar`), each with its justification.
- **The new texts are not balance data:** Inspector only, not in the Classes tab. Give them `[Tooltip]`s and a `[TextArea]`.
- **Gamepad:** the tabs must be reachable (Navigation not None), and switching works with A on a tab. Follow the Stats pane's tabs if they already work with the gamepad.
  - The ability rows are selectable, so their tooltips can be read.
  - B still goes back.
  - Watch the main menu's known trap: hidden panels stay interactable, so link navigation explicitly where Automatic lands on them (as `ChronicleUI.ChainNavigation` does).
- **Long text** scrolls inside the tab, maskable (see the Stats pane's gotcha), and never pushes Start Run or Back off their spots.

### 4. Gamepad: choosing a class jumps to Start Run (item 39)
- With the gamepad, pressing A on a class card chooses it (as today) and then selects **Start Run**.
  - On a locked class too: Start Run then shows the unlock dialog.
  - With the mouse nothing changes.
- Moving onto a card still only previews it (`CLAUDE.md` "Class select flow").
- Moving the gamepad from Start Run back up to the cards or the tabs must still work.

### 5. The pilgrim on the map (item 42)
- **Today:** the map marker (`OverworldMapRenderer.playerMarker`, a child `sprite` in `LevelSelector.unity`) shows a fixed frame of the Warrior's idle sheet, whatever the class. It jumps to the next node instantly (`MovePlayerMarker`).
- **The class's own animation:** the marker plays the played class's idle and walk, with the class's animator controller.
  - Read it from the class data (`CharacterClassData.animatorController`, which a test keeps equal to the prefab's), pushed by gameplay to the renderer. Don't look the player up from the map renderer.
  - New character art then arrives with the class data.
  - The marker is the same size as the pilgrim in a level, and its feet stand on the node.
- **Walking** (owner's decision, 2026-11-06: walk there, then wait):
  - Picking the next node, with a click or the gamepad navigator, walks the pilgrim along the path to it with the walk animation, facing the direction of travel (the animator's `DirectionX` / `DirectionY` and `IsMoving`).
  - On arrival, the pilgrim idles facing the same way. Entering the level is still a second click or A on the current node, as today.
- **Input while walking:** ignored, both clicks and the gamepad navigator, so the player can't enter a level or start a second move mid-walk.
- **What stays the same:**
  - `MapRunController.TryMoveTo` changes the state and autosaves at once, as today. The walk is only how it looks.
  - Save & Exit mid-walk loads on the new node.
  - The camera already follows the marker, so it follows the walk.
- **No walk:** arriving on the map, an act change and the developer cheats (free travel, go to act) place the pilgrim on the node without walking.
- **The path:** follow the edge as it's drawn (its `LineRenderer` points), not a straight line, if the edges are curved.
- **Tunables:** walk speed, and a cap on walk time so long edges don't drag. Put them on the renderer's Inspector or a settings SO, never in code. Pure math, such as the position along a polyline at a distance and the facing from a direction, goes in a static class with tests.
- **The Corrupted outline** on the map (`CorruptedVisual.SetAnchor`) must follow the animated sprite and its flip. This ties in with part 1.

## Acceptance criteria
- [x] A Corrupted Dreko Eye and DrekoEyeAdd show an outline that fits the sprite in all 8 directions; `CorruptionOutlineTests` cover the cause.
- [x] No class is called Warrior, Archer or Mage in code, assets, prefabs, the game's text or the current docs (archived docs excepted). The enum keeps its indexes.
- [x] An old meta save (Warrior / Archer / Mage names) loads with its Favors, unlocks and bought tiers; a run saved before the rename continues.
- [x] The class cards still choose their class after the method rename.
- [ ] CSVs, `BalanceDocs` and the reports are in step; the sheet was pushed only with the owner's OK, and the Handoff says whether it's published. _(CSVs, docs and reports done; the sheet is **not pushed yet**: waiting for the owner's OK, see Handoff.)_
- [x] Class select shows Abilities (the kit at its bought tier, tooltips), Lore and Playstyle (drafted texts, stat guide), all reachable with mouse and gamepad.
- [x] Choosing a class with A selects Start Run.
- [x] The map shows the played class, idling; picking the next node walks it along the path; entering is a second action; input is ignored mid-walk.
- [x] The EditMode suite passes after each part.

## Files likely touched
- **Code:**
  - `CharacterClassData.cs` (enum, new texts) and `MetaProgressionManager` (aliases);
  - `MainMenuController`, `ClassSelectHoverButton`, `MainMenuRestyler`, `SkillTreePanelBuilder`;
  - `OverworldMapRenderer` and `OverworldGamepadNavigator`;
  - `CorruptedVisual` / `SpriteOutlineRenderer`;
  - the dash scripts;
  - the balance data editor code and the tests.
- **Prefabs / scenes / assets:**
  - `CharacterSelect.prefab`, `MainMenu.unity`, `SkillTreePanel.prefab`, `LevelSelector.unity`;
  - the player prefabs, the Dreko Eye prefabs;
  - `Resources/CharacterData`, `Metaprogression/SkillTrees`, `Effects` (Emblems, quiver, behaviors), `Icons`, `Combat/DamageSources`.
  - No other session is open on these files.
- **Balance tables / CSVs / sheet tabs:** Classes, Items, ItemStats, ItemStatBonuses, ItemBehaviors, ItemBehaviors_MainStatOnLevelUp, DamageSources, ItemDrops, the reports. Keys only, no column changes. The sheet is edited only with the owner's OK.

## Tests
- **EditMode tests to add:**
  - the outline case;
  - the meta save aliases (old names load, new names save);
  - no asset, prefab or script named Warrior / Archer, and the class cards' listeners resolve to existing methods;
  - every class has `lore` and `playstyle` text;
  - the map walk math (position along the path, facing, time cap).
  - Update `MainMenuStyleTests` for the tabs and `LevelSelectorStyleTests` for the marker.
- **Manual checks** (go in `todo.md` "Pending manual tests"):
  - **Outline:** a Corrupted Dreko Eye in every direction.
  - **Continue and meta:** an old save continues, and the Favors and bought tiers are still there.
  - **Class select:**
    - every tab for the three classes, with the mouse and the gamepad;
    - the tooltips;
    - choosing with A lands on Start Run;
    - a locked class.
  - **Map:**
    - each class's pilgrim idles and walks in all directions, and its feet sit on the node;
    - clicks during a walk do nothing;
    - Save & Exit right after a move.
  - **Texts:** the owner reviews the Lore and Playstyle texts.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:** all five parts, in order, each its own commit with the EditMode suite green in batchmode (1032 before, 1062 after). Nothing pushed, nothing merged.
  - **38, outline:** the cause was the outline shader's alpha threshold (0.1): the enemy sheets bake a ground shadow and a soft halo at up to half alpha, so the ring went around them too. On the Dreko Eye that gave a ragged shape off its body, worst from the side. Not a flip or renderer problem: the Dreko Eye's clips only key sprites, and its body is the root renderer the outline picks (order 10 is fine). Fix in the shared code: `SpriteOutlineStyle.alphaThreshold` on `CorruptionSettings` (0.55), pushed to `_AlphaThreshold` per renderer. Checked offline on the Dreko Eye, Skull, Light Devourer and Corruptor sheets: the ring hugs each body in all 8 directions.
  - **40, rename:** everything moved with `AssetDatabase` (GUIDs kept), the enum keeps indexes 1 / 2 / 3, the meta save maps old names (`CharacterClassNames`, `MetaState.MigrateClassNames`). Pull from Sheet first: published version 15 already matched (0 changes). New `BalanceSheetSync.PullBatch [-dryRun]` (no dialogs, never writes the sheet). CSVs re-exported; reports didn't name the classes. Docs: CLAUDE.md, TenPageGDD, Lore.md (section 6 headings, section 8 table, glossary), the tester guide (Cruzado / Centinela).
  - **41, tabs:** `ClassDetailsTabsUI` + `ClassDetailsRowUI`, built by `MainMenuRestyler`; `MainMenuController.BuildDetails` pushes a plain `ClassDetailsView`. The main menu got its own `GeneralTooltipPane` (also added to `TooltipRestyler`'s scenes). Lore / Playstyle texts drafted on `CharacterClassData.lore` / `playstyle` (Inspector only, not in the Classes tab), **for the owner's review**.
  - **39:** A on a card chooses it and selects Start Run (gamepad only).
  - **42, map:** the marker plays the class data's animator, pushed as a `MapPilgrimLook` through `PlayerManager.OnMapPilgrimReady`; it walks along the edge's drawn points (`MapWalk`), input is ignored mid-walk, cheats / act change / arrival place it. Tunables `walkSpeed` 3 and `maxWalkDuration` 1.2 s on `OverworldMapRenderer`.
- **Not done / follow-ups:**
  - **The balance sheet (part 40 step 3) is waiting for the owner's OK.** Its keys still use Warrior / Archer names (Classes, Items, ItemStats, ItemStatBonuses, ItemBehaviors, ItemBehaviors_MainStatOnLevelUp, DamageSources), so a Pull now would fail on the unknown keys. Before Push All, the designer must have no unpublished edits; the dropdowns listing asset names (Icon: CrusaderEmblem, SentinelEmblem, QuiverOfTheSentinel; behaviors: CrusaderEmblemLevelUp, SentinelEmblemLevelUp; Damage Sources: PlayerCrusader_DashHitbox) need the new names too. Then the owner publishes (Balance > Publish). **Not published:** until then, test builds with `balance-override.json` read version 15 with the old keys, so their import fails and applies nothing.
  - Owner review of the Lore and Playstyle drafts (Inspector, `Resources/CharacterData/<class>`).
  - The one-line class descriptions above the tabs weren't touched (out of scope) but are long, multi-line and partly off-lore: the Crusader's says "trained in the Abidor Institute" (the Institute is the academics', Lore.md), and it says the Charge Dash makes the player invulnerable. They take room from the tabs; worth a text pass.
  - The skill tree prefab's four baked class names were renamed in place, because `SkillTreePanelBuilder.Build` asks a dialog that batchmode cancels. The builder already uses the new names, so a rebuild in the Editor gives the same result.
  - Gameplay feel is unverified: the walk speed, the map pilgrim's size and feet, and the class select layout (row heights, how much room the tabs get) need a look in the Editor.
- **Commits / branch:** `feature/demo-musts` off `develop` (`6ee9fc2`): `eae3a75` (38), `8f3f6e9` (40), `98738a9` (41), `5639f14` (39), `85a8a60` + `13ad207` (42, code then its CLAUDE.md note).
- **Gotchas for the next session:**
  - **Renamed art folders** (the art team's delivery paths): `Sprites/Player/WarriorHeroIso` → `CrusaderHeroIso`, `ArcherHeroIso` → `SentinelHeroIso`, `WarriorHero` → `CrusaderHero`, `ArcherHero` → `SentinelHero`, `archer` → `sentinel`; `Animation/PlayerWarriorIso` → `PlayerCrusaderIso` (controller `PlayerCrusaderIsoAnimController`), `ArcherHeroIso` → `SentinelHeroIso` (controller `ArcheAnimController` → `SentinelAnimController`), `ArcherHero` → `SentinelHero`. The sheet files inside are `Crusader…_SS_…` / `Sentinel…_SS_…`, and their sprite names were renamed in the `.meta` (internal ids kept; every clip's sprite keys checked).
  - The scripts `WarriorDash` / `ArcherDash` are now `CrusaderDash` / `SentinelDash` (same `.meta` GUIDs, so the prefabs kept them).
  - `ClassRenameTests` fails on any file or script line naming a former class; the alias table in `CharacterClassData.cs` is the one exception. Lore uses ("Cold Warriors", the Fallen Archer enemy) are in docs, not code.
  - The whole class select is linked by hand now (`ClassSelectNavigation`). A new selectable there needs a link, or the gamepad can't reach it.
  - The map marker's `playerOffset` is 0 now: the marker's position is the feet, and the sprite child is moved by the pushed `feetOffset`.
  - The linked GDD session may also edit `TenPageGDD.md` / `Lore.md`; this branch changed only the class-name lines there.
- **Manual tests added to todo.md:** "Demo musts (`feature/demo-musts`, BACKLOG 38-42)": outline in every direction, the rename (texts, old meta save, old run save, each class plays), class select tabs / tooltips / gamepad / A to Start Run / locked class, the map pilgrim (art, size, walking, input mid-walk, Save & Exit, cheats, Corrupted outline), and the owner's text review.
