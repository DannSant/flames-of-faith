# Backlog

_Last updated: 2026-11-01 (both design sessions done). Ordered around the v0.5.2 tester round, then the Steam demo (December 2026). Each line: item, then why. Numbers stay stable, because specs refer to them._

## Now (while testers play)
1. **Tester build v0.5.2:** released on 2026-10-30 (`main`, tag `v0.5.2`, patch notes in `PatchNotes/notas-desde-v0.5.1.md`). Left: make the Windows build, send it to testers, collect feedback. Why: a lot changed since v0.5.1, and every manual test has passed.
2. **Cutscene design:** done 2026-11-01 (7 pages with art briefs, in the spec below). Left for the owner: send the briefs to the art team and agree the size (1920 × 1080). The owner says the team will deliver all 7 pages before the demo, so there's no fallback plan. Page 6 shows the three pilgrims' faces, so it depends on the re-designed characters (item 6).
3. **Opening cutscene: the general story** (one scene; spec `docs/features/opening-cutscene.md`, branch `feature/opening-cutscene`). **Next code to build.** A small cutscene system (static images + text, skippable, gamepad), on placeholder art until the pages arrive. Why: players start the demo with no context.
4. **Pilgrim's Chronicle** (cutscene replay menu, same spec and session): replays the cutscenes the player has seen; main menu only; saved with the meta-progression, so it lasts between runs. Why: the owner's request; it also starts the screen the lore compendium can share.

## Before the Steam demo (musts, trimmed 2026-10-29)
**Code**
5. **Balance pass: enemy and player damage**, with the designer in the sheet, after this round of tester feedback. Why: the Whirlwind now deals several times its old crowd damage, and Attack Speed was cut hard. Also check how far apart a demo player arrives at the boss: the fewest fights to reach it are 3 on `act1`, against 7 on `act1B` and 6 on `act1C` (checked 2026-11-01).

**Waiting on art**
6. **Re-designed characters** (art): new sprites, then adjust the player prefabs (dimensions, colliders, hitboxes, the Corrupted outline margin, animators). Why: the demo's look.
7. **New scenarios** (art): replace the level scenes, adjust levels to the new sizes (spawn points, obstacles, camera bounds). Non-combat levels (shop, campfire, treasure, events, shrine) show the other two pilgrims with idle animations (sitting, sleeping, chatting); kept in the demo by the owner (2026-10-29). Why: the demo's look; the companions fit the lore (the three travel together).
8. **Boss invulnerability shield** (art): the art team adds a shield to show the first phase's invulnerability; wire it to `EnemyHealth.IsImmuneFlag`. Why: tester feedback; the see-through effect was removed.
9. **Adjust the melee attacker behavior** (art): the art team reworks the animation so no slowed windup is needed, and the attack area changes from a circle to a cone. Why: the LightDevourer's warning (`AttackTelegraph`) is a placeholder ring today; the vendored indicators package may suit cones.

**Launch**
10. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Demo update (after the demo launches)
_Moved out of the demo on 2026-10-29 to protect the December date; candidates for a demo update or 1.0._
11. **Item unlocks in the Skill Tree** (probably a new tab): spend Favors to unlock items into the drop pool. Needs a spec. Why: more reasons to spend Favors; the 3 locked lootable items can never drop today.
12. **More items**, especially unlockable ones and higher qualities (Epic and Legendary exist but have no items). Designer work in the sheet plus New Item; new behavior types only where needed. Why: build variety for the demo; pairs with the Skill Tree item unlocks.
13. **Overworld map redesign** (LevelSelector scene): decorations, a better look, and if possible a visual fog of war over the hidden nodes (the reveal logic already exists in `MapRunController`). Needs a spec and art. Why: the map is the demo's hub screen.
14. **Cutscene design for the other 25 scenes, and voice narration research:** cost, timeline and options for narrating the cutscenes (Spanish and English), plus what the cutscene system would need (audio per page, subtitles). Why: decides whether the cutscenes are narrated (the demo's opening scene ships with text only).
15. **Lore compendium:** a menu showing the items, enemies and encounter events the player has interacted with; entries unlock as they are met and stay between runs. Joins the Pilgrim's Chronicle. Needs discovery ids in the meta save and hooks where items are gained, enemies met and events seen. Items and events already have text; **enemies have no description field yet**, so they need writing (narrative session; Act II's lines are in its spec). Needs a spec. Why: gives the lore a home, and suits the audience.
16. **Delete the obsolete SQLite pipeline.** Why: dead code; do it after this tester round confirms the sheet path.
17. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD and `Lore.md`. Why: outdated, Spanish, contradicts the lore; useful for the Steam page.

## Delivery (not started)
18. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo builds; worth having before the demo goes up. **The best code to take if the cutscene work runs out before the art arrives,** since it has a December date.
19. **EditMode tests in CI** on every push (910 tests exist).

## For 1.0
20. **Boss architecture refactor, before the second boss.** Scoped by `docs/features/act-ii-design.md` section 4's table; chunk 9 of its build order. Why: the Act I boss's systems weren't built for several bosses; the demo's boss must play the same after it.
21. **Boss reward:** when a boss dies, the player picks 1 of 3 powerful items that only bosses grant. Happens after the boss, before the next act's map (chunk 11). Why: a payoff for each act; needs boss-only items (a new drop source).
22. **Cutscenes, full scope** (changed 2026-10-29): one opening and one ending, plus one per character before and after each act: 2 + 3 × 4 × 2 = **26 scenes**. Static images and text; narrated if the voice research says it's feasible. After-act and before-act scenes play back to back, and scenes already seen need a skip, since Act I's get replayed. Why: each pilgrim's arc is told through them (`Lore.md` §6).
23. **Run structure across acts:** decided 2026-11-01: one Pilgrimage through all four acts. Boss flow is chunk 2 of the build order. Check the length once it's playable (about 1.5 hours estimated; GDD Q15).
24. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
25. **Steam achievements.** Why: achievement hunters are a target audience.
26. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
27. **Input remapping.** Why: expected on Steam.
28. **Act II build** (spec `docs/features/act-ii-design.md`, 12 chunks in its section 6, on `feature/act-ii` after the demo; chunk 1, the difficulty curve, can land on `develop` any time). Why: designed in full on 2026-11-01. **Acts III–IV content** still needs design: bosses, enemies, act mechanics (Pools of Corruption, Will Break, Darkness), and each act must stay about Act I's size. Why: 1.0 scope.
29. **Acts II–IV events.** Why: `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla, Faelin judges Orleone, Werlem finds Kyro's work).
30. **Key items that unlock parts of the map.** Why: planned, needs a design.

## Non-programming (art/design team)
- **Demo:** re-designed character sprites; scenarios and companion idle animations; boss shield; opening cutscene art (7 pages, briefs in the spec, delivery promised before the demo). Later: map decorations (demo update); the other cutscenes' art (1.0); event art; Favor icon; 2 s Whirlwind animation; melee telegraph sprites; a check that the attack animations read well sped up (Attack Speed fix).
- **Act II (1.0):** the art brief is in `docs/features/act-ii-design.md` section 5, in priority order. World first, since it decides how readable everything else is. It comes after the demo's art.
- **Owner, lore:** five questions on Act II (spec, "Lore questions for the owner"). None blocks the build; they need answers before those texts ship.

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
