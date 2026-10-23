# Backlog

_Last updated: 2026-10-23. Order is a PROPOSAL, to be confirmed in the interview. Each line: item, then why._

## Now (gates the next tester build)
1. **Run the open manual tests** (`todo.md`, 80 items). Why: the 6.3 upgrade and the pilot changed a lot of code paths and none of it is verified in a build.
2. **Sheet round trip** (deploy script, publish, pull, test build). Why: the designer is waiting and the pilot is untested against the real sheet.

## Next
3. **Balance pass: enemy and player damage.** Why: tester feedback; tooling now exists (sheet, damage preview, reports).
4. **ItemBagPane, attack speed stats and other known bugs.** Why: reported by testers, needs a triage list first.
5. **Small rework on the Whirlwind.** Why: Warrior balance without Cleave is unproven.
6. **Run encounter events improvements.** Why: events now have an art slot but no art; more events add replay value.

## Later
7. **Add items to the meta-progression unlock system.** Why: more reasons to spend Favors.
8. **Key items that unlock parts of the map.** Why: new feature, touches the Overworld map system, needs a spec.
9. **Input remapping.** Why: expected on Steam; gamepad is already supported.
10. **Localization.** Why: Steam reach. The design docs are in Spanish; UI is English. Needs a decision on scope.
11. **Data-driven phase 2:** debuffs, Grace/Corruption, stat formulas, item drops (`ItemProvider`), Effects DB into the sheet. Why: designer asked for it; do after the pilot proves out.

## Delivery (not started)
12. **CI/CD:** GitHub Actions Unity build on merge to the release branch, Steam deploy (SteamPipe). Needs a Unity license strategy (GameCI) and a decision on the branch model.
13. **Unity Test Framework in CI:** the 699 EditMode tests already exist; run them on every push.

## Non-programming (art/design team)
- Player re-design sprites.
- Level scenario design (could change level sizes).

## Parked / cut
- Nothing cut yet.
