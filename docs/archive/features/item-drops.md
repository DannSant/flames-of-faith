# Feature: Item drops (data-driven step 4)

- **Status:** done (merged into `develop` 2026-10-25)
- **Branch:** `feature/item-drops` (off `develop`, after step 3 was merged)
- **Owner session:** data-driven pilot session (2026-10-24)
- **Related docs:** CLAUDE.md "Balance data", "Run encounter events" (Shop, Treasure), "Effect data pipeline"; `docs/archive/design/data-driven-analysis.md` 3.4

## Goal
One table in the balance sheet lists every lootable item and says where it can drop (enemy pickup, shop, treasure, event) and from how far into the run (levels beaten). Luck still tilts the quality odds and the shop's size. Today four code paths keep four hand-written lists, so a new item needs up to three manual edits, a strong item can drop on level 1, and at Luck 0 a Heroic is as likely as a Common.

## Scope
- **`ItemDropTable`:** one row per lootable item: four source checkboxes, min levels beaten, and a weight within its quality.
- **`LootSettings`:**
  - per source: a base weight per quality and a Luck strength;
  - the shop's count settings (moved off `ShopKeeper`);
  - the enemy drop chance and item share (moved off `PickupSpawner`);
  - a treasure fallback in coins.
- **`LootRoller`** (pure) and **`ItemProvider`** (runtime). All four sources roll through them.
- **Eligible items:** flagged for the source, min levels beaten ≤ `GameSession.LevelsBeaten`, unlocked, not starting-only.
- **The hand-written lists are removed:** `RandomItemPickup.possibleItems`, `TreasureRewardEntry.item`, `GiveRandomItemOptionAction.possibleItems` / `qualityScalePerStat`, and `Effect.availableForShop` (now the table's Shop flag).
- **Balance tables:** Items, LootSources, Loot, and a Report_LootOdds report. The workbook gets the tabs.
- **Sync tool:** adds rows for new lootable items and removes stale ones.

## Out of scope
- Per-event item themes: decided to use one Event pool.
- Item stats, prices and quality stay in the Effects DB (step 5).
- Unlocking items during play: nothing unlocks the 3 locked lootable items yet, so they don't drop.

## Design details
- **Quality odds:** weight = base weight of the tier × luckFactor^tierIndex, with `EffectQualityScaler`'s mirrored Luck curve. Tiers with no eligible item get 0.
- **Item pick:** by the item's weight within the rolled tier.
- **Nothing eligible:**
  - enemy pickup: drops nothing;
  - shop: shows fewer items;
  - event: gives nothing (logged);
  - treasure: gives coins (fallback range).

  It never throws, because it can run inside a level load.
- **Defaults reproduce today's odds at Luck 0** for today's pools:
  - Enemy Pickup: 35/25/20/20;
  - Treasure: 0/0/40/60;
  - Shop and Event: 25/25/25/25.
- **Luck strength:** 2 everywhere, so enemy pickups and treasures now respond to Luck as the designer asked.
- **Item weights:** start at 1. The old pickup weights mostly encoded rarity, which the quality weights now do.

## Acceptance criteria
- [ ] An item with min levels beaten N never drops before N levels are beaten (`LootRollerTests`).
- [ ] Locked, starting-only, unflagged and zero-weight items never roll.
- [ ] Luck raises higher tiers, negative Luck lowers them, and weights stay positive.
- [ ] The shop's count follows Luck and its items are unique.
- [ ] Every lootable Effect has exactly one row; no starting-only item does (`ItemDropTableTests`).
- [ ] The new tables round trip, match their CSVs, and every column has a note.
- [ ] All EditMode tests pass.

## Files likely touched
- **Code:**
  - `Scripts/Loot/*` (new);
  - `Pickups/PickupSpawner.cs`, `Pickups/RandomItemPickup.cs`;
  - `RunEncounters/Treasure/*`, `RunEncounters/EventEncounter/GiveRandomItemOptionAction.cs`, `RunEncounters/ShopKeeper.cs`;
  - `UI/RunEncounters/EventEncounterOptionUI.cs`;
  - `Utils/EffectQualityScaler.cs`;
  - `Effects/Effect.cs`, `Database/EffectLoader.cs`, `Editor/Database/EffectDatabaseWindow.cs`;
  - `BalanceData/*`.
- **Prefabs / assets:** `EnemyBase.prefab`, `RandomItemPickup.prefab`, `Shopkeeper.prefab`, `ItemTreasure.asset`, the two event action assets, new `Resources/Loot/*`.
- **Balance tables / sheet:** new Items, LootSources, Loot, Report_LootOdds.

## Tests
- **EditMode:** `LootRollerTests`, `ItemDropTableTests`; `ShopStockTests` and `EffectQualityScalerTests` updated; the `StatDependencyTests` list guards replaced.
- **Manual (todo.md):** min progression in the shop (beat nodes with the dev cheat), each source still gives items, Luck +10 in the shop, and the sheet round trip.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:** loot core (`Scripts/Loot/`, `Resources/Loot/`), the four sources rewired through `ItemProvider`, the hand-written lists and `Effect.availableForShop` removed, `EffectQualityScaler` folded into `LootRoller`, balance tables Items / LootSources / Loot and Report_LootOdds, CLAUDE.md "Item drops". Workbook: tabs Items (130), LootSources (131), Loot (132) with values, checkboxes and protection (Quality / Buy Price / Unlocked grey and locked), Report_LootOdds (133, protected, filled by Unity), Read Me rows. EditMode: 728/728.
- **Not done / follow-ups:**
  - Owner: Push Table Notes (or Push All) to fill the new tabs' explanation panels and Report_LootOdds.
  - Designer: set Min Levels Beaten per item (all 0 now), and look at Report_LootOdds: Luck Scale 2 is steep (Luck 5 makes ~70% of enemy pickups Heroic).
  - Item weights start at 1: the old pickup weights (2-20) mostly encoded rarity, which the quality weights now do.
  - The 3 locked lootable items never drop until something unlocks them during play (nothing does yet).
- **Commits / branch:** `feature/item-drops`: 29a0d98 (spec), 139f297, c768c81, db20b90, adad920, 0ab39bb.
- **Gotchas for the next session:**
  - The migration ran once (`ItemDropSeed`, deleted in c768c81). New items only need Sync Item Drop Table.
  - Re-saving prefabs in 6.3 rewrites unrelated ones (`serializedVersion: 2`, `m_MaskInteraction`); restore those.
- **Manual tests added to todo.md:** "Item drops (2026-10-24)" (todo.md left uncommitted: it holds the owner's own edits).
