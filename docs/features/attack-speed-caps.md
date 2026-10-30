# Feature: High Attack Speed fix (animation speed + caps)

- **Status:** ready for implementation
- **Branch:** `feature/attack-speed-caps` (off `develop`)
- **Owner session:** implementation session, not started
- **Related docs:** `CLAUDE.md` "Adding a player stat" (stat descriptions, `PlayerStatSettings`, special cooldowns), "Balance data" (keep the Sheets pipeline in step), "Melee attack warning" (the enemy `AttackSpeed` precedent), "Animation events in blend trees"; `docs/pm/DECISIONS.md` 2026-10-29

## Goal
Attack Speed should always make the player attack faster. Today, past a point, primary attacks stop landing at all. Fix that, and cap the stat so it can't run away.

## The bug
The attack animation always plays at speed 1 while the cooldown shrinks (`cooldown base / (1 + Attack Speed × 0.075)`), and each attack trigger restarts the attack state. Once the cooldown is shorter than the time to the shot or hit, the shot or hit never happens.
- **Archer and Augur** fire at 60% of a 0.67 s clip, 0.4 s in (`BowAttackAnimBehavior`).
- **Warrior:** the hit window opens at 0.25 s.
- **Breaks above about** 37 Attack Speed (Archer, base 1.5 s), 53 (Augur, base 2.0 s) and 67 (Warrior, base 1.5 s).

## Scope
1. **Animation speed.** A player's attack animation plays at `base cooldown / current cooldown`, so the shot or hit always lands inside the cooldown, at any Attack Speed. The art team agreed.
2. **Soft cap and hard cap on Attack Speed,** like Shop Discount's: full value up to the soft cap, a reduced rate up to the hard cap, nothing past it.
3. **Everything that shows Attack Speed tells the truth:** the Stats pane tooltip, the level-up Now / After preview.
4. **The balance sheet** gets the new tunables.

## Out of scope
- Enemy attack speeds, and `EnemyAnimationController`'s melee windup.
- The Whirlwind spin's duration or tick interval.
- New art. The clips are only played faster.
- Other stats' caps.
- Rebalancing weapons' base cooldowns (the balance pass covers that).

## Design details

### Effective Attack Speed
- Add a pure function next to `CalculateShopDiscountMultiplier` in `StatsCalculations`: stat points in, effective points out. Below the soft cap it returns the points. Between the caps each point counts for the soft-cap rate. Past the hard cap it returns the hard cap's value. Negative points pass through unchanged.
- `CalculateAttackDelay` stays as it is, and is fed the effective points. Every caller must go through it: `WeaponBase.SetupAttackSpeedVariables`, and `StatBenefitDescriber.DescribeAttackSpeed` (the Attack Speed tooltip), so the display and the real cooldown can't drift.
- Tunables go on `PlayerStatSettings`, per the "formerly hardcoded numbers" rule, with a getter like `GetShopDiscountMultiplier`:
  - `attackSpeedSoftCap`, `attackSpeedHardCap`, `attackSpeedPastSoftCapRate`.
  - **Starting values (a proposal, the designer tunes them):** soft cap 30, hard cap 60, rate 0.5. That gives 45 effective points at the hard cap, a 4.4× speed-up: the Archer's 1.5 s becomes about 0.34 s.
- `MIN_ATTACK_DELAY` (0.01 s) stays as a floor.

### Animation speed
- Add a float parameter `AttackSpeed` to the player controllers, and use it as the **Speed Multiplier** of their primary attack state: `PlayerWarriorIsoAnimController`, `ArcheAnimController`, `AugurAnimController`. This is the same pattern as the enemy parameter (`EnemyAnimationController.PlayAttack`). Check whether the Warrior's second controller (`PlayerWarrior.prefab` has two; `Sword.controller` / `Sword2.controller`) must follow too.
- `CharacterVisual` sets it, fed by the weapon: `speed = max(1, base cooldown / current cooldown)`. Negative Attack Speed never slows the animation below 1: the cooldown is longer then, so nothing can be cut off.
- Set it whenever `SetupAttackSpeedVariables` runs, so a level-up or a new item applies at once.
- `BowAttackAnimBehavior` reads normalized time, so it follows the new speed without changes. Check the Warrior's hit window: at high speed it gets very short, and the swing's start and end events may land in the same frame or two (see "Animation events in blend trees").
- **Specials keep speed 1.** Their cooldowns are long, and their timing carries gameplay (the Whirlwind's spin, the Barrage's invulnerability). Instead, a guard test makes sure that at the hard cap each special's cooldown is still longer than its action. The Whirlwind is the tight one: 10 s base / 4.4 = about 2.3 s against a 2 s spin. If a value fails, raise it with me instead of changing it on your own.

### UI
- The Attack Speed tooltip's benefit line shows the capped cooldown. Past the soft cap it says so, and past the hard cap it says more points do nothing, like Shop Discount's line in `StatBenefitDescriber`.
- The level-up preview comes from the same math, so it should need nothing; confirm it.
- Numbers stay out of the static text in `StatDatabase`.

### Balance sheet (required, `CLAUDE.md` "Keep the Google Sheets pipeline in step")
- Add the three fields to the `PlayerStats` table in `BalanceTables.All()`, each with a `[Tooltip]`.
- Export CSV, then add the steps to `todo.md`: Push All to Sheet (or Push Table Notes), then check the panel.
- `BalanceSync.gs` shouldn't need changes. Confirm it.
- **Heads-up:** a Push All is already pending for the Quiver of the Sentinel rename. Ask me whether it has been done before relying on a Pull.

## Acceptance criteria
- [ ] At any Attack Speed, including 100+ with the DevTools cheat, every class's primary attack lands, for all three classes, with auto-attack and manually.
- [ ] Effective points follow the soft and hard cap; negative points are unchanged (pure tests).
- [ ] The cooldown, the tooltip and the level-up preview agree at 0, below the soft cap, between the caps and past the hard cap.
- [ ] A level-up or a new item changes the animation speed at once, and Continue restores it.
- [ ] Negative Attack Speed: attacks are slower, the animation plays at normal speed, nothing breaks.
- [ ] The caps are on `PlayerStatSettings` and in the sheet's PlayerStats tab.
- [ ] Guard tests:
  - every class's controller has the `AttackSpeed` parameter, and its primary attack state uses it as the speed multiplier (the re-designed characters will bring new controllers, and this test must catch a forgotten one);
  - at the hard cap every special's cooldown is longer than its action;
  - at the hard cap the primary's cooldown is longer than its time to hit, divided by the animation speed.
- [ ] Existing tests stay green: `AnimationEventTests`, `StatBenefitTests`, `WhirlwindTests`, `BalanceTablesTests`, `BalanceDocsTests`.

## Files likely touched
- Code: `Scripts/Progression/StatsCalculations.cs`, `PlayerStatSettings.cs`, `StatBenefitDescriber.cs`; `Scripts/Combat/WeaponBase.cs`; `Scripts/Control/CharacterVisual.cs`; `Scripts/BalanceData/BalanceTables.cs`.
- Assets: `Resources/Progression/PlayerStatSettings.asset`; `Animation/PlayerWarriorIso/PlayerWarriorIsoAnimController.controller`, `Animation/ArcherHeroIso/ArcheAnimController.controller`, `Animation/AugurHeroIso/AugurAnimController.controller`.
- Balance: `BalanceData/PlayerStats.csv` and its reports if they change.
- Docs: `CLAUDE.md` "Adding a player stat"; the GDD's stat table (§9).
- No scenes. The player prefabs only if a controller changes, and re-saved prefabs can rewrite unrelated ones: restore those.

## Tests
- EditMode tests to add: see the acceptance criteria (`AttackSpeedTests`, or extend the stat tests).
- Manual checks (go in `todo.md` "Pending manual tests"):
  - each class at Attack Speed 0 / 30 / 60 / 100 with `DevTools/Stats`: attacks land, and the animation reads well sped up (the art team should look at this);
  - the Warrior's swing still hits every enemy in the arc at the hard cap;
  - the attack's facing, its sound, and items triggered by attacks still work at high speed;
  - the special cooldown log (Log Special Cooldown) still matches the tooltip.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
- **Not done / follow-ups:**
- **Commits / branch:**
- **Gotchas for the next session:**
- **Manual tests added to todo.md:**
