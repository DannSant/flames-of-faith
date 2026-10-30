# Backlog

_Last updated: 2026-10-29. Ordered around the next tester build, then the Steam demo (December 2026). Each line: item, then why. "Art" marks items that wait on the art team._

## Now
1. **Run encounter events rework, systems** (implementation session; spec `docs/features/run-encounter-events.md`, narrative done). Why: the last open tester-feedback item; events are the demo's main lore delivery.
2. **Next tester build** (merge `develop` → `main`, tag, patch notes). Why: a lot changed since v0.5.1 and testers haven't played it.

## Before the Steam demo
_Scope grew on 2026-10-29; see the scope risk in PROJECT_STATE._

**Code we can start now**
3. **High Attack Speed fix** (decided 2026-10-29): the attack animation speeds up with Attack Speed (base cooldown / current cooldown; the art team agreed), plus a soft cap and a hard cap on Attack Speed, like Shop Discount's (`StatsCalculations.CalculateShopDiscountMultiplier`) and Retribution's tiers. The caps go on `PlayerStatSettings` and the PlayerStats sheet tab. Needs a spec. Why: builds break above ~37 Attack Speed (Archer).
4. **Item unlocks in the Skill Tree** (probably a new tab): spend Favors to unlock items into the drop pool. Needs a spec. Why: more reasons to spend Favors; the 3 locked lootable items can never drop today.
5. **More items**, especially unlockable ones and higher qualities (Epic and Legendary exist but have no items). Designer work in the sheet plus New Item; new behavior types only where needed. Why: build variety for the demo; pairs with 4.
6. **Overworld map redesign** (LevelSelector scene): decorations, a better look, and if possible a visual fog of war over the hidden nodes (the reveal logic already exists in `MapRunController`). Needs a spec and art. Why: the map is the demo's hub screen.
7. **Opening cutscene: the general story** (one scene). Needs a small cutscene system (static images + text, skippable, gamepad) and its content from 8. Why: players start the demo with no context.
8. **Cutscene design** (design session): the lore and art direction for every cutscene in the new scope (see 1.0 below), the opening one first. Feeds 7 and the art team. Why: art needs briefs early.
9. **Voice narration research:** cost, timeline and options for narrating the cutscenes (Spanish and English), plus what the cutscene system would need (audio per page, subtitles). Why: decides whether 7 ships narrated.
10. **Balance pass: enemy and player damage**, with the designer in the sheet. Why: tester feedback; the Whirlwind now deals several times its old crowd damage.

**Waiting on art**
11. **Re-designed characters** (art): new sprites, then adjust the player prefabs (dimensions, colliders, hitboxes, the Corrupted outline margin, animators). Why: the demo's look.
12. **New scenarios** (art): replace the level scenes, adjust levels to the new sizes (spawn points, obstacles, camera bounds). Non-combat levels (shop, campfire, treasure, events, shrine) show the other two pilgrims with idle animations (sitting, sleeping, chatting). Why: the demo's look; the companions fit the lore (the three travel together).
13. **Boss invulnerability shield** (art): the art team adds a shield to show the first phase's invulnerability; wire it to `EnemyHealth.IsImmuneFlag`. Why: tester feedback; the see-through effect was removed.

**Housekeeping**
14. **Delete the obsolete SQLite pipeline.** Why: dead code; deferred until a play-test round confirms the sheet path.
15. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD and `Lore.md`. Why: outdated, Spanish, contradicts the lore; useful for the Steam page.
16. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Delivery (not started)
17. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo builds; worth having before the demo goes up.
18. **EditMode tests in CI** on every push (~730 tests exist).

## For 1.0
19. **Boss architecture refactor, before the second boss.** Why: the Act I boss's systems weren't built for several bosses; Acts II–IV need them.
20. **Boss reward:** when a boss dies, the player picks 1 of 3 powerful items that only bosses grant. Why: a payoff for each act; needs boss-only items (a new drop source).
21. **Cutscenes, full scope** (changed 2026-10-29): one opening and one ending, plus one per character before and after each act: 2 + 3 × 4 × 2 = **26 scenes**. Static images and text; narrated if 9 says it's feasible. Why: each pilgrim's arc is told through them (`Lore.md` §6).
22. **Decide the run structure across acts** (GDD Q14). Why: blocks Act II–IV planning, saves, difficulty scaling and cutscene placement.
23. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
24. **Steam achievements.** Why: achievement hunters are a target audience.
25. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
26. **Input remapping.** Why: expected on Steam.
27. **Acts II–IV content:** bosses, enemies, act mechanics (Frozen, Pools of Corruption, Will Break, Darkness). Why: 1.0 scope; boss designs are TBD.
28. **Acts II–IV events.** Why: `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla, Faelin judges Orleone, Werlem finds Kyro's work).
29. **Key items that unlock parts of the map.** Why: planned, needs a design.

## Non-programming (art/design team)
- Re-designed character sprites (gates 11); scenarios and companion idle animations (gates 12); boss shield (gates 13); map decorations (6); cutscene art (7, 21); event art; Favor icon; 2 s Whirlwind animation; melee telegraph sprites; faster attack animations or a check that they read well sped up (3).

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
