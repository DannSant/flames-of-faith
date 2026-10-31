# Feature: Act II design and the run structure

- **Status:** ready for the design session
- **Branch:** `develop`, docs only. No code, assets, scenes or sheet edits.
- **Owner session:** design session (not started)
- **Related docs:** `game-design-docs/TenPageGDD.md` §4, §7, §10 and §13 (questions 1, 3, 5, 6 and 14), `game-design-docs/Lore.md`; `CLAUDE.md` "Enemy damage", "Corruption & Grace", "Run save / load", "Two parallel map systems", "Balance data"; `docs/pm/BACKLOG.md` "For 1.0"

## Goal
Turn Act II from a concept into something buildable: settle how a Pilgrimage spans the acts, then design the act's mechanics, enemies and boss in enough detail that implementation sessions can start without asking, and the art team gets its next brief early.

Why now: the demo's remaining code waits on art and tester feedback, so this is free time. Design conflicts with nothing, and it produces the next round of coding work.

## Scope
1. **Run structure** (GDD Q14), decided by the owner, with what the choice costs.
2. **Act II rules:** Frozen and Frozen Enemies, including GDD Q3 and Q5.
3. **Act II enemies:** a behavior spec for each enemy and the Corruptor.
4. **The Ice Bull:** the boss's lore, phases, abilities and animations.
5. **Art brief:** what Act II needs from the art team.
6. **Build order:** implementation chunks, each marked by whether it can start on placeholder art.

## Out of scope
- Code, prefabs, scenes, ScriptableObjects, CSVs and the balance sheet.
- Final numbers. Use low / medium / high and damage tiers; the designer tunes them in the sheet.
- Acts III and IV, beyond noting what a decision here forces on them. Don't design their bosses.
- Anything in the demo. If this session finds a demo problem, write it under "Follow-ups" below.
- Cutscene content, which belongs to the opening cutscene session.

## Design details
All of this goes in the sections below. **This session edits only this file.** The opening cutscene session is working in the same checkout and may edit the GDD, so GDD and `Lore.md` changes go under "Changes for the Orchestrator", who applies them afterwards.

### 1. Run structure
_To be filled._ Compare the GDD's two options, then get the owner's decision. Check the code rather than assuming; these are the starting points:
- `MapRunController` already moves to the next act after a boss, through `ChangeAct` and `runState.acts`, and marks the run complete after the last act. One run through all the acts may be closer to working than the GDD suggests.
- `Resources/Overworld/` only has Act 1 maps today.
- The run save stores the **current** act's map only.
- `levelsBeaten` feeds `EnemyDamageCalculator`, the loot rolls and `Enemy` scaling, so the expected-player curve depends on this decision.
- A boss kill banks the run's Favors and deletes the save, which fits an act-per-run game. A run across acts would need to change that.

Record what the decision changes: saves, the difficulty curve, Favor pacing (a typical lost run earns about 12–18 Favors), the boss reward, cutscene placement and achievements.

### 2. Act II rules
_To be filled._ For Frozen and Frozen Enemies: exact rules, how the player reads them on screen, what counters them, and how they interact with Grace, Corruption, Evasion, armor, dash and Retribution.
- Frozen is marked optional in the GDD: decide whether it's in.
- A stun that also adds knockback risks feeling unfair in a survivor-like, so say how the player escapes or avoids it. Frost has never hit the player, so check how debuffs reach enemies today and what applying one to the player would need.
- Answer GDD Q3 (is breaking the ice optional, does it reward anything?) and Q5 (do these mechanics continue into Acts III–IV?).

### 3. Act II enemies
_To be filled._ One entry each for the Frozen Slime, Ice Sprite, Awoken, Ice Golem, Frost Ghoul, Death Witch and the Echo of Malizante:
- **Role:** what it asks of the player, and which enemy it pairs well with.
- **Behavior:** movement, attack, telegraph, and how it uses obstacles.
- **Built from:** the existing behavior pieces in `Scripts/AI/Behaviors/` it reuses, and each new piece it needs. New pieces should be small and reusable, per "Behavior composition over inheritance".
- **Damage tiers:** contact and projectile, plus relative health and speed.
- **Animations and events:** which clips it needs and where the gameplay events fall.
- **Lore:** one or two lines that follow the Glossary, for the compendium later.

Keep the roster small enough to build. If 6 enemies plus a Corruptor is too many for 1.0, say which to cut. Compare each against Act I so none is a reskin without a reason. The Echo of Malizante replaces the flee-and-shoot pattern, so check what `CorruptorPhaseController` assumes about a Corruptor: a teleporting one may change its timer, camera focus and off-screen arrow.

### 4. The Ice Bull
_To be filled._ Lore, phases, abilities with their telegraphs, adds, and the animation list, in the format of the GDD's Taio'Sandir section. The GDD proposes keeping Act I's structure (an invulnerable phase while the Altar of Malien fills, then the real fight): accept or change it, keeping in mind that a second boss with the same structure may feel like a repeat.

Note what the design needs that `Act1BossBehavior`, `BossController` and the ability pieces can't do. That list scopes the boss refactor (BACKLOG, "Boss architecture refactor").

### 5. Art brief
_To be filled._ A list for the art team, in priority order: enemy sprites and sheets (per "Enemy sprite sheets & animators"), the boss, tilesets and scenarios, the ice block, VFX and map art. Note what can reuse existing art. New enemies add to the art team's queue, so mark what's essential.

### 6. Build order
_To be filled._ Implementation chunks, in order, each with:
- what it depends on, and whether it can start on placeholder art;
- the balance tabs it adds (each new behavior type gets an `EnemyBehaviors_<Type>` tab, and every tab means work in the sheet pipeline);
- whether it can stay off `develop`'s demo path, so 1.0 work adds no risk to the December build.

`DataHygieneTests` allows no dead data: every behavior asset must be used by an enemy, every level must be on a map, and its scene must be in the build. Plan for that, since a level built early would end up in the demo build unless it stays on a branch.

### Open questions for the owner
_Filled during the session. Ask one at a time, each with a recommendation._

### Follow-ups
_Demo problems or ideas found along the way, for the Orchestrator's backlog._

### Changes for the Orchestrator
_Exact edits to apply to `TenPageGDD.md` and `Lore.md`, by section. Include the GDD §13 answers, and any lore question for the owner._

## Acceptance criteria
- [ ] The owner has chosen the run structure, and its effects are listed.
- [ ] Frozen, Frozen Enemies, Q3 and Q5 are decided, or cut.
- [ ] Each Act II enemy has a complete entry, with its new behavior pieces named.
- [ ] The Ice Bull can be built from its section without asking the owner about its design.
- [ ] Every name follows `Lore.md`'s Glossary, and no _(secret)_ leaks into player-facing text.
- [ ] The art brief and the build order are finished.
- [ ] Only this file changed.

## Files likely touched
- Docs: this file only.
- Code, prefabs, scenes, assets, CSVs, sheet tabs: none.

## Tests
None: this session changes no code. Its build order should name the guard tests each chunk needs.

## Handoff
_Written by the design session when it finishes. Nobody else edits this section._

- **Done:**
- **Not done / follow-ups:**
- **Commits / branch:**
- **Gotchas for the next session:**
- **Manual tests added to todo.md:** none
