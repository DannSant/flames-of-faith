# Backlog

_Last updated: 2026-11-06 (Light Devourer art merged; items 38 to 42 specced in `docs/features/demo-musts.md`). Ordered around the v0.5.3 tester round, then the Steam demo (December 2026). Each line: item, then why. Numbers stay stable, because specs refer to them._

## Now (while testers play)
1. **Tester build v0.5.2:** released on 2026-10-30 (`main`, tag `v0.5.2`, patch notes in `PatchNotes/notas-desde-v0.5.1.md`). Left: make the Windows build, send it to testers, collect feedback. Why: a lot changed since v0.5.1, and every manual test has passed.
2. **Cutscene design:** done 2026-11-01 (7 pages with art briefs, `docs/archive/features/opening-cutscene.md`). Left for the owner: send the briefs to the art team and agree the size (1920 × 1080). The owner says the team will deliver all 7 pages before the demo, so there's no fallback plan. Page 6 shows the three pilgrims' faces, so it depends on the re-designed characters (item 6).
3. **Opening cutscene:** done 2026-11-02, merged into `develop`, manual tests passed. It runs on placeholder art: the final pages overwrite `Sprites/Cutscenes/Opening/Opening_Page1..7.png` (1920 × 1080).
4. **Pilgrim's Chronicle:** done 2026-11-02 (same spec). It has a tab structure with one tab, Scenes, so the lore compendium can join it.

## Before the Steam demo (musts, trimmed 2026-10-29)
**Code**
5. **Balance pass: enemy and player damage**, with the designer in the sheet, after this round of tester feedback. Why: the Whirlwind now deals several times its old crowd damage, and Attack Speed was cut hard. Also check how far apart a demo player arrives at the boss: the fewest fights to reach it are 3 on `act1`, against 7 on `act1B` and 6 on `act1C` (checked 2026-11-01). Testers (v0.5.2) ask for **more boss health**; boss numbers live on the boss's own assets, outside the enemy damage system.

**From v0.5.2 tester feedback** (2026-11-04, from `todo.md`; 35 to 37 specced in `docs/archive/features/tester-fixes-v052.md`, branch `fix/tester-feedback-v052`)
35. **Done, merged 2026-11-04 (`30acb5e`); manual checks passed 2026-11-06.** **Melee enemies resist knockback while attacking:** the LightDevourer (the only melee attacker) shouldn't be pushed out of its swing. Small fix, likely in `AIMeleeAttacker` / the knockback behavior during `context.isShooting`. Pairs with item 9 if the art lands first. Why: being knocked mid-swing makes the warning lie about where the hit lands.
36. **Done, merged 2026-11-04 (`30acb5e`); manual checks passed 2026-11-06.** **A class option for Ayteel at the Wayside Shrine:** she has a class line there but no option, which feels off. Narrative work in the event's asset, within the content rules (at most one class option per class, a first-screen option risks a cost). Why: the other pilgrims' class content sets the expectation.
37. **Done, merged 2026-11-04 (`30acb5e`); manual checks passed 2026-11-06.** **Move the sound packs out of `Resources/`:** all 736 clips (124 MB) sit under `Resources/Audio/`, so every one ships in the build, used or not. Move them to a non-Resources folder; the `GameSoundTable` and prefab references keep working, and only referenced clips ship. Do it before the demo build. Why: build and download size. Result: 711 of the 736 clips (118 MB) are unused; the list is in `docs/archive/features/tester-fixes-v052-unused-clips.md`, and the owner keeps them for now (they no longer ship).

**Game feel** (spec `docs/archive/features/game-feel.md`). Keep it restrained: the Corrupted outline exists because VFX had got too noisy.
31. **Low-health warning:** done 2026-11-04, merged, manual tests passed. The vignette is a placeholder sprite (art list).
32. **Hit feel:** done 2026-11-04, merged, manual tests passed: shake, hit stop and slow motion, rumble on five moments, Settings sliders. Shake strengths are first guesses; tune them while play-testing.
33. **Sound pass:** done 2026-11-04, merged, with clips from the owner's packs and per-class voices. Left: two manual checks (the heartbeat, the once-per-event sounds) and whatever `Tools/Flames of Faith/Audio/Report Silent Sounds` still lists. See item 37 for the packs' build size.
34. **VFX pass:** after the re-designed characters and scenarios arrive (items 6 and 7), because effects must match the new palette and sizes. Why: doing it now means doing it twice.

**From the owner's list** (2026-11-06, from `todo.md`; all five are demo musts, owner's decision the same day; **specced together in `docs/features/demo-musts.md`, branch `feature/demo-musts`**)
38. **Dreko Eye's Corrupted outline doesn't fit its sprite** in some directions. Likely the outline copy (flip or sprite) or the sheet's mesh margin per direction; `CorruptionOutlineTests` should catch it once found. Small fix. Why: the outline is how players read Corruption.
39. **Gamepad, class select:** after choosing a class with A, move the selection straight to Start Run. Small fix in `MainMenuController`. Why: one less step, and it avoids walking back past the other cards.
40. **Rename the classes: Warrior to Crusader, Archer to Sentinel** (owner's decision, 2026-11-06; Augur stays). Everything is renamed once, internal names included (owner, 2026-11-06): player-facing names (class select, skill tree, events, GDD, `Lore.md`, the tester guide) and code and assets (`PlayerClass` and the class data, prefabs, kit item names such as "Warrior Emblem", which are the balance sheet's keys). Saves store some of this by name, so it needs aliases or save checks; the internal Mage / Augur mismatch could be fixed in the same pass. Why: the names match the lore (Crusader of the Faith, Sentinel of Abidor). Before the Steam page shows class names.
41. **Character select with tabs: Abilities, Lore, Playstyle.** A restyler update (`MainMenuRestyler`); the session drafts the Lore and Playstyle texts and the owner reviews them. The Playstyle tab can reuse the class stat guide (Key and Complementary stats). Why: new players learn a class before choosing it.
42. **The pilgrim on the map:** animate the map marker (idle, walking between nodes) with the class's real sprite. The pilgrim walks along the path, then waits; entering stays a second action. For the demo; best done once the re-designed characters (item 6) arrive, so it's done once (it can start on the current sprites if they're late). Why: the map is the demo's hub screen.

**Waiting on art**
6. **Re-designed characters** (art): new sprites, then adjust the player prefabs (dimensions, colliders, hitboxes, the Corrupted outline margin, animators). Why: the demo's look.
7. **New scenarios** (art): replace the level scenes, adjust levels to the new sizes (spawn points, obstacles, camera bounds). Non-combat levels (shop, campfire, treasure, events, shrine) show the other two pilgrims with idle animations (sitting, sleeping, chatting); kept in the demo by the owner (2026-10-29). Why: the demo's look; the companions fit the lore (the three travel together).
8. **Boss invulnerability shield** (art): the art team adds a shield to show the first phase's invulnerability; wire it to `EnemyHealth.IsImmuneFlag`. Why: tester feedback; the see-through effect was removed.
9. **Adjust the melee attacker behavior** (**done, merged 2026-11-06, `e48d21e`**; spec `docs/archive/features/light-devourer-art.md`): the new sheets in the game through a reusable enemy sheet builder, no slowed windup (it's drawn into the attack now), and the circle stays (owner and art team, 2026-11-05: no cone) but lies on the isometric ground as an ellipse, for the warning and the hitbox alike. Why: the demo's look, and the warning must show exactly where the hit lands.

**Launch**
10. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Demo update (after the demo launches)
_Moved out of the demo on 2026-10-29 to protect the December date; candidates for a demo update or 1.0._
11. **Item unlocks in the Skill Tree** (probably a new tab): spend Favors to unlock items into the drop pool. Needs a spec. Why: more reasons to spend Favors; the 3 locked lootable items can never drop today.
12. **More items**, especially unlockable ones and higher qualities (Epic and Legendary exist but have no items). Designer work in the sheet plus New Item; new behavior types only where needed. Why: build variety for the demo; pairs with the Skill Tree item unlocks.
13. **Overworld map redesign** (LevelSelector scene): decorations, a better look, and if possible a visual fog of war over the hidden nodes (the reveal logic already exists in `MapRunController`). Needs a spec and art. Why: the map is the demo's hub screen.
14. **Cutscene design for the other 7 scenes, and voice narration research:** cost, timeline and options for narrating the cutscenes (Spanish and English), plus what the cutscene system would need (audio per page, subtitles). Why: decides whether the cutscenes are narrated (the demo's opening scene ships with text only).
15. **Lore compendium:** a menu showing the items, enemies and encounter events the player has interacted with; entries unlock as they are met and stay between runs. Joins the Pilgrim's Chronicle. Needs discovery ids in the meta save and hooks where items are gained, enemies met and events seen. Items and events already have text; **enemies have no description field yet**, so they need writing (narrative session; Act II's lines are in its spec). Needs a spec. Why: gives the lore a home, and suits the audience.
16. **Delete the obsolete SQLite pipeline.** Why: dead code; do it after this tester round confirms the sheet path.
17. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD and `Lore.md`. Why: outdated, Spanish, contradicts the lore; useful for the Steam page.

## Delivery (not started)
18. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo builds; worth having before the demo goes up. **The best code to take next,** since it has a December date and touches no scene or prefab. Needs a spec.
19. **EditMode tests in CI** on every push (over 930 tests exist).

## For 1.0
20. **Boss architecture refactor, before the second boss.** Scoped by `docs/features/act-ii-design.md` section 4's table; chunk 9 of its build order. Why: the Act I boss's systems weren't built for several bosses; the demo's boss must play the same after it.
21. **Boss reward:** when a boss dies, the player picks 1 of 3 powerful items that only bosses grant. Happens after the boss, before the next act's map (chunk 11). Why: a payoff for each act; needs boss-only items (a new drop source).
22. **Cutscenes, full scope** (changed 2026-11-06): one at the start and one at the end of each act, for the party: **8 scenes** (the opening is Act I's start, Act IV's end is the ending). Static images and text; narrated if the voice research says it's feasible. The act-end and next act-start scenes play back to back, and scenes already seen need a skip, since Act I's get replayed. A future party (a DLC) would get its own 8. Why: the story's big beats between acts; each pilgrim's arc stays in the events (`Lore.md` §6).
23. **Run structure across acts:** decided 2026-11-01: one Pilgrimage through all four acts. Boss flow is chunk 2 of the build order. Check the length once it's playable (about 1.5 hours estimated; GDD Q15).
24. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
25. **Steam achievements.** Why: achievement hunters are a target audience.
26. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
27. **Input remapping.** Why: expected on Steam.
28. **Act II build** (spec `docs/features/act-ii-design.md`, 12 chunks in its section 6, on `feature/act-ii` after the demo; chunk 1, the difficulty curve, can land on `develop` any time). Why: designed in full on 2026-11-01. **Acts III–IV content** still needs design: bosses, enemies, act mechanics (Pools of Corruption, Will Break, Darkness), and each act must stay about Act I's size. Why: 1.0 scope.
29. **Acts II–IV events.** Why: `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla, Faelin judges Orleone, Werlem finds Kyro's work).
30. **Key items that unlock parts of the map.** Why: planned, needs a design.

## Non-programming (art/design team)
- **Demo:** re-designed character sprites; scenarios and companion idle animations; boss shield; opening cutscene art (7 pages, briefs in `docs/archive/features/opening-cutscene.md`, delivery promised before the demo). Later: a blood-like low-health vignette to replace the placeholder; map decorations (demo update); the other cutscenes' art (1.0); event art; Favor icon; 2 s Whirlwind animation; melee telegraph sprites; a check that the attack animations read well sped up (Attack Speed fix).
- **Act II (1.0):** the art brief is in `docs/features/act-ii-design.md` section 5, in priority order. World first, since it decides how readable everything else is. It comes after the demo's art.
- **Owner, lore:** five questions on Act II (spec, "Lore questions for the owner"). None blocks the build; they need answers before those texts ship.

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
