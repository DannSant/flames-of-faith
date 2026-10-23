# Flames of Faith – Ten Page GDD

_Structure follows the Ten Page GDD from Scott Rogers, *Level Up! The Guide to Great Video Game Design*. Last updated 2026-10-23. Where this document and the code disagree, the code (and `CLAUDE.md`) wins; please fix the document. Items marked **TBD** or **Proposed** are not decided yet and are collected in [Open questions](#13--open-questions)._

---

## 1 – Title page

| | |
|---|---|
| **Title** | Flames of Faith |
| **Genre** | 2D isometric action roguelite with survivor-like combat |
| **Platforms** | PC (Steam) first; Xbox possibly later |
| **Engine** | Unity 6.3 LTS (URP 2D) |
| **Target rating** | Teen / PEGI 16 (fantasy violence, dark religious and horror themes) |
| **Target audience** | Players of roguelites and survivor-likes (*Death Must Die*, *Brotato*, *Vampire Survivors*, *Hades*) who want short, replayable runs with build-crafting and meta-progression |
| **Ship date** | **TBD**. A playable Act I demo already exists for testers (v0.5.1). |
| **Monetization** | Premium, one-time purchase. No microtransactions. DLC TBD. |
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
| **Steam demo** | Act I (Jesterwoods): 3 map variants, combat levels, shop, campfire, treasure, events, Holy Altar, Act I boss. Three classes, skill trees, Favors, run save. Keyboard/mouse and gamepad. | Exists and in tester builds |
| **Full Steam release** | Acts I–IV, each with its own region, enemies, Corruptor and boss. More items, events and kit upgrades. Localization and input remapping. | Planned; Acts II–IV are **Proposed / TBD** |
| **Later** | Xbox port; possible DLC | Not scheduled |

### Out of scope (for the full release)
- Multiplayer or co-op.
- Procedurally generated level layouts (levels are hand-built scenes; variety comes from the map, waves and loot).
- Story cutscenes with voice acting (see [Cutscenes](#11--cutscenes)).

---

## 3 – Story and gameplay overview

### Story
The story takes place shortly after the year 1000 of the Decadent Age, or so it is believed: the chaos of the era has left the records confused. In a hard time of hunger, crime and corruption, strange things began to happen: sudden illnesses, unknown plagues, food that spoiled from one day to the next. The augurs of the White Church concluded that the cause might be the **Pilgrimage of the Flame**, an ancient and sacred ritual that had not been performed for centuries because of the war that had only just ended. Evil had begun to take root in the world.

All the altars that kept the **Flame of Faith** alight have gone out, and the **Years of the Miasma** have begun. The region of **Ashemdrek** is not what it used to be: it seems to shift with every step, and horrible creatures lurk around every corner. To bring order back, the pilgrims must cross the perilous **Jester Forest (Jesterwoods)**, the frozen **Uf'Faraclario Steppes**, the desolate **Sinfa Mountains**, and finally the **Ruins of Ashemdrek**, relighting the **Altars of Malien** and restoring the Flames of Faith.

**Playable characters.** The player chooses one of **three pilgrims**, one per class (Warrior, Archer, Augur), each with their own background. Their names and stories are **TBD**. The earlier draft's heroine, **Ayteel Alamoh**, a Guardian descended from the infamous Gabien Alamoh who joins the Pilgrimage to atone for her ancestor's sins, can become one of them or stay as lore.

**Enemies of the story:** mercenaries, bandits, corrupted animals and, above all, the mysterious **Drekos** and their twisted creations.

### Gameplay summary
Each run is a pilgrimage through an act. The player moves across a branching **overworld map** of nodes, choosing a path toward the act's boss. Combat nodes are isometric arenas where waves of enemies spawn for a fixed time. The player auto-attacks (or attacks manually), uses a class special and a dash, collects experience tokens to level up, and picks stat upgrades. Between fights the player meets shops, campfires, treasure chests and random events. **Grace** rises and falls at the end of each wave, changing the player's damage and, below zero, making them Corrupted. Dying ends the run; the **Favors** earned are banked and spent on class unlocks and skill tree upgrades.

---

## 4 – Game flow

```
Main menu ──> Class select ──> Overworld map (Act N)
                                   │
                    ┌──────────────┼───────────────┬───────────────┐
                 Combat         Shop / Campfire   Treasure / Event   Boss
                 (waves)        / Holy Altar                         │
                    │                                                ├─> next act (full game)
                    └────────── back to the map ◄────────────────────┘
Death or final victory ──> Favors banked ──> Skill tree (main menu) ──> new run
```

### A run, step by step
1. **Main menu.** New Game, Load Game (continue the single saved run), Settings, Skill Tree, Exit.
2. **Class select.** Choose Warrior, Archer or Augur (Archer and Augur start locked). The details show the starting weapon at its upgraded tier.
3. **Overworld map.** The act's map is picked at random from its variants (Act I has 3) and generated from the run's seed. Nodes are revealed as you advance (fog of war); you can only move forward along edges, never back. Some optional paths appear only in some runs. Each combat node shows its **Taint Level**.
4. **Node types:**
   - **Combat:** 2 waves of enemies (wave durations of 30–45 s in Act I).
   - **Shop** (Elven Shop): buy items with coins.
   - **Rest** (Campfire): gain Grace and health.
   - **Treasure:** a chest with one reward.
   - **Event** (Strange Encounter): a random story event with 2–3 options.
   - **Holy Altar:** a fixed event focused on Grace.
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
- **Golden Chronos (coins):** granted at the end of every wave, from events and treasure, and by selling items. Spent in shops.
- **Experience tokens:** dropped by enemies in several denominations (bigger tokens from tougher enemies). Leveling up offers a choice of stat upgrades.
- **Grace pickups:** occasional drops; more likely with Grace Affinity.
- **Health pickups** and **random item pickups** (the item goes to the item bag).

---

## 5 – Characters and controls

### Classes
Each class has a **kit** of six starting items: a weapon, an off-hand, an Emblem and three **Glyphs** (one per ability slot). Each kit item has upgrade tiers (+1 to +3) bought with Favors in the class's skill tree. Upgrades apply from the next run.

| | **Warrior** | **Archer** | **Augur** |
|---|---|---|---|
| **Role** | Melee bruiser | Ranged skirmisher | Magic caster, area damage |
| **Weapon** | Steeldawn Sword | Steeldawn Bow | Akeros Graceful Scepter |
| **Off-hand** | Shield of the Crusader (sturdy, slower) | Quiver of the Centinel (longer range, less protection) | Ley Shield (stronger debuffs, less protection) |
| **Emblem** (+1 main stat per level) | Warrior Emblem: Melee Damage | Archer Emblem: Ranged Damage | Augur Emblem: Magic Damage |
| **Basic attack** | Glyph of Crusader Strike: a sword swing in an arc; may inflict Bleed | Glyph of Holy Shot: a blessed arrow; upgrades add a cone of extra arrows | Glyph of Fireball: a fireball; may inflict Fire; upgrades add an impact explosion |
| **Special** | Glyph of Whirlwind: spin hitting everything around you, invulnerable while spinning; upgrades add a holy shield that absorbs enemy projectiles and heals | Glyph of Barrage: a ring of arrows, then homing arrows at the marked target; invulnerable during it | Glyph of Fire Tornadoes: seeking fire tornadoes, then explosions on the marked target; upgrades make the tornadoes explode |
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
- **Lock-on** marks one enemy (Toggle mode by default, or Hold mode in Settings). A locked target stays marked even out of range, and specials (Barrage, Fire Tornado explosions) aim at it.
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
- **Other Grace sources:** the Campfire, the Holy Altar and some events, Grace pickups, and Retribution hits.
- **Corrupted enemies** have more health and deal more damage. Their chance to appear grows with the level's **Taint Level** and the player's Corrupted level.

### Mechanic: Favors and meta-progression
- **Earned:** +1 for each wave that ends with Grace above 0, +5 for a boss kill, and from some events.
- **Banking:** Favors count only when the level they were earned in is finished. They are banked only when the run ends (death or victory). Abandoning a run loses them.
- **Spending** (between runs, in the Skill Tree): unlock classes (50 Favors), and upgrade each kit item one tier at a time (10 / 15 / 20 Favors). Favors are one shared pool across classes.
- A typical lost run earns about 12–18 Favors.

### Platform-specific features
- **PC (Steam):** keyboard and mouse or gamepad, with automatic glyph switching; scalable UI from 1920×1080; resolution and quality settings.
- **Steam features** (achievements, cloud saves, Steam Deck verification): **TBD**.
- **Xbox:** possible later port. The UI is already fully gamepad-navigable.

---

## 7 – Game world

Four regions, one per act. Each act is a branching map of about 25 nodes ending at an **Altar of Malien** guarded by the act's boss. You cannot go back once you leave a node.

| Act | Region | Mood | Status |
|---|---|---|---|
| I | **Jesterwoods** (Jester Forest) | Ordinary fantasy forest, lonely and a little gloomy | Built: 3 map variants. Levels include Jesterwood Glade, Jesterwood Cliffs, Heart of the Forest, Perkfandor Outskirts, Perkfandor Ruins, Moribairo Fields, Argent Coast Ruins, Seared Ruins and Sinfa Hillside, plus Campfire, Elven Shop, Treasure, Strange Encounter, Holy Altar and the Altar of Malien (boss) |
| II | **Uf'Faraclario Steppes** | Snow, cold, desolate | **Proposed** |
| III | **Sinfa Mountains** | Mountainous, rocky, desolate, cliffs | **Proposed** |
| IV | **Ruins of Ashemdrek** | Urban, destroyed, ruins, dark | **Proposed** |

Combat levels are hand-built isometric arenas with a few obstacles (tree trunks) that block enemy projectiles. The art team is redesigning the scenarios, which may make levels smaller.

---

## 8 – Interface

The UI follows one standard: two font sizes, three icon sizes, two button sizes, framed windows, and a color code (Grace cyan, Corruption purple, Favors gold, yellow highlight, green positive, red negative). Every window works with mouse and gamepad; a pulsing frame shows the gamepad selection.

- **HUD:**
  - **Top left:** health, the signed Grace bar with the wave's pending Corruption, the wave number and its timer. Under it, an optional tutorial hint.
  - **Top right:** level and experience bar.
  - **Bottom left:** the three ability icons with cooldowns and input glyphs, plus the auto-attack and lock-on indicators.
  - **Bottom center:** the boss's altar flame bar (boss level only).
- **Side panes:** the **Inventory** (left) and **Stats** pane (right) open with every center window. Holding Shift / LT shows which items give each stat.
- **Center windows:** level-up (with Now/After stat previews), item bag (take or sell), wave results, shop, campfire, event and results, pause, settings, death screen and victory screen.
- **Overworld map:** act title, current level name, coins, Save & Exit, the Stats pane, and gamepad prompts.
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
| Chaos, Holy | **Proposed** (designed, not implemented): Chaos makes enemies explode and spread Chaos; Holy may stun and heal the player |

### Run encounters (events)
Every event has 2–3 options, and each option costs something and gives something (no free "Ignore"). Outcomes use Luck-biased rolls. The current pool: Arcane Box, Bandits, Cursed Chest, Faraclarian Clan and House of Stars, plus the Holy Altar. Events can show art, but none has art yet.

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
| **Corruptor (Act I)** | Always Corrupted; appears at every wave end, flees from the player and shoots; escapes through a portal if not killed in time |

Acts II–IV enemies and Corruptors: **TBD**.

### Bosses

#### Act I: Taio'Sandir
**Lore.** Its original name is unknown; Taio'Sandir means "impious" in the Forgotten Language. It is an ancient Dreko that came during the Dark Age, like all of them, and took the body of a soldier. Over some 6,000 years that body changed and mutated beyond recognition into today's monstrosity. Tales from the Dark Age say its infernal screams drew corrupted creatures and vermin, and its attacks shattered armor and even small buildings. Worst of all, despite its size it could vanish into the mist that rose in its presence and strike soldiers by surprise.

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

#### Acts II–IV bosses
**TBD.** Each is proposed to follow the same two-phase altar structure (survive while the altar fills, then fight), with a mechanic tied to its region.

---

## 11 – Cutscenes

- **Current:** no cutscenes. Story is delivered through event texts, item descriptions, map and level names, and the boss's camera focus and phase-transition animation.
- **Proposed:** a short illustrated intro (still images and text) explaining the Pilgrimage of the Flame, a short scene when each altar is lit, and an ending scene. Scope **TBD**.

---

## 12 – Bonus materials

- **Meta-progression:** class unlocks and skill tree tiers (already in the game).
- **Proposed:** more classes, a lore codex unlocked by hidden objects ("discover the history of the world of Kanator"), higher difficulty levels, Steam achievements. All **TBD**.

---

## 13 – Open questions

1. **Ship date** and demo release date on Steam.
2. **The three pilgrims:** names, backgrounds, and whether Ayteel Alamoh is one of them.
3. **Acts II–IV:** enemies, Corruptors, bosses and their mechanics.
4. **Chaos and Holy debuffs:** implement or cut.
5. **Cutscenes:** intro / altar / ending, and in what format.
6. **Steam features:** achievements, cloud saves, Steam Deck support.
7. **Difficulty levels:** the code has a difficulty setting; what should each level change?
8. **Localization:** which languages (the design docs started in Spanish).
9. **Key items** that unlock parts of the map (in the backlog): design needed.
10. **DLC:** any planned content after release.
