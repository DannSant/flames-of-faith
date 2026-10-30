# Backlog

_Last updated: 2026-10-29. Ordered around the next tester build, then the Steam demo (December 2026). Each line: item, then why._

## Now
1. **Run encounter events rework, systems** (implementation session; spec `docs/features/run-encounter-events.md`, narrative done). Why: the last open tester-feedback item; events are the demo's main lore delivery.
2. **Next tester build** (merge `develop` → `main`, tag, patch notes). Why: a lot changed since v0.5.1 and testers haven't played it.

## Before the Steam demo (musts, trimmed 2026-10-29)
**Code**
3. **High Attack Speed fix** (decided 2026-10-29): the attack animation speeds up with Attack Speed (base cooldown / current cooldown; the art team agreed), plus a soft cap and a hard cap on Attack Speed, like Shop Discount's (`StatsCalculations.CalculateShopDiscountMultiplier`) and Retribution's tiers. The caps go on `PlayerStatSettings` and the PlayerStats sheet tab. Needs a spec. Why: builds break above ~37 Attack Speed (Archer).
4. **Cutscene design** (design session): the lore and art direction of the **opening** scene for the demo; the other 25 scenes later. Feeds 5 and the art team. Why: art needs the brief early.
5. **Opening cutscene: the general story** (one scene). Needs a small cutscene system (static images + text, skippable, gamepad) and its content from 4. Why: players start the demo with no context.
6. **Balance pass: enemy and player damage**, with the designer in the sheet. Why: tester feedback; the Whirlwind now deals several times its old crowd damage.

**Waiting on art**
7. **Re-designed characters** (art): new sprites, then adjust the player prefabs (dimensions, colliders, hitboxes, the Corrupted outline margin, animators). Why: the demo's look.
8. **New scenarios** (art): replace the level scenes, adjust levels to the new sizes (spawn points, obstacles, camera bounds). Non-combat levels (shop, campfire, treasure, events, shrine) show the other two pilgrims with idle animations (sitting, sleeping, chatting); kept in the demo by the owner (2026-10-29). Why: the demo's look; the companions fit the lore (the three travel together).
9. **Boss invulnerability shield** (art): the art team adds a shield to show the first phase's invulnerability; wire it to `EnemyHealth.IsImmuneFlag`. Why: tester feedback; the see-through effect was removed.

**Launch**
10. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Demo update (after the demo launches)
_Moved out of the demo on 2026-10-29 to protect the December date; candidates for a demo update or 1.0._
11. **Item unlocks in the Skill Tree** (probably a new tab): spend Favors to unlock items into the drop pool. Needs a spec. Why: more reasons to spend Favors; the 3 locked lootable items can never drop today.
12. **More items**, especially unlockable ones and higher qualities (Epic and Legendary exist but have no items). Designer work in the sheet plus New Item; new behavior types only where needed. Why: build variety for the demo; pairs with 11.
13. **Overworld map redesign** (LevelSelector scene): decorations, a better look, and if possible a visual fog of war over the hidden nodes (the reveal logic already exists in `MapRunController`). Needs a spec and art. Why: the map is the demo's hub screen.
14. **Cutscene design for the other 25 scenes, and voice narration research:** cost, timeline and options for narrating the cutscenes (Spanish and English), plus what the cutscene system would need (audio per page, subtitles). Why: decides whether the cutscenes are narrated (the demo's opening scene ships with text only).
15. **Delete the obsolete SQLite pipeline.** Why: dead code; deferred until a play-test round confirms the sheet path.
16. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD and `Lore.md`. Why: outdated, Spanish, contradicts the lore; useful for the Steam page.

## Delivery (not started)
17. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo builds; worth having before the demo goes up.
18. **EditMode tests in CI** on every push (~730 tests exist).

## For 1.0
19. **Boss architecture refactor, before the second boss.** Why: the Act I boss's systems weren't built for several bosses; Acts II–IV need them.
20. **Boss reward:** when a boss dies, the player picks 1 of 3 powerful items that only bosses grant. Why: a payoff for each act; needs boss-only items (a new drop source).
21. **Cutscenes, full scope** (changed 2026-10-29): one opening and one ending, plus one per character before and after each act: 2 + 3 × 4 × 2 = **26 scenes**. Static images and text; narrated if the voice research says it's feasible. Why: each pilgrim's arc is told through them (`Lore.md` §6).
22. **Decide the run structure across acts** (GDD Q14). Why: blocks Act II–IV planning, saves, difficulty scaling and cutscene placement.
23. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
24. **Steam achievements.** Why: achievement hunters are a target audience.
25. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
26. **Input remapping.** Why: expected on Steam.
27. **Acts II–IV content:** bosses, enemies, act mechanics (Frozen, Pools of Corruption, Will Break, Darkness). Why: 1.0 scope; boss designs are TBD.
28. **Acts II–IV events.** Why: `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla, Faelin judges Orleone, Werlem finds Kyro's work).
29. **Key items that unlock parts of the map.** Why: planned, needs a design.

## Non-programming (art/design team)
- Re-designed character sprites (demo); scenarios and companion idle animations (demo); boss shield (demo); opening cutscene art (demo); map decorations (demo update); the other cutscenes' art (1.0); event art; Favor icon; 2 s Whirlwind animation; melee telegraph sprites; a check that the attack animations read well sped up (Attack Speed fix).

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
