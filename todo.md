# Pending manual tests

Only unchecked tests are listed here. Finished sections are archived in `docs/archive/` (latest: `todo-2026-10-26.md`). Backlog: `docs/pm/BACKLOG.md`. The RunEncounter events rework notes moved to `docs/features/run-encounter-events.md`.

## High Attack Speed fix (animation speed + caps) (branch `feature/attack-speed-caps`)
Primary attacks used to stop landing at high Attack Speed, because the animation kept its speed while the cooldown shrank. The basic attack's animation now speeds up with its cooldown, and Attack Speed has a soft cap (40) and a hard cap (80). Specials keep their animation speed.

Balance sheet first:
- [ ] Tools/Flames of Faith/Balance Data/Push All to Sheet, then Balance > Publish (version 13 still has scale 0.01 and caps 30 / 60, and a Pull would bring them back). Check PlayerStats has Attack Speed Soft Cap, Hard Cap, Past Soft Cap Rate and Min Melee Hitbox Time, with their notes in the explanation panel.

With DevTools/Stats, for each class at Attack Speed 0, 40, 80 and 120:
- [ ] Attacks land, with auto-attack and manually. At 80 and 120 they fire at the same rate.
- [ ] The animation reads well sped up (the art team should look at this).
- [ ] The attack's facing, its sound, and items triggered by attacks still work.
- [ ] The Warrior's swing hits every enemy in the arc at 80+. Try it at a low frame rate too (Warrior hitbox minimum, 0.04 s).

Other checks:
- [ ] A level-up or a new item that adds Attack Speed changes the animation speed at once. It stays right after Save & Exit and Continue.
- [ ] Negative Attack Speed (Decrease -1 several times): attacks are slower, the animation plays at normal speed, nothing breaks.
- [ ] Stats pane tooltip: past 40 it says points count for less, at 80 it says more points do nothing, and the cooldown matches play. The level-up Now / After shows the same numbers.
- [ ] Log Special Cooldown still matches the tooltip.

## Melee attack warning (LightDevourer) (2026-10-26)
When a LightDevourer starts its swing, a red ring appears on the spot it will hit and a disc grows inside it; the hit lands when the disc is full, 0.8 s later (it was 0.3 s with no warning). The direction is decided when the swing starts, and the enemy stands still while winding up, so stepping out of the ring dodges it. The attack animation plays slower to match. Placeholders: the ring and disc sprites (Sprites/Enemies/Telegraph) and colors (Resources/AI/AttackTelegraphSettings).
- [ ] First: Balance Data > Push All to Sheet (EnemyBehaviors_MeleeAttacker has two new columns: Telegraph Time, Animation Hit Time).
- [ ] Let a LightDevourer reach you: the ring shows where the hit lands, the disc fills, the hit lands right when it's full. Standing in it: you're hit. Stepping out before it fills: you're not.
- [ ] The enemy doesn't slide or turn during the windup; the swing animation looks slower but not broken.
- [ ] The ring draws above the ground, under you when you stand in it, and doesn't flicker over or under other enemies badly.
- [ ] Stun it mid-windup (e.g. a stun item): the ring disappears; after the stun it finishes the swing at the same spot.
- [ ] Kill it mid-windup: the ring goes with it.
- [ ] Tune Telegraph Time in the sheet (e.g. 1.0), Pull: the warning and the animation both take longer.

## Run encounter events rework (branch `feature/run-encounter-events`)
Spec: `docs/features/run-encounter-events.md`. Steps land one commit at a time; until step 8 the old events keep playing on their old option assets.

### Step 1: data model
Events can now hold their options, follow-up steps and class texts in their own asset. No event uses it yet, so nothing changes in play.
- [ ] Open any event in `Resources/RunEncounters/EventEncounter/`: its old options now show under **Legacy Options** (same list), and the new Options / Steps / Class Intro Lines lists are empty.
- [ ] Play a random event node: the event plays exactly as before.

### Step 2: checks and their settings
Stat checks (Easy / Even / Hard + a stat, the class's Key stat or current Grace) and their tunables, on `Resources/RunEncounters/EventCheckSettings.asset`. No event uses a check yet.
- [ ] Open EventCheckSettings: base chances 75 / 50 / 30%, 3% per point by default, Max Health from 20 at 1% per point, Grace Affinity at 5%, Grace at 2% per point, limits 5–95%, and a Log Checks toggle. Every field has a tooltip.

### Step 3: structured results and the summary line
Actions now report what they actually changed, and the Results window ends with a compact colored line ("-8 health  /  +3 Grace  /  +40 Golden Chronos  /  item name"). The old events have no narrated text yet, so they show only that line.
- [ ] Play a random event and pick an option that hurts: the line shows the health actually lost after armor (red), and Grace gains in cyan, Golden Chronos gains in green, losses in red, Favors in gold, items in their quality color.
- [ ] Pick an option that costs Golden Chronos with fewer coins than it costs: you lose only what you had (never a negative purse), and the line says so.
- [ ] The treasure's Results window (a currency or experience chest) is unchanged: one green line per reward.

### Steps 4–5: steps and class content
The event window now walks follow-up steps and shows class content: class intro lines, class labels, class-only options and class result lines. An option's tooltip is its in-world hint, never numbers. The old events have no hints yet, so their options show no tooltip until step 8.
- [ ] Play a random event: it opens as before, an option closes the window, and the Results window opens.
- [ ] With a gamepad: the first option is selected when the window opens, and A picks it.

### Step 6: no repeats in a Pilgrimage
An event node now picks an event not seen yet in this Pilgrimage, from the run seed and the node, so the same node always offers the same event. The seen list is saved with the run. Once every event has been seen they can repeat, except once-per-Pilgrimage ones (Liah, step 8).
- [ ] In one Pilgrimage, visit several event nodes (DevTools map cheats: free travel): no event shows twice.
- [ ] Enter an event node, note the event, Save & Exit from the pause menu, Continue: the same event shows again (no re-roll).
- [ ] Finish that event, go to the map, Save & Exit, Continue, visit another event node: the first event doesn't show again.
- [ ] Start a New Game: events you saw last run can show again.
- [ ] The Wayside Shrine node still shows its event every time.

### Step 7: UI
The Results window now shows the narration left-aligned at the top and the summary line centered just above Continue (`Summary`, built by Restyle Run Encounter Windows). The event window fills a screen's options before it opens.
- [ ] An event's Results: the summary line sits right above Continue, centered; with no narration yet (old events) the top is empty and only the summary shows.
- [ ] A currency / experience treasure's Results: its lines at the top (now left-aligned), and no summary line.
- [ ] With a gamepad, after a step swap (step 8 events, e.g. the Weeping Chest's whisper): the first option of the new screen is selected.

### Step 8: the 10 events
All events are authored from the spec: the Vega Casket, Raiders from Gracoy, the Weeping Chest, the Faraclarian Clan, Gift of the House of Stars, A Hut in the Jesterwoods (Liah), Laughter in the Leaves, the Star Reader, the Last Torch (the random pool, 9 events) and the Wayside Shrine of Malien (its own node). Free travel (DevTools map cheats) makes this quicker; tick **Log Checks** on EventCheckSettings to see each check's chance and roll in the Console.
- [ ] Play every event with each class (Warrior / Ayteel, Archer / Faelin, Augur / Werlem): the intro shows that pilgrim's class line, class-only options show only for their class (Ayteel: Faraclarian Clan; Faelin: House of Stars, Last Torch; Werlem: Vega Casket, Raiders, House of Stars), and Liah's first option reads "Tell her who you are." for Ayteel.
- [ ] Hover (and gamepad-select) options: the tooltip is the italic in-world hint, never numbers.
- [ ] Results: the narrated text with the numbers woven in, then the summary line; the numbers in both agree. An item reward names the item in its quality color.
- [ ] Step chains:
  - [ ] Weeping Chest: Reach for what lies beneath, fail the Grace check (low or negative Grace helps), then both whisper answers.
  - [ ] Faraclarian Clan: Say you've seen the Forger > both descriptions (the giant's check intro line comes first in the result).
  - [ ] Faraclarian Clan as Ayteel: Show them Gabien's sword > "Malien's Steel." > both Kin answers; also "Steeldawn." and "Gabien's steel.".
  - [ ] House of Stars as Werlem: Show them the Vega mark > "Human Grace." > both Pride options; also the two other answers.
  - [ ] Liah: Tell her whose sword it is > both answers; "The truth." adds the pilgrim's own line at the end.
  - [ ] Laughter in the Leaves: Tell it a joke > the augur joke > both Secret options; also the other two jokes.
- [ ] Liah shows at most once per Pilgrimage, also after Save & Exit and Continue.
- [ ] With an empty item pool (rare; e.g. very early with few unlocked items) an item sentence drops cleanly instead of reading "nothing".
- [ ] The texts fit the event window and the Results window (longest: Liah's "The truth." with a class line, the Faraclarian "steel" step).

### Step 9: renames
- [ ] First: Balance Data > Push All to Sheet (the Quiver rows were renamed; a Pull fails until the sheet matches).
- [ ] The Archer's quiver reads "Quiver of the Sentinel" (Inventory, shop, skill tree, at every tier); the Sacred Writings item reads "Sacred Writings".
- [ ] A saved run with the quiver Continues fine (same effectIDs).
- [ ] The Shrine node's event title reads "Wayside Shrine of Malien".

### Cleanup: old event assets removed
The old option assets, the action sizes no event uses, the legacy code path and the unused scenes `BanditsEncounter` / `CursedChestEncounter` (and their Build Settings entries) are deleted.
- [ ] Open the project: no missing-script or missing-reference warnings on the event assets or the EventEncounter prefab.
- [ ] Build Settings no longer lists the two scenes, and a random event node, the Shrine, a shop and a treasure all still load.

## UI revamp step 8: run encounter windows
- [ ] No event has art yet, so no side panel shows. (To try it: assign any sprite to an event's **Art** field; a framed panel appears to the left of the event window.)
