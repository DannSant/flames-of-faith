# Opening cutscene: narrative session handoff

For the Orchestrator. These are changes this session wasn't allowed to make. The design itself is in `opening-cutscene.md`.

- **Spec header:** Status and Owner session still say the design session hasn't started. It's done.
- **`TenPageGDD.md`, outside §11 and §13:**
  - §4 Game flow: the first New Game plays the opening between class select and the map.
  - §8 Interface, main menu: add the Pilgrim's Chronicle. Also the header's "Last updated".
- **Implementation kickoff:**
  - The name of the replay menu is **Pilgrim's Chronicle**, main menu only.
  - The opening's `cutsceneId` is `opening`, and its replay title is "The Pilgrimage of the Flame".
  - The art briefs assume the page text sits in a band across the bottom of the image, so the window must be built that way.
- **Art team:** the page briefs are ready to send. Size 1920 × 1080 is still to agree with them, as the spec says. The owner says they will deliver all 7 pages before the demo; BACKLOG can note that as the plan, with no fallback.
