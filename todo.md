



# Priority 3

## Data-driven analysis 

**Status (2026-10-21):** analysis written in `Assets/Docs/data-driven-analysis.md`. Recommendation: Google Sheets + a Unity pull menu + a balance override in tester builds, piloted on enemies/waves/XP. Waiting on the answers in its section 8 (designer + team) before any code.

- This is not a change but an analysis.
- We need to check all current systems and check the efforts and changes we would need to do to convert them to be fully data-driven.
- Context on why we need this: A new member is joining the team, a professional game designer and he has asked to implement this to be easier to design
- The end goal is, besides the data-driven architecture, is to have a Database that feeds those data objects similar to what we already do with the Effects. However we would need to take the DB outside unity so the game designer can tinker with the numbers, upload them and read them.
- For now we will only do the analysis to see if this approach would be feasible. If not, we will think on something else so he can input the numbers.

The new designer is interested in these particular systems:
- Enemy stats, damage and health, expected damage per wave/level
- Experience drops, experience needed to level up per level
- Wave settings, enemies spawned, frequency and chance weights
- Item drop rate, rules and types of drops (treasure, random pickups and shop) We probably would need to refactor to make it a whole item provider object or singleton
- Stats formulas and calculations
- Items damage and scaling. Example what is the base damage for the item Swords of Fire has 5 base damage and scales with 1 per MeleeDamage and 2 per amount of item copies (not actual values, just and example of what we need per item)
- Something similar for debuffs, how they scale with MastowAffinity and stack in the case of Bleed
- Grace and Corruption modifiers

Which other modules would you suggest we convert to data-driven?

We will dive this change in 2 phases

- Data-driven convertion: All systems that can be converted to data-driven framework will get converted.
- External-system: Create a web app that holds all the information in a supabase DB. Add authentication and give the link to the designers. They will tweak the numbers and press a Submit button. Then, On Unity we can pull the changes with a menu option

Note: My idea on the external system is something I came up with, but it would be nice to research how professional devs approach this issue, and we can change it into that. Basically we need to solve the problem "How does a game designer that works on the project can change the numbers without having to install unity themselves and make the process as automatic as possible"





# Pending manual tests

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

## Data-driven pilot, step 2a: XP curve on ExperienceSettings (2026-10-22)
The XP needed per level (10, 12, 15, ...) is now set on Resources/Progression/ExperienceSettings (Level Curve) instead of the player prefab.
- [ ] A new run: the XP bar starts at 0/10, levels come at the same XP as before (10, 12, 15...).
- [ ] Continue a saved run: the XP bar shows the right maximum for your level.

## Data-driven pilot, step 1: cleanup (2026-10-22)
Deleted:
- **Scenes:** the old level scenes, Sandbox2, Boss1.unity.
- **Old enemy prefabs:** including their disabled copies in every scene.
- **Data:** test waves, WarriorOld, Level10/Rest1/Store2 level data, the Boss1 wave database, 2 unused treasure tables, and enemy behaviors nothing used.

Dead fields were removed. Enemy speed is split into moveSpeed (walkers) and hopForce (slimes). Fire Arrows had lost its fire since Oct 18 (behavior file renamed); fixed.
- [ ] Open the project: no missing-prefab / missing-script warnings in the Console.
- [ ] Play from the main menu through a combat level, the Treasure, an event, a shop, the campfire and the boss: all levels load.
- [ ] Slimes (chaser, explosive, roamer) hop as before; FlameShooter, Light Devourer and the Corruptor walk at the same speed as before.
- [ ] DrekoEye moves at the same pace as before. **New:** Frost now slows it too (its move behavior used to ignore Frost).
- [ ] Fire Arrows (Archer item): arrows set enemies on fire again.
- [ ] The boss still uses both bolt attacks (the "during eyes" bolt asset was renamed).
- [x] Tests: the 2 MainMenuStyleTests failures (layout overrides saved into MainMenu.unity in the v0.5.1 commit) are fixed by re-running Restyle Main Menu (a027162).
- [ ] Main menu: panels, buttons and the class select look and work as before.

## Warrior attack events x8 / Ley Charge strikes (play-test 2026-10-21)
Cause (found with the Ley Charge log): the Warrior's attack is an 8-direction blend tree, and Unity fires the animation events of all 8 clips in the same frame. So every swing ran the cleave 8 times: 8x cleave damage and knockback, and 8 Ley chains from every charged enemy in range. CharacterVisual now forwards each attack event once per frame. (The burn/bleed change was reverted: ticks triggering Ley Charge is intended.)
- [ ] Warrior with a Ley item, 2-4 enemies close: hit a charged one. Each jump draws ONE bolt with ONE Ley number; the chain stops after 3 jumps.
- [ ] Knockback from a swing is one push, not a big shove.
- [ ] The Warrior's normal attack, Whirlwind (special), bleed from Crusader Strike and Retribution still work as before. The swing hitbox still turns on and off with the swing.
- [ ] Burning / bleeding charged enemies still strike on each tick (intended).
- [ ] Ley Charge bolt: drawn once and fades out.
- [ ] Optional: tick "Log Debug" on Resources/Debuffs/LeyChargeDebuff.asset to see every Ley hit, strike and chain stop in the Console.

## Facing a left-behind enemy (2026-10-21)
The weapon's per-shot target is only set when attacking and was never cleared, so with auto-attack on the character kept facing the last enemy attacked, even across the level. Facing (and the sword's orbit) now follow it only while it is alive and within weapon range.
- [ ] Auto-attack on, any class: walk up to an enemy (you turn to it and attack), then walk away. Once it's out of range you face the mouse again (gamepad: the right stick or your movement).
- [ ] Coming back into range turns you to it again.
- [ ] Warrior: the swing still goes toward the enemy in range; out of range it follows the mouse.
- [ ] Auto-attack off: a manual, aim-assisted attack still faces its target while the attack plays.
- [ ] Lock-on (F / LT) on an enemy that then moves out of range: you face the mouse until it's back in range (the lock itself stays, with its marker).

## DrekoEye / FlameShooter shots x8 (2026-10-21)
Same cause as the Warrior: their shoot clips are an 8-direction blend tree, so every shot spawned 8 stacked volleys (DrekoEye 8 x 3 = 24 projectiles). The 0.5 s invulnerability after a hit hid it, but the Whirlwind shield healed 8x per shot. BehaviorController now forwards each animation event once per frame.
- [ ] DrekoEye: each shot is ONE 3-way spread (3 projectiles). Damage per hit is the same as before.
- [ ] FlameShooter: each shot is ONE projectile.
- [ ] Warrior Whirlwind with the holy shield (Glyph of Whirlwind upgraded) vs DrekoEye: the shield heals once per projectile it absorbs, much less than before.
- [ ] Slimes still hop, the Corruptor still shoots, melee enemies still attack (their events go through the same path).

## Cleave retired (2026-10-21)
Removed from the Steeldawn Sword (all tiers; they keep their Melee Damage) in the DB and the generated assets, the cleave code is gone from SwordWeapon, and the stat is marked Retired in StatDatabase (the StatType stays, stats are stored by index).
- [ ] Optional: Tools > Effects > Generate ScriptableObjects, then git status: the 4 Steeldawn Sword assets should not change (they were edited to match the DB). Reload the Effect Database window, it caches rows.
- [ ] Stats pane (gameplay and map): no Cleave row on either tab; the other rows and the class stars are as before.
- [ ] Steeldawn Sword tooltip (Inventory, main menu class details, skill tree): only "+N Melee Damage", no Cleave line.
- [ ] Warrior swing: no extra small damage numbers on the enemies around you; only the sword hit (and Ley / bleed if you have them).
- [ ] Level-up never offers Cleave (it never did).
- [ ] Continue an old saved run: it loads fine (the save still has Cleave 0 in it).

## UI revamp step 13: main menu
- [x ] The menu panel on the right: framed, **Flames of Faith** in yellow, Big buttons. With a saved run, the line under Load Game reads e.g. "Warrior - Act I - Level 7"; without one, Load Game is greyed out and the line says "No saved run".
- [x ] Transitions: New Game, Settings, Back and Settings' Back fade and slide quickly (no ~1 s pause with nothing on screen). Clicking repeatedly during a transition doesn't break anything.
- [ x] New Game with a saved run still asks first; Cancel stays on the menu.
- [ x] Class select: three cards (portrait, name), details on the right. Hovering a card previews it; moving off shows the chosen class again. Clicking a card chooses it (yellow ring) without starting the run. **Start Run** starts with the chosen class.
- [x ] A locked class: the card says "Locked" in red, the details footer shows the unlock cost in red, Start Run shows the unlock message. **Skill Tree** opens the skill tree on that class; after unlocking it and closing the tree, the card and footer turn unlocked (green).
- [ x] Gamepad: the menu panel takes focus (New Game); moving onto a class card chooses it; A on Start Run starts; **B on class select goes back**; Settings opens and B / Back closes it.
- [x ] Skill Tree from the menu panel still works, and Exit quits.

## UI revamp step 12: map HUD
- [ x] On the map, the top-right window shows **Act I - Jesterwoods** (yellow), the name of the level you stand on, your coins (small coin icon) and Save & Exit, framed, lined up with the Stats pane under it.
- [x ] Moving to another node updates the level name right away (combat levels, Campfire, Elven Shop, Holy Altar, Strange Encounter, Treasure, the boss).
- [x ] The coins match the run (after a shop, after a fight).
- [x ] **Stats pane bug:** the Stats pane shows on the map every time (new run, after a level, Continue from the main menu, Retry), with the run's real values; hovering a row shows its tooltip.
- [ x] Gamepad on the map: the bottom-left window shows Move / Enter level / Menu / Stats. Y enters the UI (Save & Exit selected) and the window switches to Swap windows / Back; LB/RB moves to the Stats pane (rows selectable, tabs work); B goes back, and the stick moves the player again.
- [ x] Keyboard/mouse: the prompts window is hidden; the Stats pane and Save & Exit work with the mouse.
- [x ] Save & Exit still saves and goes to the main menu.
- [ x] A very wide Game view (21:9): the Stats pane and the tooltip are no longer cut off.

## UI revamp step 11: skill tree colors
- [ x] Main menu → skill tree: the "Favors: N" counter (top right) is in the standard Favors gold.
- [ x] Node costs: green when you have enough Favors for the upgrade, red when you don't; "MAX" stays green.
- [x ] The details panel's bottom line: green for "Unlocked." / "Unlock: N Favors ... Click to unlock." / "Upgrade: N Favors. Click to buy."; red for "Not enough Favors" and for "Unlock the class to upgrade its items." Buying or unlocking turns the colors right after the refresh.
- [ x] The selected class tab's name is yellow (like the Stats pane's selected tab); the other tabs are white; a locked class's tab is grey ("(locked)") unless it's the selected one, which is yellow.
- [ x] Switching tabs moves the yellow to the new tab. Buying an upgrade or unlocking a class keeps the colors right after the refresh.

## UI revamp step 10: shop
- [ x] Walk into the shopkeeper: the shop opens as a center window (**Shop** in yellow) between the Inventory and the Stats pane, all three lined up.
- [ x] The items use the Inventory's slots (frame colored by quality) with a coin and the price under each; all items fit without scrolling (try with high Luck: up to 20 items, 4 rows).
- [ x] Items you can't afford are dimmed with a red price. Clicking one shows "Not enough Golden Chronos" (red, just above Continue) and asks nothing.
- [ x] Hovering an item (or moving onto it with the gamepad) shows the shared tooltip **beside the slot**: name in its quality color, description, stat lines on the right, and "Price: N coins" (red "... - Not enough coins" when unaffordable). It disappears when moving off and when leaving the shop.
- [ x] Buying: click → "Buy X for N coins?" → OK: the item goes to the Inventory, coins drop, the grid refreshes, and items that became unaffordable turn dim.
- [ x] Gamepad: the first item is selected when the shop opens; the d-pad moves around the grid; A buys (with the confirm); Continue is reachable; B / Cancel behave as before.
- [x ] Continue → "Are you sure you want to leave?" → OK leaves to the map; Cancel stays.
- [x ] The old "(A) Buy" prompt at the top and the old description box are gone.

## UI revamp step 9: wave results
- [ x] End a wave with some Corruption. When the window opens, the HUD still shows the **old** Grace and "Corruption -N" (it no longer jumps to the new value as the window appears).
- [x ] The window is a center window like the others: **Wave Results** yellow at the top; the source rows small, label on the left and value on the right (Corruption values "-N" purple, Grace Affinity "+N" cyan); the formula and Favors big.
- [ x] The Corruption total flies from the HUD "Corruption -N" label into the "Corruption this wave" row, and the HUD label drops to 0 as it leaves.
- [ x] "Grace +N" flies from the HUD into the formula; the formula counts to the new Grace (about 2 s) while the window's Grace bar moves; the result flies back to the HUD Grace label, and only then does the HUD bar move.
- [ x] Ending with Grace above 0: "Favor earned +1" in gold. Ending below 0: the result and the bar are purple, no Favors row.
- [ x] The button reads **Skip** while it plays: clicking it (or A on gamepad) jumps straight to the final numbers (HUD included, no numbers left flying) and the button turns into **Continue**; a second press closes the window and the run continues (item bag / level-up).
- [x ] Gamepad: the button is selected as soon as the window opens.
- [x ] A wave with no Corruption and no Grace Affinity still skips the window entirely, and the HUD is normal afterwards.
- [x ] Corruptor killed ("Corruptor killed in X.Xs") and Corruptor escaped ("Corruptor escaped") rows both read right.
- [ x] (fixed) A wave with no Corrupted damage taken and Grace Affinity 0 shows **no** "Corrupted damage taken" and **no** "Grace Affinity" rows (they used to show sample values "40 / -3" and "+1"). Check with both Continue at the end and Skip in the middle.

## UI revamp step 8: run encounter windows
- [ x] Campfire: walk into the fire. A center window opens: **Campfire** in yellow, "You feel rested. You gained 5 Grace and 10 health." (Grace cyan, health green), no gem icon, Continue at the bottom (same spot as the other windows). The Stats pane and the Inventory open beside it.
- [ x] Campfire Continue (mouse and gamepad A) goes back to the map, and the Stats pane / Inventory don't stay open there.
- [ x] Random event: the event window opens centered: yellow title, the text in small font, the options as full-width buttons at the bottom. A long option wraps to two lines and its button grows; nothing overlaps the text.
- [ x] Event options with the gamepad: the first option is selected; up/down moves between options; the option's tooltip appears beside the button (not at the top of the screen). Same with mouse hover.
- [ x] Pick an option: the Results window opens: **Results** in yellow, one line per result, "Gained…" lines green, "Lost…" lines red, "Found item…" white. Continue → item bag (if an item was found) or level-up / map.
- [ x] Holy Altar: same event window and Results flow.
- [x ] Treasure: walk into the chest. There's no treasure window anymore: the **item bag** opens with the chest's item (Take / Sell). Take adds it to the Inventory, Sell gives coins; then the run continues normally.
- [ ] No event has art yet, so no side panel shows. (To try it: assign any sprite to an event's **Art** field; a framed panel appears to the left of the event window.)

## UI revamp step 7: confirm dialog and end screen
- [x ] Pause → Save & Exit: a small framed dialog (message, OK | Cancel side by side) appears over the pause menu, and the whole screen behind it is dimmed. Text and buttons use the small size.
- [x ] With the dialog open, clicking Resume, Settings, the Inventory or the Stats pane behind it does nothing (the backdrop blocks clicks). OK exits to the menu; Cancel closes the dialog and the pause menu is usable again.
- [ x] Gamepad: the dialog takes focus with Cancel selected; left/right moves between OK and Cancel; B cancels; Esc/Start also closes it (not the pause menu).
- [ x] Main menu → New Game with a saved run: the overwrite warning uses the same dialog, centered on screen, and both buttons work.
- [x ] Main menu → pick a locked class (with unlockAllClasses off): the message shows **only OK**, centered; the gamepad lands on OK; OK and B close it.
- [ x] Skill tree: buying a tier and unlocking a class ask through the dialog; after closing it the gamepad returns to the node that asked.
- [ x] Shop: buying an item and Continue (leave) ask through the dialog, which is centered on the shop window and dims the screen. Confirm and Cancel both work.
- [x ] Beat the boss (or the last level): after the death animation, the end screen opens in the center: **VICTORY** big and yellow, "Thanks for playing the demo!", Main Menu at the bottom (same spot as Retry on the death screen). The Stats pane and the Inventory open beside it.
- [ x] Gamepad on the end screen: Main Menu is selected; LB/RB reach the Stats pane and the Inventory; Main Menu loads the main menu.
- [x ] On the end screen, Esc/Start does nothing (the pause menu doesn't open over it).
- [x ] After Main Menu from the end screen, the main menu isn't frozen (buttons animate, a new run starts normally).

## UI revamp step 6: shared tooltip
- [ x] Hover an Inventory item: the tooltip appears right beside the slot (not at the top of the screen). Title big and bold in the quality color; description and ability lines on the left; the item's stat lines (+5 Health...) in the right column. Long descriptions grow the tooltip down instead of spilling out.
- [x ] Same for the item bag (the tooltip sits beside the selected slot and stays there while you hover others), treasure rewards, and items in the shop's side inventory.
- [ x] Stats pane: hovering a row shows the tooltip to the LEFT of the pane (no room on the right): description, then the yellow benefit line under it. Hold Shift (LT): the list of items granting the stat replaces the benefit line. Hover the bottom rows: the tooltip stays on screen and never flickers.
- [x ] Upgrade choices: description, then the yellow Now/After under it.
- [ x] Short tooltips (Grace and Corruption labels in the pause menu, map Taint, dash icon, event options): small panes, no empty column on the right.
- [ x] Gamepad: the tooltip sits beside the selected element and moves with the selection.
- [x ] The frame is slightly see-through (90%). Tune "Tooltip Opacity" in Resources/UI/UISettings.asset.
  - [ ] Fix (play-test 2026-10-21): the frame is now opaque. Hover a Stats row and an Inventory item: nothing of the window behind shows through (the list column keeps its faint strip).
- [ x] Pause / unpause, closing windows and starting the next wave still hide it; it never sticks on screen.
- [ x] On the overworld map, the Taint tooltip appears beside the node.
- [x ] Your proposal prefab Prefabs/UI/Misc/NewTooltipPanel.prefab is untracked and no longer needed; delete it when you're happy.

## UI revamp step 5: Item bag
- [x ] Pick up items during a wave (random item pickups, or an event's "random item"): at wave end the item bag opens in the center spot, same height as the Inventory (now also open on the left) and Stats (right).
- [ x] "Items found" big and bold, "Take or sell each item" small grey. Items are 50x50 slots like the Inventory, frame tinted by quality, 6 per row.
- [ x] The first item is selected: yellow outline, and the shared tooltip (top center) shows its quality-colored name and full description. "Sell (N)" shows its price.
- [ ] Mouse: hovering another item previews it in the tooltip; moving off returns to the selected item. Clicking an item selects it (outline + Sell price move). - mousehover tooltip looks off on mouse and keyboard
  - [ ] Fix (play-test 2026-10-21): when the bag opens, the tooltip sits beside the first item's slot (top of the window), not over Take / Sell. Same with the mouse and with the gamepad, without moving first.
  - [ ] After Take or Sell, the tooltip jumps to the next item's slot, which is now the first cell (no gap where the taken item was).
- [ x] Gamepad: moving onto an item selects it right away (no need to press A); Sell / Take reachable below.
- [x ] Take: the item goes to the Inventory (it appears there), the next item gets selected. Sell: coins go up by the price. After the last item the window closes and the level-up window opens as before.
- [ x] Take and Sell are small buttons, Take at the same spot as Continue/Resume/Retry, Sell just above.
- [ x] The tooltip never stays on screen after the bag closes.
- [x ] Noticed in passing (not changed): the shared tooltip pane has a fixed size, so long item descriptions spill below it. It needs its own restyle step.

## UI revamp step 4: Stats pane
- [x ] At a wave end, pause, shop, death and campfire: the Stats pane is on the right, same size and height as the Inventory; Inventory, the center window and Stats line up top and bottom.
- [x ] Title "Stats" (big, bold). Main / Secondary tabs look like the skill tree tabs; the selected one has the lighter tab and a yellow label. Switching tabs works with mouse and gamepad.
- [ x] Rows: full-width dark bands, star on Key/Complementary stats, name white, value green (positive) / red (negative) / white (0). The name no longer turns green or red.
- [x ] Mouse wheel scrolls the list (Warrior has 15 rows, Archer/Augur 16 on Main); rows past the bottom are hidden under the list's edge, never drawn over the legend or the frame. The thin scrollbar on the right appears only when the list doesn't fit (Secondary tab: no scrollbar).
- [x ] Switching tabs or reopening the pane starts the list at the top.
- [x ] Gamepad: LB/RB to the Stats pane, move down: the list scrolls to keep the selected row fully visible; the selection frame wraps the whole row; moving back up scrolls back. The gamepad never selects the scrollbar.
- [x ] Hover a row: it brightens and the tooltip shows; Shift (LT on gamepad) still shows the items granting the stat, and the Inventory still dims the others.
- [ x] Legend at the bottom: small text with the Shift (keyboard) / LT (gamepad) glyph at 28x28.

## UI revamp step 3: level-up, pause, death screen, Settings, colors
- [ x] Level-up (wave end): centered window, same height and top/bottom as the Inventory. "Select an upgrade" big and bold, "You are now Level N" yellow (counts up per pick), "Coins gained: N" green with the coin. 4 small choice buttons; the star still shows for Key/Complementary stats and the tooltip's Now/After preview is yellow. Continue at the bottom center.
- [x ] Gain no level: only coins + Continue show (no title).
- [x ] Pause (Esc/Start): yellow PAUSE, buttons from the bottom: Resume, Save & Exit, Settings. Resume is in the same spot as Continue. Save & Exit still asks first.
- [x ] Pause > Settings: the Settings window opens in the middle; Inventory and Stats hide. Change volume, auto-attack, lock-on mode and tutorial hints: each applies right away. Back, B or Esc returns to the pause menu (Inventory and Stats come back); then Resume works.
- [ x] Tutorial hints OFF in Settings: the wave hint under HealthPane disappears (and comes back when ON).
- [ x] Main menu > Settings: slides in fully on screen (right side) with the new two-column layout; Back closes it as before.
- [x ] Dropdowns (Resolution, Graphic Quality, Texts): the open list is readable (pixel font, white text) and selecting works, with mouse and gamepad.
- [ x] Death screen: red YOU DIED (a bit smaller than before), Retry at the bottom (same spot as Continue/Resume), Exit above it; Stats and Inventory still beside it.
- [ x] Gamepad on each window: focus starts on the buttons, the selection frame wraps them, LB/RB still reaches Stats/Inventory from pause and death. The window frames no longer fade when focus moves to Stats/Inventory.
- [ x] Colors: auto-attack OFF is red; Stats pane values green (positive) / red (negative); Stats pane selected tab yellow; shop "Not enough Golden Chronos" red; skill tree: MAX green, "next tier" header and owned pips yellow; end-of-demo title yellow.
- [ x] Lock-on: on a fresh install (no saved settings) lock-on is Toggle mode (fixed: it defaulted to Hold).

## UI revamp step 2: Inventory (+ HUD positions fix)
- [x ] HUD: IconsPane bottom-left, ExperiencePane top-right and the boss flame bar bottom-center now sit where the restyle designed them (15 from the edges). Your manual ExperiencePane nudge from 056d8f1 was replaced; nudge it again in HudRestyler if you want it elsewhere.
- [ x] Open the inventory at a wave end, in the shop, from pause and on the death screen: framed window on the left, clear of HealthPane/hint above and IconsPane below.
- [x ] "Inventory" title is big and bold; coins are small with the coin icon on the title row, right side. Buying in the shop updates the coins.
- [x ] Slots: 8 per row, 50x50, frame tinted by item quality (grey common, white uncommon, green rare, blue heroic, purple epic, gold legendary). The count badge shows only for stacks (2+).
- [ x] The coin icon may look small inside its 28x28 box (the sprite has empty margin); OK?
- [x ] Hover an item: the shared tooltip (top center) shows the quality-colored name and the full description (stats, ability tier lines). Mouse out hides it.
- [ x] Tooltip never gets stuck: buy an item in the shop while hovering one, close the inventory while hovering, start the next wave, pause/unpause.
- [ x] Hover a stat in the Stats pane while the inventory is open: the stat tooltip isn't closed by the inventory. Shift + hover a stat still dims items that don't grant it.
- [ x] Gamepad: LB/RB reaches the inventory, the selection frame wraps a slot, navigation moves across the 8 columns, and the tooltip follows the selection.
- [ x] Fill it up (DevTools add items): up to 80 different items fit without overflow.
- [x ] Treasure encounter: item tooltips/reward text still show the quality-colored name.
- [x ] Noticed in passing (not changed): with the gamepad, the "LB/RB Swap Windows" hint panel at the top right covers the XP bar. Fix with that window's step?

## UI revamp step 1: foundation + HUD
Game view at 1920x1080 first, then 1280x720 and 2560x1440 (or Free Aspect resized).
- [ x] The whole UI (main menu, map, HUD, every window) scales with the window size instead of staying the same pixel size. At 1080p it looks the same as before (except the HUD).
- [x ] Other windows aren't restyled yet: check that none got cut off or overlaps at 720p (they scale down now).
- [ x] HUD top left: HealthPane framed (ornate corners), 15 from the edges. HP bar with "87/100", Grace bar under it, "Grace +N" left, "Corruption -N" right, "Wave N" and the timer on the right.
- [ x] Grace label is cyan at 0 and above, purple below 0. The Grace fill is cyan (grows right), the Corruption fill purple (grows left), split exactly in the middle at every resolution.
- [x ] "Corruption -N" counts up as you take Corrupted Damage and is hidden on the map.
- [ x] Wave timer turns purple during the Corruptor phase and back to white after.
- [x ] Wave hint: framed, right under HealthPane, same width, small text, purple for the Corruptor hint. Hidden when hints are off in Settings (and takes no room).
- [ x] HUD bottom left: IconsPane framed, three 80x80 ability icons. Cooldown sweeps cover each icon exactly. Input glyphs sit centered on each icon's bottom edge and swap between keyboard and gamepad.
- [x ] Dash charges number shows top-right of the dash icon (classes with 2+ charges).
- [ x] Auto-attack and Lock-On rows to the right of the icons, each with its glyph. Toggling auto-attack (Ctrl/RT) switches the label ON/OFF (OFF in orange).
- [ x] Dash icon tooltip still shows on hover.
- [x ] HUD top right: "Level N" shows the right level as soon as a level starts (it used to say "Level 1" until the first level-up), XP bar fills. After a level-up the bar fills toward the new level's requirement.
- [x ] Boss level: flame bar at the bottom center, orange fill in the same frame style, doesn't overlap IconsPane.
- [ x] Pause with gamepad: LB/RB reaches the Grace/Corruption labels, the tooltips show, and the selection frame wraps the label.
- [x ] Gamepad selection frame (all windows): a pulsing bracket frame wraps the selected element instead of the small arrow. Check the main menu, skill tree, pause menu, upgrade panel, shop, death screen and the map's Exit button / cheat window (the map had no marker before). It slides between elements and is never drawn under a window.
- [x ] Too much padding or too tight anywhere? Tune "Selection Padding" (and color, pulse) in Resources/UI/UISettings.asset.
- [x ] Text readability at 12 (small): if it's too small, change Small Font Size in UISettings and run Tools > Flames of Faith > UI Revamp > Re-apply UI Styles.
- [ x] Retest (cooldown fix): the grey cooldown sweep now covers each 80x80 icon edge to edge (square corners, no inset).
- [x ] Retest (cooldown fix): when a cooldown ends (attack, special, dash) the sweep disappears completely, no thin line left. Dash with several charges: the sweep only shows once all charges are spent, and clears when one comes back.
- [ x] Wide key glyphs (SPACE, CTRL) keep their shape now but are small inside the 28x28 prompt box. OK, or should they get a wider box?

## XP fix (commit cc821d7)
- [x ] Gain several levels from one big XP drop (e.g. the last-wave gem sweep): each level now costs its own requirement (10, 12, 15...), so you may get one level less than before in those cases. The upgrade panel still offers one pick per level gained.

## Warrior dash (Crusader Charge) - commit ba8d117
- [x ] Charge into enemies from every direction (left, right, up, down, diagonals): every charge that touches an enemy deals damage.
- [x] Charge into a group: every enemy touched takes a hit, and the knockback still feels right.
- [x ] Charge into the boss: it takes damage.
- [x ] Damage feels right with Melee Damage: at 0 Melee Damage the dash deals its tier's base (3/4/6/10); with e.g. 10 Melee Damage it should deal about base + 15. Compare to the sword swing (10 + Melee Damage).
- [ x] Tooltip of the Crusader Charge scroll says "plus 150% of Melee Damage".
- [x ] Dash damage numbers now use the melee color.
- [X ] With a Melee elemental infusion item (e.g. a melee bleed/burn item), dash hits now also apply that debuff. Decide if that's wanted.
- [ x] The hitbox now covers the feet area instead of the tall area above them: check that brushing past an enemy doesn't feel like it hits from too far away.

## Upgrade panel level label (item 8)
- [ X] Gain 2+ levels in one wave (e.g. 1 -> 3): the first pick says "You are now Level 2, pick an upgrade", after picking it says "Level 3".
- [ X] Gain exactly 1 level: the label shows that level.
- [ X] Gain no level: only the Continue button shows, with no title.
- [ X] After the last pick (Continue button), the title is hidden.
- [ X] The label fits on one line (or wraps cleanly to two) and doesn't overlap the choice buttons, at your usual resolution and at a smaller one.
- [ X] Works with gamepad too (the title doesn't steal focus).

## Scroll -> Glyph rename (item 1)
- [ X] Open the Effect Database window: the 44 items read "Glyph of ...", and no "Scroll of ..." row is left.
- [ X] Start a run with each class: the kit's 3 glyphs show as "Glyph of ..." in the item bag, stats pane and tooltips.
- [ X] Skill tree screen: every node still shows its items (the references survived the rename) and upgrades still work.
- [ X] Continue an old save made before the rename: it loads and keeps its items (saves use effect IDs, so it should).
- [ X] Shop / treasure: the shop glyphs (Strafe, Holy Orb, Ley Fracture...) show the new name.
- [ X] DevTools > Items menu shows "Glyph of ..." entries and they still give the item.

## Buy confirmation in the shop (item 4)
- [ x] Click an item you can afford: the dialog asks "Buy <item> for <price> coins?" (item name in its quality color, discounted price). Yes buys it, No/Cancel doesn't.
- [ x] Click an item you can't afford: no dialog, the "not enough coins" error shows right away.
- [ x] With gamepad: Submit on an item opens the dialog with focus on it; B cancels; after closing, focus returns to the shop items.
- [ x] Clicking another item while the dialog is open does nothing.
- [ x] The leave-shop confirmation still works as before.
- [x ] Quality colors in the dialog are readable on its background (Common/white especially).

## Wave results window (items 6 and 7)
- [ x] Keyboard/mouse: the prompt reads "Click [mouse icon] to continue"; a left click closes the window (Enter still works too).
- [ x] Click once during the counting animation: it skips to the result but does NOT close the window; a second click closes it.
- [x ] Gamepad: still "Press [A] to continue", A closes it. Switch device mid-summary: the word and icon swap.
- [ x] "Click" + icon + "to continue" line up nicely (the word is a bit wider than "Press").
- [ x] Favors row is blue (#73BFFF). If it should match the Grace bar exactly, set "Favors Color" on the prefab.
- [ x] Final Grace line and Favors line are bigger (48 vs 36) and nothing overlaps or gets cut, at your usual and a smaller resolution.



## Stats pane and Inventory on death (item 3)
- [x ] Die in a level: the death screen opens with the Stats pane and the Inventory beside it (same places as at wave end), showing the build that died.
- [ x] The Inventory is drawn below the death screen in the hierarchy: check nothing important is covered by the death box.
- [ ] Hover items and stats: tooltips work (Shift view for stat sources too).
- [ ] Gamepad: focus starts on the death screen buttons; LB/RB reach the Stats pane and the Inventory and back.
- [ x] Retry: the map shows without the Stats pane / Inventory left open. Same for Exit to main menu.
- [x ] Die in the boss level and in an event (event damage): same behavior.

## Class stat guide: sorting + stars (item 10)
- [ X ] Stats pane, each class: the Key stat (gold star) is first, then the Complementary stats (silver star), then the rest. Archer's Pierce and Augur's Skill Duration now show on the first tab (and no longer on the second).
- [x ] Play two runs with different classes in a row (Retry or back to menu): the order and stars follow the new class.
- [ x] Hover a starred stat: the tooltip ends with "Key/Complementary stat for the <class>: <why>" in gold/silver. Read the justifications and rewrite any you don't like (CharacterData/<Class>.asset, "Stat Guide").
- [ x ] Stat names now start a bit to the right to make room for the star: check long names ("Experience to level up", "Mastow Affinity", "Healing Received") don't wrap badly.
- [ x ] Upgrade panel: choices for class stats show the star at the left of the button, and the tooltip has the same line.
- [ x ] The generated star (16x16 pixel art) looks fine; replace Sprites/UI/StatRoleStar.png with your own art anytime (keep the .meta).
- [ x ] Gamepad: navigating the stat rows still works after the reorder.

## Run encounter rework (item 9)
- [ X] Play each event at least once (random pool: Vega Casket, Gracoy Raiders, Weeping Chest, Faraclarian Clan, House of Stars; plus the Holy Altar node). Read the new texts; edit any you don't like in Resources/RunEncounters/EventEncounter/<Event>Encounter.asset.
- [ X] No event offers "Ignore" anymore; each option's tooltip shows its costs/rewards correctly.
- [ X] Every option feels like a real choice. Draft numbers:
  - Vega Casket: lever = arcane item, -2..-5 Grace (bug fixed: it used to GIVE +3) | smash = +30..60 coins, -5..15 HP
  - Gracoy Raiders: fight = item, -15..25 HP | pay = -15..30 coins, +1..3 Grace | flee = -5..10 HP
  - Holy Altar: pray = +3..5 Grace | offering = -25..40 coins, +8..12 Grace | plunder = item, -5..8 Grace
  - Weeping Chest: coins = +50..100, -5 Grace | reach beneath = arcane item, -3..6 Grace, -5..10 HP | seal = +2..3 Grace
  - Faraclarian: claim Forger = -10..30 HP, item | lie = +40..60 coins, -5 Grace
  - House of Stars: accept = -20..30 HP, +1 Favor | refuse = -30..50 coins, +2..5 Grace | fight = -10..40 HP, item
- [ X] Paying with fewer coins than the cost: check what happens (wallet clamps at 0?) and if that's OK.
- [ X] Health losses are non-lethal (min 1 HP) and reduced by armor; check the result screen numbers.
- [ X] Generic background: every event now shows the holy-altar art. Change "Generic Background" on Prefabs/UI/RunEncounters/EventEncounterPane.prefab if you prefer another (or new art).
- [X ] The +1 Favor from the House of Stars is kept only if you finish the encounter (normal Favor rules).

## Corruption outline shader (item 5) - needs your eyes most
- [ x ] Corrupted enemies show a pulsing purple outline instead of the VFX. Many corrupted enemies on screen: is it readable and not noisy?
- [ x ] The outline hugs the sprite correctly while the enemy animates and flips left/right (no offset, no lag).
- [ x ] If the ring looks cut off at some edges: the sprite's mesh has no empty margin there. Fix in the sprite's import settings: Mesh Type = Full Rect, or Extrude Edges >= the outline thickness.
- [ ] Odd colored pixels at a sprite's border = neighbor frames in the sprite sheet bleeding in (needs padding in the sheet). Tell me if you see it.
- [ x ] Player: becomes outlined when Grace drops below 0, and the outline goes away above 0. On the overworld map, the marker gets the outline.
- [ x ] Boss (corrupted by default): outlined; Corruptor too.
- [x ] Death / fade-outs: the outline fades with the sprite (it follows the sprite's alpha) and disappears with it.
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

# Done

## Priority 1 (2026-10-17, committed on main, not pushed)
Commits: Glyph rename 7f601d3 + typos 64dbbda, Warrior dash ba8d117, death screen 1ec65f6, shop confirm f0bb991, corruption outline d0e4e22, wave results aa3789e, upgrade level label a6588e9, run encounters 9ce3b59, item 10 class stat guide 9c66616.

- Change names of the skill items, now they all start with Scroll of... they should be renamed to Glyph of...
- WarriorDash, check an issue that is causing that sometimes this abilities does not inflict any damage, also check how much damage it scales with MeleeDamage stat and increase it by 50%
- When the player dies, also show the StatsPane and Inventory 
- Add a confirmation dialog before buying an item. Re use the prefab we have that asks for confirmation before leaving the store
- Create a shader for the corruption effect. Right now the corruption effect on enemiesd and the player attaches a VFX. When too many corrupted enemies are on screen this gets noisy. We should investigate if is is possible to create a 2D shader that adds a purple outline to the sprites and a way to enable and disable it on runtime
- On the Wave results window, change the "Press enter to continue" to "Click to continue" using the mouse icon too when using mouse and keyboard. Gamepad stays the same
- Also on the Wave results window change the color font of the Favors gained to a different than golden. I think blue would be good, similar to the Grace color. And increase the size of the fonts of the text that shows the final Grace and the earned favors
- The Upgrade select panel should show on the top the level that you are getting the upgrade from. Example if you advance to level 3 in the first wave, when the upgrade panel comes up for the first upgrade the label should mention "You are now Level 2, pick an upgrade" and after clicking the desired stat show "You are now Level 3, pick an upgrade"
- Balance and improve the run encounter events. 
  - Either remove the Ignore option for those or add a penalty for ignoring the events.
  - Check the options of the run encounters, some events have a great options for the player, making the rest of the option useless. 
  - Remove the background for all encounters. Add a generic background for all
  - Re-write the descriptions, make them more fantasy-like
  - Add more events:
    - Encounter 1 - A Faraclarian clan has found you. They say they are looking for a Forger, but you don't know what that is. What will you do? 
      - Option 1 - Say you are the Forger - They get angry at you for lying and fight you. You get 10-30 damage but on the chaos you gain a random item.
      - Option 2 - Say you saw a Forger - They give you 40-60 coins for helping them, but you lose -5 Grace because Malien gets dissapointed for lying
    - Encounter 2 - A group of Elves from the House of Stars offer you a gift.
      - Option 1 - Accept it - It was a trap, you lose 20-30 health, but Malien favors your sacrifice and you earn +1 favor
      - Option 2 - Reject it - They get angry and rob you 30-50 coins but you gain 2-5 Grace
      - Option 3 - Fight - You fight them and lose 10-40 health but gain a random item.
- (item 10) Class stat guide: sort the stats pane, gold/silver stars, highlight on upgrades, data on the class data

## Wave results window (items 6 and 7)
- Clicking too fast might skip experience collection on the last wave of a level
  - FIXED bdf3d54, retest: the summary now opens only after enemies are killed and gems collected, plus a final sweep when it closes. It appears ~2 s later than before.

## Stats pane and Inventory on death (item 3)

- [ ] Hover items and stats: tooltips work (Shift view for stat sources too). 
  - does not work
- [ ] Gamepad: focus starts on the death screen buttons; LB/RB reach the Stats pane and the Inventory and back.
  - cannot focus other windows
  - FIXED bdf3d54, retest: death turned off the whole UI input map (Shift view, LB/RB, gamepad nav). Now only gameplay input is turned off. Esc/Start on the death screen does nothing (pause blocked while dead).
  - If plain mouse HOVER (without Shift) still shows no tooltip, tell me: that would be a different cause.

## Run encounter rework (item 9)
- Change background to plain color, similar to SkillTree or other windows
  - FIXED bdf3d54, retest: plain dark brown (skill tree palette), "Plain Background Color" on EventEncounterPane.prefab.


# Priority 2

## Revamp UI
- Revamp UI. Currently the UI is very messy and inconsistent. Here is the feedback we have been given:
  - We need to sizes of fonts, a small one and a big one only. Currently there are like 8 types
  - Margins should be consistent across all windows
  - Buttons come in too many sizes too. We should have one or two sizes only
  - Sometimes the UI looks very mobile-like, with big icons, fonts and buttons but for other windows UI is very small
- We have to resedign every window. Recently you helped me create the SkillTree window with a script, we could do the same for the rest. But instead of creating a new prefab we could just modify the existing prefabs
- Gamepad should still work and in fact, we can improve the gamepad marker because right now it is just a small triangle on the left and its offset fails to be on the right position, making it to be displayed on top of some components. By standarazing the UI this issue should go away but still we can improve the way it looks

### New standards for UI
- Have only two sized for fonts. Small(12) and big (20)
- Titles should have bold as font type
- Margin of 15 in all windows
- Three icon sizes, small(50 x 50) and medium (80 x 80) and tiny (28 x 28)
- Two sizes fpr buttons, small and big (same as font)
- Reduce the size overall of the UI so it doesn't look like a mobile game
- Try to adjust buttons that allow the player to progress to be on the same spot, or as close as possible. Example currently the button to Continue after an upgrade is in the middle, the button to continue at the shop is on the bottom right. We are aiming to fix this
- Add global settings to as an scriptable object with:
  - Sizes for fonts, icons and buttons
  - Colors that we reuse across all UI for Grace (Cyan) Corruption (Purple) and Favors (Gold) 
  - Other colors that need to be added for UI Menu clarity: 
    - Yellow for highlighting neutral elements. 
    - Light Green for stats that have been improved or if we want to hight light a positive element not related to Grace.
    - Red for stats that are negative and the Deathscreen title or other elements not related to Corruption we want to highlight in a negative way
- Add decoration frames similar to the SKillTree window to all windows

### HUD
For these we probably don't want to make a lot of changes, just let's try to adjust them to the new standards and improve the looks
The main HUD is made of 5 components
- HealthPane - font should be small, Grace and corruption use the right colors. Wave counter can probably still be white when normal and purple on corruptor phase
- IconsPane - icons should be medium, font should be small
- ExperiencePane - font should be small
- FlameProgressionBar - no chances unless you find any improvement
- WaveHintUI - fonts should be small

In addition, we should add  frames to HealthPane, IconsPane and WaveHintUI
Any othe improvement you suggest it would be fine

### InventoryPane
The InventoryPane will not have that many modifications.
- Change the design of the window to fit the new standards
- Inventory title should have big font size and bold type
- coins display label should be small size. 
- The icon of the coins should be of size 28 x 28, it is good you added that size
- The biggest change is that we are going to remove the tooltip panel and re-use the GeneralTooltipUI prefab like in other windows.
- The space we are gaining from removing the tooltip in the InventoryPane, we should expand the panel where the icons are rendered, so more items will fit.
- EffectIconUI prefab should change size to fit the standards (small icon)

### SelectUpgradePane, PauseMenu and DeathscreenPane
These screens are very similar, so we can work with them at the same time

- Change the design of the windows to fit the new standards
- Select Upgrade text will be of title type with big font size
- Coins gained text will have small font size
- UpgradeButton prefab will need the text and size adjusted to be of small size
- Buttons in PauseMenu and DeathscreenPane should be of size small
- Adjust the Window height size to be the same as the InventoryPane, so they look aligned together
- The YOU DIED text on the DeathScreen will be the exception, we need to keep it as it is, maybe just reduce the size slightly
- The PAUSE title will be adjusted to the standards. Change the color to the yellow standard and size to be title and big
- An unrelated change, now that we are stadarizing more colors, we should take a look at past changes and see if we should change any colors. The only one I could think od is the text for auto attack, when it is disabled it becomes orange, I think it should be red, specially because I have seen some testers confused as why the character is not attacking and do not realize auto attack is OFF. Is the any other element we want to standarize its colors?
- Also let's analyze how feasible would it be to add the settings window to the PauseMenu? If it is not that complex let's include it in the implementation plan

### StatsPane
- Change the design of the windows to fit the new standards
- Adjust the title "Attributes" to the standards. Should we rename it to Stats? or Statistics? What would be more appropiate here?
- Change the Main and Secondary buttons to have same style as the tabs used in the SkillTree prefab
- The Legend text labes should be adjusted to small font size.
- The input icon promps should be of type small size
- Adjust the height of the window same as SelectUpgradePane and InventoryPane so they are aligned
- The most difficult change I think will be to add a scroll view to the stats row list. I tried doing it in the past but I was not able to do it
- Improve the look of the StatRow prefab. Maybe add a background semi transparent, that could look good. Or if you have a better suggestion please go ahead and implement it.
- The size of the StatRow prefab needs some adjusting so it is of the size and shape of the row.

### ItemBagPane
- Change the design of the windows to fit the new standards
- Match window size to the other middle windows, like the SelectUpgradePane
- Currently the option buttons on this window are horizontal, we might want to make them vertical to match other windows
- Same as the inventory, remove the current tooltip and re-use GeneralTooltipUI
- Set icons sizes (ItemBagIconButton prefab) to the same as the inventory
- Set small size on the buttons

### GeneralTooltipPane
- Change the design of the windows to fit the new standards
- The size of the Window will increase by x2 
- The background and frames should have some transparency
- Ideally this window will pop up on top of other UI elements. 
- We need to reposition the window, I was thinking to the bottom left or right...but I think that will cause issues. If we put it on the right when we hover on one of the bottom stats it could make it to flicker. What would you recommend? dynamic position?
- I am proposing a new layout, please take a look at the new prefab NewTooltipPanel to see my proposal
  - Regular tooltips will go on GeneralTooltipTxt - item descriptions, stats descriptions, etc
  - Additional tooltip information will go to the DetailedTooltipTxt - Item stats sources, stat exact calculation benefits and next level stats details.
  - For the panel on the right side, we can use it to display the list of stats an item grants, so this would be a logic change because currently this list is rendered on the same GeneralTooltipTxt. And if we have any other vertical list we could display it here too

### ConfirmDialogUI and Endscreen
- Change the design of the windows to fit the new standards
- Endscreen should be very similar to the design of the DeathscreenPane and PauseMenu
- ConfirmDialogUI should adjust the size of the text to small size and buttons to small size too.
- Make sure Gamepad still works on the new designs.

### CampfirePane, EventEncounterPane, TreasurePane and ResultsPane
I have grouped these 4 windows into the same step because they all belong to the RunEncounters module. We aim to make them look very similar
- Change the design of the windows to fit the new standards
- CampfirePane:
  - We will have significant layout changes here, currently the window looks horizontal, we will change it to vertical, similar to other middle windows like the ItemBagPane
  - The title Campfire will have title format, and highlight it in yellow
  - The text You feel rested will be on the same text element as the info text "You  gained XX Grace. Grace improves all damage done."  and remove the last sentence
  - Remove the gem icon
- ResultsPane:
  - Same as campfire, make it vertical and with the same size, aligned in the middle like all other middle windows.
  - We just have 2 texts here, title and description, so just adjust them normally. The title should be highlighted in yellow too.
- TreasurePane: We can copy the same design as ItemBagPane. And actually, let's evaluate if we can re-use it.
- EventEncounterPane:
  - This will be a bit different. We also want the vertical design and same text changes as the previous windows.
  - However, we will later have special art for each event encounter, so we need a way to display it.
  - Ideally if we have no image we will show the vertical window normally, but if there is an image assigned, display a side panel to show that image. 

### WaveCorruptionSummaryUI 
- Change the design of the windows to fit the new standards
- Instead of click to continue or press A to continue we need to change that to a Continue button like all other windows
- Adjust texts and titles to the standards
  - Wave Results title yellow and centered like other titles
  - The first four text labels will have the small font size
  - The last two will have the big font size
- In addition to the format and standards...I need you help suggesting me how can we make it more clear to the player what is going on.
  - Maybe, in addition to thje formula row we can add another Grace bar that the player can see in a slower animation how it changes in real time.
  - Would it be hard to create an animation that shows the number of corruption and grace move from the main hud to this window, so the player understand where are the numbers coming from? This requirement is optional, but it would be a nice touch. If it is too complex we can discard it.

### ShopPane
- Change the design of the windows to fit the new standards
- I think we can make it very similar to the Inventory Window.
- With what we have worked so far, I think you know what to do right?

### LevelSelectorScene
I have created prefabs for this scene, this is what we need to do:
- CurrencyPane and LevelSelectorUI should be combined into a single prefab, standarize the fonts, buttons and icons and place it on the top right
  - Add a window background and frames like the other windows
  - Horizontal layout with the title on top, the name of the act.
  - The name of the level should be of small size font and currently it is not being updated, I believe LevelData SO has the names for each level, we can display it from there
  - Coin icon is too big, make it of tiny type and the currency text should be small too
- InputPrompts: standarize the font to small size and icons to tiny. Add a window and frames too
- Currently there is a bug that makes the StatsPane not show up on this scene, can you take a look into it? Also make sure it can work with Gamepad too

### MainMenu
For the Main Menu we want a to adapt to the standards and improve look and feel.
- Change the design of the windows to fit the new standards
- You already re-designed the settings page. We can take it as a base to improve the menu panel and the character select
- For the character select we can take the SkillTree window as base, for the display of the tooltip
- I made both panes into prefabs in case that is easier to modify MainMenuPanel and CharacterSelect
- Is there a way we can improve the animation for the transitions between the windows?
## Pending manual tests: sheet explanation panels

- [ ] Paste the new `BalanceData/Sheets/BalanceSync.gs` into the sheet's Apps Script, save, Deploy > Manage deployments > edit > New version.
- [ ] Unity: Tools > Flames of Faith > Balance Data > Push Table Notes to Sheet. Every data tab shows, right of the table after one empty column, "About this table" + one note per column.
- [ ] Balance > Publish still works and the published CSVs don't contain the panel (Pull from Sheet reports no errors / "no changes").
- [ ] Push All to Sheet: panels are rewritten, numbers intact, formatting and checkboxes kept.
- [ ] Add a column in a table (or shorten one) and push: the panel moves with the table width, no leftover panel text.
