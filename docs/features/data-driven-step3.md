# Feature: Data-driven step 3 (debuffs, Grace / Corruption, player stats) and pilot leftovers

- **Status:** in progress
- **Branch:** `feature/data-driven-step3` (off `develop`)
- **Owner session:** data-driven pilot session (2026-10-24)
- **Related docs:** CLAUDE.md "Balance data (data-driven pilot, Oct 2026)", `Assets/Docs/data-driven-analysis.md` sections 3.5, 3.7, 3.8 and 7 (step 0 leftovers, step 3)

## Goal
The designer can tune debuffs, Grace / Corruption, the player's stats and stat formulas, and each class's dash from the balance sheet, like the enemies and waves of the pilot. The pilot's dead fields go, so every number in the sheet does something.

## Scope
- **Leftovers (step 0):**
  - Remove dead fields: `AIBehaviorShooter.minRange`, `AIBehaviorChaserLogic.useChasePause` (and its commented-out code), `KnockbackBehavior.forceMode`.
  - Fix Frost's extra damage, which subtracted the hit's damage again instead of the extra amount (double damage).
  - Move Frost's 80% slow cap and Ley Charge's chain radius and depth onto `ElementalDebuffData`.
- **Step 3, moving numbers onto settings assets:**
  - `PlayerGrace` (range, starting Grace, damage per Grace point, damage floor) → `CorruptionSettings`.
  - `PlayerController` move speed, `PlayerHealth` invulnerability after a hit, the Retribution curve (`RetributionHandler`) → `PlayerStatSettings`.
  - Each class's dash cooldown, speed, duration and charges (dash components on the class prefabs) → `CharacterClassData`.
- **Step 3, new balance tables:** Debuffs, Corruption, PlayerStats (formula parameters), Stats (StatDatabase: start value, level-up tiers, category, Corruption), Classes (dash), Pierce. Each with CSV, explanation panel notes, and a workbook tab.

## Out of scope
- Item drops (`ItemProvider`, step 4), items and ability tier tables (step 5), the other modules (step 7).
- Formula shapes stay code.
- Stat descriptions (text) and the Retired flag are not in the sheet: retiring a stat needs code changes.

## Design details
- Values moved keep their current numbers (from the prefabs), so nothing changes in play.
- Ability Glyphs still override dash duration and charges (Archer, Augur); the class values are the fallback, as before.
- Debuff columns that apply to one element only (Frost max slow, Ley chain radius / depth) say so in their notes.
- The Stats table is keyed by stat name; rows can't be added or removed, and the key can't change.

## Acceptance criteria
- [ ] The dead fields are gone and the behavior tables no longer show them.
- [ ] Frost deals its extra percentage once (`FrostDamageTests`).
- [ ] No design number left on `PlayerGrace`, `PlayerController` (speed), `PlayerHealth` (invulnerability), `RetributionHandler` (curve) or the dash components.
- [ ] The new tables export and import back as no change, their CSVs match the assets, and every column has a note (`BalanceTablesTests`, `BalanceDocsTests`).
- [ ] All EditMode tests pass.

## Files likely touched
- Code: `Scripts/AI/Behaviors/*`, `Scripts/Combat/Elemental/*`, `Scripts/Combat/EnemyHealth.cs`, `PlayerGrace`, `PlayerHealth`, `RetributionHandler`, `CorruptionSettings`, `Scripts/Control/PlayerController.cs`, `DashBase`, `CharacterClassData`, `PlayerStatSettings`, `Scripts/BalanceData/*`.
- Prefabs / scenes / assets: `PlayerBase`, `PlayerWarrior`, `PlayerArcher`, `PlayerAugur` prefabs (moved fields removed), `CorruptionSettings`, `PlayerStatSettings`, `StatDatabase`, the 3 `CharacterData` assets, the 5 debuff assets, behavior assets (reserialized).
- Balance tables / CSVs / sheet tabs: new `Debuffs`, `Corruption`, `PlayerStats`, `Stats`, `Classes`, `Pierce`; changed `EnemyBehaviors_ChaserLogic`, `EnemyBehaviors_Shooter`, `EnemyBehaviors_Knockback`.

## Tests
- EditMode tests to add: Frost extra damage; moved values match the old prefab values; the new tables round trip and are documented.
- Manual checks (go in `todo.md` "Pending manual tests"): play each class (dash, speed, Grace, Retribution feel unchanged), Frost and Ley Charge, the new sheet tabs (publish → pull).

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:** dead behavior fields removed; Frost double damage fixed; Frost cap and Ley chain numbers on the debuff assets; Grace numbers on `CorruptionSettings`; movement, invulnerability and the Retribution curve on `PlayerStatSettings`; dashes on `CharacterClassData`; six new tables (Debuffs, Corruption, PlayerStats, Pierce, Stats, Classes) with CSVs and notes; CLAUDE.md updated. EditMode: 717/719 pass.
- **Not done / follow-ups:**
  - The workbook: the six new tabs, their checkboxes / dropdowns / protection, the Read Me, then Push All (waiting on the owner's OK to edit the sheet).
  - The 2 failing tests are `MainMenuStyleTests.SceneInstances_UseThePrefabsLayout` (MainMenuPanel, CharacterSelect): `MainMenu.unity` overrides child anchors. This branch doesn't touch those files; not re-run on `develop` to confirm.
  - For the designer: Move Speed adds 0.01 units/s per point (0.3% of the 3.5 base), and Frost's strength per Mastow point is 0.001; both look too small to matter. Frost no longer doubles hits, so Frost items are weaker than testers are used to.
- **Commits / branch:** `feature/data-driven-step3`: 9c73de6 (spec), 762faf9, 5992853, 8af10ff, 2f31285, 00bd931.
- **Gotchas for the next session:**
  - `develop` had `CorruptorAct1` health 250 in the asset but 200 in `Enemies.csv`; 00bd931 matched the CSV to the asset. If the sheet still says 200, a Pull would set it back.
  - Re-saving prefabs in 6.3 also rewrites unrelated ones (`serializedVersion: 2`, `m_MaskInteraction` order); restore those instead of committing them.
- **Manual tests added to todo.md:** "Data-driven step 3 and pilot leftovers (2026-10-24)" (todo.md itself left uncommitted: it holds the owner's own edits).
