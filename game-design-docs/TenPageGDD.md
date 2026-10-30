# Flames of Faith – Ten Page GDD

_Structure follows the Ten Page GDD from Scott Rogers, *Level Up! The Guide to Great Video Game Design*. Last updated 2026-10-29. Where this document and the code disagree, the code (and `CLAUDE.md`) wins; please fix the document. Items marked **TBD** or **Proposed** are not decided yet and are collected in [Open questions](#13--open-questions)._

---

## 1 – Title page

| | |
|---|---|
| **Title** | Flames of Faith |
| **Genre** | 2D isometric action roguelite with survivor-like combat |
| **Platforms** | PC (Steam) first, with Steam Deck support as a goal; Xbox possibly later |
| **Engine** | Unity 6.3 LTS (URP 2D) |
| **Target rating** | Teen / PEGI 16 (fantasy violence, dark religious and horror themes) |
| **Target audience** | Players of roguelites and survivor-likes (*Death Must Die*, *Brotato*, *Vampire Survivors*, *Hades*) who want short, replayable runs with build-crafting and meta-progression, and **achievement hunters** |
| **Ship date** | Steam demo (full Act I): **December 2026**. Full release (1.0): **Fall/Winter 2027**. A tester build of Act I already exists (v0.5.1). |
| **Monetization** | Premium, one-time purchase. No microtransactions. No DLC planned; post-1.0 content updates are not ruled out. |
| **Team** | Solo developer, with friends helping on art and testing, plus a game designer working on balance |

---

## 2 – Scope and goals

### Vision
Short, readable runs where every wave is a small decision about **Grace**: fight cleanly and the holy power grows, take Corrupted hits or let the Corruptor escape and you slide into Corruption, which makes you weaker but the run more dangerous and rewarding to recover from.

### Design goals
1. **Easy to pick up.** Movement, an auto-attack that can be toggled, a special and a dash. A run is understood in one sitting.
2. **Meaningful build-crafting.** Items (with quality tiers), level-up stat picks and class kits combine into builds that feel different run to run.
3. **Grace as the central tension.** One signed resource that is both a damage bonus and a measure of how corrupted you are.
4. **Replayability.** A branching overworld map per act, several map variants per act, random events, shop stock and item drops.
5. **Long-term goals between runs.** Favors earned in runs buy class unlocks and upgrades in per-class skill trees.
6. **Readable on screen.** A standardized UI with full gamepad support.

### Release scope

| Release | Content | Status |
|---|---|---|
| **Steam demo** (December 2026) | The full Act I (Jesterwoods): 3 map variants, combat levels, Plerian Shop, campfire, treasure, events, Wayside Shrine of Malien, Act I boss. Three classes, skill trees, Favors, run save. Keyboard/mouse and gamepad. | Act I is playable in tester builds; polish and balance in progress |
| **Full release, 1.0** (Fall/Winter 2027) | Acts I–IV, each with its own region, mechanic, enemies, Corruptor and boss. Act intro and ending cutscenes. Chaos and Holy debuffs. More items (including Epic and Legendary), events and kit upgrades. Steam achievements. Spanish localization. Input remapping. | Acts II–IV are designed at concept level (see [Enemies](#10--enemies)); bosses II–IV are **TBD** |
| **Nice to have for 1.0** | Steam Cloud saves, Steam Deck verification, Chinese and Japanese localization, a Hard mode | Optional |
| **After 1.0** | Xbox port; content updates. No DLC planned. | Not scheduled |

### Localization
- **1.0:** English and Spanish.
- **Optional:** Chinese (Simplified) and Japanese.
- The UI uses a pixel font, so Chinese and Japanese also need a font that supports those scripts.

### Out of scope (for the full release)
- Multiplayer or co-op.
- Procedurally generated level layouts (levels are hand-built scenes; variety comes from the map, waves and loot).
- Story cutscenes with voice acting (see [Cutscenes](#11--cutscenes)).

---

## 3 – Story and gameplay overview

### Story
_The full lore is in [`Lore.md`](Lore.md); this is the summary._

The story is set on **Sialant**, a continent of the world of **Kanator**, about sixty years after the fall of the kingdom of **Ashemdrek**. Thousands of years ago, in the Dark Age, the **Drekos** came from the stars and enslaved the world, until they were driven underground and held there by the **Altars of Malien**, whose white **Flames of Faith** keep them down. Over the ages the Drekos became a children's tale, and the **White Church** of Ashemdrek let the **Pilgrimage of the Flame**, the rite that tends the altars, fall out of use.

Sixty years ago **Malizante**, the Sea Witch, leader of the surviving Drekos, woke and put out the altars one by one. With Gracoy mercenaries and the **Forsaken** (people who renounced Malien's Grace) she invaded Ashemdrek, where the last Flame still burned. **Gabien Alamoh**, a **Seared Soul** whose Grace burned his very soul, beat her, but she was too ancient to kill. The relic meant to banish her, the **Seal of Malien**, was gone: Gabien had broken it to save his sister and the woman he loved. So his party threw Malizante into the last altar. The Flame died with her, and the darkness it held broke loose. Gabien died of his wounds; his companions **Orleone Vidal** and **Kyro Feth** escaped, and under questioning in Abidor they had to confess that he had misused the Seal. Common folk hate the Alamoh name because Gabien lost; the nobles hate it because he squandered a sacred relic.

Since then, the **Years of the Miasma**: a dark mist breathed out by Dreko creatures has spread over Sialant. Ashemdrek, Gracoy and the northern land of **Fara Uf'Clario** have fallen, and only the elven kingdom of **Fustina** still stands. Ashemdrek's survivors fled across the sea to **Abidor**, a kingdom of the Glenos Empire, where the White Church renamed itself the **Shardless Church**, vowing never to split into factions again. Now the Miasma is reaching Abidor: sudden illnesses, unknown plagues, food that spoils overnight. The Shardless Church's augurs call for the Pilgrimage of the Flame to be walked again, to relight the Altars of Malien across Sialant: through the **Jesterwoods**, the frozen **Fara Uf'Clario**, the **Sinfa Mountains**, and finally the **Ruins of Ashemdrek**.

**Playable characters.** Three pilgrims set out together, the grandchildren of the three who were there when Ashemdrek fell. The player chooses which one fights; the other two walk beside them and speak in events. Each has an arc, told mostly through events: a start in Act I, a turning point in Acts II–III, a resolution in Act IV.

- **Ayteel Alamoh – Warrior (Crusader of the Faith).** Granddaughter of Gabien and of **Dayla**, raised in Abidor among refugees by a family nobody would speak to. She carries Gabien's Steeldawn sword, **Ashem Elwyn** ("Shining Tide"), and calls herself a Crusader of the Faith, a rank the Church never granted an Alamoh. She means to clear her family's name by finishing what Gabien failed. Along the road she learns why he broke the Seal, and that she is alive because he did.
- **Faelin Vidal – Archer (Sentinel of Abidor).** Granddaughter of Orleone Vidal, Grand Crusader of Abidor and Gabien's mentor, recruited young by Abidor's **Sentinels**. Learning that some blame Orleone for Gabien's failure shook her beliefs; Ayteel, a true outcast who kept going anyway, gave her back her nerve. Her question: was Orleone's confession honor or betrayal?
- **Werlem Feth – Augur (Shardless Church).** Grandson of Kyro Feth, whose findings founded **Runic Math**, the new science that lets humans cast without the Mastow elves are born with. Against the wishes of his academic parents he chose faith and became an Augur of the Shardless Church. Along the road he finds what Kyro spent his life calculating, and the one term his science could never write.

**Enemies of the story:** Gracoy mercenaries, the Forsaken and the will-less **Awoken**, corrupted beasts, the soul-eating **Devourers**, and above all the **Drekos**, their Miasma creatures and the **echoes** of the great dead (Malizante, Pal'bor, Gabien). In Act III the pilgrims face **Forsaken Daviro the Unholy**, a Forsaken from Gabien's time.

### Gameplay summary
Each run is a pilgrimage through an act. The player moves across a branching **overworld map** of nodes, choosing a path toward the act's boss. Combat nodes are isometric arenas where waves of enemies spawn for a fixed time. The player auto-attacks (or attacks manually), uses a class special and a dash, collects experience tokens to level up, and picks stat upgrades. Between fights the player meets shops, campfires, treasure chests and random events. **Grace** rises and falls at the end of each wave, changing the player's damage and, below zero, making them Corrupted. Dying ends the run; the **Favors** earned are banked and spent on class unlocks and skill tree upgrades.

### Terms
Some of this document's words are internal. The player always sees the lore name instead:

| Internal term | Shown to the player | Meaning |
|---|---|---|
| Run | **Pilgrimage** | One attempt: from the start of a new game until the player dies or defeats the final boss (in the demo, the Act I boss). Coins, items, Grace and level are lost when it ends; the Favors earned are banked. |
| Coins, gold | **Golden Chronos** | The currency of the lore, used during a Pilgrimage: earned in waves, events and treasures and by selling items, spent at Shops, lost when the Pilgrimage ends. |

---

## 4 – Game flow

```
Main menu ──> Class select ──> Overworld map (Act N)
                                   │
                    ┌──────────────┼───────────────┬───────────────┐
                 Combat         Shop / Campfire   Treasure / Event   Boss
                 (waves)        / Wayside Shrine                     │
                    │                                                ├─> next act? (open question 14)
                    └────────── back to the map ◄────────────────────┘
Death or final victory ──> Favors banked ──> Skill tree (main menu) ──> new run
```

### A run, step by step
1. **Main menu.** New Game, Load Game (continue the single saved run), Settings, Skill Tree, Exit.
2. **Class select.** Choose Warrior, Archer or Augur (Archer and Augur start locked). The details show the starting weapon at its upgraded tier.
3. **Overworld map.** The act's map is picked at random from its variants (Act I has 3) and generated from the run's seed. Nodes are revealed as you advance (fog of war); you can only move forward along edges, never back. Some optional paths appear only in some runs. Each combat node shows its **Taint Level**.
4. **Node types:**
   - **Combat:** 2 waves of enemies (wave durations of 30–45 s in Act I).
   - **Shop** (Plerian Shop): buy items with Golden Chronos from a travelling Plerian trader.
   - **Rest** (Campfire): gain Grace and health.
   - **Treasure:** a chest with one reward.
   - **Event** (Strange Encounter): a random story event with 2–3 options.
   - **Wayside Shrine of Malien:** a fixed event focused on Grace (a rest node).
   - **Boss:** the act's final node.
5. **Inside a combat level:**
   1. A wave starts; enemies spawn from portals for the wave's duration.
   2. When the timer ends, spawning stops and the **Corruptor** appears (see [Grace and Corruption](#mechanic-grace-and-corruption)). Kill it before it escapes.
   3. The wave ends: remaining enemies die, experience is collected on the last wave, then the **Wave Results** window shows how Grace changed and any Favor earned.
   4. The **item bag** opens for items picked up during the wave (take or sell each), then the **level-up** window for each level gained.
   5. After the last wave the player returns to the map.
6. **Saving.** The run autosaves on every map arrival and move. Quitting inside a level returns the player to the map as it was before entering it.
7. **End of run.** Death, or beating the final boss, ends the run. The run's Favors are banked for the meta-progression.

### Collectibles
- **Golden Chronos (coins):** granted at the end of every wave, from events and treasure, and by selling items. Spent in shops, and lost when the Pilgrimage ends.
- **Experience tokens:** dropped by enemies in several denominations (bigger tokens from tougher enemies). Leveling up offers a choice of stat upgrades.
- **Grace pickups:** occasional drops; more likely with Grace Affinity.
- **Health pickups** and **random item pickups** (the item goes to the item bag).

---

## 5 – Characters and controls

### Classes
Each class has a **kit** of six starting items: a weapon, an off-hand, an Emblem and three **Glyphs** (one per ability slot). Each kit item has upgrade tiers (+1 to +3) bought with Favors in the class's skill tree. Upgrades apply from the next run.

| | **Warrior** | **Archer** | **Augur** |
|---|---|---|---|
| **Character** | Ayteel Alamoh | Faelin Vidal | Werlem Feth |
| **Role** | Melee bruiser | Ranged skirmisher | Magic caster, area damage |
| **Weapon** | Steeldawn Sword | Steeldawn Bow | Akeros Graceful Scepter |
| **Off-hand** | Shield of the Crusader (sturdy, slower) | Quiver of the Centinel, to be renamed Quiver of the Sentinel (longer range, less protection) | Ley Shield (stronger debuffs, less protection) |
| **Emblem** (+1 main stat per level) | Warrior Emblem: Melee Damage | Archer Emblem: Ranged Damage | Augur Emblem: Magic Damage |
| **Basic attack** | Glyph of Crusader Strike: a sword swing in an arc; may inflict Bleed | Glyph of Holy Shot: a blessed arrow; upgrades add a cone of extra arrows | Glyph of Fireball: a fireball; may inflict Fire; upgrades add an impact explosion |
| **Special** | Glyph of Whirlwind: a 2 s spin that hits every enemy in it on contact and again at a regular interval (faster on upgrades); the Warrior can still be hurt while spinning; upgrades add a holy shield that absorbs enemy projectiles and heals | Glyph of Barrage: a ring of arrows, then homing arrows at the marked target; invulnerable during it | Glyph of Fire Tornadoes: seeking fire tornadoes, then explosions on the marked target; upgrades make the tornadoes explode |
| **Dash** | Glyph of Crusader Charge: charge through enemies, damaging and knocking them back | Glyph of Trap Step: leaves a trap that damages the first enemy to step on it | Glyph of Flame Dash: pass through enemies and leave a trail of flames |
| **Unlock** | Unlocked by default | 50 Favors | 50 Favors |

Each class also has a **stat guide**: one **Key** stat (gold star) and some **Complementary** stats (silver star), shown in the Stats pane and on level-up choices so new players know what to pick.

### Controls

| Action | Keyboard and mouse | Gamepad (Xbox layout) |
|---|---|---|
| Move | WASD / arrow keys | Left stick |
| Aim / face | Mouse position | Right stick (or movement direction) |
| Basic attack (manual) | Left mouse button / Enter | X |
| Special attack | Right mouse button | Y |
| Dash | Space | A |
| Toggle auto-attack | Ctrl | RT |
| Lock on to a target | F | LT |
| Show stat sources (in menus) | Left Shift | LT |
| Pause | Esc | Start |
| Switch UI windows | Mouse | LB / RB |
| Back / cancel | Esc | B |

- **Auto-attack** (on by default) attacks the nearest enemy in weapon range. With it off, the player attacks manually with light aim assist.
- **Lock-on** marks one enemy (Toggle mode by default, or Hold mode in Settings). In Toggle mode a tap locks the nearest enemy, the next taps cycle clockwise through the enemies in range, and a tap with no enemy in range releases the lock. With the mouse, clicking an enemy locks it. A locked target stays marked even out of range, and specials (Barrage, Fire Tornado explosions) aim at it.
- The character faces the target while it is alive and in range, otherwise the mouse (or the stick).

---

## 6 – Main gameplay concepts

### Combat
Combat is simple to control and deep in builds: auto-attacks on the nearest enemy, a special on a cooldown, and a dash with charges. Enemies come in timed waves from spawn portals. Damage scales with the player's damage stats, items, debuffs and Grace.

### Mechanic: Grace and Corruption
Grace is the core resource.

- **Grace** is a single signed value from **−50 to +50**, kept for the whole run.
  - **Damage:** each point of Grace raises (or, below zero, lowers) all damage the player deals, with a minimum multiplier.
  - **Corrupted:** below 0 the player is **Corrupted**. Each point below zero lowers Armor and Healing Received, and raises the chance that enemies spawn Corrupted. Corrupted players, enemies and the boss get a pulsing purple outline.
- **Corruption** is counted **per wave only** and comes from two sources:
  - **Corrupted Damage:** damage taken from Corrupted enemies, their projectiles and explosions (capped per wave).
  - **The Corruptor:** at the end of every wave's timer a Corruptor appears. Spawning stops, the camera shows it, and a timer starts. The longer it lives, the more Corruption it gives. If it isn't killed in time it escapes through a portal and gives even more.
- **Wave end:** `Grace = Grace + Grace Affinity − Corruption`. Then Corruption resets. The Wave Results window explains the change step by step.
- **Other Grace sources:** the Campfire, the Wayside Shrine and some events, Grace pickups, and Retribution hits.
- **Corrupted enemies** have more health and deal more damage. Their chance to appear grows with the level's **Taint Level** and the player's Corrupted level.

### Mechanic: Favors and meta-progression
- **Earned:** +1 for each wave that ends with Grace above 0, +5 for a boss kill, and from some events.
- **Banking:** Favors count only when the level they were earned in is finished. They are banked only when the run ends (death or victory). Abandoning a run loses them.
- **Spending** (between runs, in the Skill Tree): unlock classes (50 Favors), and upgrade each kit item one tier at a time (10 / 15 / 20 Favors). Favors are one shared pool across classes.
- A typical lost run earns about 12–18 Favors.

### Platform-specific features
- **PC (Steam):** keyboard and mouse or gamepad, with automatic glyph switching; scalable UI from 1920×1080; resolution and quality settings.
- **Steam achievements (planned for 1.0):** achievement hunters are part of the target audience, so achievements are a feature, not an afterthought. Candidates: clear each act, beat each boss with each class, max a skill tree node, end a level with high Grace, kill the Corruptor fast, win while Corrupted. The list is **TBD**.
- **Steam Cloud saves (optional):** the run save and meta-progression are small files, so this should be cheap.
- **Steam Deck (optional, recommended):** the game already plays fully on gamepad, so verification is mostly testing text size at 1280×800.
- **Xbox:** possible port after 1.0. The UI is already fully gamepad-navigable.

---

## 7 – Game world

Four regions, one per act. Each act is a branching map of about 25 nodes ending at an **Altar of Malien** guarded by the act's boss. You cannot go back once you leave a node.

| Act | Region | Mood | Act mechanic | Boss | Status |
|---|---|---|---|---|---|
| I | **Jesterwoods** | Ordinary fantasy forest, lonely and a little gloomy | Grace and the Corruptor (the core loop) | Taio'Sandir | Built: 3 map variants. Levels include Jesterwood Glade, Jesterwood Cliffs, Heart of the Forest, Perkfandor Outskirts, Perkfandor Ruins, Moribairo Fields, Argent Coast Ruins, Seared Ruins and Sinfa Hillside, plus Campfire, Plerian Shop, Treasure, Strange Encounter, Wayside Shrine of Malien and the Altar of Malien (boss) |
| II | **Fara Uf'Clario** | Snow, cold, desolate | Frozen (frost on the player) and Frozen Enemies | The Ice Bull | Designed (enemies), boss mechanics **TBD** |
| III | **Sinfa Mountains** | Mountainous, rocky, desolate, cliffs | Pools of Corruption | Forsaken Daviro the Unholy | Designed (enemies), boss mechanics **TBD** |
| IV | **Ruins of Ashemdrek** | Urban, destroyed, ruins, dark | Pools of Corruption, Will Break and Darkness | **TBD** | Designed (enemies), boss **TBD** |

Each act adds one new mechanic on top of the previous ones, so the core loop stays the same while the pressure grows. The act mechanics are described with the act's enemies in [Enemies](#10--enemies).

Combat levels are hand-built isometric arenas with a few obstacles (tree trunks) that block enemy projectiles. The art team is redesigning the scenarios, which may make levels smaller.

---

## 8 – Interface

The UI follows one standard: two font sizes, three icon sizes, two button sizes, framed windows, and a color code (Grace cyan, Corruption purple, Favors gold, yellow highlight, green positive, red negative). Every window works with mouse and gamepad; a pulsing frame shows the gamepad selection.

- **HUD:**
  - **Top left:** health, the signed Grace bar with the wave's pending Corruption, the wave number and its timer. Under it, an optional tutorial hint.
  - **Top right:** level and experience bar.
  - **Bottom left:** the three ability icons with cooldowns and input glyphs, plus the auto-attack and lock-on indicators.
  - **Bottom center:** the boss's altar flame bar (boss level only).
- **Side panes:** the **Inventory** (left) and **Stats** pane (right) open with every center window. Holding Shift / LT shows which items give each stat. The Inventory's title row shows the Favors (banked, and in brackets those earned this Pilgrimage, e.g. "42 (+5)") and the Golden Chronos, each with a tooltip that explains it.
- **Center windows:** level-up (with Now/After stat previews), item bag (take or sell), wave results, shop, campfire, event and results, pause, settings, death screen and victory screen.
- **Overworld map:** act title, current level name, Favors and Golden Chronos (with the same tooltips as the Inventory), Save & Exit, the Stats pane, and gamepad prompts.
- **Main menu:** menu panel, class select with class details, Skill Tree, Settings.
- **Tooltips:** one shared tooltip for items, stats and options, placed beside the hovered or selected element.

---

## 9 – Mechanics and power-ups

### Player stats
Stats are raised by level-up picks, items and kit Emblems. Their descriptions in game come from the same formulas as gameplay.

| Stat | Effect |
|---|---|
| Max Health | Maximum health |
| Armor | Reduces all damage taken, with diminishing returns; negative Armor increases damage taken. Lowered while Corrupted |
| Evasion | Chance to avoid all damage from a hit |
| Health Regen | Heals over time |
| Healing Received | Adds health to every heal (not regeneration). Lowered while Corrupted |
| Life Steal | Chance for hits to heal |
| Melee Damage | Added to melee weapons, melee skills and Retribution |
| Ranged Damage | Added to projectile weapons and skills |
| Magic Damage | Added to magic weapons and some skills |
| Attack Speed | Shortens basic and special attack cooldowns |
| Range | Weapon reach and targeting range |
| Pierce | Projectiles pass through more enemies; arrows may split on hit |
| Skill Duration | Skills and item effects last longer |
| Mastow Affinity | Stronger, longer elemental debuffs |
| Retribution | Reflects part of the damage taken as melee damage; hits may grant Grace |
| Grace Affinity | Grace gained at every wave end; more Grace pickups |
| Move Speed | Movement speed |
| Dash Cooldown | Faster dash recharge |
| Pickup Range | Pickups fly to you from further away |
| Experience to Level Up | Enemies drop more experience |
| Luck | Better level-up tiers, more and better shop items, better event outcomes |
| Shop Item Discount | Lower shop prices |

_(Cleave was retired in Oct 2026.)_

### Leveling up
Experience needed grows by 20% per level (10, 12, 14.4...). Drops are sized so the player gains about the same number of levels per wave at any point. Each level gained offers stat upgrade choices in tiers (Luck improves the tiers).

### Items
- **43 items** outside the class kits. Six qualities exist (Common, Uncommon, Rare, Heroic, Epic, Legendary); items currently go up to Heroic, and Epic and Legendary items are **TBD**.
- Items give flat or percentage stats, triggered effects (orbiting swords, holy missiles, daggers on being hit, thunder, energy orbs...), elemental infusions, and some give abilities.
- **Sources:** shops (stock count and quality scale with Luck), treasure chests, events, random item pickups.
- Kit items with stat bonus rules (e.g. "every N points of X give Y") are starting items only.

### Elemental debuffs
Inflicted by weapons, Glyphs and items; all scale with **Mastow Affinity**.

| Debuff | Effect |
|---|---|
| **Fire** | Burn damage over time; can spread to enemies that touch a burning one |
| **Frost** | Slows the target and makes it take more damage |
| **Bleed** | Damage over time that stacks (up to 5) |
| **Ley Charge** | When a charged enemy takes damage, a bolt jumps to a nearby enemy, damages it and charges it (limited jumps) |
| **Chaos** _(planned for 1.0)_ | On damage or death, the target may explode, damaging nearby enemies and spreading Chaos to them; enemies already affected also explode |
| **Holy** _(planned for 1.0)_ | Chance to stun the target briefly; when it triggers, the player heals a little and gains 1 Grace |

From Act II, some enemies also inflict **Frost on the player** (see [Act II](#act-ii-fara-ufclario)).

### Run encounters (events)
Every event has 2–3 options, and each option costs something and gives something (no free "Ignore"). **In the current build:** outcomes use Luck-biased rolls, and the random pool is **The Vega Casket**, **Raiders from Gracoy**, **The Weeping Chest**, **The Faraclarian Clan** and **Gift of the House of Stars**, plus the **Wayside Shrine of Malien** node. Events can show art, but none has art yet.

**Rework (written 2026-10-26, implementation pending; content in `docs/features/run-encounter-events.md`, "Narrative"):**
- **Options** show an in-world hint instead of numbers, and results are narrated.
- **Outcomes** can be stat checks (Easy / Even / Hard plus a stat such as the class's Key stat, Armor, Move Speed or current Grace).
- **Steps:** events can have follow-up steps, up to 3 deep.
- **Class content:** each pilgrim gets class lines, plus a few class-only options.
- **Pure gains** (options with no cost) are earned only by answering a lore question correctly.
- **The pool** grows to 9 random events: the 5 above plus **A Hut in the Jesterwoods** (Liah, Gabien's sister; once per Pilgrimage), **Laughter in the Leaves** (an Aylinean), **The Star Reader** and **The Last Torch**.

---

## 10 – Enemies

Enemy damage is sized against the *expected* player at each point of the run, using damage tiers. That way, a player who builds more health or armor than expected really does outscale the enemies. Enemy health grows per wave and per level. Any enemy can spawn **Corrupted**.

### Act I enemies

| Enemy | Behavior |
|---|---|
| **Slime Chaser** | Hops toward the player; contact damage |
| **Slime Roamer** | Hops in random directions; contact damage |
| **Explosive Slime** | Hops toward the player and explodes |
| **Flame Shooter** | Floats after the player and shoots fireballs while moving |
| **Dreko Eye** | Walks, stops and fires a three-way spread |
| **Light Devourer** | Heavy melee attacker with high health |
| **Dreko Spawn (Corruptor, Act I)** | Always Corrupted; appears at every wave end, flees from the player and shoots; escapes through a portal if not killed in time. Working name **Dreko Spawn**, a creature of the Miasma (a better name may come later) |

_Acts II–IV below are concept designs. "Damage" and "health" are relative (low / medium / high) and will be set as damage tiers and health values in the balance sheet._

### Act II: Fara Uf'Clario

**New mechanics**
- **Frozen** _(optional)_: Frost now hits the player too. After **3 frost hits** the player is **Frozen** for a few seconds: unable to move or attack, and enemy hits knock them back further, sliding like a block of ice.
- **Frozen Enemies:** blocks of ice with an enemy trapped inside appear in the level. Breaking the ice releases the enemy, which attacks. _(Open: is breaking them optional, and do they reward anything?)_

| Enemy | Behavior | Damage | Health | Notes |
|---|---|---|---|---|
| **Frozen Slime** | Like the Slime Chaser, but now and then shoots ice shards at the player | High (projectiles), low (contact) | Low | Inflicts Frost |
| **Ice Sprite** | Small icy creature that shoots bursts of many weak projectiles | Low | Low | Inflicts Frost |
| **Awoken** | A Forsaken who rose from a night in the grave, will-less and deathless; casts big fire projectiles from long range | Very high (projectiles) | Medium | A fire caster in the frozen act |
| **Ice Golem** | Big, slow and heavy; contact damage only | High | High | Inflicts Frost |
| **Frost Ghoul** | Undead that chases the player and attacks with its claws | Medium | Medium | Fast. Inflicts Frost |
| **Death Witch** | Flies in a circle around the player and casts a slow, clearly telegraphed beam at them | Very high (beam), low (contact) | Low | Fast |
| **Echo of Malizante (Corruptor)** | The lingering echo of the Sea Witch, the winged Dreko who led the invasion; it appears near the player for a few seconds, always within the player's line of sight, fires an ice projectile and vanishes | – | – | Replaces the flee-and-shoot pattern of the Act I Corruptor |

### Act III: Sinfa Mountains

**New mechanic: Pools of Corruption**
- Damage from some enemies has a chance to leave a **Pool of Corruption** on the ground.
- Pools grow over time and **persist between waves**.
- A player who touches a pool loses **1 Grace**, and the pool shrinks.
- An enemy that steps on a pool becomes **Corrupted**.

| Enemy | Behavior | Damage | Health | Notes |
|---|---|---|---|---|
| **Fur Spider** | Fast creature; contact damage | Medium | Low | Fast |
| **Jumper** | A strange leopard with elven ears that charges at the player | High | Medium | |
| **Crawler** | A four-armed creature that crawls slowly and now and then launches a homing projectile | TBD | TBD | |
| **Atractor** | Disguised as a flower; when the player gets close it becomes a huge worm that spits bursts of acid | High | High | Can't move |
| **Incinerator** | Fiery humanoid that shoots a flame projectile that splits into smaller ones; bursts into flames around it when it dies | High | Medium | |
| **Awoken Zealot** | A fallen Crusader who renounced Grace and rose as one of the Awoken; charges at the player and heals nearby enemies | Medium | High | Healer: a priority target |
| **Echo of Pal'bor (Corruptor)** | The echo of the oldest Dreko, who once led the Forsaken: a shape wrapped in dark flames that shoots bursts of projectiles | – | – | Becomes immune while standing in a Pool of Corruption |

### Act IV: Ruins of Ashemdrek

**New mechanics**
- **Pools of Corruption** stay from Act III.
- **Will Break:** the more enemies alive in the level, the higher the chance to suffer Will Break. Will Break increases the Corruption gained and **carries over between levels**. _(Open: how it stacks and how it is cleared.)_
- **Darkness:** Will Break also lowers the lighting and the player's visibility.

| Enemy | Behavior | Damage | Health | Notes |
|---|---|---|---|---|
| **Fallen Defender** | A risen skeleton that chases the player. On death it leaves a **Pile of Bones**; if not destroyed quickly, a new Fallen Defender rises from it | Low | High | |
| **Fallen Archer** | Like the Fallen Defender, but shoots black arrows (also leaves bones) | High | Low | |
| **Awoken Summoner** | Shoots strong projectiles and has a chance to summon an enemy from a previous act | High | Low | |
| **Amalgamated Meat** | Moves slowly and randomly, not toward the player. Releases 3 Darkness Shooters on death | Low | Low | |
| **Darkness Shooter** | Like the Flame Shooter, but fires very fast in random directions | Medium | Low | Only spawns from the Amalgamated Meat |
| **Cursed Sword** | A sword that spins toward the player. It can only be killed after it enters a Pool of Corruption, which turns it into a Fallen Defender | TBD | TBD | Puzzle enemy: lure it into a pool |
| **Revenant** | A powerful Dreko soldier that throws spears while chasing the player | High | Medium | Fast |
| **Echo of Gabien (Corruptor)** | The burnt echo of Gabien's Seared Soul, bound to the ruins where he fell and twisted by the darkness he let loose. Shoots projectiles and flees from the player | – | – | A story beat for Ayteel |

### Bosses

#### Act I: Taio'Sandir
**Lore.** Its original name is unknown; Taio'Sandir means "impious" in the Forgotten Language. It is an ancient Dreko that came during the Dark Age, like all of them, and took the body of a soldier. Over thousands of years that body changed and mutated beyond recognition into today's monstrosity. Tales from the Dark Age say its infernal screams drew corrupted creatures and vermin, and its attacks shattered armor and even small buildings. Worst of all, despite its size it could vanish into the mist that rose in its presence and strike soldiers by surprise.

**Phase 1: Light the altar.** The player stands at the Altar of Malien while its Flame of Faith fills over time (about 100 s). The boss can't be damaged.
- Enemies keep spawning from portals.
- **Bolt of Darkness:** the boss emerges from the mist at a random point, throws a dark bolt at the player, and fades away again.
- **Summon Eyes** (every 30 s): it summons Dreko Eyes that walk to the altar instead of attacking. Each one that reaches the altar lowers the flame. While the eyes walk, the boss throws bolts more often (every 3 s).
- Killing the eyes enrages the boss: each time, its bolts fire more projectiles (up to a cap) and the next summon brings an extra eye.

**Phase 2: The real fight.** When the flame is full, the boss is struck by it (transition animation) and becomes vulnerable. Enemy spawns stop, and the boss patrols between points, casting:
- **Bolt of Darkness** every 5 s.
- **Corruption Burst** (every 15 s): a 5-second channel firing a stream of small projectiles in an arc toward the player.
- **Summon Dark Drone** (every 30 s): a slow projectile that follows the player until the boss dies.

The boss is Corrupted by default (more damage). Killing it ends the act and grants +5 Favors.

**Animations:** Idle, Walk, Attack (throw), Channel / channel loop, Fade In / Fade Out (mist), Damage (phase transition), Die.

#### Act II: The Ice Bull
Mechanics **TBD**.

#### Act III: Forsaken Daviro the Unholy
**Lore.** Daviro Piktuon was secretary to the governor of the Argent Coast, Gabien's home town. Ruined and about to end his life, he was taken in by the Forsaken in Gabien's time. Sixty years later he waits in the Sinfa Mountains. How he has lived so long is **TBD** (see [`Lore.md`](Lore.md)).

Mechanics **TBD**.

#### Act IV: TBD
Name and mechanics **TBD**.

_Proposal for Acts II–IV:_ keep the Act I structure (an invulnerable phase while the Altar of Malien fills, then the real fight). Each boss would use its act's mechanic, for example the Ice Bull freezing the player, Daviro spawning Pools of Corruption, and the Act IV boss using Will Break and Darkness.

---

## 11 – Cutscenes

- **Format:** static illustrations with text (no animation or voice acting).
- **When:** one cutscene at the **start of each act** and one at the **end of each act**: 8 in total for 1.0.
  - **Act I intro:** the Pilgrimage of the Flame, the extinguished altars, and the three pilgrims setting out.
  - **Act ends:** the Altar of Malien is relit and the next region comes into view.
  - **Act IV end:** the game's ending.
  - The content of each scene is **TBD**.
- **Current state:** no cutscenes yet. Story is delivered through event texts, item descriptions, map and level names, and the boss's camera focus and phase-transition animation.

---

## 12 – Bonus materials

- **Meta-progression:** class unlocks and skill tree tiers (already in the game).
- **Steam achievements** (planned for 1.0, see [Platform-specific features](#platform-specific-features)).
- **Hard mode** _(not planned, but not ruled out)_: if added, it will be a harder mode only; there will be no easier mode.
- **Key items** that unlock parts of the overworld map: planned, design to be discussed.
- **Ideas, not planned:** more classes, a lore codex unlocked by hidden objects ("discover the history of the world of Kanator"), post-1.0 content updates.

---

## 13 – Open questions

_Answered on 2026-10-23 and integrated above: ship and demo dates, the three pilgrims, Acts II–IV mechanics and enemies, Chaos and Holy (to implement), cutscene format, Steam features, difficulty, localization and DLC._

1. **Bosses:** the Ice Bull's and Forsaken Daviro the Unholy's mechanics; the Act IV boss's name and mechanics.
2. **Corruptor names:** Act I is "Dreko Spawn" for now (a better name may come later); Acts II–IV are the **Echo of Malizante**, the **Echo of Pal'bor** and the **Echo of Gabien** (decided 2026-10-26). The Act III boss is **Forsaken Daviro the Unholy** (decided 2026-10-29).
3. **Frozen Enemies (Act II):** is breaking the ice optional, and does it reward anything?
4. **Will Break (Act IV):** how it stacks, how it carries over between levels, and how the player clears it.
5. **Act mechanics carrying over:** do Frozen and the Frozen Enemies continue into Acts III–IV? (Pools of Corruption do continue into Act IV.)
6. **Missing values:** the Crawler's and the Cursed Sword's damage and health.
7. **Cutscene content:** what each of the 8 scenes shows.
8. **Achievements:** the final list.
9. **Key items:** design.
10. **Hard mode:** whether to make it, and what it changes.
11. **Epic and Legendary items:** none exist yet.
12. **Localization extras:** whether Chinese and Japanese make it into 1.0.
13. **Lore:** the timeline and the other contradictions were settled on 2026-10-26 and 2026-10-29 (see [`Lore.md`](Lore.md) and `docs/features/run-encounter-events.md`, "Lore decisions"). Gabien's sword is **Ashem Elwyn** ("Shining Tide"), and the Faraclarian Faun Chief is not of Alamoh blood. Still open: how Daviro has lived sixty years past Gabien's time.
14. **Run structure across acts (decision needed):** two options.
    - **One run through all acts in a row:** a run goes Act I → IV without stopping, and death means starting over from Act I. Builds grow across the whole game and the ending is earned in one sitting, but runs get long (4 acts) and early acts get replayed a lot.
    - **One act per run:** after beating an act, the player goes back to the main menu and chooses to replay a cleared act or start the next one. Runs stay short and later acts are reachable without replaying Act I. Open points with this option: what a new act starts with (a fresh level 1 kit or a scaled starting point), how difficulty is set, and how Favors are paced.

    The choice affects run saves, difficulty scaling (`levelsBeaten`, the expected-player curve), Favor pacing, the cutscene placement and the achievements.
