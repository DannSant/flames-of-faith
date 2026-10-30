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
_Written by the narrative session with the owner, 2026-10-26. The lore behind every line is in `game-design-docs/Lore.md` (§7 has the rules for event writers). Player-facing names follow its glossary._

#### How the content is written (decided with the owner)
- **No numbers before choosing.** An option shows its **label** and, in its tooltip, an in-world **hint** (italics below). Numbers appear only in the result.
- **Stat checks.** An outcome is either:
  - **Fixed:** one result, with its amounts rolled as today;
  - **Check (stat, difficulty):** a chance from the difficulty (**Easy / Even / Hard**) plus the stat, which picks the **Success** or the **Failure** branch. Each branch has its own text and actions.
  - The base chances, the chance per stat point and the cap are tunables for a settings asset; they aren't decided here.
- **Check stats:**
  - **Key stat** is the class's Key stat from its stat guide (Warrior: Melee Damage; Archer: Ranged Damage; Augur: Magic Damage; check `CharacterClassData.statPriorities`).
  - **Grace** means the player's **current Grace**, which can be negative. It isn't a `StatType`, so the check reads `PlayerGrace`.
  - Any other stat is named directly (Armor, Move Speed, Mastow Affinity, Grace Affinity, Max Health, Luck).
- **Class content:**
  - **Class lines** are a short paragraph shown only for the chosen pilgrim: appended to the intro, or to a result.
  - **Class options** are offered only to one pilgrim. They're marked _[Ayteel]_, _[Faelin]_ or _[Werlem]_.
  - The two pilgrims not chosen walk along and may speak in a class line.
- **Voice:** "you" is the party as a whole. The pilgrims are always named in the third person, including the chosen one, so one text works for all three classes.
- **Option count:** every event has 2–3 options for everyone, plus at most one class option for the chosen pilgrim.
- **Follow-ups:** an option or a branch can lead to a **Step**, a new prompt with its own options, at most 3 steps deep. A wrong answer always costs something.
- **Costs:** every option risks a cost and offers a gain. Pure gains have to be **earned by reading the lore** (owner's rule, 2026-10-26):
  - A pure-gain option appears only in a **follow-up step**, reached by a **lore-correct answer** in the step before; mostly in class arc steps.
  - A first-step option always risks a cost.
  - The answer must be findable in the game: an event text, an item description, a class line.
  - The only exception is the Wayside Shrine's "Kneel and pray", because the Shrine is a rest node for Grace.
- **Result text:**
  - **Placeholders:**
    - `{health}` is the health lost after armor, or the health healed;
    - `{grace}`, `{chronos}` and `{favors}` are the absolute amounts;
    - `{item}` is the item's name, colored by quality as in the Inventory.
  - **Optional sentences:** a sentence in `[square brackets]` shows only if its action gave something. An item can fail to drop when nothing is eligible.
  - **Results window:** the narrated text, then a compact summary line in the usual colors, so the numbers are still easy to read (e.g. "−8 health · +3 Grace").
- **Amounts** are sizes, not numbers. The defaults below follow the current assets, and the balance belongs to the designer:

| Size | Health lost | Health healed | Golden Chronos gained | Golden Chronos lost | Grace gained | Grace lost | Favors |
|---|---|---|---|---|---|---|---|
| **S** | 5–10 | 10–15 | 20–40 | 15–30 | 1–3 | 2–5 | +1 |
| **M** | 10–25 | 20–30 | 40–60 | 25–40 | 3–5 | 3–6 | – |
| **L** | 20–40 | – | 50–100 | 30–50 | 8–12 | 5–8 | – |

**Item** is one random Event item from the drop table (`GiveRandomItemOptionAction`, Luck-biased as today).

- **Pool:**
  - **Act I's random pool** (`EventEncounter.prefab`'s `encountersList`) holds 9 events: the Vega Casket, Raiders from Gracoy, the Weeping Chest, the Faraclarian Clan, Gift of the House of Stars, Laughter in the Leaves, the Star Reader, the Last Torch, and a Hut in the Jesterwoods.
  - **Repeats:** no event appears twice in one Pilgrimage, and a Hut in the Jesterwoods (Liah) appears at most once.
  - **Shrine:** the Wayside Shrine keeps its own node and scene.

---

#### 1. The Vega Casket (`arcaneBox`, reworked)
**Intro:** Half-buried in the moss lies a casket of elven make, its seams humming with a cold blue light. Elvish letters run along the lid: *Property of the Vega Unit.* Someone has tried to pry it open before you. The scratches end in a small star, cut deep into the wood.
- **Class lines:**
  - **Ayteel:** Ayteel keeps a hand on her sword. "Elves don't lose things. They leave them."
  - **Faelin:** Faelin kneels by the scratches. "Whoever did this wanted it badly, and stopped in a hurry."
  - **Werlem:** Werlem's breath catches. The same maker's mark is cut into the haft of his scepter: *Akeros the Observer.*

**Options**
- **Pull the silver lever.** *The casket hums louder as you reach for the lever. Elven work rarely forgives clumsy hands.*
  - Check: **Mastow Affinity, Even**.
  - **Success:** Item. "The lid slides open on a bed of blue light. Inside are vials of Crisalia, pages of notes on human Grace, and {item}."
  - **Failure:** Grace lost M. "The lever bites back. Crisalia sprays across your hands, and your Grace gutters like a candle in the wind: {grace} Grace. When the light fades, the casket is empty."
- **Smash it open.** *The wood is old; the light inside is not. Whatever guards it will answer force with force.*
  - Check: **Armor, Even**.
  - **Success:** Golden Chronos gained M. "The casket splits. A ward bursts against your armor and dies. Under the splinters lies an elf's travelling purse: {chronos} Golden Chronos."
  - **Failure:** Golden Chronos gained S, Health lost M. "The ward bursts in your face: blue fire, then silence. You find {chronos} Golden Chronos among the splinters, and the burns cost you {health} health."
- _[Werlem]_ **Read the maker's runes.** *Akeros wrote the way scholars do: for whoever could understand.*
  - Fixed: Golden Chronos lost S, Item, Grace gained S.
  - Result: "The lock needs Crisalia to wake, and you spend {chronos} Golden Chronos' worth of it. The runes answer to a name: Kyro Feth. Inside are Akeros's notes on human Grace, a letter to Kyro that was never sent, [and {item}.] The ink on the letter has run, as if someone wept on it. +{grace} Grace."
- **Lore:** the Vega Unit, and Akeros the Observer (the maker of Werlem's scepter), who studied human Grace and knew Kyro Feth. The House of Stars wants what the Vega Unit has (the star scratched on the lid). Crisalia and Mastow.

#### 2. Raiders from Gracoy (`bandits`, reworked)
**Intro:** Riders on faikans step out of the treeline: Gracoy mercenaries, from a company whose grandmothers sold their swords to the Sea Witch and never went home. Their captain, a broad woman with a notched blade, smiles without warmth. "The High Mother is dead, and her daughters are hungry. Your purse, pilgrim, or your blood."
- **Class lines:**
  - **Ayteel:** The captain squints at Ayteel's sword. "Steeldawn. My grandmother said a man with a blade like that cut through half her company."
  - **Faelin:** Faelin counts them before anyone moves. "Seven. Two bows. The big one favors his left leg."
  - **Werlem:** Werlem raises a hand in blessing. The captain spits. "Keep your Malien. We pray to the Mother of All."

**Options**
- **Fight them.** *Seven of them, thin and tired. Hungry people fight hard, but not for long.*
  - Check: **Key stat, Even**.
  - **Success:** Health lost S, Item. "They break at the first real blow and scatter into the trees. You lose only {health} health, [and a saddlebag left behind holds {item}.]"
  - **Failure:** Health lost L. "There were more in the trees than you counted. You drive them off, bloodied, with nothing to show for it but {health} health lost."
- **Pay them.** *Mercenaries remember who pays. Gold might buy more than the road.*
  - Fixed: Golden Chronos lost M, Grace gained S.
  - Result: "The captain weighs your {chronos} Golden Chronos in her palm. 'Ashemdrek coin. My grandmother was paid in this.' They let you pass. No blood spilled today: +{grace} Grace."
- **Run for the trees.** *Faikans are fast on open ground. The Jesterwoods are not open ground.*
  - Check: **Move Speed, Even**.
  - **Success:** Golden Chronos gained S. "You lose them among the roots, and stumble into their empty camp. Someone left {chronos} Golden Chronos in a cooking pot."
  - **Failure:** Health lost S, Golden Chronos lost S. "A faikan catches your cloak. You tear free, leaving {health} health and {chronos} Golden Chronos behind."
- _[Werlem]_ **Offer to heal their wounded.** *Two of them can barely stand. Augurs heal. Augurs also charge.*
  - Fixed: Grace lost S, Golden Chronos gained M.
  - Result: "You kneel by the wounded and let your Grace flow into them: {grace} Grace. The captain pays like a mercenary, exactly: {chronos} Golden Chronos. 'A village augur would have charged double,' she says. It's almost a compliment."
- **Lore:** Gracoy, the Mercenary Kingdom: its matriarchy (the High Mother), its faikans and its gods (Gyra, "Mother of All"). Its companies fought for Malizante. Golden Chronos is Ashemdrek's coin. The village augur joke.

#### 3. The Weeping Chest (`cursedChest`, reworked; multi-step)
**Intro:** An iron-bound chest sits in a ring of burnt-out candles: a Forsaken tithe, left for whatever crawls up from below. When you lift the lid, black ichor seeps between the gold, and a voice like wet leaves whispers the oldest lie there is. *Malien left you.* Something glints at the very bottom.
- **Class lines:**
  - **Ayteel:** The voice finds the name it wants. "Alamoh. They will never forgive you. Why serve them?"
  - **Faelin:** Faelin's jaw tightens. "This is how they take you. Not with claws. With doubt."
  - **Werlem:** Werlem knows this from his novice years: a tithe chest can be sealed, if someone has the Grace to spare.

**Options**
- **Take the gold.** *Gold is gold, the traders say. The voice agrees with them a little too warmly.*
  - Fixed: Golden Chronos gained L, Grace lost M.
  - Result: "You scoop out {chronos} Golden Chronos. The ichor clings to your fingers, and the voice laughs as {grace} Grace drains out of you. It got what it wanted, too."
- **Reach for what lies beneath.** *Deeper means longer in the whisper's reach. A steady soul might bear it.*
  - Check: **Grace, Even**.
  - **Success:** Item, Grace lost S. "You shut your ears to the voice and reach down through the cold. Your fingers close on {item}. The whisper costs you only {grace} Grace."
  - **Failure:** go to **Step: The Whisper**.
  - **Step: The Whisper.** "The voice is inside now, gentle as a mother. *Say it. Say He left you, and it all stops.*"
    - **Say the words.** *It would be so easy. It would stop the cold.*
      - Fixed: Item, Golden Chronos gained M, Grace lost L.
      - Result: "You say it. The cold stops at once. The voice gives you everything at the bottom: {item} and {chronos} Golden Chronos. Nothing stops you, and that is the worst part. {grace} Grace, gone."
    - **Pray the Opening Prayer.** *Ashemdrek's oldest prayer. Without it, they say, Malien doesn't listen.*
      - Fixed: Health lost S, Grace gained S.
      - Result: "*Malien, who fills this world with your goodness…* The voice shrieks. The lid slams on your hand ({health} health), and the ichor boils away. You walk off empty-handed, with {grace} Grace that is truly yours."
- **Seal it with a prayer.** *A tithe chest can be sealed. It takes something of yours to do it.*
  - Fixed: Health lost S, Grace gained M.
  - Result: "You press your palm to the lid and pray until the ichor hisses. It burns ({health} health), but when you lift your hand the chest is cold and silent, and you are stronger for it: +{grace} Grace."
- **Lore:** the Forsaken and their tithes. The Drekos corrupt people by breaking their faith ("Malien left you"); Grace protects you only while you hold on to it. The Opening Prayer.

#### 4. The Faraclarian Clan (`faraclarianClan`, reworked; multi-step)
**Intro:** Warriors with ash-painted faces surround you, spears lowered: a Faraclarian clan, a long way south of their frozen home. Their Golden Spear steps forward. "Our land is dark and our Forger is dead. We seek a new one: one who walks between fire and iron. Have you seen such a soul?" Every spear waits for your answer.
- **Class lines:**
  - **Ayteel:** Ayteel's hand drifts to her sword. The Golden Spear's eyes follow it.
  - **Faelin:** "We're pilgrims of the Flame," Faelin says, the way Sentinels are taught. Her bow, Faraclarian-made, earns a long look.
  - **Werlem:** Werlem bows the northern way, fist to heart. "Omiara." Two of the spears lift a little.

**Options**
- **Say you've seen the Forger.** *They want to believe you. Faraclarians prize honor above life, and despise a liar above all.*
  - Go to **Step: Describe the Forger**.
  - **Step: Describe the Forger.** "'Then tell us,' says the Golden Spear. 'What did the Forger look like?'"
    - **"A soul that burns. Eyes like embers, forge-heat in the hands."** *You remember the old tales, or you think you do.*
      - Fixed: Golden Chronos gained M, Grace lost S.
      - Result: "The Golden Spear lowers his spear. 'Yes. That is a Forger.' The clan presses {chronos} Golden Chronos on you for the news. The lie sits heavy all the same: {grace} Grace."
    - **"A giant of a man, with a hammer of black iron."** *Every smith in every tale looks like that.*
      - Check: **Key stat, Even**.
      - Intro line: "'A smith,' the Golden Spear says flatly. 'You've seen a smith.' The spears come up."
      - **Success:** Health lost M, Item. "You fight your way out, {health} health poorer. One of them drops {item} in the snowmelt. They'll sing about you, and not kindly."
      - **Failure:** Health lost L. "They beat you the way the north beats liars: thoroughly. {health} health."
- **Tell them the truth: you've never heard of the Forger.** *Honor costs nothing but the hope you might have given them.*
  - Fixed: Golden Chronos lost S, Grace gained S.
  - Result: "The Golden Spear studies you a long time. 'Honest. Rare, south of the snow.' He asks the road toll their custom demands, {chronos} Golden Chronos, and gives you Omiara's blessing in return: +{grace} Grace."
- **Claim to be the Forger.** *A bold lie. Forgers are tested in fire.*
  - Check: **Max Health, Hard**.
  - **Success:** Health lost M, Item. "They hold a coal to your palm, as their custom demands. You don't flinch, for long enough. 'Not a Forger,' the Golden Spear decides, 'but not nothing.' They give you {item} for your nerve. Your hand: {health} health."
  - **Failure:** Health lost M, Golden Chronos lost S. "You flinch. The clan laughs, then fines the liar as their law demands: {chronos} Golden Chronos, and the burn, {health} health."
- _[Ayteel]_ **Show them Gabien's sword.** *The north knows its steel. It will know who forged this one.*
  - Go to **Step: The Steel**.
  - **Step: The Steel.** "The Golden Spear takes the blade in both hands, and the whole clan goes quiet. 'Ashem Elwyn. The Shining Tide. Gabien's work, the southern Alamoh.' He looks at you again, harder. 'Rondo's eldest stayed in the snow, and his line still rides with the clans. If you are kin, tell me: what do we call this steel?'"
    - **"Malien's Steel."** *What the Faraclarians who made Faelin's bow call it.* (Lore-correct: the Steeldawn Bow's description says so; see `Lore.md` §8.)
      - Go to **Step: Kin**.
    - **"Steeldawn."** *Every smith in the south knows the word.*
      - Fixed: Golden Chronos lost S.
      - Result: "'The southern word.' He hands the blade back, a little cooler. 'Kin, then, but a stranger to our ways.' The clan takes the road toll from a stranger: {chronos} Golden Chronos."
    - **"Gabien's steel."** *It was his, after all.*
      - Fixed: Golden Chronos lost S, Grace lost S.
      - Result: "'No man owns Malien's gifts,' the Golden Spear says sharply. Honor demands a toll for the insult: {chronos} Golden Chronos, and a cold silence that costs you {grace} Grace."
  - **Step: Kin.** "He smiles for the first time. 'Kin.' Then, softer: 'Do you burn, cousin?'" (Both answers are pure gains, earned by the answer before.)
    - **"Yes. Sometimes I feel it."** *You've never said it aloud.*
      - Fixed: Grace gained M, Favors +1.
      - Result: "'Then you are what we seek, or will be.' They kneel, not to you but to what you carry. Before they go, they swear to keep the north's Altar of Malien for you until you come. +{grace} Grace, +{favors} Favor."
    - **"No. I'm only his granddaughter."** *Only.*
      - Fixed: Item.
      - Result: "'Only.' The Golden Spear smiles for the first time. 'Gabien said the same at your age, my father told me.' He gives you a gift of the north for the road: {item}."
- **Lore:**
  - The Faraclarians: their Golden Spears, Omiara (Grace), honor, and Malien's Steel.
  - A Forger is a Seared Soul, about one each generation, and theirs is dead.
  - The Alamohs are Faraclarian by blood (Ayteel).
  - Ayteel may be a Seared Soul; it stays open.
  - The north's altar sets up Act II.

#### 5. Gift of the House of Stars (`houseOfStars`, reworked)
**Intro:** Elves in cloaks that shimmer like the night sky bar the road: the House of Stars, whom every other elven house distrusts. Their lady holds out a small box wrapped in silver thread. "A gift, traveller. From the stars to you." She studies you the way a jeweller studies a stone. Her smile doesn't reach her eyes.
- **Class lines:**
  - **Ayteel:** Her gaze rests on Ayteel longer than on the others. "Alamoh," she says, as if tasting it. "How very interesting."
  - **Faelin:** Faelin doesn't lower her bow. "Sentinels learn one thing about elves: when they give, ask what it costs."
  - **Werlem:** Werlem's scepter hums. The lady glances at the Vega mark on its haft, and her smile thins.

**Options**
- **Accept the gift.** *Star relics are said to be wondrous. They are also said to take something back.*
  - Fixed: Item, Health lost L.
  - Result: "Inside the box lies a relic of black glass and starlight: {item}. When you touch it, something leaves you. A few years, maybe, or a memory you won't miss until you need it. {health} health. The lady seems satisfied, and that worries you more than the cost."
- **Refuse politely.** *The House of Stars is not used to hearing no. Courtesy might soften it.*
  - Check: **Luck, Even**.
  - **Success:** Grace gained S, Golden Chronos lost S. "'Another time,' the lady says. As they pass, one of them brushes against you, and your purse is lighter: {chronos} Golden Chronos. Still, your soul is your own: +{grace} Grace."
  - **Failure:** Golden Chronos lost M, Grace lost S. "'No one refuses the stars,' she says pleasantly. Her people take a toll for the insult, {chronos} Golden Chronos, and something colder slips into your mind and out again: {grace} Grace."
- **Draw your weapon.** *They are few, and elves bleed like anyone. But they came here for a reason, and the reason might be you.*
  - Check: **Key stat, Hard**.
  - **Success:** Health lost M, Item. "Steel against starlight. When it's over they're simply gone, the way no elf should be able to vanish. The box lies in the grass. Inside: {item}. It cost you {health} health."
  - **Failure:** Health lost L. "They don't fight fair. You never see the blow that lands. When you wake, they're gone, and so is {health} health."
- _[Faelin]_ **Ask what the gift costs.** *Elves rarely lie outright. They let you assume.*
  - Fixed: Golden Chronos lost S, Grace gained S.
  - Result: "'Answers have a price too, Sentinel.' The lady takes {chronos} Golden Chronos before she answers. 'Years,' she admits. 'Only a few.' You decline the gift. Faelin doesn't take her eyes off them until they're gone. +{grace} Grace."
- _[Werlem]_ **Show them the Vega mark.** *The House of Stars and the Vega Unit have been rivals for ages. Elves remember debts.*
  - Go to **Step: The Observer**.
  - **Step: The Observer.** "The lady's smile disappears. 'Akeros's toy. And what does the Observer study these days, little augur?'"
    - **"Human Grace."** *What the Vega Casket's notes were about.* (Lore-correct: Event 1 and `Lore.md` §3.)
      - Go to **Step: Pride**.
    - **"The stars, like you."** *Elves of every house watch the sky.*
      - Fixed: Grace lost S.
      - Result: "'Then he's wasting his time,' she says, and something cold brushes your mind as they pass: {grace} Grace."
    - **"Whatever you're afraid of."** *A guess, and a bold one.*
      - Check: **Luck, Hard**.
      - **Success:** Golden Chronos lost S. "She laughs, genuinely, and charges you for the entertainment: {chronos} Golden Chronos."
      - **Failure:** Health lost M. "She doesn't laugh. You never see what hits you: {health} health."
  - **Step: Pride.** "'Grace,' she repeats, as if it were a dirty word. 'Tell the Observer we are not finished.' They leave the box behind, out of pride, not kindness." (Both options are pure gains, earned by the answer before.)
    - **Open the box.** *Whatever's inside, it was meant to cost someone.*
      - Fixed: Item.
      - Result: "Unclaimed, the relic asks nothing of you. It's simply {item}. You'll wonder, later, what it would have cost."
    - **Leave it closed, for the Vega Unit's honor.** *Akeros would want to know what they were carrying.*
      - Fixed: Grace gained M.
      - Result: "You wrap the box back in its silver thread, unopened, to bring to Akeros one day. Turning down a star feels like Grace: +{grace} Grace."
- **Lore:**
  - The House of Stars: distrusted, and a rival of the Vega Unit. Its relics take a price (the Star Relics; their making is secret, so this is only a hint).
  - Its interest in the Alamoh line (it hunts Seared Souls; secret, a hint only).

#### 6. Wayside Shrine of Malien (`holyAltar`, reworked; was "Forgotten Altar of Malien")
**Intro:** Beneath a ruined arch stands a wayside shrine to Malien, built by Pilgrimages long gone. The Sandir on its face, eight points and a tear, is worn almost smooth. No Keeper has tended it in sixty years, yet the stone is warm, and a faint light lingers over the offerings of the last pilgrims who passed.
- **Class lines:**
  - **Ayteel:** Among the names scratched into the stone, one stops Ayteel cold: GABIEN. Beside it, in a careful hand: DAYLA.
  - **Faelin:** Faelin finds her grandfather's mark, cut deep and square, the way he did everything: O. VIDAL, GRAND CRUSADER.
  - **Werlem:** Werlem finds a row of tiny runes no priest would carve, and under them a signature: K. FETH. Even here, his grandfather was taking notes.

**Options**
- **Kneel and pray.** *Old shrines answer old prayers. The warmth is faint, but it's there.*
  - Check: **Grace Affinity, Easy**.
  - **Success:** Grace gained M. "The warmth rises through your knees and settles behind your ribs. +{grace} Grace."
  - **Failure:** Grace gained S. "The shrine is tired, and so are you. Still, something answers: +{grace} Grace."
  - Pure gain, as the old "Kneel and pray" was. The shrine is the act's Grace node.
- **Leave an offering of Golden Chronos.** *The old Keepers said the Flame remembers generosity.*
  - Fixed: Golden Chronos lost M, Grace gained L.
  - Result: "You lay {chronos} Golden Chronos among the old offerings. For a moment the shrine burns white, the way the Flames of Faith once did, and the light goes into you: +{grace} Grace."
- **Take the old offerings.** *No one has come for them in sixty years. No one but Malien, anyway.*
  - Fixed: Item, Grace lost M.
  - Result: "You take what the last pilgrims left: {item}. The warmth goes out of the stone as you do, and out of you: {grace} Grace."
- **Lore:**
  - The wayside shrines and the Keepers of the Flame. The Sandir, Ashemdrek's symbol of Malien.
  - Gabien's company walked this road. Each grandparent's mark is here, and Kyro's runes seed Werlem's arc.

#### 7. A Hut in the Jesterwoods (`liah`, new; once per Pilgrimage; multi-step)
**Intro:** Smoke rises from a hut at the forest's edge, within sound of the sea. An old woman sits mending nets with hands that shake. She doesn't look at your faces. She looks at the sword at Ayteel's hip, and her hands go still. "Steeldawn," she says. "I haven't seen that light in sixty years."
- **Class lines:**
  - **Ayteel:** The sword feels warm against Ayteel's side, as if it knows the voice.
  - **Faelin:** Faelin glances at Ayteel. Ayteel hasn't moved.
  - **Werlem:** Werlem feels her Grace before he understands it: old and patched, a soul mended once by a hand far stronger than hers.

**Options**
- **Tell her whose sword it is.** For Ayteel, the label reads **"Tell her who you are."** *She already knows. She wants to hear it said.*
  - Go to **Step: The Name**.
  - **Step: The Name.** "'Alamoh,' she whispers. 'Then you've come for the truth, or to spit on it like the rest.'"
    - **"The truth."** *Sixty years she's waited for someone to ask.*
      - Fixed: Golden Chronos lost S, Grace gained M.
      - Result: "'I am Liah. His sister. When I was nine a Devourer ate my soul, and for six years I was nothing. A doll they fed and washed. Then my brother came home with a broken Seal in his hands and gave me back.' Her voice hardens. 'They call it a sin. I call it my life.' Before you go, you fill her pantry ({chronos} Golden Chronos). +{grace} Grace."
      - Class lines (in the result):
        - **Ayteel:** "'He broke it for you,' Ayteel says. Liah looks at her a long time. 'Not only for me, child.'"
        - **Faelin:** "Liah sees the Vidal crest on Faelin's quiver. 'The Grand Crusader told them the truth. Good. Gabien would have hated a lie told for him.'"
        - **Werlem:** "'Kyro cried at the altar,' she tells Werlem. 'He was the only one who didn't pray. He was counting.'"
    - **"To clear his name."** *Names matter a great deal to the living.*
      - Fixed: Item, Golden Chronos lost S.
      - Result: "'Names.' She laughs, and it turns into a cough. 'He never cared about his.' She presses something into your hand: {item}. 'He left this with me. He'd want it walking again.' You leave her what you can spare for the winter: {chronos} Golden Chronos."
- **Buy smoked fish and move on.** *She sells what the sea gives. Pilgrims have to eat.*
  - Fixed: Golden Chronos lost S, Health healed M.
  - Result: "She sells you smoked fish for {chronos} Golden Chronos and watches you go. You eat well for the first time in days: +{health} health. Behind you, very quietly, she says a name."
- **Ask about the Devourers.** *She speaks of them the way survivors do: low, as if they might hear.*
  - Check: **Grace Affinity, Even**.
  - **Success:** Grace gained S. "'They hunt by the soul's light,' she says, and teaches you the old way of hiding your Grace, the way children did in the Dark Age. You learn quickly. +{grace} Grace."
  - **Failure:** Health lost S, Grace gained S. "She makes you practice hiding your Grace until your head pounds ({health} health). It's worth it: +{grace} Grace."
- **Lore:**
  - Liah is alive. A Devourer took her soul, and Gabien broke the Seal to give it back.
  - "Not only for me" is the hint toward Dayla, Ayteel's turning point; the rest comes in later acts.
  - Faelin's arc (Orleone told the truth) and Werlem's (Kyro "was counting") are seeded here.
  - Humans learned to hide their Grace in the Dark Age.

#### 8. Laughter in the Leaves (`aylinean`, new; multi-step)
**Intro:** A fox sits on a stump in the middle of the path. Then it stands up on two legs, dusts off its paws and grins with far too many teeth. An Aylinean, straight out of a nursery tale. "Pilgrims! It's been ages. Make me laugh and I'll tell you a secret. Bore me…" It shrugs. "And I'll make *myself* laugh."
- **Class lines:**
  - **Ayteel:** "Ooh, that sword. I knew the last one who carried it. Terrible at jokes."
  - **Faelin:** Faelin has an arrow nocked before she knows it. The Aylinean is suddenly behind her. "Rude."
  - **Werlem:** "They're pure ley energy," Werlem whispers. "My grandfather would have given an arm to—" "Arms are not funny," says the Aylinean.

**Options**
- **Tell it a joke.** *Aylineans love human foolishness. Their own dignity, much less.*
  - Go to **Step: The Joke**.
  - **Step: The Joke.** "It sits back down, tail curled, waiting."
    - **"How do you know a village augur has healed you? You still need him tomorrow."** *Every pilgrim has met that augur.*
      - Lore-correct: the village augur is a known joke; Raiders from Gracoy uses it too.
      - Go to **Step: The Secret**.
      - **Step: The Secret.** "It falls off the stump laughing. When it climbs back up, it keeps its word. 'The white fires? We lit them, with the Golden Rider and the old prophet, and we kept a seal for them. A boy came for it sixty years ago, from Papik Lotio's tomb.' It holds out a closed paw. 'And for the laugh, a gift. Or another secret. Choose.'" (Both options are pure gains, earned by the answer before.)
        - **Take the gift.** *Aylineans "find" things. Best not to ask where.*
          - Fixed: Item.
          - Result: "It opens its paw: {item}. 'Found it,' it says, and doesn't elaborate."
        - **Ask about the boy.** *Sixty years is nothing to an Aylinean.*
          - Fixed: Grace gained M.
          - Result: "'Sad eyes. He burned, that one, like the old Riders. He said the seal was for someone he loved, not for a war.' It looks at you a moment too long. 'You have his eyes.' +{grace} Grace."
    - **"What do you call a Dreko at a feast? The main course."** *Everyone hates the Drekos. Everyone.*
      - Fixed: Golden Chronos lost M.
      - Result: "The grin goes away. 'We buried a lot of friends because of those.' When it's gone, so is {chronos} Golden Chronos from your purse."
    - **"Why did the fox cross the road? To get to the other fox."** *A classic.*
      - Fixed: Health lost S, Golden Chronos lost S.
      - Result: "'Fox jokes. To a fox.' It leads you, very politely, the wrong way through the deepest part of the woods. You get out hours later, {health} health and {chronos} Golden Chronos poorer, and you're not sure how the coins went missing."
- **Laugh at it first.** *Nobody has ever laughed at an Aylinean. That might be very funny. To it.*
  - Check: **Luck, Hard**.
  - **Success:** Item. "It stares at you. Then it laughs so hard it vanishes, and reappears, and vanishes again. It leaves {item} on the stump as a tip."
  - **Failure:** Golden Chronos lost M. "It doesn't laugh. It just keeps grinning, and you blink, and your purse is {chronos} Golden Chronos lighter."
- **Bargain: a coin for a secret.** *Aylineans keep their word. They also love a bargain.*
  - Fixed: Golden Chronos lost S, Grace gained S.
  - Result: "'A coin for a secret? How very Wanderer of you.' It takes {chronos} Golden Chronos and tells you something small and true: where the old shrine roads run, and which of them still hold a little warmth. +{grace} Grace."
- **Lore:**
  - The Aylineans guard the forests (laugh, or be pranked).
  - They lit the Flames of Faith with Alam and Weinsol, and kept the Seal. Gabien came for it from Papik Lotio's tomb.
  - The village augur joke.
- _(Aylineans play with luck, so this event's checks use Luck on purpose.)_

#### 9. The Star Reader (`starReader`, new)
**Intro:** A Wanderer caravan has camped by the road: three wagons and a cookfire. Their Star Reader, a woman older than her braids, beckons you over. "Sit, pilgrim. The stars have been talking about you." Wanderers live by Good-for-Good: what you take, you repay. She hasn't said yet what she'll take.

**Options**
- **Have your fortune read.** *Most call her readings charlatanry. Most have never had one.*
  - Fixed: Golden Chronos lost S, Grace gained S.
  - Result: "She takes {chronos} Golden Chronos, turns your palm to the sky and reads it by starlight. +{grace} Grace." Then the class line.
  - Class lines (in the result):
    - **Ayteel:** "'You carry a dead man's sword, and a broken thing you hide even from your friends. The sword will cut. The broken thing…' She frowns. 'The broken thing isn't finished.'"
    - **Faelin:** "'Your grandfather kept a promise that cost him everything. One day you'll choose between a promise and the truth. Choose both, if you can.'"
    - **Werlem:** "'You're walking away from your family's road and toward it at the same time. The stars find that very funny.'"
- **Make a wish on the Eye of Malien.** *The Eye of Malien is in the sky tonight. Wishes are free. Wanderers say nothing is.*
  - Check: **Luck, Even**.
  - **Success:** Grace gained M. "The Eye of Malien flares as you wish. 'Judged worthy,' the Star Reader murmurs. 'Don't waste it.' +{grace} Grace."
  - **Failure:** Grace lost S. "The Eye winks out behind a cloud. 'You asked for something you hadn't earned,' she says, not unkindly. {grace} Grace."
- **Help them mend a wagon.** *A broken axle and a long road. Good-for-Good: strong arms are repaid.*
  - Check: **Max Health, Even**.
  - **Success:** Health lost S, Item. "You lift while they hammer. It's done by moonrise ({health} health). Good-for-Good: they repay you in kind with {item}."
  - **Failure:** Health lost M, Golden Chronos gained S. "The axle slips and takes some skin with it ({health} health). They finish it together, and pay what they can: {chronos} Golden Chronos."
- **Lore:**
  - The Wanderers, Good-for-Good, and the Eye of Malien.
  - Each pilgrim's arc gets a hint: the Seal "isn't finished" (Ayteel); a promise against the truth (Faelin); the family road (Werlem).

#### 10. The Last Torch (`lastTorch`, new)
**Intro:** Three old soldiers sit around a torch with a weak white flame: the last ember of a Flame of Faith, carried out of Ashemdrek City sixty years ago. Their Torch Guard tabards have faded to grey. "Every night it's smaller," says the eldest. "When it goes out, the Devourers come. You're pilgrims. Can you feed it?"
- **Class lines:**
  - **Ayteel:** The eldest sees her sword and spits. "Alamoh steel. Your grandfather had the whole city in his hand and dropped it." Then, quieter: "Still. He was the only one who went in."
  - **Faelin:** "A Sentinel," says another. "Abidor's archers stood at the border and watched us burn." Faelin doesn't argue.
  - **Werlem:** Werlem sees at once what the flame needs: Grace, and a lot of it. Whoever feeds it will feel the cost.

**Options**
- **Feed the flame with your Grace.** *The flame is starving. It will take what it needs, and maybe a little more.*
  - Check: **Grace, Even**.
  - **Success:** Grace lost S, Favors +1. "The torch roars white, and the old soldiers weep openly. 'Tell the Church,' the eldest says. 'Tell them the Torch Guard held.' It cost you {grace} Grace, and the Shardless Church will hear of it: +{favors} Favor."
  - **Failure:** Grace lost L, Favors +1. "It drinks far more than you meant to give ({grace} Grace), but it catches. 'Tell them the Torch Guard held,' the eldest says. +{favors} Favor."
- **Buy the torch.** *They'd never sell it. They'd also never admit how hungry they are.*
  - Fixed: Golden Chronos lost L, Grace gained M.
  - Result: "They take {chronos} Golden Chronos without meeting your eyes and press the torch into your hands. Its warmth fills you: +{grace} Grace. Behind you, they turn toward the coast and the boats to Abidor."
- **Escort them to the Argent Coast.** *The coast has boats, and the woods have Devourers. It's out of your way.*
  - Check: **Key stat, Even**.
  - **Success:** Health lost S, Item. "Something pale follows you through the trees and flees from the torch. At the shore, the eldest gives you his old Torch Guard gear: {item}. It cost you {health} health."
  - **Failure:** Health lost M, Item. "A Devourer comes out of the dark between the trees. You drive it off ({health} health). At the shore, the eldest presses his old gear on you: {item}."
- _[Faelin]_ **Write them a Sentinel's pass to Abidor.** *A Sentinel's seal opens Abidor's border. It isn't hers to give.*
  - Fixed: Grace gained M, Golden Chronos lost S.
  - Result: "Faelin signs her own name under the Sentinel seal. If anyone asks, it's on her. The boat fare comes out of your purse: {chronos} Golden Chronos. The eldest salutes her with a hand that shakes. 'Abidor's archers,' he says, and this time it isn't an insult. +{grace} Grace."
- **Lore:**
  - The Torch Guard carried the Flame of Faith on torches. Ashemdrek fell, and Abidor watched from its border.
  - The Devourers fear the white fire.
  - Favors are the Church's recognition.
  - Gabien was "the only one who went in".

#### In-game texts to fix alongside
Item and node texts that contradict the lore are listed in `Lore.md` §8 (Sentinel quiver, Cursed Coin's Awoken, the Rising Darkness, Ring of Vitality, level-name typos, the Wayside Shrine, and the **Plerian Shop**, formerly the Elven Shop). Fix them in the same change.

### Systems (for the implementation session, after the narrative)
- Current data: `EventEncounterData` (title, text, options, optional art) → `EventOptionsList` (label + actions) → `OptionActions/` (`Change{Currency,Grace,Health,Favors}Option`, `GiveRandomItemOptionAction`), rolls through `StatBiasedRoll` with `biasStat`.
- Likely needs: a hint text per option, result text templates per outcome, a stat per roll (not only Luck), an optional next step per option, a class filter per option or line.
- Player-facing words follow the glossary (Pilgrimage, Golden Chronos).
- If event numbers become a balance table, follow `CLAUDE.md` "Keep the Google Sheets pipeline in step".

## Open questions
_Questions 1–5 were answered by the owner on 2026-10-26; the answers are in "Narrative", under "How the content is written"._
1. ~~Multi-step depth~~ Up to 3 steps; a wrong answer always costs something.
2. ~~Class-specific~~ Class lines in most events, plus a few class-only options.
3. ~~Stats~~ Stat checks: Easy / Even / Hard plus a stat (Key stat, Armor, Move Speed, Mastow Affinity, Grace Affinity, Max Health, current Grace, Luck). The strength per point is a tunable for the implementation.
4. ~~Numbers~~ None before choosing; the result narrates them, and a compact summary line follows.
5. ~~Event count~~ The 5 events and the Shrine reworked, plus 4 new ones: Liah, the Aylinean, the Star Reader, the Last Torch.
7. ~~Pure-gain options~~ Earned only through a lore-correct answer, in a follow-up step; the Shrine is the exception (owner, 2026-10-26).
8. ~~Faun Chief's blood~~ The Faun Chief is **not** of Alamoh blood; Rondo's eldest son's line lives among the clans (owner, 2026-10-29).
9. ~~Gabien's sword~~ **Ashem Elwyn**, "Shining Tide" (owner, 2026-10-29). The Faraclarian event names it.
10. ~~Aylinean checks~~ Laughter in the Leaves stays Luck-only, as written (owner, 2026-10-29).
11. **Open:** how Forsaken Daviro the Unholy (Act III boss) has lived sixty years past Gabien's time. It doesn't affect Act I.
6. ~~Lore contradictions to settle first~~ **Settled with the owner on 2026-10-26**, see "Lore decisions" below. The story bible is `game-design-docs/Lore.md`.

### Lore decisions (narrative session, 2026-10-26)
The owner's novels (*Aceros del Alba*, "Steeldawn") and notes in `lore-docs/` (gitignored, Spanish, reference only) are the canon. The game follows them, with the changes below.
1. **Timeline.**
   - **The Dark Age:** the Drekos first invaded in the Dark Age, thousands of years ago. They were driven underground and became a children's tale.
   - **Gabien's time:** in year 350 of the Decadent Age, Malizante, leader of the Drekos, woke and began putting out the Altars of Malien. She invaded Ashemdrek with Gracoy mercenaries and the Forsaken. Gabien Alamoh fought her (see 5).
   - **The game:** about **60 years later** (about year 410). The **Years of the Miasma** are those 60 years.
   - **The GDD's "year 1000"** and Taio'Sandir's "6,000 years" become "thousands of years".
   - **`lore-docs/Timeline.docx`** puts the game 150 years after Gabien; the owner chose 60.
2. **Churches:** one church, two names.
   - **The White Church** of Ashemdrek is about 2,000 years old. It started the Pilgrimage of the Flame, and its Keepers of the Flame tended the altars.
   - **The shards:** it split into factions (military, civic, commerce...) that people called "shards", and they were too slow to answer the invasion.
   - **In exile in Abidor** it renamed itself the **Shardless Church**, to mean "never divided again". Its augurs start this Pilgrimage, and Werlem is one of them.
3. **Places.**
   - **Kanator** is the world.
   - **Abidor** is a kingdom of the **Glenos Empire**, on the continent of **Rousatan** across the Fauner Sea; its capital is **Divinity**.
   - **The game takes place in the continent of Sialant.** Its four nations were **Fustina** (elves), **Gracoy**, **Fara Uf'Clario** and **Ashemdrek**, and only Fustina still stands.
   - **The four acts:** the Jesterwoods, Fara Uf'Clario, the Sinfa Mountains and the ruins of Ashemdrek City.
   - **Perkfandor** is a walled city that fell in the invasion.
   - **The Seared Ruins** are ancient ruins where Gabien found his power.
   - **The Argent Coast** is Gabien's home town (the level "Argent Coast Ruins").
4. **Names and spelling.**
   - **Sentinels** (not "Centinels") are Abidor's.
   - **Jesterwoods** is the region's name, and "Jester Forest" goes.
   - **Fara Uf'Clario** is the northern land and its first clan; its people are the **Faraclarians**. This replaces "Uf'Faraclario Wastes" and "Fara Uf'clario".
5. **Gabien** was a **Seared Soul**: born with so much Grace that it burns the soul. Everyone took him for Disgraced (born without Grace) until the Seared Ruins showed what he was.
   - **The Seal of Malien:** he found it, the relic the prophecies said would banish Malizante. He broke it to cure his sister **Liah**, who was Soulless; the cure was why he set out in the first place. Its power also held back the fatal Mastow condition of **Dayla**, his partner.
   - **The fall:** in fallen Ashemdrek he beat Malizante but couldn't kill her, so his party threw her into the last lit Altar of Malien. Nobody knew the altars held most of the Drekos, so the darkness broke loose over the region, and Malizante died in it. Gabien died of his wounds soon after. **Orleone Vidal** and **Kyro Feth** escaped.
   - **The truth:** only Orleone, Kyro and Dayla knew what happened. Before Abidor's nobles, Orleone and Kyro defended him, but under questioning they had to confess that he misused the Seal.
   - **The blame:** common folk hate Gabien because he lost; the nobles hate him because he misused a sacred relic.
6. **The other proper nouns.**
   - **Mastow** is a substance in the blood that lets a body capture ley energy. Elves have a lot of it and humans almost none. A human born with a lot can cast spells, but it is fatal; Dayla was one.
   - **Runic Math** is a new science built on Kyro's findings. Calculated runes, written on **Blue Parchment**, replace Mastow; **Crisalia** wakes the rune into a **Glyph** drawn in the air. Glyphs themselves are the traditional way of casting, not runic math.
   - **The Forger** is the Faraclarians' word for a Seared Soul, because only a Seared Soul can forge Steeldawn.
   - **The House of Stars** is an elven house that is a rival of nearly every other house, the **Vega Unit** among them (for elves, a "Unit" is a family).
   - **Gracoy** is the Mercenary Kingdom.
   - **Golden Chronos** is the old currency of Ashemdrek, kept alive by people who won't let their country die; traders don't care, gold is gold.
   - **Altars:** the Holy Altar node becomes the **Wayside Shrine of Malien**; the great Altars of Malien are the act bosses' altars.
   - **Grace** is Malien's gift to every human and dwarf soul, and other races don't have it.
   - **The Forsaken** are cultists who renounced their Grace.
   - **The Awoken** are Forsaken who spent a night in a grave and rose at dawn without their will, with dark power and immortality. This replaces "Dawner".
7. **Protagonist:** the three pilgrims travel together, and the chosen one fights. Events can let the others speak.
8. **Corruptors:** Act I **Dreko Spawn** (a working name), Act II **Echo of Malizante**, Act III **Echo of Pal'bor** (the oldest Dreko, who led the Forsaken and was stopped by Gabien), Act IV **Echo of Gabien**. The Act III boss is **Forsaken Daviro the Unholy** (Daviro Piktuon, a Forsaken from Gabien's time; owner, 2026-10-29).
9. **Liah** may still be alive, and very old, and the party can meet her. **Dayla is dead**: she kept the cracked, powerless Seal.

### Lore integrity issues found by the Orchestrator (2026-10-26)
_Kept for the record; resolved by "Lore decisions" above._
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
