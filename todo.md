# Pending manual tests

Only unchecked tests are listed here. Finished sections and checked items moved to `docs/archive/todo-2026-10-23.md`. Backlog items moved to `docs/pm/BACKLOG.md`.

## Unity 6.3 upgrade (2026-10-23)
The project moved from 6000.2.10f1 to 6000.3.13f1 (URP 17.3, Input System 1.19). The unused AI Inference package and the `APP_UI_EDITOR_ONLY` define are gone, and Tools > Sprites > Batch Set Pivot (Isometric) now uses the sprite data provider (the old `spritesheet` API stopped working in 6.3).
- [ ] The Console is clean after a recompile: no errors, and no obsolete warnings outside `Assets/AssetPackages`.
- [ ] Test Runner > EditMode: the whole suite passes.
- [ ] Play a run from Bootstrapper: lighting, the purple Corrupted outline, damage numbers and the loading screen look as before.
- [ ] Gamepad and keyboard: menus, the map and combat respond as before (Input System 1.19).
- [ ] Tools > Effects > Effect Database opens and saves (it uses Newtonsoft.Json, now a direct package).
- [ ] Batch Set Pivot: select a Multiple-mode sheet, run it, and every sprite's pivot is (0.5, 0.4).
- [ ] Make a Windows build: it starts, Continue loads a run saved on 6.2, and the balance override still applies with `balance-override.json`.

## Corruptor: its own sprites and animations (2026-10-22)
The Corruptor used a dark-tinted Skull as a placeholder. It now has its own sheets (Sprites/Enemies/Corruptor), 8-direction Idle / Walk / Attack clips and its own animator (Animation/Enemies/Corruptor), built by Tools > Flames of Faith > Enemies > Build Corruptor Art.
- [ ] Open EnemyCorruptor.prefab: the creature shows (no dark tint), centered on the object, the health bar just above its head.
- [ ] Play to a Corruptor phase: it idles while far, walks while fleeing (facing where it goes), and plays the attack toward the player on each shot; the projectile leaves when the mouth opens.
- [ ] It stops completely for each shot (no sliding while the attack plays), keeps facing the player during it, and starts walking again right when the attack ends.
- [ ] Stun it mid-shot (or wait out the camera focus when it spawns): it moves again afterwards, never stays stuck in place.
- [ ] FlameShooter unchanged: it still shoots while floating after the player.
- [ ] The idle and walk loops no longer hitch on their last frame.
- [ ] Its purple Corrupted outline fits the new sprite and nothing is cut at the sprite's edges.
- [ ] Size feels right (about 1.3x a Skull). If not, change PixelsPerUnit in CorruptorArtBuilder (bigger number = smaller) and run the tool again.
- [ ] Hitbox: the variant's collider (offset y -0.04) still sits on the body; adjust it on the prefab if hits feel off.

## Shop froze the load screen at negative Luck (2026-10-22)
At Luck -2 or -3 (e.g. Cursed Coin -5 + Traveler Coin +2) the shop's item roll threw while the level loaded, so the black loading screen never went away. Negative Luck now makes better items rarer instead of breaking the roll.
- [ ] Get Luck to -3 with DevTools > Stats > Decrease Luck (usable on the map: it logs the new Luck and keeps it for the next level; Free travel gets you to a shop fast), then enter a shop: it loads, and shows 7 items of mixed qualities (mostly Common/Uncommon).
- [ ] Same at Luck -2 (8 items) and at Luck 0 / positive: the shop looks as before.
- [ ] At Luck -7 or lower (e.g. Cursed Coin + 2 Faikan Boots = -7): the shop still shows 3 items (the new minimum, `minItemCount` on the Shopkeeper prefab).
- [ ] Run the EditMode tests: EffectQualityScalerTests and ShopStockTests pass.

## Data-driven pilot, step 5: Google Sheets sync and the test build override (2026-10-23)
Workbook: "Flames of Faith - Balance Data" in Drive/balance-data. Script: BalanceData/Sheets/BalanceSync.gs.
One-time setup (owner):
- [ ] Open the workbook > Extensions > Apps Script. Replace the default code with the whole BalanceSync.gs, save (name the project e.g. "Balance Sync").
- [ ] Deploy > New deployment > type Web app. Execute as: Me. Who has access: Anyone. Deploy, authorize, copy the URL ending in /exec.
- [ ] Reload the workbook: a "Balance" menu appears. Balance > Sync settings (owner): copy the token.
- [ ] Unity: Tools > Flames of Faith > Balance Data > Sheet Settings: paste URL and token, Save, Test connection says "Connected".
Checks:
- [ ] In the sheet, change SlimeChaser Health Base to 55, then Balance > Publish with a note. It says "Published version 1".
- [ ] Unity: Pull from Sheet: the dialog lists "SlimeChaser.Health Base: 50 -> 55"; Apply. The asset shows 55, BalanceData/Enemies.csv and snapshot.json changed, the Report_ tabs in the sheet were refreshed (EnemyHealth for SlimeChaser now starts at 55), the Console says the guard tests passed.
- [ ] Publish again without changing anything: "Nothing changed since version 1".
- [ ] Put a bad value in the sheet (e.g. a negative Contact Damage Tier), Publish, Pull: Unity lists the error with the tab and row and changes nothing. Put it back and Publish.
- [ ] Push All to Sheet (after confirming): the data tabs show the project's numbers again, checkboxes and dropdowns still work.
- [ ] Test build: make a Windows build, Sheet Settings > Save test build config... next to the .exe. Start it: top-left message "Balance data version N by ...: X values differ". Play: SlimeChaser has the sheet's health.
- [ ] Same build with a broken published version: the message lists the errors and says none of it is used; the game plays with the built-in numbers.
- [ ] A build without balance-override.json shows no message at all.
- [ ] Share the workbook with the designer as editor: he can edit white cells, not the grey ones or the Report_ tabs, and Balance > Publish works for him (he authorizes the script once). Sync settings says only the owner can see them.

## Data-driven pilot, steps 3-4: balance data CSV tables (2026-10-22)
The enemies, waves, XP and damage-source numbers export to CSV files in BalanceData/ (repo root) and import back into the assets. Reports (enemy damage, enemy health, XP per level) are in BalanceData/Reports.
- [ ] Open the CSV files (Excel or Google Sheets import): headers are readable, enemy names match between Enemies and WaveEnemies, settings have Notes.
- [ ] Tools > Flames of Faith > Balance Data > Import CSV (Dry Run) right after opening Unity: "The assets already match the CSV files."
- [ ] Change one value in BalanceData/Enemies.csv (e.g. SlimeChaser Health Base 50 -> 55), Import CSV: the dialog lists that one change; Apply; the asset shows 55; the Console says the guard tests passed. Ctrl+Z undoes it (then Export CSV, or put 50 back in the CSV).
- [ ] Put a bad value in a CSV (e.g. "abc" as a health, or an enemy name that doesn't exist): Import lists the error with the file and line, and nothing changes.
- [ ] Optional: add a row "Level1Waves,3,..." to Waves.csv and two rows for it to WaveEnemies.csv, Import: a Wave3 asset is created in Level1's folder and appended to Level1Waves (I tested this in batchmode, then reverted it).
- [ ] Play a level after an import: enemy health and waves behave as the CSV says.

## Main menu: gamepad chose the wrong class (2026-10-22)
Moving onto a class card with the gamepad used to choose it, so going from the Warrior card down to Start Run passed the Augur card and chose it (and a locked Augur blocked the run). Now moving onto a card only previews it; A on a card chooses it.
- [ ] Gamepad, Augur locked: on class select, Warrior is chosen (yellow ring). Move down to Start Run: the details go back to the Warrior when you leave the cards, and A on Start Run starts a Warrior run.
- [ ] Gamepad: moving onto the Archer / Augur card shows its details without moving the ring; A on it moves the ring (chooses it).
- [ ] Gamepad: choose the locked Augur with A, then Start Run: the unlock message names the Augur.
- [ ] Mouse: hovering previews, clicking a card chooses it, Start Run starts the clicked class (unchanged).

## Data-driven pilot, step 2b: one DamageSourceData per damage source (2026-10-22)
The 16 player damage sources (weapon projectiles, item objects, the Warrior's dash hitbox) now take their damage, hit mode, pierce, knockback, debuff, projectile speed and lifetime from an asset in Resources/Combat/DamageSources instead of their prefab. Values are unchanged.
- [ ] Damage numbers are the same as before for each class's attacks and specials: Warrior swing, Whirlwind, Crusader Charge (dash hitbox); Archer arrows, barrage, Strafe arrows (dash), trap; Augur fireball, fireball explosion, fire tornado, flame trail (dash).
- [ ] Items: Swords of Fire (orbiting swords + fire), Holy Missiles, daggers (when hit), Holy Cross, Ether Thunder, Energy Ball / Holy Orb (special), Fire Arrows.
- [ ] Projectiles fly at the same speed and disappear at the same time as before (arrows, fireball, tornado, holy orb bounce, daggers).
- [ ] Split arrows (Archer with Pierce): a split arrow still deals half damage, flies at the arrow's speed, and still counts as Ranged (Ranged Damage applies).
- [ ] Optional: change Base Damage on Resources/Combat/DamageSources/OrbitingSword, play: the sword hits for the new value.

## Data-driven pilot, step 1: cleanup (2026-10-22)
Deleted:
- **Scenes:** the old level scenes, Sandbox2, Boss1.unity.
- **Old enemy prefabs:** including their disabled copies in every scene.
- **Data:** test waves, WarriorOld, Level10/Rest1/Store2 level data, the Boss1 wave database, 2 unused treasure tables, and enemy behaviors nothing used.
Dead fields were removed. Enemy speed is split into moveSpeed (walkers) and hopForce (slimes). Fire Arrows had lost its fire since Oct 18 (behavior file renamed); fixed.
- [ ] Open the project: no missing-prefab / missing-script warnings in the Console.
- [ ] Play from the main menu through a combat level, the Treasure, an event, a shop, the campfire and the boss: all levels load.
- [ ] Fire Arrows (Archer item): arrows set enemies on fire again.
- [ ] The boss still uses both bolt attacks (the "during eyes" bolt asset was renamed).
- [ ] Main menu: panels, buttons and the class select look and work as before.

## Facing a left-behind enemy (2026-10-21)
The weapon's per-shot target is only set when attacking and was never cleared, so with auto-attack on the character kept facing the last enemy attacked, even across the level. Facing (and the sword's orbit) now follow it only while it is alive and within weapon range.
- [ ] Auto-attack on, any class: walk up to an enemy (you turn to it and attack), then walk away. Once it's out of range you face the mouse again (gamepad: the right stick or your movement).
- [ ] Auto-attack off: a manual, aim-assisted attack still faces its target while the attack plays.

## DrekoEye / FlameShooter shots x8 (2026-10-21)
Same cause as the Warrior: their shoot clips are an 8-direction blend tree, so every shot spawned 8 stacked volleys (DrekoEye 8 x 3 = 24 projectiles). The 0.5 s invulnerability after a hixt hid it, but the Whirlwind shield healed 8x per shot. BehaviorController now forwards each animation event once per frame.
- [ ] DrekoEye: each shot is ONE 3-way spread (3 projectiles). Damage per hit is the same as before.

## Cleave retired (2026-10-21)
Removed from the Steeldawn Sword (all tiers; they keep their Melee Damage) in the DB and the generated assets, the cleave code is gone from SwordWeapon, and the stat is marked Retired in StatDatabase (the StatType stays, stats are stored by index).
- [ ] Optional: Tools > Effects > Generate ScriptableObjects, then git status: the 4 Steeldawn Sword assets should not change (they were edited to match the DB). Reload the Effect Database window, it caches rows.

## UI revamp step 8: run encounter windows
- [ ] No event has art yet, so no side panel shows. (To try it: assign any sprite to an event's **Art** field; a framed panel appears to the left of the event window.)

## UI revamp step 6: shared tooltip
- [x ] The frame is slightly see-through (90%). Tune "Tooltip Opacity" in Resources/UI/UISettings.asset.
  - [ ] Fix (play-test 2026-10-21): the frame is now opaque. Hover a Stats row and an Inventory item: nothing of the window behind shows through (the list column keeps its faint strip).

## UI revamp step 5: Item bag
- [ ] Mouse: hovering another item previews it in the tooltip; moving off returns to the selected item. Clicking an item selects it (outline + Sell price move). - mousehover tooltip looks off on mouse and keyboard
  - [ ] Fix (play-test 2026-10-21): when the bag opens, the tooltip sits beside the first item's slot (top of the window), not over Take / Sell. Same with the mouse and with the gamepad, without moving first.
  - [ ] After Take or Sell, the tooltip jumps to the next item's slot, which is now the first cell (no gap where the taken item was).

## Stats pane and Inventory on death (item 3)
- [ ] Hover items and stats: tooltips work (Shift view for stat sources too).
- [ ] Gamepad: focus starts on the death screen buttons; LB/RB reach the Stats pane and the Inventory and back.

## Corruption outline shader (item 5) - needs your eyes most
- [ ] Odd colored pixels at a sprite's border = neighbor frames in the sprite sheet bleeding in (needs padding in the sheet). Tell me if you see it.
- [ ] Tune it in Resources/Combat/CorruptionSettings.asset > Visuals: color, thickness, pulse speed/min alpha, sorting offset (1 = over neighbors, -1 = under). "Corrupted Visual Mode" switches between Outline / Vfx / Both to compare with the old look.
- [ ] Works in a build too (shader included: it's referenced by the material in Resources, so it should be).

## Map cheats (Ctrl+Shift+D on the map, DevTools > Map)
- [ ] On the overworld map, Ctrl+Shift+D opens the cheat window (Developer Cheats on in Settings). Skip/Add item are greyed out there; the map buttons are greyed out inside a level.
- [ ] Map buttons are placed below Add Effect and don't overlap anything (positions: mapButtonsTopY / mapButtonSpacing on the prefab).
- [ ] Reveal full map: every node shows (RNG-blocked paths stay hidden), edges included.
- [ ] Free travel ON: click any revealed node to walk there, click again to enter it (shop, boss...). OFF restores normal rules.
- [ ] Beat current node: the node turns cleared and its next nodes reveal; on a fight it raises difficulty (levels beaten); on the boss it moves to the next act.
- [ ] Go to Act N: the map switches to that act with the marker on its start node.
- [ ] Continue after using the cheats: the save has the revealed/cleared map (autosave is on).
- [ ] Same actions from the DevTools > Map menu in Play mode.

## Sheet explanation panels (2026-10-23)
- [ ] Paste the new `BalanceData/Sheets/BalanceSync.gs` into the sheet's Apps Script, save, Deploy > Manage deployments > edit > New version.
- [ ] Unity: Tools > Flames of Faith > Balance Data > Push Table Notes to Sheet. Every data tab shows, right of the table after one empty column, "About this table" + one note per column.
- [ ] Balance > Publish still works and the published CSVs don't contain the panel (Pull from Sheet reports no errors / "no changes").
- [ ] Push All to Sheet: panels are rewritten, numbers intact, formatting and checkboxes kept.
- [ ] Add a column in a table (or shorten one) and push: the panel moves with the table width, no leftover panel text.
