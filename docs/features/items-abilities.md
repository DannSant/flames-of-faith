# Feature: Items and abilities in the sheet (data-driven step 5)

- **Status:** in progress
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
- **Not done / follow-ups:**
- **Commits / branch:**
- **Gotchas for the next session:**
- **Manual tests added to todo.md:**
