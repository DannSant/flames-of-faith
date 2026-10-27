# Feature: Run encounter events rework (narrative + systems)

- **Status:** draft. Narrative design first (design session), then the systems work (implementation session).
- **Branch:** `feature/run-encounter-events` (off `develop`) for the implementation. The narrative session only writes docs, on `develop`.
- **Owner session:** narrative session (lore and event content), then an implementation session TBD
- **Related docs:** `game-design-docs/TenPageGDD.md` (§3 Story, §9 Run encounters, §13 Open questions), `game-design-docs/Lore.md` (to be created by the narrative session), `CLAUDE.md` "Run encounter events", "Item drops", glossary; tester feedback archived in `docs/archive/todo-2026-10-26.md`

## Goal
Events are where the player meets the lore. Make them read as small stories, not as stat menus, and give each of the three pilgrims a light personal arc through them.

## Scope
From tester feedback (2026-10-26):
- **Vaguer option hints.** Testers skip the text and read only the outcome tooltip ("May reduce health 10-20 and grant an item"). Replace it with an in-world hint ("They seem offended; a fight could break out, but they carry riches...").
- **Narrated results.** The Results window lists only numbers ("You gained 10 health, you lost 5 Grace"). Add what happened, with the numbers woven in ("They attacked, but were no match: you lost only 4 health, and among their loot you found <item>").
- **More stats as modifiers.** Luck stays the default, but e.g. a fight can use Melee/Ranged/Magic Damage or Armor, and fleeing can use Move Speed.
- **Multi-step events (evaluate).** An option can lead to a follow-up question. Example: the Faraclarian Clan asks "then describe the Forger", with a right and a wrong answer.
- **Class-specific options.** The three classes are now three characters (Ayteel, Faelin, Werlem), so some options or lines differ by class.
- **Lore and character arcs.** Each event carries a piece of the world, and across a Pilgrimage the events hint at each pilgrim's story (their grandparents, Gabien's fall).

## Out of scope
- Event art (the art panel exists; art is the art team's).
- Cutscenes (GDD §11).
- New map node types.
- Acts II–IV events, unless the narrative session proposes them as lore notes.

## Design details

### Narrative (owned by the narrative session)
_To be filled: the story bible lives in `game-design-docs/Lore.md`; this section lists the event content in a form an implementation session can author directly._
For each event: title, intro text, per-class variants (if any), options (label + in-world hint), outcomes (what happens, which stat modifies it, narrated result text with placeholders for numbers and items), follow-up steps, and the lore it reveals.

### Systems (for the implementation session, after the narrative)
- Current data: `EventEncounterData` (title, text, options, optional art) → `EventOptionsList` (label + actions) → `OptionActions/` (`Change{Currency,Grace,Health,Favors}Option`, `GiveRandomItemOptionAction`), rolls through `StatBiasedRoll` with `biasStat`.
- Likely needs: a hint text per option, result text templates per outcome, a stat per roll (not only Luck), an optional next step per option, a class filter per option or line.
- Player-facing words follow the glossary (Pilgrimage, Golden Chronos).
- If event numbers become a balance table, follow `CLAUDE.md` "Keep the Google Sheets pipeline in step".

## Open questions
1. Multi-step events: how deep (2 steps max?), and does a wrong answer always cost something?
2. Class-specific: whole extra options, or the same options with different lines?
3. Which stats can modify which outcomes, and how strongly compared with Luck?
4. How much number detail stays visible: none before choosing, exact numbers after?
5. Event count for the demo (December 2026): rework the 5 + Holy Altar only, or add new ones?
6. Lore contradictions to settle first, see below.

### Lore integrity issues found by the Orchestrator (2026-10-26)
Settle these in `Lore.md` before writing event text; then fix the GDD to match.
1. **Timeline:** the story is set "shortly after the year 1000 of the Decadent Age", Taio'Sandir came "during the Dark Age" about 6,000 years ago, and Gabien fought the Drekos two generations ago (GDD open question 13). How many eras and invasions are there?
2. **Churches:** the "augurs of the White Church" start the Pilgrimage, but the church of Ashemdrek became the "Shardless Church", and Werlem is an Augur of the Shardless Church. One church or two?
3. **Places:** Ashemdrek is the region, a fallen kingdom and Act IV's ruins; Kanator is the world; Abidor is where Ayteel and Faelin grew up. Which kingdom do the Centinels defend? Is Abidor a city or a kingdom?
4. **Names and spelling:** "Centinel" vs "Sentinel"; "Jester Forest" vs "Jesterwoods"; are the Faraclarian Clan the people of the Uf'Faraclario Wastes?
5. **Gabien:** "it is said he lost and let the darkness spread", yet Act IV's Corruptor is the "Echo of Gabien". Was he corrupted, betrayed, or slandered (Faelin's grandfather Orleone is also blamed)? This is the spine of all three arcs.
6. **Proper nouns used in game with no lore yet:** Vega (Casket), Gracoy (Raiders), the House of Stars (elves), the Forger, Mastow (the Mastow Affinity stat), Runic Math, Golden Chronos, the Years of the Miasma, the Elven Shop, Malien's altars vs the "Forgotten Altar of Malien" event.
7. **Protagonist:** the original story had Ayteel travel alone, shunned; now three pilgrims travel together. Does each Pilgrimage follow one of them alone (the class chosen), with the others as off-screen companions?

## Acceptance criteria
_Draft; finalized after the narrative is written._
- [ ] No option tooltip shows raw numbers before choosing.
- [ ] Every outcome has a narrated result text; numbers and item names fill placeholders.
- [ ] At least one option per event uses a stat other than Luck.
- [ ] At least one multi-step event.
- [ ] At least one class-specific option or line per class.
- [ ] Guard tests: every option has a hint, every outcome a result text, every placeholder is filled (extend `EventEncounterTests`).

## Files likely touched
- Code: `Scripts/RunEncounters/` (event data, option actions, `EventEncounterUI`, `RunEncounterResultUI`), `StatBiasedRoll`.
- Prefabs / scenes / assets: `Resources/RunEncounters/EventEncounter/*`, `Prefabs/RunEncounters/EventEncounter.prefab`, the event window prefabs under `Prefabs/UI/RunEncounters/` (layout through `RunEncounterRestyler`).
- Docs: `game-design-docs/Lore.md`, `TenPageGDD.md` §9, `CLAUDE.md` "Run encounter events".

## Tests
- EditMode tests to add: see acceptance criteria.
- Manual checks (go in `todo.md` "Pending manual tests"): play each event with each class.

## Handoff
_Written by the implementation session when it finishes. Nobody else edits this section._

- **Done:**
- **Not done / follow-ups:**
- **Commits / branch:**
- **Gotchas for the next session:**
- **Manual tests added to todo.md:**
