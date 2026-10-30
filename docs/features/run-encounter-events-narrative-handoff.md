# Handoff: run encounter events, narrative session → Orchestrator

_Written 2026-10-29 by the narrative session, for the Orchestrator. The narrative session could edit only the spec's Narrative and Open questions sections, `Lore.md`, and the GDD's story sections and §13. This file holds what it couldn't edit itself: proposed text for the spec's **Systems** and **Acceptance criteria** sections, and notes for `docs/pm/`. Delete it once applied._

## What was done
| Commit | What |
|---|---|
| `5de5124` | Spec: "Lore decisions", the 7 lore contradictions settled with the owner |
| `2567e01` | `game-design-docs/Lore.md`: the story bible (world, timeline, factions, acts, Gabien, the three pilgrims' arcs, text fixes, glossary) |
| `d785f04` | Spec, "Narrative": 10 events written (authoring rules, checks, class content, steps, placeholders) |
| `96b56e5` | GDD §3 Story rewritten; §13 updated |
| _this commit_ | The owner's clarifications of 2026-10-29 (below), GDD names updated everywhere, this handoff |

**The owner's clarifications (2026-10-29), now recorded in `Lore.md`, the spec and the GDD:**
- Gabien's sword is **Ashem Elwyn**, "Shining Tide".
- The Faraclarian Faun Chief is **not** of Alamoh blood.
- The Act III boss is **Forsaken Daviro the Unholy**.
- The Aylinean event stays Luck-only.

**The owner's own edits, uncommitted in the working tree** (no session committed them; ask the owner):
- **Item texts** from `Lore.md` §8, plus the regenerated `BalanceData/Items.csv`, `ItemStats.csv`, `ItemDrops.csv` and `snapshot.json`.
- **Sacred Writtings renamed to Sacred Writings.** I checked it: the GUID and `effectID` are unchanged, so nothing that refers to it breaks.
- **Level names:** "Jesterwood Cliffs", "Heart of the Forest", "Plerian Shop", "Wayside Shrine of Malien".
- **`.gitignore`:** `lore-docs/` added.
- **Still to do** (left in `Lore.md` §8): rename the item **Quiver of the Centinel** to **Quiver of the Sentinel**, and retitle the Holy Altar event ("Forgotten Altar of Malien") to **Wayside Shrine of Malien**. The second fits the implementation session.

## Proposed text for the spec's "Systems" section
Replace the current "Likely needs" bullet with this (the Narrative section's "How the content is written" is the source):

- **Option data** (`EventOptionsList`):
  - a **hint** (shown in the tooltip instead of the outcome list);
  - an optional **per-class label**;
  - an optional **class filter** (Warrior / Archer / Augur);
  - an **outcome**: either **Fixed** (actions, result text, optional per-class result lines, optional next step) or a **Check** (stat source, difficulty, a Success branch and a Failure branch, each with the same fields as Fixed).
- **Check stat sources:**
  - any `StatType`;
  - **Key stat**, the chosen class's Key stat from `CharacterClassData.statPriorities`;
  - **current Grace**, read from `PlayerGrace` and possibly negative.
- **Check tunables:** the base chance per difficulty (Easy / Even / Hard), the chance per stat point and the cap go on a new settings SO, per the "tunables in settings" rule. If they become a balance table, follow "Keep the Google Sheets pipeline in step".
- **Event data** (`EventEncounterData`): per-class **intro lines**, and a **once per Pilgrimage** flag (Liah).
- **Steps:** a step is a prompt text plus a list of options, with the same structure as the event's options, at most 3 deep. Options reach a step through "next step" fields. The window swaps its content to the step, and the result shows at the end of the chain.
- **Results:**
  - **Structured actions:** actions return a structured result (kind, signed amount, item) instead of a finished string, so the result text can fill `{health}`, `{grace}`, `{chronos}`, `{favors}` and `{item}`.
  - **Optional sentences:** a sentence in `[brackets]` is dropped when its action gave nothing (no eligible item).
  - **The window** (`RunEncounterResultUI`) shows the narration, then a compact colored summary line.
- **Amount sizes** (S / M / L per resource; the table is in the Narrative) map to option action assets. Reuse the existing `Lose…` / `Gain…` assets where their ranges match and add the rest, keeping the naming rule that `EventEncounterTests` checks.
- **Pool:**
  - `EventEncounter.prefab`'s `encountersList` gets the 9 random events.
  - **No repeats:** an event can't appear twice in one Pilgrimage. That is **run state**: the list of seen events must go into `PlayerData`, `RunSaveData` and the `GameSession` mapping, or Continue forgets it (CLAUDE.md, "When adding run state").
  - **Scenes:** the Wayside Shrine keeps its node and scene, and no new scenes are needed.
- **UI:**
  - `EventEncounterUI` shows hints in the tooltips, class-only options and the step swaps.
  - Layout changes go through `RunEncounterRestyler` and `RunEncounterStyleTests`.
- **Text:** all event text stays in the assets (Spanish localization comes in 1.0). Names follow `Lore.md`'s glossary.

## Proposed "Acceptance criteria"
Replace the draft list with:
- [ ] **Hints:** no option tooltip shows numbers before choosing; every option shows its in-world hint.
- [ ] **Results:**
  - every outcome and branch has a narrated result text;
  - every placeholder in a text matches an action in that branch;
  - optional `[sentences]` drop when their action gives nothing;
  - the Results window ends with the colored summary line.
- [ ] **Checks:** they work with a `StatType`, the class's Key stat and current Grace, and their tunables are on a settings asset.
- [ ] **Steps:** steps work up to 3 deep, and every wrong answer costs something.
- [ ] **Class content:** class intro lines, class result lines and class-only options show only for their class. Each class has at least one class-only option: Ayteel in the Faraclarian Clan, Faelin in the House of Stars and the Last Torch, Werlem in the Vega Casket, the Raiders and the House of Stars.
- [ ] **Pure gains:** a first-step option always risks a cost. A pure gain appears only in a step reached by a lore-correct answer. The only exception is the Wayside Shrine's "Kneel and pray".
- [ ] **Non-Luck checks:** every event except Laughter in the Leaves has at least one non-Luck check; the Aylinean is Luck-only on purpose.
- [ ] **Repeats:** no event repeats within a Pilgrimage, and Liah appears at most once. This survives Save & Exit and Continue.
- [ ] **Titles:** the Wayside Shrine's event title is "Wayside Shrine of Malien".
- [ ] **Guard tests** (extend `EventEncounterTests`):
  - hints present;
  - result texts present;
  - placeholders match the actions;
  - `Lose…` / `Gain…` names;
  - step depth ≤ 3;
  - the first-step cost rule;
  - every class has a class-only option.
- [ ] **Manual tests in `todo.md`:**
  - every event with each class;
  - each step chain;
  - an event, then Save & Exit, then Continue: the event doesn't repeat.

## For `docs/pm/`
- **PROJECT_STATE, "In progress":** the narrative is done, and the next step is the implementation session on `feature/run-encounter-events`, using the spec's Narrative section as its content.
- **PROJECT_STATE, uncommitted:** the owner's asset text edits listed above.
- **BACKLOG candidates:**
  - **Rewrite `OnePageGDD.md`:** it is still in Spanish and outdated, now also against `Lore.md`.
  - **Cutscene content** (GDD §13.7) can draw on `Lore.md` §6: the pilgrims' arcs, and the legend of "the descendant of Gabien who relit the altars".
  - **Acts II–IV events:** `Lore.md` §6 places the arcs' turning points there (Ayteel learns about Dayla; Faelin judges Orleone; Werlem finds Kyro's work).
  - **Open lore:** how Forsaken Daviro the Unholy has lived sixty years past Gabien's time (Act III; not needed for the demo).
- **DECISIONS** (if the Orchestrator records lore there):
  - the game is set 60 years after Gabien (not the 150 years of `lore-docs/Timeline.docx`);
  - pure gains are earned through lore answers;
  - "Awoken" (not "Dawner") is the English for the Amanecidos;
  - Shop → Plerian Shop, Holy Altar → Wayside Shrine of Malien, Centinel → Sentinel.
