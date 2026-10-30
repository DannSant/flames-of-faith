# Backlog

_Last updated: 2026-10-29. Ordered around the next tester build, then the Steam demo (December 2026). Each line: item, then why._

## Now
1. **Run encounter events rework, systems** (implementation session; spec `docs/features/run-encounter-events.md`, narrative done). Why: the last open tester-feedback item; events are the demo's main lore delivery.
2. **Next tester build** (merge `develop` → `main`, tag, patch notes). Why: a lot changed since v0.5.1 and testers haven't played it.

## Next (before the demo)
3. **High Attack Speed fix** (decision pending, see PROJECT_STATE). Why: builds break at high Attack Speed.
4. **Balance pass: enemy and player damage**, with the designer in the sheet. Why: tester feedback; the Whirlwind now deals several times its old crowd damage.
5. **Delete the obsolete SQLite pipeline.** Why: dead code; deferred until a play-test round confirms the sheet path.
6. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD and `Lore.md`. Why: it's outdated, Spanish, and now contradicts the lore.
7. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Delivery (not started)
8. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo and tester builds.
9. **EditMode tests in CI** on every push (~730 tests exist).

## For 1.0
10. **Decide the run structure across acts** (GDD Q14). Why: blocks Act II–IV planning, saves and difficulty scaling.
11. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
12. **Steam achievements.** Why: achievement hunters are a target audience.
13. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
14. **Input remapping.** Why: expected on Steam.
15. **Acts II–IV content:** bosses, enemies, act mechanics (Frozen, Pools of Corruption, Will Break, Darkness). Why: 1.0 scope; boss designs are TBD.
16. **Epic and Legendary items; items in the meta-progression unlocks.** Why: build variety and more reasons to spend Favors.
17. **Cutscenes** (8 static scenes). Why: 1.0 scope; the content can draw on `Lore.md` §6 (the pilgrims' arcs, the legend of the descendant who relit the altars).
18. **Key items that unlock parts of the map.** Why: planned, needs a design.
19. **Acts II–IV events.** Why: `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla, Faelin judges Orleone, Werlem finds Kyro's work).

## Non-programming (art/design team)
- Player re-design sprites; level scenario design (may shrink levels); event art; Favor icon; boss invulnerable look; 2 s Whirlwind animation; melee telegraph sprites.

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
