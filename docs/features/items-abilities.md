# Feature: Items and abilities in the sheet (data-driven step 5)

- **Status:** done (merged into `develop` 2026-10-25)
- **Branch:** `feature/items-abilities` (off `develop`, after item drops was merged)
- **Owner session:** data-driven pilot session (2026-10-24)
- **Related docs:** CLAUDE.md "Effect data pipeline", "Combat/effect pipeline", "Class kits, skill trees & Favors", "Balance data", "Item drops"; `Assets/Docs/data-driven-analysis.md` 3.6

## Goal
The designer edits every item and class ability number in the balance sheet: what an item is (text, quality, prices, icon, stats, stat bonuses, behaviors), what its behaviors do (counts, chances, cooldowns, damage, stacking), the class abilities' tier tables and the weapons. Damage comes from one place per attack: whoever spawns it.

## Scope
- **Importer extensions:** nested fields in `DesignFields`, info columns on `ChildListTable`, Sprite and EffectBehavior reference cells, multiline text cells.
- **Effects DB → sheet tables:** Items, ItemStats, ItemStatBonuses and ItemBehaviors, editing the Effect assets directly. The drop tab is renamed ItemDrops.
- **New item tool:** `Tools/Flames of Faith/Items/New Item`.
- **SQLite pipeline obsolete:** `EffectDatabase`, `EffectRow`, `EffectLoader`, `ScriptableObjectGenerator`, `DBEditorHelper` and `EffectDatabaseWindow` are marked `[Obsolete]`, and their menu items removed. They are deleted after a play-test round.
- **Spawner owns damage:**
  - `DamageProfile` on item spawn behaviors (`SpawningEffectBehavior`), the dashes (class `dashDamageStat`, ability stat scales) and the Fire Tornado explosion.
  - `DamageSourceData` loses base damage, weapon class and attack scale.
  - `Effect.scalingValue` is retired into the profiles.
- **Report_ItemDamage**, used to prove the damage migration changed nothing.
- **Tables:**
  - ItemBehaviors_<Type> and ItemBehaviorStacking;
  - Ability_<Name> × 9 (tier rows);
  - Weapons.
- **Workbook:** the tabs, Read Me and docs.

## Out of scope
- Creating items from the sheet: done in Unity.
- Deleting the SQLite code: a later cleanup commit.
- Boss abilities.

## Design details
- **Assets stay the source of truth;** the CSVs and the sheet mirror them, as in every other table.
- **Item identity:** keyed by asset name. Names can't change from the sheet, so asset GUIDs and `effectID`s (saves, unlocks, skill trees, drop table) stay valid.
- **Stat scale:** `statScale` = old `Effect.scalingValue` × old prefab `attackScale`, so every item deals the same damage.
- **Dashes without their glyph** deal no damage. The kit always has the glyph (`ClassKitTests`).

## Acceptance criteria
- [ ] Report_ItemDamage is identical before and after the damage move.
- [ ] Every damaging spawner sets damage (guard test).
- [ ] The new tables round trip, match their CSVs, and are documented.
- [ ] Ability tiers still never get weaker (`SkillTreeTests`).
- [ ] New Item creates a unique `effectID` and a drop row.
- [ ] All EditMode tests pass.

## Files likely touched
- **Code:**
  - `BalanceData/*`;
  - `Combat/DamageSourceData.cs`, `DamageSourceBase.cs`, `DamageCalculator.cs`, `ExplosionDamage.cs`, `ScepterWeapon.cs`;
  - `Effects/*`, `Effects/EffectBehaviors/*`, `Effects/Abilities/*`;
  - `Control/*Dash.cs`, `Control/CharacterClassData.cs`;
  - `Database/*` and `Editor/Database/*` (obsolete);
  - new `Items/Editor/NewItemTool.cs`.
- **Prefabs / assets:** the damage-source prefabs and their data assets, the behavior and ability assets, the Effect assets, `CharacterData`, `Resources/Weapons`.
- **Balance tables / sheet:** Items, ItemStats, ItemStatBonuses, ItemBehaviors, ItemBehaviors_*, ItemBehaviorStacking, Ability_*, Weapons and Report_ItemDamage are new. Items becomes ItemDrops, and DamageSources loses 3 columns.

## Tests
- **EditMode:** importer extensions, damage profile application, the spawner guard, New Item, and the round trips.
- **Manual (todo.md):**
  - item and ability damage looks unchanged in play;
  - sheet edits to items and abilities pull correctly;
  - New Item and Push All;
  - the test build override.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
  - Importer: nested and reference `DesignFields`, `ChildListTable.WithInfoColumns` / `AllowingEmpty`, text normalization, Sprite and EffectBehavior reference columns.
  - Items in the sheet: Items, ItemStats, ItemStatBonuses, ItemBehaviors; the drop tab renamed ItemDrops; `Tools/Flames of Faith/Items/New Item`; the SQLite pipeline `[Obsolete]` with its menus removed.
  - Spawner owns damage: `DamageProfile`, `SpawningEffectBehavior`, `CharacterClassData.dashDamageStat`, stat shares on the Trap Step / Flame Dash / Fire Tornadoes tiers; `DamageSourceData` lost its damage fields, `Effect.scalingValue` is `[Obsolete]` and unread. `Report_ItemDamage` was byte-identical before and after.
  - Tables: `ItemBehaviors_<Type>` x 8, ItemBehaviorStacking, `Ability_<Name>` x 9, Weapons; report ItemDamage. `WeaponData.graceGenerated` (unread) removed.
  - Docs: CLAUDE.md (Effect data pipeline rewritten; Combat/effect pipeline, Class kits, Item drops, Balance data), the analysis's progress note.
- **Not done / follow-ups:**
  - Delete the SQLite pipeline (`Database/*`, `Editor/Database/EffectDatabaseWindow.cs`, `SQLite.cs`, the sqlite3 plugins, `Database/effects.db`, `Effect.InitializeFromData`, `Effect.scalingValue`) after a play-test round.
  - Weapons has no Base Damage / Projectile Amount columns: the ability Glyphs set them, so the `WeaponData` values are only a fallback no class uses.
- **Commits / branch:** `feature/items-abilities` off `develop`: 5415e7e spec, 0d3686f importer, 5d61e2d item tables, ecec875 ItemDamage report, b65ca97 damage ownership, 9352bed item behavior tables, c3feedb vector note fix, 54c27e3 ability and Weapons tables, 26bea8a docs, 7509ee9 stacking rule fixes. Fast-forwarded into `develop` on 2026-10-25 after the user's play-test; then 334b905 (the user's sheet test edits, version 8) on `develop`.
- **Play-test (2026-10-25):** every class and item behavior dealt the same damage as before. Swords of Fire scaled spawn count instead of damage, and Consecrate / Holy Missiles had no stacking rule. Both were wrong on `develop` before this feature; fixed, and `DamageOwnershipTests.EverySpawningBehavior_HasAStackingRule` now guards it. The sheet round trip (edit, Publish, Pull), New Item and Push All all worked.
- **Workbook (2026-10-25):** tab Items renamed ItemDrops; 24 tabs added (sheetIds 134-157); DamageSources lost its 3 damage columns; Classes got Dash Damage Stat; Read Me updated. The tabs were created with headers, checks and protection only; Push All filled them.
- **Gotchas for the next session:**
  - A new damaging spawner must set damage (`DamageProfile.ApplyTo`, `SetDamage` or `Initialize`); otherwise it deals 0 and warns once. `DamageOwnershipTests` covers item behaviors, dashes and explosions.
  - Item rows can't be added from the sheet: New Item, Export CSV, Push All.
  - Ability tier rows can't be added from the sheet either; they follow the skill tree.
- **Manual tests added to todo.md:** "Items and abilities (2026-10-24)".
