# Data-driven analysis (todo.md Priority 3)

Analysis only, no code changes. Written 2026-10-21 from the code and assets on `main` (after v0.5.1).

**Progress:** the pilot (enemies, waves, XP) is underway; see CLAUDE.md "Balance data". Step 1, the cleanup, was done on 2026-10-22.

**The question:** what would it take to make every system data-driven, with its numbers fed from a database that a game designer can edit outside Unity, the way the Effects already come from a database?

## 1. Verdict

**It's feasible, and cheaper than it sounds.** The game is already mostly data-driven:
- **Few hardcoded numbers.** Nearly every tunable is a serialized field or sits on a settings asset, because that has been a project rule.
- **Seven settings assets** hold the global rules: Corruption, Enemy Damage, Experience, Player Stats, Pierce, Meta-progression and the Stat Database.
- **About 200 content assets**: enemies, waves, levels, debuffs, abilities, events and more.

The real problem is that the numbers are **scattered**. For one item, a designer would have to look in the database, two behavior assets, a prefab, a debuff asset and the code (see 3.6). So Phase 1 is mostly about **consolidating and addressing** the numbers, not rewriting systems. Only one system needs a real refactor: item drops, which today run through four separate code paths.

**For Phase 2, I recommend Google Sheets as the designer's tool instead of a custom web app:**
- The designer edits a sheet, and a "Publish" step freezes a version of it.
- In Unity, a menu option pulls that version, validates it and writes it into the assets. The developer commits the result.
- Tester builds can also load the latest published version at startup, so the designer can play his own numbers without Unity and without waiting for a new build.

**The Supabase web app from `todo.md` would work too,** but it means building and maintaining an editing form for some 25 tables. A spreadsheet gives you those for free, and designers already work in spreadsheets. A studio that tried per-table web forms abandoned them for that reason (section 6.1).

**Start with a pilot:** convert enemies, waves and XP (the combat curve) end to end, about 2–3 weeks, and have the designer use it before converting the rest.

**Rough total:** about 4–7 weeks of developer time for all eight systems plus the pipeline, and 1.5–3 more for the other modules suggested in section 4. Section 7 has the breakdown.

## 2. Where the numbers live today

| Place | What's there | Can the designer edit it without Unity? |
|---|---|---|
| **ScriptableObject assets** (`Assets/Resources/`) | **Combat:** 8 enemies, ~31 enemy AI behaviors, 6 boss abilities and 5 boss conditions, 5 debuffs, 6 weapons. **Run content:** 11 wave databases and 23 waves, 18 levels and 3 maps, 25 item behaviors, 6 events (16 option lists, 24 option actions), 3 treasure tables, 3 skill trees. **Rules:** the 7 settings assets. | Not in practice. The files are Unity YAML: references are GUIDs, and int arrays are stored as hex (the Stat Database's level-up tiers read `050000000a000000…`). |
| **Prefab and scene components** | About 270 numeric serialized fields on components, part of them design numbers (listed below the table). | No. |
| **SQLite database** (`Database/effects.db`) | 115 item rows: 43 lootable items and 72 class-kit tier rows. An Editor tool generates the item assets from it. | Only with a SQLite tool. The file is binary in git: 52 commits whose diffs can't be read, reviewed or merged. |
| **Code** | **Formula shapes,** such as how enemy health grows, the damage formula and the stat curves. Their parameters are exposed already. **A few hardcoded numbers:** Frost's 80% slow cap, Ley Charge's 6-unit radius and 3 jumps (on a component added at runtime, so not even Inspector-tunable), and how the treasure picks a "random" reward type. | No, and it shouldn't be: formula shapes stay code (see 5). |

**Design numbers on prefabs and scenes:**
- **`PlayerBase.prefab`:** the XP curve (10 × 1.2^(level − 1)), the Grace range and damage per Grace point, the Retribution curve, move speed, invulnerability after a hit.
- **`EnemyBase.prefab`:** the 5% pickup drop chance and its pool.
- **`RandomItemPickup.prefab`:** the 16 items it can give, with weights.
- **16 damage-source prefabs:** the base damage of every item and ability projectile.
- **Economy:** coins per wave (`CurrencyGenerator.prefab`), shop size, the health pickup.
- **Movement:** the dash components, and the Bow and Scepter specials.
- **Projectiles:** their speeds.
- **Levels:** each level scene's spawner settings.

**Dead or misleading data** that a designer would edit to no effect:
- **Waves:** `WaveData.enemies` (fixed amounts) and `WaveData.cooldownDuration` show in the Inspector but nothing reads them.
- **Enemies:** `EnemyData.attackRangeBase` is never read.
- **Treasure:** `CurrencyTreasure` and `ExperienceTreasure` are unused; the Treasure scene only uses `ItemTreasure`.
- **XP drop behaviors:** the 6 assets still carry old `dropChance` / `xpAmount` values that the code ignores, since `ExperienceSettings` replaced them.
- **Health pickup:** the prefab still stores an old `healAmount: 3`. The heal really comes from the code defaults (5 + 2 per level).
- **Test leftovers:** `testWave`, `TestWaveDatabase`, `WarriorOld`. (`PhaseOneBoltOfDarkness 1` looked like a duplicate, but the boss uses it as its bolt during the eyes phase. It was renamed `PhaseOneBoltOfDarknessDuringEyes`.)
- **Found during the cleanup** (deleted 2026-10-22):
  - `Boss1.unity` and `Sandbox2.unity` (`Sandbox1` stays: the Bootstrapper loads it);
  - the `Store2` and `Rest1` level data;
  - the `Boss1` wave database;
  - `SlimeRoamBounce`, which only changed its own private state.

  Also found: Fire Arrows had lost its fire behavior since the Oct 18 rename of its file (fixed).
- **Mixed units:** `EnemyData.speedBase` means three different things. For hopping slimes it's a push force (100–150). For chasers it's units per second (1–3). DrekoEye ignores it and uses its move behavior's own `speed`.

## 3. The designer's systems

Each system lists what exists today, what's missing for an external table, the Phase 1 work and a size. Sizes: **XS** ≤ 0.5 day, **S** 1–2 days, **M** 2–4 days, **L** 4–6 days of developer time.

### 3.1 Enemy stats, damage, health, expected damage per wave/level

**Today:**
- **`EnemyData`** (8 assets) holds the base health, health per wave and per level beaten, contact and projectile damage tiers, speed, XP tier and the Corrupted overrides.
  - Health = `healthBase + (wave − 1) × healthPerWave + levelsBeaten × healthPerLevel`, then × (1 + Corrupted bonus). The formula is in `Enemy.Initialize`.
  - The boss only uses `healthBase` (2000) and doesn't scale.
- **Damage** comes from tiers against an expected player (`EnemyDamageSettings`: expected health and armor curve, hits-to-kill per tier, armor constants).
- **Attack behavior** lives on ~31 small AI behavior assets: cooldowns, ranges, shots per volley, spread angle, contact-damage cooldown, flee distances. Boss abilities have their own damage on 6 assets.
- **"Expected damage per wave/level" is an output,** not an input. The Editor window *Enemy Damage Preview* already computes it (raw damage and hits-to-die per enemy, at every point of the run).

**Gaps:**
- **Behavior numbers are polymorphic.** A shooter, a chaser and a melee attacker each have different fields, so they need one table per behavior type.
- **Data cleanup:** fix the speed units and remove `attackRangeBase`.
- **The designer can't see the outputs** (expected damage, enemy health per level) without Unity.

**Phase 1:**
- Key enemies by `EnemyType` name.
- Give each behavior type a table keyed by asset name.
- Split `speedBase` into move speed and hop force.
- Optionally, turn the linear health growth into a per-level table, if the designer wants control over the curve's shape.
- Export the preview's numbers as a report (section 5).

**Size:** M.

### 3.2 Experience drops and XP needed per level

**Today:** `ExperienceSettings` drives the drops:
- drop chance, target levels per wave, expected kill ratio, cooldown blend;
- the XP bonus per Experience Reduction point;
- the token tiers (promote chance and value multiplier).

`EnemyData.xpTier` caps each enemy's token tier.

**Gaps:**
- **The XP curve is on the player prefab:** `PlayerExperience` holds 10 and 1.2 on `PlayerBase.prefab`.
- **Treasure XP is a flat 20–40,** not sized to the player's level like wave XP. It only matters if the Experience treasure comes back.
- **Leftover drop values:** the old per-enemy drop chances on the drop behavior assets look editable but are ignored.

**Phase 1:** move the curve to `ExperienceSettings` and clean the leftover fields.

**Size:** XS.

### 3.3 Wave settings: enemies spawned, frequency, chance weights

**Today:**
- **One `WaveDatabase` per level** lists its waves in order. Each level scene's `WaveSpawner` points to it and holds the spawn zones and the minimum spawn distance.
- **Each `WaveData`** has:
  - its duration;
  - the enemy pool (type + weight);
  - the long and regular spawn cooldowns and how many enemies get the long one;
  - an override for the expected enemy count;
  - the Corrupted chance multiplier;
  - the Corruptor settings.
- **Corrupted spawn chances** are global on `CorruptionSettings`, plus each level's `taintLevel`.

**Gaps:**
- **Dead fields:** `enemies` and `cooldownDuration` (section 2).
- **Adding a wave means creating an asset** and inserting it in the list, so the importer must create assets, not just update them.

**Phase 1:**
- Use two tables:
  - **Waves:** level, wave number, duration, cooldowns, Corrupted multiplier, Corruptor.
  - **Wave enemies:** level, wave number, enemy, weight.
- Remove the dead fields.
- Write an importer that creates missing wave assets in order.

**Size:** M.

### 3.4 Item drops: treasure, random pickups and shop

**Today:** four sources, each with its own pool and its own roll:

| Source | Pool | How it rolls | Luck |
|---|---|---|---|
| Enemy pickup | `EnemyBase.prefab`: 5% chance, then the item or health pickup; `RandomItemPickup.prefab`: 16 hand-picked items with weights 2–20 | flat weights | no |
| Treasure | `ItemTreasure.asset`: 12 items, weight 50 each | flat weights | no |
| Event | 2 `GiveRandomItemOptionAction` assets with their own lists | quality rolled by a stat (Luck), then weights | yes |
| Shop | every item flagged for the shop in the DB, minus starting items | 10 + Luck items (max 20), quality rolled per slot by Luck, uniform within it; price × discount | yes |

The Grace pickup is rolled separately (`CorruptionSettings`, scaled by Grace Affinity).

**Gaps:**
- **New items need manual list updates.** A new item has to be added to up to three hand-kept lists, and nothing tells you which.
- **Luck is inconsistent:** it affects two sources and not the other two.
- **Rolls are spread across four classes** and a prefab.

**Phase 1:** this is the one real refactor, matching what `todo.md` suggests.
- **An `ItemProvider` service plus `LootTable` assets.** A table has an id, a roll mode (flat weights, or quality by a stat and then weights), a bias stat, a quality scale, a count and whether results must be unique.
- **Entries** name an item, or a rule such as "quality ≥ Rare" or "tag Arcane". Item tags would be a new DB column.
- **All four sources** (the pickup, the treasure, the event action and the shop) call the provider with their table id.
- **Designer tables:** Loot tables, plus Loot table entries.

**Size:** L, including tests.

### 3.5 Stat formulas and calculations

**Today:**
- **The formulas are pure functions** in `StatsCalculations`.
- **Their parameters** come from:
  - `PlayerStatSettings` (regen, life steal, dash, shop discount, pickup range, level-up tier weights);
  - `EnemyDamageSettings` (armor, Evasion);
  - `PierceSettings`;
  - `WeaponData` (attack cooldown and attack speed scale).
- **Each stat's starting value and level-up tier values** (1/2/3/4, Max Health 5/10/15/20) are in `StatDatabase`. So are whether level-up can offer it and its Corruption reduction.
- **The tooltips use the same parameters,** so they stay right when numbers change.

**Gaps:** the rest of the parameters are on player components:
- the Retribution curve (16 fields on `RetributionHandler`);
- move speed base and scale (`PlayerController`);
- the dash cooldown, speed, duration and charges (dash components);
- invulnerability after a hit.

**Phase 1:**
- Move those onto `PlayerStatSettings`, keeping per-class values where the classes differ (dash).
- Tables:
  - **Stats:** start value, tier values, upgradeable, Corruption factor.
  - **Stat formula parameters:** one row per parameter.

**What stays code:** formula shapes. The designer tunes parameters; a new shape (say, a soft cap on Armor) is a code request. Section 5 explains why.

**Size:** S.

### 3.6 Item damage and scaling

**Today, one item, "Glyph of Swords of Fire"** (Heroic, 150 coins):

| What | Value | Where |
|---|---|---|
| Quality, prices, shop flag, stat scale | Heroic, 150 / 50, 1.0 | `Database/effects.db` |
| Swords spawned, stacking | 2 swords, +1 sword per extra copy | `SpawnOrbitingSwords.asset` |
| Base damage, damage stat | 5, Melee (adds Melee Damage), ×1 | `OrbitingSword.prefab` |
| Fire on sword hits, and on melee attacks | Fire Debuff | `OrbitingSword.prefab`, `MeleeFireElementalInfusion.asset` |
| Fire itself | 3 s + 0.25 s per Mastow Affinity, 3 + 1 per Mastow Affinity damage every 0.5 s, 50% chance, 35% spread | `FireDebuff.asset` |
| Formula | `(base + stat × item scale × prefab scale) × stack multiplier × Grace multiplier` | `DamageCalculator.cs` |

That's five places for one item. The designer's example ("5 base damage, +1 per Melee Damage, +2 per copy") maps onto it directly. The parts exist; they're spread out.

Two more facts matter:
- **Prefab base damage:** the base damage of every item projectile sits on 16 prefabs. A prefab used by two items can't give them different damage.
- **Class abilities** keep their numbers in per-tier tables on their ability assets, for example Fireball +0..+3 at 12/13/14/15 damage with an impact explosion of 0/4/6/8. The weapon's `WeaponData.baseDamage` is the fallback when the Glyph is missing.

**Phase 1:**
- **Damage profiles on behaviors.** Give each spawning behavior a damage profile (base damage, damage stat, stat scale) and push it into the spawned prefab. The method for that, `DamageSourceBase.Initialize`, already exists for weapons. Prefabs keep only visuals and physics.
- **The cheaper alternative,** where the importer writes into the prefabs instead, works but keeps the "one prefab, one damage" limit.
- **Turn the DB's three JSON columns into child tables:**
  - **Items:** id, name, quality, prices, flags, tier, tags.
  - **Item stat modifiers:** item, stat, value, flat or %.
  - **Item behaviors:** item, behavior id.
  - **Stat bonus rules.**
- **Per behavior type, tables of parameters:** damage profile, count, cooldown, stack rules (target, per stack, mode, cap).
- **Per ability, tier tables.**
- **The Effects database moves into the external source** (section 6), and the Effect Database window retires or becomes read-only.

**Size:** L. It's the largest system, by count of moving parts.

### 3.7 Debuffs: scaling with Mastow Affinity, Bleed stacking

**Today:** these are almost table-ready. Each `ElementalDebuffData` (5 assets: Bleed, Bleed High Chance, Fire, Frost, Ley Charge) holds:
- base duration and strength, each + N per Mastow Affinity point;
- the chance to apply and the extra chance to reapply;
- the tick interval;
- the contact spread (chance, cooldown, generations, falloffs);
- the stacking (max stacks, refresh, burst per stack).

The formulas: duration = base + Mastow × scale; strength = base + Mastow × scale. Bleed deals strength × stacks per tick, then bursts for stacks × burst on expiry.

**Gaps:**
- **Two hardcoded numbers:** Frost's 80% slow cap, and Ley Charge's radius and jump count (`handoff-2026-10-21.md` open points).
- **Frost barely scales:** its Mastow scale is 0.001, so each point adds 0.1% slow. The designer should know.
- **The open Frost double-damage bug** should be fixed before tuning Frost.

**Phase 1:** move the three numbers onto the debuff asset; then it's one table, a row per debuff.

**Size:** XS–S.

### 3.8 Grace and Corruption modifiers

**Today:**
- **`CorruptionSettings`** holds nearly everything: Corrupted spawn chances, damage and health, Corruption from damage, the Grace pickup, the Corruptor phase.
- **The rest** sits on these:
  - `PlayerGrace` on the player prefab: the −50..+50 range, damage per Grace point (0.02) and the damage floor;
  - `StatDatabase`: Armor and Healing Received reduction per Corrupted level;
  - `LevelData.taintLevel` and `WaveData.corruptedChanceMultiplier`;
  - per-enemy overrides;
  - Retribution's Grace chance;
  - the campfire;
  - the events' Grace options.

**Phase 1:** move `PlayerGrace`'s numbers onto `CorruptionSettings`. Then it's one key/value table plus the per-level and per-wave columns that already exist.

**Size:** XS–S.

## 4. Other modules worth converting

| Module | Why | Numbers today | Size |
|---|---|---|---|
| **Class kits and abilities** | Class balance is the designer's core job. | 9 ability tier tables, 6 `WeaponData`, dash components on prefabs, Bow/Scepter specials | M |
| **Skill trees and Favors** | Progression pacing. | 3 skill tree assets (node costs 10/15/20, unlock 50), `MetaProgressionSettings` | XS |
| **Economy** | Coins in vs prices out is one balance. | Coins per wave on `CurrencyGenerator.prefab` (45–60 + 5 per wave), item prices in the DB, shop size on the Shopkeeper prefab, shop discount caps | S |
| **Run encounters** | Event risk vs reward. | 24 option actions (Gain/Lose ranges), treasure tables, campfire rest values | S |
| **Bosses** | The run's final check. | Boss health (no scaling), 6 ability assets, 5 conditions, boss movement on its prefab | S–M |
| **Player base stats and level-ups** | Start-of-run feel. | `StatDatabase`, Luck's level-up tier weights | XS (already assets) |
| **Pickups** | Small, and part of the economy. | Health pickup (5 + 2 per level), Grace pickup amount | XS |
| **Levels** (numbers only) | Difficulty per node. | `LevelData` taint, level type | XS |

**Leave in Unity:**
- **The map graph:** it's spatial, and has its own Map Editor.
- **Technical settings:** UI, input, audio, VFX, camera, enemy obstacle steering.
- **Art and prefab references:** the external data names things by id, and Unity owns which sprite or prefab an id means.

## 5. Cross-cutting Phase 1 work

These decisions apply to every system above:

1. **The runtime doesn't change.** ScriptableObjects stay the format the game reads. The external data is the *authoring* source, and an Editor importer bakes it into the assets, as the Effects DB does today. No database ships with the game, nothing needs network access, and gameplay code keeps reading the same assets.
2. **Names, never indices.** Stats and enemy types are stored by enum index in assets today. The external tables must use names (`MeleeDamage`, `SlimeChaser`) and stable item ids, and the importer maps them. Cleave showed why: removing one enum entry would have shifted every stat after it.
3. **One generic importer.**
   - **Each table declares** which asset type it feeds, its key column, and the column-to-field mapping, by field name. Child tables fill lists.
   - **It writes through `SerializedObject`,** so Undo and dirty flags work.
   - **It only touches mapped fields.** Sprites, prefabs and VFX stay Unity's.
   - **It replaces the Effects-only generator.**
4. **Rows are never deleted, only retired.** Saves store items by id and stats by name, so a deleted row breaks a saved run. A `retired` column follows the Stat Database's Retired flag.
5. **Validate on import:**
   - **types and ranges,** reusing the `[Min]`/`[Range]` attributes already on the fields;
   - **unknown ids** (an enemy or item that doesn't exist);
   - **the existing guard tests,** run after import. They already enforce several balance rules: enemy damage tuning, items with stat bonus rules never sold or rewarded, class kits.

   A failed import changes nothing and says which row is wrong.
6. **Clean the dead data first** (section 2), so the designer never tunes a field that does nothing.
7. **Formula shapes stay code; parameters become data.** Making formulas editable (expressions in the data) is possible, but it brings an expression parser, slower gameplay math, and tooltips that can't describe arbitrary formulas. Where the designer wants control over a shape, use a curve or a per-level table instead, such as enemy health per level.
8. **Computed reports for the designer.** Export the outputs next to the inputs:
   - expected enemy damage and hits-to-die per enemy at every point of the run (the Enemy Damage Preview's math is pure, in `EnemyDamageCalculator`);
   - enemy health per level and wave;
   - XP per wave and the levels it gives;
   - shop odds by Luck.

   This answers "expected damage per wave/level" without Unity.

## 6. Phase 2: changing numbers without Unity

### 6.1 How studios approach it

- **Spreadsheets are the usual authoring tool.** The usual pipeline treats the spreadsheet as the source of truth and generates the engine's data from it. Unreal's Data Tables import CSV this way, and guides warn that a re-import overwrites edits made in the engine, so you edit in one place only ([UE5 guide](https://sarahhyperdense.substack.com/p/data-tables-for-game-designers-spreadsheet), [Habr](https://habr.com/en/articles/737534/)). For Unity, libraries such as [BakingSheet](https://openupm.com/packages/com.cathei.bakingsheet/) (Excel, Google Sheets, CSV, JSON → typed C# data, with cross-sheet id references and validation; its repository was last updated in 2024) and [DataGraph](https://assetstore.unity.com/packages/tools/datagraph-5857361) (commercial, Google Sheets/Excel/CSV → ScriptableObjects) do the import.
- **Hutch Games' account (F1 Clash)** is the most useful real case ([Hutch tech blog](https://www.hutch.io/blog/tech/tech-blog-game-changer/)).
  - **Their first pipeline:** Google Sheets → JSON → SVN → upload. It broke for three reasons: the latest data sometimes lived only on one laptop; errors only showed up at upload, so bad data reached production more than once; and handoffs between people lost changes.
  - **The rewrite:** the sheet became the only source. Imports read it directly, validate it where the designer sees the errors, and version each import by a hash of its content.
  - **Before both,** they tried **a web form per data type and gave up on it:** building the forms was tedious and entering values one by one was slow.
  - **Their lessons:** build around the tools designers already use, shorten the pipeline, and validate where the person who made the mistake can act on it.
- **Dedicated data editors** such as [CastleDB](https://lib.haxe.org/p/castle) (a spreadsheet-like editor with a schema that saves git-diffable JSON) solve the schema problem. They still need a desktop install and git, and CastleDB's standalone editor is now legacy.
- **Remote config and LiveOps services** let teams change numbers in a web dashboard and deliver them to builds already in players' hands, without a new build: [Unity Remote Config](https://docs.unity.com/en-us/remote-config), [Balancy](https://assetstore.unity.com/packages/tools/game-toolkits/balancy-the-liveops-and-monetization-platform-for-games-128920), PlayFab, Firebase. They're aimed at live mobile games (segments, A/B tests), which is more than we need. But the idea, a build that fetches its numbers, solves our play-test problem (6.4).

### 6.2 Options for us

| | A. Google Sheets + Unity pull | B. Supabase + custom web app (current idea) | C. CSV/JSON files in git | D. LiveOps service |
|---|---|---|---|---|
| **Designer's experience** | A tool he knows, with formulas and charts for curves, bulk edits, comments and version history | Only the forms we build; no formulas | Edits text in GitHub's web editor, or needs git | A web dashboard, key/value oriented |
| **Schema and validation** | Weak in the sheet (dropdowns from id tabs, protected headers); strong in the importer | Strong (Postgres types, constraints) | Strong in CI | Medium |
| **Login** | Google accounts and sharing, free | Supabase Auth, set up by us | GitHub accounts | The service's |
| **What we build** | Sheet template, Publish script, importer transport | Web app: forms for about 25 tables, hosting, maintenance, plus the importer | CI validation, plus the importer | SDK integration, mapping |
| **Cost** | Free | Free tier pauses after 7 days idle; 2 projects, 500 MB ([pricing guide](https://automationatlas.io/answers/supabase-free-tier-limits-2026/)) | Free | Free tier, then paid |
| **Size** (the pipeline only; the tables come from Phase 1) | ~1 week | ~3–5 weeks | ~3–5 days | ~1–2 weeks, plus vendor lock-in |

### 6.3 Recommendation: option A

1. **The workbook:** one tab per table from Phase 1, plus id tabs that feed the dropdowns (enemies, items, stats, behaviors). Header rows and id columns are protected, and the computed report tabs are read-only.
2. **Publish:** a button in the sheet (Apps Script) checks the basics and saves a snapshot: JSON of every tab, plus a version number, author, note and content hash. That's the designer's "Submit". He can keep editing while the snapshot stays fixed.
3. **Pull in Unity:** the menu option *Tools → Flames of Faith → Pull Balance Data* (proposed name):
   1. downloads the latest snapshot (Google API with a service account);
   2. runs the Phase 1 importer and validation;
   3. writes the assets;
   4. saves the snapshot JSON in the repo;
   5. runs the guard tests.

   The developer commits `balance - snapshot 14: <designer's note>`. Git keeps a readable history: text YAML and JSON instead of the binary `effects.db`.
4. **Reports:** the same tool writes the computed tabs (section 5) back to the sheet after each pull.

### 6.4 Closing the play-test loop

A designer who can't play his changes is tuning blind. Without help, the loop is: the designer publishes, you pull, build and send a build, and he plays, which can take days.

**Proposal: a balance override in tester and dev builds.**
- **At startup** (or from a debug menu button), the build downloads the latest published snapshot and applies it to the loaded assets in memory, with the same field mapping as the importer.
- **Result:** the designer publishes, restarts the build and plays his numbers within minutes, with no Unity and no new build.
- **Release builds** keep this off and use only the baked assets.

**Two cautions:**
- **Editor protection:** in the Editor, changes to assets at runtime are saved to disk, so the override must stay off in the Editor.
- **Timing:** it has to run in the Bootstrapper, before anything caches a value in `Awake`.

**Size:** S–M.

Automatic builds in CI ([GameCI](https://game.ci/) on GitHub Actions) would remove the "you build" step too. But Unity Personal licenses have had activation problems in CI ([Unity forum](https://discussions.unity.com/t/unity-no-longer-supports-manual-activation-of-personal-licenses/926760)), and the override makes CI unnecessary for tuning.

### 6.5 When Supabase would be the better choice

Choose B (or B with Supabase only as the snapshot store, and the sheet kept for editing) if any of these holds:
- several designers edit the same data at the same time and need per-table permissions;
- the data needs strict relational integrity that dropdowns can't give;
- the game later needs live tuning of shipped builds.

None of these holds today with one designer and a demo. Everything above stays compatible with B: the importer reads a snapshot, and where it comes from is one small piece.

## 7. Effort and order

| Step | Contents | Size |
|---|---|---|
| 0. Cleanup | Dead fields and assets, speed units, Frost/Ley numbers onto assets, the Frost bug | S |
| 1. Importer core | Table → asset mapping, names not indices, retire not delete, validation, guard tests after import | M |
| 2. **Pilot:** enemies, waves, XP | 3.1–3.3, the XP curve, the enemy and XP reports; then the sheet, Publish and Pull (6.3), so the designer uses it for real | M, plus ~1 week for the pipeline |
| 3. Debuffs, Grace / Corruption, stat formulas | 3.5, 3.7, 3.8 (mostly moving prefab numbers onto settings) | S + S |
| 4. Item drops | 3.4: `ItemProvider` and loot tables | L |
| 5. Items and abilities | 3.6: damage profiles, the Effects DB migrated, child tables | L |
| 6. Balance override in builds | 6.4 | S–M |
| 7. Other modules | Section 4 | 1.5–3 weeks |

**Totals:** steps 0–6 add up to about 21–35 developer days (4–7 weeks); step 7 adds 7–14.

**Before you decide:**
- **Pilot first:** steps 0–2 (about 2–3 weeks) deliver the full pipeline for the combat curve. That answers whether the approach works for the designer before most of the cost is spent.
- **Steps are independent:** after the pilot, each step can be stopped or reordered.
- **Steps 4 and 5 are the big ones,** and the only ones that change gameplay code paths. Each needs a play-test round.

## 8. Questions for the designer and the team

1. **Spreadsheet or forms?** Does he prefer to work in a spreadsheet? Most designers do, but it decides between A and B.
2. **Play-testing:** does he need to play his own changes, and on what machine (Windows build)? That decides whether the balance override (6.4) comes early.
3. **Which system first?** The pilot proposal is the combat curve. Does he have a different first priority?
4. **Formula shapes:** does he expect to change them, or only their parameters? Section 5.7 recommends curves and tables over editable formulas.
5. **Accounts:** is a shared Google account or Workspace acceptable for the workbook and the service account?
6. **Reports:** what outputs does he want to see next to the inputs? Section 5.8 lists a first set.
