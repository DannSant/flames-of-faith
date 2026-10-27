# Backlog

_Last updated: 2026-10-26. Ordered around the next tester build, then the Steam demo (December 2026). Each line: item, then why._

## Now
1. **Narrative design for the run encounter events** (design session; spec `docs/features/run-encounter-events.md`). Why: the events rework needs its content first, and the lore has contradictions that block writing (listed in the spec).
2. **Run encounter events rework, systems** (implementation, after 1). Why: the last open tester-feedback item; events are the demo's main lore delivery.
3. **Next tester build** (merge `develop` → `main`, tag, patch notes). Why: a lot changed since v0.5.1 and testers haven't played it.

## Next (before the demo)
4. **High Attack Speed fix** (decision pending, see PROJECT_STATE). Why: builds break at high Attack Speed.
5. **Balance pass: enemy and player damage**, with the designer in the sheet. Why: tester feedback; the Whirlwind now deals several times its old crowd damage.
6. **Delete the obsolete SQLite pipeline.** Why: dead code; deferred until a play-test round confirms the sheet path.
7. **Rewrite `OnePageGDD.md`** in English from the Ten Page GDD. Why: it's outdated and Spanish (marked as such).
8. **Steam demo prep:** store page, demo build settings, Steam Deck text-size check. Why: December 2026 date.

## Delivery (not started)
9. **CI/CD:** GitHub Actions Unity build when `develop` is merged into `main`, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI). Why: repeatable demo and tester builds.
10. **EditMode tests in CI** on every push (~730 tests exist).

## For 1.0
11. **Decide the run structure across acts** (GDD Q14). Why: blocks Act II–IV planning, saves and difficulty scaling.
12. **Chaos and Holy debuffs** (decided: implement). Why: in the 1.0 scope.
13. **Steam achievements.** Why: achievement hunters are a target audience.
14. **Spanish localization** (Chinese and Japanese optional). Why: 1.0 scope; needs a font for CJK.
15. **Input remapping.** Why: expected on Steam.
16. **Acts II–IV content:** bosses, enemies, act mechanics (Frozen, Pools of Corruption, Will Break, Darkness). Why: 1.0 scope; boss designs are TBD.
17. **Epic and Legendary items; items in the meta-progression unlocks.** Why: build variety and more reasons to spend Favors.
18. **Cutscenes** (8 static scenes). Why: 1.0 scope; content follows the narrative work.
19. **Key items that unlock parts of the map.** Why: planned, needs a design.

## Non-programming (art/design team)
- Player re-design sprites; level scenario design (may shrink levels); event art; Favor icon; boss invulnerable look; 2 s Whirlwind animation; melee telegraph sprites.

## Parked / cut
- Hard mode: not planned, not ruled out.
- Xbox port: after 1.0.
